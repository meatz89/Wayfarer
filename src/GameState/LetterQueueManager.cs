using System;
using System.Linq;
using Wayfarer.GameState;

public class LetterQueueManager
{
    private readonly GameWorld _gameWorld;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly StandingObligationManager _obligationManager;
    private readonly TokenMechanicsManager _connectionTokenManager;
    private readonly LetterCategoryService _categoryService;
    private readonly GameConfiguration _config;
    private readonly IGameRuleEngine _ruleEngine;
    private readonly ITimeManager _timeManager;
    private readonly Random _random = new Random();

    public LetterQueueManager(GameWorld gameWorld, LetterTemplateRepository letterTemplateRepository, NPCRepository npcRepository, MessageSystem messageSystem, StandingObligationManager obligationManager, TokenMechanicsManager connectionTokenManager, LetterCategoryService categoryService, GameConfiguration config, IGameRuleEngine ruleEngine, ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _letterTemplateRepository = letterTemplateRepository;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _obligationManager = obligationManager;
        _connectionTokenManager = connectionTokenManager;
        _categoryService = categoryService;
        _config = config;
        _ruleEngine = ruleEngine;
        _timeManager = timeManager;
    }

    // Get the player's letter queue
    public Letter[] GetPlayerQueue()
    {
        return _gameWorld.GetPlayer().LetterQueue;
    }
    
    // Get the LetterQueue object wrapper for weight management
    public LetterQueue GetLetterQueue()
    {
        var queue = new LetterQueue();
        var playerQueue = _gameWorld.GetPlayer().LetterQueue;
        
        // Populate the LetterQueue wrapper with current letters
        for (int i = 0; i < playerQueue.Length; i++)
        {
            if (playerQueue[i] != null)
            {
                queue.AddLetter(playerQueue[i], i + 1);
            }
        }
        
        return queue;
    }
    
    // Get active letters (non-null entries in the queue)
    public Letter[] GetActiveLetters()
    {
        var player = _gameWorld.GetPlayer();
        Console.WriteLine($"[GetActiveLetters] Player has {player.LetterQueue.Length} queue slots");
        
        var activeLetters = player.LetterQueue
            .Where(l => l != null)
            .ToArray();
            
        Console.WriteLine($"[GetActiveLetters] Found {activeLetters.Length} non-null letters");
        
        foreach (var letter in activeLetters)
        {
            Console.WriteLine($"  - Letter: {letter.Description}, State: {letter.State}");
        }
        
        var collected = activeLetters
            .Where(l => l.State == LetterState.Collected)
            .ToArray();
            
        Console.WriteLine($"[GetActiveLetters] Returning {collected.Length} collected letters");
        
        return collected;
    }

    // Add letter to queue at specific position
    public bool AddLetterToQueue(Letter letter, int position)
    {
        if (letter == null || position < 1 || position > _config.LetterQueue.MaxQueueSize) return false;

        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
        if (queue[position - 1] != null) return false; // Position occupied

        queue[position - 1] = letter;
        letter.QueuePosition = position;

        _messageSystem.AddSystemMessage(
            $"📨 Letter from {letter.SenderName} added to position {position}",
            SystemMessageTypes.Success
        );

        return true;
    }
    // Add letter to first available slot - queue fills from position 1
    public int AddLetter(Letter letter)
    {
        if (letter == null) return 0;

        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;

        // Find the FIRST empty slot, filling from position 1
        for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
        {
            if (queue[i] == null)
            {
                queue[i] = letter;
                letter.QueuePosition = i + 1;
                letter.State = LetterState.Collected; // Letter enters queue in Accepted state

                _messageSystem.AddSystemMessage(
                    $"📨 New letter from {letter.SenderName} enters queue at position {i + 1}",
                    SystemMessageTypes.Success
                );

                return i + 1;
            }
        }
        return 0; // Queue full
    }

    // Add letter with leverage-aware positioning
    public int AddLetterWithObligationEffects(Letter letter)
    {
        if (letter == null) return 0;

        // Apply deadline bonuses from obligations
        ApplyDeadlineBonuses(letter);

        // Patron letters use extreme leverage through debt, not special handling
        // Their massive debt (e.g., -20 tokens) naturally pushes them to top positions

        // Calculate leverage position
        int targetPosition = CalculateLeveragePosition(letter);

        return AddLetterWithLeverage(letter, targetPosition);
    }

    // Apply deadline bonuses from active obligations
    private void ApplyDeadlineBonuses(Letter letter)
    {
        List<StandingObligation> activeObligations = _obligationManager.GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            // Check if obligation applies to this letter type
            if (!obligation.AppliesTo(letter.TokenType)) continue;

            // Check for DeadlinePlus2Days effect
            if (obligation.HasEffect(ObligationEffect.DeadlinePlus2Days))
            {
                // Check if the letter is from the specific NPC if obligation is NPC-specific
                if (!string.IsNullOrEmpty(obligation.RelatedNPCId))
                {
                    NPC npc = _npcRepository.GetByName(letter.SenderName);
                    if (npc == null || npc.ID != obligation.RelatedNPCId) continue;
                }

                letter.DeadlineInHours += 48;
                _messageSystem.AddSystemMessage(
                    $"📅 {obligation.Name} grants +2 days to deadline for letter from {letter.SenderName}",
                    SystemMessageTypes.Info
                );
            }
        }

