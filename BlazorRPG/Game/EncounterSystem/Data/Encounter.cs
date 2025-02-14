public class Encounter
{
    public string EncounterGoal { get; }
    public string Situation { get; }
    public int CurrentStageIndex { get; private set; }
    private List<EncounterStage> stages = new();
    public EncounterContext EncounterContext { get; }
    public EncounterStage LastStage { get; internal set; }
    public EncounterChoice LastChoice { get; internal set; }
    public ChoiceArchetypes LastChoiceType { get; internal set; }
    public ChoiceApproaches LastChoiceApproach { get; internal set; }

    public Encounter(string goal)
    {
        EncounterGoal = goal;
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
    
}