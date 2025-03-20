public class WorldLocationsContent
{
    public static Location Library => new LocationBuilder()
        .ForLocation(LocationNames.AncientLibrary)
        .AddLocationSpot(spot => spot
            .WithName("Entrance")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Service)
            .AddAction(ActionNames.LibraryResearch))
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .AddTravelConnection(LocationNames.Market)
        .Build();

    public static Location Marketplace => new LocationBuilder()
        .ForLocation(LocationNames.Market)
        .AddLocationSpot(spot => spot
            .WithName("Marketplace")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Commercial)
            .AddAction(ActionNames.MerchantPersuasion))
        .AddLocationSpot(spot => spot
            .WithName("Backalleys")
            .WithAccessibility(Accessibility.Private)
            .WithEngagement(LocationType.Rest)
            .AddAction(ActionNames.BackalleyTravel))
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .AddTravelConnection(LocationNames.AncientLibrary)
        .Build();
}