public class EncounterResult
{
    public ActionImplementation ActionImplementation { get; set; }
    public ActionResults ActionResult { get; set; }
    public NarrativeResult NarrativeResult { get; set; }
    public string EncounterEndMessage { get; set; }
    public NarrativeContext NarrativeContext { get; set; }
    public PostEncounterEvolutionResult PostEncounterEvolution { get; set; }
}