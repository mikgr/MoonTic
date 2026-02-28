using SpikeDb;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class UsherTicketHandler
{
    public void Execute(User? usherUser, string contractAddress, int ticketId, string secret)
    {
        if (usherUser is null) throw new Exception("Current user not set");
        
        var eventContract = SpikeRepo.ReadSingle<EventContract>(x => x.ContractAddress.ToLower() == contractAddress.ToLower());
        if (eventContract.OwnerId != usherUser.Id)
        {
            Console.WriteLine($"Not authorized to let people in. Contract:{eventContract.Id}  Owner: {eventContract.OwnerId}, Usher: {usherUser.Id}");
            throw new DomainInvariant("Not authorized to let people in");
        }
        
        if (!eventContract.CheckOutBlockIsActive(TimeProvider.System)) // todo inject
            throw new DomainInvariant("Event cannot be entered before checkout block is active");
        
        var eventEntered = SpikeRepo.ReadFirstOrDefault<EventEnteredEvent>(x => 
            x.ContractAddress.ToLower() == eventContract.ContractAddress.ToLower() && x.TicketId == ticketId);
        if (eventEntered is not null) throw new DomainInvariant("Ticket is already used to enter the event");
        
        var ticketHolderKnowsSecret = eventContract.ProofByTicketHolder(ticketId, secret);
        if (!ticketHolderKnowsSecret) throw new DomainInvariant("Ticket holder proof failed");
        
        var address = eventContract.GetHolderOfTicket(ticketId) 
            ?? throw new DomainInvariant("Ticket has no holder");
        
        var holderWallet = SpikeRepo.ReadSingle<UserWallet>(x => x.Address.ToLower() == address.ToLower());
        
        new EventEnteredEvent 
        {
            Id = -1,
            ContractAddress = eventContract.ContractAddress,
            TicketId = ticketId,
            HolderId = holderWallet.Id,
            EnteredAt = DateTime.UtcNow
        }.SpikePersistInt();
    }
}