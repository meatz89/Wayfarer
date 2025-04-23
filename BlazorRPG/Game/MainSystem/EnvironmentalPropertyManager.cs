public static class EnvironmentalPropertyManager
{
    public static void UpdateLocationForTime(Location location, TimeWindows timeWindow)
    {
        switch (timeWindow)
        {
            case TimeWindows.Morning:
            case TimeWindows.Afternoon:
                SetIllumination(location, Illumination.Bright);
                break;

            case TimeWindows.Evening:
                SetIllumination(location, Illumination.Shadowy);
                break;

            case TimeWindows.Night:
                SetIllumination(location, Illumination.Dark);
                break;
        }

        // Location-specific time adjustments based on location type
        switch (location.LocationType)
        {
            case LocationTypes.Hub:
                UpdateHubForTime(location, timeWindow);
                break;

                // Other location types with specific behaviors
        }
    }

    private static void UpdateHubForTime(Location location, TimeWindows timeWindow)
    {
        // Hub locations (towns, cities) behaviors
        switch (timeWindow)
        {
            case TimeWindows.Morning:
                SetPopulation(location, Population.Quiet);
                SetAtmosphere(location, Atmosphere.Tense);
                break;

            case TimeWindows.Afternoon:
                SetPopulation(location, Population.Crowded);
                SetAtmosphere(location, Atmosphere.Tense);
                break;

            case TimeWindows.Evening:
                SetPopulation(location, Population.Crowded);
                SetAtmosphere(location, Atmosphere.Chaotic);
                break;

            case TimeWindows.Night:
                SetPopulation(location, Population.Quiet);
                SetAtmosphere(location, Atmosphere.Rough);
                break;
        }
    }

    private static void SetIllumination(Location location, Illumination illumination)
    {
        foreach (LocationSpot spot in location.LocationSpots)
        {
            spot.Illumination = illumination;
        }
    }

    private static void SetPopulation(Location location, Population population)
    {
        foreach (LocationSpot spot in location.LocationSpots)
        {
            spot.Population = population;
        }
    }

    private static void SetAtmosphere(Location location, Atmosphere atmosphere)
    {
        foreach (LocationSpot spot in location.LocationSpots)
        {
            spot.Atmosphere = atmosphere;
        }
    }

    private static void SetPhysical(Location location, Physical physical)
    {
        foreach (LocationSpot spot in location.LocationSpots)
        {
            spot.Physical = physical;
        }
    }

}