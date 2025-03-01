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

    public EncounterState ApplyChoice(Choice choice, bool isStable)
    {
        EncounterState newState = new EncounterState()
        {
            ApproachTags = new Dictionary<ApproachTypes, int>(ApproachTags),
            FocusTags = new Dictionary<FocusTypes, int>(FocusTags),
            Momentum = Momentum,
            Pressure = Pressure,
            CurrentTurn = CurrentTurn + 1
        };

        if (choice.EffectType == EffectTypes.Momentum)
            newState.Momentum += choice.GetEffectValue(isStable);
        else
            newState.Pressure += choice.GetEffectValue(isStable);

        newState.ApproachTags[choice.Approach] = newState.ApproachTags[choice.Approach] + 1;
        newState.FocusTags[choice.Focus] = newState.FocusTags[choice.Focus] + 1;

        return newState;
    }
}
