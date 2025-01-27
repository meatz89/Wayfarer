public class LocationContent
{
    public static Location WaysideInn => new LocationBuilder()
        .SetLocationType(LocationTypes.Residential)
        .ForLocation(LocationNames.WaysideInn)
        .WithArchetype(LocationArchetypes.Tavern)
        .WithCrowdLevel(CrowdDensity.Busy)
        .WithLocationScale(LocationScale.Medium)
        .AddLocationSpot(spot => spot
            .WithName("Hearth")
            //.ForActionType(BasicActionTypes.Recover)
            .WithAccessibility(Accessibility.Communal)
            .WithTemperature(Temperature.Warm))
        .AddLocationSpot(spot => spot
            .WithName("Bar")
            //.ForActionType(BasicActionTypes.Persuade)
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service))
        .AddLocationSpot(spot => spot
            .WithName("Quiet Corner")
            //.ForActionType(BasicActionTypes.Investigate)
            .WithRoomLayout(RoomLayout.Secluded)
            .WithAccessibility(Accessibility.Private))
        .AddLocationSpot(spot => spot
            .WithName("Central Area")
            //.ForActionType(BasicActionTypes.Mingle)
            .WithAccessibility(Accessibility.Public)
            .WithAtmosphere(Atmosphere.Social))
        .WithDifficultyLevel(1)
        .Build();

}