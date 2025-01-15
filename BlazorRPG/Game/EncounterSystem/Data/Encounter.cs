public class Encounter
{
    public EncounterContext Context { get; }
    private List<EncounterStage> stages = new();
    private int currentStage { get; set; } = 0;
    public int NumberOfStages => stages.Count();
    public string Situation { get; set; }

    public Encounter(EncounterContext context, string situation)
    {
        Context = context;
        Situation = situation;
    }

    public void AddStage(EncounterStage encounterStage)
    {
        stages.Add(encounterStage);

        if (NumberOfStages != 1)
        {
            NextStage();
        }
    }

    public EncounterStage GetCurrentStage()
    {
        return stages[currentStage];
    }

    public void NextStage()
    {
        currentStage++;
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

        // Clamp all values to valid range
        //Context.CurrentValues.ClampValues();
    }
}