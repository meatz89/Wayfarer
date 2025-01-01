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
            .ForFeatureType(LocationSpotTypes.BasicShelter)
            .WithCoinCost(0)
            )
        // Street vendors sell cheap but limited food
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.HarborStreets)
            .ForFeatureType(LocationSpotTypes.GeneralStore)
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
            .ForFeatureType(LocationSpotTypes.GeneralStore)
            .WithCoinCost(2)
            .WithOutputResource(ResourceTypes.Food, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        // Luxury goods trader
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.MarketSquare)
            .ForFeatureType(LocationSpotTypes.SpecialtyShop)
            .WithInputResource(ResourceTypes.Planks, 1)
            .WithCoinReward(8)
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
            .ForFeatureType(LocationSpotTypes.ServingArea)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithEnergyCost(1, EnergyTypes.Social)
            .WithCoinReward(3)
        )
        // Tutorial common tables
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForFeatureType(LocationSpotTypes.CommonArea)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        // Tutorial bar counter
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForFeatureType(LocationSpotTypes.TavernBar)
            .WithCoinCost(3)
            .WithOutputResource(ResourceTypes.Food, 1)
        )
        // Tutorial corner
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForFeatureType(LocationSpotTypes.PrivateCorner)
            .WithEnergyCost(1, EnergyTypes.Focus)
        )
        // Tutorial back room (initially locked)
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForFeatureType(LocationSpotTypes.StorageRoom)
            .WithCoinCost(1)
            .SetAccessType(AccessTypes.Restricted)
        )
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

}