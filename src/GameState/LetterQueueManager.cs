using System;
using System.Linq;
public class LetterQueueManager
{
    private readonly GameWorld _gameWorld;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly StandingObligationManager _obligationManager;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly Random _random = new Random();
    
    public LetterQueueManager(GameWorld gameWorld, LetterTemplateRepository letterTemplateRepository, NPCRepository npcRepository, MessageSystem messageSystem, StandingObligationManager obligationManager, ConnectionTokenManager connectionTokenManager)
    {
        _gameWorld = gameWorld;
        _letterTemplateRepository = letterTemplateRepository;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _obligationManager = obligationManager;
        _connectionTokenManager = connectionTokenManager;
    }
    
    // Get the player's letter queue
    public Letter[] GetPlayerQueue()
    {
        return _gameWorld.GetPlayer().LetterQueue;
    }
    
    // Add letter to queue at specific position
    public bool AddLetterToQueue(Letter letter, int position)
    {
        if (letter == null || position < 1 || position > 8) return false;
        
        var queue = _gameWorld.GetPlayer().LetterQueue;
        if (queue[position - 1] != null) return false; // Position occupied
        
        queue[position - 1] = letter;
        letter.QueuePosition = position;
        return true;
    }
    
    // Add letter to first available slot (for minimal POC)
    public int AddLetterToFirstEmpty(Letter letter)
    {
        if (letter == null) return 0;
        
        var queue = _gameWorld.GetPlayer().LetterQueue;
        for (int i = 0; i < 8; i++)
        {
            if (queue[i] == null)
            {
                queue[i] = letter;
                letter.QueuePosition = i + 1;
                return i + 1;
            }
        }
        return 0; // Queue full
    }
    
    // Add letter with obligation-aware positioning
    public int AddLetterWithObligationEffects(Letter letter)
    {
        if (letter == null) return 0;
        
        // Calculate base position (slot 8 by default)
        int basePosition = 8;
        
        // Apply obligation effects to determine best entry position
        int bestPosition = _obligationManager.CalculateBestEntryPosition(letter, basePosition);
        
        // Handle patron letters specially
        if (letter.IsFromPatron)
        {
            return AddPatronLetter(letter);
        }
        
        // Try to place letter at calculated position or higher
        var queue = _gameWorld.GetPlayer().LetterQueue;
        for (int i = bestPosition - 1; i < 8; i++) // Start from bestPosition, work down
        {
            if (queue[i] == null)
            {
                queue[i] = letter;
                letter.QueuePosition = i + 1;
                
                // Show message about positioning
                if (i + 1 < basePosition)
                {
                    _messageSystem.AddSystemMessage(
                        $"Letter entered at slot {i + 1} due to standing obligations",
                        SystemMessageTypes.Info
                    );
                }
                
                return i + 1;
            }
        }
        
        return 0; // Queue full
    }
    
    // Handle patron letters that jump to top positions
    private int AddPatronLetter(Letter letter)
    {
        var queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Try positions 1-3 first for patron letters
        for (int i = 0; i < 3; i++)
        {
            if (queue[i] == null)
            {
                queue[i] = letter;
                letter.QueuePosition = i + 1;
                
                _messageSystem.AddSystemMessage(
                    $"Patron letter entered at priority slot {i + 1}!",
                    SystemMessageTypes.Warning
                );
                
                return i + 1;
            }
        }
        
        // If top slots full, find any empty slot
        for (int i = 3; i < 8; i++)
        {
            if (queue[i] == null)
            {
                queue[i] = letter;
                letter.QueuePosition = i + 1;
                return i + 1;
            }
        }
        
        return 0; // Queue completely full
    }
    
    // Remove letter from queue
    public bool RemoveLetterFromQueue(int position)
    {
        if (position < 1 || position > 8) return false;
        
        var queue = _gameWorld.GetPlayer().LetterQueue;
        var letter = queue[position - 1];
        if (letter == null) return false;
        
        letter.QueuePosition = 0;
        queue[position - 1] = null;
        
        // Shift all letters below the removed position up by one
        ShiftQueueUp(position);
        
        return true;
    }
    
    // Get letter at position
    public Letter GetLetterAt(int position)
    {
        if (position < 1 || position > 8) return null;
        return _gameWorld.GetPlayer().LetterQueue[position - 1];
    }
    
