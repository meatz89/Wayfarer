public enum LocationTypes
{
    None = 0,
    Industrial,
    Commercial,
    Social,
    Nature,
}

public enum LocationSpotNames
{
    // LABOR features - each handles unique coin generation
    DocksideWarehouse,    // Work opportunity at the Docks
    MarketPorters,        // Work opportunity at the Market
    LumberYard,           // Work opportunity at the Forest
    TavernKitchen,        // Work opportunity at the Tavern

    // GATHERing features - each provides unique resources
    FishingWharf,         // GATHERing opportunity at the Docks
    HerbGarden,           // GATHERing opportunity at the Market
    MysticGrove,          // GATHERing opportunity at the Forest
    CellarPantry,         // GATHERing opportunity at the Tavern

    // Trading features - each has unique trade rules
    WharfMerchant,        // Trading opportunity at the Docks
    MarketBazaar,         // Trading opportunity at the Market
    WoodworkerCabin,      // Trading opportunity at the Forest
    TavernBarterTable,    // Trading opportunity at the Tavern

    // Social features
    DocksidePub,          // Social opportunity at the Docks
    MarketSquare,         // Social opportunity at the Market
    GroveShrine,          // Social opportunity at the Forest
    InnFireplace,         // Social opportunity at the Tavern

    // Rest features
    Campground,           // No cost, prevents penalties
    TravelerLodge      // Enhanced shelter
}
