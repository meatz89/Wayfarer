using BlazorRPG.Game.EncounterManager;

/// <summary>
/// Represents the current state of an encounter
/// </summary>
public class EncounterState
{
    public int Momentum { get; private set; }
    public int Pressure { get; private set; }
    public BaseTagSystem TagSystem { get; }
    public List<IEncounterTag> ActiveTags { get; }
    public int CurrentTurn { get; private set; }
    public LocationInfo Location { get; }
    public LocationSpot LocationSpot { get; internal set; }

    public const int MaxPressure = 10;

    private int _escalationLevel = 0;

    // Momentums bonuses from active strategic tags
    private readonly Dictionary<ApproachTypes, int> _approachMomentumBonuses = new();
    private readonly Dictionary<FocusTags, int> _focusMomentumBonuses = new();
    private int _endOfTurnPressureReduction = 0;

    // Add dictionaries to track pressure modifiers similar to momentum modifiers
    private readonly Dictionary<ApproachTypes, int> _approachPressureModifiers = new();
    private readonly Dictionary<FocusTags, int> _focusPressureModifiers = new();

    // Add a list to track disabled tags
    private List<string> _disabledTagNames = new List<string>();

    public EncounterState(LocationInfo location)
    {
        Momentum = 0;
        Pressure = 0;
        TagSystem = new BaseTagSystem();
        ActiveTags = new List<IEncounterTag>();
        CurrentTurn = 0;
        Location = location;
    }


    // Check if a tag is disabled by high pressure
    public bool IsTagDisabled(string tagName)
    {
        return _disabledTagNames.Contains(tagName);
    }

    // Get all tags disabled by pressure
    public List<string> GetDisabledTagNames()
    {
        return _disabledTagNames.ToList(); // Return a copy to prevent external modifications
    }

    // Add this method to update which tags are disabled based on pressure
    private void UpdateDisabledTags()
    {
        _disabledTagNames.Clear();

        // At pressure 6+, disable minor beneficial tags
        if (Pressure >= 6)
        {
            foreach (IEncounterTag tag in Location.AvailableTags)
            {
                if (tag is StrategicTag strategicTag &&
                    IsPositiveEffect(strategicTag.EffectType) &&
                    strategicTag.EffectValue == 1 &&
                    tag.IsActive(TagSystem))
                {
                    _disabledTagNames.Add(tag.Name);
                }
            }
        }

        // At pressure 8+, disable all beneficial tags
        if (Pressure >= 8)
        {
            foreach (IEncounterTag tag in Location.AvailableTags)
            {
                if (tag is StrategicTag strategicTag &&
                    IsPositiveEffect(strategicTag.EffectType) &&
                    tag.IsActive(TagSystem) &&
                    !_disabledTagNames.Contains(tag.Name))
                {
                    _disabledTagNames.Add(tag.Name);
                }
            }
        }
    }

    // Determine if an effect type is positive (and can be disabled)
    private bool IsPositiveEffect(StrategicEffectTypes effectType)
    {
        switch (effectType)
        {
            case StrategicEffectTypes.AddMomentumToApproach:
            case StrategicEffectTypes.AddMomentumToFocus:
            case StrategicEffectTypes.ReducePressureFromApproach:
            case StrategicEffectTypes.ReducePressureFromFocus:
            case StrategicEffectTypes.ReducePressurePerTurn:
            case StrategicEffectTypes.AddMomentumPerTurn:
            case StrategicEffectTypes.AddMomentumOnActivation:
            case StrategicEffectTypes.ReducePressureOnActivation:
                return true;
            default:
                return false;
        }
    }

    // Update which tags are active based on current tag values
    public void UpdateActiveTags(IEnumerable<IEncounterTag> locationTags)
    {
        // First update disabled tags based on current pressure
        UpdateDisabledTags();

        // Clear active tags
        ActiveTags.Clear();
        ResetTagEffects();

        // Re-activate relevant tags that aren't disabled
        foreach (IEncounterTag tag in locationTags)
        {
            if (tag.IsActive(TagSystem) && !IsTagDisabled(tag.Name))
            {
                ActiveTags.Add(tag);
                tag.ApplyEffect(this);
            }
        }
    }

