public class LocationSpot
{
    public string Name { get; set; }
    public LocationNames LocationName { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<ActionImplementation> Actions { get; set; } = new(); // Actions available at this spot
    public CharacterNames? Character { get; set; } = null;
    public LocationProperties SpotProperties { get; set; }

    public void AddAction(ActionImplementation action)
    {
        Actions.Add(action);
    }

    public LocationSpot(string name, LocationNames locationName, BasicActionTypes actionType, LocationProperties properties)
    {
        Name = name;
        LocationName = locationName;
        ActionType = actionType;
        SpotProperties = properties;
    }
}
