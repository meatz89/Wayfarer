namespace Wayfarer.Tests.AI;

/// <summary>
/// Canonical game contexts for AI narrative evaluation.
/// Each fixture represents a specific, reproducible game scenario.
/// Used to evaluate AI output quality, optimize prompts, and compare models.
///
/// USAGE:
/// - Run against real Ollama (not mocked)
/// - Each fixture has expected quality criteria
/// - Results logged for manual review and comparison
/// </summary>
public static class NarrativeTestFixtures
{
    // ==================== SITUATION NARRATIVE FIXTURES ====================

    /// <summary>
    /// Friendly innkeeper negotiating lodging rates.
    /// Expected: Warm atmosphere, mention of inn name, hospitality tone.
    /// </summary>
    public static NarrativeTestCase InnkeeperLodgingNegotiation()
    {
        Location location = new Location("The Weary Traveler Inn")
        {
            Purpose = LocationPurpose.Dwelling,
            Privacy = LocationPrivacy.SemiPublic,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Moderate,
            Description = "A cozy inn with worn wooden floors and the smell of hearth smoke."
        };

        NPC npc = new NPC
        {
            Name = "Martha Holloway",
            PersonalityType = PersonalityType.DEVOTED,
            Profession = Professions.Innkeeper
        };

        ScenePromptContext context = new ScenePromptContext
        {
            Location = location,
            NPC = npc,
            CurrentTimeBlock = TimeBlocks.Evening,
            CurrentWeather = WeatherCondition.Rain,
            CurrentDay = 3,
            NPCBondLevel = 0
        };

        NarrativeHints hints = new NarrativeHints
        {
            Tone = "warm",
            Theme = "economic_negotiation",
            Context = "Player seeking lodging for the night",
            Style = "Victorian hospitality"
        };

        Situation situation = new Situation
        {
            Name = "Seeking Lodging",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social
        };

        return new NarrativeTestCase
        {
            Name = "InnkeeperLodgingNegotiation",
            Context = context,
            Hints = hints,
            Situation = situation,
            ValidEntityNames = new List<string>
            {
                "Martha Holloway", "Martha", "Holloway",    // NPC name variants (allowed if used)
                "The Weary Traveler Inn", "Weary Traveler"  // Location name variants (allowed if used)
            },
            RequiredContextMarkers = new List<string>
            {
                "inn", "hearth", "warm", "fire", "lodging", "evening", "rain"
            },
            RequiredMarkerCount = 1,
            ExpectedTone = "warm, welcoming",
            ExpectedLengthRange = (50, 150)
        };
    }

    /// <summary>
    /// Hostile guard at city checkpoint.
    /// Expected: Tense atmosphere, authority dynamics, suspicion.
    /// </summary>
    public static NarrativeTestCase GuardCheckpointConfrontation()
    {
        Location location = new Location("The Northern Gate")
        {
            Purpose = LocationPurpose.Commerce,
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Dangerous,
            Activity = LocationActivity.Busy,
            Description = "A fortified gate with armed guards checking travelers."
        };

        NPC npc = new NPC
        {
            Name = "Sergeant Aldric Vance",
            PersonalityType = PersonalityType.PROUD,
            Profession = Professions.Guard
        };

        ScenePromptContext context = new ScenePromptContext
        {
            Location = location,
            NPC = npc,
            CurrentTimeBlock = TimeBlocks.Midday,
            CurrentWeather = WeatherCondition.Clear,
            CurrentDay = 5,
            NPCBondLevel = -1
        };

        NarrativeHints hints = new NarrativeHints
        {
            Tone = "tense",
            Theme = "authority_confrontation",
            Context = "Player needs to pass through checkpoint",
            Style = "Victorian bureaucracy"
        };

        Situation situation = new Situation
        {
            Name = "Checkpoint Inspection",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social
        };

        return new NarrativeTestCase
        {
            Name = "GuardCheckpointConfrontation",
            Context = context,
            Hints = hints,
            Situation = situation,
            ValidEntityNames = new List<string>
            {
                "Sergeant Aldric Vance", "Aldric Vance", "Sergeant Vance", "Vance",
                "The Northern Gate", "Northern Gate"
            },
            RequiredContextMarkers = new List<string>
            {
                // Core location/role words
                "gate", "guard", "checkpoint", "sergeant", "vance",
                // Tension/mood words (including partial matches)
                "tense", "tension", "authority", "inspection", "assess",
                // Time indicators
                "midday", "sun", "noon", "bright"
            },
            RequiredMarkerCount = 1,
            ExpectedTone = "tense, authoritative",
            ExpectedLengthRange = (50, 150)
        };
    }

