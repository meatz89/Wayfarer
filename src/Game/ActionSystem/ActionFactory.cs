public class ActionFactory
{
    private WorldState worldState;

    public ActionFactory(
        ActionRepository actionRepository,
        GameWorld gameWorld)
    {
        this.worldState = gameWorld.WorldState;
    }

    public LocationAction CreateActionFromTemplate(
        ActionDefinition template,
        string location,
        string locationSpot,
        ActionExecutionTypes actionType)
    {
        LocationAction locationAction = new LocationAction();
        locationAction.ActionId = template.Id;
        locationAction.Name = template.Name;
        locationAction.ObjectiveDescription = template.Description;
        locationAction.LocationId = location;
        locationAction.LocationSpotId = locationSpot;

        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            locationAction.DestinationLocation = template.MoveToLocation;
        }
        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            locationAction.DestinationLocationSpot = template.MoveToLocationSpot;
        }

        locationAction.RequiredCardType = SkillCategories.Physical;
        locationAction.ActionExecutionType = actionType;

        locationAction.Requirements = CreateRequirements(template);

        int actionCost = 1;
        locationAction.Requirements.Add(new ActionPointRequirement(actionCost));
        return locationAction;
    }

    private List<IRequirement> CreateRequirements(ActionDefinition template)
    {
        List<IRequirement> requirements = new();
        // Time window requirement
        if (template.TimeWindows != null && template.TimeWindows.Count > 0)
        {
            requirements.Add(new TimeWindowRequirement(template.TimeWindows));
        }
        return requirements;
    }

}