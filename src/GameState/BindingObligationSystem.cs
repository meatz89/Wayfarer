using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks binding obligations - promises and debts that create narrative pressure
/// These appear as persistent reminders of what the player owes to NPCs
/// </summary>
public class BindingObligationSystem
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly ITimeManager _timeManager;
    private readonly List<BindingObligation> _activeObligations;
    
    public BindingObligationSystem(
        GameWorld gameWorld,
        TokenMechanicsManager tokenManager,
        ITimeManager timeManager)
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
        int hoursUntilDue = 24)
    {
        var obligation = new BindingObligation
        {
            Id = Guid.NewGuid().ToString(),
            NpcId = npcId,
            NpcName = npcName,
            Type = type,
            Description = description,
            CreatedAtHour = _timeManager.GetCurrentTimeHours(),
            DueByHour = _timeManager.GetCurrentTimeHours() + hoursUntilDue,
            IsActive = true
        };
        
        _activeObligations.Add(obligation);
    }
    
    /// <summary>
    /// Get active obligations for display
    /// </summary>
    public List<BindingObligationViewModel> GetActiveObligations()
    {
        var currentHour = _timeManager.GetCurrentTimeHours();
        var viewModels = new List<BindingObligationViewModel>();
        
        foreach (var obligation in _activeObligations.Where(o => o.IsActive))
        {
            var hoursRemaining = obligation.DueByHour - currentHour;
            var urgency = hoursRemaining switch
            {
                <= 2 => ObligationUrgency.Critical,
                <= 6 => ObligationUrgency.Urgent,
                <= 12 => ObligationUrgency.Soon,
                _ => ObligationUrgency.Later
            };
            
            viewModels.Add(new BindingObligationViewModel
            {
                Icon = GetObligationIcon(obligation.Type, urgency),
                Text = FormatObligationText(obligation, hoursRemaining),
                Urgency = urgency,
                NpcName = obligation.NpcName
            });
        }
        
        // Sort by urgency
        return viewModels.OrderBy(o => o.Urgency).Take(3).ToList();
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
        var obligation = _activeObligations.FirstOrDefault(o => o.Id == obligationId);
        if (obligation != null)
        {
            obligation.IsActive = false;
            obligation.FulfilledAtHour = _timeManager.GetCurrentTimeHours();
        }
    }
    
    /// <summary>
    /// Check for broken obligations and apply consequences
    /// </summary>
    public void ProcessBrokenObligations()
    {
        var currentHour = _timeManager.GetCurrentTimeHours();
        
        foreach (var obligation in _activeObligations.Where(o => 
            o.IsActive && o.DueByHour < currentHour))
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
        var penalty = obligation.Type switch
        {
            ObligationType.Promise => -2,      // Breaking promises hurts trust
            ObligationType.Debt => -1,         // Unpaid debts hurt commerce
            ObligationType.Favor => -1,        // Unfulfilled favors hurt status
            ObligationType.Secret => -3,       // Betraying secrets is severe
            _ => -1
        };
        
        // Apply the penalty
        var tokenType = obligation.Type switch
        {
            ObligationType.Promise => ConnectionType.Trust,
            ObligationType.Debt => ConnectionType.Commerce,
            ObligationType.Favor => ConnectionType.Status,
            ObligationType.Secret => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };
        
        _tokenManager.AddTokensToNPC(tokenType, penalty, obligation.NpcId);
    }
    
    private string GetObligationIcon(ObligationType type, ObligationUrgency urgency)
    {
        if (urgency == ObligationUrgency.Critical)
            return "🔥"; // Critical obligations burn
            
        return type switch
        {
            ObligationType.Promise => "🤝",
            ObligationType.Debt => "💰",
            ObligationType.Favor => "⭐",
            ObligationType.Secret => "🤫",
            _ => "⛓"
        };
    }
    
    private string FormatObligationText(BindingObligation obligation, int hoursRemaining)
    {
        var timeText = hoursRemaining switch
        {
            <= 0 => "OVERDUE",
            1 => "1 hour",
            _ => $"{hoursRemaining} hours"
        };
        
        return $"{obligation.NpcName}: {obligation.Description} ({timeText})";
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
    public int CreatedAtHour { get; set; }
    public int DueByHour { get; set; }
    public int? FulfilledAtHour { get; set; }
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
    Critical = 0,  // <= 2 hours
    Urgent = 1,    // <= 6 hours
    Soon = 2,      // <= 12 hours
    Later = 3      // > 12 hours
}

/// <summary>
/// View model for displaying binding obligations
/// </summary>
public class BindingObligationViewModel
{
    public string Icon { get; set; }
    public string Text { get; set; }
    public ObligationUrgency Urgency { get; set; }
    public string NpcName { get; set; }
}