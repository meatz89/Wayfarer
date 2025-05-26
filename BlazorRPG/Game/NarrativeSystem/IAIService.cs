public interface IAIService
{
    // Core AI functions
    /// <summary>
    /// Set the initial scene based on location and inciting action
    /// </summary>
    Task<string> GenerateIntroductionAsync(
        NarrativeContext context,
        string memoryContent,
        WorldStateInput worldStateInput,
        int priority);

    Task<List<AiChoice>> GenerateEncounterChoicesAsync(
        NarrativeContext context,
        EncounterState encounterState,
        WorldStateInput worldStateInput,
        int priority);

    Task<string> GenerateReactionAsync(
        NarrativeContext context,
        EncounterState encounterState,
        AiChoice chosenOption,
        ChoiceOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

    Task<string> GenerateEndingAsync(
        NarrativeContext context,
        AiChoice chosenOption,
        ChoiceOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

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
    string GetProviderName();
}
