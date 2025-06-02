public class ChoiceTemplate
{
    // Template identity
    public string TemplateName { get; private set; }
    public string StrategicPurpose { get; private set; }
    public int Weight { get; private set; }

    // Input mechanics
    public InputMechanics InputMechanics { get; private set; }

    // Direct effect class references
    public Type SuccessEffectClass { get; private set; }
    public Type FailureEffectClass { get; private set; }

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
        Type successEffectClass,
        Type failureEffectClass,
        string conceptualOutput,
        string successOutcomeNarrativeGuidance,
        string failureOutcomeNarrativeGuidance)
    {
        if (!typeof(IMechanicalEffect).IsAssignableFrom(successEffectClass))
        {
            throw new ArgumentException($"Success effect class must implement IMechanicalEffect: {successEffectClass.Name}");
        }

        if (!typeof(IMechanicalEffect).IsAssignableFrom(failureEffectClass))
        {
            throw new ArgumentException($"Failure effect class must implement IMechanicalEffect: {failureEffectClass.Name}");
        }

        TemplateName = templateName;
        StrategicPurpose = strategicPurpose;
        Weight = weight;
        InputMechanics = inputMechanics;
        SuccessEffectClass = successEffectClass;
        FailureEffectClass = failureEffectClass;
        ConceptualOutput = conceptualOutput;
        SuccessOutcomeNarrativeGuidance = successOutcomeNarrativeGuidance;
        FailureOutcomeNarrativeGuidance = failureOutcomeNarrativeGuidance;
    }


    // Get the appropriate cost based on approach and context
    public ApproachCost GetCost(SkillCategories approach, Location location, TimeOfDay timeOfDay)
    {
        // Base cost from the approach
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

    // Create and execute the success effect
    public void ExecuteSuccessEffect(EncounterState state)
    {
        IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(SuccessEffectClass);
        effect.Apply(state);
    }

    // Create and execute the failure effect
    public void ExecuteFailureEffect(EncounterState state)
    {
        IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(FailureEffectClass);
        effect.Apply(state);
    }

    // For JSON serialization - provides template info to AI
    public object ToJsonObject()
    {
        return new
        {
            TemplateName = TemplateName,
            StrategicPurpose = StrategicPurpose,
            Weight = Weight,
            InputMechanics = InputMechanics.ToJsonObject(),
            ConceptualOutput = ConceptualOutput,
            SuccessOutcomeNarrativeGuidance = SuccessOutcomeNarrativeGuidance,
            FailureOutcomeNarrativeGuidance = FailureOutcomeNarrativeGuidance
        };
    }
}
