using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates contextual action beats for conversations
/// These are the physical gestures and environmental details that punctuate dialogue
/// </summary>
public class ActionBeatGenerator
{
    private readonly Random _random = new Random();
    
    /// <summary>
    /// Generate an action beat based on NPC state and conversation context
    /// </summary>
    public string GenerateActionBeat(
        NPCEmotionalState emotionalState,
        StakeType? stakes,
        int conversationTurn,
        bool isUrgent)
    {
        // For early turns, use establishing beats
        if (conversationTurn <= 1)
        {
            return GenerateEstablishingBeat(emotionalState);
        }
        
        // For mid-conversation, use emotional beats
        if (conversationTurn <= 3)
        {
            return GenerateEmotionalBeat(emotionalState, stakes, isUrgent);
        }
        
        // For late conversation, use closing beats
        return GenerateClosingBeat(emotionalState);
    }
    
    private string GenerateEstablishingBeat(NPCEmotionalState state)
    {
        var beats = state switch
        {
            NPCEmotionalState.DESPERATE => new[]
            {
                "Their hands clutch at the edge of the table",
                "They lean forward, eyes wide with urgency",
                "Their voice drops to an urgent whisper"
            },
            NPCEmotionalState.HOSTILE => new[]
            {
                "They cross their arms, jaw clenched tight",
                "Their eyes narrow to suspicious slits",
                "They lean back, creating distance between you"
            },
            NPCEmotionalState.CALCULATING => new[]
            {
                "They steeple their fingers thoughtfully",
                "Their gaze becomes appraising",
                "They pause, weighing their words carefully"
            },
            NPCEmotionalState.WITHDRAWN => new[]
            {
                "They barely meet your eyes",
                "Their shoulders slump with disinterest",
                "They glance toward the door"
            },
            _ => new[] { "They wait for you to speak" }
        };
        
        return beats[_random.Next(beats.Length)];
    }
    
    private string GenerateEmotionalBeat(
        NPCEmotionalState state,
        StakeType? stakes,
        bool isUrgent)
    {
        if (isUrgent && state == NPCEmotionalState.DESPERATE)
        {
            return stakes switch
            {
                StakeType.SAFETY => "Their hand reaches toward yours across the table, trembling slightly",
                StakeType.WEALTH => "They pull out a worn purse, coins clinking desperately",
                StakeType.REPUTATION => "Their voice cracks, pride warring with necessity",
                StakeType.SECRET => "They glance nervously at the other patrons",
                _ => "Their breathing quickens with mounting panic"
            };
        }
        
        if (state == NPCEmotionalState.HOSTILE)
        {
            return stakes switch
            {
                StakeType.SAFETY => "Their hand moves to their belt, a subtle threat",
                StakeType.WEALTH => "They slam a fist on the table, coins jumping",
                StakeType.REPUTATION => "Their lip curls in barely concealed contempt",
                StakeType.SECRET => "They lean in close, voice dripping with menace",
                _ => "Their patience visibly frays"
            };
        }
        
        if (state == NPCEmotionalState.CALCULATING)
        {
            return stakes switch
            {
                StakeType.WEALTH => "They slide a few coins across the table, testing",
                StakeType.REPUTATION => "They study your face for signs of weakness",
                StakeType.SECRET => "Their smile doesn't reach their eyes",
                _ => "They wait, letting silence do their work"
            };
        }
        
        // Default withdrawn
        return "They shift uncomfortably in their seat";
    }
    
    private string GenerateClosingBeat(NPCEmotionalState state)
    {
        var beats = state switch
        {
            NPCEmotionalState.DESPERATE => new[]
            {
                "They grip your sleeve as you prepare to leave",
                "Tears threaten at the corners of their eyes",
                "They follow you halfway to the door"
            },
            NPCEmotionalState.HOSTILE => new[]
            {
                "They turn away dismissively",
                "Their parting look promises consequences",
                "They mutter something under their breath"
            },
            NPCEmotionalState.CALCULATING => new[]
            {
                "They nod once, transaction complete",
                "A knowing smile plays at their lips",
                "They're already planning their next move"
            },
            NPCEmotionalState.WITHDRAWN => new[]
            {
                "They've already forgotten you exist",
                "Their attention drifts to the window",
                "They return to their previous occupation"
            },
            _ => new[] { "The conversation ends" }
        };
        
        return beats[_random.Next(beats.Length)];
    }
    
    /// <summary>
    /// Generate environmental action beats based on location
    /// </summary>
    public string GenerateEnvironmentalBeat(string locationId, int hour)
    {
        var beats = locationId switch
        {
            "market_square" when hour < 12 => new[]
            {
                "Morning crowds jostle past your conversation",
                "A merchant's cry momentarily drowns out their words",
                "The smell of fresh bread wafts between you"
            },
            "market_square" => new[]
            {
                "The afternoon sun beats down mercilessly",
                "Guards patrol past, eyeing you both",
                "The crowd thins as shops begin to close"
            },
            "noble_district" => new[]
            {
                "A servant discretely averts their gaze",
                "Somewhere, a bell tower chimes the hour",
                "Fine curtains twitch in nearby windows"
            },
            "riverside" => new[]
            {
                "The sound of lapping water fills the silence",
                "A gull cries overhead",
                "Dock workers pause to watch your exchange"
            },
            "merchant_row" => new[]
            {
                "Coins change hands at nearby stalls",
                "The air smells of spices and ambition",
                "A deal concludes with a handshake nearby"
            },
            _ => new[]
            {
                "The world continues around you",
                "Time seems to slow for this moment",
                "Everything else fades to background"
            }
        };
        
        return beats[_random.Next(beats.Length)];
    }
}