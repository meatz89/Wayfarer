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