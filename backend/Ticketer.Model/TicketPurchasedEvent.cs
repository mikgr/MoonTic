

using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("TicketContractEvent")]
public class TicketPurchasedEvent :  IContractEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey] 
    public required long TimestampUtc { get; init; }
    public string EventType => "TicketPurchased";
    
    public required string OwnerId { get; init; }
    public required string EventContractId { get; init; }
    public required int TicketId { get; init; }
    public required string TransactionHash { get; init; }
    public required string ToAddress { get; init; }
    public required decimal TicketPrice { get; init; }
}