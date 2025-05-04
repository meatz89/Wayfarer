public class ProjectionService
{
    private readonly TagManager _tagManager;
    private readonly ResourceManager _resourceManager;
    private readonly Encounter _encounterInfo;
    private readonly PlayerState _playerState;

    public ProjectionService(
        TagManager tagManager,
        ResourceManager resourceManager,
        Encounter encounterInfo,
        PlayerState playerState)
    {
        _tagManager = tagManager;
        _resourceManager = resourceManager;
        _encounterInfo = encounterInfo;
        _playerState = playerState;
    }

    public ChoiceProjection CreateChoiceProjection(
        CardDefinition choice,
        int currentMomentum,
        int currentPressure,
        int currentTurn)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Create working copy of tag system
        EncounterTagSystem clonedTagSystem = _tagManager.CloneTagSystem();

        // Apply all tag modifications from the card
        ApplyTagModifications(choice, clonedTagSystem, projection);

        // Track momentum and pressure changes
        int momentumChange = 0;
        int pressureChange = 0;

        // 1. Calculate base card effect
        CalculateBaseCardEffect(choice, projection, ref momentumChange, ref pressureChange);

        // 2. Calculate environmental turn pressure
        CalculateEnvironmentalPressure(currentTurn, projection, ref pressureChange);

        // 3. Calculate skill bonuses
        CalculateSkillBonuses(choice, projection, ref momentumChange, ref pressureChange);

        // 4. Calculate environmental strategic effects
        CalculateStrategicTagEffects(choice, projection, clonedTagSystem, ref momentumChange, ref pressureChange);

        // 5. Calculate approach bonuses
        CalculateApproachBonuses(choice, projection, clonedTagSystem, ref momentumChange, ref pressureChange);

        // Ensure values don't go negative
        EnsureNoNegativeValues(currentMomentum, currentPressure, projection, ref momentumChange, ref pressureChange);

        // Set final projection values
        projection.MomentumGained = momentumChange;
        projection.PressureBuilt = pressureChange;
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = currentPressure + pressureChange;

        // Calculate pressure-based resource damage
        CalculateResourceImpacts(projection);

        // Determine if encounter will end
        DetermineEncounterOutcome(projection, currentTurn);

        return projection;
    }

    private void ApplyTagModifications(
        CardDefinition choice,
        EncounterTagSystem tagSystem,
        ChoiceProjection projection)
    {
        foreach (TagModification mod in choice.TagModifications)
        {
            if (mod.EncounterTagType == TagModification.TagTypes.Approach)
            {
                ApproachTags tag = (ApproachTags)mod.TagName;
                int currentValue = tagSystem.GetApproachTagValue(tag);
                tagSystem.SetApproachTagValue(tag, currentValue + 1);
                projection.ApproachTagChanges[tag] = currentValue + 1;
            }
            else if (mod.EncounterTagType == TagModification.TagTypes.Focus)
            {
                FocusTags tag = (FocusTags)mod.TagName;
                int currentValue = tagSystem.GetFocusTagValue(tag);
                tagSystem.SetFocusTagValue(tag, currentValue + 1);
                projection.FocusTagChanges[tag] = currentValue + 1;
            }
        }
    }

    private void CalculateBaseCardEffect(
        CardDefinition choice,
        ChoiceProjection projection,
        ref int momentumChange,
        ref int pressureChange)
    {
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseValue = choice.EffectValue;
            momentumChange += baseValue;
            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = $"Card Base Effect (Tier {choice.Tier})",
                Value = baseValue
            });
        }
        else if (choice.EffectType == EffectTypes.Pressure)
        {
            int baseValue = -choice.EffectValue; // Pressure cards reduce pressure
            pressureChange += baseValue;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = $"Card Base Effect (Tier {choice.Tier})",
                Value = baseValue
            });
        }
    }

    private void CalculateEnvironmentalPressure(
        int currentTurn,
        ChoiceProjection projection,
        ref int pressureChange)
    {
        int environmentalPressure = _encounterInfo.GetEnvironmentalPressure(currentTurn);
        if (environmentalPressure > 0)
        {
            pressureChange += environmentalPressure;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Environmental Pressure",
                Value = environmentalPressure
            });
        }
    }

    private void CalculateSkillBonuses(
        CardDefinition choice,
        ChoiceProjection projection,
        ref int momentumChange,
        ref int pressureChange)
    {
        // Check for skill bonuses that enhance this card
        foreach (SkillRequirement req in choice.UnlockRequirements)
        {
            int skillLevel = _playerState.Skills.GetLevelForSkill(req.SkillType);
            if (skillLevel > req.RequiredLevel)
            {
                int skillBonus = skillLevel - req.RequiredLevel;

                if (choice.EffectType == EffectTypes.Momentum)
                {
                    momentumChange += skillBonus;
                    projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{req.SkillType} Skill Bonus",
                        Value = skillBonus
                    });
                }
                else if (choice.EffectType == EffectTypes.Pressure)
                {
                    pressureChange -= skillBonus;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{req.SkillType} Skill Bonus",
                        Value = -skillBonus
                    });
                }
            }
        }
    }

    private void CalculateStrategicTagEffects(
        CardDefinition choice,
        ChoiceProjection projection,
        EncounterTagSystem tagSystem,
        ref int momentumChange,
        ref int pressureChange)
    {
        if (choice.StrategicEffect == null)
            return;

        List<StrategicTag> strategicTags = _tagManager.GetStrategicActiveTags();

        foreach (StrategicTag tag in strategicTags)
        {
            if (choice.StrategicEffect.IsActive(tag))
            {
                projection.StrategicTagEffects.Add(choice.StrategicEffect.ToString());

                int targetApproachValue = tagSystem.GetApproachTagValue(choice.StrategicEffect.TargetApproach);
                int effectValue = targetApproachValue / 2; // 1 point per 2 approach points

                if (choice.StrategicEffect.EffectType == StrategicTagEffectType.IncreaseMomentum)
                {
                    momentumChange += effectValue;
                    projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.NarrativeName} Strategic Effect ({choice.StrategicEffect.TargetApproach})",
                        Value = effectValue
                    });
                }
                else if (choice.StrategicEffect.EffectType == StrategicTagEffectType.DecreaseMomentum)
                {
                    momentumChange -= effectValue;
                    projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.NarrativeName} Strategic Effect ({choice.StrategicEffect.TargetApproach})",
                        Value = -effectValue
                    });
                }
                else if (choice.StrategicEffect.EffectType == StrategicTagEffectType.IncreasePressure)
                {
                    pressureChange += effectValue;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.NarrativeName} Strategic Effect ({choice.StrategicEffect.TargetApproach})",
                        Value = effectValue
                    });
                }
                else if (choice.StrategicEffect.EffectType == StrategicTagEffectType.DecreasePressure)
                {
                    pressureChange -= effectValue;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.NarrativeName} Strategic Effect ({choice.StrategicEffect.TargetApproach})",
                        Value = -effectValue
                    });
                }
            }
        }
    }

    private void CalculateApproachBonuses(
        CardDefinition choice,
        ChoiceProjection projection,
        EncounterTagSystem tagSystem,
        ref int momentumChange,
        ref int pressureChange)
    {
        // Calculate approach-based bonuses (e.g., archetype advantage)
        int approachValue = tagSystem.GetApproachTagValue(choice.Approach);

        // Check if this approach aligns with player's archetype
        if (IsArchetypeApproach(_playerState.Archetype, choice.Approach) && approachValue >= 3)
        {
            int archetypeBonus = approachValue / 3; // 1 point per 3 approach points

            if (choice.EffectType == EffectTypes.Momentum)
            {
                momentumChange += archetypeBonus;
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = $"Archetype Approach Bonus ({choice.Approach})",
                    Value = archetypeBonus
                });
            }
            else if (choice.EffectType == EffectTypes.Pressure)
            {
                pressureChange -= archetypeBonus;
                projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = $"Archetype Approach Bonus ({choice.Approach})",
                    Value = -archetypeBonus
                });
            }
        }
    }

    private bool IsArchetypeApproach(ArchetypeTypes archetype, ApproachTags approach)
    {
        return (archetype == ArchetypeTypes.Guard && approach == ApproachTags.Dominance) ||
               (archetype == ArchetypeTypes.Diplomat && approach == ApproachTags.Rapport) ||
               (archetype == ArchetypeTypes.Scholar && approach == ApproachTags.Analysis) ||
               (archetype == ArchetypeTypes.Explorer && approach == ApproachTags.Precision) ||
               (archetype == ArchetypeTypes.Rogue && approach == ApproachTags.Concealment);
    }

    private void EnsureNoNegativeValues(
        int currentMomentum,
        int currentPressure,
        ChoiceProjection projection,
        ref int momentumChange,
        ref int pressureChange)
    {
        if (currentMomentum + momentumChange < 0)
        {
            int adjustment = -(currentMomentum + momentumChange);
            momentumChange += adjustment;

            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Minimum Momentum Limit",
                Value = adjustment
            });
        }

        if (currentPressure + pressureChange < 0)
        {
            int adjustment = -(currentPressure + pressureChange);
            pressureChange += adjustment;

            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Minimum Pressure Limit",
                Value = adjustment
            });
        }
    }

    private void CalculateResourceImpacts(ChoiceProjection projection)
    {
        // Calculate resource damage based on pressure
        int resourceImpact = 0;

        if (projection.FinalPressure > _encounterInfo.MaxPressure * 0.75f)
        {
            resourceImpact = -1; // High pressure causes resource damage

            switch (_encounterInfo.EncounterType)
            {
                case EncounterApproaches.Force:
                    projection.HealthChange = resourceImpact;
                    projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = "High Pressure Impact",
                        Value = resourceImpact
                    });
                    break;

                case EncounterApproaches.Observation:
                    projection.ConcentrationChange = resourceImpact;
                    projection.ConcentrationComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = "High Pressure Impact",
                        Value = resourceImpact
                    });
                    break;


                case EncounterApproaches.Persuasion:
                    projection.ConcentrationChange = resourceImpact;
                    projection.ConcentrationComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = "High Pressure Impact",
                        Value = resourceImpact
                    });
                    break;


                case EncounterApproaches.Precision:
                    projection.HealthChange = resourceImpact;
                    projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = "High Pressure Impact",
                        Value = resourceImpact
                    });
                    break;

                case EncounterApproaches.Rapport:
                    projection.ConfidenceChange = resourceImpact;
                    projection.ConfidenceComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = "High Pressure Impact",
                        Value = resourceImpact
                    });
                    break;
            }
        }
    }

    private void DetermineEncounterOutcome(ChoiceProjection projection, int currentTurn)
    {
        int projectedTurn = currentTurn + 1;
        projection.ProjectedTurn = projectedTurn;

        bool encounterEnds =
            projectedTurn >= _encounterInfo.MaxTurns ||
            projection.FinalMomentum >= _encounterInfo.ExceptionalThreshold ||
            projection.FinalPressure >= _encounterInfo.MaxPressure;

        projection.EncounterWillEnd = encounterEnds;

        if (encounterEnds)
        {
            if (projection.FinalPressure >= _encounterInfo.MaxPressure)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
            else if (projection.FinalMomentum >= _encounterInfo.ExceptionalThreshold)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Exceptional;
            }
            else if (projection.FinalMomentum >= _encounterInfo.StandardThreshold)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Standard;
            }
            else if (projection.FinalMomentum >= _encounterInfo.PartialThreshold)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Partial;
            }
            else
            {
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
        }
    }
}