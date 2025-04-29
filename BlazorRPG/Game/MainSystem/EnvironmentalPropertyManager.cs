
public class EnvironmentalPropertyManager
{
    private readonly LocationSystem locationSystem;

    public EnvironmentalPropertyManager(
        LocationSystem locationSystem)
    {
        this.locationSystem = locationSystem;
    }

    public void UpdateLocationForTime(Location location, TimeWindow timeWindow)
    {
        List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(location.Name);
        
        SetClosed(locationSpots, timeWindow);

        switch (timeWindow)
        {
            case TimeWindow.Morning:
            case TimeWindow.Afternoon:
                SetIllumination(locationSpots, Illumination.Bright);
                break;

            case TimeWindow.Evening:
                SetIllumination(locationSpots, Illumination.Shadowy);
                break;

            case TimeWindow.Night:
                SetIllumination(locationSpots, Illumination.Dark);
                break;
        }

        // Location-specific time adjustments based on location type
        switch (location.LocationType)
        {
            case LocationTypes.Hub:
                UpdateHubForTime(locationSpots, timeWindow);
                break;

                // Other location types with specific behaviors
        }
    }

    private void SetClosed(List<LocationSpot> locationSpots, TimeWindow timeWindow)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            spot.IsClosed = !spot.TimeWindowsOpen.Contains(timeWindow);
        }
    }

    private static void UpdateHubForTime(List<LocationSpot> locationSpots, TimeWindow timeWindow)
    {
        // Hub locations (towns, cities) behaviors
        switch (timeWindow)
        {
            case TimeWindow.Morning:
                SetPopulation(locationSpots, Population.Quiet);
                SetAtmosphere(locationSpots, Atmosphere.Tense);
                break;

            case TimeWindow.Afternoon:
                SetPopulation(locationSpots, Population.Crowded);
                SetAtmosphere(locationSpots, Atmosphere.Tense);
                break;

            case TimeWindow.Evening:
                SetPopulation(locationSpots, Population.Crowded);
                SetAtmosphere(locationSpots, Atmosphere.Chaotic);
                break;

            case TimeWindow.Night:
                SetPopulation(locationSpots, Population.Quiet);
                SetAtmosphere(locationSpots, Atmosphere.Rough);
                break;
        }
    }

    private static void SetIllumination(List<LocationSpot> locationSpots, Illumination illumination)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            spot.Illumination = illumination;
        }
    }

    private static void SetPopulation(List<LocationSpot> locationSpots, Population population)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            spot.Population = population;
        }
    }

    private static void SetAtmosphere(List<LocationSpot> locationSpots, Atmosphere atmosphere)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            spot.Atmosphere = atmosphere;
        }
    }

    private static void SetPhysical(List<LocationSpot> locationSpots, Physical physical)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            spot.Physical = physical;
        }
    }

}