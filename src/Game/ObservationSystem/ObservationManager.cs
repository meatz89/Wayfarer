using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks an observation that has been taken with its details for display
/// </summary>
public class TakenObservation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string NarrativeText { get; set; }
    public ObservationCard GeneratedCard { get; set; }
    public DateTime TimeTaken { get; set; }
    public TimeBlocks TimeBlockTaken { get; set; }
}

/// <summary>
/// Manages observation tracking and card generation for the player
/// Each observation can only be taken once per time block and generates observation cards with decay
/// </summary>
public class ObservationManager
{
    private readonly TimeManager _timeManager;
    private readonly Dictionary<string, HashSet<string>> _takenObservationsByTimeBlock;
    private readonly List<ObservationCard> _currentObservationCards;
    private readonly Dictionary<string, TakenObservation> _takenObservations;

    public ObservationManager(TimeManager timeManager)
    {
        _timeManager = timeManager;
        _takenObservationsByTimeBlock = new Dictionary<string, HashSet<string>>();
        _currentObservationCards = new List<ObservationCard>();
        _takenObservations = new Dictionary<string, TakenObservation>();
    }

    /// <summary>
    /// Check if an observation has already been taken this time block
    /// </summary>
    public bool HasTakenObservation(string observationId)
    {
        var currentTimeBlock = GetCurrentTimeBlockKey();
        return _takenObservationsByTimeBlock.ContainsKey(currentTimeBlock) &&
               _takenObservationsByTimeBlock[currentTimeBlock].Contains(observationId);
    }

    /// <summary>
    /// Take an observation and generate an observation card with decay tracking
    /// Returns the generated observation card on success, null if already taken or invalid
    /// </summary>
    public ObservationCard TakeObservation(Observation observation, TokenMechanicsManager tokenManager)
    {
        if (observation == null)
            return null;

        var currentTimeBlock = GetCurrentTimeBlockKey();
        
        // Check if already taken this time block
        if (HasTakenObservation(observation.Id))
        {
            Console.WriteLine($"[ObservationManager] Observation {observation.Id} already taken this time block");
            return null;
        }

        // Mark as taken for this time block
        if (!_takenObservationsByTimeBlock.ContainsKey(currentTimeBlock))
        {
            _takenObservationsByTimeBlock[currentTimeBlock] = new HashSet<string>();
        }
        _takenObservationsByTimeBlock[currentTimeBlock].Add(observation.Id);

        // Generate conversation card first
        var conversationCard = GenerateConversationCard(observation, tokenManager);
        
        if (conversationCard != null)
        {
            // Create observation card with decay tracking
            var currentGameTime = GetCurrentGameTime();
            var observationCard = ObservationCard.FromConversationCard(conversationCard, observation.Id, currentGameTime);
            
            _currentObservationCards.Add(observationCard);
            Console.WriteLine($"[ObservationManager] Generated observation card {observationCard.Id} from {observation.Id} at {currentGameTime}");
            
            // Store taken observation details for UI display
            var takenObs = new TakenObservation
            {
                Id = observation.Id,
                Name = observation.Text,
                Description = observation.Description,
                NarrativeText = observation.Description,  // Use description as narrative
                GeneratedCard = observationCard,
                TimeTaken = currentGameTime,
                TimeBlockTaken = _timeManager.GetCurrentTimeBlock()
            };
            _takenObservations[observation.Id] = takenObs;
            
            return observationCard;
        }

        return null;
    }

    /// <summary>
    /// Get all current observation cards in player's hand
    /// Updates decay states and filters out expired cards
    /// </summary>
    public List<ObservationCard> GetObservationCards()
    {
        var currentGameTime = GetCurrentGameTime();
        
        // Update decay states for all observation cards
        foreach (var card in _currentObservationCards)
        {
            card.UpdateDecayState(currentGameTime);
        }
        
        // Remove expired cards automatically
        var expiredCards = _currentObservationCards.Where(c => c.DecayState == ObservationDecayState.Expired).ToList();
        foreach (var expired in expiredCards)
        {
            _currentObservationCards.Remove(expired);
            Console.WriteLine($"[ObservationManager] Automatically discarded expired observation card {expired.Id}");
        }
        
        return _currentObservationCards.ToList();
    }
    
