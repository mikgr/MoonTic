namespace Ticketer.Model;

public interface IContractEvent
{
    public string ContractAddress { get; init; }
    public DateTime TimestampUtc { get; init; } // todo use datetime here
    public string TransactionHash { get; init; }
    
}