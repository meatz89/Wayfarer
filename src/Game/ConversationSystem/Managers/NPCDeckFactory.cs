using System;
using System.Collections.Generic;
using System.Linq;

public class NPCDeckFactory
{
    private readonly Random _random = new Random();
    
    public CardDeck CreateDeckForNPC(NPC npc)
    {
        var deck = new CardDeck();
        
        // Generate cards based on NPC personality
        var cards = GenerateCardsForPersonality(npc.PersonalityType);
        foreach (var card in cards)
        {
            deck.AddCard(card);
        }
        
        // Add crisis cards if NPC has urgent obligations
        if (HasUrgentNeed(npc))
        {
            deck.AddCard(CreateCrisisCard(npc));
        }
        
        // Add obligation manipulation cards
        var obligationCards = CreateObligationManipulationCards();
        foreach (var card in obligationCards)
        {
            deck.AddCard(card);
        }
        
        return deck;
    }
    
    private List<ConversationCard> GenerateCardsForPersonality(PersonalityType personality)
    {
        var cards = new List<ConversationCard>();
        
        // Base cards every NPC has
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "How can I help you?",
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 2
        });
        
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Tell me more about that.",
            Type = CardType.Status,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 3
        });
        
        // Personality-specific cards
        switch (personality)
        {
            case PersonalityType.MERCANTILE:
                cards.AddRange(CreateMerchantCards());
                break;
            case PersonalityType.PROUD:
                cards.AddRange(CreateNobleCards());
                break;
            case PersonalityType.DEVOTED:
                cards.AddRange(CreateGuardCards());
                break;
            case PersonalityType.CUNNING:
                cards.AddRange(CreateCunningCards());
                break;
            case PersonalityType.STEADFAST:
            default:
                cards.AddRange(CreateDefaultCards());
                break;
        }
        
        // Add state change cards
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Let me think about this carefully...",
            Type = CardType.Status,
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
                Type = CardType.Commerce,
                Persistence = PersistenceType.Opportunity,
                Weight = 1,
                BaseComfort = 2
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "I might have a delivery job for you.",
                Type = CardType.Commerce,
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
                Type = CardType.Status,
                Persistence = PersistenceType.Persistent,
                Weight = 2,
                BaseComfort = 3
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "I need someone I can trust with this.",
                Type = CardType.Trust,
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
                Type = CardType.Commerce,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 1
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "I've seen some strange things lately.",
                Type = CardType.Status,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 3
            }
        };
    }
    
    private List<ConversationCard> CreateCunningCards()
    {
        return new List<ConversationCard>
        {
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "I know something you might find interesting...",
                Type = CardType.Shadow,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 4
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Let's keep this between us.",
                Type = CardType.Shadow,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
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
                Type = CardType.Trust,
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
            Type = CardType.Trust,
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
    
    private List<ConversationCard> CreateObligationManipulationCards()
    {
        var cards = new List<ConversationCard>();
        
        // Card to negotiate obligations
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "About that letter you gave me...",
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 1,
            ManipulatesObligations = true
        });
        
        // Card to deliver letter personally
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Text = "I have your letter right here.",
            Type = CardType.Trust,
            Persistence = PersistenceType.OneShot,
            Weight = 1,
            BaseComfort = 5,
            CanDeliverLetter = true
        });
        
        return cards;
    }
}