using System.Collections.Generic;

/// <summary>
/// Wrapper for active cards available to the player.
/// Used by narrative providers to analyze card options and generate appropriate NPC dialogue.
/// </summary>
public class CardCollection
{
    /// <summary>
    /// List of cards currently available for the player to use.
    /// Used for backwards narrative construction - NPC dialogue generated to work with all these cards.
    /// </summary>
    public List<CardInfo> Cards { get; set; } = new List<CardInfo>();
}

/// <summary>
/// Simplified card information for narrative generation.
/// Contains only the mechanical data needed to understand card behavior and generate narratives.
/// </summary>
public class CardInfo
{
    /// <summary>
    /// Unique identifier for the card.
    /// Used to map generated narratives back to specific cards.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Focus cost required to play this card.
    /// Indicates the intensity/commitment level of the card's effect.
    /// </summary>
    public int Focus { get; set; }
    
    /// <summary>
    /// Base difficulty level of the card.
    /// Affects success percentage and determines narrative boldness.
    /// </summary>
    public Difficulty Difficulty { get; set; }
    
    /// <summary>
    /// Description of the card's mechanical effect.
    /// Used to understand what the card does for narrative context.
    /// </summary>
    public string Effect { get; set; }
    
    /// <summary>
    /// Persistence type determining when card is removed from hand.
    /// Affects narrative timing requirements (Impulse needs urgency, Opening invites elaboration).
    /// </summary>
    public CardPersistence Persistence { get; set; }
    
    /// <summary>
    /// Narrative category for backwards construction.
    /// Examples: "risk", "support", "pressure", "probe"
    /// Helps determine what kind of NPC dialogue this card should respond to.
    /// </summary>
    public string NarrativeCategory { get; set; }
}

/// <summary>
/// Card persistence types for narrative timing requirements.
/// </summary>
public enum CardPersistence
{
    /// <summary>
    /// Card stays in hand until played or conversation ends.
    /// No special narrative timing requirements.
    /// </summary>
    Persistent,
    
    /// <summary>
    /// Card removed after SPEAK action if unplayed.
    /// Narrative should create urgency requiring immediate response.
    /// </summary>
    Impulse,
    
    /// <summary>
    /// Card removed after LISTEN action if unplayed.
    /// Narrative should invite elaboration or follow-up questions.
    /// </summary>
    Opening
}