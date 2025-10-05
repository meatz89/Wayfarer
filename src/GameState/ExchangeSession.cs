using System;
using System.Collections.Generic;

/// <summary>
/// Represents an active exchange session between the player and an NPC.
/// Tracks the state of negotiation and available exchanges.
/// No hidden state - all mechanics are explicit and visible.
/// </summary>
public class ExchangeSession
{
    /// <summary>
    /// Unique identifier for this session.
    /// </summary>
    public string SessionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The NPC involved in this exchange session.
    /// Empty string for location-based or system exchanges.
    /// </summary>
    public string NpcId { get; set; }

    /// <summary>
    /// The location where this exchange is taking place.
    /// </summary>
    public string LocationId { get; set; }

    /// <summary>
    /// The time block when this session started.
    /// </summary>
    public TimeBlocks StartTimeBlock { get; set; }

    /// <summary>
    /// Available exchange options for this session.
    /// These are the exchanges the NPC is offering.
    /// </summary>
    public List<ExchangeOption> AvailableExchanges { get; set; } = new List<ExchangeOption>();

    /// <summary>
    /// Exchange cards the player has selected to consider.
    /// Player can select multiple exchanges to compare.
    /// </summary>
    public List<string> SelectedExchangeIds { get; set; } = new List<string>();

    /// <summary>
    /// The exchange currently being executed, if any.
    /// Null when no exchange is in progress.
    /// </summary>
    public string ActiveExchangeId { get; set; }

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
    /// Alias for StartedAt for backward compatibility.
    /// </summary>
    public DateTime StartTime { get => StartedAt; set => StartedAt = value; }

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
    /// </summary>
    public ExchangeOption GetSelectedExchange()
    {
        if (SelectedExchangeIds.Count != 1)
            return null;

        return AvailableExchanges.Find(e => e.ExchangeId == SelectedExchangeIds[0]);
    }

    /// <summary>
    /// Gets the active exchange being executed.
    /// </summary>
    public ExchangeOption GetActiveExchange()
    {
        if (string.IsNullOrEmpty(ActiveExchangeId))
            return null;

        return AvailableExchanges.Find(e => e.ExchangeId == ActiveExchangeId);
    }

    /// <summary>
    /// Completes an exchange and records it in history.
    /// </summary>
    public void CompleteExchange(string exchangeId, bool success, ExchangeCostStructure actualCost, ExchangeRewardStructure actualReward)
    {
        ExchangeOption? exchange = AvailableExchanges.Find(e => e.ExchangeId == exchangeId);
        if (exchange == null)
            return;

        CompletedExchange completed = new CompletedExchange
        {
            ExchangeId = exchangeId,
            ExchangeName = exchange.Name,
            Success = success,
            Cost = actualCost,
            Reward = success ? actualReward : null,
            CompletedAt = DateTime.UtcNow
        };

        CompletedExchanges.Add(completed);

        // Mark single-use exchanges as completed in the underlying ExchangeData
        if (exchange.ExchangeCard != null)
        {
            // TODO: Track completion in ExchangeData if needed
        }

        // Clear active exchange
        ActiveExchangeId = null;
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