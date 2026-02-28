namespace Ticketer.Model;

public interface IAccount
{
    decimal Balance { get; }
    void ReceiveMoney(decimal amount);
}