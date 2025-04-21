public class ProjectionService
{
    private readonly TagManager _locationTags;
    private readonly ResourceManager _resourceManager;
    private readonly Encounter encounterInfo;
    private readonly PlayerState playerState;

    public ProjectionService(TagManager tagManager, ResourceManager resourceManager, Encounter encounterInfo, PlayerState playerState)
    {
        _locationTags = tagManager;
        _resourceManager = resourceManager;
        this.encounterInfo = encounterInfo;
        this.playerState = playerState;
    }

    public ChoiceProjection CreateChoiceProjection(
        CardDefinition choice,
        int currentMomentum,
        int currentPressure,
        int currentTurn)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Create working copy of tag system
        EncounterTagSystem clonedTagSystem = _locationTags.CloneTagSystem();

        // Calculate momentum effects
        int momentumChange = 0;

        // Calculate pressure effects
        int pressureChange = 0;

        // Process Approach And Focus Effects
        ProcessMomentumPressureIncrease(choice, currentTurn, projection, ref momentumChange, ref pressureChange);

        // Process Strategic Tag Effects/
        ProcessStrategicTagEffects(choice, projection, clonedTagSystem, ref momentumChange, ref pressureChange);

        // Set projection values
        projection.MomentumGained = momentumChange;
        projection.PressureBuilt = pressureChange;

        // Ensure pressure can't go below 0
        EnsureNoNegativeEncounterValues(currentMomentum, currentPressure, projection);

        // Apply all explicit tag modifications from the choice
        ProcessChoiceTagIncreases(choice, projection, clonedTagSystem, playerState);

        // Calculate pressure-based resource damage that will apply at start of turn
        CalculateDamageFromPressure(choice, projection, playerState, currentPressure, projection.PressureBuilt);

        // Set final calculated values
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = Math.Max(0, currentPressure + projection.PressureBuilt);

        // Calculate projected turn and check if encounter will end
        int projectedTurn = currentTurn + 1;
        projection.ProjectedTurn = projectedTurn;

        DetermineEncounterEnd(projection, projectedTurn);

        return projection;
    }

    private static void ProcessChoiceTagIncreases(CardDefinition choice, ChoiceProjection projection, EncounterTagSystem clonedTagSystem, PlayerState playerState)
    {
        foreach (TagModification mod in choice.TagModifications)
        {
            if (mod.EncounterTagType == TagModification.TagTypes.Approach)
            {
                ApproachTags tag = (ApproachTags)mod.TagName;
                int newValue = clonedTagSystem.GetApproachTagValue(tag);
                projection.ApproachTagChanges[tag] = newValue;
            }
            else if (mod.EncounterTagType == TagModification.TagTypes.Focus)
            {
                FocusTags tag = (FocusTags)mod.TagName;
                int newValue = clonedTagSystem.GetFocusTagValue(tag);
                projection.FocusTagChanges[tag] = newValue;
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

    private void ProcessStrategicTagEffects(CardDefinition choice, ChoiceProjection projection, EncounterTagSystem baseTagSystem, ref int momentumChange, ref int pressureChange)
    {
        EnvironmentalPropertyEffect effect = choice.StrategicEffect;
        List<StrategicTag> strategicTags = _locationTags.GetStrategicActiveTags();

        foreach (StrategicTag tag in strategicTags)
        {
            if (effect == null || !effect.IsActive(tag))
            {
                continue;
            }
            projection.StrategicTagEffects.Add($"{effect.ToString()}");

            int momentumEffect = effect.GetMomentumModifierForTag(tag, baseTagSystem);
            if (momentumEffect != 0)
            {
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = $"{tag.NarrativeName} ({tag.EnvironmentalProperty.GetPropertyType()})",
                    Value = momentumEffect
                });
                momentumChange += momentumEffect;
            }

            int pressureEffect = effect.GetPressureModifierForTag(tag, baseTagSystem);
            if (pressureEffect != 0)
            {
                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = $"{tag.NarrativeName} ({tag.EnvironmentalProperty.GetPropertyType()})",
                    Value = pressureEffect
                });
                pressureChange += pressureEffect;
            }
        }
    }

    private void CalculateDamageFromPressure(CardDefinition choice, ChoiceProjection projection, PlayerState playerState, int currentPressure, int choicePressure)
    {
        int injuryEffect = 0;
        injuryEffect = -currentPressure;

        if (injuryEffect != 0)
        {
            EncounterTypes encounterType = encounterInfo.EncounterType;
            if (encounterType == EncounterTypes.Physical)
            {
                projection.HealthChange += injuryEffect;
                projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Dangerous Archetype choice",
                    Value = injuryEffect
                });
            }
            else if (encounterType == EncounterTypes.Intellectual)
            {
                projection.ConcentrationChange += injuryEffect;
                projection.ConcentrationComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Dangerous Archetype choice",
                    Value = injuryEffect
                });
            }
            else if (encounterType == EncounterTypes.Social)
            {
                projection.ConfidenceChange += injuryEffect;
                projection.ConfidenceComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Dangerous Archetype choice",
                    Value = injuryEffect
                });
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
    private void ProcessMomentumPressureIncrease(
        CardDefinition choice,
        int currentTurn,
        ChoiceProjection projection,
        ref int momentumChange,
        ref int pressureChange)
    {
        // Momentum Choice
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = choice.EffectValue;
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
            int basePressure = -choice.EffectValue;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = $"Pressure Reduction Choice (Tier {(int)choice.Tier})",
                Value = basePressure
            });
            pressureChange += basePressure;

        }
    }

    private void DetermineEncounterEnd(
        ChoiceProjection projection,
        int projectedTurn)
    {
        bool encounterEnds =
            (projectedTurn >= encounterInfo.MaxTurns)
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
    }
}