using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

public interface IRepository
{
    IDynamoDBContext DbContext { get; }
    ITransactWrite<T> CreateTransactWrite<T>();
    IMultiTableTransactWrite CreateMultiTableTransactWrite(params ITransactWrite[] writes);
    Task<User> LoadUserAsync(string userId);
    Task Persist<T>(T obj);
    Task<List<EventInfo>> EventInfo(string ownerId);
    Task<List<EventContract>> LoadContractsBy(string ownerId);
    Task<List<EventContract>> LoadAllContracts();
    Task<EventContract> LoadContractBy(string contractAddress);
    Task<UserWallet> LoadUserWallet(string userId);
    Task<UserWallet?> LoadUserWalletOrNullBy(string address);
    Task<UserTicketContainer> LoadUserTicketContainer(string userId);
    Task<TicketPurchasedEvent> LoadEventsBy(string contractAddress);
    Task<List<IContractEvent>> LoadContractEvents(string contractAddress);
    Task<TicketAsk> FindAsk(string contractAddress, int ticketId);
}