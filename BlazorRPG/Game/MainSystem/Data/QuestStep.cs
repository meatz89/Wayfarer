public class QuestStep
{
    public string Description { get; set; }
    public ActionImplementation QuestAction { get; set; }
    public List<Requirement> Requirements = new();
    public List<Outcome> Outcomes = new();
    public List<IGameStateModifier> StateModifiers = new();
    public string Location { get; set; }
    public string Character { get; set; }
}
