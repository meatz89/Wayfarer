
public class UserActionOption
{
    public int Index { get; set; }
    public string Description { get; set; }
    public bool IsDisabled { get; set; }
    public ActionImplementation BasicAction { get; set; }
    public LocationNames Location { get; set; }
    public LocationSpotNames LocationSpot { get; set; }
    public CharacterNames Character { get; set; }

    public string Display()
    {
        return $"{Index}. {Description}";
    }
}

public class UserNarrativeChoiceOption
{
    public int Index { get; set; }
    public string Description { get; set; }
    public bool IsDisabled { get; set; }
    public Narrative Narrative { get; set; }
    public NarrativeStage NarrativeStage { get; set; }
    public NarrativeChoice NarrativeChoice { get; set; }
    public LocationNames Location { get; set; }
    public LocationSpotNames LocationSpot { get; set; }
    public CharacterNames Character { get; set; }

    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
