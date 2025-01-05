public class ActionGenerationContext
{
    public LocationTypes LocationType { get; set; }
    public BasicActionTypes BaseAction { get; set; }
    public SpaceProperties Space { get; set; }
    public SocialContext Social { get; set; }
    public ActivityProperties Activity { get; set; }
    public List<ActionModifier> Modifiers { get; set; } = new();
}