    // Check if position 1 has a letter
    public bool CanDeliverFromPosition1()
    {
        return GetLetterAt(1) != null;
    }
    
    // Process daily deadline countdown
    public void ProcessDailyDeadlines()
    {
        var queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Process from back to front to avoid issues with shifting
        for (int i = 7; i >= 0; i--)
        {
            var letter = queue[i];
            if (letter != null)
            {
                letter.Deadline--;
                if (letter.Deadline <= 0)
                {
                    // Apply relationship damage before removing
                    ApplyRelationshipDamage(letter, _connectionTokenManager);
                    
                    // Remove expired letter (which will shift the queue)
                    RemoveLetterFromQueue(i + 1);
                }
            }
        }
    }
    
    // Apply token penalty when a letter expires
    private void ApplyRelationshipDamage(Letter letter, ConnectionTokenManager _connectionTokenManager)
    {
        // Determine penalty amount (2 tokens for expired letters)
        int tokenPenalty = 2;
        
        // Get the sender's NPC ID (for per-NPC tracking)
        var senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;
        
        // Remove tokens from the relationship with this NPC
        _connectionTokenManager.RemoveTokensFromNPC(letter.TokenType, tokenPenalty, senderId);
        
        // Record the expiry in letter history
        var player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }
        player.NPCLetterHistory[senderId].RecordExpiry();
        
