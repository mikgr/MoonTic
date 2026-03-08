using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Ticketer.Model;

namespace Ticketer.Web.Pages.Explorer;

public record ContractCreatedViewModel(DateTime TimeStampUtc, string ContractAddress, string ContractName, string TxHash);

public class Contracts : PageModel
{
    public List<ContractCreatedViewModel> ContractCreatedEvents = [];
    public List<IContractEvent> ContractEvents = [];
    
    [BindProperty(SupportsGet = true)]
    public string? ContractAddress { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public int? TicketId { get; set; }
    
    public async Task OnGet(string? address, string? ticketSegment, int? ticketId)
    {
        if (ticketSegment != null && ticketSegment != "ticket")
            NotFound();
        
        ContractAddress = address;

        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        
        if (!string.IsNullOrEmpty(ContractAddress))
        {
            var ticketPurchasedEvents = await repo.LoadContractEvents(ContractAddress);

            ContractEvents = ticketId is null
                ? ticketPurchasedEvents
                    .OrderByDescending(x => x.TimestampUtc)
                    .ToList()
                : ticketPurchasedEvents
                    .Where(x => x.TicketId == ticketId)
                    .OrderByDescending(x => x.TimestampUtc)
                    .ToList();

            return;
        }
        
        var contracts = await repo.LoadAllContracts();
        
        ContractCreatedEvents =
            contracts.Select(x => new ContractCreatedViewModel(x.DeployedAtUtc, x.ContractAddress, x.Name, x.DeployTxHash)) // todo sort by publish time, not open time
            .OrderByDescending(x => x.TimeStampUtc)
            .ToList();
    }
}