using System.Collections.Generic;
using Wayfarer.GameState.Actions;

/// <summary>
/// Requirement that checks if an action is allowed during an active narrative
/// Integrates with the existing IActionRequirement system for clean architecture
/// </summary>
public class NarrativeRequirement : IActionRequirement
{
    private readonly NarrativeManager _narrativeManager;
    private readonly FlagService _flagService;
    private readonly LocationAction _action;
    private readonly string _targetId;
    
    public NarrativeRequirement(
        NarrativeManager narrativeManager, 
        FlagService flagService,
        LocationAction action,
        string targetId = null)
    {
        _narrativeManager = narrativeManager;
        _flagService = flagService;
        _action = action;
        _targetId = targetId;
    }
    
    public bool IsSatisfied(Player player, GameWorld world)
    {
        // If no active narrative, always satisfied
        if (!_narrativeManager.HasActiveNarrative())
            return true;
            
        // Get allowed actions from narrative manager
        var allowedActions = _narrativeManager.FilterActions(new List<LocationAction> { _action });
        
        // Check if this specific action is allowed
        if (!allowedActions.Contains(_action))
            return false;
            
        // Additional validation for specific targets if needed
        // This would check if the action targets the right NPC/location
        // The actual validation happens in NarrativeManager.OnActionCompleted
        
        return true;
    }
    
    public string GetFailureReason(Player player, GameWorld world)
    {
        // Provide context-specific guidance
        foreach (var narrativeId in GetActiveNarrativeIds())
        {
            var step = _narrativeManager.GetCurrentStep(narrativeId);
            if (step != null)
            {
                // If this is the wrong action entirely
                if (step.RequiredAction.HasValue && step.RequiredAction.Value != _action)
                {
                    return $"Current objective: {step.Description}";
                }
                
                // If it's the right action but wrong target
                if (step.RequiredAction == _action)
                {
                    if (!string.IsNullOrEmpty(step.RequiredNPC) && _targetId != step.RequiredNPC)
                    {
                        return $"You need to speak with someone else for this objective.";
                    }
                    
                    if (!string.IsNullOrEmpty(step.RequiredLocation) && _targetId != step.RequiredLocation)
                    {
                        return $"This action needs to be performed elsewhere.";
                    }
                }
                
                return step.GuidanceText ?? "Complete your current objective first.";
            }
        }
        
        return "This action is not available during the current narrative.";
    }
    
    // Helper to get active narrative IDs
    private List<string> GetActiveNarrativeIds()
    {
        return _narrativeManager.GetActiveNarrativeIds();
    }

    public string GetDescription()
    {
        return "Complete current narrative objectives";
    }
    
    public bool CanBeRemedied => true;
    
    public string GetRemediationHint()
    {
        return "Complete your current objective first";
    }
    
    public double GetProgress(Player player, GameWorld world)
    {
        if (!_narrativeManager.HasActiveNarrative())
            return 1.0;
        return 0.0;
    }
}

/// <summary>
/// Factory for creating narrative requirements
/// Allows LocationActionManager to easily check narrative constraints
/// </summary>
public class NarrativeRequirementFactory
{
    private readonly NarrativeManager _narrativeManager;
    private readonly FlagService _flagService;
    
    public NarrativeRequirementFactory(NarrativeManager narrativeManager, FlagService flagService)
    {
        _narrativeManager = narrativeManager;
        _flagService = flagService;
    }
    
    public NarrativeRequirement CreateForAction(LocationAction action, string targetId = null)
    {
        return new NarrativeRequirement(_narrativeManager, _flagService, action, targetId);
    }
    
    public bool IsActionAllowed(LocationAction action, string targetId = null)
    {
        var requirement = CreateForAction(action, targetId);
        // Need player reference - would come from GameWorld in real implementation
        // For now, just check if narrative is active
        return !_narrativeManager.HasActiveNarrative() || 
               _narrativeManager.FilterActions(new List<LocationAction> { action }).Contains(action);
    }
}