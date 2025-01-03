public class LocationContent
{
    // === PORT DISTRICT ===
    public static Location HarborStreets => new LocationBuilder()
        .ForLocation(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Social)
        .AddTravelConnection(LocationNames.Docks)          // Directly connected to the docks
        .AddTravelConnection(LocationNames.MarketSquare)   // Connect to the main market area
        .AddTravelConnection(LocationNames.LionsHeadTavern) // A place to rest near the harbor
                                                            // Poor travelers need a place to sleep
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.HarborStreets)
            .ForLocationSpot(LocationSpotNames.BasicShelter)
            .WithCoinCost(0)
            .WithEnergyReward(1, EnergyTypes.Physical)
        )
        // Street vendors sell cheap but limited food
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.HarborStreets)
            .ForLocationSpot(LocationSpotNames.GeneralStore)
            .WithCoinCost(1)
            .WithOutputResource(ResourceTypes.Food, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    // === MARKET DISTRICT ===
    public static Location MarketSquare => new LocationBuilder()
        .ForLocation(LocationNames.MarketSquare)
        .SetLocationType(LocationTypes.Commerce)
        .AddTravelConnection(LocationNames.HarborStreets) // Main connection to the rest of the town
        .AddTravelConnection(LocationNames.ArtisanRow)    // Goods flow to/from artisan workshops
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // Standard food merchant
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.MarketSquare)
            .ForLocationSpot(LocationSpotNames.GeneralStore)
            .WithCharacter(CharacterNames.WealthyMerchant)
            .WithCoinCost(2)
            .WithOutputResource(ResourceTypes.Food, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    // === RESIDENTIAL DISTRICT ===
    public static Location Tavern => new LocationBuilder()
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationType(LocationTypes.Social)
        .AddTravelConnection(LocationNames.HarborStreets)
        .AddTravelConnection(LocationNames.MarketSquare)
        // Tutorial entrance spot
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.ServingArea)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithEnergyCost(1, EnergyTypes.Social)
            .WithCoinReward(3)
        )
        // Tutorial common tables
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.CommonArea)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        // Tutorial bar counter
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .WithCharacter(CharacterNames.Bartender)
            .ForLocationSpot(LocationSpotNames.TavernBar)
            .WithCoinCost(3)
            .WithOutputResource(ResourceTypes.Food, 1)
        )
        // Tutorial corner
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.PrivateCorner)
            .WithEnergyCost(1, EnergyTypes.Focus)
        )
        // Tutorial back room (initially locked)
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.GoodShelter)
            .WithCoinCost(1)
            .WithEnergyReward(5, EnergyTypes.Physical)
            .WithEnergyReward(5, EnergyTypes.Focus)
            .WithEnergyReward(5, EnergyTypes.Social)
            .SetAccessType(AccessTypes.Restricted)
        )
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

}