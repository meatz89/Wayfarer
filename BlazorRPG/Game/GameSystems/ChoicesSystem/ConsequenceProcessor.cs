
public class ConsequenceProcessor
{
    private readonly GameState gameState;
    private readonly EncounterActionContext context;
    private readonly LocationProperties locationProperties;

    public ConsequenceProcessor(GameState gameState, EncounterActionContext context, LocationProperties locationProperties)
    {
        this.gameState = gameState;
        this.context = context;
        this.locationProperties = locationProperties;
    }

    public void ProcessConsequences(EncounterChoice choice, Encounter encounter)
    {
        // 1. Apply Energy Costs (and handle potential consequences)
        ApplyEnergyCosts(choice, context);

        // 2. Narrative Value Changes (direct changes from the choice, including modifiers)
        ApplyValueChanges(choice.EncounterValueChanges);

        // 3. Record Modifiers from this Choice in Encounter Context
        RecordChoiceModifiers(choice, context);

        // 4. Apply Encounter State Value Modifications (indirect effects based on current state)
        ApplyEncounterStateValueModifications(encounter);

        // 5. Apply Choice Costs and Rewards (permanent effects)
        foreach (Outcome cost in choice.PermanentCosts)
        {
            cost.Apply(gameState.Player);
        }

        foreach (Outcome reward in choice.PermanentRewards)
        {
            reward.Apply(gameState.Player);
        }

        // 6. Process Consequences (determine the narrative outcome based on values)
        ProcessOutcomeConsequences();
        ProcessPressureConsequences();
        ProcessInsightConsequences();
        ProcessResonanceConsequences();

        // 7. Compound effects from value combinations
        ProcessCombinedValueConsequences();

        // 8. Context-specific modifications
        ApplyLocationTypeModifiers();
        ApplyTimeOfDayModifiers();
        ApplyPlayerStateModifiers();

        // Check for game over conditions after applying choice effects
        if (IsGameOver())
        {
            // Handle game over
            Console.WriteLine("Game Over!");
            gameState.Actions.SetActiveEncounter(null);
        }
    }

    private void ApplyValueChanges(List<ValueChange> valueChanges)
    {
        foreach (ValueChange change in valueChanges)
        {
            switch (change.ValueType)
            {
                case ValueTypes.Outcome:
                    context.CurrentValues.Outcome += change.Change;
                    break;
                case ValueTypes.Pressure:
                    context.CurrentValues.Pressure += change.Change;
                    break;
                case ValueTypes.Insight:
                    context.CurrentValues.Insight += change.Change;
                    break;
                case ValueTypes.Resonance:
                    context.CurrentValues.Resonance += change.Change;
                    break;
            }
        }

        // Clamp values to 0-10 range
        context.CurrentValues.Outcome = Math.Clamp(context.CurrentValues.Outcome, 0, 20);
        context.CurrentValues.Insight = Math.Clamp(context.CurrentValues.Insight, 0, 20);
        context.CurrentValues.Resonance = Math.Clamp(context.CurrentValues.Resonance, 0, 20);
        context.CurrentValues.Pressure = Math.Clamp(context.CurrentValues.Pressure, 0, 20);
    }

    private void RecordChoiceModifiers(EncounterChoice choice, EncounterActionContext context)
    {
        // Store the modifiers used in this choice for later display in the UI
        if (choice.ValueModifiers != null)
        {
            foreach (KeyValuePair<string, int> modifier in choice.ValueModifiers)
            {
                ChoiceModifierEntry entry = new ChoiceModifierEntry()
                {
                    Name = modifier.Key,
                    Value = modifier.Value,
                };
                context.ChoiceModifiersHistory.Add(entry);
            }
        }
    }

    private void ApplyEncounterStateValueModifications(Encounter encounter)
    {
        EncounterActionContext context = encounter.Context;

        // **Resonance Modifiers**
        if (context.CurrentValues.Resonance >= 8)
        {
            context.CurrentValues.Outcome += 2;
        }
        else if (context.CurrentValues.Resonance >= 5)
        {
            context.CurrentValues.Outcome += 1;
        }

        // **Pressure as a Strain Mechanic**
        // Instead of directly applying penalties based on Pressure, we now modify the encounter context.
        // This includes increasing energy costs for choices and potentially affecting other modifiers.
        // We do this by modifying the EnergyRequirements of each choice directly.

        foreach (EncounterChoice nextChoice in encounter.GetCurrentStage().Choices)
        {
            foreach (Requirement req in nextChoice.ChoiceRequirements)
            {
                if (req is EnergyRequirement energyReq)
                {
                    // Increase base energy cost by 1 for every 3 points of Pressure
                    energyReq.Amount += context.CurrentValues.Pressure / 3;
                }
            }
        }
    }

