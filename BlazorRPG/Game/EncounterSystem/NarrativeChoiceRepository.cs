public class NarrativeChoiceRepository
{
    private readonly List<EncounterOption> narrativeChoices = new();

    public NarrativeChoiceRepository()
    {
        InitializeChoices();
    }

    private void InitializeChoices()
    {
        // Safety Options (0 Focus cost - always available)
        InitializeSafetyOptions();

        // Token Generation Options (1-2 Focus cost)
        InitializeTokenGenerationOptions();

        // Token Conversion Options (1-2 Focus cost, require tokens)
        InitializeTokenConversionOptions();
    }

    private void InitializeSafetyOptions()
    {
        // Physical Safety Options
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "observe_physical_situation",
            "Survey the Situation",
            "Take a moment to carefully assess the physical challenges ahead",
            SkillTypes.Agility,
            4,
            0, // Focus cost
            UniversalActionType.SafetyOption,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Flow, 1 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.ThresholdIncrease,
            new List<string> { "Physical", "Safety", "Observation" }
        ));

        // Social Safety Options  
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "observe_social_situation",
            "Listen and Assess",
            "Quietly observe the social dynamics before taking action",
            SkillTypes.Persuasion,
            4,
            0, // Focus cost
            UniversalActionType.SafetyOption,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Focus, 1 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.ThresholdIncrease,
            new List<string> { "Social", "Safety", "Observation" }
        ));

        // Intellectual Safety Options
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "observe_intellectual_situation",
            "Consider Options",
            "Think through the problem methodically before proceeding",
            SkillTypes.Planning,
            4,
            0, // Focus cost
            UniversalActionType.SafetyOption,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Focus, 1 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.ThresholdIncrease,
            new List<string> { "Intellectual", "Safety", "Planning" }
        ));
    }

    private void InitializeTokenGenerationOptions()
    {
        // FORCE GENERATION OPTIONS

        // Physical Force Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "apply_physical_force",
            "Apply Direct Force",
            "Use raw physical strength to overcome the obstacle",
            SkillTypes.Strength,
            3,
            1, // Focus cost
            UniversalActionType.GenerateForce,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Force, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.ProgressLoss,
            new List<string> { "Physical", "Force", "Direct" }
        ));

        // Social Force Generation  
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "assert_dominance",
            "Assert Dominance",
            "Take command of the social situation through forceful presence",
            SkillTypes.Intimidation,
            3,
            1, // Focus cost
            UniversalActionType.GenerateForce,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Force, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.ProgressLoss,
            new List<string> { "Social", "Force", "Intimidation" }
        ));

        // Intellectual Force Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "cut_to_analysis",
            "Direct Analysis",
            "Cut straight to the core of the problem with analytical thinking",
            SkillTypes.Analysis,
            3,
            1, // Focus cost
            UniversalActionType.GenerateForce,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Force, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.ProgressLoss,
            new List<string> { "Intellectual", "Force", "Analysis" }
        ));

        // FLOW GENERATION OPTIONS

        // Physical Flow Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "move_fluidly",
            "Move Fluidly",
            "Adapt your movement to flow around obstacles",
            SkillTypes.Agility,
            3,
            1, // Focus cost
            UniversalActionType.GenerateFlow,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Flow, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Physical", "Flow", "Adaptive" }
        ));

        // Social Flow Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "read_and_adapt",
            "Read and Adapt",
            "Read the social situation and adapt your approach accordingly",
            SkillTypes.Charm,
            3,
            1, // Focus cost
            UniversalActionType.GenerateFlow,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Flow, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Social", "Flow", "Adaptive" }
        ));

        // Intellectual Flow Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "follow_patterns",
            "Follow Patterns",
            "Notice emerging patterns and follow where they lead",
            SkillTypes.Observation,
            3,
            1, // Focus cost
            UniversalActionType.GenerateFlow,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Flow, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Intellectual", "Flow", "Observation" }
        ));

        // FOCUS GENERATION OPTIONS

        // Physical Focus Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "work_precisely",
            "Work with Precision",
            "Apply careful, precise technique to the physical task",
            SkillTypes.Precision,
            3,
            1, // Focus cost
            UniversalActionType.GenerateFocus,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Focus, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.FutureCostIncrease,
            new List<string> { "Physical", "Focus", "Precision" }
        ));

        // Social Focus Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "targeted_persuasion",
            "Targeted Persuasion",
            "Focus your words precisely to convince the other person",
            SkillTypes.Persuasion,
            3,
            1, // Focus cost
            UniversalActionType.GenerateFocus,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Focus, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.FutureCostIncrease,
            new List<string> { "Social", "Focus", "Persuasion" }
        ));

        // Intellectual Focus Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "apply_expertise",
            "Apply Expertise",
            "Draw upon your concentrated knowledge and expertise",
            SkillTypes.Knowledge,
            3,
            1, // Focus cost
            UniversalActionType.GenerateFocus,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Focus, 2 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.FutureCostIncrease,
            new List<string> { "Intellectual", "Focus", "Knowledge" }
        ));

        // FORTITUDE GENERATION OPTIONS (Higher cost, higher reward)

        // Physical Fortitude Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "sustained_effort",
            "Sustained Physical Effort",
            "Commit to persistent, methodical physical work",
            SkillTypes.Endurance,
            4,
            2, // Higher Focus cost
            UniversalActionType.GenerateFortitude,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Fortitude, 3 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Physical", "Fortitude", "Endurance" }
        ));

        // Social Fortitude Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "maintain_deception",
            "Maintain Consistent Facade",
            "Keep up a consistent social facade over time",
            SkillTypes.Deception,
            4,
            2, // Higher Focus cost
            UniversalActionType.GenerateFortitude,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Fortitude, 3 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Social", "Fortitude", "Deception" }
        ));

        // Intellectual Fortitude Generation
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "methodical_planning",
            "Methodical Planning",
            "Engage in careful, long-term strategic thinking",
            SkillTypes.Planning,
            4,
            2, // Higher Focus cost
            UniversalActionType.GenerateFortitude,
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Fortitude, 3 } },
            new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Intellectual", "Fortitude", "Planning" }
        ));
    }

    private void InitializeTokenConversionOptions()
    {
        // BASIC CONVERSION OPTIONS (Force + Flow → Progress)

        // Physical Basic Conversion
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "combined_physical_effort",
            "Combined Physical Effort",
            "Combine strength and agility for efficient physical work",
            SkillTypes.Strength,
            4,
            1, // Focus cost
            UniversalActionType.BasicConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Force, 1 }, { AspectTokenTypes.Flow, 1 } },
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Physical", "Conversion", "Combined" }
        ));

        // Social Basic Conversion
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "balanced_social_approach",
            "Balanced Social Approach",
            "Balance assertiveness with adaptability in your social approach",
            SkillTypes.Persuasion,
            4,
            1, // Focus cost
            UniversalActionType.BasicConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Force, 1 }, { AspectTokenTypes.Flow, 1 } },
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Social", "Conversion", "Balanced" }
        ));

        // Intellectual Basic Conversion  
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "analytical_observation",
            "Analytical Observation",
            "Combine direct analysis with careful observation",
            SkillTypes.Analysis,
            4,
            1, // Focus cost
            UniversalActionType.BasicConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Force, 1 }, { AspectTokenTypes.Flow, 1 } },
            NegativeConsequenceTypes.TokenDisruption,
            new List<string> { "Intellectual", "Conversion", "Analysis" }
        ));

        // SPECIALIZED CONVERSION OPTIONS (2 of same token → Progress + bonus)

        // Physical Specialized Conversions
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "pure_strength_application",
            "Pure Strength Application",
            "Apply overwhelming physical force to the problem",
            SkillTypes.Strength,
            4,
            1, // Focus cost
            UniversalActionType.SpecializedConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Force, 2 } },
            NegativeConsequenceTypes.FutureCostIncrease,
            new List<string> { "Physical", "Specialized", "Force" }
        ));

        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "sustained_endurance",
            "Sustained Endurance",
            "Apply persistent, methodical physical effort over time",
            SkillTypes.Endurance,
            4,
            1, // Focus cost
            UniversalActionType.SpecializedConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Fortitude, 2 } },
            NegativeConsequenceTypes.FutureCostIncrease,
            new List<string> { "Physical", "Specialized", "Endurance" }
        ));

        // Social Specialized Conversions
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "sustained_social_pressure",
            "Sustained Social Pressure",
            "Maintain consistent social pressure over time",
            SkillTypes.Deception,
            4,
            1, // Focus cost
            UniversalActionType.SpecializedConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Fortitude, 2 } },
            NegativeConsequenceTypes.FutureCostIncrease,
            new List<string> { "Social", "Specialized", "Sustained" }
        ));

        // Intellectual Specialized Conversions
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "deep_expertise_application",
            "Deep Expertise Application",
            "Apply concentrated expertise to solve the problem",
            SkillTypes.Knowledge,
            4,
            1, // Focus cost
            UniversalActionType.SpecializedConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> { { AspectTokenTypes.Focus, 2 } },
            NegativeConsequenceTypes.FutureCostIncrease,
            new List<string> { "Intellectual", "Specialized", "Expertise" }
        ));

        // PREMIUM CONVERSION OPTIONS (Force + Flow + Focus → Maximum Progress)

        // Physical Premium Conversion
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "masterful_physical_execution",
            "Masterful Physical Execution",
            "Execute a masterful combination of strength, agility, and precision",
            SkillTypes.Strength,
            5,
            2, // Higher Focus cost
            UniversalActionType.PremiumConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> {
                { AspectTokenTypes.Force, 1 },
                { AspectTokenTypes.Flow, 1 },
                { AspectTokenTypes.Focus, 1 }
            },
            NegativeConsequenceTypes.FocusLoss,
            new List<string> { "Physical", "Premium", "Masterful" }
        ));

        // Social Premium Conversion
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "masterful_social_execution",
            "Masterful Social Execution",
            "Execute a perfect combination of force, adaptability, and precision in social interaction",
            SkillTypes.Persuasion,
            5,
            2, // Higher Focus cost
            UniversalActionType.PremiumConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> {
                { AspectTokenTypes.Force, 1 },
                { AspectTokenTypes.Flow, 1 },
                { AspectTokenTypes.Focus, 1 }
            },
            NegativeConsequenceTypes.FocusLoss,
            new List<string> { "Social", "Premium", "Masterful" }
        ));

        // Intellectual Premium Conversion
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildUniversalChoice(
            "masterful_intellectual_synthesis",
            "Masterful Intellectual Synthesis",
            "Synthesize analysis, observation, and knowledge into a perfect solution",
            SkillTypes.Analysis,
            5,
            2, // Higher Focus cost
            UniversalActionType.PremiumConversion,
            new Dictionary<AspectTokenTypes, int>(), // No token generation
            new Dictionary<AspectTokenTypes, int> {
                { AspectTokenTypes.Force, 1 },
                { AspectTokenTypes.Flow, 1 },
                { AspectTokenTypes.Focus, 1 }
            },
            NegativeConsequenceTypes.FocusLoss,
            new List<string> { "Intellectual", "Premium", "Synthesis" }
        ));
    }

    public List<EncounterOption> GetAll()
    {
        return narrativeChoices.ToList();
    }

    public List<EncounterOption> GetForEncounter(EncounterState state)
    {
        CardTypes encounterType = state.EncounterType;
        int currentStage = state.CurrentStageIndex + 1; // Convert to 1-based
        int totalStages = state.EncounterInfo.Stages.Count;

        List<EncounterOption> stageChoices = new List<EncounterOption>();

        // Always include safety option
        stageChoices.AddRange(GetSafetyOptionsForEncounterType(encounterType));

        // Add appropriate choices based on stage
        if (currentStage <= 2) // Early stages - token generation
        {
            stageChoices.AddRange(GetTokenGenerationOptionsForEncounterType(encounterType));
        }

        if (currentStage >= 2) // Later stages - token conversion  
        {
            stageChoices.AddRange(GetTokenConversionOptionsForEncounterType(encounterType, state));
        }

        if (currentStage >= 3) // Final stage - premium options
        {
            stageChoices.AddRange(GetPremiumOptionsForEncounterType(encounterType, state));
        }

        return stageChoices;
    }

    private List<EncounterOption> GetSafetyOptionsForEncounterType(CardTypes encounterType)
    {
        string encounterTag = encounterType.ToString();
        return narrativeChoices
            .Where(choice => choice.Tags.Contains(encounterTag) && choice.Tags.Contains("Safety"))
            .ToList();
    }

    private List<EncounterOption> GetTokenGenerationOptionsForEncounterType(CardTypes encounterType)
    {
        string encounterTag = encounterType.ToString();
        return narrativeChoices
            .Where(choice => choice.Tags.Contains(encounterTag) &&
                           (choice.ActionType == UniversalActionType.GenerateForce ||
                            choice.ActionType == UniversalActionType.GenerateFlow ||
                            choice.ActionType == UniversalActionType.GenerateFocus ||
                            choice.ActionType == UniversalActionType.GenerateFortitude))
            .ToList();
    }

    private List<EncounterOption> GetTokenConversionOptionsForEncounterType(CardTypes encounterType, EncounterState state)
    {
        string encounterTag = encounterType.ToString();
        List<EncounterOption> conversionOptions = narrativeChoices
            .Where(choice => choice.Tags.Contains(encounterTag) &&
                           (choice.ActionType == UniversalActionType.BasicConversion ||
                            choice.ActionType == UniversalActionType.SpecializedConversion))
            .ToList();

        // Filter by available tokens
        return conversionOptions.Where(choice => HasRequiredTokens(choice, state)).ToList();
    }

    private List<EncounterOption> GetPremiumOptionsForEncounterType(CardTypes encounterType, EncounterState state)
    {
        string encounterTag = encounterType.ToString();
        List<EncounterOption> premiumOptions = narrativeChoices
            .Where(choice => choice.Tags.Contains(encounterTag) &&
                           choice.ActionType == UniversalActionType.PremiumConversion)
            .ToList();

        // Filter by available tokens
        return premiumOptions.Where(choice => HasRequiredTokens(choice, state)).ToList();
    }

    private bool HasRequiredTokens(EncounterOption choice, EncounterState state)
    {
        foreach (KeyValuePair<AspectTokenTypes, int> requirement in choice.TokenCosts)
        {
            if (!state.HasAspectTokens(requirement.Key, requirement.Value))
            {
                return false;
            }
        }
        return true;
    }
}