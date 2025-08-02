using System;
using System.Collections.Generic;

/// <summary>
/// Represents discovered information in the game's two-phase discovery system.
/// Phase 1: Learn information exists (discovery)
/// Phase 2: Access requires tokens/seals/equipment (gating)
/// </summary>
public class Information
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public InformationType Type { get; set; }
    public int Tier { get; set; } = 1; // 1-5, affects discovery and access requirements
    
    // What this information reveals when discovered
    public string TargetId { get; set; } // ID of route/NPC/location/service being revealed
    public string RevealText { get; set; } // Text shown when information is discovered
    
    // Discovery requirements (Phase 1)
    public List<string> DiscoveryRequirements { get; set; } = new List<string>();
    public int MinimumRelationshipForDiscovery { get; set; } = 0;
    
    // Access requirements (Phase 2)
    public Dictionary<ConnectionType, int> TokenRequirements { get; set; } = new Dictionary<ConnectionType, int>();
    public List<string> SealRequirements { get; set; } = new List<string>();
    public List<string> EquipmentRequirements { get; set; } = new List<string>();
    public int CoinCost { get; set; } = 0;
    
    // Sources of this information
    public List<InformationSource> Sources { get; set; } = new List<InformationSource>();
    
    // Leverage mechanics
    public bool CanBeUsedAsLeverage { get; set; } = false;
    public string LeverageAgainstNpcId { get; set; }
    public int LeverageValue { get; set; } = 0; // How much this info is worth as leverage
    
    // Tracking
    public bool IsDiscovered { get; set; } = false;
    public bool IsAccessUnlocked { get; set; } = false;
    public int DayDiscovered { get; set; } = -1;
    public int DayAccessUnlocked { get; set; } = -1;
    
    public Information()
    {
        Id = Guid.NewGuid().ToString();
    }
}

public enum InformationType
{
    RouteExistence,       // Knowledge that a route exists
    LocationExistence,    // Knowledge that a location/spot exists  
    NPCExistence,        // Knowledge that an NPC exists
    ServiceAvailability, // Knowledge that a service is available
    LetterAvailability,  // Knowledge that certain letters can be obtained
    SecretKnowledge,     // Leverage-worthy secrets about NPCs
    AccessRequirement    // Knowledge of what's needed to access something
}

public class InformationSource
{
    public string SourceType { get; set; } // "NPC", "Letter", "Location", "Exploration"
    public string SourceId { get; set; }
    public Dictionary<ConnectionType, int> TokenThreshold { get; set; } = new Dictionary<ConnectionType, int>();
    public string DiscoveryText { get; set; } // What the source says when revealing info
}

