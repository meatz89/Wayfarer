public class QuestStep
{
    public string Description { get; set; }
    public BasicAction QuestAction { get; set; }
    public List<Requirement> Requirements = new();
    public List<Outcome> Outcomes = new();
    public List<IGameStateModifier> StateModifiers = new();
    public LocationNames Location { get; set; }
    public LocationSpotNames LocationSpot { get; set; }
    public CharacterNames Character { get; set; }
}
