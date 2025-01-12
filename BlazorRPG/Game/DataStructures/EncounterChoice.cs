public class EncounterChoice
{
    // Identity & Description
    public int Index { get; set; }
    public string Description { get; set; }
    public ChoiceArchetypes Archetype { get; set; }
    public ChoiceApproaches Approach { get; set; }
    public SkillTypes ChoiceRelevantSkill { get; set; }

    // Base Values (unmodified inputs from pattern)
    public EnergyTypes EnergyType { get; set; }
    public int EnergyCost { get; set; }
    public List<ValueChange> BaseValueChanges { get; set; } = new();
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> BaseCosts { get; set; } = new();
    public List<Outcome> BaseRewards { get; set; } = new();

    // Calculated Values and Modifications
    public int ModifiedEnergyCost { get; set; }
    public List<ChoiceModification> Modifications { get; set; } = new();
    public List<ValueChange> ModifiedValueChanges { get; set; } = new();
    public List<Requirement> ModifiedRequirements { get; set; } = new();
    public List<Outcome> ModifiedCosts { get; set; } = new();
    public List<Outcome> ModifiedRewards { get; set; } = new();
}

public class ChoiceModification
{
    public ModificationSource Source { get; set; }
    public ModificationType Type { get; set; }
    public string Effect { get; set; } // Human-readable description
    public string SourceDetails { get; set; } // Added property for source details

    // Specific modification data (only one of these will be populated based on Type)
    public ValueConversionModification ValueConversion { get; set; }
    public ValueChangeModification ValueChange { get; set; }
    public EnergyChangeModification EnergyChange { get; set; }
    public RequirementModification Requirement { get; set; }
    public OutcomeModification Cost { get; set; }
    public OutcomeModification Reward { get; set; }
}

// Data for modifying a EnergyChange
public class EnergyChangeModification
{
    public EnergyTypes EnergyType { get; set; }
    public ChoiceArchetypes ChoiceArchetype { get; set; }
    public int OriginalValue { get; set; }
    public int NewValue { get; set; }
}

// Data for modifying a ValueChange
public class ValueChangeModification
{
    public ValueTypes ValueType { get; set; }
    public int OriginalValue { get; set; }
    public int TargetValue { get; set; }
    public int ConversionAmount { get; set; } // Only used for conversions and value bonus
    public ValueChangeSourceType ValueChangeSourceType { get; set; }
}


// Data for modifying a ValueChange
public class ValueConversionModification
{
    public ValueTypes ValueType { get; set; }
    public int OriginalValue { get; set; }
    public int SourceTargetValue { get; set; }
    public ValueTypes? TargetValueType { get; set; }  // Only used for conversions
    public int OriginalTargetValue { get; set; }      // Only used for conversions
    public int NewTargetValue { get; set; }           // Only used for conversions
    public int ConversionAmount { get; set; }         // Only used for conversions
    public ValueChangeSourceType ValueChangeSourceType { get; internal set; }
}

// Data for modifying a Requirement
public class RequirementModification
{
    public string RequirementType { get; set; } // Use string instead of enum for flexibility
    public int Amount { get; set; }
}

// Data for modifying an Outcome (Cost or Reward)
public class OutcomeModification
{
    public string OutcomeType { get; set; } // Use string instead of enum for flexibility
    public int Amount { get; set; }
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