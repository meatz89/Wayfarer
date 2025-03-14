public class StrategicTagRespository
{
    public static StrategicTag GrowingTension = new StrategicTag(
            "Growing Tension",
            StrategicEffectTypes.AddPressurePerTurn,
            null,  // Applies to all approaches
            null,  // Applies to all focuses
            null);

    public static StrategicTag CombatExhaustion = new StrategicTag(
            "Combat Exhaustion",
            StrategicEffectTypes.ReduceHealthByPressure,
            ApproachTags.Force,
            null,
            EncounterStateTags.Dominance);

    public static StrategicTag PoorlyCoordinated = new StrategicTag(
            "Poorly Coordinated",
            StrategicEffectTypes.AddMomentumToApproach,
            ApproachTags.Force,
            null,
            EncounterStateTags.Precision);

    public static StrategicTag EasilyDistracted = new StrategicTag(
            "Easily Distracted",
            StrategicEffectTypes.AddMomentumToApproach,
            ApproachTags.Stealth,
            null,
            EncounterStateTags.Concealment);

}
