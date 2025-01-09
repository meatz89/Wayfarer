using System.Text;
public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly List<ChoiceSetTemplate> choiceSetTemplates;
    private readonly ChoiceSetFactory choiceSetFactory;
    private readonly ChoiceSetSelector choiceSetSelector;
    private readonly ChoiceCalculator calculator;
    private readonly ChoiceExecutor executor;

    private readonly List<LocationPropertyChoiceEffect> locationContextEffects;

    public ChoiceSystem(GameContentProvider contentProvider, GameState gameState, GameContentProvider gameContentProvider)
    {
        this.gameState = gameState;
        this.choiceSetTemplates = contentProvider.GetChoiceSetTemplates();
        this.choiceSetSelector = new ChoiceSetSelector();
        this.choiceSetFactory = new ChoiceSetFactory();
        this.executor = new ChoiceExecutor(gameState);

        this.locationContextEffects = gameContentProvider.GetLocationArchetypeEffects();
        this.calculator = new ChoiceCalculator(locationContextEffects);
    }

    public List<EncounterChoice> GenerateChoices(EncounterContext context)
    {
        // 1. Select appropriate template based on context
        ChoiceSetTemplate template = choiceSetSelector.SelectTemplate(
            choiceSetTemplates, context);
        if (template == null) return null;

        // 2. Create base choices with unmodified values
        ChoiceSet choiceSet = choiceSetFactory.CreateFromChoiceSet(
            template, context);

        // 3. Calculate consequences for each choice
        foreach (EncounterChoice choice in choiceSet.Choices)
        {
            calculator.CalculateChoice(choice, context);
        }

        return choiceSet.Choices;
    }

    public void ExecuteChoice(EncounterChoice choice)
    {
        executor.ExecuteChoice(choice);
    }
}