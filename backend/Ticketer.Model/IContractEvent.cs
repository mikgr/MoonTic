namespace Ticketer.Model;

public interface IContractEvent
{
    public string ContractAddress { get; init; }
    public long TimestampUtc { get; init; } // todo use datetime here
    public string TransactionHash { get; init; }
    public string EventType { get; }
}