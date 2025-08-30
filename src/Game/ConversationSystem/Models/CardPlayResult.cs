using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Result of playing one or more conversation cards
/// </summary>
public class CardPlayResult
{
    /// <summary>
    /// Total comfort gained from all cards and bonuses
    /// </summary>
    public int TotalComfort { get; init; }

    /// <summary>
    /// New emotional state if changed (null if no change)
    /// </summary>
    public EmotionalState? NewState { get; init; }

    /// <summary>
    /// Individual results for each card played
    /// </summary>
    public List<SingleCardResult> Results { get; init; }

    /// <summary>
    /// Bonus comfort from playing same type cards
    /// </summary>
    public int SetBonus { get; init; }

    /// <summary>
    /// Bonus from CONNECTED state
    /// </summary>
    public int ConnectedBonus { get; init; }

    /// <summary>
    /// Bonus from EAGER state
    /// </summary>
    public int EagerBonus { get; init; }

    /// <summary>
    /// Whether any letter was delivered
    /// </summary>
    public bool DeliveredLetter { get; init; }

    /// <summary>
    /// Whether obligations were manipulated
    /// </summary>
    public bool ManipulatedObligations { get; init; }

    /// <summary>
    /// Letter negotiation results (if letter cards were played)
    /// </summary>
    public List<LetterNegotiationResult> LetterNegotiations { get; init; } = new List<LetterNegotiationResult>();
    
    /// <summary>
    /// Overall success of the play (true if at least one card succeeded)
    /// </summary>
    public bool Success => Results?.Any(r => r.Success) ?? false;
}

/// <summary>
/// Result of a single card's success roll
/// </summary>
public class SingleCardResult
{
    /// <summary>
    /// The card that was played
    /// </summary>
    public CardInstance Card { get; init; }

    /// <summary>
    /// Whether the roll was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Comfort gained from this card
    /// </summary>
    public int Comfort { get; init; }

    /// <summary>
    /// The actual roll (0-99)
    /// </summary>
    public int Roll { get; init; }

    /// <summary>
    /// The success chance that was needed
    /// </summary>
    public int SuccessChance { get; init; }

    /// <summary>
    /// Patience added to conversation (for Patience cards)
    /// </summary>
    public int PatienceAdded { get; init; }
}

/// <summary>
/// Result of negotiating a letter through card play
/// </summary>
public class LetterNegotiationResult
{
    /// <summary>
    /// The promise card that was played
    /// </summary>
    public string PromiseCardId { get; init; }

    /// <summary>
    /// Whether the negotiation was successful
    /// </summary>
    public bool NegotiationSuccess { get; init; }

    /// <summary>
    /// Final terms after negotiation (success or failure terms)  
    /// </summary>
    public TermDetails FinalTerms { get; init; }

    /// <summary>
    /// The promise card that generated this negotiation
    /// </summary>
    public CardInstance SourcePromiseCard { get; init; }

    /// <summary>
    /// The delivery obligation that needs to be created
    /// </summary>
    public DeliveryObligation CreatedObligation { get; init; }
}