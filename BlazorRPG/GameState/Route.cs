public class Route
{
    public Location Origin { get; private set; }
    public Location Destination { get; private set; }
    public int TravelTime { get; private set; }
    public int MoneyCost { get; private set; }
    public int ConditionCost { get; private set; }
}
