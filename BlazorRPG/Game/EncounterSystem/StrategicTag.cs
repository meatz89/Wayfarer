public class StrategicTag : IEncounterTag
{
    public string Name { get; }
    public FocusTags? AffectedFocus { get; } // What focus this affects
    public StrategicEffectTypes EffectType { get; }
    public ApproachTags? ScalingApproachTag { get; } // Which tag it scales with
    public int EffectValue { get; }

    private bool _activationEffectApplied = false;

    public StrategicTag(
        string name,
        StrategicEffectTypes effectType,
        FocusTags? affectedFocus = null,
        ApproachTags? scalingApproachTag = null)
    {
        Name = name;
        EffectType = effectType;
        AffectedFocus = affectedFocus;
        ScalingApproachTag = scalingApproachTag;
        EffectValue = 1; // Default value when not scaling
    }

    // Strategic tags are ALWAYS active
    public bool IsActive(BaseTagSystem tagSystem)
    {
        return true;
    }

    public void ApplyEffect(EncounterState state)
    {
        int effectValue = GetEffectValueForState(state);

        switch (EffectType)
        {
            case StrategicEffectTypes.AddMomentumToFocus:
                if (AffectedFocus.HasValue)
                    state.AddFocusMomentumBonus(AffectedFocus.Value, effectValue);
                break;

            case StrategicEffectTypes.ReducePressurePerTurn:
                state.AddEndOfTurnPressureReduction(effectValue);
                break;

            case StrategicEffectTypes.AddPressurePerTurn:
                state.AddEndOfTurnPressureReduction(-effectValue);
                break;

                // Resource effects are handled in ApplyPersistentResourceEffects
        }
    }

    public void ApplyActivationEffect(EncounterState state)
    {
        if (_activationEffectApplied)
            return;

        switch (EffectType)
        {
            case StrategicEffectTypes.AddMomentumOnActivation:
                state.BuildMomentum(GetEffectValueForState(state));
                break;

            case StrategicEffectTypes.ReducePressureOnActivation:
                state.ReducePressure(GetEffectValueForState(state));
                break;
        }

        _activationEffectApplied = true;
    }

    public int GetMomentumModifierForChoice(IChoice choice)
    {
        if (!ShouldAffectChoice(choice))
            return 0;

        switch (EffectType)
        {
            case StrategicEffectTypes.AddMomentumToFocus:
                return GetEffectValueForState(null); // Will be properly applied in the state
            default:
                return 0;
        }
    }

    public int GetPressureModifierForChoice(IChoice choice)
    {
        if (!ShouldAffectChoice(choice))
            return 0;

        switch (EffectType)
        {
            case StrategicEffectTypes.ReducePressureFromFocus:
                return -GetEffectValueForState(null); // Will be properly applied in the state
            default:
                return 0;
        }
    }

    private bool ShouldAffectChoice(IChoice choice)
    {
        if (AffectedFocus.HasValue && choice.Focus != AffectedFocus.Value)
            return false;

        return true;
    }

    // Get the actual effect value based on scaling
    public int GetEffectValueForState(EncounterState state)
    {
        // If no scaling tag is specified or we don't have state, use the base effect value
        if (!ScalingApproachTag.HasValue || state == null)
            return EffectValue;

        // Get the scaled value from the specified approach tag
        return state.TagSystem.GetEncounterStateTagValue(ScalingApproachTag.Value);
    }

    public string GetEffectDescription()
    {
        string baseDesc = GetBaseEffectDescription();

        if (ScalingApproachTag.HasValue)
        {
            return baseDesc.Replace("{X}", $"{ScalingApproachTag.Value} value");
        }

        return baseDesc.Replace("{X}", EffectValue.ToString());
    }

    private string GetBaseEffectDescription()
    {
        switch (EffectType)
        {
            case StrategicEffectTypes.AddMomentumToFocus:
                return $"+X momentum to {AffectedFocus} choices";

            case StrategicEffectTypes.ReducePressurePerTurn:
                return $"-X pressure at end of each turn";

            default:
                return "Affects encounter mechanics";
        }
    }

    public string GetActivationDescription()
    {
        return "Always active"; // Strategic tags are always active
    }
}