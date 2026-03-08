using Ticketer.Model;

namespace Ticketer.UseCases;

public class CancelAskHandler(IRepository repo, TicketContractClient ticketContractClient)
{
    public async Task Execute(User user, string contractAddress, int ticketId)
    {
        // ticket must be for sale by user
        var userTicketContainer = await repo.LoadUserTicketContainer(user.Id);
        var ticket = userTicketContainer.GetAllTickets()
            .SingleOrDefault(x => x.ContractAddress == contractAddress && x.TicketId == ticketId)
            ?? throw new DomainInvariant("Ticket not found");
        
        if (ticket.State != UserTicketState.IsForSale) throw new DomainInvariant("Ticket is not for sale");
        
        var contract = await repo.LoadContractBy(contractAddress);
        // call contract
        var (receipt, blockTimestamp) = await ticketContractClient.OnChainCancelAsk(user, ticketId, contract);
        
        // store event 
        var @event = new AskCanceledEvent
        {
            ContractAddress = contractAddress,
            TimestampUtc = blockTimestamp,
            TicketId = ticketId,
            UserId = user.Id,
            TransactionHash = receipt.TransactionHash,
            Address = receipt.From,
        };
        
        var userTickets = await repo.LoadUserTicketContainer(user.Id);
        userTickets.ApplyEvent(@event);
        
        var askToDelete = await repo.FindAsk(contractAddress, ticketId);
        
        var writeEvent = repo.CreateTransactWrite<AskCanceledEvent>();
        writeEvent.AddSaveItem(@event);
        
        var writeUserTickets = repo.CreateTransactWrite<UserTicketContainerState>();
        writeUserTickets.AddSaveItem(userTickets.GetState());
        
        var writeAsk = repo.CreateTransactWrite<TicketAsk>();
        writeAsk.AddDeleteItem(askToDelete);
     
        var transaction = repo.CreateMultiTableTransactWrite(writeEvent, writeUserTickets, writeAsk);
        await transaction.ExecuteAsync();
    }
}