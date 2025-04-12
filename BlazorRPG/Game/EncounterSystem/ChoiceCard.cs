using System.Xml.Linq;

/// <summary>
/// Standard choice implementation - either builds momentum or pressure
/// </summary>
public class ChoiceCard
{
    public string Name { get; }
    public string Description { get; }
    public bool IsBlocked { get; private set; }
    public FocusTags Focus { get; }
    public EffectTypes EffectType { get; }
    public CardTiers Tier { get; }
    public int BaseEffectValue { get; }
    public IReadOnlyList<TagModification> TagModifications { get; }
    public RequirementInfo Requirement { get; }
    public StrategicEffect StrategicEffect { get; }
    public ApproachTags Approach { get; }
    public int OptimalApproachValue { get; }
    public int OptimalFocusValue { get; }


    public ChoiceCard(string name, string description, FocusTags focus,
                 EffectTypes effectType, CardTiers tier, int baseEffectValue,
                 RequirementInfo requirement, StrategicEffect strategicEffect,
                 IReadOnlyList<TagModification> tagModifications,
                 ApproachTags optimalApproach, int optimalApproachValue, int optimalFocusValue)
    {
        Approach = optimalApproach;
        OptimalApproachValue = optimalApproachValue;
        OptimalFocusValue = optimalFocusValue;
        Name = name;
        Description = description;
        Focus = focus;
        EffectType = effectType;
        Tier = tier;
        BaseEffectValue = baseEffectValue;
        Requirement = requirement;
        TagModifications = tagModifications;
        StrategicEffect = strategicEffect;
    }

    public virtual void ApplyChoice(EncounterState state)
    {
        // Apply tag modifications
        foreach (TagModification mod in TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState)
                state.TagSystem.ModifyEncounterStateTag((ApproachTags)mod.Tag, mod.Delta);
            else
                state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
        }

        // Apply momentum or pressure effect
        if (EffectType == EffectTypes.Momentum)
        {
            int totalMomentum = state.GetTotalMomentum(this, BaseEffectValue);
            state.BuildMomentum(totalMomentum);
        }
        else // Pressure
        {
            int basePressure = -BaseEffectValue; // Negative because it reduces pressure
            int totalPressure = state.GetTotalPressure(this, basePressure);
            state.BuildPressure(totalPressure);
        }

        // Apply requirement reduction if applicable
        if (Requirement.Type == RequirementInfo.RequirementTypes.Approach)
        {
            state.TagSystem.ModifyEncounterStateTag(Requirement.ApproachTag, -Requirement.ReductionAmount);
        }
        else if (Requirement.Type == RequirementInfo.RequirementTypes.Focus)
        {
            state.TagSystem.ModifyFocusTag(Requirement.FocusTag, -Requirement.ReductionAmount);
        }
    }

    public void SetBlocked()
    {
        IsBlocked = true;
    }

    public override string ToString()
    {
        return $"{Name} - Tier {(int)Tier} - {Focus.ToString()} - {EffectType.ToString()}";
    }
}


/// <summary>
/// Card with effects that scale based on player position
/// </summary>
public class PositionScaledChoiceCard : ChoiceCard
{
    public ScalingType ScalingType { get; }
    public ApproachTags ScalingApproach { get; }

    public PositionScaledChoiceCard(
        string name, string description, FocusTags focus,
        EffectTypes effectType, CardTiers tier, int baseEffectValue,
        RequirementInfo requirement, StrategicEffect strategicEffect,
        IReadOnlyList<TagModification> tagModifications,
        ApproachTags optimalApproach, int optimalApproachValue, int optimalFocusValue,
        ScalingType scalingType, ApproachTags scalingApproach = ApproachTags.None)
        : base(name, description, focus, effectType, tier, baseEffectValue,
              requirement, strategicEffect, tagModifications,
              optimalApproach, optimalApproachValue, optimalFocusValue)
    {
        ScalingType = scalingType;
        ScalingApproach = scalingApproach;
    }

    public override void ApplyChoice(EncounterState state)
    {
        // Apply tag modifications
        foreach (TagModification mod in TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState)
                state.TagSystem.ModifyEncounterStateTag((ApproachTags)mod.Tag, mod.Delta);
            else
                state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
        }

