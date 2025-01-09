// Updated LocationPropertyChoiceEffect
public class LocationPropertyChoiceEffect
{
    public LocationPropertyTypeValue LocationProperty { get; set; }
    public ValueTransformation ValueTypeEffect { get; set; }
    public string RuleDescription { get; set; }
}

public class LocationPropertyChoiceEffects
{
    public static List<LocationPropertyChoiceEffect> Effects { get; set; } = new()
    {
        // Example 1: Crowd Level (Crowded) -> Convert Insight to Resonance
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new CrowdLevelValue { CrowdLevel = CrowdLevelTypes.Crowded },
            ValueTypeEffect = new ConvertValueTransformation
            {
                SourceValueType = ValueTypes.Insight,
                TargetValueType = ValueTypes.Resonance
            },
            RuleDescription = "Crowded areas convert Insight to Resonance."
        },

        // Thematic Fit: Crowded areas might limit deep thought (Insight) but increase social interaction and emotional impact (Resonance).

        // Example 2: Scale (Intimate) -> Increase Insight
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ScaleValue { ScaleVariation = ScaleVariationTypes.Intimate },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Insight, ChangeInValue = 1 },
            RuleDescription = "Intimate spaces increase Insight by 1."
        },

        // Example 3: Scale (Intimate) -> Reduce Pressure
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ScaleValue { ScaleVariation = ScaleVariationTypes.Intimate },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Pressure, ChangeInValue = -1 },
            RuleDescription = "Intimate spaces reduce Pressure by 1."
        },

        // Thematic Fit: Intimate spaces are conducive to introspection and deep thought (Insight) and can feel less pressured.

        // Example 4: Exposure (Outdoor) -> Reduce Physical Energy Cost
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ExposureValue { ExposureCondition = ExposureConditionTypes.Outdoor },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Physical, ChangeInValue = -1 },
            RuleDescription = "Being outdoors reduces Physical Energy cost by 1."
        },

        // Example 5: Exposure (Outdoor) -> Increase Pressure
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ExposureValue { ExposureCondition = ExposureConditionTypes.Outdoor },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Pressure, ChangeInValue = 1 },
            RuleDescription = "Being outdoors increases Pressure by 1."
        },

        // Thematic Fit: Being outdoors might make physical activities easier but can increase the feeling of being exposed or vulnerable (Pressure).

        // Example 6: Legality (Illegal) -> Increase Pressure
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new LegalityValue { Legality = LegalityTypes.Illegal },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Pressure, ChangeInValue = 2 },
            RuleDescription = "Illegal locations increase Pressure by 2."
        },

        // Example 7: Legality (Illegal) -> Cancel Resonance
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new LegalityValue { Legality = LegalityTypes.Illegal },
            ValueTypeEffect = new CancelValueTransformation { ValueType = ValueTypes.Resonance },
            RuleDescription = "Illegal locations cancel Resonance."
        },

        // Thematic Fit: Illegal locations naturally increase pressure and might prevent positive social interactions or emotional connection (Resonance).

        // Example 8: Pressure (Hostile) -> Reduce Outcome
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new PressureValue { PressureState = PressureStateTypes.Hostile },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Outcome, ChangeInValue = -2 },
            RuleDescription = "Hostile environments reduce Outcome by 2."
        },

        // Example 9: Pressure (Hostile) -> Increase Physical Energy Cost
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new PressureValue { PressureState = PressureStateTypes.Hostile },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Physical, ChangeInValue = 1 },
            RuleDescription = "Hostile environments increase Physical Energy cost by 1."
        },

        // Thematic Fit: Hostile environments make it harder to achieve desired outcomes and are physically taxing.

        // Example 10: Complexity (Complex) -> Reduce Social Energy Cost
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ComplexityValue { Complexity = ComplexityTypes.Complex },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Social, ChangeInValue = -1 },
            RuleDescription = "Complex environments reduce Social Energy cost by 1."
        },

        // Example 11: Complexity (Complex) -> Increase Insight
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ComplexityValue { Complexity = ComplexityTypes.Complex },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Insight, ChangeInValue = 2 },
            RuleDescription = "Complex environments increase Insight by 2."
        },

        // Thematic Fit: Complex environments might be socially easier to navigate (if you are skilled) and offer opportunities for learning and gaining insight.

        // Example 12: Resource (Food) -> Convert Outcome to Resonance
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ResourceValue { Resource = ResourceTypes.Food },
            ValueTypeEffect = new ConvertValueTransformation
            {
                SourceValueType = ValueTypes.Outcome,
                TargetValueType = ValueTypes.Resonance
            },
            RuleDescription = "Having food resources converts Outcome to Resonance."
        },

        // Example 13: Resource (Food) -> Reduce Focus Energy Cost
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ResourceValue { Resource = ResourceTypes.Food },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Focus, ChangeInValue = -1 },
            RuleDescription = "Having food resources reduces Focus energy cost by 1."
        },

    };
}