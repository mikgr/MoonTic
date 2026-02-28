using SpikeDb;

namespace Ticketer.Model;

public record UserTicket(int TicketId, int EventId, bool IsCheckedIn);

public class UserTicketContainer : ISpikeObjIntKey
{
    // todo rename to UserTickets
    private readonly List<UserTicket> _baseStateTickets = [];
    
    public int Id { get; set; }
    public required int UserId { get; init; }

    public IEnumerable<UserTicket> GetAllTickets()
    {
        return _baseStateTickets;
    }
    
    // get past tickets
    // get ongoing tickets
        
    public UserTicketContainer ApplyEvent(TicketPurchasedEvent evnt)
    {
        if (evnt.OwnerId != UserId) throw new InvalidOperationException("Can only purchase tickets for yourself");
        RemoveTicketStateIfExists(evnt.EventContractId, evnt.TicketId);
        _baseStateTickets.Add(new UserTicket(evnt.TicketId, evnt.EventContractId, IsCheckedIn: false));
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
            _baseStateTickets.Add(new UserTicket(evnt.TicketId, evnt.ContractId, IsCheckedIn: false));
            return this;
        }
        
        throw new InvalidOperationException("Event does not apply to this user");
    }

    public UserTicketContainer ApplyEvent(TicketCheckedInEvent evnt)
    {
        if (evnt.UserId != UserId) throw new InvalidOperationException("Can only check in tickets for yourself");
        RemoveTicketStateIfExists(evnt.EventContractId, evnt.TicketId);
        _baseStateTickets.Add(new UserTicket(evnt.TicketId, evnt.EventContractId, IsCheckedIn: true));
        return this;
    }

    public UserTicketContainer ApplyEvent(TicketCheckedOutEvent evnt)
    {
        if (evnt.UserId != UserId) throw new InvalidOperationException("Can only check out tickets for yourself");
        RemoveTicketStateIfExists(evnt.EventContractId, evnt.TicketId);
        _baseStateTickets.Add(new UserTicket(evnt.TicketId, evnt.EventContractId, IsCheckedIn: false));
        return this;
    }
    
    private void RemoveTicketStateIfExists(int eventContractId, int ticketId)
    {
        var maybe = _baseStateTickets
            .SingleOrDefault(x => x.EventId == eventContractId && x.TicketId == ticketId);

        if (maybe is { } ut)
            _baseStateTickets.Remove(ut);
    }

    
    public int[] GetContractIds()
    {
        return _baseStateTickets.Select(x => x.EventId).ToHashSet().ToArray();
    }
}