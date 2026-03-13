using System.Numerics;
using Amazon.DynamoDBv2.DataModel;
using Nethereum.Util;

using Ticketer.Model;

namespace Ticketer.UseCases;

public class PublishEventHandler(
    DeployContractHandler deployContractHandler,
    IStableCoinInfoProvider stableCoinInfoProvider,
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
        BigInteger checkOutBlockedTime = eventContract.GetCheckOutBlockStart().ToUnixTimestamp();
        BigInteger venueOpenTime = eventContract.VenueOpenTimeUtc.ToUnixTimestamp();
        BigInteger venueCloseTime = eventContract.VenueCloseTimeUtc.ToUnixTimestamp();
        BigInteger totalTicketCount = eventContract.TotalTickets; // uint64 can be BigInteger in Nethereum

        // todo address is valid reachable ERC20 address, check symbol/name matchse
        var stableCoinInfo = stableCoinInfoProvider.GetStableCoinInfo("USDC"); // TODO
        
        // todo verify uint8 usdcDecimals = IERC20Metadata(usdcAddress).decimals(); matcher stablecoininfo.decimals
        BigInteger maxResellPrice = eventContract.MaxResellPrice(stableCoinInfo.decimals);
        
        // todo Assert that the stablecoin address is actually a stable coin address on the network as expectedt with the right symbol
        
        var constructorArgs = new object[]
        {
            checkOutBlockedTime,
            venueOpenTime,
            venueCloseTime,
            totalTicketCount,
            eventInfo.FullVenueAddress,
            stableCoinInfo.contractAddress,
            maxResellPrice
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