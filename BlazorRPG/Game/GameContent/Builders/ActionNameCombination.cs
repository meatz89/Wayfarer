public class ActionNameCombination
{
    public LocationSpotNames LocationContext { get; set; }
    public BasicActionTypes BaseAction { get; set; }
    public Verb Verb { get; set; }
    public Adjective Adjective { get; set; }
    public ComplexityTypes? Complexity { get; set; } // Optional
    public TensionState? Tension { get; set; } // Optional

    public ActionNameCombination(LocationSpotNames locationContext, BasicActionTypes baseAction, Verb verb, Adjective adjective = Adjective.None, ComplexityTypes? complexity = null, TensionState? tension = null)
    {
        LocationContext = locationContext;
        BaseAction = baseAction;
        Verb = verb;
        Adjective = adjective;
        Complexity = complexity;
        Tension = tension;
    }
}

public static class LocationContextExtensions
{
    public static Verb DefaultVerb(this LocationSpotNames context)
    {
        switch (context)
        {
            case LocationSpotNames.Market:
                return Verb.Browse;
            case LocationSpotNames.Road:
                return Verb.Forage;
            case LocationSpotNames.Forest:
                return Verb.Forage;
            case LocationSpotNames.Field:
                return Verb.Forage;
            case LocationSpotNames.Warehouse:
                return Verb.Labor;
            case LocationSpotNames.Factory:
                return Verb.Labor;
            case LocationSpotNames.Workshop:
                return Verb.Labor;
            case LocationSpotNames.Shop:
                return Verb.Browse;
            default:
                return Verb.None; // Or some other generic default verb
        }
    }
}