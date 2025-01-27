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
            .WithAccessibility(Accessibility.Communal)
            .WithTemperature(Temperature.Warm))
        .AddLocationSpot(spot => spot
            .WithName("Bar")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service))
        .AddLocationSpot(spot => spot
            .WithName("Quiet Corner")
            .WithRoomLayout(RoomLayout.Secluded)
            .WithAccessibility(Accessibility.Private))
        .AddLocationSpot(spot => spot
            .WithName("Central Area")
            .WithAccessibility(Accessibility.Public)
            .WithAtmosphere(Atmosphere.Social))
        .WithDifficultyLevel(1)
        .Build();

}