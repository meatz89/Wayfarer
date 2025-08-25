using System;
using System.Linq;

public class StandingObligationManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly TokenMechanicsManager _connectionTokenManager;
    private readonly StandingObligationRepository _obligationRepository;
    private readonly TimeManager _timeManager;

    public StandingObligationManager(GameWorld gameWorld, MessageSystem messageSystem, TokenMechanicsManager connectionTokenManager, StandingObligationRepository obligationRepository, TimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _connectionTokenManager = connectionTokenManager;
        _obligationRepository = obligationRepository;
        _timeManager = timeManager;
    }

    // Get all active obligations for the player
    public List<StandingObligation> GetActiveObligations()
    {
        return _gameWorld.GetPlayer().StandingObligations
            .Where(o => o.IsActive)
            .ToList();
    }

    // Add a new obligation to the player
    public bool AddObligation(StandingObligation obligation)
    {
        if (obligation == null) return false;

        Player player = _gameWorld.GetPlayer();

        // Check for conflicts with existing obligations
        List<StandingObligation> conflicts = CheckObligationConflicts(obligation);
        if (conflicts.Any())
        {
            _messageSystem.AddSystemMessage(
                $"Cannot accept {obligation.Name}: conflicts with {string.Join(", ", conflicts.Select(c => c.Name))}",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Add the obligation
        player.StandingObligations.Add(obligation);

        _messageSystem.AddSystemMessage(
            $"Accepted Standing Obligation: {obligation.Name}",
            SystemMessageTypes.Success
        );

        // Log the effects for the player
        _messageSystem.AddSystemMessage(
            obligation.GetEffectsSummary(),
            SystemMessageTypes.Info
        );

        return true;
    }

    // Remove an obligation (rare, usually has consequences)
    public bool RemoveObligation(string obligationId, bool isVoluntary = true)
    {
        Player player = _gameWorld.GetPlayer();
        StandingObligation? obligation = player.StandingObligations.FirstOrDefault(o => o.ID == obligationId);

        if (obligation == null) return false;

        // Apply consequences for breaking obligations voluntarily
        if (isVoluntary)
        {
            ApplyBreakingConsequences(obligation);
        }

        obligation.IsActive = false;

        _messageSystem.AddSystemMessage(
            $"Standing Obligation removed: {obligation.Name}",
            isVoluntary ? SystemMessageTypes.Warning : SystemMessageTypes.Info
        );

        return true;
    }

    // Check for conflicts between obligations
    public List<StandingObligation> CheckObligationConflicts(StandingObligation newObligation)
    {
        List<StandingObligation> conflicts = new List<StandingObligation>();
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation existing in activeObligations)
        {
            // Max one obligation per NPC rule (US-8.1)
            if (existing.RelatedNPCId == newObligation.RelatedNPCId)
            {
                conflicts.Add(existing);
                continue;
            }

            // Same token type with conflicting effects
            if (existing.RelatedTokenType == newObligation.RelatedTokenType)
            {
                // Check for directly conflicting effects
                if (HasConflictingEffects(existing, newObligation))
                {
                    conflicts.Add(existing);
                }
            }

            // Special conflict cases
            if (IsSpecialConflict(existing, newObligation))
            {
                conflicts.Add(existing);
            }
        }

        return conflicts;
    }

    // Check if we should generate forced letters today
    public List<StandingObligation> GetObligationsRequiringForcedLetters()
    {
        return GetActiveObligations()
            .Where(o => o.ShouldGenerateForcedLetter())
            .ToList();
    }

    // Process daily obligations and return any forced letters generated
    public List<DeliveryObligation> ProcessDailyObligations(int currentDay)
    {
        List<DeliveryObligation> forcedLetters = new List<DeliveryObligation>();
        List<StandingObligation> obligationsNeedingLetters = GetObligationsRequiringForcedLetters();

        foreach (StandingObligation obligation in obligationsNeedingLetters)
        {
            DeliveryObligation letter = GenerateForcedLetter(obligation);
            if (letter != null)
            {
                forcedLetters.Add(letter);
                obligation.RecordForcedLetterGenerated();

                _messageSystem.AddSystemMessage(
                    $"Forced letter generated from {obligation.Name}: {letter.SenderName} → {letter.RecipientName}",
                    SystemMessageTypes.Info
                );
            }
        }

        return forcedLetters;
    }

    // Obligations no longer generate forced letters
    // Letters are ONLY created through conversation choices
    public DeliveryObligation GenerateForcedLetter(StandingObligation obligation)
    {
        // This method is deprecated - obligations create conversation opportunities, not automatic letters
        _messageSystem.AddSystemMessage(
            $"Obligation {obligation.Name} creates new conversation opportunities!",
            SystemMessageTypes.Info
        );
        return null;
    }


    // Advance time for all obligations (called daily)
    public void AdvanceDailyTime()
    {
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            obligation.DaysSinceLastForcedDeliveryObligation++;
        }
    }

    // Helper to get NPC ID from name
    private string GetNPCIdByName(string npcName)
    {
        // This would typically use NPCRepository, but we don't have it injected here
        // For now, return empty string - the calling code should handle this
        return "";
    }


    // Calculate total skip cost multiplier
    public int CalculateSkipCostMultiplier(DeliveryObligation letter)
    {
        int maxMultiplier = 1;
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            int multiplier = obligation.CalculateSkipCostMultiplier(letter);
            maxMultiplier = Math.Max(maxMultiplier, multiplier);
        }

        return maxMultiplier;
    }

    // Check if any obligation forbids an action
    public bool IsActionForbidden(string actionType, DeliveryObligation letter, out string reason)
    {
        reason = "";
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            if (obligation.IsForbiddenAction(actionType, letter))
            {
                reason = $"Forbidden by {obligation.Name}";
                return true;
            }
        }

        return false;
    }

    // Check for threshold-based obligations that should activate or deactivate
    public void CheckThresholdActivations()
    {
        Player player = _gameWorld.GetPlayer();
        List<StandingObligation> allTemplates = _obligationRepository.GetAllObligationTemplates();

        // Check each threshold-based obligation template
        foreach (StandingObligation template in allTemplates.Where(o => o.IsThresholdBased))
        {
            // Skip if not properly configured
            if (!template.ActivationThreshold.HasValue || !template.RelatedTokenType.HasValue)
                continue;

            // Get relevant token count
            int tokenCount = GetRelevantTokenCount(template);

            // Check if player already has this obligation
            StandingObligation? existingObligation = player.StandingObligations
                .FirstOrDefault(o => o.ID == template.ID);

            if (existingObligation == null)
            {
                // Check if we should activate this obligation
                if (template.ShouldBeActiveForTokenCount(tokenCount))
                {
                    ActivateThresholdObligation(template, tokenCount);
                }
            }
            else if (existingObligation.WasAutoActivated)
            {
                // Check if we should deactivate this obligation
                if (existingObligation.ShouldDeactivateForTokenCount(tokenCount))
                {
                    DeactivateThresholdObligation(existingObligation, tokenCount);
                }
            }
        }
    }

    // Get the relevant token count for a threshold-based obligation
    private int GetRelevantTokenCount(StandingObligation obligation)
    {
        if (!obligation.RelatedTokenType.HasValue)
            return 0;

        if (!string.IsNullOrEmpty(obligation.RelatedNPCId))
        {
            // Get tokens with specific NPC
            Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(obligation.RelatedNPCId);
            return npcTokens.GetValueOrDefault(obligation.RelatedTokenType.Value, 0);
        }
        else
        {
            // Get total tokens of type
            return _connectionTokenManager.GetTokenCount(obligation.RelatedTokenType.Value);
        }
    }

    // Get the relevant token count for a letter-based calculation
    private int GetRelevantTokenCountForLetter(StandingObligation obligation, DeliveryObligation letter, string senderId)
    {
        // If obligation is NPC-specific, only apply if letter is from that NPC
        if (!string.IsNullOrEmpty(obligation.RelatedNPCId) && obligation.RelatedNPCId != senderId)
        {
            return 0;
        }

        // Get tokens with the letter sender
        Dictionary<ConnectionType, int> npcTokens = _connectionTokenManager.GetTokensWithNPC(senderId);

        // Use obligation's token type if specified, otherwise use letter's token type
        ConnectionType relevantType = obligation.RelatedTokenType ?? letter.TokenType;
        return npcTokens.GetValueOrDefault(relevantType, 0);
    }

    // Activate a threshold-based obligation
    private void ActivateThresholdObligation(StandingObligation template, int currentTokenCount)
    {
        // Create a copy of the template for the player
        StandingObligation newObligation = new StandingObligation
        {
            ID = template.ID,
            Name = template.Name,
            Description = template.Description,
            Source = template.Source,
            BenefitEffects = new List<ObligationEffect>(template.BenefitEffects),
            ConstraintEffects = new List<ObligationEffect>(template.ConstraintEffects),
            RelatedTokenType = template.RelatedTokenType,
            RelatedNPCId = template.RelatedNPCId,
            ActivationThreshold = template.ActivationThreshold,
            DeactivationThreshold = template.DeactivationThreshold,
            IsThresholdBased = true,
            ActivatesAboveThreshold = template.ActivatesAboveThreshold,
            DayAccepted = _gameWorld.CurrentDay,
            IsActive = true,
            WasAutoActivated = true,
            DaysSinceAccepted = 0,
            DaysSinceLastForcedDeliveryObligation = 0
        };

        Player player = _gameWorld.GetPlayer();
        player.StandingObligations.Add(newObligation);

        // Announce activation
        string thresholdDirection = template.ActivatesAboveThreshold ? "reached" : "dropped to";
        string npcInfo = !string.IsNullOrEmpty(template.RelatedNPCId) ? $" with {template.RelatedNPCId}" : "";

        _messageSystem.AddSystemMessage(
            $"Standing Obligation Activated: {newObligation.Name}",
            SystemMessageTypes.Warning
        );

        _messageSystem.AddSystemMessage(
            $"Your {template.RelatedTokenType} tokens{npcInfo} have {thresholdDirection} {currentTokenCount}.",
            SystemMessageTypes.Info
        );

        _messageSystem.AddSystemMessage(
            newObligation.GetEffectsSummary(),
            SystemMessageTypes.Info
        );
    }

    // Deactivate a threshold-based obligation
    private void DeactivateThresholdObligation(StandingObligation obligation, int currentTokenCount)
    {
        obligation.IsActive = false;

        // Announce deactivation
        string thresholdDirection = obligation.ActivatesAboveThreshold ? "dropped below" : "risen above";
        string npcInfo = !string.IsNullOrEmpty(obligation.RelatedNPCId) ? $" with {obligation.RelatedNPCId}" : "";

        _messageSystem.AddSystemMessage(
            $"Standing Obligation Deactivated: {obligation.Name}",
            SystemMessageTypes.Success
        );

        _messageSystem.AddSystemMessage(
            $"Your {obligation.RelatedTokenType} tokens{npcInfo} have {thresholdDirection} the required threshold.",
            SystemMessageTypes.Info
        );
    }

    // Helper methods
    private bool HasConflictingEffects(StandingObligation existing, StandingObligation newObligation)
    {
        // Check for effects that cannot coexist
        IEnumerable<ObligationEffect> allExistingEffects = existing.BenefitEffects.Concat(existing.ConstraintEffects);
        IEnumerable<ObligationEffect> allNewEffects = newObligation.BenefitEffects.Concat(newObligation.ConstraintEffects);

        // Example: Cannot have both "Cannot purge trade letters" and "Must purge trade letters"
        // This is a simplified check - expand as needed

        return false; // No conflicts for now
    }

    private bool IsSpecialConflict(StandingObligation existing, StandingObligation newObligation)
    {
        // Special cases where obligations conflict regardless of token type
        // For example, patron-related obligations might conflict

        return false; // No special conflicts for now
    }

    private void ApplyBreakingConsequences(StandingObligation obligation)
    {
        // Apply exactly -5 token penalty for breaking obligations (US-8.3)
        _connectionTokenManager.SpendTokens(obligation.RelatedTokenType.Value, GameRules.OBLIGATION_BREAKING_PENALTY);

        _messageSystem.AddSystemMessage(
            $"Lost {GameRules.OBLIGATION_BREAKING_PENALTY} {obligation.RelatedTokenType} tokens for breaking {obligation.Name}",
            SystemMessageTypes.Danger
        );

        // CRITICAL: Trigger HOSTILE state by making NPC's letters overdue
        // This creates the path: obligation breaking → overdue letters → HOSTILE state → betrayal cards available
        if (!string.IsNullOrEmpty(obligation.RelatedNPCId))
        {
            TriggerHostileStateForNPC(obligation.RelatedNPCId, obligation.Name);
        }
    }

    /// <summary>
    /// Trigger HOSTILE relationship state for an NPC due to broken obligation.
    /// Uses proper NPCRelationship.Hostile state instead of temporal data corruption.
    /// </summary>
    private void TriggerHostileStateForNPC(string npcId, string obligationName)
    {
        NPC npc = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null) return;

        // Use proper NPCStateOperations to set BETRAYED relationship
        NPCState currentState = NPCState.FromNPC(npc);
        NPCOperationResult result = NPCStateOperations.UpdateRelationship(currentState, NPCRelationship.Betrayed);

        if (result.IsSuccess)
        {
            // Update the mutable NPC object (until full immutable migration)
            npc.PlayerRelationship = NPCRelationship.Betrayed;

            _messageSystem.AddSystemMessage(
                $"💀 {npc.Name} is now HOSTILE - breaking {obligationName} has severe consequences!",
                SystemMessageTypes.Danger
            );

            _messageSystem.AddSystemMessage(
                $"🗡️ Betrayal conversation options are now available with {npc.Name}",
                SystemMessageTypes.Warning
            );

            _messageSystem.AddSystemMessage(
                result.Message,
                SystemMessageTypes.Info
            );
        }
    }


    // Apply dynamic deadline bonuses from obligations
    public void ApplyDynamicDeadlineBonuses(DeliveryObligation letter)
    {
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            if (obligation.HasEffect(ObligationEffect.DynamicDeadlineBonus))
            {
                string senderId = GetNPCIdByName(letter.SenderName);
                if (!string.IsNullOrEmpty(senderId))
                {
                    int tokenCount = GetRelevantTokenCountForLetter(obligation, letter, senderId);
                    int deadlineBonus = obligation.CalculateDynamicDeadlineBonus(letter, tokenCount);

                    if (deadlineBonus > 0)
                    {
                        letter.DeadlineInMinutes += deadlineBonus;
                        _messageSystem.AddSystemMessage(
                            $"📅 {obligation.Name} grants +{deadlineBonus} days deadline (scaled by {tokenCount} tokens)",
                            SystemMessageTypes.Info
                        );
                    }
                }
            }
        }
    }
}