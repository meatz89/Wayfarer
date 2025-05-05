
public class EnvironmentalPropertyManager
{
    private readonly LocationSystem locationSystem;

    public EnvironmentalPropertyManager(
        LocationSystem locationSystem)
    {
        this.locationSystem = locationSystem;
    }

    public void UpdateLocationForTime(Location location, TimeWindows timeWindow)
    {
        List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(location.Id);

        SetClosed(locationSpots, timeWindow);

        switch (timeWindow)
        {
            case TimeWindows.Morning:
                SetIllumination(locationSpots, Illumination.Roguey);
                break;

            case TimeWindows.Afternoon:
                SetIllumination(locationSpots, Illumination.Bright);
                break;

            case TimeWindows.Evening:
                SetIllumination(locationSpots, Illumination.Roguey);
                break;

            case TimeWindows.Night:
                SetIllumination(locationSpots, Illumination.Dark);
                break;
        }
    }

    private void SetClosed(List<LocationSpot> locationSpots, TimeWindows timeWindow)
    {
        foreach (LocationSpot spot in locationSpots)
        {
            List<TimeWindows> timeWindows = spot.TimeWindows;
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