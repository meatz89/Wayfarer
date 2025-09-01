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
    private readonly Dictionary<string, TakenObservation> _takenObservations;

    public ObservationManager(TimeManager timeManager, GameWorld gameWorld)
    {
        _timeManager = timeManager;
        _gameWorld = gameWorld;
        _takenObservationsByTimeBlock = new Dictionary<string, HashSet<string>>();
        _takenObservations = new Dictionary<string, TakenObservation>();
    }

    /// <summary>
    /// Check if an observation has already been taken this time block
    /// </summary>
    public bool HasTakenObservation(string observationId)
    {
        string currentTimeBlock = GetCurrentTimeBlockKey();
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

        string currentTimeBlock = GetCurrentTimeBlockKey();

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
        ConversationCard conversationCard = GenerateConversationCard(observation, tokenManager);

        if (conversationCard != null)
        {
            // Create observation card with decay tracking
            DateTime currentGameTime = GetCurrentGameTime();
            ObservationCard observationCard = ObservationCard.FromConversationCard(conversationCard);
            // Note: SourceObservationId is set in the FromConversationCard method using the card.Id

            // Add to player's observation deck instead of internal storage
            int currentDay = _timeManager.GetCurrentDay();
            TimeBlocks timeBlock = _timeManager.GetCurrentTimeBlock();
            bool addedToDeck = _gameWorld.GetPlayer().ObservationDeck.AddCard(observationCard, currentDay, timeBlock);

            if (addedToDeck)
            {
                Console.WriteLine($"[ObservationManager] Generated observation card {observationCard.Id} from {observation.Id} at {currentGameTime}");

                // Store taken observation details for UI display
                TakenObservation takenObs = new TakenObservation
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
            else
            {
                Console.WriteLine($"[ObservationManager] Failed to add observation card to player deck (deck full?)");
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Get all current observation cards in player's deck
    /// Delegates to player's observation deck for proper tracking
    /// </summary>
    public List<ObservationCard> GetObservationCards()
    {
        int currentDay = _timeManager.GetCurrentDay();
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();

        // Get cards from player's deck with automatic expiration handling
        return _gameWorld.GetPlayer().ObservationDeck.GetActiveCards(currentDay, currentTimeBlock);
    }

    /// <summary>
    /// Get observation cards as conversation cards for use in conversation system
    /// Delegates to player's observation deck
    /// </summary>
    public List<ConversationCard> GetObservationCardsAsConversationCards()
    {
        DateTime currentGameTime = GetCurrentGameTime();
        int currentDay = _timeManager.GetCurrentDay();
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();

        // Get cards from player's deck already formatted as conversation cards
        return _gameWorld.GetPlayer().ObservationDeck.GetAsConversationCards(currentGameTime, currentDay, currentTimeBlock);
    }

    /// <summary>
    /// Remove an observation card after it's been played or has expired
    /// Delegates to player's observation deck
    /// </summary>
    public void RemoveObservationCard(string observationCardId)
    {
        _gameWorld.GetPlayer().ObservationDeck.RemoveCard(observationCardId);
    }

    /// <summary>
    /// Get all taken observations for the current time block
    /// </summary>
    public List<TakenObservation> GetTakenObservations()
    {
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
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
        string currentTimeBlock = GetCurrentTimeBlockKey();
        TimeBlocks currentTimeBlockEnum = _timeManager.GetCurrentTimeBlock();
        Console.WriteLine($"[ObservationManager] Refreshing observations for time block {currentTimeBlock}");

        // Clear taken observations for this time block
        if (_takenObservationsByTimeBlock.ContainsKey(currentTimeBlock))
        {
            _takenObservationsByTimeBlock[currentTimeBlock].Clear();
        }

        // Clear taken observations from previous time blocks
        List<string> toRemove = _takenObservations.Where(kvp => kvp.Value.TimeBlockTaken != currentTimeBlockEnum).Select(kvp => kvp.Key).ToList();
        foreach (string? id in toRemove)
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
        // Note: Player's observation deck handles its own expiration, not cleared on new day
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
        if (!_gameWorld.AllCardDefinitions.TryGetValue(observation.CardTemplate, out ConversationCard? baseCard))
        {
            Console.WriteLine($"[ObservationManager] Card template '{observation.CardTemplate}' not found in AllCardDefinitions for observation {observation.Id}");
            return null;
        }

        // Deep clone the card from the template - no procedural generation
        ConversationCard observationCard = baseCard.DeepClone();
        
        // Only update the ID and observation-specific metadata
        observationCard.Id = $"{observation.Id}_card_{Guid.NewGuid()}";
        observationCard.IsObservation = true;
        observationCard.ObservationSource = observation.Id;
        observationCard.Persistence = PersistenceType.Fleeting; // Observation cards are always fleeting
        
        // Update display information if not already set
        if (string.IsNullOrEmpty(observationCard.DisplayName))
            observationCard.DisplayName = observation.Text;
        if (string.IsNullOrEmpty(observationCard.Description))
            observationCard.Description = observation.Description;

        Console.WriteLine($"[ObservationManager] Cloned observation card {observationCard.Id} from template {observation.CardTemplate} for observation {observation.Id}");
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
        DateTime baseDate = new DateTime(2024, 1, 1, 0, 0, 0); // Arbitrary base date
        int gameDay = _timeManager.GetCurrentDay();
        int gameHour = _timeManager.GetCurrentTimeHours();
        int gameMinutes = _timeManager.GetCurrentMinutes();

        return baseDate.AddDays(gameDay - 1).AddHours(gameHour).AddMinutes(gameMinutes);
    }

    /// <summary>
    /// Get the current time block as a string key
    /// </summary>
    private string GetCurrentTimeBlockKey()
    {
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
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