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

        // Calculate pressure-based resource damage that will apply at start of turn
        int pressureHealthDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Health, currentPressure);
        int pressureConcentrationDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Concentration, currentPressure);
        int pressureConfidenceDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Confidence, currentPressure);

        // Add pressure resource components to projection
        if (encounterInfo.DisfavoredFocuses.Contains(choice.Focus))
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

        // Calculate momentum effects
        int momentumChange = 0;

        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = choice is SpecialChoice ? 3 : (2);
            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Momentum Building Choice",
                Value = baseMomentum
            });
            momentumChange += baseMomentum;
        }

        if (encounterInfo.FavoredFocuses.Contains(choice.Focus))
        {
            int favoredBonus = 1;
            if (choice.EffectType == EffectTypes.Momentum)
            {
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Favored Choice",
                    Value = favoredBonus
                });
                momentumChange += favoredBonus;
            }
        }

        // Calculate pressure effects
        int pressureChange = 0;

        if (choice.EffectType == EffectTypes.Pressure)
        {
            int basePressure = -1;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure Reduction Choice",
                Value = basePressure
            });
            pressureChange += basePressure;
        }

        int environmentalPressure = encounterInfo.GetEnvironmentalPressure(currentTurn);
        if (environmentalPressure > 0)
        {
            if (encounterInfo.DisfavoredFocuses.Contains(choice.Focus))
            {
                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Disfavored Choice",
                    Value = environmentalPressure
                });
                pressureChange += environmentalPressure;
            }
        }

        foreach (IEncounterTag tag in _tagManager.ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                bool affectsChoice = true;

                if (choice.Approach != strategicTag.ScalingApproachTag)
                    affectsChoice = false;

                if (affectsChoice)
                {
                    int momentumEffect = strategicTag.GetMomentumModifierForChoice(choice, clonedTagSystem);
                    if (momentumEffect != 0)
                    {
                        projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                        {
                            Source = tag.Name,
                            Value = momentumEffect
                        });
                        momentumChange += momentumEffect;
                    }

                    int pressureEffect = strategicTag.GetPressureModifierForChoice(choice, clonedTagSystem);
                    if (pressureEffect != 0)
                    {
                        projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                        {
                            Source = tag.Name,
                            Value = pressureEffect
                        });
                        pressureChange += pressureEffect;
                    }
                }
            }
        }

        // Calculate end-of-turn pressure changes
        int endOfTurnPressureChange = _tagManager.GetEndOfTurnPressureReduction();
        if (endOfTurnPressureChange != 0)
        {
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "End of Turn",
                Value = endOfTurnPressureChange
            });
            pressureChange += endOfTurnPressureChange;
        }

        // Set projection values
        projection.MomentumGained = momentumChange;
        projection.PressureBuilt = pressureChange;

        // Ensure pressure can't go below 0
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

        // Set final calculated values
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = Math.Max(0, currentPressure + projection.PressureBuilt);

        // Calculate projected turn and check if encounter will end
        int projectedTurn = currentTurn + 1;
        projection.ProjectedTurn = projectedTurn;
        
        projection = DetermineEncounterEnd(projection, projectedTurn);

        // Determine which tags will be active based on new tag values
        List<IEncounterTag> newlyActivatedTags = _tagManager.GetNewlyActivatedTags(clonedTagSystem, encounterInfo.AvailableTags);
        List<IEncounterTag> deactivatedTags = _tagManager.GetDeactivatedTags(clonedTagSystem, encounterInfo.AvailableTags);

        newlyActivatedTags.ForEach(tag => projection.NewlyActivatedTags.Add(tag.Name));
        deactivatedTags.ForEach(tag => projection.DeactivatedTags.Add(tag.Name));

        // Apply all explicit tag modifications from the choice
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

        return projection;
    }

    private ChoiceProjection DetermineEncounterEnd(
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
                    projection.ProjectedOutcome = EncounterOutcomes.Exceptional;
                else if (projection.FinalMomentum >= encounterInfo.StandardThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Standard;
                else if (projection.FinalMomentum >= encounterInfo.PartialThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Partial;
                else
                    projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
        }

        return projection;
    }
}
