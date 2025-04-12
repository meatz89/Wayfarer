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

