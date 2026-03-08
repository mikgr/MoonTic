namespace Ticketer.Model;

public interface IContractEvent
{
    public string ContractAddress { get; init; }
    public DateTime TimestampUtc { get; init; }
    public string TransactionHash { get; init; }
    public int TicketId { get; init; }
}