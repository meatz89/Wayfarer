
public class ActionNameCombination
{
    public LocationContext LocationContext { get; set; }
    public BasicActionTypes BaseAction { get; set; }
    public Verb Verb { get; set; }
    public Adjective Adjective { get; set; } // Optional

    public ActionNameCombination(LocationContext locationContext, BasicActionTypes baseAction, Verb verb, Adjective adjective = Adjective.None)
    {
        LocationContext = locationContext;
        BaseAction = baseAction;
        Verb = verb;
        Adjective = adjective;
    }
}
