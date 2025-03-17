/// <summary>
/// Represents the outcome of the player character (PC)'s choice
/// </summary>
public class ChoiceOutcome
{
    public string Description { get; }
    public bool IsEncounterOver { get; }
    public EncounterOutcomes Outcome { get; }

    public int MomentumGain { get; }
    public int PressureGain { get; }

    public int HealthChange { get; }
    public int FocusChange { get; }
    public int ConfidenceChange { get; }

    // Added tag-related fields
    public Dictionary<FocusTags, int> FocusTagChanges { get; }
    public Dictionary<EncounterStateTags, int> EncounterStateTagChanges { get; }
    public List<string> NewlyActivatedTags { get; }
    public List<string> DeactivatedTags { get; }

    public ChoiceOutcome(
        int momentumGained,
        int pressureBuilt,
        string description,
        bool isEncounterOver,
        EncounterOutcomes outcome,
        int healthChange = 0,
        int concentrationChange = 0,
        int reputationChange = 0)
    {
        MomentumGain = momentumGained;
        PressureGain = pressureBuilt;
        Description = description;
        IsEncounterOver = isEncounterOver;
        Outcome = outcome;
        HealthChange = healthChange;
        FocusChange = concentrationChange;
        ConfidenceChange = reputationChange;

        // Initialize empty collections
        FocusTagChanges = new Dictionary<FocusTags, int>();
        EncounterStateTagChanges = new Dictionary<EncounterStateTags, int>();
        NewlyActivatedTags = new List<string>();
        DeactivatedTags = new List<string>();
    }
}
