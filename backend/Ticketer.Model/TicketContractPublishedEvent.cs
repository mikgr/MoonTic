using SpikeDb;

namespace Ticketer.Model;

public class TicketContractPublishedEvent : ISpikeObjIntKey
{
    public required int Id { get; set; }
    public required int ContractId { get; init; }
    public required string ContractAddress { get; init; }
    public required DateTime TimeStamp { get; init; }
}