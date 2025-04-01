public class ProjectionService
{
    private readonly TagManager _tagManager;
    private readonly ResourceManager _resourceManager;
    private readonly EncounterInfo encounterInfo;

    public ProjectionService(TagManager tagManager, ResourceManager resourceManager, EncounterInfo location)
    {
        _tagManager = tagManager;
        _resourceManager = resourceManager;
        encounterInfo = location;
    }

    public ChoiceProjection CreateChoiceProjection(
        IChoice choice,
        int currentMomentum,
        int currentPressure,
        int currentTurn)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Create working copy of tag system
        BaseTagSystem clonedTagSystem = _tagManager.CloneTagSystem();

        // Calculate momentum effects
        int momentumChange = 0;

        // Calculate pressure effects
        int pressureChange = 0;

        // Process Approach And Focus Effects
        ProcessApproachAndFocusEffects(choice, currentTurn, projection, ref momentumChange, ref pressureChange);

        // Process Strategic Tag Effects/
        ProcessStrategicTagEffects(choice, projection, clonedTagSystem, ref momentumChange, ref pressureChange);

        // Set projection values
        projection.MomentumGained = momentumChange;
        projection.PressureBuilt = pressureChange;

        // Ensure pressure can't go below 0
        EnsureNoNegativeEncounterValues(currentMomentum, currentPressure, projection);

        // Set final calculated values
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = Math.Max(0, currentPressure + projection.PressureBuilt);

        // Calculate pressure-based resource damage that will apply at start of turn
        CalculateDamageFromPressure(choice, encounterInfo, projection, currentPressure, projection.PressureBuilt);

        // Calculate projected turn and check if encounter will end
        int projectedTurn = currentTurn + 1;
        projection.ProjectedTurn = projectedTurn;

        DetermineEncounterEnd(projection, projectedTurn);

        // Apply all explicit tag modifications from the choice
        ProcessChoiceTagIncreases(choice, projection, clonedTagSystem);

        // Determine which tags will be active based on new tag values
        List<IEncounterTag> newlyActivatedTags = _tagManager.GetNewlyActivatedTags(clonedTagSystem, encounterInfo.AvailableTags);
        List<IEncounterTag> deactivatedTags = _tagManager.GetDeactivatedTags(clonedTagSystem, encounterInfo.AvailableTags);

        newlyActivatedTags.ForEach(tag => projection.NewlyActivatedTags.Add(tag.Name));
        deactivatedTags.ForEach(tag => projection.DeactivatedTags.Add(tag.Name));

        return projection;
    }

    private static void ProcessChoiceTagIncreases(IChoice choice, ChoiceProjection projection, BaseTagSystem clonedTagSystem)
    {
        foreach (TagModification mod in choice.TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState)
            {
                ApproachTags tag = (ApproachTags)mod.Tag;
                int oldValue = clonedTagSystem.GetEncounterStateTagValue(tag);
                clonedTagSystem.ModifyEncounterStateTag(tag, mod.Delta);
                int newValue = clonedTagSystem.GetEncounterStateTagValue(tag);
                int actualDelta = newValue - oldValue;

                if (actualDelta != 0)
                    projection.EncounterStateTagChanges[tag] = actualDelta;
            }
            else if (mod.Type == TagModification.TagTypes.Focus)
            {
                FocusTags tag = (FocusTags)mod.Tag;
                int oldValue = clonedTagSystem.GetFocusTagValue(tag);
                clonedTagSystem.ModifyFocusTag(tag, mod.Delta);
                int newValue = clonedTagSystem.GetFocusTagValue(tag);
                int actualDelta = newValue - oldValue;

                if (actualDelta != 0)
                    projection.FocusTagChanges[tag] = actualDelta;
            }
        }
    }

    private void CalculateDamageFromPressure(IChoice choice, EncounterInfo encounterInfo, ChoiceProjection projection, int currentPressure, int choicePressure)
    {
        int pressureHealthDamage = _resourceManager.CalculatePressureResourceDamage(
            encounterInfo, PlayerStatusResources.Health, currentPressure);

        int pressureConcentrationDamage = _resourceManager.CalculatePressureResourceDamage(
            encounterInfo, PlayerStatusResources.Concentration, currentPressure);

        int pressureConfidenceDamage = _resourceManager.CalculatePressureResourceDamage(
            encounterInfo, PlayerStatusResources.Confidence, currentPressure);

        // Add pressure resource components to projection
        if (encounterInfo.DangerousApproaches.Contains(choice.Approach))
        {
            if (pressureHealthDamage != 0)
            {
                projection.HealthChange = pressureHealthDamage;
                projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Pressure health damage",
                    Value = pressureHealthDamage
                });
            }

            if (pressureConcentrationDamage != 0)
            {
                projection.ConcentrationChange = pressureConcentrationDamage;
                projection.ConcentrationComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Pressure concentration damage",
                    Value = pressureConcentrationDamage
                });
            }

            if (pressureConfidenceDamage != 0)
            {
                projection.ConfidenceChange = pressureConfidenceDamage;
                projection.ConfidenceComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Pressure confidence damage",
                    Value = pressureConfidenceDamage
                });
            }
        }
    }

    private static void EnsureNoNegativeEncounterValues(int currentMomentum, int currentPressure, ChoiceProjection projection)
    {
        if (currentMomentum + projection.MomentumGained < 0)
        {
            if (projection.MomentumGained < 0)
            {
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Minimum momentum limit",
                    Value = -currentMomentum - projection.MomentumGained
                });
            }
            projection.MomentumGained = -currentMomentum;
        }
        if (currentPressure + projection.PressureBuilt < 0)
        {
            if (projection.PressureBuilt < 0)
            {
                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Minimum pressure limit",
                    Value = -currentPressure - projection.PressureBuilt
                });
            }
            projection.PressureBuilt = -currentPressure;
        }
    }

    private void ProcessStrategicTagEffects(IChoice choice, ChoiceProjection projection, BaseTagSystem baseTagSystem, ref int momentumChange, ref int pressureChange)
    {
        StrategicEffect effect = choice.StrategicEffect;
        List<StrategicTag> strategicTags = _tagManager.GetStrategicActiveTags();

        foreach (StrategicTag tag in strategicTags)
        {
            int momentumEffect = effect.GetMomentumModifierForTag(tag, baseTagSystem);
            if (momentumEffect != 0)
            {
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = $"{tag.Name} ({tag.EnvironmentalProperty.GetPropertyType()})",
                    Value = momentumEffect
                });
                momentumChange += momentumEffect;
            }

            int pressureEffect = effect.GetPressureModifierForTag(tag, baseTagSystem);
            if (pressureEffect != 0)
            {
                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = $"{tag.Name} ({tag.EnvironmentalProperty.GetPropertyType()})",
                    Value = pressureEffect
                });
                pressureChange += pressureEffect;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="choice"></param>
    /// <param name="currentTurn"></param>
    /// <param name="projection"></param>
    /// <param name="momentumChange"></param>
    /// <param name="pressureChange"></param>
    private void ProcessApproachAndFocusEffects(IChoice choice, int currentTurn, ChoiceProjection projection, ref int momentumChange, ref int pressureChange)
    {
        // Momentum Choice
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = choice.BaseEffectValue;
            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = $"Momentum Building Choice (Tier {(int)choice.Tier})",
                Value = baseMomentum
            });
            momentumChange += baseMomentum;

            int environmentalPressure = encounterInfo.GetEnvironmentalPressure(currentTurn);
            if (environmentalPressure > 0)
            {
                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Environmental Pressure",
                    Value = environmentalPressure
                });
                pressureChange += environmentalPressure;
            }
        }

        // Pressure Reduction Choice

        if (choice.EffectType == EffectTypes.Pressure)
        {
            int basePressure = -choice.BaseEffectValue;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = $"Pressure Reduction Choice (Tier {(int)choice.Tier})",
                Value = basePressure
            });
            pressureChange += basePressure;

        }

        if (encounterInfo.MomentumBoostApproaches.Contains(choice.Approach))
        {
            int favoredBonus = 2;
            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Favored Approach",
                Value = favoredBonus
            });
            momentumChange += favoredBonus;
        }

        if (encounterInfo.DangerousApproaches.Contains(choice.Approach))
        {
            int dangerousApproachBonus = 3;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Dangerous Approach",
                Value = dangerousApproachBonus
            });
            pressureChange += dangerousApproachBonus;
        }
    }

    private void DetermineEncounterEnd(
        ChoiceProjection projection,
        int projectedTurn)
    {
        bool encounterEnds =
            (projectedTurn >= encounterInfo.TurnDuration)
            || (projection.FinalMomentum >= encounterInfo.ExceptionalThreshold)
            || (projection.FinalPressure >= encounterInfo.MaxPressure);

        projection.EncounterWillEnd = encounterEnds;

        // Determine outcome if encounter ends
        if (encounterEnds)
        {
            if (projection.FinalPressure >= encounterInfo.MaxPressure)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
            else
            {
                if (projection.FinalMomentum >= encounterInfo.ExceptionalThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.ExceptionalSuccess;
                else if (projection.FinalMomentum >= encounterInfo.StandardThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.StandardSuccess;
                else if (projection.FinalMomentum >= encounterInfo.PartialThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.PartialSuccess;
                else
                    projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
        }
    }
}
