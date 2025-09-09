using System.Collections.Generic;

/// <summary>
/// Generated narrative output from a narrative provider.
/// Contains all text content needed to display the current conversation turn.
/// </summary>
public class NarrativeOutput
{
    /// <summary>
    /// The NPC's dialogue for this conversation turn.
    /// Generated using backwards construction to work with all available player cards.
    /// </summary>
    public string NPCDialogue { get; set; }
    
    /// <summary>
    /// Optional environmental or descriptive narrative text.
    /// Describes the setting, NPC's body language, or other contextual details.
    /// Can be null if no environmental narrative is needed.
    /// </summary>
    public string NarrativeText { get; set; }
    
    /// <summary>
    /// Dictionary mapping card IDs to their narrative text.
    /// Each card gets contextually appropriate dialogue that responds to the NPC's statement.
    /// Key: Card ID, Value: Narrative text for that card's button/display.
    /// </summary>
    public Dictionary<string, string> CardNarratives { get; set; } = new Dictionary<string, string>();
    
    /// <summary>
    /// Optional hint about conversation progression or strategy.
    /// Helps guide the player toward meaningful choices or explains current situation.
    /// Can be null if no hint is appropriate for the current state.
    /// </summary>
    public string ProgressionHint { get; set; }
}