        // Calculate scaled effect based on position
        int scaledEffect = CalculateScaledEffect(state.TagSystem);

        // Apply the scaled effect
        if (EffectType == EffectTypes.Momentum)
            state.BuildMomentum(scaledEffect);
        else // Pressure
            state.BuildPressure(-scaledEffect); // Negative for pressure reduction
    }

    private int CalculateScaledEffect(BaseTagSystem tagSystem)
    {
        switch (ScalingType)
        {
            case ScalingType.DirectApproachValue:
                return BaseEffectValue + tagSystem.GetEncounterStateTagValue(ScalingApproach);

            case ScalingType.HighestToLowestDifference:
                // Find highest and lowest approach values
                int highest = GetHighestApproachValue(tagSystem);
                int lowest = GetLowestApproachValue(tagSystem);
                return BaseEffectValue + (highest - lowest);

            case ScalingType.ApproachesWithinTwoPoints:
                // Count approaches within 2 points of each other
                return BaseEffectValue + CountApproachesWithinTwoPoints(tagSystem);

            case ScalingType.FocusDifference:
                // Difference between highest and second-highest focus
                return BaseEffectValue + GetFocusDifferential(tagSystem);

            default:
                return BaseEffectValue;
        }
    }

    private int GetHighestApproachValue(BaseTagSystem tagSystem)
    {
        int highest = 0;
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach))
            {
                int value = tagSystem.GetEncounterStateTagValue(approach);
                if (value > highest)
                    highest = value;
            }
        }
        return highest;
    }

    private int GetLowestApproachValue(BaseTagSystem tagSystem)
    {
        int lowest = int.MaxValue;
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach))
            {
                int value = tagSystem.GetEncounterStateTagValue(approach);
                if (value < lowest)
                    lowest = value;
            }
        }
        return lowest == int.MaxValue ? 0 : lowest;
    }

    private int CountApproachesWithinTwoPoints(BaseTagSystem tagSystem)
    {
        List<int> values = new List<int>();
        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach))
                values.Add(tagSystem.GetEncounterStateTagValue(approach));
        }

        int count = 0;
        for (int i = 0; i < values.Count; i++)
        {
            for (int j = i + 1; j < values.Count; j++)
            {
                if (Math.Abs(values[i] - values[j]) <= 2)
                    count++;
            }
        }
        return count;
    }

    private int GetFocusDifferential(BaseTagSystem tagSystem)
    {
        List<int> values = new List<int>();
        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            values.Add(tagSystem.GetFocusTagValue(focus));
        }

        values.Sort();
        values.Reverse();

        // Return difference between highest and second highest
        return values.Count >= 2 ? values[0] - values[1] : 0;
    }

    private bool IsApproachTag(ApproachTags tag)
    {
        return tag == ApproachTags.Dominance ||
               tag == ApproachTags.Rapport ||
               tag == ApproachTags.Analysis ||
               tag == ApproachTags.Precision ||
               tag == ApproachTags.Concealment;
    }
}

/// <summary>
/// Card that modifies player position in special ways
/// </summary>
public class PositionModificationCard : ChoiceCard
{
    public PositionModificationType ModificationType { get; }

    public PositionModificationCard(
        string name, string description, FocusTags focus,
        EffectTypes effectType, CardTiers tier, int baseEffectValue,
        RequirementInfo requirement, StrategicEffect strategicEffect,
        IReadOnlyList<TagModification> tagModifications,
        ApproachTags optimalApproach, int optimalApproachValue, int optimalFocusValue,
        PositionModificationType modificationType)
        : base(name, description, focus, effectType, tier, baseEffectValue,
              requirement, strategicEffect, tagModifications,
              optimalApproach, optimalApproachValue, optimalFocusValue)
    {
        ModificationType = modificationType;
    }

    public override void ApplyChoice(EncounterState state)
    {
        // Apply standard tag modifications
        foreach (TagModification mod in TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState)
                state.TagSystem.ModifyEncounterStateTag((ApproachTags)mod.Tag, mod.Delta);
            else
                state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
        }

