public class Encounter
{
    public EncounterContext EncounterContext { get; }
    private List<EncounterStage> stages = new();
    private int currentStage { get; set; } = 0;
    public int NumberOfStages => stages.Count();
    public string Situation { get; set; }

    public List<ChoiceConsequences> ChoiceValueModifications { get; } = new();

    public Encounter(EncounterContext context, string situation)
    {
        EncounterContext = context;
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
                EncounterContext.CurrentValues.Outcome += change;
                break;
            case ValueTypes.Pressure:
                EncounterContext.CurrentValues.Pressure += change;
                break;
            case ValueTypes.Insight:
                EncounterContext.CurrentValues.Insight += change;
                break;
            case ValueTypes.Resonance:
                EncounterContext.CurrentValues.Resonance += change;
                break;
        }

        // Clamp all values to valid range
        EncounterContext.CurrentValues.ClampValues();
    }
}