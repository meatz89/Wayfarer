public class SerializableGameWorld
{
    public string CurrentLocationId { get; set; }
    public string CurrentLocationSpotId { get; set; }
    public int CurrentDay { get; set; }
    public int CurrentTimeHours { get; set; }
    public SerializablePlayerState Player { get; set; }
    public FlagServiceState FlagServiceState { get; set; }
    public NarrativeManagerState NarrativeManagerState { get; set; }
}

public class NarrativeManagerState
{
    public Dictionary<string, NarrativeDefinition> ActiveNarratives { get; set; }
    public Dictionary<string, NarrativeDefinition> NarrativeDefinitions { get; set; }
}