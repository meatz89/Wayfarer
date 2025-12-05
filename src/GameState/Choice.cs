
/// <summary>
/// Choice - runtime instance of ChoiceTemplate with AI-generated label
/// Part of Template/Instance pattern: ChoiceTemplate (immutable) -> Choice (mutable)
///
/// ARCHITECTURAL (Two-Pass Procedural Generation - arc42 §8.28):
/// - Created at SCENE ACTIVATION in SceneInstantiator (Pass 2B) with SCALED mechanical values
/// - Label initially ActionTextTemplate placeholder
/// - Label AI-generated LAZILY at SITUATION ENTRY via SituationFacade.ActivateSituationAsync()
/// - Label REGENERATED on each re-entry (dynamic storytelling, reflects CURRENT game state)
/// - Mechanical values (costs, consequences) calculated ONCE at scene activation
/// - Stored in Situation.Choices collection
///
/// HIGHLANDER: NO Id property - identified by object reference only
/// </summary>
public class Choice
{
    /// <summary>
    /// Reference to immutable ChoiceTemplate this was created from
    /// Composition pattern: Choice.Template for accessing immutable archetype properties
    /// </summary>
    public ChoiceTemplate Template { get; set; }

    /// <summary>
    /// AI-generated contextual action label
    /// Initial value: Template.ActionTextTemplate placeholder (set at scene activation)
    /// Updated LAZILY by SituationFacade.ActivateSituationAsync() when player enters situation
    /// REGENERATED on each re-entry (reflects CURRENT game state, relationships, time)
    /// Fallback: Template.ActionTextTemplate with placeholders replaced if AI fails
    /// Example: "Press Elena for information about lodging rates"
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Pre-scaled requirement from Template.RequirementFormula
    /// Calculated at activation using RuntimeScalingContext
    /// Used for display AND execution (Perfect Information)
    /// </summary>
    public CompoundRequirement ScaledRequirement { get; set; }

    /// <summary>
    /// Pre-scaled consequence from Template.Consequence
    /// Calculated at activation using RuntimeScalingContext
    /// Negative values = costs, positive values = rewards
    /// </summary>
    public Consequence ScaledConsequence { get; set; }

    /// <summary>
    /// Pre-scaled success consequence from Template.OnSuccessConsequence
    /// Applied when challenge succeeds (StartChallenge actions)
    /// null for Instant actions
    /// </summary>
    public Consequence ScaledOnSuccessConsequence { get; set; }

    /// <summary>
    /// Pre-scaled failure consequence from Template.OnFailureConsequence
    /// Applied when challenge fails (StartChallenge actions)
    /// null for Instant actions
    /// </summary>
    public Consequence ScaledOnFailureConsequence { get; set; }
}
