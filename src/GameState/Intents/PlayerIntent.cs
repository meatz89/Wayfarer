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
/// Intent to move to a different Venue location
/// HIGHLANDER: Stores Location object, not string ID
/// </summary>
public class MoveIntent : PlayerIntent
{
    public Location TargetSpot { get; }

    public MoveIntent(Location targetSpot)
    {
        TargetSpot = targetSpot;
    }
}

/// <summary>
/// Intent to have a conversation with an NPC
/// HIGHLANDER: Stores NPC object, not string ID
/// </summary>
public class TalkIntent : PlayerIntent
{
    public NPC Npc { get; }

    public TalkIntent(NPC npc)
    {
        Npc = npc;
    }
}
/// <summary>
/// Intent to wait until the next time period (refreshes attention)
/// </summary>
public class WaitIntent : PlayerIntent
{
    public WaitIntent()
    {
        // No parameters needed - always advances to next time block
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
        LetterId = letterId;
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
/// Intent to travel to another Venue via a route
/// </summary>
public class TravelIntent : PlayerIntent
{
    public RouteOption Route { get; }

    public TravelIntent(RouteOption route)
    {
        Route = route;
    }
}

/// <summary>
/// Intent to discover a route from an NPC
/// HIGHLANDER: Stores typed objects, not string IDs
/// </summary>
public class DiscoverRouteIntent : PlayerIntent
{
    public NPC Npc { get; }
    public RouteOption Route { get; }

    public DiscoverRouteIntent(NPC npc, RouteOption route)
    {
        Npc = npc;
        Route = route;
    }
}

/// <summary>
/// Intent to view player's equipment and inventory
/// Triggers navigation to equipment screen - backend decides
/// </summary>
public class CheckBelongingsIntent : PlayerIntent
{
}

/// <summary>
/// Intent to engage with an NPC
/// Spawns scene for NPC engagement, shows situations/choices
/// This is the CORRECT way to interact with NPCs (not direct challenge execution)
/// HIGHLANDER: Stores NPC object, not string ID
/// </summary>
public class EngageNPCIntent : PlayerIntent
{
    public NPC Npc { get; }

    public EngageNPCIntent(NPC npc)
    {
        Npc = npc ?? throw new ArgumentNullException(nameof(npc));
    }
}

/// <summary>
/// Intent to look around at current location
/// Triggers navigation to LookingAround view showing NPCs, challenges, opportunities
/// </summary>
public class LookAroundIntent : PlayerIntent
{
}

/// <summary>
/// Intent to sleep rough without shelter
/// Uses PlayerAction entity for data-driven health cost
/// </summary>
public class SleepOutsideIntent : PlayerIntent
{
}

/// <summary>
/// Intent to rest at current location
/// Uses LocationAction entity for data-driven recovery rewards
/// Replaces old RestIntent(int segments) with data-driven approach
/// </summary>
public class RestAtLocationIntent : PlayerIntent
{
}

/// <summary>
/// Intent to secure a paid room for full recovery
/// Uses LocationAction entity for cost and fullRecovery flag
/// </summary>
public class SecureRoomIntent : PlayerIntent
{
}

/// <summary>
/// Intent to work at current location for coins
/// Uses LocationAction entity for data-driven coin rewards
/// </summary>
public class WorkIntent : PlayerIntent
{
}

/// <summary>
/// Intent to investigate current location for familiarity
/// Delegates to LocationFacade
/// </summary>
public class InvestigateLocationIntent : PlayerIntent
{
}

/// <summary>
/// Intent to open the Travel screen to view available routes
/// Different from TravelIntent which executes travel to a specific destination
/// </summary>
public class OpenTravelScreenIntent : PlayerIntent
{
}

/// <summary>
/// Intent to view job board at current Commercial location
/// Opens modal showing available delivery jobs
/// </summary>
public class ViewJobBoardIntent : PlayerIntent
{
}

/// <summary>
/// Intent to accept a delivery job
/// Player can only have ONE active job at a time
/// </summary>
public class AcceptDeliveryJobIntent : PlayerIntent
{
    public DeliveryJob Job { get; }

    public AcceptDeliveryJobIntent(DeliveryJob job)
    {
        Job = job;
    }
}

/// <summary>
/// Intent to complete active delivery job at destination
/// Pays player and clears active job
/// </summary>
public class CompleteDeliveryIntent : PlayerIntent
{
}

