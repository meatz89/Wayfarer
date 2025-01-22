public class LocationSpot
{
    public string Name { get; set; }
    public LocationNames LocationName { get; set; }
    public List<ActionImplementation> Actions { get; set; } = new();
    public CharacterNames? Character { get; set; } = null;
    public LocationSpotProperties SpotProperties { get; set; }

    public void AddAction(ActionImplementation action)
    {
        Actions.Add(action);
    }

    public LocationSpot(
        string name,
        LocationNames locationName,
        LocationSpotProperties properties)
    {
        Name = name;
        LocationName = locationName;
        SpotProperties = properties;
    }
}
