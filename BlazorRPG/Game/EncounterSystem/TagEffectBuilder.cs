
/// <summary>
/// Builder for creating tag effects
/// </summary>
public class TagEffectBuilder
{
    private FocusTypes? _affectedFocus;
    private ApproachTypes? _affectedApproach;
    private int _momentumModifier;
    private int _pressureModifier;
    private bool _zeroPressure;
    private bool _doubleMomentum;

    public TagEffectBuilder()
    {
        _affectedFocus = null;
        _affectedApproach = null;
        _momentumModifier = 0;
        _pressureModifier = 0;
        _zeroPressure = false;
        _doubleMomentum = false;
    }

    public TagEffectBuilder ForFocusType(FocusTypes focusType)
    {
        _affectedFocus = focusType;
        return this;
    }

    public TagEffectBuilder ForApproachType(ApproachTypes approachType)
    {
        _affectedApproach = approachType;
        return this;
    }

    public TagEffectBuilder AddMomentum(int amount)
    {
        _momentumModifier = amount;
        return this;
    }

    public TagEffectBuilder AddPressure(int amount)
    {
        _pressureModifier = amount;
        return this;
    }

    public TagEffectBuilder ZeroPressure()
    {
        _zeroPressure = true;
        return this;
    }

    public TagEffectBuilder DoubleMomentum()
    {
        _doubleMomentum = true;
        return this;
    }

    public TagEffect Build()
    {
        return new TagEffect
        {
            AffectedFocus = _affectedFocus,
            AffectedApproach = _affectedApproach,
            MomentumModifier = _momentumModifier,
            PressureModifier = _pressureModifier,
            ZeroPressure = _zeroPressure,
            DoubleMomentum = _doubleMomentum
        };
    }
}
