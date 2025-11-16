/// <summary>
/// Tracks binding obligations - promises and debts that create narrative pressure
/// These appear as persistent reminders of what the player owes to NPCs
/// </summary>
public class BindingObligationSystem
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly TimeManager _timeManager;
    private readonly List<BindingObligation> _activeObligations;

    public BindingObligationSystem(
        GameWorld gameWorld,
        TokenMechanicsManager tokenManager,
        TimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _timeManager = timeManager;
        _activeObligations = new List<BindingObligation>();
    }

    /// <summary>
    /// Create a new binding obligation from a conversation choice
    /// </summary>
    public void CreateObligation(
        string npcId,
        string npcName,
        ObligationType type,
        string description,
        int segmentsUntilDue = 24)
    {
        BindingObligation obligation = new BindingObligation
        {
            Id = Guid.NewGuid().ToString(),
            NpcId = npcId,
            NpcName = npcName,
            Type = type,
            Description = description,
            CreatedAtSegment = _timeManager.CurrentSegment,
            DueBySegment = _timeManager.CurrentSegment + segmentsUntilDue,
            IsActive = true
        };

        _activeObligations.Add(obligation);
    }

    /// <summary>
    /// Check if player has specific type of obligation to an NPC
    /// </summary>
    public bool HasObligationTo(string npcId, ObligationType type)
    {
        return _activeObligations.Any(o =>
            o.IsActive &&
            o.NpcId == npcId &&
            o.Type == type);
    }

    /// <summary>
    /// Fulfill an obligation
    /// </summary>
    public void FulfillObligation(string obligationId)
    {
        BindingObligation? obligation = _activeObligations.FirstOrDefault(o => o.Id == obligationId);
        if (obligation != null)
        {
            obligation.IsActive = false;
            obligation.FulfilledAtSegment = _timeManager.CurrentSegment;
        }
    }

    /// <summary>
    /// Check for broken obligations and apply consequences
    /// </summary>
    public void ProcessBrokenObligations()
    {
        int currentSegment = _timeManager.CurrentSegment;

        foreach (BindingObligation? obligation in _activeObligations.Where(o =>
            o.IsActive && o.DueBySegment < currentSegment))
        {
            // Apply token penalties for broken promises
            ApplyBrokenObligationPenalty(obligation);

            // Mark as broken
            obligation.IsActive = false;
            obligation.WasBroken = true;
        }
    }

    private void ApplyBrokenObligationPenalty(BindingObligation obligation)
    {
        // Different penalties based on obligation type
        int penalty = obligation.Type switch
        {
            ObligationType.Promise => -2,      // Breaking promises hurts trust
            ObligationType.Debt => -1,         // Unpaid debts hurt diplomacy
            ObligationType.Favor => -1,        // Unfulfilled favors hurt status
            ObligationType.Secret => -3,       // Betraying secrets is severe
            _ => -1
        };

        // Apply the penalty
        ConnectionType tokenType = obligation.Type switch
        {
            ObligationType.Promise => ConnectionType.Trust,
            ObligationType.Debt => ConnectionType.Diplomacy,
            ObligationType.Favor => ConnectionType.Status,
            ObligationType.Secret => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };

        _tokenManager.AddTokensToNPC(tokenType, penalty, obligation.NpcId);
    }
}

/// <summary>
/// Represents a binding obligation to an NPC
/// </summary>
public class BindingObligation
{
    public string Id { get; set; }
    public string NpcId { get; set; }
    public string NpcName { get; set; }
    public ObligationType Type { get; set; }
    public string Description { get; set; }
    public int CreatedAtSegment { get; set; }
    public int DueBySegment { get; set; }
    public int? FulfilledAtSegment { get; set; }
    public bool IsActive { get; set; }
    public bool WasBroken { get; set; }
}

/// <summary>
/// Types of binding obligations
/// </summary>
public enum ObligationType
{
    Promise,    // You promised to do something
    Debt,       // You owe money or goods
    Favor,      // You owe a favor
    Secret      // You're keeping a secret
}

/// <summary>
/// Urgency levels for obligations
/// </summary>
public enum ObligationUrgency
{
    Critical = 0,  // <= 2 segments
    Urgent = 1,    // <= 4 segments
    Soon = 2,      // <= 8 segments
    Later = 3      // > 8 segments
}