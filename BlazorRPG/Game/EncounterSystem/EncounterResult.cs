public class EncounterResult
{
    public LocationAction locationAction { get; set; }
    public ActionResults ActionResult { get; set; }
    public BeatResponse AIResponse { get; set; }
    public string EncounterEndMessage { get; set; }
    public EncounterContext EncounterContext { get; set; }
    public PostEncounterEvolutionResult PostEncounterEvolution { get; set; }
    public IEnumerable<ProposedChange> ProposedChanges { get; set; }
}