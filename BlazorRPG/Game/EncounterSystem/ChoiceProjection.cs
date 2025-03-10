using BlazorRPG.Game.EncounterManager;

public class ChoiceProjection
{
    // Source choice
    public IChoice Choice { get; }

    // State changes
    public Dictionary<ApproachTags, int> ApproachTagChanges { get; }
    public Dictionary<FocusTags, int> FocusTagChanges { get; }
    public int MomentumGained { get; set; }
    public int PressureBuilt { get; set; }

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

    public ChoiceProjection(IChoice choice)
    {
        Choice = choice;
        ApproachTagChanges = new Dictionary<ApproachTags, int>();
        FocusTagChanges = new Dictionary<FocusTags, int>();
        NewlyActivatedTags = new List<string>();
        DeactivatedTags = new List<string>();
    }
}