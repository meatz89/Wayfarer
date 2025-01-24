public class GameRules
{
    public static GameRules StandardRuleset = new GameRules
    {
        StartingHealth = 10,
        StartingCoins = 10,
        StartingInventorySize = 10,

        StartingPhysicalEnergy = 2,
        StartingFocusEnergy = 3,
        StartingSocialEnergy = 7,

        MinimumHealth = 0,
        DailyFoodRequirement = 2,
        NoFoodEffectOnHealth = -2,
        NoShelterEffectOnHealth = -2,
    };

    public int StartingHealth;
    public int StartingCoins;
    public int StartingInventorySize;

    public int StartingPhysicalEnergy;
    public int StartingFocusEnergy;
    public int StartingSocialEnergy;

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
        ChoiceArchetypes archetype,
        EncounterValues values)
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

        // Add Aggressive and Desperate to the end of primary archetype's list only
        if (archetype == values.LastChoiceType)
        {
            baseOrder.Add(ChoiceApproaches.Aggressive);
            if (values.Pressure >= 6)
            {
                baseOrder.Add(ChoiceApproaches.Desperate);
            }
        }

        return baseOrder;
    }

    public static List<ChoiceApproaches> GetAvailableApproaches(
        ChoiceArchetypes archetype,
        EncounterValues values,
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
        return approaches.Where(approach => IsApproachAvailable(archetype, approach, values, playerState))
                        .ToList();
    }

    private static bool IsApproachAvailable(
        ChoiceArchetypes archetype,
        ChoiceApproaches approach,
        EncounterValues values,
        PlayerState playerState)
    {
        // Then check standard value requirements
        return approach switch
        {
            ChoiceApproaches.Tactical => GetArchetypeValue(archetype, values) >= 6,
            ChoiceApproaches.Strategic => values.Outcome >= 6,
            ChoiceApproaches.Diplomatic => values.Resonance >= 4 && values.Insight >= 4,
            ChoiceApproaches.Methodical => values.Insight >= 4 && values.Momentum >= 4,
            ChoiceApproaches.Forceful => values.Momentum >= 4 && values.Resonance >= 4,
            ChoiceApproaches.Desperate => values.Pressure > 6,
            ChoiceApproaches.Aggressive => values.Pressure <= 6,
            _ => true // Careful always available
        };
    }

    public static int GetArchetypeValue(ChoiceArchetypes archetype, EncounterValues values)
    {
        return archetype switch
        {
            ChoiceArchetypes.Physical => values.Momentum,
            ChoiceArchetypes.Focus => values.Insight,
            ChoiceArchetypes.Social => values.Resonance,
            _ => throw new ArgumentException("Invalid archetype")
        };
    }

    // Conversion methods (copied from EncounterChoice for consistency)
    public static ChangeTypes ConvertValueTypeToChangeType(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Outcome => ChangeTypes.Outcome,
            ValueTypes.Momentum => ChangeTypes.Momentum,
            ValueTypes.Insight => ChangeTypes.Insight,
            ValueTypes.Resonance => ChangeTypes.Resonance,
            ValueTypes.Pressure => ChangeTypes.Pressure,
            _ => throw new ArgumentException("Invalid ValueType")
        };
    }

    public static ChangeTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ChangeTypes.PhysicalEnergy,
            EnergyTypes.Focus => ChangeTypes.FocusEnergy,
            EnergyTypes.Social => ChangeTypes.SocialEnergy,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }

    public static List<Outcome> CreateCostsForAction(ActionTemplate template)
    {
        List<Outcome> costs = new List<Outcome>();
        switch (template.ActionType)
        {
            case BasicActionTypes.Labor:
                costs.Add(new HealthOutcome(-1));
                break;
            case BasicActionTypes.Persuade:
                costs.Add(new ReputationOutcome(-1));
                break;
            case BasicActionTypes.Investigate:
                costs.Add(new ConcentrationOutcome(-1));
                break;
            case BasicActionTypes.Mingle:
                costs.Add(new ReputationOutcome(-1));
                break;
            case BasicActionTypes.Recover:
                costs.Add(new CoinsOutcome(-5));
                costs.Add(new ResourceOutcome(ResourceTypes.Food, -1));
                break;
        }
        return costs;
    }


    public static List<Outcome> CreateRewardsForTemplate(ActionTemplate template)
    {
        List<Outcome> rewards = new List<Outcome>();
        switch (template.ActionType)
        {
            case BasicActionTypes.Labor:
                rewards.Add(new CoinsOutcome(5));
                rewards.Add(new ResourceOutcome(ResourceTypes.Wood, 1));
                break;
            case BasicActionTypes.Persuade:
                rewards.Add(new CoinsOutcome(3));
                rewards.Add(new ReputationOutcome(2));
                break;
            case BasicActionTypes.Investigate:
                rewards.Add(new KnowledgeOutcome(KnowledgeTypes.LocalHistory, 1));
                rewards.Add(new ReputationOutcome(1));
                break;
            case BasicActionTypes.Mingle:
                rewards.Add(new ReputationOutcome(3));
                break;
            case BasicActionTypes.Recover:
                rewards.Add(new EnergyOutcome(EnergyTypes.Physical, 5));
                rewards.Add(new EnergyOutcome(EnergyTypes.Focus, 5));
                rewards.Add(new EnergyOutcome(EnergyTypes.Social, 5));
                rewards.Add(new HealthOutcome(1));
                break;
        }
        return rewards;
    }

    public static EnergyTypes GetEnergyTypeForAction(BasicActionTypes actionType)
    {
        EnergyTypes energyType = EnergyTypes.Social;
        energyType = actionType switch
        {
            BasicActionTypes.Labor => EnergyTypes.Physical,
            BasicActionTypes.Gather => EnergyTypes.Physical,
            BasicActionTypes.Travel => EnergyTypes.Physical,
            BasicActionTypes.Rest => EnergyTypes.Physical,
            BasicActionTypes.Recover => EnergyTypes.Physical,

            BasicActionTypes.Investigate => EnergyTypes.Focus,
            BasicActionTypes.Study => EnergyTypes.Focus,
            BasicActionTypes.Reflect => EnergyTypes.Focus,

            BasicActionTypes.Mingle => EnergyTypes.Social,
            BasicActionTypes.Discuss => EnergyTypes.Social,
            BasicActionTypes.Persuade => EnergyTypes.Social,
            BasicActionTypes.Perform => EnergyTypes.Social,
        };
        return energyType;
    }

    public static int GetBaseEnergyCost(BasicActionTypes actionType)
    {

        int energycost = 2;
        energycost = actionType switch
        {
            BasicActionTypes.Labor => 3,
            BasicActionTypes.Gather => 2,
            BasicActionTypes.Travel => 1,
            BasicActionTypes.Investigate => 2,
            BasicActionTypes.Study => 3,
            BasicActionTypes.Reflect => 1,
            BasicActionTypes.Rest => 0,
            BasicActionTypes.Recover => 1,
            BasicActionTypes.Mingle => 1,
            BasicActionTypes.Discuss => 2,
            BasicActionTypes.Persuade => 3,
            BasicActionTypes.Perform => 3,
        };

        return energycost;
    }

    public static CompositionPattern SetDefaultCompositionForActionType(BasicActionTypes actionType)
    {
        CompositionPattern compositionPattern = new CompositionPattern
        {
            PrimaryArchetype = ChoiceArchetypes.Social,
            SecondaryArchetype = ChoiceArchetypes.Focus,
        };

        switch (actionType)
        {
            case BasicActionTypes.Labor:
            case BasicActionTypes.Gather:
            case BasicActionTypes.Travel:
                // Physical-focused composition
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Physical,
                    SecondaryArchetype = ChoiceArchetypes.Focus,
                };
                break;

            case BasicActionTypes.Investigate:
            case BasicActionTypes.Study:
            case BasicActionTypes.Reflect:
            case BasicActionTypes.Rest:
            case BasicActionTypes.Recover:
                // Focus-focused composition
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Focus,
                    SecondaryArchetype = ChoiceArchetypes.Social,
                };
                break;

            case BasicActionTypes.Mingle:
            case BasicActionTypes.Discuss:
            case BasicActionTypes.Persuade:
            case BasicActionTypes.Perform:
                // Social-focused composition
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Social,
                    SecondaryArchetype = ChoiceArchetypes.Focus,
                };
                break;
            default:
                // default composition pattern
                compositionPattern = new CompositionPattern
                {
                    PrimaryArchetype = ChoiceArchetypes.Social,
                    SecondaryArchetype = ChoiceArchetypes.Physical,
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

    public static List<ValueModification> GetChoiceBaseValueEffects(EncounterChoice choice)
    {
        List<ValueModification> modifications = new List<ValueModification>();

        // Each archetype builds its specific mastery value
        ChoiceApproaches approach = choice.Approach;

        switch (choice.Archetype)
        {
            case ChoiceArchetypes.Physical:
                // Physical actions build Momentum
                GetPhysicalChoiceModifications(choice, modifications, approach);

                break;

            case ChoiceArchetypes.Focus:
                // Focus actions build Insight
                GetFocusChoiceModifications(choice, modifications, approach);

                break;

            case ChoiceArchetypes.Social:
                // Social actions build Resonance
                GetSocialChoiceModifications(choice, modifications, approach);

                break;
        }

        modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 1,
                $"Base"));
        return modifications;
    }

    private static void GetPhysicalChoiceModifications(EncounterChoice choice, List<ValueModification> modifications, ChoiceApproaches approach)
    {
        if (approach == ChoiceApproaches.Forceful)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Momentum, -1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Resonance, -1,
                $"{choice.Archetype} {choice.Approach}"));
        }
        if (approach == ChoiceApproaches.Tactical)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 3,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, -45,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Momentum, -3,
                $"{choice.Archetype} {choice.Approach}"));
        }
        if (approach == ChoiceApproaches.Strategic)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Momentum, -2,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Resonance, 4,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Careful)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, -1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Momentum, 4,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Aggressive)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 5,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 2,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Desperate)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Momentum, -4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 2,
                $"{choice.Archetype} {choice.Approach}"));
        }
    }

    private static void GetFocusChoiceModifications(EncounterChoice choice, List<ValueModification> modifications, ChoiceApproaches approach)
    {
        if (approach == ChoiceApproaches.Methodical)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Insight, -1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Momentum, -1,
                $"{choice.Archetype} {choice.Approach}"));
        }
        if (approach == ChoiceApproaches.Tactical)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 3,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, -5,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Insight, -3,
                $"{choice.Archetype} {choice.Approach}"));
        }
        if (approach == ChoiceApproaches.Strategic)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Insight, -2,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Momentum, 4,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Careful)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, -1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Insight, 4,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Aggressive)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 5,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 2,
                $"{choice.Archetype} {choice.Approach}"));

        }
        else if (approach != ChoiceApproaches.Desperate)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Insight, -4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 2,
                $"{choice.Archetype} {choice.Approach}"));
        }
    }

    private static void GetSocialChoiceModifications(EncounterChoice choice, List<ValueModification> modifications, ChoiceApproaches approach)
    {
        if (approach == ChoiceApproaches.Diplomatic)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Resonance, -1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Insight, -1,
                $"{choice.Archetype} {choice.Approach}"));
        }
        if (approach == ChoiceApproaches.Tactical)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 3,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, -4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Resonance, -3,
                $"{choice.Archetype} {choice.Approach}"));
        }
        if (approach == ChoiceApproaches.Strategic)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Resonance, -2,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Insight, 4,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Careful)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, -1,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Resonance, 4,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Aggressive)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 5,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 2,
                $"{choice.Archetype} {choice.Approach}"));
        }
        else if (approach == ChoiceApproaches.Desperate)
        {
            modifications.Add(new EncounterValueModification(ValueTypes.Outcome, 4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Resonance, -4,
                $"{choice.Archetype} {choice.Approach}"));

            modifications.Add(new EncounterValueModification(ValueTypes.Pressure, 2,
                $"{choice.Archetype} {choice.Approach}"));
        }
    }
}
