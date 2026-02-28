using SpikeDb;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class BuyTicketHandler(MintTicketHandler mintTicketHandler)
{
    // todo brug https://www.mollie.com/ til payments. can support user resell -> user receive proceeds
    
    public async Task Execute(int eventContractId, User? currentUser)
    {
        if (currentUser is not {} usr) throw new Exception("User not set");
        
        var contract = SpikeRepo.ReadIntId<EventContract>(eventContractId);
        var userWallet = SpikeRepo.ReadSingle<UserWallet>(x => x.UserId == usr.Id);
        
        var mintResult = await mintTicketHandler.Execute(
            contract.ContractAddress, 
            toAddress: userWallet.Address);
        
        contract.SpikePersistInt();

        var @event = new TicketPurchasedEvent
        {
            Id = -1,
            TimestampUtc = DateTime.UtcNow,
            OwnerId = currentUser.Id,
            EventContractId = contract.Id,
            TicketId = mintResult.tokenId,
            ContractAddress = contract.ContractAddress,
            TransactionHash = mintResult.transactionHash,
            ToAddress = userWallet.Address,
            TicketPrice = contract.TicketPrice
        }.SpikePersistInt();
        
        contract.ApplyEvent(@event);
        contract.SpikePersistInt();
        
        var userTickets = SpikeRepo.ReadSingle<UserTicketContainer>(x => x.UserId == currentUser.Id);
        userTickets.ApplyEvent(@event);
        userTickets.SpikePersistInt();
    }
}