    private void ApplyEnergyCosts(EncounterChoice choice, EncounterActionContext context)
    {
        foreach (Requirement req in choice.ChoiceRequirements)
        {
            if (req is EnergyRequirement energyReq)
            {
                int cost = energyReq.Amount;

                switch (energyReq.EnergyType)
                {
                    case EnergyTypes.Physical:
                        gameState.Player.PhysicalEnergy -= cost;
                        if (gameState.Player.PhysicalEnergy < 0)
                        {
                            gameState.Player.Health += gameState.Player.PhysicalEnergy; // Health penalty equals the amount of energy overspent
                            gameState.Player.PhysicalEnergy = 0; // Deplete energy
                        }
                        break;

                    case EnergyTypes.Focus:
                        gameState.Player.FocusEnergy -= cost;
                        if (gameState.Player.FocusEnergy < 0)
                        {
                            // Apply a negative consequence related to low Focus Energy
                            context.CurrentValues.Pressure += 2; // Example: Increase Pressure due to lack of focus
                            gameState.Player.FocusEnergy = 0; // Deplete energy
                        }
                        break;

                    case EnergyTypes.Social:
                        gameState.Player.SocialEnergy -= cost;
                        if (gameState.Player.SocialEnergy < 0)
                        {
                            gameState.Player.Reputation += gameState.Player.SocialEnergy; // Reputation penalty equals the amount of energy overspent
                            gameState.Player.SocialEnergy = 0; // Deplete energy
                        }
                        break;
                }
            }
        }
    }

    private bool IsGameOver()
    {
        return gameState.Player.Health <= 0 || gameState.Player.Reputation <= 0;
    }

    private void ProcessOutcomeConsequences()
    {
        int magnitude = context.CurrentValues.Outcome;

        // Base resource gain scaled by outcome
        int resourceGain = magnitude * GetLocationResourceMultiplier();
        gameState.Player.ModifyResource(ResourceChangeTypes.Added, context.LocationProperties.Resource.Value, resourceGain);

        // Skill experience gain
        if (magnitude >= 5)
        {
            gameState.Player.ModifySkillLevel(GetRelevantSkill(context), 1); // Assuming a method to determine relevant skill
        }
    }

    private void ProcessPressureConsequences()
    {
        int pressure = context.CurrentValues.Pressure;

        // Health impact from high pressure
        if (pressure >= 7)
        {
            int healthLoss = (pressure - 6) * GetLocationDangerMultiplier();
            gameState.Player.ModifyHealth(-healthLoss);
        }
    }

    private void ProcessInsightConsequences()
    {
        int insight = context.CurrentValues.Insight;

        if (insight >= 7)
        {
            // Example: Grant a piece of knowledge relevant to the location or encounter
            gameState.Player.ModifyKnowledge(GetRelevantKnowledgeType(context), 1);
        }
    }

    private void ProcessResonanceConsequences()
    {
        int resonance = context.CurrentValues.Resonance;

        if (resonance >= 7)
        {
            // Example: Increase reputation with a faction relevant to the location
            gameState.Player.ModifyReputation(GetRelevantReputationType(context), 1);
        }
    }

    private void ProcessCombinedValueConsequences()
    {
        EncounterStateValues values = context.CurrentValues;

        // Success with mastery
        if (values.Outcome >= 7 && values.Insight >= 7)
        {
            // Bonus rewards and potential knowledge gain
            ProcessMasterySuccess();
        }

        // Catastrophic failure
        if (values.Outcome <= 3 && values.Pressure >= 7)
        {
            // Severe consequences based on context
            ProcessCatastrophicFailure();
        }

        // Social triumph
        if (values.Outcome >= 7 && values.Resonance >= 7)
        {
            // Relationship and reputation gains
            ProcessSocialTriumph();
        }
    }

    private void ApplyLocationTypeModifiers()
    {
        LocationTypes locationType = context.LocationType;

        switch (locationType)
        {
            case LocationTypes.Industrial:
                ApplyIndustrialModifiers();
                break;
            case LocationTypes.Social:
                ApplySocialModifiers();
                break;
            case LocationTypes.Nature:
                ApplyNaturalModifiers();
                break;
        }
    }

