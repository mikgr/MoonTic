using SpikeDb;

namespace Ticketer.Model;

public class TicketPurchasedEvent : ISpikeObjIntKey, IContractEvent
{
    public required int Id { get; set; }
    public required DateTimeOffset TimestampUtc { get; init; }
    public required int OwnerId { get; init; }
    public required int EventContractId { get; init; }
    public required int TicketId { get; init; }
    public required string TransactionHash { get; init; }
    public required string ContractAddress { get; init; }
    public required string ToAddress { get; init; }
}