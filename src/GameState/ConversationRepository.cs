using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Repository for storing and retrieving conversation definitions loaded during initialization
/// </summary>
public class ConversationRepository
{
    private readonly Dictionary<string, ConversationDefinition> _conversations;
    private readonly Dictionary<string, string> _npcToConversation;
    
    public ConversationRepository()
    {
        _conversations = new Dictionary<string, ConversationDefinition>();
        _npcToConversation = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Initialize repository with conversations loaded during game initialization
    /// </summary>
    public void Initialize(Dictionary<string, ConversationDefinition> conversations, Dictionary<string, string> npcDialogues)
    {
        _conversations.Clear();
        _npcToConversation.Clear();
        
        if (conversations != null)
        {
            foreach (var kvp in conversations)
            {
                _conversations[kvp.Key] = kvp.Value;
            }
        }
        
        if (npcDialogues != null)
        {
            foreach (var kvp in npcDialogues)
            {
                _npcToConversation[kvp.Key] = kvp.Value;
            }
        }
    }
    
    /// <summary>
    /// Get conversation by ID
    /// </summary>
    public ConversationDefinition GetConversation(string conversationId)
    {
        return _conversations.TryGetValue(conversationId, out var conversation) ? conversation : null;
    }
    
    /// <summary>
    /// Get conversation for a specific NPC
    /// </summary>
    public ConversationDefinition GetConversationForNpc(string npcId)
    {
        if (_npcToConversation.TryGetValue(npcId, out var conversationId))
        {
            return GetConversation(conversationId);
        }
        return null;
    }
    
    /// <summary>
    /// Check if NPC has a special conversation
    /// </summary>
    public bool HasConversation(string npcId)
    {
        return _npcToConversation.ContainsKey(npcId);
    }
    
    /// <summary>
    /// Get the introductory text for an NPC's conversation
    /// </summary>
    public string GetNpcIntroduction(string npcId)
    {
        var conversation = GetConversationForNpc(npcId);
        if (conversation?.Nodes != null && !string.IsNullOrEmpty(conversation.InitialNode))
        {
            if (conversation.Nodes.TryGetValue(conversation.InitialNode, out var node))
            {
                return node.Text;
            }
        }
        return null;
    }
}