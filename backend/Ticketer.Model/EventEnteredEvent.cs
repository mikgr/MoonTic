using SpikeDb;

namespace Ticketer.Model;

public class EventEnteredEvent : ISpikeObjIntKey
{
    public required int Id { get; set; }
    public required string ContractAddress { get; init; }
    public required int TicketId { get; init; }
    public required DateTime EnteredAt { get; init; }
    public int HolderId { get; init; }
}