using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;

namespace Ticketer.Web.Pages;

public class ProfileModel : PageModel
{
    public string Title => "Ticketer.Cli - Your Event Ticketing Solution";

    public User? CurrentUser;
  
    public string Address { get; set; } = "";
    public IEnumerable<TicketPurchaseViewModel> Purchases { get; set; } = new List<TicketPurchaseViewModel>();

    public record TicketPurchaseViewModel(
        string EventName, 
        string ContractAddress, 
        int TicketId, 
        DateTimeOffset TimeStamp, 
        decimal Price);
    
    public void OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId != null)
        {
            CurrentUser = SpikeRepo.ReadOrNullByInt<User>(userId.Value);
            Address = SpikeRepo.ReadSingleOrDefault<UserWallet>(x => x.UserId == userId)?.Address ?? "";

            var purchases = SpikeRepo.ReadCollection<TicketPurchasedEvent>(x => x.OwnerId == userId)
                .ToArray();
            var contractIds = purchases.Select(x => x.EventContractId).Distinct().ToArray();
            var contracts = SpikeRepo.ReadCollection<EventContract>(x => contractIds.Contains(x.Id));

            Purchases =
                from p in purchases
                join c in contracts on p.EventContractId equals c.Id
                orderby p.TimestampUtc descending 
                select new TicketPurchaseViewModel(
                    c.Name,
                    p.ContractAddress,
                    p.TicketId,
                    p.TimestampUtc,
                    p.TicketPrice
                );
        }
    }



    public IActionResult OnPostLogOut()
    {
        HttpContext.Session.SetInt32("UserId", -1);
        HttpContext.Session.SetString("UserName","");
        
        return Redirect("/Events");
    }
}