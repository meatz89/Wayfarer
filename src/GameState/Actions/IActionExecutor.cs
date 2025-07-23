using System.Threading.Tasks;

namespace Wayfarer.GameState.Actions;

/// <summary>
/// Interface for executing player actions with unified flow
/// </summary>
public interface IActionExecutor
{
    /// <summary>
    /// Execute an action and return the result
    /// </summary>
    Task<ActionExecutionResult> Execute(ActionOption action, Player player, GameWorld world);
    
    /// <summary>
    /// Check if this executor can handle the given action type
    /// </summary>
    bool CanHandle(LocationAction actionType);
}

/// <summary>
/// Result of executing an action
/// </summary>
public class ActionExecutionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<ActionEffect> Effects { get; set; } = new();
    public bool RequiresConversation { get; set; }
    public ConversationManager ConversationManager { get; set; }
    
    public static ActionExecutionResult Succeeded(string message)
    {
        return new ActionExecutionResult { Success = true, Message = message };
    }
    
    public static ActionExecutionResult Failed(string message)
    {
        return new ActionExecutionResult { Success = false, Message = message };
    }
    
    public static ActionExecutionResult RequiringConversation(ConversationManager manager)
    {
        return new ActionExecutionResult 
        { 
            Success = true, 
            RequiresConversation = true, 
            ConversationManager = manager 
        };
    }
}

/// <summary>
/// Effect of an action on game state
/// </summary>
public class ActionEffect
{
    public string Type { get; set; } // "coins", "stamina", "tokens", etc.
    public string Description { get; set; }
    public int Amount { get; set; }
    public string TargetId { get; set; } // For NPC-specific effects
}