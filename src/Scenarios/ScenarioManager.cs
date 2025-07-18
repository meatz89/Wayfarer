using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages scenario challenges as an independent overlay system.
/// Does not modify core game code - works entirely through existing game systems.
/// </summary>
public class ScenarioManager
{
    private readonly GameWorld _gameWorld;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly PatronLetterService _patronLetterService;
    private readonly MessageSystem _messageSystem;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;
    
    // Scenario state - stored separately from game state
    private ScenarioState _currentScenario;
    
    public ScenarioManager(
        GameWorld gameWorld,
        LetterQueueManager letterQueueManager,
        ConnectionTokenManager connectionTokenManager,
        PatronLetterService patronLetterService,
        MessageSystem messageSystem,
        LetterTemplateRepository letterTemplateRepository,
        NPCRepository npcRepository,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _letterQueueManager = letterQueueManager;
        _connectionTokenManager = connectionTokenManager;
        _patronLetterService = patronLetterService;
        _messageSystem = messageSystem;
        _letterTemplateRepository = letterTemplateRepository;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
    }
    
    // Start a scenario - creates a save snapshot and applies scenario conditions
    public void StartScenario(string scenarioId)
    {
        if (_currentScenario != null && _currentScenario.IsActive)
        {
            _messageSystem.AddSystemMessage("A scenario is already in progress!", SystemMessageTypes.Warning);
            return;
        }
        
        // Create scenario based on ID
        switch (scenarioId)
        {
            case "14_day_challenge":
                _currentScenario = Create14DayChallenge();
                break;
            default:
                _messageSystem.AddSystemMessage($"Unknown scenario: {scenarioId}", SystemMessageTypes.Danger);
                return;
        }
        
        // Save current game state
        _currentScenario.SavedGameState = CreateGameStateSnapshot();
        
        // Apply scenario starting conditions
        ApplyScenarioStartingConditions(_currentScenario);
        
        _messageSystem.AddSystemMessage(
            $"📜 SCENARIO STARTED: {_currentScenario.Name}",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            $"Goal: {_currentScenario.VictoryConditionDescription}",
            SystemMessageTypes.Info
        );
    }
    
    // End current scenario and restore saved state
    public void EndScenario()
    {
        if (_currentScenario == null || !_currentScenario.IsActive)
        {
            _messageSystem.AddSystemMessage("No scenario is currently active.", SystemMessageTypes.Warning);
            return;
        }
        
        // Restore saved game state
        RestoreGameState(_currentScenario.SavedGameState);
        
        _currentScenario = null;
        
        _messageSystem.AddSystemMessage("Scenario ended. Game state restored.", SystemMessageTypes.Info);
    }
    
    // Check scenario progress - called from game's daily update
    public void CheckScenarioProgress()
    {
        if (_currentScenario == null || !_currentScenario.IsActive) return;
        
        int daysElapsed = _gameWorld.CurrentDay - _currentScenario.StartDay;
        
        // Check for timed events
        foreach (var timedEvent in _currentScenario.TimedEvents)
        {
            if (timedEvent.Day == daysElapsed && !timedEvent.Triggered)
            {
                timedEvent.Execute(_gameWorld, this);
                timedEvent.Triggered = true;
            }
        }
        
        // Check victory/failure conditions
        if (_currentScenario.CheckVictoryCondition(_gameWorld))
        {
            ShowVictoryScreen();
            _currentScenario.IsActive = false;
        }
        else if (_currentScenario.CheckFailureCondition(_gameWorld))
        {
            ShowFailureScreen();
            _currentScenario.IsActive = false;
        }
    }
    
