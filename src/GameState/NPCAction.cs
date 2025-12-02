
/// <summary>
/// NPCAction - Runtime entity for NPC interaction choices
/// Generated from ChoiceTemplate when Scene-spawned at NPC placements
/// Represents a single interaction option available with an NPC
/// </summary>
public class NPCAction
{
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
    /// HIGHLANDER: Object reference ONLY, no NPCId
    /// Determines which NPC portrait/name to display
    /// </summary>
    public NPC NPC { get; set; }

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
    /// Target Situation to initiate (if ActionType = InitiateSituation)
    /// HIGHLANDER: Object reference ONLY, no TargetSituationId
    /// References Situation in Scene.Situations (situations embedded in scenes)
    /// null for other action types
    /// </summary>
    public Situation TargetSituation { get; set; }

    /// <summary>
    /// Conversation tree to start (if ActionType = StartConversationTree)
    /// HIGHLANDER: Object reference ONLY, no ConversationTreeId
    /// References ConversationTree in GameWorld.ConversationTrees
    /// null for other action types
    /// </summary>
    public ConversationTree ConversationTree { get; set; }

    /// <summary>
    /// Exchange offer (if ActionType = StartExchange)
    /// HIGHLANDER: Object reference ONLY, no ExchangeId
    /// References ExchangeOffer in NPC.ExchangeDeck or GameWorld.NPCExchangeCards
    /// null for other action types
    /// </summary>
    public ExchangeCard Exchange { get; set; }

    /// <summary>
    /// Challenge (if ActionType = StartConversation)
    /// HIGHLANDER: Object reference ONLY, no ChallengeId
    /// May reference specific conversation request or challenge setup
    /// null for other action types
    /// </summary>
    public Situation Challenge { get; set; }

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
    /// COMPOSITION not copy - access CompoundRequirement, OnSuccessConsequence, OnFailureConsequence through this reference
    ///
    /// Always non-null for NPCActions (Scene-spawned only, no legacy pattern)
    /// NPCActions are ONLY generated from ChoiceTemplates within Scenes
    /// Static NPC interactions use Exchange system or ConversationTrees
    ///
    /// ChoiceTemplate provides:
    /// - RequirementFormula (CompoundRequirement with OR paths for bond/scale requirements)
    /// - OnSuccessConsequence (Consequence with resource changes, bonds, scales, states, scene spawns)
    /// - OnFailureConsequence (Consequence with failure outcomes)
    ///
    /// Enables unified action execution: All NPCActions use ChoiceTemplate for
    /// requirements checking, cost application, and consequence application
    /// </summary>
    public ChoiceTemplate ChoiceTemplate { get; set; }

    /// <summary>
    /// THREE-TIER TIMING MODEL: Source Situation
    /// HIGHLANDER: Object reference ONLY, no SituationId
    /// Links ephemeral action to source Situation for cleanup after execution
    /// Actions are QUERY-TIME instances (Tier 3), created when Situation activates
    /// After action executes, GameOrchestrator deletes ALL actions for this Situation
    /// Next time player enters context, actions recreated fresh from ChoiceTemplates
    /// NOTE: Different from TargetSituation above (which is for InitiateSituation routing)
    /// </summary>
    public Situation Situation { get; set; }

    /// <summary>
    /// PERFECT INFORMATION: Scene spawn previews
    /// If this action spawns scenes (ChoiceTemplate.OnSuccessConsequence.ScenesToSpawn),
    /// SceneFacade generates ScenePreview from SceneTemplate metadata
    /// Player sees WHERE scene will spawn, WHAT it contains, BEFORE selecting action
    /// Enables strategic decision-making with full knowledge of consequences
    ///
    /// HIGHLANDER COMPLIANCE: Replaces provisional scene pattern
    /// - OLD: Create Scene entity with State=Provisional, delete if not selected
    /// - NEW: Generate ScenePreview DTO from template, no entity until action executes
    /// </summary>
    public List<ScenePreview> ScenePreviews { get; set; } = new List<ScenePreview>();

    /// <summary>
    /// Entity-derived scaled requirement for display (query-time scaling).
    /// TWO-PHASE SCALING MODEL (arc42 §8.26):
    /// - Parse-time: Catalogue generates rhythm structure + tier-based values
    /// - Query-time: Entity-derived adjustments from RuntimeScalingContext
    ///
    /// Created by SceneFacade when building NPCAction from ChoiceTemplate.
    /// null = no scaling applied (use ChoiceTemplate.RequirementFormula directly)
    /// non-null = scaled version reflecting current NPC relationship
    /// </summary>
    public CompoundRequirement ScaledRequirement { get; set; }

    /// <summary>
    /// Entity-derived scaled consequence for display (query-time scaling).
    /// TWO-PHASE SCALING MODEL (arc42 §8.26):
    /// Costs adjusted based on NPC relationship and location quality.
    /// </summary>
    public Consequence ScaledConsequence { get; set; }
}
