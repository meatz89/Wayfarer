public class EncounterChoiceGenerator
{
    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        List<EncounterChoice> choices = new();

        // Generate Basic Choices (Always available)
        choices.AddRange(GenerateBasicChoices(context));

        // Generate Strategic Choices (if applicable)
        choices.AddRange(GenerateStrategicChoices(context));

        // Generate Special Choices (if requirements are met)
        choices.AddRange(GenerateSpecialChoices(context));

        return choices;
    }

    private List<EncounterChoice> GenerateBasicChoices(EncounterActionContext context)
    {
        List<EncounterChoice> basicChoices = new();

        // Example: Basic choice based on context's ActionType
        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                basicChoices.Add(StandardPatterns.DirectProgressImmediate.CreateChoice(context, basicChoices.Count + 1));
                break;
            case BasicActionTypes.Trade:
                basicChoices.Add(StandardPatterns.DirectProgressImmediate.CreateChoice(context, basicChoices.Count + 1));
                break;
            case BasicActionTypes.Mingle:
                basicChoices.Add(StandardPatterns.DirectProgressImmediate.CreateChoice(context, basicChoices.Count + 1));
                break;
            case BasicActionTypes.Investigate:
                basicChoices.Add(StandardPatterns.DirectProgressImmediate.CreateChoice(context, basicChoices.Count + 1));
                break;
                // Add cases for other BasicActionTypes
        }

        return basicChoices;
    }

    private List<EncounterChoice> GenerateStrategicChoices(EncounterActionContext context)
    {
        List<EncounterChoice> strategicChoices = new();

        // Example: Strategic choice to increase Understanding
        if (context.CurrentValues.Understanding < 7) // Limit so this isn't always available
        {
            strategicChoices.Add(StandardPatterns.CarefulPositionInvested.CreateChoice(context, strategicChoices.Count + 1));
        }

        // Example: Strategic choice to increase Connection
        if (context.CurrentValues.Connection < 7) // Limit so this isn't always available
        {
            strategicChoices.Add(StandardPatterns.TacticalOpportunityStrategic.CreateChoice(context, strategicChoices.Count + 1));
        }

        // Example: Reduce Tension (only if Understanding is high enough and Tension is high)
        if (context.CurrentValues.Understanding >= 6 && context.CurrentValues.Tension >= 6)
        {
            // Create a new ChoicePattern for reducing tension
            ChoicePattern reduceTensionPattern = new()
            {
                Position = PositionTypes.Careful,
                Intent = IntentTypes.Position,
                Scope = ScopeTypes.Immediate,
                BaseEnergyCost = 2,
                StandardValueChanges = new()
                {
                    new(ValueTypes.Tension, -2),
                    new(ValueTypes.Understanding, 1)
                },
                StandardRequirements = new()
                {
                    new EnergyRequirement(EnergyTypes.Focus, 2) // Requires Focus Energy
                }
            };

            strategicChoices.Add(reduceTensionPattern.CreateChoice(context, strategicChoices.Count + 1));
        }


        return strategicChoices;
    }

    private List<EncounterChoice> GenerateSpecialChoices(EncounterActionContext context)
    {
        List<EncounterChoice> specialChoices = new();

        // Power Moves (High Tension)
        if (context.CurrentValues.Tension >= 7)
        {
            // Example: Power Move (Combat)
            // Requires: Tension ≥ 7, relevant Physical Skill, context-appropriate item (WEAPON)
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Strength) && context.PlayerState.Skills[SkillTypes.Strength] >= 5 /* Example threshold */ 
                && context.PlayerState.Inventory.HasItemOfType(ItemTypes.Weapon))
            {
                ChoicePattern powerMovePattern = new()
                {
                    Position = PositionTypes.Direct,
                    Intent = IntentTypes.Progress,
                    Scope = ScopeTypes.Immediate,
                    BaseEnergyCost = 3, // High cost for a powerful move
                    StandardValueChanges = new()
                    {
                        new(ValueTypes.Advantage, 3), // Large Advantage gain
                        new(ValueTypes.Tension, -2) // Reduce Tension
                    },
                    StandardRequirements = new()
                    {
                        new EnergyRequirement(EnergyTypes.Physical, 3)
                    },
                    StandardOutcomes = new()
                    {
                        new ItemOutcome(ItemTypes.Weapon, -1) // Example: Weapon might be damaged
                    }
                };

                specialChoices.Add(powerMovePattern.CreateChoice(context, specialChoices.Count + 1));
            }

            // Add more Power Move patterns for other contexts (Negotiation, Investigation, Labor, Social)
        }

        // Expert Moves (High Understanding)
        if (context.CurrentValues.Understanding >= 7)
        {
            // Example: Expert Move (Investigation)
            // Requires: Understanding ≥ 7, relevant Focus Skill, specific Knowledge flag (CLUE)
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Perception) && context.PlayerState.Skills[SkillTypes.Perception] >= 5 /* Example threshold */ &&
                context.PlayerState.HasKnowledge(KnowledgeTypes.Clue))
            {
                ChoicePattern expertMovePattern = new()
                {
                    Position = PositionTypes.Tactical,
                    Intent = IntentTypes.Opportunity,
                    Scope = ScopeTypes.Strategic,
                    BaseEnergyCost = 2,
                    StandardValueChanges = new()
                    {
                        new(ValueTypes.Advantage, 2),
                        new(ValueTypes.Understanding, 2)
                    },
                    StandardRequirements = new()
                    {
                        new EnergyRequirement(EnergyTypes.Focus, 2)
                    },
                    StandardOutcomes = new()
                    {
                         new KnowledgeOutcome(KnowledgeTypes.Weakness, 1) // Example: Gain new knowledge
                    }
                };

                specialChoices.Add(expertMovePattern.CreateChoice(context, specialChoices.Count + 1));
            }

            // Add more Expert Move patterns for other contexts (Combat, Negotiation, Labor, Social)
        }

        // Social Moves (High Connection)
        if (context.CurrentValues.Connection >= 7)
        {
            // Example: Social Move (Negotiation)
            // Requires: Connection ≥ 7, relevant Social Skill, specific Reputation type (HONEST)
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Charisma) && context.PlayerState.Skills[SkillTypes.Charisma] >= 5 /* Example threshold */ && context.PlayerState.HasReputation(ReputationTypes.Honest) &&
                context.PlayerState.GetReputationValue(ReputationTypes.Honest) >= 5)
            {
                ChoicePattern socialMovePattern = new()
                {
                    Position = PositionTypes.Careful,
                    Intent = IntentTypes.Position,
                    Scope = ScopeTypes.Invested,
                    BaseEnergyCost = 2,
                    StandardValueChanges = new()
                    {
                        new(ValueTypes.Advantage, 2),
                        new(ValueTypes.Connection, 1)
                    },
                    StandardRequirements = new()
                    {
                        new EnergyRequirement(EnergyTypes.Social, 2)
                    },
                    StandardOutcomes = new()
                    {
                        new ReputationOutcome(ReputationTypes.Honest, 1) // Example: Reinforce Honest reputation
                    }
                };

                specialChoices.Add(socialMovePattern.CreateChoice(context, specialChoices.Count + 1));
            }

            // Add more Social Move patterns for other contexts (Combat, Investigation, Labor, Social)
        }

        return specialChoices;
    }

}