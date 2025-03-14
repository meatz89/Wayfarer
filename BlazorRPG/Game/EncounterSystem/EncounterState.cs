using BlazorRPG.Game.EncounterManager;
using static BlazorRPG.Game.EncounterManager.TagModification;

public class EncounterState
{
    public int Momentum { get; private set; }
    public int Pressure { get; private set; }
    public BaseTagSystem TagSystem { get; }
    public List<IEncounterTag> ActiveTags { get; }
    public int CurrentTurn { get; private set; }
    public LocationInfo Location { get; }
    public LocationSpot LocationSpot { get; internal set; }
    public PlayerState PlayerState { get; }

    public const int MaxPressure = 30;

    private readonly Dictionary<ApproachTags, int> _approachMomentumBonuses = new Dictionary<ApproachTags, int>();
    private readonly Dictionary<FocusTags, int> _focusMomentumBonuses = new Dictionary<FocusTags, int>();
    private readonly Dictionary<ApproachTags, int> _approachPressureModifiers = new Dictionary<ApproachTags, int>();
    private readonly Dictionary<FocusTags, int> _focusPressureModifiers = new Dictionary<FocusTags, int>();
    private int _endOfTurnPressureReduction = 0;
    private int _escalationLevel = 0;
    private IChoice _lastChoice;

