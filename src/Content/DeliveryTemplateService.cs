using System;
using System.Collections.Generic;
using System.Linq;

public class DeliveryTemplateService
{
    private readonly GameWorld _gameWorld;
    private readonly Random _random = new Random();
    private LetterCategoryService _categoryService;
    private ConversationRepository _conversationRepository;

    public DeliveryTemplateService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public void SetCategoryService(LetterCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public void SetConversationRepository(ConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public List<LetterTemplate> GetAllTemplates()
    {
        List<LetterTemplate> templates = _gameWorld.WorldState.LetterTemplates;

        // Filter based on game mode and tutorial state
        if (_gameWorld.GameMode == GameMode.Tutorial)
        {
            // During tutorial, only show tutorial letters
            return templates.Where(t => t.Id.StartsWith("tutorial_")).ToList();
        }
        else
        {
            // Not in tutorial mode, exclude tutorial letters
            return templates.Where(t => !t.Id.StartsWith("tutorial_")).ToList();
        }
    }

    public LetterTemplate GetTemplateById(string templateId)
    {
        // For specific template requests, check if it's allowed
        LetterTemplate? template = _gameWorld.WorldState.LetterTemplates
            .FirstOrDefault(t => t.Id == templateId);

        if (template == null) return null;

        // Check if this template should be accessible
        bool isTutorialTemplate = templateId.StartsWith("tutorial_");
        bool isTutorialActive = _gameWorld.GameMode == GameMode.Tutorial;

        // Only allow tutorial templates during tutorial
        if (isTutorialTemplate && !isTutorialActive) return null;

        // Don't allow non-tutorial templates during tutorial
        if (!isTutorialTemplate && isTutorialActive) return null;

        return template;
    }

    public List<LetterTemplate> GetTemplatesByTokenType(ConnectionType tokenType)
    {
        // First get all templates (which applies tutorial filtering)
        List<LetterTemplate> templates = GetAllTemplates();

        // Then filter by token type
        return templates
            .Where(t => t.TokenType == tokenType)
            .ToList();
    }

    public LetterTemplate GetRandomTemplate()
    {
        List<LetterTemplate> templates = GetAllTemplates();
        if (!templates.Any()) return null;

        return templates[_random.Next(templates.Count)];
    }

    public LetterTemplate GetRandomTemplateByTokenType(ConnectionType tokenType)
    {
        List<LetterTemplate> templates = GetTemplatesByTokenType(tokenType);
        if (!templates.Any()) return null;

        return templates[_random.Next(templates.Count)];
    }

    public List<LetterTemplate> GetForcedShadowTemplates()
    {
        // Apply tutorial filtering
        List<LetterTemplate> templates = GetAllTemplates();
        return templates
            .Where(t => t.Id.StartsWith("forced_shadow_") && t.TokenType == ConnectionType.Shadow)
            .ToList();
    }

    public List<LetterTemplate> GetForcedPatronTemplates()
    {
        // Apply tutorial filtering
        List<LetterTemplate> templates = GetAllTemplates();
        return templates
            .Where(t => t.Id.StartsWith("forced_patron_") && t.TokenType == ConnectionType.Status)
            .ToList();
    }

    public LetterTemplate GetRandomForcedShadowTemplate()
    {
        List<LetterTemplate> templates = GetForcedShadowTemplates();
        if (!templates.Any()) return null;

        return templates[_random.Next(templates.Count)];
    }

    public LetterTemplate GetRandomForcedPatronTemplate()
    {
        List<LetterTemplate> templates = GetForcedPatronTemplates();
        if (!templates.Any()) return null;

        return templates[_random.Next(templates.Count)];
    }

    // Generate a letter from a template with random values
    public Letter? GenerateLetterFromTemplate(LetterTemplate template, string senderName, string recipientName)
    {
        if (template == null) return null;

        // Override sender/recipient if template specifies them (for tutorial letters)
        if (template.PossibleSenders != null && template.PossibleSenders.Length > 0)
        {
            string senderId = template.PossibleSenders[_random.Next(template.PossibleSenders.Length)];
            NPC? senderNpc = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == senderId);
            if (senderNpc != null) senderName = senderNpc.Name;
        }

        if (template.PossibleRecipients != null && template.PossibleRecipients.Length > 0)
        {
            string recipientId = template.PossibleRecipients[_random.Next(template.PossibleRecipients.Length)];
            NPC? recipientNpc = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == recipientId);
            if (recipientNpc != null) recipientName = recipientNpc.Name;
        }

        Letter letter = new Letter
        {
            SenderName = senderName,
            RecipientName = recipientName,
            Weight = template.Size == SizeCategory.Large ? 3 : template.Size == SizeCategory.Medium ? 2 : 1,
            PhysicalProperties = template.PhysicalProperties,
            SpecialType = template.SpecialType
        };

        // Special letter mechanical effects are handled by the services that use them
        // The Letter object just needs to identify its type
        return letter;
    }

