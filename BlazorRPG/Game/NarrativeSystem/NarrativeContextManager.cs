public class NarrativeContextManager
{
    // Store the full history for record-keeping
    private readonly Dictionary<string, List<ConversationEntry>> _fullConversationHistories = new();

    // Initialize a conversation with system message and introduction
    public void InitializeConversation(string conversationId, string systemMessage, string userMessage)
    {
        List<ConversationEntry> history = new()
        {
            new ConversationEntry { Role = "system", Content = systemMessage, Type = MessageType.System },
            new ConversationEntry { Role = "user", Content = userMessage, Type = MessageType.Introduction }
        };
        _fullConversationHistories[conversationId] = history;
    }

    public bool ConversationExists(string conversationId)
    {
        return _fullConversationHistories.ContainsKey(conversationId);
    }

    public void UpdateSystemMessage(string conversationId, string systemMessage)
    {
        List<ConversationEntry> history = _fullConversationHistories[conversationId];
        if (history.Count > 0 && history[0].Role == "system")
        {
            history[0] = new ConversationEntry { Role = "system", Content = systemMessage, Type = MessageType.System };
        }
        else
        {
            history.Insert(0, new ConversationEntry { Role = "system", Content = systemMessage, Type = MessageType.System });
        }
    }

    // Add a message with type tracking
    public void AddUserMessage(string conversationId, string message, MessageType type, string choiceDescription)
    {
        _fullConversationHistories[conversationId].Add(new ConversationEntry
        {
            Role = "user",
            Content = message,
            Type = type,
            choiceDescription = choiceDescription
        });
    }

    public void AddAssistantMessage(string conversationId, string message, MessageType type)
    {
        _fullConversationHistories[conversationId].Add(new ConversationEntry
        {
            Role = "assistant",
            Content = message,
            Type = type
        });
    }

    // Get optimized conversation history for AI calls
    public List<ConversationEntry> GetOptimizedConversationHistory(string conversationId)
    {
        List<ConversationEntry> fullHistory = _fullConversationHistories[conversationId];
        List<ConversationEntry> optimizedHistory = new List<ConversationEntry>();

        // Always include system message
        ConversationEntry? systemMessage = fullHistory.FirstOrDefault(m =>
        {
            return m.Type == MessageType.System;
        });
        if (systemMessage != null)
        {
            optimizedHistory.Add(new ConversationEntry { Role = systemMessage.Role, Content = systemMessage.Content });
        }

        // Always include introduction with memory and its response
        ConversationEntry? introPrompt = fullHistory.FirstOrDefault(m =>
        {
            return m.Type == MessageType.Introduction && m.Role == "user";
        });
        string simplifiedIntro = introPrompt!.Content;

        ConversationEntry? introResponse = fullHistory.FirstOrDefault(m =>
        {
            return m.Type == MessageType.Introduction && m.Role == "assistant";
        });

        if (introPrompt != null)
            optimizedHistory.Add(new ConversationEntry { Role = introPrompt.Role, Content = simplifiedIntro });

        if (introResponse != null)
            optimizedHistory.Add(new ConversationEntry { Role = introResponse.Role, Content = introResponse.Content });

        // For each player choice and narrative response pair
        for (int i = 0; i < fullHistory.Count; i++)
        {
            ConversationEntry entry = fullHistory[i];

            // Skip all choice generation prompts and responses
            if (entry.Type == MessageType.ChoiceGeneration)
                continue;

            // Skip all choice generation prompts and responses
            if (entry.Type == MessageType.MemoryUpdate)
                continue;

            // Skip all choice generation prompts and responses
            if (entry.Type == MessageType.PostEncounterEvolution)
                continue;

            // For player choices, simplify to just "Player chose X"
            if (entry.Type == MessageType.PlayerChoice && entry.Role == "user")
            {
                // Extract just the choice information
                string simplifiedChoice = SimplifyPlayerChoicePrompt(entry.Content, entry.choiceDescription!);
                optimizedHistory.Add(new ConversationEntry { Role = entry.Role, Content = simplifiedChoice });
            }

            // Include all narrative responses from the AI
            else if (entry.Type == MessageType.Narrative && entry.Role == "assistant")
            {
                optimizedHistory.Add(new ConversationEntry { Role = entry.Role, Content = entry.Content });
            }
        }

        // Include the current prompt we're about to send
        ConversationEntry? currentPrompt = fullHistory.LastOrDefault(m =>
        {
            return m.Role == "user";
        });
        if (currentPrompt != null)
        {
            // Don't add it again if it's already the last one we added
            ConversationEntry? lastAdded = optimizedHistory.LastOrDefault();
            if (lastAdded == null || lastAdded.Role != "user" || lastAdded.Content != currentPrompt.Content)
            {
                optimizedHistory.Add(new ConversationEntry { Role = currentPrompt.Role, Content = currentPrompt.Content });
            }
        }

        return optimizedHistory;
    }

    // Simplify player choice prompt to just "Player chose X"
    private string SimplifyPlayerChoicePrompt(string fullPrompt, string choiceDescription)
    {
        if (choiceDescription != null)
        {
            return $"The Player chose: '{choiceDescription}'. Generate the narrative response.";
        }

        // If we can't parse it properly, return a default simplified version
        return "Player made a choice. Generate narrative response.";
    }
}