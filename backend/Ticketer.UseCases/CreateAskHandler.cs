using Ticketer.Model;

namespace Ticketer.UseCases;

public class CreateAskHandler(
    IRepository repo,
    TicketContractClient ticketContractClient,
    IStableCoinInfoProvider stableCoinInfoProvider)
{
    public async Task Execute(User user, string contractAddress, int ticketId, int askPrice)
    {
        var contract = await repo.LoadContractBy(contractAddress);
        var stablesInfo = stableCoinInfoProvider.GetStableCoinInfo(contract.PaymentStableCoinSymbol);
        
        if (contract.MaxResellPrice(stablesInfo.decimals) < askPrice) throw new DomainInvariant("Ask price is too high");
        
        var userTicketContainer = await repo.LoadUserTicketContainer(user.Id);
        
        var ticket = userTicketContainer.GetAllTickets()
            .SingleOrDefault(x => x.ContractAddress == contractAddress && x.TicketId == ticketId)
            ?? throw new DomainInvariant("Cannot create Ask, Ticket not found");
        
        if (ticket.State != UserTicketState.BaseState) throw new DomainInvariant($"Cannot create Ask, Ticket state is {ticket.State.ToString()}");
        
        // call contract to create ask
        var (receipt, blockTimestamp) = await ticketContractClient.OnChainCreateAsk(user, ticketId, askPrice, contract);
        
        // store ask in db in tx todo 
        var ticketAsk = new TicketAsk
        {
            ContractAddress = contractAddress,
            UserId = user.Id,
            TicketId = ticketId,
            Price = askPrice,
            TimestampUtc = blockTimestamp,
            TransactionHash = receipt.TransactionHash,
            Address = receipt.From
        };
        
        // store event
        var @event = new AskCreatedEvent
        {
            ContractAddress = contractAddress,
            TimestampUtc = blockTimestamp,
            TicketId = ticketId,
            UserId = user.Id,
            TransactionHash = receipt.TransactionHash,
            Address = receipt.From,
            Price = askPrice
        };
        
        userTicketContainer.ApplyEvent(@event);

        
        var writeAsk = repo.CreateTransactWrite<TicketAsk>();
        writeAsk.AddSaveItem(ticketAsk);
        
        var eventWrite = repo.CreateTransactWrite<AskCreatedEvent>();
        eventWrite.AddSaveItem(@event);

        var ticketContainerWrite = repo.CreateTransactWrite<UserTicketContainerState>();
        ticketContainerWrite.AddSaveItem(userTicketContainer.GetState());
        
        var transaction = repo.CreateMultiTableTransactWrite(writeAsk, eventWrite, ticketContainerWrite);
        await transaction.ExecuteAsync();
    }
}