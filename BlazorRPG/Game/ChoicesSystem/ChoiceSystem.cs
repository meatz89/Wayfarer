public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly List<ChoiceSetTemplate> choiceSetTemplates;
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
        ChoiceSetTemplate template = choiceSetSelector.SelectTemplate(
            choiceSetTemplates, context);

        ActionImplementation actionImplementation = context.ActionImplementation;

        if (template == null)
        {
            template = GetStandardTemplate(context.ActionType, actionImplementation.Name);
        }

        // 2. Create base choices with unmodified values
        ChoiceSet choiceSet = choiceSetGenerator.Generate(
            template, context);

        return choiceSet;
    }

    private ChoiceSetTemplate GetStandardTemplate(BasicActionTypes actionType, string name)
    {
        ChoiceSetTemplate template = new ChoiceSetTemplate()
        {
            ActionType = actionType,
            Name = name,
            CompositionPattern = new CompositionPattern()
            {
                SecondaryArchetype = ChoiceArchetypes.Physical,
                PrimaryArchetype = ChoiceArchetypes.Social,
            }
        };
        return template;
    }
}
