using SpikeDb;

namespace Ticketer.Model;

public class TicketTransferredEvent : ISpikeObjIntKey, IContractEvent
{
    public int Id { get; set; }
    public required int ContractId { get; init; }
    public required int TicketId { get; init; }
    public required int FromUserId { get; init; }
    public required int ToUserId { get; init; }
    public required DateTimeOffset TimestampUtc { get; init; }
    public required string TransactionHash { get; init; }
    public required string FromAddress { get; init; }
    public required string ToAddress { get; init; }
    public required string ContractAddress { get; init; }
}