public class LocationActionsContent
{
    public static LocationActions Docks => new LocationActionBuilder()
        .ForLocation(LocationNames.Docks)
        .AddAction(BasicActionTypes.Investigate)
        .AddAction(BasicActionTypes.Labor)
        .Build();

    public static LocationActions Market => new LocationActionBuilder()
        .ForLocation(LocationNames.Market)
        .AddAction(BasicActionTypes.Investigate)
        .AddAction(BasicActionTypes.Observe)
        .Build();
}
