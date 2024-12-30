
public class UserActionOption
{
    public int Index { get; set; }
    public string Description { get; set; }
    public BasicAction BasicAction { get; set; }
    public bool IsDisabled { get; set; }

    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
