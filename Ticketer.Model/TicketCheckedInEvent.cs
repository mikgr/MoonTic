using SpikeDb;

namespace Ticketer.Model;

public class TicketCheckedInEvent : ISpikeObjIntKey, IContractEvent
{
    public required int Id { get; set; }
    public required int EventContractId { get; init; }
    public required int TicketId { get; init; }
    public required int UserId { get; init; }
    public required DateTimeOffset TimestampUtc { get; init; }
    public required string ContractAddress { get; init; }
    public required string TransactionHash { get; init; }
    public required string Address { get; init; }
    public required string CheckInSecretHash { get; init; }
}