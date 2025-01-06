public class LocationSpotBuilder
{
    private string name;
    private BasicActionTypes actionType;
    private LocationNames locationName;

    public LocationSpotBuilder(LocationNames locationName)
    {
        this.locationName = locationName;
    }

    public LocationSpotBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public LocationSpotBuilder ForActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public LocationSpot Build()
    {
        // Validation: Ensure name and actionType are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("LocationSpot must have a name.");
        }
        if (actionType == BasicActionTypes.Wait) // Example check
        {
            throw new InvalidOperationException("LocationSpot must have a valid BasicActionType.");
        }

        return new LocationSpot(name, locationName, actionType);
    }
}