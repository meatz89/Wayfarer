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
/// List of card-specific narratives with their IDs and provider sources.
/// Each card gets contextually appropriate dialogue that responds to the NPC's statement.
/// Using strongly typed objects instead of dictionary for better type safety.
/// </summary>
public List<CardNarrative> CardNarratives { get; set; } = new List<CardNarrative>();

/// <summary>
/// Optional hint about conversation progression or strategy.
/// Helps guide the player toward meaningful choices or explains current situation.
/// Can be null if no hint is appropriate for the current state.
/// </summary>
public string ProgressionHint { get; set; }

/// <summary>
/// Identifies which provider generated this narrative content.
/// Used to apply different visual styling in the UI.
/// </summary>
public NarrativeProviderType ProviderSource { get; set; } = NarrativeProviderType.JsonFallback;
}