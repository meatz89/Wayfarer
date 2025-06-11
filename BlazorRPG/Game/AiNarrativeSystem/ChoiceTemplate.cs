public class ChoiceTemplate
{
    // Template identity
    public string TemplateName { get; private set; }
    public string StrategicPurpose { get; private set; }
    public int Weight { get; private set; }

    // Input mechanics
    public InputMechanics InputMechanics { get; private set; }

    // Direct effect class references
    public IMechanicalEffect SuccessEffect { get; private set; }
    public IMechanicalEffect FailureEffect { get; private set; }

    // Narrative guidance for AI
    public string ConceptualOutput { get; private set; }
    public string SuccessOutcomeNarrativeGuidance { get; private set; }
    public string FailureOutcomeNarrativeGuidance { get; private set; }

    public Dictionary<SkillCategories, ApproachCost> ContextualCosts { get; private set; }

    public ChoiceTemplate(
        string templateName,
        string strategicPurpose,
        int weight,
        InputMechanics inputMechanics,
        IMechanicalEffect successEffect,
        IMechanicalEffect failureEffect,
        string conceptualOutput,
        string successOutcomeNarrativeGuidance,
        string failureOutcomeNarrativeGuidance)
    {

        TemplateName = templateName;
        StrategicPurpose = strategicPurpose;
        Weight = weight;
        InputMechanics = inputMechanics;
        SuccessEffect = successEffect;
        FailureEffect = failureEffect;
        ConceptualOutput = conceptualOutput;
        SuccessOutcomeNarrativeGuidance = successOutcomeNarrativeGuidance;
        FailureOutcomeNarrativeGuidance = failureOutcomeNarrativeGuidance;
    }


    public ApproachCost GetCost(SkillCategories approach, Location location, TimeOfDay timeOfDay)
    {
        ApproachCost baseCost = ContextualCosts.ContainsKey(approach)
            ? ContextualCosts[approach]
            : new ApproachCost(1, 0, 0, TimeSpan.FromHours(1));

        // Modify based on location properties
        List<FlagStates> locationFlags = location.GetCurrentFlags(timeOfDay);

        // Apply modifiers based on flags
        // This is a simple example - you would expand this based on your flag system
        if (locationFlags.Contains(FlagStates.Crowded) && approach == SkillCategories.Intellectual)
        {
            // Intellectual approaches cost more energy in crowded places
            return new ApproachCost(
                baseCost.EnergyCost + 1,
                baseCost.MoneyCost,
                baseCost.ReputationImpact,
                baseCost.TimeCost
            );
        }

        // More contextual modifiers here...

        return baseCost;
    }
}
