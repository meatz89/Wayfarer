public interface IAIService
{
    // Core AI functions
    Task<DiscoveredEntities> ExtractWorldDiscoveries(string encounterNarrative, WorldContext worldContext);
    Task<EntityDetails> DevelopEntityDetails(string entityType, string entityId, EntityContext entityContext);
    Task<StateChangeRecommendations> GenerateStateChanges(string encounterOutcome, EncounterContext context);

    Task<LocationDetails> GenerateLocationDetailsAsync(LocationGenerationContext context);

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
