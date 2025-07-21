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
    private readonly LetterCategoryService _categoryService;
    private readonly ConversationFactory _conversationFactory;
    private readonly Random _random = new Random();
    
    public LetterQueueManager(GameWorld gameWorld, LetterTemplateRepository letterTemplateRepository, NPCRepository npcRepository, MessageSystem messageSystem, StandingObligationManager obligationManager, ConnectionTokenManager connectionTokenManager, LetterCategoryService categoryService, ConversationFactory conversationFactory)
    {
        _gameWorld = gameWorld;
        _letterTemplateRepository = letterTemplateRepository;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _obligationManager = obligationManager;
        _connectionTokenManager = connectionTokenManager;
        _categoryService = categoryService;
        _conversationFactory = conversationFactory;
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
    // Add letter to first available slot - queue fills from position 1
    public int AddLetter(Letter letter)
    {
        if (letter == null) return 0;
        
        var queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Find the FIRST empty slot, filling from position 1
        for (int i = 0; i < 8; i++)
        {
            if (queue[i] == null)
            {
                queue[i] = letter;
                letter.QueuePosition = i + 1;
                letter.State = LetterState.Accepted; // Letter enters queue in Accepted state
                return i + 1;
            }
        }
        return 0; // Queue full
    }
    
    // Add letter with leverage-aware positioning
    public int AddLetterWithObligationEffects(Letter letter)
    {
        if (letter == null) return 0;
        
        // Handle patron letters specially
        if (letter.IsFromPatron)
        {
            return AddPatronLetter(letter);
        }
        
        // Calculate leverage position
        int targetPosition = CalculateLeveragePosition(letter);
        
        return AddLetterWithLeverage(letter, targetPosition);
    }
    
    // Calculate leverage-based entry position for a letter
    private int CalculateLeveragePosition(Letter letter)
    {
        // Get base position from social status
        int basePosition = GetBasePositionForTokenType(letter.TokenType);
        
        // Get token balance with sender
        var senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId))
        {
            return basePosition; // Default if NPC not found
        }
        
        var npcTokens = _connectionTokenManager.GetTokensWithNPC(senderId);
        var tokenBalance = npcTokens[letter.TokenType];
        
        // Apply token-based leverage
        int leveragePosition = basePosition;
        
        if (tokenBalance < 0)
        {
            // Debt creates leverage - each negative token moves position up
            leveragePosition += tokenBalance; // Subtracts since negative
        }
        else if (tokenBalance >= 4)
        {
            // High positive relationship reduces leverage
            leveragePosition += 1;
        }
        
        // Apply pattern modifiers
        leveragePosition = ApplyPatternModifiers(leveragePosition, senderId, tokenBalance);
        
        // Apply obligation modifiers
        leveragePosition = _obligationManager.ApplyLeverageModifiers(letter, leveragePosition);
        
        // Clamp to valid queue range
        return Math.Max(1, Math.Min(8, leveragePosition));
    }
    
    // Get base position for token type
    private int GetBasePositionForTokenType(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Noble => 3,    // Noble base position
            ConnectionType.Trade => 5,    // Trade base position  
            ConnectionType.Shadow => 5,   // Shadow base position
            ConnectionType.Common => 7,   // Common base position
            ConnectionType.Trust => 7,    // Trust base position
            _ => 7                        // Default to lowest priority
        };
    }
    
    // Apply relationship pattern modifiers
    private int ApplyPatternModifiers(int currentPosition, string npcId, int tokenBalance)
    {
        var player = _gameWorld.GetPlayer();
        var history = player.NPCLetterHistory.GetValueOrDefault(npcId);
        if (history == null) return currentPosition;
        
        // Repeated skipping creates leverage even without debt
        if (history.SkippedCount >= 2 && tokenBalance >= 0)
        {
            currentPosition -= 1; // More leverage due to pattern
        }
        
        return currentPosition;
    }
    
    // Add letter with leverage-based displacement
    private int AddLetterWithLeverage(Letter letter, int targetPosition)
    {
        var queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Check if queue is completely full
        if (IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                $"🚫 Cannot accept letter from {letter.SenderName} - your queue is completely full!",
                SystemMessageTypes.Danger
            );
            return 0;
        }
        
        // If target position is empty, simple insertion
        if (queue[targetPosition - 1] == null)
        {
            queue[targetPosition - 1] = letter;
            letter.QueuePosition = targetPosition;
            letter.State = LetterState.Accepted;
            
            // Show leverage narrative if position differs from normal
            ShowLeverageNarrative(letter, targetPosition);
            
            return targetPosition;
        }
        
        // Target occupied - need displacement
        return DisplaceAndInsertLetter(letter, targetPosition);
    }
    
    // Displace letters to insert at leverage position
    private int DisplaceAndInsertLetter(Letter letter, int targetPosition)
    {
        var queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Announce the leverage-based displacement
        ShowLeverageDisplacement(letter, targetPosition);
        
        // Collect all letters from target position downward
        var lettersToDisplace = new System.Collections.Generic.List<Letter>();
        for (int i = targetPosition - 1; i < 8; i++)
        {
            if (queue[i] != null)
            {
                lettersToDisplace.Add(queue[i]);
                queue[i] = null; // Clear old position
            }
        }
        
        // Insert new letter at its leverage position
        queue[targetPosition - 1] = letter;
        letter.QueuePosition = targetPosition;
        letter.State = LetterState.Accepted;
        
        // Reinsert displaced letters
        int nextAvailable = targetPosition;
        foreach (var displaced in lettersToDisplace)
        {
            nextAvailable++;
            if (nextAvailable <= 8)
            {
                queue[nextAvailable - 1] = displaced;
                displaced.QueuePosition = nextAvailable;
                NotifyLetterShifted(displaced, nextAvailable);
            }
            else
            {
                HandleQueueOverflow(displaced);
            }
        }
        
        return targetPosition;
    }
    
    // Show leverage narrative when letter enters
    private void ShowLeverageNarrative(Letter letter, int position)
    {
        var basePosition = GetBasePositionForTokenType(letter.TokenType);
        var senderId = GetNPCIdByName(letter.SenderName);
        var tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[letter.TokenType];
        
        if (tokenBalance < 0)
        {
            // Debt leverage narrative
            _messageSystem.AddSystemMessage(
                $"💸 {letter.SenderName} has LEVERAGE! Your debt gives them power.",
                SystemMessageTypes.Warning
            );
            _messageSystem.AddSystemMessage(
                $"  • Enters at position {position} (normally {basePosition}) due to {Math.Abs(tokenBalance)} token debt",
                SystemMessageTypes.Info
            );
            
            if (position <= 3 && basePosition >= 5)
            {
                _messageSystem.AddSystemMessage(
                    $"  • Social hierarchy inverts when you owe money!",
                    SystemMessageTypes.Warning
                );
            }
        }
        else if (position > basePosition)
        {
            // Reduced leverage narrative
            _messageSystem.AddSystemMessage(
                $"✨ Strong relationship with {letter.SenderName} reduces their demands.",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • Enters at position {position} (normally {basePosition}) due to mutual respect",
                SystemMessageTypes.Info
            );
        }
        else
        {
            // Normal entry
            string urgency = letter.Deadline <= 3 ? " ⚠️" : "";
            _messageSystem.AddSystemMessage(
                $"📨 New letter from {letter.SenderName} enters queue at position {position}{urgency}",
                SystemMessageTypes.Info
            );
        }
    }
    
    // Show displacement narrative
    private void ShowLeverageDisplacement(Letter letter, int targetPosition)
    {
        var senderId = GetNPCIdByName(letter.SenderName);
        var tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[letter.TokenType];
        
        if (tokenBalance < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚡ {letter.SenderName} demands position {targetPosition} - you owe them!",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                $"  • Your {Math.Abs(tokenBalance)} token debt gives them power to displace others",
                SystemMessageTypes.Warning
            );
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"📬 {letter.SenderName}'s letter pushes into position {targetPosition}",
                SystemMessageTypes.Warning
            );
        }
    }
    
    // Notify when letter is shifted
    private void NotifyLetterShifted(Letter letter, int newPosition)
    {
        string urgency = letter.Deadline <= 2 ? " 🆘" : "";
        _messageSystem.AddSystemMessage(
            $"  • {letter.SenderName}'s letter pushed to position {newPosition}{urgency}",
            letter.Deadline <= 2 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
        );
    }
    
    // Handle letter pushed out of queue
    private void HandleQueueOverflow(Letter overflowLetter)
    {
        _messageSystem.AddSystemMessage(
            $"💥 {overflowLetter.SenderName}'s letter FORCED OUT by leverage!",
            SystemMessageTypes.Danger
        );
        
        _messageSystem.AddSystemMessage(
            $"  • The weight of your debts crushes other obligations",
            SystemMessageTypes.Warning
        );
        
        // Apply relationship damage - sender doesn't care WHY their letter was dropped
        var senderId = GetNPCIdByName(overflowLetter.SenderName);
        int tokenPenalty = 2; // Same penalty as expiration
        
        _connectionTokenManager.RemoveTokensFromNPC(overflowLetter.TokenType, tokenPenalty, senderId);
        
        _messageSystem.AddSystemMessage(
            $"💔 Lost {tokenPenalty} {overflowLetter.TokenType} tokens with {overflowLetter.SenderName}!",
            SystemMessageTypes.Danger
        );
        
        _messageSystem.AddSystemMessage(
            $"  • \"{overflowLetter.SenderName} won't care that you were 'forced' - you failed to deliver.\"",
            SystemMessageTypes.Warning
        );
        
        // Record in history
        var player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }
        player.NPCLetterHistory[senderId].RecordExpiry(); // Use existing expiry tracking
    }
    
    // Handle patron letters that jump to top positions
    public int AddPatronLetter(Letter letter)
    {
        var queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Show dramatic patron letter arrival
        _messageSystem.AddSystemMessage(
            $"🌟 A GOLD-SEALED LETTER ARRIVES FROM YOUR PATRON!",
            SystemMessageTypes.Warning
        );
        
        // Determine target position (1-3)
        int targetPos = 0;
        if (letter.IsPatronLetter && letter.PatronQueuePosition > 0 && letter.PatronQueuePosition <= 3)
        {
            targetPos = letter.PatronQueuePosition - 1; // Convert to 0-based
        }
        else
        {
            // Choose random position 1-3 if not specified
            targetPos = _random.Next(0, 3);
        }
        
        // Check if queue is completely full
        if (IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                $"🚫 CRISIS: Cannot accept patron letter - queue completely full!",
                SystemMessageTypes.Danger
            );
            
            _messageSystem.AddSystemMessage(
                $"  • Your patron will not be pleased with this failure",
                SystemMessageTypes.Danger
            );
            
            return 0;
        }
        
        // If target position is empty, simple placement
        if (queue[targetPos] == null)
        {
            queue[targetPos] = letter;
            letter.QueuePosition = targetPos + 1;
            
            _messageSystem.AddSystemMessage(
                $"  • Commands priority position {targetPos + 1} - all other obligations must wait!",
                SystemMessageTypes.Warning
            );
            
            _messageSystem.AddSystemMessage(
                $"  • Your mysterious patron's needs supersede all else",
                SystemMessageTypes.Info
            );
            
            return targetPos + 1;
        }
        
        // Target position is occupied - need to push letters down
        _messageSystem.AddSystemMessage(
            $"  ⚡ Patron demands position {targetPos + 1} - displacing existing obligations!",
            SystemMessageTypes.Warning
        );
        
        // Collect all letters that need to be pushed down
        var lettersToPush = new System.Collections.Generic.List<Letter>();
        for (int i = targetPos; i < 8; i++)
        {
            if (queue[i] != null)
            {
                lettersToPush.Add(queue[i]);
                queue[i] = null; // Clear old position
            }
        }
        
        // Place patron letter at target position
        queue[targetPos] = letter;
        letter.QueuePosition = targetPos + 1;
        
        _messageSystem.AddSystemMessage(
            $"  • Patron letter seizes position {targetPos + 1}!",
            SystemMessageTypes.Success
        );
        
        // Push other letters down
        int nextAvailable = targetPos + 1;
        var pushedLetters = new System.Collections.Generic.List<string>();
        
        foreach (var pushedLetter in lettersToPush)
        {
            // Find next available slot
            while (nextAvailable < 8 && queue[nextAvailable] != null)
            {
                nextAvailable++;
            }
            
            if (nextAvailable < 8)
            {
                // Place pushed letter
                int oldPos = pushedLetter.QueuePosition;
                queue[nextAvailable] = pushedLetter;
                pushedLetter.QueuePosition = nextAvailable + 1;
                
                pushedLetters.Add($"{pushedLetter.SenderName}'s letter: {oldPos} → {pushedLetter.QueuePosition}");
                nextAvailable++;
            }
            else
            {
                // No room - letter falls off queue
                _messageSystem.AddSystemMessage(
                    $"  💥 {pushedLetter.SenderName}'s letter PUSHED OUT OF QUEUE!",
                    SystemMessageTypes.Danger
                );
                
                // Apply relationship damage for forced removal
                ApplyRelationshipDamage(pushedLetter, _connectionTokenManager);
                
                _messageSystem.AddSystemMessage(
                    $"  • Your patron's demands have cost you dearly with {pushedLetter.SenderName}",
                    SystemMessageTypes.Danger
                );
            }
        }
        
        // Show all the disruption
        if (pushedLetters.Any())
        {
            _messageSystem.AddSystemMessage(
                $"📬 Queue disrupted by patron's authority:",
                SystemMessageTypes.Warning
            );
            
            foreach (var pushed in pushedLetters)
            {
                _messageSystem.AddSystemMessage(
                    $"  • {pushed}",
                    SystemMessageTypes.Info
                );
            }
        }
        
        _messageSystem.AddSystemMessage(
            $"  • The gold seal brooks no argument - your patron's will is absolute",
            SystemMessageTypes.Info
        );
        
        return targetPos + 1;
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
    
    // Check if position 1 has a letter AND it's collected
    public bool CanDeliverFromPosition1()
    {
        var letter = GetLetterAt(1);
        if (letter == null) return false;
        
        // Must be collected to deliver
        if (letter.State != LetterState.Collected)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ Cannot deliver! Letter from {letter.SenderName} is not collected yet.",
                SystemMessageTypes.Warning
            );
            _messageSystem.AddSystemMessage(
                $"  • Visit {letter.SenderName} to collect the physical letter",
                SystemMessageTypes.Info
            );
            return false;
        }
        
        return true;
    }
    
    // Generate a letter from an NPC and add to queue
    public Letter GenerateLetterFromNPC(NPC npc)
    {
        if (npc == null) return null;
        
        // Check if queue has space
        var player = _gameWorld.GetPlayer();
        var queue = player.LetterQueue;
        var emptySlot = queue.Any(l => l == null);
        
        if (!emptySlot)
        {
            _messageSystem.AddSystemMessage(
                "❌ Letter queue is full! Make room before accepting more letters.",
                SystemMessageTypes.Danger
            );
            return null;
        }
        
        // Generate letter based on NPC token type
        var tokenType = npc.LetterTokenTypes.FirstOrDefault();
        var letter = _letterTemplateRepository.GenerateLetterFromNPC(npc.ID, npc.Name, tokenType);
        
        if (letter != null)
        {
            // Calculate leverage for position
            var tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            var tokenBalance = tokens.Values.Sum(); // Sum of all tokens with this NPC
            
            // Determine position based on token type
            int basePosition = tokenType switch
            {
                ConnectionType.Noble => 3,
                ConnectionType.Trade => 5,
                ConnectionType.Shadow => 5,
                ConnectionType.Common => 7,
                ConnectionType.Trust => 7,
                _ => 7
            };
            
            // If in debt (negative tokens), letter gets higher priority
            int leverage = tokenBalance < 0 ? Math.Abs(tokenBalance) : 0;
            int targetPosition = Math.Max(1, Math.Min(8, basePosition - leverage));
            
            // Add to queue at calculated position
            AddLetterToQueue(letter, targetPosition);
        }
        
        return letter;
    }
    
    // Deliver letter from position 1
    public bool DeliverFromPosition1()
    {
        if (!CanDeliverFromPosition1()) return false;
        
        var player = _gameWorld.GetPlayer();
        var letter = GetLetterAt(1);
        
        // Remove from queue
        player.LetterQueue[0] = null;
        
        // Shift all letters up
        CompressQueue();
        
        // Pay the player
        player.ModifyCoins(letter.Payment);
        
        // Record delivery
        RecordLetterDelivery(letter);
        
        return true;
    }
    
    // Compress queue by shifting letters up to fill gaps
    private void CompressQueue()
    {
        var player = _gameWorld.GetPlayer();
        int writeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (player.LetterQueue[i] != null)
            {
                if (i != writeIndex)
                {
                    player.LetterQueue[writeIndex] = player.LetterQueue[i];
                    player.LetterQueue[i] = null;
                }
                writeIndex++;
            }
        }
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
        
        // Get the sender NPC for narrative context
        var senderNpc = _npcRepository.GetNPCById(senderId);
        
        // Show the dramatic moment of failure
        _messageSystem.AddSystemMessage(
            $"⏰ TIME'S UP! {letter.SenderName}'s letter has expired!",
            SystemMessageTypes.Danger
        );
        
        // Remove tokens from the relationship with this NPC
        _connectionTokenManager.RemoveTokensFromNPC(letter.TokenType, tokenPenalty, senderId);
        
        // Record the expiry in letter history
        var player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }
        player.NPCLetterHistory[senderId].RecordExpiry();
        
        // Show the relationship damage with narrative weight
        _messageSystem.AddSystemMessage(
            $"💔 Lost {tokenPenalty} {letter.TokenType} tokens with {letter.SenderName}. Trust broken.",
            SystemMessageTypes.Danger
        );
        
        // Add contextual reaction based on NPC type
        if (senderNpc != null)
        {
            string consequence = GetExpiryConsequence(senderNpc, letter);
            _messageSystem.AddSystemMessage(
                $"  • {consequence}",
                SystemMessageTypes.Warning
            );
        }
        
        // Show cumulative damage
        var history = player.NPCLetterHistory[senderId];
        if (history.ExpiredCount > 1)
        {
            _messageSystem.AddSystemMessage(
                $"  ⚠️ This is the {GetOrdinal(history.ExpiredCount)} letter from {letter.SenderName} you've let expire.",
                SystemMessageTypes.Danger
            );
        }
    }
    
    // Get contextual consequence for expired letter
    private string GetExpiryConsequence(NPC npc, Letter letter)
    {
        if (npc.LetterTokenTypes.Contains(ConnectionType.Trust))
        {
            return $"{npc.Name} waited for your help that never came. Some wounds don't heal.";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Trade))
        {
            return $"{npc.Name}'s opportunity has passed. 'Time is money, and you've cost me both.'";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Noble))
        {
            return $"Word of your failure reaches {npc.Name}. Your reputation in court circles suffers.";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Shadow))
        {
            return $"{npc.Name} doesn't forget broken promises. You've made an enemy in dark places.";
        }
        else
        {
            return $"{npc.Name} needed this delivered on time. Another bridge burned.";
        }
    }
    
    // Helper to get ordinal string (1st, 2nd, 3rd, etc.)
    private string GetOrdinal(int number)
    {
        if (number <= 0) return number.ToString();
        
        switch (number % 100)
        {
            case 11:
            case 12:
            case 13:
                return number + "th";
        }
        
        switch (number % 10)
        {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
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
        
        // Get the sender NPC for narrative context
        var senderNpc = _npcRepository.GetNPCById(senderId);
        if (senderNpc != null)
        {
            // Show the relationship improvement
            _messageSystem.AddSystemMessage(
                $"👥 {letter.SenderName} appreciates your reliable service.",
                SystemMessageTypes.Success
            );
            
            // Show trust building based on delivery history
            var history = player.NPCLetterHistory[senderId];
            if (history.DeliveredCount == 1)
            {
                _messageSystem.AddSystemMessage(
                    $"  • First successful delivery to {letter.SenderName} - a good start to your relationship.",
                    SystemMessageTypes.Info
                );
            }
            else if (history.DeliveredCount % 5 == 0)
            {
                _messageSystem.AddSystemMessage(
                    $"  • {history.DeliveredCount} letters delivered! Your reputation with {letter.SenderName} grows stronger.",
                    SystemMessageTypes.Success
                );
            }
        }
        
        // Process chain letters
        ProcessChainLetters(letter);
        
        // Store delivered letter info in player state for scenario tracking
        if (!player.DeliveredLetters.Contains(letter))
        {
            player.DeliveredLetters.Add(letter);
            player.TotalLettersDelivered++;
        }
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
        
        // Get the sender NPC for narrative context
        var senderNpc = _npcRepository.GetNPCById(senderId);
        if (senderNpc != null)
        {
            // Show the relationship damage based on NPC personality
            string reaction = GetSkipReaction(senderNpc, letter);
            _messageSystem.AddSystemMessage(
                $"💔 {reaction}",
                SystemMessageTypes.Warning
            );
            
            // Show cumulative damage if multiple skips
            var history = player.NPCLetterHistory[senderId];
            if (history.SkippedCount > 1)
            {
                _messageSystem.AddSystemMessage(
                    $"  • You've now skipped {history.SkippedCount} letters from {letter.SenderName}. Trust erodes.",
                    SystemMessageTypes.Danger
                );
            }
            
            // Warn about potential consequences
            if (history.SkippedCount >= 3)
            {
                _messageSystem.AddSystemMessage(
                    $"  ⚠️ {letter.SenderName} may stop offering you letters if this continues.",
                    SystemMessageTypes.Danger
                );
            }
        }
    }
    
    // Get contextual skip reaction based on NPC type
    private string GetSkipReaction(NPC npc, Letter letter)
    {
        // Generate reaction based on NPC's token types and profession
        if (npc.LetterTokenTypes.Contains(ConnectionType.Trust))
        {
            return $"{npc.Name} looks hurt as you prioritize other obligations over their {GetTokenTypeDescription(letter.TokenType)} request.";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Trade))
        {
            return $"{npc.Name} frowns at the delay. 'Business waits for no one,' they mutter.";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Noble))
        {
            return $"{npc.Name} raises an eyebrow coldly. Such delays are noted in aristocratic circles.";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Shadow))
        {
            return $"{npc.Name}'s eyes narrow dangerously. Broken promises have consequences in the shadows.";
        }
        else
        {
            return $"{npc.Name} notices you skipping their letter. Another promise deferred.";
        }
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
        
        // If no letters to shift, we're done
        if (!remainingLetters.Any()) return;
        
        // Announce the queue reorganization
        _messageSystem.AddSystemMessage(
            "📬 Your remaining obligations shift forward:",
            SystemMessageTypes.Info
        );
        
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
                int oldPosition = letter.QueuePosition;
                queue[writePosition] = letter;
                letter.QueuePosition = writePosition + 1; // Convert back to 1-based
                
                // Provide narrative context for each letter moving
                string urgency = letter.Deadline <= 2 ? " ⚠️ URGENT!" : "";
                string deadlineText = letter.Deadline == 1 ? "expires tomorrow!" : $"{letter.Deadline} days left";
                
                _messageSystem.AddSystemMessage(
                    $"  • {letter.SenderName}'s letter moves from slot {oldPosition} → {letter.QueuePosition} ({deadlineText}){urgency}",
                    letter.Deadline <= 2 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
                );
            }
        }
    }
    
    // Create conversation context for skip delivery action
    public QueueManagementContext CreateSkipDeliverContext(int position)
    {
        if (position <= 1 || position > 8) return null;
        
        var letter = GetLetterAt(position);
        if (letter == null) return null;
        
        // Check if position 1 is occupied
        if (GetLetterAt(1) != null) return null;
        
        // Calculate token cost and get skipped letters
        int tokenCost = position - 1;
        var skippedLetters = new Dictionary<int, Letter>();
        
        for (int i = 2; i < position; i++)
        {
            var skippedLetter = GetLetterAt(i);
            if (skippedLetter != null)
            {
                skippedLetters[i] = skippedLetter;
            }
        }
        
        return new QueueManagementContext
        {
            TargetLetter = letter,
            ManagementAction = "SkipDeliver",
            TokenCost = tokenCost,
            SkippedLetters = skippedLetters,
            GameWorld = _gameWorld,
            Player = _gameWorld.GetPlayer()
        };
    }
    
    // Trigger conversation for skip delivery
    public async Task<bool> TriggerSkipConversation(int position)
    {
        var context = CreateSkipDeliverContext(position);
        if (context == null)
        {
            _messageSystem.AddSystemMessage("Cannot skip this letter.", SystemMessageTypes.Warning);
            return false;
        }
        
        // Store the skip position for later processing
        _gameWorld.SetMetadata("PendingSkipPosition", position.ToString());
        
        // Create conversation
        var conversation = await _conversationFactory.CreateConversation(context, _gameWorld.GetPlayer());
        
        // Set as pending conversation
        _gameWorld.PendingConversationManager = conversation;
        _gameWorld.ConversationPending = true;
        
        return true;
    }
    
    // Create conversation context for purge action
    public QueueManagementContext CreatePurgeContext()
    {
        var letter = GetLetterAt(8);
        if (letter == null) return null;
        
        // Check if purging is forbidden
        if (_obligationManager.IsActionForbidden("purge", letter, out string reason))
        {
            return null;
        }
        
        return new QueueManagementContext
        {
            TargetLetter = letter,
            ManagementAction = "Purge",
            TokenCost = 3,
            GameWorld = _gameWorld,
            Player = _gameWorld.GetPlayer()
        };
    }
    
    // Trigger conversation for purge action
    public async Task<bool> TriggerPurgeConversation()
    {
        var context = CreatePurgeContext();
        if (context == null)
        {
            _messageSystem.AddSystemMessage("Cannot purge - no letter in position 8 or action forbidden.", SystemMessageTypes.Warning);
            return false;
        }
        
        // Store purge flag for later processing
        _gameWorld.SetMetadata("PendingPurgePosition", "8");
        
        // Create conversation
        var conversation = await _conversationFactory.CreateConversation(context, _gameWorld.GetPlayer());
        
        // Set as pending conversation
        _gameWorld.PendingConversationManager = conversation;
        _gameWorld.ConversationPending = true;
        
        return true;
    }
    
    // Skip letter delivery by spending tokens
    public bool TrySkipDeliver(int position)
    {
        if (position <= 1 || position > 8) return false;
        
        var letter = GetLetterAt(position);
        if (letter == null) return false;
        
        // Check if position 1 is occupied (can't skip to occupied slot)
        if (GetLetterAt(1) != null)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Cannot skip - position 1 is already occupied!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Calculate token cost: position - 1 (skip from position 3 costs 2 tokens)
        int baseCost = position - 1;
        int multiplier = _obligationManager.CalculateSkipCostMultiplier(letter);
        int tokenCost = baseCost * multiplier;
        
        // Get sender NPC for narrative context
        var senderId = GetNPCIdByName(letter.SenderName);
        
        // Show what's about to happen
        _messageSystem.AddSystemMessage(
            $"💸 Attempting to skip {letter.SenderName}'s letter to position 1...",
            SystemMessageTypes.Info
        );
        
        // Validate token availability through ConnectionTokenManager
        if (!_connectionTokenManager.HasTokens(letter.TokenType, tokenCost))
        {
            _messageSystem.AddSystemMessage(
                $"  ❌ Insufficient {letter.TokenType} tokens! Need {tokenCost}, have {_connectionTokenManager.GetTokenCount(letter.TokenType)}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Spend the tokens with specific NPC context
        _messageSystem.AddSystemMessage(
            $"  • Spending {tokenCost} {letter.TokenType} tokens with {letter.SenderName}...",
            SystemMessageTypes.Warning
        );
        
        if (!_connectionTokenManager.SpendTokensWithNPC(letter.TokenType, tokenCost, senderId))
        {
            return false;
        }
        
        // Move letter to position 1
        var queue = _gameWorld.GetPlayer().LetterQueue;
        queue[0] = letter; // Position 1 is index 0
        queue[position - 1] = null; // Clear original position
        letter.QueuePosition = 1;
        
        // Success narrative
        _messageSystem.AddSystemMessage(
            $"✅ {letter.SenderName}'s letter jumps the queue to position 1!",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            $"  • You call in {tokenCost} favors with {letter.SenderName} for urgent handling",
            SystemMessageTypes.Info
        );
        
        // Shift remaining letters
        ShiftQueueUp(position);
        
        // Track the skip
        RecordLetterSkip(letter);
        
        return true;
    }
    
    // Generate 1-2 daily letters from available NPCs and templates
    public int GenerateDailyLetters()
    {
        // Generate 1-2 letters per day
        int lettersToGenerate = _random.Next(1, 3);
        int lettersGenerated = 0;
        
        // Check if we can generate any letters
        if (IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                "📬 Your letter queue is full - no new correspondence can be accepted today.",
                SystemMessageTypes.Warning
            );
            return 0;
        }
        
        // Announce the arrival of new correspondence
        _messageSystem.AddSystemMessage(
            "🌅 Dawn brings new correspondence:",
            SystemMessageTypes.Info
        );
        
        for (int i = 0; i < lettersToGenerate; i++)
        {
            // Check if queue has space
            if (IsQueueFull())
            {
                _messageSystem.AddSystemMessage(
                    "  📭 Additional letters arrive but your queue is now full.",
                    SystemMessageTypes.Warning
                );
                break;
            }
            
            // Get random NPCs for sender and recipient
            var allNpcs = _npcRepository.GetAllNPCs();
            if (allNpcs.Count < 2) continue; // Need at least 2 NPCs
            
            // Find NPCs who can send letters (have token types and player has relationship)
            var eligibleSenders = allNpcs.Where(npc => 
                npc.LetterTokenTypes.Any() && 
                _categoryService.CanNPCOfferLetters(npc.ID)).ToList();
                
            if (!eligibleSenders.Any()) 
            {
                // Fallback to any NPC with letter token types
                eligibleSenders = allNpcs.Where(npc => npc.LetterTokenTypes.Any()).ToList();
            }
            
            if (!eligibleSenders.Any()) continue;
            
            var sender = eligibleSenders[_random.Next(eligibleSenders.Count)];
            
            // Pick a token type the sender can offer
            var availableCategories = _categoryService.GetAvailableCategories(sender.ID);
            ConnectionType tokenType;
            
            if (availableCategories.Any())
            {
                // Use a token type where we have enough relationship
                var tokenTypes = availableCategories.Keys.ToList();
                tokenType = tokenTypes[_random.Next(tokenTypes.Count)];
            }
            else
            {
                // Fallback to sender's first token type
                tokenType = sender.LetterTokenTypes.FirstOrDefault();
                if (tokenType == default) continue;
            }
            
            // Generate letter respecting category thresholds
            var letter = _letterTemplateRepository.GenerateLetterFromNPC(sender.ID, sender.Name, tokenType);
            if (letter == null)
            {
                // Try with basic template if category system fails
                var template = _letterTemplateRepository.GetRandomTemplateByTokenType(tokenType);
                if (template == null) continue;
                
                var recipient = allNpcs.Where(n => n.ID != sender.ID).FirstOrDefault();
                if (recipient == null) continue;
                
                letter = _letterTemplateRepository.GenerateLetterFromTemplate(template, sender.Name, recipient.Name);
            }
            
            if (letter != null)
            {
                // Add to first empty slot
                int position = AddLetter(letter);
                if (position > 0)
                {
                    lettersGenerated++;
                    
                    // Narrative context for each new letter
                    string urgency = letter.Deadline <= 3 ? " - needs urgent delivery!" : "";
                    string tokenTypeText = GetTokenTypeDescription(letter.TokenType);
                    
                    // Show category if from relationship
                    var category = _categoryService.GetAvailableCategory(sender.ID, tokenType);
                    string categoryText = "";
                    if (_categoryService.CanNPCOfferLetters(sender.ID))
                    {
                        categoryText = $" [{category} letter]";
                    }
                    
                    _messageSystem.AddSystemMessage(
                        $"  • Letter from {sender.Name} to {letter.RecipientName} ({tokenTypeText} correspondence{categoryText}){urgency}",
                        letter.Deadline <= 3 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
                    );
                    
                    _messageSystem.AddSystemMessage(
                        $"    → Enters your queue at position {position} - {letter.Payment} coins on delivery",
                        SystemMessageTypes.Info
                    );
                }
            }
        }
        
        if (lettersGenerated == 0 && !IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                "  📭 No new letters arrive today. The roads are quiet.",
                SystemMessageTypes.Info
            );
        }
        
        return lettersGenerated;
    }
    
    // Helper method to get descriptive text for token types
    private string GetTokenTypeDescription(ConnectionType tokenType)
    {
        switch (tokenType)
        {
            case ConnectionType.Trust:
                return "personal";
            case ConnectionType.Trade:
                return "commercial";
            case ConnectionType.Noble:
                return "aristocratic";
            case ConnectionType.Common:
                return "local";
            case ConnectionType.Shadow:
                return "clandestine";
            default:
                return tokenType.ToString().ToLower();
        }
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
            _messageSystem.AddSystemMessage(
                $"❌ No letter in position 8 to purge!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Show what's at stake
        _messageSystem.AddSystemMessage(
            $"🔥 PURGING: Preparing to destroy {letterToPurge.SenderName}'s letter...",
            SystemMessageTypes.Danger
        );
        
        // Check if purging this letter is forbidden by obligations
        if (_obligationManager.IsActionForbidden("purge", letterToPurge, out string reason))
        {
            _messageSystem.AddSystemMessage(
                $"  ❌ Cannot purge: {reason}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Validate token payment totals 3
        int totalTokens = tokenPayment.Values.Sum();
        if (totalTokens != 3)
        {
            _messageSystem.AddSystemMessage(
                $"  ❌ Purging requires exactly 3 tokens! You offered {totalTokens}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Check if player has enough tokens
        foreach (var payment in tokenPayment)
        {
            if (!_connectionTokenManager.HasTokens(payment.Key, payment.Value))
            {
                _messageSystem.AddSystemMessage(
                    $"  ❌ Insufficient {payment.Key} tokens! Need {payment.Value}, have {_connectionTokenManager.GetTokenCount(payment.Key)}",
                    SystemMessageTypes.Danger
                );
                return false;
            }
        }
        
        // Show the desperate measure being taken
        _messageSystem.AddSystemMessage(
            $"  💸 Burning social capital to make this letter disappear...",
            SystemMessageTypes.Warning
        );
        
        // Spend the tokens with narrative weight
        foreach (var payment in tokenPayment)
        {
            if (payment.Value > 0)
            {
                _messageSystem.AddSystemMessage(
                    $"    • Spending {payment.Value} {payment.Key} token{(payment.Value > 1 ? "s" : "")}",
                    SystemMessageTypes.Warning
                );
                
                if (!_connectionTokenManager.SpendTokens(payment.Key, payment.Value))
                {
                    return false;
                }
            }
        }
        
        // Get sender info for final narrative
        var senderId = GetNPCIdByName(letterToPurge.SenderName);
        
        // Remove the letter from position 8
        RemoveLetterFromQueue(8);
        
        // Dramatic conclusion
        _messageSystem.AddSystemMessage(
            $"🔥 {letterToPurge.SenderName}'s letter has been destroyed!",
            SystemMessageTypes.Danger
        );
        
        _messageSystem.AddSystemMessage(
            $"  • The obligation is gone, but at what cost?",
            SystemMessageTypes.Warning
        );
        
        _messageSystem.AddSystemMessage(
            $"  • {letterToPurge.SenderName} will never know their letter was purged... you hope.",
            SystemMessageTypes.Info
        );
        
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
            _messageSystem.AddSystemMessage(
                $"❌ Cannot priority move - position 1 is already occupied!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Get sender NPC for narrative context
        var senderId = GetNPCIdByName(letter.SenderName);
        
        // Show the crisis requiring priority handling
        _messageSystem.AddSystemMessage(
            $"🎆 PRIORITY HANDLING: {letter.SenderName}'s letter needs urgent delivery!",
            SystemMessageTypes.Warning
        );
        
        // Check token cost (5 matching tokens)
        if (!_connectionTokenManager.HasTokens(letter.TokenType, 5))
        {
            _messageSystem.AddSystemMessage(
                $"  ❌ Insufficient {letter.TokenType} tokens! Need 5, have {_connectionTokenManager.GetTokenCount(letter.TokenType)}",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                $"  • Major favors require substantial social capital",
                SystemMessageTypes.Info
            );
            return false;
        }
        
        // Spend the tokens with dramatic weight
        _messageSystem.AddSystemMessage(
            $"  💸 Burning 5 {letter.TokenType} tokens with {letter.SenderName} for emergency priority...",
            SystemMessageTypes.Warning
        );
        
        if (!_connectionTokenManager.SpendTokensWithNPC(letter.TokenType, 5, senderId))
        {
            return false;
        }
        
        // Move letter to position 1
        var queue = _gameWorld.GetPlayer().LetterQueue;
        queue[fromPosition - 1] = null; // Clear original position
        queue[0] = letter; // Place in position 1
        letter.QueuePosition = 1;
        
        // Success narrative
        _messageSystem.AddSystemMessage(
            $"✅ {letter.SenderName}'s letter rockets to position 1!",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            $"  • You've called in major favors - this better be worth it",
            SystemMessageTypes.Warning
        );
        
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
        
        // Get sender NPC for narrative context
        var senderId = GetNPCIdByName(letter.SenderName);
        
        // Show the negotiation
        _messageSystem.AddSystemMessage(
            $"📅 Negotiating deadline extension with {letter.SenderName}...",
            SystemMessageTypes.Info
        );
        
        // Show current deadline pressure
        string urgency = letter.Deadline <= 2 ? " 🆘 CRITICAL!" : "";
        _messageSystem.AddSystemMessage(
            $"  • Current deadline: {letter.Deadline} days{urgency}",
            letter.Deadline <= 2 ? SystemMessageTypes.Danger : SystemMessageTypes.Info
        );
        
        // Check token cost (2 matching tokens)
        if (!_connectionTokenManager.HasTokens(letter.TokenType, 2))
        {
            _messageSystem.AddSystemMessage(
                $"  ❌ Insufficient {letter.TokenType} tokens! Need 2, have {_connectionTokenManager.GetTokenCount(letter.TokenType)}",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                $"  • {letter.SenderName} won't grant extensions without compensation",
                SystemMessageTypes.Info
            );
            return false;
        }
        
        // Spend the tokens
        _messageSystem.AddSystemMessage(
            $"  • Offering 2 {letter.TokenType} tokens to {letter.SenderName}...",
            SystemMessageTypes.Warning
        );
        
        if (!_connectionTokenManager.SpendTokensWithNPC(letter.TokenType, 2, senderId))
        {
            return false;
        }
        
        // Extend the deadline
        int oldDeadline = letter.Deadline;
        letter.Deadline += 2;
        
        // Success narrative
        _messageSystem.AddSystemMessage(
            $"✅ {letter.SenderName} grants a 2-day extension!",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            $"  • New deadline: {letter.Deadline} days (was {oldDeadline})",
            SystemMessageTypes.Info
        );
        
        _messageSystem.AddSystemMessage(
            $"  • \"Just this once,\" {letter.SenderName} says, \"but don't make a habit of it.\"",
            SystemMessageTypes.Info
        );
        
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