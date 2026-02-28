using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;

public record TicketHolding(int TicketNo, int EventId, string ContractAddress, string EventName, DateTime Date, bool IsCheckedIn);

public class TicketsModel : PageModel
{
    public IEnumerable<TicketHolding> Tickets { get; set; } = [];
    public string ErrorMessage { get; set; } = "";

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null or -1) return RedirectToPage("/LogIn");
        
        LoadUserTickets(userId.Value);
        return Page();
    }

    private void LoadUserTickets(int userId)
    {
        var ticketPurchases = SpikeRepo.ReadSingleOrDefault<UserTicketContainer>(x => x.UserId == userId);
        if (ticketPurchases == null)
        {
            Console.Error.WriteLine($"No user ticket container found for {userId}");
            return ;
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
    }
    
    public async Task<IActionResult> OnPostCheckIn(int eventId, int ticketId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/LogIn");
        var user = SpikeRepo.ReadIntId<User>(userId.Value);
        
        var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
        var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
        
        await jobQueue.EnqueueAsync(async ct =>
        {
            using var scope = scopeFactory.CreateScope();
            var checkInTicketHandler = scope.ServiceProvider.GetRequiredService<CheckInTicketHandler>();
            await checkInTicketHandler.Execute(user, eventId, ticketId);
        });
        
        // todo i need to return at job id here so i can poll for status
        return RedirectToPage("/Tickets");
        // todo feedback ok or failed or already checked in
    }
    public async Task<IActionResult> OnPostCheckOut(int eventId, int ticketId)
    {
        try
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/LogIn");
            var user = SpikeRepo.ReadIntId<User>(userId.Value);
            
            var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
            var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();

            await jobQueue.EnqueueAsync(async ct =>
            {
                using var scope = scopeFactory.CreateScope();
                var checkOutTicketHandler = scope.ServiceProvider.GetRequiredService<CheckOutTicketHandler>();
                await checkOutTicketHandler.Execute(user, eventId, ticketId);
            });
            
            return RedirectToPage("/Tickets");
            // todo feedback ok or failed or already checked in
        }
        catch (DomainInvariant e)
        {
            ErrorMessage = e.Message;
            return Page();
        }
    }
    
    public async Task<IActionResult> OnPostTransfer(int eventId, int ticketId, string recipientAddress)
    {
        // todo handle failures like transfer to self or timeout or transaction already seen
        try
        {
            ArgumentNullException.ThrowIfNull(recipientAddress);
            
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/LogIn");
            var user = SpikeRepo.ReadIntId<User>(userId.Value);
            var transferTicketHandler = HttpContext.RequestServices.GetRequiredService<TransferTicketHandler>();
            await transferTicketHandler.Execute(user, eventId, ticketId, recipientAddress);
            return RedirectToPage("/Tickets");
            // todo feedback ok or failed
        }
        catch (DomainInvariant e)
        {
            ErrorMessage = e.Message;
            return Page();
        }
    }
}