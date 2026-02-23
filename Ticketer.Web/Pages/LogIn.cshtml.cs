using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;

namespace Ticketer.Web.Pages;

public class LogInModel : PageModel
{
    public string Title => "Ticketer.Cli - Your Event Ticketing Solution";

    public List<User> Items = [];
    
    public void OnGet()
    {
        var users = SpikeRepo.ReadCollection<User>();
        Items.AddRange(users);
    }
    
    public IActionResult OnPost(string userId)
    {
        var userIdInt = Convert.ToInt32(userId);
        var user = SpikeRepo.ReadIntId<User>(userIdInt);
        
        HttpContext.Session.SetInt32("UserId", userIdInt);
        HttpContext.Session.SetString("UserName", user.UserName);
        
        return RedirectToPage("/Events");
    }
}