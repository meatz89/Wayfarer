public class ChoiceModification
{
    public ModificationSource Source { get; set; }
    public ModificationType Type { get; set; }
    public string Effect { get; set; } // Human-readable description
    public string SourceDetails { get; set; } // Added property for source details

    // Specific modification data (only one of these will be populated based on Type)
    public EnergyCostReduction EnergyChange { get; set; }
    public RequirementModification Requirement { get; set; }
    public OutcomeModification Cost { get; set; }
    public OutcomeModification Reward { get; set; }
}


// Data for modifying a Requirement
public class RequirementModification
{
    public Requirement RequirementType { get; set; }
}

// Data for modifying an Outcome (Cost or Reward)
public class OutcomeModification
{
    public Outcome OutcomeType { get; set; }
}

public enum ModificationSource
{
    LocationProperty,
    LocationType,
    LocationScale,
    LocationCrowdLevel,
    PlayerSkill,
    PlayerInsight,
    CurrentPressure,
    ChoiceArchetype,
    ChoiceApproach,
}

public enum ModificationType
{
    ValueChange,
    Requirement,
    Cost,
    Reward,
    EnergyCost,
    ValueConversion
}

public enum RequirementSource
{
    Base,
    LocationArchetype,
    LocationProperty,
    EncounterContext,
    PlayerState,
}

public enum OutcomeSource
{
    Base,
    LocationArchetype,
    LocationProperty,
    EncounterContext,
    PlayerState,
}

public enum ValueChangeSourceType
{
    Base,
    LocationArchetype,
    LocationProperty,
    EncounterContext,
    PlayerState,
}