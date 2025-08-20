using System.Collections.Generic;
using Wayfarer.Game.ConversationSystem.Core;

namespace Wayfarer.Game.ConversationSystem.Models
{
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
    }

    /// <summary>
    /// Result of a single card's success roll
    /// </summary>
    public class SingleCardResult
    {
        /// <summary>
        /// The card that was played
        /// </summary>
        public ConversationCard Card { get; init; }

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
    }
}