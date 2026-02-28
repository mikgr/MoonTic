using SpikeDb;

namespace Ticketer.Model;

public class Account : ISpikeObjIntKey, IAccount
{
    public required int Id { get; set; }
    public required int UserId { get; init; } // todo account must be owned by user or contract
    public decimal Balance { get; private set; } = 0m;

    public void SendMoney(decimal amount, IAccount to)
    {
        if (amount <= 0) throw new DomainInvariant($"{nameof(SendMoney)} Failed. Amount {amount} is not valid");
        if (Balance < amount && UserId != 0) throw new DomainInvariant("Insufficient funds");
        Balance -= amount;
        to.ReceiveMoney(amount);
    }

    public void ReceiveMoney(decimal amount)
    {
        Balance += amount;
    }
}