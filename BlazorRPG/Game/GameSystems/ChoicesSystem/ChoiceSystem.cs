using System.Text;

public class ChoiceSystem
{
    private readonly GameState gameState;
    private readonly List<ChoiceSetTemplate> choiceSetTemplates;
    private readonly ChoiceSetFactory choiceSetFactory;
    private readonly ChoiceSetSelector choiceSetSelector;
    private readonly ChoiceCalculator calculator;
    private readonly ChoiceExecutor executor;

    public ChoiceSystem(GameContentProvider contentProvider, GameState gameState)
    {
        this.gameState = gameState;
        this.choiceSetTemplates = contentProvider.GetChoiceSetTemplates();
        this.choiceSetSelector = new ChoiceSetSelector();
        this.choiceSetFactory = new ChoiceSetFactory();
        this.calculator = new ChoiceCalculator();
        this.executor = new ChoiceExecutor(gameState);
    }

    public List<EncounterChoice> GenerateChoices(EncounterContext context)
    {
        // 1. Select appropriate template based on context
        ChoiceSetTemplate template = choiceSetSelector.SelectTemplate(
            choiceSetTemplates, context);
        if (template == null) return null;

        // 2. Create base choices with unmodified values
        ChoiceSet choiceSet = choiceSetFactory.CreateFromTemplate(
            template, context);

        // 3. Calculate consequences for each choice
        foreach (EncounterChoice choice in choiceSet.Choices)
        {
            choice.Consequences = calculator.CalculateConsequences(choice, context);
        }

        return choiceSet.Choices;
    }

    public void ExecuteChoice(EncounterChoice choice)
    {
        executor.ExecuteChoice(choice);
    }

    public string GetChoicePreview(EncounterChoice choice)
    {
        StringBuilder preview = new StringBuilder();
        ChoiceConsequences consequences = choice.Consequences;

        // Value Changes Preview
        preview.AppendLine("Encounter Value Changes:");
        foreach (ValueChange baseChange in consequences.BaseValueChanges)
        {
            ValueChangeDetail? detail = consequences.ValueChangeDetails
                .FirstOrDefault(d => d.ValueType == baseChange.ValueType);

            if (detail != null)
            {
                preview.AppendLine($"{baseChange.ValueType}:");
                foreach (ValueChangeSource source in detail.ValueChanges)
                {
                    preview.AppendLine($"  {source.Source}: {source.Amount:+#;-#;0}");
                }
                int total = detail.ValueChanges.Sum(s => s.Amount);
                preview.AppendLine($"  Total: {total:+#;-#;0}");
            }
        }

        // Cost Modifications Preview
        if (consequences.CostModifications.Any())
        {
            preview.AppendLine("\nCost Modifications:");
            foreach (OutcomeModification mod in consequences.CostModifications)
            {
                preview.AppendLine($"{mod.OutcomeType}:");
                preview.AppendLine($"  Base: {consequences.BaseCosts.First(c => c.GetDescription().Contains(mod.OutcomeType)).GetDescription()}");
                preview.AppendLine($"  {mod.Source}: {mod.Amount:+#;-#;0}");
                Outcome modifiedCost = consequences.ModifiedCosts.First(c => c.GetDescription().Contains(mod.OutcomeType));
                preview.AppendLine($"  Final: {modifiedCost.GetDescription()}");
            }
        }

        // Reward Modifications Preview
        if (consequences.RewardModifications.Any())
        {
            preview.AppendLine("\nReward Modifications:");
            foreach (OutcomeModification mod in consequences.RewardModifications)
            {
                preview.AppendLine($"{mod.OutcomeType}:");
                preview.AppendLine($"  Base: {consequences.BaseRewards.First(r => r.GetDescription().Contains(mod.OutcomeType)).GetDescription()}");
                preview.AppendLine($"  {mod.Source}: {mod.Amount:+#;-#;0}");
                Outcome modifiedReward = consequences.ModifiedRewards.First(r => r.GetDescription().Contains(mod.OutcomeType));
                preview.AppendLine($"  Final: {modifiedReward.GetDescription()}");
            }
        }

        return preview.ToString();
    }
}