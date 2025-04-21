public class LocationAction
{
    public string LocationId { get; private set; }
    public EncounterTypes ActionType { get; private set; }

    public LocationAction(string locationId, EncounterTypes actionType)
    {
        LocationId = locationId;
        ActionType = actionType;
    }
}