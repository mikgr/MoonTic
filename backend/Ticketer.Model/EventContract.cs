
using System.Security.Cryptography;
using System.Text;
using SpikeDb;

namespace Ticketer.Model;

// todo spike repo support auto property like this - no inint or setter break read
// public DateTime VenueOpenTime { get; }

public class EventContract : ISpikeObjIntKey, IAccount
{
    public int Id { get; set; } // todo se if Id can be init-only
    public int OwnerId { get; private set;}
    // private readonly int _eventId;
    // todo turn intp range 
    // todo enforce explicit timezones on times
    public DateTime VenueOpenTime { get; private set; } 
    public DateTime VenueCloseTime { get; private set; }
    private uint BlockCheckOutBeforeVenueOpenInHours { get; set; }

    private int _ticketCounter = 0;
    private readonly int _totalTickets;
    public decimal TicketPrice { get; private set; }

    public int SoldTickets => _ticketCounter;
    public int RemainingTickets => _totalTickets - _ticketCounter;
    public int TotalTickets => _totalTickets;
    public string Name { get; private set; }
    public string ContractAddress { get; set; } = "";

    public decimal Balance { get; private set; } = 0m;
    public string DeployTxHash { get; set; } = "";

    private EventContract(EventInfo eventInfo)
    {
        Id = -1;
        OwnerId = eventInfo.Owner;
        // _eventId = eventInfo.Id;
        Name = eventInfo.Name;
        VenueOpenTime = eventInfo.VenueOpenTime;
        VenueCloseTime = eventInfo.VenueCloseTime;
        BlockCheckOutBeforeVenueOpenInHours = eventInfo.BlockCheckOutBeforeVenueOpenInHours;
        _totalTickets = eventInfo.Tickets;
        TicketPrice = eventInfo.Price;
    }
    
    public static EventContract New(EventInfo eventInfo) => new (eventInfo);
    
    // sell ticket, crate ask, cancel ask, when ticket has ask it cannot be transferred
    void IAccount.ReceiveMoney(decimal amount) =>
        Balance += amount;
    
    private readonly Dictionary<int, string> _ticketAllocation = new();
    
    
    public string? GetHolderOfTicket(int ticketId)
    {
        if(_ticketAllocation.TryGetValue(ticketId, out var userId))
            return userId;
        
        return null;
    }
    
    // todo rename to _checkInSecretHashes
    Dictionary<int, string> _checkSecretInHashes = new();
    
    
    public bool ProofByTicketHolder(int ticketId, string secret)
    {
        if(!_checkSecretInHashes.TryGetValue(ticketId, out var hash))
            throw new DomainInvariant($"{nameof(ProofByTicketHolder)} failed. Ticket not checked in");
         
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(secret);
        var secretHash = sha256.ComputeHash(bytes);
        
        return Convert.ToHexString(secretHash) == hash; 
    }

    public DateTime GetCheckOutBlockStart() => 
        VenueOpenTime.AddHours(-BlockCheckOutBeforeVenueOpenInHours);

    
    public bool CheckOutBlockIsActive(TimeProvider clock)=>
        GetCheckOutBlockStart() < clock.GetUtcNow();

    
    public void ApplyEvent(TicketCheckedInEvent @event) => 
        _checkSecretInHashes[@event.TicketId] = @event.CheckInSecretHash;

    
    public void ApplyEvent(TicketPurchasedEvent @event)
    {
        _ticketAllocation[@event.TicketId] = @event.ToAddress;
        _ticketCounter = _ticketAllocation.Count;
    }


    public void ApplyEvent(TicketCheckedOutEvent newCheckOutEvent) => 
        _checkSecretInHashes.Remove(newCheckOutEvent.TicketId);

    
    public void ApplyEvent(TicketTransferredEvent newCheckOutEvent) => 
        _ticketAllocation[newCheckOutEvent.TicketId] = newCheckOutEvent.ToAddress;
}