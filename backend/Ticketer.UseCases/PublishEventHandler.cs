using System.Numerics;
using Amazon.DynamoDBv2.DataModel;
using Nethereum.Util;

using Ticketer.Model;

namespace Ticketer.UseCases;

public class PublishEventHandler(
    DeployContractHandler deployContractHandler,
    IDynamoDBContext dynamo)
{
    public async Task Execute(string eventInfoId, User currentUser)
    {
        Console.WriteLine("Publishing event");
        // todo prevent the same event from being published twice
        var eventInfo = await dynamo.LoadAsync<EventInfo>(currentUser.Id, eventInfoId);
        if (eventInfo is null) throw new InvalidOperationException("Event not found");
        if (eventInfo.Owner != currentUser?.Id) throw new DomainInvariant("Not authorized to publish event");

        var eventContract = EventContract.New(eventInfo);
      
        
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

        await dynamo.SaveAsync(eventContract.GetState());
        await dynamo.DeleteAsync<EventInfo>(eventInfo.Owner, eventInfoId);
        
        var ticketContractPublishedEvent = new TicketContractPublishedEvent
        {
            ContractId = eventContract.Id,
            ContractAddress = eventContract.ContractAddress,
            TimeStamp = DateTime.UtcNow
        };
        
        await dynamo.SaveAsync(ticketContractPublishedEvent);
    }
}