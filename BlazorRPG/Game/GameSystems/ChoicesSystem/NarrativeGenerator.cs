public class NarrativeGenerator
{
    public List<NarrativeChoice> GenerateChoices(NarrativeActionContext context)
    {
        NarrativeChoiceGenerator generator = new NarrativeChoiceGenerator();
        List<NarrativeChoice> choices = generator.GenerateChoices(context);

        return choices;
    }

}