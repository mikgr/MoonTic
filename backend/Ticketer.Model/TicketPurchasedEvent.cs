

using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("TicketPurchasedEvent")]
public class TicketPurchasedEvent :  IContractEvent
{
    [DynamoDBHashKey]
    public required string ContractAddress { get; init; }
    
    [DynamoDBRangeKey] 
    public required DateTime TimestampUtc { get; init; }
    
    [DynamoDBGlobalSecondaryIndexHashKey("OwnerIdIndex")]
    public required string OwnerId { get; init; }
    
    [DynamoDBGlobalSecondaryIndexHashKey("SellerIdIndex")]
    public required string SellerId { get; init; }
    
    public required string EventContractId { get; init; }
    public required int TicketId { get; init; }
    public required string TransactionHash { get; init; }
    public required string ToAddress { get; init; }
    public required decimal TicketPrice { get; init; }
    public required string PurchaseType { get; init; }
}

public enum PurchaseType { Primary, Secondary }