    // Add momentum penalty based on pressure
    public int GetMomentumPenaltyFromPressure()
    {
        // No penalty below 3 pressure
        if (Pressure < 3)
            return 0;

        // At 3-4 pressure: -1 momentum
        // At 5-6 pressure: -2 momentum  
        // At 7-8 pressure: -3 momentum
        // At 9+ pressure: -4 momentum
        return -(Pressure / 2);
    }


    // Update GetTotalMomentum to apply pressure penalties
    public int GetTotalMomentum(IChoice choice, int baseMomentum)
    {
        int total = baseMomentum;

        // Add approach/focus bonuses
        if (_approachMomentumBonuses.ContainsKey(choice.Approach))
            total += _approachMomentumBonuses[choice.Approach];

        if (_focusMomentumBonuses.ContainsKey(choice.Focus))
            total += _focusMomentumBonuses[choice.Focus];

        // Apply pressure penalty (only for momentum choices)
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int pressurePenalty = GetMomentumPenaltyFromPressure();
            total += pressurePenalty; // Add penalty (negative number)

            // Ensure at least 1 momentum is gained
            total = Math.Max(1, total);
        }

        return total;
    }

    // Build momentum (only increases, never decreases)
    public void BuildMomentum(int amount)
    {
        Momentum += amount;
    }

    // Build pressure (only increases through choices)
    public void BuildPressure(int amount)
    {
        Pressure += amount;
    }

    // Reduce pressure (only happens through strategic tags)
    public void ReducePressure(int amount)
    {
        Pressure = Math.Max(0, Pressure - amount);
    }

    // Add approach pressure modifier methods
    public void AddApproachPressureModifier(ApproachTypes approach, int modifier)
    {
        if (!_approachPressureModifiers.ContainsKey(approach))
            _approachPressureModifiers[approach] = 0;

        _approachPressureModifiers[approach] += modifier;
    }

    // Add focus pressure modifier methods
    public void AddFocusPressureModifier(FocusTags focus, int modifier)
    {
        if (!_focusPressureModifiers.ContainsKey(focus))
            _focusPressureModifiers[focus] = 0;

        _focusPressureModifiers[focus] += modifier;
    }

    // Get the total pressure for a choice, considering all active modifiers
    public int GetTotalPressure(IChoice choice, int basePressure)
    {
        int total = basePressure;

        // Add approach-specific modifiers
        if (_approachPressureModifiers.ContainsKey(choice.Approach))
            total += _approachPressureModifiers[choice.Approach];

        // Add focus-specific modifiers
        if (_focusPressureModifiers.ContainsKey(choice.Focus))
            total += _focusPressureModifiers[choice.Focus];

        return Math.Max(0, total); // Pressure can't be negative
    }

    // Reset tag effects (update existing method)
    public void ResetTagEffects()
    {
        // Reset existing effects
        foreach (ApproachTypes approach in Enum.GetValues(typeof(ApproachTypes)))
            _approachMomentumBonuses[approach] = 0;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusMomentumBonuses[focus] = 0;

        _endOfTurnPressureReduction = 0;

        // Reset pressure modifiers
        foreach (ApproachTypes approach in Enum.GetValues(typeof(ApproachTypes)))
            _approachPressureModifiers[approach] = 0;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusPressureModifiers[focus] = 0;
    }

    // Add a momentum bonus for a specific approach
    public void AddApproachMomentumBonus(ApproachTypes approach, int bonus)
    {
        if (!_approachMomentumBonuses.ContainsKey(approach))
            _approachMomentumBonuses[approach] = 0;

        _approachMomentumBonuses[approach] += bonus;
    }

    // Add a momentum bonus for a specific focus
    public void AddFocusMomentumBonus(FocusTags focus, int bonus)
    {
        if (!_focusMomentumBonuses.ContainsKey(focus))
            _focusMomentumBonuses[focus] = 0;

        _focusMomentumBonuses[focus] += bonus;
    }

    // Add an end-of-turn pressure reduction
    public void AddEndOfTurnPressureReduction(int reduction)
    {
        _endOfTurnPressureReduction += reduction;
    }


    // Process end of turn effects
    public void EndTurn()
    {
        CurrentTurn++;

        _escalationLevel = Math.Min(3, (CurrentTurn - 1) / 2); // Caps at level 3 after 6 turns

        // Apply end of turn pressure reduction from strategic tags
        if (_endOfTurnPressureReduction > 0)
            ReducePressure(_endOfTurnPressureReduction);
    }

    // Handle the activation effect values
    public void ApplyTagActivationEffects(List<IEncounterTag> newlyActivatedTags)
    {
        foreach (IEncounterTag tag in newlyActivatedTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                strategicTag.ApplyActivationEffect(this);
            }
        }
    }

    // Check if encounter is over (pressure or turn limit)
    public bool IsEncounterOver()
    {
        return Pressure >= MaxPressure || CurrentTurn >= Location.Duration;
    }

    // Get the encounter outcome based on momentum
    public EncounterOutcomes GetOutcome()
    {
        if (Pressure >= MaxPressure || Momentum < Location.PartialThreshold)
            return EncounterOutcomes.Failure;
        if (Momentum < Location.StandardThreshold)
            return EncounterOutcomes.Partial;
        if (Momentum < Location.ExceptionalThreshold)
            return EncounterOutcomes.Exceptional;

        return EncounterOutcomes.Standard;
    }
    
    public void ApplyChoiceProjection(ChoiceProjection projection)
    {
        // 1. Record pre-change state
        int initialMomentum = Momentum;
        int initialPressure = Pressure;

        // 2. Apply tag changes first
        foreach (KeyValuePair<ApproachTags, int> pair in projection.ApproachTagChanges)
            TagSystem.ModifyApproachTag(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in projection.FocusTagChanges)
            TagSystem.ModifyFocusTag(pair.Key, pair.Value);

        // 3. Reset previous tag effects and update active tags
        ResetTagEffects();
        UpdateActiveTags(Location.AvailableTags);

        // 4. Apply exactly the values that were projected
        Momentum = projection.FinalMomentum;
        Pressure = projection.FinalPressure;

        // 5. Increment turn counter
        CurrentTurn++;
    }

    // Apply end-of-turn effects
    public void ApplyEndOfTurnEffects()
    {
        // Reduce pressure (if any)
        if (_endOfTurnPressureReduction > 0)
        {
            Pressure = Math.Max(0, Pressure - _endOfTurnPressureReduction);
        }

        // Other end-of-turn effects would go here
    }

    public List<IEncounterTag> GetTagsDisabledByPressure()
    {
        List<IEncounterTag> disabledTags = new List<IEncounterTag>();

        // At pressure 6+, minor beneficial tags become disabled
        if (Pressure >= 6)
        {
            foreach (IEncounterTag tag in ActiveTags)
            {
                if (tag is StrategicTag strategicTag &&
                    IsPositiveEffect(strategicTag.EffectType) &&
                    strategicTag.EffectValue == 1) // Minor tags typically have value 1
                {
                    disabledTags.Add(tag);
                }
            }
        }

        // At pressure 8+, all beneficial tags become disabled
        if (Pressure >= 8)
        {
            foreach (IEncounterTag tag in ActiveTags)
            {
                if (tag is StrategicTag strategicTag &&
                    IsPositiveEffect(strategicTag.EffectType) &&
                    !disabledTags.Contains(tag))
                {
                    disabledTags.Add(tag);
                }
            }
        }

        return disabledTags;
    }

    public ChoiceProjection CreateChoiceProjection(IChoice choice)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Clone current state
        BaseTagSystem clonedTagSystem = TagSystem.Clone();
        int currentMomentum = Momentum;
        int currentPressure = Pressure;
        int currentTurn = CurrentTurn;

        // Add the current disabled tag names to the projection
        foreach (string disabledTag in GetDisabledTagNames())
        {
            projection.DisabledTagNames.Add(disabledTag);
        }

        // Apply tag modifications to cloned system and track changes
        foreach (TagModification mod in choice.TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.Approach)
            {
                ApproachTags tag = (ApproachTags)mod.Tag;
                int oldValue = clonedTagSystem.GetApproachTagValue(tag);
                clonedTagSystem.ModifyApproachTag(tag, mod.Delta);
                int newValue = clonedTagSystem.GetApproachTagValue(tag);
                int actualDelta = newValue - oldValue;

                if (actualDelta != 0)
                    projection.ApproachTagChanges[tag] = actualDelta;
            }
            else
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

        // Check for newly activated and deactivated tags
        List<IEncounterTag> newlyActivatedTags = new List<IEncounterTag>();
        List<IEncounterTag> deactivatedTags = new List<IEncounterTag>();

        foreach (IEncounterTag tag in Location.AvailableTags)
        {
            bool wasActive = tag.IsActive(TagSystem);
            bool willBeActive = tag.IsActive(clonedTagSystem);

            if (!wasActive && willBeActive)
            {
                newlyActivatedTags.Add(tag);
                projection.NewlyActivatedTags.Add(tag.Name);
            }
            else if (wasActive && !willBeActive)
            {
                deactivatedTags.Add(tag);
                projection.DeactivatedTags.Add(tag.Name);
            }
        }

        // Calculate momentum effects
        int momentumChange = 0;

        // Base momentum for momentum choices
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = choice is SpecialChoice ? 3 : (choice is EmergencyChoice ? 1 : 2);
            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Base value",
                Value = baseMomentum
            });
            momentumChange += baseMomentum;

            // Add pressure penalty for momentum choices
            int pressurePenalty = GetMomentumPenaltyFromPressure();
            if (pressurePenalty < 0)
            {
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "High pressure penalty",
                    Value = pressurePenalty
                });
                momentumChange += pressurePenalty;
            }

            // Ensure at least 1 momentum gain
            if (momentumChange < 1)
            {
                int minimumBonus = 1 - momentumChange;
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Minimum momentum guarantee",
                    Value = minimumBonus
                });
                momentumChange = 1;
            }
        }
        // For pressure choices, momentum is still tracked but starts at 0
        else
        {
            // Some tags might give momentum even on pressure choices
            // So we start with 0 but still calculate potential bonuses
            momentumChange = 0;
        }

        // Calculate pressure effects
        int pressureChange = 0;

        // Base pressure for pressure choices or emergency choices
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int basePressure = 1; // Standard pressure
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Base value",
                Value = basePressure
            });
            pressureChange += basePressure;
        }

        if (choice.EffectType == EffectTypes.Pressure || choice is EmergencyChoice)
        {
            int basePressure = 3; // Standard pressure
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Base value",
                Value = basePressure
            });
            pressureChange += basePressure;
        }

        if (choice.EffectType == EffectTypes.Pressure && _escalationLevel > 0)
        {
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Escalation",
                Value = _escalationLevel
            });
            pressureChange += _escalationLevel;
        }

        int environmentalPressure = Location.GetEnvironmentalPressure(CurrentTurn);
        if (environmentalPressure > 0)
        {
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Environmental pressure",
                Value = environmentalPressure
            });
            pressureChange += environmentalPressure;
        }

        // Now apply tag effects to BOTH momentum and pressure for ALL choices

        // Active tag bonuses
        foreach (IEncounterTag tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                // Always check momentum effects regardless of choice type
                int momentumEffect = strategicTag.GetMomentumModifierForChoice(choice);
                if (momentumEffect != 0)
                {
                    projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = tag.Name,
                        Value = momentumEffect
                    });
                    momentumChange += momentumEffect;
                }

                // Always check pressure effects regardless of choice type
                int pressureEffect = strategicTag.GetPressureModifierForChoice(choice);
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

        // Handle newly activated tags
        foreach (IEncounterTag tag in newlyActivatedTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                // Check for immediate activation effects - you may need to add these enum values
                // and implement the corresponding logic in StrategicTag
                if (strategicTag.EffectType == StrategicEffectTypes.AddMomentumOnActivation)
                {
                    int activationMomentum = strategicTag.EffectValue;
                    projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name} (activation)",
                        Value = activationMomentum
                    });
                    momentumChange += activationMomentum;
                }

                if (strategicTag.EffectType == StrategicEffectTypes.ReducePressureOnActivation)
                {
                    int activationPressure = -strategicTag.EffectValue;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name} (activation)",
                        Value = activationPressure
                    });
                    pressureChange += activationPressure;
                }
            }
        }

        // End of turn pressure reduction from current and newly activated tags
        int endOfTurnPressureChange = 0;

        // First check existing active tags
        foreach (IEncounterTag tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                if (strategicTag.EffectType == StrategicEffectTypes.ReducePressurePerTurn)
                {
                    int reduction = -strategicTag.EffectValue;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name} (end of turn)",
                        Value = reduction
                    });
                    endOfTurnPressureChange += reduction;
                }
                else if (strategicTag.EffectType == StrategicEffectTypes.AddPressurePerTurn)
                {
                    int increase = strategicTag.EffectValue;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name} (end of turn)",
                        Value = increase
                    });
                    endOfTurnPressureChange += increase;
                }
            }
        }

        // Then check newly activated tags
        foreach (IEncounterTag tag in newlyActivatedTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                if (strategicTag.EffectType == StrategicEffectTypes.ReducePressurePerTurn)
                {
                    int reduction = -strategicTag.EffectValue;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name} (new, end of turn)",
                        Value = reduction
                    });
                    endOfTurnPressureChange += reduction;
                }
                else if (strategicTag.EffectType == StrategicEffectTypes.AddPressurePerTurn)
                {
                    int increase = strategicTag.EffectValue;
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name} (new, end of turn)",
                        Value = increase
                    });
                    endOfTurnPressureChange += increase;
                }
            }
        }

        // Set final values
        projection.MomentumGained = momentumChange;
        projection.PressureBuilt = pressureChange + endOfTurnPressureChange;

        // Ensure pressure can't go below 0
        if (currentPressure + projection.PressureBuilt < 0)
        {
            // Add a component to show clamping
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

        // Check if projected pressure would disable tags
        int projectedPressure = currentPressure + projection.PressureBuilt;

        // At pressure 6+, check for minor tags that would be disabled
        if (projectedPressure >= 6)
        {
            foreach (IEncounterTag tag in Location.AvailableTags)
            {
                if (tag is StrategicTag strategicTag &&
                    IsPositiveEffect(strategicTag.EffectType) &&
                    strategicTag.EffectValue == 1 &&
                    tag.IsActive(clonedTagSystem) &&
                    !projection.DisabledTagNames.Contains(tag.Name))
                {
                    projection.DisabledTagNames.Add(tag.Name);
                    projection.NewlyDisabledTags.Add(tag.Name);
                }
            }
        }

        // At pressure 8+, check for all beneficial tags that would be disabled
        if (projectedPressure >= 8)
        {
            foreach (IEncounterTag tag in Location.AvailableTags)
            {
                if (tag is StrategicTag strategicTag &&
                    IsPositiveEffect(strategicTag.EffectType) &&
                    tag.IsActive(clonedTagSystem) &&
                    !projection.DisabledTagNames.Contains(tag.Name))
                {
                    projection.DisabledTagNames.Add(tag.Name);
                    projection.NewlyDisabledTags.Add(tag.Name);
                }
            }
        }

        projection.MomentumGained = momentumChange;
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = Math.Max(0, projectedPressure);

        // Project end of turn effects
        currentTurn++;
        projection.ProjectedTurn = currentTurn;

        // Check if encounter will end
        bool encounterEnds = (currentTurn >= Location.Duration) || (projection.FinalPressure >= MaxPressure);
        projection.EncounterWillEnd = encounterEnds;

        // Determine outcome if encounter ends
        if (encounterEnds)
        {
            if (projection.FinalPressure >= MaxPressure)
            {
                projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
            else
            {
                // Determine outcome based on momentum thresholds
                if (projection.FinalMomentum >= Location.ExceptionalThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Exceptional;
                else if (projection.FinalMomentum >= Location.StandardThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Standard;
                else if (projection.FinalMomentum >= Location.PartialThreshold)
                    projection.ProjectedOutcome = EncounterOutcomes.Partial;
                else
                    projection.ProjectedOutcome = EncounterOutcomes.Failure;
            }
        }

        return projection;
    }
}
