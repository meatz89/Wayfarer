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

    // Momentums bonuses from active strategic tags
    private readonly Dictionary<ApproachTypes, int> _approachMomentumBonuses = new();
    private readonly Dictionary<FocusTags, int> _focusMomentumBonuses = new();
    private int _endOfTurnPressureReduction = 0;

    // Add dictionaries to track pressure modifiers similar to momentum modifiers
    private readonly Dictionary<ApproachTypes, int> _approachPressureModifiers = new();
    private readonly Dictionary<FocusTags, int> _focusPressureModifiers = new();

    public EncounterState(LocationInfo location)
    {
        Momentum = 0;
        Pressure = 0;
        TagSystem = new BaseTagSystem();
        ActiveTags = new List<IEncounterTag>();
        CurrentTurn = 0;
        Location = location;
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

    // Get the total momentum for a choice, considering all active bonuses
    public int GetTotalMomentum(IChoice choice, int baseMomentum)
    {
        int total = baseMomentum;

        // Add approach-specific bonuses
        if (_approachMomentumBonuses.ContainsKey(choice.Approach))
            total += _approachMomentumBonuses[choice.Approach];

        // Add focus-specific bonuses
        if (_focusMomentumBonuses.ContainsKey(choice.Focus))
            total += _focusMomentumBonuses[choice.Focus];

        return total;
    }

    // Process end of turn effects
    public void EndTurn()
    {
        CurrentTurn++;

        // Apply end of turn pressure reduction from strategic tags
        if (_endOfTurnPressureReduction > 0)
            ReducePressure(_endOfTurnPressureReduction);
    }

    // Update which tags are active based on current tag values
    public void UpdateActiveTags(IEnumerable<IEncounterTag> locationTags)
    {
        // Find newly activated tags (not in active, but should be)
        List<IEncounterTag> newlyActivatedTags = new List<IEncounterTag>();
        foreach (IEncounterTag tag in locationTags)
        {
            if (tag.IsActive(TagSystem) && !ActiveTags.Contains(tag))
            {
                newlyActivatedTags.Add(tag);
            }
        }

        // Clear active tags and re-populate
        ActiveTags.Clear();
        ResetTagEffects(); // Clear previous tag effects

        foreach (IEncounterTag tag in locationTags)
        {
            if (tag.IsActive(TagSystem))
            {
                ActiveTags.Add(tag);
                tag.ApplyEffect(this);
            }
        }

        // Apply activation effects for newly activated tags
        foreach (IEncounterTag tag in newlyActivatedTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                strategicTag.ApplyActivationEffect(this);
            }
        }
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

    public ChoiceProjection CreateChoiceProjection(IChoice choice)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Clone current state
        BaseTagSystem clonedTagSystem = TagSystem.Clone();
        int currentMomentum = Momentum;
        int currentPressure = Pressure;
        int currentTurn = CurrentTurn;

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

            // Add location bonuses
            if (Location.FavoredApproaches.Contains(choice.Approach))
            {
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Favored approach",
                    Value = 1
                });
                momentumChange += 1;
            }

            if (Location.FavoredFocuses.Contains(choice.Focus))
            {
                projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
                {
                    Source = "Favored focus",
                    Value = 1
                });
                momentumChange += 1;
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
        if (choice.EffectType == EffectTypes.Pressure || choice is EmergencyChoice)
        {
            int basePressure = 2; // Standard pressure
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Base value",
                Value = basePressure
            });
            pressureChange += basePressure;
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

        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = Math.Max(0, currentPressure + projection.PressureBuilt);

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
