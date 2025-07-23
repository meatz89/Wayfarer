using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Wayfarer.Core.Repositories;

namespace Wayfarer.Services
{
    /// <summary>
    /// Service for letter generation business logic
    /// </summary>
    public class LetterGenerationService
    {
        private readonly ILetterTemplateRepository _templateRepository;
        private readonly INPCRepository _npcRepository;
        private readonly LetterCategoryService _categoryService;
        private readonly ILogger<LetterGenerationService> _logger;
        private readonly Random _random = new Random();

        public LetterGenerationService(
            ILetterTemplateRepository templateRepository,
            INPCRepository npcRepository,
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

            var letter = new Letter
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
            var (senderName, recipientName) = GenerateNarrativeNames(template.TokenType);

            var letter = new Letter
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
            var availableTemplates = GetAvailableTemplatesForNPC(npcId, tokenType);
            if (!availableTemplates.Any())
            {
                _logger.LogWarning($"No templates available for NPC '{npcId}' with token type {tokenType}");
                return null;
            }

            // Get the category based on actual token count
            var category = _categoryService?.GetAvailableCategory(npcId, tokenType) ?? LetterCategory.Basic;

            // Filter templates to only those matching the category
            var categoryTemplates = availableTemplates.Where(t => t.Category == category).ToList();
            if (!categoryTemplates.Any())
            {
                // Fallback to any available template if no exact category match
                categoryTemplates = availableTemplates.ToList();
                _logger.LogDebug($"No templates for category {category}, using any available template");
            }

            // Select a random template from category-appropriate ones
            var template = categoryTemplates[_random.Next(categoryTemplates.Count)];

            // Find a random recipient (not the sender)
            var allNpcs = _npcRepository.GetAll().ToList();
            var possibleRecipients = allNpcs.Where(n => n.Name != senderName).ToList();
            if (!possibleRecipients.Any())
            {
                _logger.LogWarning($"No possible recipients found for letter from '{senderName}'");
                return null;
            }

            var recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];

            // Generate letter with category-appropriate payment
            var letter = GenerateLetterFromTemplate(template, senderName, recipient.Name);

            // Override payment to match category if needed
            if (_categoryService != null)
            {
                var (minPay, maxPay) = _categoryService.GetCategoryPaymentRange(category);
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
                    var shadowSenders = new[] { "The Fence", "Midnight Contact", "Shadow Broker", "Anonymous Source" };
                    var shadowRecipients = new[] { "Dead Drop", "Safe House", "Underground Contact", "Hidden Ally" };
                    return (
                        shadowSenders[_random.Next(shadowSenders.Length)],
                        shadowRecipients[_random.Next(shadowRecipients.Length)]
                    );

                case ConnectionType.Noble:
                    var patronSenders = new[] { "Your Patron", "Patron's Secretary", "House Steward" };
                    var patronRecipients = new[] { "Field Agent", "Local Contact", "Resource Master" };
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
}