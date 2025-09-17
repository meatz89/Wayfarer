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
        /// Get all cards to display, combining exhausting cards, regular hand cards, and animating cards.
        /// Ensures promise cards appear first and maintains original positions during animation.
        /// CRITICAL: Exhausting cards are shown FIRST to maintain their DOM presence for animation.
        /// </summary>
        public List<CardDisplayInfo> GetAllDisplayCards(ConversationSession session, List<AnimatingCard> animatingCards, ExhaustingCardStore exhaustingCardStore)
        {
            List<CardDisplayInfo> displayCards = new List<CardDisplayInfo>();

            // FIRST: Add exhausting cards (these are being animated out)
            // These are copies that exist ONLY for animation display
            if (exhaustingCardStore != null)
            {
                foreach (var exhaustingCard in exhaustingCardStore.GetExhaustingCards())
                {
                    displayCards.Add(new CardDisplayInfo
                    {
                        Card = exhaustingCard.Card,
                        IsAnimating = false,  // Not using the old animation system
                        IsExhausting = true,  // New flag for exhaust animations
                        AnimationDelay = exhaustingCard.AnimationDelay,
                        AnimationDirection = exhaustingCard.AnimationDirection,
                        AnimationState = "card-exhausting"
                    });
                }
            }

            // SECOND: Add current hand cards (preserving positions)
            if (session?.Deck?.Hand?.Cards != null)
            {
                List<CardInstance> handList = session.Deck.Hand.Cards.ToList();

                // Add all cards, checking if they're animating
                foreach (CardInstance? card in handList)
                {
                    AnimatingCard? animatingCard = animatingCards.FirstOrDefault(ac => ac.Card.InstanceId == card.InstanceId);

                    displayCards.Add(new CardDisplayInfo
                    {
                        Card = card,
                        IsAnimating = animatingCard != null,
                        IsExhausting = false,
                        AnimationDelay = 0,
                        AnimationDirection = null,
                        AnimationState = animatingCard != null
                            ? GetAnimationClass(animatingCard.AnimationType)
                            : null
                    });
                }
            }

            // THIRD: Add any animating cards that are no longer in the hand (just played and removed)
            foreach (AnimatingCard animatingCard in animatingCards)
            {
                bool inHand = session?.Deck?.Hand?.Cards?.Any(c => c.InstanceId == animatingCard.Card.InstanceId) ?? false;

                if (!inHand)
                {
                    // Insert at original position or at end
                    int insertPos = Math.Min(animatingCard.OriginalPosition, displayCards.Count);

                    displayCards.Insert(insertPos, new CardDisplayInfo
                    {
                        Card = animatingCard.Card,
                        IsAnimating = true,
                        IsExhausting = false,
                        AnimationDelay = 0,
                        AnimationDirection = null,
                        AnimationState = GetAnimationClass(animatingCard.AnimationType)
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
            List<CardDisplayInfo> promiseCards = cards.Where(dc => dc.Card.CardType == CardType.Letter || dc.Card.CardType == CardType.Promise || dc.Card.CardType == CardType.BurdenGoal).ToList();
            List<CardDisplayInfo> regularCards = cards.Where(dc => !(dc.Card.CardType == CardType.Letter || dc.Card.CardType == CardType.Promise || dc.Card.CardType == CardType.BurdenGoal)).ToList();

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
            if (card == null || session?.Deck?.Hand?.Cards == null) return -1;

            List<CardInstance> handList = session.Deck.Hand.Cards.ToList();
            for (int i = 0; i < handList.Count; i++)
            {
                if (handList[i].InstanceId == card.InstanceId)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the CSS animation class for a given animation type
        /// </summary>
        private string GetAnimationClass(CardAnimationType animationType)
        {
            return animationType switch
            {
                CardAnimationType.PlayedSuccess => "card-played-success",
                CardAnimationType.PlayedFailure => "card-played-failure",
                CardAnimationType.Exhausting => "card-exhausting",
                _ => "card-played-failure"
            };
        }
    }
}