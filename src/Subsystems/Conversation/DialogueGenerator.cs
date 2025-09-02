using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Generates dialogue and narrative text for conversations.
/// Creates NPC responses, descriptions, and emotional cues.
/// </summary>
public class DialogueGenerator
{
    private readonly Random _random;

    public DialogueGenerator()
    {
        _random = new Random();
    }

    /// <summary>
    /// Generate NPC response for LISTEN action
    /// </summary>
    public string GenerateListenResponse(NPC npc, EmotionalState state, List<CardInstance> drawnCards)
    {
        StringBuilder response = new StringBuilder();

        // Add emotional cue
        response.AppendLine(GenerateEmotionalCue(npc, state));

        // Describe drawn cards narratively
        if (drawnCards.Any())
        {
            response.AppendLine(GenerateCardDrawDescription(npc, drawnCards, state));
        }

        // Add state-specific dialogue
        response.AppendLine(GenerateStateDialogue(npc, state, "listen"));

        return response.ToString().Trim();
    }

    /// <summary>
    /// Generate NPC response for SPEAK action
    /// </summary>
    public string GenerateSpeakResponse(NPC npc, EmotionalState state, HashSet<CardInstance> playedCards, CardPlayResult result, int comfortChange)
    {
        StringBuilder response = new StringBuilder();

        // React to played cards
        response.AppendLine(GenerateCardReaction(npc, playedCards, state));

        // Describe comfort change
        if (comfortChange != 0)
        {
            response.AppendLine(GenerateComfortChangeDescription(npc, comfortChange, state));
        }

        // Add state transition if occurred
        if (result.NewState != state)
        {
            response.AppendLine(GenerateStateTransitionText(npc, state, result.NewState ?? state));
        }

        // Add bonus descriptions
        if (result.SetBonus > 0)
        {
            response.AppendLine($"{npc.Name} appreciates the thoughtful combination of topics.");
        }

        return response.ToString().Trim();
    }

    /// <summary>
    /// Generate action description for UI
    /// </summary>
    public string GenerateActionDescription(ConversationAction action, NPC npc)
    {
        return action.ActionType switch
        {
            ActionType.Listen => $"Listen to what {npc.Name} has to say",
            ActionType.Speak => "Share your thoughts",
            ActionType.None => "End the conversation",
            _ => "Continue"
        };
    }

    /// <summary>
    /// Generate state transition narrative
    /// </summary>
    public string GenerateStateTransitionText(NPC npc, EmotionalState fromState, EmotionalState toState)
    {
        bool isPositive = IsPositiveTransition(fromState, toState);

        if (isPositive)
        {
            return toState switch
            {
                EmotionalState.CONNECTED => $"{npc.Name}'s eyes light up with genuine warmth. You've formed a real connection.",
                EmotionalState.OPEN => $"{npc.Name} leans forward with interest, eager to continue the conversation.",
                EmotionalState.NEUTRAL => $"{npc.Name}'s guard drops slightly as they settle into the conversation.",
                EmotionalState.TENSE => $"{npc.Name} seems less hostile, though still wary.",
                _ => $"{npc.Name}'s demeanor shifts."
            };
        }
        else
        {
            return toState switch
            {
                EmotionalState.DESPERATE => $"{npc.Name}'s expression hardens with anger.",
                EmotionalState.TENSE => $"{npc.Name} pulls back, becoming more guarded.",
                EmotionalState.NEUTRAL => $"{npc.Name} returns to a neutral stance.",
                EmotionalState.OPEN => $"{npc.Name} relaxes noticeably, becoming more open to discussion.",
                _ => $"{npc.Name}'s mood shifts."
            };
        }
    }

    /// <summary>
    /// Generate card play narrative
    /// </summary>
    public string GenerateCardPlayText(CardInstance card, NPC npc)
    {
        // Use card's description if available
        if (!string.IsNullOrEmpty(card.Description))
        {
            return card.Description.Replace("{npc}", npc.Name);
        }

        // Generate based on card properties
        if (card.Properties.Contains(CardProperty.Observable))
        {
            return $"You share an observation with {npc.Name}.";
        }
        else if (card.Properties.Contains(CardProperty.Fleeting) && card.Properties.Contains(CardProperty.Opportunity))
        {
            return $"You make an important request to {npc.Name}.";
        }
        else if (card.Properties.Contains(CardProperty.Exchange))
        {
            return $"You make a trade offer to {npc.Name}.";
        }
        else
        {
            return $"You continue talking with {npc.Name}.";
        }
    }

