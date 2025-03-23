public class WorldLocationsContent
{
    public static Location Village => new LocationBuilder()
        .ForLocation(LocationNames.Village)
        .AddLocationSpot(spot => spot
            .WithName("Village Square")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Commercial)
            .AddAction(ActionNames.VillageGathering))
        .AddLocationSpot(spot => spot
            .WithName("Elder's Home")
            .WithAccessibility(Accessibility.Private)
            .WithEngagement(LocationType.Service)
            .AddAction(ActionNames.ElderCounsel))
        .AddLocationSpot(spot => spot
            .WithName("Market Stall")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Commercial)
            .AddAction(ActionNames.TradeGoods))
        .AddLocationSpot(spot => spot
            .WithName("Village Well")
            .WithAccessibility(Accessibility.Communal)
            .WithEngagement(LocationType.Rest)
            .AddAction(ActionNames.GatherInformation))
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .AddTravelConnection(LocationNames.Forest)
        .AddTravelConnection(LocationNames.Tavern)
        .Build();

    public static Location Forest => new LocationBuilder()
        .ForLocation(LocationNames.Forest)
        .AddLocationSpot(spot => spot
            .WithName("Forest Path")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Rest)
            .AddAction(ActionNames.ForestTravel))
        .AddLocationSpot(spot => spot
            .WithName("Hunter's Camp")
            .WithAccessibility(Accessibility.Communal)
            .WithEngagement(LocationType.Rest)
            .AddAction(ActionNames.HuntingTraining))
        .AddLocationSpot(spot => spot
            .WithName("Ancient Oak")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Service)
            .AddAction(ActionNames.NatureStudy))
        .AddLocationSpot(spot => spot
            .WithName("Hidden Clearing")
            .WithAccessibility(Accessibility.Private)
            .WithEngagement(LocationType.Rest)
            .AddAction(ActionNames.SecretMeeting))
        .WithPlayerKnowledge(false)
        .WithDifficultyLevel(2)
        .AddTravelConnection(LocationNames.Village)
        .Build();

    public static Location Tavern => new LocationBuilder()
        .ForLocation(LocationNames.Tavern)
        .AddLocationSpot(spot => spot
            .WithName("Main Hall")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Commercial)
            .AddAction(ActionNames.DrinkAndGossip))
        .AddLocationSpot(spot => spot
            .WithName("Private Corner")
            .WithAccessibility(Accessibility.Private)
            .WithEngagement(LocationType.Rest)
            .AddAction(ActionNames.SecretDeal))
        .AddLocationSpot(spot => spot
            .WithName("Innkeeper's Counter")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Commercial)
            .AddAction(ActionNames.RentRoom))
        .AddLocationSpot(spot => spot
            .WithName("Notice Board")
            .WithAccessibility(Accessibility.Public)
            .WithEngagement(LocationType.Service)
            .AddAction(ActionNames.FindQuests))
        .AddLocationSpot(spot => spot
            .WithName("Backroom")
            .WithAccessibility(Accessibility.Private)
            .WithEngagement(LocationType.Service)
            .AddAction(ActionNames.MeetContact))
        .WithPlayerKnowledge(true)
        .WithDifficultyLevel(1)
        .AddTravelConnection(LocationNames.Village)
        .Build();
}
