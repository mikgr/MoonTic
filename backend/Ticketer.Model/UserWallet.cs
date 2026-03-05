using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("UserWallet")]
public class UserWallet
{
    [DynamoDBHashKey]  
    public required string UserId { get; init; }
    
    [DynamoDBGlobalSecondaryIndexHashKey("AddressIndex")]
    public required string Address { get; init; }
    
    public required string PrivateKey { get; init; }
}