    // Create the 14-day challenge scenario
    private ScenarioState Create14DayChallenge()
    {
        var scenario = new ScenarioState
        {
            Id = "14_day_challenge",
            Name = "Master the Queue - Survive 14 Days",
            Description = "Manage your letter queue for 14 days while maintaining relationships",
            VictoryConditionDescription = "Maintain positive tokens with 3+ NPCs AND deliver patron's final letter",
            StartDay = _gameWorld.CurrentDay,
            Duration = 14,
            IsActive = true
        };
        
        // Store starting NPC token counts
        foreach (var npc in _npcRepository.GetAllNPCs())
        {
            var tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            scenario.StartingNPCTokens[npc.ID] = tokens.Values.Sum();
        }
        
        // Add timed events
        scenario.TimedEvents.Add(new ScenarioTimedEvent
        {
            Day = 3,
            Description = "Urgent letter wave",
            Execute = (world, manager) => GenerateUrgentLetterWave()
        });
        
        scenario.TimedEvents.Add(new ScenarioTimedEvent
        {
            Day = 6,
            Description = "Shadow obligation opportunity",
            Execute = (world, manager) => GenerateShadowOpportunity()
        });
        
        scenario.TimedEvents.Add(new ScenarioTimedEvent
        {
            Day = 9,
            Description = "Noble attention",
            Execute = (world, manager) => GenerateNobleChallenge()
        });
        
        scenario.TimedEvents.Add(new ScenarioTimedEvent
        {
            Day = 12,
            Description = "Queue overflow crisis",
            Execute = (world, manager) => GenerateQueueCrisis()
        });
        
        scenario.TimedEvents.Add(new ScenarioTimedEvent
        {
            Day = 13,
            Description = "Final patron letter",
            Execute = (world, manager) => GenerateFinalPatronLetter(scenario)
        });
        
        // Set victory condition
        scenario.CheckVictoryCondition = (world) =>
        {
            if (world.CurrentDay - scenario.StartDay < 14) return false;
            
            // Check NPC relationships
            int npcsWithPositive = 0;
            foreach (var npcId in scenario.StartingNPCTokens.Keys)
            {
                var currentTokens = _connectionTokenManager.GetTokensWithNPC(npcId).Values.Sum();
                if (currentTokens > scenario.StartingNPCTokens[npcId])
                {
                    npcsWithPositive++;
                }
            }
            
            // Check if final patron letter was delivered
            bool patronLetterDelivered = false;
            if (scenario.CustomData.ContainsKey("final_patron_letter_id"))
            {
                int finalLetterId = (int)scenario.CustomData["final_patron_letter_id"];
                var player = _gameWorld.GetPlayer();
                patronLetterDelivered = player.DeliveredLetters.Any(l => 
                    l.IsPatronLetter && l.GetHashCode() == finalLetterId);
            }
            
            return npcsWithPositive >= 3 && patronLetterDelivered;
        };
        
        // Set failure condition
        scenario.CheckFailureCondition = (world) =>
        {
            return world.CurrentDay - scenario.StartDay >= 14 && !scenario.CheckVictoryCondition(world);
        };
        
        // Set starting conditions
        scenario.StartingConditions = new ScenarioStartingConditions
        {
            StartingLocation = "millbrook",
            StartingMoney = 12,
            StartingTokens = new Dictionary<ConnectionType, int>
            {
                { ConnectionType.Trust, 2 },
                { ConnectionType.Trade, 1 },
                { ConnectionType.Common, 1 }
            },
            StartingEquipment = new List<string> { "letter_satchel" },
            StartingLetters = CreateStartingLetters()
        };
        
        return scenario;
    }
    
    // Create starting letters for 14-day challenge
    private List<Letter> CreateStartingLetters()
    {
        var letters = new List<Letter>();
        
        // Patron letter
        letters.Add(new Letter
        {
            SenderName = "Your Patron",
            RecipientName = "Marcus the Merchant",
            RecipientId = "crossbridge_merchant",
            TokenType = ConnectionType.Noble,
            Deadline = 5,
            Payment = 20,
            IsPatronLetter = true,
            Description = "Gold-sealed letter from your mysterious patron",
            Size = LetterSize.Small
        });
        
        // Trade letter
        letters.Add(new Letter
        {
            SenderName = "Market Trader",
            SenderId = "market_trader",
            RecipientName = "Camp Boss",
            RecipientId = "thornwood_camp_boss",
            TokenType = ConnectionType.Trade,
            Deadline = 3,
            Payment = 5,
            Description = "Merchant correspondence regarding shipments",
            Size = LetterSize.Small
        });
        
        // Trust letter
        letters.Add(new Letter
        {
            SenderName = "Elena the Scribe",
            SenderId = "elena_scribe",
            RecipientName = "Her dear friend",
            RecipientId = "millbrook_friend",
            TokenType = ConnectionType.Trust,
            Deadline = 4,
            Payment = 3,
            Description = "Personal letter filled with warmth",
            Size = LetterSize.Small
        });
        
        return letters;
    }
    
