using Amazon.DynamoDBv2.DataModel;

using Ticketer.Model;

namespace Ticketer.UseCases;

public class UsherTicketHandler(IRepository repo)
{
    public async Task Execute(User usherUser, string contractAddress, int ticketId, string secret)
    {
        var eventContract = await repo.LoadContractBy(contractAddress);
        
        if (eventContract.OwnerId != usherUser.Id)
        {
            Console.WriteLine($"Not authorized to let people in. Contract:{eventContract.Id}  Owner: {eventContract.OwnerId}, Usher: {usherUser.Id}");
            throw new DomainInvariant("Not authorized to let people in");
        }
        
        if (!eventContract.CheckOutBlockIsActive(TimeProvider.System)) // todo inject
            throw new DomainInvariant("Event cannot be entered before checkout block is active");
        
        var eventEntered = await repo.DbContext.LoadAsync<EventEnteredEvent>(
            eventContract.ContractAddress, // Partition key
            ticketId                        // Sort key
        );
        
        if (eventEntered is not null) throw new DomainInvariant("Ticket is already used to enter the event");
        
        var ticketHolderKnowsSecret = eventContract.ProofByTicketHolder(ticketId, secret);
        if (!ticketHolderKnowsSecret) throw new DomainInvariant("Ticket holder proof failed");
        
        var address = eventContract.GetHolderOfTicket(ticketId) 
            ?? throw new DomainInvariant("Ticket has no holder");
        
        var holderWallet = await repo.LoadUserWalletOrNullBy(address: address.ToLower());
        
        var eventEnteredEvent = new EventEnteredEvent 
        {
            ContractAddress = eventContract.ContractAddress,
            TicketId = ticketId,
            HolderId = holderWallet?.UserId ?? address,
            EnteredAt = DateTime.UtcNow
        };
        
        await repo.DbContext.SaveAsync(eventEnteredEvent);
    }
}