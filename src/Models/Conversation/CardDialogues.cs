/// <summary>
/// Container for card dialogues and narrative templates loaded from JSON.
/// </summary>
public class CardDialogues
{
    public Dictionary<string, CardDialogue> dialogues { get; set; }
    public Dictionary<string, Dictionary<string, Dictionary<string, string>>> narrativeTemplates { get; set; }
}
