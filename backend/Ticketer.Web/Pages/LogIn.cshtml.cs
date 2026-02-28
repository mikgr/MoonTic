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
    
    public IActionResult OnPost(string userName)
    {
        var user = SpikeRepo.ReadSingle<User>(x => x.UserName.ToLower() == userName.ToLower());
        
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.UserName);
        
        return RedirectToPage("/Events");
    }
}