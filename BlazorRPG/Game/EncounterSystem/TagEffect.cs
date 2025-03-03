using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extended TagEffect class to include negative effects
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
    public bool IsNegative { get; set; }
    public bool BlockMomentum { get; set; }
    public bool DoublePressure { get; set; }

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
        IsNegative = false;
        BlockMomentum = false;
        DoublePressure = false;
    }
}
