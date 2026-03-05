using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;

namespace Ticketer;

public class NewEventInfoHandler(IDynamoDBContext dynamo)
{
    public async Task Execute(
        User? currentUser,
        string name,
        DateTime venueOpenTime,
        DateTime venueCloseTime,
        int tickets,
        decimal price)
    {
        if (currentUser is null) throw new Exception("User not set");

        var eventInfo = new EventInfo
        {
            Owner = currentUser.Id,
            Name = name,
            VenueOpenTime = venueOpenTime,
            VenueCloseTime = venueCloseTime,
            Tickets = tickets,
            Price = price,
            Description = "",
            BlockCheckOutBeforeVenueOpenInHours = 5 // todo dont default
        };
        
        await dynamo.SaveAsync(eventInfo);
        
    }
}