    /// <summary>
    /// Get observation cards as conversation cards for use in conversation system
    /// Only returns playable (non-expired) cards with adjusted comfort values
    /// </summary>
    public List<ConversationCard> GetObservationCardsAsConversationCards()
    {
        var currentGameTime = GetCurrentGameTime();
        var observationCards = GetObservationCards(); // This updates decay states
        var conversationCards = new List<ConversationCard>();
        
        foreach (var obsCard in observationCards.Where(c => c.IsPlayable))
        {
            // Create a modified conversation card with adjusted comfort value for stale cards
            var originalCard = obsCard.ConversationCard;
            
            // Create updated context with decay information
            var updatedContext = new CardContext
            {
                Personality = originalCard.Context?.Personality ?? PersonalityType.STEADFAST,
                EmotionalState = originalCard.Context?.EmotionalState ?? EmotionalState.NEUTRAL,
                UrgencyLevel = originalCard.Context?.UrgencyLevel ?? 0,
                HasDeadline = originalCard.Context?.HasDeadline ?? false,
                MinutesUntilDeadline = originalCard.Context?.MinutesUntilDeadline,
                ObservationType = originalCard.Context?.ObservationType,
                LetterId = originalCard.Context?.LetterId,
                TargetNpcId = originalCard.Context?.TargetNpcId,
                NPCName = originalCard.Context?.NPCName,
                NPCPersonality = originalCard.Context?.NPCPersonality ?? PersonalityType.STEADFAST,
                ExchangeData = originalCard.Context?.ExchangeData,
                ObservationId = originalCard.Context?.ObservationId,
                ObservationText = originalCard.Context?.ObservationText,
                ObservationDescription = originalCard.Context?.ObservationDescription,
                ExchangeName = originalCard.Context?.ExchangeName,
                ExchangeCost = originalCard.Context?.ExchangeCost,
                ExchangeReward = originalCard.Context?.ExchangeReward,
                // Add decay state information
                ObservationDecayState = obsCard.DecayState,
                ObservationDecayDescription = obsCard.GetDecayStateDescription(currentGameTime)
            };
            
            var adjustedCard = new ConversationCard
            {
                Id = originalCard.Id,
                Template = originalCard.Template,
                Context = updatedContext,
                Type = originalCard.Type,
                Persistence = PersistenceType.Fleeting, // Observations are Opportunity type but DON'T vanish on Listen - they decay over time
                Weight = originalCard.Weight,
                BaseComfort = obsCard.EffectiveComfortValue, // Use decay-adjusted comfort value
                Category = originalCard.Category,
                IsObservation = true,
                ObservationSource = obsCard.SourceObservationId,
                CanDeliverLetter = originalCard.CanDeliverLetter,
                ManipulatesObligations = originalCard.ManipulatesObligations,
                Depth = originalCard.Depth,
                SuccessState = originalCard.SuccessState,
                FailureState = originalCard.FailureState,
                SuccessRate = originalCard.SuccessRate,
                DisplayName = originalCard.DisplayName,
                Description = originalCard.Description,
                PowerLevel = originalCard.PowerLevel
            };
            
            conversationCards.Add(adjustedCard);
        }
        
        return conversationCards;
    }

    /// <summary>
    /// Remove an observation card after it's been played or has expired
    /// </summary>
    public void RemoveObservationCard(string observationCardId)
    {
        var card = _currentObservationCards.FirstOrDefault(c => c.Id == observationCardId);
        if (card != null)
        {
            _currentObservationCards.Remove(card);
            Console.WriteLine($"[ObservationManager] Removed observation card {card.Id}");
        }
    }
    
    /// <summary>
    /// Remove observation card by conversation card ID (for backwards compatibility)
    /// </summary>
    public void RemoveObservationCardByConversationId(string conversationCardId)
    {
        var card = _currentObservationCards.FirstOrDefault(c => c.ConversationCard.Id == conversationCardId);
        if (card != null)
        {
            _currentObservationCards.Remove(card);
            Console.WriteLine($"[ObservationManager] Removed observation card {card.Id} by conversation ID {conversationCardId}");
        }
    }

    /// <summary>
    /// Get all taken observations for the current time block
    /// </summary>
    public List<TakenObservation> GetTakenObservations()
    {
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        return _takenObservations.Values
            .Where(o => o.TimeBlockTaken == currentTimeBlock)
            .ToList();
    }

    /// <summary>
    /// Clear observation cards for new time block
    /// Observations refresh each time block
    /// </summary>
    public void RefreshForNewTimeBlock()
    {
        var currentTimeBlock = GetCurrentTimeBlockKey();
        var currentTimeBlockEnum = _timeManager.GetCurrentTimeBlock();
        Console.WriteLine($"[ObservationManager] Refreshing observations for time block {currentTimeBlock}");
        
        // Clear taken observations for this time block
        if (_takenObservationsByTimeBlock.ContainsKey(currentTimeBlock))
        {
            _takenObservationsByTimeBlock[currentTimeBlock].Clear();
        }
        
        // Clear taken observations from previous time blocks
        var toRemove = _takenObservations.Where(kvp => kvp.Value.TimeBlockTaken != currentTimeBlockEnum).Select(kvp => kvp.Key).ToList();
        foreach (var id in toRemove)
        {
            _takenObservations.Remove(id);
        }
    }

