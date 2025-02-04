public class ChoiceSystem
{
    private readonly GameState gameState;
    private ChoiceGenerator choiceSetGenerator;

    public ChoiceSystem(
        GameContentProvider contentProvider,
        GameState gameState)
    {
        this.gameState = gameState;
        this.choiceSetGenerator = new ChoiceGenerator(gameState);
    }

    public ChoiceSet GenerateChoices(
        Encounter encounter,
        EncounterContext context)
    {
        ChoiceSet choiceSet = choiceSetGenerator.Generate(encounter, context);

        return choiceSet;
    }
}
