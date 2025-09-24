using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer
{
    /// <summary>
    /// Manages card display logic for conversations.
    /// SIMPLIFIED: Only displays actual hand cards and animating played cards.
    /// </summary>
    public class CardDisplayManager
    {
        /// <summary>
        /// Get all cards to display: hand cards + animating played cards.
        /// Promise cards appear first for visibility.
        /// </summary>
        public List<CardDisplayInfo> GetAllDisplayCards(IReadOnlyList<CardInstance> handCards, List<AnimatingCard> animatingCards)
        {
            List<CardDisplayInfo> displayCards = new List<CardDisplayInfo>();

            // SINGLE CHECK: If animations disabled, just return hand cards without any animation info
            if (!CardAnimationManager.AnimationsEnabled)
            {
                if (handCards != null)
                {
                    foreach (CardInstance? card in handCards)
                    {
                        displayCards.Add(new CardDisplayInfo
                        {
                            Card = card,
                            IsAnimating = false,
                            IsExhausting = false,
                            AnimationDelay = 0,
                            AnimationDirection = null,
                            AnimationState = null
                        });
                    }
                }
                return SortCardsWithPromiseFirst(displayCards);
            }

            // ANIMATIONS ENABLED: Normal animation logic
            // FIRST: Add current hand cards
            if (handCards != null)
            {
                List<CardInstance> handList = handCards.ToList();

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

            // SECOND: Add animating cards that were just played
            foreach (AnimatingCard animatingCard in animatingCards)
            {
                bool inHand = handCards?.Any(c => c.InstanceId == animatingCard.Card.InstanceId) ?? false;

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
            List<CardDisplayInfo> promiseCards = cards.Where(dc => dc.Card.CardType == CardType.Letter || dc.Card.CardType == CardType.Promise || dc.Card.CardType == CardType.Letter).ToList();
            List<CardDisplayInfo> regularCards = cards.Where(dc => !(dc.Card.CardType == CardType.Letter || dc.Card.CardType == CardType.Promise || dc.Card.CardType == CardType.Letter)).ToList();

            List<CardDisplayInfo> sorted = new List<CardDisplayInfo>();
            sorted.AddRange(promiseCards);
            sorted.AddRange(regularCards);

            return sorted;
        }

        /// <summary>
        /// Get the position of a card in the current hand.
        /// </summary>
        public int GetCardPosition(CardInstance card, IReadOnlyList<CardInstance> handCards)
        {
            if (card == null || handCards == null) return -1;

            List<CardInstance> handList = handCards.ToList();
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