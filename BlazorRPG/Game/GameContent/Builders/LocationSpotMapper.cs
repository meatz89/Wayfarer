
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
    // Social
    Tavern,         // Social + Indoor + Any Scale
    PublicMarket,   // Social + Outdoor + Any Scale

    // Nature
    Road,           // Nature + Outdoor + Intimate
    Forest,         // Nature + Outdoor + Medium
    Field,          // Nature + Outdoor + Large
    Cave,           // Nature + Indoor + Intimate
    Cavern,         // Nature + Indoor + Medium
    UndergroundLake, // Nature + Indoor + Large

    // Industrial
    Dock,           // Industrial + Outdoor + Any Scale
    Warehouse,      // Industrial + Indoor + Large
    Factory,        // Industrial + Indoor + Medium
    Workshop,       // Industrial + Indoor + Intimate

    // Commercial
    Market,         // Commercial + Outdoor + Any Scale
    Shop,           // Commercial + Indoor + Any Scale

    Garden,     // Commercial + Outdoor + Any Scale 
    Campground, // Nature + Outdoor + Any Scale
    TravelerLodge, // Social + Indoor + Any Scale
    GroveShrine, // Nature + Outdoor + Intimate,
    WoodworkerCabin, // Nature + Outdoor + Intimate
    MysticGrove, // Nature + Outdoor + Medium
    LumberYard, // Nature + Outdoor + Medium
    InnFireplace, // Social + Indoor + Intimate,
    MarketBazaar, // Commercial + Outdoor + Medium
    MarketSquare, // Commercial + Outdoor + Large
    HerbGarden, // Commercial + Outdoor + Intimate
    FishingWharf, // Industrial + Outdoor + Medium
    WharfMerchant, // Industrial + Outdoor + Medium
    DocksidePub, // Industrial + Outdoor + Intimate
    DocksideWarehouse, // Industrial + Indoor + Large
    MarketPorters, // Commercial + Outdoor + Medium
    TavernBarterTable, // Social + Indoor + Intimate
    CellarPantry, // Social + Indoor + Intimate
    TavernKitchen, // Social + Indoor + Intimate
    Undefined
}

public static class LocationSpotMapper
{
    public static LocationSpotNames GetLocationSpot(LocationTypes locationType, ExposureConditions exposure, ScaleVariations scale)
    {
        if (locationType == LocationTypes.Social)
        {
            return exposure == ExposureConditions.Indoor ? LocationSpotNames.Tavern : LocationSpotNames.PublicMarket;
        }
        else if (locationType == LocationTypes.Nature)
        {
            if (exposure == ExposureConditions.Outdoor)
            {
                if (scale == ScaleVariations.Intimate) return LocationSpotNames.Road;
                if (scale == ScaleVariations.Medium) return LocationSpotNames.Forest;
                if (scale == ScaleVariations.Large) return LocationSpotNames.Field;
            }
            else
            {
                if (scale == ScaleVariations.Intimate) return LocationSpotNames.Cave;
                if (scale == ScaleVariations.Medium) return LocationSpotNames.Cavern;
                if (scale == ScaleVariations.Large) return LocationSpotNames.UndergroundLake;
            }
        }
        else if (locationType == LocationTypes.Industrial)
        {
            if (exposure == ExposureConditions.Outdoor) return LocationSpotNames.Dock;
            if (scale == ScaleVariations.Large) return LocationSpotNames.Warehouse;
            if (scale == ScaleVariations.Medium) return LocationSpotNames.Factory;
            return LocationSpotNames.Workshop; // Intimate or default
        }
        else if (locationType == LocationTypes.Commercial)
        {
            return exposure == ExposureConditions.Outdoor ? LocationSpotNames.Market : LocationSpotNames.Shop;
        }

        return LocationSpotNames.Undefined;
    }
}

public enum BasicActionTypes
{
    Wait,   // Advance time
    Rest,   // Restore energy, advance time

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
        new ActionNameCombination(LocationSpotNames.Tavern, BasicActionTypes.Rest, Verb.Rest), // Changed Labor to Rest

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
        new ActionNameCombination(LocationSpotNames.Road, BasicActionTypes.Rest, Verb.Rest),

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



