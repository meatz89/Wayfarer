/// <summary>
/// Interface for narrative AI services that generate narrative content
/// </summary>
public interface INarrativeAIService
{
    /// <summary>
    /// Set the initial scene based on location and inciting action
    /// </summary>
    Task<string> GenerateIntroductionAsync(
        NarrativeContext context,
        EncounterStatus state);

    /// <summary>
    /// Generate reaction to player's choice and setup for next choices
    /// </summary>
    Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState);

    Task<string> GenerateEndingAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceOutcome outcome,
        EncounterStatus newState);

    Task<string> GenerateMemoryFileAsync(
        NarrativeContext context, 
        ChoiceOutcome outcome, 
        EncounterStatus newState,
        string oldMemory);

    /// <summary>
    /// Generate narrative descriptions for choices
    /// </summary>
    Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state);
}
