public enum LocationPropertyType
{
    // Existing ones
    Scale,
    Exposure,
    Density,
    Structure,
    Legality,
    Tension,
    Complexity,

    // New ones
    Resource, // e.g., Herbs, Fish, Ore, Wood (can be an enum if you have specific resources)
    CrowdLevel, // e.g., Empty, Sparse, Busy, Packed
    Archetype,
    ActivityFocus
    // ... add more as needed
}


public class LocationPropertyCondition
{
    public LocationPropertyType PropertyType { get; set; }
    public object ExpectedValue { get; set; }

    public LocationPropertyCondition(LocationPropertyType propertyType, object expectedValue)
    {
        PropertyType = propertyType;
        ExpectedValue = expectedValue;
    }

    // Check if the condition is met by the given LocationProperties
    public bool IsMet(LocationProperties properties)
    {
        object actualValue = properties.GetProperty(PropertyType);

        if (actualValue == null)
        {
            return ExpectedValue == null;
        }

        // Handle special cases for enums with a None value
        if (actualValue is Enum && IsNoneEnumValue((Enum)actualValue))
        {
            return ExpectedValue == null || ExpectedValue is Enum && IsNoneEnumValue((Enum)ExpectedValue);
        }

        return actualValue.Equals(ExpectedValue);
    }

    // Helper method to check if an enum value is a 'None' equivalent
    private bool IsNoneEnumValue(Enum enumValue)
    {
        return enumValue.ToString() == "None";
    }
}