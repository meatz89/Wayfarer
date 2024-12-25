
public class UserActionOption
{
    public int Index { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public string Description { get; set; }

    public string Display()
    {
        return $"{Index}. {Description}";
    }
}

public class TravelOption
{
    public int Index { get; set; }
    public LocationNames Location { get; set; }
}