    // Generate a forced letter from a template (for standing obligations)
    // Narrative names should be provided by the calling service, not the template
    public DeliveryObligation GenerateForcedLetterFromTemplate(LetterTemplate template)
    {
        if (template == null) return null;

        // Generate narrative names based on token type
        string senderName;
        string recipientName;

        switch (template.TokenType)
        {
            case ConnectionType.Shadow:
                string[] shadowSenders = new[] { "The Fence", "Midnight Contact", "Shadow Broker", "Anonymous Source" };
                string[] shadowRecipients = new[] { "Dead Drop", "Safe House", "Underground Contact", "Hidden Ally" };
                senderName = shadowSenders[_random.Next(shadowSenders.Length)];
                recipientName = shadowRecipients[_random.Next(shadowRecipients.Length)];
                break;

            case ConnectionType.Status:
                string[] patronSenders = new[] { "Your Patron", "Patron's Secretary", "House Steward" };
                string[] patronRecipients = new[] { "Field Agent", "Local Contact", "Resource Master" };
                senderName = patronSenders[_random.Next(patronSenders.Length)];
                recipientName = patronRecipients[_random.Next(patronRecipients.Length)];
                break;

            default:
                // For other types, this method shouldn't be used
                senderName = "Unknown Sender";
                recipientName = "Unknown Recipient";
                break;
        }

        DeliveryObligation obligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = senderName,
            RecipientName = recipientName,
            SenderId = "anonymous", // Forced letters don't have specific NPC IDs
            RecipientId = "anonymous",
            DeadlineInMinutes = _random.Next(template.MinDeadlineInMinutes, template.MaxDeadlineInMinutes + 1),
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            TokenType = template.TokenType,
            Description = template.Description,
            Stakes = template.Stakes,
            IsGenerated = true,
            GenerationReason = "Forced by Standing Obligation",
            DaysInQueue = 0,
            // State is only for physical Letters, not abstract DeliveryObligations
        };

