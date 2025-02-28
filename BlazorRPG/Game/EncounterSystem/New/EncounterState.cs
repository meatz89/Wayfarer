
/// <summary>
/// Represents the current state of an encounter
/// </summary>
public class EncounterState
{
    public int Momentum { get; set; }
    public int Pressure { get; set; }
    public Dictionary<ApproachTypes, int> ApproachTags { get; set; } = new Dictionary<ApproachTypes, int>();
    public Dictionary<FocusTypes, int> FocusTags { get; set; } = new Dictionary<FocusTypes, int>();
    public int CurrentTurn { get; set; }

    public bool IsStable => Pressure <= Momentum;

    public EncounterState()
    {
        // Initialize all tags to 0
        foreach (ApproachTypes approach in Enum.GetValues(typeof(ApproachTypes)))
        {
            ApproachTags[approach] = 0;
        }

        foreach (FocusTypes focus in Enum.GetValues(typeof(FocusTypes)))
        {
            FocusTags[focus] = 0;
        }
    }

    // Apply a choice's effects to this state
    public void ApplyChoice(Choice choice, bool isStable)
    {
        if (choice.EffectType == EffectTypes.Momentum)
            Momentum += choice.GetEffectValue(isStable);
        else
            Pressure += choice.GetEffectValue(isStable);

        ApproachTags[choice.Approach]++;
        FocusTags[choice.Focus]++;

        CurrentTurn++;
    }
}
