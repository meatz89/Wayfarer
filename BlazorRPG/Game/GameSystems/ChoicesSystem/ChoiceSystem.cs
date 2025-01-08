public class ChoiceSystem
{
    private readonly Random random = new();
    private readonly List<ChoiceSetTemplate> choiceSetTemplates;
    private readonly ChoiceSetFactory choiceSetFactory;
    private readonly ChoiceSetSelector choiceSetSelector;

    public ChoiceSystem(GameContentProvider contentProvider)
    {
        this.choiceSetTemplates = contentProvider.GetChoiceSetTemplates();
        this.choiceSetFactory = new ChoiceSetFactory();
        this.choiceSetSelector = new ChoiceSetSelector();
    }

    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        // Get suitable template
        ChoiceSetTemplate template = choiceSetSelector.SelectTemplate(
            choiceSetTemplates,
            context);

        // Create implementation
        ChoiceSet choiceSet = choiceSetFactory.CreateFromTemplate(
            template,
            context);

        return choiceSet.Choices;
    }
}