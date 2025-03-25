public class WorldLocationsContent
{
    public static Location Village => new LocationBuilder()
        .ForLocation(LocationNames.Village)
        .AddLocationSpot(spot => spot
            .WithName("Village Square")
            .WithPopulation(Population.Crowded)
            .WithEconomic(Economic.Wealthy)
            .WithAtmosphere(Atmosphere.Formal)
            .AddAction(ActionNames.VillageGathering))
        .AddLocationSpot(spot => spot
            .WithName("Market Stall")
            .WithPopulation(Population.Crowded)
            .WithEconomic(Economic.Commercial)
            .AddAction(ActionNames.TradeGoods))
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .AddTravelConnection(LocationNames.Forest)
        .AddTravelConnection(LocationNames.Tavern)
        .Build();

    public static Location Forest => new LocationBuilder()
        .ForLocation(LocationNames.Forest)
        .AddLocationSpot(spot => spot
            .WithName("Forest Path")
            .WithPopulation(Population.Isolated)
            .WithPhysical(Physical.Hazardous)
            .AddAction(ActionNames.ForestTravel))
        .AddLocationSpot(spot => spot
            .WithName("Hidden Clearing")
            .WithIllumination(Illumination.Bright)
            .WithPopulation(Population.Isolated)
            .AddAction(ActionNames.SecretMeeting))
        .WithPlayerKnowledge(false)
        .WithDifficultyLevel(2)
        .AddTravelConnection(LocationNames.Village)
        .Build();

    public static Location Tavern => new LocationBuilder()
        .ForLocation(LocationNames.Tavern)
        .AddLocationSpot(spot => spot
            .WithName("Private Corner")
            .WithIllumination(Illumination.Dark)
            .WithPopulation(Population.Isolated)
            .AddAction(ActionNames.SecretDeal))
        .AddLocationSpot(spot => spot
            .WithName("Innkeeper's Counter")
            .WithPopulation(Population.Quiet)
            .WithEconomic(Economic.Commercial)
            .AddAction(ActionNames.RentRoom))
        .AddLocationSpot(spot => spot
            .WithName("Notice Board")
            .WithPopulation(Population.Quiet)
            .WithEconomic(Economic.Humble)
            .AddAction(ActionNames.FindQuests))
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .AddTravelConnection(LocationNames.Village)
        .Build();
}
