public enum InformationTypes
{
    Location
}

public abstract class Information
{
    public InformationTypes InformationType;
}

public class LocationInformation : Information
{
    public LocationInformation(LocationNames locationName)
    {
        InformationType = InformationTypes.Location;
        LocationNames = locationName;
    }

    public LocationNames LocationNames;
}