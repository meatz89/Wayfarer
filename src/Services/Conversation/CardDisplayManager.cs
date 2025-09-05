using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer
{
    /// <summary>
    /// Manages card display logic for conversations, including animations and positioning.
    /// Uses composition to provide card display functionality to ConversationContent component.
    /// </summary>
    public class CardDisplayManager
    {
        /// <summary>
        /// Get all cards to display, combining regular hand cards with animating cards.
        /// Ensures promise cards appear first and maintains original positions during animation.
        /// </summary>
        public List<CardDisplayInfo> GetAllDisplayCards(ConversationSession session, List<AnimatingCard> animatingCards)
        {
            List<CardDisplayInfo> displayCards = new List<CardDisplayInfo>();

            // Start with ALL cards in their current hand order (preserving positions)
            if (session?.HandCards != null)
            {
                List<CardInstance> handList = session.HandCards.ToList();

                // Add all cards, checking if they're animating
                foreach (CardInstance? card in handList)
                {
                    AnimatingCard? animatingCard = animatingCards.FirstOrDefault(ac => ac.Card.InstanceId == card.InstanceId);

                    displayCards.Add(new CardDisplayInfo
                    {
                        Card = card,
                        IsAnimating = animatingCard != null,
                        AnimationState = animatingCard != null
                            ? (animatingCard.Success ? "card-played-success" : "card-played-failure")
                            : null
                    });
                }
            }

            // Add any animating cards that are no longer in the hand (just played and removed)
            foreach (AnimatingCard animatingCard in animatingCards)
            {
                bool inHand = session?.HandCards?.Any(c => c.InstanceId == animatingCard.Card.InstanceId) ?? false;

                if (!inHand)
                {
                    // Insert at original position or at end
                    int insertPos = Math.Min(animatingCard.OriginalPosition, displayCards.Count);

                    displayCards.Insert(insertPos, new CardDisplayInfo
                    {
                        Card = animatingCard.Card,
                        IsAnimating = true,
                        AnimationState = animatingCard.Success ? "card-played-success" : "card-played-failure"
                    });
                }
            }

            // Sort to ensure promise cards appear first while preserving relative positions
            return SortCardsWithPromiseFirst(displayCards);
        }

        /// <summary>
        /// Sort cards so promise/delivery cards appear first, preserving relative order within categories.
        /// </summary>
        private List<CardDisplayInfo> SortCardsWithPromiseFirst(List<CardDisplayInfo> cards)
        {
            List<CardDisplayInfo> promiseCards = cards.Where(dc => dc.Card.Properties.Contains(CardProperty.DeliveryEligible)).ToList();
            List<CardDisplayInfo> regularCards = cards.Where(dc => !dc.Card.Properties.Contains(CardProperty.DeliveryEligible)).ToList();

            List<CardDisplayInfo> sorted = new List<CardDisplayInfo>();
            sorted.AddRange(promiseCards);
            sorted.AddRange(regularCards);

            return sorted;
        }

        /// <summary>
        /// Get the position of a card in the current hand.
        /// </summary>
        public int GetCardPosition(CardInstance card, ConversationSession session)
        {
            if (card == null || session?.HandCards == null) return -1;

            List<CardInstance> handList = session.HandCards.ToList();
            for (int i = 0; i < handList.Count; i++)
            {
                if (handList[i].InstanceId == card.InstanceId)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}