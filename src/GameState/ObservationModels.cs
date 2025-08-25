using System.Collections.Generic;

/// <summary>
/// Models for the observation system
/// </summary>
public class Observation
{
    public string Id { get; set; }
    public string Text { get; set; }
    public ObservationType Type { get; set; }
    public int AttentionCost { get; set; }
    public string[] RelevantNPCs { get; set; }
    public EmotionalState? CreatesState { get; set; }
    public string CardTemplate { get; set; }
    public string Description { get; set; }
    public ObservationInfoType? ProvidesInfo { get; set; }
    public bool CreatesUrgency { get; set; }
    public bool Automatic { get; set; }
    public string SpotId { get; set; } // Which spot this observation is associated with
}

public enum ObservationType
{
    Important,
    Normal,
    Useful,
    Critical,
    Shadow,
    NPC
}

public enum ObservationInfoType
{
    Transport,
    Timing,
    Secret,
    Location
}

public class ObservationsData
{
    public Dictionary<string, List<Observation>> locations { get; set; }
    public Dictionary<string, ObservationTypeData> observationTypes { get; set; }
}

public class ObservationTypeData
{
    public int Weight { get; set; }
    public int BaseComfort { get; set; }
    public bool CreatesOpportunity { get; set; }
    public bool ProvidesInformation { get; set; }
    public bool CreatesUrgency { get; set; }
    public bool RequiresShadowTokens { get; set; }
}