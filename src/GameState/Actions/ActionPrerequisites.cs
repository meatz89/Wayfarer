using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.GameState.Actions;

/// <summary>
/// Container for all prerequisites of an action, providing unified validation
/// </summary>
public class ActionPrerequisites
{
    public List<IActionRequirement> Requirements { get; set; } = new();
    
    /// <summary>
    /// Validate all requirements and return detailed results
    /// </summary>
    public ActionValidationResult Validate(Player player, GameWorld world)
    {
        var result = new ActionValidationResult { IsValid = true };
        
        foreach (var req in Requirements)
        {
            if (!req.IsSatisfied(player, world))
            {
                result.IsValid = false;
                result.AddFailure(
                    req.GetFailureReason(player, world), 
                    req.CanBeRemedied
                );
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get all requirements with their current satisfaction status
    /// </summary>
    public List<RequirementStatus> GetRequirementStatuses(Player player, GameWorld world)
    {
        return Requirements.Select(req => new RequirementStatus
        {
            Requirement = req,
            IsSatisfied = req.IsSatisfied(player, world),
            Progress = req.GetProgress(player, world)
        }).ToList();
    }
    
    /// <summary>
    /// Check if all requirements are met
    /// </summary>
    public bool AllSatisfied(Player player, GameWorld world)
    {
        return Requirements.All(r => r.IsSatisfied(player, world));
    }
    
    /// <summary>
    /// Get a summary of what this action requires
    /// </summary>
    public string GetRequirementsSummary()
    {
        if (!Requirements.Any()) return "No requirements";
        
        return string.Join(", ", Requirements.Select(r => r.GetDescription()));
    }
}

/// <summary>
/// Status of a single requirement
/// </summary>
public class RequirementStatus
{
    public IActionRequirement Requirement { get; set; }
    public bool IsSatisfied { get; set; }
    public double Progress { get; set; }
}