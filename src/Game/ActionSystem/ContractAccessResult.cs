namespace Wayfarer.Game.ActionSystem;

/// <summary>
/// Result of checking whether a player can accept or complete a contract
/// </summary>
public class ContractAccessResult
{
    public bool CanAccept { get; set; }
    public bool CanComplete { get; set; }
    public List<string> AcceptanceBlockers { get; set; } = new();
    public List<string> CompletionBlockers { get; set; } = new();
    public List<string> MissingRequirements { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static ContractAccessResult Allowed()
    {
        return new ContractAccessResult { CanAccept = true, CanComplete = true };
    }

    public static ContractAccessResult AcceptanceBlocked(string reason)
    {
        return new ContractAccessResult
        {
            CanAccept = false,
            CanComplete = false,
            AcceptanceBlockers = new List<string> { reason }
        };
    }

    public static ContractAccessResult CompletionBlocked(string reason)
    {
        return new ContractAccessResult
        {
            CanAccept = true,
            CanComplete = false,
            CompletionBlockers = new List<string> { reason }
        };
    }
}