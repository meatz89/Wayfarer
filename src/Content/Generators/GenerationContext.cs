using Wayfarer.GameState.Enums;

namespace Wayfarer.Content.Generators;

public class GenerationContext
{
    public int Tier { get; set; }
    public PersonalityType? NpcPersonality { get; set; }
    public string NpcLocationId { get; set; }
    public string NpcId { get; set; }
    public string NpcName { get; set; }
    public int PlayerCoins { get; set; }
    public List<LocationPropertyType> LocationProperties { get; set; } = new();

    public ServiceType ServiceType { get; set; } = ServiceType.Lodging;
    public ServiceQuality ServiceQuality { get; set; } = ServiceQuality.Standard;
    public SpotComfort SpotComfort { get; set; } = SpotComfort.Standard;
    public NPCDemeanor NpcDemeanor { get; set; } = NPCDemeanor.Neutral;

    public static GenerationContext Categorical(int tier)
    {
        return new GenerationContext
        {
            Tier = tier,
            NpcPersonality = null,
            NpcLocationId = null,
            NpcId = null,
            NpcName = "",
            PlayerCoins = 0,
            LocationProperties = new()
        };
    }

    public static GenerationContext FromEntities(
        int tier,
        NPC npc,
        Location location,
        Player player)
    {
        return new GenerationContext
        {
            Tier = tier,
            NpcPersonality = npc?.PersonalityType,
            NpcLocationId = npc?.Location?.Id,
            NpcId = npc?.ID,
            NpcName = npc?.Name ?? "",
            PlayerCoins = player?.Coins ?? 0,
            LocationProperties = location?.LocationProperties ?? new(),

            // Derive categorical properties from entity state
            ServiceQuality = DeriveServiceQuality(location),
            SpotComfort = DeriveSpotComfort(location),
            NPCDemeanor = DeriveNPCDemeanor(npc)
        };
    }

    /// <summary>
    /// Derive service quality tier from location tier.
    /// Maps location.Tier (1-4+) to categorical quality (Basic/Standard/Premium/Luxury).
    /// </summary>
    private static ServiceQuality DeriveServiceQuality(Location location)
    {
        if (location == null) return ServiceQuality.Standard;

        return location.Tier switch
        {
            1 => ServiceQuality.Basic,
            2 => ServiceQuality.Standard,
            3 => ServiceQuality.Premium,
            >= 4 => ServiceQuality.Luxury,
            _ => ServiceQuality.Standard
        };
    }

    /// <summary>
    /// Derive spot comfort from location properties.
    /// Checks LocationProperties for comfort-related descriptors.
    /// </summary>
    private static SpotComfort DeriveSpotComfort(Location location)
    {
        if (location == null || location.LocationProperties == null || location.LocationProperties.Count == 0)
            return SpotComfort.Standard;

        // Check for premium indicators
        if (location.LocationProperties.Contains(LocationPropertyType.luxurious) ||
            location.LocationProperties.Contains(LocationPropertyType.opulent))
        {
            return SpotComfort.Premium;
        }

        // Check for standard comfort indicators
        if (location.LocationProperties.Contains(LocationPropertyType.restful) ||
            location.LocationProperties.Contains(LocationPropertyType.comfortable))
        {
            return SpotComfort.Standard;
        }

        // Default to basic
        return SpotComfort.Basic;
    }

    /// <summary>
    /// Derive NPC demeanor from relationship flow.
    /// Maps RelationshipFlow ranges to Hostile/Neutral/Friendly demeanor.
    /// Ranges based on ConnectionState thresholds:
    /// - <= 9: DISCONNECTED/GUARDED → Hostile
    /// - <= 14: NEUTRAL → Neutral
    /// - >= 15: RECEPTIVE/TRUSTING → Friendly
    /// </summary>
    private static NPCDemeanor DeriveNPCDemeanor(NPC npc)
    {
        if (npc == null) return NPCDemeanor.Neutral;

        return npc.RelationshipFlow switch
        {
            <= 9 => NPCDemeanor.Hostile,   // DISCONNECTED (<=4) or GUARDED (<=9)
            <= 14 => NPCDemeanor.Neutral,  // NEUTRAL (<=14)
            _ => NPCDemeanor.Friendly      // RECEPTIVE (<=19) or TRUSTING (>19)
        };
    }
}
