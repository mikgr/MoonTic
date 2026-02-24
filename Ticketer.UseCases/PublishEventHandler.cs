using System.Numerics;
using Nethereum.Util;
using SpikeDb;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class PublishEventHandler(DeployContractHandler deployContractHandler)
{
    public async Task Execute(int eventInfoId, User? currentUser)
    {
        Console.WriteLine("Publishing event");
        // todo prevent the same event from being published twice
        var eventInfo = SpikeRepo.ReadOrNullByInt<EventInfo>(eventInfoId);
        if (eventInfo is null) throw new InvalidOperationException("Event not found");
        if (eventInfo.Owner != currentUser?.Id) throw new DomainInvariant("Not authorized to publish event");

        var eventContract = EventContract.New(eventInfo);
        eventContract.SpikePersistInt();
        SpikeRepo.Delete<EventInfo>(eventInfoId);
        
        // Constructor arguments
        BigInteger fakeCheckOutBlockedTime = eventContract.GetCheckOutBlockStart().ToUnixTimestamp();
        BigInteger venueOpenTime = eventContract.VenueOpenTime.ToUnixTimestamp();
        BigInteger venueCloseTime = eventContract.VenueCloseTime.ToUnixTimestamp();
        BigInteger totalTicketCount = eventContract.TotalTickets; // uint64 can be BigInteger in Nethereum
        string location = "Store VEGA, Enghavevej 40, 1674 Copenhagen V, Denmark"; // todo fix

        var constructorArgs = new object[]
        {
            fakeCheckOutBlockedTime,
            venueOpenTime,
            venueCloseTime,
            totalTicketCount,
            location
        };
                
        await deployContractHandler.Execute(constructorArgs, eventContract);

        new TicketContractPublishedEvent
        {
            Id = -1,
            ContractId = eventContract.Id,
            ContractAddress = eventContract.ContractAddress,
            TimeStamp = DateTime.UtcNow
        }.SpikePersistInt();
    }
}