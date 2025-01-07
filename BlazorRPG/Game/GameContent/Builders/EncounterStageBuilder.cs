public class EncounterStageBuilder
{
    private int id;
    private string situationDescription;
    private List<EncounterChoice> choices = new();

    public EncounterStageBuilder WithId(int id)
    {
        this.id = id;
        return this;
    }

    public EncounterStageBuilder WithSituation(string description)
    {
        this.situationDescription = description;
        return this;
    }

    public EncounterStageBuilder AddChoice(Action<ChoiceBuilder> buildChoice)
    {
        ChoiceBuilder builder = new ChoiceBuilder();
        buildChoice(builder);
        choices.Add(builder.Build());
        return this;
    }

    public EncounterStage Build()
    {
        return new EncounterStage
        {
            Id = id,
            Situation = situationDescription,
            Choices = choices
        };
    }
}
