public class PendingTravel
{
    public Location TravelOrigin { get; set; }
    public string TravelDestination { get; set; }
    public TravelMethods TravelMethod { get; set; }
    public bool IsTravelPending => !string.IsNullOrEmpty(TravelDestination);

    public void Clear()
    {
        TravelOrigin = null;
        TravelDestination = null;
        TravelMethod = TravelMethods.Walking;
    }
}

