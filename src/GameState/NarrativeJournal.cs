using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks the history of narrative events, choices, and player decisions
/// Enables querying past narrative interactions and player behavior patterns
/// </summary>
public class NarrativeJournal
{
    // History of all narrative events
    public List<NarrativeEvent> History { get; set; } = new List<NarrativeEvent>();
    
    // Count of specific choices made across all narratives
    public Dictionary<string, int> ChoiceCounters { get; set; } = new Dictionary<string, int>();
    
    // Track which narratives have been started/completed
    public HashSet<string> StartedNarratives { get; set; } = new HashSet<string>();
    public HashSet<string> CompletedNarratives { get; set; } = new HashSet<string>();
    
    // Track relationships formed through narratives
    public Dictionary<string, List<string>> NarrativeRelationships { get; set; } = new Dictionary<string, List<string>>();
    
    /// <summary>
    /// Record that a narrative has been started
    /// </summary>
    public void RecordNarrativeStarted(string narrativeId)
    {
        StartedNarratives.Add(narrativeId);
        
        History.Add(new NarrativeEvent
        {
            Timestamp = DateTime.Now,
            EventType = NarrativeEventType.NarrativeStarted,
            NarrativeId = narrativeId
        });
    }
    
    /// <summary>
    /// Record that a narrative has been completed
    /// </summary>
    public void RecordNarrativeCompleted(string narrativeId)
    {
        CompletedNarratives.Add(narrativeId);
        
        History.Add(new NarrativeEvent
        {
            Timestamp = DateTime.Now,
            EventType = NarrativeEventType.NarrativeCompleted,
            NarrativeId = narrativeId
        });
    }
    
    /// <summary>
    /// Record a narrative step completion
    /// </summary>
    public void RecordStepCompleted(string narrativeId, string stepId)
    {
        History.Add(new NarrativeEvent
        {
            Timestamp = DateTime.Now,
            EventType = NarrativeEventType.StepCompleted,
            NarrativeId = narrativeId,
            StepId = stepId
        });
    }
    
    /// <summary>
    /// Record a choice made during a narrative
    /// </summary>
    public void RecordChoice(string narrativeId, string stepId, string choiceId, Dictionary<string, object> context = null)
    {
        History.Add(new NarrativeEvent
        {
            Timestamp = DateTime.Now,
            EventType = NarrativeEventType.ChoiceMade,
            NarrativeId = narrativeId,
            StepId = stepId,
            ChoiceId = choiceId,
            Context = context
        });
        
        // Update choice counters
        var key = $"{narrativeId}.{choiceId}";
        ChoiceCounters[key] = ChoiceCounters.GetValueOrDefault(key) + 1;
        
        // Also track global choice patterns
        var globalKey = $"global.{choiceId}";
        ChoiceCounters[globalKey] = ChoiceCounters.GetValueOrDefault(globalKey) + 1;
    }
    
    /// <summary>
    /// Record a relationship formed through narrative
    /// </summary>
    public void RecordRelationshipFormed(string narrativeId, string npcId, string relationshipType)
    {
        if (!NarrativeRelationships.ContainsKey(npcId))
        {
            NarrativeRelationships[npcId] = new List<string>();
        }
        
        NarrativeRelationships[npcId].Add($"{narrativeId}:{relationshipType}");
        
        History.Add(new NarrativeEvent
        {
            Timestamp = DateTime.Now,
            EventType = NarrativeEventType.RelationshipFormed,
            NarrativeId = narrativeId,
            Context = new Dictionary<string, object>
            {
                ["npcId"] = npcId,
                ["relationshipType"] = relationshipType
            }
        });
    }
    
    /// <summary>
    /// Check if player has made a specific choice before
    /// </summary>
    public bool HasMadeChoice(string narrativeId, string choiceId)
    {
        return ChoiceCounters.ContainsKey($"{narrativeId}.{choiceId}");
    }
    
    /// <summary>
    /// Get count of how many times a choice has been made
    /// </summary>
    public int GetChoiceCount(string narrativeId, string choiceId)
    {
        return ChoiceCounters.GetValueOrDefault($"{narrativeId}.{choiceId}", 0);
    }
    
