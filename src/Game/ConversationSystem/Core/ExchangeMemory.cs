using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks memorable exchanges with NPCs to create narrative continuity.
/// Each exchange becomes part of the shared history between player and NPC.
/// </summary>
public class ExchangeMemory
{
    private readonly Dictionary<string, List<MemorableExchange>> _npcExchanges = new();
    private const int MAX_MEMORIES_PER_NPC = 5; // Recent memories only
    
    /// <summary>
    /// Record an exchange that happened
    /// </summary>
    public void RecordExchange(string npcId, string exchangeType, EmotionalState npcState, bool wasGenerous)
    {
        if (!_npcExchanges.ContainsKey(npcId))
        {
            _npcExchanges[npcId] = new List<MemorableExchange>();
        }
        
        var memory = new MemorableExchange
        {
            ExchangeType = exchangeType,
            EmotionalContext = npcState,
            WasGenerous = wasGenerous,
            GameDay = 0, // Would be set by TimeManager
            Timestamp = DateTime.Now
        };
        
        _npcExchanges[npcId].Add(memory);
        
        // Keep only recent memories
        if (_npcExchanges[npcId].Count > MAX_MEMORIES_PER_NPC)
        {
            _npcExchanges[npcId].RemoveAt(0);
        }
    }
    
    /// <summary>
    /// Get the most emotionally significant exchange with an NPC
    /// </summary>
    public MemorableExchange GetMostSignificantExchange(string npcId)
    {
        if (!_npcExchanges.ContainsKey(npcId) || !_npcExchanges[npcId].Any())
            return null;
            
        // Prioritize exchanges during emotional states
        return _npcExchanges[npcId]
            .OrderByDescending(e => GetEmotionalWeight(e.EmotionalContext))
            .ThenByDescending(e => e.Timestamp)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// Check if player has shown generosity pattern
    /// </summary>
    public bool HasGenerosityPattern(string npcId)
    {
        if (!_npcExchanges.ContainsKey(npcId))
            return false;
            
        var recentExchanges = _npcExchanges[npcId].TakeLast(3);
        return recentExchanges.Count(e => e.WasGenerous) >= 2;
    }
    
    /// <summary>
    /// Get narrative flavor based on exchange history
    /// </summary>
    public string GetExchangeFlavor(string npcId, string exchangeType)
    {
        if (!_npcExchanges.ContainsKey(npcId) || !_npcExchanges[npcId].Any())
            return "for the first time";
            
        var sameTypeCount = _npcExchanges[npcId].Count(e => e.ExchangeType == exchangeType);
        
        if (sameTypeCount == 0)
            return "for the first time";
        else if (sameTypeCount == 1)
            return "once again";
        else if (sameTypeCount < 5)
            return "as you have before";
        else
            return "as has become your custom";
    }
    
    private int GetEmotionalWeight(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => 10,
            EmotionalState.TENSE => 7,
            EmotionalState.EAGER => 5,
            EmotionalState.OPEN => 3,
            EmotionalState.NEUTRAL => 1,
            _ => 0
        };
    }
}

/// <summary>
/// A single memorable exchange that shapes future interactions
/// </summary>
public class MemorableExchange
{
    public string ExchangeType { get; set; }
    public EmotionalState EmotionalContext { get; set; }
    public bool WasGenerous { get; set; } // Player gave more than minimum
    public int GameDay { get; set; }
    public DateTime Timestamp { get; set; }
}