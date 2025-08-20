using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates categorical narrative data for conversations.
/// NO TEXT GENERATION - only categories, enums, and context objects.
/// Frontend components will map these to actual narrative prose.
/// </summary>
public class ConversationNarrativeGenerator
{
    private readonly NPCStateResolver _stateResolver;
    private readonly ObligationQueueManager _queueManager;
    private readonly ITimeManager _timeManager;
    private readonly NPCRelationshipTracker _relationshipTracker;

    public ConversationNarrativeGenerator(
        NPCStateResolver stateResolver,
        ObligationQueueManager queueManager,
        ITimeManager timeManager,
        NPCRelationshipTracker relationshipTracker)
    {
        _stateResolver = stateResolver;
        _queueManager = queueManager;
        _timeManager = timeManager;
        _relationshipTracker = relationshipTracker;
    }

    /// <summary>
    /// Generate narrative categories for a conversation
    /// </summary>
    public ConversationNarrativeContext GenerateContext(NPC npc, EmotionalState currentState)
    {
        // Get NPC's emotional condition from letter deadlines
        NPCEmotionalState npcCondition = _stateResolver.CalculateState(npc);
        
        // Get most urgent letter if any
        DeliveryObligation urgentLetter = GetMostUrgentLetter(npc);
        
        // Calculate pressure level
        PressureLevel pressure = CalculatePressure(urgentLetter);
        
        // Get relationship context
        NPCRelationship relationship = _relationshipTracker.GetRelationship(npc.ID);
        RelationshipDepth depth = CalculateRelationshipDepth(relationship);
        
        // Determine scene tone
        SceneTone tone = DetermineSceneTone(currentState, npcCondition, pressure);
        
        // Generate action beats (what's happening physically)
        List<ActionBeat> actionBeats = GenerateActionBeats(npc, currentState, npcCondition);
        
        return new ConversationNarrativeContext
        {
            NPCState = npcCondition,
            ConversationState = currentState,
            Pressure = pressure,
            UrgentStakes = urgentLetter?.Stakes,
            HoursToDeadline = urgentLetter?.DeadlineInMinutes,
            RelationshipDepth = depth,
            SceneTone = tone,
            ActionBeats = actionBeats,
            NPCPersonality = npc.Personality,
            LocationContext = GetLocationContext()
        };
    }

    /// <summary>
    /// Generate dialogue categories (NOT text) for NPC speech
    /// </summary>
    public DialogueContext GenerateDialogueContext(NPC npc, EmotionalState state, CardPlayResult lastResult = null)
    {
        NPCEmotionalState npcCondition = _stateResolver.CalculateState(npc);
        
        // Determine dialogue type based on state and context
        DialogueType dialogueType = DetermineDialogueType(state, npcCondition, lastResult);
        
        // Calculate urgency
        DialogueUrgency urgency = CalculateDialogueUrgency(npcCondition);
        
        // Determine emotional coloring
        EmotionalColoring coloring = DetermineEmotionalColoring(state, npc.Personality);
        
        return new DialogueContext
        {
            Type = dialogueType,
            Urgency = urgency,
            EmotionalColoring = coloring,
            Personality = npc.Personality,
            NPCState = npcCondition,
            ConversationState = state,
            ResponseToLastAction = lastResult != null
        };
    }

    private DeliveryObligation GetMostUrgentLetter(NPC npc)
    {
        var queue = _queueManager.GetActiveObligations();
        return queue
            .Where(l => l.SenderId == npc.ID || l.SenderName == npc.Name)
            .OrderBy(l => l.DeadlineInMinutes)
            .FirstOrDefault();
    }

    private PressureLevel CalculatePressure(DeliveryObligation letter)
    {
        if (letter == null) return PressureLevel.None;
        
        return letter.DeadlineInMinutes switch
        {
            <= 2 => PressureLevel.Extreme,
            <= 6 => PressureLevel.High,
            <= 12 => PressureLevel.Moderate,
            _ => PressureLevel.Low
        };
    }

    private RelationshipDepth CalculateRelationshipDepth(NPCRelationship relationship)
    {
        int totalTokens = relationship.Trust + relationship.Commerce + 
                         relationship.Status + relationship.Shadow;
        
        return totalTokens switch
        {
            >= 15 => RelationshipDepth.Intimate,
            >= 10 => RelationshipDepth.Close,
            >= 5 => RelationshipDepth.Friendly,
            >= 1 => RelationshipDepth.Acquaintance,
            _ => RelationshipDepth.Stranger
        };
    }

    private SceneTone DetermineSceneTone(EmotionalState convState, NPCEmotionalState npcState, PressureLevel pressure)
    {
        // Crisis states override everything
        if (convState == EmotionalState.DESPERATE || npcState == NPCEmotionalState.DESPERATE)
            return SceneTone.Crisis;
        
        if (convState == EmotionalState.HOSTILE || npcState == NPCEmotionalState.HOSTILE)
            return SceneTone.Confrontational;
        
        // High pressure creates tension
        if (pressure >= PressureLevel.High)
            return SceneTone.Urgent;
        
        // Positive states
        if (convState == EmotionalState.CONNECTED)
            return SceneTone.Intimate;
        
        if (convState == EmotionalState.OPEN || convState == EmotionalState.EAGER)
            return SceneTone.Warm;
        
        // Negative states
        if (convState == EmotionalState.TENSE || npcState == NPCEmotionalState.ANXIOUS)
            return SceneTone.Tense;
        
        if (convState == EmotionalState.GUARDED)
            return SceneTone.Cautious;
        
        // Default
        return SceneTone.Neutral;
    }