        // Apply position modification based on type
        ApplyPositionModification(state.TagSystem);

        // Apply momentum/pressure effect
        if (EffectType == EffectTypes.Momentum)
            state.BuildMomentum(BaseEffectValue);
        else // Pressure
            state.BuildPressure(-BaseEffectValue); // Negative for pressure reduction
    }

    private void ApplyPositionModification(BaseTagSystem tagSystem)
    {
        switch (ModificationType)
        {
            case PositionModificationType.IncreaseLowestDecreaseHighest:
                // Find highest and lowest approaches
                ApproachTags highest = GetHighestApproach(tagSystem);
                ApproachTags lowest = GetLowestApproach(tagSystem);

                // Increase lowest by 2, decrease highest by 1
                tagSystem.ModifyEncounterStateTag(lowest, 2);
                tagSystem.ModifyEncounterStateTag(highest, -1);
                break;

            case PositionModificationType.TransferFocusPoints:
                // Find highest and lowest focuses
                FocusTags highestFocus = GetHighestFocus(tagSystem);
                FocusTags lowestFocus = GetLowestFocus(tagSystem);

                // Transfer 2 points from highest to lowest
                tagSystem.ModifyFocusTag(highestFocus, -2);
                tagSystem.ModifyFocusTag(lowestFocus, 2);
                break;

            case PositionModificationType.BalanceApproaches:
                // Calculate average approach value
                int sum = 0;
                int count = 0;

                foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
                {
                    if (IsApproachTag(approach))
                    {
                        sum += tagSystem.GetEncounterStateTagValue(approach);
                        count++;
                    }
                }

                int average = count > 0 ? sum / count : 0;

                // Adjust each approach toward the average
                foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
                {
                    if (IsApproachTag(approach))
                    {
                        int current = tagSystem.GetEncounterStateTagValue(approach);
                        int delta = average - current;

                        // Limit adjustments to +/-1
                        if (delta > 1) delta = 1;
                        if (delta < -1) delta = -1;

                        tagSystem.ModifyEncounterStateTag(approach, delta);
                    }
                }
                break;
        }
    }

    private ApproachTags GetHighestApproach(BaseTagSystem tagSystem)
    {
        ApproachTags highest = ApproachTags.Analysis; // Default
        int highestValue = -1;

        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach))
            {
                int value = tagSystem.GetEncounterStateTagValue(approach);
                if (value > highestValue)
                {
                    highestValue = value;
                    highest = approach;
                }
            }
        }

        return highest;
    }

    private ApproachTags GetLowestApproach(BaseTagSystem tagSystem)
    {
        ApproachTags lowest = ApproachTags.Analysis; // Default
        int lowestValue = int.MaxValue;

        foreach (ApproachTags approach in Enum.GetValues(typeof(ApproachTags)))
        {
            if (IsApproachTag(approach))
            {
                int value = tagSystem.GetEncounterStateTagValue(approach);
                if (value < lowestValue)
                {
                    lowestValue = value;
                    lowest = approach;
                }
            }
        }

        return lowest;
    }

    private FocusTags GetHighestFocus(BaseTagSystem tagSystem)
    {
        FocusTags highest = FocusTags.Information; // Default
        int highestValue = -1;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            int value = tagSystem.GetFocusTagValue(focus);
            if (value > highestValue)
            {
                highestValue = value;
                highest = focus;
            }
        }

        return highest;
    }

    private FocusTags GetLowestFocus(BaseTagSystem tagSystem)
    {
        FocusTags lowest = FocusTags.Information; // Default
        int lowestValue = int.MaxValue;

        foreach (FocusTags focus in Enum.GetValues(typeof(FocusTags)))
        {
            int value = tagSystem.GetFocusTagValue(focus);
            if (value < lowestValue)
            {
                lowestValue = value;
                lowest = focus;
            }
        }

        return lowest;
    }

    private bool IsApproachTag(ApproachTags tag)
    {
        return tag == ApproachTags.Dominance ||
               tag == ApproachTags.Rapport ||
               tag == ApproachTags.Analysis ||
               tag == ApproachTags.Precision ||
               tag == ApproachTags.Concealment;
    }
}