/// <summary>
/// Manages the discovery and access of information
/// </summary>
public class InformationDiscoveryManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ConnectionTokenManager _tokenManager;
    
    // Information registry
    private readonly Dictionary<string, Information> _allInformation = new Dictionary<string, Information>();
    private readonly Dictionary<string, List<string>> _informationByTarget = new Dictionary<string, List<string>>();
    
    public InformationDiscoveryManager(GameWorld gameWorld, MessageSystem messageSystem, ConnectionTokenManager tokenManager)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _tokenManager = tokenManager;
    }
    
    /// <summary>
    /// Register information that can be discovered
    /// </summary>
    public void RegisterInformation(Information info)
    {
        _allInformation[info.Id] = info;
        
        // Index by target for quick lookup
        if (!string.IsNullOrEmpty(info.TargetId))
        {
            if (!_informationByTarget.ContainsKey(info.TargetId))
            {
                _informationByTarget[info.TargetId] = new List<string>();
            }
            _informationByTarget[info.TargetId].Add(info.Id);
        }
    }
    
    /// <summary>
    /// Check if player has discovered specific information
    /// </summary>
    public bool IsInformationDiscovered(string informationId)
    {
        if (_allInformation.TryGetValue(informationId, out Information info))
        {
            return info.IsDiscovered;
        }
        return false;
    }
    
    /// <summary>
    /// Check if player has unlocked access to information's target
    /// </summary>
    public bool IsAccessUnlocked(string informationId)
    {
        if (_allInformation.TryGetValue(informationId, out Information info))
        {
            return info.IsAccessUnlocked;
        }
        return false;
    }
    
    /// <summary>
    /// Discover information through NPC conversation
    /// </summary>
    public bool TryDiscoverFromNPC(string npcId, ConnectionType tokenType)
    {
        bool discoveredAny = false;
        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npcId);
        
        foreach (Information info in _allInformation.Values)
        {
            if (info.IsDiscovered) continue;
            
            // Check if this NPC is a source
            InformationSource source = info.Sources.Find(s => s.SourceType == "NPC" && s.SourceId == npcId);
            if (source == null) continue;
            
            // Check token threshold
            if (source.TokenThreshold.TryGetValue(tokenType, out int required) && 
                npcTokens.GetValueOrDefault(tokenType) >= required)
            {
                DiscoverInformation(info, source.DiscoveryText);
                discoveredAny = true;
            }
        }
        
        return discoveredAny;
    }
    
    /// <summary>
    /// Discover information from letter delivery
    /// </summary>
    public void DiscoverFromLetterDelivery(string letterId, string senderId, string recipientId)
    {
        foreach (Information info in _allInformation.Values)
        {
            if (info.IsDiscovered) continue;
            
            InformationSource source = info.Sources.Find(s => 
                s.SourceType == "Letter" && 
                (s.SourceId == letterId || s.SourceId == senderId || s.SourceId == recipientId));
                
            if (source != null)
            {
                DiscoverInformation(info, source.DiscoveryText);
            }
        }
    }
    
    /// <summary>
    /// Discover information from location visit
    /// </summary>
    public void DiscoverFromLocationVisit(string locationId)
    {
        foreach (Information info in _allInformation.Values)
        {
            if (info.IsDiscovered) continue;
            
            InformationSource source = info.Sources.Find(s => 
                s.SourceType == "Location" && s.SourceId == locationId);
                
            if (source != null)
            {
                DiscoverInformation(info, source.DiscoveryText);
            }
        }
    }
    
    /// <summary>
    /// Try to unlock access to discovered information
    /// </summary>
    public bool TryUnlockAccess(string informationId)
    {
        if (!_allInformation.TryGetValue(informationId, out Information info))
            return false;
            
        if (!info.IsDiscovered || info.IsAccessUnlocked)
            return false;
            
        Player player = _gameWorld.GetPlayer();
        
        // Check token requirements
        foreach (var tokenReq in info.TokenRequirements)
        {
            if (!_tokenManager.HasTokens(tokenReq.Key, tokenReq.Value))
            {
                _messageSystem.AddSystemMessage(
                    $"Need {tokenReq.Value} {tokenReq.Key} tokens to access {info.Name}",
                    SystemMessageTypes.Warning
                );
                return false;
            }
        }
        
        // Check seal requirements
        foreach (string sealId in info.SealRequirements)
        {
            if (!player.Inventory.HasItem(sealId))
            {
                _messageSystem.AddSystemMessage(
                    $"Need {sealId} to access {info.Name}",
                    SystemMessageTypes.Warning
                );
                return false;
            }
        }
        
        // Check equipment requirements
        foreach (string equipmentId in info.EquipmentRequirements)
        {
            if (!player.Inventory.HasItem(equipmentId))
            {
                _messageSystem.AddSystemMessage(
                    $"Need {equipmentId} to access {info.Name}",
                    SystemMessageTypes.Warning
                );
                return false;
            }
        }
        
        // Check coin cost
        if (player.Coins < info.CoinCost)
        {
            _messageSystem.AddSystemMessage(
                $"Need {info.CoinCost} coins to access {info.Name}",
                SystemMessageTypes.Warning
            );
            return false;
        }
        
        // All requirements met - unlock access
        info.IsAccessUnlocked = true;
        info.DayAccessUnlocked = _gameWorld.CurrentDay;
        
        // Pay costs
        if (info.CoinCost > 0)
        {
            player.SpendMoney(info.CoinCost);
        }
        
        _messageSystem.AddSystemMessage(
            $"Access unlocked: {info.Name}",
            SystemMessageTypes.Success
        );
        
        return true;
    }
    
    /// <summary>
    /// Use information as leverage against an NPC
    /// </summary>
    public bool UseAsLeverage(string informationId, string targetNpcId)
    {
        if (!_allInformation.TryGetValue(informationId, out Information info))
            return false;
            
        if (!info.IsDiscovered || !info.CanBeUsedAsLeverage)
            return false;
            
        if (info.LeverageAgainstNpcId != targetNpcId)
            return false;
            
        // Apply leverage effects (this would integrate with obligation system)
        _messageSystem.AddSystemMessage(
            $"You use your knowledge to gain leverage over the NPC",
            SystemMessageTypes.Success
        );
        
        // Mark information as used (can only leverage once)
        info.CanBeUsedAsLeverage = false;
        
        return true;
    }
    
    /// <summary>
    /// Get all discovered information of a specific type
    /// </summary>
    public List<Information> GetDiscoveredInformation(InformationType? type = null)
    {
        List<Information> result = new List<Information>();
        
        foreach (Information info in _allInformation.Values)
        {
            if (info.IsDiscovered && (type == null || info.Type == type))
            {
                result.Add(info);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Check if a specific target (route/NPC/location) has been discovered
    /// </summary>
    public bool IsTargetDiscovered(string targetId)
    {
        if (_informationByTarget.TryGetValue(targetId, out List<string> infoIds))
        {
            foreach (string infoId in infoIds)
            {
                if (_allInformation[infoId].IsDiscovered)
                    return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Discover information by ID
    /// </summary>
    public bool DiscoverInformation(string informationId)
    {
        if (!_allInformation.TryGetValue(informationId, out Information info))
            return false;
            
        if (info.IsDiscovered) return false;
        
        DiscoverInformation(info);
        return true;
    }
    
    private void DiscoverInformation(Information info, string customText = null)
    {
        info.IsDiscovered = true;
        info.DayDiscovered = _gameWorld.CurrentDay;
        
        string message = customText ?? info.RevealText ?? $"Discovered: {info.Name}";
        _messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);
        
        // Add to player's knowledge
        _gameWorld.GetPlayer().AddMemory(
            $"info_{info.Id}",
            info.Description,
            _gameWorld.CurrentDay,
            info.Tier // Importance based on tier
        );
    }
}