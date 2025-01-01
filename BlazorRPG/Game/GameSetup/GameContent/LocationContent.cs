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

    public static Location Docks => new LocationBuilder()
        .ForLocation(LocationNames.Docks)
        .SetLocationType(LocationTypes.Industry)
        .AddTravelConnection(LocationNames.HarborStreets)// Connect back to the harbor streets
                // No direct connection to the market - goods are likely transported through Harbor Streets
         // Salvage materials from ships and cargo
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.Docks)
            .ForFeatureType(LocationSpotTypes.ForestGrove)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithOutputResource(ResourceTypes.Salvage, 1))
        // Trade salvaged goods
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.Docks)
            .ForFeatureType(LocationSpotTypes.ResourceMarket)
            .WithInputResource(ResourceTypes.Salvage, 1)
            .WithCoinReward(1)
            .WithEnergyCost(1, EnergyTypes.Social))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Dangerous)
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

    public static Location ArtisanRow => new LocationBuilder()
        .ForLocation(LocationNames.ArtisanRow)
        .SetLocationType(LocationTypes.Commerce)
        .AddTravelConnection(LocationNames.MarketSquare)   // Connect to the main market
        .AddTravelConnection(LocationNames.CarpentersWorkshop)
        // Buy raw materials at good prices
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.ArtisanRow)
            .ForFeatureType(LocationSpotTypes.ResourceMarket)
            .WithCoinCost(4)
            .WithOutputResource(ResourceTypes.Metal, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        // Process metal into tools
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.ArtisanRow)
            .ForFeatureType(LocationSpotTypes.SmithyForge)
            .WithInputResource(ResourceTypes.Metal, 1)
            .WithOutputResource(ResourceTypes.Tools, 1)
            .WithEnergyCost(3, EnergyTypes.Physical))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    // === RESIDENTIAL DISTRICT ===
    public static Location Tavern => new LocationBuilder()
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationType(LocationTypes.Social)
        .AddTravelConnection(LocationNames.HarborStreets) // Near the harbor
        .AddTravelConnection(LocationNames.MarketSquare)
        // Comfortable rest with energy recovery
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForFeatureType(LocationSpotTypes.CozyShelter)
            .WithCoinCost(1))
        // Expensive but convenient food
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForFeatureType(LocationSpotTypes.GeneralStore)
            .WithCoinCost(3)
            .WithOutputResource(ResourceTypes.Food, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    // === OUTSKIRTS ===
    public static Location ForestEdge => new LocationBuilder()
        .ForLocation(LocationNames.ForestEdge)
        .SetLocationType(LocationTypes.Nature)
        .AddTravelConnection(LocationNames.CarpentersWorkshop) // Carpenters would likely be near the forest
        // Gather wood
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.ForestEdge)
            .ForFeatureType(LocationSpotTypes.ForestGrove)
            .WithEnergyCost(2, EnergyTypes.Physical)
            .WithOutputResource(ResourceTypes.Wood, 1))
        // Sell to traveling merchants
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.ForestEdge)
            .ForFeatureType(LocationSpotTypes.ResourceMarket)
            .WithInputResource(ResourceTypes.Wood, 1)
            .WithCoinReward(2)
            .WithEnergyCost(1, EnergyTypes.Social))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    // === WORKSHOPS ===
    public static Location CarpentersWorkshop => new LocationBuilder()
        .ForLocation(LocationNames.CarpentersWorkshop)
        .SetLocationType(LocationTypes.Industry)
        .AddTravelConnection(LocationNames.ForestEdge)    // Close to the wood source
        .AddTravelConnection(LocationNames.ArtisanRow)
        // Process wood into planks
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.CarpentersWorkshop)
            .ForFeatureType(LocationSpotTypes.WoodworkBench)
            .WithInputResource(ResourceTypes.Wood, 1)
            .WithOutputResource(ResourceTypes.Planks, 1)
            .WithEnergyCost(2, EnergyTypes.Physical))
        // Buy raw materials
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.CarpentersWorkshop)
            .ForFeatureType(LocationSpotTypes.ResourceMarket)
            .WithCoinCost(3)
            .WithOutputResource(ResourceTypes.Wood, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();
}