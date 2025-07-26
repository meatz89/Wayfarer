public class SerializableGameWorld
{
    public string CurrentLocationId { get; set; }
    public string CurrentLocationSpotId { get; set; }
    public int CurrentDay { get; set; }
    public int CurrentTimeHours { get; set; }
    public SerializablePlayerState Player { get; set; }
    
    // Narrative state
    public FlagServiceState FlagServiceState { get; set; }
    public Dictionary<string, NarrativeState> ActiveNarratives { get; set; }
}
