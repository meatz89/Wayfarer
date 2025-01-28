public class Encounter
{
    public string EncounterGoal { get; }
    public string Situation { get; }
    public EncounterContext Context { get; }
    public List<EncounterChoiceSlot> BaseSlots { get; set; } = new();
    public List<EncounterChoiceSlot> ModifiedSlots { get; set; } = new();

    private List<EncounterStage> stages = new();
    public int CurrentStageIndex { get; private set; }

    public Encounter(EncounterContext context, string goal, List<EncounterChoiceSlot> baseSlots)
    {
        Context = context;
        EncounterGoal = goal;
        CurrentStageIndex = 0;
        BaseSlots = baseSlots;
    }

    public void AddStage(EncounterStage stage)
    {
        stages.Add(stage);
    }

    public EncounterStage GetCurrentStage()
    {
        if (CurrentStageIndex >= stages.Count)
            return null;

        return stages[CurrentStageIndex];
    }

    public void AdvanceStage()
    {
        CurrentStageIndex++;
    }

    public void ModifyValue(ValueTypes valueType, int change)
    {
        switch (valueType)
        {
            case ValueTypes.Outcome:
                Context.CurrentValues.Outcome += change;
                break;
            case ValueTypes.Momentum:
                Context.CurrentValues.Momentum += change;
                break;
            case ValueTypes.Pressure:
                Context.CurrentValues.Pressure += change;
                break;
            case ValueTypes.Insight:
                Context.CurrentValues.Insight += change;
                break;
            case ValueTypes.Resonance:
                Context.CurrentValues.Resonance += change;
                break;
        }
        Context.CurrentValues.ClampValues();
    }
}