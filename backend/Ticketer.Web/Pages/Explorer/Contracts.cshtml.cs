using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Ticketer.Model;

namespace Ticketer.Web.Pages.Explorer;

public record ContractCreatedViewModel(DateTimeOffset TimeStamp, string ContractAddress, string ContractName, string TxHash);

public class Contracts : PageModel
{
    public List<ContractCreatedViewModel> ContractCreatedEvents = [];
    public List<IContractEvent> ContractEvents = [];
    
    [BindProperty(SupportsGet = true)]
    public string? ContractAddress { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public int? TicketId { get; set; }
    
    public void OnGet(string? address, string? ticketSegment, int? ticketId)
    {
        if (ticketSegment != null && ticketSegment != "ticket")
            NotFound();
        
        ContractAddress = address;

        if (!string.IsNullOrEmpty(ContractAddress))
        {
            throw new NotImplementedException();
            // var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
            //
            // var ticketPurchasedEvents = await repo.LoadEventsBy(ContractAddress); // SpikeRepo.ReadCollection<TicketPurchasedEvent>(e =>
            //     e.ContractAddress == ContractAddress && (e.TicketId == ticketId || ticketId == null))
            //     .AsEnumerable<IContractEvent>();
            //
            // var ticketCheckedInEvents = SpikeRepo.ReadCollection<TicketCheckedInEvent>(x => 
            //     x.ContractAddress == ContractAddress && (x.TicketId == ticketId || ticketId == null))
            //     .AsEnumerable<IContractEvent>();
            //
            // var ticketCheckedOutEvents = SpikeRepo.ReadCollection<TicketCheckedOutEvent>(x => 
            //     x.ContractAddress == ContractAddress && (x.TicketId == ticketId || ticketId == null))
            //     .AsEnumerable<IContractEvent>();
            //
            // var ticketTransferredEvents = SpikeRepo.ReadCollection<TicketTransferredEvent>(x =>
            //     x.ContractAddress == ContractAddress && (x.TicketId == ticketId || ticketId == null));
            //
            // ContractEvents = ticketPurchasedEvents
            //     .Union(ticketCheckedInEvents)
            //     .Union(ticketCheckedOutEvents)
            //     .Union(ticketTransferredEvents)
            //     .OrderByDescending(x => x.TimestampUtc)
            //     .ToList();
            //
            // return;
        }
        
        // var publishedEvents = SpikeRepo.ReadCollection<TicketContractPublishedEvent>();
        // var contracts = SpikeRepo.ReadCollection<EventContract>();
        
        // ContractCreatedEvents = publishedEvents
        //     .Join(contracts, 
        //         pe => pe.ContractId, 
        //         c => c.Id, 
        //         (pe, c) => new ContractCreatedViewModel(pe.TimeStamp, pe.ContractAddress, c.Name, c.DeployTxHash))
        //     .OrderByDescending(x => x.TimeStamp)
        //     .ToList();
    }
}