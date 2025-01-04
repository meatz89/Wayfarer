public class ChoiceValidator
{
    // Constants for validation thresholds
    private const int MinimumChoices = 2;
    private const int MaximumChoices = 5;

    public bool ValidateChoiceSet(List<NarrativeChoice> choices)
    {
        // First check if we have enough choices
        if (choices.Count < MinimumChoices || choices.Count > MaximumChoices)
        {
            return false;
        }

        return HasBaseChoice(choices) &&
               HasTradeoffChoice(choices) &&
               HasOpportunityChoice(choices) &&
               AllValuesAboveZero(choices) &&
               HandlesCriticalValues(choices);
    }

    private bool HasBaseChoice(List<NarrativeChoice> choices)
    {
        // A base choice should have no special requirements and use standard energy costs
        return choices.Any(choice =>
            choice.Requirement == null &&
            choice.Cost is EnergyOutcome energyCost &&
            energyCost.Amount <= 2);
    }

    private bool HasTradeoffChoice(List<NarrativeChoice> choices)
    {
        // A tradeoff choice should have both positive and negative narrative state changes
        return choices.Any(choice =>
        {
            var changes = choice.NarrativeStateChanges;
            bool hasPositive = changes.Momentum > 0 || changes.Advantage > 0 ||
                              changes.Understanding > 0 || changes.Connection > 0;
            bool hasNegative = changes.Momentum < 0 || changes.Advantage < 0 ||
                              changes.Understanding < 0 || changes.Connection < 0;

            return hasPositive && hasNegative;
        });
    }

    private bool HasOpportunityChoice(List<NarrativeChoice> choices)
    {
        // An opportunity choice should either have a permanent reward or unlock special options
        return choices.Any(choice =>
            HasPermanentReward(choice) || UnlocksSpecialOption(choice));
    }

    private bool HasPermanentReward(NarrativeChoice choice)
    {
        return choice.Reward is SkillLevelOutcome ||
               choice.Reward is ReputationOutcome ||
               choice.Reward is AchievementOutcome;
    }

    private bool UnlocksSpecialOption(NarrativeChoice choice)
    {
        return choice.UnlockedOptions.Any();
    }

    private bool AllValuesAboveZero(List<NarrativeChoice> choices)
    {
        // Ensure no choice would reduce any value below zero
        foreach (var choice in choices)
        {
            var changes = choice.NarrativeStateChanges;

            // We need context of current values to properly validate this
            // For now, we'll just ensure no individual change is too extreme
            if (changes.Momentum < -3 || changes.Advantage < -3 ||
                changes.Understanding < -3 || changes.Connection < -3)
            {
                return false;
            }
        }

        return true;
    }

    private bool HandlesCriticalValues(List<NarrativeChoice> choices)
    {
        // For high-value states (8+), ensure we have appropriate special choices
        bool hasFlowStateChoice = choices.Any(choice =>
            choice.ChoiceType == ChoiceTypes.Aggressive &&
            choice.CompletionPoints >= 4);  // Double normal points

        bool hasPowerPlayChoice = choices.Any(choice =>
            choice.Requirement == null &&  // Requirements skipped
            choice.Reward != null);        // Still has rewards

        bool hasInsightChoice = choices.Any(choice =>
            choice.UnlockedOptions.Any() &&
            choice.NarrativeStateChanges.Connection > 0);

        bool hasTrustChoice = choices.Any(choice =>
            choice.UnlockedOptions.Contains("SPECIAL_REQUEST") &&
            choice.NarrativeStateChanges.Advantage > 0);

        bool hasPressureChoice = choices.Any(choice =>
            choice.NarrativeStateChanges.Momentum > 0 &&
            choice.NarrativeStateChanges.Tension > 0);

        // We don't require all of these to be present, but at least one should exist
        return hasFlowStateChoice || hasPowerPlayChoice || hasInsightChoice ||
               hasTrustChoice || hasPressureChoice;
    }

    public string GetValidationReport(List<NarrativeChoice> choices)
    {
        var issues = new List<string>();

        if (!HasBaseChoice(choices))
            issues.Add("Missing a basic choice with no special requirements");

        if (!HasTradeoffChoice(choices))
            issues.Add("Missing a choice with meaningful tradeoffs");

        if (!HasOpportunityChoice(choices))
            issues.Add("Missing a choice with permanent rewards or special opportunities");

        if (!AllValuesAboveZero(choices))
            issues.Add("Some choices have too extreme negative value changes");

        if (!HandlesCriticalValues(choices))
            issues.Add("Missing appropriate choices for high-value states");

        return issues.Any()
            ? $"Validation Issues:\n{string.Join("\n", issues)}"
            : "All validation criteria met";
    }
}