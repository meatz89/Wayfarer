using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages observation tracking and card generation for the player
/// Each observation can only be taken once per time block and generates a conversation card
/// </summary>
public class ObservationManager
{
    private readonly TimeManager _timeManager;
    private readonly Dictionary<string, HashSet<string>> _takenObservationsByTimeBlock;
    private readonly List<ConversationCard> _currentObservationCards;

    public ObservationManager(TimeManager timeManager)
    {
        _timeManager = timeManager;
        _takenObservationsByTimeBlock = new Dictionary<string, HashSet<string>>();
        _currentObservationCards = new List<ConversationCard>();
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
    /// Take an observation and generate a conversation card
    /// Returns the generated card on success, null if already taken or invalid
    /// </summary>
    public ConversationCard TakeObservation(Observation observation, TokenMechanicsManager tokenManager)
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

        // Generate conversation card
        var observationCard = GenerateObservationCard(observation, tokenManager);
        
        if (observationCard != null)
        {
            _currentObservationCards.Add(observationCard);
            Console.WriteLine($"[ObservationManager] Generated observation card {observationCard.Id} from {observation.Id}");
        }

        return observationCard;
    }

    /// <summary>
    /// Get all current observation cards in player's hand
    /// </summary>
    public List<ConversationCard> GetObservationCards()
    {
        return _currentObservationCards.ToList();
    }

    /// <summary>
    /// Remove an observation card after it's been played (OneShot behavior)
    /// </summary>
    public void RemoveObservationCard(ConversationCard card)
    {
        if (card != null && card.IsObservation)
        {
            _currentObservationCards.Remove(card);
            Console.WriteLine($"[ObservationManager] Removed played observation card {card.Id}");
        }
    }

    /// <summary>
    /// Clear observation cards for new time block
    /// Observations refresh each time block
    /// </summary>
    public void RefreshForNewTimeBlock()
    {
        var currentTimeBlock = GetCurrentTimeBlockKey();
        Console.WriteLine($"[ObservationManager] Refreshing observations for time block {currentTimeBlock}");
        
        // Clear taken observations for this time block
        if (_takenObservationsByTimeBlock.ContainsKey(currentTimeBlock))
        {
            _takenObservationsByTimeBlock[currentTimeBlock].Clear();
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
    private ConversationCard GenerateObservationCard(Observation observation, TokenMechanicsManager tokenManager)
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
            Persistence = PersistenceType.OneShot, // All observation cards are one-shot
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