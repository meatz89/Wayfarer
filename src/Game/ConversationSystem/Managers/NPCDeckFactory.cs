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
            Template = CardTemplateType.OfferHelp,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 2,
            Category = CardCategory.COMFORT
        });
        
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Template = CardTemplateType.ActiveListening,
            Context = new CardContext { Personality = personality },
            Type = CardType.Status,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 3,
            Category = CardCategory.COMFORT
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
            Template = CardTemplateType.OpeningUp,
            Context = new CardContext { Personality = personality },
            Type = CardType.Status,
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 1,
            Category = CardCategory.STATE,
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
                Template = CardTemplateType.DiscussBusiness,
                Context = new CardContext { Personality = PersonalityType.MERCANTILE },
                Type = CardType.Commerce,
                Persistence = PersistenceType.Opportunity,
                Weight = 1,
                BaseComfort = 2,
                Category = CardCategory.COMFORT
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Template = CardTemplateType.OfferWork,
                Context = new CardContext { Personality = PersonalityType.MERCANTILE },
                Type = CardType.Commerce,
                Persistence = PersistenceType.OneShot,
                Weight = 2,
                BaseComfort = 4,
                Category = CardCategory.COMFORT,
                CanDeliverLetter = true
            },
            // Add STATE card for testing
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Template = CardTemplateType.ShowingTension,
                Context = new CardContext { Personality = PersonalityType.MERCANTILE },
                Type = CardType.Commerce,
                Persistence = PersistenceType.OneShot,
                Weight = 2,
                BaseComfort = 0,
                Category = CardCategory.STATE,
                SuccessState = EmotionalState.TENSE,
                FailureState = EmotionalState.GUARDED
            },
            // Add CRISIS card for testing
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Template = CardTemplateType.DesperateRequest,
                Context = new CardContext { 
                    Personality = PersonalityType.MERCANTILE,
                    EmotionalState = EmotionalState.DESPERATE
                },
                Type = CardType.Trust,
                Persistence = PersistenceType.Crisis,
                Weight = 3,
                BaseComfort = 8,
                Category = CardCategory.CRISIS
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
                Template = CardTemplateType.DiscussPolitics,
                Context = new CardContext { Personality = PersonalityType.PROUD },
                Type = CardType.Status,
                Persistence = PersistenceType.Persistent,
                Weight = 2,
                BaseComfort = 3,
                Category = CardCategory.COMFORT
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Template = CardTemplateType.MakePromise,
                Context = new CardContext { Personality = PersonalityType.PROUD },
                Type = CardType.Trust,
                Persistence = PersistenceType.OneShot,
                Weight = 3,
                BaseComfort = 5,
                Category = CardCategory.COMFORT,
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
                Template = CardTemplateType.ShowRespect,
                Context = new CardContext { Personality = PersonalityType.DEVOTED },
                Type = CardType.Commerce,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 1,
                Category = CardCategory.COMFORT
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Template = CardTemplateType.ShareInformation,
                Context = new CardContext { Personality = PersonalityType.DEVOTED },
                Type = CardType.Status,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 3,
                Category = CardCategory.COMFORT
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
                Template = CardTemplateType.ImplyKnowledge,
                Context = new CardContext { Personality = PersonalityType.CUNNING },
                Type = CardType.Shadow,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 4,
                Category = CardCategory.COMFORT
            },
            new ConversationCard
            {
                Id = Guid.NewGuid().ToString(),
                Template = CardTemplateType.RequestDiscretion,
                Context = new CardContext { Personality = PersonalityType.CUNNING },
                Type = CardType.Shadow,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 3,
                Category = CardCategory.COMFORT
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
                Template = CardTemplateType.SimpleGreeting,
                Context = new CardContext { Personality = PersonalityType.STEADFAST },
                Type = CardType.Trust,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 2,
                Category = CardCategory.COMFORT
            }
        };
    }
    
    private ConversationCard CreateCrisisCard(NPC npc)
    {
        return new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Template = CardTemplateType.DesperateRequest,
            Context = new CardContext 
            { 
                Personality = npc.PersonalityType,
                EmotionalState = EmotionalState.DESPERATE,
                UrgencyLevel = 3,
                HasDeadline = true
            },
            Type = CardType.Trust,
            Persistence = PersistenceType.Crisis,
            Weight = 3,
            BaseComfort = 10,
            Category = CardCategory.CRISIS,
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
            Template = CardTemplateType.DiscussObligation,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 1,
            Category = CardCategory.COMFORT,
            ManipulatesObligations = true
        });
        
        // Card to deliver letter personally
        cards.Add(new ConversationCard
        {
            Id = Guid.NewGuid().ToString(),
            Template = CardTemplateType.DeliverLetter,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.OneShot,
            Weight = 1,
            BaseComfort = 5,
            Category = CardCategory.COMFORT,
            CanDeliverLetter = true
        });
        
        return cards;
    }
}