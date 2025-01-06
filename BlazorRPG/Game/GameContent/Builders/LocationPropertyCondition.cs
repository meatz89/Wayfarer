public class LocationPropertyCondition
{
    public LocationPropertyType PropertyType { get; set; }
    public object ExpectedValue { get; set; }

    public LocationPropertyCondition(LocationPropertyType propertyType, object expectedValue)
    {
        PropertyType = propertyType;
        ExpectedValue = expectedValue;
    }

    public bool IsMet(Location location)
    {
        var property = location.LocationProperties.GetProperty(PropertyType);

        // If the property is not set, consider the condition as met
        if (property == null)
        {
            return true;
        }

        return property.Equals(ExpectedValue);
    }
}