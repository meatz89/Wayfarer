public class ChoiceSystem
{
    public ChoiceSystem(NarrativeGenerator narrativeGenerator)
    {
        NarrativeGenerator = narrativeGenerator;
    }

    public NarrativeGenerator NarrativeGenerator { get; }

    public List<NarrativeChoice> GenerateChoices(NarrativeActionContext context)
    {
        NarrativeStateValues state = NarrativeStateValues.InitialState;

        NarrativeGenerator generator = new NarrativeGenerator();
        List<NarrativeChoice> choices = generator.GenerateChoices(context);

        return choices;

    }
}
