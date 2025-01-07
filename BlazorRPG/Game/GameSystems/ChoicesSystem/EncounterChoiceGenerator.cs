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
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Strength) && context.PlayerState.Skills[SkillTypes.Strength] >= 5 &&
                context.PlayerState.Inventory.HasItemOfType(ItemTypes.Weapon))
            {
                specialChoices.Add(CreatePowerMoveChoice(context, specialChoices.Count + 1));
            }

            // Negotiation Power Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Charisma) && context.PlayerState.Skills[SkillTypes.Charisma] >= 5 &&
                context.PlayerState.Inventory.HasItemOfType(ItemTypes.Valuable))
            {
                specialChoices.Add(CreateNegotiationPowerMoveChoice(context, specialChoices.Count + 1));
            }

            // Investigation Power Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Perception) && context.PlayerState.Skills[SkillTypes.Perception] >= 5 &&
                context.PlayerState.Inventory.HasItemOfType(ItemTypes.Tool)) // Assuming 'Tool' is used for investigation
            {
                specialChoices.Add(CreateInvestigationPowerMoveChoice(context, specialChoices.Count + 1));
            }

            // Labor Power Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Strength) && context.PlayerState.Skills[SkillTypes.Strength] >= 5 &&
                context.PlayerState.Inventory.HasItemOfType(ItemTypes.Equipment)) // Assuming 'Equipment' is used for labor
            {
                specialChoices.Add(CreateLaborPowerMoveChoice(context, specialChoices.Count + 1));
            }

            // Social Power Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Charisma) && context.PlayerState.Skills[SkillTypes.Charisma] >= 5 &&
                context.PlayerState.Inventory.HasItemOfType(ItemTypes.Gift))
            {
                specialChoices.Add(CreateSocialPowerMoveChoice(context, specialChoices.Count + 1));
            }
        }

        // Expert Moves (High Understanding)
        if (context.CurrentValues.Understanding >= 7)
        {
            // Example: Expert Move (Investigation)
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Perception) && context.PlayerState.Skills[SkillTypes.Perception] >= 5 &&
                context.PlayerState.Knowledge.Any(k => k.KnowledgeType == KnowledgeTypes.Clue))
            {
                specialChoices.Add(CreateExpertMoveChoice(context, specialChoices.Count + 1));
            }

            // Combat Expert Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Strength) && context.PlayerState.Skills[SkillTypes.Strength] >= 5 &&
                context.PlayerState.Knowledge.Any(k => k.KnowledgeType == KnowledgeTypes.Weakness))
            {
                specialChoices.Add(CreateCombatExpertMoveChoice(context, specialChoices.Count + 1));
            }

            // Negotiation Expert Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Charisma) && context.PlayerState.Skills[SkillTypes.Charisma] >= 5 &&
                context.PlayerState.Knowledge.Any(k => k.KnowledgeType == KnowledgeTypes.Leverage))
            {
                specialChoices.Add(CreateNegotiationExpertMoveChoice(context, specialChoices.Count + 1));
            }

            // Labor Expert Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Crafting) && context.PlayerState.Skills[SkillTypes.Crafting] >= 5 &&
                context.PlayerState.Knowledge.Any(k => k.KnowledgeType == KnowledgeTypes.Technique))
            {
                specialChoices.Add(CreateLaborExpertMoveChoice(context, specialChoices.Count + 1));
            }

            // Social Expert Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Charisma) && context.PlayerState.Skills[SkillTypes.Charisma] >= 5 &&
                context.PlayerState.Knowledge.Any(k => k.KnowledgeType == KnowledgeTypes.Secret))
            {
                specialChoices.Add(CreateSocialExpertMoveChoice(context, specialChoices.Count + 1));
            }
        }

        // Social Moves (High Connection)
        if (context.CurrentValues.Connection >= 7)
        {
            // Example: Social Move (Negotiation)
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Charisma) && context.PlayerState.Skills[SkillTypes.Charisma] >= 5 &&
                context.PlayerState.Reputations.ContainsKey(ReputationTypes.Honest) && context.PlayerState.Reputations[ReputationTypes.Honest] >= 5)
            {
                specialChoices.Add(CreateSocialMoveChoice(context, specialChoices.Count + 1));
            }
            // Combat Social Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Strength) && context.PlayerState.Skills[SkillTypes.Strength] >= 5 &&
                context.PlayerState.Reputations.ContainsKey(ReputationTypes.Unbreakable) && context.PlayerState.Reputations[ReputationTypes.Unbreakable] >= 5)
            {
                specialChoices.Add(CreateCombatSocialMoveChoice(context, specialChoices.Count + 1));
            }

            // Investigation Social Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Perception) && context.PlayerState.Skills[SkillTypes.Perception] >= 5 &&
                context.PlayerState.Reputations.ContainsKey(ReputationTypes.Sharp) && context.PlayerState.Reputations[ReputationTypes.Sharp] >= 5)
            {
                specialChoices.Add(CreateInvestigationSocialMoveChoice(context, specialChoices.Count + 1));
            }

            // Labor Social Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Crafting) && context.PlayerState.Skills[SkillTypes.Crafting] >= 5 &&
                context.PlayerState.Reputations.ContainsKey(ReputationTypes.Reliable) && context.PlayerState.Reputations[ReputationTypes.Reliable] >= 5)
            {
                specialChoices.Add(CreateLaborSocialMoveChoice(context, specialChoices.Count + 1));
            }

            // General Social Move
            if (context.PlayerState.Skills.ContainsKey(SkillTypes.Charisma) && context.PlayerState.Skills[SkillTypes.Charisma] >= 5 &&
                context.PlayerState.Reputations.ContainsKey(ReputationTypes.Trusted) && context.PlayerState.Reputations[ReputationTypes.Trusted] >= 5)
            {
                specialChoices.Add(CreateGeneralSocialMoveChoice(context, specialChoices.Count + 1));
            }
        }

        return specialChoices;
    }


    // --- Power Moves ---
    private EncounterChoice CreatePowerMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern powerMovePattern = new()
        {
            Position = PositionTypes.Direct,
            Intent = IntentTypes.Progress,
            Scope = ScopeTypes.Immediate,
            BaseEnergyCost = 3,
            StandardValueChanges = new()
            {
                new(ValueTypes.Advantage, 3),
                new(ValueTypes.Tension, -2)
            },
            StandardRequirements = new()
            {
                new EnergyRequirement(EnergyTypes.Physical, 3),
                new SkillRequirement(SkillTypes.Strength, 5)
            },
            StandardOutcomes = new()
            {
                new ItemOutcome(ItemTypes.Weapon, -1, ItemConditionChangeTypes.Damage)
            }
        };

        return powerMovePattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateNegotiationPowerMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
        {
            Position = PositionTypes.Direct,
            Intent = IntentTypes.Progress,
            Scope = ScopeTypes.Immediate,
            BaseEnergyCost = 3,
            StandardValueChanges = new()
            {
                new(ValueTypes.Advantage, 3),
                new(ValueTypes.Tension, -2)
            },
            StandardRequirements = new()
            {
                new EnergyRequirement(EnergyTypes.Social, 3),
                new SkillRequirement(SkillTypes.Charisma, 5)
            },
            StandardOutcomes = new()
            {
                new ItemOutcome(ItemTypes.Valuable, -1, ItemConditionChangeTypes.Consume) // Consume the valuable item
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateInvestigationPowerMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
        {
            Position = PositionTypes.Direct,
            Intent = IntentTypes.Progress,
            Scope = ScopeTypes.Immediate,
            BaseEnergyCost = 3,
            StandardValueChanges = new()
            {
                new(ValueTypes.Advantage, 3),
                new(ValueTypes.Tension, -2)
            },
            StandardRequirements = new()
            {
                new EnergyRequirement(EnergyTypes.Focus, 3),
                new SkillRequirement(SkillTypes.Perception, 5)
            },
            StandardOutcomes = new()
            {
                new ItemOutcome(ItemTypes.Tool, -1, ItemConditionChangeTypes.Damage) // Tool might be damaged
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateLaborPowerMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
        {
            Position = PositionTypes.Direct,
            Intent = IntentTypes.Progress,
            Scope = ScopeTypes.Immediate,
            BaseEnergyCost = 3,
            StandardValueChanges = new()
            {
                new(ValueTypes.Advantage, 3),
                new(ValueTypes.Tension, -2)
            },
            StandardRequirements = new()
            {
                new EnergyRequirement(EnergyTypes.Physical, 3),
                new SkillRequirement(SkillTypes.Strength, 5)
            },
            StandardOutcomes = new()
            {
                new ItemOutcome(ItemTypes.Equipment, -1, ItemConditionChangeTypes.Damage) // Equipment might be damaged
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateSocialPowerMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
        {
            Position = PositionTypes.Direct,
            Intent = IntentTypes.Progress,
            Scope = ScopeTypes.Immediate,
            BaseEnergyCost = 3,
            StandardValueChanges = new()
            {
                new(ValueTypes.Advantage, 3),
                new(ValueTypes.Tension, -2)
            },
            StandardRequirements = new()
            {
                new EnergyRequirement(EnergyTypes.Social, 3),
                new SkillRequirement(SkillTypes.Charisma, 5)
            },
            StandardOutcomes = new()
            {
                new ItemOutcome(ItemTypes.Gift, -1, ItemConditionChangeTypes.Consume) // Gift is given away
            }
        };

        return pattern.CreateChoice(context, index);
    }

    // --- Expert Moves ---
    private EncounterChoice CreateExpertMoveChoice(EncounterActionContext context, int index)
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
                new EnergyRequirement(EnergyTypes.Focus, 2),
                new SkillRequirement(SkillTypes.Perception, 5)
            },
            StandardOutcomes = new()
            {
                new KnowledgeOutcome(KnowledgeTypes.Weakness, 1) // Example: Gain new knowledge
            }
        };

        return expertMovePattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateCombatExpertMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Physical, 2),
                new SkillRequirement(SkillTypes.Strength, 5)
            },
            StandardOutcomes = new()
            {
                new KnowledgeOutcome(KnowledgeTypes.Technique, 1) // Learn a new combat technique
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateNegotiationExpertMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Social, 2),
                new SkillRequirement(SkillTypes.Charisma, 5)
            },
            StandardOutcomes = new()
            {
                new KnowledgeOutcome(KnowledgeTypes.Leverage, 1) // Gain leverage over the opponent
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateLaborExpertMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Focus, 2),
                new SkillRequirement(SkillTypes.Crafting, 5)
            },
            StandardOutcomes = new()
            {
                new KnowledgeOutcome(KnowledgeTypes.Technique, 1) // Learn a new crafting technique
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateSocialExpertMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Social, 2),
                new SkillRequirement(SkillTypes.Charisma, 5)
            },
            StandardOutcomes = new()
            {
                new KnowledgeOutcome(KnowledgeTypes.Secret, 1) // Learn a secret about someone
            }
        };

        return pattern.CreateChoice(context, index);
    }

    // --- Social Moves ---
    private EncounterChoice CreateSocialMoveChoice(EncounterActionContext context, int index)
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
                new EnergyRequirement(EnergyTypes.Social, 2),
                new SkillRequirement(SkillTypes.Charisma, 5)
            },
            StandardOutcomes = new()
            {
                new ReputationOutcome(ReputationTypes.Honest, 1) // Example: Reinforce Honest reputation
            }
        };

        return socialMovePattern.CreateChoice(context, index);
    }
    private EncounterChoice CreateCombatSocialMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Physical, 2),
                new SkillRequirement(SkillTypes.Strength, 5)
            },
            StandardOutcomes = new()
            {
                new ReputationOutcome(ReputationTypes.Unbreakable, 1) // Reinforce Unbreakable reputation
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateInvestigationSocialMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Focus, 2),
                new SkillRequirement(SkillTypes.Perception, 5)
            },
            StandardOutcomes = new()
            {
                new ReputationOutcome(ReputationTypes.Sharp, 1) // Reinforce Sharp reputation
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateLaborSocialMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Physical, 2),
                new SkillRequirement(SkillTypes.Crafting, 5)
            },
            StandardOutcomes = new()
            {
                new ReputationOutcome(ReputationTypes.Reliable, 1) // Reinforce Reliable reputation
            }
        };

        return pattern.CreateChoice(context, index);
    }

    private EncounterChoice CreateGeneralSocialMoveChoice(EncounterActionContext context, int index)
    {
        ChoicePattern pattern = new()
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
                new EnergyRequirement(EnergyTypes.Social, 2),
                new SkillRequirement(SkillTypes.Charisma, 5)
            },
            StandardOutcomes = new()
            {
                new ReputationOutcome(ReputationTypes.Trusted, 1) // Reinforce Trusted reputation
            }
        };

        return pattern.CreateChoice(context, index);
    }
}
