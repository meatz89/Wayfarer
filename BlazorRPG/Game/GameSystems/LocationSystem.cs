public class LocationSystem
{
    private readonly List<LocationProperties> locationProperties;

    public LocationSystem(GameContentProvider contentProvider)
    {
        this.locationProperties = contentProvider.GetLocationProperties();
    }

    public List<BasicActionTypes> GetLocationActionsFor(LocationNames location)
    {
        LocationProperties actionsForLocation = locationProperties.FirstOrDefault(n => n.Location == location);
        if (actionsForLocation != null)
        {
            List<BasicActionTypes> actions = new();
            actions.Add(actionsForLocation.PrimaryAction);
            return actions;
        }

        return null;
    }
}