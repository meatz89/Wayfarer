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

    // Calculate total momentum for a choice including strategic tag bonuses
    public int GetTotalMomentum(IChoice choice, int baseMomentum)
    {
        int total = baseMomentum;

        // Apply approach bonus if any
        if (_approachMomentumBonuses.ContainsKey(choice.Approach))
            total += _approachMomentumBonuses[choice.Approach];

        // Apply focus bonus if any
        if (_focusMomentumBonuses.ContainsKey(choice.Focus))
            total += _focusMomentumBonuses[choice.Focus];

        return total;
    }

    // Add approach momentum bonus (from strategic tags)
    public void AddApproachMomentumBonus(ApproachTypes approach, int bonus)
    {
        if (!_approachMomentumBonuses.ContainsKey(approach))
            _approachMomentumBonuses[approach] = 0;

        _approachMomentumBonuses[approach] += bonus;
    }

    // Add focus momentum bonus (from strategic tags)
    public void AddFocusMomentumBonus(FocusTags focus, int bonus)
    {
        if (!_focusMomentumBonuses.ContainsKey(focus))
            _focusMomentumBonuses[focus] = 0;

        _focusMomentumBonuses[focus] += bonus;
    }

    // Add end-of-turn pressure reduction (from strategic tags)
    public void AddEndOfTurnPressureReduction(int reduction)
    {
        //_endOfTurnPressureReduction += reduction;
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
        ActiveTags.Clear();

        foreach (IEncounterTag tag in locationTags)
        {
            if (tag.IsActive(TagSystem))
            {
                ActiveTags.Add(tag);
                tag.ApplyEffect(this);
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

    public ChoiceProjection CreateChoiceProjection(IChoice choice)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        // Clone the tag system for simulation
        BaseTagSystem tempTagSystem = new BaseTagSystem();
        foreach (KeyValuePair<ApproachTags, int> pair in TagSystem.GetAllApproachTags())
            tempTagSystem.SetApproachTagValue(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in TagSystem.GetAllFocusTags())
            tempTagSystem.SetFocusTagValue(pair.Key, pair.Value);

        // 1. Apply tag modifications
        foreach (TagModification mod in choice.TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.Approach)
            {
                ApproachTags tag = (ApproachTags)mod.Tag;
                int currentValue = tempTagSystem.GetApproachTagValue(tag);
                int newValue = Math.Clamp(currentValue + mod.Delta, 0, 10);
                tempTagSystem.SetApproachTagValue(tag, newValue);

                projection.ApproachTagChanges[tag] = mod.Delta;
            }
            else
            {
                FocusTags tag = (FocusTags)mod.Tag;
                int currentValue = tempTagSystem.GetFocusTagValue(tag);
                int newValue = Math.Clamp(currentValue + mod.Delta, 0, 10);
                tempTagSystem.SetFocusTagValue(tag, newValue);

                projection.FocusTagChanges[tag] = mod.Delta;
            }
        }

        // 2. Calculate base momentum/pressure effects
        if (choice.EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = (choice is SpecialChoice) ? 3 : 2;
            int totalMomentum = baseMomentum;

            // Apply approach bonus if any
            int approachBonuses = _approachMomentumBonuses.ContainsKey(choice.Approach)
                ? _approachMomentumBonuses[choice.Approach] : 0;
            totalMomentum += approachBonuses;

            // Apply focus bonus if any
            int focusBonuses = _focusMomentumBonuses.ContainsKey(choice.Focus)
                ? _focusMomentumBonuses[choice.Focus] : 0;
            totalMomentum += focusBonuses;

            projection.MomentumGained = totalMomentum;
            projection.PressureBuilt = 0;
        }
        else // Pressure
        {
            projection.MomentumGained = 0;
            projection.PressureBuilt = 2; // Standard pressure from choice
        }

        // Special case for emergency choices
        if (choice is EmergencyChoice)
        {
            projection.MomentumGained = 1;
            projection.PressureBuilt = 2;
        }

        // 3. Calculate tag activations/deactivations
        HashSet<string> currentlyActiveTags = new HashSet<string>(ActiveTags.Select(t => t.Name));

        foreach (IEncounterTag tag in Location.AvailableTags)
        {
            bool wasActive = currentlyActiveTags.Contains(tag.Name);
            bool wouldBeActive = tag.IsActive(tempTagSystem);

            if (!wasActive && wouldBeActive)
            {
                projection.NewlyActivatedTags.Add(tag.Name);

                // Apply effect if it's a strategic tag
                if (tag is StrategicTag strategicTag)
                {
                    // The logic here depends on the actual implementation of strategic tags
                    // This is a simplified version based on tag naming conventions
                    if (tag.Name.Contains("momentum") &&
                        (tag.Name.Contains(choice.Approach.ToString()) ||
                         tag.Name.Contains(choice.Focus.ToString())))
                    {
                        if (choice.EffectType == EffectTypes.Momentum)
                            projection.MomentumGained += 1;
                    }

                    if (tag.Name.Contains("pressure reduction"))
                        projection.PressureBuilt = Math.Max(0, projection.PressureBuilt - 1);
                }
            }
            else if (wasActive && !wouldBeActive)
            {
                projection.DeactivatedTags.Add(tag.Name);
            }
            else if (wasActive && wouldBeActive && tag is StrategicTag)
            {
                // Apply effect for existing active tags
                // This logic mirrors what's in the strategic tag's effect
                if (tag.Name.Contains("momentum") &&
                    (tag.Name.Contains(choice.Approach.ToString()) ||
                     tag.Name.Contains(choice.Focus.ToString())))
                {
                    if (choice.EffectType == EffectTypes.Momentum)
                        projection.MomentumGained += 1;
                }

                if (tag.Name.Contains("pressure reduction"))
                    projection.PressureBuilt = Math.Max(0, projection.PressureBuilt - 1);
            }
        }

        // 4. Project final state
        projection.FinalMomentum = Momentum + projection.MomentumGained;
        projection.FinalPressure = Pressure + projection.PressureBuilt;
        projection.ProjectedTurn = CurrentTurn + 1;

        // 5. Check if encounter will end
        projection.EncounterWillEnd =
            projection.FinalPressure >= MaxPressure ||
            projection.ProjectedTurn >= Location.Duration;

        // 6. Project outcome
        if (projection.FinalPressure >= MaxPressure)
            projection.ProjectedOutcome = EncounterOutcomes.Failure;
        else if (projection.FinalMomentum < Location.PartialThreshold)
            projection.ProjectedOutcome = EncounterOutcomes.Failure;
        else if (projection.FinalMomentum < Location.StandardThreshold)
            projection.ProjectedOutcome = EncounterOutcomes.Partial;
        else if (projection.FinalMomentum < Location.ExceptionalThreshold)
            projection.ProjectedOutcome = EncounterOutcomes.Standard;
        else
            projection.ProjectedOutcome = EncounterOutcomes.Exceptional;

        return projection;
    }

    // Apply a choice projection to the actual state
    public void ApplyChoiceProjection(ChoiceProjection projection)
    {
        // 1. Apply tag changes
        foreach (KeyValuePair<ApproachTags, int> pair in projection.ApproachTagChanges)
            TagSystem.ModifyApproachTag(pair.Key, pair.Value);

        foreach (KeyValuePair<FocusTags, int> pair in projection.FocusTagChanges)
            TagSystem.ModifyFocusTag(pair.Key, pair.Value);

        // 2. Apply momentum/pressure changes
        Momentum += projection.MomentumGained;
        Pressure += projection.PressureBuilt;

        // 3. Update active tags based on new tag values
        UpdateActiveTags(Location.AvailableTags);

        // 4. Process end of turn effects
        EndTurn();
    }
}
