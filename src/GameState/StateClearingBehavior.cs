using System.Collections.Generic;

/// <summary>
/// Strongly-typed behavior object describing HOW a state can be cleared
/// Stored on State entity after parse-time translation by StateClearConditionsCatalogue
///
/// ARCHITECTURE: Follows Catalogue + Resolver pattern
/// - Created by StateClearConditionsCatalogue at parse time
/// - Stored on State.ClearingBehavior
/// - Checked by StateClearingResolver at runtime
/// - Properties named for execution contexts (WHERE they're checked)
///
/// This is a data class with NO logic - all clearing logic lives in StateClearingResolver
/// </summary>
public class StateClearingBehavior
{
    // ========================================
    // REST CLEARING CONTEXT (ResourceFacade)
    // ========================================

    /// <summary>
    /// State clears when player rests
    /// Checked by: ResourceFacade.ExecuteRest()
    /// </summary>
    public bool ClearsOnRest { get; set; }

    /// <summary>
    /// If ClearsOnRest=true, this specifies whether rest must be at a safe location
    /// Checked by: ResourceFacade.ExecuteRest() - only clears if Location.IsSafe matches
    /// </summary>
    public bool RequiresSafeLocation { get; set; }

    // ========================================
    // ITEM CONSUMPTION CONTEXT (ResourceFacade)
    // ========================================

    /// <summary>
    /// State clears when player consumes items of these types
    /// Checked by: ResourceFacade.ConsumeItem() - clears if consumed item's type is in this list
    /// </summary>
    public List<ItemType> ClearingItemTypes { get; set; } = new List<ItemType>();

    // ========================================
    // CHALLENGE COMPLETION CONTEXT (Challenge Facades)
    // ========================================

    /// <summary>
    /// State clears when player successfully completes any challenge (Mental/Physical/Social)
    /// Checked by: ChallengeFacade success handlers
    /// </summary>
    public bool ClearsOnChallengeSuccess { get; set; }

    /// <summary>
    /// State clears when player fails any challenge (Mental/Physical/Social)
    /// Checked by: ChallengeFacade failure handlers
    /// </summary>
    public bool ClearsOnChallengeFailure { get; set; }

    // ========================================
    // TIME PASSAGE CONTEXT (TimeFacade)
    // ========================================

    // NOTE: Duration-based clearing uses State.Duration property directly (already exists)
    // TimeFacade.AdvanceSegments() checks Duration, not a property here

    // ========================================
    // MANUAL CLEARING CONTEXT (Player Action Facade)
    // ========================================

    /// <summary>
    /// State can be manually cleared by player via UI action
    /// Checked by: Player action facade - allows UI to offer "Clear State" button
    /// </summary>
    public bool AllowsManualClear { get; set; }

    // ========================================
    // CONSEQUENCE RESOLUTION CONTEXT (ConsequenceFacade - Phase 6)
    // ========================================

    /// <summary>
    /// State clears when player resolves penalties of these types
    /// Checked by: ConsequenceFacade.ResolvePenalty()
    /// </summary>
    public List<PenaltyResolutionType> ClearingPenaltyTypes { get; set; } = new List<PenaltyResolutionType>();

    /// <summary>
    /// State clears when player completes quests of these types
    /// Checked by: ConsequenceFacade.CompleteQuest()
    /// </summary>
    public List<QuestCompletionType> ClearingQuestTypes { get; set; } = new List<QuestCompletionType>();

    /// <summary>
    /// State clears when social events of these types occur
    /// Checked by: ConsequenceFacade.TriggerSocialEvent()
    /// </summary>
    public List<SocialEventType> ClearingSocialEventTypes { get; set; } = new List<SocialEventType>();
}
