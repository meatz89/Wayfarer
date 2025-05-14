public class ChoiceProjection
{
    // Source choice
    public CardDefinition Choice { get; }

    // State changes
    public int MomentumGained { get; set; }
    public int PressureBuilt { get; set; }

    // Value components for detailed breakdowns
    public class ValueComponent
    {
        public string Source { get; set; }
        public int Value { get; set; }
    }

    public List<ValueComponent> MomentumComponents { get; } = new List<ValueComponent>();
    public List<ValueComponent> PressureComponents { get; } = new List<ValueComponent>();

    // Resource change tracking
    public List<ValueComponent> HealthComponents { get; } = new List<ValueComponent>();
    public List<ValueComponent> ConcentrationComponents { get; } = new List<ValueComponent>();
    public List<ValueComponent> ConfidenceComponents { get; } = new List<ValueComponent>();
    public List<string> StrategicTagEffects = new List<string>();

    public int HealthChange { get; set; }
    public int ConcentrationChange { get; set; }
    public int ConfidenceChange { get; set; }

    // Tag changes
    public List<string> NewlyActivatedTags { get; }
    public List<string> DeactivatedTags { get; }

    // Projected state
    public int FinalMomentum { get; set; }
    public int FinalPressure { get; set; }
    public int ProjectedTurn { get; set; }
    public bool EncounterWillEnd { get; set; }
    public EncounterOutcomes ProjectedOutcome { get; set; }

    // Narrative description
    public string NarrativeDescription { get; set; }

    public ChoiceProjection(CardDefinition choice)
    {
        Choice = choice;
        NewlyActivatedTags = new List<string>();
        DeactivatedTags = new List<string>();
    }
}