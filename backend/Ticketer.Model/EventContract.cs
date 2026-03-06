using System.Security.Cryptography;
using System.Text;
using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

// todo spike repo support auto property like this - no inint or setter break read
// public DateTime VenueOpenTime { get; }

[DynamoDBTable("EventContractState")]
public class EventContractState
{
    [DynamoDBHashKey]
    public string ContractAddress { get; set; } = "";
    [DynamoDBGlobalSecondaryIndexHashKey("OwnerIdIndex")]
    public required string OwnerId { get; init; }
    // private readonly int _eventId;
    // todo turn intp range 
    // todo enforce explicit timezones on times
    [DynamoDBGlobalSecondaryIndexRangeKey("OwnerIdVenueOpenTimeIndex")]
    public DateTime VenueOpenTimeUtc { get; set; }
    public DateTime VenueCloseTimeUtc { get; set; }
    public uint BlockCheckOutBeforeVenueOpenInHours { get; set; }
    public int TicketCounter = 0;
    public int TotalTickets { get; set; }
    public decimal TicketPrice { get; set; }
    public string Name { get; set; } = "";
    public decimal Balance { get; set; } = 0m;
    public string DeployTxHash { get; set; } = "";
    public DateTime DeployedAtUtc { get; set; } = default;
    public Dictionary<string, string> TicketAllocation { get; set; } = new();
    public Dictionary<string, string> CheckInSecretHashes { get; set; } = new();
}



public class EventContract(EventContractState state) : IAccount
{
    public EventContractState GetState() => state;
    public string Id => state.ContractAddress;
    public string OwnerId => state.OwnerId;
    // private readonly int _eventId;
    // todo turn intp range 
    // todo enforce explicit timezones on times
    public DateTime VenueOpenTime => state.VenueOpenTimeUtc;
    public DateTime VenueCloseTime => state.VenueCloseTimeUtc;
    public decimal TicketPrice => state.TicketPrice;
    public string Name => state.Name;
    public string ContractAddress
    {
        get => state.ContractAddress;
        set => state.ContractAddress = value.ToLower();
    }
    public decimal Balance => state.Balance;
    public string DeployTxHash
    {
        get => state.DeployTxHash;
        set => state.DeployTxHash = value;    
    }
    public DateTime DeployedAtUtc
    {
        get => state.DeployedAtUtc;
        set => state.DeployedAtUtc = value;    
    }
    public int SoldTickets => state.TicketCounter;
    public int RemainingTickets => state.TotalTickets - state.TicketCounter;
    public int TotalTickets => state.TotalTickets;
    
    
    public static EventContract New(EventInfo eventInfo)
    {
        var blockCheckOutBeforeVenueOpenInHours = eventInfo.BlockCheckOutBeforeVenueOpenInHours;
        if (blockCheckOutBeforeVenueOpenInHours < 5) throw new DomainInvariant($"{nameof(blockCheckOutBeforeVenueOpenInHours)} Cannot be less than 5 hours");
        
        return new EventContract(new EventContractState
        {
            OwnerId = eventInfo.Owner,
            Name = eventInfo.Name,
            VenueOpenTimeUtc = eventInfo.VenueOpenTime,
            VenueCloseTimeUtc = eventInfo.VenueCloseTime,
            BlockCheckOutBeforeVenueOpenInHours = blockCheckOutBeforeVenueOpenInHours,
            TotalTickets = eventInfo.Tickets,
            TicketPrice = eventInfo.Price,
        });
    }

    
    // sell ticket, crate ask, cancel ask, when ticket has ask it cannot be transferred
    void IAccount.ReceiveMoney(decimal amount) =>
        state.Balance += amount;
    
    
    public string? GetHolderOfTicket(int ticketId)
    {
        if(state.TicketAllocation.TryGetValue(ticketId.ToString(), out var userId))
            return userId;
        
        return null;
    }
    
    
    public bool ProofByTicketHolder(int ticketId, string secret)
    {
        if(!state.CheckInSecretHashes.TryGetValue(ticketId.ToString(), out var hash))
            throw new DomainInvariant($"{nameof(ProofByTicketHolder)} failed. Ticket not checked in");
         
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(secret);
        var secretHash = sha256.ComputeHash(bytes);
        
        return Convert.ToHexString(secretHash) == hash; 
    }

    
    public DateTime GetCheckOutBlockStart() => 
        VenueOpenTime.AddHours(-state.BlockCheckOutBeforeVenueOpenInHours);

    
    public bool CheckOutBlockIsActive(TimeProvider clock)=>
        GetCheckOutBlockStart() < clock.GetUtcNow();

    
    public void ApplyEvent(TicketCheckedInEvent @event) => 
        state.CheckInSecretHashes[@event.TicketId.ToString()] = @event.CheckInSecretHash;

    
    public void ApplyEvent(TicketPurchasedEvent @event)
    {
        state.TicketAllocation[@event.TicketId.ToString()] = @event.ToAddress;
        state.TicketCounter = state.TicketAllocation.Count;
    }


    public void ApplyEvent(TicketCheckedOutEvent newCheckOutEvent) => 
        state.CheckInSecretHashes.Remove(newCheckOutEvent.TicketId.ToString());

    
    public void ApplyEvent(TicketTransferredEvent newCheckOutEvent) => 
        state.TicketAllocation[newCheckOutEvent.TicketId.ToString()] = newCheckOutEvent.ToAddress;
}