    public EncounterState(LocationInfo location, PlayerState playerState)
    {
        Momentum = 0;
        Pressure = 0;
        TagSystem = new BaseTagSystem();
        ActiveTags = new List<IEncounterTag>();
        CurrentTurn = 0;
        Location = location;
        PlayerState = playerState;
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            _approachMomentumBonuses[approach] = 0;
            _approachPressureModifiers[approach] = 0;
        }

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            _focusMomentumBonuses[focus] = 0;
            _focusPressureModifiers[focus] = 0;
        }
    }

    public void ApplyChoiceProjection(ChoiceProjection projection)
    {
        _lastChoice = projection.Choice;

        // 1. Apply tag changes
        foreach (KeyValuePair<EncounterStateTags, int> pair in projection.EncounterStateTagChanges)
            TagSystem.ModifyEncounterStateTag(pair.Key, pair.Value);

        foreach (KeyValuePair<ApproachTags, int> pair in projection.ApproachTagChanges)
            TagSystem.ModifyApproachTag(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in projection.FocusTagChanges)
            TagSystem.ModifyFocusTag(pair.Key, pair.Value);

        // 2. Update active tags based on new tag values
        ResetTagEffects();
        UpdateActiveTags(Location.AvailableTags);

        // 3. Apply exactly the values from the projection
        Momentum = projection.FinalMomentum;
        Pressure = projection.FinalPressure;

        // 4. Apply resource changes directly from the projection
        if (projection.HealthChange != 0)
            PlayerState.ModifyHealth(projection.HealthChange);

        if (projection.FocusChange != 0)
            PlayerState.ModifyFocus(projection.FocusChange);

        if (projection.ConfidenceChange != 0)
            PlayerState.ModifyConfidence(projection.ConfidenceChange);

        // 5. Increment turn counter
        CurrentTurn++;
        _escalationLevel = Math.Min(3, (CurrentTurn - 1) / 2);
    }

    public void UpdateActiveTags(IEnumerable<IEncounterTag> locationTags)
    {
        // Save previous active tags for comparison
        HashSet<string> previouslyActive = new HashSet<string>(ActiveTags.Select(t => t.Name));

        // Clear active tags and reset effects
        ActiveTags.Clear();
        ResetTagEffects();

        // Determine which tags are active based on current tag values
        foreach (IEncounterTag tag in locationTags)
        {
            bool shouldActivate = tag is StrategicTag || (tag is NarrativeTag && tag.IsActive(TagSystem));

            if (shouldActivate)
            {
                ActiveTags.Add(tag);
                tag.ApplyEffect(this);
            }
        }
    }

    public int GetTotalMomentum(IChoice choice, int baseMomentum)
    {
        int total = baseMomentum;

        if (_approachMomentumBonuses.ContainsKey(choice.Approach))
            total += _approachMomentumBonuses[choice.Approach];

        if (_focusMomentumBonuses.ContainsKey(choice.Focus))
            total += _focusMomentumBonuses[choice.Focus];

        return total;
    }

    public int GetTotalPressure(IChoice choice, int basePressure)
    {
        int total = basePressure;

        if (_approachPressureModifiers.ContainsKey(choice.Approach))
            total += _approachPressureModifiers[choice.Approach];

        if (_focusPressureModifiers.ContainsKey(choice.Focus))
            total += _focusPressureModifiers[choice.Focus];

        return Math.Max(0, total);
    }

    public void BuildMomentum(int amount) => Momentum += amount;

    public void BuildPressure(int amount) => Pressure += amount;

    public void ReducePressure(int amount) => Pressure = Math.Max(0, Pressure - amount);

    public void AddApproachMomentumBonus(ApproachTags approach, int bonus)
    {
        if (!_approachMomentumBonuses.ContainsKey(approach))
            _approachMomentumBonuses[approach] = 0;

        _approachMomentumBonuses[approach] += bonus;
    }

    public void AddFocusMomentumBonus(FocusTags focus, int bonus)
    {
        if (!_focusMomentumBonuses.ContainsKey(focus))
            _focusMomentumBonuses[focus] = 0;

        _focusMomentumBonuses[focus] += bonus;
    }

    public void AddApproachPressureModifier(ApproachTags approach, int modifier)
    {
        if (!_approachPressureModifiers.ContainsKey(approach))
            _approachPressureModifiers[approach] = 0;

        _approachPressureModifiers[approach] += modifier;
    }

    public void AddFocusPressureModifier(FocusTags focus, int modifier)
    {
        if (!_focusPressureModifiers.ContainsKey(focus))
            _focusPressureModifiers[focus] = 0;

        _focusPressureModifiers[focus] += modifier;
    }

    public void AddEndOfTurnPressureReduction(int reduction) => _endOfTurnPressureReduction += reduction;

    public void ResetTagEffects()
    {
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
            _approachMomentumBonuses[approach] = 0;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusMomentumBonuses[focus] = 0;

        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
            _approachPressureModifiers[approach] = 0;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
            _focusPressureModifiers[focus] = 0;

        _endOfTurnPressureReduction = 0;
    }

    public void EndTurn()
    {
        CurrentTurn++;
        _escalationLevel = Math.Min(3, (CurrentTurn - 1) / 2);

        if (_endOfTurnPressureReduction > 0)
            ReducePressure(_endOfTurnPressureReduction);
    }

    public bool IsEncounterOver() => Pressure >= MaxPressure || CurrentTurn >= Location.TurnDuration;

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

    public int CalculateResourceChange(IChoice choice, ResourceTypes resourceType)
    {
        int change = 0;

        foreach (var tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                if (strategicTag.AffectedApproach.HasValue && choice.Approach != strategicTag.AffectedApproach.Value)
                    continue;

                if (strategicTag.AffectedFocus.HasValue && choice.Focus != strategicTag.AffectedFocus.Value)
                    continue;

                switch (resourceType)
                {
                    case ResourceTypes.Health:
                        if (strategicTag.EffectType == StrategicEffectTypes.ReduceHealthByPressure)
                            change -= Pressure;
                        else if (strategicTag.EffectType == StrategicEffectTypes.ReduceHealthByApproachValue &&
                                 strategicTag.ScalingApproachTag.HasValue)
                            change -= TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                        break;

                    case ResourceTypes.Focus:
                        if (strategicTag.EffectType == StrategicEffectTypes.ReduceFocusByPressure)
                            change -= Pressure;
                        else if (strategicTag.EffectType == StrategicEffectTypes.ReduceFocusByApproachValue &&
                                 strategicTag.ScalingApproachTag.HasValue)
                            change -= TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                        break;

                    case ResourceTypes.Confidence:
                        if (strategicTag.EffectType == StrategicEffectTypes.ReduceConfidenceByPressure)
                            change -= Pressure;
                        else if (strategicTag.EffectType == StrategicEffectTypes.ReduceConfidenceByApproachValue &&
                                 strategicTag.ScalingApproachTag.HasValue)
                            change -= TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                        break;
                }
            }
        }

        return change;
    }

    public ChoiceProjection CreateChoiceProjection(IChoice choice)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Create working copies to simulate state changes
        BaseTagSystem clonedTagSystem = TagSystem.Clone();
        int currentMomentum = Momentum;
        int currentPressure = Pressure;
        int currentTurn = CurrentTurn;

        // Add implicit tag modifications for approach and focus
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

        // Determine which tags will be active based on new tag values
        List<IEncounterTag> newlyActivatedTags = new List<IEncounterTag>();
        List<IEncounterTag> deactivatedTags = new List<IEncounterTag>();
        List<IEncounterTag> projectedActiveTags = new List<IEncounterTag>();

        foreach (IEncounterTag tag in Location.AvailableTags)
        {
            bool wasActive = ActiveTags.Any(t => t.Name == tag.Name);
            bool willBeActive = tag is StrategicTag || (tag is NarrativeTag && tag.IsActive(clonedTagSystem));

            if (willBeActive)
            {
                projectedActiveTags.Add(tag);

                if (!wasActive)
                {
                    newlyActivatedTags.Add(tag);
                    projection.NewlyActivatedTags.Add(tag.Name);
                }
            }
            else if (wasActive)
            {
                deactivatedTags.Add(tag);
                projection.DeactivatedTags.Add(tag.Name);
            }
        }

        // Calculate momentum effects
        int momentumChange = 0;

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
        else
        {
            momentumChange = 0;
        }

        // Calculate pressure effects
        int pressureChange = 0;

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

        // Apply effects from currently active tags only, NOT newly activated ones
        foreach (IEncounterTag tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                bool affectsChoice = true;

                if (strategicTag.AffectedApproach.HasValue && choice.Approach != strategicTag.AffectedApproach.Value)
                    affectsChoice = false;

                if (strategicTag.AffectedFocus.HasValue && choice.Focus != strategicTag.AffectedFocus.Value)
                    affectsChoice = false;

                if (affectsChoice)
                {
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

        // Calculate end-of-turn pressure changes from currently active tags only
        int endOfTurnPressureChange = 0;

        foreach (IEncounterTag tag in ActiveTags)
        {
            if (tag is StrategicTag strategicTag)
            {
                if (strategicTag.EffectType == StrategicEffectTypes.ReducePressurePerTurn)
                {
                    int reduction = -strategicTag.GetEffectValueForState(this);
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = tag.Name,
                        Value = reduction
                    });
                    endOfTurnPressureChange += reduction;
                }
                else if (strategicTag.EffectType == StrategicEffectTypes.AddPressurePerTurn)
                {
                    int increase = strategicTag.GetEffectValueForState(this);
                    projection.PressureComponents.Add(new ChoiceProjection.ValueComponent
                    {
                        Source = tag.Name,
                        Value = increase
                    });
                    endOfTurnPressureChange += increase;
                }
            }
        }

        // Set projection values
        projection.MomentumGained = momentumChange;
        projection.PressureBuilt = pressureChange + endOfTurnPressureChange;

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

        // Calculate resource changes using current active tags only (not newly activated ones)
        projection.HealthChange = CalculateResourceChange(choice, ResourceTypes.Health);
        projection.FocusChange = CalculateResourceChange(choice, ResourceTypes.Focus);
        projection.ConfidenceChange = CalculateResourceChange(choice, ResourceTypes.Confidence);

        // Add resource change components to the projection
        if (projection.HealthChange != 0)
        {
            projection.HealthComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = projection.HealthChange
            });
        }

        if (projection.FocusChange != 0)
        {
            projection.FocusComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = projection.FocusChange
            });
        }

        if (projection.ConfidenceChange != 0)
        {
            projection.ConfidenceComponents.Add(new ChoiceProjection.ValueComponent
            {
                Source = "Strategic tag effects",
                Value = projection.ConfidenceChange
            });
        }

        return projection;
    }
}