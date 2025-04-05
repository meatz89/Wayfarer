public class EncounterResult
{
    public EncounterManager Encounter { get; set; }
    public NarrativeResult NarrativeResult { get; set; }
    public EncounterResults EncounterResults { get; set; }
    public string EncounterEndMessage { get; set; }
    public NarrativeContext NarrativeContext { get; set; }
    public Location TravelLocation { get; set; }

    public PostEncounterEvolutionResponse PostEncounterEvolution { get; set; }
}