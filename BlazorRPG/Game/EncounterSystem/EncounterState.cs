using BlazorRPG.Game.EncounterManager;
using static BlazorRPG.Game.EncounterManager.TagModification;

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

    public const int MaxPressure = 30;

    private int _escalationLevel = 0;

    // Momentums bonuses from active strategic tags
    private readonly Dictionary<ApproachTags, int> _approachMomentumBonuses = new();
    private readonly Dictionary<FocusTags, int> _focusMomentumBonuses = new();
    private int _endOfTurnPressureReduction = 0;

    // Pressure modifiers
    private readonly Dictionary<ApproachTags, int> _approachPressureModifiers = new();
    private readonly Dictionary<FocusTags, int> _focusPressureModifiers = new();

    // Track the last choice for effect application
    private IChoice lastChoice;

    public PlayerState PlayerState { get; }

    public EncounterState(LocationInfo location, PlayerState playerState)
    {
        Momentum = 0;
        Pressure = 0;
        TagSystem = new BaseTagSystem();
        ActiveTags = new List<IEncounterTag>();
        CurrentTurn = 0;
        Location = location;
        PlayerState = playerState;
    }

    // Apply persistent resource effects based on strategic tags
    public void ApplyPersistentResourceEffects()
    {
        if (lastChoice == null) return;

        foreach (var tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                // Skip if this tag doesn't affect the current choice
                if (strategicTag.AffectedApproach.HasValue &&
                    lastChoice.Approach != strategicTag.AffectedApproach.Value)
                    continue;

                // Skip if this tag doesn't affect the choice's focus
                if (strategicTag.AffectedFocus.HasValue &&
                    lastChoice.Focus != strategicTag.AffectedFocus.Value)
                    continue;

                int effectValue = strategicTag.GetEffectValueForState(this);

                switch (strategicTag.EffectType)
                {
                    case StrategicEffectTypes.ReduceHealthByPressure:
                        PlayerState.ModifyHealth(-Pressure);
                        break;

                    case StrategicEffectTypes.ReduceConcentrationByPressure:
                        PlayerState.ModifyConcentration(-Pressure);
                        break;

                    case StrategicEffectTypes.ReduceReputationByPressure:
                        PlayerState.ModifyReputation(-Pressure);
                        break;

                    case StrategicEffectTypes.ReduceHealthByApproachValue:
                        if (strategicTag.ScalingApproachTag.HasValue)
                        {
                            int value = TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                            PlayerState.ModifyHealth(-value);
                        }
                        break;

                    case StrategicEffectTypes.ReduceConcentrationByApproachValue:
                        if (strategicTag.ScalingApproachTag.HasValue)
                        {
                            int value = TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                            PlayerState.ModifyConcentration(-value);
                        }
                        break;

                    case StrategicEffectTypes.ReduceReputationByApproachValue:
                        if (strategicTag.ScalingApproachTag.HasValue)
                        {
                            int value = TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                            PlayerState.ModifyReputation(-value);
                        }
                        break;
                }
            }
        }
    }

    public void ApplyChoiceProjection(ChoiceProjection projection)
    {
        // Store the choice for resource effects
        lastChoice = projection.Choice;

        // 1. Apply tag changes first
        foreach (KeyValuePair<EncounterStateTags, int> pair in projection.EncounterStateTagChanges)
            TagSystem.ModifyEncounterStateTag(pair.Key, pair.Value);

        foreach (KeyValuePair<ApproachTags, int> pair in projection.ApproachTagChanges)
            TagSystem.ModifyApproachTag(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in projection.FocusTagChanges)
            TagSystem.ModifyFocusTag(pair.Key, pair.Value);

        // 2. Reset previous tag effects and update active tags
        ResetTagEffects();
        UpdateActiveTags(Location.AvailableTags);

        // 3. Apply exactly the values that were projected
        Momentum = projection.FinalMomentum;
        Pressure = projection.FinalPressure;

        // 4. Increment turn counter
        CurrentTurn++;

        // 5. Apply persistent resource effects
        ApplyPersistentResourceEffects();
    }

    // Calculate resource changes for a choice
    public int CalculateResourceChange(IChoice choice, ResourceTypes resourceType)
    {
        int change = 0;

        foreach (var tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                // Skip if this tag doesn't affect the choice's approach
                if (strategicTag.AffectedApproach.HasValue &&
                    choice.Approach != strategicTag.AffectedApproach.Value)
                    continue;

                // Skip if this tag doesn't affect the choice's focus
                if (strategicTag.AffectedFocus.HasValue &&
                    choice.Focus != strategicTag.AffectedFocus.Value)
                    continue;

                switch (resourceType)
                {
                    case ResourceTypes.Health:
                        if (strategicTag.EffectType == StrategicEffectTypes.ReduceHealthByPressure)
                        {
                            change -= Pressure;
                        }
                        else if (strategicTag.EffectType == StrategicEffectTypes.ReduceHealthByApproachValue &&
                                 strategicTag.ScalingApproachTag.HasValue)
                        {
                            change -= TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                        }
                        break;

                    case ResourceTypes.Focus:
                        if (strategicTag.EffectType == StrategicEffectTypes.ReduceConcentrationByPressure)
                        {
                            change -= Pressure;
                        }
                        else if (strategicTag.EffectType == StrategicEffectTypes.ReduceConcentrationByApproachValue &&
                                 strategicTag.ScalingApproachTag.HasValue)
                        {
                            change -= TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                        }
                        break;

                    case ResourceTypes.Confidence:
                        if (strategicTag.EffectType == StrategicEffectTypes.ReduceReputationByPressure)
                        {
                            change -= Pressure;
                        }
                        else if (strategicTag.EffectType == StrategicEffectTypes.ReduceReputationByApproachValue &&
                                 strategicTag.ScalingApproachTag.HasValue)
                        {
                            change -= TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                        }
                        break;
                }
            }
        }

        return change;
    }

    public void UpdateActiveTags(IEnumerable<IEncounterTag> locationTags)
    {
        // Clear active tags
        ActiveTags.Clear();
        ResetTagEffects();

        foreach (IEncounterTag tag in locationTags)
        {
            // Strategic tags are always active
            if (tag is StrategicTag strategicTag)
            {
                ActiveTags.Add(tag);
                tag.ApplyEffect(this);
            }
            // Only narrative tags have activation conditions
            else if (tag is NarrativeTag narrativeTag && tag.IsActive(TagSystem))
            {
                ActiveTags.Add(tag);
                tag.ApplyEffect(this);
            }
        }
    }

    // Get total momentum for a choice, considering all modifiers
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

    // Get total pressure for a choice, considering all modifiers
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

    // Add approach momentum bonus
    public void AddApproachMomentumBonus(ApproachTags approach, int bonus)
    {
        if (!_approachMomentumBonuses.ContainsKey(approach))
            _approachMomentumBonuses[approach] = 0;

        _approachMomentumBonuses[approach] += bonus;
    }

    // Add focus momentum bonus
    public void AddFocusMomentumBonus(FocusTags focus, int bonus)
    {
        if (!_focusMomentumBonuses.ContainsKey(focus))
            _focusMomentumBonuses[focus] = 0;

        _focusMomentumBonuses[focus] += bonus;
    }

    // Add approach pressure modifier
    public void AddApproachPressureModifier(ApproachTags approach, int modifier)
    {
        if (!_approachPressureModifiers.ContainsKey(approach))
            _approachPressureModifiers[approach] = 0;

        _approachPressureModifiers[approach] += modifier;
    }

    // Add focus pressure modifier
    public void AddFocusPressureModifier(FocusTags focus, int modifier)
    {
        if (!_focusPressureModifiers.ContainsKey(focus))
            _focusPressureModifiers[focus] = 0;

        _focusPressureModifiers[focus] += modifier;
    }

    // Add an end-of-turn pressure reduction
    public void AddEndOfTurnPressureReduction(int reduction)
    {
        _endOfTurnPressureReduction += reduction;
    }

    // Reset all tag effects
    public void ResetTagEffects()
    {
        // Reset momentum bonuses
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
            _approachMomentumBonuses[approach] = 0;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusMomentumBonuses[focus] = 0;

        // Reset pressure modifiers
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
            _approachPressureModifiers[approach] = 0;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusPressureModifiers[focus] = 0;

        _endOfTurnPressureReduction = 0;
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

    // Check if encounter is over
    public bool IsEncounterOver()
    {
        return Pressure >= MaxPressure || CurrentTurn >= Location.TurnDuration;
    }

    // Get the encounter outcome
    public EncounterOutcomes GetOutcome()
    {
        if (Pressure >= MaxPressure || Momentum < Location.PartialThreshold)
            return EncounterOutcomes.Failure;
        if (Momentum < Location.StandardThreshold)
            return EncounterOutcomes.Partial;
        if (Momentum < Location.ExceptionalThreshold)
            return EncounterOutcomes.Standard;

        return EncounterOutcomes.Exceptional;
    }

    // Create a projection of what would happen if a choice was selected
    public ChoiceProjection CreateChoiceProjection(IChoice choice)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Clone current state
        BaseTagSystem clonedTagSystem = TagSystem.Clone();
        int currentMomentum = Momentum;
        int currentPressure = Pressure;
        int currentTurn = CurrentTurn;

        TagModification approachTagMod = ForApproach(choice.Approach, 1);
        ApproachTags tagApproach = (ApproachTags)approachTagMod.Tag;
        int oldValueApproach = clonedTagSystem.GetApproachTagValue(tagApproach);
        clonedTagSystem.ModifyApproachTag(tagApproach, approachTagMod.Delta);
        int newValueApproach = clonedTagSystem.GetApproachTagValue(tagApproach);
        int actualDeltaApproach = newValueApproach - oldValueApproach;
        if (actualDeltaApproach != 0) projection.ApproachTagChanges[tagApproach] = actualDeltaApproach;

        TagModification focusTagMod = ForFocus(choice.Focus, 1);
        FocusTags tagFocus = (FocusTags)focusTagMod.Tag;
        int oldValueFocus = clonedTagSystem.GetFocusTagValue(tagFocus);
        clonedTagSystem.ModifyFocusTag(tagFocus, focusTagMod.Delta);
        int newValueFocus = clonedTagSystem.GetFocusTagValue(tagFocus);
        int actualDeltaFocus = newValueFocus - oldValueFocus;
        if (actualDeltaFocus != 0) projection.FocusTagChanges[tagFocus] = actualDeltaFocus;

        // Apply tag modifications to cloned system and track changes
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
            else if (mod.Type == TagModification.TagTypes.Approach)
            {
                ApproachTags tag = (ApproachTags)mod.Tag;
                int oldValue = clonedTagSystem.GetApproachTagValue(tag);
                clonedTagSystem.ModifyApproachTag(tag, mod.Delta);
                int newValue = clonedTagSystem.GetApproachTagValue(tag);
                int actualDelta = newValue - oldValue;

                if (actualDelta != 0)
                    projection.ApproachTagChanges[tag] = actualDelta;
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

        // Check for newly activated and deactivated tags
        List<IEncounterTag> newlyActivatedTags = new List<IEncounterTag>();
        List<IEncounterTag> deactivatedTags = new List<IEncounterTag>();

        foreach (IEncounterTag tag in Location.AvailableTags)
        {
            if (tag is NarrativeTag)
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
        }

        // Calculate momentum effects
        int momentumChange = 0;

        // Base momentum for momentum choices
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = choice is SpecialChoice ? 3 : (choice is EmergencyChoice ? 1 : 2);
            projection.MomentumComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Momentum Choice Base",
                Value = baseMomentum
            });
            momentumChange += baseMomentum;
        }
        // For pressure choices, momentum is still tracked but starts at 0
        else
        {
            momentumChange = 0;
        }

        // Calculate pressure effects
        int pressureChange = 0;

        // Base pressure for pressure choices or emergency choices
        if (choice.EffectType == EffectTypes.Pressure || choice is EmergencyChoice)
        {
            int basePressure = 1;
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure Choice Base",
                Value = basePressure
            });
            pressureChange += basePressure;
        }

        if (choice.EffectType == EffectTypes.Pressure && _escalationLevel > 0)
        {
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Escalation Pressure",
                Value = _escalationLevel
            });
            pressureChange += _escalationLevel;
        }

        int environmentalPressure = Location.GetEnvironmentalPressure(CurrentTurn);
        if (environmentalPressure > 0)
        {
            projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Pressure From Location",
                Value = environmentalPressure
            });
            pressureChange += environmentalPressure;
        }

        // Apply tag effects to momentum and pressure for all choices
        foreach (IEncounterTag tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                // Check if this tag should affect this choice
                bool affectsChoice = true;

                if (strategicTag.AffectedApproach.HasValue &&
                    choice.Approach != strategicTag.AffectedApproach.Value)
                    affectsChoice = false;

                if (strategicTag.AffectedFocus.HasValue &&
                    choice.Focus != strategicTag.AffectedFocus.Value)
                    affectsChoice = false;

                if (affectsChoice)
                {
                    // Apply momentum effects
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

                    // Apply pressure effects
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
        }

        // End of turn pressure reduction from current and newly activated tags
        int endOfTurnPressureChange = 0;

        // Check existing active tags
        foreach (IEncounterTag tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                if (strategicTag.EffectType == StrategicEffectTypes.ReducePressurePerTurn)
                {
                    int reduction = -strategicTag.GetEffectValueForState(this);
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name}",
                        Value = reduction
                    });
                    endOfTurnPressureChange += reduction;
                }
                else if (strategicTag.EffectType == StrategicEffectTypes.AddPressurePerTurn)
                {
                    int increase = strategicTag.GetEffectValueForState(this);
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name}",
                        Value = increase
                    });
                    endOfTurnPressureChange += increase;
                }
            }
        }

        // Check newly activated tags
        foreach (IEncounterTag tag in newlyActivatedTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                if (strategicTag.EffectType == StrategicEffectTypes.ReducePressurePerTurn)
                {
                    int reduction = -strategicTag.GetEffectValueForState(this);
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = $"{tag.Name} (new, end of turn)",
                        Value = reduction
                    });
                    endOfTurnPressureChange += reduction;
                }
                else if (strategicTag.EffectType == StrategicEffectTypes.AddPressurePerTurn)
                {
                    int increase = strategicTag.GetEffectValueForState(this);
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

        // Set final calculated values
        projection.FinalMomentum = currentMomentum + momentumChange;
        projection.FinalPressure = Math.Max(0, currentPressure + projection.PressureBuilt);

        // Project end of turn effects
        currentTurn++;
        projection.ProjectedTurn = currentTurn;

        // Check if encounter will end
        bool encounterEnds = (currentTurn >= Location.TurnDuration) || (projection.FinalPressure >= MaxPressure);
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

        // Calculate resource changes before returning
        projection.HealthChange = CalculateResourceChange(choice, ResourceTypes.Health);
        projection.ConcentrationChange = CalculateResourceChange(choice, ResourceTypes.Focus);
        projection.ReputationChange = CalculateResourceChange(choice, ResourceTypes.Confidence);

        // Add resource change components to the projection
        if (projection.HealthChange != 0)
        {
            projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = projection.HealthChange
            });
        }

        if (projection.ConcentrationChange != 0)
        {
            projection.ConcentrationComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = projection.ConcentrationChange
            });
        }

        if (projection.ReputationChange != 0)
        {
            projection.ReputationComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = projection.ReputationChange
            });
        }

        return projection;
    }
}