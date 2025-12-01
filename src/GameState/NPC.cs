public class NPC
{
    // Identity - Name is natural key (NO ID property per HIGHLANDER: object references only)
    public string Name { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }

    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton (e.g., "letter_template_elena_refusal")

    // Categorical Properties for Logical System Interactions
    public Professions Profession { get; set; }

    // Orthogonal Categorical Dimensions (Entity Resolution)
    // These dimensions compose to create narrative archetypes
    // Example: Notable + Obstacle + Informed = Gatekeeper who knows local events
    public NPCSocialStanding SocialStanding { get; set; } = NPCSocialStanding.Commoner;
    public NPCStoryRole StoryRole { get; set; } = NPCStoryRole.Neutral;
    public NPCKnowledgeLevel KnowledgeLevel { get; set; } = NPCKnowledgeLevel.Ignorant;

    // Personality system
    public string PersonalityDescription { get; set; } = string.Empty; // Authentic description from JSON
    public PersonalityType PersonalityType { get; set; } // NO DEFAULT - must be set explicitly from JSON
    public PersonalityModifier ConversationModifier { get; set; } // Personality-specific conversation rules

    // Crisis system - personal troubles affecting NPC state
    public CrisisType Crisis { get; set; } = CrisisType.None; // Crisis type for DISCONNECTED NPCs (narrative framing)

    // Level system (1-5) for difficulty/content progression and XP scaling
    public int Level { get; set; } = 1;

    // Conversation difficulty level (1-3) for XP multipliers
    public int ConversationDifficulty { get; set; } = 1;

    // NPCs are always available - no schedule system
    public NPCRelationship PlayerRelationship { get; set; } = NPCRelationship.Neutral;

    // Confrontation Tracking
    public int LastConfrontationCount { get; set; } = 0;  // Track confrontations already shown
    public int RedemptionProgress { get; set; } = 0;      // Progress toward emotional recovery
    public bool HasPermanentScar { get; set; } = false;   // Some wounds never fully heal

    // DeliveryObligation offering system

    // Work and Home locations (for deeper world building)
    // HIGHLANDER: Object references ONLY, no ID properties
    public Location WorkLocation { get; set; }
    public Location HomeLocation { get; set; }

    // Known routes (for HELP verb sharing)
    private List<RouteOption> _knownRoutes = new List<RouteOption>();

    // Boolean flags violate deck-based architecture

    // Relationship Flow (Single value 0-24 encoding both state and battery)
    // 0-4: DISCONNECTED, 5-9: GUARDED, 10-14: NEUTRAL, 15-19: RECEPTIVE, 20-24: TRUSTING
    // Within each range: 0=-2, 1=-1, 2=0, 3=+1, 4=+2 (displays as -3 to +3, transition at �4)
    public int RelationshipFlow { get; set; } = 12; // Start at NEUTRAL with 0 flow

    // Localized mastery - StoryCubes reduce Social Doubt with THIS NPC only
    // 0-10 scale: 0 cubes = full doubt, 10 cubes = complete understanding (no doubt)
    public int StoryCubes { get; set; } = 0;

    // Scene-Situation Architecture addition: Bond Strength
    /// <summary>
    /// Bond Strength - relationship depth with this NPC (0-30 scale)
    /// Used for CompoundRequirement unlock paths and achievement tracking
    /// Different from RelationshipFlow (which tracks connection state/battery)
    /// BondStrength = depth of relationship, Flow = current emotional state
    /// </summary>
    public int BondStrength { get; set; } = 0;

    /// <summary>
    /// Last time player interacted with this NPC (for relationship decay)
    /// </summary>
    public DateTime LastInteractionTime { get; set; } = DateTime.MinValue;

    // Calculated properties from single flow value
    public ConnectionState CurrentState => GetConnectionState();

    // NPC DECK ARCHITECTURE
    // ObservationDeck and BurdenDeck systems eliminated - replaced by transparent resource competition
    public List<ExchangeCard> ExchangeDeck { get; set; } = new();  // 5-10 exchange cards: Simple instant trades (Mercantile NPCs only)

    // NOTE: ActiveSituationIds DELETED - situations embedded in scenes, query GameWorld.Scenes by NPC
    // NOTE: Old SceneIds property removed - NEW Scene-Situation architecture
    // Scenes now spawn via Situation spawn rewards (SceneSpawnReward) instead of NPC ownership
    // NPCs no longer directly own scenes - scenes are managed by Situation lifecycle

    /// <summary>
    /// Object reference to location (for runtime navigation)
    /// </summary>
    public Location Location { get; set; }

    // Equipment available for purchase from this vendor NPC (Core Loop design)
    // HIGHLANDER: Object references ONLY, no ID lists
    // Only applicable for NPCs with Mercantile or vendor service types
    public List<Item> AvailableEquipment { get; set; } = new List<Item>();

    // Initial token values to be applied during game initialization
    public List<InitialTokenValue> InitialTokenValues { get; set; } = new List<InitialTokenValue>();

    // Stranger-specific properties (for unnamed one-time NPCs)
    public bool IsStranger { get; set; } = false;
    public TimeBlocks? AvailableTimeBlock { get; set; } // When stranger appears
    public bool HasBeenEncountered { get; set; } = false; // One-time flag

    // Initialize exchange deck (for Mercantile NPCs only)
    public void InitializeExchangeDeck(List<ExchangeCard> exchangeCards)
    {
        if (ExchangeDeck == null || !ExchangeDeck.Any())
        {
            ExchangeDeck = new List<ExchangeCard>();
            // Only Mercantile NPCs have exchange decks
            if (PersonalityType == PersonalityType.MERCANTILE && exchangeCards != null)
            {
                foreach (ExchangeCard card in exchangeCards)
                {
                    ExchangeDeck.Add(card);
                }
            }
        }
    }

    // Check if NPC has exchange cards available
    public bool HasExchangeCards()
    {
        return ExchangeDeck != null && ExchangeDeck.Any();
    }

    // NOTE: GetTodaysExchange() DELETED - violates Catalogue Pattern and Perfect Information
    // ExchangeCard filtering handled by ExchangeFacade.GetAvailableExchanges() with categorical properties
    // No deterministic selection needed - player sees all available exchanges filtered by game state

    // Helper methods for UI display
    public string ProfessionDescription => Profession.ToString().Replace('_', ' ');

    public string ScheduleDescription => "Always available";

    public bool IsAvailable(TimeBlocks currentTime)
    {
        // NPCs are always available by default
        return true;
    }

    // Method for adding known routes (used by HELP verb)
    public void AddKnownRoute(RouteOption route)
    {
        if (!_knownRoutes.Contains(route))
        {
            _knownRoutes.Add(route);
        }
    }

    /// <summary>
    /// Get connection state from single flow value
    /// </summary>
    public ConnectionState GetConnectionState()
    {
        return RelationshipFlow switch
        {
            <= 4 => ConnectionState.DISCONNECTED,
            <= 9 => ConnectionState.GUARDED,
            <= 14 => ConnectionState.NEUTRAL,
            <= 19 => ConnectionState.RECEPTIVE,
            _ => ConnectionState.TRUSTING
        };
    }

    /// <summary>
    /// Apply daily decay - move flow toward global neutral value 12
    /// </summary>
    public void ApplyDailyDecay()
    {
        // Move toward global neutral (value 12)
        if (RelationshipFlow < 12)
        {
            RelationshipFlow++; // Move up toward neutral
        }
        else if (RelationshipFlow > 12)
        {
            RelationshipFlow--; // Move down toward neutral
        }
        // If at neutral (12), stay there
    }

    // Stranger-specific methods

    /// <summary>
    /// Check if this stranger is available at the current time block
    /// </summary>
    public bool IsAvailableAtTime(TimeBlocks currentTime)
    {
        if (!IsStranger) return true; // Named NPCs are always available
        return AvailableTimeBlock.HasValue && AvailableTimeBlock.Value == currentTime && !HasBeenEncountered;
    }

    /// <summary>
    /// Mark this stranger as encountered (one-time flag)
    /// </summary>
    public void MarkAsEncountered()
    {
        if (IsStranger)
        {
            HasBeenEncountered = true;
        }
    }

    /// <summary>
    /// Reset stranger availability for new time block
    /// </summary>
    public void RefreshForNewTimeBlock()
    {
        if (IsStranger)
        {
            HasBeenEncountered = false;
        }
    }
}