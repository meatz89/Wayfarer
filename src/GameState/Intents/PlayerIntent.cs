using System;

/// <summary>
/// Base class for all player intents. Intents represent what the player wants to do,
/// without any execution logic or context. They are pure data objects.
/// </summary>
public abstract class PlayerIntent
{
    /// <summary>
    /// Unique identifier for this intent instance
    /// </summary>
    public string IntentId { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// When this intent was created
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Intent to move to a different location spot
/// </summary>
public class MoveIntent : PlayerIntent
{
    public string TargetSpotId { get; }

    public MoveIntent(string targetSpotId)
    {
        TargetSpotId = targetSpotId ?? throw new ArgumentNullException(nameof(targetSpotId));
    }
}

/// <summary>
/// Intent to have a conversation with an NPC
/// </summary>
public class TalkIntent : PlayerIntent
{
    public string NpcId { get; }

    public TalkIntent(string npcId)
    {
        NpcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
    }
}

/// <summary>
/// Intent to rest for a certain number of hours
/// </summary>
public class RestIntent : PlayerIntent
{
    public int Hours { get; }

    public RestIntent(int hours)
    {
        if (hours <= 0) throw new ArgumentException("Rest hours must be positive", nameof(hours));
        Hours = hours;
    }
}

/// <summary>
/// Intent to deliver a letter to its recipient
/// </summary>
public class DeliverLetterIntent : PlayerIntent
{
    public string LetterId { get; }

    public DeliverLetterIntent(string letterId)
    {
        LetterId = letterId ?? throw new ArgumentNullException(nameof(letterId));
    }
}

/// <summary>
/// Intent to collect a letter from a sender
/// </summary>
public class CollectLetterIntent : PlayerIntent
{
    public string LetterId { get; }

    public CollectLetterIntent(string letterId)
    {
        LetterId = letterId ?? throw new ArgumentNullException(nameof(letterId));
    }
}

/// <summary>
/// Intent to observe the current location
/// </summary>
public class ObserveLocationIntent : PlayerIntent
{
}

/// <summary>
/// Intent to explore an area for discoveries
/// </summary>
public class ExploreAreaIntent : PlayerIntent
{
}

/// <summary>
/// Intent to request funds from patron
/// </summary>
public class RequestPatronFundsIntent : PlayerIntent
{
}

/// <summary>
/// Intent to accept a letter board offer
/// </summary>
public class AcceptLetterOfferIntent : PlayerIntent
{
    public string OfferId { get; }

    public AcceptLetterOfferIntent(string offerId)
    {
        OfferId = offerId ?? throw new ArgumentNullException(nameof(offerId));
    }
}

/// <summary>
/// Intent to travel to another location via a route
/// </summary>
public class TravelIntent : PlayerIntent
{
    public string RouteId { get; }

    public TravelIntent(string routeId)
    {
        RouteId = routeId ?? throw new ArgumentNullException(nameof(routeId));
    }
}

/// <summary>
/// Intent to discover a route from an NPC
/// </summary>
public class DiscoverRouteIntent : PlayerIntent
{
    public string NpcId { get; }
    public string RouteId { get; }

    public DiscoverRouteIntent(string npcId, string routeId)
    {
        NpcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        RouteId = routeId ?? throw new ArgumentNullException(nameof(routeId));
    }
}

/// <summary>
/// Intent to convert endorsements to a guild seal
/// </summary>
public class ConvertEndorsementsIntent : PlayerIntent
{
    public string LocationId { get; }
    public string TargetTier { get; }

    public ConvertEndorsementsIntent(string locationId, string targetTier)
    {
        LocationId = locationId ?? throw new ArgumentNullException(nameof(locationId));
        TargetTier = targetTier ?? throw new ArgumentNullException(nameof(targetTier));
    }
}