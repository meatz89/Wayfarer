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
        CalculateModifiedOutcomes(consequences, context);

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
        int skillVsDifficulty = context.PlayerState.GetSkillLevel(context.PrimarySkillType) - context.LocationDifficulty;
        modifiers.OutcomeModifier += skillVsDifficulty;
        modifiers.AddModifierDetail("Skill Level", skillVsDifficulty);
        AddValueChangeDetail(valueChangeDetails, ValueTypes.Outcome, "Skill Level", skillVsDifficulty);

        // Location impact
        switch (context.LocationType)
        {
            case LocationTypes.Industrial:
                modifiers.PressureGainModifier += 2;
                modifiers.AddModifierDetail("Industrial Location", 2);
                AddValueChangeDetail(valueChangeDetails, ValueTypes.Pressure, "Industrial Location", 2);
                break;
            case LocationTypes.Social:
                modifiers.ResonanceGainModifier += 1;
                modifiers.AddModifierDetail("Social Location", 1);
                AddValueChangeDetail(valueChangeDetails, ValueTypes.Resonance, "Social Location", 1);
                break;
            case LocationTypes.Nature:
                modifiers.InsightGainModifier += 1;
                modifiers.AddModifierDetail("Natural Location", 1);
                AddValueChangeDetail(valueChangeDetails, ValueTypes.Insight, "Natural Location", 1);
                break;
        }

        // Insight impact on Pressure
        int insightLevel = context.CurrentValues.Insight;
        int pressureReduction = -insightLevel / 2;
        modifiers.PressureGainModifier += pressureReduction;
        modifiers.AddModifierDetail("Insight Pressure Reduction", pressureReduction);
        AddValueChangeDetail(valueChangeDetails, ValueTypes.Pressure, "Insight Reduction", pressureReduction);

        // Energy cost strain from pressure
        int strainModifier = context.CurrentValues.Pressure / 3;
        if (strainModifier > 0)
        {
            modifiers.EnergyCostModifier += strainModifier;
            modifiers.AddModifierDetail("Pressure Strain", strainModifier);
            AddValueChangeDetail(valueChangeDetails, ValueTypes.Energy, "Pressure Strain", strainModifier);
        }

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

            // Add details for UI/preview
            consequences.ValueChangeDetails.Add(new ValueChangeDetail(
                baseChange.ValueType,
                new List<ValueChangeSource>
                {
                new("Base", baseChange.Change),
                new("Modifier", modifiedChange.Change - baseChange.Change)
                }));
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

    private void CalculateModifiedOutcomes(ChoiceConsequences consequences, EncounterContext context)
    {
        // First handle costs
        foreach (Outcome baseCost in consequences.BaseCosts)
        {
            if (baseCost is EnergyOutcome energyCost)
            {
                int strainModifier = context.CurrentValues.Pressure / 3;
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
                float insightBonus = context.CurrentValues.Insight * 0.1f;
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
                float resonanceBonus = context.CurrentValues.Resonance * 0.2f;
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
