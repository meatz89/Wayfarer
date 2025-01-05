public class ChoiceSystem
{
    public ChoiceSystem(NarrativeGenerator narrativeGenerator)
    {
        NarrativeGenerator = narrativeGenerator;
    }

    public NarrativeGenerator NarrativeGenerator { get; }

    public List<NarrativeChoice> GenerateChoices(NarrativeActionContext context)
    {
        NarrativeState state = NarrativeState.InitialState;

        var generator = new NarrativeGenerator();
        List<NarrativeChoice> choices = generator.GenerateChoices(context);

        return choices;

    }
}
