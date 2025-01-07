public class ChoiceSystem
{
    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        EncounterStateValues state = EncounterStateValues.InitialState;

        EncounterChoiceGenerator generator = new EncounterChoiceGenerator();
        List<EncounterChoice> choices = generator.GenerateChoices(context);

        return choices;

    }
}
