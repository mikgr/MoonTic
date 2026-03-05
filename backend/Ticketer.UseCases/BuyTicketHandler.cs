using Amazon.DynamoDBv2.DataModel;
using Nethereum.Util;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class BuyTicketHandler(MintTicketHandler mintTicketHandler, IRepository repo)
{
    // todo brug https://www.mollie.com/ til payments. can support user resell -> user receive proceeds
    
    public async Task Execute(string contractAddress, User currentUser)
    {
        if (currentUser is not {} usr) throw new Exception("User not set");

        var contract = await repo.LoadContractBy(contractAddress); 
        
        var userWallet = await repo.LoadUserWallet(currentUser.Id);
           
        var mintResult = await mintTicketHandler.Execute(
            contract.ContractAddress, 
            toAddress: userWallet.Address);
        
        
        var @event = new TicketPurchasedEvent
        {
            TimestampUtc = DateTime.UtcNow,
            OwnerId = currentUser.Id,
            EventContractId = contract.Id,
            TicketId = mintResult.tokenId,
            ContractAddress = contract.ContractAddress,
            TransactionHash = mintResult.transactionHash,
            ToAddress = userWallet.Address,
            TicketPrice = contract.TicketPrice
        };
        
        // todo transaction
        await repo.Persist(@event);
        
        contract.ApplyEvent(@event);
        await repo.Persist(contract.GetState());

        var userTickets = await repo.LoadUserTicketContainer(currentUser.Id);
        
        userTickets.ApplyEvent(@event);
        
        await repo.Persist(userTickets.GetState());
    }
}