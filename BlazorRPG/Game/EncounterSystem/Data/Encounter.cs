public class Encounter
{
    public string EncounterGoal { get; }
    public string Situation { get; }
    public int CurrentStageIndex { get; private set; }
    private List<EncounterStage> stages = new();
    public EncounterContext EncounterContext { get; set; }
    public EncounterStage LastStage { get; internal set; }
    public Choice LastChoice { get; internal set; }
    public EffectTypes LastChoiceEffectType { get; internal set; }
    public ApproachTypes LastChoiceApproach { get; internal set; }
    public FocusTypes LastChoiceFocusType { get; internal set; }

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