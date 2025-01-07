public class EncounterStageBuilder
{
    private string situationDescription;
    private List<EncounterChoice> choices = new();

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
            Situation = situationDescription,
            Choices = choices
        };
    }
}
