public class ProjectionService
{
    private readonly TagManager _tagManager;
    private readonly ResourceManager _resourceManager;
    private readonly LocationEncounterInfo encounterInfo;

    public ProjectionService(TagManager tagManager, ResourceManager resourceManager, LocationEncounterInfo location)
    {
        _tagManager = tagManager;
        _resourceManager = resourceManager;
        encounterInfo = location;
    }

    public ChoiceProjection CreateChoiceProjection(IChoice choice, int currentMomentum, int currentPressure, int currentTurn, int escalationLevel)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Create working copy of tag system
        BaseTagSystem clonedTagSystem = _tagManager.CloneTagSystem();

        // Calculate pressure-based resource damage that will apply at start of turn
        int pressureHealthDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Health, currentPressure);
        int pressureFocusDamage = _resourceManager.CalculatePressureResourceDamage(ResourceTypes.Concentration, currentPressure);
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

        // Determine which tags will be active based on new tag values
        List<IEncounterTag> newlyActivatedTags = _tagManager.GetNewlyActivatedTags(clonedTagSystem, encounterInfo.AvailableTags);
        List<IEncounterTag> deactivatedTags = _tagManager.GetDeactivatedTags(clonedTagSystem, encounterInfo.AvailableTags);

        newlyActivatedTags.ForEach(tag => projection.NewlyActivatedTags.Add(tag.Name));
        deactivatedTags.ForEach(tag => projection.DeactivatedTags.Add(tag.Name));

        // Calculate momentum effects
        int momentumChange = 0;

        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = choice is SpecialChoice ? 3 : (2);
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
            int basePressure = 1;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure Choice Base",
                Value = basePressure
            });
            pressureChange += basePressure;
        }

        if (choice.EffectType == EffectTypes.Pressure && escalationLevel > 0)
        {
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Escalation Pressure",
                Value = escalationLevel
            });
            pressureChange += escalationLevel;
        }

        int environmentalPressure = encounterInfo.GetEnvironmentalPressure(currentTurn);
        if (environmentalPressure > 0)
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
                bool affectsChoice = true;

                if (choice.Approach != strategicTag.AffectedApproach)
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

        bool encounterEnds = (projectedTurn >= encounterInfo.TurnDuration) || (projection.FinalPressure >= EncounterState.MaxPressure);
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

        // Set total resource changes (pressure damage + tag effects)
        projection.HealthChange = pressureHealthDamage;
        projection.FocusChange = pressureFocusDamage;
        projection.ConfidenceChange = pressureConfidenceDamage;

        return projection;
    }
}