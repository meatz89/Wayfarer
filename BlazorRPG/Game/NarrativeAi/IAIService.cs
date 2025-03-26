public interface IAIService
{
    // Core AI functions
    /// <summary>
    /// Set the initial scene based on location and inciting action
    /// </summary>
    Task<string> GenerateIntroductionAsync(
        NarrativeContext context,
        EncounterStatusModel state,
        string memoryContent);

    Task<string> GenerateEncounterNarrative(
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

    /// <summary>
    /// Generate narrative descriptions for choices
    /// </summary>
    Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state);

    Task<WorldEvolutionResponse> ProcessWorldEvolution(NarrativeContext context, WorldEvolutionInput input);
    Task<string> ProcessMemoryConsolidation(NarrativeContext context, MemoryConsolidationInput input);
    Task<LocationDetails> GenerateLocationDetailsAsync(LocationGenerationContext context);

}
