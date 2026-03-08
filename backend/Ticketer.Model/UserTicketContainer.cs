
using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;


public class UserTicket
{
    public required int TicketId { get; init; } 
    public required string ContractAddress { get; init; }
    public required UserTicketState State { get; init; } = UserTicketState.BaseState;
}

public enum UserTicketState
{
    BaseState = 0,
    IsCheckedIn = 1,
    IsForSale = 2,
}


[DynamoDBTable("UserTicketContainerState")]
public class UserTicketContainerState
{
    [DynamoDBHashKey] 
    public required string UserId { get; init; }
    // todo rename to UserTickets

    public List<UserTicket> BaseStateTickets { get; set; } = new(); // todo fix ther is a 400kb size limit on the doc
}



public class UserTicketContainer(UserTicketContainerState state)
{
    public UserTicketContainerState GetState() => state;
    
    public string UserId => state.UserId;
    
    public IEnumerable<UserTicket> GetAllTickets() => 
        state.BaseStateTickets;
    
    // get past tickets
    // get ongoing tickets
        
    public UserTicketContainer ApplyEvent(TicketPurchasedEvent evnt)
    {
        // Apply event to resell-seller
        if (evnt.OwnerId != UserId && evnt.PurchaseType == nameof(PurchaseType.Secondary))
        {
            RemoveTicketStateIfExists(evnt.EventContractId, evnt.TicketId);
            return this;
        }
        
        // Apply event to any buyer
        if (evnt.OwnerId != UserId) throw new InvalidOperationException("Can only purchase tickets for yourself");
        
        RemoveTicketStateIfExists(evnt.EventContractId, evnt.TicketId);
        state.BaseStateTickets.Add(new UserTicket
            {
                TicketId = evnt.TicketId,
                ContractAddress = evnt.EventContractId,
                State = UserTicketState.BaseState
            }
        );
        return this;
    }

    public UserTicketContainer ApplyEvent(TicketTransferredEvent evnt)
    {
        if (evnt.FromUserId == UserId)
        {
            RemoveTicketStateIfExists(evnt.ContractId, evnt.TicketId);
            return this;
        }
        else if (evnt.ToUserId == UserId)
        {
            state.BaseStateTickets.Add(new UserTicket
            {
                TicketId = evnt.TicketId,
                ContractAddress = evnt.ContractId,
                State = UserTicketState.BaseState
            });
                
            return this;
        }
        
        throw new InvalidOperationException("Event does not apply to this user");
    }

    public UserTicketContainer ApplyEvent(TicketCheckedInEvent evnt)
    {
        if (evnt.UserId != UserId) throw new InvalidOperationException("Can only check in tickets for yourself");
        RemoveTicketStateIfExists(evnt.EventContractId, evnt.TicketId);
        state.BaseStateTickets.Add(
            new UserTicket
            {
                TicketId = evnt.TicketId,
                ContractAddress = evnt.EventContractId,
                State = UserTicketState.IsCheckedIn
            });
        return this;
    }

    public UserTicketContainer ApplyEvent(TicketCheckedOutEvent evnt)
    {
        if (evnt.UserId != UserId) throw new InvalidOperationException("Can only check out tickets for yourself");
        RemoveTicketStateIfExists(evnt.EventContractId, evnt.TicketId);
        state.BaseStateTickets.Add(new UserTicket
        {
            TicketId = evnt.TicketId,
            ContractAddress = evnt.EventContractId,
            State = UserTicketState.BaseState
        });
        return this;
    }
    
    public UserTicketContainer ApplyEvent(AskCreatedEvent evnt)
    {
        if (evnt.UserId != UserId) throw new InvalidOperationException("Can only create ask for tickets you own");
        RemoveTicketStateIfExists(evnt.ContractAddress, evnt.TicketId);
        state.BaseStateTickets.Add(new UserTicket
        {
            TicketId = evnt.TicketId,
            ContractAddress = evnt.ContractAddress,
            State = UserTicketState.IsForSale
        });
        return this;
    }
    
    public UserTicketContainer ApplyEvent(AskCanceledEvent @event)
    {
        if (@event.UserId != UserId) throw new InvalidOperationException("Can only cancel ask for tickets you own");
        RemoveTicketStateIfExists(@event.ContractAddress, @event.TicketId);
        state.BaseStateTickets.Add(new UserTicket
        {
            TicketId = @event.TicketId,
            ContractAddress = @event.ContractAddress,
            State = UserTicketState.BaseState
        });
        return this;
    }
    
    private void RemoveTicketStateIfExists(string eventContractId, int ticketId)
    {
        var maybe = state.BaseStateTickets
            .SingleOrDefault(x => x.ContractAddress == eventContractId && x.TicketId == ticketId);

        if (maybe is { } ut)
            state.BaseStateTickets.Remove(ut);
    }

    
    public string[] GetContractIds()
    {
        return state.BaseStateTickets.Select(x => x.ContractAddress).ToHashSet().ToArray();
    }

    public bool? IsCheckedIn(string eventId, int ticketId)
    {
        var ticket = state.BaseStateTickets.SingleOrDefault(x => 
            x.ContractAddress == eventId && x.TicketId == ticketId);
        
        if (ticket is null) 
            return null;

        return ticket.State == UserTicketState.IsCheckedIn;
    }



}