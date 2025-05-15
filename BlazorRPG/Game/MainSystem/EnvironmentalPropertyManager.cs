public class EnvironmentalPropertyManager
{
    private readonly LocationSystem locationSystem;

    public EnvironmentalPropertyManager(
        LocationSystem locationSystem)
    {
        this.locationSystem = locationSystem;
    }

    public void UpdateLocationForTime(Location location, TimeWindowTypes timeWindow)
    {
        UpdateEnvironmentalProperties(location, timeWindow);

        List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(location.Id);
        SetClosed(locationSpots, timeWindow);
    }

    private void SetClosed(List<LocationSpot> locationSpots, TimeWindowTypes timeWindow)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            TimeWindows timeWindows = spot.TimeWindows;
            spot.IsClosed = !timeWindows.Contains(timeWindow);
        }
    }

    private static void UpdateEnvironmentalProperties(Location location, TimeWindowTypes timeWindow)
    {
        List<string> properties;

        switch (timeWindow)
        {
            case TimeWindowTypes.Morning:
                properties = location.MorningProperties;
                break;
            case TimeWindowTypes.Afternoon:
                properties = location.AfternoonProperties;
                break;
            case TimeWindowTypes.Evening:
                properties = location.EveningProperties;
                break;
            case TimeWindowTypes.Night:
                properties = location.NightProperties;
                break;
            default:
                properties = location.MorningProperties; // Default to morning
                break;
        }

        foreach (string prop in properties)
        {
            string upperProp = prop.ToUpper();

            // Set Illumination
            if (upperProp == "WELL-LIT" || upperProp == "BRIGHT")
                location.Illumination = Illumination.Bright;
            else if (upperProp == "SHADOWY" || upperProp == "ROGUEY")
                location.Illumination = Illumination.Roguey;
            else if (upperProp == "DARK")
                location.Illumination = Illumination.Dark;

            // Set Population
            if (upperProp == "CROWDED")
                location.Population = Population.Crowded;
            else if (upperProp == "QUIET")
                location.Population = Population.Quiet;
            else if (upperProp == "SCHOLARLY")
                location.Population = Population.Scholarly;

            // Set Physical
            if (upperProp == "CONFINED" || upperProp == "INTERIOR")
                location.Physical = Physical.Confined;
            else if (upperProp == "EXPANSIVE" || upperProp == "OUTDOOR")
                location.Physical = Physical.Expansive;
            else if (upperProp == "HAZARDOUS")
                location.Physical = Physical.Hazardous;

            // Set Atmosphere
            if (upperProp == "TENSE")
                location.Atmosphere = Atmosphere.Tense;
            else if (upperProp == "FORMAL")
                location.Atmosphere = Atmosphere.Formal;
            else if (upperProp == "CHAOTIC")
                location.Atmosphere = Atmosphere.Chaotic;
            else if (upperProp == "CALM")
                location.Atmosphere = Atmosphere.Calm;

            // Add additional mappings for other properties like COMMERCE, URBAN, etc.
            // These might set additional properties or services on the location
            if (upperProp == "COMMERCE")
                location.AvailableServices.Add(ServiceTypes.Market);
            if (upperProp == "URBAN")
                location.LocationType = LocationTypes.Settlement;
            if (upperProp == "TRAVEL")
                location.LocationType = LocationTypes.Connective;
            if (upperProp == "SECLUDED")
                location.LocationType = LocationTypes.Rest;
        }
    }
}