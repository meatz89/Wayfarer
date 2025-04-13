/// <summary>
/// Standard choice implementation - either builds momentum or pressure
/// </summary>
public class ChoiceCard
{
    public string Name { get; }
    public string Description { get; }
    public bool IsBlocked { get; private set; }
    public CardTiers Tier { get; }
    public EffectTypes EffectType { get; }
    public int BaseEffectValue { get; }
    public ApproachTags Approach { get; }
    public int OptimalApproachValue { get; }
    public FocusTags Focus { get; }
    public int OptimalFocusValue { get; }
    public IReadOnlyList<TagModification> TagModifications { get; }
    public StrategicEffect StrategicEffect { get; set;  }

    public override string ToString()
    {
        string desc = $"{Name} - Tier {(int)Tier} ({StrategicEffect.ToString()})";
        return desc;
    }

    public ChoiceCard(string name, string description, 
        EffectTypes effectType, CardTiers tier, int baseEffectValue, StrategicEffect strategicEffect, IReadOnlyList<TagModification> tagModifications,
        ApproachTags approach, int approachPosition, FocusTags focus, int focusPosition)
    {
        Approach = approach;
        OptimalApproachValue = approachPosition;
        OptimalFocusValue = focusPosition;
        Name = name;
        Description = description;
        Focus = focus;
        EffectType = effectType;
        Tier = tier;
        BaseEffectValue = baseEffectValue;
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
    }

    public void SetBlocked()
    {
        IsBlocked = true;
    }

}

