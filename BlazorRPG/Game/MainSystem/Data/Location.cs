
public class Location
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> ConnectedTo { get; set; } = new List<string>();
    public List<IEnvironmentalProperty> EnvironmentalProperties { get; set; } = new List<IEnvironmentalProperty>();
    public List<LocationSpot> LocationSpots { get; set; } = new List<LocationSpot>();
    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; }
    public int Difficulty { get; set; }
    public string DetailedDescription { get; set; }
    public string History { get; set; }
    public string PointsOfInterest { get; set; }
    public List<EnvironmentPropertyTag> StrategicTags { get; set; } = new List<EnvironmentPropertyTag>();
    public List<NarrativeTag> NarrativeTags { get; set; } = new List<NarrativeTag>();
    public int Depth { get; set; }
    public LocationTypes LocationType { get; set; } = LocationTypes.Connective;
    public List<ServiceTypes> AvailableServices { get; set; } = new List<ServiceTypes>();
    public int DiscoveryBonusXP { get; set; }
    public int DiscoveryBonusCoins { get; set; }
    public bool HasBeenVisited { get; set; }
    public int VisitCount { get; set; }
    public bool PlayerKnowledge { get; }

    public void AddSpot(LocationSpot spot)
    {
        LocationSpots.Add(spot);
    }

    // Called when time changes to update time-dependent properties
    public void OnTimeChanged(TimeWindows newTimeWindow)
    {
        // Each location type can have specific time-based property adjustments
        switch (LocationType)
        {
            case LocationTypes.Village:
                UpdateVillagePropertiesForTime(newTimeWindow);
                break;

            case LocationTypes.Forest:
                UpdateForestPropertiesForTime(newTimeWindow);
                break;

                // Other location types...
        }

        // Update location spots as well
        foreach (LocationSpot spot in LocationSpots)
        {
            spot.OnTimeChanged(newTimeWindow);
        }
    }

    private void UpdateVillagePropertiesForTime(TimeWindows timeWindow)
    {
        // Villages are busy during day, quiet at night
        switch (timeWindow)
        {
            case TimeWindows.Morning:
            case TimeWindows.Afternoon:
                SetPopulationProperty(Population.Crowded);
                break;

            case TimeWindows.Evening:
                SetPopulationProperty(Population.Quiet);
                break;

            case TimeWindows.Night:
                SetPopulationProperty(Population.Scholarly); // Very quiet/empty
                break;
        }
    }

    private void UpdateForestPropertiesForTime(TimeWindows timeWindow)
    {
        // Forests become more dangerous at night
        switch (timeWindow)
        {
            case TimeWindows.Night:
                SetAtmosphereProperty(Atmosphere.Rough);
                break;

            default:
                SetAtmosphereProperty(Atmosphere.Formal);
                break;
        }
    }

    private void SetPopulationProperty(Population population)
    {
        UpdateEnvironmentalProperty<Population>(population);
    }

    private void SetAtmosphereProperty(Atmosphere atmosphere)
    {
        UpdateEnvironmentalProperty<Atmosphere>(atmosphere);
    }

    private void UpdateEnvironmentalProperty<T>(T newProperty) where T : class, IEnvironmentalProperty
    {
        // Find and replace existing property of this type
        for (int i = 0; i < EnvironmentalProperties.Count; i++)
        {
            if (EnvironmentalProperties[i] is T)
            {
                EnvironmentalProperties[i] = newProperty;
                return;
            }
        }

        // If no property of this type exists, add it
        EnvironmentalProperties.Add(newProperty);
    }

    public void SetIllumination(Illumination illumination)
    {
        // Find and replace existing illumination property
        for (int i = 0; i < EnvironmentalProperties.Count; i++)
        {
            if (EnvironmentalProperties[i] is Illumination)
            {
                EnvironmentalProperties[i] = illumination;
                return;
            }
        }

        // If no illumination property exists, add one
        EnvironmentalProperties.Add(illumination);
    }


    public Location()
    {

    }

    public Location(
        string locationName,
        string description,
        List<string> travelConnections,
        List<LocationSpot> locationSpots,
        int difficultyLevel,
        bool playerKnowledge)
    {
        Name = locationName.ToString();
        Description = description;
        ConnectedTo = travelConnections;
        LocationSpots = locationSpots;
        Difficulty = difficultyLevel;
        PlayerKnowledge = playerKnowledge;
    }
}
