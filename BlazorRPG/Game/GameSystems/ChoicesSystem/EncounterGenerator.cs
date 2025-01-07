public class EncounterGenerator
{
    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        EncounterChoiceGenerator generator = new EncounterChoiceGenerator();
        List<EncounterChoice> choices = generator.GenerateChoices(context);

        return choices;
    }

}