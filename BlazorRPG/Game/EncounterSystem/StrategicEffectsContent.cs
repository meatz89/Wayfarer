public class StrategicEffectsContent
{
    public static EnvironmentalPropertyEffect Forceful
        => new EnvironmentalPropertyEffect(new() { Atmosphere.Rough },
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Dominance);

    public static EnvironmentalPropertyEffect Disruptive
        => new EnvironmentalPropertyEffect(new() { Population.Quiet },
            StrategicTagEffectType.IncreasePressure, ApproachTags.Dominance);

    public static EnvironmentalPropertyEffect Insightful
        => new EnvironmentalPropertyEffect(new() { Illumination.Bright },
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Analysis);

    public static EnvironmentalPropertyEffect Confusing
        => new EnvironmentalPropertyEffect(new() { Atmosphere.Chaotic },
            StrategicTagEffectType.IncreasePressure, ApproachTags.Analysis);

    public static EnvironmentalPropertyEffect Calculated
        => new EnvironmentalPropertyEffect(new() { Physical.Confined },
            StrategicTagEffectType.DecreasePressure, ApproachTags.Precision);

    public static EnvironmentalPropertyEffect Hindered
        => new EnvironmentalPropertyEffect(new() { Illumination.Dark },
            StrategicTagEffectType.DecreaseMomentum, ApproachTags.Precision);

    public static EnvironmentalPropertyEffect Persuasive
        => new EnvironmentalPropertyEffect(new() { Population.Crowded },
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Rapport);

    public static EnvironmentalPropertyEffect Inappropriate
        => new EnvironmentalPropertyEffect(new() { Atmosphere.Tense },
            StrategicTagEffectType.IncreasePressure, ApproachTags.Rapport);

    public static EnvironmentalPropertyEffect Stealthy
        => new EnvironmentalPropertyEffect(new() { Illumination.Shadowy },
            StrategicTagEffectType.IncreaseMomentum, ApproachTags.Concealment);

    public static EnvironmentalPropertyEffect Exposed
        => new EnvironmentalPropertyEffect(new() { Population.Crowded },
            StrategicTagEffectType.IncreasePressure, ApproachTags.Concealment);
}