
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
    public required string Description { get; set; } = "";
    public required DateTime VenueOpenTime { get; init; }

    public required uint BlockCheckOutBeforeVenueOpenInHours { get; init; } = 10;

    public required DateTime VenueCloseTime { get; init; }
    // venue address (optional) might be a virtual event
    public required decimal Price { get; set; }
    public required int Tickets { get; set; }
    // todo  change state on publish
}

// add publish time to event
// bids
// asks
// Artwork