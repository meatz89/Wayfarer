/// <summary>
/// Special implementation of IEnvironmentalProperty that matches any property of the same type
/// </summary>
public class LocationPropertyAny : ILocationProperty
{
    private string _propertyType;

    public LocationPropertyAny(string propertyType)
    {
        _propertyType = propertyType;
    }

    public string GetPropertyType()
    {
        return _propertyType;
    }

    public string GetPropertyValue()
    {
        return "Any";
    }

    public override bool Equals(object obj)
    {
        if (obj is ILocationProperty other)
        {
            // Match any property with the same type
            return GetPropertyType() == other.GetPropertyType();
        }
        return false;
    }

    public override int GetHashCode()
    {
        return _propertyType.GetHashCode();
    }

    public override string ToString()
    {
        return $"Any {GetPropertyType()}";
    }
}
