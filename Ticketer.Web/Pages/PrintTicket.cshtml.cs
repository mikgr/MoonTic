using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;

namespace Ticketer.Web.Pages;



public class PrintTicket : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ContractAddress { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public int? TicketId { get; set; }
    
    public string Secret { get; set; } = "";

    
    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null or -1) return RedirectToPage("/LogIn");
        
        if (TicketId is null) return NotFound();
        
        var contract = SpikeRepo.ReadFirstOrDefault<EventContract>(x =>
            x.ContractAddress.ToLower() == ContractAddress?.ToLower());

        if (contract is null) return NotFound();
        
        ContractAddress = contract.ContractAddress;
        
        var user = SpikeRepo.ReadSingle<User>(x => x.Id == userId);
        Secret = user.GetSecret(contract.Id, TicketId.Value) ?? "n/a";

        return Page();
    }
}