    // Apply scenario starting conditions
    private void ApplyScenarioStartingConditions(ScenarioState scenario)
    {
        var player = _gameWorld.GetPlayer();
        var conditions = scenario.StartingConditions;
        
        // Set location
        var location = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == conditions.StartingLocation);
        if (location != null)
        {
            player.CurrentLocation = location;
        }
        
        // Set money
        player.Coins = conditions.StartingMoney;
        
        // Clear and set tokens
        player.ConnectionTokens.Clear();
        player.NPCTokens.Clear();
        foreach (var token in conditions.StartingTokens)
        {
            player.ConnectionTokens[token.Key] = token.Value;
        }
        
        // Clear and set equipment
        player.Inventory.Clear();
        foreach (var itemId in conditions.StartingEquipment)
        {
            var item = _itemRepository.GetItemById(itemId);
            if (item != null)
            {
                player.Inventory.AddItem(item.Id);
            }
        }
        
        // Clear queue and add starting letters
        for (int i = 0; i < 8; i++)
        {
            player.LetterQueue[i] = null;
        }
        foreach (var letter in conditions.StartingLetters)
        {
            _letterQueueManager.AddLetterToFirstEmpty(letter);
        }
        
        // Set time to dawn (6 AM)
        _gameWorld.TimeManager.SetNewTime(6);
    }
    
    // Create snapshot of current game state
    private GameStateSnapshot CreateGameStateSnapshot()
    {
        var player = _gameWorld.GetPlayer();
        return new GameStateSnapshot
        {
            CurrentDay = _gameWorld.CurrentDay,
            CurrentTime = _gameWorld.TimeManager.GetCurrentTimeBlock(),
            LocationId = player.CurrentLocation?.Id,
            Money = player.Coins,
            Tokens = new Dictionary<ConnectionType, int>(player.ConnectionTokens),
            NPCTokens = player.NPCTokens.ToDictionary(
                kvp => kvp.Key,
                kvp => new Dictionary<ConnectionType, int>(kvp.Value)
            ),
            Inventory = player.Inventory.GetAllItems().Where(id => !string.IsNullOrEmpty(id)).ToList(),
            LetterQueue = player.LetterQueue.Where(l => l != null).ToList(),
            Obligations = player.StandingObligations.ToList()
        };
    }
    
    // Restore game state from snapshot
    private void RestoreGameState(GameStateSnapshot snapshot)
    {
        if (snapshot == null) return;
        
        var player = _gameWorld.GetPlayer();
        
        // Restore location
        var location = _gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == snapshot.LocationId);
        if (location != null)
        {
            player.CurrentLocation = location;
        }
        
        // Restore basic properties
        player.Coins = snapshot.Money;
        
        // Restore tokens
        player.ConnectionTokens.Clear();
        foreach (var token in snapshot.Tokens)
        {
            player.ConnectionTokens[token.Key] = token.Value;
        }
        
        player.NPCTokens.Clear();
        foreach (var npcToken in snapshot.NPCTokens)
        {
            player.NPCTokens[npcToken.Key] = new Dictionary<ConnectionType, int>(npcToken.Value);
        }
        
        // Restore inventory
        player.Inventory.Clear();
        foreach (var itemId in snapshot.Inventory)
        {
            var item = _itemRepository.GetItemById(itemId);
            if (item != null)
            {
                player.Inventory.AddItem(item.Id);
            }
        }
        
        // Restore letter queue
        for (int i = 0; i < 8; i++)
        {
            player.LetterQueue[i] = null; // Clear existing
        }
        for (int i = 0; i < snapshot.LetterQueue.Count && i < 8; i++)
        {
            player.LetterQueue[i] = snapshot.LetterQueue[i];
        }
        
        // Restore obligations
        player.StandingObligations.Clear();
        player.StandingObligations.AddRange(snapshot.Obligations);
    }
    
    // Scenario event generators
    private void GenerateUrgentLetterWave()
    {
        _messageSystem.AddSystemMessage(
            "📮 Day 3: The morning brings an unusual number of urgent requests!",
            SystemMessageTypes.Warning
        );
        
        // Generate 3 urgent letters
        for (int i = 0; i < 3; i++)
        {
            var template = _letterTemplateRepository.GetRandomTemplate();
            if (template == null) continue;
            
            var npcs = _npcRepository.GetAllNPCs();
            if (npcs.Count < 2) continue;
            
            var sender = npcs[new Random().Next(npcs.Count)];
            var recipient = npcs.Where(n => n.ID != sender.ID).First();
            
            var letter = _letterTemplateRepository.GenerateLetterFromTemplate(
                template, sender.Name, recipient.Name
            );
            
            if (letter != null && _letterQueueManager.GetLetterCount() < 8)
            {
                letter.Deadline = 2; // Very urgent!
                _letterQueueManager.AddLetterToFirstEmpty(letter);
            }
        }
    }
    
    private void GenerateShadowOpportunity()
    {
        _messageSystem.AddSystemMessage(
            "🌑 Day 6: A shadow contact approaches with a proposition...",
            SystemMessageTypes.Warning
        );
        
        var shadowTemplate = _letterTemplateRepository.GetTemplatesByTokenType(ConnectionType.Shadow)
            .FirstOrDefault();
            
        if (shadowTemplate != null && _letterQueueManager.GetLetterCount() < 8)
        {
            var letter = new Letter
            {
                SenderName = "Shadow Contact",
                RecipientName = "Unknown Associate",
                RecipientId = "crossbridge_merchant",
                TokenType = ConnectionType.Shadow,
                Deadline = 3,
                Payment = 15,
                Description = "A mysterious package wrapped in black cloth",
                Size = LetterSize.Medium
            };
            
            _letterQueueManager.AddLetterToFirstEmpty(letter);
        }
    }
    
    private void GenerateNobleChallenge()
    {
        _messageSystem.AddSystemMessage(
            "👑 Day 9: The nobility takes notice of your reliable service...",
            SystemMessageTypes.Warning
        );
        
        // Generate 2 noble letters
        for (int i = 0; i < 2; i++)
        {
            if (_letterQueueManager.GetLetterCount() >= 8) break;
            
            var letter = new Letter
            {
                SenderName = "Lord Ashford",
                RecipientName = "Court Official",
                RecipientId = "millbrook_friend",
                TokenType = ConnectionType.Noble,
                Deadline = 4,
                Payment = 8,
                Description = "Noble correspondence with official seal",
                RequiredEquipment = ItemCategory.Clothing,
                Size = LetterSize.Small
            };
            
            _letterQueueManager.AddLetterToFirstEmpty(letter);
        }
    }
    
    private void GenerateQueueCrisis()
    {
        _messageSystem.AddSystemMessage(
            "😰 Day 12: Everyone seems to need letters delivered NOW!",
            SystemMessageTypes.Danger
        );
        
        // Fill remaining queue slots
        while (_letterQueueManager.GetLetterCount() < 8)
        {
            var template = _letterTemplateRepository.GetRandomTemplate();
            if (template == null) break;
            
            var npcs = _npcRepository.GetAllNPCs();
            if (npcs.Count < 2) break;
            
            var sender = npcs[new Random().Next(npcs.Count)];
            var recipient = npcs.Where(n => n.ID != sender.ID).First();
            
            var letter = _letterTemplateRepository.GenerateLetterFromTemplate(
                template, sender.Name, recipient.Name
            );
            
            if (letter != null)
            {
                letter.Deadline = new Random().Next(2, 4);
                _letterQueueManager.AddLetterToFirstEmpty(letter);
            }
        }
    }
    
    private void GenerateFinalPatronLetter(ScenarioState scenario)
    {
        _messageSystem.AddSystemMessage(
            "🌟 YOUR PATRON'S FINAL LETTER ARRIVES! It must be delivered TODAY!",
            SystemMessageTypes.Danger
        );
        
        var finalLetter = new Letter
        {
            SenderName = "Your Patron",
            RecipientName = "The Answer",
            RecipientId = "crossbridge_merchant",
            TokenType = ConnectionType.Noble,
            Deadline = 1,
            Payment = 50,
            IsPatronLetter = true,
            PatronQueuePosition = 1,
            Description = "FINAL LETTER: The culmination of your patron's plans",
            Size = LetterSize.Large
        };
        
        // Store reference for tracking
        scenario.CustomData["final_patron_letter_id"] = finalLetter.GetHashCode();
        
        _letterQueueManager.AddPatronLetter(finalLetter);
    }
    
    // Check if a specific letter has been delivered
    private bool CheckLetterDelivered(Letter targetLetter)
    {
        if (_currentScenario == null || !_currentScenario.IsActive) return false;
        
        var player = _gameWorld.GetPlayer();
        foreach (var delivered in player.DeliveredLetters)
        {
            // Check if it's the final patron letter
            if (delivered.IsPatronLetter && 
                targetLetter.IsPatronLetter &&
                delivered.GetHashCode() == targetLetter.GetHashCode())
            {
                return true;
            }
        }
        
        return false;
    }
    
    // Victory screen
    private void ShowVictoryScreen()
    {
        _messageSystem.AddSystemMessage(
            "🎉 VICTORY! You mastered the queue and survived 14 days!",
            SystemMessageTypes.Success
        );
        
        var player = _gameWorld.GetPlayer();
        _messageSystem.AddSystemMessage(
            $"📊 Final Score: {CalculateScore()} points",
            SystemMessageTypes.Info
        );
    }
    
    // Failure screen
    private void ShowFailureScreen()
    {
        _messageSystem.AddSystemMessage(
            "💔 FAILURE: You couldn't maintain the delicate balance...",
            SystemMessageTypes.Danger
        );
        
        _messageSystem.AddSystemMessage(
            "Try the scenario again to master the queue!",
            SystemMessageTypes.Warning
        );
    }
    
    // Calculate score for scenario completion
    private int CalculateScore()
    {
        var player = _gameWorld.GetPlayer();
        int score = 0;
        
        // Points for money
        score += player.Coins * 2;
        
        // Points for total tokens
        score += player.ConnectionTokens.Values.Sum() * 10;
        
        // Points for NPC relationships
        foreach (var npcTokens in player.NPCTokens.Values)
        {
            score += npcTokens.Values.Sum() * 5;
        }
        
        return score;
    }
    
    // Get current scenario status
    public string GetScenarioStatus()
    {
        if (_currentScenario == null || !_currentScenario.IsActive)
            return "No scenario active";
            
        int daysElapsed = _gameWorld.CurrentDay - _currentScenario.StartDay;
        int daysRemaining = _currentScenario.Duration - daysElapsed;
        
        return $"{_currentScenario.Name} - Day {daysElapsed + 1}/{_currentScenario.Duration}";
    }
    
    public bool IsScenarioActive() => _currentScenario != null && _currentScenario.IsActive;
}

