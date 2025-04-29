
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
        List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(location.Id);

        SetClosed(locationSpots, timeWindow);

        switch (timeWindow)
        {
            case TimeWindow.Morning:
                SetIllumination(locationSpots, Illumination.Shadowy);
                break;

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
    }

    private void SetClosed(List<LocationSpot> locationSpots, TimeWindow timeWindow)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            List<TimeWindow> timeWindows = spot.TimeWindows;
            spot.IsClosed = !timeWindows.Contains(timeWindow);
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