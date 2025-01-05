
public class SpaceProperties
{
    public ScaleVariations Scale { get; set; }
    public ExposureConditions Exposure { get; set; }
}

public class SocialContext
{
    public LegalityTypes Legality { get; set; }
    public TensionState Tension { get; set; }
}

public class ActivityProperties
{
    public ComplexityTypes Complexity { get; set; }
}

public enum ScaleVariations
{
    Medium,
    Intimate,
    Large
}

public enum ExposureConditions
{
    Indoor,
    Outdoor,
}


public enum LegalityTypes
{
    Legal,
    Illegal
}

public enum TensionState
{
    Relaxed,
    Alert,
    Hostile,
}

// Activity Properties define execution requirements
public enum ComplexityTypes
{
    Complex,
    Simple
}


public enum Verb
{
    ServeDrinks,
    PlayMusic,
    Forage,
    Hunt,
    Browse,
    Collect,
    Labor,
    Negotiate,
    Barter,
    Trade,
    Socialize,
    Network,
    Chat,
    Mine,       
    Fish,       
    Repair,     
    Manufacture,
    Store,      
    Load,       
    Unload,     
    Guard,      
    Patrol,     
    Rest,       
    Perform,    
    Gamble,     
    Gossip,     
    Observe,    
    Study,      
    Purchase,   
    Sell,
    None
}

public enum Adjective
{
    Stealthily,
    Expertly,
    Intensely,
    // ... add more as needed
    None
}

public enum LocationSpotNames
{
    // Tavern
    Tavern,
    TavernBarterTable,
    CellarPantry,
    TavernKitchen,
    InnFireplace,
    // Public Market
    PublicMarket,
    MarketSquare,
    MarketBazaar,
    MarketPorters,
    HerbGarden,
    // Road
    Road,
    // Forest
    Forest,
    MysticGrove,
    LumberYard,
    WoodworkerCabin,
    GroveShrine,
    // Field
    Field,
    Campground,
    // Dock
    Dock,
    FishingWharf,
    WharfMerchant,
    DocksidePub,
    // Warehouse
    Warehouse,
    DocksideWarehouse,
    // Factory
    Factory,
    // Workshop
    Workshop,
    // Market
    Market,
    // Shop
    Shop,
    // Garden
    Garden,
    // TravelerLodge
    TravelerLodge,

    Undefined,
    Cave,
    Cavern,
    UndergroundLake
}


public static class LocationSpotMapper
{
    public static LocationSpotNames GetLocationSpotName(LocationTypes locationType, BasicActionTypes baseAction, ExposureConditions exposure, ScaleVariations scale)
    {
        if (locationType == LocationTypes.Social)
        {
            if (exposure == ExposureConditions.Indoor)
            {
                switch (baseAction)
                {
                    case BasicActionTypes.Labor:
                        return LocationSpotNames.TavernKitchen;
                    case BasicActionTypes.Gather:
                        return LocationSpotNames.CellarPantry;
                    case BasicActionTypes.Trade:
                        return LocationSpotNames.TavernBarterTable;
                    case BasicActionTypes.Mingle:
                    case BasicActionTypes.Perform:
                    case BasicActionTypes.Investigate:
                    default:
                        return LocationSpotNames.Tavern;
                }
            }
            else // Outdoor
            {
                switch (baseAction)
                {
                    case BasicActionTypes.Labor:
                        return LocationSpotNames.MarketPorters;
                    case BasicActionTypes.Gather:
                        return LocationSpotNames.HerbGarden;
                    case BasicActionTypes.Trade:
                        return LocationSpotNames.MarketBazaar;
                    default:
                        return LocationSpotNames.PublicMarket;
                }
            }
        }
        else if (locationType == LocationTypes.Nature)
        {
            if (exposure == ExposureConditions.Outdoor)
            {
                switch (scale)
                {
                    case ScaleVariations.Intimate:
                        return LocationSpotNames.Road;
                    case ScaleVariations.Medium:
                        return LocationSpotNames.Forest;
                    case ScaleVariations.Large:
                        return LocationSpotNames.Field;
                }
            }
            else // Indoor
            {
                switch (scale)
                {
                    case ScaleVariations.Intimate:
                        return LocationSpotNames.Cave;
                    case ScaleVariations.Medium:
                        return LocationSpotNames.Cavern;
                    case ScaleVariations.Large:
                        return LocationSpotNames.UndergroundLake;
                }
            }
        }
        else if (locationType == LocationTypes.Industrial)
        {
            if (exposure == ExposureConditions.Outdoor)
            {
                switch (baseAction)
                {
                    case BasicActionTypes.Gather:
                        return LocationSpotNames.FishingWharf;
                    case BasicActionTypes.Trade:
                        return LocationSpotNames.WharfMerchant;
                    case BasicActionTypes.Mingle:
                        return LocationSpotNames.DocksidePub;
                    default:
                        return LocationSpotNames.Dock;
                }
            }
            else // Indoor
            {
                switch (scale)
                {
                    case ScaleVariations.Large:
                        return LocationSpotNames.Warehouse;
                    case ScaleVariations.Medium:
                        return LocationSpotNames.Factory;
                    case ScaleVariations.Intimate:
                    default:
                        return LocationSpotNames.Workshop;
                }
            }
        }
        else if (locationType == LocationTypes.Commercial)
        {
            if (exposure == ExposureConditions.Outdoor)
            {
                switch (scale)
                {
                    case ScaleVariations.Intimate:
                        return LocationSpotNames.HerbGarden;
                    case ScaleVariations.Medium:
                        return LocationSpotNames.MarketBazaar;
                    case ScaleVariations.Large:
                        return LocationSpotNames.MarketSquare;
                    default:
                        return LocationSpotNames.Market;
                }
            }
            else // Indoor
            {
                return LocationSpotNames.Shop;
            }
        }

        return LocationSpotNames.Undefined; // Fallback
    }
}

