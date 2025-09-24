using System.Collections.Generic;

namespace Wayfarer
{
    /// <summary>
    /// Simple presentation model for cards in the conversation UI.
    /// Static display only - no animations, no transitions, no DOM manipulation.
    /// Perfect card stability from creation to removal.
    /// </summary>
    public class CardDisplayInfo
    {
        // Core card data - ONLY what's needed for static display
        public CardInstance Card { get; set; }

        /// <summary>
        /// Simple constructor for static card display
        /// </summary>
        public CardDisplayInfo(CardInstance card)
        {
            Card = card;
        }
    }
}