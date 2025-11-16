/// <summary>
/// Models for the observation system
/// </summary>
public class Observation
{
    // HIGHLANDER: NO Id property - Observation identified by object reference
    public string Text { get; set; }
    public ObservationType Type { get; set; }
    public int AttentionCost { get; set; } = 0;
    // HIGHLANDER: Object references ONLY, no RelevantNPCs IDs (using string array for NPC names)
    public string[] RelevantNPCs { get; set; }
    public ConnectionState? CreatesState { get; set; }
    public string CardTemplate { get; set; }
    public string Description { get; set; }
    public ObservationInfoType? ProvidesInfo { get; set; }
    public bool CreatesUrgency { get; set; }
    public bool Automatic { get; set; }
    // HIGHLANDER: Object reference ONLY, no LocationId
    public Location Location { get; set; } // Which location this observation is associated with
}
