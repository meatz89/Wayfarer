public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly List<ChoiceSetTemplate> choiceSetTemplates;
    private ChoiceSetGenerator choiceSetGenerator;
    private readonly ChoiceSetSelector choiceSetSelector;
    private readonly ChoiceExecutor executor;

    public ChoiceSystem(GameContentProvider contentProvider, GameState gameState, GameContentProvider gameContentProvider)
    {
        this.gameState = gameState;
        this.choiceSetTemplates = contentProvider.GetChoiceSetTemplates();
        this.choiceSetSelector = new ChoiceSetSelector(gameState);
        this.executor = new ChoiceExecutor(gameState);
    }

    public void ExecuteChoice(EncounterChoice choice)
    {
        executor.ExecuteChoice(choice);
    }

    public ChoiceSet GenerateChoices(
        EncounterContext context)
    {
        // 1. Select appropriate template based on context
        ChoiceSetTemplate template = choiceSetSelector.SelectTemplate(
            choiceSetTemplates, context);
        if (template == null) return null;

        // 2. Create base choices with unmodified values
        this.choiceSetGenerator = new ChoiceSetGenerator(context);

        ChoiceSet choiceSet = choiceSetGenerator.GenerateChoiceSet(
            template);

        return choiceSet;
    }
}
