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
    /// Take an observation and generate observation cards for relevant NPCs
    /// Each observation is added to the specific NPC's observation deck
    /// Returns the first generated observation card on success, null if already taken or invalid
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
        SocialCard conversationCard = GenerateConversationCard(observation, tokenManager);

        if (conversationCard != null)
        {
            // Create observation card with decay tracking
            DateTime currentGameTime = GetCurrentGameTime();

            // ARCHITECTURE: Observations are stored in NPC-specific decks
            // Each observation must specify which NPCs it's relevant to
            // This ensures observations are contextually appropriate when played
            if (observation.RelevantNPCs != null && observation.RelevantNPCs.Length > 0)
            {
                bool addedToAnyNPC = false;

                // Add observation to each relevant NPC's deck
                foreach (string npcId in observation.RelevantNPCs)
                {
                    NPC targetNpc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
                    if (targetNpc != null)
                    {
                        Console.WriteLine($"[ObservationManager] Added observation {observation.Id} to {targetNpc.Name}'s deck");
                        addedToAnyNPC = true;
                    }
                    else
                    {
                        Console.WriteLine($"[ObservationManager] Warning: NPC {npcId} not found for observation {observation.Id}");
                    }
                }

                if (addedToAnyNPC)
                {
                    // Store taken observation details for UI display
                    TakenObservation takenObs = new TakenObservation
                    {
                        Id = observation.Id,
                        Name = observation.Text,
                        Description = observation.Description,
                        NarrativeText = observation.Description,  // Use description as narrative
                        TimeTaken = currentGameTime,
                        TimeBlockTaken = _timeManager.GetCurrentTimeBlock()
                    };
                    _takenObservations[observation.Id] = takenObs;
                }
                else
                {
                    Console.WriteLine($"[ObservationManager] Failed to add observation to any NPC deck");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"[ObservationManager] Observation {observation.Id} has no relevant NPCs specified");
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Get all taken observations for the current time block
    /// Tracks which Venue observations the player has taken this time block
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
    /// Generate a conversation card from an observation using JSON-based card templates
    /// </summary>
    private SocialCard GenerateConversationCard(Observation observation, TokenMechanicsManager tokenManager)
    {
        if (string.IsNullOrEmpty(observation.CardTemplate))
        {
            Console.WriteLine($"[ObservationManager] No card template specified for observation {observation.Id}");
            return null;
        }

        // Load the base card template from GameWorld's AllCardDefinitions
        SocialCard? entry = _gameWorld.SocialCards.FindById(observation.CardTemplate);
        if (entry == null)
        {
            Console.WriteLine($"[ObservationManager] Card template '{observation.CardTemplate}' not found in AllCardDefinitions for observation {observation.Id}");
            return null;
        }

        // Create new observation card based on template with observation-specific properties
        SocialCard baseCard = entry;
        string description = !string.IsNullOrEmpty(baseCard.Title) ? baseCard.Title :
                           !string.IsNullOrEmpty(observation.Text) ? observation.Text : observation.Description;

        SocialCard observationCard = new SocialCard
        {
            Id = $"{observation.Id}_card_{Guid.NewGuid()}",
            Title = description,
            InitiativeCost = baseCard.InitiativeCost,
            TokenType = baseCard.TokenType,
            SuccessType = baseCard.SuccessType,
            DialogueText = baseCard.DialogueText,
            VerbPhrase = baseCard.VerbPhrase,
            PersonalityTypes = baseCard.PersonalityTypes,
            MinimumTokensRequired = baseCard.MinimumTokensRequired,
            MomentumThreshold = baseCard.MomentumThreshold,
            QueuePosition = baseCard.QueuePosition,
            InstantMomentum = baseCard.InstantMomentum,
            RequiredTokenType = baseCard.RequiredTokenType,
            // Override for observation cards
            Persistence = PersistenceType.Statement, // Observations persist through LISTEN
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

        // Convert segments to fractional hours for DateTime calculation
        double segmentAsHours = gameSegment * 0.5; // Convert segments to hours for DateTime

        return baseDate.AddDays(gameDay - 1).AddHours(segmentAsHours);
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