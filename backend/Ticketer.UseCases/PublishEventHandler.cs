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
                
        var (
            deployedAtUtc, 
            contractAddress, 
            deployTxHash
        ) = await deployContractHandler.Execute(constructorArgs);
        
        eventContract.ContractAddress = contractAddress;
        eventContract.DeployTxHash = deployTxHash;
        eventContract.DeployedAtUtc = deployedAtUtc;

        
        var saveContract = dynamo.CreateTransactWrite<EventContractState>();
        saveContract.AddSaveItem(eventContract.GetState());

        var deleteEventInfo = dynamo.CreateTransactWrite<EventInfo>();
        deleteEventInfo.AddDeleteKey(eventInfo.Owner, eventInfoId);

        var transaction = dynamo.CreateMultiTableTransactWrite(saveContract, deleteEventInfo);
        await transaction.ExecuteAsync();
    }
}