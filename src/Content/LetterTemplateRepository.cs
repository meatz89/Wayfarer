using System;
using System.Collections.Generic;
using System.Linq;
public class LetterTemplateRepository
{
    private readonly GameWorld _gameWorld;
    private readonly Random _random = new Random();
    private LetterCategoryService _categoryService;

    public LetterTemplateRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public void SetCategoryService(LetterCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public List<LetterTemplate> GetAllTemplates()
    {
        return _gameWorld.WorldState.LetterTemplates;
    }

    public LetterTemplate GetTemplateById(string templateId)
    {
        return _gameWorld.WorldState.LetterTemplates
            .FirstOrDefault(t => t.Id == templateId);
    }

    public List<LetterTemplate> GetTemplatesByTokenType(ConnectionType tokenType)
    {
        return _gameWorld.WorldState.LetterTemplates
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
        return _gameWorld.WorldState.LetterTemplates
            .Where(t => t.Id.StartsWith("forced_shadow_") && t.TokenType == ConnectionType.Shadow)
            .ToList();
    }

    public List<LetterTemplate> GetForcedPatronTemplates()
    {
        return _gameWorld.WorldState.LetterTemplates
            .Where(t => t.Id.StartsWith("forced_patron_") && t.TokenType == ConnectionType.Noble)
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
    public Letter GenerateLetterFromTemplate(LetterTemplate template, string senderName, string recipientName)
    {
        if (template == null) return null;

        Letter letter = new Letter
        {
            SenderName = senderName,
            RecipientName = recipientName,
            TokenType = template.TokenType,
            Deadline = _random.Next(template.MinDeadline, template.MaxDeadline + 1),
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            Size = template.Size,
            PhysicalProperties = template.PhysicalProperties,
            RequiredEquipment = template.RequiredEquipment,
            Description = template.Description
        };

        return letter;
    }

    // Generate a forced letter from a template (for standing obligations)
    // Narrative names should be provided by the calling service, not the template
    public Letter GenerateForcedLetterFromTemplate(LetterTemplate template)
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

            case ConnectionType.Noble:
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

        Letter letter = new Letter
        {
            SenderName = senderName,
            RecipientName = recipientName,
            TokenType = template.TokenType,
            Deadline = _random.Next(template.MinDeadline, template.MaxDeadline + 1),
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            IsGenerated = true,
            GenerationReason = $"Forced from template: {template.Id}",
            Size = template.Size,
            PhysicalProperties = template.PhysicalProperties,
            RequiredEquipment = template.RequiredEquipment,
            Description = template.Description
        };

        return letter;
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
    public Letter GenerateLetterFromNPC(string npcId, string senderName, ConnectionType tokenType)
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

        // Find a random recipient (not the sender)
        List<NPC> allNpcs = _gameWorld.WorldState.NPCs;
        List<NPC> possibleRecipients = allNpcs.Where(n => n.Name != senderName).ToList();
        if (!possibleRecipients.Any()) return null;

        NPC recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];

        // Generate letter with category-appropriate payment
        Letter letter = GenerateLetterFromTemplate(template, senderName, recipient.Name);

        // Override payment to match category if needed
        if (_categoryService != null)
        {
            (int minPay, int maxPay) = _categoryService.GetCategoryPaymentRange(category);
            letter.Payment = _random.Next(minPay, maxPay + 1);
        }

        return letter;
    }
}