using SpikeDb;

namespace Ticketer.Model;

public class EventInfo : ISpikeObjIntKey
{
    public required int Id { get; set; }
    public required int Owner { get; init; }
    public required string Name { get; init; }
    public required string Description { get; set; } = "";
    public required DateTime VenueOpenTime { get; init; }

    public required uint BlockCheckOutBeforeVenueOpenInHours
    {
        get;
        init
        {
            if (value < 5) throw new DomainInvariant($"{nameof(value)} Cannot be less than 5 hours");
            field = value;
        }
    } = 10;

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