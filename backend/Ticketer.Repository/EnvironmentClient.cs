using Microsoft.Extensions.Configuration;

namespace Ticketer.Repository;

public class EnvironmentClient(IConfiguration config)
{
    private bool _forceDevelopment = false;
    
    public  bool IsDevelopment()
    {
        var forceDeveloperMode = config.GetValue<bool?>("ForceDeveloperMode");
        if (forceDeveloperMode ?? false) return true;
        
        var environment = 
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
            ?? "Production";
        
        return environment == "Development";
    }
    
    public void SetEnvironmentDevelopment() => _forceDevelopment = true;
}