public class StrategicEffectsContent
{
    public static StrategicEffect Forceful
        => new StrategicEffect(new() { Atmosphere.Rough }, 
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance);
    
    public static StrategicEffect Disruptive
        => new StrategicEffect(new() { Population.Quiet }, 
            StrategicTagEffectType.IncreasePressure, ApproachTags.Dominance);

    public static StrategicEffect Insightful
        => new StrategicEffect(new() { Population.Scholarly },
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis);

    public static StrategicEffect Confusing
        => new StrategicEffect(new() { Atmosphere.Chaotic }, 
            StrategicTagEffectType.IncreasePressure, ApproachTags.Analysis);
    
    public static StrategicEffect Calculated
        => new StrategicEffect(new() { Physical.Confined }, 
            StrategicTagEffectType.DecreasePressure, ApproachTags.Precision);
    
    public static StrategicEffect Hindered
        => new StrategicEffect(new() { Illumination.Dark }, 
            StrategicTagEffectType.DecreaseMomentum, ApproachTags.Precision);

    public static StrategicEffect Persuasive
        => new StrategicEffect(new() { Population.Crowded }, 
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport);

    public static StrategicEffect Inappropriate
        => new StrategicEffect(new() { Atmosphere.Formal }, 
            StrategicTagEffectType.IncreasePressure, ApproachTags.Rapport);

    public static StrategicEffect Stealthy
        => new StrategicEffect(new() { Illumination.Shadowy }, 
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment);
    
    public static StrategicEffect Exposed
        => new StrategicEffect(new() { Population.Crowded }, 
            StrategicTagEffectType.IncreasePressure, ApproachTags.Concealment);
}