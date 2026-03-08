using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;

public class SecondaryMarketViewModel
{
    public required string ContractAddress { get; init; }
    public required TicketAsk[] TicketAsks { get; init; }
}

public class SecondaryMarketPayViewModel
{
    public required string EventName { get; init; }
    public required string ContractAddress { get; init; }
    public required int TicketId { get; init; }
    public required int Price { get; init; }
}

public class AllEventsModel : PageModel
{
    public IEnumerable<EventContract> EventContracts { get; private set; } = [];
    public string ErrorMessage { get; set; } = "";
    
    public async Task OnGet()
    {
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        EventContracts = await repo.LoadAllContracts();
    }
    
    public async Task<IActionResult> OnGetSecondaryMarketModal(string contractAddress)
    {
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        var asks = await repo.FindAsks(contractAddress);
        var viewModel = new SecondaryMarketViewModel
        {
            ContractAddress = contractAddress, 
            TicketAsks = asks
        };
        return Partial("_SecondaryMarketModal", viewModel);    
    }
    
    public async Task<IActionResult> OnGetSecondaryMarketPayModal(string contractAddress, int ticketId)
    {
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        var contract = await repo.LoadContractBy(contractAddress);
        var ask = await repo.FindAsk(contractAddress, ticketId);
        var viewModel = new SecondaryMarketPayViewModel
        {
            ContractAddress = contractAddress,
            EventName = contract.Name,
            TicketId = ticketId,
            Price = ask.Price,
        };
        
        return Partial("_SecondaryMarketPayModal", viewModel);    
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
    
    public async Task<IActionResult> OnPostPaySecondaryMarket(string contractAddress, int ticketId, int price)
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userId)) return RedirectToPage("/LogIn");
            
            var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
            
            var user = await repo.LoadUserAsync(userId);
        
            var jobQueue = HttpContext.RequestServices.GetRequiredService<IJobQueue>();
            var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
        
            await jobQueue.EnqueueAsync(async ct =>
            {
                using var scope = scopeFactory.CreateScope();
                // todo include card details
                var acceptAskHandler = scope.ServiceProvider.GetRequiredService<AcceptAskHandler>();
                await acceptAskHandler.Execute(contractAddress, ticketId, price, user);
            });
            
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