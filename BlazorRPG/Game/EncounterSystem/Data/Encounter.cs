public class Encounter
{
    public EncounterContext Context { get; }
    public string Situation { get; }
    private List<EncounterStage> stages = new();
    public int CurrentStageIndex { get; private set; }

    public Encounter(EncounterContext context, string situation)
    {
        Context = context;
        Situation = situation;
        CurrentStageIndex = 0;
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