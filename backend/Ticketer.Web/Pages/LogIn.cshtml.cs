using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Ticketer.Model;

namespace Ticketer.Web.Pages;

public class LogInModel : PageModel
{
    public string Title => "Ticketer.Cli - Your Event Ticketing Solution";

  
    
    public void OnGet()
    {
        
    }
    
    public async Task<IActionResult> OnPost(string userName)
    {
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();

        var user = await repo.LoadUserAsync(userName.ToLower());
        
        HttpContext.Session.SetString("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.UserName);
        
        return RedirectToPage("/Events");
    }
}