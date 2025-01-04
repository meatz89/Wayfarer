public class NarrativeStageBuilder
{
    private int id;
    private string situationDescription;
    private List<NarrativeChoice> choices = new();

    public NarrativeStageBuilder WithId(int id)
    {
        this.id = id;
        return this;
    }

    public NarrativeStageBuilder WithSituation(string description)
    {
        this.situationDescription = description;
        return this;
    }

    public NarrativeStageBuilder AddChoice(Action<ChoiceBuilder> buildChoice)
    {
        ChoiceBuilder builder = new ChoiceBuilder();
        buildChoice(builder);
        choices.Add(builder.Build());
        return this;
    }

    public NarrativeStage Build()
    {
        return new NarrativeStage
        {
            Id = id,
            Situation = situationDescription,
            Choices = choices
        };
    }
}
