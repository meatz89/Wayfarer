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
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.BasicShelter)
            .WithCoinCost(0)
            )
        // Street vendors sell cheap but limited food
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.GeneralStore)
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
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.ForestGrove)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithOutputResource(ResourceTypes.Salvage, 1))
        // Trade salvaged goods
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.ResourceMarket)
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
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.GeneralStore)
            .WithCoinCost(2)
            .WithOutputResource(ResourceTypes.Food, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        // Luxury goods trader
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.SpecialtyShop)
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
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.ResourceMarket)
            .WithCoinCost(4)
            .WithOutputResource(ResourceTypes.Metal, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        // Process metal into tools
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.SmithyForge)
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
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.CozyShelter)
            .WithCoinCost(1))
        // Expensive but convenient food
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.GeneralStore)
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
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.ForestGrove)
            .WithEnergyCost(2, EnergyTypes.Physical)
            .WithOutputResource(ResourceTypes.Wood, 1))
        // Sell to traveling merchants
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.ResourceMarket)
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
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.WoodworkBench)
            .WithInputResource(ResourceTypes.Wood, 1)
            .WithOutputResource(ResourceTypes.Planks, 1)
            .WithEnergyCost(2, EnergyTypes.Physical))
        // Buy raw materials
        .AddFeature(feature => feature
            .ForFeatureType(LocationFeatureTypes.ResourceMarket)
            .WithCoinCost(3)
            .WithOutputResource(ResourceTypes.Wood, 1)
            .WithEnergyCost(1, EnergyTypes.Social))
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();
}