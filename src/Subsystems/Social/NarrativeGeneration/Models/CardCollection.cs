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
    /// Initiative cost required to play this card.
    /// Indicates the intensity/commitment level of the card's effect.
    /// </summary>
    public int InitiativeCost { get; set; }

    /// <summary>
    /// Conversational move type - what kind of statement this represents.
    /// Remark (pointed statement), Observation (supportive), or Argument (complex developed point).
    /// Null for Letter/Promise/Burden cards (not part of conversation mechanics).
    /// </summary>
    public ConversationalMove? Move { get; set; }

    /// <summary>
    /// Player stat this card requires for stat checks.
    /// Determines which stat threshold must be met to play the card successfully.
    /// Null for cards not bound to any stat.
    /// </summary>
    public PlayerStatType? BoundStat { get; set; }

    /// <summary>
    /// Card depth (1-10) indicating stat requirement tier.
    /// Higher depth = higher stat thresholds required.
    /// </summary>
    public CardDepth Depth { get; set; }

    /// <summary>
    /// Primary resource affected by this card's effect.
    /// Null for compound effects (multiple resources affected).
    /// </summary>
    public SocialChallengeResourceType? PrimaryTargetResource { get; set; }

    /// <summary>
    /// Formula type determining how effect is calculated.
    /// Fixed, Scaling, Conditional, Trading, Setting, or Compound.
    /// </summary>
    public EffectFormulaType? PrimaryFormulaType { get; set; }

    /// <summary>
    /// True if this card has compound effects (multiple resource changes).
    /// Compound effects need special handling for narrative generation.
    /// </summary>
    public bool IsCompound { get; set; }

    /// <summary>
    /// Persistence type determining when card is removed from hand.
    /// Affects narrative timing requirements (Echo for repeatability, Statement for one-time effects).
    /// </summary>
    public PersistenceType Persistence { get; set; }

    /// <summary>
    /// Narrative category for backwards construction.
    /// Determines what kind of NPC dialogue this card should respond to.
    /// Derived from card's mechanical properties (persistence, token type, success effect).
    /// </summary>
    public NarrativeCategoryType NarrativeCategory { get; set; }

    /// <summary>
    /// Indicates if this card has a draw effect (draws additional cards).
    /// Used for narrative generation to avoid ID string parsing.
    /// </summary>
    public bool HasDrawEffect { get; set; }

    /// <summary>
    /// Indicates if this card has a focus effect (manipulates initiative/momentum).
    /// Used for narrative generation to avoid ID string parsing.
    /// </summary>
    public bool HasFocusEffect { get; set; }
}