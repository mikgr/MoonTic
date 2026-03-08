using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("AskCanceledEvent")]
public class AskCanceledEvent : IContractEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey] 
    public required DateTime TimestampUtc { get; init; }
    
    public required int TicketId { get; init; }
    public required string UserId { get; init; }
    public required string TransactionHash { get; init; }
    public required string Address { get; init; }
}