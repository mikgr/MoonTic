using SpikeDb;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class CheckInTicketHandler(TicketContractClient ticketContractClient)
{
    public async Task Execute(User? currentUser, int eventId, int ticketId)
    {
        if (currentUser is null) throw new Exception("User not set");
                
        var contract = SpikeRepo.ReadIntId<EventContract>(eventId);
        
        var checkInSecretHash = currentUser.CreateSecretHashed(contract.Id, ticketId);
        currentUser.SpikePersistInt();
        
        var (receipt, blockTimestamp) = await ticketContractClient.OnChainCheckIn(
            currentUser, ticketId, contract, checkInSecretHash);
        
        contract.SpikePersistInt();

        var @event = new TicketCheckedInEvent
        {
            Id = -1,
            EventContractId = contract.Id,
            TicketId = ticketId,
            UserId = currentUser.Id,
            TimestampUtc = blockTimestamp,
            ContractAddress = contract.ContractAddress,
            TransactionHash = receipt.TransactionHash,
            Address = receipt.From,
            CheckInSecretHash = checkInSecretHash
        }.SpikePersistInt();
        
        var userTickets = SpikeRepo.ReadSingle<UserTicketContainer>(x => x.UserId == currentUser.Id);
        userTickets.ApplyEvent(@event);
        userTickets.SpikePersistInt();
        contract.ApplyEvent(@event);
        contract.SpikePersistInt();
    }
}