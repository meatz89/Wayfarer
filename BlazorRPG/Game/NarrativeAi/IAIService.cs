public interface IAIService
{
    // Core AI functions
    /// <summary>
    /// Set the initial scene based on location and inciting action
    /// </summary>
    Task<string> GenerateIntroductionAsync(
        NarrativeContext context,
        EncounterStatusModel state,
        string memoryContent,
        WorldStateInput worldStateInput);

    Task<string> GenerateEncounterNarrative(
        NarrativeContext context,
        ChoiceCard chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput);

    Task<string> GenerateEndingAsync(
        NarrativeContext context,
        ChoiceCard chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        WorldStateInput worldStateInput);

    Task<Dictionary<ChoiceCard, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<ChoiceCard> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state,
        WorldStateInput worldStateInput);

    Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        NarrativeContext context, 
        PostEncounterEvolutionInput input,
        WorldStateInput worldStateInput);
    Task<string> ProcessMemoryConsolidation(
        NarrativeContext context, 
        MemoryConsolidationInput input,
        WorldStateInput worldStateInput);
    Task<LocationDetails> GenerateLocationDetailsAsync(
        LocationCreationInput context,
        WorldStateInput worldStateInput);
    Task<string> GenerateActionsAsync(
        ActionGenerationContext input,
        WorldStateInput worldStateInput);
}
