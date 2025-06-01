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
        EncounterState encounterState,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

    Task<string> ProcessMemoryConsolidation(
        EncounterContext context,
        EncounterState encounterState,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

    Task<LocationDetails> GenerateLocationDetails(
        EncounterContext context,
        EncounterState encounterState,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

    Task<string> GenerateActions(
        EncounterContext context,
        EncounterState encounterState,
        EncounterChoice chosenOption,
        BeatOutcome outcome,
        WorldStateInput worldStateInput,
        int priority);

    string GetProviderName();
}
