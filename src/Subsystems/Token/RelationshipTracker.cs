/// <summary>
/// Tracks relationship states, progression, debts, and milestones with NPCs.
/// </summary>
public class RelationshipTracker
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;

    // Relationship milestones
    private readonly List<MilestoneMessage> _relationshipMilestones = new List<MilestoneMessage>
    {
        new MilestoneMessage { TokenThreshold = 3, Message = "now trusts you enough to share private correspondence" },
        new MilestoneMessage { TokenThreshold = 5, Message = "bond has deepened. They'll offer more valuable letters" },
        new MilestoneMessage { TokenThreshold = 8, Message = "considers you among their most trusted associates" },
        new MilestoneMessage { TokenThreshold = 12, Message = "few people enjoy this level of trust" },
        new MilestoneMessage { TokenThreshold = 15, Message = "would trust you with their life" },
        new MilestoneMessage { TokenThreshold = 20, Message = "shares an unbreakable bond with you" }
    };

    public RelationshipTracker(
        GameWorld gameWorld,
        ConnectionTokenManager tokenManager,
        NPCRepository npcRepository,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Update relationship state after token changes
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public void UpdateRelationshipState(NPC npc)
    {
        if (npc == null) return;

        // Update last interaction time on NPC entity
        npc.LastInteractionTime = DateTime.Now;

        // Check if NPC has debt (negative tokens)
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);
        bool hasDebt = tokens.Values.Any(count => count < 0);

        // Update NPC state if needed
        UpdateNPCDisposition(npc, hasDebt);
    }

    /// <summary>
    /// Check and announce relationship milestones
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public void CheckRelationshipMilestone(NPC npc, int totalTokens)
    {
        if (npc == null) return;

        MilestoneMessage milestone = _relationshipMilestones
            .FirstOrDefault(m => m.TokenThreshold == totalTokens);

        if (milestone != null)
        {
            _messageSystem.AddSystemMessage(
                $"{npc.Name} {milestone.Message}.",
                SystemMessageTypes.Success
            );

            // Special announcements for major milestones
            if (milestone.TokenThreshold >= 10)
            {
                _messageSystem.AddSystemMessage(
                    $"Your relationship with {npc.Name} has reached a rare level of trust.",
                    SystemMessageTypes.Success
                );
            }
        }
    }

    /// <summary>
    /// Record a debt to an NPC
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public void RecordDebt(NPC npc, ConnectionType type, int amount)
    {
        if (amount <= 0 || npc == null) return;

        // Debts are tracked as negative tokens - no separate storage needed
        _messageSystem.AddSystemMessage(
            $"You now owe {npc.Name} {amount} {type} favor{(amount > 1 ? "s" : "")}.",
            SystemMessageTypes.Warning
        );
    }

    /// <summary>
    /// Get the primary connection type with an NPC (highest positive token count)
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public ConnectionType GetPrimaryConnection(NPC npc)
    {
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);

        KeyValuePair<ConnectionType, int> highest = tokens
            .Where(kvp => kvp.Key != ConnectionType.None && kvp.Value > 0)
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault();

        return highest.Key != ConnectionType.None ? highest.Key : ConnectionType.None;
    }

    /// <summary>
    /// Get relationship tier based on total tokens
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public RelationshipTier GetRelationshipTier(NPC npc)
    {
        int totalTokens = GetTotalPositiveTokens(npc);

        if (totalTokens >= 13) return RelationshipTier.InnerCircle;
        if (totalTokens >= 9) return RelationshipTier.Confidant;
        if (totalTokens >= 6) return RelationshipTier.CloseFriend;
        if (totalTokens >= 3) return RelationshipTier.Friend;
        if (totalTokens >= 1) return RelationshipTier.Acquaintance;
        return RelationshipTier.None;
    }

    /// <summary>
    /// Get all NPCs the player owes tokens to
    /// HIGHLANDER: GetNPCsWithTokens now returns List<NPC>
    /// </summary>
    public List<DebtInfo> GetAllDebts()
    {
        List<DebtInfo> debts = new List<DebtInfo>();

        foreach (NPC npc in _tokenManager.GetNPCsWithTokens())
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);
            List<TokenCount> negativeTokens = tokens
                .Where(kvp => kvp.Value < 0)
                .Select(kvp => new TokenCount { Type = kvp.Key, Count = Math.Abs(kvp.Value) })
                .ToList();

            if (negativeTokens.Count == 0) continue;

            DebtInfo debtInfo = new DebtInfo
            {
                Npc = npc,
                Debts = negativeTokens,
                TotalDebt = negativeTokens.Sum(t => t.Count)
            };

            debts.Add(debtInfo);
        }

        return debts.OrderByDescending(d => d.TotalDebt).ToList();
    }

    /// <summary>
    /// Check if player has any debts
    /// HIGHLANDER: GetNPCsWithTokens now returns List<NPC>
    /// </summary>
    public bool HasAnyDebt()
    {
        foreach (NPC npc in _tokenManager.GetNPCsWithTokens())
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);
            if (tokens.Values.Any(count => count < 0))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get relationship summary for an NPC
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    public RelationshipSummary GetRelationshipSummary(NPC npc)
    {
        if (npc == null) return null;

        Dictionary<ConnectionType, int> tokenDict = _tokenManager.GetTokensWithNPC(npc);
        List<TokenCount> tokens = tokenDict
            .Select(kvp => new TokenCount { Type = kvp.Key, Count = kvp.Value })
            .ToList();
        bool hasDebt = tokenDict.Values.Any(count => count < 0);
        int totalDebt = tokenDict.Values.Where(count => count < 0).Sum(count => Math.Abs(count));

        return new RelationshipSummary
        {
            Npc = npc,
            Tokens = tokens,
            PrimaryConnection = GetPrimaryConnection(npc),
            RelationshipTier = GetRelationshipTier(npc),
            TotalPositiveTokens = GetTotalPositiveTokens(npc),
            HasDebt = hasDebt,
            TotalDebt = totalDebt,
            LastInteraction = npc.LastInteractionTime
        };
    }

    /// <summary>
    /// Process relationship decay over time
    /// HIGHLANDER: GetNPCsWithTokens now returns List<NPC>
    /// </summary>
    public void ProcessRelationshipDecay()
    {
        DateTime now = DateTime.Now;

        foreach (NPC npc in _tokenManager.GetNPCsWithTokens())
        {
            DateTime lastInteraction = npc.LastInteractionTime;
            if (lastInteraction == DateTime.MinValue) continue;

            int daysSinceInteraction = (int)(now - lastInteraction).TotalDays;

            // Only decay after a week of no interaction
            if (daysSinceInteraction < 7) continue;

            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);
            bool hadDecay = false;

            foreach (KeyValuePair<ConnectionType, int> kvp in tokens)
            {
                if (kvp.Key == ConnectionType.None) continue;
                if (kvp.Value <= 0) continue;

                // Calculate decay based on time and token type
                int decay = CalculateDecay(kvp.Key, kvp.Value, daysSinceInteraction);
                if (decay > 0)
                {
                    _tokenManager.RemoveTokensFromNPC(kvp.Key, decay, npc);
                    hadDecay = true;
                }
            }

            if (hadDecay)
            {
                _messageSystem.AddSystemMessage(
                    $"Your relationship with {npc.Name} has weakened due to lack of contact.",
                    SystemMessageTypes.Warning
                );
            }
        }
    }

    // ========== PRIVATE HELPER METHODS ==========

    /// <summary>
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    private void UpdateNPCDisposition(NPC npc, bool hasDebt)
    {
        // This would update NPC's disposition/attitude towards player
        // For now, we just track the state

        if (hasDebt)
        {
            // NPC is less friendly when owed favors
            // This could affect conversation options, prices, etc.
        }
    }

    /// <summary>
    /// HIGHLANDER: Accept typed NPC object
    /// </summary>
    private int GetTotalPositiveTokens(NPC npc)
    {
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc);
        return tokens.Values.Where(count => count > 0).Sum();
    }

    private int CalculateDecay(ConnectionType type, int currentTokens, int daysSinceInteraction)
    {
        // Different token types decay at different rates (basis points: 10000 = 100%)
        int decayRateBasisPoints = type switch
        {
            ConnectionType.Trust => 200,      // Trust decays slowly (2%)
            ConnectionType.Diplomacy => 100,  // Diplomacy is most stable (1%)
            ConnectionType.Status => 400,     // Status decays faster (4%)
            ConnectionType.Shadow => 500,     // Shadow decays fastest (5%)
            _ => 200
        };

        // Decay accelerates with time (basis points: 10000 = 1.0)
        int weeksWithoutContact = daysSinceInteraction / 7;
        int decayMultiplierBasisPoints = 10000 + (weeksWithoutContact * 1000);

        int decay = currentTokens * decayRateBasisPoints / 10000 * decayMultiplierBasisPoints / 10000;

        // Never decay more than 1 token per week for low token counts
        if (currentTokens <= 3)
        {
            decay = Math.Min(decay, weeksWithoutContact);
        }

        return decay;
    }
}

// ========== SUPPORTING TYPES ==========

/// <summary>
/// HIGHLANDER: Store NPC object reference, not string IDs
/// </summary>
public class RelationshipSummary
{
    public NPC Npc { get; set; }
    public List<TokenCount> Tokens { get; set; }
    public ConnectionType PrimaryConnection { get; set; }
    public RelationshipTier RelationshipTier { get; set; }
    public int TotalPositiveTokens { get; set; }
    public bool HasDebt { get; set; }
    public int TotalDebt { get; set; }
    public DateTime LastInteraction { get; set; }
    public List<string> AvailableUnlocks { get; set; } = new List<string>();
}

public class MilestoneMessage
{
    public int TokenThreshold { get; set; }
    public string Message { get; set; }
}
