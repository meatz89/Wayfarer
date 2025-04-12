public class ChoiceProjection
{
    // Source choice
    public ChoiceCard Choice { get; }

    // State changes
    public Dictionary<ApproachTags, int> EncounterStateTagChanges { get; }
    public Dictionary<FocusTags, int> FocusTagChanges { get; }
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

    public ChoiceProjection(ChoiceCard choice)
    {
        Choice = choice;
        EncounterStateTagChanges = new Dictionary<ApproachTags, int>();
        FocusTagChanges = new Dictionary<FocusTags, int>();
        NewlyActivatedTags = new List<string>();
        DeactivatedTags = new List<string>();
    }
}