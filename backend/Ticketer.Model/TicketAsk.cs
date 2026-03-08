using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

/// <summary>
/// These are the active asks (not the events) on cancel or accept, they will be deleted
/// </summary>
[DynamoDBTable("TicketAsk")]
public class TicketAsk
{
    [DynamoDBHashKey]
    [DynamoDBGlobalSecondaryIndexHashKey("ContractAddressPriceIndex")]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey]
    public required DateTime TimestampUtc { get; init; }
    
    public required int TicketId { get; init; }
    
    [DynamoDBGlobalSecondaryIndexHashKey("UserIdIndex")]
    public required string UserId { get; init; }
    
    public required string TransactionHash { get; init; }
    
    public required string Address { get; init; }
    
    [DynamoDBGlobalSecondaryIndexRangeKey("UserIdIndex", "ContractAddressPriceIndex")]
    public required int Price { get; init; }
}