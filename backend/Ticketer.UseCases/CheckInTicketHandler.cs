using Amazon.DynamoDBv2.DataModel;
using Nethereum.Util;
using Ticketer.Model;

namespace Ticketer.UseCases;

public class CheckInTicketHandler(
    TicketContractClient ticketContractClient,
    IDynamoDBContext dynamo,
    IRepository repo
    )
{
    public async Task Execute(User currentUser, string contractAddress, int ticketId)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(contractAddress);
        
        var contract = await repo.LoadContractBy(contractAddress);
        
        var checkInSecretHash = currentUser.CreateSecretHashed(contract.Id, ticketId);
        await dynamo.SaveAsync(currentUser.GetState());
        
        var (receipt, blockTimestamp) = await ticketContractClient.OnChainCheckIn(
            currentUser, ticketId, contract, checkInSecretHash);
        
        var @event = new TicketCheckedInEvent
        {
            EventContractId = contract.Id,
            TicketId = ticketId,
            UserId = currentUser.Id,
            TimestampUtc = blockTimestamp.ToUnixTimestamp(),
            ContractAddress = contract.ContractAddress,
            TransactionHash = receipt.TransactionHash,
            Address = receipt.From,
            CheckInSecretHash = checkInSecretHash
        };
        
        await dynamo.SaveAsync(@event);

        var userTicketsState = await dynamo.LoadAsync<UserTicketContainerState>(currentUser.Id);
        var userTickets = new UserTicketContainer(userTicketsState);
        userTickets.ApplyEvent(@event);
        // userTickets.SpikePersistInt();
        await dynamo.SaveAsync(userTickets.GetState());
        
        contract.ApplyEvent(@event);
        // contract.SpikePersistInt();
        await dynamo.SaveAsync(contract.GetState());
    }
}