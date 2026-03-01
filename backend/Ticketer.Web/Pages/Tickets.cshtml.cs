using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;


public record TicketHolding(int TicketNo, int EventId, string ContractAddress, string EventName, DateTime Date, bool IsCheckedIn);
public record TicketActionStatus(int EventId, int TicketId, string Status, string Action);

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
        return Partial("_CheckInModal", new TicketActionStatus(eventId, ticketId, "",""));    
    }

    
    public IActionResult OnGetCheckOutModal(int eventId, int ticketId)
    {
        return Partial("_CheckOutModal", new TicketActionStatus(eventId, ticketId, "", ""));    
    }

    
    public IActionResult OnGetTransferModal(int eventId, int ticketId)
    {
        return Partial("_TransferModal", new TicketActionStatus(eventId, ticketId, "", ""));
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

        return Partial("_TicketStatus", new TicketActionStatus(eventId, ticketId, "pending", "check-in"));
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
            
            return Partial("_TicketStatus", new TicketActionStatus(eventId, ticketId, "pending", "check-out")); 
        
            // todo feedback ok or failed or already checked in
        }
        catch (DomainInvariant)
        {
            //todo ErrorMessage = e.Message;
            return Page();
        }
    }
    
    
    public async Task<IActionResult> OnPostTransfer(int eventId, int ticketId, string recipientAddress)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(recipientAddress);

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return HtmxRedirect("/LogIn");
            
            var user = SpikeRepo.ReadIntId<User>(userId.Value);
            var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
            var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();

            await jobQueue.EnqueueAsync(async ct =>
            {
                using var scope = scopeFactory.CreateScope();
                var transferTicketHandler = scope.ServiceProvider.GetRequiredService<TransferTicketHandler>();
                await transferTicketHandler.Execute(user, eventId, ticketId, recipientAddress);
            });
            
            return Partial("_TicketStatus", new TicketActionStatus(eventId, ticketId, "pending", "transfer"));
        }
        catch (DomainInvariant)
        {
            // todo show error in modal
            return Partial("_TransferModal", new TicketActionStatus(eventId, ticketId, "", ""));
        }
    }
    
    
    public IActionResult OnGetTicketStatus(string action, int eventId, int ticketId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/LogIn");
        
        var ticketPurchases = SpikeRepo.ReadSingleOrDefault<UserTicketContainer>(x => x.UserId == userId)
            ?? throw new Exception("No user ticket container found");

        var hasTicket = ticketPurchases.GetAllTickets().Any(t => t.EventId == eventId && t.TicketId == ticketId);
        var status = (action, ticketPurchases.IsCheckedIn(eventId, ticketId), hasTicket) switch
        {
            ("check-in", false, _) => "pending",
            ("check-in", true, _) => "checked-in",
            ("check-out", true, _) => "pending",
            ("check-out", false, _) => "not-checked-in",
            ("transfer", _, true) => "pending",
            ("transfer", _, false) => "transferred",
            (_, _, _) => "unknown"
        };

        if (action == "transfer" && status == "transferred" && Request.Headers.ContainsKey("HX-Request"))
        {
            Response.Headers.Append("HX-Trigger", $"{{\"fadeOutCard\": {{\"eventId\": {eventId}, \"ticketId\": {ticketId}}}}}");
        }
        
        return Partial("_TicketStatus", new TicketActionStatus(eventId, ticketId, status, action)); 
    }
    
    
    public IActionResult OnGetActionButton(int? eventId, int? ticketId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/LogIn");
        
        var contracts = SpikeRepo.ReadSingleOrDefault<EventContract>(x => x.Id == eventId);
        var ticketContainer = SpikeRepo.ReadSingleOrDefault<UserTicketContainer>(x => x.UserId == userId);
        
        if (contracts is null || ticketContainer is null) return new NotFoundResult();

        var xx = ticketContainer.GetAllTickets().SingleOrDefault(x =>
            x.EventId == eventId && x.TicketId == ticketId) 
                 ?? throw new Exception("No ticket found");
        
        var x = new TicketHolding(
            ticketId!.Value, 
            eventId!.Value, 
            contracts.ContractAddress, 
            contracts.Name, 
            contracts.VenueOpenTime, 
            xx.IsCheckedIn
        );
        
        return Partial("_TicketActionButton", x);
    }
    
    
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