using Amazon.DynamoDBv2.DataModel;
using Ticketer.Model;

namespace Ticketer;

// todo use this in web code behind
public class NewEventInfoHandler(IDynamoDBContext dynamo)
{
    public async Task Execute(
        User currentUser,
        string name,
        string fullVenueAddress,
        DateTime venueOpenTime,
        DateTime venueCloseTime,
        string venueTimeZone,
        int tickets,
        decimal price,
        decimal maxResellPrice,
        string paymentStableCoinSymbol)
    {
        if (currentUser is null) throw new Exception("User not set");

        var eventInfo = new EventInfo
        {
            Owner = currentUser.Id,
            Name = name,
            FullVenueAddress = "",
            VenueOpenTime = venueOpenTime,
            VenueTimeZone = venueTimeZone,
            VenueCloseTime = venueCloseTime,
            Tickets = tickets,
            Price = price,
            Description = "",
            BlockCheckOutBeforeVenueOpenInHours = 5,
            MaxResellPrice = maxResellPrice,
            PaymentStableCoinSymbol = paymentStableCoinSymbol // todo dont default
        };
        
        await dynamo.SaveAsync(eventInfo);
        
    }
}