    /// <summary>
    /// Cunning merchant with valuable information.
    /// Expected: Calculating atmosphere, trade dynamics, shrewd undertones.
    /// </summary>
    public static NarrativeTestCase MerchantInformationExchange()
    {
        Location location = new Location("Blackwood Market")
        {
            Purpose = LocationPurpose.Commerce,
            Privacy = LocationPrivacy.Public,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Busy,
            Description = "A bustling market square with merchants hawking wares."
        };

        NPC npc = new NPC
        {
            Name = "Viktor Crane",
            PersonalityType = PersonalityType.CUNNING,
            Profession = Professions.Merchant
        };

        ScenePromptContext context = new ScenePromptContext
        {
            Location = location,
            NPC = npc,
            CurrentTimeBlock = TimeBlocks.Afternoon,
            CurrentWeather = WeatherCondition.Fog,
            CurrentDay = 7,
            NPCBondLevel = 1
        };

        NarrativeHints hints = new NarrativeHints
        {
            Tone = "calculating",
            Theme = "information_exchange",
            Context = "Player seeking information the merchant possesses",
            Style = "Victorian commerce"
        };

        Situation situation = new Situation
        {
            Name = "Trading Secrets",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social
        };

        return new NarrativeTestCase
        {
            Name = "MerchantInformationExchange",
            Context = context,
            Hints = hints,
            Situation = situation,
            ValidEntityNames = new List<string>
            {
                "Viktor Crane", "Viktor", "Crane",
                "Blackwood Market", "Blackwood"
            },
            RequiredContextMarkers = new List<string>
            {
                // Core location/role
                "market", "blackwood", "merchant", "trade", "wares",
                // Weather/mood synonyms
                "fog", "chill", "damp", "mist",
                // Tone synonyms
                "calculating", "shrewd", "sharp", "assess"
            },
            RequiredMarkerCount = 1,
            ExpectedTone = "shrewd, calculating",
            ExpectedLengthRange = (50, 150)
        };
    }

    /// <summary>
    /// Steadfast scholar in a library.
    /// Expected: Scholarly atmosphere, knowledge dynamics, measured tone.
    /// </summary>
    public static NarrativeTestCase ScholarResearchAssistance()
    {
        Location location = new Location("The Antiquarian Archive")
        {
            Purpose = LocationPurpose.Civic,
            Privacy = LocationPrivacy.SemiPublic,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Quiet,
            Description = "Dusty shelves lined with ancient tomes and scrolls."
        };

        NPC npc = new NPC
        {
            Name = "Professor Helena Ashworth",
            PersonalityType = PersonalityType.STEADFAST,
            Profession = Professions.Scholar
        };

        ScenePromptContext context = new ScenePromptContext
        {
            Location = location,
            NPC = npc,
            CurrentTimeBlock = TimeBlocks.Morning,
            CurrentWeather = WeatherCondition.Clear,
            CurrentDay = 12,
            NPCBondLevel = 2
        };

        NarrativeHints hints = new NarrativeHints
        {
            Tone = "intellectual",
            Theme = "information_exchange",
            Context = "Player seeking scholarly assistance",
            Style = "Victorian academia"
        };

        Situation situation = new Situation
        {
            Name = "Research Consultation",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social
        };

        return new NarrativeTestCase
        {
            Name = "ScholarResearchAssistance",
            Context = context,
            Hints = hints,
            Situation = situation,
            ValidEntityNames = new List<string>
            {
                "Professor Helena Ashworth", "Helena Ashworth", "Professor Ashworth", "Ashworth",
                "The Antiquarian Archive", "Antiquarian Archive"
            },
            RequiredContextMarkers = new List<string>
            {
                "archive", "library", "scholar", "dusty", "tomes", "books", "quiet", "morning"
            },
            RequiredMarkerCount = 1,
            ExpectedTone = "scholarly, measured",
            ExpectedLengthRange = (50, 150)
        };
    }

