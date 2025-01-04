public enum LocationTypes
{
    None = 0,
    Industry,
    Commercial,
    Social,
    Nature,
}

public enum LocationSpotNames
{
    // Processing features - each handles unique resource conversion
    WoodworkBench,    // Wood -> Planks
    SmithyForge,      // Ore -> Metal
    TanningRack,      // Hide -> Leather
    WeavingLoom,      // Cloth -> Clothes

    // Trading features - each has unique trade rules
    GeneralStore,     // Basic goods at standard prices
    ResourceMarket,   // Raw materials at bulk prices
    SpecialtyShop,    // Processed goods at premium prices

    // Gathering features - each provides unique resources
    ForestGrove,      // Wood gathering
    MineralDeposit,   // Ore gathering
    HuntingSpot,      // Hide gathering

    // Social features
    TavernBar,        // Trading + social interaction
    CommonArea,       // Group interactions
    PrivateCorner,    // Observation spot
    ServingArea,      // Work opportunity

    // Shelter features - progressive improvement
    BasicShelter,     // No cost, prevents penalties
    GoodShelter       // Enhanced shelter
}
