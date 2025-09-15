using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

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

        // Create new observation card based on template with observation-specific properties
        string description = !string.IsNullOrEmpty(baseCard.Description) ? baseCard.Description :
                           !string.IsNullOrEmpty(observation.Text) ? observation.Text : observation.Description;

        ConversationCard observationCard = new ConversationCard
        {
            Id = $"{observation.Id}_card_{Guid.NewGuid()}",
            Description = description,
            Focus = baseCard.Focus,
            Difficulty = baseCard.Difficulty,
            TokenType = baseCard.TokenType,
            SuccessType = baseCard.SuccessType,
            FailureType = baseCard.FailureType,
            ExhaustType = baseCard.ExhaustType,
            DialogueFragment = baseCard.DialogueFragment,
            VerbPhrase = baseCard.VerbPhrase,
            PersonalityTypes = baseCard.PersonalityTypes,
            LevelBonuses = baseCard.LevelBonuses,
            MinimumTokensRequired = baseCard.MinimumTokensRequired,
            RapportThreshold = baseCard.RapportThreshold,
            QueuePosition = baseCard.QueuePosition,
            InstantRapport = baseCard.InstantRapport,
            RequestId = baseCard.RequestId,
            IsSkeleton = baseCard.IsSkeleton,
            SkeletonSource = baseCard.SkeletonSource,
            RequiredTokenType = baseCard.RequiredTokenType,
            // Override for observation cards
            Persistence = PersistenceType.Thought, // Observations persist through LISTEN
            CardType = CardType.Observation
        };

        Console.WriteLine($"[ObservationManager] Cloned observation card {observationCard.Id} from template {observation.CardTemplate} for observation {observation.Id}");
        return observationCard;
    }

    // REMOVED: DetermineCardType - hardcoded type assignment violates architecture
    // Card types should come from observation JSON properties, not switch statements

    // REMOVED: DetermineCardTemplate - hardcoded template assignment violates architecture  
    // Card templates should be specified in observation JSON, not defaulted in code

    // REMOVED: GetObservationTypeData - hardcoded values violate architecture
    // All card properties (focus, flow) should come from JSON templates

    /// <summary>
    /// Get current game time for decay calculations
    /// </summary>
    private DateTime GetCurrentGameTime()
    {
        // Convert game time to a DateTime for decay calculations
        // Day 1, Segment 0 = DateTime base, using segments instead of hours/minutes
        DateTime baseDate = new DateTime(2024, 1, 1, 0, 0, 0); // Arbitrary base date
        int gameDay = _timeManager.GetCurrentDay();
        int gameSegment = _timeManager.CurrentSegment;
        
        // Convert segments to fractional hours for DateTime calculation (16 segments = 8 hours, 2 segments per hour)
        double segmentAsHours = gameSegment * 0.5; // Each segment = 30 minutes (0.5 hours)

        return baseDate.AddDays(gameDay - 1).AddHours(segmentAsHours);
    }

    /// <summary>
    /// Get available observation rewards for a location based on familiarity and completion status
    /// </summary>
    public List<ObservationReward> GetAvailableObservationRewards(string locationId)
    {
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null)
        {
            return new List<ObservationReward>();
        }

        Player player = _gameWorld.GetPlayer();
        int currentFamiliarity = player.GetLocationFamiliarity(locationId);
        int highestCompleted = location.HighestObservationCompleted;

        List<ObservationReward> availableRewards = new List<ObservationReward>();

        foreach (ObservationReward reward in location.ObservationRewards)
        {
            // Check familiarity requirement
            if (currentFamiliarity < reward.FamiliarityRequired)
            {
                continue;
            }

            // Check prior observation requirement
            if (reward.PriorObservationRequired.HasValue && highestCompleted < reward.PriorObservationRequired.Value)
            {
                continue;
            }

            availableRewards.Add(reward);
        }

        return availableRewards;
    }

    /// <summary>
    /// Complete an observation reward and add the card to the target NPC's observation deck
    /// </summary>
    public bool CompleteObservationReward(string locationId, ObservationReward reward)
    {
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location == null)
        {
            Console.WriteLine($"[ObservationManager] Location {locationId} not found");
            return false;
        }

        // Find target NPC
        NPC targetNpc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == reward.ObservationCard.TargetNpcId);
        if (targetNpc == null)
        {
            Console.WriteLine($"[ObservationManager] Target NPC {reward.ObservationCard.TargetNpcId} not found");
            return false;
        }

        // Create observation card and add to NPC's observation deck
        ConversationCard observationCard = CreateObservationCardForNPC(reward.ObservationCard);
        if (targetNpc.ObservationDeck == null)
        {
            targetNpc.ObservationDeck = new CardDeck();
        }

        targetNpc.ObservationDeck.AddCard(observationCard);

        // Update highest observation completed (assume sequential 1, 2, 3...)
        int observationNumber = reward.FamiliarityRequired; // Use familiarity required as observation number for now
        if (observationNumber > location.HighestObservationCompleted)
        {
            location.HighestObservationCompleted = observationNumber;
        }

        Console.WriteLine($"[ObservationManager] Added observation card {observationCard.Id} to {targetNpc.Name}'s observation deck");
        return true;
    }

    /// <summary>
    /// Create a conversation card from an observation card reward
    /// </summary>
    private ConversationCard CreateObservationCardForNPC(ObservationCardReward cardReward)
    {
        // Parse the effect string to determine categorical effect type
        SuccessEffectType successType = ParseObservationEffectType(cardReward.Effect);

        return new ConversationCard
        {
            Id = cardReward.Id,
            Description = cardReward.Name,
            DialogueFragment = cardReward.Description,
            Focus = 0, // Observations cost 0 focus according to Work Packet 3
            CardType = CardType.Observation,
            Persistence = PersistenceType.Thought, // Observations persist through LISTEN
            SuccessType = successType,
            FailureType = FailureEffectType.None,
            ExhaustType = ExhaustEffectType.None,
            Difficulty = Difficulty.VeryEasy
        };
    }

    /// <summary>
    /// Parse observation effect string into a categorical effect type
    /// </summary>
    private SuccessEffectType ParseObservationEffectType(string effectString)
    {
        // Simple effect parsing for now - can be expanded based on needs
        if (effectString == "AdvanceToNeutralState")
        {
            return SuccessEffectType.Atmospheric; // Will set atmosphere based on magnitude
        }
        else if (effectString == "UnlockExchange")
        {
            return SuccessEffectType.Rapport; // Unlock exchange by adding rapport
        }

        // Default to no effect
        return SuccessEffectType.None;
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
            TimeBlocks.Midday => "midday",
            TimeBlocks.Afternoon => "afternoon",
            TimeBlocks.Evening => "evening",
            TimeBlocks.Night => "night",
            _ => "unknown"
        };
    }
}