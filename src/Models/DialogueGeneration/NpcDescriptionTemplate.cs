using System.Collections.Generic;

/// <summary>
/// Template for NPC description generation
/// </summary>
public class NpcDescriptionTemplate
{
    public Dictionary<string, List<string>> ProfessionBase { get; set; }

    public Dictionary<string, Dictionary<string, List<string>>> EmotionalModifiers { get; set; }
}