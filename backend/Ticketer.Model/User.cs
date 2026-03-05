using System.Security.Cryptography;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("User")]
public class UserState
{
    [DynamoDBHashKey]  
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [DynamoDBGlobalSecondaryIndexHashKey("UserNameIndex")]
    public required string UserName { get; init; }
    
    // todo index
    public required string Email { get; init; }
    public Dictionary<string, string> Secrets { get; set; } = new();
}


public class User(UserState state)
{
    public UserState GetState() => state;
    
    public string Id => state.Id;
    public string UserName => state.UserName;
    public string Email => state.Email;
    
    public string? GetSecret(string contractId, int ticketId) => 
        state.Secrets.GetValueOrDefault($"{contractId}_{ticketId}");
    
    public string CreateSecretHashed(string contractId, int ticketId)
    {
        var secret = Guid.NewGuid().ToString();
        state.Secrets[$"{contractId}_{ticketId}"] = secret;
        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(secret);
        var hash = sha256.ComputeHash(bytes);
        
        return Convert.ToHexString(hash); 
    }
}