// Scenario state classes
public class ScenarioState
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string VictoryConditionDescription { get; set; }
    public int StartDay { get; set; }
    public int Duration { get; set; }
    public bool IsActive { get; set; }
    
    public Dictionary<string, int> StartingNPCTokens { get; set; } = new Dictionary<string, int>();
    public List<ScenarioTimedEvent> TimedEvents { get; set; } = new List<ScenarioTimedEvent>();
    public ScenarioStartingConditions StartingConditions { get; set; }
    public GameStateSnapshot SavedGameState { get; set; }
    public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
    
    public Func<GameWorld, bool> CheckVictoryCondition { get; set; }
    public Func<GameWorld, bool> CheckFailureCondition { get; set; }
}

public class ScenarioTimedEvent
{
    public int Day { get; set; }
    public string Description { get; set; }
    public bool Triggered { get; set; }
    public Action<GameWorld, ScenarioManager> Execute { get; set; }
}

public class ScenarioStartingConditions
{
    public string StartingLocation { get; set; }
    public int StartingMoney { get; set; }
    public Dictionary<ConnectionType, int> StartingTokens { get; set; }
    public List<string> StartingEquipment { get; set; }
    public List<Letter> StartingLetters { get; set; }
}

public class GameStateSnapshot
{
    public int CurrentDay { get; set; }
    public TimeBlocks CurrentTime { get; set; }
    public string LocationId { get; set; }
    public int Money { get; set; }
    public Dictionary<ConnectionType, int> Tokens { get; set; }
    public Dictionary<string, Dictionary<ConnectionType, int>> NPCTokens { get; set; }
    public List<string> Inventory { get; set; }
    public List<Letter> LetterQueue { get; set; }
    public List<StandingObligation> Obligations { get; set; }
}