
// Updated LocationPropertyChoiceEffect
public class LocationPropertyChoiceEffect
{
    public LocationPropertyTypeValue LocationProperty { get; set; } // Changed to abstract type
    public ValueTransformation ValueTypeEffect { get; set; } // Changed to base class

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

        // Example 2: Scale (Intimate) -> Increase Insight, Reduce Pressure
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ScaleValue { ScaleVariation = ScaleVariationTypes.Intimate },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Insight, ChangeInValue = 1 },
            RuleDescription = "Intimate spaces increase Insight by 1."
        },
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ScaleValue { ScaleVariation = ScaleVariationTypes.Intimate },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Pressure, ChangeInValue = -1 },
            RuleDescription = "Intimate spaces reduce Pressure by 1."
        },

        // Thematic Fit: Intimate spaces are conducive to introspection and deep thought (Insight) and can feel less pressured.

        // Example 3: Exposure (Outdoor) -> Reduce Physical Energy Cost, Increase Pressure
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ExposureValue { ExposureCondition = ExposureConditionTypes.Outdoor },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Physical, ChangeInValue = -1 }
        },
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ExposureValue { ExposureCondition = ExposureConditionTypes.Outdoor },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Pressure, ChangeInValue = 1 }
        },

        // Thematic Fit: Being outdoors might make physical activities easier but can increase the feeling of being exposed or vulnerable (Pressure).

        // Example 4: Legality (Illegal) -> Increase Pressure, Cancel Resonance
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new LegalityValue { Legality = LegalityTypes.Illegal },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Pressure, ChangeInValue = 2 }
        },
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new LegalityValue { Legality = LegalityTypes.Illegal },
            ValueTypeEffect = new CancelValueTransformation { ValueType = ValueTypes.Resonance }
        },

        // Thematic Fit: Illegal locations naturally increase pressure and might prevent positive social interactions or emotional connection (Resonance).

        // Example 5: Pressure (Hostile) -> Reduce Outcome, Increase Physical Energy Cost
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new PressureValue { PressureState = PressureStateTypes.Hostile },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Outcome, ChangeInValue = -2 }
        },
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new PressureValue { PressureState = PressureStateTypes.Hostile },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Physical, ChangeInValue = 1 }
        },

        // Thematic Fit: Hostile environments make it harder to achieve desired outcomes and are physically taxing.

        // Example 6: Complexity (Complex) -> Reduce Social Energy Cost, Increase Insight
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ComplexityValue { Complexity = ComplexityTypes.Complex },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Social, ChangeInValue = -1 }
        },
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ComplexityValue { Complexity = ComplexityTypes.Complex },
            ValueTypeEffect = new ChangeValueTransformation { ValueType = ValueTypes.Insight, ChangeInValue = 2 }
        },

        // Thematic Fit: Complex environments might be socially easier to navigate (if you are skilled) and offer opportunities for learning and gaining insight.

        // Example 7: Resource (Food) -> Convert Outcome to Resonance, Reduce Focus Energy Cost
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ResourceValue { Resource = ResourceTypes.Food },
            ValueTypeEffect = new ConvertValueTransformation
            {
                SourceValueType = ValueTypes.Outcome,
                TargetValueType = ValueTypes.Resonance
            }
        },
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new ResourceValue { Resource = ResourceTypes.Food },
            ValueTypeEffect = new EnergyValueTransformation { EnergyType = EnergyTypes.Focus, ChangeInValue = -1 }
        }

        // Thematic Fit: Having food resources can turn a successful outcome into a positive social experience (Resonance) and reduce the mental effort needed.
    };
}