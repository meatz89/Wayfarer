public class ConversationHistoryManager
{
    private Dictionary<string, List<ConversationEntry>> _fullConversationHistories = new();

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

    public void AddUserMessage(string conversationId, string message, MessageType type)
    {
        _fullConversationHistories[conversationId].Add(new ConversationEntry
        {
            Role = "user",
            Content = message,
            Type = type,
        });
    }

    public void AddUserChoiceSelectionMessage(string conversationId, string message, string selectedChoice)
    {
        _fullConversationHistories[conversationId].Add(new ConversationEntry
        {
            Role = "user",
            Content = message,
            Type = MessageType.PlayerChoice,
            SelectedChoice = selectedChoice
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

    public List<ConversationEntry> GetConversationHistory(string conversationId)
    {
        List<ConversationEntry> fullHistory = _fullConversationHistories[conversationId];
        List<ConversationEntry> optimizedHistory = new List<ConversationEntry>();

        ConversationEntry? systemMessage = fullHistory.FirstOrDefault(m =>
        {
            return m.Type == MessageType.System;
        });
        if (systemMessage != null)
        {
            optimizedHistory.Add(new ConversationEntry { Role = systemMessage.Role, Content = systemMessage.Content });
        }

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

        for (int i = 0; i < fullHistory.Count; i++)
        {
            ConversationEntry entry = fullHistory[i];

            if (entry.Type == MessageType.ChoicesGeneration)
                continue;

            if (entry.Type == MessageType.MemoryUpdate)
                continue;

            if (entry.Type == MessageType.PostEncounterEvolution)
                continue;

            if (entry.Type == MessageType.PlayerChoice && entry.Role == "user")
            {
                string simplifiedChoice = SimplifyPlayerChoicePrompt(entry.Content, entry.SelectedChoice!);
                optimizedHistory.Add(new ConversationEntry { Role = entry.Role, Content = simplifiedChoice });
            }

            else if (entry.Type == MessageType.Reaction && entry.Role == "assistant")
            {
                optimizedHistory.Add(new ConversationEntry { Role = entry.Role, Content = entry.Content });
            }
        }

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

    private string SimplifyPlayerChoicePrompt(string fullPrompt, string choiceDescription)
    {
        if (choiceDescription != null)
        {
            return $"The Player chose: '{choiceDescription}'. Generate the narrative response.";
        }

        return "Player made a choice. Generate narrative response.";
    }
}