

using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("TicketTransferredEvent")]
public class TicketTransferredEvent : IContractEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey] 
    public required DateTime TimestampUtc { get; init; }
    
    public required string ContractId { get; init; }
    public required int TicketId { get; init; }
    public required string FromUserId { get; init; }
    public required string ToUserId { get; init; }
    public required string TransactionHash { get; init; }
    public required string FromAddress { get; init; }
    public required string ToAddress { get; init; }
}