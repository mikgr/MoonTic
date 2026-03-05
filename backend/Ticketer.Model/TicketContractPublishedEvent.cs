using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;


[DynamoDBTable("ContractEvents")]
public class TicketContractPublishedEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [DynamoDBRangeKey] 
    public required DateTime TimeStamp { get; init; }
    
    [DynamoDBProperty("EventType")]
    public string EventType { get; set; } = "ContractPublished";
    
    public required string ContractId { get; init; }
}