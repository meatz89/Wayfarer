using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Service for verifying game state consistency and debugging state issues
/// </summary>
public class StateVerificationService
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly LocationSystem _locationSystem;
    private readonly DebugLogger _debugLogger;
    
    public StateVerificationService(
        GameWorld gameWorld,
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        LocationSystem locationSystem,
        DebugLogger debugLogger)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _locationSystem = locationSystem;
        _debugLogger = debugLogger;
    }
    
    /// <summary>
    /// Verify NPCs are loaded for a specific location and spot
    /// </summary>
    public bool VerifyNPCsLoaded(string locationId, string spotId)
    {
        _debugLogger.LogDebug($"Verifying NPCs for {locationId}/{spotId}");
        
        var allNPCs = _npcRepository.GetAllNPCs();
        if (!allNPCs.Any())
        {
            _debugLogger.LogWarning("StateVerification", "No NPCs loaded in repository");
            return false;
        }
        
        var locationNPCs = allNPCs.Where(n => n.Location == locationId).ToList();
        _debugLogger.LogDebug($"Found {locationNPCs.Count} NPCs at location {locationId}");
        
        if (string.IsNullOrEmpty(spotId))
        {
            return locationNPCs.Any();
        }
        
        var spotNPCs = locationNPCs.Where(n => n.SpotId == spotId).ToList();
        _debugLogger.LogDebug($"Found {spotNPCs.Count} NPCs at spot {spotId}");
        
        return spotNPCs.Any();
    }
    
    /// <summary>
    /// Verify conversation is ready to be displayed
    /// </summary>
    public bool VerifyConversationReady()
    {
        var isReady = _gameWorld.ConversationPending && _gameWorld.PendingConversationManager != null;
        
        _debugLogger.LogDebug($"Conversation verification: Pending={_gameWorld.ConversationPending}, Manager exists={_gameWorld.PendingConversationManager != null}");
        
        if (_gameWorld.PendingConversationManager != null)
        {
            _debugLogger.LogDebug($"Conversation state: IsAwaitingResponse={_gameWorld.PendingConversationManager.IsAwaitingResponse}");
        }
        
        return isReady;
    }
    
    /// <summary>
    /// Verify travel state is consistent
    /// </summary>
    public bool VerifyTravelState()
    {
        var player = _gameWorld.GetPlayer();
        var location = _locationRepository.GetCurrentLocation();
        
        bool isValid = player.CurrentLocation != null && 
                      location != null && 
                      player.CurrentLocation.Id == location.Id;
        
        _debugLogger.LogDebug($"Travel state verification: Player location={player.CurrentLocation?.Id}, Repo location={location?.Id}, Valid={isValid}");
        
        return isValid;
    }
    
    /// <summary>
    /// Get a comprehensive state report for debugging
    /// </summary>
    public string GetStateReport()
    {
        var report = new StringBuilder();
        report.AppendLine("=== GAME STATE REPORT ===");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        
        // Player State
        var player = _gameWorld.GetPlayer();
        report.AppendLine("PLAYER STATE:");
        report.AppendLine($"  Name: {player.Name}");
        report.AppendLine($"  Location: {player.CurrentLocation?.Name ?? "NULL"} ({player.CurrentLocation?.Id ?? "NULL"})");
        report.AppendLine($"  Spot: {player.CurrentLocationSpot?.Name ?? "NULL"} ({player.CurrentLocationSpot?.SpotID ?? "NULL"})");
        report.AppendLine($"  Stamina: {player.Stamina}/{player.MaxStamina}");
        report.AppendLine($"  Coins: {player.Coins}");
        report.AppendLine($"  Archetype: {player.Archetype}");
        report.AppendLine();
        
        // Time State
        report.AppendLine("TIME STATE:");
        report.AppendLine($"  Current Day: {_gameWorld.CurrentDay}");
        report.AppendLine($"  Time Block: {_gameWorld.TimeManager.GetCurrentTimeBlock()}");
        report.AppendLine($"  Current Hours: {_gameWorld.TimeManager.CurrentTimeHours}");
        report.AppendLine($"  Hours Remaining: {_gameWorld.TimeManager.HoursRemaining}");
        report.AppendLine();
        
        // Location State
        report.AppendLine("LOCATION STATE:");
        var currentLocation = _locationRepository.GetCurrentLocation();
        var currentSpot = _locationRepository.GetCurrentLocationSpot();
        report.AppendLine($"  Repository Location: {currentLocation?.Name ?? "NULL"} ({currentLocation?.Id ?? "NULL"})");
        report.AppendLine($"  Repository Spot: {currentSpot?.Name ?? "NULL"} ({currentSpot?.SpotID ?? "NULL"})");
        
        if (currentLocation != null)
        {
            var spots = _locationSystem.GetLocationSpots(currentLocation.Id);
            report.AppendLine($"  Available Spots: {spots.Count}");
            foreach (var spot in spots)
            {
                report.AppendLine($"    - {spot.Name} ({spot.SpotID}) - {(spot.IsClosed ? "CLOSED" : "OPEN")}");
            }
        }
        report.AppendLine();
        
        // NPC State
        report.AppendLine("NPC STATE:");
        report.Append(GetNPCStateReport());
        report.AppendLine();
        
        // Conversation State
        report.AppendLine("CONVERSATION STATE:");
        report.AppendLine($"  Conversation Pending: {_gameWorld.ConversationPending}");
        report.AppendLine($"  Pending Manager Exists: {_gameWorld.PendingConversationManager != null}");
        if (_gameWorld.PendingConversationManager != null)
        {
            var manager = _gameWorld.PendingConversationManager;
            report.AppendLine($"  Is Awaiting Response: {manager.IsAwaitingResponse}");
            report.AppendLine($"  Current Narrative Length: {manager.State?.CurrentNarrative?.Length ?? 0}");
            report.AppendLine($"  Choices Count: {manager.Choices?.Count ?? 0}");
        }
        
        if (_gameWorld.PendingAction != null)
        {
            report.AppendLine($"  Pending Action: {_gameWorld.PendingAction.Name} ({_gameWorld.PendingAction.Action})");
        }
        report.AppendLine();
        
        // Inventory State
        report.AppendLine("INVENTORY STATE:");
        report.AppendLine($"  Items: {string.Join(", ", player.Inventory.ItemSlots.Where(i => !string.IsNullOrEmpty(i)))}");
        report.AppendLine($"  Empty Slots: {player.Inventory.ItemSlots.Count(string.IsNullOrEmpty)}");
        report.AppendLine();
        
        // Recent System Messages
        report.AppendLine("RECENT SYSTEM MESSAGES:");
        var recentMessages = _gameWorld.SystemMessages.TakeLast(5);
        foreach (var msg in recentMessages)
        {
            report.AppendLine($"  [{msg.Timestamp:HH:mm:ss}] {msg.Type}: {msg.Message}");
        }
        
        return report.ToString();
    }
    
    /// <summary>
    /// Get detailed NPC availability report
    /// </summary>
    public string GetNPCStateReport()
    {
        var report = new StringBuilder();
        var player = _gameWorld.GetPlayer();
        var currentTime = _gameWorld.TimeManager.GetCurrentTimeBlock();
        
        // Get all NPCs
        var allNPCs = _npcRepository.GetAllNPCs();
        report.AppendLine($"  Total NPCs in game: {allNPCs.Count}");
        
        if (player.CurrentLocation == null)
        {
            report.AppendLine("  Cannot check location NPCs - player location is null");
            return report.ToString();
        }
        
        // Filter by location
        var locationNPCs = allNPCs.Where(n => n.Location == player.CurrentLocation.Id).ToList();
        report.AppendLine($"  NPCs at current location '{player.CurrentLocation.Id}': {locationNPCs.Count}");
        
        if (locationNPCs.Any())
        {
            report.AppendLine("  Location NPCs:");
            foreach (var npc in locationNPCs)
            {
                report.AppendLine($"    - {npc.Name} ({npc.ID})");
                report.AppendLine($"      Spot: {npc.SpotId ?? "NULL"}");
                report.AppendLine($"      Schedule: {npc.AvailabilitySchedule}");
                report.AppendLine($"      Available now: {npc.IsAvailable(currentTime)}");
            }
        }
        
        if (player.CurrentLocationSpot != null)
        {
            // Filter by spot
            var spotNPCs = locationNPCs.Where(n => n.SpotId == player.CurrentLocationSpot.SpotID).ToList();
            report.AppendLine($"  NPCs at current spot '{player.CurrentLocationSpot.SpotID}': {spotNPCs.Count}");
            
            // Filter by availability
            var availableNPCs = spotNPCs.Where(n => n.IsAvailable(currentTime)).ToList();
            report.AppendLine($"  Available NPCs at current time '{currentTime}': {availableNPCs.Count}");
            
            if (availableNPCs.Any())
            {
                report.AppendLine("  Available NPCs:");
                foreach (var npc in availableNPCs)
                {
                    report.AppendLine($"    - {npc.Name}: {string.Join(", ", npc.ProvidedServices)}");
                }
            }
        }
        
        return report.ToString();
    }
    
    /// <summary>
    /// Verify morning activities are ready
    /// </summary>
    public bool VerifyMorningActivitiesReady()
    {
        // Morning activities happen at Dawn (6 AM)
        var currentTime = _gameWorld.TimeManager.GetCurrentTimeBlock();
        var currentHour = _gameWorld.TimeManager.CurrentTimeHours;
        
        bool isCorrectTime = currentTime == TimeBlocks.Dawn && currentHour == 6;
        
        _debugLogger.LogDebug($"Morning activities check: Time={currentTime}, Hour={currentHour}, Ready={isCorrectTime}");
        
        return isCorrectTime;
    }
    
    /// <summary>
    /// Run all verifications and log results
    /// </summary>
    public void RunFullVerification()
    {
        _debugLogger.LogDebug("=== RUNNING FULL STATE VERIFICATION ===");
        
        var player = _gameWorld.GetPlayer();
        
        // Verify basic state
        bool playerValid = player != null && player.IsInitialized;
        _debugLogger.LogDebug($"Player valid: {playerValid}");
        
        // Verify location
        bool travelStateValid = VerifyTravelState();
        _debugLogger.LogDebug($"Travel state valid: {travelStateValid}");
        
        // Verify NPCs
        if (player?.CurrentLocation != null)
        {
            bool npcsLoaded = VerifyNPCsLoaded(player.CurrentLocation.Id, player.CurrentLocationSpot?.SpotID);
            _debugLogger.LogDebug($"NPCs loaded: {npcsLoaded}");
        }
        
        // Verify conversation
        bool conversationReady = VerifyConversationReady();
        _debugLogger.LogDebug($"Conversation ready: {conversationReady}");
        
        // Verify morning
        bool morningReady = VerifyMorningActivitiesReady();
        _debugLogger.LogDebug($"Morning activities ready: {morningReady}");
        
        _debugLogger.LogDebug("=== VERIFICATION COMPLETE ===");
    }
}