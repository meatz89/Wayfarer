

public class ObligationQueueManager
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    private readonly StandingObligationManager _obligationManager;
    private readonly TokenMechanicsManager _connectionTokenManager;
    private readonly GameConfiguration _config;
    private readonly IGameRuleEngine _ruleEngine;
    private readonly TimeManager _timeManager;
    private readonly Random _random = new Random();

    public ObligationQueueManager(GameWorld gameWorld, NPCRepository npcRepository, MessageSystem messageSystem, StandingObligationManager obligationManager, TokenMechanicsManager connectionTokenManager, GameConfiguration config, IGameRuleEngine ruleEngine, TimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
        _obligationManager = obligationManager;
        _connectionTokenManager = connectionTokenManager;
        _config = config;
        _ruleEngine = ruleEngine;
        _timeManager = timeManager;
    }

    // Get the player's letter queue
    public DeliveryObligation[] GetPlayerQueue()
    {
        return _gameWorld.GetPlayer().ObligationQueue;
    }

    // Get the total size of all physical letters in satchel
    public int GetTotalSize()
    {
        Player player = _gameWorld.GetPlayer();
        int totalSize = 0;
        foreach (Letter letter in player.CarriedLetters)
        {
            totalSize += letter.Size;
        }
        return totalSize;
    }

    // Get active obligations (non-null entries in the queue)
    public DeliveryObligation[] GetActiveObligations()
    {
        Player player = _gameWorld.GetPlayer();
        Console.WriteLine($"[GetActiveObligations] Player has {player.ObligationQueue.Length} queue slots");

        DeliveryObligation[] activeObligations = player.ObligationQueue
            .Where(o => o != null)
            .ToArray();

        Console.WriteLine($"[GetActiveObligations] Found {activeObligations.Length} non-null obligations");

        foreach (DeliveryObligation obligation in activeObligations)
        {
            Console.WriteLine($"  - Obligation: {obligation.Id}, Sender: {obligation.SenderId}");
        }

        return activeObligations;
    }

    // Get active meeting obligations
    public List<MeetingObligation> GetActiveMeetingObligations()
    {
        Player player = _gameWorld.GetPlayer();
        return player.MeetingObligations
            .Where(m => m.DeadlineInSegments > 0)
            .ToList();
    }

    // Get meeting obligation with specific NPC
    public MeetingObligation GetMeetingWithNPC(string npcId)
    {
        return GetActiveMeetingObligations()
            .FirstOrDefault(m => m.RequesterId == npcId);
    }

    // Add a meeting obligation
    public void AddMeetingObligation(MeetingObligation meeting)
    {
        _gameWorld.GetPlayer().MeetingObligations.Add(meeting);
        _messageSystem.AddSystemMessage($"{meeting.RequesterName} urgently requests to meet you!", SystemMessageTypes.Warning);
    }

    // Complete a meeting obligation
    public void CompleteMeeting(string meetingId)
    {
        MeetingObligation? meeting = _gameWorld.GetPlayer().MeetingObligations
            .FirstOrDefault(m => m.Id == meetingId);
        if (meeting != null)
        {
            _gameWorld.GetPlayer().MeetingObligations.Remove(meeting);
            _messageSystem.AddSystemMessage($"Met with {meeting.RequesterName}", SystemMessageTypes.Success);
        }
    }

    // Get the queue position of an obligation (1-based)
    public int GetQueuePosition(DeliveryObligation obligation)
    {
        if (obligation == null) return -1;

        DeliveryObligation[] queue = GetPlayerQueue();
        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i]?.Id == obligation.Id)
            {
                return i + 1; // Return 1-based position
            }
        }

        return -1; // Not found
    }

    // Deliver an obligation (mark as completed)
    public bool DeliverObligation(string obligationId)
    {
        DeliveryObligation? obligation = GetActiveObligations().FirstOrDefault(o => o.Id == obligationId);
        if (obligation == null) return false;

        int position = GetQueuePosition(obligation);
        if (position <= 0) return false;

        // Remove from queue
        RemoveObligationFromQueue(position);

        // Grant tokens for successful delivery
        if (obligation.TokenType != ConnectionType.None)
        {
            _connectionTokenManager.AddTokensToNPC(obligation.TokenType, 1, obligation.RecipientId);
        }

        _messageSystem.AddSystemMessage(
            $"Letter delivered to {obligation.RecipientName}!",
            SystemMessageTypes.Success
        );

        return true;
    }

    // Add letter to queue at specific position
    public bool AddObligationToQueue(DeliveryObligation obligation, int position)
    {
        if (obligation == null || position < 1 || position > _config.LetterQueue.MaxQueueSize) return false;

        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        if (queue[position - 1] != null) return false; // Position occupied

        queue[position - 1] = obligation;
        obligation.QueuePosition = position;

        _messageSystem.AddSystemMessage(
            $"📨 DeliveryObligation from {obligation.SenderName} added to position {position}",
            SystemMessageTypes.Success
        );

        return true;
    }
    // Add obligation to first available slot - queue fills from position 1
    public int AddObligation(DeliveryObligation obligation)
    {
        if (obligation == null) return 0;

        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

        // Find the FIRST empty slot, filling from position 1
        for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
        {
            if (queue[i] == null)
            {
                queue[i] = obligation;
                obligation.QueuePosition = i + 1;

                _messageSystem.AddSystemMessage(
                    $"📨 New obligation from {obligation.SenderName} enters queue at position {i + 1}",
                    SystemMessageTypes.Success
                );

                return i + 1;
            }
        }
        return 0; // Queue full
    }

    // Add obligation with leverage-aware positioning
    // Regular delivery letters go to BOTH queue AND satchel (compete for physical space)
    public int AddObligationWithEffects(DeliveryObligation obligation)
    {
        if (obligation == null) return 0;

        // Apply deadline bonuses from obligations
        ApplyDeadlineBonuses(obligation);

        // Check for automatic displacement scenarios (POC Package 4)
        bool shouldForcePosition = CheckForAutomaticDisplacement(obligation);

        if (shouldForcePosition)
        {
            // Determine forced position and reason
            int forcedPosition = DetermineForcedPosition(obligation);
            string displacementReason = GetDisplacementReason(obligation);

            // Use automatic displacement system
            bool success = AutomaticDisplacement(obligation, forcedPosition, displacementReason);

            if (success)
            {
                // Add to satchel
                AddPhysicalLetter(obligation);
                return forcedPosition;
            }
        }

        // Normal leverage-based positioning
        int targetPosition = CalculateLeveragePosition(obligation);

        // Add to queue
        int queuePosition = AddObligationWithLeverage(obligation, targetPosition);

        // Also add to satchel (physical inventory) - regular letters take physical space
        if (queuePosition > 0)
        {
            AddPhysicalLetter(obligation);
        }

        return queuePosition;
    }

    /// <summary>
    /// Check if this obligation should force automatic displacement
    /// </summary>
    private bool CheckForAutomaticDisplacement(DeliveryObligation obligation)
    {
        // Failed letter negotiations force position 1
        if (obligation.GenerationReason != null &&
            obligation.GenerationReason.Contains("failure", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Lord Blackwood (proud NPC) attempts position 1
        if (obligation.SenderName == "Lord Blackwood" ||
            obligation.RecipientName == "Lord Blackwood")
        {
            return true;
        }

        // Check if obligation is marked as disconnected
        // (This would be set during conversation based on NPC's connection state)
        if (obligation.EmotionalFocus == EmotionalFocus.CRITICAL)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determine the forced position for automatic displacement
    /// </summary>
    private int DetermineForcedPosition(DeliveryObligation obligation)
    {
        // Most forced scenarios go to position 1
        // Could be extended to support other positions
        return 1;
    }

    /// <summary>
    /// Get the reason for automatic displacement
    /// </summary>
    private string GetDisplacementReason(DeliveryObligation obligation)
    {
        if (obligation.GenerationReason != null &&
            obligation.GenerationReason.Contains("failure", StringComparison.OrdinalIgnoreCase))
        {
            return "Failed negotiation - NPC's terms are non-negotiable!";
        }

        // Crisis system removed - use categorical mechanics instead

        if (obligation.SenderName == "Lord Blackwood")
        {
            return "Lord Blackwood's pride demands immediate attention!";
        }

        if (obligation.EmotionalFocus == EmotionalFocus.CRITICAL)
        {
            return $"{obligation.SenderName} is DISCONNECTED - their letter takes priority!";
        }

        return "Urgent circumstances demand immediate delivery!";
    }

    /// <summary>
    /// Add physical letter to satchel
    /// </summary>
    private void AddPhysicalLetter(DeliveryObligation obligation)
    {
        Letter physicalLetter = new Letter
        {
            Id = obligation.Id,
            SenderName = obligation.SenderName,
            RecipientName = obligation.RecipientName,
            Size = 1, // Standard letter size
            PhysicalProperties = LetterPhysicalProperties.None,
            SpecialType = LetterSpecialType.None // Physical letters default to regular type
        };

        _gameWorld.GetPlayer().CarriedLetters.Add(physicalLetter);
    }

    // Apply deadline bonuses from active obligations
    private void ApplyDeadlineBonuses(DeliveryObligation obligation)
    {
        List<StandingObligation> activeObligations = _obligationManager.GetActiveObligations();

        foreach (StandingObligation standingObligation in activeObligations)
        {
            // Check if obligation applies to this letter type
            if (!standingObligation.AppliesTo(obligation.TokenType)) continue;

            // Check for DeadlinePlus2Days effect
            if (standingObligation.HasEffect(ObligationEffect.DeadlinePlus2Days))
            {
                // Check if the letter is from the specific NPC if obligation is NPC-specific
                if (!string.IsNullOrEmpty(standingObligation.RelatedNPCId))
                {
                    NPC npc = _npcRepository.GetByName(obligation.SenderName);
                    if (npc == null || npc.ID != standingObligation.RelatedNPCId) continue;
                }

                obligation.DeadlineInSegments += 48;
                _messageSystem.AddSystemMessage(
                    $"📅 {standingObligation.Name} grants +2 days to deadline for letter from {obligation.SenderName}",
                    SystemMessageTypes.Info
                );
            }
        }

        // Apply dynamic deadline bonuses that scale with tokens
        _obligationManager.ApplyDynamicDeadlineBonuses(obligation);
    }

    // Calculate relationship-based entry position using the queue position algorithm
    // Position = 8 - (highest positive token) + (worst negative token penalty)
    private int CalculateLeveragePosition(DeliveryObligation obligation)
    {
        string senderId = GetNPCIdByName(obligation.SenderName);
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
        Dictionary<ConnectionType, int> allTokens = _connectionTokenManager.GetTokensWithNPC(senderId);

        // Calculate position using specification algorithm
        int highestPositiveToken = GetHighestPositiveToken(allTokens);
        int worstNegativeTokenPenalty = GetWorstNegativeTokenPenalty(allTokens);

        // Base algorithm: Position = 8 - (highest positive token) + (worst negative token penalty)
        int position = _config.LetterQueue.MaxQueueSize - highestPositiveToken + worstNegativeTokenPenalty;

        // Apply Commerce debt leverage override
        if (allTokens.Any(kvp => kvp.Key == ConnectionType.Commerce) && allTokens[ConnectionType.Commerce] <= -3)
        {
            position = 2; // Commerce debt >= 3 forces position 2
        }

        // Clamp to valid queue range
        position = Math.Max(1, Math.Min(_config.LetterQueue.MaxQueueSize, position));

        // Record positioning data for UI translation
        RecordObligationPositioning(obligation, senderId, allTokens, position, highestPositiveToken, worstNegativeTokenPenalty);

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
        foreach (int tokenCount in allTokens.Values)
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
        foreach (int tokenCount in allTokens.Values)
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
    private void RecordObligationPositioning(DeliveryObligation letter, string senderId, Dictionary<ConnectionType, int> allTokens,
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

        if (allTokens.Any(kvp => kvp.Key == ConnectionType.Commerce) && allTokens[ConnectionType.Commerce] <= -3)
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

    // Add obligation with leverage-based displacement
    private int AddObligationWithLeverage(DeliveryObligation obligation, int targetPosition)
    {
        if (!ValidateObligationCanBeAdded(obligation))
            return 0;

        // Validate target position is within bounds
        if (targetPosition < 1 || targetPosition > _config.LetterQueue.MaxQueueSize)
        {
            _messageSystem.AddSystemMessage(
                $"Invalid queue position {targetPosition} calculated for obligation from {obligation.SenderName}. Using default position.",
                SystemMessageTypes.Warning
            );
            targetPosition = Math.Max(1, Math.Min(_config.LetterQueue.MaxQueueSize, targetPosition));
        }

        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

        // Additional safety check for array bounds
        if (targetPosition - 1 < 0 || targetPosition - 1 >= queue.Length)
        {
            _messageSystem.AddSystemMessage(
                $"Critical error: position {targetPosition} is outside queue bounds. Defaulting to last position.",
                SystemMessageTypes.Danger
            );
            targetPosition = _config.LetterQueue.MaxQueueSize;
        }

        // If target position is empty, simple insertion
        if (queue[targetPosition - 1] == null)
        {
            return InsertObligationAtPosition(obligation, targetPosition);
        }

        // Target occupied - need displacement
        return DisplaceAndInsertLetter(obligation, targetPosition);
    }

    // Get the current position of an obligation in the queue
    public int? GetObligationPosition(string obligationId)
    {
        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i]?.Id == obligationId)
            {
                return i + 1; // Return 1-based position
            }
        }
        return null;
    }

    // Validate that an obligation can be added to the queue
    private bool ValidateObligationCanBeAdded(DeliveryObligation obligation)
    {
        if (IsQueueFull())
        {
            _messageSystem.AddSystemMessage(
                $"Cannot accept obligation from {obligation.SenderName} - your queue is completely full!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        return true;
    }

    // Insert obligation at an empty position
    private int InsertObligationAtPosition(DeliveryObligation obligation, int position)
    {
        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        queue[position - 1] = obligation;
        obligation.QueuePosition = position;

        // Track original vs actual position for leverage visibility
        int basePosition = _config.LetterQueue.MaxQueueSize;
        if (position < basePosition)
        {
            obligation.OriginalQueuePosition = basePosition;
            obligation.LeverageBoost = basePosition - position;
        }

        // Show leverage narrative if position differs from normal
        ShowLeverageNarrative(obligation, position);

        return position;
    }

    // Show narrative explaining why letter entered at this position
    private void ShowLeverageNarrative(DeliveryObligation letter, int actualPosition)
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
                $"📨 DeliveryObligation from {letter.SenderName} enters queue at position {actualPosition}",
                SystemMessageTypes.Info
            );
        }
    }

    // Displace letters to insert at leverage position
    private int DisplaceAndInsertLetter(DeliveryObligation letter, int targetPosition)
    {
        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

        // Announce the leverage-based displacement
        ShowLeverageDisplacement(letter, targetPosition);

        // Collect all letters from target position downward
        List<DeliveryObligation> lettersToDisplace = new System.Collections.Generic.List<DeliveryObligation>();
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

        // Reinsert displaced letters
        int nextAvailable = targetPosition;
        foreach (DeliveryObligation displaced in lettersToDisplace)
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
    private void ShowDebtLeverageNarrative(DeliveryObligation letter, int position, int basePosition, int tokenBalance)
    {
        ShowNormalDebtNarrative(letter, position, basePosition, tokenBalance);

        if (position <= 3 && basePosition >= 5)
        {
            _messageSystem.AddSystemMessage(
                $"  • Social hierarchy inverts when you owe money!",
                SystemMessageTypes.Warning
            );
        }
    }


    // Show narrative for normal debt leverage
    private void ShowNormalDebtNarrative(DeliveryObligation letter, int position, int basePosition, int tokenBalance)
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
    private void ShowReducedLeverageNarrative(DeliveryObligation letter, int position, int basePosition)
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
    private void ShowNormalEntryNarrative(DeliveryObligation letter, int position)
    {
        string urgency = letter.DeadlineInSegments <= 72 ? " [URGENT]" : "";
        _messageSystem.AddSystemMessage(
            $"📨 New letter from {letter.SenderName} enters queue at position {position}{urgency}",
            SystemMessageTypes.Info
        );
    }

    // Show displacement narrative
    private void ShowLeverageDisplacement(DeliveryObligation letter, int targetPosition)
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
    private void NotifyLetterShifted(DeliveryObligation letter, int newPosition)
    {
        string urgency = letter.DeadlineInSegments <= 48 ? " 🆘" : "";
        _messageSystem.AddSystemMessage(
            $"  • {letter.SenderName}'s letter pushed to position {newPosition}{urgency}",
            letter.DeadlineInSegments <= 48 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
        );
    }

    // Handle letter pushed out of queue
    private void HandleQueueOverflow(DeliveryObligation overflowLetter)
    {
        _messageSystem.AddSystemMessage(
            $"💥 {overflowLetter.SenderName}'s letter FORCED OUT by leverage!",
            SystemMessageTypes.Danger
        );

        _messageSystem.AddSystemMessage(
            $"  • The focus of your debts crushes other obligations",
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
        if (!player.NPCLetterHistory.Any(h => h.NpcId == senderId))
        {
            player.NPCLetterHistory.AddOrUpdateHistory(senderId, new LetterHistory());
        }
        player.NPCLetterHistory.GetHistory(senderId).RecordExpiry(); // Use existing expiry tracking
    }


    // Remove letter from queue
    public bool RemoveLetterFromQueue(int position)
    {
        if (position < 1 || position > _config.LetterQueue.MaxQueueSize) return false;

        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        DeliveryObligation letter = queue[position - 1];
        if (letter == null) return false;

        letter.QueuePosition = 0;
        queue[position - 1] = null;

        // Shift all letters below the removed position up by one
        ShiftQueueUp(position);

        return true;
    }

    // Get letter at position
    public DeliveryObligation GetLetterAt(int position)
    {
        if (position < 1 || position > _config.LetterQueue.MaxQueueSize) return null;
        return _gameWorld.GetPlayer().ObligationQueue[position - 1];
    }

    // Check if position 1 has a letter and player is at recipient location
    public bool CanDeliverFromPosition1()
    {
        DeliveryObligation letter = GetLetterAt(1);
        if (letter == null) return false;

        // Check if player is at the recipient's location
        if (!IsPlayerAtRecipientLocation(letter))
        {
            NPC recipient = _npcRepository.GetById(letter.RecipientId);
            string recipientName = recipient?.Name ?? "recipient";
            _messageSystem.AddSystemMessage(
                $"Cannot deliver! You must be at {recipientName}'s location.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        return true;
    }

    // Generate a letter from an NPC and add to queue - REMOVED
    // Letters are now created through ConversationLetterService during conversations only

    // Get the position of an obligation in the queue
    public int? GetLetterPosition(string obligationId)
    {
        return GetObligationPosition(obligationId);
    }

    // Add a delivery obligation to the queue
    public int AddLetterToQueue(DeliveryObligation obligation, int position)
    {
        bool success = AddObligationToQueue(obligation, position);
        return success ? position : 0;
    }

    // Add an obligation to the queue with default positioning
    public int AddLetter(DeliveryObligation obligation)
    {
        return AddObligation(obligation);
    }

    // Add an obligation with leverage effects
    public int AddLetterWithObligationEffects(DeliveryObligation obligation)
    {
        return AddObligationWithEffects(obligation);
    }

    // Move an obligation to a specific position
    public void MoveObligationToPosition(DeliveryObligation obligation, int targetPosition)
    {
        MoveLetterToPosition(obligation, targetPosition);
    }

    // Remove an obligation from the queue
    public bool RemoveObligationFromQueue(int position)
    {
        return RemoveLetterFromQueue(position);
    }

    // Check if player is at the recipient's location
    private bool IsPlayerAtRecipientLocation(DeliveryObligation letter)
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
        DeliveryObligation letter = GetLetterAt(1);

        // CRITICAL: Validate player is at recipient's location
        if (!IsPlayerAtRecipientLocation(letter))
        {
            _messageSystem.AddSystemMessage(
                $"Cannot deliver! You must be at {letter.RecipientName}'s location.",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                $"  • Find {letter.RecipientName} and deliver the letter in person",
                SystemMessageTypes.Info
            );
            return false;
        }

        // Remove from queue
        player.ObligationQueue[0] = null;

        // Shift all letters up
        CompressQueue();

        // Pay the player
        player.ModifyCoins(letter.Payment);

        // Show success message with payment details
        _messageSystem.AddSystemMessage(
            $"📬 DeliveryObligation delivered to {letter.RecipientName}!",
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
            if (player.ObligationQueue[i] != null)
            {
                if (i != writeIndex)
                {
                    player.ObligationQueue[writeIndex] = player.ObligationQueue[i];
                    player.ObligationQueue[i] = null;
                }
                writeIndex++;
            }
        }
    }

    // Process segment-based deadline countdown
    public void ProcessSegmentDeadlines(int segmentsElapsed = 1)
    {
        if (segmentsElapsed <= 0) return;

        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

        // PHASE 1: Update deadlines and track expired letters - O(8)
        List<DeliveryObligation> expiredLetters = new List<DeliveryObligation>();
        for (int i = 0; i < 8; i++)
        {
            if (queue[i] != null)
            {
                queue[i].DeadlineInSegments -= segmentsElapsed;
                if (queue[i].DeadlineInSegments <= 0)
                {
                    expiredLetters.Add(queue[i]);
                }
            }
        }

        // PHASE 2: Apply consequences for all expired letters - O(k)
        foreach (DeliveryObligation letter in expiredLetters)
        {
            ApplyRelationshipDamage(letter, _connectionTokenManager);
        }

        // PHASE 3: Compact array in-place, removing expired letters - O(8)
        int writeIndex = 0;
        for (int readIndex = 0; readIndex < 8; readIndex++)
        {
            if (queue[readIndex] != null && queue[readIndex].DeadlineInSegments > 0)
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
    private void ApplyRelationshipDamage(DeliveryObligation letter, TokenMechanicsManager _connectionTokenManager)
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
    private void ShowExpiryFailure(DeliveryObligation letter)
    {
        _messageSystem.AddSystemMessage(
            $"TIME'S UP! {letter.SenderName}'s letter has expired!",
            SystemMessageTypes.Danger
        );
    }

    // Apply token penalty for expired letter
    private void ApplyTokenPenalty(DeliveryObligation letter, string senderId, int tokenPenalty)
    {
        _connectionTokenManager.RemoveTokensFromNPC(letter.TokenType, tokenPenalty, senderId);
    }

    // Record the expiry in letter history
    private void RecordExpiryInHistory(string senderId)
    {
        Player player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.Any(h => h.NpcId == senderId))
        {
            player.NPCLetterHistory.AddOrUpdateHistory(senderId, new LetterHistory());
        }
        player.NPCLetterHistory.GetHistory(senderId).RecordExpiry();
    }

    // Show narrative for relationship damage
    private void ShowRelationshipDamageNarrative(DeliveryObligation letter, NPC senderNpc, int tokenPenalty, string senderId)
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
    private void ShowConsequenceNarrative(NPC senderNpc, DeliveryObligation letter)
    {
        string consequence = GetExpiryConsequence(senderNpc, letter);
        _messageSystem.AddSystemMessage(
            $"  • {consequence}",
            SystemMessageTypes.Warning
        );
    }

    // Show cumulative damage if multiple letters expired
    private void ShowCumulativeDamage(DeliveryObligation letter, string senderId)
    {
        Player player = _gameWorld.GetPlayer();
        LetterHistory history = player.NPCLetterHistory.GetHistory(senderId);

        if (history.ExpiredCount > 1)
        {
            _messageSystem.AddSystemMessage(
                $"  This is the {GetOrdinal(history.ExpiredCount)} letter from {letter.SenderName} you've let expire.",
                SystemMessageTypes.Danger
            );
        }
    }

    // Get contextual consequence for expired letter
    private string GetExpiryConsequence(NPC npc, DeliveryObligation letter)
    {
        if (npc.LetterTokenTypes.Contains(ConnectionType.Trust))
        {
            return $"{npc.Name} waited for your help that never came. Some wounds don't heal.";
        }
        else if (npc.LetterTokenTypes.Contains(ConnectionType.Commerce))
        {
            return $"{npc.Name}'s opening has passed. 'Time is money, and you've cost me both.'";
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
        return npc?.ID ?? "";
    }

    public void RecordLetterDelivery(DeliveryObligation letter)
    {
        if (letter == null) return;

        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;

        Player player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.Any(h => h.NpcId == senderId))
        {
            player.NPCLetterHistory.AddOrUpdateHistory(senderId, new LetterHistory());
        }

        player.NPCLetterHistory.GetHistory(senderId).RecordDelivery();

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
            LetterHistory history = player.NPCLetterHistory.GetHistory(senderId);
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


        // Store delivered letter info in player state for scenario tracking
        if (!player.DeliveredLetters.Contains(letter))
        {
            player.DeliveredLetters.Add(letter);
            player.TotalLettersDelivered++;
        }
    }

    // Track letter skip in history
    public void RecordLetterSkip(DeliveryObligation letter)
    {
        if (letter == null) return;

        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId)) return;

        Player player = _gameWorld.GetPlayer();
        if (!player.NPCLetterHistory.Any(h => h.NpcId == senderId))
        {
            player.NPCLetterHistory.AddOrUpdateHistory(senderId, new LetterHistory());
        }

        player.NPCLetterHistory.GetHistory(senderId).RecordSkip();

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
            LetterHistory history = player.NPCLetterHistory.GetHistory(senderId);
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
                    $"  {letter.SenderName} may stop offering you letters if this continues.",
                    SystemMessageTypes.Danger
                );
            }
        }
    }

    // Get contextual skip reaction based on NPC type
    private string GetSkipReaction(NPC npc, DeliveryObligation letter)
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
    public DeliveryObligation[] GetExpiringLetters(int daysThreshold)
    {
        return _gameWorld.GetPlayer().ObligationQueue
            .Where(l => l != null && l.DeadlineInSegments <= daysThreshold * 24)
            .OrderBy(l => l.DeadlineInSegments)
            .ToArray();
    }

    // Check if queue is full
    public bool IsQueueFull()
    {
        return _gameWorld.GetPlayer().ObligationQueue.All(slot => slot != null);
    }

    // Get queue fill status
    public int GetLetterCount()
    {
        return _gameWorld.GetPlayer().ObligationQueue.Count(slot => slot != null);
    }

    // Shift queue up after removal - compact all letters to fill gaps
    private void ShiftQueueUp(int removedPosition)
    {
        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

        // Collect all letters after the removed position
        List<DeliveryObligation> remainingLetters = new System.Collections.Generic.List<DeliveryObligation>();
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
        foreach (DeliveryObligation letter in remainingLetters)
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
                string urgency = letter.DeadlineInSegments <= 48 ? " [URGENT!]" : "";
                string deadlineText = letter.DeadlineInSegments <= 24 ? "expires today!" : $"{letter.DeadlineInSegments / 24} days left";

                _messageSystem.AddSystemMessage(
                    $"  • {letter.SenderName}'s letter moves from slot {oldPosition} → {letter.QueuePosition} ({deadlineText}){urgency}",
                    letter.DeadlineInSegments <= 48 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
                );
            }
        }
    }

    // Skip letter delivery by spending tokens
    public bool TrySkipDeliver(int position)
    {
        if (!ValidateSkipDeliverPosition(position))
            return false;

        DeliveryObligation letter = GetLetterAt(position);
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
                $"Cannot skip - position 1 is already occupied!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        return true;
    }

    // Calculate token cost for skipping
    private int CalculateSkipCost(int position, DeliveryObligation letter)
    {
        int baseCost = position - 1;
        int multiplier = _obligationManager.CalculateSkipCostMultiplier(letter);
        return baseCost * multiplier;
    }

    // Process token payment for skip delivery
    private bool ProcessSkipPayment(DeliveryObligation letter, int tokenCost, string senderId)
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
    private bool ValidateTokenAvailability(DeliveryObligation letter, int tokenCost)
    {
        if (!_connectionTokenManager.HasTokens(letter.TokenType, tokenCost))
        {
            _messageSystem.AddSystemMessage(
                $"  Insufficient {letter.TokenType} tokens! Need {tokenCost}, have {_connectionTokenManager.GetTokenCount(letter.TokenType)}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        return true;
    }

    // Spend tokens for skip action
    private bool SpendTokensForSkip(DeliveryObligation letter, int tokenCost, string senderId)
    {
        _messageSystem.AddSystemMessage(
            $"  • Spending {tokenCost} {letter.TokenType} tokens with {letter.SenderName}...",
            SystemMessageTypes.Warning
        );

        return _connectionTokenManager.SpendTokensWithNPC(letter.TokenType, tokenCost, senderId);
    }

    // Perform the skip delivery action
    private void PerformSkipDelivery(DeliveryObligation letter, int position, int tokenCost)
    {
        MoveLetterToPosition1(letter, position);
        ShowSkipSuccessNarrative(letter, tokenCost);
        ShiftQueueUp(position);
        RecordLetterSkip(letter);
    }

    // Move letter to specific position
    public void MoveLetterToPosition(DeliveryObligation letter, int targetPosition)
    {
        if (targetPosition < 1 || targetPosition > _gameWorld.GetPlayer().ObligationQueue.Length) return;

        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

        // Clear current position
        for (int i = 0; i < queue.Length; i++)
        {
            if (queue[i] == letter)
            {
                queue[i] = null;
                break;
            }
        }

        // Set new position
        queue[targetPosition - 1] = letter;
        letter.QueuePosition = targetPosition;
    }

    // Move letter to position 1
    private void MoveLetterToPosition1(DeliveryObligation letter, int fromPosition)
    {
        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        queue[0] = letter;
        queue[fromPosition - 1] = null;
        letter.QueuePosition = 1;
    }

    // Show success narrative for skip
    private void ShowSkipSuccessNarrative(DeliveryObligation letter, int tokenCost)
    {
        _messageSystem.AddSystemMessage(
            $"{letter.SenderName}'s letter jumps the queue to position 1!",
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

        DeliveryObligation letter = GenerateLetterFromSender(sender, tokenType);
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
        // Find NPCs that can send letters
        List<NPC> eligibleSenders = allNpcs.Where(npc => npc.LetterTokenTypes.Any()).ToList();

        return eligibleSenders;
    }

    // Select appropriate token type for the sender
    private ConnectionType SelectTokenType(NPC sender)
    {
        // Simple random selection from NPC's available token types
        if (sender.LetterTokenTypes.Any())
        {
            return sender.LetterTokenTypes[_random.Next(sender.LetterTokenTypes.Count)];
        }
        throw new InvalidOperationException($"NPC {sender.Name} has no LetterTokenTypes defined - add token types to NPC definition");
    }

    // Generate a letter from the selected sender
    private DeliveryObligation GenerateLetterFromSender(NPC sender, ConnectionType tokenType)
    {
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        NPC recipient = allNpcs.Where(n => n.ID != sender.ID).FirstOrDefault();
        if (recipient == null) return null;

        return new DeliveryObligation
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = sender.ID,
            SenderName = sender.Name,
            RecipientId = recipient.ID,
            RecipientName = recipient.Name,
            TokenType = tokenType,
            DeadlineInSegments = _random.Next(72, 120), // 3-5 days in segments (24 segments per day)
            Payment = _random.Next(3, 8)
        };
    }

    // Add generated letter to queue and show narrative
    private void AddGeneratedLetter(DeliveryObligation letter, NPC sender, ConnectionType tokenType, ref int lettersGenerated)
    {
        int position = AddLetter(letter);
        if (position > 0)
        {
            lettersGenerated++;
            ShowGeneratedLetterNarrative(letter, sender, tokenType, position);
        }
    }

    // Show narrative for newly generated letter
    private void ShowGeneratedLetterNarrative(DeliveryObligation letter, NPC sender, ConnectionType tokenType, int position)
    {
        string urgency = letter.DeadlineInSegments <= 72 ? " - needs urgent delivery!" : "";
        string tokenTypeText = GetTokenTypeDescription(letter.TokenType);
        string categoryText = GetCategoryText(sender, tokenType);

        _messageSystem.AddSystemMessage(
            $"  • DeliveryObligation from {sender.Name} to {letter.RecipientName} ({tokenTypeText} correspondence{categoryText}){urgency}",
            letter.DeadlineInSegments <= 72 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
        );

        _messageSystem.AddSystemMessage(
            $"    → Enters your queue at position {position} - {letter.Payment} coins on delivery",
            SystemMessageTypes.Info
        );
    }

    // Get category text for letter narrative
    private string GetCategoryText(NPC sender, ConnectionType tokenType)
    {
        // Categories removed - letters are now created through conversations
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
        if (player.LastMorningSwapDay == _timeManager.GetCurrentDay())
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
        DeliveryObligation? letter1 = GetLetterAt(position1);
        DeliveryObligation letter2 = GetLetterAt(position2);

        // At least one position must have a letter
        if (letter1 == null && letter2 == null)
        {
            return false;
        }

        // Perform the swap
        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        queue[position1 - 1] = letter2;
        queue[position2 - 1] = letter1;

        // Queue positions are implicit from array index, no need to update

        // Mark as used today
        player.LastMorningSwapDay = _timeManager.GetCurrentDay();

        return true;
    }

    // Purge: Remove bottom letter for 3 tokens of any type
    public bool TryPurgeLetter(Dictionary<ConnectionType, int> tokenPayment)
    {
        // Check if there's a letter in the last position
        DeliveryObligation letterToPurge = GetLetterAt(_config.LetterQueue.MaxQueueSize);
        if (letterToPurge == null)
        {
            _messageSystem.AddSystemMessage(
                $"No letter in position {_config.LetterQueue.MaxQueueSize} to purge!",
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
                $"  Cannot purge: {reason}",
                SystemMessageTypes.Danger
            );
            return false;
        }

        // Validate token payment totals 3
        int totalTokens = tokenPayment.Values.Sum();
        if (totalTokens != 3)
        {
            _messageSystem.AddSystemMessage(
                $"  Purging requires exactly 3 tokens! You offered {totalTokens}",
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
                    $"  Insufficient {payment.Key} tokens! Need {payment.Value}, have {_connectionTokenManager.GetTokenCount(payment.Key)}",
                    SystemMessageTypes.Danger
                );
                return false;
            }
        }

        // Show the disconnected measure being taken
        _messageSystem.AddSystemMessage(
            $"  💸 Burning social capital to make this letter disappear...",
            SystemMessageTypes.Warning
        );

        // Spend the tokens with narrative focus
        foreach (KeyValuePair<ConnectionType, int> payment in tokenPayment)
        {
            if (payment.Value > 0)
            {
                _messageSystem.AddSystemMessage(
                    $"    • Spending {payment.Value} {payment.Key} token{(payment.Value > 1 ? "s" : "")}",
                    SystemMessageTypes.Warning
                );

                if (!_connectionTokenManager.SpendTokensOfType(payment.Key, payment.Value))
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
        DeliveryObligation letter = GetLetterAt(fromPosition);
        if (letter == null)
        {
            return false; // No letter at position
        }

        // Check if position 1 is occupied
        if (GetLetterAt(1) != null)
        {
            _messageSystem.AddSystemMessage(
                $"Cannot priority move - position 1 is already occupied!",
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
                $"  Insufficient {letter.TokenType} tokens! Need 5, have {_connectionTokenManager.GetTokenCount(letter.TokenType)}",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                $"  • Major favors require substantial social capital",
                SystemMessageTypes.Info
            );
            return false;
        }

        // Spend the tokens with dramatic focus
        _messageSystem.AddSystemMessage(
            $"  💸 Burning 5 {letter.TokenType} tokens with {letter.SenderName} for emergency priority...",
            SystemMessageTypes.Warning
        );

        if (!_connectionTokenManager.SpendTokensWithNPC(letter.TokenType, 5, senderId))
        {
            return false;
        }

        // Move letter to position 1
        DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
        queue[fromPosition - 1] = null; // Clear original position
        queue[0] = letter; // Place in position 1
        letter.QueuePosition = 1;

        // Success narrative
        _messageSystem.AddSystemMessage(
            $"{letter.SenderName}'s letter rockets to position 1!",
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
        DeliveryObligation letter = GetLetterAt(position);
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
        string urgency = letter.DeadlineInSegments <= 48 ? " 🆘 CRITICAL!" : "";
        _messageSystem.AddSystemMessage(
            $"  • Current deadline: {letter.DeadlineInSegments / 24} days{urgency}",
            letter.DeadlineInSegments <= 48 ? SystemMessageTypes.Danger : SystemMessageTypes.Info
        );

        // Check token cost (2 matching tokens)
        if (!_connectionTokenManager.HasTokens(letter.TokenType, 2))
        {
            _messageSystem.AddSystemMessage(
                $"  Insufficient {letter.TokenType} tokens! Need 2, have {_connectionTokenManager.GetTokenCount(letter.TokenType)}",
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
        int oldDeadlineSegments = letter.DeadlineInSegments;
        letter.DeadlineInSegments += 8;

        // Success narrative
        _messageSystem.AddSystemMessage(
            $"{letter.SenderName} grants a 2-day extension!",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"  • New deadline: {letter.DeadlineInSegments / 24} days (was {oldDeadlineSegments / 24})", // 24 segments per day
            SystemMessageTypes.Info
        );

        _messageSystem.AddSystemMessage(
            $"  • \"Just this once,\" {letter.SenderName} says, \"but don't make a habit of it.\"",
            SystemMessageTypes.Info
        );

        return true;
    }

    #region Obligation Manipulation Through Conversation

    /// <summary>
    /// Manipulate an obligation through conversation
    /// </summary>
    public bool ManipulateObligation(string obligationId, ObligationManipulationType manipulation, string npcId)
    {
        DeliveryObligation? obligation = GetActiveObligations().FirstOrDefault(o => o.Id == obligationId);
        if (obligation == null) return false;

        return manipulation switch
        {
            ObligationManipulationType.Prioritize => PrioritizeObligation(obligation),
            ObligationManipulationType.BurnToClear => BurnTokensToClearPath(obligation, npcId),
            ObligationManipulationType.Purge => PurgeObligation(obligation, npcId),
            ObligationManipulationType.ExtendDeadline => ExtendObligationDeadline(obligation, npcId),
            ObligationManipulationType.Transfer => TransferObligation(obligation, npcId),
            ObligationManipulationType.Cancel => CancelObligation(obligation, npcId),
            _ => false,
        };
    }

    /// <summary>
    /// Move an obligation to position 1 (prioritize)
    /// </summary>
    private bool PrioritizeObligation(DeliveryObligation obligation)
    {
        int currentPos = GetQueuePosition(obligation);
        if (currentPos == 1) return true; // Already at position 1

        // Check if position 1 is empty
        DeliveryObligation[] queue = GetPlayerQueue();
        if (queue[0] != null)
        {
            _messageSystem.AddSystemMessage(
                "Cannot prioritize - position 1 is occupied!",
                SystemMessageTypes.Warning
            );
            return false;
        }

        MoveObligationToPosition(obligation, 1);
        _messageSystem.AddSystemMessage(
            $"📌 Moved {obligation.SenderName}'s letter to position 1",
            SystemMessageTypes.Success
        );
        return true;
    }

    /// <summary>
    /// Burn tokens to clear queue slots above an obligation
    /// </summary>
    private bool BurnTokensToClearPath(DeliveryObligation obligation, string npcId)
    {
        int currentPos = GetQueuePosition(obligation);
        if (currentPos == 1) return true; // Already at top

        DeliveryObligation[] queue = GetPlayerQueue();
        int tokenCost = 0;

        // Calculate cost - 2 tokens per letter to purge
        for (int i = 0; i < currentPos - 1; i++)
        {
            if (queue[i] != null)
            {
                tokenCost += 2;
            }
        }

        if (tokenCost == 0) return true; // Path already clear

        // Check if player has enough tokens
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int totalTokens = tokens.Values.Sum();

        if (totalTokens < tokenCost)
        {
            _messageSystem.AddSystemMessage(
                $"Need {tokenCost} tokens to clear path, but only have {totalTokens}",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Spend tokens and clear path
        _connectionTokenManager.SpendTokens(obligation.TokenType, tokenCost, npcId);

        // Remove all letters above
        for (int i = 0; i < currentPos - 1; i++)
        {
            if (queue[i] != null)
            {
                RemoveObligationFromQueue(i + 1);
            }
        }

        // Move obligation to position 1
        MoveObligationToPosition(obligation, 1);

        _messageSystem.AddSystemMessage(
            $"🔥 Burned {tokenCost} tokens to clear path for {obligation.SenderName}'s letter",
            SystemMessageTypes.Success
        );
        return true;
    }

    /// <summary>
    /// Purge an obligation using tokens
    /// </summary>
    private bool PurgeObligation(DeliveryObligation obligation, string npcId)
    {
        int purgeTokenCost = 3; // Cost to purge

        // Check if player has enough tokens
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int availableTokens = tokens.GetValueOrDefault(obligation.TokenType, 0);

        if (availableTokens < purgeTokenCost)
        {
            _messageSystem.AddSystemMessage(
                $"Need {purgeTokenCost} {obligation.TokenType} tokens to purge, but only have {availableTokens}",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Spend tokens and remove obligation
        _connectionTokenManager.SpendTokens(obligation.TokenType, purgeTokenCost, npcId);
        RemoveObligationFromQueue(GetQueuePosition(obligation));

        _messageSystem.AddSystemMessage(
            $"💨 Purged {obligation.SenderName}'s letter using {purgeTokenCost} {obligation.TokenType} tokens",
            SystemMessageTypes.Success
        );
        return true;
    }

    /// <summary>
    /// Extend the deadline of an obligation
    /// </summary>
    private bool ExtendObligationDeadline(DeliveryObligation obligation, string npcId)
    {
        int extensionCost = 2; // Tokens to extend deadline
        int extensionMinutes = 1440; // 1 day extension

        // Check if player has enough tokens
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int availableTokens = tokens.GetValueOrDefault(obligation.TokenType, 0);

        if (availableTokens < extensionCost)
        {
            _messageSystem.AddSystemMessage(
                $"Need {extensionCost} {obligation.TokenType} tokens to extend deadline",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Spend tokens and extend deadline
        _connectionTokenManager.SpendTokens(obligation.TokenType, extensionCost, npcId);
        obligation.DeadlineInSegments += extensionMinutes;

        _messageSystem.AddSystemMessage(
            $"Extended deadline for {obligation.SenderName}'s letter by 1 day",
            SystemMessageTypes.Success
        );
        return true;
    }

    /// <summary>
    /// Transfer an obligation to another NPC
    /// </summary>
    private bool TransferObligation(DeliveryObligation obligation, string newRecipientId)
    {
        int transferCost = 4; // High cost to transfer

        // Check if player has enough tokens with original sender
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(obligation.SenderId);
        int availableTokens = tokens.GetValueOrDefault(obligation.TokenType, 0);

        if (availableTokens < transferCost)
        {
            _messageSystem.AddSystemMessage(
                $"Need {transferCost} {obligation.TokenType} tokens with {obligation.SenderName} to transfer",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Get new recipient
        NPC newRecipient = _npcRepository.GetById(newRecipientId);
        if (newRecipient == null) return false;

        // Spend tokens and transfer
        _connectionTokenManager.SpendTokens(obligation.TokenType, transferCost, obligation.SenderId);
        obligation.RecipientId = newRecipientId;
        obligation.RecipientName = newRecipient.Name;

        _messageSystem.AddSystemMessage(
            $"📤 Transferred letter from {obligation.SenderName} to {newRecipient.Name}",
            SystemMessageTypes.Success
        );
        return true;
    }

    /// <summary>
    /// Cancel an obligation (requires high relationship)
    /// </summary>
    private bool CancelObligation(DeliveryObligation obligation, string npcId)
    {
        int cancelTokenRequirement = 10; // Need high relationship to cancel

        // Check relationship level
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int totalTokens = tokens.Values.Sum();

        if (totalTokens < cancelTokenRequirement)
        {
            _messageSystem.AddSystemMessage(
                $"Need {cancelTokenRequirement} total tokens with {obligation.SenderName} to cancel (have {totalTokens})",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Don't spend tokens, just require the relationship
        RemoveObligationFromQueue(GetQueuePosition(obligation));

        _messageSystem.AddSystemMessage(
            $"{obligation.SenderName} agrees to cancel the letter delivery",
            SystemMessageTypes.Success
        );
        return true;
    }

    #endregion

    /// <summary>
    /// Prepare to skip an obligation (queue manipulation)
    /// </summary>
    public bool PrepareSkipAction(string obligationId)
    {
        DeliveryObligation? obligation = GetActiveObligations().FirstOrDefault(o => o.Id == obligationId);
        if (obligation == null) return false;

        // For now, skip moves obligation to end of queue
        int position = GetQueuePosition(obligation);
        if (position > 0)
        {
            MoveObligationToPosition(obligation, _config.LetterQueue.MaxQueueSize);
            _messageSystem.AddSystemMessage(
                $"Moved {obligation.SenderName}'s letter to end of queue",
                SystemMessageTypes.Info
            );
            return true;
        }
        return false;
    }

    /// <summary>
    /// Prepare to purge an obligation (remove from queue)
    /// </summary>
    public bool PreparePurgeAction(string obligationId)
    {
        DeliveryObligation? obligation = GetActiveObligations().FirstOrDefault(o => o.Id == obligationId);
        if (obligation == null) return false;

        // Purge requires token cost or consequences
        int position = GetQueuePosition(obligation);
        if (position > 0)
        {
            RemoveObligationFromQueue(position);
            _messageSystem.AddSystemMessage(
                $"Purged {obligation.SenderName}'s letter - expect consequences",
                SystemMessageTypes.Warning
            );
            return true;
        }
        return false;
    }

    #region Queue Displacement with Token Burning

    /// <summary>
    /// Displace an obligation by burning tokens with displaced NPCs
    /// Implements the core queue displacement system from POC requirements
    /// </summary>
    public QueueDisplacementResult TryDisplaceObligation(string obligationId, int targetPosition)
    {
        QueueDisplacementResult result = new QueueDisplacementResult();

        // Find the obligation
        DeliveryObligation? obligation = GetActiveObligations().FirstOrDefault(o => o.Id == obligationId);
        if (obligation == null)
        {
            result.ErrorMessage = "Obligation not found in queue";
            return result;
        }

        int currentPosition = GetQueuePosition(obligation);
        if (currentPosition <= 0)
        {
            result.ErrorMessage = "Unable to determine obligation position";
            return result;
        }

        // Can't displace backwards (position must be lower number = earlier in queue)
        if (targetPosition >= currentPosition)
        {
            result.ErrorMessage = $"Cannot displace backwards from position {currentPosition} to {targetPosition}";
            return result;
        }

        // Can't displace to position 1 or beyond valid range
        if (targetPosition < 1 || targetPosition > _config.LetterQueue.MaxQueueSize)
        {
            result.ErrorMessage = $"Invalid target position {targetPosition}";
            return result;
        }

        // Calculate displacement plan
        ObligationDisplacementPlan displacementPlan = CalculateDisplacementCost(obligation, currentPosition, targetPosition);
        if (displacementPlan.TotalTokenCost == 0)
        {
            result.ErrorMessage = "No displacement needed";
            return result;
        }

        result.DisplacementPlan = displacementPlan;
        result.CanExecute = ValidateTokenAvailability(displacementPlan);

        if (!result.CanExecute)
        {
            result.ErrorMessage = "Insufficient tokens for displacement";
            return result;
        }

        return result;
    }

    /// <summary>
    /// Execute the displacement by burning tokens and moving obligations
    /// </summary>
    public bool ExecuteDisplacement(QueueDisplacementResult displacementResult)
    {
        if (!displacementResult.CanExecute || displacementResult.DisplacementPlan == null)
        {
            _messageSystem.AddSystemMessage("Cannot execute displacement", SystemMessageTypes.Danger);
            return false;
        }

        ObligationDisplacementPlan plan = displacementResult.DisplacementPlan;
        DeliveryObligation obligation = plan.ObligationToMove;

        _messageSystem.AddSystemMessage(
            $"🔥 BURNING TOKENS: Moving {obligation.SenderName}'s letter from position {plan.OriginalPosition} to {plan.TargetPosition}",
            SystemMessageTypes.Warning
        );

        // Burn tokens with each displaced NPC
        foreach (ObligationDisplacement displacement in plan.Displacements)
        {
            string npcId = GetNPCIdByName(displacement.DisplacedObligation.SenderName);
            if (!string.IsNullOrEmpty(npcId))
            {
                bool success = _connectionTokenManager.SpendTokensWithNPC(
                    displacement.DisplacedObligation.TokenType,
                    displacement.TokenCost,
                    npcId
                );

                if (success)
                {
                    _messageSystem.AddSystemMessage(
                        $"💸 Burned {displacement.TokenCost} {displacement.DisplacedObligation.TokenType} tokens with {displacement.DisplacedObligation.SenderName}",
                        SystemMessageTypes.Warning
                    );
                }
                else
                {
                    _messageSystem.AddSystemMessage(
                        $"Failed to burn tokens with {displacement.DisplacedObligation.SenderName}",
                        SystemMessageTypes.Danger
                    );
                    return false;
                }
            }
        }

        // Execute the queue rearrangement
        PerformQueueDisplacement(plan);

        _messageSystem.AddSystemMessage(
            $"Successfully moved {obligation.SenderName}'s letter to position {plan.TargetPosition}!",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"Total relationship cost: {plan.TotalTokenCost} tokens burned permanently",
            SystemMessageTypes.Warning
        );

        return true;
    }

    /// <summary>
    /// Calculate the cost and plan for displacing an obligation
    /// </summary>
    private ObligationDisplacementPlan CalculateDisplacementCost(DeliveryObligation obligation, int currentPosition, int targetPosition)
    {
        ObligationDisplacementPlan plan = new ObligationDisplacementPlan
        {
            ObligationToMove = obligation,
            OriginalPosition = currentPosition,
            TargetPosition = targetPosition
        };

        DeliveryObligation[] queue = GetPlayerQueue();

        // Calculate jumps and affected obligations
        int positionsJumped = currentPosition - targetPosition;

        // Each obligation that gets displaced costs tokens equal to the jump distance
        for (int pos = targetPosition; pos < currentPosition; pos++)
        {
            DeliveryObligation displacedObligation = queue[pos - 1];
            if (displacedObligation != null)
            {
                ObligationDisplacement displacement = new ObligationDisplacement
                {
                    DisplacedObligation = displacedObligation,
                    OriginalPosition = pos,
                    NewPosition = pos + 1,
                    TokenCost = positionsJumped // Cost equals positions jumped
                };

                plan.Displacements.Add(displacement);
                plan.TotalTokenCost += displacement.TokenCost;
            }
        }

        return plan;
    }

    /// <summary>
    /// Validate the player has enough tokens for the displacement
    /// </summary>
    private bool ValidateTokenAvailability(ObligationDisplacementPlan plan)
    {
        foreach (ObligationDisplacement displacement in plan.Displacements)
        {
            string npcId = GetNPCIdByName(displacement.DisplacedObligation.SenderName);
            if (string.IsNullOrEmpty(npcId)) continue;

            Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
            int availableTokens = tokens.GetValueOrDefault(displacement.DisplacedObligation.TokenType, 0);

            if (availableTokens < displacement.TokenCost)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Perform the actual queue rearrangement
    /// </summary>
    private void PerformQueueDisplacement(ObligationDisplacementPlan plan)
    {
        DeliveryObligation[] queue = GetPlayerQueue();

        // Remove the obligation from its current position
        queue[plan.OriginalPosition - 1] = null;

        // Shift affected obligations down by one position
        for (int pos = plan.TargetPosition; pos < plan.OriginalPosition; pos++)
        {
            if (queue[pos - 1] != null)
            {
                DeliveryObligation shiftedObligation = queue[pos - 1];
                queue[pos] = shiftedObligation; // Move to next position (pos + 1, 0-indexed)
                shiftedObligation.QueuePosition = pos + 1;
            }
        }

        // Insert the displaced obligation at the target position
        queue[plan.TargetPosition - 1] = plan.ObligationToMove;
        plan.ObligationToMove.QueuePosition = plan.TargetPosition;
    }

    /// <summary>
    /// Get displacement cost preview for UI
    /// </summary>
    public QueueDisplacementPreview GetDisplacementPreview(string obligationId, int targetPosition)
    {
        QueueDisplacementResult result = TryDisplaceObligation(obligationId, targetPosition);

        QueueDisplacementPreview preview = new QueueDisplacementPreview
        {
            CanExecute = result.CanExecute,
            ErrorMessage = result.ErrorMessage,
            TotalTokenCost = result.DisplacementPlan?.TotalTokenCost ?? 0,
            DisplacementDetails = new List<DisplacementDetail>()
        };

        if (result.DisplacementPlan != null)
        {
            foreach (ObligationDisplacement displacement in result.DisplacementPlan.Displacements)
            {
                preview.DisplacementDetails.Add(new DisplacementDetail
                {
                    NPCName = displacement.DisplacedObligation.SenderName,
                    TokenType = displacement.DisplacedObligation.TokenType,
                    TokenCost = displacement.TokenCost,
                    FromPosition = displacement.OriginalPosition,
                    ToPosition = displacement.NewPosition
                });
            }
        }

        return preview;
    }

    #endregion

    #region Automatic Queue Displacement System (POC Package 4)

    /// <summary>
    /// Automatically displace obligations when a new obligation forces position 1
    /// Used for failed letter negotiations, crisis letters, and proud NPCs
    /// </summary>
    public bool AutomaticDisplacement(DeliveryObligation newObligation, int forcedPosition, string displacementReason)
    {
        if (newObligation == null || forcedPosition < 1 || forcedPosition > _config.LetterQueue.MaxQueueSize)
        {
            _messageSystem.AddSystemMessage(
                $"Invalid displacement request for position {forcedPosition}",
                SystemMessageTypes.Danger
            );
            return false;
        }

        DeliveryObligation[] queue = GetPlayerQueue();

        // Announce the forced displacement
        _messageSystem.AddSystemMessage(
            $"⚡ {newObligation.SenderName}'s letter FORCES position {forcedPosition}!",
            SystemMessageTypes.Danger
        );
        _messageSystem.AddSystemMessage(
            $"  • {displacementReason}",
            SystemMessageTypes.Warning
        );

        // Check if the forced position is already occupied
        if (queue[forcedPosition - 1] != null)
        {
            // Calculate and execute displacement cascade
            List<DisplacedObligation> displacedObligations = CalculateDisplacementCascade(queue, forcedPosition);

            if (displacedObligations.Count > 0)
            {
                _messageSystem.AddSystemMessage(
                    $"📦 QUEUE CASCADE: {displacedObligations.Count} obligations displaced!",
                    SystemMessageTypes.Warning
                );

                // Burn tokens with each displaced NPC
                foreach (DisplacedObligation displaced in displacedObligations)
                {
                    BurnDisplacementTokens(displaced.Obligation, displaced.DisplacementAmount);
                }

                // Execute the cascade
                ExecuteDisplacementCascade(queue, forcedPosition, displacedObligations);
            }
        }

        // Place the new obligation at the forced position
        queue[forcedPosition - 1] = newObligation;
        newObligation.QueuePosition = forcedPosition;

        // Set positioning metadata for UI display
        newObligation.PositioningReason = DeterminePositioningReasonForForced(newObligation, displacementReason);
        newObligation.FinalQueuePosition = forcedPosition;

        _messageSystem.AddSystemMessage(
            $"{newObligation.SenderName}'s letter locked into position {forcedPosition}",
            SystemMessageTypes.Success
        );

        return true;
    }

    /// <summary>
    /// Calculate which obligations will be displaced and by how much
    /// </summary>
    private List<DisplacedObligation> CalculateDisplacementCascade(DeliveryObligation[] queue, int forcedPosition)
    {
        List<DisplacedObligation> displaced = new List<DisplacedObligation>();

        // Starting from the forced position, cascade everything down
        for (int i = forcedPosition - 1; i < queue.Length - 1; i++)
        {
            if (queue[i] != null)
            {
                // This obligation will be pushed down by 1 position
                displaced.Add(new DisplacedObligation
                {
                    Obligation = queue[i],
                    OriginalPosition = i + 1,
                    NewPosition = i + 2,
                    DisplacementAmount = 1  // Each gets pushed down by 1 in a cascade
                });
            }
        }

        // Check if any obligation falls off the end
        if (queue[queue.Length - 1] != null)
        {
            displaced.Add(new DisplacedObligation
            {
                Obligation = queue[queue.Length - 1],
                OriginalPosition = queue.Length,
                NewPosition = -1,  // Falls off the queue
                DisplacementAmount = 1
            });
        }

        return displaced;
    }

    /// <summary>
    /// Burn tokens with an NPC when their obligation is displaced
    /// </summary>
    private void BurnDisplacementTokens(DeliveryObligation displacedObligation, int positionsDisplaced)
    {
        string npcId = GetNPCIdByName(displacedObligation.SenderName);
        if (string.IsNullOrEmpty(npcId)) return;

        // Calculate token cost based on displacement distance
        int tokenCost = Math.Min(3, positionsDisplaced);  // Cap at 3 tokens max

        // Try to burn tokens with this NPC
        Dictionary<ConnectionType, int> tokens = _connectionTokenManager.GetTokensWithNPC(npcId);
        int availableTokens = tokens.GetValueOrDefault(displacedObligation.TokenType, 0);

        if (availableTokens >= tokenCost)
        {
            // Burn the tokens
            _connectionTokenManager.RemoveTokensFromNPC(displacedObligation.TokenType, tokenCost, npcId);

            _messageSystem.AddSystemMessage(
                $"💔 Burned {tokenCost} {displacedObligation.TokenType} token(s) with {displacedObligation.SenderName}",
                SystemMessageTypes.Danger
            );

            // Add burden cards to the NPC's deck for each burned token
            for (int i = 0; i < tokenCost; i++)
            {
                AddBurdenCardToNPC(npcId, displacedObligation.TokenType);
            }
        }
        else if (availableTokens > 0)
        {
            // Burn what we can
            _connectionTokenManager.RemoveTokensFromNPC(displacedObligation.TokenType, availableTokens, npcId);

            _messageSystem.AddSystemMessage(
                $"💔 Burned {availableTokens} {displacedObligation.TokenType} token(s) with {displacedObligation.SenderName} (all they had)",
                SystemMessageTypes.Danger
            );

            // Add burden cards
            for (int i = 0; i < availableTokens; i++)
            {
                AddBurdenCardToNPC(npcId, displacedObligation.TokenType);
            }
        }
        else
        {
            // No tokens to burn - relationship already terrible
            _messageSystem.AddSystemMessage(
                $"No tokens left to burn with {displacedObligation.SenderName} - relationship already ruined",
                SystemMessageTypes.Warning
            );
        }
    }

    /// <summary>
    /// Add burden cards to an NPC's burden deck when tokens are burned
    /// </summary>
    private void AddBurdenCardToNPC(string npcId, ConnectionType tokenType)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return;

        // Initialize burden deck if needed
        if (npc.BurdenDeck == null)
        {
            npc.InitializeBurdenDeck();
        }

        // Create a burden card representing the damaged relationship
        ConversationCard burdenCard = new ConversationCard
        {
            Id = $"burden_{npcId}_{Guid.NewGuid()}",
            CardType = CardType.Letter,  // Burden cards use BurdenGoal type
            Description = $"Past betrayal weighs on {npc.Name}'s mind",
            TokenType = tokenType,
            Persistence = PersistenceType.Thought,  // Burdens persist
            SuccessType = SuccessEffectType.None,
            FailureType = FailureEffectType.None,
        };

        // Add to the NPC's burden deck
        npc.BurdenDeck.AddCard(burdenCard);

        _messageSystem.AddSystemMessage(
            $"  • {npc.Name} gains a burden card (relationship damage)",
            SystemMessageTypes.Info
        );
    }

    /// <summary>
    /// Execute the displacement cascade, moving obligations down
    /// </summary>
    private void ExecuteDisplacementCascade(DeliveryObligation[] queue, int forcedPosition, List<DisplacedObligation> displacements)
    {
        // First, clear the forced position and everything after it
        DeliveryObligation[] tempQueue = new DeliveryObligation[queue.Length];

        // Copy obligations before the forced position
        for (int i = 0; i < forcedPosition - 1; i++)
        {
            tempQueue[i] = queue[i];
        }

        // Leave the forced position empty (will be filled by new obligation)
        tempQueue[forcedPosition - 1] = null;

        // Place displaced obligations in their new positions
        foreach (DisplacedObligation displaced in displacements)
        {
            if (displaced.NewPosition > 0 && displaced.NewPosition <= queue.Length)
            {
                tempQueue[displaced.NewPosition - 1] = displaced.Obligation;
                displaced.Obligation.QueuePosition = displaced.NewPosition;

                _messageSystem.AddSystemMessage(
                    $"  • {displaced.Obligation.SenderName}'s letter displaced: position {displaced.OriginalPosition} → {displaced.NewPosition}",
                    SystemMessageTypes.Info
                );
            }
            else if (displaced.NewPosition == -1)
            {
                // This obligation fell off the queue
                _messageSystem.AddSystemMessage(
                    $"💥 {displaced.Obligation.SenderName}'s letter FORCED OUT of queue!",
                    SystemMessageTypes.Danger
                );

                // Apply severe relationship penalty for being forced out
                string senderId = GetNPCIdByName(displaced.Obligation.SenderName);
                _connectionTokenManager.RemoveTokensFromNPC(displaced.Obligation.TokenType, 3, senderId);

                _messageSystem.AddSystemMessage(
                    $"  • Lost 3 additional {displaced.Obligation.TokenType} tokens for forcing their letter out completely",
                    SystemMessageTypes.Danger
                );
            }
        }

        // Copy the rearranged queue back
        for (int i = 0; i < queue.Length; i++)
        {
            queue[i] = tempQueue[i];
        }
    }

    /// <summary>
    /// Determine the positioning reason for forced displacement
    /// </summary>
    private LetterPositioningReason DeterminePositioningReasonForForced(DeliveryObligation obligation, string displacementReason)
    {
        if (displacementReason.Contains("failed", StringComparison.OrdinalIgnoreCase))
        {
            return LetterPositioningReason.PoorStanding;
        }
        else if (displacementReason.Contains("proud", StringComparison.OrdinalIgnoreCase) ||
                 displacementReason.Contains("Lord Blackwood", StringComparison.OrdinalIgnoreCase))
        {
            return LetterPositioningReason.CommerceDebt;  // Using this to indicate power/status
        }
        else
        {
            return LetterPositioningReason.Neutral;
        }
    }

    /// <summary>
    /// Helper class for tracking displaced obligations
    /// </summary>
    private class DisplacedObligation
    {
        public DeliveryObligation Obligation { get; set; }
        public int OriginalPosition { get; set; }
        public int NewPosition { get; set; }
        public int DisplacementAmount { get; set; }
    }

    #endregion

}
