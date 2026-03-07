using Ticketer.Model;

namespace Ticketer.UseCases;

public class CreateAskHandler(IRepository repo)
{
    public async Task Execute(User user, string contractAddress, int ticketId, int askPrice)
    {
        var contract = await repo.LoadContractBy(contractAddress);
        var userTicketContainer = await repo.LoadUserTicketContainer(user.Id);
        
        var ticket = userTicketContainer.GetAllTickets()
            .SingleOrDefault(x => x.ContractAddress == contractAddress && x.TicketId == ticketId)
            ?? throw new DomainInvariant("Cannot create Ask, Ticket not found");
        
        if (ticket.IsCheckedIn) throw new DomainInvariant("Cannot create Ask, Ticket is already checked in");
        
        
        
        throw new NotImplementedException("create ask not implemented");
    }
}