    /// <summary>
    /// Clear all observations for a new day
    /// </summary>
    public void StartNewDay()
    {
        Console.WriteLine("[ObservationManager] Starting new day - clearing all observations");
        _takenObservationsByTimeBlock.Clear();
        _currentObservationCards.Clear();
    }

    /// <summary>
    /// Generate a conversation card from an observation
    /// </summary>
    private ConversationCard GenerateConversationCard(Observation observation, TokenMechanicsManager tokenManager)
    {
        // Determine card type based on observation type and content
        var cardType = DetermineCardType(observation);
        var observationTypeData = GetObservationTypeData(observation.Type);
        
        // Create card context
        var context = new CardContext
        {
            ObservationId = observation.Id,
            ObservationText = observation.Text,
            ObservationDescription = observation.Description
        };

        // Determine template based on observation properties
        var template = DetermineCardTemplate(observation);

        return new ConversationCard
        {
            Id = $"obs_{observation.Id}_{Guid.NewGuid().ToString("N")[..8]}",
            Template = template,
            Context = context,
            Type = cardType,
            Persistence = PersistenceType.Fleeting, // All observation cards are one-shot
            Weight = observationTypeData.Weight,
            BaseComfort = observationTypeData.BaseComfort,
            Category = CardCategory.COMFORT, // Most observations provide comfort
            IsObservation = true,
            ObservationSource = observation.Id,
            CanDeliverLetter = false,
            ManipulatesObligations = false,
            Depth = 0 // Observations can be played at any comfort level
        };
    }

    /// <summary>
    /// Determine the card type based on observation characteristics
    /// </summary>
    private CardType DetermineCardType(Observation observation)
    {
        // Check observation type first
        switch (observation.Type)
        {
            case ObservationType.Shadow:
                return CardType.Shadow;
            case ObservationType.Important:
            case ObservationType.Critical:
                return CardType.Status; // Important information = Status
            case ObservationType.Normal:
            case ObservationType.Useful:
                return CardType.Trust; // Regular observations build trust
            default:
                return CardType.Trust;
        }
    }

    /// <summary>
    /// Determine the card template based on observation properties
    /// </summary>
    private CardTemplateType DetermineCardTemplate(Observation observation)
    {
        // Use cardTemplate from JSON if specified
        if (!string.IsNullOrEmpty(observation.CardTemplate))
        {
            return observation.CardTemplate switch
            {
                "MentionGuards" => CardTemplateType.ShareInformation,
                "DiscussBusiness" => CardTemplateType.DiscussBusiness,
                "ShareSecret" => CardTemplateType.ShareSecret,
                _ => CardTemplateType.ShareInformation
            };
        }

        // Default template based on observation type
        return observation.Type switch
        {
            ObservationType.Shadow => CardTemplateType.ShareSecret,
            ObservationType.Critical => CardTemplateType.ShareUrgentNews,
            ObservationType.Important => CardTemplateType.ShareInformation,
            _ => CardTemplateType.MentionObservation
        };
    }

    /// <summary>
    /// Get observation type configuration data
    /// </summary>
    private ObservationTypeData GetObservationTypeData(ObservationType type)
    {
        // Default values if not found in configuration
        return type switch
        {
            ObservationType.Important => new ObservationTypeData { Weight = 1, BaseComfort = 2 },
            ObservationType.Critical => new ObservationTypeData { Weight = 2, BaseComfort = 3 },
            ObservationType.Shadow => new ObservationTypeData { Weight = 2, BaseComfort = 3 },
            ObservationType.Useful => new ObservationTypeData { Weight = 0, BaseComfort = 0 },
            ObservationType.Normal => new ObservationTypeData { Weight = 1, BaseComfort = 1 },
            _ => new ObservationTypeData { Weight = 1, BaseComfort = 1 }
        };
    }

    /// <summary>
    /// Get current game time for decay calculations
    /// </summary>
    private DateTime GetCurrentGameTime()
    {
        // Convert game time to a DateTime for decay calculations
        // Day 1, Hour 0 = DateTime base, each game day = 24 real hours for decay purposes
        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0); // Arbitrary base date
        var gameDay = _timeManager.GetCurrentDay();
        var gameHour = _timeManager.GetCurrentTimeHours();
        var gameMinutes = _timeManager.GetCurrentMinutes();
        
        return baseDate.AddDays(gameDay - 1).AddHours(gameHour).AddMinutes(gameMinutes);
    }
    
    /// <summary>
    /// Get the current time block as a string key
    /// </summary>
    private string GetCurrentTimeBlockKey()
    {
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        return currentTimeBlock switch
        {
            TimeBlocks.Dawn => "dawn",
            TimeBlocks.Morning => "morning",
            TimeBlocks.Afternoon => "afternoon",
            TimeBlocks.Evening => "evening",
            TimeBlocks.Night => "night",
            TimeBlocks.LateNight => "latenight",
            _ => "unknown"
        };
    }
}