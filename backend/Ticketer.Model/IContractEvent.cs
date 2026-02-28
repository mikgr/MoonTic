namespace Ticketer.Model;

public interface IContractEvent
{
    public string ContractAddress { get; init; }
    public DateTimeOffset TimestampUtc { get; init; }
    public string TransactionHash { get; init; }
}