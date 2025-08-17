using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Represents an NPC's conversation deck - their unique set of possible interactions
/// Decks evolve through letter delivery and conversation outcomes
/// </summary>
public class NPCDeck
{
    public string NpcId { get; set; }
    public List<ConversationCard> Cards { get; set; } = new List<ConversationCard>();
    
    // Maximum deck size to prevent bloat
    public const int MaxDeckSize = 20;
    
    public NPCDeck(string npcId, PersonalityType personalityType = PersonalityType.STEADFAST)
    {
        NpcId = npcId;
        InitializeStartingDeck(personalityType);
    }
    
    /// <summary>
    /// Initialize with universal starting cards + personality-specific cards
    /// </summary>
    private void InitializeStartingDeck(PersonalityType personalityType)
    {
        // Universal starting cards (from docs)
        Cards.AddRange(new[]
        {
            new ConversationCard
            {
                Id = "small_talk",
                Name = "Small Talk",
                Description = "Simple social courtesy",
                Difficulty = 2,
                PatienceCost = 1,
                ComfortGain = 2,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "listen",
                Name = "Listen",
                Description = "Give them your full attention",
                Difficulty = 3,
                PatienceCost = 1,
                ComfortGain = 3,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "how_are_things",
                Name = "How Are Things?",
                Description = "Check on their current situation",
                Difficulty = 2,
                PatienceCost = 1,
                ComfortGain = 1,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "offer_help",
                Name = "Offer Help",
                Description = "Show willingness to assist",
                Difficulty = 4,
                PatienceCost = 2,
                ComfortGain = 4,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "quick_exit",
                Name = "Quick Exit",
                Description = "Politely end the conversation",
                Difficulty = 0,
                PatienceCost = 0,
                ComfortGain = 0,
                Category = RelationshipCardCategory.Basic
            },
            // Negative cards (deck pollution)
            new ConversationCard
            {
                Id = "awkward_silence",
                Name = "Awkward Silence",
                Description = "Uncomfortable pause in conversation",
                Difficulty = 8,
                PatienceCost = 0,
                ComfortGain = -2,
                Category = RelationshipCardCategory.Basic
            },
            new ConversationCard
            {
                Id = "bad_memory",
                Name = "Bad Memory",
                Description = "Brings up an unfortunate past event",
                Difficulty = 6,
                PatienceCost = 1,
                ComfortGain = -1,
                Category = RelationshipCardCategory.Basic
            },
            // Crisis cards - only available during DESPERATE/ANXIOUS states
            new ConversationCard
            {
                Id = "promise_personal_help",
                Name = "Promise Personal Help", 
                Description = "Swear to prioritize their needs above all others",
                Difficulty = 3,
                PatienceCost = 3,
                ComfortGain = 6,
                Category = RelationshipCardCategory.Crisis
            },
            new ConversationCard
            {
                Id = "emergency_arrangement",
                Name = "Emergency Arrangement",
                Description = "Offer immediate assistance for their urgent situation",
                Difficulty = 4,
                PatienceCost = 2,
                ComfortGain = 5,
                Category = RelationshipCardCategory.Crisis
            }
        });

        // Add personality-specific cards based on NPC type
        Cards.AddRange(GetPersonalityCards(personalityType));
    }

