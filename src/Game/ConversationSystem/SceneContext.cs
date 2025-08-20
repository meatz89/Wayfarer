/// <summary>
/// Context for scenes including conversations, locations, and narrative state.
/// Combines game mechanics with literary UI presentation.
/// </summary>
public class SceneContext
{
    public GameWorld GameWorld { get; set; }
    public Player Player { get; set; }

    // Location context
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public List<string> LocationProperties { get; set; }

    // Conversation context
    public NPC TargetNPC { get; set; }
    public string ConversationTopic { get; set; }
    public int StartingFocusPoints { get; set; }

    // Relationship tracking
    public Dictionary<ConnectionType, int> CurrentTokens { get; set; }
    public int RelationshipLevel { get; set; }

    // Conversation flow
    public List<string> ConversationHistory { get; set; } = new List<string>();

    // Literary UI context tags
    public List<PressureTag> PressureTags { get; set; } = new List<PressureTag>();
    public List<RelationshipTag> RelationshipTags { get; set; } = new List<RelationshipTag>();
    public List<DiscoveryTag> DiscoveryTags { get; set; } = new List<DiscoveryTag>();
    public List<ResourceTag> ResourceTags { get; set; } = new List<ResourceTag>();
    public List<FeelingTag> FeelingTags { get; set; } = new List<FeelingTag>();

    // Attention system
    public AttentionManager AttentionManager { get; set; }

    // Scene pressure metrics
    public int MinutesUntilDeadline { get; set; }
    public int ObligationQueueSize { get; set; }

    // Factory method for standard scene context
    public static SceneContext Standard(GameWorld gameWorld, Player player, NPC targetNPC, Location location, LocationSpot spot)
    {
        return new SceneContext
        {
            GameWorld = gameWorld,
            Player = player,
            TargetNPC = targetNPC,
            LocationName = location?.Name ?? "",
            LocationSpotName = spot?.Name ?? "",
            LocationProperties = GetCurrentLocationProperties(location, gameWorld) ?? new List<string>(),
            StartingFocusPoints = player?.Concentration ?? 0,
            CurrentTokens = new Dictionary<ConnectionType, int>(),
            RelationshipLevel = 0
        };
    }

    private static List<string> GetCurrentLocationProperties(Location location, GameWorld gameWorld)
    {
        if (location == null) return new List<string>();

        // Get current time - defaulting to morning if not available
        TimeBlocks timeBlock = TimeBlocks.Morning;
        if (gameWorld != null)
        {
            Player player = gameWorld.GetPlayer();
            if (player != null)
            {
                // We could get time from game state if needed
                // For now default to morning
            }
        }

        return timeBlock switch
        {
            TimeBlocks.Dawn => location.MorningProperties,
            TimeBlocks.Morning => location.MorningProperties,
            TimeBlocks.Afternoon => location.AfternoonProperties,
            TimeBlocks.Evening => location.EveningProperties,
            TimeBlocks.Night => location.NightProperties,
            _ => location.MorningProperties
        };
    }
}

/// <summary>
/// Extended context for queue management conversations
/// </summary>
public class QueueManagementContext : SceneContext
{
    public DeliveryObligation TargetDeliveryObligation { get; set; }
    public string ManagementAction { get; set; } // "SkipDeliver", "Purge", etc.
    public int TokenCost { get; set; }
    public Dictionary<int, Letter> SkippedLetters { get; set; } // For skip action - letters that would be skipped
}

/// <summary>
/// Extended context for action-based conversations
/// </summary>
public class ActionConversationContext : SceneContext
{
    public string InitialNarrative { get; set; }
    public List<ChoiceTemplate> AvailableTemplates { get; set; }
}

/// <summary>
/// Extended context for travel conversations
/// </summary>
public class TravelConversationContext : SceneContext
{
    public RouteOption Route { get; set; }
    public Location Origin { get; set; }
    public Location Destination { get; set; }
}