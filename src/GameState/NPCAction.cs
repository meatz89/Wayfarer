
/// <summary>
/// NPCAction - Runtime entity for NPC interaction choices
/// Generated from ChoiceTemplate when Scene-spawned at NPC placements
/// Represents a single interaction option available with an NPC
/// </summary>
public class NPCAction
{
    /// <summary>
    /// Unique identifier for this NPC action
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for this action
    /// Example: "Ask about the missing grain", "Trade for supplies", "Challenge to a duel"
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Detailed description of this action
    /// Provides context about what player is attempting
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Target NPC for this action
    /// References NPC.ID in GameWorld.NPCs
    /// Determines which NPC portrait/name to display
    /// </summary>
    public string NPCId { get; set; }

    /// <summary>
    /// What happens when player selects this action
    /// Determines routing to appropriate subsystem:
    /// - StartConversation → SocialFacade (tactical conversation)
    /// - StartConversationTree → ConversationTreeFacade (branching dialogue)
    /// - StartExchange → ExchangeFacade (resource trading)
    /// - InitiateSituation → SituationFacade (strategic action)
    /// - Instant → Direct consequence application
    /// </summary>
    public NPCActionType ActionType { get; set; } = NPCActionType.Instant;

    // ==================== ROUTING PAYLOADS ====================

    /// <summary>
    /// Target Situation ID to initiate (if ActionType = InitiateSituation)
    /// References Situation in Scene.Situations (situations embedded in scenes)
    /// null for other action types
    /// RENAMED from SituationId to avoid confusion with SourceSituationId below
    /// </summary>
    public string TargetSituationId { get; set; }

    /// <summary>
    /// Conversation tree ID to start (if ActionType = StartConversationTree)
    /// References ConversationTree in GameWorld.ConversationTrees
    /// null for other action types
    /// </summary>
    public string ConversationTreeId { get; set; }

    /// <summary>
    /// Exchange offer ID (if ActionType = StartExchange)
    /// References ExchangeOffer in NPC.ExchangeDeck or GameWorld.NPCExchangeCards
    /// null for other action types
    /// </summary>
    public string ExchangeId { get; set; }

    /// <summary>
    /// Challenge identifier for Social challenges (if ActionType = StartConversation)
    /// May reference specific conversation request or challenge setup
    /// null for other action types
    /// </summary>
    public string ChallengeId { get; set; }

    /// <summary>
    /// Challenge type classification (if action starts a challenge)
    /// Typically Social for NPC interactions
    /// May be Mental/Physical for specific NPC-based challenges
    /// null if no challenge involved
    /// </summary>
    public TacticalSystemType? ChallengeType { get; set; }

    // ==================== SIR BRANTE LAYER (UNIFIED ACTION ARCHITECTURE) ====================

    /// <summary>
    /// ChoiceTemplate source (Sir Brante layer - Scene-Situation architecture)
    /// COMPOSITION not copy - access CompoundRequirement, ChoiceCost, ChoiceReward through this reference
    ///
    /// Always non-null for NPCActions (Scene-spawned only, no legacy pattern)
    /// NPCActions are ONLY generated from ChoiceTemplates within Scenes
    /// Static NPC interactions use Exchange system or ConversationTrees
    ///
    /// ChoiceTemplate provides:
    /// - RequirementFormula (CompoundRequirement with OR paths for bond/scale requirements)
    /// - CostTemplate (ChoiceCost with Coins/Resolve/TimeSegments)
    /// - RewardTemplate (ChoiceReward with bond changes, scale shifts, states, scene spawns)
    ///
    /// Enables unified action execution: All NPCActions use ChoiceTemplate for
    /// requirements checking, cost application, and consequence application
    /// </summary>
    public ChoiceTemplate ChoiceTemplate { get; set; }

    /// <summary>
    /// THREE-TIER TIMING MODEL: Source Situation ID
    /// Links ephemeral action to source Situation for cleanup after execution
    /// Actions are QUERY-TIME instances (Tier 3), created when Situation activates
    /// After action executes, GameFacade deletes ALL actions for this Situation
    /// Next time player enters context, actions recreated fresh from ChoiceTemplates
    /// NOTE: Different from TargetSituationId above (which is for InitiateSituation routing)
    /// </summary>
    public string SituationId { get; set; }

    /// <summary>
    /// PERFECT INFORMATION: Scene spawn previews
    /// If this action spawns scenes (ChoiceTemplate.RewardTemplate.ScenesToSpawn),
    /// SceneFacade generates ScenePreview from SceneTemplate metadata
    /// Player sees WHERE scene will spawn, WHAT it contains, BEFORE selecting action
    /// Enables strategic decision-making with full knowledge of consequences
    ///
    /// HIGHLANDER COMPLIANCE: Replaces provisional scene pattern
    /// - OLD: Create Scene entity with State=Provisional, delete if not selected
    /// - NEW: Generate ScenePreview DTO from template, no entity until action executes
    /// </summary>
    public List<ScenePreview> ScenePreviews { get; set; } = new List<ScenePreview>();
}
