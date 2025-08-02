using System;
using System.Threading.Tasks;
using Wayfarer.GameState;

namespace Wayfarer.Services;

/// <summary>
/// Service that wraps action execution to provide deadline warnings
/// </summary>
public class ActionExecutionService
{
    private readonly TimeImpactCalculator _timeImpactCalculator;
    private readonly CommandExecutor _commandExecutor;
    private readonly LocationActionsUIService _locationActionsService;
    
    // State for pending action
    private string _pendingActionId;
    private TaskCompletionSource<bool> _pendingActionCompletion;
    
    public ActionExecutionService(
        TimeImpactCalculator timeImpactCalculator,
        CommandExecutor commandExecutor,
        LocationActionsUIService locationActionsService)
    {
        _timeImpactCalculator = timeImpactCalculator;
        _commandExecutor = commandExecutor;
        _locationActionsService = locationActionsService;
    }
    
    /// <summary>
    /// Checks if an action would cause deadline impacts
    /// </summary>
    public TimeImpactInfo CheckActionTimeImpact(string actionId)
    {
        // Get the action details to determine time cost
        var actions = _locationActionsService.GetLocationActionsViewModel();
        
        foreach (var group in actions.ActionGroups)
        {
            var action = group.Actions.FirstOrDefault(a => a.Id == actionId);
            if (action != null && action.TimeCost > 0)
            {
                return _timeImpactCalculator.CalculateTimeImpact(action.TimeCost);
            }
        }
        
        // No time cost found
        return null;
    }
    
    /// <summary>
    /// Begins action execution, returns true if warning is needed
    /// </summary>
    public async Task<(bool needsWarning, TimeImpactInfo timeImpact)> BeginActionExecutionAsync(string actionId)
    {
        var timeImpact = CheckActionTimeImpact(actionId);
        
        if (timeImpact?.LettersExpiring > 0)
        {
            // Store pending action for later execution
            _pendingActionId = actionId;
            _pendingActionCompletion = new TaskCompletionSource<bool>();
            
            return (true, timeImpact);
        }
        
        // No warning needed, execute immediately
        var result = await _locationActionsService.ExecuteActionAsync(actionId);
        return (false, null);
    }
    
    /// <summary>
    /// Confirms execution of pending action after warning
    /// </summary>
    public async Task<bool> ConfirmPendingActionAsync()
    {
        if (_pendingActionId == null || _pendingActionCompletion == null)
        {
            return false;
        }
        
        try
        {
            var result = await _locationActionsService.ExecuteActionAsync(_pendingActionId);
            _pendingActionCompletion.SetResult(result);
            return result;
        }
        finally
        {
            _pendingActionId = null;
            _pendingActionCompletion = null;
        }
    }
    
    /// <summary>
    /// Cancels pending action
    /// </summary>
    public void CancelPendingAction()
    {
        if (_pendingActionCompletion != null)
        {
            _pendingActionCompletion.SetResult(false);
        }
        
        _pendingActionId = null;
        _pendingActionCompletion = null;
    }
}