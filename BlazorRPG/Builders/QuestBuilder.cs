public class QuestBuilder
{
    private string title;
    private string description;
    private readonly List<QuestStep> steps = new();

    public QuestBuilder WithTitle(string title)
    {
        this.title = title;
        return this;
    }

    public QuestBuilder AddStep(Action<QuestStepBuilder> buildStep)
    {
        QuestStepBuilder builder = new();
        buildStep(builder);
        steps.Add(builder.Build());
        return this;
    }

    public QuestBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public Quest Build()
    {
        // Create immutable quest with fixed array of steps
        return new Quest(title, description, steps.ToArray());
    }

}
