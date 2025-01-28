public class LocationContent
{
    public static Location WaysideInn => new LocationBuilder()
        .ForLocation(LocationNames.WaysideInn)
        .WithArchetype(LocationArchetypes.Tavern)
        .SetLocationType(LocationTypes.Residential)
        .WithOpportunity(OpportunityTypes.Commercial)
        .WithCrowdLevel(CrowdDensity.Bustling)
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
        .WithArchetype(LocationArchetypes.Market)
        .SetLocationType(LocationTypes.Commercial)
        .WithOpportunity(OpportunityTypes.Commercial)
        .WithCrowdLevel(CrowdDensity.Busy)
        .AddLocationSpot(spot => spot
            .WithName("Market Stalls")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(Engagement.Service))
        .WithPlayerKnowledge(false)
        .WithDifficultyLevel(1)
        .Build();

}