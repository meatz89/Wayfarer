public class ChoiceSystem
{
    public ChoiceSystem(EncounterGenerator encounterGenerator)
    {
        EncounterGenerator = encounterGenerator;
    }

    public EncounterGenerator EncounterGenerator { get; }

    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        EncounterStateValues state = EncounterStateValues.InitialState;

        EncounterGenerator generator = new EncounterGenerator();
        List<EncounterChoice> choices = generator.GenerateChoices(context);

        return choices;

    }
}
