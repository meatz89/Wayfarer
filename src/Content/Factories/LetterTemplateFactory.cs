using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating letter templates with guaranteed valid references.
/// Letter templates reference NPCs as possible senders/recipients.
/// </summary>
public class LetterTemplateFactory
{
    public LetterTemplateFactory()
    {
        // No dependencies - factory is stateless
    }
    
    /// <summary>
    /// Create a letter template with validated references
    /// </summary>
    public LetterTemplate CreateLetterTemplate(
        string id,
        string description,
        ConnectionType tokenType,
        int minDeadline,
        int maxDeadline,
        int minPayment,
        int maxPayment,
        LetterCategory category = LetterCategory.Basic,
        int minTokensRequired = 3,
        IEnumerable<NPC> possibleSenders = null,
        IEnumerable<NPC> possibleRecipients = null,
        IEnumerable<string> unlocksLetterIds = null,
        bool isChainLetter = false,
        LetterSize size = LetterSize.Medium,
        LetterPhysicalProperties physicalProperties = LetterPhysicalProperties.None,
        ItemCategory? requiredEquipment = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Letter template ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(description))
            throw new ArgumentException("Letter template description cannot be empty", nameof(description));
        if (minDeadline < 1)
            throw new ArgumentException("Minimum deadline must be at least 1 day", nameof(minDeadline));
        if (maxDeadline < minDeadline)
            throw new ArgumentException("Maximum deadline must be greater than or equal to minimum deadline", nameof(maxDeadline));
        if (minPayment < 0)
            throw new ArgumentException("Minimum payment cannot be negative", nameof(minPayment));
        if (maxPayment < minPayment)
            throw new ArgumentException("Maximum payment must be greater than or equal to minimum payment", nameof(maxPayment));
        
        var template = new LetterTemplate
        {
            Id = id,
            Description = description,
            TokenType = tokenType,
            MinDeadline = minDeadline,
            MaxDeadline = maxDeadline,
            MinPayment = minPayment,
            MaxPayment = maxPayment,
            Category = category,
            MinTokensRequired = minTokensRequired,
            PossibleSenders = possibleSenders?.Select(npc => npc.ID).ToArray() ?? new string[0],
            PossibleRecipients = possibleRecipients?.Select(npc => npc.ID).ToArray() ?? new string[0],
            UnlocksLetterIds = unlocksLetterIds?.ToArray() ?? new string[0],
            IsChainLetter = isChainLetter,
            Size = size,
            PhysicalProperties = physicalProperties,
            RequiredEquipment = requiredEquipment
        };
        
        return template;
    }
    
    /// <summary>
    /// Create a letter template from string IDs with validation
    /// </summary>
    public LetterTemplate CreateLetterTemplateFromIds(
        string id,
        string description,
        ConnectionType tokenType,
        int minDeadline,
        int maxDeadline,
        int minPayment,
        int maxPayment,
        LetterCategory category,
        int minTokensRequired,
        IEnumerable<string> possibleSenderIds,
        IEnumerable<string> possibleRecipientIds,
        IEnumerable<NPC> availableNPCs,
        IEnumerable<string> unlocksLetterIds = null,
        bool isChainLetter = false,
        LetterSize size = LetterSize.Medium,
        LetterPhysicalProperties physicalProperties = LetterPhysicalProperties.None,
        ItemCategory? requiredEquipment = null)
    {
        // Validate senders
        var senders = new List<NPC>();
        if (possibleSenderIds != null)
        {
            foreach (var senderId in possibleSenderIds)
            {
                var npc = availableNPCs.FirstOrDefault(n => n.ID == senderId);
                if (npc == null)
                {
                    Console.WriteLine($"WARNING: Letter template '{id}' references non-existent sender NPC '{senderId}'");
                }
                else
                {
                    senders.Add(npc);
                }
            }
        }
        
        // Validate recipients
        var recipients = new List<NPC>();
        if (possibleRecipientIds != null)
        {
            foreach (var recipientId in possibleRecipientIds)
            {
                var npc = availableNPCs.FirstOrDefault(n => n.ID == recipientId);
                if (npc == null)
                {
                    Console.WriteLine($"WARNING: Letter template '{id}' references non-existent recipient NPC '{recipientId}'");
                }
                else
                {
                    recipients.Add(npc);
                }
            }
        }
        
        return CreateLetterTemplate(id, description, tokenType, minDeadline, maxDeadline, 
                                   minPayment, maxPayment, category, minTokensRequired,
                                   senders, recipients, unlocksLetterIds, isChainLetter,
                                   size, physicalProperties, requiredEquipment);
    }
}