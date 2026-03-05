using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;

namespace Ticketer.Repository;

public class Repository(IDynamoDBContext dynamo) : IRepository
{
    public async Task<User> LoadUserAsync(string userId)
    {
        var userState = await dynamo.LoadAsync<UserState>(userId)
            ?? throw new InvalidOperationException("User not found");
        
        var user = new User(userState);
        
        return user;
    }
    
    public async Task Persist<T>(T obj)
    {
        await dynamo.SaveAsync(obj); 
    }

    public Task<List<EventInfo>> EventInfo(string ownerId)
    {
        return dynamo.QueryAsync<EventInfo>(ownerId).GetRemainingAsync();
    }

    public Task<List<EventContract>> LoadContractsBy(string ownerId)
    {
        return dynamo.QueryAsync<EventContractState>(ownerId, new QueryConfig
        {
            IndexName = "OwnerIdIndex"
        }).GetRemainingAsync().ContinueWith(t => t.Result.Select(x => new EventContract(x)).ToList());
    }

    public async Task<List<EventContract>> LoadAllContracts()
    {
        var scan = dynamo.ScanAsync<EventContractState>(new List<ScanCondition>());
        var allItems = await scan.GetRemainingAsync();
        return allItems.Select(x => new EventContract(x)).ToList();
    }


    public async Task<EventContract> LoadContractBy(string contractAddress)
    {
        var state = await  dynamo.LoadAsync<EventContractState>(contractAddress.ToLower())
            ?? throw new Exception("Contract not found");
        
        return new EventContract(state);
    }

    public Task<UserWallet> LoadUserWallet(string userId)
    {
        return dynamo.LoadAsync<UserWallet>(userId);
    }

    public async Task<UserTicketContainer> LoadUserTicketContainer(string userId)
    {
        var state = await dynamo.LoadAsync<UserTicketContainerState>(userId);
        return new UserTicketContainer(state);
    }

    public async Task<TicketPurchasedEvent> LoadEventsBy(string contractAddress)
    {
        var events = await dynamo.QueryAsync<TicketPurchasedEvent>(contractAddress).GetRemainingAsync();
        throw new NotImplementedException();
    }
}