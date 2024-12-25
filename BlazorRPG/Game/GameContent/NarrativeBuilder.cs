
public class NarrativeBuilder
{
    private BasicActionTypes actionType;
    private List<NarrativeStage> stages = new();

    public NarrativeBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public NarrativeBuilder AddStage(Action<NarrativeStageBuilder> buildStage)
    {
        NarrativeStageBuilder builder = new NarrativeStageBuilder();
        buildStage(builder);
        stages.Add(builder.Build());
        return this;
    }

    public Narrative Build()
    {
        return new Narrative
        {
            ActionType = actionType,
            Stages = stages
        };
    }
}