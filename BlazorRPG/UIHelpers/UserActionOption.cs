
public class UserActionOption
{
    public int Index { get; set; }
    public string Description { get; set; }
    public BasicActionDefinition Action { get; set; }

    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
