public enum InformationTypes
{
    Location,
    ActionOpportunity
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

    public LocationNames LocationNames { get; }
}

public class ActionOpportunityInformation : Information
{
    public ActionOpportunityInformation(LocationNames locationName, BasicActionTypes actionType)
    {
        InformationType = InformationTypes.ActionOpportunity;
        LocationName = locationName;
        ActionType = actionType;
    }

    public LocationNames LocationName { get; }
    public BasicActionTypes ActionType { get; }
}