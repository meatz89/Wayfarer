public class GameRules
{
    public static GameRules StandardRuleset = new GameRules
    {
        StartingHealth = 20,
        StartingCoins = 5,
        StartingInventorySize = 10,

        StartingPhysicalEnergy = 3,
        StartingFocus = 3,
        StartingConfidence = 7,

        MinimumHealth = 0,
    };

    public int StartingHealth;
    public int StartingCoins;
    public int StartingInventorySize;

    public int StartingPhysicalEnergy;
    public int StartingFocus;
    public int StartingConfidence;

    public int MinimumHealth;
    public int DailyFoodRequirement;
    public int NoFoodEffectOnHealth;
    public int NoShelterEffectOnHealth;

    public static int StrategicMomentumRequirement = 4;
    public static int StrategicInsightRequirement = 4;
    public static int StrategicResonanceRequirement = 4;

    public static List<ChoiceApproaches> PrimaryArchetypeOnlyApproaches = new()
    {
        ChoiceApproaches.Aggressive,
        ChoiceApproaches.Desperate,
    };

    public static List<ChoiceApproaches> GetPriorityOrder(
        ChoiceArchetypes archetype)
    {
        // Priority orders should only include approaches that are valid for the archetype
        List<ChoiceApproaches> baseOrder = archetype switch
        {
            ChoiceArchetypes.Physical => new List<ChoiceApproaches>
            {
                ChoiceApproaches.Forceful,
                ChoiceApproaches.Tactical,
                ChoiceApproaches.Strategic,
                ChoiceApproaches.Careful
            },

            ChoiceArchetypes.Focus => new List<ChoiceApproaches>
            {
                ChoiceApproaches.Methodical,
                ChoiceApproaches.Tactical,
                ChoiceApproaches.Strategic,
                ChoiceApproaches.Careful
            },

            ChoiceArchetypes.Social => new List<ChoiceApproaches>
            {
                ChoiceApproaches.Diplomatic,
                ChoiceApproaches.Tactical,
                ChoiceApproaches.Strategic,
                ChoiceApproaches.Careful
            },

            _ => throw new ArgumentException("Invalid archetype")
        };

        return baseOrder;
    }

    public static List<ChoiceApproaches> GetAvailableApproaches(
        ChoiceArchetypes archetype,
        PlayerState playerState)
    {
        // Start with base approaches
        List<ChoiceApproaches> approaches = new List<ChoiceApproaches>
        {
            ChoiceApproaches.Diplomatic,    // Available with requirements
            ChoiceApproaches.Methodical,    // Available with requirements
            ChoiceApproaches.Forceful,    // Available with requirements
            ChoiceApproaches.Careful,    // Always available
            ChoiceApproaches.Strategic,  // Available with requirements
            ChoiceApproaches.Tactical,    // Available with requirements
            ChoiceApproaches.Aggressive,    // Available with requirements
            ChoiceApproaches.Desperate,    // Available with requirements
        };

        // Filter based on value requirements
        return approaches.Where(approach => IsApproachAvailable(archetype, approach, playerState))
                        .ToList();
    }

    private static bool IsApproachAvailable(
        ChoiceArchetypes archetype,
        ChoiceApproaches approach,
        PlayerState playerState)
    {
        return true;
    }

    public static ValueTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ValueTypes.Health,
            EnergyTypes.Focus => ValueTypes.Focus,
            EnergyTypes.None => ValueTypes.None,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }

    public static List<Outcome> CreateCostsForAction(ActionTemplate template)
    {
        List<Outcome> costs = new List<Outcome>();
        switch (template.ActionType)
        {
            case BasicActionTypes.Travel:
                break;

            case BasicActionTypes.Rest:
                costs.Add(new ResourceOutcome(ItemTypes.Food, -1));
                costs.Add(new CoinsOutcome(-5));
                break;

            case BasicActionTypes.Labor:
                costs.Add(new HealthOutcome(-1));
                break;

            case BasicActionTypes.Fight:
                break;

            case BasicActionTypes.Gather:
                break;

            case BasicActionTypes.Study:
                break;

            case BasicActionTypes.Investigate:
                break;

            case BasicActionTypes.Analyze:
                break;

            case BasicActionTypes.Discuss:
                break;

            case BasicActionTypes.Persuade:
                costs.Add(new ConfidenceOutcome(-1));
                break;

            case BasicActionTypes.Perform:
                break;
        }
        return costs;
    }

    public static List<Outcome> CreateRewardsForTemplate(ActionTemplate template)
    {
        List<Outcome> rewards = new List<Outcome>();
        switch (template.ActionType)
        {
            case BasicActionTypes.Travel:
                break;

            case BasicActionTypes.Rest:
                rewards.Add(new EnergyOutcome(EnergyTypes.Physical, 5));
                rewards.Add(new EnergyOutcome(EnergyTypes.Focus, 5));
                rewards.Add(new HealthOutcome(1));
                break;

            case BasicActionTypes.Labor:
                rewards.Add(new CoinsOutcome(5));
                break;

            case BasicActionTypes.Fight:
                break;

            case BasicActionTypes.Gather:
                break;

            case BasicActionTypes.Study:
                break;

            case BasicActionTypes.Investigate:
                rewards.Add(new KnowledgeOutcome(KnowledgeTags.MarketRoutines, KnowledgeCategories.Commerce));
                break;

            case BasicActionTypes.Analyze:
                break;

            case BasicActionTypes.Discuss:
                rewards.Add(new ConfidenceOutcome(3));
                break;

            case BasicActionTypes.Persuade:
                break;

            case BasicActionTypes.Perform:
                break;
        }
        return rewards;
    }

    public static EnergyTypes GetEnergyTypeForAction(BasicActionTypes actionType)
    {
        EnergyTypes energyType = EnergyTypes.None;
        energyType = actionType switch
        {
            BasicActionTypes.Travel => EnergyTypes.Physical,
            BasicActionTypes.Rest => EnergyTypes.Physical,

            BasicActionTypes.Labor => EnergyTypes.Physical,
            BasicActionTypes.Fight => EnergyTypes.Physical,
            BasicActionTypes.Gather => EnergyTypes.Physical,

            BasicActionTypes.Study => EnergyTypes.Focus,
            BasicActionTypes.Investigate => EnergyTypes.Focus,
            BasicActionTypes.Analyze => EnergyTypes.Focus,

            BasicActionTypes.Discuss => EnergyTypes.None,
            BasicActionTypes.Persuade => EnergyTypes.Focus,
            BasicActionTypes.Perform => EnergyTypes.Physical,
        };
        return energyType;
    }

    public static int GetBaseEnergyCost(BasicActionTypes actionType)
    {
        int energycost = 2;
        energycost = actionType switch
        {
            BasicActionTypes.Travel => 1,
            BasicActionTypes.Rest => 0,

            BasicActionTypes.Labor => 3,
            BasicActionTypes.Fight => 3,
            BasicActionTypes.Gather => 2,

            BasicActionTypes.Study => 3,
            BasicActionTypes.Investigate => 2,
            BasicActionTypes.Analyze => 2,

            BasicActionTypes.Discuss => 1,
            BasicActionTypes.Persuade => 3,
            BasicActionTypes.Perform => 3,
        };

        return energycost;
    }

    public static CompositionPattern GetCompositionPatternForActionType(BasicActionTypes actionType)
    {
        CompositionPattern compositionPattern = new CompositionPattern
        {
            PrimaryArchetype = ChoiceArchetypes.Focus,
            SecondaryArchetype = ChoiceArchetypes.Focus,
        };

        switch (actionType)
        {
            case BasicActionTypes.Labor:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Physical,
                    SecondaryArchetype = ChoiceArchetypes.Physical,
                };
                break;

            case BasicActionTypes.Travel:
            case BasicActionTypes.Rest:
            case BasicActionTypes.Gather:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Physical,
                    SecondaryArchetype = ChoiceArchetypes.Focus,
                };
                break;

            case BasicActionTypes.Fight:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Physical,
                    SecondaryArchetype = ChoiceArchetypes.Social,
                };
                break;

            case BasicActionTypes.Study:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Focus,
                    SecondaryArchetype = ChoiceArchetypes.Focus,
                };
                break;

            case BasicActionTypes.Investigate:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Focus,
                    SecondaryArchetype = ChoiceArchetypes.Physical,
                };
                break;
            case BasicActionTypes.Analyze:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Focus,
                    SecondaryArchetype = ChoiceArchetypes.Social,
                };
                break;

            case BasicActionTypes.Discuss:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Social,
                    SecondaryArchetype = ChoiceArchetypes.Social,
                };
                break;

            case BasicActionTypes.Persuade:
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Social,
                    SecondaryArchetype = ChoiceArchetypes.Focus,
                };
                break;

            case BasicActionTypes.Perform:
                // Social-focused composition
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Social,
                    SecondaryArchetype = ChoiceArchetypes.Physical,
                };
                break;

            default:
                // default composition pattern
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Focus,
                    SecondaryArchetype = ChoiceArchetypes.Focus,
                };
                break;
        }

        return compositionPattern;
    }
    public static ChoiceArchetypes[] GetStrategicArchetypeOrder(CompositionPattern pattern)
    {
        // Primary archetype gets priority for strategic choices
        // Then secondary, then the remaining one
        ChoiceArchetypes remainingArchetype = GetRemainingArchetype(
            pattern.PrimaryArchetype,
            pattern.SecondaryArchetype);

        return new[]
        {
        pattern.PrimaryArchetype,
        pattern.SecondaryArchetype,
        remainingArchetype
    };
    }

    public static List<ChoiceArchetypes> GetArchetypePriority(CompositionPattern pattern)
    {
        List<ChoiceArchetypes> priority = new()
    {
        pattern.PrimaryArchetype,
        pattern.SecondaryArchetype
    };

        // Add the remaining archetype last
        ChoiceArchetypes remainingArchetype = GetRemainingArchetype(
            pattern.PrimaryArchetype,
            pattern.SecondaryArchetype);
        priority.Add(remainingArchetype);

        return priority;
    }

    private static ChoiceArchetypes GetRemainingArchetype(
        ChoiceArchetypes first,
        ChoiceArchetypes second)
    {
        return Enum.GetValues<ChoiceArchetypes>()
            .First(a => a != first && a != second);
    }

}
