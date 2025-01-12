public class LocationPropertyChoiceEffects
{
    public static List<LocationPropertyChoiceEffect> AllEffects { get; set; } = new()
    {
        // Crowded spaces make focus harder but increase social rewards
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ActivityLevelValue { ActivityLevel = ActivityLevelTypes.Bustling },
            ValueTypeEffect = new EnergyModification
            {
                TargetArchetype = ChoiceArchetypes.Focus,
                EnergyCostModifier = 1
            },
            RuleDescription = "Focus tasks require extra energy in crowded spaces"
        },

        // Indoor spaces help with social actions
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ExposureValue { Exposure = ExposureTypes.Indoor },
            ValueTypeEffect = new ValueBonus
            {
                ChoiceArchetype = ChoiceArchetypes.Social,
                ValueType = ValueTypes.Resonance,
                BonusAmount = 1
            },
            RuleDescription = "Social choices gain bonus Resonance indoors"
        },

        // Relaxed atmosphere reduces pressure gain
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new SupervisionValue { Supervision = SupervisionTypes.Unsupervised },
            ValueTypeEffect = new ValueModification
            {
                ValueType = ValueTypes.Pressure,
                ModifierAmount = -1
            },
            RuleDescription = "Relaxed atmosphere reduces pressure gain from all choices"
        },

        // Tavern archetype converts some Outcome to Resonance for social choices
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ArchetypeValue { Archetype = LocationArchetypes.Tavern },
            ValueTypeEffect = new PartialValueConversion
            {
                SourceValueType = ValueTypes.Outcome,
                TargetValueType = ValueTypes.Resonance,
                ConversionAmount = 1,
                TargetArchetype = ChoiceArchetypes.Social
            },
            RuleDescription = "Social successes in taverns build extra relationships"
        }
    };
}