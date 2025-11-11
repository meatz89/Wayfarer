/// <summary>
/// UNIVERSAL generation context for ALL scene and situation archetypes.
///
/// Contains categorical properties derived from entity state that scale
/// procedural content generation across all domains: services, combat,
/// romance, investigation, stealth, politics, etc.
///
/// Supports both NPC-placed scenes (with NPC context) and Location-placed scenes (without NPC).
///
/// NO domain-specific properties. All properties are universal and apply
/// to multiple situation types.
/// </summary>
public class GenerationContext
{
    // Tier (unchanged - universal difficulty scalar)
    public int Tier { get; set; }

    // A-Story Sequence (for infinite main story progression)
    // null for non-A-story scenes, sequence number (11+) for A-story scenes
    // Used to calculate next A-scene ID for final situation spawn rewards
    public int? AStorySequence { get; set; }

    // Entity IDs for RequiredLocationId/RequiredNpcId in situations
    public string LocationId { get; set; }  // Base location ID (ALWAYS present)
    public string NpcId { get; set; }       // NPC ID (null for location-only scenes)

    // NPC Context (for categorical property derivation)
    public PersonalityType? NpcPersonality { get; set; }
    public string NpcName { get; set; }

    // Player Context
    public int PlayerCoins { get; set; }
    public int PlayerHealth { get; set; }

    // Location Context
    public List<LocationPropertyType> LocationProperties { get; set; } = new();

    // UNIVERSAL CATEGORICAL PROPERTIES (apply to ALL archetypes)
    public DangerLevel Danger { get; set; } = DangerLevel.Safe;
    public SocialStakes Stakes { get; set; } = SocialStakes.Witnessed;
    public TimePressure Urgency { get; set; } = TimePressure.Leisurely;
    public PowerDynamic Power { get; set; } = PowerDynamic.Equal;
    public EmotionalTone Tone { get; set; } = EmotionalTone.Cold;
    public MoralClarity Morality { get; set; } = MoralClarity.Ambiguous;
    public Quality Quality { get; set; } = Quality.Standard;
    public EnvironmentQuality Environment { get; set; } = EnvironmentQuality.Standard;
    public NPCDemeanor NpcDemeanor { get; set; } = NPCDemeanor.Neutral;

    /// <summary>
    /// Create categorical context (tier-based only, no entity derivation).
    /// Used for abstract archetype testing.
    /// </summary>
    public static GenerationContext Categorical(int tier)
    {
        return new GenerationContext
        {
            Tier = tier,
            LocationId = null,
            NpcId = null,
            NpcPersonality = null,
            NpcName = "",
            PlayerCoins = 0,
            PlayerHealth = 100,
            LocationProperties = new()
        };
    }

    /// <summary>
    /// Create generation context from entities with automatic categorical property derivation.
    ///
    /// Supports two scenarios:
    /// 1. NPC-placed scenes: npc != null, location = NPC's location
    ///    - LocationId = NPC's location ID
    ///    - NpcId = NPC's ID
    ///    - Full categorical properties (including NPC-derived)
    ///
    /// 2. Location-placed scenes: npc == null, location = placement location
    ///    - LocationId = placement location ID
    ///    - NpcId = null
    ///    - Limited categorical properties (no NPC-derived properties)
    ///
    /// ALL universal properties are derived from entity state.
    /// NO manual property setting required.
    /// </summary>
    public static GenerationContext FromEntities(
        int tier,
        NPC npc,
        Location location,
        Player player,
        int? mainStorySequence = null)
    {
        return new GenerationContext
        {
            Tier = tier,
            AStorySequence = mainStorySequence,

            // Entity IDs (for RequiredLocationId/RequiredNpcId)
            LocationId = location?.Id,  // Base location (NPC's location OR placement location)
            NpcId = npc?.ID,            // NPC ID (null for location-only scenes)

            // NPC context
            NpcPersonality = npc?.PersonalityType,
            NpcName = npc?.Name ?? "",

            // Player context
            PlayerCoins = player?.Coins ?? 0,
            PlayerHealth = player?.Health ?? 100,

            // Location context
            LocationProperties = location?.LocationProperties ?? new(),

            // UNIVERSAL CATEGORICAL PROPERTIES (auto-derived)
            Danger = DeriveDangerLevel(location, npc, player),
            Stakes = DeriveSocialStakes(location),
            Urgency = DeriveTimePressure(location, player),
            Power = DerivePowerDynamic(npc, player),
            Tone = DeriveEmotionalTone(npc),
            Morality = DeriveMoralClarity(npc, location),
            Quality = DeriveQuality(location),
            Environment = DeriveEnvironmentQuality(location),
            NpcDemeanor = DeriveNPCDemeanor(npc)
        };
    }

    /// <summary>
    /// Derive danger level from location properties, NPC hostility, and player state.
    ///
    /// Scales: Crisis consequences, Physical challenge damage, Confrontation escalation
    /// </summary>
    private static DangerLevel DeriveDangerLevel(Location location, NPC npc, Player player)
    {
        // Check location properties for danger indicators (Guarded, Outdoor=wilderness, Checkpoint=authority)
        if (location?.LocationProperties.Any(p =>
            p == LocationPropertyType.Guarded ||
            p == LocationPropertyType.Outdoor) ?? false)
        {
            return DangerLevel.Risky;
        }

        // Check NPC hostility
        if (npc?.RelationshipFlow <= 5) return DangerLevel.Risky;

        // Check player health
        if (player?.Health < 30) return DangerLevel.Risky;

        return DangerLevel.Safe;
    }

