public enum LocationTypes
{
    None = 0,
    Industry,
    Commerce,
    Social,
    Nature
}

public enum LocationFeatureTypes
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

    // Shelter features - progressive improvement
    BasicShelter,     // No cost, prevents penalties
    CozyShelter      // Costs coins, restores energy
}