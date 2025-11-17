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
    /// </summary>
    public void UpdateRelationshipState(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return;

        // Update last interaction time on NPC entity
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            npc.LastInteractionTime = DateTime.Now;
        }

        // Check if NPC has debt (negative tokens)
        List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npcId);
        bool hasDebt = tokens.Any(t => t.Count < 0);

        // Update NPC state if needed
        UpdateNPCDisposition(npcId, hasDebt);
    }

    /// <summary>
    /// Check and announce relationship milestones
    /// </summary>
    public void CheckRelationshipMilestone(string npcId, int totalTokens)
    {
        NPC npc = _npcRepository.GetById(npcId);
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
    /// </summary>
    public void RecordDebt(string npcId, ConnectionType type, int amount)
    {
        if (amount <= 0) return;

        // Debts are tracked as negative tokens - no separate storage needed
        NPC npc = _npcRepository.GetById(npcId);
        if (npc != null)
        {
            _messageSystem.AddSystemMessage(
                $"You now owe {npc.Name} {amount} {type} favor{(amount > 1 ? "s" : "")}.",
                SystemMessageTypes.Warning
            );
        }
    }

    /// <summary>
    /// Get the primary connection type with an NPC (highest positive token count)
    /// </summary>
    public ConnectionType GetPrimaryConnection(string npcId)
    {
        List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npcId);

        TokenCount highest = tokens
            .Where(t => t.Type != ConnectionType.None && t.Count > 0)
            .OrderByDescending(t => t.Count)
            .FirstOrDefault();

        return highest?.Type ?? ConnectionType.None;
    }

    /// <summary>
    /// Get relationship tier based on total tokens
    /// </summary>
    public RelationshipTier GetRelationshipTier(string npcId)
    {
        int totalTokens = GetTotalPositiveTokens(npcId);

        if (totalTokens >= 13) return RelationshipTier.InnerCircle;
        if (totalTokens >= 9) return RelationshipTier.Confidant;
        if (totalTokens >= 6) return RelationshipTier.CloseFriend;
        if (totalTokens >= 3) return RelationshipTier.Friend;
        if (totalTokens >= 1) return RelationshipTier.Acquaintance;
        return RelationshipTier.None;
    }

    /// <summary>
    /// Get all NPCs the player owes tokens to
    /// </summary>
    public List<DebtInfo> GetAllDebts()
    {
        List<DebtInfo> debts = new List<DebtInfo>();

        foreach (string npcId in _tokenManager.GetNPCsWithTokens())
        {
            List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npcId);
            List<TokenCount> negativeTokens = tokens
                .Where(t => t.Count < 0)
                .Select(t => new TokenCount { Type = t.Type, Count = Math.Abs(t.Count) })
                .ToList();

            if (negativeTokens.Count == 0) continue;

            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) continue;

            DebtInfo debtInfo = new DebtInfo
            {
                NPCId = npcId,
                NPCName = npc.Name,
                Debts = negativeTokens,
                TotalDebt = negativeTokens.Sum(t => t.Count)
            };

            debts.Add(debtInfo);
        }

        return debts.OrderByDescending(d => d.TotalDebt).ToList();
    }

    /// <summary>
    /// Check if player has any debts
    /// </summary>
    public bool HasAnyDebt()
    {
        foreach (string npcId in _tokenManager.GetNPCsWithTokens())
        {
            List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npcId);
            if (tokens.Any(t => t.Count < 0))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get relationship summary for an NPC
    /// </summary>
    public RelationshipSummary GetRelationshipSummary(string npcId)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return null;

        List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npcId);
        bool hasDebt = tokens.Any(t => t.Count < 0);
        int totalDebt = tokens.Where(t => t.Count < 0).Sum(t => Math.Abs(t.Count));

        return new RelationshipSummary
        {
            NPCId = npcId,
            NPCName = npc.Name,
            Tokens = tokens,
            PrimaryConnection = GetPrimaryConnection(npcId),
            RelationshipTier = GetRelationshipTier(npcId),
            TotalPositiveTokens = GetTotalPositiveTokens(npcId),
            HasDebt = hasDebt,
            TotalDebt = totalDebt,
            LastInteraction = npc.LastInteractionTime
        };
    }

    /// <summary>
    /// Process relationship decay over time
    /// </summary>
    public void ProcessRelationshipDecay()
    {
        DateTime now = DateTime.Now;

        foreach (string npcId in _tokenManager.GetNPCsWithTokens())
        {
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) continue;

            DateTime lastInteraction = npc.LastInteractionTime;
            if (lastInteraction == DateTime.MinValue) continue;

            int daysSinceInteraction = (int)(now - lastInteraction).TotalDays;

            // Only decay after a week of no interaction
            if (daysSinceInteraction < 7) continue;

            List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npcId);
            bool hadDecay = false;

            foreach (TokenCount tokenCount in tokens)
            {
                if (tokenCount.Type == ConnectionType.None) continue;
                if (tokenCount.Count <= 0) continue;

                // Calculate decay based on time and token type
                int decay = CalculateDecay(tokenCount.Type, tokenCount.Count, daysSinceInteraction);
                if (decay > 0)
                {
                    _tokenManager.RemoveTokensFromNPC(tokenCount.Type, decay, npc);
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

    private void UpdateNPCDisposition(string npcId, bool hasDebt)
    {
        // This would update NPC's disposition/attitude towards player
        // For now, we just track the state

        if (hasDebt)
        {
            // NPC is less friendly when owed favors
            // This could affect conversation options, prices, etc.
        }
    }

    private int GetTotalPositiveTokens(string npcId)
    {
        List<TokenCount> tokens = _tokenManager.GetTokensWithNPC(npcId);
        return tokens.Where(t => t.Count > 0).Sum(t => t.Count);
    }

    private int CalculateDecay(ConnectionType type, int currentTokens, int daysSinceInteraction)
    {
        // Different token types decay at different rates
        float decayRate = type switch
        {
            ConnectionType.Trust => 0.02f,    // Trust decays slowly
            ConnectionType.Diplomacy => 0.01f, // Diplomacy is most stable
            ConnectionType.Status => 0.04f,   // Status decays faster
            ConnectionType.Shadow => 0.05f,   // Shadow decays fastest
            _ => 0.02f
        };

        // Decay accelerates with time
        int weeksWithoutContact = daysSinceInteraction / 7;
        float decayMultiplier = 1.0f + (weeksWithoutContact * 0.1f);

        int decay = (int)Math.Floor(currentTokens * decayRate * decayMultiplier);

        // Never decay more than 1 token per week for low token counts
        if (currentTokens <= 3)
        {
            decay = Math.Min(decay, weeksWithoutContact);
        }

        return decay;
    }
}

// ========== SUPPORTING TYPES ==========

public class RelationshipSummary
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
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
