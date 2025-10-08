using System.Collections.Generic;

/// <summary>
/// Models for the observation system
/// </summary>
public class Observation
{
    public string Id { get; set; }
    public string Text { get; set; }
    public ObservationType Type { get; set; }
    public int AttentionCost { get; set; } = 0;
    public string[] RelevantNPCs { get; set; }
    public ConnectionState? CreatesState { get; set; }
    public string CardTemplate { get; set; }
    public string Description { get; set; }
    public ObservationInfoType? ProvidesInfo { get; set; }
    public bool CreatesUrgency { get; set; }
    public bool Automatic { get; set; }
    public string SpotId { get; set; } // Which spot this observation is associated with
}
