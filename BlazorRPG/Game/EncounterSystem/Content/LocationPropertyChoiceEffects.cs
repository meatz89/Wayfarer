public class LocationPropertyChoiceEffects
{
    public static List<LocationPropertyChoiceEffect> AllEffects { get; set; } = new()
    {
        // Atmosphere effects
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new AtmosphereValue { Atmosphere = AtmosphereTypes.Relaxed },
            ValueTypeEffect = new ValueModification
            {
                ValueType = ValueTypes.Pressure,
                ModifierAmount = -2  // Relaxed atmosphere reduces pressure gain
            },
            RuleDescription = "The relaxed atmosphere helps keep pressure low"
        },

        new LocationPropertyChoiceEffect
        {
            LocationProperty = new AtmosphereValue { Atmosphere = AtmosphereTypes.Tense },
            ValueTypeEffect = new ValueModification
            {
                ValueType = ValueTypes.Pressure,
                ModifierAmount = 2  // Tense atmosphere increases pressure gain
            },
            RuleDescription = "The tense atmosphere increases pressure"
        },

        // Space effects
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ActivityLevelValue { ActivityLevel = ActivityLevelTypes.Quiet },
            ValueTypeEffect = new ValueBonus
            {
                ChoiceArchetype = ChoiceArchetypes.Focus,
                ValueType = ValueTypes.Insight,
                BonusAmount = 2  // Quet spaces enhance focus gains
            },
            RuleDescription = "The quet space helps concentration"
        }
    };
}