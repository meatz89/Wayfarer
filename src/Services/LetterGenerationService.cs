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
            DeadlineInHours = _random.Next(template.MinDeadlineInHours, template.MaxDeadlineInHours + 1),
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            Size = template.Size,
            PhysicalProperties = template.PhysicalProperties,
            RequiredEquipment = template.RequiredEquipment,
            Description = template.Description,
            HumanContext = template.HumanContext,
            ConsequenceIfLate = template.ConsequenceIfLate,
            ConsequenceIfDelivered = template.ConsequenceIfDelivered,
            EmotionalWeight = template.EmotionalWeight,
            Stakes = template.Stakes,
            Tier = template.TierLevel
        };

        _logger.LogDebug($"Generated letter from template '{template.Id}': {senderName} -> {recipientName}");

        return letter;
    }

    /// <summary>
    /// Generate a forced letter from a template (for standing obligations)
    /// Forced letters can be any tier as they are part of obligations
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
            DeadlineInHours = _random.Next(template.MinDeadlineInHours, template.MaxDeadlineInHours + 1),
            Payment = _random.Next(template.MinPayment, template.MaxPayment + 1),
            IsGenerated = true,
            GenerationReason = $"Forced from template: {template.Id}",
            Size = template.Size,
            PhysicalProperties = template.PhysicalProperties,
            RequiredEquipment = template.RequiredEquipment,
            Description = template.Description,
            HumanContext = template.HumanContext,
            ConsequenceIfLate = template.ConsequenceIfLate,
            ConsequenceIfDelivered = template.ConsequenceIfDelivered,
            EmotionalWeight = template.EmotionalWeight,
            Stakes = template.Stakes,
            Tier = template.TierLevel
        };

        _logger.LogDebug($"Generated forced letter from template '{template.Id}': {letter.SenderName} -> {letter.RecipientName}");

        return letter;
    }

    /// <summary>
    /// Generate a letter from an NPC respecting category thresholds and player tier
    /// </summary>
    public Letter GenerateLetterFromNPC(string npcId, string senderName, ConnectionType tokenType, TierLevel playerTier = TierLevel.T1)
    {
        List<LetterTemplate> availableTemplates = GetAvailableTemplatesForNPC(npcId, tokenType);
        if (!availableTemplates.Any())
        {
            _logger.LogWarning($"No templates available for NPC '{npcId}' with token type {tokenType}");
            return null;
        }
        
        // Filter templates by player tier - can only receive letters at or below their tier
        availableTemplates = availableTemplates.Where(t => t.TierLevel <= playerTier).ToList();
        if (!availableTemplates.Any())
        {
            _logger.LogWarning($"No templates available for player tier {playerTier}");
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
    /// Get templates available for a player based on their tier level
    /// </summary>
    public List<LetterTemplate> GetAvailableTemplatesByTier(TierLevel playerTier)
    {
        return _templateRepository.GetAllTemplates()
            .Where(t => t.TierLevel <= playerTier)
            .ToList();
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

    /// <summary>
    /// Generate a special letter with specific effects.
    /// CONTENT EFFICIENT: Works with existing 5 NPCs by using professions/services.
    /// </summary>
    public Letter GenerateSpecialLetter(LetterSpecialType specialType, string recipientNpcId = null)
    {
        var letter = new Letter
        {
            SpecialType = specialType,
            DeadlineInHours = _random.Next(36, 72), // Special letters have longer deadlines
            State = LetterState.Offered,
            Tier = TierLevel.T3 // Special letters are always T3
        };

        // Find appropriate recipient if not specified
        NPC recipient = null;
        if (!string.IsNullOrEmpty(recipientNpcId))
        {
            recipient = _npcRepository.GetById(recipientNpcId);
        }

        switch (specialType)
        {
            case LetterSpecialType.AccessPermit:
                // Target Transport NPCs (Ferryman, Harbor_Master) or NPCs with Transport service
                if (recipient == null)
                {
                    var transportNpcs = _npcRepository.GetNPCsProvidingService(ServiceTypes.Transport);
                    if (!transportNpcs.Any())
                    {
                        // Fallback to NPCs who might control routes
                        transportNpcs = _npcRepository.GetNPCsByProfession(Professions.Ferryman)
                            .Concat(_npcRepository.GetNPCsByProfession(Professions.Harbor_Master))
                            .ToList();
                    }
                    recipient = transportNpcs.Any() ? transportNpcs[_random.Next(transportNpcs.Count)] : _npcRepository.GetAllNPCs().FirstOrDefault();
                }
                
                letter.TokenType = ConnectionType.Commerce;
                letter.Stakes = StakeType.WEALTH;
                letter.SenderName = "Trade Authority";
                letter.RecipientName = recipient?.Name ?? "Transport Official";
                letter.RecipientId = recipient?.ID ?? "";
                letter.Description = "Official Travel Permit - Unlocks new routes";
                letter.HumanContext = "This permit grants access to restricted trade routes";
                letter.ConsequenceIfLate = "The permit expires and routes remain locked";
                letter.ConsequenceIfDelivered = "New routes become available for travel";
                letter.Payment = _random.Next(15, 25);
                letter.PhysicalProperties = LetterPhysicalProperties.Valuable;
                break;

            case LetterSpecialType.Introduction:
                // Target higher-tier NPCs for introductions
                if (recipient == null)
                {
                    var higherTierNpcs = _npcRepository.GetAllNPCs().Where(n => n.Tier >= 2).ToList();
                    recipient = higherTierNpcs.Any() ? higherTierNpcs[_random.Next(higherTierNpcs.Count)] : _npcRepository.GetAllNPCs().FirstOrDefault();
                }
                
                letter.TokenType = ConnectionType.Trust;
                letter.Stakes = StakeType.REPUTATION;
                letter.SenderName = "Respected Merchant";
                letter.RecipientName = recipient?.Name ?? "Guild Representative";
                letter.RecipientId = recipient?.ID ?? "";
                letter.Description = "Letter of Introduction - Unlocks new contacts";
                letter.HumanContext = "A personal recommendation that opens doors";
                letter.ConsequenceIfLate = "The introduction loses its value";
                letter.ConsequenceIfDelivered = "Access to previously unreachable contacts";
                letter.Payment = _random.Next(10, 20);
                
                // Specify which NPC this unlocks (if any)
                var lockedNpcs = _npcRepository.GetAllNPCs().Where(n => n.Tier > 1).ToList();
                if (lockedNpcs.Any())
                {
                    letter.UnlocksNPCId = lockedNpcs[_random.Next(lockedNpcs.Count)].ID;
                }
                break;

            case LetterSpecialType.Endorsement:
                // Target Status-aligned NPCs
                if (recipient == null)
                {
                    var statusNpcs = _npcRepository.GetAllNPCs()
                        .Where(n => n.LetterTokenTypes.Contains(ConnectionType.Status))
                        .ToList();
                    recipient = statusNpcs.Any() ? statusNpcs[_random.Next(statusNpcs.Count)] : _npcRepository.GetAllNPCs().FirstOrDefault();
                }
                
                letter.TokenType = ConnectionType.Status;
                letter.Stakes = StakeType.REPUTATION;
                letter.SenderName = "Noble Patron";
                letter.RecipientName = recipient?.Name ?? "Local Authority";
                letter.RecipientId = recipient?.ID ?? "";
                letter.Description = "Letter of Endorsement - Temporary relationship bonus";
                letter.HumanContext = "A powerful recommendation from someone important";
                letter.ConsequenceIfLate = "The endorsement loses its impact";
                letter.ConsequenceIfDelivered = "Significant temporary boost to standing";
                letter.Payment = _random.Next(20, 30);
                letter.BonusDuration = 3; // 3 days
                break;

            case LetterSpecialType.Information:
                // Target Shadow-aligned NPCs or Information service providers
                if (recipient == null)
                {
                    var infoNpcs = _npcRepository.GetNPCsProvidingService(ServiceTypes.Information)
                        .Concat(_npcRepository.GetAllNPCs().Where(n => n.LetterTokenTypes.Contains(ConnectionType.Shadow)))
                        .ToList();
                    recipient = infoNpcs.Any() ? infoNpcs[_random.Next(infoNpcs.Count)] : _npcRepository.GetAllNPCs().FirstOrDefault();
                }
                
                letter.TokenType = ConnectionType.Shadow;
                letter.Stakes = StakeType.SECRET;
                letter.SenderName = "Anonymous Source";
                letter.RecipientName = recipient?.Name ?? "Information Broker";
                letter.RecipientId = recipient?.ID ?? "";
                letter.Description = "Confidential Intelligence - Reveals hidden information";
                letter.HumanContext = "Dangerous knowledge that could change everything";
                letter.ConsequenceIfLate = "The information becomes worthless";
                letter.ConsequenceIfDelivered = "Hidden routes and opportunities revealed";
                letter.Payment = _random.Next(25, 35);
                letter.PhysicalProperties = LetterPhysicalProperties.RequiresProtection;
                letter.InformationId = $"info_{Guid.NewGuid().ToString().Substring(0, 8)}";
                break;

            default:
                // Regular letter as fallback
                letter.SpecialType = LetterSpecialType.None;
                letter.TokenType = ConnectionType.Trust;
                letter.SenderName = "Local Resident";
                letter.RecipientName = "Neighbor";
                letter.Description = "Personal Correspondence";
                letter.Payment = _random.Next(5, 15);
                break;
        }

        letter.EmotionalWeight = letter.Stakes switch
        {
            StakeType.SECRET => EmotionalWeight.HIGH,
            StakeType.SAFETY => EmotionalWeight.CRITICAL,
            StakeType.WEALTH => EmotionalWeight.MEDIUM,
            _ => EmotionalWeight.LOW
        };

        _logger.LogDebug($"Generated special letter: {letter.SpecialType} from {letter.SenderName} to {letter.RecipientName}");
        
        return letter;
    }

    /// <summary>
    /// Determine if a special letter should be generated based on game state.
    /// CONTENT EFFICIENT: Uses existing game state to trigger special letters.
    /// </summary>
    public bool ShouldGenerateSpecialLetter(Player player, int currentDay)
    {
        // Special letters appear:
        // 1. Every 3-4 days to maintain pacing
        // 2. When player has high tokens with specific NPCs
        // 3. When player reaches new tier
        // 4. When queue is getting low (help player)
        
        // Simple probability based on day
        if (currentDay % 3 == 0)
        {
            return _random.Next(100) < 40; // 40% chance every 3 days
        }
        
        // Higher chance if player is doing well (has tokens)
        int totalTokens = player.GetTotalTokenCount();
        if (totalTokens > 10)
        {
            return _random.Next(100) < 25; // 25% chance when successful
        }
        
        return _random.Next(100) < 10; // 10% base chance
    }
}