    private void ApplyIndustrialModifiers()
    {
        // Industrial locations increase physical consequences
        if (context.CurrentValues.Pressure >= 5)
        {
            int additionalHealthLoss = context.CurrentValues.Pressure / 2;
            gameState.Player.ModifyHealth(-additionalHealthLoss);
        }
    }

    private void ApplySocialModifiers()
    {
        // Social locations might increase reputation consequences
        if (context.CurrentValues.Pressure >= 5)
        {
            int additionalReputationLoss = context.CurrentValues.Pressure / 3;
            gameState.Player.ModifyReputation(GetRelevantReputationType(context), -additionalReputationLoss); // Example using a relevant reputation type
        }
    }

    private void ApplyNaturalModifiers()
    {
        // Natural locations might increase resource gain based on Insight
        if (context.CurrentValues.Insight >= 5)
        {
            int additionalResourceGain = context.CurrentValues.Insight / 2;
            gameState.Player.ModifyResource(ResourceChangeTypes.Added, context.LocationProperties.Resource.Value, additionalResourceGain);
        }
    }

    private void ApplyTimeOfDayModifiers()
    {
        // Example: Nighttime increases Pressure
        if (context.TimeSlot == TimeSlots.Night)
        {
            context.CurrentValues.Pressure += 1;
        }
    }

    private void ApplyPlayerStateModifiers()
    {
        // Low health reduces reward magnitude
        if (gameState.Player.Health <= 3)
        {
            ModifyAllRewards(0.5f);
        }

        // High stress increases pressure impact
        if (gameState.Player.Stress >= 7)
        {
            IncreasePressureEffects(1.5f);
        }
    }

    // Placeholder methods - you'll need to implement the actual logic
    private int GetLocationResourceMultiplier()
    {
        // Example: Check the location properties for a resource multiplier
        if (context.LocationProperties.Resource != null)
        {
            switch (context.LocationProperties.Resource)
            {
                case ResourceTypes.Food:
                    return 2; // Food resources are more abundant
                case ResourceTypes.Wood:
                    return 3;
                default: return 1;
            }
        }
        return 1;
    }
    private int GetLocationDangerMultiplier()
    {
        // Example: Check the location properties for a danger multiplier
        if (context.LocationProperties.Pressure == PressureStateTypes.Hostile)
        {
            return 3;
        }
        else if (context.LocationProperties.Pressure == PressureStateTypes.Alert)
        {
            return 2;
        }
        else return 1;
    }
    private SkillTypes GetRelevantSkill(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => SkillTypes.Strength,
            BasicActionTypes.Investigate => SkillTypes.Perception,
            BasicActionTypes.Mingle => SkillTypes.Charisma,
            _ => SkillTypes.None
        };
    }
    private KnowledgeTypes GetRelevantKnowledgeType(EncounterActionContext context)
    {
        // Example: Return a KnowledgeType relevant to the current location or action
        return KnowledgeTypes.Secret;
    }

    private ReputationTypes GetRelevantReputationType(EncounterActionContext context)
    {
        // Example: Return a ReputationType relevant to the current location or action
        return context.LocationProperties.ReputationType;
    }

    private void ProcessMasterySuccess()
    {
        //grant bonus rewards
        gameState.Player.ModifyResource(ResourceChangeTypes.Added, context.LocationProperties.Resource.Value, context.CurrentValues.Outcome);
    }
    private void ProcessCatastrophicFailure()
    {
        switch (context.LocationType)
        {
            case LocationTypes.Industrial:
                gameState.Player.ModifyHealth(-10);
                break;
            case LocationTypes.Social:
                gameState.Player.ModifyReputation(GetRelevantReputationType(context), -5);
                break;
            case LocationTypes.Nature:
                gameState.Player.ModifyResource(ResourceChangeTypes.Removed, ResourceTypes.Food, -5);
                break;
        }
    }
    private void ProcessSocialTriumph()
    {
        gameState.Player.ModifyReputation(GetRelevantReputationType(context), 5);
    }
    private void ModifyAllRewards(float v)
    {
        // Example: Reduce all resource rewards
        // You might need to adjust this based on how you track rewards
        // gameState.Player.ModifyResource(ResourceChangeTypes.Added, context.LocationProperties.Resource, (int)(resourceGain * multiplier));
    }

    private void IncreasePressureEffects(float v)
    {
        // Example: Increase health loss from Pressure
        // You might need to adjust this based on how you track consequences
        // int healthLoss = (pressure - 6) * GetLocationDangerMultiplier();
        // gameState.Player.ModifyHealth(-(int)(healthLoss * multiplier));
    }

    // ... other helper methods as needed ...
}