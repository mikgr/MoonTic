using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Ticketer.Model;

namespace Ticketer.Repository;

public class Repository(IDynamoDBContext dynamo) : IRepository
{
    public IDynamoDBContext DbContext => dynamo;
    public ITransactWrite<T> CreateTransactWrite<T>() => dynamo.CreateTransactWrite<T>();
    public IMultiTableTransactWrite CreateMultiTableTransactWrite(params ITransactWrite[] writes) => 
        dynamo.CreateMultiTableTransactWrite(writes);
    
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

    public async Task<UserWallet?> LoadUserWalletOrNullBy(string address)
    {
        var holderWalletSearch = dynamo.QueryAsync<UserWallet>(
            address.ToLower(), new QueryConfig{ IndexName = "AddressIndex" });
        
        var holderWallet = (await holderWalletSearch.GetRemainingAsync()).SingleOrDefault();

        return holderWallet;
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

    public async Task<List<IContractEvent>> LoadContractEvents(string contractAddress)
    {
        var purchasedTask = dynamo.QueryAsync<TicketPurchasedEvent>(contractAddress).GetRemainingAsync(); // ScanAsync<TicketPurchasedEvent>([]).GetRemainingAsync();
        var checkedInTask = dynamo.QueryAsync<TicketCheckedInEvent>(contractAddress).GetRemainingAsync();   //.ScanAsync<TicketCheckedInEvent>([]).GetRemainingAsync();
        var checkedOutTask = dynamo.QueryAsync<TicketCheckedOutEvent>(contractAddress).GetRemainingAsync();  //.ScanAsync<TicketCheckedOutEvent>([]).GetRemainingAsync();
        var transferredTask = dynamo.QueryAsync<TicketTransferredEvent>(contractAddress).GetRemainingAsync(); //.ScanAsync<TicketTransferredEvent>([]).GetRemainingAsync();

        await Task.WhenAll(purchasedTask, checkedInTask, checkedOutTask, transferredTask);

        return purchasedTask.Result.Cast<IContractEvent>()
            .Concat(checkedInTask.Result)
            .Concat(checkedOutTask.Result)
            .Concat(transferredTask.Result)
            .OrderByDescending(e => e.TimestampUtc)
            .ToList();
    }
}