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
            ApproachTags.Dominance);


}
