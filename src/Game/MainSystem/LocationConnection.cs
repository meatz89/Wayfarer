using System.Collections.Generic;

namespace Wayfarer.Game.MainSystem
{

public class LocationConnection
{
    public string DestinationLocationId { get; set; }
    public List<RouteOption> RouteOptions { get; set; } = new List<RouteOption>();
}

}
