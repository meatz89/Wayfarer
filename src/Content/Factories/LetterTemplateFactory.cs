using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating letter templates with guaranteed valid references.
/// DeliveryObligation templates reference NPCs as possible senders/recipients.
/// </summary>
public class LetterTemplateFactory
{
    public LetterTemplateFactory()
    {
        // No dependencies - factory is stateless
    }

    /// <summary>
    /// Create a minimal letter template with just an ID.
    /// Used for dummy/placeholder creation when references are missing.
    /// </summary>
    public LetterTemplate CreateMinimalLetterTemplate(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("DeliveryObligation template ID cannot be empty", nameof(id));

        string name = FormatIdAsName(id);

        return new LetterTemplate
        {
            Id = id,
            Description = $"A letter template called {name}",
            TokenType = ConnectionType.Trust, // Most basic type
            MinDeadlineInMinutes = 1440, // One day minimum (24*60)
            MaxDeadlineInMinutes = 2880, // Two days maximum (48*60)
            MinPayment = 2, // Low but reasonable
            MaxPayment = 5,
            Category = LetterCategory.Basic,
            MinTokensRequired = 0, // No token requirements
            PossibleSenders = new string[0],
            PossibleRecipients = new string[0],
            UnlocksLetterIds = new string[0],
            Size = SizeCategory.Small, // Easy to carry
            PhysicalProperties = LetterPhysicalProperties.None,
            RequiredEquipment = null,
            SpecialType = LetterSpecialType.None,
            SpecialTargetId = "",
            HumanContext = "A routine correspondence",
            ConsequenceIfLate = "The message arrives late",
            ConsequenceIfDelivered = "Communication reaches its destination",
            EmotionalWeight = EmotionalWeight.LOW,
            Stakes = StakeType.REPUTATION,
            TierLevel = TierLevel.T1
        };
    }

