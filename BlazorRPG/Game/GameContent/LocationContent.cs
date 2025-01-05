public class LocationContent
{
    // === INDUSTRIAL CHOICES PLAYGROUND ===
    public static Location Industrial => new LocationBuilder()
        .SetLocationType(LocationTypes.Industrial)
        .ForLocation(LocationNames.GenericDocks)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Labor)
            .ForLocationSpot(LocationSpotNames.DocksideWarehouse)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithCoinReward(3)
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Gather)
            .ForLocationSpot(LocationSpotNames.FishingWharf)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithOutputResource(ResourceTypes.Fish, 1)
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Trade)
            .ForLocationSpot(LocationSpotNames.WharfMerchant)
            .WithCharacter(CharacterNames.Shopkeeper)
            .WithCoinCost(2)
            .WithOutputResource(ResourceTypes.Food, 1)
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Mingle)
            .ForLocationSpot(LocationSpotNames.DocksidePub)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        .Build();

    // === COMMERCIAL CHOICES PLAYGROUND ===
    public static Location Commercial => new LocationBuilder()
        .SetLocationType(LocationTypes.Commercial)
        .ForLocation(LocationNames.GenericMarket)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Labor)
            .ForLocationSpot(LocationSpotNames.MarketPorters)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithCoinReward(3)
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Gather)
            .ForLocationSpot(LocationSpotNames.HerbGarden)
            .WithEnergyCost(1, EnergyTypes.Focus)
            .WithOutputResource(ResourceTypes.Herbs, 1)
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Trade)
            .ForLocationSpot(LocationSpotNames.MarketBazaar)
            .WithCharacter(CharacterNames.Shopkeeper)
            .WithCoinCost(2)
            .WithOutputResource(ResourceTypes.Cloth, 1)
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Mingle)
            .ForLocationSpot(LocationSpotNames.MarketSquare)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        .Build();

    // === SOCIAL CHOICES PLAYGROUND ===
    public static Location Social => new LocationBuilder()
        .SetLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.GenericTavern)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Labor)
            .ForLocationSpot(LocationSpotNames.TavernKitchen)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithCoinReward(3)
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Gather)
            .ForLocationSpot(LocationSpotNames.CellarPantry)
            .WithEnergyCost(1, EnergyTypes.Focus)
            .WithOutputResource(ResourceTypes.Food, 1)
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Trade)
            .ForLocationSpot(LocationSpotNames.TavernBarterTable)
            .WithCharacter(CharacterNames.Bartender)
            .WithCoinCost(2)
            .WithOutputResource(ResourceTypes.Food, 1)
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Mingle)
            .ForLocationSpot(LocationSpotNames.InnFireplace)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        .Build();

    // === NATURE CHOICES PLAYGROUND ===
    public static Location Nature => new LocationBuilder()
        .SetLocationType(LocationTypes.Nature)
        .ForLocation(LocationNames.GenericForest)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Labor)
            .ForLocationSpot(LocationSpotNames.LumberYard)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithCoinReward(3)
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Gather)
            .ForLocationSpot(LocationSpotNames.MysticGrove)
            .WithEnergyCost(1, EnergyTypes.Focus)
            .WithOutputResource(ResourceTypes.Wood, 1)
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Trade)
            .ForLocationSpot(LocationSpotNames.WoodworkerCabin)
            .WithCharacter(CharacterNames.ForestTrader)
            .WithCoinCost(2)
            .WithOutputResource(ResourceTypes.Wood, 1)
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Mingle)
            .ForLocationSpot(LocationSpotNames.GroveShrine)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        .Build();

    // === RESIDENTIAL DISTRICT ===
    public static Location LionsHeadTavern => new LocationBuilder()
        .SetLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.LionsHeadTavern)
        .AddTravelConnection(LocationNames.GenericDocks)
        .AddTravelConnection(LocationNames.GenericTavern)
        .AddTravelConnection(LocationNames.GenericMarket)
        .AddTravelConnection(LocationNames.GenericForest)
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .AddLocationSpot(feature => feature
            .ForActionType(BasicActionTypes.Mingle)
            .ForLocationSpot(LocationSpotNames.TavernKitchen)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        .Build();
}
