using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;


public record TicketHolding(
    int TicketNo,
    string ContractAddress,
    string EventName,
    DateTimeOffset Date,
    bool IsCheckedIn,
    bool CheckoutIsBlocked
    );

public record TicketActionStatus(string ContractAddress, int TicketId, string Status, string Action);

public class Test : PageModel
{
    public IEnumerable<TicketHolding> Tickets { get; set; } = new List<TicketHolding>();
    
    
    public async Task<IActionResult> OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId == null) return RedirectToPage("/LogIn");

        var dynamo = HttpContext.RequestServices.GetRequiredService<IDynamoDBContext>();

        var userTicketContainerState = await dynamo.LoadAsync<UserTicketContainerState>(userId);
        
        var ticketPurchases = new UserTicketContainer(userTicketContainerState);
        if (userTicketContainerState == null)
        {
            await Console.Error.WriteLineAsync($"No user ticket container found for {userId}");
            return Page();
        }
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        
        var contractIds = ticketPurchases.GetContractIds();
        var contracts = (await repo.LoadAllContracts()) // todo fix this query
            .Where(x => contractIds.Contains(x.Id))
            .ToList();
        
        var tickets = 
            from t in ticketPurchases.GetAllTickets()
            join c in contracts on t.ContractAddress equals c.Id
            orderby c.VenueOpenTime, t.TicketId
            select new TicketHolding(
                t.TicketId, c.ContractAddress, c.Name, c.VenueOpenTime, t.IsCheckedIn, c.CheckOutBlockIsActive(TimeProvider.System));
        
        Tickets = tickets;
        
        return Page();
    }


    public IActionResult OnGetCheckInModal(string eventId, int ticketId)
    {
        return Partial("_CheckInModal", new TicketActionStatus(eventId, ticketId, "",""));    
    }

    
    public IActionResult OnGetCheckOutModal(string eventId, int ticketId)
    {
        return Partial("_CheckOutModal", new TicketActionStatus(eventId, ticketId, "", ""));    
    }

    
    public IActionResult OnGetTransferModal(string eventId, int ticketId)
    {
        return Partial("_TransferModal", new TicketActionStatus(eventId, ticketId, "", ""));
    }
    
    
    public async Task<IActionResult> OnPostCheckIn(string contractAddress, int ticketId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId == null) return HtmxRedirect("/LogIn");
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        var user = await repo.LoadUserAsync(userId);
        
        var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
        var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
        
        await jobQueue.EnqueueAsync(async ct =>
        {
            using var scope = scopeFactory.CreateScope();
            var checkInTicketHandler = scope.ServiceProvider.GetRequiredService<CheckInTicketHandler>();
            await checkInTicketHandler.Execute(user, contractAddress, ticketId);
        });

        return Partial("_TicketStatus", new TicketActionStatus(contractAddress, ticketId, "pending", "check-in"));
    }
    
    
    public async Task<IActionResult> OnPostCheckOut(string contractAddress, int ticketId)
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId is not {} uid) return HtmxRedirect("/LogIn");
            
            var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
            
            var user = await repo.LoadUserAsync(uid);
            
            var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
            var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();

            await jobQueue.EnqueueAsync(async ct =>
            {
                using var scope = scopeFactory.CreateScope();
                var checkOutTicketHandler = scope.ServiceProvider.GetRequiredService<CheckOutTicketHandler>();
                await checkOutTicketHandler.Execute(user, contractAddress, ticketId);
            });
            
            return Partial("_TicketStatus", new TicketActionStatus(contractAddress, ticketId, "pending", "check-out")); 
        
            // todo feedback ok or failed or already checked in
        }
        catch (DomainInvariant)
        {
            //todo ErrorMessage = e.Message;
            return Page();
        }
    }
    
    
    public async Task<IActionResult> OnPostTransfer(string contractAddress, int ticketId, string recipientAddress)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(recipientAddress);

            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return HtmxRedirect("/LogIn");
            
            var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
            var user = await repo.LoadUserAsync(userId);
            
            var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
            var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();

            await jobQueue.EnqueueAsync(async ct =>
            {
                using var scope = scopeFactory.CreateScope();
                var transferTicketHandler = scope.ServiceProvider.GetRequiredService<TransferTicketHandler>();
                await transferTicketHandler.Execute(user, contractAddress, ticketId, recipientAddress);
            });
            
            return Partial("_TicketStatus", new TicketActionStatus(contractAddress, ticketId, "pending", "transfer"));
        }
        catch (DomainInvariant)
        {
            // todo show error in modal
            return Partial("_TransferModal", new TicketActionStatus(contractAddress, ticketId, "", ""));
        }
    }
    
    
    public async Task<IActionResult> OnGetTicketStatus(string action, string contractAddress, int ticketId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId == null) return RedirectToPage("/LogIn");
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();

        var ticketPurchases = await repo.LoadUserTicketContainer(userId); 

        var hasTicket = ticketPurchases.GetAllTickets().Any(t => t.ContractAddress == contractAddress && t.TicketId == ticketId);
        var status = (action, ticketPurchases.IsCheckedIn(contractAddress, ticketId), hasTicket) switch
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
            Response.Headers.Append("HX-Trigger", $"{{\"fadeOutCard\": {{\"eventId\": {contractAddress}, \"ticketId\": {ticketId}}}}}");
        }
        
        return Partial("_TicketStatus", new TicketActionStatus(contractAddress, ticketId, status, action)); 
    }
    
    
    public async Task<IActionResult> OnGetActionButton(string contractAddress, int ticketId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId == null) return RedirectToPage("/LogIn");
        
       
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        
      
        var contract = await repo.LoadContractBy(contractAddress); 
       
        var ticketContainer = await repo.LoadUserTicketContainer(userId); 

        var userTicket = ticketContainer.GetAllTickets().SingleOrDefault(x =>
            x.ContractAddress == contractAddress && x.TicketId == ticketId) 
                 ?? throw new Exception("No ticket found");
        
        var x = new TicketHolding(
            ticketId, 
            contract.ContractAddress, 
            contract.Name, 
            contract.VenueOpenTime, 
            userTicket.IsCheckedIn,
            contract.CheckOutBlockIsActive(TimeProvider.System)
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