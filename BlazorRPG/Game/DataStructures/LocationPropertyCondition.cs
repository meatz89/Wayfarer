public class LocationPropertyCondition
{
    public LocationPropertyTypes PropertyType { get; set; }
    public object ExpectedValue { get; set; }

    public LocationPropertyCondition(LocationPropertyTypes propertyType, object expectedValue)
    {
        PropertyType = propertyType;
        ExpectedValue = expectedValue;
    }

    public bool IsMet(Location location)
    {
        object property = location.LocationProperties.GetProperty(PropertyType);

        // If the property is not set, consider the condition as met
        if (property == null)
        {
            return true;
        }

        return property.Equals(ExpectedValue);
    }
}