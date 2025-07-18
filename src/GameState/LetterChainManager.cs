/// <summary>
/// Manages letter chain generation when letters are delivered.
/// Creates follow-up letters that provide narrative continuity and relationship depth.
/// </summary>
public class LetterChainManager
{
    private readonly GameWorld _gameWorld;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly NPCRepository _npcRepository;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly MessageSystem _messageSystem;

    public LetterChainManager(
        GameWorld gameWorld,
        LetterTemplateRepository letterTemplateRepository,
        NPCRepository npcRepository,
        LetterQueueManager letterQueueManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _letterTemplateRepository = letterTemplateRepository;
        _npcRepository = npcRepository;
        _letterQueueManager = letterQueueManager;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Process letter delivery and generate any follow-up letters
    /// </summary>
    public List<Letter> ProcessLetterDelivery(Letter deliveredLetter)
    {
        var chainLetters = new List<Letter>();
        
        // Check if this letter unlocks any chain letters
        if (deliveredLetter.UnlocksLetterIds.Any())
        {
            // Generate follow-up letters from template IDs
            foreach (var templateId in deliveredLetter.UnlocksLetterIds)
            {
                var chainLetter = GenerateChainLetter(templateId, deliveredLetter);
                if (chainLetter != null)
                {
                    chainLetters.Add(chainLetter);
                }
            }
        }
        else
        {
            // Check if the letter's template has chain letters
            var letterTemplate = FindLetterTemplate(deliveredLetter);
            if (letterTemplate != null && letterTemplate.UnlocksLetterIds.Any())
            {
                // Generate follow-up letters from template
                foreach (var templateId in letterTemplate.UnlocksLetterIds)
                {
                    var chainLetter = GenerateChainLetter(templateId, deliveredLetter);
                    if (chainLetter != null)
                    {
                        chainLetters.Add(chainLetter);
                    }
                }
            }
        }

        // Add chain letters to the queue
        foreach (var chainLetter in chainLetters)
        {
            _letterQueueManager.AddLetterWithObligationEffects(chainLetter);
            
            // Provide feedback about chain letter generation
            _messageSystem.AddSystemMessage($"📬 Follow-up letter generated!", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage($"✉️ {chainLetter.SenderName} → {chainLetter.RecipientName}", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage($"🔗 Chain letter from completing {deliveredLetter.SenderName}'s delivery", SystemMessageTypes.Info);
        }

        return chainLetters;
    }

    /// <summary>
    /// Generate a chain letter from a template ID
    /// </summary>
    private Letter? GenerateChainLetter(string templateId, Letter parentLetter)
    {
        var template = _letterTemplateRepository.GetTemplateById(templateId);
        if (template == null)
        {
            return null;
        }

        // Determine sender and recipient for the chain letter
        var senderName = DetermineChainSender(template, parentLetter);
        var recipientName = DetermineChainRecipient(template, parentLetter);

        if (string.IsNullOrEmpty(senderName) || string.IsNullOrEmpty(recipientName))
        {
            return null;
        }

        // Generate the chain letter
        var chainLetter = _letterTemplateRepository.GenerateLetterFromTemplate(template, senderName, recipientName);
        
        if (chainLetter != null)
        {
            // Mark it as a chain letter
            chainLetter.IsChainLetter = true;
            chainLetter.ParentLetterId = parentLetter.Id;
            
            // Chain letters typically have similar or longer deadlines
            chainLetter.Deadline = Math.Max(chainLetter.Deadline, parentLetter.Deadline + 1);
            
            // Chain letters often have better payment (reward for completing the chain)
            chainLetter.Payment = (int)(chainLetter.Payment * 1.2f);
        }

        return chainLetter;
    }

    /// <summary>
    /// Determine the sender for a chain letter based on the template and parent letter
    /// </summary>
    private string DetermineChainSender(LetterTemplate template, Letter parentLetter)
    {
        // Check if template specifies possible senders
        if (template.PossibleSenders != null && template.PossibleSenders.Length > 0)
        {
            var random = new Random();
            return template.PossibleSenders[random.Next(template.PossibleSenders.Length)];
        }

        // Default logic: chain letters often come from the original recipient
        // This creates a "reply" effect
        return parentLetter.RecipientName;
    }

    /// <summary>
    /// Determine the recipient for a chain letter based on the template and parent letter
    /// </summary>
    private string DetermineChainRecipient(LetterTemplate template, Letter parentLetter)
    {
        // Check if template specifies possible recipients
        if (template.PossibleRecipients != null && template.PossibleRecipients.Length > 0)
        {
            var random = new Random();
            return template.PossibleRecipients[random.Next(template.PossibleRecipients.Length)];
        }

        // Default logic: chain letters often go to the original sender
        // This creates a "reply" effect
        return parentLetter.SenderName;
    }

    /// <summary>
    /// Find the letter template that was used to generate a letter
    /// </summary>
    private LetterTemplate? FindLetterTemplate(Letter letter)
    {
        // This is a simplified approach - in a full implementation,
        // we might store the template ID on the letter itself
        var allTemplates = _letterTemplateRepository.GetAllTemplates();
        
        // Try to find a template that matches the letter's characteristics
        return allTemplates.FirstOrDefault(t => 
            t.TokenType == letter.TokenType && 
            t.MinPayment <= letter.Payment && 
            t.MaxPayment >= letter.Payment);
    }

    /// <summary>
    /// Check if a letter has potential chain letters
    /// </summary>
    public bool HasChainLetters(Letter letter)
    {
        // Check if the letter itself has chain letters
        if (letter.UnlocksLetterIds.Any())
        {
            return true;
        }

        // Check if the letter's template has chain letters
        var template = FindLetterTemplate(letter);
        return template != null && template.UnlocksLetterIds.Any();
    }

    /// <summary>
    /// Get information about potential chain letters for a letter
    /// </summary>
    public List<string> GetChainLetterInfo(Letter letter)
    {
        var chainInfo = new List<string>();

        // Check letter's own chain letters
        foreach (var templateId in letter.UnlocksLetterIds)
        {
            var template = _letterTemplateRepository.GetTemplateById(templateId);
            if (template != null)
            {
                chainInfo.Add($"Unlocks: {template.Description}");
            }
        }

        // Check template's chain letters
        var letterTemplate = FindLetterTemplate(letter);
        if (letterTemplate != null)
        {
            foreach (var templateId in letterTemplate.UnlocksLetterIds)
            {
                var template = _letterTemplateRepository.GetTemplateById(templateId);
                if (template != null)
                {
                    chainInfo.Add($"May unlock: {template.Description}");
                }
            }
        }

        return chainInfo;
    }
}