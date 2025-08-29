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
    private readonly GameWorld _gameWorld;
    private readonly Dictionary<string, HashSet<string>> _takenObservationsByTimeBlock;
    private readonly List<ObservationCard> _currentObservationCards;
    private readonly Dictionary<string, TakenObservation> _takenObservations;

    public ObservationManager(TimeManager timeManager, GameWorld gameWorld)
    {
        _timeManager = timeManager;
        _gameWorld = gameWorld;
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
            var observationCard = ObservationCard.FromConversationCard(conversationCard);
            // Note: SourceObservationId is set in the FromConversationCard method using the card.Id
            
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
                TemplateId = originalCard.TemplateId,
                Mechanics = originalCard.Mechanics,
                Category = originalCard.Category,
                Context = updatedContext,
                Type = originalCard.Type,
                Persistence = PersistenceType.Fleeting, // Observations are Opportunity type but DON'T vanish on Listen - they decay over time
                Weight = originalCard.Weight,
                BaseComfort = obsCard.EffectiveComfortValue, // Use decay-adjusted comfort value
                IsObservation = true,
                ObservationSource = obsCard.SourceObservationId,
                CanDeliverLetter = originalCard.CanDeliverLetter,
                ManipulatesObligations = originalCard.ManipulatesObligations,
                SuccessState = originalCard.SuccessState,
                FailureState = originalCard.FailureState,
                SuccessRate = originalCard.SuccessRate,
                DisplayName = originalCard.DisplayName,
                Description = originalCard.Description,
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
    /// Generate a conversation card from an observation using JSON-based card templates
    /// </summary>
    private ConversationCard GenerateConversationCard(Observation observation, TokenMechanicsManager tokenManager)
    {
        if (string.IsNullOrEmpty(observation.CardTemplate))
        {
            Console.WriteLine($"[ObservationManager] No card template specified for observation {observation.Id}");
            return null;
        }

        // Load the base card template from GameWorld's AllCardDefinitions
        if (!_gameWorld.AllCardDefinitions.TryGetValue(observation.CardTemplate, out var baseCard))
        {
            Console.WriteLine($"[ObservationManager] Card template '{observation.CardTemplate}' not found in AllCardDefinitions for observation {observation.Id}");
            return null;
        }

        // Create a new card instance based on the template, customized for this observation
        var observationCard = new ConversationCard
        {
            Id = $"{observation.Id}_card_{Guid.NewGuid()}",
            TemplateId = baseCard.TemplateId,
            Mechanics = baseCard.Mechanics,
            Category = baseCard.Category,
            Type = baseCard.Type,
            Persistence = PersistenceType.Fleeting, // Observation cards are fleeting
            Weight = baseCard.Weight,
            BaseComfort = baseCard.BaseComfort,
            IsObservation = true,
            ObservationSource = observation.Id,
            CanDeliverLetter = baseCard.CanDeliverLetter,
            ManipulatesObligations = baseCard.ManipulatesObligations,
            SuccessState = baseCard.SuccessState,
            FailureState = baseCard.FailureState,
            SuccessRate = baseCard.SuccessRate,
            DisplayName = baseCard.DisplayName ?? observation.Text,
            Description = baseCard.Description ?? observation.Description,
            Context = new CardContext
            {
                Personality = PersonalityType.STEADFAST,
                EmotionalState = EmotionalState.NEUTRAL,
                UrgencyLevel = 0,
                HasDeadline = false,
                ObservationType = observation.Type,
                ObservationId = observation.Id,
                ObservationText = observation.Text,
                ObservationDescription = observation.Description
            }
        };

        Console.WriteLine($"[ObservationManager] Generated observation card {observationCard.Id} from template {observation.CardTemplate} for observation {observation.Id}");
        return observationCard;
    }

    // REMOVED: DetermineCardType - hardcoded type assignment violates architecture
    // Card types should come from observation JSON properties, not switch statements
    
    // REMOVED: DetermineCardTemplate - hardcoded template assignment violates architecture  
    // Card templates should be specified in observation JSON, not defaulted in code
    
    // REMOVED: GetObservationTypeData - hardcoded values violate architecture
    // All card properties (weight, comfort) should come from JSON templates

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