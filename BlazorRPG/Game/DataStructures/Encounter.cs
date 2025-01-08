
public class Encounter
{
    public EncounterActionContext Context { get; }
    private List<EncounterStage> stages = new();
    private int currentStage { get; set; } = 0;
    public int NumberOfStages => stages.Count();
    public string Situation { get; set; }

    public Encounter(EncounterActionContext context, string situation)
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
}