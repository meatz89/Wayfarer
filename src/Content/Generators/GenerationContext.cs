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
    // Used ONLY to calculate next A-scene ID for final situation spawn rewards
    // NOT used for archetype branching (see arc42 §8.23 - archetype reusability)
    // FORBIDDEN: if (AStorySequence == N) branching in choice generation
    public int? AStorySequence { get; set; }

    // HIGHLANDER: Entity objects for situation placement (not string IDs)
    public Location Location { get; set; }  // Base location (ALWAYS present)
    public NPC Npc { get; set; }            // NPC (null for location-only scenes)

    // NPC Context (for categorical property derivation)
    public PersonalityType? NpcPersonality { get; set; }
    public string NpcName { get; set; }

    // Player Context
    public int PlayerCoins { get; set; }
    public int PlayerHealth { get; set; }

    // Location Context (orthogonal categorical dimensions)
    public LocationRole? LocationRole { get; set; }
    public LocationPurpose? LocationPurpose { get; set; }

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

    // Sir Brante rhythm pattern - AUTHORED (not derived)
    // Determines choice generation pattern and consequence polarity
    // See arc42/08_crosscutting_concepts.md §8.26
    public RhythmPattern Rhythm { get; set; } = RhythmPattern.Mixed;

    /// <summary>
    /// Create categorical context (tier-based only, no entity derivation).
    /// Used for abstract archetype testing.
    /// HIGHLANDER: No entities assigned (null Location/Npc)
    /// </summary>
    public static GenerationContext Categorical(int tier)
    {
        return new GenerationContext
        {
            Tier = tier,
            Location = null,
            Npc = null,
            NpcPersonality = null,
            NpcName = "",
            PlayerCoins = 0,
            PlayerHealth = 100
        };
    }

    /// <summary>
    /// Create generation context from entities with automatic categorical property derivation.
    ///
    /// Supports two scenarios:
    /// 1. NPC-placed scenes: npc != null, location = NPC's location
    ///    - Location = NPC's location object
    ///    - Npc = NPC object
    ///    - Full categorical properties (including NPC-derived)
    ///
    /// 2. Location-placed scenes: npc == null, location = placement location
    ///    - Location = placement location object
    ///    - Npc = null
    ///    - Limited categorical properties (no NPC-derived properties)
    ///
    /// ALL universal properties are derived from entity state.
    /// NO manual property setting required.
    /// HIGHLANDER: Store entity objects, not string IDs
    ///
    /// RhythmPattern is AUTHORED (not derived) - comes from SceneTemplate.
    /// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
    /// </summary>
    public static GenerationContext FromEntities(
        int tier,
        NPC npc,
        Location location,
        Player player,
        int? mainStorySequence = null,
        RhythmPattern rhythm = RhythmPattern.Mixed)
    {
        return new GenerationContext
        {
            Tier = tier,
            AStorySequence = mainStorySequence,

            // HIGHLANDER: Entity objects (not IDs)
            Location = location,  // Base location (NPC's location OR placement location)
            Npc = npc,            // NPC (null for location-only scenes)

            // NPC context
            NpcPersonality = npc?.PersonalityType,
            NpcName = npc?.Name ?? "",

            // Player context
            PlayerCoins = player?.Coins ?? 0,
            PlayerHealth = player?.Health ?? 100,

            // Location context
            LocationRole = location?.Role,
            LocationPurpose = location?.Purpose,

            // UNIVERSAL CATEGORICAL PROPERTIES (auto-derived)
            Danger = DeriveDangerLevel(location, npc, player),
            Stakes = DeriveSocialStakes(location),
            Urgency = DeriveTimePressure(location, player),
            Power = DerivePowerDynamic(npc, player),
            Tone = DeriveEmotionalTone(npc),
            Morality = DeriveMoralClarity(npc, location),
            Quality = DeriveQuality(location),
            Environment = DeriveEnvironmentQuality(location),
            NpcDemeanor = DeriveNPCDemeanor(npc),

            // Sir Brante rhythm pattern (AUTHORED, not derived)
            Rhythm = rhythm
        };
    }

    /// <summary>
    /// Derive danger level from location properties, NPC hostility, and player state.
    ///
    /// Scales: Crisis consequences, Physical challenge damage, Confrontation escalation
    /// </summary>
    private static DangerLevel DeriveDangerLevel(Location location, NPC npc, Player player)
    {
        // Check location safety level for danger indicators
        if (location != null && location.Safety == LocationSafety.Dangerous)
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
        if (location == null) return SocialStakes.Witnessed;

        // Use categorical Privacy dimension (not capabilities)
        return location.Privacy switch
        {
            LocationPrivacy.Public => SocialStakes.Public,
            LocationPrivacy.Private => SocialStakes.Private,
            LocationPrivacy.SemiPublic => SocialStakes.Witnessed,
            _ => SocialStakes.Witnessed
        };
    }

    /// <summary>
    /// Derive time pressure from location properties and player state.
    ///
    /// Scales: Available choices, time costs, retry availability
    /// </summary>
    private static TimePressure DeriveTimePressure(Location location, Player player)
    {
        // Check for crisis/emergency location purposes (Governance/Civic = authority pressure)
        if (location != null &&
            (location.Purpose == global::LocationPurpose.Governance ||
             location.Purpose == global::LocationPurpose.Civic))
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
        if (location != null && location.Purpose == global::LocationPurpose.Worship)
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
        if (location == null)
            return EnvironmentQuality.Standard;

        // Premium environment (high tier locations)
        if (location.Tier >= 3)
        {
            return EnvironmentQuality.Premium;
        }

        // Standard environment (mid tier locations)
        if (location.Tier == 2)
        {
            return EnvironmentQuality.Standard;
        }

        // Basic environment (low tier locations)
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
