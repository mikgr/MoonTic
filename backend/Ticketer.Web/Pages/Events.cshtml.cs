using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;

public class AllEventsModel : PageModel
{
    public IEnumerable<EventContract> EventContracts { get; private set; } = [];
    public string ErrorMessage { get; set; } = "";
    
    public void OnGet()
    {
        EventContracts = SpikeRepo.ReadCollection<EventContract>();
    }
    
    public async Task<IActionResult> OnPostBuy(int eventId)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null or -1) return RedirectToPage("/LogIn");
            var user = SpikeRepo.ReadOrNullByInt<User>((int)userId);
            var buyTicketHandler = HttpContext.RequestServices.GetRequiredService<BuyTicketHandler>();
            await buyTicketHandler.Execute(eventId, user);

            return RedirectToPage("/Tickets");
        }
        catch (DomainInvariant e)
        {
            ErrorMessage = e.Message;
            return Page();
        }
        catch (Exception e) // todo add ILogger 
        {
            Console.WriteLine(e.StackTrace);
            Console.WriteLine(e.Message);
            // todo handle insufficient funds
            return RedirectToPage("/Error");
        }
    }
}