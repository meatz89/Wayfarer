/// <summary>
/// Base interface for all choices
/// </summary>
public interface IChoice
{
    string Name { get; }
    string Description { get; }
    FocusTags Focus { get; }
    EffectTypes EffectType { get; }
    IReadOnlyList<TagModification> TagModifications { get; }
    void ApplyChoice(EncounterState state);
}

/// <summary>
/// Standard choice implementation - either builds momentum or reduces pressure
/// </summary>
public class Choice : IChoice
{
    public string Name { get; }
    public string Description { get; }
    public FocusTags Focus { get; }
    public EffectTypes EffectType { get; }
    public IReadOnlyList<TagModification> TagModifications { get; }

    public Choice(string name, string description, FocusTags focus,
                  EffectTypes effectType, IReadOnlyList<TagModification> tagModifications)
    {
        Name = name;
        Description = description;
        Focus = focus;
        EffectType = effectType;
        TagModifications = tagModifications;
    }

    // This method is ONLY used through the projection system and should not be called directly
    public virtual void ApplyChoice(EncounterState state)
    {
        // Apply tag modifications
        foreach (TagModification mod in TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState)
                state.TagSystem.ModifyEncounterStateTag((EncounterStateTags)mod.Tag, mod.Delta);
            else
                state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
        }

        // Apply momentum or pressure effect
        if (EffectType == EffectTypes.Momentum)
        {
            int baseMomentum = 2; // Standard choices build 2 momentum as per project knowledge
            int totalMomentum = state.GetTotalMomentum(this, baseMomentum);
            state.BuildMomentum(totalMomentum);
        }
        else // Pressure
        {
            int basePressureReduction = 1; // Standard choices reduce 1 pressure as per project knowledge
            int totalPressureReduction = state.GetTotalPressure(this, basePressureReduction);
            state.ReducePressure(totalPressureReduction); // REDUCE pressure, not increase
        }
    }

    public override string ToString()
    {
        return $"{Name} - {Focus.ToString()} - {EffectType.ToString()}";
    }
}

/// <summary>
/// Special choice that requires specific tag values and builds more momentum
/// </summary>
public class SpecialChoice : Choice
{
    public IReadOnlyList<Func<BaseTagSystem, bool>> Requirements { get; }

    public SpecialChoice(string name, string description, FocusTags focus,
                        IReadOnlyList<TagModification> tagModifications,
                        IReadOnlyList<Func<BaseTagSystem, bool>> requirements)
        : base(name, description, focus, EffectTypes.Momentum, tagModifications)
    {
        Requirements = requirements;
    }

    public bool CanBeSelected(BaseTagSystem tagSystem)
    {
        return Requirements.All(req => req(tagSystem));
    }

    public override void ApplyChoice(EncounterState state)
    {
        // Apply tag modifications
        foreach (TagModification mod in TagModifications)
        {
            if (mod.Type == TagModification.TagTypes.EncounterState)
                state.TagSystem.ModifyEncounterStateTag((EncounterStateTags)mod.Tag, mod.Delta);
            else
                state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
        }

        // Special choices build 3 momentum (plus bonuses)
        int baseMomentum = 3;
        int totalMomentum = state.GetTotalMomentum(this, baseMomentum);
        state.BuildMomentum(totalMomentum);
    }

    public override string ToString()
    {
        return $"{Name} - {TagModifications.Select(x => x.Type).ToList().ToString()} - {Focus.ToString()} - {EffectType.ToString()}";
    }
}
