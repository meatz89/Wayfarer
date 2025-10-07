/// <summary>
/// Base class for all conversation contexts containing common properties
/// </summary>
public abstract class ConversationContextBase
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string NpcId { get; set; }
    public NPC Npc { get; set; }
    public string RequestId { get; set; }
    public string ConversationTypeId { get; set; }
    public ConnectionState InitialState { get; set; }
    public SocialChallengeSession Session { get; set; }
    public List<CardInstance> ObservationCards { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }
    public string RequestText { get; set; }  // Text displayed when NPC presents a request

    protected ConversationContextBase()
    {
        ObservationCards = new List<CardInstance>();
    }
}