using System;
using System.Collections.Generic;
using System.Linq;

public class NPCDeckFactory
{
    private readonly Random _random = new Random();
    
    public CardDeck CreateDeckForNPC(NPC npc)
    {
        var deck = new CardDeck
        {
            NPCPersonality = npc.Personality,
            NPCId = npc.ID
        };
        
        // Generate cards based on NPC personality
        deck.AvailableCards.AddRange(GenerateCardsForPersonality(npc.Personality));
        
        // Add crisis cards if NPC has urgent obligations
        if (HasUrgentNeed(npc))
        {
            deck.AvailableCards.Add(CreateCrisisCard(npc));
        }
        
        return deck;
    }
    
    private List<ConversationCard> GenerateCardsForPersonality(string personality)
    {
        var cards = new List<ConversationCard>();
        
        // Base cards every NPC has
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "How can I help you?",
            Type = CardType.Practical,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 2
        });
        
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Tell me more about that.",
            Type = CardType.Social,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 3
        });
        
        // Personality-specific cards
        switch (personality?.ToLower())
        {
            case "merchant":
                cards.AddRange(CreateMerchantCards());
                break;
            case "noble":
                cards.AddRange(CreateNobleCards());
                break;
            case "guard":
                cards.AddRange(CreateGuardCards());
                break;
            default:
                cards.AddRange(CreateDefaultCards());
                break;
        }
        
        // Add state change cards
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Let me think about this carefully...",
            Type = CardType.Social,
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 1,
            IsStateCard = true,
            SuccessState = EmotionalState.OPEN,
            FailureState = EmotionalState.GUARDED
        });
        
        return cards;
    }
    
    private List<ConversationCard> CreateMerchantCards()
    {
        return new List<ConversationCard>
        {
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Business has been good lately.",
                Type = CardType.Practical,
                Persistence = PersistenceType.Opportunity,
                Weight = 1,
                BaseComfort = 2
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "I might have a delivery job for you.",
                Type = CardType.Practical,
                Persistence = PersistenceType.OneShot,
                Weight = 2,
                BaseComfort = 4,
                CanDeliverLetter = true
            }
        };
    }
    
    private List<ConversationCard> CreateNobleCards()
    {
        return new List<ConversationCard>
        {
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "The political situation is quite delicate.",
                Type = CardType.Social,
                Persistence = PersistenceType.Persistent,
                Weight = 2,
                BaseComfort = 3
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "I need someone I can trust with this.",
                Type = CardType.Intimacy,
                Persistence = PersistenceType.OneShot,
                Weight = 3,
                BaseComfort = 5,
                CanDeliverLetter = true
            }
        };
    }
    
    private List<ConversationCard> CreateGuardCards()
    {
        return new List<ConversationCard>
        {
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Stay out of trouble.",
                Type = CardType.Practical,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 1
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "I've seen some strange things lately.",
                Type = CardType.Social,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 3
            }
        };
    }
    
    private List<ConversationCard> CreateDefaultCards()
    {
        return new List<ConversationCard>
        {
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "It's good to see you.",
                Type = CardType.Comfort,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 2
            }
        };
    }
    
    private ConversationCard CreateCrisisCard(NPC npc)
    {
        return new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Please, I desperately need your help!",
            Type = CardType.Comfort,
            Persistence = PersistenceType.Crisis,
            Weight = 3,
            BaseComfort = 10,
            IsCrisis = true,
            CanDeliverLetter = true
        };
    }
    
    private bool HasUrgentNeed(NPC npc)
    {
        // This would check if the NPC has urgent obligations
        // For now, return false
        return false;
    }
}