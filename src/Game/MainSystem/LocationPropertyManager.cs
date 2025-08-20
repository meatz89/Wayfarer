using System.Collections.Generic;


public class LocationPropertyManager
{
private LocationSystem locationSystem;

public LocationPropertyManager(
    LocationSystem locationSystem)
{
    this.locationSystem = locationSystem;
}

public void UpdateLocationForTime(Location location, TimeBlocks CurrentTimeBlock)
{
    UpdateEnvironmentalProperties(location, CurrentTimeBlock);

    List<LocationSpot> locationSpots = locationSystem.GetLocationSpots(location.Id);
    SetClosed(locationSpots, CurrentTimeBlock);
}

private void SetClosed(List<LocationSpot> locationSpots, TimeBlocks CurrentTimeBlock)
{
    foreach (LocationSpot spot in locationSpots)
    {
        spot.IsClosed = !spot.CurrentTimeBlocks.Contains(CurrentTimeBlock);
    }
}

private static void UpdateEnvironmentalProperties(Location location, TimeBlocks CurrentTimeBlock)
{
    List<string> properties;

    switch (CurrentTimeBlock)
    {
        case TimeBlocks.Morning:
            properties = location.MorningProperties;
            break;
        case TimeBlocks.Afternoon:
            properties = location.AfternoonProperties;
            break;
        case TimeBlocks.Evening:
            properties = location.EveningProperties;
            break;
        case TimeBlocks.Night:
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
        else if (upperProp == "SHADOWY" || upperProp == "ThiefY")
            location.Illumination = Illumination.Thiefy;
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
