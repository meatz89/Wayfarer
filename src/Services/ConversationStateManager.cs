using Wayfarer.GameState;

/// <summary>
/// Manages conversation state separately from GameWorld to maintain clean architecture.
/// GameWorld should only contain pure game state, not UI flow state.
/// </summary>
public class ConversationStateManager
{
    private ConversationManager _pendingConversationManager;
    private bool _conversationPending;

    /// <summary>
    /// Gets or sets the pending conversation manager.
    /// </summary>
    public ConversationManager PendingConversationManager
    {
        get
        {
            return _pendingConversationManager;
        }

        set
        {
            _pendingConversationManager = value;
        }
    }

    /// <summary>
    /// Gets or sets whether a conversation is pending.
    /// </summary>
    public bool ConversationPending
    {
        get
        {
            return _conversationPending;
        }

        set
        {
            _conversationPending = value;
        }
    }

    /// <summary>
    /// Clears the pending conversation state.
    /// </summary>
    public void ClearPendingConversation()
    {
        _pendingConversationManager = null;
        _conversationPending = false;
    }

    /// <summary>
    /// Sets a pending conversation.
    /// </summary>
    public void SetPendingConversation(ConversationManager conversationManager)
    {
        Console.WriteLine($"[ConversationStateManager] SetPendingConversation called. Manager null? {conversationManager == null}");
        _pendingConversationManager = conversationManager;
        _conversationPending = true;
        Console.WriteLine($"[ConversationStateManager] ConversationPending = {_conversationPending}");
    }

    /// <summary>
    /// Sets the current conversation (alias for SetPendingConversation).
    /// </summary>
    public void SetCurrentConversation(ConversationManager conversationManager)
    {
        SetPendingConversation(conversationManager);
    }


}