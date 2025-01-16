public class LocationPropertyChoiceEffect
{
    public LocationPropertyTypes PropertyType { get; }
    public object PropertyValue { get; }
    public string Description { get; }

    public LocationPropertyChoiceEffect(
        LocationPropertyTypes propertyType,
        object propertyValue,
        string description)
    {
        PropertyType = propertyType;
        PropertyValue = propertyValue;
        Description = description;
    }
}