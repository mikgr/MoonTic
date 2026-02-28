using System.Security.Cryptography;
using System.Text;
using SpikeDb;

namespace Ticketer.Model;

public class User : ISpikeObjIntKey
{
    public required int Id { get; set; }
    public required string UserName { get; init; }

    public required string Email { get; init; }

    // todo this will not work with json only message pack
    // private readonly Dictionary<(int,int), string> _secrets = new();
    private readonly Dictionary<string, string> _secrets = new();
    public string? GetSecret(int contractId, int ticketId) => 
        _secrets.GetValueOrDefault($"{contractId}_{ticketId}");
    
    public string CreateSecretHashed(int contractId, int ticketId)
    {
        var secret = Guid.NewGuid().ToString();
        _secrets[$"{contractId}_{ticketId}"] = secret;
        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(secret);
        var hash = sha256.ComputeHash(bytes);
        
        return Convert.ToHexString(hash); 
    }
}