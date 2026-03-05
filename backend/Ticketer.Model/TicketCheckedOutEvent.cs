using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("TicketContractEvent")]
public class TicketCheckedOutEvent : IContractEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey] 
    public required long TimestampUtc { get; init; }
    
    public string EventType => "TicketCheckedOut";
    
    public required string EventContractId { get; init; }
    public required int TicketId { get; init; }
    public required string UserId { get; init; }
    public required string TransactionHash { get; init; }
    public required string Address { get; init; }
}