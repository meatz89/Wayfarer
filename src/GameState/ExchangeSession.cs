/// <summary>
/// Represents an active exchange session between the player and an NPC.
/// Tracks the state of negotiation and available exchanges.
/// No hidden state - all mechanics are explicit and visible.
/// ADR-007: Object references only (no ID properties except tracking IDs)
/// </summary>
public class ExchangeSession
{
    // ADR-007: SessionId DELETED - sessions tracked by GameWorld reference, not string ID

    // ADR-007: NpcId DELETED - use NPC object reference
    public NPC Npc { get; set; }

    // ADR-007: LocationId DELETED - use Location object reference
    public Location Location { get; set; }

    /// <summary>
    /// The time block when this session started.
    /// </summary>
    public TimeBlocks StartTimeBlock { get; set; }

    /// <summary>
    /// Available exchange options for this session.
    /// These are the exchanges the NPC is offering.
    /// </summary>
    public List<ExchangeOption> AvailableExchanges { get; set; } = new List<ExchangeOption>();

    // ADR-007: SelectedExchangeIds DELETED - use List<ExchangeOption> objects
    public List<ExchangeOption> SelectedExchanges { get; set; } = new List<ExchangeOption>();

    // ADR-007: ActiveExchangeId DELETED - use ExchangeOption object reference
    public ExchangeOption ActiveExchange { get; set; }

    /// <summary>
    /// History of completed exchanges in this session.
    /// Tracks what has been traded for audit trail.
    /// </summary>
    public List<CompletedExchange> CompletedExchanges { get; set; } = new List<CompletedExchange>();

    /// <summary>
    /// Current phase of the exchange session.
    /// </summary>
    public ExchangePhase CurrentPhase { get; set; } = ExchangePhase.Browsing;

    /// <summary>
    /// Whether this session is still active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the session started.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the session ended (if ended).
    /// </summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// Token state at the start of the session.
    /// Used to track token changes during the session.
    /// </summary>
    public Dictionary<ConnectionType, int> InitialTokenState { get; set; } = new Dictionary<ConnectionType, int>();

    /// <summary>
    /// Resource state at the start of the session.
    /// Used to track resource changes during the session.
    /// </summary>
    public SessionResourceSnapshot InitialResources { get; set; }

    /// <summary>
    /// Gets the currently selected exchange if exactly one is selected.
    /// ADR-007: Direct object access (no ID lookup)
    /// </summary>
    public ExchangeOption GetSelectedExchange()
    {
        if (SelectedExchanges.Count != 1)
            return null;

        return SelectedExchanges[0];
    }

    /// <summary>
    /// Gets the active exchange being executed.
    /// ADR-007: Direct object access (no ID lookup)
    /// </summary>
    public ExchangeOption GetActiveExchange()
    {
        return ActiveExchange;
    }

    /// <summary>
    /// Completes an exchange and records it in history.
    /// ADR-007: Accept ExchangeOption object (not ID)
    /// </summary>
    public void CompleteExchange(ExchangeOption exchange, bool success, ExchangeCostStructure actualCost, ExchangeRewardStructure actualReward)
    {
        CompletedExchange completed = new CompletedExchange
        {
            ExchangeId = exchange.ExchangeId, // Audit trail preserves ID for history
            ExchangeName = exchange.Name,
            Success = success,
            Cost = actualCost,
            Reward = success ? actualReward : null,
            CompletedAt = DateTime.UtcNow
        };

        CompletedExchanges.Add(completed);

        // ADR-007: Clear active exchange (null object reference, not empty string ID)
        ActiveExchange = null;
        CurrentPhase = ExchangePhase.Browsing;
    }

    /// <summary>
    /// Ends the session and marks it as inactive.
    /// </summary>
    public void EndSession()
    {
        IsActive = false;
        EndedAt = DateTime.UtcNow;
        CurrentPhase = ExchangePhase.Completed;
    }

    /// <summary>
    /// Gets a summary of all exchanges completed in this session.
    /// </summary>
    public string GetSessionSummary()
    {
        if (CompletedExchanges.Count == 0)
            return "No exchanges completed";

        List<string> parts = new List<string>();
        foreach (CompletedExchange exchange in CompletedExchanges)
        {
            string status = exchange.Success ? "✓" : "✗";
            parts.Add($"{status} {exchange.ExchangeName}");
        }

        return string.Join(", ", parts);
    }
}

/// <summary>
/// Phases of an exchange session.
/// </summary>
public enum ExchangePhase
{
    /// <summary>
    /// Player is browsing available exchanges.
    /// </summary>
    Browsing,

    /// <summary>
    /// Player has selected an exchange to consider.
    /// </summary>
    Considering,

    /// <summary>
    /// Exchange is being executed.
    /// </summary>
    Executing,

    /// <summary>
    /// Session has been completed.
    /// </summary>
    Completed
}

/// <summary>
/// Record of a completed exchange within a session.
/// </summary>
public class CompletedExchange
{
    public string ExchangeId { get; set; }
    public string ExchangeName { get; set; }
    public bool Success { get; set; }
    public ExchangeCostStructure Cost { get; set; }
    public ExchangeRewardStructure Reward { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Snapshot of resources at a point in time.
/// </summary>
public class SessionResourceSnapshot
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Stamina { get; set; }
    public int Attention { get; set; }
    public Dictionary<string, int> Items { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Creates a snapshot from current player state.
    /// </summary>
    public static SessionResourceSnapshot FromPlayerState(PlayerResourceState state, int attention)
    {
        return new SessionResourceSnapshot
        {
            Coins = state.Coins,
            Health = state.Health,
            Stamina = state.Stamina,
            Attention = attention
        };
    }
}