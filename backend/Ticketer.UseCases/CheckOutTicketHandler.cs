using Amazon.DynamoDBv2.DataModel;
using Nethereum.Util;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class CheckOutTicketHandler(
    TicketContractClient ticketContractClient,
    IDynamoDBContext dynamo,
    IRepository repo)
{
    public async Task Execute(User? currentUser, string contractAddress, int ticketId)
    {
        if (currentUser is null) throw new Exception("User not set");
        var eventContract = await repo.LoadContractBy(contractAddress);
        
        var checkoutResult = await ticketContractClient.OnChainCheckOut(currentUser, ticketId, eventContract);
        
        var ticketContainerState = await dynamo.LoadAsync<UserTicketContainerState>(currentUser.Id);
        var ticketContainer = new UserTicketContainer(ticketContainerState);
        
        var newCheckOutEvent = new TicketCheckedOutEvent
        {
            EventContractId = eventContract.Id,
            TicketId = ticketId,
            UserId = currentUser.Id,
            TimestampUtc = checkoutResult.blockTimestamp.ToUnixTimestamp(),
            ContractAddress = eventContract.ContractAddress,
            TransactionHash = checkoutResult.receipt.TransactionHash,
            Address = checkoutResult.receipt.From
        };

        await dynamo.SaveAsync(newCheckOutEvent);
        ticketContainer.ApplyEvent(newCheckOutEvent);
        eventContract.ApplyEvent(newCheckOutEvent);
        
        //ticketContainer.SpikePersistInt();
        await dynamo.SaveAsync(ticketContainer.GetState());
        //eventContract.SpikePersistInt();
        await dynamo.SaveAsync(eventContract.GetState());
    }
    
}