public class PendingTravel
{
    public string Destination { get; set; }
    public TravelMethods TravelMethod { get; set; }
    public bool IsTravelPending => !string.IsNullOrEmpty(Destination);

    public void Clear()
    {
        Destination = null;
        TravelMethod = TravelMethods.Walking;
    }
}

