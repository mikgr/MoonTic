using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Ticketer.Model;

namespace Ticketer.Web.Pages;

public record TicketPurchaseViewModel(
    string EventName, 
    string ContractAddress, 
    int TicketId, 
    DateTimeOffset TimeStamp, 
    decimal Price);



public class ProfileModel : PageModel
{
    public string Title => "Ticketer.Cli - Your Event Ticketing Solution";

    public User? CurrentUser;
  
    public string Address { get; set; } = "";
    public IEnumerable<TicketPurchaseViewModel> Purchases { get; set; } = new List<TicketPurchaseViewModel>();

    
    public async Task<IActionResult> OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId is null) return RedirectToPage("/LogIn");
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        CurrentUser = await repo.LoadUserAsync(userId);
        
        var userWallet =  repo.LoadUserWallet(userId);
        var fiatEvents =  repo.LoadFiatEventsFor(userId);

        await Task.WhenAll(userWallet, fiatEvents);
        Address = userWallet.Result.Address;

        var contractAddresses = fiatEvents.Result.Select(x => x.ContractAddress).Distinct().ToArray();

        var contractTasks = new List<Task<EventContract?>>();

        // todo dont do this lookup, just have the event name on the purchase event 
        foreach (var contractAddress in contractAddresses)
            contractTasks.Add(repo.LoadContractOrNullBy(contractAddress));
                
        await Task.WhenAll(contractTasks);
        
        var eventNameMap = new Dictionary<string, string>();
        
        foreach (var contractTask in contractTasks)
            if (contractTask.Result is {} contract)
                eventNameMap[contract.ContractAddress] = contract.Name;
        
        Purchases = fiatEvents.Result
            .OrderByDescending(x => x.TimestampUtc)
            .Select(x => new TicketPurchaseViewModel(
            eventNameMap.TryGetValue(x.ContractAddress, out var c) ? c : "Unknown Event",
            x.ContractAddress, 
            x.TicketId,
            x.TimestampUtc,
            x.TicketPrice));
      
        return Page();   
    }



    public IActionResult OnPostLogOut()
    {
        HttpContext.Session.SetInt32("UserId", -1);
        HttpContext.Session.SetString("UserName","");
        
        return Redirect("/Events");
    }
}