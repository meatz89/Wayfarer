using System.Collections.Generic;

/// <summary>
/// Container for all dialogue templates loaded from JSON
/// </summary>
public class DialogueTemplates
{
    public Dictionary<string, ConnectionStateTemplate> ConnectionStateDialogue { get; set; }

    public CardDialogueTemplate CardDialogue { get; set; }

    public NpcDescriptionTemplate NpcDescriptions { get; set; }

    public Dictionary<string, Dictionary<string, string>> NarrativeElements { get; set; }
}