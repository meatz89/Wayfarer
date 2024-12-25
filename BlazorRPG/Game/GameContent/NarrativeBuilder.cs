
public class NarrativeBuilder
{
    private BasicActionTypes actionType;
    private string situationDescription;
    private List<Choice> choices = new();

    public NarrativeBuilder ForAction(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public NarrativeBuilder WithSituation(string description)
    {
        this.situationDescription = description;
        return this;
    }

    public NarrativeBuilder AddChoice(Action<ChoiceBuilder> buildChoice)
    {
        ChoiceBuilder builder = new ChoiceBuilder();
        buildChoice(builder);
        choices.Add(builder.Build());
        return this;
    }

    public Narrative Build()
    {
        return new Narrative
        {
            ActionType = actionType,
            SituationDescription = situationDescription,
            Choices = choices
        };
    }
}