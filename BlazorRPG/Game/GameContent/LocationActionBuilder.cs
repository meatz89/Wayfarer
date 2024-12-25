
public class LocationActionBuilder
{
    private LocationNames location;
    private List<BasicActionTypes> actions = new();

    internal LocationActionBuilder ForLocation(LocationNames location)
    {
        this.location = location;
        return this;
    }

    public LocationActionBuilder AddAction(BasicActionTypes basicActionType)
    {
        actions.Add(basicActionType);
        return this;
    }

    public LocationActions Build()
    {
        return new LocationActions
        {
            Location = location,
            Actions = actions
        };
    }
}