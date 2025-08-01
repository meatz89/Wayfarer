using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for letter generation business logic
/// </summary>
public class LetterGenerationService
{
    private readonly LetterTemplateRepository _templateRepository;
    private readonly NPCRepository _npcRepository;
    private readonly LetterCategoryService _categoryService;
    private readonly ILogger<LetterGenerationService> _logger;
    private readonly Random _random = new Random();

    public LetterGenerationService(
        LetterTemplateRepository templateRepository,
        NPCRepository npcRepository,
        LetterCategoryService categoryService,
        ILogger<LetterGenerationService> logger)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _categoryService = categoryService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate a letter from a template with random values
    /// </summary>
    public Letter GenerateLetterFromTemplate(LetterTemplate template, string senderName, string recipientName)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (string.IsNullOrWhiteSpace(senderName))
        {
            throw new ArgumentException("Sender name is required", nameof(senderName));
        }

        if (string.IsNullOrWhiteSpace(recipientName))
        {
            throw new ArgumentException("Recipient name is required", nameof(recipientName));
        }

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

        _logger.LogDebug($"Generated letter from template '{template.Id}': {senderName} -> {recipientName}");

        return letter;
    }

    /// <summary>
    /// Generate a forced letter from a template (for standing obligations)
    /// </summary>
    public Letter GenerateForcedLetterFromTemplate(LetterTemplate template)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        // Generate narrative names based on token type
        (string senderName, string recipientName) = GenerateNarrativeNames(template.TokenType);

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

        _logger.LogDebug($"Generated forced letter from template '{template.Id}': {letter.SenderName} -> {letter.RecipientName}");

        return letter;
    }

    /// <summary>
    /// Generate a letter from an NPC respecting category thresholds
    /// </summary>
    public Letter GenerateLetterFromNPC(string npcId, string senderName, ConnectionType tokenType)
    {
        List<LetterTemplate> availableTemplates = GetAvailableTemplatesForNPC(npcId, tokenType);
        if (!availableTemplates.Any())
        {
            _logger.LogWarning($"No templates available for NPC '{npcId}' with token type {tokenType}");
            return null;
        }

        // Get the category based on actual token count
        LetterCategory category = _categoryService?.GetAvailableCategory(npcId, tokenType) ?? LetterCategory.Basic;

        // Filter templates to only those matching the category
        List<LetterTemplate> categoryTemplates = availableTemplates.Where(t => t.Category == category).ToList();
        if (!categoryTemplates.Any())
        {
            // Fallback to any available template if no exact category match
            categoryTemplates = availableTemplates.ToList();
            _logger.LogDebug($"No templates for category {category}, using any available template");
        }

        // Select a random template from category-appropriate ones
        LetterTemplate template = categoryTemplates[_random.Next(categoryTemplates.Count)];

        // Find a random recipient (not the sender)
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        List<NPC> possibleRecipients = allNpcs.Where(n => n.Name != senderName).ToList();
        if (!possibleRecipients.Any())
        {
            _logger.LogWarning($"No possible recipients found for letter from '{senderName}'");
            return null;
        }

        NPC recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];

        // Generate letter with category-appropriate payment
        Letter letter = GenerateLetterFromTemplate(template, senderName, recipient.Name);

        // Override payment to match category if needed
        if (_categoryService != null)
        {
            (int minPay, int maxPay) = _categoryService.GetCategoryPaymentRange(category);
            letter.Payment = _random.Next(minPay, maxPay + 1);
            _logger.LogDebug($"Adjusted payment for category {category}: {letter.Payment} coins");
        }

        return letter;
    }

    /// <summary>
    /// Get templates available for an NPC based on their relationship with the player
    /// </summary>
    private List<LetterTemplate> GetAvailableTemplatesForNPC(string npcId, ConnectionType tokenType)
    {
        if (_categoryService == null)
        {
            return _templateRepository.GetTemplatesByTokenType(tokenType).ToList();
        }

        return _categoryService.GetAvailableTemplates(npcId, tokenType);
    }

    /// <summary>
    /// Generate narrative-appropriate names based on token type
    /// </summary>
    private (string senderName, string recipientName) GenerateNarrativeNames(ConnectionType tokenType)
    {
        switch (tokenType)
        {
            case ConnectionType.Shadow:
                string[] shadowSenders = new[] { "The Fence", "Midnight Contact", "Shadow Broker", "Anonymous Source" };
                string[] shadowRecipients = new[] { "Dead Drop", "Safe House", "Underground Contact", "Hidden Ally" };
                return (
                    shadowSenders[_random.Next(shadowSenders.Length)],
                    shadowRecipients[_random.Next(shadowRecipients.Length)]
                );

            case ConnectionType.Status:
                string[] patronSenders = new[] { "Your Patron", "Patron's Secretary", "House Steward" };
                string[] patronRecipients = new[] { "Field Agent", "Local Contact", "Resource Master" };
                return (
                    patronSenders[_random.Next(patronSenders.Length)],
                    patronRecipients[_random.Next(patronRecipients.Length)]
                );

            default:
                // For other types, generate generic names
                return ("Unknown Sender", "Unknown Recipient");
        }
    }
}