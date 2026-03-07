
using Amazon.DynamoDBv2.DataModel;

namespace Ticketer.Model;

[DynamoDBTable("EventInfo")]
public class EventInfo 
{
    [DynamoDBHashKey]
    public required string Owner { get; init; }
    
    [DynamoDBRangeKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // todo use this as partition key
    public required string Name { get; init; }
    public required string FullVenueAddress { get; set; }
    public required string Description { get; set; } = "";
    public required DateTime VenueOpenTime { get; init; } // todo rename to UTC
    public required DateTime VenueCloseTime { get; init; } // todo rename to UTC
    public required string VenueTimeZone { get; init; } // todo add tests 

    public required uint BlockCheckOutBeforeVenueOpenInHours { get; init; } = 10;

    // venue address (optional) might be a virtual event
    public required decimal Price { get; set; }
    public required decimal MaxResellPrice { get; set; }
    public required int Tickets { get; set; }
    public required string PaymentStableCoinSymbol { get; init; }

    // todo  change state on publish
}

// add publish time to event
// bids
// asks
// Artwork