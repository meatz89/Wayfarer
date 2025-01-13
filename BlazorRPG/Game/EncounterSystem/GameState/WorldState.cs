public class WorldState
{
    // Current location tracking
    public Location CurrentLocation { get; set; }
    public LocationSpot CurrentLocationSpot { get; set; }

    // Navigation options
    public List<UserLocationTravelOption> CurrentTravelOptions { get; set; } = new();
    public List<UserLocationSpotOption> CurrentLocationSpotOptions { get; set; } = new();

    // Time tracking - moved here since it affects world state
    public int CurrentTimeInHours { get; set; }
    public TimeSlots CurrentTimeSlot { get; private set; } = TimeSlots.Morning;

    public void SetNewLocation(Location location)
    {
        CurrentLocation = location;
        // Default to first spot in new location
        CurrentLocationSpot = location.LocationSpots.FirstOrDefault();
    }

    public void SetNewLocationSpot(LocationSpot locationSpot)
    {
        CurrentLocationSpot = locationSpot;
    }

    public void SetCurrentTravelOptions(List<UserLocationTravelOption> options)
    {
        CurrentTravelOptions = options;
    }

    public void SetCurrentLocationSpotOptions(List<UserLocationSpotOption> options)
    {
        CurrentLocationSpotOptions = options;
    }

    public void DetermineCurrentTimeSlot(int timeSlot)
    {
        CurrentTimeSlot = timeSlot switch
        {
            0 => TimeSlots.Night,
            1 => TimeSlots.Morning,
            2 => TimeSlots.Afternoon,
            _ => TimeSlots.Evening
        };
    }
}