    /// <summary>
    /// Generate emotional cues (body language)
    /// </summary>
    public string GenerateEmotionalCues(NPC npc, EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => $"{npc.Name} wrings their hands anxiously, eyes darting about.",
            EmotionalState.TENSE => $"{npc.Name} shifts uncomfortably, shoulders rigid.",
            EmotionalState.NEUTRAL => $"{npc.Name} stands relaxed but attentive.",
            EmotionalState.OPEN => $"{npc.Name} leans in slightly, genuinely interested.",
            EmotionalState.CONNECTED => $"{npc.Name} mirrors your posture, completely engaged.",
            _ => $"{npc.Name} continues the conversation."
        };
    }

    /// <summary>
    /// Generate exchange accepted response
    /// </summary>
    public string GenerateExchangeAcceptedResponse(NPC npc, ExchangeData exchange)
    {
        string[] responses = new[]
        {
            $"{npc.Name} nods and completes the exchange.",
            $"\"Fair deal,\" {npc.Name} says, making the trade.",
            $"{npc.Name} accepts your offer with a brief smile.",
            $"\"Agreed,\" says {npc.Name}, finalizing the exchange."
        };

        return responses[_random.Next(responses.Length)];
    }

    /// <summary>
    /// Generate exchange declined response
    /// </summary>
    public string GenerateExchangeDeclinedResponse(NPC npc)
    {
        string[] responses = new[]
        {
            $"{npc.Name} shakes their head. \"Perhaps another time.\"",
            $"\"I'll pass,\" {npc.Name} says politely.",
            $"{npc.Name} declines with a apologetic gesture.",
            $"\"Not interested right now,\" says {npc.Name}."
        };

        return responses[_random.Next(responses.Length)];
    }

    // Private helper methods

    private string GenerateEmotionalCue(NPC npc, EmotionalState state)
    {
        return GenerateEmotionalCues(npc, state);
    }

    private string GenerateCardDrawDescription(NPC npc, List<CardInstance> cards, EmotionalState state)
    {
        if (cards.Count == 1)
        {
            return $"{npc.Name} brings up {GetCardTopic(cards[0])}.";
        }
        else
        {
            return $"{npc.Name} touches on several topics.";
        }
    }

    private string GetCardTopic(CardInstance card)
    {
        if (card.Properties.Contains(CardProperty.Observable))
        {
            return "your observations";
        }
        else if (card.Properties.Contains(CardProperty.Fleeting) && card.Properties.Contains(CardProperty.Opportunity))
        {
            return "an urgent matter";
        }
        else if (card.Properties.Contains(CardProperty.Exchange))
        {
            return "a trade proposal";
        }
        else
        {
            return "something important";
        }
    }

    private string GenerateStateDialogue(NPC npc, EmotionalState state, string context)
    {
        // Generate contextual dialogue based on state
        string[] dialogueOptions = GetStateDialogueOptions(state, context);
        if (dialogueOptions.Any())
        {
            string dialogue = dialogueOptions[_random.Next(dialogueOptions.Length)];
            return $"\"{dialogue}\" says {npc.Name}.";
        }

        return "";
    }

    private string[] GetStateDialogueOptions(EmotionalState state, string context)
    {
        return state switch
        {
            EmotionalState.DESPERATE => new[] { "Please, I need help!", "This is urgent!", "I don't know what to do!" },
            EmotionalState.TENSE => new[] { "What do you want?", "I'm listening...", "Be quick about it." },
            EmotionalState.NEUTRAL => new[] { "Interesting.", "Tell me more.", "I see." },
            EmotionalState.OPEN => new[] { "That's fascinating!", "Please continue.", "I'd like to hear more." },
            EmotionalState.CONNECTED => new[] { "I understand completely.", "We're on the same wavelength.", "I feel the same way." },
            _ => new[] { "..." }
        };
    }

    private string GenerateCardReaction(NPC npc, HashSet<CardInstance> cards, EmotionalState state)
    {
        int cardCount = cards.Count;
        // Determine primary card property for reaction
        var hasObservation = cards.Any(c => c.Properties.Contains(CardProperty.Observable));
        var hasGoal = cards.Any(c => c.Properties.Contains(CardProperty.Fleeting) && c.Properties.Contains(CardProperty.Opportunity));
        var hasExchange = cards.Any(c => c.Properties.Contains(CardProperty.Exchange));

        if (cardCount == 1)
        {
            return $"{npc.Name} considers your words carefully.";
        }
        else if (cardCount >= 3)
        {
            return $"{npc.Name} listens intently to your detailed thoughts.";
        }
        else
        {
            return $"{npc.Name} nods as you speak.";
        }
    }

    private string GenerateComfortChangeDescription(NPC npc, int change, EmotionalState state)
    {
        if (change > 0)
        {
            return change switch
            {
                >= 5 => $"{npc.Name} seems much more comfortable.",
                >= 3 => $"{npc.Name} relaxes visibly.",
                _ => $"{npc.Name} appears slightly more at ease."
            };
        }
        else
        {
            return change switch
            {
                <= -5 => $"{npc.Name} becomes very uncomfortable.",
                <= -3 => $"{npc.Name} tenses up.",
                _ => $"{npc.Name} seems slightly uneasy."
            };
        }
    }

    private bool IsPositiveTransition(EmotionalState from, EmotionalState to)
    {
        EmotionalState[] stateOrder = new[]
        {
            EmotionalState.DESPERATE,
            EmotionalState.TENSE,
            EmotionalState.NEUTRAL,
            EmotionalState.OPEN,
            EmotionalState.CONNECTED
        };

        int fromIndex = Array.IndexOf(stateOrder, from);
        int toIndex = Array.IndexOf(stateOrder, to);

        return toIndex > fromIndex;
    }
}