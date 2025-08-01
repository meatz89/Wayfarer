using System;
using System.Linq;
public class StandingObligationManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly ConnectionTokenManager _connectionTokenManager;
    private readonly StandingObligationRepository _obligationRepository;
    private readonly ITimeManager _timeManager;

    public StandingObligationManager(GameWorld gameWorld, MessageSystem messageSystem, LetterTemplateRepository letterTemplateRepository, ConnectionTokenManager connectionTokenManager, StandingObligationRepository obligationRepository, ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _letterTemplateRepository = letterTemplateRepository;
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
    public List<Letter> ProcessDailyObligations(int currentDay)
    {
        List<Letter> forcedLetters = new List<Letter>();
        List<StandingObligation> obligationsNeedingLetters = GetObligationsRequiringForcedLetters();

        foreach (StandingObligation obligation in obligationsNeedingLetters)
        {
            Letter letter = GenerateForcedLetter(obligation);
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

    // Generate a forced letter for a specific obligation
    public Letter GenerateForcedLetter(StandingObligation obligation)
    {
        if (obligation.HasEffect(ObligationEffect.ShadowForced))
        {
            return GenerateShadowForcedLetter();
        }

        if (obligation.HasEffect(ObligationEffect.PatronMonthly))
        {
            return GeneratePatronMonthlyLetter();
        }

        return null;
    }

    // Generate shadow obligation forced letter using templates
    private Letter GenerateShadowForcedLetter()
    {
        LetterTemplate template = _letterTemplateRepository.GetRandomForcedShadowTemplate();
        if (template != null)
        {
            return _letterTemplateRepository.GenerateForcedLetterFromTemplate(template);
        }

        // Fallback to hardcoded generation if no templates available
        string[] shadowSenders = new[] { "The Fence", "Midnight Contact", "Shadow Broker", "Anonymous Source" };
        string[] shadowRecipients = new[] { "Dead Drop", "Safe House", "Underground Contact", "Hidden Ally" };
        Random random = new Random();

        return new Letter
        {
            SenderName = shadowSenders[random.Next(shadowSenders.Length)],
            RecipientName = shadowRecipients[random.Next(shadowRecipients.Length)],
            TokenType = ConnectionType.Shadow,
            Payment = random.Next(20, 40), // High base payment for dangerous work
            Deadline = random.Next(1, 4), // Urgent deadlines
            IsGenerated = true,
            GenerationReason = "Shadow Obligation Forced (Fallback)"
        };
    }

    // Generate patron monthly resource letter using templates
    private Letter GeneratePatronMonthlyLetter()
    {
        LetterTemplate template = _letterTemplateRepository.GetRandomForcedPatronTemplate();
        if (template != null)
        {
            return _letterTemplateRepository.GenerateForcedLetterFromTemplate(template);
        }

        // Fallback to hardcoded generation if no templates available
        Random random = new Random();

        return new Letter
        {
            SenderName = "Your Patron",
            RecipientName = "Resources Contact",
            TokenType = ConnectionType.Status, // Patron letters usually noble
            Payment = random.Next(50, 100), // Large resource package
            Deadline = random.Next(3, 7), // Reasonable deadline
            IsGenerated = true,
            GenerationReason = "Patron Monthly Package (Fallback)"
        };
    }

    // Record that forced letters were generated for obligations
    public void RecordForcedLettersGenerated(List<StandingObligation> obligations)
    {
        foreach (StandingObligation obligation in obligations)
        {
            obligation.RecordForcedLetterGenerated();
        }
    }

    // Advance time for all obligations (called daily)
    public void AdvanceDailyTime()
    {
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            obligation.DaysSinceLastForcedLetter++;
        }
    }

    // Calculate total coin bonus for a letter delivery
    public int CalculateTotalCoinBonus(Letter letter)
    {
        int totalBonus = 0;
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            // Static bonuses
            totalBonus += obligation.CalculateCoinBonus(letter, letter.Payment);
            
            // Dynamic payment bonuses that scale with tokens
            if (obligation.HasEffect(ObligationEffect.DynamicPaymentBonus))
            {
                string senderId = GetNPCIdByName(letter.SenderName);
                if (!string.IsNullOrEmpty(senderId))
                {
                    int tokenCount = GetRelevantTokenCountForLetter(obligation, letter, senderId);
                    totalBonus += obligation.CalculateDynamicPaymentBonus(letter, letter.Payment, tokenCount);
                }
            }
        }

        return totalBonus;
    }

    // Calculate the best entry position for a new letter
    public int CalculateBestEntryPosition(Letter letter, int basePosition)
    {
        int bestPosition = basePosition;
        List<StandingObligation> activeObligations = GetActiveObligations();

        foreach (StandingObligation obligation in activeObligations)
        {
            int obligationPosition = obligation.CalculateEntryPosition(letter, bestPosition);
            bestPosition = Math.Min(bestPosition, obligationPosition);
        }

        return bestPosition;
    }

    // Apply leverage modifiers from standing obligations
    public int ApplyLeverageModifiers(Letter letter, int currentPosition)
    {
        List<StandingObligation> activeObligations = GetActiveObligations();
        int modifiedPosition = currentPosition;

        foreach (StandingObligation obligation in activeObligations)
        {
            modifiedPosition = ApplySingleObligationLeverage(obligation, letter, modifiedPosition);
        }

        return modifiedPosition;
    }

    // Apply leverage modifier from a single obligation
    private int ApplySingleObligationLeverage(StandingObligation obligation, Letter letter, int currentPosition)
    {
        if (obligation.HasEffect(ObligationEffect.ShadowEqualsNoble))
        {
            return ApplyShadowNobleEffect(letter, currentPosition);
        }

        if (obligation.HasEffect(ObligationEffect.CommonRevenge))
        {
            return ApplyCommonRevengeEffect(obligation, letter, currentPosition);
        }

        if (obligation.HasEffect(ObligationEffect.DebtSpiral))
        {
            return ApplyDebtSpiralEffect(letter, currentPosition);
        }

        if (obligation.HasEffect(ObligationEffect.MerchantRespect))
        {
            return ApplyMerchantRespectEffect(letter, currentPosition);
        }

        if (obligation.HasEffect(ObligationEffect.DynamicLeverageModifier))
        {
            return ApplyDynamicLeverageEffect(obligation, letter, currentPosition);
        }

        return currentPosition;
    }

    // Apply shadow equals noble leverage effect
    private int ApplyShadowNobleEffect(Letter letter, int currentPosition)
    {
        if (letter.TokenType == ConnectionType.Shadow)
        {
            return Math.Min(currentPosition, 3); // Noble base position
        }
        return currentPosition;
    }

    // Apply common revenge effect - common letters from debt get noble priority
    private int ApplyCommonRevengeEffect(StandingObligation obligation, Letter letter, int currentPosition)
    {
        if (letter.TokenType != ConnectionType.Common)
            return currentPosition;

        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId))
            return currentPosition;

        int tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[ConnectionType.Common];
        if (tokenBalance < 0)
        {
            return 3; // Noble position for debt leverage
        }

        return currentPosition;
    }

    // Apply debt spiral effect - all negative positions get extra leverage
    private int ApplyDebtSpiralEffect(Letter letter, int currentPosition)
    {
        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId))
            return currentPosition;

        int tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[letter.TokenType];
        if (tokenBalance < 0)
        {
            return currentPosition - 1; // Additional leverage from debt
        }

        return currentPosition;
    }

    // Apply merchant respect effect - trade letters with 5+ tokens get less leverage
    private int ApplyMerchantRespectEffect(Letter letter, int currentPosition)
    {
        if (letter.TokenType != ConnectionType.Commerce)
            return currentPosition;

        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId))
            return currentPosition;

        int tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[ConnectionType.Commerce];
        if (tokenBalance >= 5)
        {
            return currentPosition + 1; // Less leverage due to respect
        }

        return currentPosition;
    }

    // Apply dynamic leverage effect - scales with token count
    private int ApplyDynamicLeverageEffect(StandingObligation obligation, Letter letter, int currentPosition)
    {
        string senderId = GetNPCIdByName(letter.SenderName);
        if (string.IsNullOrEmpty(senderId))
            return currentPosition;

        int tokenCount = GetRelevantTokenCountForLetter(obligation, letter, senderId);
        return obligation.CalculateDynamicLeverage(letter, currentPosition, tokenCount);
    }

    // Helper to get NPC ID from name
    private string GetNPCIdByName(string npcName)
    {
        // This would typically use NPCRepository, but we don't have it injected here
        // For now, return empty string - the calling code should handle this
        return "";
    }

    // Check if any obligation provides free deadline extension
    public bool HasFreeDeadlineExtension(Letter letter)
    {
        return GetActiveObligations()
            .Any(o => o.IsFreeDeadlineExtension(letter));
    }

    // Calculate total skip cost multiplier
    public int CalculateSkipCostMultiplier(Letter letter)
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
    public bool IsActionForbidden(string actionType, Letter letter, out string reason)
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


    // Get obligations affecting a specific token type
    public List<StandingObligation> GetObligationsForTokenType(ConnectionType tokenType)
    {
        return GetActiveObligations()
            .Where(o => o.AppliesTo(tokenType))
            .ToList();
    }

    // Get summary of all active obligation effects
    public string GetObligationsSummary()
    {
        List<StandingObligation> activeObligations = GetActiveObligations();
        if (!activeObligations.Any())
        {
            return "No active standing obligations";
        }

        List<string> lines = new List<string>();
        foreach (StandingObligation obligation in activeObligations)
        {
            lines.Add($"• {obligation.Name} (Day {obligation.DaysSinceAccepted})");
            lines.Add($"  {obligation.GetEffectsSummary()}");
        }

        return string.Join("\n", lines);
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
    private int GetRelevantTokenCountForLetter(StandingObligation obligation, Letter letter, string senderId)
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
            DaysSinceLastForcedLetter = 0
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

    // This should be called whenever tokens change
    public void OnTokensChanged(string npcId, ConnectionType tokenType, int oldCount, int newCount)
    {
        // Only check if the token count actually changed
        if (oldCount != newCount)
        {
            CheckThresholdActivations();
        }
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
        // Apply relationship damage or other consequences for breaking obligations
        // Remove tokens based on obligation type
        if (obligation.RelatedTokenType.HasValue)
        {
            int penaltyTokens = Math.Min(5, _connectionTokenManager.GetTokenCount(obligation.RelatedTokenType.Value));
            if (penaltyTokens > 0)
            {
                _connectionTokenManager.SpendTokens(obligation.RelatedTokenType.Value, penaltyTokens);

                _messageSystem.AddSystemMessage(
                    $"Lost {penaltyTokens} {obligation.RelatedTokenType} tokens for breaking {obligation.Name}",
                    SystemMessageTypes.Danger
                );
            }
        }
    }

    // Apply dynamic deadline bonuses from obligations
    public void ApplyDynamicDeadlineBonuses(Letter letter)
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
                        letter.Deadline += deadlineBonus;
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