    /// <summary>
    /// Derive social stakes from location properties.
    ///
    /// Scales: Reputation impact, face-saving costs, romance intimacy options
    /// </summary>
    private static SocialStakes DeriveSocialStakes(Location location)
    {
        if (location?.LocationProperties.Any(p =>
            p == LocationPropertyType.Public ||
            p == LocationPropertyType.Market) ?? false)
        {
            return SocialStakes.Public;
        }

        if (location?.LocationProperties.Any(p =>
            p == LocationPropertyType.Private ||
            p == LocationPropertyType.Isolated ||
            p == LocationPropertyType.Intimate) ?? false)
        {
            return SocialStakes.Private;
        }

        return SocialStakes.Witnessed;
    }

    /// <summary>
    /// Derive time pressure from location properties and player state.
    ///
    /// Scales: Available choices, time costs, retry availability
    /// </summary>
    private static TimePressure DeriveTimePressure(Location location, Player player)
    {
        // Check for crisis/emergency location properties (Guarded, Official = authority pressure)
        if (location?.LocationProperties.Any(p =>
            p == LocationPropertyType.Guarded ||
            p == LocationPropertyType.Official) ?? false)
        {
            return TimePressure.Urgent;
        }

        // Default to leisurely (time pressure requires specialized context)
        return TimePressure.Leisurely;
    }

    /// <summary>
    /// Derive power dynamic from NPC tier (authority concept removed from architecture).
    ///
    /// Scales: Confrontation difficulty, Negotiation leverage, Social_maneuvering thresholds
    /// </summary>
    private static PowerDynamic DerivePowerDynamic(NPC npc, Player player)
    {
        if (npc == null) return PowerDynamic.Equal;

        // Use NPC tier as power indicator (1=low, 5=high authority)
        return npc.Tier switch
        {
            >= 4 => PowerDynamic.Submissive,  // High tier NPC = player submissive
            <= 2 => PowerDynamic.Dominant,    // Low tier NPC = player dominant
            _ => PowerDynamic.Equal           // Mid tier = equal footing
        };
    }

    /// <summary>
    /// Derive emotional tone from NPC bond with player.
    ///
    /// Scales: Social_maneuvering rewards, Negotiation rapport bonuses, Romance options
    /// </summary>
    private static EmotionalTone DeriveEmotionalTone(NPC npc)
    {
        if (npc == null) return EmotionalTone.Cold;

        int bond = npc.BondStrength;

        // High positive bond = Passionate (love)
        if (bond >= 15) return EmotionalTone.Passionate;

        // Medium bond = Warm (friendship)
        if (bond >= 8) return EmotionalTone.Warm;

        // Very low bond = Passionate (hate)
        if (bond <= 3) return EmotionalTone.Passionate;

        // Default = Cold (professional)
        return EmotionalTone.Cold;
    }

    /// <summary>
    /// Derive moral clarity from NPC personality and location properties.
    ///
    /// Scales: Narrative framing, conscience tracking, faction reputation
    /// </summary>
    private static MoralClarity DeriveMoralClarity(NPC npc, Location location)
    {
        // No CRUEL personality type in current 5-type system (DEVOTED/MERCANTILE/PROUD/CUNNING/STEADFAST)
        // All personalities can present moral ambiguity based on context

        // Holy locations = clear moral context
        if (location?.LocationProperties.Any(p =>
            p == LocationPropertyType.Temple) ?? false)
        {
            return MoralClarity.Clear;
        }

        // Most situations are morally ambiguous
        return MoralClarity.Ambiguous;
    }

    /// <summary>
    /// Derive quality tier from location tier.
    ///
    /// Scales: Costs (Basic 0.6x, Standard 1.0x, Premium 1.6x, Luxury 2.4x)
    /// Applies to: Services, items, equipment across ALL domains
    /// </summary>
    private static Quality DeriveQuality(Location location)
    {
        if (location == null) return Quality.Standard;

        return location.Tier switch
        {
            1 => Quality.Basic,
            2 => Quality.Standard,
            3 => Quality.Premium,
            >= 4 => Quality.Luxury,
            _ => Quality.Standard
        };
    }

    /// <summary>
    /// Derive environment quality from location properties.
    ///
    /// Scales: Restoration multiplier (Basic 1x, Standard 2x, Premium 3x)
    /// Applies to: Rest, study, crafting, recovery across ALL domains
    /// </summary>
    private static EnvironmentQuality DeriveEnvironmentQuality(Location location)
    {
        if (location?.LocationProperties == null || location.LocationProperties.Count == 0)
            return EnvironmentQuality.Standard;

        // Premium environment (luxurious, opulent)
        if (location.LocationProperties.Any(p =>
            p == LocationPropertyType.Wealthy ||
            p == LocationPropertyType.Prestigious))
        {
            return EnvironmentQuality.Premium;
        }

        // Standard environment (comfortable, restful)
        if (location.LocationProperties.Any(p =>
            p == LocationPropertyType.Restful ||
            p == LocationPropertyType.Cozy))
        {
            return EnvironmentQuality.Standard;
        }

        // Basic environment (rough, minimal)
        return EnvironmentQuality.Basic;
    }

    /// <summary>
    /// Derive NPC demeanor from relationship flow.
    ///
    /// Scales: Stat thresholds (Hostile 1.4x, Neutral 1.0x, Friendly 0.6x)
    /// Applies to: ALL NPC interactions
    /// </summary>
    private static NPCDemeanor DeriveNPCDemeanor(NPC npc)
    {
        if (npc == null) return NPCDemeanor.Neutral;

        return npc.RelationshipFlow switch
        {
            <= 9 => NPCDemeanor.Hostile,   // DISCONNECTED/GUARDED
            <= 14 => NPCDemeanor.Neutral,  // NEUTRAL
            _ => NPCDemeanor.Friendly      // RECEPTIVE/TRUSTING
        };
    }
}