    /// <summary>
    /// Route encounter - travel situation.
    /// Expected: Journey atmosphere, environmental details, travel tension.
    /// </summary>
    public static NarrativeTestCase ForestPathEncounter()
    {
        Location originLocation = new Location("Millbrook Village")
        {
            Purpose = LocationPurpose.Dwelling
        };

        Location destinationLocation = new Location("Thornfield Manor")
        {
            Purpose = LocationPurpose.Dwelling
        };

        RouteOption route = new RouteOption
        {
            Name = "The Old Forest Road",
            OriginLocation = originLocation,
            DestinationLocation = destinationLocation
        };

        ScenePromptContext context = new ScenePromptContext
        {
            Route = route,
            CurrentTimeBlock = TimeBlocks.Afternoon,
            CurrentWeather = WeatherCondition.Storm,
            CurrentDay = 4,
            NPCBondLevel = 0
        };

        NarrativeHints hints = new NarrativeHints
        {
            Tone = "ominous",
            Theme = "crisis_response",
            Context = "Obstacle on the forest path",
            Style = "Victorian travel narrative"
        };

        Situation situation = new Situation
        {
            Name = "Blocked Path",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Physical
        };

        return new NarrativeTestCase
        {
            Name = "ForestPathEncounter",
            Context = context,
            Hints = hints,
            Situation = situation,
            ValidEntityNames = new List<string>
            {
                "The Old Forest Road", "Old Forest Road",
                "Millbrook Village", "Millbrook",
                "Thornfield Manor", "Thornfield"
            },
            RequiredContextMarkers = new List<string>
            {
                "forest", "road", "path", "wind", "journey", "ominous", "afternoon", "trees"
            },
            RequiredMarkerCount = 1,
            ExpectedTone = "atmospheric, tense",
            ExpectedLengthRange = (50, 150)
        };
    }

    // ==================== CHOICE LABEL FIXTURES ====================

