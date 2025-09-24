using NUnit.Framework;


namespace Wayfarer.Tests
{
    /// <summary>
    /// Integration test to verify the card category system works with the parser
    /// </summary>
    public class CardCategoryIntegrationTest
    {
        [Test]
        public void ConversationCardParser_ParsesCardWithAutoDetectedCategory()
        {
            // Arrange
            ConversationCardDTO dto = new ConversationCardDTO
            {
                Id = "test_strike_card",
                Description = "Test Strike Card",
                SuccessType = "Strike" // Should auto-map to Expression category
            };

            // Act
            ConversationCard result = ConversationCardParser.ConvertDTOToCard(dto);

            // Assert
            Assert.That(result.Category, Is.EqualTo(CardCategory.Expression));
            Assert.That(result.SuccessType, Is.EqualTo(SuccessEffectType.Strike));
            Assert.That(result.IsCategoryConsistent(), Is.True);
        }

        [Test]
        public void ConversationCardParser_ParsesCardWithExplicitCategory()
        {
            // Arrange
            ConversationCardDTO dto = new ConversationCardDTO
            {
                Id = "test_regulation_card",
                Description = "Test Regulation Card",
                Category = "Regulation", // Explicitly set category
                SuccessType = "Soothe"
            };

            // Act
            ConversationCard result = ConversationCardParser.ConvertDTOToCard(dto);

            // Assert
            Assert.That(result.Category, Is.EqualTo(CardCategory.Regulation));
            Assert.That(result.SuccessType, Is.EqualTo(SuccessEffectType.Soothe));
            Assert.That(result.IsCategoryConsistent(), Is.True);
        }
    }
}