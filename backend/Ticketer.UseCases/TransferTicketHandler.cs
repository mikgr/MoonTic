using Amazon.DynamoDBv2.DataModel;
using Nethereum.Util;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class TransferTicketHandler(
    TicketContractClient ticketContractClient,
    IDynamoDBContext dynamo,
    IRepository repo)
{
    // todo handle time of RPC CALL
    // todo long running, better ux with pending -> failed / completed
    // todo handle already seen 
    public async Task Execute(User? currentUser, string eventId, int ticketId, string toAddress)
    {
        Console.WriteLine($"{nameof(TransferTicketHandler)} {nameof(Execute)}");
        // todo validate address cannot be self + address format
        // todo make sure we can send to any valid address
        ArgumentNullException.ThrowIfNull(toAddress);
        
        if (currentUser is null) throw new Exception("User not set");
        var contract = await repo.LoadContractBy(eventId); // SpikeRepo.ReadIntId<EventContract>(eventId);
        var fromUserTicketContainerState = await dynamo.LoadAsync<UserTicketContainerState>(currentUser.Id);
        var fromUserTicketContainer = new UserTicketContainer(fromUserTicketContainerState);
        
        // todo must be transferrable to non MoonTic addresses!!
        var search = dynamo.QueryAsync<UserWallet>(
            toAddress.ToLower(),
            new QueryConfig
            {
                IndexName = "AddressIndex"
            });

        var userWallet = (await search.GetRemainingAsync())
            .SingleOrDefault() ?? throw new Exception("User wallet not found");
        
        var transferResult = await ticketContractClient.OnChainTransferTicket
            (currentUser, contract, ticketId, toAddress);
        
        //contract.Transfer(ticketId, from: currentUser.Id, to: userWallet.UserId);
        
        var toUserTicketContainer = await repo.LoadUserTicketContainer(userWallet.UserId);
        
        var transferredEvent = new TicketTransferredEvent
        {
            ContractId = contract.Id,
            ContractAddress = contract.ContractAddress,
            TicketId = ticketId,
            FromUserId = currentUser.Id,
            ToUserId = userWallet.UserId,
            TransactionHash = transferResult.receipt.TransactionHash,
            FromAddress = transferResult.receipt.From,
            ToAddress = toAddress,
            TimestampUtc = transferResult.blockTimestamp
        };
        
        fromUserTicketContainer.ApplyEvent(transferredEvent);
        toUserTicketContainer.ApplyEvent(transferredEvent);
        contract.ApplyEvent(transferredEvent);
        
        await dynamo.SaveAsync(transferredEvent);
        await dynamo.SaveAsync(contract.GetState());
        await dynamo.SaveAsync(fromUserTicketContainer.GetState());
        await dynamo.SaveAsync(toUserTicketContainer.GetState());
    }
}