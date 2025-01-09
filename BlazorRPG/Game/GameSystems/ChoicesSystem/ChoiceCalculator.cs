public class ChoiceCalculator
{
    public ChoiceConsequences CalculateConsequences(EncounterChoice choice, EncounterContext context)
    {
        ChoiceConsequences consequences = new ChoiceConsequences
        {
            BaseValueChanges = choice.BaseValueChanges,
            BaseRequirements = choice.Requirements,
            BaseCosts = choice.BaseCosts,
            BaseRewards = choice.BaseRewards
        };

        // Calculate all modifiers first
        CalculateValueModifiers(choice, consequences, context);

        // Apply modifiers to get modified values
        CalculateModifiedValues(consequences);
        CalculateModifiedRequirements(consequences, context);
        CalculateModifiedOutcomes(choice, consequences, context); // Pass the choice here

        return consequences;
    }

    private void CalculateValueModifiers(EncounterChoice choice, ChoiceConsequences consequences, EncounterContext context)
    {
        ChoiceValueModifiers modifiers = new ChoiceValueModifiers();
        Dictionary<ValueTypes, List<(string Source, int Amount)>> valueChangeDetails = new Dictionary<ValueTypes, List<(string Source, int Amount)>>();

        // Initialize details dictionary with base values
        foreach (ValueChange baseChange in consequences.BaseValueChanges)
        {
            valueChangeDetails[baseChange.ValueType] = new List<(string Source, int Amount)>
            {
                ("Base", baseChange.Change)
            };
        }

        // Skill vs Difficulty impact
        int skillVsDifficulty = context.PlayerState.GetSkillLevel(choice.ChoiceRelevantSkill) - context.LocationDifficulty;

        // Only give a bonus to outcome if it is the right archetype
        if (choice.Archetype == ChoiceArchetypes.Physical)
        {
            modifiers.OutcomeModifier += skillVsDifficulty;
            modifiers.AddModifierDetail("Skill Level", skillVsDifficulty);
            AddValueChangeDetail(valueChangeDetails, ValueTypes.Outcome, "Skill Level", skillVsDifficulty);
        }
        else if (choice.Archetype == ChoiceArchetypes.Focus)
        {
            modifiers.InsightGainModifier += skillVsDifficulty;
            modifiers.AddModifierDetail("Skill Level", skillVsDifficulty);
            AddValueChangeDetail(valueChangeDetails, ValueTypes.Insight, "Skill Level", skillVsDifficulty);
        }
        else if (choice.Archetype == ChoiceArchetypes.Social)
        {
            modifiers.ResonanceGainModifier += skillVsDifficulty;
            modifiers.AddModifierDetail("Skill Level", skillVsDifficulty);
            AddValueChangeDetail(valueChangeDetails, ValueTypes.Resonance, "Skill Level", skillVsDifficulty);
        }

        // Location impact
        switch (context.LocationType)
        {
            case LocationTypes.Industrial:
                // Industrial locations generally increase pressure. 
                // Aggressive choices are even more risky here.
                if (choice.Approach == ChoiceApproaches.Aggressive)
                {
                    modifiers.PressureGainModifier += 2;
                    modifiers.AddModifierDetail("Industrial Location (Aggressive)", 2);
                    AddValueChangeDetail(valueChangeDetails, ValueTypes.Pressure, "Industrial Location (Aggressive)", 2);
                }
                else
                {
                    modifiers.PressureGainModifier += 1;
                    modifiers.AddModifierDetail("Industrial Location", 1);
                    AddValueChangeDetail(valueChangeDetails, ValueTypes.Pressure, "Industrial Location", 1);
                }
                break;
            case LocationTypes.Social:
                // Social locations generally favor resonance gain. 
                // Strategic choices are particularly effective here.
                if (choice.Approach == ChoiceApproaches.Strategic)
                {
                    modifiers.ResonanceGainModifier += 2;
                    modifiers.AddModifierDetail("Social Location (Strategic)", 2);
                    AddValueChangeDetail(valueChangeDetails, ValueTypes.Resonance, "Social Location (Strategic)", 2);
                }
                else
                {
                    modifiers.ResonanceGainModifier += 1;
                    modifiers.AddModifierDetail("Social Location", 1);
                    AddValueChangeDetail(valueChangeDetails, ValueTypes.Resonance, "Social Location", 1);
                }
                break;
            case LocationTypes.Nature:
                // Nature locations generally favor insight gain. 
                // Careful choices are particularly effective here.
                if (choice.Approach == ChoiceApproaches.Careful)
                {
                    modifiers.InsightGainModifier += 2;
                    modifiers.AddModifierDetail("Nature Location (Careful)", 2);
                    AddValueChangeDetail(valueChangeDetails, ValueTypes.Insight, "Nature Location (Careful)", 2);
                }
                else
                {
                    modifiers.InsightGainModifier += 1;
                    modifiers.AddModifierDetail("Nature Location", 1);
                    AddValueChangeDetail(valueChangeDetails, ValueTypes.Insight, "Nature Location", 1);
                }
                break;
        }

        // Insight impact on Pressure
        // Higher insight reduces pressure gain, especially for Focus archetype choices.
        int insightLevel = context.CurrentValues.Insight;
        int pressureReduction = -insightLevel / 2;

        if (choice.Archetype == ChoiceArchetypes.Focus)
        {
            modifiers.PressureGainModifier += pressureReduction * 2; // Focus choices benefit more from insight
            modifiers.AddModifierDetail("Insight (Focus)", pressureReduction * 2);
            AddValueChangeDetail(valueChangeDetails, ValueTypes.Pressure, "Insight (Focus)", pressureReduction * 2);
        }
        else
        {
            modifiers.PressureGainModifier += pressureReduction;
            modifiers.AddModifierDetail("Insight", pressureReduction);
            AddValueChangeDetail(valueChangeDetails, ValueTypes.Pressure, "Insight", pressureReduction);
        }

        // Energy cost strain from pressure
        // This is handled in CalculateModifiedOutcomes to affect energy costs directly

        consequences.Modifiers = modifiers;
        consequences.ValueChangeDetails = GetValueChangeDetails(valueChangeDetails);
    }

    private void AddValueChangeDetail(Dictionary<ValueTypes, List<(string Source, int Amount)>> details,
        ValueTypes type, string source, int amount)
    {
        if (!details.ContainsKey(type))
        {
            details[type] = new List<(string Source, int Amount)>();
        }
        details[type].Add((source, amount));
    }

    private List<ValueChangeDetail> GetValueChangeDetails(Dictionary<ValueTypes, List<(string Source, int Amount)>> details)
    {
        List<ValueChangeDetail> valueChangeDetails = new List<ValueChangeDetail>();
        foreach (KeyValuePair<ValueTypes, List<(string Source, int Amount)>> detail in details)
        {
            List<ValueChangeSource> sources = new List<ValueChangeSource>();
            foreach ((string Source, int Amount) in detail.Value)
            {
                sources.Add(new ValueChangeSource(Source, Amount));
            }
            valueChangeDetails.Add(new ValueChangeDetail(detail.Key, sources));
        }
        return valueChangeDetails;
    }

    private void CalculateModifiedValues(ChoiceConsequences consequences)
    {
        foreach (ValueChange baseChange in consequences.BaseValueChanges)
        {
            ValueChange modifiedChange = new ValueChange(baseChange.ValueType, baseChange.Change);

            switch (baseChange.ValueType)
            {
                case ValueTypes.Outcome:
                    modifiedChange.Change += consequences.Modifiers.OutcomeModifier;
                    break;
                case ValueTypes.Pressure:
                    modifiedChange.Change += consequences.Modifiers.PressureGainModifier;
                    break;
                case ValueTypes.Insight:
                    modifiedChange.Change += consequences.Modifiers.InsightGainModifier;
                    break;
                case ValueTypes.Resonance:
                    modifiedChange.Change += consequences.Modifiers.ResonanceGainModifier;
                    break;
            }

            consequences.ModifiedValueChanges.Add(modifiedChange);
        }
    }

    private void CalculateModifiedRequirements(ChoiceConsequences consequences, EncounterContext context)
    {
        foreach (Requirement baseReq in consequences.BaseRequirements)
        {
            if (baseReq is EnergyRequirement energyReq)
            {
                // Create new requirement with pressure modifier
                int pressureModifier = context.CurrentValues.Pressure / 3;
                if (pressureModifier > 0)
                {
                    consequences.RequirementModifications.Add(new RequirementModification
                    {
                        Source = "High Pressure",
                        RequirementType = energyReq.EnergyType.ToString(),
                        Amount = pressureModifier
                    });

                    consequences.ModifiedRequirements.Add(new EnergyRequirement(
                        energyReq.EnergyType,
                        energyReq.Amount + pressureModifier));
                }
                else
                {
                    consequences.ModifiedRequirements.Add(baseReq);
                }
            }
            else
            {
                consequences.ModifiedRequirements.Add(baseReq);
            }
        }
    }

    private void CalculateModifiedOutcomes(EncounterChoice choice, ChoiceConsequences consequences, EncounterContext context)
    {
        // First handle costs
        foreach (Outcome baseCost in consequences.BaseCosts)
        {
            // High pressure now makes choices more expensive
            if (baseCost is EnergyOutcome energyCost)
            {
                int strainModifier = context.CurrentValues.Pressure / 3;

                // Increase energy cost based on choice archetype
                if (choice.Archetype == ChoiceArchetypes.Physical)
                {
                    strainModifier *= 2; // Physical choices are more expensive under pressure
                }
                else if (choice.Archetype == ChoiceArchetypes.Social)
                {
                    strainModifier += strainModifier / 2; // Social choices are slightly more expensive
                }

                if (strainModifier > 0)
                {
                    consequences.CostModifications.Add(new OutcomeModification
                    {
                        Source = "High Pressure",
                        OutcomeType = energyCost.EnergyType.ToString(),
                        Amount = strainModifier
                    });

                    consequences.ModifiedCosts.Add(new EnergyOutcome(
                        energyCost.EnergyType,
                        energyCost.Amount + strainModifier));
                }
                else
                {
                    consequences.ModifiedCosts.Add(baseCost);
                }
            }
            else
            {
                consequences.ModifiedCosts.Add(baseCost);
            }
        }

        // Then handle rewards
        foreach (Outcome baseReward in consequences.BaseRewards)
        {
            if (baseReward is ResourceOutcome resourceReward)
            {
                float insightBonus = context.CurrentValues.Insight;
                // Focus choices get a larger bonus from insight for resource rewards
                if (choice.Archetype == ChoiceArchetypes.Focus)
                {
                    insightBonus *= 2;
                }

                if (insightBonus > 0)
                {
                    int bonusAmount = (int)(resourceReward.Amount * insightBonus);
                    consequences.RewardModifications.Add(new OutcomeModification
                    {
                        Source = "High Insight",
                        OutcomeType = resourceReward.ResourceType.ToString(),
                        Amount = bonusAmount
                    });

                    consequences.ModifiedRewards.Add(new ResourceOutcome(
                        resourceReward.ResourceType,
                        resourceReward.Amount + bonusAmount));
                }
                else
                {
                    consequences.ModifiedRewards.Add(baseReward);
                }
            }
            else if (baseReward is ReputationOutcome reputationReward)
            {
                float resonanceBonus = context.CurrentValues.Resonance;
                // Social choices get a larger bonus from resonance for reputation rewards
                if (choice.Archetype == ChoiceArchetypes.Social)
                {
                    resonanceBonus *= 2;
                }

                if (resonanceBonus > 0)
                {
                    int bonusAmount = (int)(reputationReward.Amount * resonanceBonus);
                    consequences.RewardModifications.Add(new OutcomeModification
                    {
                        Source = "High Resonance",
                        OutcomeType = reputationReward.ReputationType.ToString(),
                        Amount = bonusAmount
                    });

                    consequences.ModifiedRewards.Add(new ReputationOutcome(
                        reputationReward.ReputationType,
                        reputationReward.Amount + bonusAmount));
                }
                else
                {
                    consequences.ModifiedRewards.Add(baseReward);
                }
            }
            else
            {
                consequences.ModifiedRewards.Add(baseReward);
            }
        }
    }
}