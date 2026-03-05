using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class CheckInTicketHandler(TicketContractClient ticketContractClient, IRepository repo)
{
    public async Task Execute(User currentUser, string contractAddress, int ticketId)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(contractAddress);
        
        var contract = await repo.LoadContractBy(contractAddress);
        
        var checkInSecretHash = currentUser.CreateSecretHashed(contract.Id, ticketId);
        await repo.DbContext.SaveAsync(currentUser.GetState());
        
        var (receipt, blockTimestamp) = await ticketContractClient.OnChainCheckIn(
            currentUser, ticketId, contract, checkInSecretHash);
        
        var @event = new TicketCheckedInEvent
        {
            EventContractId = contract.Id,
            TicketId = ticketId,
            UserId = currentUser.Id,
            TimestampUtc = blockTimestamp,
            ContractAddress = contract.ContractAddress,
            TransactionHash = receipt.TransactionHash,
            Address = receipt.From,
            CheckInSecretHash = checkInSecretHash
        };
        
        await repo.DbContext.SaveAsync(@event);

        var userTickets = await repo.LoadUserTicketContainer(currentUser.Id);
        
        userTickets.ApplyEvent(@event);
        // userTickets.SpikePersistInt();
        await repo.DbContext.SaveAsync(userTickets.GetState());
        
        contract.ApplyEvent(@event);
        // contract.SpikePersistInt();
        await repo.DbContext.SaveAsync(contract.GetState());
    }
}