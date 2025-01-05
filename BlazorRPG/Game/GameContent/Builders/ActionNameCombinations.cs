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
    Sell        
}

public enum Adjective
{
    Stealthily,
    Expertly,
    Intensely,
    // ... add more as needed
    None
}

public enum LocationContext
{
    None,

    // Social
    Tavern,         // Social + Indoor + Any Scale
    PublicMarket,   // Social + Outdoor + Any Scale

    // Nature
    Road,           // Nature + Outdoor + Intimate
    Forest,         // Nature + Outdoor + Medium
    Field,          // Nature + Outdoor + Large

    // Industrial
    Dock,           // Industrial + Outdoor + Any Scale
    Warehouse,      // Industrial + Indoor + Large
    Factory,        // Industrial + Indoor + Medium
    Workshop,       // Industrial + Indoor + Intimate

    // Commercial
    Market,         // Commercial + Outdoor + Any Scale
    Shop,            // Commercial + Indoor + Any Scale
    Garden
}
public static class LocationContextMapper
{
    public static LocationContext GetLocationContext(LocationTypes locationType, ExposureConditions exposure, ScaleVariations scale)
    {
        if (locationType == LocationTypes.Social)
        {
            return exposure == ExposureConditions.Indoor ? LocationContext.Tavern : LocationContext.PublicMarket;
        }
        else if (locationType == LocationTypes.Nature)
        {
            if (exposure == ExposureConditions.Outdoor)
            {
                if (scale == ScaleVariations.Intimate) return LocationContext.Road;
                if (scale == ScaleVariations.Medium) return LocationContext.Forest;
                if (scale == ScaleVariations.Large) return LocationContext.Field;
            }
            else
            {
                return LocationContext.Garden;
            }
        }
        else if (locationType == LocationTypes.Industrial)
        {
            if (exposure == ExposureConditions.Outdoor) return LocationContext.Dock;
            if (scale == ScaleVariations.Large) return LocationContext.Warehouse;
            if (scale == ScaleVariations.Medium) return LocationContext.Factory;
            return LocationContext.Workshop; // Intimate or default
        }
        else if (locationType == LocationTypes.Commercial)
        {
            return exposure == ExposureConditions.Outdoor ? LocationContext.Market : LocationContext.Shop;
        }

        return LocationContext.None;
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
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Labor, Verb.ServeDrinks),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Mingle, Verb.PlayMusic),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Mingle, Verb.Gamble),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Mingle, Verb.Gossip),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Mingle, Verb.Socialize),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Mingle, Verb.Observe),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationContext.Tavern, BasicActionTypes.Labor, Verb.Rest),

        // ========================================
        // Public Market (Social + Outdoor)
        // ========================================
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Gather, Verb.Browse),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Mingle, Verb.Gossip),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Mingle, Verb.Observe),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Mingle, Verb.Perform),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Trade, Verb.Purchase),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Trade, Verb.Sell),
        new ActionNameCombination(LocationContext.PublicMarket, BasicActionTypes.Labor, Verb.Guard),

        // ========================================
        // Road (Nature + Outdoor + Intimate)
        // ========================================
        new ActionNameCombination(LocationContext.Road, BasicActionTypes.Gather, Verb.Forage),
        new ActionNameCombination(LocationContext.Road, BasicActionTypes.Labor, Verb.Patrol),
        new ActionNameCombination(LocationContext.Road, BasicActionTypes.Labor, Verb.Rest),

        // ========================================
        // Forest (Nature + Outdoor + Medium)
        // ========================================
        new ActionNameCombination(LocationContext.Forest, BasicActionTypes.Gather, Verb.Hunt),
        new ActionNameCombination(LocationContext.Forest, BasicActionTypes.Gather, Verb.Forage),
        new ActionNameCombination(LocationContext.Forest, BasicActionTypes.Labor, Verb.Patrol),

        // ========================================
        // Field (Nature + Outdoor + Large)
        // ========================================
        new ActionNameCombination(LocationContext.Field, BasicActionTypes.Gather, Verb.Forage),
        new ActionNameCombination(LocationContext.Field, BasicActionTypes.Labor, Verb.Patrol),

        // ========================================
        // Dock (Industrial + Outdoor)
        // ========================================
        new ActionNameCombination(LocationContext.Dock, BasicActionTypes.Labor, Verb.Labor),
        new ActionNameCombination(LocationContext.Dock, BasicActionTypes.Labor, Verb.Load),
        new ActionNameCombination(LocationContext.Dock, BasicActionTypes.Labor, Verb.Unload),
        new ActionNameCombination(LocationContext.Dock, BasicActionTypes.Trade, Verb.Negotiate),
        new ActionNameCombination(LocationContext.Dock, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationContext.Dock, BasicActionTypes.Gather, Verb.Fish),

        // ========================================
        // Warehouse (Industrial + Indoor + Large)
        // ========================================
        new ActionNameCombination(LocationContext.Warehouse, BasicActionTypes.Labor, Verb.Labor),
        new ActionNameCombination(LocationContext.Warehouse, BasicActionTypes.Labor, Verb.Store),
        new ActionNameCombination(LocationContext.Warehouse, BasicActionTypes.Labor, Verb.Load),
        new ActionNameCombination(LocationContext.Warehouse, BasicActionTypes.Labor, Verb.Unload),

        // ========================================
        // Factory (Industrial + Indoor + Medium)
        // ========================================
        new ActionNameCombination(LocationContext.Factory, BasicActionTypes.Labor, Verb.Labor),
        new ActionNameCombination(LocationContext.Factory, BasicActionTypes.Labor, Verb.Manufacture),

        // ========================================
        // Workshop (Industrial + Indoor + Intimate)
        // ========================================
        new ActionNameCombination(LocationContext.Workshop, BasicActionTypes.Labor, Verb.Labor),
        new ActionNameCombination(LocationContext.Workshop, BasicActionTypes.Labor, Verb.Repair),
        new ActionNameCombination(LocationContext.Workshop, BasicActionTypes.Labor, Verb.Study),

        // ========================================
        // Market (Commercial + Outdoor)
        // ========================================
        new ActionNameCombination(LocationContext.Market, BasicActionTypes.Gather, Verb.Browse),
        new ActionNameCombination(LocationContext.Market, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationContext.Market, BasicActionTypes.Trade, Verb.Purchase),
        new ActionNameCombination(LocationContext.Market, BasicActionTypes.Trade, Verb.Sell),
        new ActionNameCombination(LocationContext.Market, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationContext.Market, BasicActionTypes.Mingle, Verb.Observe),

        // ========================================
        // Shop (Commercial + Indoor)
        // ========================================
        new ActionNameCombination(LocationContext.Shop, BasicActionTypes.Gather, Verb.Browse),
        new ActionNameCombination(LocationContext.Shop, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationContext.Shop, BasicActionTypes.Trade, Verb.Purchase),
        new ActionNameCombination(LocationContext.Shop, BasicActionTypes.Trade, Verb.Sell),
        new ActionNameCombination(LocationContext.Shop, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationContext.Shop, BasicActionTypes.Labor, Verb.Study),
    };
}