        // Forced letters are delivery obligations for the queue
        return obligation;
    }

    /// <summary>
    /// Generate a DeliveryObligation from template for queue placement.
    /// </summary>
    public DeliveryObligation? GenerateObligationFromTemplate(LetterTemplate template, string senderName, string recipientName)
    {
        if (template == null) return null;

        // Find NPCs for proper ID assignment
        NPC? sender = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.Name == senderName);
        NPC? recipient = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.Name == recipientName);

        if (sender == null || recipient == null) return null;

        DeliveryObligation obligation = new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = senderName,
            RecipientName = recipientName,
            SenderId = sender.ID,
            RecipientId = recipient.ID,
            DeadlineInMinutes = _random.Next(template.MinDeadlineInMinutes, template.MaxDeadlineInMinutes + 1),
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            TokenType = template.TokenType,
            Description = template.Description,
            Stakes = template.Stakes,
            IsGenerated = true,
            GenerationReason = "NPC Request",
            DaysInQueue = 0,
            // State is only for physical Letters, not abstract DeliveryObligations
        };

        return obligation;
    }

    /// <summary>
    /// Get templates available for an NPC based on their relationship with the player.
    /// </summary>
    public List<LetterTemplate> GetAvailableTemplatesForNPC(string npcId, ConnectionType tokenType)
    {
        if (_categoryService == null)
            return GetTemplatesByTokenType(tokenType); // Fallback to all templates

        return _categoryService.GetAvailableTemplates(npcId, tokenType);
    }

    /// <summary>
    /// Generate a letter from an NPC respecting category thresholds.
    /// </summary>
    public DeliveryObligation GenerateLetterFromNPC(string npcId, string senderName, ConnectionType tokenType)
    {
        List<LetterTemplate> availableTemplates = GetAvailableTemplatesForNPC(npcId, tokenType);
        if (!availableTemplates.Any()) return null;

        // Get the category based on actual token count
        LetterCategory category = _categoryService?.GetAvailableCategory(npcId, tokenType) ?? LetterCategory.Basic;

        // Filter templates to only those matching the category
        List<LetterTemplate> categoryTemplates = availableTemplates.Where(t => t.Category == category).ToList();
        if (!categoryTemplates.Any())
        {
            // Fallback to any available template if no exact category match
            categoryTemplates = availableTemplates;
        }

        // Select a random template from category-appropriate ones
        LetterTemplate template = categoryTemplates[_random.Next(categoryTemplates.Count)];

        // Determine recipient based on template constraints
        string recipientName = null;

        if (template.PossibleRecipients != null && template.PossibleRecipients.Length > 0)
        {
            // Use specific recipients from template (for tutorial letters)
            string recipientId = template.PossibleRecipients[_random.Next(template.PossibleRecipients.Length)];
            NPC? recipientNpc = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == recipientId);
            if (recipientNpc != null) recipientName = recipientNpc.Name;
        }
        else
        {
            // Find a random recipient (not the sender)
            List<NPC> allNpcs = _gameWorld.WorldState.NPCs;
            List<NPC> possibleRecipients = allNpcs.Where(n => n.Name != senderName).ToList();
            if (!possibleRecipients.Any()) return null;

            NPC recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];
            recipientName = recipient.Name;
        }

        // Generate delivery obligation with category-appropriate payment
        DeliveryObligation letter = GenerateObligationFromTemplate(template, senderName, recipientName);

        // Override payment to match category if needed
        if (_categoryService != null)
        {
            (int minPay, int maxPay) = _categoryService.GetCategoryPaymentRange(category);
            letter.Payment = _random.Next(minPay, maxPay + 1);
        }

        return letter;
    }

    /// <summary>
    /// Generate a basic delivery obligation from an NPC's characteristics.
    /// </summary>
    public DeliveryObligation GenerateObligationFromNPC(NPC npc)
    {
        if (npc.LetterTokenTypes == null || !npc.LetterTokenTypes.Any())
            return null;

        // Use NPC's primary token type
        ConnectionType tokenType = npc.LetterTokenTypes.FirstOrDefault();
        
        // Find available templates for this NPC and token type
        List<LetterTemplate> availableTemplates = GetAvailableTemplatesForNPC(npc.ID, tokenType);
        if (!availableTemplates.Any())
        {
            // Create a basic obligation if no templates available
            return CreateBasicObligationFromNPC(npc, tokenType);
        }

        // Use existing template-based generation
        return GenerateLetterFromNPC(npc.ID, npc.Name, tokenType);
    }

    /// <summary>
    /// Generate an urgent delivery obligation from an NPC.
    /// </summary>
    public DeliveryObligation GenerateUrgentObligationFromNPC(NPC npc)
    {
        DeliveryObligation obligation = GenerateObligationFromNPC(npc);
        if (obligation != null)
        {
            // Make it urgent - short deadline and higher stakes
            obligation.DeadlineInMinutes = _random.Next(1, 3); // 1-2 hours
            obligation.Stakes = StakeType.SAFETY; // Higher stakes
            obligation.GenerationReason = "Urgent Request";
        }
        return obligation;
    }

    /// <summary>
    /// Create a basic obligation when no templates are available.
    /// </summary>
    private DeliveryObligation CreateBasicObligationFromNPC(NPC npc, ConnectionType tokenType)
    {
        // Find a reasonable recipient (not the sender)
        List<NPC> allNpcs = _gameWorld.WorldState.NPCs;
        List<NPC> possibleRecipients = allNpcs.Where(n => n.ID != npc.ID).ToList();
        if (!possibleRecipients.Any()) return null;

        NPC recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];

        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = npc.Name,
            RecipientName = recipient.Name,
            SenderId = npc.ID,
            RecipientId = recipient.ID,
            DeadlineInMinutes = _random.Next(12, 72), // 12-72 hours
            Payment = _random.Next(5, 25), // Basic payment range
            TokenType = tokenType,
            Stakes = GetStakesByProfession(npc.Profession),
            Description = $"Delivery request from {npc.Name}",
            // State is only for physical Letters, not abstract DeliveryObligations
            IsGenerated = true,
            GenerationReason = "NPC Request",
            DaysInQueue = 0,
            QueuePosition = -1
        };
    }

    /// <summary>
    /// Get appropriate stakes based on NPC profession.
    /// </summary>
    private StakeType GetStakesByProfession(Professions profession)
    {
        return profession switch
        {
            Professions.Merchant => StakeType.REPUTATION,
            Professions.Noble => StakeType.STATUS,
            Professions.Scribe => StakeType.REPUTATION,
            Professions.Guard => StakeType.SAFETY,
            Professions.Priest => StakeType.SECRET,
            _ => StakeType.REPUTATION
        };
    }
}