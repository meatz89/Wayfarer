namespace Wayfarer.GameState.Enums;

/// <summary>
/// Lifecycle state for Situation entities
/// Controls when ChoiceTemplates are instantiated into action entities
/// Part of three-tier timing model: Parse → Instantiation → Query
/// </summary>
public enum SituationState
{
    /// <summary>
    /// Situation exists as data structure but player hasn't entered context
    /// ChoiceTemplates NOT yet instantiated into actions
    /// NO actions exist in GameWorld collections
    /// Waiting for player to enter location/conversation/route
    /// </summary>
    Dormant,

    /// <summary>
    /// Player entered context (location/conversation/route)
    /// SceneFacade triggered state transition
    /// ChoiceTemplates instantiated into LocationActions/NPCActions/PathCards
    /// Actions created in GameWorld flat collections
    /// Provisional Scenes created for actions with spawn rewards
    /// Actions available for player selection
    /// </summary>
    Active
}
