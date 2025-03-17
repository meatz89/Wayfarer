public class ProjectionService
{
    private readonly TagManager _tagManager;
    private readonly ResourceManager _resourceManager;
    private readonly LocationInfo _location;

    public ProjectionService(TagManager tagManager, ResourceManager resourceManager, LocationInfo location)
    {
        _tagManager = tagManager;
        _resourceManager = resourceManager;
        _location = location;
    }

    public ChoiceProjection CreateChoiceProjection(IChoice choice, int currentMomentum, int currentPressure, int currentTurn, int escalationLevel)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Create working copy of tag system
        BaseTagSystem clonedTagSystem = _tagManager.CloneTagSystem();

        // Calculate pressure-based resource damage that will apply at start of turn
        int pressureHealthDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Health, currentPressure);
        int pressureFocusDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Focus, currentPressure);
        int pressureConfidenceDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Confidence, currentPressure);

        // Add pressure resource components to projection
        if (pressureHealthDamage != 0)
        {
            projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure damage",
                Value = pressureHealthDamage
            });
        }

        if (pressureFocusDamage != 0)
        {
            projection.FocusComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure damage",
                Value = pressureFocusDamage
            });
        }

        if (pressureConfidenceDamage != 0)
        {
            projection.ConfidenceComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure damage",
                Value = pressureConfidenceDamage
            });
        }

        // Apply all explicit tag modifications from the choice
        foreach (TagModification mod in choice.TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState)
            {
                EncounterStateTags tag = (EncounterStateTags)mod.Tag;
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

        // Determine which tags will be active based on new tag values
        List<IEncounterTag> newlyActivatedTags = _tagManager.GetNewlyActivatedTags(clonedTagSystem, _location.AvailableTags);
        List<IEncounterTag> deactivatedTags = _tagManager.GetDeactivatedTags(clonedTagSystem, _location.AvailableTags);

        newlyActivatedTags.ForEach(tag => projection.NewlyActivatedTags.Add(tag.Name));
        deactivatedTags.ForEach(tag => projection.DeactivatedTags.Add(tag.Name));

        // Calculate momentum effects
        int momentumChange = 0;

        if (choice.EffectType == EffectTypes.Momentum)
        {
            // Standard choice builds +2 momentum, special choices +3
            int baseMomentum = choice is SpecialChoice ? 3 : 2;
            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Momentum Choice Base",
                Value = baseMomentum
            });
            momentumChange += baseMomentum;
        }

        // Calculate pressure effects
        int pressureChange = 0;

        if (choice.EffectType == EffectTypes.Pressure)
        {
            // Standard choices REDUCE -1 pressure (project knowledge)
            int basePressureReduction = -1;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure Choice Base",
                Value = basePressureReduction
            });
            pressureChange += basePressureReduction;
        }

        // Environmental pressure happens regardless of choice type
        int environmentalPressure = _location.GetEnvironmentalPressure(currentTurn);
        if (environmentalPressure != 0)
        {
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure From Location",
                Value = environmentalPressure
            });
            pressureChange += environmentalPressure;
        }

        // Apply effects from active tags, excluding newly activated ones
        foreach (IEncounterTag tag in _tagManager.ActiveTags)
        {
            if (newlyActivatedTags.Any(t => t.Name == tag.Name))
                continue;

            if (tag is StrategicTag strategicTag)
            {
                // Check if this tag affects this choice's focus (if specified)
                bool affectsChoice = true;
                if (strategicTag.AffectedFocus.HasValue && choice.Focus != strategicTag.AffectedFocus.Value)
                    affectsChoice = false;

                if (affectsChoice)
                {
                    // Strategic tags scale effects at 1 effect point per 2 approach points
                    int approachValue = 0;
                    if (strategicTag.ScalingApproachTag.HasValue)
                    {
                        approachValue = clonedTagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                    }

                    int scalingFactor = Math.Max(0, approachValue / 2); // 1 effect point per 2 approach points

                    // Apply strategic tag effects based on type
                    switch (strategicTag.EffectType)
                    {
                        case StrategicEffectTypes.IncreaseMomentum:
                            if (scalingFactor > 0)
                            {
                                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                                {
                                    Source = tag.Name,
                                    Value = scalingFactor
                                });
                                momentumChange += scalingFactor;
                            }
                            break;

                        case StrategicEffectTypes.DecreaseMomentum:
                            if (scalingFactor > 0)
                            {
                                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                                {
                                    Source = tag.Name,
                                    Value = -scalingFactor
                                });
                                momentumChange -= scalingFactor;
                            }
                            break;

                        case StrategicEffectTypes.DecreasePressure:
                            if (scalingFactor > 0)
                            {
                                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                                {
                                    Source = tag.Name,
                                    Value = -scalingFactor
                                });
                                pressureChange -= scalingFactor;
                            }
                            break;

                        case StrategicEffectTypes.IncreasePressure:
                            if (scalingFactor > 0)
                            {
                                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                                {
                                    Source = tag.Name,
                                    Value = scalingFactor
                                });
                                pressureChange += scalingFactor;
                            }
                            break;
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
            int minimumAdjustment = -currentPressure - projection.PressureBuilt;

            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Minimum pressure limit",
                Value = minimumAdjustment
            });

            projection.PressureBuilt = -currentPressure;
        }

        // Set final calculated values
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = Math.Max(0, currentPressure + projection.PressureBuilt);

        // Calculate projected turn and check if encounter will end
        int projectedTurn = currentTurn + 1;
        projection.ProjectedTurn = projectedTurn;

        bool encounterEnds = (projectedTurn >= _location.TurnDuration) || (projection.FinalPressure >= EncounterState.MaxPressure);
        projection.EncounterWillEnd = encounterEnds;

        // Determine outcome if encounter ends
        if (encounterEnds)
        {
            if (projection.FinalPressure >= EncounterState.MaxPressure)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
            else
            {
                if (projection.FinalMomentum >= _location.ExceptionalThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Exceptional;
                else if (projection.FinalMomentum >= _location.StandardThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Standard;
                else if (projection.FinalMomentum >= _location.PartialThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Partial;
                else
                    projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
        }

        // Calculate tag-based resource changes, excluding newly activated tags
        int tagHealthChange = _resourceManager.CalculateTagResourceChange(choice, ResourceTypes.Health, currentPressure, newlyActivatedTags);
        int tagFocusChange = _resourceManager.CalculateTagResourceChange(choice, ResourceTypes.Focus, currentPressure, newlyActivatedTags);
        int tagConfidenceChange = _resourceManager.CalculateTagResourceChange(choice, ResourceTypes.Confidence, currentPressure, newlyActivatedTags);

        // Set total resource changes (pressure damage + tag effects)
        projection.HealthChange = pressureHealthDamage + tagHealthChange;
        projection.FocusChange = pressureFocusDamage + tagFocusChange;
        projection.ConfidenceChange = pressureConfidenceDamage + tagConfidenceChange;

        // Add tag-based resource change components
        if (tagHealthChange != 0)
        {
            projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = tagHealthChange
            });
        }

        if (tagFocusChange != 0)
        {
            projection.FocusComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = tagFocusChange
            });
        }

        if (tagConfidenceChange != 0)
        {
            projection.ConfidenceComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = tagConfidenceChange
            });
        }

        return projection;
    }
}