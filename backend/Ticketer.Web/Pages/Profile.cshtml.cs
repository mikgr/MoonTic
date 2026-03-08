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
        
        var userWallet = await repo.LoadUserWallet(userId);
        Address = userWallet.Address;

        var fiatEvents = await repo.LoadFiatEventsFor(userId);

        var contractAddresses = fiatEvents.Select(x => x.ContractAddress).Distinct().ToArray();

        var eventNameMap = new Dictionary<string, string>();
        
        foreach (var contractAddress in contractAddresses)
            if (await repo.LoadContractOrNullBy(contractAddress) is {} contract)
                eventNameMap[contractAddress] = contract.Name;
            else 
                eventNameMap[contractAddress] = "(unknown event)";
        
        Purchases = fiatEvents
            .OrderByDescending(x => x.TimestampUtc)
            .Select(x => new TicketPurchaseViewModel(
            eventNameMap[x.ContractAddress],
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