        // Apply dynamic deadline bonuses that scale with tokens
        _obligationManager.ApplyDynamicDeadlineBonuses(letter);
    }

    // Calculate relationship-based entry position using the queue position algorithm
    // Position = 8 - (highest positive token) + (worst negative token penalty)
    private int CalculateLeveragePosition(Letter letter)
    {
        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId))
        {
            // Default to position 8 if NPC not found
            return _config.LetterQueue.MaxQueueSize;
        }

        // Check for active obligations first - highest priority
        if (HasActiveObligationWithNPC(senderId))
        {
            return 1; // Obligation letters always enter at position 1
        }

        // Get all token balances with this NPC
        var allTokens = _connectionTokenManager.GetTokensWithNPC(senderId);
        
        // Calculate position using specification algorithm
        int highestPositiveToken = GetHighestPositiveToken(allTokens);
        int worstNegativeTokenPenalty = GetWorstNegativeTokenPenalty(allTokens);
        
        // Base algorithm: Position = 8 - (highest positive token) + (worst negative token penalty)
        int position = _config.LetterQueue.MaxQueueSize - highestPositiveToken + worstNegativeTokenPenalty;
        
        // Apply Commerce debt leverage override
        if (allTokens.ContainsKey(ConnectionType.Commerce) && allTokens[ConnectionType.Commerce] <= -3)
        {
            position = 2; // Commerce debt >= 3 forces position 2
        }
        
        // Clamp to valid queue range
        position = Math.Max(1, Math.Min(_config.LetterQueue.MaxQueueSize, position));
        
        // Record positioning data for UI translation
        RecordLetterPositioning(letter, senderId, allTokens, position, highestPositiveToken, worstNegativeTokenPenalty);
        
        return position;
    }

    /// <summary>
    /// Check if an NPC has any active standing obligations with the player
    /// </summary>
    private bool HasActiveObligationWithNPC(string npcId)
    {
        List<StandingObligation> activeObligations = _obligationManager.GetActiveObligations();
        return activeObligations.Any(obligation => obligation.RelatedNPCId == npcId);
    }

    /// <summary>
    /// Get the highest positive token value across all connection types with an NPC
    /// </summary>
    private int GetHighestPositiveToken(Dictionary<ConnectionType, int> allTokens)
    {
        int highest = 0;
        foreach (var tokenCount in allTokens.Values)
        {
            if (tokenCount > highest)
            {
                highest = tokenCount;
            }
        }
        return highest;
    }

    /// <summary>
    /// Get the worst negative token penalty (absolute value of most negative token)
    /// </summary>
    private int GetWorstNegativeTokenPenalty(Dictionary<ConnectionType, int> allTokens)
    {
        int worstPenalty = 0;
        foreach (var tokenCount in allTokens.Values)
        {
            if (tokenCount < 0)
            {
                int penalty = Math.Abs(tokenCount);
                if (penalty > worstPenalty)
                {
                    worstPenalty = penalty;
                }
            }
        }
        return worstPenalty;
    }

    /// <summary>
    /// Record letter positioning data for frontend translation
    /// Backend only sets categorical types - UI translates to text
    /// </summary>
    private void RecordLetterPositioning(Letter letter, string senderId, Dictionary<ConnectionType, int> allTokens, 
        int finalPosition, int highestPositiveToken, int worstNegativeTokenPenalty)
    {
        // Determine positioning reason category
        LetterPositioningReason reason = DeterminePositioningReason(senderId, allTokens, worstNegativeTokenPenalty);
        
        // Store categorical data on letter for UI translation
        letter.PositioningReason = reason;
        letter.RelationshipStrength = highestPositiveToken;
        letter.RelationshipDebt = worstNegativeTokenPenalty;
        letter.FinalQueuePosition = finalPosition;
        
        // Send categorical message for frontend translation
        _messageSystem.AddLetterPositioningMessage(letter.SenderName, reason, finalPosition, highestPositiveToken, worstNegativeTokenPenalty);
    }

    /// <summary>
    /// Determine categorical reason for letter positioning
    /// </summary>
    private LetterPositioningReason DeterminePositioningReason(string senderId, Dictionary<ConnectionType, int> allTokens, int worstNegativeTokenPenalty)
    {
        if (HasActiveObligationWithNPC(senderId))
        {
            return LetterPositioningReason.Obligation;
        }
        
        if (allTokens.ContainsKey(ConnectionType.Commerce) && allTokens[ConnectionType.Commerce] <= -3)
        {
            return LetterPositioningReason.CommerceDebt;
        }
        
        if (worstNegativeTokenPenalty > 0)
        {
            return LetterPositioningReason.PoorStanding;
        }
        
        if (GetHighestPositiveToken(allTokens) > 0)
        {
            return LetterPositioningReason.GoodStanding;
        }
        
        return LetterPositioningReason.Neutral;
    }

    // Add letter with leverage-based displacement
    private int AddLetterWithLeverage(Letter letter, int targetPosition)
    {
        if (!ValidateLetterCanBeAdded(letter))
            return 0;

        // Validate target position is within bounds
        if (targetPosition < 1 || targetPosition > _config.LetterQueue.MaxQueueSize)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ Invalid queue position {targetPosition} calculated for letter from {letter.SenderName}. Using default position.",
                SystemMessageTypes.Warning
            );
            targetPosition = Math.Max(1, Math.Min(_config.LetterQueue.MaxQueueSize, targetPosition));
        }

        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;

        // Additional safety check for array bounds
        if (targetPosition - 1 < 0 || targetPosition - 1 >= queue.Length)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Critical error: position {targetPosition} is outside queue bounds. Defaulting to last position.",
                SystemMessageTypes.Danger
            );
            targetPosition = _config.LetterQueue.MaxQueueSize;
        }

        // If target position is empty, simple insertion
        if (queue[targetPosition - 1] == null)
        {
            return InsertLetterAtPosition(letter, targetPosition);
        }

        // Target occupied - need displacement
        return DisplaceAndInsertLetter(letter, targetPosition);
    }

    // Get the current position of a letter in the queue
    public int? GetLetterPosition(string letterId)
    {
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i]?.Id == letterId)
            {
                return i + 1; // Return 1-based position
            }
        }
        return null;
    }

    // Validate that a letter can be added to the queue
    private bool ValidateLetterCanBeAdded(Letter letter)
    {
        if (IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                $"🚫 Cannot accept letter from {letter.SenderName} - your queue is completely full!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        return true;
    }

    // Insert letter at an empty position
    private int InsertLetterAtPosition(Letter letter, int position)
    {
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
        queue[position - 1] = letter;
        letter.QueuePosition = position;
        letter.State = LetterState.Collected;

        // Track original vs actual position for leverage visibility
        int basePosition = _config.LetterQueue.MaxQueueSize;
        if (position < basePosition)
        {
            letter.OriginalQueuePosition = basePosition;
            letter.LeverageBoost = basePosition - position;
        }

        // Show leverage narrative if position differs from normal
        ShowLeverageNarrative(letter, position);

        return position;
    }

    // Show narrative explaining why letter entered at this position
    private void ShowLeverageNarrative(Letter letter, int actualPosition)
    {
        int basePosition = _config.LetterQueue.MaxQueueSize;

        if (actualPosition < basePosition)
        {
            string senderId = GetNPCIdByName(letter.SenderName);
            if (!string.IsNullOrEmpty(senderId))
            {
                Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(senderId);
                int balance = tokens[letter.TokenType];

                if (balance < 0)
                {
                    _messageSystem.AddSystemMessage(
                        $"💸 Debt leverage: {letter.SenderName}'s letter jumps to position {actualPosition} (you owe {Math.Abs(balance)} {letter.TokenType} tokens)",
                        SystemMessageTypes.Warning
                    );
                }
                else if (letter.IsFromPatron)
                {
                    _messageSystem.AddSystemMessage(
                        $"👑 Patron privilege: Letter enters at position {actualPosition} due to patronage debt",
                        SystemMessageTypes.Warning
                    );
                }
            }
        }
        else if (actualPosition > basePosition)
        {
            _messageSystem.AddSystemMessage(
                $"💚 Strong relationship: {letter.SenderName}'s letter enters at position {actualPosition} (reduced leverage)",
                SystemMessageTypes.Success
            );
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"📨 Letter from {letter.SenderName} enters queue at position {actualPosition}",
                SystemMessageTypes.Info
            );
        }
    }

    // Displace letters to insert at leverage position
    private int DisplaceAndInsertLetter(Letter letter, int targetPosition)
    {
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;

        // Announce the leverage-based displacement
        ShowLeverageDisplacement(letter, targetPosition);

        // Collect all letters from target position downward
        List<Letter> lettersToDisplace = new System.Collections.Generic.List<Letter>();
        for (int i = targetPosition - 1; i < _config.LetterQueue.MaxQueueSize; i++)
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

        // Track leverage effect
        int basePosition = _config.LetterQueue.MaxQueueSize;
        if (targetPosition < basePosition)
        {
            letter.OriginalQueuePosition = basePosition;
            letter.LeverageBoost = basePosition - targetPosition;
        }
        letter.State = LetterState.Collected;

        // Reinsert displaced letters
        int nextAvailable = targetPosition;
        foreach (Letter displaced in lettersToDisplace)
        {
            nextAvailable++;
            if (nextAvailable <= _config.LetterQueue.MaxQueueSize)
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


    // Show narrative for debt-based leverage
    private void ShowDebtLeverageNarrative(Letter letter, int position, int basePosition, int tokenBalance)
    {
        if (letter.IsFromPatron && tokenBalance <= -10)
        {
            ShowPatronLeverageNarrative(letter, position, tokenBalance);
        }
        else
        {
            ShowNormalDebtNarrative(letter, position, basePosition, tokenBalance);
        }

        if (position <= 3 && basePosition >= 5)
        {
            _messageSystem.AddSystemMessage(
                $"  • Social hierarchy inverts when you owe money!",
                SystemMessageTypes.Warning
            );
        }
    }

    // Show narrative for patron letters with extreme debt
    private void ShowPatronLeverageNarrative(Letter letter, int position, int tokenBalance)
    {
        _messageSystem.AddSystemMessage(
            $"🌟 A GOLD-SEALED LETTER ARRIVES FROM YOUR PATRON!",
            SystemMessageTypes.Warning
        );
        _messageSystem.AddSystemMessage(
            $"💸 Your MASSIVE DEBT ({Math.Abs(tokenBalance)} tokens) gives them ABSOLUTE LEVERAGE!",
            SystemMessageTypes.Danger
        );
        _messageSystem.AddSystemMessage(
            $"  • Commands priority position {position} - all other obligations must wait!",
            SystemMessageTypes.Warning
        );
    }

    // Show narrative for normal debt leverage
    private void ShowNormalDebtNarrative(Letter letter, int position, int basePosition, int tokenBalance)
    {
        _messageSystem.AddSystemMessage(
            $"💸 {letter.SenderName} has LEVERAGE! Your debt gives them power.",
            SystemMessageTypes.Warning
        );
        _messageSystem.AddSystemMessage(
            $"  • Enters at position {position} (normally {basePosition}) due to {Math.Abs(tokenBalance)} token debt",
            SystemMessageTypes.Info
        );
    }

    // Show narrative for reduced leverage due to good relationship
    private void ShowReducedLeverageNarrative(Letter letter, int position, int basePosition)
    {
        _messageSystem.AddSystemMessage(
            $"✨ Strong relationship with {letter.SenderName} reduces their demands.",
            SystemMessageTypes.Success
        );
        _messageSystem.AddSystemMessage(
            $"  • Enters at position {position} (normally {basePosition}) due to mutual respect",
            SystemMessageTypes.Info
        );
    }

    // Show narrative for normal letter entry
    private void ShowNormalEntryNarrative(Letter letter, int position)
    {
        string urgency = letter.DeadlineInHours <= 72 ? " ⚠️" : "";
        _messageSystem.AddSystemMessage(
            $"📨 New letter from {letter.SenderName} enters queue at position {position}{urgency}",
            SystemMessageTypes.Info
        );
    }

    // Show displacement narrative
    private void ShowLeverageDisplacement(Letter letter, int targetPosition)
    {
        string senderId = GetNPCIdByName(letter.SenderName);
        int tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[letter.TokenType];

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
        string urgency = letter.DeadlineInHours <= 48 ? " 🆘" : "";
        _messageSystem.AddSystemMessage(
            $"  • {letter.SenderName}'s letter pushed to position {newPosition}{urgency}",
            letter.DeadlineInHours <= 48 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
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
        string senderId = GetNPCIdByName(overflowLetter.SenderName);
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
        Player player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }
        player.NPCLetterHistory[senderId].RecordExpiry(); // Use existing expiry tracking
    }

    // Handle patron letters that jump to top positions

    // Remove letter from queue
    public bool RemoveLetterFromQueue(int position)
    {
        if (position < 1 || position > _config.LetterQueue.MaxQueueSize) return false;

        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
        Letter letter = queue[position - 1];
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
        if (position < 1 || position > _config.LetterQueue.MaxQueueSize) return null;
        return _gameWorld.GetPlayer().LetterQueue[position - 1];
    }

    // Check if position 1 has a letter and player is at recipient location
    public bool CanDeliverFromPosition1()
    {
        Letter letter = GetLetterAt(1);
        if (letter == null) return false;

        // Check if player is at the recipient's location
        if (!IsPlayerAtRecipientLocation(letter))
        {
            NPC recipient = _npcRepository.GetById(letter.RecipientId);
            string recipientName = recipient?.Name ?? "recipient";
            _messageSystem.AddSystemMessage(
                $"⚠️ Cannot deliver! You must be at {recipientName}'s location.",
                SystemMessageTypes.Warning
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
        Player player = _gameWorld.GetPlayer();
        Letter[] queue = player.LetterQueue;
        bool emptySlot = queue.Any(l => l == null);

        if (!emptySlot)
        {
            _messageSystem.AddSystemMessage(
                "❌ Letter queue is full! Make room before accepting more letters.",
                SystemMessageTypes.Danger
            );
            return null;
        }

        // Generate letter based on NPC token type
        ConnectionType tokenType = npc.LetterTokenTypes.FirstOrDefault();
        Letter? letter = _letterTemplateRepository.GenerateLetterFromNPC(npc.ID, npc.Name, tokenType);

        if (letter != null)
        {
            // Calculate leverage for position
            Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npc.ID);
            int tokenBalance = tokens.Values.Sum(); // Sum of all tokens with this NPC

            // Determine position based on token type
            int basePosition = tokenType switch
            {
                ConnectionType.Status => 3,
                ConnectionType.Commerce => 5,
                ConnectionType.Shadow => 5,
                ConnectionType.Trust => 7,
                _ => 7
            };

            // If in debt (negative tokens), letter gets higher priority
            int leverage = tokenBalance < 0 ? Math.Abs(tokenBalance) : 0;
            int targetPosition = Math.Max(1, Math.Min(_config.LetterQueue.MaxQueueSize, basePosition - leverage));

            // Add to queue at calculated position
            AddLetterToQueue(letter, targetPosition);
        }

        return letter;
    }

    // Check if player is at the recipient's location
    private bool IsPlayerAtRecipientLocation(Letter letter)
    {
        if (letter == null) return false;
        
        Player player = _gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null) return false;
        
        // Get the recipient NPC
        NPC recipient = _npcRepository.GetById(letter.RecipientId);
        if (recipient == null) return false;
        
        // Check if recipient is at the same spot as the player
        return recipient.SpotId == player.CurrentLocationSpot.SpotID;
    }
    
    // Deliver letter from position 1
    public bool DeliverFromPosition1()
    {
        if (!CanDeliverFromPosition1()) return false;

        Player player = _gameWorld.GetPlayer();
        Letter letter = GetLetterAt(1);
        
        // CRITICAL: Validate player is at recipient's location
        if (!IsPlayerAtRecipientLocation(letter))
        {
            _messageSystem.AddSystemMessage(
                $"❌ Cannot deliver! You must be at {letter.RecipientName}'s location.",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                $"  • Find {letter.RecipientName} and deliver the letter in person",
                SystemMessageTypes.Info
            );
            return false;
        }

        // Remove from queue
        player.LetterQueue[0] = null;

        // Shift all letters up
        CompressQueue();

        // Pay the player
        player.ModifyCoins(letter.Payment);
        
        // Show success message with payment details
        _messageSystem.AddSystemMessage(
            $"📬 Letter delivered to {letter.RecipientName}!",
            SystemMessageTypes.Success
        );
        _messageSystem.AddSystemMessage(
            $"💰 Earned {letter.Payment} coins for delivery.",
            SystemMessageTypes.Success
        );

        // Time advancement for delivery handled by GameFacade to ensure letter deadlines are updated

        // Record delivery
        RecordLetterDelivery(letter);

        return true;
    }

    // Compress queue by shifting letters up to fill gaps
    private void CompressQueue()
    {
        Player player = _gameWorld.GetPlayer();
        int writeIndex = 0;
        for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
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

    // Process hourly deadline countdown
    public void ProcessHourlyDeadlines(int hoursElapsed = 1)
    {
        if (hoursElapsed <= 0) return;
        
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
        
        // PHASE 1: Update deadlines and track expired letters - O(8)
        List<Letter> expiredLetters = new List<Letter>();
        for (int i = 0; i < 8; i++)
        {
            if (queue[i] != null)
            {
                queue[i].DeadlineInHours -= hoursElapsed;
                if (queue[i].DeadlineInHours <= 0)
                {
                    expiredLetters.Add(queue[i]);
                }
            }
        }
        
        // PHASE 2: Apply consequences for all expired letters - O(k)
        foreach (Letter letter in expiredLetters)
        {
            ApplyRelationshipDamage(letter, _connectionTokenManager);
        }
        
        // PHASE 3: Compact array in-place, removing expired letters - O(8)
        int writeIndex = 0;
        for (int readIndex = 0; readIndex < 8; readIndex++)
        {
            if (queue[readIndex] != null && queue[readIndex].DeadlineInHours > 0)
            {
                if (writeIndex != readIndex)
                {
                    queue[writeIndex] = queue[readIndex];
                    queue[writeIndex].QueuePosition = writeIndex + 1; // Update position
                    queue[readIndex] = null;
                }
                writeIndex++;
            }
        }
        
        // Clear remaining positions
        for (int i = writeIndex; i < 8; i++)
        {
            queue[i] = null;
        }
    }

    // Apply token penalty when a letter expires
    private void ApplyRelationshipDamage(Letter letter, TokenMechanicsManager _connectionTokenManager)
    {
        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;

        // Record in history first (needed for failure count)
        RecordExpiryInHistory(senderId);
        
        // Apply basic penalty system for missed deadlines
        int tokenPenalty = _config.LetterQueue.DeadlinePenaltyTokens;
        NPC senderNpc = _npcRepository.GetById(senderId);
        
        ShowExpiryFailure(letter);
        ApplyTokenPenalty(letter, senderId, tokenPenalty);
        ShowRelationshipDamageNarrative(letter, senderNpc, tokenPenalty, senderId);
    }

    // Show the dramatic moment of letter expiry
    private void ShowExpiryFailure(Letter letter)
    {
        _messageSystem.AddSystemMessage(
            $"⏰ TIME'S UP! {letter.SenderName}'s letter has expired!",
            SystemMessageTypes.Danger
        );
    }

    // Apply token penalty for expired letter
    private void ApplyTokenPenalty(Letter letter, string senderId, int tokenPenalty)
    {
        _connectionTokenManager.RemoveTokensFromNPC(letter.TokenType, tokenPenalty, senderId);
    }

    // Record the expiry in letter history
    private void RecordExpiryInHistory(string senderId)
    {
        Player player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }
        player.NPCLetterHistory[senderId].RecordExpiry();
    }

    // Show narrative for relationship damage
    private void ShowRelationshipDamageNarrative(Letter letter, NPC senderNpc, int tokenPenalty, string senderId)
    {
        _messageSystem.AddSystemMessage(
            $"💔 Lost {tokenPenalty} {letter.TokenType} tokens with {letter.SenderName}. Trust broken.",
            SystemMessageTypes.Danger
        );

        if (senderNpc != null)
        {
            ShowConsequenceNarrative(senderNpc, letter);
        }

        ShowCumulativeDamage(letter, senderId);
    }

    // Show contextual consequence for expired letter
    private void ShowConsequenceNarrative(NPC senderNpc, Letter letter)
    {
        string consequence = GetExpiryConsequence(senderNpc, letter);
        _messageSystem.AddSystemMessage(
            $"  • {consequence}",
            SystemMessageTypes.Warning
        );
    }

    // Show cumulative damage if multiple letters expired
    private void ShowCumulativeDamage(Letter letter, string senderId)
    {
        Player player = _gameWorld.GetPlayer();
        LetterHistory history = player.NPCLetterHistory[senderId];

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
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Commerce))
        {
            return $"{npc.Name}'s opportunity has passed. 'Time is money, and you've cost me both.'";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Status))
        {
            return $"Word of your failure reaches {npc.Name}. Your standing in court circles suffers.";
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
        NPC? npc = _npcRepository.GetAllNPCs().FirstOrDefault(n => n.Name == npcName);
        if (npc == null)
        {
            // Try case-insensitive search as fallback
            npc = _npcRepository.GetByName(npcName);
        }
        return npc?.ID ?? "";
    }

    // Track letter delivery in history and process chain letters
    public void RecordLetterDelivery(Letter letter)
    {
        if (letter == null) return;

        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;

        Player player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }

        player.NPCLetterHistory[senderId].RecordDelivery();

        // Get the sender NPC for narrative context
        NPC senderNpc = _npcRepository.GetById(senderId);
        if (senderNpc != null)
        {
            // Show the relationship improvement
            _messageSystem.AddSystemMessage(
                $"👥 {letter.SenderName} appreciates your reliable service.",
                SystemMessageTypes.Success
            );

            // Show trust building based on delivery history
            LetterHistory history = player.NPCLetterHistory[senderId];
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
                    $"  • {history.DeliveredCount} letters delivered! Your relationship with {letter.SenderName} grows stronger.",
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

        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;

        Player player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.ContainsKey(senderId))
        {
            player.NPCLetterHistory[senderId] = new LetterHistory();
        }

        player.NPCLetterHistory[senderId].RecordSkip();

        // Get the sender NPC for narrative context
        NPC senderNpc = _npcRepository.GetById(senderId);
        if (senderNpc != null)
        {
            // Show the relationship damage based on NPC personality
            string reaction = GetSkipReaction(senderNpc, letter);
            _messageSystem.AddSystemMessage(
                $"💔 {reaction}",
                SystemMessageTypes.Warning
            );

            // Show cumulative damage if multiple skips
            LetterHistory history = player.NPCLetterHistory[senderId];
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
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Commerce))
        {
            return $"{npc.Name} frowns at the delay. 'Business waits for no one,' they mutter.";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Status))
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
            .Where(l => l != null && l.DeadlineInHours <= daysThreshold * 24)
            .OrderBy(l => l.DeadlineInHours)
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
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;

        // Collect all letters after the removed position
        List<Letter> remainingLetters = new System.Collections.Generic.List<Letter>();
        for (int i = removedPosition; i < _config.LetterQueue.MaxQueueSize; i++)
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
        foreach (Letter letter in remainingLetters)
        {
            // Find next empty slot starting from the removed position
            while (writePosition < _config.LetterQueue.MaxQueueSize && queue[writePosition] != null)
            {
                writePosition++;
            }

            if (writePosition < _config.LetterQueue.MaxQueueSize)
            {
                int oldPosition = letter.QueuePosition;
                queue[writePosition] = letter;
                letter.QueuePosition = writePosition + 1; // Convert back to 1-based

                // Provide narrative context for each letter moving
                string urgency = letter.DeadlineInHours <= 48 ? " ⚠️ URGENT!" : "";
                string deadlineText = letter.DeadlineInHours <= 24 ? "expires today!" : $"{letter.DeadlineInHours / 24} days left";

                _messageSystem.AddSystemMessage(
                    $"  • {letter.SenderName}'s letter moves from slot {oldPosition} → {letter.QueuePosition} ({deadlineText}){urgency}",
                    letter.DeadlineInHours <= 48 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
                );
            }
        }
    }

    // Create conversation context for skip delivery action
    public QueueManagementContext CreateSkipDeliverContext(int position)
    {
        if (position <= 1 || position > _config.LetterQueue.MaxQueueSize) return null;

        Letter letter = GetLetterAt(position);
        if (letter == null) return null;

        // Check if position 1 is occupied
        if (GetLetterAt(1) != null) return null;

        // Calculate token cost and get skipped letters
        int tokenCost = position - 1;
        Dictionary<int, Letter> skippedLetters = new Dictionary<int, Letter>();

        for (int i = 2; i < position; i++)
        {
            Letter skippedLetter = GetLetterAt(i);
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
    public bool PrepareSkipAction(int position)
    {
        QueueManagementContext context = CreateSkipDeliverContext(position);
        if (context == null)
        {
            _messageSystem.AddSystemMessage("Cannot skip this letter.", SystemMessageTypes.Warning);
            return false;
        }

        // Store the skip position for later processing
        _gameWorld.PendingQueueState.PendingSkipPosition = position;
        _gameWorld.PendingQueueState.PendingAction = QueueActionType.Skip;

        return true;
    }

    // Create conversation context for purge action
    public QueueManagementContext CreatePurgeContext()
    {
        Letter letter = GetLetterAt(_config.LetterQueue.MaxQueueSize);
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
            TokenCost = _config.LetterQueue.PurgeCostTokens,
            GameWorld = _gameWorld,
            Player = _gameWorld.GetPlayer()
        };
    }

    // Trigger conversation for purge action
    public bool PreparePurgeAction()
    {
        QueueManagementContext context = CreatePurgeContext();
        if (context == null)
        {
            _messageSystem.AddSystemMessage($"Cannot purge - no letter in position {_config.LetterQueue.MaxQueueSize} or action forbidden.", SystemMessageTypes.Warning);
            return false;
        }

        // Store purge flag for later processing
        _gameWorld.PendingQueueState.PendingPurgePosition = _config.LetterQueue.MaxQueueSize;
        _gameWorld.PendingQueueState.PendingAction = QueueActionType.Purge;

        return true;
    }

    // Skip letter delivery by spending tokens
    public bool TrySkipDeliver(int position)
    {
        if (!ValidateSkipDeliverPosition(position))
            return false;

        Letter letter = GetLetterAt(position);
        if (letter == null) return false;

        if (!ValidatePosition1Available())
            return false;

        int tokenCost = CalculateSkipCost(position, letter);
        string senderId = GetNPCIdByName(letter.SenderName);

        if (!ProcessSkipPayment(letter, tokenCost, senderId))
            return false;

        PerformSkipDelivery(letter, position, tokenCost);
        return true;
    }

    // Validate position for skip delivery
    private bool ValidateSkipDeliverPosition(int position)
    {
        return position > 1 && position <= _config.LetterQueue.MaxQueueSize;
    }

    // Validate position 1 is available
    private bool ValidatePosition1Available()
    {
        if (GetLetterAt(1) != null)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Cannot skip - position 1 is already occupied!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        return true;
    }

    // Calculate token cost for skipping
    private int CalculateSkipCost(int position, Letter letter)
    {
        int baseCost = position - 1;
        int multiplier = _obligationManager.CalculateSkipCostMultiplier(letter);
        return baseCost * multiplier;
    }

    // Process token payment for skip delivery
    private bool ProcessSkipPayment(Letter letter, int tokenCost, string senderId)
    {
        _messageSystem.AddSystemMessage(
            $"💸 Attempting to skip {letter.SenderName}'s letter to position 1...",
            SystemMessageTypes.Info
        );

        if (!ValidateTokenAvailability(letter, tokenCost))
            return false;

        return SpendTokensForSkip(letter, tokenCost, senderId);
    }

    // Validate player has enough tokens
    private bool ValidateTokenAvailability(Letter letter, int tokenCost)
    {
        if (!_connectionTokenManager.HasTokens(letter.TokenType, tokenCost))
        {
            _messageSystem.AddSystemMessage(
                $"  ❌ Insufficient {letter.TokenType} tokens! Need {tokenCost}, have {_connectionTokenManager.GetTokenCount(letter.TokenType)}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        return true;
    }

    // Spend tokens for skip action
    private bool SpendTokensForSkip(Letter letter, int tokenCost, string senderId)
    {
        _messageSystem.AddSystemMessage(
            $"  • Spending {tokenCost} {letter.TokenType} tokens with {letter.SenderName}...",
            SystemMessageTypes.Warning
        );

        return _connectionTokenManager.SpendTokensWithNPC(letter.TokenType, tokenCost, senderId);
    }

    // Perform the skip delivery action
    private void PerformSkipDelivery(Letter letter, int position, int tokenCost)
    {
        MoveLetterToPosition1(letter, position);
        ShowSkipSuccessNarrative(letter, tokenCost);
        ShiftQueueUp(position);
        RecordLetterSkip(letter);
    }

    // Move letter to position 1
    private void MoveLetterToPosition1(Letter letter, int fromPosition)
    {
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
        queue[0] = letter;
        queue[fromPosition - 1] = null;
        letter.QueuePosition = 1;
    }

    // Show success narrative for skip
    private void ShowSkipSuccessNarrative(Letter letter, int tokenCost)
    {
        _messageSystem.AddSystemMessage(
            $"✅ {letter.SenderName}'s letter jumps the queue to position 1!",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"  • You call in {tokenCost} favors with {letter.SenderName} for urgent handling",
            SystemMessageTypes.Info
        );
    }

    // Generate 1-2 daily letters from available NPCs and templates
    public int GenerateDailyLetters()
    {
        int lettersToGenerate = _random.Next(1, 3);
        int lettersGenerated = 0;

        if (!CanGenerateLetters())
            return 0;

        AnnounceNewCorrespondence();

        for (int i = 0; i < lettersToGenerate; i++)
        {
            if (!TryGenerateSingleLetter(ref lettersGenerated))
                break;
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

    // Check if letters can be generated
    private bool CanGenerateLetters()
    {
        if (IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                "📬 Your letter queue is full - no new correspondence can be accepted today.",
                SystemMessageTypes.Warning
            );
            return false;
        }
        return true;
    }

    // Announce the arrival of new correspondence
    private void AnnounceNewCorrespondence()
    {
        _messageSystem.AddSystemMessage(
            "🌅 Dawn brings new correspondence:",
            SystemMessageTypes.Info
        );
    }

    // Try to generate a single letter
    private bool TryGenerateSingleLetter(ref int lettersGenerated)
    {
        if (IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                "  📭 Additional letters arrive but your queue is now full.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        NPC sender = SelectLetterSender();
        if (sender == null) return true; // Continue trying

        ConnectionType tokenType = SelectTokenType(sender);
        if (tokenType == default) return true; // Continue trying

        Letter letter = GenerateLetterFromSender(sender, tokenType);
        if (letter != null)
        {
            AddGeneratedLetter(letter, sender, tokenType, ref lettersGenerated);
        }

        return true;
    }

    // Select an eligible NPC to send a letter
    private NPC SelectLetterSender()
    {
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        if (allNpcs.Count < 2) return null;

        List<NPC> eligibleSenders = FindEligibleSenders(allNpcs);
        if (!eligibleSenders.Any()) return null;

        return eligibleSenders[_random.Next(eligibleSenders.Count)];
    }

    // Find NPCs eligible to send letters
    private List<NPC> FindEligibleSenders(List<NPC> allNpcs)
    {
        // First try NPCs with existing relationships
        List<NPC> eligibleSenders = allNpcs.Where(npc =>
            npc.LetterTokenTypes.Any() &&
            _categoryService.CanNPCOfferLetters(npc.ID)).ToList();

        if (!eligibleSenders.Any())
        {
            // Fallback to any NPC with letter token types
            eligibleSenders = allNpcs.Where(npc => npc.LetterTokenTypes.Any()).ToList();
        }

        return eligibleSenders;
    }

    // Select appropriate token type for the sender
    private ConnectionType SelectTokenType(NPC sender)
    {
        Dictionary<ConnectionType, LetterCategory> availableCategories = _categoryService.GetAvailableCategories(sender.ID);

        if (availableCategories.Any())
        {
            List<ConnectionType> tokenTypes = availableCategories.Keys.ToList();
            return tokenTypes[_random.Next(tokenTypes.Count)];
        }
        else
        {
            return sender.LetterTokenTypes.FirstOrDefault();
        }
    }

    // Generate a letter from the selected sender
    private Letter GenerateLetterFromSender(NPC sender, ConnectionType tokenType)
    {
        // Try category-based generation first
        Letter letter = _letterTemplateRepository.GenerateLetterFromNPC(sender.ID, sender.Name, tokenType);
        if (letter != null) return letter;

        // Fallback to template-based generation
        return GenerateLetterFromTemplate(sender, tokenType);
    }

    // Generate letter using template system
    private Letter GenerateLetterFromTemplate(NPC sender, ConnectionType tokenType)
    {
        LetterTemplate template = _letterTemplateRepository.GetRandomTemplateByTokenType(tokenType);
        if (template == null) return null;

        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        NPC recipient = allNpcs.Where(n => n.ID != sender.ID).FirstOrDefault();
        if (recipient == null) return null;

        return _letterTemplateRepository.GenerateLetterFromTemplate(template, sender.Name, recipient.Name);
    }

    // Add generated letter to queue and show narrative
    private void AddGeneratedLetter(Letter letter, NPC sender, ConnectionType tokenType, ref int lettersGenerated)
    {
        int position = AddLetter(letter);
        if (position > 0)
        {
            lettersGenerated++;
            ShowGeneratedLetterNarrative(letter, sender, tokenType, position);
        }
    }

    // Show narrative for newly generated letter
    private void ShowGeneratedLetterNarrative(Letter letter, NPC sender, ConnectionType tokenType, int position)
    {
        string urgency = letter.DeadlineInHours <= 72 ? " - needs urgent delivery!" : "";
        string tokenTypeText = GetTokenTypeDescription(letter.TokenType);
        string categoryText = GetCategoryText(sender, tokenType);

        _messageSystem.AddSystemMessage(
            $"  • Letter from {sender.Name} to {letter.RecipientName} ({tokenTypeText} correspondence{categoryText}){urgency}",
            letter.DeadlineInHours <= 72 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
        );

        _messageSystem.AddSystemMessage(
            $"    → Enters your queue at position {position} - {letter.Payment} coins on delivery",
            SystemMessageTypes.Info
        );
    }

    // Get category text for letter narrative
    private string GetCategoryText(NPC sender, ConnectionType tokenType)
    {
        if (_categoryService.CanNPCOfferLetters(sender.ID))
        {
            LetterCategory category = _categoryService.GetAvailableCategory(sender.ID, tokenType);
            return $" [{category} letter]";
        }
        return "";
    }

    // Helper method to get descriptive text for token types
    private string GetTokenTypeDescription(ConnectionType tokenType)
    {
        switch (tokenType)
        {
            case ConnectionType.Trust:
                return "personal";
            case ConnectionType.Commerce:
                return "commercial";
            case ConnectionType.Status:
                return "aristocratic";
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
        if (_timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
        {
            return false; // Can only use during dawn
        }

        // Check if already used today
        Player player = _gameWorld.GetPlayer();
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
        if (position1 < 1 || position1 > _config.LetterQueue.MaxQueueSize || position2 < 1 || position2 > _config.LetterQueue.MaxQueueSize)
        {
            return false;
        }

        // Get letters at positions
        Letter? letter1 = GetLetterAt(position1);
        Letter letter2 = GetLetterAt(position2);

        // At least one position must have a letter
        if (letter1 == null && letter2 == null)
        {
            return false;
        }

        // Perform the swap
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
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
        // Check if there's a letter in the last position
        Letter letterToPurge = GetLetterAt(_config.LetterQueue.MaxQueueSize);
        if (letterToPurge == null)
        {
            _messageSystem.AddSystemMessage(
                $"❌ No letter in position {_config.LetterQueue.MaxQueueSize} to purge!",
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
        foreach (KeyValuePair<ConnectionType, int> payment in tokenPayment)
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
        foreach (KeyValuePair<ConnectionType, int> payment in tokenPayment)
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
        string senderId = GetNPCIdByName(letterToPurge.SenderName);

        // Remove the letter from the last position
        RemoveLetterFromQueue(_config.LetterQueue.MaxQueueSize);

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
        if (fromPosition < 2 || fromPosition > _config.LetterQueue.MaxQueueSize)
        {
            return false; // Can't priority move from position 1 or invalid position
        }

        // Get the letter
        Letter letter = GetLetterAt(fromPosition);
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
        string senderId = GetNPCIdByName(letter.SenderName);

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
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
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
        if (position < 1 || position > _config.LetterQueue.MaxQueueSize)
        {
            return false;
        }

        // Get the letter
        Letter letter = GetLetterAt(position);
        if (letter == null)
        {
            return false; // No letter at position
        }

        // Get sender NPC for narrative context
        string senderId = GetNPCIdByName(letter.SenderName);

        // Show the negotiation
        _messageSystem.AddSystemMessage(
            $"📅 Negotiating deadline extension with {letter.SenderName}...",
            SystemMessageTypes.Info
        );

        // Show current deadline pressure
        string urgency = letter.DeadlineInHours <= 48 ? " 🆘 CRITICAL!" : "";
        _messageSystem.AddSystemMessage(
            $"  • Current deadline: {letter.DeadlineInHours / 24} days{urgency}",
            letter.DeadlineInHours <= 48 ? SystemMessageTypes.Danger : SystemMessageTypes.Info
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
        int oldDeadlineHours = letter.DeadlineInHours;
        letter.DeadlineInHours += 48;

        // Success narrative
        _messageSystem.AddSystemMessage(
            $"✅ {letter.SenderName} grants a 2-day extension!",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"  • New deadline: {letter.DeadlineInHours / 24} days (was {oldDeadlineHours / 24})",
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
        List<Letter> chainLetters = CollectChainLetters(deliveredLetter);
        AddChainLettersToQueue(chainLetters, deliveredLetter);
    }

    // Collect all chain letters from delivered letter
    private List<Letter> CollectChainLetters(Letter deliveredLetter)
    {
        List<Letter> chainLetters = new System.Collections.Generic.List<Letter>();

        // Check letter's own chain letters
        chainLetters.AddRange(GenerateChainLettersFromIds(deliveredLetter.UnlocksLetterIds.ToArray(), deliveredLetter));

        // Check template's chain letters if letter has none
        if (!deliveredLetter.UnlocksLetterIds.Any())
        {
            LetterTemplate letterTemplate = FindLetterTemplate(deliveredLetter);
            if (letterTemplate != null && letterTemplate.UnlocksLetterIds.Any())
            {
                chainLetters.AddRange(GenerateChainLettersFromIds(letterTemplate.UnlocksLetterIds, deliveredLetter));
            }
        }

        return chainLetters;
    }

    // Generate chain letters from template IDs
    private List<Letter> GenerateChainLettersFromIds(string[] templateIds, Letter parentLetter)
    {
        List<Letter> chainLetters = new System.Collections.Generic.List<Letter>();

        foreach (string templateId in templateIds)
        {
            Letter chainLetter = GenerateChainLetter(templateId, parentLetter);
            if (chainLetter != null)
            {
                chainLetters.Add(chainLetter);
            }
        }

        return chainLetters;
    }

    // Add chain letters to queue with narrative
    private void AddChainLettersToQueue(List<Letter> chainLetters, Letter deliveredLetter)
    {
        foreach (Letter chainLetter in chainLetters)
        {
            AddLetterWithObligationEffects(chainLetter);
            ShowChainLetterNarrative(chainLetter, deliveredLetter);
        }
    }

    // Show narrative for chain letter generation
    private void ShowChainLetterNarrative(Letter chainLetter, Letter parentLetter)
    {
        _messageSystem.AddSystemMessage($"📬 Follow-up letter generated!", SystemMessageTypes.Info);
        _messageSystem.AddSystemMessage($"✉️ {chainLetter.SenderName} → {chainLetter.RecipientName}", SystemMessageTypes.Info);
        _messageSystem.AddSystemMessage($"🔗 Chain letter from completing {parentLetter.SenderName}'s delivery", SystemMessageTypes.Info);
    }

    // Generate a chain letter from a template ID
    private Letter GenerateChainLetter(string templateId, Letter parentLetter)
    {
        LetterTemplate template = _letterTemplateRepository.GetTemplateById(templateId);
        if (template == null)
        {
            return null;
        }

        // Determine sender and recipient for the chain letter
        string senderName = DetermineChainSender(template, parentLetter);
        string recipientName = DetermineChainRecipient(template, parentLetter);

        if (string.IsNullOrEmpty(senderName) || string.IsNullOrEmpty(recipientName))
        {
            return null;
        }

        // Generate the chain letter
        Letter? chainLetter = _letterTemplateRepository.GenerateLetterFromTemplate(template, senderName, recipientName);

        if (chainLetter != null)
        {
            // Mark it as a chain letter
            chainLetter.IsChainLetter = true;
            chainLetter.ParentLetterId = parentLetter.Id;

            // Chain letters typically have similar or longer deadlines
            chainLetter.DeadlineInHours = Math.Max(chainLetter.DeadlineInHours, parentLetter.DeadlineInHours + 24);

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
            Random random = new Random();
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
            Random random = new Random();
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
        List<LetterTemplate> allTemplates = _letterTemplateRepository.GetAllTemplates();

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
        LetterTemplate template = FindLetterTemplate(letter);
        return template != null && template.UnlocksLetterIds.Any();
    }

    // Get information about potential chain letters for a letter
    public System.Collections.Generic.List<string> GetChainLetterInfo(Letter letter)
    {
        List<string> chainInfo = new System.Collections.Generic.List<string>();

        // Check letter's own chain letters
        foreach (string templateId in letter.UnlocksLetterIds)
        {
            LetterTemplate template = _letterTemplateRepository.GetTemplateById(templateId);
            if (template != null)
            {
                chainInfo.Add($"Unlocks: {template.Description}");
            }
        }

        // Check template's chain letters
        LetterTemplate letterTemplate = FindLetterTemplate(letter);
        if (letterTemplate != null)
        {
            foreach (string templateId in letterTemplate.UnlocksLetterIds)
            {
                LetterTemplate template = _letterTemplateRepository.GetTemplateById(templateId);
                if (template != null)
                {
                    chainInfo.Add($"May unlock: {template.Description}");
                }
            }
        }

        return chainInfo;
    }

    // Move letter to specific position (for undo operations)
    public void MoveLetterToPosition(Letter letter, int position)
    {
        if (letter == null || position < 1 || position > _config.LetterQueue.MaxQueueSize)
            return;

        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;

        // Clear current position if letter is in queue
        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i] == letter)
            {
                queue[i] = null;
                break;
            }
        }

        // Place at new position
        queue[position - 1] = letter;
        letter.QueuePosition = position;
    }

    // Check if positions can be swapped
    public bool CanSwapPositions(int position1, int position2)
    {
        if (position1 < 1 || position1 > _config.LetterQueue.MaxQueueSize ||
            position2 < 1 || position2 > _config.LetterQueue.MaxQueueSize)
            return false;

        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;

        // At least one position must have a letter
        if (queue[position1 - 1] == null && queue[position2 - 1] == null)
            return false;

        // Can't swap position 1 if letter is being delivered
        Letter letterAt1 = queue[0];
        if (letterAt1 != null && letterAt1.State == LetterState.Delivering)
            return false;

        return true;
    }
    
    // === SIMPLE POSITION-BASED QUEUE MANIPULATION ===
    
    // Calculate simple position-based reorder cost
    public int CalculateReorderCost(int fromPosition, int toPosition)
    {
        return Math.Abs(toPosition - fromPosition);
    }
    
    // Check if player can afford to reorder based on tokens with sender
    public bool CanAffordReorder(string npcId, ConnectionType tokenType, int fromPos, int toPos)
    {
        int cost = CalculateReorderCost(fromPos, toPos);
        var tokens = _connectionTokenManager.GetTokensWithNPC(npcId)[tokenType];
        return tokens >= cost;
    }
    
    // Execute reorder with token spending
    public bool ExecuteReorder(Letter letter, int fromPos, int toPos)
    {
        if (letter == null) return false;
        
        // Validate positions
        if (fromPos < 1 || fromPos > _config.LetterQueue.MaxQueueSize ||
            toPos < 1 || toPos > _config.LetterQueue.MaxQueueSize)
            return false;
            
        int cost = CalculateReorderCost(fromPos, toPos);
        
        // Get sender NPC ID
        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return false;
        
        // Spend tokens with the letter's sender
        if (!_connectionTokenManager.SpendTokensWithNPC(
            letter.TokenType, 
            cost, 
            senderId))
        {
            _messageSystem.AddSystemMessage(
                $"❌ Cannot reorder - insufficient {letter.TokenType} tokens with {letter.SenderName}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Move the letter
        bool success = MoveLetterInQueue(fromPos, toPos);
        
        if (success)
        {
            _messageSystem.AddSystemMessage(
                $"✅ Reordered {letter.SenderName}'s letter from position {fromPos} to {toPos} (cost: {cost} {letter.TokenType} tokens)",
                SystemMessageTypes.Success
            );
        }
        
        return success;
    }
    
    // Move letter between positions in queue
    private bool MoveLetterInQueue(int fromPos, int toPos)
    {
        Letter[] queue = _gameWorld.GetPlayer().LetterQueue;
        
        // Get the letter to move
        Letter letterToMove = queue[fromPos - 1];
        if (letterToMove == null) return false;
        
        // Check if target position is occupied
        if (queue[toPos - 1] != null)
        {
            // Need to shift other letters
            if (toPos < fromPos)
            {
                // Moving up - shift letters down
                for (int i = fromPos - 1; i > toPos - 1; i--)
                {
                    queue[i] = queue[i - 1];
                    if (queue[i] != null) queue[i].QueuePosition = i + 1;
                }
            }
            else
            {
                // Moving down - shift letters up
                for (int i = fromPos - 1; i < toPos - 1; i++)
                {
                    queue[i] = queue[i + 1];
                    if (queue[i] != null) queue[i].QueuePosition = i + 1;
                }
            }
        }
        else
        {
            // Simple move - clear old position
            queue[fromPos - 1] = null;
        }
        
        // Place letter in new position
        queue[toPos - 1] = letterToMove;
        letterToMove.QueuePosition = toPos;
        
        return true;
    }
}