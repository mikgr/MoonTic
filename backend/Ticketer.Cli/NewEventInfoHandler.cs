using SpikeDb;
using Ticketer.Model;

namespace Ticketer;

public static class NewEventInfoHandler
{
    public static void Execute(
        User? currentUser,
        string name,
        DateTime venueOpenTime,
        DateTime venueCloseTime,
        int tickets,
        decimal price)
    {
        if (currentUser is null) throw new Exception("User not set");

        new EventInfo
        {
            Owner = currentUser.Id,
            Id = -1,
            Name = name,
            VenueOpenTime = venueOpenTime,
            VenueCloseTime = venueCloseTime,
            Tickets = tickets,
            Price = price,
            Description = "",
            BlockCheckOutBeforeVenueOpenInHours = 5 // todo dont default
        }.SpikePersistInt();
    }
}