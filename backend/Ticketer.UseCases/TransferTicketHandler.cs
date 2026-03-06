using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class TransferTicketHandler(
    TicketContractClient ticketContractClient,
    IRepository repo)
{
    // todo handle time of RPC CALL
    // todo handle already seen 
    public async Task Execute(User currentUser, string eventId, int ticketId, string toAddress)
    {
        Console.WriteLine($"{nameof(TransferTicketHandler)} {nameof(Execute)}");
        
        // todo validate address cannot be self + address format
        // todo make sure we can send to any valid address
        ArgumentNullException.ThrowIfNull(toAddress);
        
        var contract = await repo.LoadContractBy(eventId);
        var fromUserTicketContainer = await repo.LoadUserTicketContainer(currentUser.Id);
        
        // NB: The to address could be any address on the C-chain
        var maybeToUserTicketContainer = await LoadToUserTicketContainerOrNull(toAddress);

        var transferResult = await ticketContractClient.OnChainTransferTicket
            (currentUser, contract, ticketId, toAddress);
        
        var transferredEvent = new TicketTransferredEvent
        {
            ContractId = contract.Id,
            ContractAddress = contract.ContractAddress,
            TicketId = ticketId,
            FromUserId = currentUser.Id,
            ToUserId = maybeToUserTicketContainer?.UserId ?? "unknown",
            TransactionHash = transferResult.receipt.TransactionHash,
            FromAddress = transferResult.receipt.From,
            ToAddress = toAddress,
            TimestampUtc = transferResult.blockTimestamp
        };
        
        fromUserTicketContainer.ApplyEvent(transferredEvent);
        
        if (maybeToUserTicketContainer is null) return;
            maybeToUserTicketContainer.ApplyEvent(transferredEvent);
        
        contract.ApplyEvent(transferredEvent);
        
        
        var eventWrite = repo.CreateTransactWrite<TicketTransferredEvent>();
        eventWrite.AddSaveItem(transferredEvent);
        
        var contractWrite = repo.CreateTransactWrite<EventContractState>();
        contractWrite.AddSaveItem(contract.GetState());
        
        var fromUserWrite = repo.CreateTransactWrite<UserTicketContainerState>();
        fromUserWrite.AddSaveItem(fromUserTicketContainer.GetState());
        
        if (maybeToUserTicketContainer is { } tutc)
            fromUserWrite.AddSaveItem(tutc.GetState());
        
        var transaction = repo.CreateMultiTableTransactWrite(eventWrite, contractWrite, fromUserWrite);
        
        await transaction.ExecuteAsync();
    }

    
    private async Task<UserTicketContainer?> LoadToUserTicketContainerOrNull(string toAddress)
    {
        var search = repo.DbContext.QueryAsync<UserWallet>(
            toAddress.ToLower(),
            new QueryConfig
            {
                IndexName = "AddressIndex"
            });

        var userWallet = (await search.GetRemainingAsync()).SingleOrDefault();
        if (userWallet is null) return null;
        
        var toUserTicketContainer = await repo.LoadUserTicketContainer(userWallet.UserId);
        
        return toUserTicketContainer;
    }
}