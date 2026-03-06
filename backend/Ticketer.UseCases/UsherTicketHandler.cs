using Amazon.DynamoDBv2.DataModel;

using Ticketer.Model;

namespace Ticketer.UseCases;

public class UsherTicketHandler(IDynamoDBContext dynamo)
{
    public async Task Execute(User usherUser, string contractAddress, int ticketId, string secret)
    {
        var eventContractStateSearch = dynamo.QueryAsync<EventContractState>(
            contractAddress.ToLower());
        
        var eventContractState = (await eventContractStateSearch.GetRemainingAsync()).Single();
        
        var eventContract = new EventContract(eventContractState);
        
        if (eventContract.OwnerId != usherUser.Id)
        {
            Console.WriteLine($"Not authorized to let people in. Contract:{eventContract.Id}  Owner: {eventContract.OwnerId}, Usher: {usherUser.Id}");
            throw new DomainInvariant("Not authorized to let people in");
        }
        
        if (!eventContract.CheckOutBlockIsActive(TimeProvider.System)) // todo inject
            throw new DomainInvariant("Event cannot be entered before checkout block is active");
        
        var eventEntered = await dynamo.LoadAsync<EventEnteredEvent>(
            eventContract.ContractAddress, // Partition key
            ticketId                        // Sort key
        );
        
        if (eventEntered is not null) throw new DomainInvariant("Ticket is already used to enter the event");
        
        var ticketHolderKnowsSecret = eventContract.ProofByTicketHolder(ticketId, secret);
        if (!ticketHolderKnowsSecret) throw new DomainInvariant("Ticket holder proof failed");
        
        var address = eventContract.GetHolderOfTicket(ticketId) 
            ?? throw new DomainInvariant("Ticket has no holder");
        
        var holderWalletSearch = dynamo.QueryAsync<UserWallet>(
            address.ToLower(), new QueryConfig{ IndexName = "AddressIndex" });
        
        var holderWallet = (await holderWalletSearch.GetRemainingAsync()).Single();
        
        var eventEnteredEvent = new EventEnteredEvent 
        {
            ContractAddress = eventContract.ContractAddress,
            TicketId = ticketId,
            HolderId = holderWallet.UserId,
            EnteredAt = DateTime.UtcNow
        };
        
        await dynamo.SaveAsync(eventEnteredEvent);
    }
}