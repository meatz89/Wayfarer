public class SessionCardDeck
{
    private readonly List<CardInstance> drawPile = new();
    private readonly List<CardInstance> discardPile = new();
    private readonly string npcId;
    private readonly Random random = new Random();

    public SessionCardDeck(string npcId)
    {
        this.npcId = npcId;
    }

    public static SessionCardDeck CreateFromTemplates(List<ConversationCard> templates, string npcId)
    {
        SessionCardDeck deck = new SessionCardDeck(npcId);
        foreach (ConversationCard template in templates)
        {
            var cardInstance = new CardInstance(template);
            
            // If this is an exchange card, set up the Context with ExchangeData
            if (cardInstance.Properties.Contains(CardProperty.Exchange) && 
                cardInstance.SuccessEffect?.ExchangeData != null)
            {
                var context = new CardContext();
                context.ExchangeData = cardInstance.SuccessEffect.ExchangeData;
                cardInstance.Context = context;
            }
            
            // Add to draw pile initially
            deck.drawPile.Add(cardInstance);
        }
        
        // Shuffle the initial draw pile
        deck.ShuffleDrawPile();
        
        return deck;
    }

    public void AddCard(CardInstance card)
    {
        // Add new cards directly to draw pile
        drawPile.Add(card);
    }

    public void AddGoalCard(CardInstance goalCard)
    {
        // Add goal card to draw pile at conversation start
        if (goalCard != null)
        {
            drawPile.Add(goalCard);
            ShuffleDrawPile(); // Shuffle after adding goal card
        }
    }

    public CardInstance DrawCard()
    {
        // If draw pile is empty, reshuffle discard pile into draw pile
        if (drawPile.Count == 0 && discardPile.Count > 0)
        {
            ReshuffleDiscardPile();
        }

        // If still no cards available, return null
        if (drawPile.Count == 0)
        {
            return null;
        }

        // Draw from top of draw pile
        CardInstance card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }

    public List<CardInstance> DrawCards(int count)
    {
        List<CardInstance> drawn = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            // Check if we need to reshuffle before each draw
            if (drawPile.Count == 0 && discardPile.Count > 0)
            {
                ReshuffleDiscardPile();
            }

            if (drawPile.Count > 0)
            {
                CardInstance card = drawPile[0];
                drawPile.RemoveAt(0);
                drawn.Add(card);
            }
        }
        return drawn;
    }

    public void DiscardCard(CardInstance card)
    {
        // Add to discard pile when card is played or exhausted
        if (card != null && !discardPile.Contains(card))
        {
            discardPile.Add(card);
        }
    }

    public void DiscardCard(string instanceId)
    {
        // Find card in hand or other locations and move to discard
        // This is for compatibility with existing code
        // The card should already be removed from hand by the caller
    }

    public void Discard(CardInstance card)
    {
        DiscardCard(card);
    }

    private void ReshuffleDiscardPile()
    {
        Console.WriteLine($"[SessionCardDeck] Reshuffling {discardPile.Count} cards from discard pile into draw pile");
        
        // Move all cards from discard to draw pile
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        
        // Shuffle the draw pile
        ShuffleDrawPile();
    }

    private void ShuffleDrawPile()
    {
        // Fisher-Yates shuffle
        int n = drawPile.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            CardInstance temp = drawPile[k];
            drawPile[k] = drawPile[n];
            drawPile[n] = temp;
        }
    }

    public void ResetForNewConversation()
    {
        // Move all cards back to draw pile for a fresh conversation
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDrawPile();
    }

    public List<CardInstance> GetAllCards()
    {
        // Return all cards from both piles
        var allCards = new List<CardInstance>();
        allCards.AddRange(drawPile);
        allCards.AddRange(discardPile);
        return allCards;
    }

    public void Shuffle()
    {
        ShuffleDrawPile();
    }

    public List<CardInstance> DrawFilteredByProperties(List<CardProperty> requiredProperties, int count)
    {
        List<CardInstance> available = new List<CardInstance>();
        List<CardInstance> drawn = new List<CardInstance>();
        
        // Find all cards in draw pile with required properties
        for (int i = drawPile.Count - 1; i >= 0; i--)
        {
            CardInstance card = drawPile[i];
            bool hasAllProperties = true;
            foreach (CardProperty prop in requiredProperties)
            {
                if (!card.Properties.Contains(prop))
                {
                    hasAllProperties = false;
                    break;
                }
            }
            if (hasAllProperties)
            {
                available.Add(card);
            }
        }

        // Draw the requested number of cards
        for (int i = 0; i < Math.Min(count, available.Count); i++)
        {
            CardInstance card = available[random.Next(available.Count)];
            drawPile.Remove(card);
            drawn.Add(card);
            available.Remove(card); // Prevent drawing same card twice
        }
        
        return drawn;
    }
    
    // Helper methods for common property-based filtering
    public List<CardInstance> DrawRequestCards(int count)
    {
        // Request cards have both Impulse AND Opening properties
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Impulse, CardProperty.Opening }, count);
    }
    
    public List<CardInstance> DrawBurdenCards(int count)
    {
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Burden }, count);
    }
    
    public List<CardInstance> DrawObservableCards(int count)
    {
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Observable }, count);
    }
    
    public List<CardInstance> DrawExchangeCards(int count)
    {
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Exchange }, count);
    }

    public int RemainingCards => drawPile.Count;
    
    public int DiscardPileCount => discardPile.Count;
    
    public int TotalCards => drawPile.Count + discardPile.Count;

    public bool HasCardsAvailable()
    {
        // Cards are available if draw pile has cards OR discard pile can be reshuffled
        return drawPile.Count > 0 || discardPile.Count > 0;
    }

    public void Clear()
    {
        drawPile.Clear();
        discardPile.Clear();
    }
}