public enum BasicActionTypes
{
    // Physical Actions define direct interaction with the world:
    Labor, // for directed physical effort
    Gather, // for collecting and taking
    Craft, // for creating and combining
    Move, // for traversing and positioning

    // Social Actions handle character interactions:
    Mingle, // for casual interaction
    Trade, // for formal exchange
    Persuade, // for directed influence
    Perform, // for entertainment and display

    // Mental Actions cover intellectual activities:
    Investigate, // for directed observation
    Study, // for focused learning
    Plan, // for strategic thinking
    Reflect, // for processing and rest
}

public partial class ActionNameCombinations
{
    public List<ActionNameCombination> ValidCombinations { get; } = new List<ActionNameCombination>()
    {
        // ========================================
        // Tavern (Social + Indoor)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Labor, Verb.ServeDrinks),
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Perform, Verb.PlayMusic), // Changed Mingle to Perform
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Mingle, Verb.Gamble),
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Mingle, Verb.Gossip),
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Mingle, Verb.Socialize),
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Investigate, Verb.Observe), // Changed Mingle to Investigate
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Trade, Verb.Barter),

        // ========================================
        // Public Market (Social + Outdoor)
        // ========================================
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Gather, Verb.Browse),
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Mingle, Verb.Gossip),
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Investigate, Verb.Observe), // Changed Mingle to Investigate
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Perform, Verb.Perform), // Changed Mingle to Perform
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Trade, Verb.Purchase),
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Trade, Verb.Sell),
        new ActionNameCombination(LocationSpotNames.PublicMarket, BasicActionTypes.Labor, Verb.Guard),

        // ========================================
        // Road (Nature + Outdoor + Intimate)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Road, BasicActionTypes.Gather, Verb.Forage),
        new ActionNameCombination(LocationSpotNames.Road, BasicActionTypes.Labor, Verb.Patrol),

        // ========================================
        // Forest (Nature + Outdoor + Medium)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Forest, BasicActionTypes.Gather, Verb.Hunt, complexity: ComplexityTypes.Complex), // Only allow hunting if complex
        new ActionNameCombination(LocationSpotNames.Forest, BasicActionTypes.Gather, Verb.Forage),
        new ActionNameCombination(LocationSpotNames.Forest, BasicActionTypes.Labor, Verb.Patrol),

        // ========================================
        // Field (Nature + Outdoor + Large)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Field, BasicActionTypes.Gather, Verb.Forage),
        new ActionNameCombination(LocationSpotNames.Field, BasicActionTypes.Labor, Verb.Patrol),

        // ========================================
        // Dock (Industrial + Outdoor)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Dock, BasicActionTypes.Labor, Verb.Load),
        new ActionNameCombination(LocationSpotNames.Dock, BasicActionTypes.Labor, Verb.Unload),
        new ActionNameCombination(LocationSpotNames.Dock, BasicActionTypes.Trade, Verb.Negotiate),
        new ActionNameCombination(LocationSpotNames.Dock, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationSpotNames.Dock, BasicActionTypes.Gather, Verb.Fish),

        // ========================================
        // Warehouse (Industrial + Indoor + Large)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Warehouse, BasicActionTypes.Labor, Verb.Store),
        new ActionNameCombination(LocationSpotNames.Warehouse, BasicActionTypes.Labor, Verb.Load),
        new ActionNameCombination(LocationSpotNames.Warehouse, BasicActionTypes.Labor, Verb.Unload),

        // ========================================
        // Factory (Industrial + Indoor + Medium)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Factory, BasicActionTypes.Labor, Verb.Manufacture),

        // ========================================
        // Workshop (Industrial + Indoor + Intimate)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Workshop, BasicActionTypes.Labor, Verb.Repair),
        new ActionNameCombination(LocationSpotNames.Workshop, BasicActionTypes.Study, Verb.Study),

        // ========================================
        // Market (Commercial + Outdoor)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Gather, Verb.Browse),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Trade, Verb.Purchase),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Trade, Verb.Sell),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Investigate, Verb.Observe),

        // ========================================
        // Shop (Commercial + Indoor)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Shop, BasicActionTypes.Gather, Verb.Browse),
        new ActionNameCombination(LocationSpotNames.Shop, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationSpotNames.Shop, BasicActionTypes.Trade, Verb.Purchase),
        new ActionNameCombination(LocationSpotNames.Shop, BasicActionTypes.Trade, Verb.Sell),
        new ActionNameCombination(LocationSpotNames.Shop, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationSpotNames.Shop, BasicActionTypes.Study, Verb.Study),
    };

}



