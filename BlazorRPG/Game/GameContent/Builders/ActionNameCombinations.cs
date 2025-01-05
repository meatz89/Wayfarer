public class ActionNameCombinations
{
    public List<ActionNameCombination> ValidCombinations { get; } = new List<ActionNameCombination>()
    {
        // ========================================
        // Tavern (Social + Indoor)
        // ========================================
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Labor, Verb.ServeDrinks),
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Perform, Verb.PlayMusic), // Changed Mingle to Perform
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Mingle, Verb.Gamble),
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Mingle, Verb.Gossip),
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Mingle, Verb.Socialize),
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Investigate, Verb.Observe), // Changed Mingle to Investigate
        new ActionNameCombination(LocationSpotNames.TavernBarterTable, BasicActionTypes.Trade, Verb.Barter),

        // ========================================
        // Public Market (Social + Outdoor)
        // ========================================
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Gather, Verb.Browse),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Trade, Verb.Barter),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Mingle, Verb.Chat),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Mingle, Verb.Gossip),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Investigate, Verb.Observe), // Changed Mingle to Investigate
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Perform, Verb.Perform), // Changed Mingle to Perform
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Trade, Verb.Purchase),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Trade, Verb.Sell),
        new ActionNameCombination(LocationSpotNames.Market, BasicActionTypes.Labor, Verb.Guard),

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

    public List<ActionNameCombination> GetCombinations(LocationSpotNames context, BasicActionTypes actionType)
    {
        // Returns a list of combinations that match the provided context and action type
        return ValidCombinations.Where(c => c.LocationContext == context && c.BaseAction == actionType).ToList();
    }

    public Verb? GetVerb(LocationSpotNames locationContext, BasicActionTypes baseAction, ComplexityTypes? complexity = null, TensionState? tension = null)
    {
        foreach (var combo in ValidCombinations)
        {
            if (combo.LocationContext == locationContext &&
                combo.BaseAction == baseAction &&
                (combo.Complexity == null || combo.Complexity == complexity) &&
                (combo.Tension == null || combo.Tension == tension))
            {
                return combo.Verb;
            }
        }
        return null; // Or a default verb
    }

    public Adjective? GetAdjective(ActionGenerationContext context)
    {
        foreach (var combo in ValidCombinations)
        {
            if ((combo.Tension == null || combo.Tension == context.Social.Tension) &&
                (combo.Complexity == null || combo.Complexity == context.Activity.Complexity))
            {
                return combo.Adjective;
            }
        }
        return Adjective.None;
    }

    public void CheckForMissingCombinations()
    {
        var allLocationContexts = Enum.GetValues(typeof(LocationSpotNames)).Cast<LocationSpotNames>();
        var allBaseActions = Enum.GetValues(typeof(BasicActionTypes)).Cast<BasicActionTypes>();
        var allComplexities = Enum.GetValues(typeof(ComplexityTypes)).Cast<ComplexityTypes>();
        var allTensions = Enum.GetValues(typeof(TensionState)).Cast<TensionState>();

        foreach (var locationContext in allLocationContexts)
        {
            foreach (var baseAction in allBaseActions)
            {
                foreach (var complexity in allComplexities)
                {
                    foreach (var tension in allTensions)
                    {
                        bool found = ValidCombinations.Any(c => c.LocationContext == locationContext &&
                                                               c.BaseAction == baseAction &&
                                                               (c.Complexity == null || c.Complexity == complexity) &&
                                                               (c.Tension == null || c.Tension == tension));
                        if (!found)
                        {
                            // Only print if it's a combination we haven't explicitly excluded
                            if (!ShouldSkipCombination(locationContext, baseAction, complexity, tension))
                            {
                                Console.WriteLine($"Missing combination: LocationContext={locationContext}, BaseAction={baseAction}, Complexity={complexity}, Tension={tension}");
                            }
                        }
                    }
                }
            }
        }
    }

    // Add logic to exclude combinations that are not relevant based on your game's rules
    private bool ShouldSkipCombination(LocationSpotNames locationContext, BasicActionTypes baseAction, ComplexityTypes complexity, TensionState tension)
    {
        // Example: Skip combinations where Labor is attempted in a Field
        if (locationContext == LocationSpotNames.Field && baseAction == BasicActionTypes.Labor)
        {
            return true;
        }

        // Example: Skip combinations where Trade is attempted in a Forest
        if (locationContext == LocationSpotNames.Forest && baseAction == BasicActionTypes.Trade)
        {
            return true;
        }

        // Add more skip conditions here based on your game's logic

        return false; // Do not skip by default
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
