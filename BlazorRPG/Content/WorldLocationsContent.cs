public class WorldLocationsContent
{
    public static Location WaysideInn => new LocationBuilder()
        .ForLocation(LocationNames.WaysideInn)
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
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .Build();

    public static Location Market => new LocationBuilder()
    .ForLocation(LocationNames.Market)
    .AddLocationSpot(spot => spot
        .WithName("Market Stalls")
        .WithAccessibility(Accessibility.Public)
        .WithEngagement(Engagement.Service)
        .WithAtmosphere(Atmosphere.Social))
    .AddLocationSpot(spot => spot
        .WithName("Food Vendor")
        .WithAccessibility(Accessibility.Public)
        .WithEngagement(Engagement.Service)
        .WithTemperature(Temperature.Warm))
    .AddLocationSpot(spot => spot
        .WithName("Loading Area")
        .WithAccessibility(Accessibility.Public)
        .WithEngagement(Engagement.Service)
        .WithRoomLayout(RoomLayout.Open))
    .AddLocationSpot(spot => spot
        .WithName("Quiet Corner")
        .WithAccessibility(Accessibility.Private)
        .WithRoomLayout(RoomLayout.Secluded)
        .WithAtmosphere(Atmosphere.Aloof))
    .WithPlayerKnowledge(false)
    .WithDifficultyLevel(1)
    .Build();
}