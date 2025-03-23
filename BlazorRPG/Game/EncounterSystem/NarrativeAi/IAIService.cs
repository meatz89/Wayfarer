// General world information for discovery context
public class WorldContext
{
    public List<string> KnownLocations { get; set; }
    public List<string> KnownCharacters { get; set; }
    public List<string> RecentWorldEvents { get; set; }
}

// Context for developing specific entities
public class EntityContext
{
    public string Name { get; set; }
    public string CurrentDescription { get; set; }
    public Dictionary<string, string> RelatedEntities { get; set; }
    public List<string> InteractionHistory { get; set; }
}
// New entities discovered in encounter narrative
public class DiscoveredEntities
{
    public List<DiscoveredLocation> Locations { get; set; }
    public List<DiscoveredCharacter> Characters { get; set; }
    public List<DiscoveredOpportunity> Opportunities { get; set; }
}

// Minimal location information from discovery
public class DiscoveredLocation
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedLocations { get; set; }
}

public class DiscoveredCharacter
{
    public string Name { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }
    public string HomeLocation { get; set; }
}


public class DiscoveredOpportunity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string Location { get; set; }
    public List<string> Characters { get; set; }
}


// Detailed entity information from development
public class EntityDetails
{
    public string DetailedDescription { get; set; }
    public string Background { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; }
    public List<IEnvironmentalProperty> EnvironmentalProperties { get; set; }
    public Dictionary<string, List<IEnvironmentalProperty>> TimeBasedProperties { get; set; }
}

// AI recommendations for state changes after encounters
public class StateChangeRecommendations
{
    public Dictionary<string, int> ResourceChanges { get; set; }
    public Dictionary<string, int> RelationshipChanges { get; set; }
    public List<string> SkillExperienceGained { get; set; }
    public List<string> SuggestedWorldEvents { get; set; }
}

public interface IAIService
{
    // Core AI functions
    Task<DiscoveredEntities> ExtractWorldDiscoveries(string encounterNarrative, WorldContext worldContext);
    Task<EntityDetails> DevelopEntityDetails(string entityType, string entityId, EntityContext entityContext);
    Task<StateChangeRecommendations> GenerateStateChanges(string encounterOutcome, EncounterContext context);

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
