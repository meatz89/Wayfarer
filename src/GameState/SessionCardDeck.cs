public class SessionCardDeck
{
    private readonly List<CardInstance> allCards = new();
    private readonly HashSet<string> drawnCardIds = new();
    private readonly HashSet<string> discardedCardIds = new();
    private readonly string npcId;

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
                cardInstance.SuccessEffect?.Data != null)
            {
                var context = new CardContext();
                var exchangeData = new ExchangeData();
                
                // Extract cost from SuccessEffect.Data
                if (cardInstance.SuccessEffect.Data.TryGetValue("cost", out object costObj) && 
                    costObj is Dictionary<string, object> costDict)
                {
                    exchangeData.Cost = new Dictionary<ResourceType, int>();
                    foreach (var kvp in costDict)
                    {
                        // Try to parse as ResourceType enum first
                        if (Enum.TryParse<ResourceType>(kvp.Key, true, out var resourceType) && 
                            kvp.Value is int amount)
                        {
                            exchangeData.Cost[resourceType] = amount;
                        }
                        // Handle "coins" specifically since it maps to Coins enum value
                        else if (kvp.Key.ToLower() == "coins" && kvp.Value is int coinAmount)
                        {
                            exchangeData.Cost[ResourceType.Coins] = coinAmount;
                        }
                        // Handle other resource mappings
                        else if (kvp.Key.ToLower() == "health" && kvp.Value is int healthAmount)
                        {
                            exchangeData.Cost[ResourceType.Health] = healthAmount;
                        }
                        else if (kvp.Key.ToLower() == "hunger" && kvp.Value is int hungerAmount)
                        {
                            exchangeData.Cost[ResourceType.Hunger] = hungerAmount;
                        }
                        else if (kvp.Key.ToLower() == "food" && kvp.Value is int foodAmount)
                        {
                            exchangeData.Cost[ResourceType.Food] = foodAmount;
                        }
                        else if (kvp.Key.ToLower() == "attention" && kvp.Value is int attentionAmount)
                        {
                            exchangeData.Cost[ResourceType.Attention] = attentionAmount;
                        }
                    }
                }
                
                // Extract reward from SuccessEffect.Data 
                exchangeData.Reward = new Dictionary<ResourceType, int>();
                if (cardInstance.SuccessEffect.Data.TryGetValue("reward", out object rewardObj) && 
                    rewardObj is Dictionary<string, object> rewardDict)
                {
                    foreach (var kvp in rewardDict)
                    {
                        // Handle standard resource types
                        if (Enum.TryParse<ResourceType>(kvp.Key, true, out var resourceType) && 
                            kvp.Value is int amount)
                        {
                            exchangeData.Reward[resourceType] = amount;
                        }
                        // Handle special cases like "hunger": -20 (reduces hunger)
                        else if (kvp.Key.ToLower() == "hunger" && kvp.Value is int hungerAmount)
                        {
                            exchangeData.Reward[ResourceType.Food] = Math.Abs(hungerAmount);
                        }
                        else if (kvp.Key.ToLower() == "stamina" && kvp.Value is int staminaAmount)
                        {
                            exchangeData.Reward[ResourceType.Food] = staminaAmount;
                        }
                        else if (kvp.Key.ToLower() == "patience" && kvp.Value is int patienceAmount)
                        {
                            // Store as attention for now
                            exchangeData.Reward[ResourceType.Attention] = patienceAmount;
                        }
                        // Handle item placeholders - store in PlayerGives/PlayerReceives for display
                        else if ((kvp.Key.ToLower() == "item" || kvp.Key.ToLower() == "items"))
                        {
                            // These are placeholder values - store them in the string dictionaries
                            if (exchangeData.PlayerReceives == null)
                                exchangeData.PlayerReceives = new Dictionary<string, int>();
                                
                            if (kvp.Value is string itemName)
                            {
                                exchangeData.PlayerReceives[itemName] = 1;
                            }
                            else if (kvp.Value is int itemCount)
                            {
                                exchangeData.PlayerReceives["items"] = itemCount;
                            }
                        }
                    }
                }
                
                context.ExchangeData = exchangeData;
                cardInstance.Context = context;
            }
            
            deck.AddCard(cardInstance);
        }
        return deck;
    }

    public void AddCard(CardInstance card)
    {
        allCards.Add(card);
    }

    public CardInstance DrawCard()
    {
        List<CardInstance> available = new List<CardInstance>();
        foreach (CardInstance c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId))
                available.Add(c);
        }
        if (!available.Any()) return null;

        CardInstance card = available[new Random().Next(available.Count)];
        drawnCardIds.Add(card.InstanceId);
        return card;
    }

    public List<CardInstance> DrawCards(int count)
    {
        List<CardInstance> drawn = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            CardInstance card = DrawCard();
            if (card != null) drawn.Add(card);
        }
        return drawn;
    }

    public void DiscardCard(string instanceId)
    {
        discardedCardIds.Add(instanceId);
    }

    public void Discard(CardInstance card)
    {
        discardedCardIds.Add(card.InstanceId);
    }

    public void ResetForNewConversation()
    {
        drawnCardIds.Clear();
        // Keep discarded cards discarded (single-use cards)
    }

    public List<CardInstance> GetAllCards()
    {
        return allCards.ToList();
    }

    public void Shuffle()
    {
        // Shuffle is handled by randomized drawing
    }

    public List<CardInstance> DrawFilteredByProperties(List<CardProperty> requiredProperties, int count)
    {
        List<CardInstance> available = new List<CardInstance>();
        foreach (CardInstance c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) &&
                !discardedCardIds.Contains(c.InstanceId))
            {
                // Check if card has all required properties
                bool hasAllProperties = true;
                foreach (CardProperty prop in requiredProperties)
                {
                    if (!c.Properties.Contains(prop))
                    {
                        hasAllProperties = false;
                        break;
                    }
                }
                if (hasAllProperties)
                {
                    available.Add(c);
                }
            }
        }

        List<CardInstance> drawn = new List<CardInstance>();
        Random random = new Random();
        for (int i = 0; i < Math.Min(count, available.Count); i++)
        {
            CardInstance card = available[random.Next(available.Count)];
            drawnCardIds.Add(card.InstanceId);
            drawn.Add(card);
            available.Remove(card); // Prevent drawing same card twice
        }
        return drawn;
    }
    
    // Helper methods for common property-based filtering
    public List<CardInstance> DrawGoalCards(int count)
    {
        // Goal cards have both Fleeting AND Opportunity properties
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Fleeting, CardProperty.Opportunity }, count);
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

    public int RemainingCards
    {
        get
        {
            int count = 0;
            foreach (CardInstance c in allCards)
            {
                if (!drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId))
                    count++;
            }
            return count;
        }
    }

    public bool HasCardsAvailable()
    {
        return RemainingCards > 0;
    }

    public void Clear()
    {
        allCards.Clear();
        drawnCardIds.Clear();
        discardedCardIds.Clear();
    }
}