    /// <summary>
    /// Check if player tends toward certain choice patterns
    /// </summary>
    public Dictionary<string, float> GetChoiceTendencies()
    {
        var tendencies = new Dictionary<string, float>();
        var globalChoices = ChoiceCounters.Where(kvp => kvp.Key.StartsWith("global."));
        
        var total = globalChoices.Sum(kvp => kvp.Value);
        if (total == 0) return tendencies;
        
        foreach (var choice in globalChoices)
        {
            var choiceType = choice.Key.Substring(7); // Remove "global."
            tendencies[choiceType] = (float)choice.Value / total;
        }
        
        return tendencies;
    }
    
    /// <summary>
    /// Get history of interactions with a specific NPC
    /// </summary>
    public List<NarrativeEvent> GetNPCHistory(string npcId)
    {
        return History.Where(e => 
            e.Context != null && 
            e.Context.TryGetValue("npcId", out var id) && 
            id.ToString() == npcId
        ).ToList();
    }
    
    /// <summary>
    /// Get all events for a specific narrative
    /// </summary>
    public List<NarrativeEvent> GetNarrativeHistory(string narrativeId)
    {
        return History.Where(e => e.NarrativeId == narrativeId).ToList();
    }
    
    /// <summary>
    /// Get recent narrative activity
    /// </summary>
    public List<NarrativeEvent> GetRecentHistory(int days = 7)
    {
        var cutoff = DateTime.Now.AddDays(-days);
        return History.Where(e => e.Timestamp >= cutoff).ToList();
    }
    
    /// <summary>
    /// Check if player completed narrative within time limit
    /// </summary>
    public bool CompletedNarrativeInTime(string narrativeId, int maxDays)
    {
        var started = History.FirstOrDefault(e => 
            e.NarrativeId == narrativeId && 
            e.EventType == NarrativeEventType.NarrativeStarted);
            
        var completed = History.FirstOrDefault(e => 
            e.NarrativeId == narrativeId && 
            e.EventType == NarrativeEventType.NarrativeCompleted);
            
        if (started == null || completed == null) return false;
        
        return (completed.Timestamp - started.Timestamp).TotalDays <= maxDays;
    }
    
    /// <summary>
    /// Get statistics about narrative participation
    /// </summary>
    public NarrativeStatistics GetStatistics()
    {
        return new NarrativeStatistics
        {
            TotalNarrativesStarted = StartedNarratives.Count,
            TotalNarrativesCompleted = CompletedNarratives.Count,
            CompletionRate = StartedNarratives.Count > 0 
                ? (float)CompletedNarratives.Count / StartedNarratives.Count 
                : 0,
            TotalChoicesMade = ChoiceCounters.Values.Sum(),
            UniqueRelationshipsFormed = NarrativeRelationships.Count,
            MostCommonChoices = ChoiceCounters
                .Where(kvp => kvp.Key.StartsWith("global."))
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => new { Choice = kvp.Key.Substring(7), Count = kvp.Value })
                .ToList()
        };
    }
}

/// <summary>
/// Types of narrative events that can be recorded
/// </summary>
public enum NarrativeEventType
{
    NarrativeStarted,
    NarrativeCompleted,
    StepCompleted,
    ChoiceMade,
    RelationshipFormed,
    RewardReceived,
    ConsequenceApplied
}

/// <summary>
/// Record of a single narrative event
/// </summary>
public class NarrativeEvent
{
    public DateTime Timestamp { get; set; }
    public NarrativeEventType EventType { get; set; }
    public string NarrativeId { get; set; }
    public string StepId { get; set; }
    public string ChoiceId { get; set; }
    public Dictionary<string, object> Context { get; set; }
}

/// <summary>
/// Statistics about player's narrative participation
/// </summary>
public class NarrativeStatistics
{
    public int TotalNarrativesStarted { get; set; }
    public int TotalNarrativesCompleted { get; set; }
    public float CompletionRate { get; set; }
    public int TotalChoicesMade { get; set; }
    public int UniqueRelationshipsFormed { get; set; }
    public object MostCommonChoices { get; set; }
}