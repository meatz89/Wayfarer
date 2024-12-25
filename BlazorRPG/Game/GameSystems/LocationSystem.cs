public class LocationSystem
{
    private readonly List<LocationActions> locationActions;

    public LocationSystem(GameContentProvider contentProvider)
    {
        this.locationActions = contentProvider.GetLocationActions();
    }

    public List<BasicActionTypes> GetLocationActionsFor(LocationNames location)
    {
        LocationActions actionsForLocation = locationActions.FirstOrDefault(n => n.Location == location);
        if (actionsForLocation != null)
        {
            List<BasicActionTypes> actions = actionsForLocation.Actions;
            return actions;
        }

        return null;
    }
}