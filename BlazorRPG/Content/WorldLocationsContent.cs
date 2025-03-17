public class WorldLocationsContent
{
    public static Location Library => new LocationBuilder()
        .ForLocation(LocationNames.WaysideInn)
        .AddLocationSpot(spot => spot
            .WithName("Entrance")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service))
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .Build();

}