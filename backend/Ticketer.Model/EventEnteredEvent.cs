
using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("EventEnteredEvent")]
public class EventEnteredEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey]
    public required int TicketId { get; init; }
    
    public required DateTime EnteredAt { get; init; }
    public required string HolderId { get; init; }
}