using Xunit;

public class ConversationCardParserTest
{
    [Fact]
    public void ConvertDTOToCard_ExchangeType_SetsExchangeCardType()
    {
        // Arrange
        var dto = new ConversationCardDTO
        {
            Id = "test_exchange",
            Type = "Exchange",
            Description = "Test exchange card",
            Focus = 2,
            Difficulty = "Medium"
        };

        // Act
        var card = ConversationCardParser.ConvertDTOToCard(dto);

        // Assert
        Assert.Equal(CardType.Exchange, card.CardType);
        Assert.Equal("test_exchange", card.Id);
        Assert.Equal(2, card.Focus);
        Assert.Equal(Difficulty.Medium, card.Difficulty);
    }

    [Fact]
    public void ConvertDTOToCard_LetterRequestType_SetsLetterCardType()
    {
        // Arrange
        var dto = new ConversationCardDTO
        {
            Id = "test_letter",
            Type = "LetterRequest",
            Description = "Test letter request card",
            Focus = 1,
            SuccessEffect = new CardEffectDTO
            {
                Type = "OfferLetter",
                Value = "some_letter_id"
            }
        };

        // Act
        var card = ConversationCardParser.ConvertDTOToCard(dto);

        // Assert
        Assert.Equal(CardType.Letter, card.CardType);
        Assert.Equal("test_letter", card.Id);
        Assert.Equal(0, card.Focus); // Goal cards have 0 focus
        Assert.Equal(Difficulty.VeryEasy, card.Difficulty); // Goal cards always succeed
        Assert.Contains(CardProperty.Persistent, card.Properties);
    }

    [Fact]
    public void ConvertDTOToCard_GoalWithOfferLetter_SetsLetterCardType()
    {
        // Arrange
        var dto = new ConversationCardDTO
        {
            Id = "test_goal_letter",
            Type = "Goal",
            Description = "Test goal card with letter",
            Focus = 1,
            SuccessEffect = new CardEffectDTO
            {
                Type = "OfferLetter",
                Value = "letter_id"
            }
        };

        // Act
        var card = ConversationCardParser.ConvertDTOToCard(dto);

        // Assert
        Assert.Equal(CardType.Letter, card.CardType);
        Assert.Equal(CardEffectType.OfferLetter, card.SuccessEffect.Type);
    }

    [Fact]
    public void ConvertDTOToCard_GoalWithoutOfferLetter_SetsPromiseCardType()
    {
        // Arrange
        var dto = new ConversationCardDTO
        {
            Id = "test_goal_promise",
            Type = "Goal",
            Description = "Test goal card without letter",
            Focus = 1,
            SuccessEffect = new CardEffectDTO
            {
                Type = "AddRapport",
                Value = "2"
            }
        };

        // Act
        var card = ConversationCardParser.ConvertDTOToCard(dto);

        // Assert
        Assert.Equal(CardType.Promise, card.CardType);
        Assert.Equal(CardEffectType.AddRapport, card.SuccessEffect.Type);
    }

    [Fact]
    public void ConvertDTOToCard_ObservationType_SetsObservationCardType()
    {
        // Arrange
        var dto = new ConversationCardDTO
        {
            Id = "test_observation",
            Type = "Observation",
            Description = "Test observation card",
            Focus = 1
        };

        // Act
        var card = ConversationCardParser.ConvertDTOToCard(dto);

        // Assert
        Assert.Equal(CardType.Observation, card.CardType);
    }

    [Fact]
    public void ConvertDTOToCard_NormalType_SetsConversationCardType()
    {
        // Arrange
        var dto = new ConversationCardDTO
        {
            Id = "test_normal",
            Type = "Normal",
            Description = "Test normal card",
            Focus = 2
        };

        // Act
        var card = ConversationCardParser.ConvertDTOToCard(dto);

        // Assert
        Assert.Equal(CardType.Conversation, card.CardType);
        Assert.Equal(2, card.Focus);
    }

    [Fact]
    public void ConvertDTOToCard_NoType_DefaultsToConversationCardType()
    {
        // Arrange
        var dto = new ConversationCardDTO
        {
            Id = "test_no_type",
            Description = "Test card with no type",
            Focus = 1
        };

        // Act
        var card = ConversationCardParser.ConvertDTOToCard(dto);

        // Assert
        Assert.Equal(CardType.Conversation, card.CardType);
    }
}