    private List<ActionBeat> GenerateActionBeats(NPC npc, EmotionalState state, NPCEmotionalState npcState)
    {
        var beats = new List<ActionBeat>();
        
        // Add beats based on NPC state
        if (npcState == NPCEmotionalState.DESPERATE)
        {
            beats.Add(ActionBeat.HandsTrembling);
            beats.Add(ActionBeat.EyesDarting);
        }
        else if (npcState == NPCEmotionalState.ANXIOUS)
        {
            beats.Add(ActionBeat.ShiftingWeight);
            beats.Add(ActionBeat.ForcedSmile);
        }
        else if (npcState == NPCEmotionalState.HOSTILE)
        {
            beats.Add(ActionBeat.ArmsCrossed);
            beats.Add(ActionBeat.ColdStare);
        }
        
        // Add beats based on conversation state
        if (state == EmotionalState.CONNECTED)
        {
            beats.Add(ActionBeat.LeaningIn);
            beats.Add(ActionBeat.GenuineSmile);
        }
        else if (state == EmotionalState.TENSE)
        {
            beats.Add(ActionBeat.StiffPosture);
        }
        
        return beats;
    }

    private DialogueType DetermineDialogueType(EmotionalState state, NPCEmotionalState npcState, CardPlayResult lastResult)
    {
        if (lastResult?.StateChanged == true)
            return DialogueType.StateReaction;
        
        if (npcState == NPCEmotionalState.DESPERATE)
            return DialogueType.Pleading;
        
        if (npcState == NPCEmotionalState.HOSTILE)
            return DialogueType.Accusatory;
        
        if (state == EmotionalState.CONNECTED)
            return DialogueType.Confiding;
        
        if (state == EmotionalState.OPEN)
            return DialogueType.Sharing;
        
        return DialogueType.Conversational;
    }

    private DialogueUrgency CalculateDialogueUrgency(NPCEmotionalState npcState)
    {
        return npcState switch
        {
            NPCEmotionalState.DESPERATE => DialogueUrgency.Desperate,
            NPCEmotionalState.ANXIOUS => DialogueUrgency.Pressing,
            NPCEmotionalState.HOSTILE => DialogueUrgency.Angry,
            _ => DialogueUrgency.Normal
        };
    }

    private EmotionalColoring DetermineEmotionalColoring(EmotionalState state, PersonalityType personality)
    {
        return (state, personality) switch
        {
            (EmotionalState.DESPERATE, _) => EmotionalColoring.Frantic,
            (EmotionalState.CONNECTED, PersonalityType.DEVOTED) => EmotionalColoring.Tender,
            (EmotionalState.CONNECTED, PersonalityType.MERCANTILE) => EmotionalColoring.Respectful,
            (EmotionalState.TENSE, _) => EmotionalColoring.Strained,
            (EmotionalState.EAGER, _) => EmotionalColoring.Excited,
            _ => EmotionalColoring.Neutral
        };
    }

    private LocationContextCategory GetLocationContext()
    {
        // This would get actual location context
        // For now, return a default
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        
        return currentTime switch
        {
            TimeBlocks.Dawn => LocationContextCategory.QuietMorning,
            TimeBlocks.Morning => LocationContextCategory.BusyDay,
            TimeBlocks.Afternoon => LocationContextCategory.Crowded,
            TimeBlocks.Evening => LocationContextCategory.WindingDown,
            TimeBlocks.Night => LocationContextCategory.Intimate,
            TimeBlocks.LateNight => LocationContextCategory.Secretive,
            _ => LocationContextCategory.Neutral
        };
    }
}

/// <summary>
/// Context for generating narrative (categories only, no text)
/// </summary>
public class ConversationNarrativeContext
{
    public NPCEmotionalState NPCState { get; set; }
    public EmotionalState ConversationState { get; set; }
    public PressureLevel Pressure { get; set; }
    public StakeType? UrgentStakes { get; set; }
    public int? HoursToDeadline { get; set; }
    public RelationshipDepth RelationshipDepth { get; set; }
    public SceneTone SceneTone { get; set; }
    public List<ActionBeat> ActionBeats { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public LocationContextCategory LocationContext { get; set; }
}

/// <summary>
/// Context for generating dialogue (categories only, no text)
/// </summary>
public class DialogueContext
{
    public DialogueType Type { get; set; }
    public DialogueUrgency Urgency { get; set; }
    public EmotionalColoring EmotionalColoring { get; set; }
    public PersonalityType Personality { get; set; }
    public NPCEmotionalState NPCState { get; set; }
    public EmotionalState ConversationState { get; set; }
    public bool ResponseToLastAction { get; set; }
}

// Categorical enums (NO text values)
public enum PressureLevel { None, Low, Moderate, High, Extreme }
public enum RelationshipDepth { Stranger, Acquaintance, Friendly, Close, Intimate }
public enum SceneTone { Neutral, Warm, Intimate, Tense, Urgent, Crisis, Confrontational, Cautious }
public enum ActionBeat 
{ 
    HandsTrembling, EyesDarting, ShiftingWeight, ForcedSmile, 
    ArmsCrossed, ColdStare, LeaningIn, GenuineSmile, StiffPosture,
    ClutchingLetter, GlancingAtDoor, CountingCoins, WhisperingUrgently
}
public enum DialogueType { Conversational, Sharing, Confiding, Pleading, Accusatory, StateReaction }
public enum DialogueUrgency { Normal, Pressing, Desperate, Angry }
public enum EmotionalColoring { Neutral, Tender, Respectful, Strained, Excited, Frantic }
public enum LocationContextCategory 
{ 
    Neutral, QuietMorning, BusyDay, Crowded, 
    WindingDown, Intimate, Secretive 
}