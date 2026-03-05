using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;

public class AllEventsModel : PageModel
{
    public IEnumerable<EventContract> EventContracts { get; private set; } = [];
    public string ErrorMessage { get; set; } = "";
    
    public async Task OnGet()
    {
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        EventContracts = await repo.LoadAllContracts();// SpikeRepo.ReadCollection<EventContract>();
    }
    
    public async Task<IActionResult> OnPostBuy(string eventId)
    {
        try
        {
            var dynamo = HttpContext.RequestServices.GetRequiredService<IDynamoDBContext>();
            
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userId)) return RedirectToPage("/LogIn");
            var userState = await dynamo.LoadAsync<UserState>(userId);
            var buyTicketHandler = HttpContext.RequestServices.GetRequiredService<BuyTicketHandler>();
            await buyTicketHandler.Execute(eventId, new User(userState));

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