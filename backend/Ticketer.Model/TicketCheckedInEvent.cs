using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("TicketContractEvent")]
public class TicketCheckedInEvent : IContractEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey] 
    public required long TimestampUtc { get; init; }
    
    public string EventType => "TicketCheckedIn";
    
    public required string EventContractId { get; init; }
    public required int TicketId { get; init; }
    public required string UserId { get; init; }
    public required string TransactionHash { get; init; }
    public required string Address { get; init; }
    public required string CheckInSecretHash { get; init; }
}