    /// <summary>
    /// Create personality-specific conversation cards that reflect authentic human interaction patterns.
    /// These cards emerge from the NPC's natural way of communicating, not mechanical abilities.
    /// </summary>
    private List<ConversationCard> GetPersonalityCards(PersonalityType personalityType)
    {
        return personalityType switch
        {
            PersonalityType.DEVOTED => new List<ConversationCard>
            {
                new ConversationCard
                {
                    Id = "offer_comfort",
                    Name = "Offer Comfort",
                    Description = "Show genuine care for their emotional wellbeing",
                    Difficulty = 3,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "listen_deeply",
                    Name = "Listen Deeply",
                    Description = "Give your complete, patient attention to their concerns",
                    Difficulty = 4,
                    PatienceCost = 2,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "share_personal_story",
                    Name = "Share Personal Story",
                    Description = "Open up about your own experiences to build connection",
                    Difficulty = 5,
                    PatienceCost = 3,
                    ComfortGain = 5,
                    Category = RelationshipCardCategory.Basic
                }
            },

            PersonalityType.MERCANTILE => new List<ConversationCard>
            {
                new ConversationCard
                {
                    Id = "discuss_business",
                    Name = "Discuss Business",
                    Description = "Talk about practical matters and mutual opportunities",
                    Difficulty = 3,
                    PatienceCost = 1,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "negotiate_terms",
                    Name = "Negotiate Terms",
                    Description = "Work out the details of a mutually beneficial arrangement",
                    Difficulty = 4,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "assess_worth",
                    Name = "Assess Worth",
                    Description = "Evaluate the value of goods, services, or opportunities",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Basic
                }
            },

            PersonalityType.PROUD => new List<ConversationCard>
            {
                new ConversationCard
                {
                    Id = "show_respect",
                    Name = "Show Respect",
                    Description = "Acknowledge their position and importance with proper deference",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "acknowledge_status",
                    Name = "Acknowledge Status",
                    Description = "Recognize their social standing and the authority it brings",
                    Difficulty = 3,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "formal_address",
                    Name = "Formal Address",
                    Description = "Use proper titles and courteous language befitting their rank",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Basic
                }
            },

            PersonalityType.CUNNING => new List<ConversationCard>
            {
                new ConversationCard
                {
                    Id = "read_between_lines",
                    Name = "Read Between Lines",
                    Description = "Pick up on subtle hints and unspoken meanings",
                    Difficulty = 5,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "share_information",
                    Name = "Share Information",
                    Description = "Offer a useful piece of knowledge or gossip",
                    Difficulty = 4,
                    PatienceCost = 2,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "test_loyalty",
                    Name = "Test Loyalty",
                    Description = "Carefully probe to see where their true allegiances lie",
                    Difficulty = 6,
                    PatienceCost = 3,
                    ComfortGain = 5,
                    Category = RelationshipCardCategory.Basic
                }
            },

            PersonalityType.STEADFAST => new List<ConversationCard>
            {
                new ConversationCard
                {
                    Id = "show_honor",
                    Name = "Show Honor",
                    Description = "Demonstrate your commitment to doing what's right",
                    Difficulty = 3,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "speak_plainly",
                    Name = "Speak Plainly",
                    Description = "Be direct and honest without fancy words or deception",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic
                },
                new ConversationCard
                {
                    Id = "respect_duty",
                    Name = "Respect Duty",
                    Description = "Acknowledge the importance of their responsibilities",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Basic
                }
            },

            _ => new List<ConversationCard>() // Fallback to no personality cards
        };
    }
    
    /// <summary>
    /// Draw 5 cards from deck for this conversation round
    /// </summary>
    public List<ConversationCard> DrawCards(Dictionary<ConnectionType, int> currentTokens, int currentComfort = 0, NPCEmotionalState emotionalState = NPCEmotionalState.CALCULATING)
    {
        var availableCards = Cards
            .Where(card => card.CanPlay(currentTokens, currentComfort) && card.IsAvailableInState(emotionalState))
            .ToList();
        
        // Shuffle available cards
        var random = new Random();
        availableCards = availableCards.OrderBy(x => random.Next()).ToList();
        
        // Draw up to 5 cards, always include Quick Exit if available
        var drawnCards = new List<ConversationCard>();
        var exitCard = availableCards.FirstOrDefault(c => c.Id == "quick_exit");
        if (exitCard != null)
        {
            drawnCards.Add(exitCard);
            availableCards.Remove(exitCard);
        }
        
        // Fill remaining slots with other cards
        drawnCards.AddRange(availableCards.Take(4));
        
        return drawnCards;
    }
    
    /// <summary>
    /// Add a card to the deck (from letter delivery rewards)
    /// </summary>
    public void AddCard(ConversationCard card)
    {
        if (Cards.Count >= MaxDeckSize) return;
        
        // Don't add duplicates
        if (Cards.Any(c => c.Id == card.Id)) return;
        
        Cards.Add(card);
    }
    
    /// <summary>
    /// Remove a card from the deck (clearing negatives)
    /// </summary>
    public void RemoveCard(string cardId)
    {
        Cards.RemoveAll(c => c.Id == cardId);
    }
    
    /// <summary>
    /// Calculate starting patience for conversations
    /// Negative cards reduce starting patience by 0.5 each
    /// </summary>
    public double GetPatiencePenalty()
    {
        var negativeCards = Cards.Where(c => c.ComfortGain < 0).Count();
        return negativeCards * 0.5;
    }
}