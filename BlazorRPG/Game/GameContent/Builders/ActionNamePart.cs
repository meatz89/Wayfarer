

public class ActionNamePart
{
    public LocationSpotNames? LocationContext { get; set; } // Location context
    public BasicActionTypes? BaseAction { get; set; }
    public ComplexityTypes? Complexity { get; set; }
    public TensionState? Tension { get; set; }

    public Verb? VerbResult { get; set; }
    public Adjective? AdjectiveResult { get; set; } = Adjective.None;

    public ActionNamePart(
        LocationSpotNames? locationContext = null,
        BasicActionTypes? baseAction = null,
        ComplexityTypes? complexity = null,
        TensionState? tension = null,
        Verb? verbResult = null,
        Adjective? adjectiveResult = null)
    {
        LocationContext = locationContext;
        BaseAction = baseAction;
        Complexity = complexity;
        Tension = tension;
        VerbResult = verbResult;
        AdjectiveResult = adjectiveResult;
    }
}
