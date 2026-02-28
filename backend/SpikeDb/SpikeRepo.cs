using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SpikeDb;

// this should run as a singleton
public static class SpikeRepo 
{
    const string Spike = "s_p_i_k_e";
    
    // todo this must be thread safe
    public static T Persist<T>(T obj) where T : class, ISpikeObj
    {
        var type = obj.GetType();
        var jsonObject = new JsonObject();

        // Get all fields including inherited ones
        // todo fix save enum correct
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            var fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        
            foreach (var field in fields)
            {
                if (field.Name.Contains("k__BackingField"))
                    continue;

                var value = field.GetValue(obj);
                jsonObject[field.Name] = JsonValue.Create(value);
            }

            var properties = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        
            foreach (var prop in properties)
            {
                if (prop.CanRead && !jsonObject.ContainsKey(prop.Name))
                {
                    var value = prop.GetValue(obj);
                    jsonObject[prop.Name] = JsonValue.Create(value);
                }
            }

            currentType = currentType.BaseType;
        }

        // Add type info for deserialization
        jsonObject["$type"] = type.AssemblyQualifiedName;
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()
        };
        var json = jsonObject.ToJsonString(options);
        
        var idString = obj switch
        {
            ISpikeObjIntKey => jsonObject["Id"]?.GetValue<int>().ToString() ?? throw new Exception("Id not found"),
            ISpikeObjGuid => jsonObject["Id"]?.GetValue<Guid>().ToString() ?? throw new Exception("Id not found"),
            _ => throw new Exception("Unknown object type")
        };

        var currentDir = EnsureRootDir<T>();
        File.WriteAllText(Path.Combine(currentDir, $"{idString}.json"), json);
        
        // #if DEBUG
        // Console.WriteLine(json);
        // #endif
        
        return obj;
    }
    
    public static IEnumerable<T> ReadCollection<T>(Func<T,bool>? filter = null) where T : class
    {
        var dir = EnsureRootDir<T>();
        
        if (!Directory.Exists(dir))
            yield break;
            
        foreach (var file in Directory.GetFiles(dir, "*.json"))
        {
            if (DeserializeJsonToInstance<T>(file) is not {} instance)
                continue;

            if (filter is {} f)
                if (!f(instance)) continue;
            
            yield return (T)instance;
        }
    }
    

    private static T? DeserializeJsonToInstance<T>(string filePath) where T : class
    {
        var type = typeof(T);
        
        if(!File.Exists(filePath))
            return null;
        
        var json = File.ReadAllText(filePath);
        var jsonObject = JsonNode.Parse(json)?.AsObject();
        
        if (jsonObject == null)
            return null;

        // Create an instance using CompilerServices.RuntimeHelpers (bypasses constructor)
        var instance = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);

        // Load fields and properties from the entire inheritance hierarchy
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            // Set fields first
            var fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                if (field.Name.Contains("k__BackingField"))
                    continue;

                if (jsonObject.TryGetPropertyValue(field.Name, out var jsonValue) && jsonValue != null)
                {
                    var value = ConvertJsonValue(jsonValue, field.FieldType);
                    field.SetValue(instance, value);
                }
            }

            // Set properties (will write to backing fields for auto-properties)
            var properties = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                if (!prop.CanWrite) continue;
                    
                if (jsonObject.TryGetPropertyValue(prop.Name, out var jsonValue) && jsonValue != null)
                {
                    var value = ConvertJsonValue(jsonValue, prop.PropertyType);
                    prop.SetValue(instance, value);
                }
            }

            currentType = currentType.BaseType;
        }

        return (T)instance;
    }

    private static string EnsureRootDir<T>()
    {
        var dir = Path.Combine(
            SpikeDbConfig.GetInstance().GetRootFolder(), 
            Spike, 
            typeof(T).FullName ?? throw new InvalidOperationException("EnsureRootDir failed. Fullname was null"));
        
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        
        return dir;
    }

    private static object? ConvertJsonValue(JsonNode jsonValue, Type targetType)
    {
        if (targetType == typeof(Guid))
            return Guid.Parse(jsonValue.GetValue<string>());
        if (targetType == typeof(DateTime))
            return DateTime.Parse(jsonValue.GetValue<string>());
        if (targetType == typeof(decimal))
            return jsonValue.GetValue<decimal>();
        if (targetType == typeof(int))
            return jsonValue.GetValue<int>();
        if (targetType == typeof(uint))
            return jsonValue.GetValue<uint>();
        if (targetType == typeof(long))
            return jsonValue.GetValue<long>();
        if (targetType == typeof(string))
            return jsonValue.GetValue<string>();
        if (targetType == typeof(bool))
            return jsonValue.GetValue<bool>();
        if (targetType.IsEnum)
            return jsonValue.GetValue<int>();
        
        // Fallback: try deserialize
        return JsonSerializer.Deserialize(jsonValue.ToJsonString(), targetType);
    }

    public static T? ReadOrNullByGuid<T>(Guid id) where T : class, ISpikeObjGuid
    {
        var rootDir = EnsureRootDir<T>();
        var filePath = Path.Combine(rootDir, $"{id}.json");
        var obj = DeserializeJsonToInstance<T>(filePath);

        return obj;
    }
    
    // todo FindOrDefault
    public static T? ReadOrNullByInt<T>(int id) where T : class, ISpikeObjIntKey
    {
        var rootDir = EnsureRootDir<T>();
        var filePath = Path.Combine(rootDir, $"{id}.json");
        var obj = DeserializeJsonToInstance<T>(filePath);

        return obj;
    }

    public static T Read<T>(Guid id) where T : class, ISpikeObjGuid
    {
        var maybeObj = ReadOrNullByGuid<T>(id);
        return maybeObj ?? throw new InvalidOperationException();
    }
    public static T ReadIntId<T>(int id) where T : class, ISpikeObjIntKey
    {
        var maybeObj = ReadOrNullByInt<T>(id);
        return maybeObj ?? throw new InvalidOperationException();
    }

    public static void DangerousDeleteAllWithOutRecover<T>()
    {
        var rootDir = EnsureRootDir<T>();
        var files = Directory.GetFiles(rootDir, "*.json");
        foreach (var file in files) File.Delete(file);
    }

    public static int Count<T>()
    {
        var rootDir = EnsureRootDir<T>();
        var files = Directory .GetFiles(rootDir, "*.json");
        return files.Length;
    }

    public static T SpikePersist<T>(this T obj) where T : class, ISpikeObjGuid
    {
        return Persist(obj);
    }

    public static T SpikePersistInt<T>(this T obj) where T : class, ISpikeObjIntKey
    {
        if (obj.Id < 0)
        {
            var rootDir = EnsureRootDir<T>();
            // ensure counter file exists
            var counterFile = Path.Combine(rootDir, ".counter");
            if (!File.Exists(counterFile))
            {
                File.WriteAllText(counterFile, "0");
                obj.Id = 0;
            }
            else
            {
                var prevValue = int.Parse(File.ReadAllText(counterFile));
                obj.Id = prevValue + 1;
                File.WriteAllText(counterFile, obj.Id.ToString());
            }
        }

        return Persist(obj);
    }


    public static void Truncate<T>() where T : class, ISpikeObjIntKey
    {
        File.Delete(Path.Combine(EnsureRootDir<T>(), ".counter"));
    }

    public static T ReadSingle<T>(Expression<Func<T, bool>> by) where T : class, ISpikeObjIntKey
    {
        try
        {
            return ReadCollection(by.Compile()).Single();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Could not find single {typeof(T).Name} with predicate {by}", e);
        }
    }
    
    public static T? ReadSingleOrDefault<T>(Func<T, bool> by) where T : class, ISpikeObjIntKey
    {
        return ReadCollection(by).SingleOrDefault();
    }
    
    public static T? ReadFirstOrDefault<T>(Func<T, bool> by) where T : class, ISpikeObjIntKey
    {
        return ReadCollection(by).FirstOrDefault();
    }

    public static void Delete<T>(int eventId)
    {
        var rootDir = EnsureRootDir<T>();
        var filePath = Path.Combine(rootDir, $"{eventId}.json");
        File.Delete(filePath);
    }
}