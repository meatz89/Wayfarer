using System.Collections.Generic;


public class LocationConnection
{
    public string DestinationLocationId { get; set; }
    public List<RouteOption> RouteOptions { get; set; } = new List<RouteOption>();
}

