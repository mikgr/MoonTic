using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;


public record TestStatus(int EventId, int TicketId, string Status); 


public class Test : PageModel
{
    public IEnumerable<TicketHolding> Tickets { get; set; } = new List<TicketHolding>();
    
    
    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/LogIn");
        
        var ticketPurchases = SpikeRepo.ReadSingleOrDefault<UserTicketContainer>(x => x.UserId == userId);
        if (ticketPurchases == null)
        {
            Console.Error.WriteLine($"No user ticket container found for {userId}");
            return Page();
        }
        
        var contractIds = ticketPurchases.GetContractIds();
        var contracts = SpikeRepo
            .ReadCollection<EventContract>(x => contractIds.Contains(x.Id))
            .ToList();
        
        var tickets = 
            from t in ticketPurchases.GetAllTickets()
            join c in contracts on t.EventId equals c.Id
            orderby c.VenueOpenTime, t.TicketId
            select new TicketHolding(
                t.TicketId, t.EventId, c.ContractAddress, c.Name, c.VenueOpenTime, t.IsCheckedIn);
        
        Tickets = tickets;
        
        return Page();
    }


    public IActionResult OnGetCheckInModal(int eventId, int ticketId)
    {
        return Partial("_CheckInModal", new TestStatus(eventId, ticketId, "post acitonOne"));    
    }

    
    public IActionResult OnGetCheckOutModal(int eventId, int ticketId)
    {
        return Partial("_CheckOutModal", new TestStatus(eventId, ticketId, "post acitonOne"));    
    }
    
    
    public async Task<IActionResult> OnPostCheckIn(int eventId, int ticketId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return HtmxRedirect("/LogIn");
        var user = SpikeRepo.ReadIntId<User>(userId.Value);
        
        var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
        var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
        
        await jobQueue.EnqueueAsync(async ct =>
        {
            using var scope = scopeFactory.CreateScope();
            var checkInTicketHandler = scope.ServiceProvider.GetRequiredService<CheckInTicketHandler>();
            await checkInTicketHandler.Execute(user, eventId, ticketId);
        });

        return Partial("_TicketStatus", new TicketInfo(eventId, ticketId, "pending", "check-in"));
    }
    
    
    public async Task<IActionResult> OnPostCheckOut(int eventId, int ticketId)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is not {} uid) return HtmxRedirect("/LogIn");
            
            var user = SpikeRepo.ReadIntId<User>(uid);
            
            var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
            var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();

            await jobQueue.EnqueueAsync(async ct =>
            {
                using var scope = scopeFactory.CreateScope();
                var checkOutTicketHandler = scope.ServiceProvider.GetRequiredService<CheckOutTicketHandler>();
                await checkOutTicketHandler.Execute(user, eventId, ticketId);
            });
            
            return Partial("_TicketStatus", new TicketInfo(eventId, ticketId, "pending", "check-out")); 
        
            // todo feedback ok or failed or already checked in
        }
        catch (DomainInvariant)
        {
            //todo ErrorMessage = e.Message;
            return Page();
        }
    }
    
    
    // todo post transfer
    
    private IActionResult HtmxRedirect(string page)
    {
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            Response.Headers["HX-Redirect"] = Url.Page(page);
            return new EmptyResult();
        }

        return RedirectToPage(page);
    }
    
}