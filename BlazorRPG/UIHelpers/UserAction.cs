
public class UserAction
{
    public int Index { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public string Description { get; set; }

    public string Display()
    {
        return $"{Index}. {Description}";
    }
}