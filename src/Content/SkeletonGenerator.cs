
/// <summary>
/// Generates mechanically complete but narratively generic skeleton content
/// for missing references. These skeletons allow the game to run without AI
/// while supporting lazy content resolution.
/// </summary>
public static class SkeletonGenerator
{
    private static readonly Random _random = new Random();

    // Generic name templates
    private static readonly string[] GenericNpcNames =
    {
    "Unnamed Merchant", "Unknown Traveler", "Mysterious Figure",
    "Local Resident", "Wandering Soul", "Silent Observer"
};

    private static readonly string[] GenericLocationNames =
    {
    "Unknown District", "Unexplored Area", "Mysterious Place",
    "Hidden Location", "Undiscovered Region", "Remote location"
};

    private static readonly string[] GenericSpotNames =
    {
    "Central Area", "Quiet Corner", "Main Square",
    "Side Street", "Meeting Point", "Resting Place"
};

    /// <summary>
    /// Generate a skeleton NPC with mechanical defaults
    /// NOTE: Skeleton generation is TECHNICAL DEBT - will be deleted when JSON refactored to hex coordinates
    /// </summary>
    public static NPC GenerateSkeletonNPC(string id, string source)
    {
        // Skeletons are fallback for broken JSON references (should not exist with hex architecture)
        NPC npc = new NPC
        {
            // NPC no longer has ID property (HIGHLANDER pattern - Name is natural key)
            Name = $"{GenericNpcNames[0]} (missing content)",  // Simple default, no hash
            Description = "This person's story has not yet been told.",
            Role = "Unknown",
            IsSkeleton = true,
            SkeletonSource = source,

            // Simple defaults (no hash-based selection)
            PersonalityType = PersonalityType.Neutral,  // Default personality
            Profession = Professions.Commoner,  // Default profession
            Tier = 1,  // Default tier

            // Default relationship flow (NEUTRAL state at neutral position)
            RelationshipFlow = 12
        };

        // Initialize card decks (required for NPC to function)
        npc.ExchangeDeck = new List<ExchangeCard>();

        return npc;
    }

    /// <summary>
    /// Generate a skeleton venue with mechanical defaults
    /// Venue is a CONTAINER - gameplay properties belong on Location
    /// NOTE: Skeleton generation is TECHNICAL DEBT - will be deleted when JSON refactored to hex coordinates
    /// </summary>
    public static Venue GenerateSkeletonVenue(string id, string source)
    {
        // ADR-007: Constructor uses Name only (no Id parameter)
        Venue venue = new Venue($"{id} - {GenericLocationNames[0]} (missing content)")
        {
            Description = "This place remains to be discovered.",
            IsSkeleton = true,
            SkeletonSource = source,
            Tier = 1,  // Default tier
            Type = VenueType.Wilderness // Default for skeleton venues
        };

        return venue;
    }

    /// <summary>
    /// Generate a skeleton Venue location with mechanical defaults
    /// Location is the gameplay entity with all mechanical properties
    /// NOTE: Skeleton generation is TECHNICAL DEBT - will be deleted when JSON refactored to hex coordinates
    /// </summary>
    public static Location GenerateSkeletonSpot(string id, string venueId, string source)
    {
        // ADR-007: Constructor uses Name only (no Id parameter)
        Location location = new Location($"{id} - {GenericSpotNames[0]} (missing content)")
        {
            // ADR-007: VenueId deleted - venue assignment happens during parsing/resolution
            // Skeleton locations will have Venue assigned when venue is resolved
            IsSkeleton = true,
            SkeletonSource = source,

            // Simple defaults (no hash-based selection)
            Tier = 1,  // Default tier
            FlowModifier = 0,  // Neutral modifier
            // Knowledge system eliminated - Understanding resource replaces Knowledge tokens

            // Gameplay properties moved from Location
            Role = LocationRole.Connective,  // Default role for skeleton locations
            TravelTimeSegments = 1,  // Default travel time
            Difficulty = 1,  // Default difficulty

            // Orthogonal categorical dimensions
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Neutral,
            Activity = LocationActivity.Moderate,
            Purpose = LocationPurpose.Generic,
            Environment = LocationEnvironment.Indoor,
            Setting = LocationSetting.Urban
        };

        return location;
    }

    /// <summary>
    /// Generate a skeleton exchange card with mechanical defaults
    /// NOTE: Skeleton generation is TECHNICAL DEBT - will be deleted when JSON refactored to hex coordinates
    /// </summary>
    public static ExchangeCard GenerateSkeletonExchangeCard(string id, string npcId, string source)
    {
        ExchangeCard card = new ExchangeCard
        {
            // ExchangeCard uses object references, no ID property
            Name = "Exchange (missing content)",
            Description = "A simple trade of resources.",
            IsSkeleton = true,
            SkeletonSource = source,
            // HIGHLANDER: Npc object reference set during entity resolution, not at skeleton generation

            // Simple default exchange type
            ExchangeType = ExchangeType.Purchase,

            // Simple default cost
            Cost = new ExchangeCostStructure
            {
                Resources = new List<ResourceAmount>
            {
                new ResourceAmount { Type = ResourceType.Coins, Amount = 10 }
            }
            },

            // Simple default reward
            Reward = new ExchangeRewardStructure
            {
                Resources = new List<ResourceAmount>
            {
                new ResourceAmount { Type = ResourceType.Coins, Amount = 15 }
            }
            },

            // Default exchange properties
            SingleUse = false,
            SuccessRate = 100,

            // Generic flavor
            FlavorText = "An exchange of mutual benefit."
        };

        return card;
    }

    /// <summary>
    /// Check if an entity needs a skeleton and generate it
    /// </summary>
    public static bool TryGenerateSkeleton<T>(string name, string source, GameWorld gameWorld, out T skeleton) where T : class
    {
        skeleton = null;

        if (typeof(T) == typeof(NPC))
        {
            if (!gameWorld.NPCs.Any(n => n.Name == name))
            {
                skeleton = GenerateSkeletonNPC(name, source) as T;
                return true;
            }
        }
        else if (typeof(T) == typeof(Venue))
        {
            if (!gameWorld.Venues.Any(v => v.Name == name))
            {
                skeleton = GenerateSkeletonVenue(name, source) as T;
                return true;
            }
        }
        else if (typeof(T) == typeof(Location))
        {
            if (!gameWorld.Locations.Any(l => l.Name == name))
            {
                // Need to find or create parent Venue first
                string venueId = source.Contains("location_")
                    ? source.Substring(source.IndexOf("location_") + 9).Split('_')[0]
                    : "unknown_location";
                skeleton = GenerateSkeletonSpot(name, venueId, source) as T;
                return true;
            }
        }

        return false;
    }
}