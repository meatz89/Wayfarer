public class LocationContent
{
    // The Wanderer's Welcome represents our social challenge
    public static Location WaysideInn => new LocationBuilder()
        .SetLocationType(LocationTypes.Residential)
        .ForLocation(LocationNames.WaysideInn)
        .WithArchetype(LocationArchetypes.Tavern)
        .WithCrowdLevel(CrowdDensity.Busy)
        .WithLocationScale(LocationScale.Medium)
        .AddLocationSpot(spot => spot
            .WithName("Hearth")
            //.ForActionType(BasicActionTypes.Recover)
            .WithLocationProperties(properties => properties
                .WithAccessability(Accessability.Communal)
                .WithTemperature(Temperature.Warm)))
        .AddLocationSpot(spot => spot
            .WithName("Bar")
            //.ForActionType(BasicActionTypes.Persuade)
            .WithLocationProperties(properties => properties
                .WithAccessability(Accessability.Public)
                .WithEngagement(Engagement.Service)))
        .AddLocationSpot(spot => spot
            .WithName("Quiet Corner")
            //.ForActionType(BasicActionTypes.Investigate)
            .WithLocationProperties(properties => properties
                .WithRoomLayout(RoomLayout.Secluded)
                .WithAccessability(Accessability.Private)))
        .AddLocationSpot(spot => spot
            .WithName("Central Area")
            //.ForActionType(BasicActionTypes.Mingle)
            .WithLocationProperties(properties => properties
                .WithAccessability(Accessability.Public)
                .WithAtmosphere(Atmosphere.Social)))
        .WithDifficultyLevel(1)
        .Build();

}