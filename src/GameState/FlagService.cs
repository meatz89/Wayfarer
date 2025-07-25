using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for managing game flags and state tracking in a decoupled way.
/// Systems drop flags here, and other systems (like NarrativeManager) can check them.
/// This allows existing game systems to remain unaware of the narrative system.
/// </summary>
public class FlagService
{
    // Boolean flags (e.g., tutorial_started, first_letter_delivered)
    private readonly Dictionary<string, bool> _booleanFlags = new Dictionary<string, bool>();
    
    // Counter flags (e.g., letters_delivered_count, tokens_earned_count)
    private readonly Dictionary<string, int> _counterFlags = new Dictionary<string, int>();
    
    // Timestamp flags (e.g., last_patron_request_day)
    private readonly Dictionary<string, DateTime> _timestampFlags = new Dictionary<string, DateTime>();
    
    // Tutorial-specific flags
    public const string TUTORIAL_STARTED = "tutorial_started";
    public const string TUTORIAL_COMPLETE = "tutorial_complete";
    public const string TUTORIAL_FIRST_MOVEMENT = "tutorial_first_movement";
    public const string TUTORIAL_FIRST_NPC_TALK = "tutorial_first_npc_talk";
    public const string TUTORIAL_FIRST_WORK = "tutorial_first_work";
    public const string TUTORIAL_FIRST_TOKEN_EARNED = "tutorial_first_token_earned";
    public const string TUTORIAL_FIRST_LETTER_OFFERED = "tutorial_first_letter_offered";
    public const string TUTORIAL_FIRST_LETTER_ACCEPTED = "tutorial_first_letter_accepted";
    public const string TUTORIAL_FIRST_LETTER_COLLECTED = "tutorial_first_letter_collected";
    public const string TUTORIAL_FIRST_LETTER_DELIVERED = "tutorial_first_letter_delivered";
    public const string TUTORIAL_FIRST_TOKEN_BURNED = "tutorial_first_token_burned";
    public const string TUTORIAL_DESPERATION_REACHED = "tutorial_desperation_reached";
    public const string TUTORIAL_PATRON_LETTER_RECEIVED = "tutorial_patron_letter_received";
    public const string TUTORIAL_PATRON_MET = "tutorial_patron_met";
    public const string TUTORIAL_PATRON_ACCEPTED = "tutorial_patron_accepted";
    
    // General game event flags
    public const string FIRST_COLLAPSE = "first_collapse";
    public const string MEDICINE_DELIVERED_IN_TIME = "medicine_delivered_in_time";
    public const string CHILD_DIED = "child_died";
    
    /// <summary>
    /// Set a boolean flag
    /// </summary>
    public void SetFlag(string flagName, bool value = true)
    {
        _booleanFlags[flagName] = value;
    }
    
    /// <summary>
    /// Check if a boolean flag is set
    /// </summary>
    public bool HasFlag(string flagName)
    {
        return _booleanFlags.TryGetValue(flagName, out bool value) && value;
    }
    
    /// <summary>
    /// Set a counter value
    /// </summary>
    public void SetCounter(string counterName, int value)
    {
        _counterFlags[counterName] = value;
    }
    
    /// <summary>
    /// Increment a counter
    /// </summary>
    public void IncrementCounter(string counterName, int amount = 1)
    {
        if (_counterFlags.TryGetValue(counterName, out int currentValue))
        {
            _counterFlags[counterName] = currentValue + amount;
        }
        else
        {
            _counterFlags[counterName] = amount;
        }
    }
    
    /// <summary>
    /// Get a counter value
    /// </summary>
    public int GetCounter(string counterName)
    {
        return _counterFlags.TryGetValue(counterName, out int value) ? value : 0;
    }
    
    /// <summary>
    /// Set a timestamp
    /// </summary>
    public void SetTimestamp(string timestampName, DateTime value)
    {
        _timestampFlags[timestampName] = value;
    }
    
    /// <summary>
    /// Get a timestamp
    /// </summary>
    public DateTime? GetTimestamp(string timestampName)
    {
        return _timestampFlags.TryGetValue(timestampName, out DateTime value) ? value : null;
    }
    
    /// <summary>
    /// Clear all flags (useful for new game)
    /// </summary>
    public void ClearAllFlags()
    {
        _booleanFlags.Clear();
        _counterFlags.Clear();
        _timestampFlags.Clear();
    }
    
    /// <summary>
    /// Get all flags for serialization
    /// </summary>
    public FlagServiceState GetState()
    {
        return new FlagServiceState
        {
            BooleanFlags = new Dictionary<string, bool>(_booleanFlags),
            CounterFlags = new Dictionary<string, int>(_counterFlags),
            TimestampFlags = new Dictionary<string, DateTime>(_timestampFlags)
        };
    }
    
    /// <summary>
    /// Restore flags from serialization
    /// </summary>
    public void RestoreState(FlagServiceState state)
    {
        if (state == null) return;
        
        _booleanFlags.Clear();
        _counterFlags.Clear();
        _timestampFlags.Clear();
        
        if (state.BooleanFlags != null)
        {
            foreach (var kvp in state.BooleanFlags)
            {
                _booleanFlags[kvp.Key] = kvp.Value;
            }
        }
        
        if (state.CounterFlags != null)
        {
            foreach (var kvp in state.CounterFlags)
            {
                _counterFlags[kvp.Key] = kvp.Value;
            }
        }
        
        if (state.TimestampFlags != null)
        {
            foreach (var kvp in state.TimestampFlags)
            {
                _timestampFlags[kvp.Key] = kvp.Value;
            }
        }
    }
}

/// <summary>
/// Serializable state for FlagService
/// </summary>
public class FlagServiceState
{
    public Dictionary<string, bool> BooleanFlags { get; set; }
    public Dictionary<string, int> CounterFlags { get; set; }
    public Dictionary<string, DateTime> TimestampFlags { get; set; }
}