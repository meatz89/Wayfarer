using System;
using System.Collections.Generic;

/// <summary>
/// Service for tracking game events, tutorial state, and achievements
/// Used by various systems to report events without coupling
/// </summary>
public class FlagService
{
    private Dictionary<string, bool> _booleanFlags = new Dictionary<string, bool>();
    private Dictionary<string, int> _counters = new Dictionary<string, int>();
    private Dictionary<string, DateTime> _timestamps = new Dictionary<string, DateTime>();
    
    // Tutorial-specific flags
    public const string TUTORIAL_STARTED = "tutorial_started";
    public const string TUTORIAL_COMPLETED = "tutorial_completed";
    public const string TUTORIAL_DAY1_COMPLETE = "tutorial_day1_complete";
    public const string TUTORIAL_DAY2_COMPLETE = "tutorial_day2_complete";
    public const string TUTORIAL_DAY3_COMPLETE = "tutorial_day3_complete";
    
    // Tutorial step flags
    public const string TUTORIAL_FIRST_REST = "tutorial_first_rest";
    public const string TUTORIAL_FIRST_MOVEMENT = "tutorial_first_movement";
    public const string TUTORIAL_FIRST_CONVERSATION = "tutorial_first_conversation";
    public const string TUTORIAL_FIRST_WORK = "tutorial_first_work";
    public const string TUTORIAL_FIRST_LETTER_ACCEPTED = "tutorial_first_letter_accepted";
    public const string TUTORIAL_FIRST_LETTER_COLLECTED = "tutorial_first_letter_collected";
    public const string TUTORIAL_FIRST_LETTER_DELIVERED = "tutorial_first_letter_delivered";
    public const string TUTORIAL_PATRON_MET = "tutorial_patron_met";
    public const string TUTORIAL_PATRON_LETTER_RECEIVED = "tutorial_patron_letter_received";
    public const string TUTORIAL_QUEUE_CONFLICT_EXPERIENCED = "tutorial_queue_conflict_experienced";
    
    // Additional tutorial flags for complete flow
    public const string TUTORIAL_TAM_CONVERSATION = "tutorial_tam_conversation";
    public const string TUTORIAL_DOCKS_VISITED = "tutorial_docks_visited";
    public const string TUTORIAL_SCHEDULE_LEARNED = "tutorial_schedule_learned";
    public const string TUTORIAL_MARTHA_MET = "tutorial_martha_met";
    public const string TUTORIAL_ELENA_MET = "tutorial_elena_met";
    public const string FISHMONGER_LETTER_ACCEPTED = "fishmonger_letter_accepted";
    public const string TUTORIAL_QUEUE_CRISIS = "tutorial_queue_crisis";
    public const string TUTORIAL_TOKEN_BURNING_LEARNED = "tutorial_token_burning_learned";
    public const string TUTORIAL_DEBT_INTRODUCED = "tutorial_debt_introduced";
    public const string TUTORIAL_TAM_PROPHECY = "tutorial_tam_prophecy";
    public const string TUTORIAL_INN_REACHED = "tutorial_inn_reached";
    public const string TUTORIAL_PATRON_LETTER_ACCEPTED = "tutorial_patron_letter_accepted";
    public const string RELATIONSHIP_STRENGTHENED = "relationship_strengthened";
    public const string SECOND_LETTER_ACCEPTED = "second_letter_accepted";
    
    // Game event flags
    public const string FIRST_TOKEN_EARNED = "first_token_earned";
    public const string FIRST_LETTER_IN_QUEUE = "first_letter_in_queue";
    public const string FIRST_STANDING_OBLIGATION = "first_standing_obligation";
    public const string STAMINA_DEPLETED = "stamina_depleted";
    public const string COINS_DEPLETED = "coins_depleted";
    public const string FIRST_CONVERSATION = "first_conversation";
    public const string FIRST_LETTER_ACCEPTED = "first_letter_accepted";
    public const string FIRST_LETTER_COLLECTED = "first_letter_collected";
    public const string FIRST_LETTER_DELIVERED = "first_letter_delivered";
    
    // Counters
    public const string LETTERS_DELIVERED = "letters_delivered";
    public const string TOKENS_EARNED = "tokens_earned";
    public const string DAYS_SURVIVED = "days_survived";
    public const string CONVERSATIONS_HELD = "conversations_held";
    public const string WORK_ACTIONS_COMPLETED = "work_actions_completed";
    
    public void SetFlag(string key, bool value)
    {
        _booleanFlags[key] = value;
        if (value)
        {
            _timestamps[key] = DateTime.Now;
        }
    }
    
    public bool GetFlag(string key)
    {
        return _booleanFlags.ContainsKey(key) && _booleanFlags[key];
    }
    
    public void IncrementCounter(string key, int amount = 1)
    {
        if (!_counters.ContainsKey(key))
        {
            _counters[key] = 0;
        }
        _counters[key] += amount;
    }
    
    public int GetCounter(string key)
    {
        return _counters.ContainsKey(key) ? _counters[key] : 0;
    }
    
    public void ResetCounter(string key)
    {
        _counters[key] = 0;
    }
    
    public DateTime? GetFlagTimestamp(string key)
    {
        return _timestamps.ContainsKey(key) ? _timestamps[key] : (DateTime?)null;
    }
    
    public bool IsTutorialActive()
    {
        return GetFlag(TUTORIAL_STARTED) && !GetFlag(TUTORIAL_COMPLETED);
    }
    
    public int GetTutorialDay()
    {
        if (!IsTutorialActive()) return 0;
        
        if (GetFlag(TUTORIAL_DAY3_COMPLETE)) return 4; // Tutorial complete
        if (GetFlag(TUTORIAL_DAY2_COMPLETE)) return 3;
        if (GetFlag(TUTORIAL_DAY1_COMPLETE)) return 2;
        return 1;
    }
    
    // Serialization support
    public FlagServiceState GetState()
    {
        return new FlagServiceState
        {
            BooleanFlags = new Dictionary<string, bool>(_booleanFlags),
            Counters = new Dictionary<string, int>(_counters),
            Timestamps = new Dictionary<string, DateTime>(_timestamps)
        };
    }
    
    public void LoadState(FlagServiceState state)
    {
        if (state != null)
        {
            _booleanFlags = new Dictionary<string, bool>(state.BooleanFlags ?? new Dictionary<string, bool>());
            _counters = new Dictionary<string, int>(state.Counters ?? new Dictionary<string, int>());
            _timestamps = new Dictionary<string, DateTime>(state.Timestamps ?? new Dictionary<string, DateTime>());
        }
    }
}

public class FlagServiceState
{
    public Dictionary<string, bool> BooleanFlags { get; set; }
    public Dictionary<string, int> Counters { get; set; }
    public Dictionary<string, DateTime> Timestamps { get; set; }
}