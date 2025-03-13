

// Manages conversation history and context
public class NarrativeContextManager
{
    private readonly Dictionary<string, List<ConversationEntry>> _conversationHistories = new();

    public void InitializeConversation(string conversationId, string systemMessage, string userMessage)
    {
        List<ConversationEntry> history = new()
        {
            new ConversationEntry { Role = "system", Content = systemMessage },
            new ConversationEntry { Role = "user", Content = userMessage }
        };

        _conversationHistories[conversationId] = history;
    }

    public bool ConversationExists(string conversationId)
    {
        return _conversationHistories.ContainsKey(conversationId);
    }

    public void UpdateSystemMessage(string conversationId, string systemMessage)
    {
        List<ConversationEntry> history = _conversationHistories[conversationId];

        if (history.Count > 0 && history[0].Role == "system")
        {
            history[0] = new ConversationEntry { Role = "system", Content = systemMessage };
        }
        else
        {
            history.Insert(0, new ConversationEntry { Role = "system", Content = systemMessage });
        }
    }

    public void AddUserMessage(string conversationId, string message)
    {
        _conversationHistories[conversationId].Add(new ConversationEntry { Role = "user", Content = message });
    }

    public void AddAssistantMessage(string conversationId, string message)
    {
        _conversationHistories[conversationId].Add(new ConversationEntry { Role = "assistant", Content = message });
    }

    public List<ConversationEntry> GetConversationHistory(string conversationId)
    {
        return _conversationHistories[conversationId];
    }
}
