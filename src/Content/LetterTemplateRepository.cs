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
        var templates = GetAllTemplates();
        if (!templates.Any()) return null;
        
        return templates[_random.Next(templates.Count)];
    }

    public LetterTemplate GetRandomTemplateByTokenType(ConnectionType tokenType)
    {
        var templates = GetTemplatesByTokenType(tokenType);
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
        var templates = GetForcedShadowTemplates();
        if (!templates.Any()) return null;
        
        return templates[_random.Next(templates.Count)];
    }

    public LetterTemplate GetRandomForcedPatronTemplate()
    {
        var templates = GetForcedPatronTemplates();
        if (!templates.Any()) return null;
        
        return templates[_random.Next(templates.Count)];
    }

    // Generate a letter from a template with random values
    public Letter GenerateLetterFromTemplate(LetterTemplate template, string senderName, string recipientName)
    {
        if (template == null) return null;

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

        return letter;
    }

    // Generate a forced letter from a template using template's possible senders/recipients
    public Letter GenerateForcedLetterFromTemplate(LetterTemplate template)
    {
        if (template == null) return null;

        // Use template's defined senders and recipients if available
        string senderName = "Unknown Sender";
        string recipientName = "Unknown Recipient";

        if (template.PossibleSenders != null && template.PossibleSenders.Length > 0)
        {
            senderName = template.PossibleSenders[_random.Next(template.PossibleSenders.Length)];
        }

        if (template.PossibleRecipients != null && template.PossibleRecipients.Length > 0)
        {
            recipientName = template.PossibleRecipients[_random.Next(template.PossibleRecipients.Length)];
        }

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
        var availableTemplates = GetAvailableTemplatesForNPC(npcId, tokenType);
        if (!availableTemplates.Any()) return null;
        
        // Select a random template from available ones
        var template = availableTemplates[_random.Next(availableTemplates.Count)];
        
        // Find a random recipient (not the sender)
        var allNpcs = _gameWorld.WorldState.NPCs;
        var possibleRecipients = allNpcs.Where(n => n.Name != senderName).ToList();
        if (!possibleRecipients.Any()) return null;
        
        var recipient = possibleRecipients[_random.Next(possibleRecipients.Count)];
        
        return GenerateLetterFromTemplate(template, senderName, recipient.Name);
    }
}