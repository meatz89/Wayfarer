using System.Collections.Generic;

/// <summary>
/// Narrative text and dialogues for conversation states and transitions.
/// </summary>
public class ConversationNarratives
{
    public string initialNarrative { get; set; }
    public Dictionary<string, string> listenNarratives { get; set; }
    public Dictionary<string, string> speakNarratives { get; set; }
    public Dictionary<string, string> stateDialogues { get; set; }
    public Dictionary<string, string> initialDialogues { get; set; }
    public Dictionary<string, string> flowResponses { get; set; }
    public Dictionary<string, string> stateTransitionVerbs { get; set; }
    public Dictionary<string, string> cardStateTransitionDialogues { get; set; }
}
