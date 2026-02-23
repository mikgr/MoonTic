namespace SpikeDb;

public sealed class SpikeDbConfig
{
    private SpikeDbConfig() { }
    private static SpikeDbConfig? _instance;
    
    public static SpikeDbConfig GetInstance()
    {
        _instance ??= new SpikeDbConfig();

        return _instance;
    }

    private string? _rootFolder = null;
    public void SetRootFolder(string rootFolder)
    {
        _rootFolder = rootFolder;
    }
    public string GetRootFolder() => 
        _rootFolder ?? Directory.GetCurrentDirectory() ;
}