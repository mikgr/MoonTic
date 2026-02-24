﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpikeDb;
using Ticketer.Model;
using Ticketer.UseCases;

namespace Ticketer.Web.Pages;

public class EventOrganizerModel : PageModel
{
    public List<EventInfo> DraftEvents { get; set; } = [];
    public List<EventContract> PublishedEvents { get; set; } = [];
    
    public IActionResult OnGet()
    {
        if (TryGetCurrentUser() is not {} currentUser)
            return RedirectToPage("/LogIn");
        
        LoadDraftEvents(currentUser);
        
        return Page();
    }

    private void LoadDraftEvents(User currentUser)
    {
        DraftEvents = SpikeRepo
            .ReadCollection<EventInfo>(x => x.Owner == currentUser.Id)
            .ToList();
    }

    private User? TryGetCurrentUser()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        User? currentUser = null;
        if (userId is null or -1)
            currentUser = null;
        else 
            currentUser = SpikeRepo.ReadIntId<User>(userId.Value);
        return currentUser;
    }

    public async Task<IActionResult> OnPostPublish(int eventId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null) return RedirectToPage("/LogIn");
        
        var currentUser = SpikeRepo.ReadIntId<User>(userId.Value);
        
        var publishEventHandler = HttpContext.RequestServices.GetRequiredService<PublishEventHandler>();
        await publishEventHandler.Execute(eventId, currentUser);
        
        return Redirect("/Events");
    }
    
    public IActionResult OnPostCreateEvent(string eventName, DateTime venueOpenTime, DateTime venueCloseTime, int ticketCount, decimal price)
    {
        if (venueOpenTime >= venueCloseTime) 
            throw new ArgumentException($"{nameof(venueOpenTime)} must be before {nameof(venueCloseTime)}");
        
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null or -1)
            return RedirectToPage("/LogIn");

        // todo validate with fluent validation
        
        // todo use handler
        new EventInfo
        {
            Owner = userId.Value,
            Id = -1,
            Name = eventName,
            VenueOpenTime = venueOpenTime,
            Tickets = ticketCount,
            Price = price,
            Description = "",
            BlockCheckOutBeforeVenueOpenInHours = 5,
            VenueCloseTime = venueCloseTime
        }.SpikePersistInt();

        return RedirectToPage("/EventOrganizer");
    }
    
    public IActionResult OnGetDraftEvents()
    {
        if (TryGetCurrentUser() is not {} currentUser) return RedirectToPage("/LogIn");
        
        LoadDraftEvents(currentUser);
        
        return Partial("_DraftEvents", DraftEvents);
    }
    
    public IActionResult OnGetPublishedEvents()
    {
        if (TryGetCurrentUser() is not {} currentUser) return RedirectToPage("/LogIn");
        
        var eventContracts = SpikeRepo.ReadCollection<EventContract>(x => x.OwnerId == currentUser.Id);
        PublishedEvents = eventContracts.ToList();
        
        return Partial("_PublishedEvents", PublishedEvents);
    }
}