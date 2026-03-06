using Ticketer.Model;

namespace Ticketer.UseCases;


// todo handle check out is blocked Nethereum.ABI.FunctionEncoding.SmartContractRevertException: Smart contract error: Check out is blocked
public class CheckOutTicketHandler(TicketContractClient ticketContractClient, IRepository repo)
{
    public async Task Execute(User? currentUser, string contractAddress, int ticketId)
    {
        if (currentUser is null) throw new Exception("User not set");
        var eventContract = await repo.LoadContractBy(contractAddress);
        
        var checkoutResult = await ticketContractClient.OnChainCheckOut(currentUser, ticketId, eventContract);
        
        var ticketContainer = await repo.LoadUserTicketContainer(currentUser.Id);
        
        var newCheckOutEvent = new TicketCheckedOutEvent
        {
            EventContractId = eventContract.Id,
            TicketId = ticketId,
            UserId = currentUser.Id,
            TimestampUtc = checkoutResult.blockTimestamp,
            ContractAddress = eventContract.ContractAddress,
            TransactionHash = checkoutResult.receipt.TransactionHash,
            Address = checkoutResult.receipt.From
        };
        
        eventContract.ApplyEvent(newCheckOutEvent);
        ticketContainer.ApplyEvent(newCheckOutEvent);
        
        var checkOutEventWrite = repo.CreateTransactWrite<TicketCheckedOutEvent>();
        checkOutEventWrite.AddSaveItem(newCheckOutEvent);
        
        var ticketContainerWrite = repo.CreateTransactWrite<UserTicketContainerState>();
        ticketContainerWrite.AddSaveItem(ticketContainer.GetState());
        
        var eventContractWrite = repo.CreateTransactWrite<EventContractState>();
        eventContractWrite.AddSaveItem(eventContract.GetState());
        
        var transaction = repo.CreateMultiTableTransactWrite(
            checkOutEventWrite, ticketContainerWrite, eventContractWrite);
        
        await transaction.ExecuteAsync();
    }
    
}