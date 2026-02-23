using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;

namespace Ticketer.Web.Pages;

public class ProfileModel : PageModel
{
    public string Title => "Ticketer.Cli - Your Event Ticketing Solution";

    public User? CurrentUser;
    public string Balance { get; set; } = "n/a";
    public string Address { get; set; } = "";
    
    public void OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId != null)
        {
            CurrentUser = SpikeRepo.ReadOrNullByInt<User>((int)userId!);
            var account = SpikeRepo.ReadSingleOrDefault<Account>(x => x.UserId == userId);
            Balance = account?.Balance.ToString("0.00") ?? "0.00";
            
            Address = SpikeRepo.ReadSingleOrDefault<UserWallet>(x => x.UserId == userId)?.Address ?? "";
        }
    }


    public IActionResult OnPostLogOut()
    {
        HttpContext.Session.SetInt32("UserId", -1);
        HttpContext.Session.SetString("UserName","");
        
        return Redirect("/Events");
    }
}