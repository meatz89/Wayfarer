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

// Summarized player information for AI context
public class PlayerSummary
{
    public string Name { get; set; }
    public string Background { get; set; }
    public int Level { get; set; }
    public List<string> TopSkills { get; set; }
    public List<ChoiceCard> TopCards { get; set; }
    public Dictionary<string, string> KeyRelationships { get; set; }
}
