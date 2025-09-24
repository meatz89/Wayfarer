using NUnit.Framework;


namespace Wayfarer.Tests
{
    /// <summary>
    /// Simple tests for the card category system implementation
    /// </summary>
    public class CardCategorySystemTestSimple
    {
        [Test]
        public void DetermineCategoryFromEffect_Strike_ReturnsExpression()
        {
            // Act
            CardCategory result = ConversationCard.DetermineCategoryFromEffect(SuccessEffectType.Strike);

            // Assert
            Assert.That(result, Is.EqualTo(CardCategory.Expression));
        }

        [Test]
        public void DetermineCategoryFromEffect_Advancing_ReturnsRealization()
        {
            // Act
            CardCategory result = ConversationCard.DetermineCategoryFromEffect(SuccessEffectType.Advancing);

            // Assert
            Assert.That(result, Is.EqualTo(CardCategory.Realization));
        }

        [Test]
        public void DetermineCategoryFromEffect_Soothe_ReturnsRegulation()
        {
            // Act
            CardCategory result = ConversationCard.DetermineCategoryFromEffect(SuccessEffectType.Soothe);

            // Assert
            Assert.That(result, Is.EqualTo(CardCategory.Regulation));
        }

        [Test]
        public void ConversationCard_IsCategoryConsistent_WhenMatched_ReturnsTrue()
        {
            // Arrange
            ConversationCard card = new ConversationCard
            {
                Id = "test_card",
                Description = "Test Card",
                SuccessType = SuccessEffectType.Strike,
                Category = CardCategory.Expression
            };

            // Act
            bool result = card.IsCategoryConsistent();

            // Assert
            Assert.That(result, Is.True);
        }
    }
}