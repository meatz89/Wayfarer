public class EncounterContext
{
    public Location Location { get; set; }
    public LocationSpot LocationSpot { get; set; }
    public List<Character> PresentCharacters { get; set; }
    public List<Opportunity> AvailableOpportunities { get; set; }
    public string TimeOfDay { get; set; }
    public List<IEnvironmentalProperty> CurrentEnvironmentalProperties { get; set; }
    public PlayerSummary Player { get; set; }
    public List<string> PreviousInteractions { get; internal set; }

    public ActionImplementation ActionImplementation;
    public BasicActionTypes ActionType;
    public IChoice LastChoice;
}
