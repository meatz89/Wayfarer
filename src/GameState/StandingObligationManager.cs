using System;
using System.Linq;
public class StandingObligationManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    private readonly ConnectionTokenManager _connectionTokenManager;

    public StandingObligationManager(GameWorld gameWorld, MessageSystem messageSystem, LetterTemplateRepository letterTemplateRepository, ConnectionTokenManager connectionTokenManager)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _letterTemplateRepository = letterTemplateRepository;
        _connectionTokenManager = connectionTokenManager;
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
            TokenType = ConnectionType.Noble, // Patron letters usually noble
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
            totalBonus += obligation.CalculateCoinBonus(letter, letter.Payment);
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
            // Shadow equals noble leverage
            if (obligation.HasEffect(ObligationEffect.ShadowEqualsNoble) && letter.TokenType == ConnectionType.Shadow)
            {
                modifiedPosition = Math.Min(modifiedPosition, 3); // Noble base position
            }

            // Common revenge - common letters from debt get noble priority
            if (obligation.HasEffect(ObligationEffect.CommonRevenge) && letter.TokenType == ConnectionType.Common)
            {
                string senderId = GetNPCIdByName(letter.SenderName);
                if (!string.IsNullOrEmpty(senderId))
                {
                    int tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[ConnectionType.Common];
                    if (tokenBalance < 0)
                    {
                        modifiedPosition = 3; // Noble position for debt leverage
                    }
                }
            }

            // Debt spiral - all negative positions get extra leverage
            if (obligation.HasEffect(ObligationEffect.DebtSpiral))
            {
                string senderId = GetNPCIdByName(letter.SenderName);
                if (!string.IsNullOrEmpty(senderId))
                {
                    int tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[letter.TokenType];
                    if (tokenBalance < 0)
                    {
                        modifiedPosition -= 1; // Additional leverage from debt
                    }
                }
            }

            // Merchant respect - trade letters with 5+ tokens get additional position down
            if (obligation.HasEffect(ObligationEffect.MerchantRespect) && letter.TokenType == ConnectionType.Trade)
            {
                string senderId = GetNPCIdByName(letter.SenderName);
                if (!string.IsNullOrEmpty(senderId))
                {
                    int tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[ConnectionType.Trade];
                    if (tokenBalance >= 5)
                    {
                        modifiedPosition += 1; // Less leverage due to respect
                    }
                }
            }
        }

        return modifiedPosition;
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

}