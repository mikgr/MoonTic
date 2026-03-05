using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;

public class EventOrganizerModel : PageModel
{
    public List<EventInfo> DraftEvents { get; set; } = [];
    public List<EventContract> PublishedEvents { get; set; } = [];
    
    public async Task<IActionResult> OnGet()
    {
        if (await TryGetCurrentUser() is not {} currentUser)
            return RedirectToPage("/LogIn");
        
        await LoadDraftEvents(currentUser);
        
        return Page();
    }

    private async Task LoadDraftEvents(User currentUser)
    {
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        DraftEvents = await repo.EventInfo(currentUser.Id);
    }

    private async Task<User?> TryGetCurrentUser()
    {
        var userId = HttpContext.Session.GetString("UserId");
        User? currentUser = null;
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();

        if (string.IsNullOrWhiteSpace(userId))
            currentUser = null;
        else
            currentUser = await repo.LoadUserAsync(userId);
        
        return currentUser;
    }

    public async Task<IActionResult> OnPostPublish(string eventId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (userId is null) return RedirectToPage("/LogIn");
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        var currentUser = await repo.LoadUserAsync(userId);
        
        var publishEventHandler = HttpContext.RequestServices.GetRequiredService<PublishEventHandler>();
        await publishEventHandler.Execute(eventId, currentUser);
        
        return Redirect("/Events");
    }
    
    
    public async Task<IActionResult> OnPostCreateEvent(
        string eventName, DateTime venueOpenTime, DateTime venueCloseTime, int ticketCount, decimal price)
    {
        if (venueOpenTime >= venueCloseTime) 
            throw new ArgumentException($"{nameof(venueOpenTime)} must be before {nameof(venueCloseTime)}");
        
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId))
            return RedirectToPage("/LogIn");

        // todo validate with fluent validation
        
        // todo use handler
        var eventInfo = new EventInfo
        {
            Owner = userId,
            Name = eventName,
            VenueOpenTime = venueOpenTime,
            Tickets = ticketCount,
            Price = price,
            Description = "",
            BlockCheckOutBeforeVenueOpenInHours = 5,
            VenueCloseTime = venueCloseTime
        };

        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        await repo.Persist(eventInfo);
        
        return RedirectToPage("/EventOrganizer");
    }
    
    
    public async Task<IActionResult> OnGetDraftEvents()
    {
        if (await TryGetCurrentUser() is not {} currentUser) return RedirectToPage("/LogIn");
        
        await LoadDraftEvents(currentUser);
        
        return Partial("_DraftEvents", DraftEvents);
    }
    
    
    public async Task<IActionResult> OnGetPublishedEvents()
    {
        if (await TryGetCurrentUser() is not {} currentUser) return RedirectToPage("/LogIn");
        
        var repo = HttpContext.RequestServices.GetRequiredService<IRepository>();
        PublishedEvents = await repo.LoadContractsBy(ownerId: currentUser.Id); 
        
        return Partial("_PublishedEvents", PublishedEvents);
    }
}