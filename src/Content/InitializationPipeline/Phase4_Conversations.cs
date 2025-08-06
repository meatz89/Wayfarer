using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Phase 4: Load conversation definitions from JSON files
/// Dependencies: NPCs must exist (Phase 3)
/// </summary>
public class Phase4_Conversations : IInitializationPhase
{
    public int PhaseNumber => 4;
    public string Name => "Conversation Loading";
    public bool IsCritical => false; // Conversations are optional content

    public void Execute(InitializationContext context)
    {
        Console.WriteLine("Loading conversation definitions...");

        // Store conversations in shared data for other systems to access
        Dictionary<string, ConversationDefinition> conversations = new Dictionary<string, ConversationDefinition>();

        // Load conversations from JSON files
        string conversationsPath = Path.Combine(context.ContentPath, "..", "Conversations");
        if (Directory.Exists(conversationsPath))
        {
            string[] conversationFiles = Directory.GetFiles(conversationsPath, "*.json");
            Console.WriteLine($"Found {conversationFiles.Length} conversation files");

            foreach (string file in conversationFiles)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    ConversationDefinition? conversation = JsonConvert.DeserializeObject<ConversationDefinition>(json);

                    if (conversation != null && !string.IsNullOrEmpty(conversation.ConversationId))
                    {
                        conversations[conversation.ConversationId] = conversation;
                        Console.WriteLine($"  - Loaded conversation: {conversation.ConversationId} for NPC: {conversation.NpcId}");

                        // Store NPC-specific dialogue mapping
                        if (!string.IsNullOrEmpty(conversation.NpcId))
                        {
                            Dictionary<string, string> npcDialogues = context.SharedData.GetOrCreate<Dictionary<string, string>>("NpcDialogues");
                            npcDialogues[conversation.NpcId] = conversation.ConversationId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    context.Warnings.Add($"Failed to load conversation from {Path.GetFileName(file)}: {ex.Message}");
                }
            }
        }
        else
        {
            context.Warnings.Add($"Conversations directory not found at: {conversationsPath}");
        }

        // Store loaded conversations for other phases/services
        context.SharedData["Conversations"] = conversations;
        Console.WriteLine($"Loaded {conversations.Count} conversations total");
    }
}

/// <summary>
/// Represents a conversation loaded from JSON
/// </summary>
public class ConversationDefinition
{
    public string ConversationId { get; set; }
    public string NpcId { get; set; }
    public string InitialNode { get; set; }
    public Dictionary<string, ConversationNode> Nodes { get; set; }
}

/// <summary>
/// Represents a node in a conversation tree
/// </summary>
public class ConversationNode
{
    public string Text { get; set; }
    public List<ConversationChoiceDefinition> Choices { get; set; }
    public bool IsEnd { get; set; }
    public ConversationEffects Effects { get; set; }
}

/// <summary>
/// Represents a player choice in a conversation definition loaded from JSON
/// </summary>
public class ConversationChoiceDefinition
{
    public string Id { get; set; }
    public string Text { get; set; }
    public string NextNode { get; set; }
    public int AttentionCost { get; set; }
}

/// <summary>
/// Effects that occur when reaching a conversation node
/// </summary>
public class ConversationEffects
{
    public AddLetterEffect AddLetter { get; set; }
    public AddTokenEffect AddToken { get; set; }
}

public class AddLetterEffect
{
    public string LetterId { get; set; }
    public int Payment { get; set; }
    public int Deadline { get; set; }
}

public class AddTokenEffect
{
    public string NpcId { get; set; }
    public string TokenType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Extension method to get or create dictionary entries
/// </summary>
public static class DictionaryExtensions
{
    public static T GetOrCreate<T>(this Dictionary<string, object> dict, string key) where T : new()
    {
        if (!dict.ContainsKey(key))
        {
            dict[key] = new T();
        }
        return (T)dict[key];
    }
}