    /// <summary>
    /// Diplomatic approach to innkeeper.
    /// Expected: Polite action, mentions NPC name, concrete action.
    /// </summary>
    public static ChoiceLabelTestCase DiplomaticInnkeeperApproach()
    {
        NarrativeTestCase baseCase = InnkeeperLodgingNegotiation();

        ChoiceTemplate choiceTemplate = new ChoiceTemplate
        {
            Id = "diplomatic_approach",
            ActionTextTemplate = "Politely negotiate with {NPCName}",
            ActionType = ChoiceActionType.Instant,
            PathType = ChoicePathType.InstantSuccess
        };

        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { DiplomacyRequired = 3 }
            }
        };

        Consequence consequence = new Consequence
        {
            Coins = -5
        };

        return new ChoiceLabelTestCase
        {
            Name = "DiplomaticInnkeeperApproach",
            Context = baseCase.Context,
            Situation = baseCase.Situation,
            ChoiceTemplate = choiceTemplate,
            Requirement = requirement,
            Consequence = consequence,
            ExpectedElements = new List<string> { "Martha" },
            ExpectedWordCountRange = (5, 12)
        };
    }

    /// <summary>
    /// Authoritative approach to guard.
    /// Expected: Commanding action, mentions authority, concrete action.
    /// </summary>
    public static ChoiceLabelTestCase AuthoritativeGuardResponse()
    {
        NarrativeTestCase baseCase = GuardCheckpointConfrontation();

        ChoiceTemplate choiceTemplate = new ChoiceTemplate
        {
            Id = "authority_demand",
            ActionTextTemplate = "Demand passage with authority",
            ActionType = ChoiceActionType.StartChallenge,
            PathType = ChoicePathType.Challenge
        };

        CompoundRequirement requirement = new CompoundRequirement
        {
            OrPaths = new List<OrPath>
            {
                new OrPath { AuthorityRequired = 4 }
            }
        };

        Consequence consequence = new Consequence
        {
            Coins = 0
        };

        return new ChoiceLabelTestCase
        {
            Name = "AuthoritativeGuardResponse",
            Context = baseCase.Context,
            Situation = baseCase.Situation,
            ChoiceTemplate = choiceTemplate,
            Requirement = requirement,
            Consequence = consequence,
            ExpectedElements = new List<string> { "Aldric" },
            ExpectedWordCountRange = (5, 12)
        };
    }

    // ==================== ALL FIXTURES ====================

    public static List<NarrativeTestCase> AllSituationFixtures()
    {
        return new List<NarrativeTestCase>
        {
            InnkeeperLodgingNegotiation(),
            GuardCheckpointConfrontation(),
            MerchantInformationExchange(),
            ScholarResearchAssistance(),
            ForestPathEncounter()
        };
    }

    public static List<ChoiceLabelTestCase> AllChoiceLabelFixtures()
    {
        return new List<ChoiceLabelTestCase>
        {
            DiplomaticInnkeeperApproach(),
            AuthoritativeGuardResponse()
        };
    }
}

/// <summary>
/// Test case for situation narrative generation.
/// Contains context, expected quality criteria, and evaluation metadata.
///
/// VALIDATION PHILOSOPHY (Additive, Not Conflicting):
/// AI narrative does NOT need to always spell out entity names.
/// But IF it references them, they MUST be correct (not fabricated).
///
/// AUTOMATED VALIDATION:
/// - ValidEntityNames: Names that are ALLOWED if referenced (for reporting, not strict checking)
/// - RequiredContextMarkers: Atmospheric markers to validate context relevance
/// - RequiredMarkerCount: How many markers must appear to prove context awareness
///
/// HUMAN/AI REVIEW (via JSON export):
/// - Detect fabricated names (hard to automate, easy for humans to spot)
/// - Verify narrative is additive to mechanical context
///
/// PROMPT GUIDANCE ensures AI uses correct names IF referencing entities.
/// </summary>
public class NarrativeTestCase
{
    public string Name { get; set; } = "";
    public ScenePromptContext Context { get; set; } = new ScenePromptContext();
    public NarrativeHints Hints { get; set; } = new NarrativeHints();
    public Situation Situation { get; set; } = new Situation();

    // For reporting: entity names that ARE ALLOWED if AI references them (not required)
    public List<string> ValidEntityNames { get; set; } = new List<string>();

    // For validation: atmospheric/context markers (threshold-based)
    public List<string> RequiredContextMarkers { get; set; } = new List<string>();
    public int RequiredMarkerCount { get; set; } = 0;

    // Quality criteria (for documentation/reporting)
    public string ExpectedTone { get; set; } = "";
    public (int Min, int Max) ExpectedLengthRange { get; set; }
}

/// <summary>
/// Test case for choice label generation.
/// Contains context, choice template, and expected quality criteria.
/// </summary>
public class ChoiceLabelTestCase
{
    public string Name { get; set; } = "";
    public ScenePromptContext Context { get; set; } = new ScenePromptContext();
    public Situation Situation { get; set; } = new Situation();
    public ChoiceTemplate ChoiceTemplate { get; set; } = new ChoiceTemplate();
    public CompoundRequirement Requirement { get; set; } = new CompoundRequirement();
    public Consequence Consequence { get; set; } = new Consequence();

    // Quality criteria
    public List<string> ExpectedElements { get; set; } = new List<string>();
    public (int Min, int Max) ExpectedWordCountRange { get; set; }
}
