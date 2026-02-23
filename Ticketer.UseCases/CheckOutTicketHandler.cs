using SpikeDb;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class CheckOutTicketHandler(TicketContractClient ticketContractClient)
{
    public async Task Execute(User? currentUser, int eventId, int ticketId)
    {
        if (currentUser is null) throw new Exception("User not set");
        var eventContract = SpikeRepo.ReadIntId<EventContract>(eventId);
        
        var checkoutResult = await ticketContractClient.OnChainCheckOut(currentUser, ticketId, eventContract);
        
        var ticketContainer = SpikeRepo.ReadSingle<UserTicketContainer>(x => x.UserId == currentUser.Id);
        
        var newCheckOutEvent = new TicketCheckedOutEvent
        {
            Id = -1,
            EventContractId = eventContract.Id,
            TicketId = ticketId,
            UserId = currentUser.Id,
            TimestampUtc = checkoutResult.blockTimestamp,
            ContractAddress = eventContract.ContractAddress,
            TransactionHash = checkoutResult.receipt.TransactionHash,
            Address = checkoutResult.receipt.From
        };

        ticketContainer.ApplyEvent(newCheckOutEvent);
        eventContract.ApplyEvent(newCheckOutEvent);
                
        eventContract.SpikePersistInt();
        newCheckOutEvent.SpikePersistInt();
        ticketContainer.SpikePersistInt();
        eventContract.SpikePersistInt();
    }
    
}