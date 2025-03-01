
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
    public EncounterState ApplyChoice(Choice choice, bool isStable)
    {
        var newState = new EncounterState()
        {
            ApproachTags = ApproachTags,
            FocusTags = FocusTags,
            Momentum = Momentum,
            Pressure = Pressure,
            CurrentTurn = CurrentTurn + 1
        };

        if (choice.EffectType == EffectTypes.Momentum)
            newState.Momentum += choice.GetEffectValue(isStable);
        else
            newState.Pressure += choice.GetEffectValue(isStable);

        newState.ApproachTags[choice.Approach]++;
        newState.FocusTags[choice.Focus]++;

        return newState;
    }
}
