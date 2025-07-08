public class SpotLevel
{
    public int Level { get; set; }
    public string Description { get; set; }
    public List<string> AddedActionIds { get; set; } = new List<string>();
    public List<string> RemovedActionIds { get; set; } = new List<string>();
    public string EncounterActionId { get; set; }
}