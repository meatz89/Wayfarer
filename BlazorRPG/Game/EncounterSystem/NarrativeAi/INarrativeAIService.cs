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
        EncounterStatusModel state,
        string memoryContent);

    Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState);

    Task<string> GenerateEndingAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState);

    Task<string> GenerateMemoryFileAsync(
        NarrativeContext context,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        string oldMemory);

    Task<string> GenerateStateChangesAsync(
        NarrativeContext context,
        ChoiceOutcome outcome,
        EncounterStatusModel newState);

    /// <summary>
    /// Generate narrative descriptions for choices
    /// </summary>
    Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state);
}