    private string FormatIdAsName(string id)
    {
        // Convert snake_case or kebab-case to Title Case
        return string.Join(" ",
            id.Replace('_', ' ').Replace('-', ' ')
              .Split(' ')
              .Select(word => string.IsNullOrEmpty(word) ? "" :
                  char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }

    /// <summary>
    /// Create a letter template with validated references
    /// </summary>
    public LetterTemplate CreateLetterTemplate(
        string id,
        string description,
        ConnectionType tokenType,
        int minDeadlineInHours,
        int maxDeadlineInHours,
        int minPayment,
        int maxPayment,
        LetterCategory category = LetterCategory.Basic,
        int minTokensRequired = 3,
        IEnumerable<NPC> possibleSenders = null,
        IEnumerable<NPC> possibleRecipients = null,
        IEnumerable<string> unlocksLetterIds = null,
        bool isChainDeliveryObligation = false,
        SizeCategory size = SizeCategory.Medium,
        LetterPhysicalProperties physicalProperties = LetterPhysicalProperties.None,
        ItemCategory? requiredEquipment = null,
        LetterSpecialType specialType = LetterSpecialType.None,
        string specialTargetId = null,
        string humanContext = null,
        string consequenceIfLate = null,
        string consequenceIfDelivered = null,
        EmotionalWeight emotionalWeight = EmotionalWeight.MEDIUM,
        StakeType stakes = StakeType.REPUTATION,
        TierLevel tierLevel = TierLevel.T1)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("DeliveryObligation template ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(description))
            throw new ArgumentException("DeliveryObligation template description cannot be empty", nameof(description));
        if (minDeadlineInHours < 1)
            throw new ArgumentException("Minimum deadline must be at least 1 hour", nameof(minDeadlineInHours));
        if (maxDeadlineInHours < minDeadlineInHours)
            throw new ArgumentException("Maximum deadline must be greater than or equal to minimum deadline", nameof(maxDeadlineInHours));
        if (minPayment < 0)
            throw new ArgumentException("Minimum payment cannot be negative", nameof(minPayment));
        if (maxPayment < minPayment)
            throw new ArgumentException("Maximum payment must be greater than or equal to minimum payment", nameof(maxPayment));

        LetterTemplate template = new LetterTemplate
        {
            Id = id,
            Description = description,
            TokenType = tokenType,
            MinDeadlineInMinutes = minDeadlineInHours,
            MaxDeadlineInMinutes = maxDeadlineInHours,
            MinPayment = minPayment,
            MaxPayment = maxPayment,
            Category = category,
            MinTokensRequired = minTokensRequired,
            PossibleSenders = possibleSenders?.Select(npc => npc.ID).ToArray() ?? new string[0],
            PossibleRecipients = possibleRecipients?.Select(npc => npc.ID).ToArray() ?? new string[0],
            UnlocksLetterIds = unlocksLetterIds?.ToArray() ?? new string[0],
            Size = size,
            PhysicalProperties = physicalProperties,
            RequiredEquipment = requiredEquipment,
            SpecialType = specialType,
            SpecialTargetId = specialTargetId ?? "",
            HumanContext = humanContext ?? "",
            ConsequenceIfLate = consequenceIfLate ?? "",
            ConsequenceIfDelivered = consequenceIfDelivered ?? "",
            EmotionalWeight = emotionalWeight,
            Stakes = stakes,
            TierLevel = tierLevel
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
        int minDeadlineInHours,
        int maxDeadlineInHours,
        int minPayment,
        int maxPayment,
        LetterCategory category,
        int minTokensRequired,
        IEnumerable<string> possibleSenderIds,
        IEnumerable<string> possibleRecipientIds,
        IEnumerable<NPC> availableNPCs,
        IEnumerable<string> unlocksLetterIds = null,
        bool isChainDeliveryObligation = false,
        SizeCategory size = SizeCategory.Medium,
        LetterPhysicalProperties physicalProperties = LetterPhysicalProperties.None,
        ItemCategory? requiredEquipment = null,
        LetterSpecialType specialType = LetterSpecialType.None,
        string specialTargetId = null,
        string humanContext = null,
        string consequenceIfLate = null,
        string consequenceIfDelivered = null,
        EmotionalWeight emotionalWeight = EmotionalWeight.MEDIUM,
        StakeType stakes = StakeType.REPUTATION,
        TierLevel tierLevel = TierLevel.T1)
    {
        // Special narrative letter templates that use placeholder names, not NPC IDs
        HashSet<string> narrativeTemplates = new HashSet<string>
        {
            "forced_patron_resources", "forced_patron_instructions", "forced_patron_summons",
            "patron_letter_resources", "patron_letter_instructions",
            "forced_shadow_dead_drop", "forced_shadow_intelligence", "forced_shadow_blackmail"
        };

        // Skip NPC validation for narrative templates
        bool isNarrativeTemplate = narrativeTemplates.Contains(id);
        // Validate senders
        List<NPC> senders = new List<NPC>();
        if (possibleSenderIds != null && !isNarrativeTemplate)
        {
            foreach (string senderId in possibleSenderIds)
            {
                NPC? npc = availableNPCs.FirstOrDefault(n => n.ID == senderId);
                if (npc == null)
                {
                    Console.WriteLine($"WARNING: DeliveryObligation template '{id}' references non-existent sender NPC '{senderId}'");
                }
                else
                {
                    senders.Add(npc);
                }
            }
        }

        // Validate recipients
        List<NPC> recipients = new List<NPC>();
        if (possibleRecipientIds != null && !isNarrativeTemplate)
        {
            foreach (string recipientId in possibleRecipientIds)
            {
                NPC? npc = availableNPCs.FirstOrDefault(n => n.ID == recipientId);
                if (npc == null)
                {
                    Console.WriteLine($"WARNING: DeliveryObligation template '{id}' references non-existent recipient NPC '{recipientId}'");
                }
                else
                {
                    recipients.Add(npc);
                }
            }
        }

        return CreateLetterTemplate(id, description, tokenType, minDeadlineInHours, maxDeadlineInHours,
                                   minPayment, maxPayment, category, minTokensRequired,
                                   possibleSenders: null, possibleRecipients: null, unlocksLetterIds: null, 
                                   isChainDeliveryObligation: false,
                                   size, physicalProperties, requiredEquipment,
                                   specialType, specialTargetId, humanContext, consequenceIfLate,
                                   consequenceIfDelivered, emotionalWeight, stakes, tierLevel);
    }
}