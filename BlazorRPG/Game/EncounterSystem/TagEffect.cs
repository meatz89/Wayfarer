using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Describes what effect a tag has on choices
/// </summary>
public class TagEffect
{
    public FocusTypes? AffectedFocus { get; set; }
    public ApproachTypes? AffectedApproach { get; set; }
    public int MomentumModifier { get; set; }
    public int PressureModifier { get; set; }
    public bool ZeroPressure { get; set; }
    public bool DoubleMomentum { get; set; }
    public bool IsSpecialEffect { get; set; }
    public string SpecialEffectId { get; set; }

    public TagEffect()
    {
        AffectedFocus = null;
        AffectedApproach = null;
        MomentumModifier = 0;
        PressureModifier = 0;
        ZeroPressure = false;
        DoubleMomentum = false;
        IsSpecialEffect = false;
        SpecialEffectId = string.Empty;
    }
}
