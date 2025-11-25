/// <summary>
/// Immutable snapshot of NPC state at spawn time
/// Captures identity and categorical properties for tracing
/// </summary>
public class NPCSnapshot
{
    public string Name { get; set; }
    public Professions Profession { get; set; }
    public PersonalityType PersonalityType { get; set; }
    public NPCSocialStanding SocialStanding { get; set; }
    public NPCStoryRole StoryRole { get; set; }
}
