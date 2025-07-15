namespace Wayfarer.Game.ActionSystem;

/// <summary>
/// Result of checking whether a player can perform an action
/// </summary>
public class ActionAccessResult
{
    public bool IsAllowed { get; set; }
    public List<string> BlockingReasons { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Requirements { get; set; } = new();

    public static ActionAccessResult Allowed()
    {
        return new ActionAccessResult { IsAllowed = true };
    }

    public static ActionAccessResult Blocked(string reason)
    {
        return new ActionAccessResult
        {
            IsAllowed = false,
            BlockingReasons = new List<string> { reason }
        };
    }

    public static ActionAccessResult Blocked(List<string> reasons)
    {
        return new ActionAccessResult
        {
            IsAllowed = false,
            BlockingReasons = reasons
        };
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public void AddRequirement(string requirement)
    {
        Requirements.Add(requirement);
    }
}
