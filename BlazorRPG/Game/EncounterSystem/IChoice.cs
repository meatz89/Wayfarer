public interface IChoice
{
    string Name { get; }
    string Description { get; }
    bool IsBlocked { get; }
    ApproachTags Approach { get; }
    FocusTags Focus { get; }
    EffectTypes EffectType { get; }
    CardTiers Tier { get; }
    int BaseEffectValue { get; }
    IReadOnlyList<TagModification> TagModifications { get; }
    RequirementInfo Requirement { get; }
    StrategicEffect StrategicEffect { get; }
    void ApplyChoice(EncounterState state);
    void SetBlocked();
}

/// <summary>
/// Standard choice implementation - either builds momentum or pressure
/// </summary>
public class Choice : IChoice
{
    public string Name { get; }
    public string Description { get; }
    public bool IsBlocked { get; private set; }
    public ApproachTags Approach => this.GetPrimaryApproach();
    public FocusTags Focus { get; }
    public EffectTypes EffectType { get; }
    public CardTiers Tier { get; }
    public int BaseEffectValue { get; }
    public IReadOnlyList<TagModification> TagModifications { get; }
    public RequirementInfo Requirement { get; }
    public StrategicEffect StrategicEffect { get; }

    public Choice(string name, string description, FocusTags focus,
                 EffectTypes effectType, CardTiers tier, int baseEffectValue,
                 RequirementInfo requirement, StrategicEffect strategicEffect, IReadOnlyList<TagModification> tagModifications)
    {
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