        // Log the relationship damage for UI feedback
        _messageSystem.AddSystemMessage(
            $"Letter from {letter.SenderName} expired! Lost {tokenPenalty} {letter.TokenType} tokens with them.", 
            SystemMessageTypes.Danger
        );
    }
    
    // Helper to get NPC ID from name (since letters store names, not IDs)
    private string GetNPCIdByName(string npcName)
    {
        var npc = _npcRepository.GetAllNPCs().FirstOrDefault(n => n.Name == npcName);
        return npc?.ID ?? "";
    }
    
    // Track letter delivery in history and process chain letters
    public void RecordLetterDelivery(Letter letter)
    {
        if (letter == null) return;
        
        var senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;
        
        var player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }
        
        player.NPCLetterHistory[senderId].RecordDelivery();
        
        // Process chain letters
        ProcessChainLetters(letter);
    }
    
    // Track letter skip in history
    public void RecordLetterSkip(Letter letter)
    {
        if (letter == null) return;
        
        var senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;
        
        var player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }
        
        player.NPCLetterHistory[senderId].RecordSkip();
    }
    
    // Get expiring letters
    public Letter[] GetExpiringLetters(int daysThreshold)
    {
        return _gameWorld.GetPlayer().LetterQueue
            .Where(l => l != null && l.Deadline <= daysThreshold)
            .OrderBy(l => l.Deadline)
            .ToArray();
    }
    
    // Check if queue is full
    public bool IsQueueFull()
    {
        return _gameWorld.GetPlayer().LetterQueue.All(slot => slot != null);
    }
    
    // Get queue fill status
    public int GetLetterCount()
    {
        return _gameWorld.GetPlayer().LetterQueue.Count(slot => slot != null);
    }
    
    // Shift queue up after removal - compact all letters to fill gaps
    private void ShiftQueueUp(int removedPosition)
    {
        var queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Collect all letters after the removed position
        var remainingLetters = new System.Collections.Generic.List<Letter>();
        for (int i = removedPosition; i < 8; i++)
        {
            if (queue[i] != null)
            {
                remainingLetters.Add(queue[i]);
                queue[i] = null; // Clear the old position
            }
        }
        
        // Place remaining letters starting from the removed position, filling gaps
        int writePosition = removedPosition - 1; // Convert to 0-based index
        foreach (var letter in remainingLetters)
        {
            // Find next empty slot starting from the removed position
            while (writePosition < 8 && queue[writePosition] != null)
            {
                writePosition++;
            }
            
            if (writePosition < 8)
            {
                queue[writePosition] = letter;
                letter.QueuePosition = writePosition + 1; // Convert back to 1-based
            }
        }
    }
    
    // Skip letter delivery by spending tokens
    public bool TrySkipDeliver(int position)
    {
        if (position <= 1 || position > 8) return false;
        
        var letter = GetLetterAt(position);
        if (letter == null) return false;
        
        // Check if position 1 is occupied (can't skip to occupied slot)
        if (GetLetterAt(1) != null) return false;
        
        // Calculate token cost: position - 1 (skip from position 3 costs 2 tokens)
        int baseCost = position - 1;
        int multiplier = _obligationManager.CalculateSkipCostMultiplier(letter);
        int tokenCost = baseCost * multiplier;
        
        // Validate token availability through ConnectionTokenManager
        if (!_connectionTokenManager.HasTokens(letter.TokenType, tokenCost))
        {
            return false;
        }
        
        // Spend the tokens
        if (!_connectionTokenManager.SpendTokens(letter.TokenType, tokenCost))
        {
            return false;
        }
        
        // Move letter to position 1
        var queue = _gameWorld.GetPlayer().LetterQueue;
        queue[0] = letter; // Position 1 is index 0
        queue[position - 1] = null; // Clear original position
        letter.QueuePosition = 1;
        
        return true;
    }
    
    // Generate 1-2 daily letters from available NPCs and templates
    public int GenerateDailyLetters()
    {
        // Generate 1-2 letters per day
        int lettersToGenerate = _random.Next(1, 3);
        int lettersGenerated = 0;
        
        for (int i = 0; i < lettersToGenerate; i++)
        {
            // Check if queue has space
            if (IsQueueFull())
            {
                break; // Stop generating if queue is full
            }
            
            // Get a random template
            var template = _letterTemplateRepository.GetRandomTemplate();
            if (template == null) continue;
            
            // Get random NPCs for sender and recipient
            var allNpcs = _npcRepository.GetAllNPCs();
            if (allNpcs.Count < 2) continue; // Need at least 2 NPCs
            
            var sender = allNpcs[_random.Next(allNpcs.Count)];
            var recipient = allNpcs[_random.Next(allNpcs.Count)];
            
            // Ensure sender and recipient are different
            if (sender.ID == recipient.ID && allNpcs.Count > 1)
            {
                recipient = allNpcs.Where(n => n.ID != sender.ID).First();
            }
            
            // Generate letter from template
            var letter = _letterTemplateRepository.GenerateLetterFromTemplate(template, sender.Name, recipient.Name);
            if (letter != null)
            {
                // Add to first empty slot
                if (AddLetterToFirstEmpty(letter) > 0)
                {
                    lettersGenerated++;
                }
            }
        }
        
        return lettersGenerated;
    }
    
    // Morning Swap: Free swap of two adjacent letters once per day
    public bool TryMorningSwap(int position1, int position2)
    {
        // Validate it's morning (dawn time block)
        if (_gameWorld.TimeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
        {
            return false; // Can only use during dawn
        }
        
        // Check if already used today
        var player = _gameWorld.GetPlayer();
        if (player.LastMorningSwapDay == _gameWorld.CurrentDay)
        {
            return false; // Already used today
        }
        
        // Validate positions are adjacent
        if (Math.Abs(position1 - position2) != 1)
        {
            return false; // Must be adjacent positions
        }
        
        // Validate positions are in range
        if (position1 < 1 || position1 > 8 || position2 < 1 || position2 > 8)
        {
            return false;
        }
        
        // Get letters at positions
        var letter1 = GetLetterAt(position1);
        var letter2 = GetLetterAt(position2);
        
        // At least one position must have a letter
        if (letter1 == null && letter2 == null)
        {
            return false;
        }
        
        // Perform the swap
        var queue = _gameWorld.GetPlayer().LetterQueue;
        queue[position1 - 1] = letter2;
        queue[position2 - 1] = letter1;
        
        // Update queue positions
        if (letter1 != null) letter1.QueuePosition = position2;
        if (letter2 != null) letter2.QueuePosition = position1;
        
        // Mark as used today
        player.LastMorningSwapDay = _gameWorld.CurrentDay;
        
        return true;
    }
    
    // Purge: Remove bottom letter for 3 tokens of any type
    public bool TryPurgeLetter(Dictionary<ConnectionType, int> tokenPayment)
    {
        // Check if there's a letter in position 8
        var letterToPurge = GetLetterAt(8);
        if (letterToPurge == null)
        {
            return false; // No letter to purge
        }
        
        // Check if purging this letter is forbidden by obligations
        if (_obligationManager.IsActionForbidden("purge", letterToPurge, out string reason))
        {
            _messageSystem.AddSystemMessage(
                $"Cannot purge letter: {reason}",
                SystemMessageTypes.Warning
            );
            return false;
        }
        
        // Validate token payment totals 3
        int totalTokens = tokenPayment.Values.Sum();
        if (totalTokens != 3)
        {
            return false; // Must pay exactly 3 tokens
        }
        
        // Check if player has enough tokens
        foreach (var payment in tokenPayment)
        {
            if (!_connectionTokenManager.HasTokens(payment.Key, payment.Value))
            {
                return false; // Insufficient tokens
            }
        }
        
        // Spend the tokens
        foreach (var payment in tokenPayment)
        {
            if (!_connectionTokenManager.SpendTokens(payment.Key, payment.Value))
            {
                return false; // Failed to spend tokens
            }
        }
        
        // Remove the letter from position 8
        RemoveLetterFromQueue(8);
        
        return true;
    }
    
    // Priority: Move letter to position 1 for 5 matching tokens
    public bool TryPriorityMove(int fromPosition)
    {
        // Validate position
        if (fromPosition < 2 || fromPosition > 8)
        {
            return false; // Can't priority move from position 1 or invalid position
        }
        
        // Get the letter
        var letter = GetLetterAt(fromPosition);
        if (letter == null)
        {
            return false; // No letter at position
        }
        
        // Check if position 1 is occupied
        if (GetLetterAt(1) != null)
        {
            return false; // Position 1 must be empty
        }
        
        // Check token cost (5 matching tokens)
        if (!_connectionTokenManager.HasTokens(letter.TokenType, 5))
        {
            return false; // Insufficient tokens
        }
        
        // Spend the tokens
        if (!_connectionTokenManager.SpendTokens(letter.TokenType, 5))
        {
            return false; // Failed to spend tokens
        }
        
        // Move letter to position 1
        var queue = _gameWorld.GetPlayer().LetterQueue;
        queue[fromPosition - 1] = null; // Clear original position
        queue[0] = letter; // Place in position 1
        letter.QueuePosition = 1;
        
        // Shift other letters down to fill the gap
        ShiftQueueUp(fromPosition);
        
        return true;
    }
    
    // Extend: Add 2 days to deadline for 2 matching tokens
    public bool TryExtendDeadline(int position)
    {
        // Validate position
        if (position < 1 || position > 8)
        {
            return false;
        }
        
        // Get the letter
        var letter = GetLetterAt(position);
        if (letter == null)
        {
            return false; // No letter at position
        }
        
        // Check token cost (2 matching tokens)
        if (!_connectionTokenManager.HasTokens(letter.TokenType, 2))
        {
            return false; // Insufficient tokens
        }
        
        // Spend the tokens
        if (!_connectionTokenManager.SpendTokens(letter.TokenType, 2))
        {
            return false; // Failed to spend tokens
        }
        
        // Extend the deadline
        letter.Deadline += 2;
        
        return true;
    }
    
    // Process chain letters when a letter is delivered
    private void ProcessChainLetters(Letter deliveredLetter)
    {
        var chainLetters = new System.Collections.Generic.List<Letter>();
        
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
            AddLetterWithObligationEffects(chainLetter);
            
            // Provide feedback about chain letter generation
            _messageSystem.AddSystemMessage($"📬 Follow-up letter generated!", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage($"✉️ {chainLetter.SenderName} → {chainLetter.RecipientName}", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage($"🔗 Chain letter from completing {deliveredLetter.SenderName}'s delivery", SystemMessageTypes.Info);
        }
    }

    // Generate a chain letter from a template ID
    private Letter GenerateChainLetter(string templateId, Letter parentLetter)
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

    // Determine the sender for a chain letter based on the template and parent letter
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

    // Determine the recipient for a chain letter based on the template and parent letter
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

    // Find the letter template that was used to generate a letter
    private LetterTemplate FindLetterTemplate(Letter letter)
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

    // Check if a letter has potential chain letters
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

    // Get information about potential chain letters for a letter
    public System.Collections.Generic.List<string> GetChainLetterInfo(Letter letter)
    {
        var chainInfo = new System.Collections.Generic.List<string>();

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