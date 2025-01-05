public class NarrativeGenerator
{
    public List<NarrativeChoice> GenerateChoices(NarrativeActionContext context)
    {
        var generator = new NarrativeChoiceGenerator();
        var choices = generator.GenerateChoices(context);

        return choices;
    }

}