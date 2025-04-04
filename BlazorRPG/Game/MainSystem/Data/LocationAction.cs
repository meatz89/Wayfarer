public class LocationAction
{
    public string LocationId { get; private set; }
    public BasicActionTypes ActionType { get; private set; }

    public LocationAction(string locationId, BasicActionTypes actionType)
    {
        LocationId = locationId;
        ActionType = actionType;
    }
}