using System.Collections.Generic;

public class LocationConnection
{
    public string DestinationVenueId { get; set; }
    public List<RouteOption> RouteOptions { get; set; } = new List<RouteOption>();
}

