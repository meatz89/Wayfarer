public interface IAIService
{
    // Core AI functions
    /// <summary>
    /// Set the initial scene based on location and inciting action
    /// </summary>
    Task<string> GenerateIntroduction(
        EncounterContext context,
        EncounterState state,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority);

    Task<List<EncounterChoice>> GenerateChoices(
        EncounterContext context,
        EncounterState state,
        PlayerChoiceSelection chosenOption,
        WorldStateInput worldStateInput,
        int priority);

    Task<string> GenerateReaction(
        EncounterContext context,
        EncounterState encounterState,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

    Task<string> GenerateConclusion(
        EncounterContext context,
        EncounterState encounterState,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

    Task<PostEncounterEvolutionResult> ProcessPostEncounterEvolution(
        EncounterContext context,
        PostEncounterEvolutionInput input,
        WorldStateInput worldStateInput);
    Task<string> ProcessMemoryConsolidation(
        EncounterContext context,
        MemoryConsolidationInput input,
        WorldStateInput worldStateInput);
    Task<LocationDetails> GenerateLocationDetails(
        LocationCreationInput context,
        WorldStateInput worldStateInput);
    Task<string> GenerateActions(
        ActionGenerationContext input,
        WorldStateInput worldStateInput);
    string GetProviderName();
}
