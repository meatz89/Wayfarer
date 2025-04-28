public class SerializableGameState
{
    // World state
    public string CurrentLocationId { get; set; }
    public string CurrentLocationSpotId { get; set; }
    public int CurrentDay { get; set; }
    public int CurrentTimeHours { get; set; }

    // Player state
    public SerializablePlayerState Player { get; set; }

    public SerializableGameState()
    {
        Player = new SerializablePlayerState();
    }
}
