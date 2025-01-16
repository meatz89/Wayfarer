public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly List<ChoiceSetTemplate> choiceSetTemplates;
    private ChoiceSetGenerator choiceSetGenerator;
    private readonly ChoiceSetSelector choiceSetSelector;

    public ChoiceSystem(
        GameContentProvider contentProvider,
        GameState gameState)
    {
        this.gameState = gameState;
        this.choiceSetTemplates = contentProvider.GetChoiceSetTemplates();
        this.choiceSetSelector = new ChoiceSetSelector(gameState);
        this.choiceSetGenerator = new ChoiceSetGenerator(gameState);
    }

    public ChoiceSet GenerateChoices(
        EncounterContext context)
    {
        // 1. Select appropriate template based on context
        ChoiceSetTemplate template = choiceSetSelector.SelectTemplate(
            choiceSetTemplates, context);
        if (template == null) return null;

        // 2. Create base choices with unmodified values
        ChoiceSet choiceSet = choiceSetGenerator.Generate(
            template, context);

        return choiceSet;
    }

}
