public class LocationPropertyChoiceEffects
{
    public static List<LocationPropertyChoiceEffect> AllEffects { get; set; } = new()
    {
        //// Crowded spaces make focus harder but increase social rewards
        //new LocationPropertyChoiceEffect
        //{
        //    LocationProperty = new ActivityLevelValue { ActivityLevel = ActivityLevelTypes.Bustling },
        //    ValueTypeEffect = new EnergyModification
        //    {
        //        TargetArchetype = ChoiceArchetypes.Focus,
        //        EnergyCostModifier = 1
        //    },
        //    RuleDescription = "Focus tasks require extra energy in crowded spaces"
        //},

        //// Relaxed atmosphere reduces pressure gain
        //new LocationPropertyChoiceEffect
        //{
        //    LocationProperty = new AtmosphereValue { Atmosphere = AtmosphereTypes.Relaxed },
        //    ValueTypeEffect = new ValueModification
        //    {
        //        ValueType = ValueTypes.Pressure,
        //        ModifierAmount = -1
        //    },
        //    RuleDescription = "Relaxed atmosphere reduces pressure gain from all choices"
        //},
    };
}