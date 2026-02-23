using System.Text;

namespace SpikeCli;

public class CommandInfo(string verb, string noun, uint actionArgumentCount, bool withDependencyInjection = false)
{
    private readonly List<ParamInfo> _args = [];
    private readonly List<ParamInfo> _options = [];
    
    internal IEnumerable<ParamInfo> Options => _options;
    internal int ParamCount => _args.Count + _options.Count;
    internal int ArgsCount => _args.Count;
    internal (string,string) GetKey() => (verb, noun);
    internal bool WithDependencyInjection => withDependencyInjection;
    
    internal void Validate()
    {
        if (_args.Count + _options.Count != actionArgumentCount) 
            throw new SpikeCliRunException(
                $"Param/Option count mismatch in '{verb} {noun}' arguments.count + options.count is {_args.Count+_options.Count}." +
                $" Action takes {actionArgumentCount} parameters.");
        
        foreach (var group in _args.GroupBy(x => x))
            if (group.Count() > 1) 
                throw new SpikeCliRunException($"Duplicate parameter '{group.Key.Name}' in '{verb} {noun}'");
        
        foreach (var group in _options.GroupBy(x => x))
            if (group.Count() > 1) 
                throw new SpikeCliRunException($"Duplicate options '{group.Key.Name}' in '{verb} {noun}'");
    } 
    
    internal Func<object?[], object?>? Action { get; set; }

    internal CommandInfo AddParam<T>(string name)
    {
        _args.Add(new ParamInfo{ Name = name, Type = typeof(T)});
        return this;
    }

    internal void AddOption<TO>(string optionName) => 
        _options.Add(new ParamInfo{ Name = optionName, Type = typeof(TO)});

    internal void AddFlag(string flagName) => 
        _options.Add(new ParamInfo { Name = flagName, Type = typeof(bool) });

    internal ParamInfo GetArgAt(int index) => 
        _args[index];
    
    internal string GetHelpText()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"  {verb} {noun} ");
        foreach(var a in _args) builder.AppendLine($"      {a.Name} : {a.Type.Name}");
        foreach(var o in _options) builder.AppendLine($"      {o.Name} : {o.Type.Name}");
        return builder.ToString();
    }
}