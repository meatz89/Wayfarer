public class Encounter
{
    public EncounterActionContext Context { get; }
    public List<EncounterStage> Stages { get; set; } = new();
    public int CurrentStage { get; set; } = 0;
    public int NumberOfStages => Stages.Count();
    public string Situation { get; set; }

    public Encounter(EncounterActionContext context, string situation)
    {
        Context = context;
        Situation = situation;
    }
}