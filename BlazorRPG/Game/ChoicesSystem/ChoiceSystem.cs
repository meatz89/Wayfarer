
public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly List<SpecialChoiceTemplate> choiceSetTemplates;
    private ChoiceGenerator choiceSetGenerator;
    private readonly ChoiceSetSelector choiceSetSelector;

    public ChoiceSystem(
        GameContentProvider contentProvider,
        GameState gameState)
    {
        this.gameState = gameState;
        this.choiceSetTemplates = contentProvider.GetChoiceSetTemplates();
        this.choiceSetSelector = new ChoiceSetSelector(gameState);
        this.choiceSetGenerator = new ChoiceGenerator(gameState);
    }

    public ChoiceSet GenerateChoices(
        EncounterContext context)
    {
        // 1. Select appropriate template based on context
        SpecialChoiceTemplate template = choiceSetSelector.SelectTemplate(
            choiceSetTemplates, context);

        ActionImplementation actionImplementation = context.ActionImplementation;

        if (template == null)
        {
            template = GetStandardTemplate(actionImplementation.Name, context.ActionType);
        }

        // 2. Create base choices with unmodified values
        ChoiceSet choiceSet = choiceSetGenerator.Generate(
            template, context);

        return choiceSet;
    }

    private SpecialChoiceTemplate? GetStandardTemplate(string name, BasicActionTypes actionType)
    {
        return new SpecialChoiceTemplate(name, actionType,
            null, null, null, null, null
            );
    }
}
