/// <summary>
/// Represents the reward structure for an exchange.
/// All rewards are granted atomically upon successful exchange.
/// </summary>
public class ExchangeRewardStructure
{
    /// <summary>
    /// Resources granted by this exchange.
    /// All resources are added to the player's totals.
    /// </summary>
    public List<ResourceAmount> Resources { get; set; } = new List<ResourceAmount>();

    /// <summary>
    /// Items granted by this exchange.
    /// These are item template IDs that will be instantiated.
    /// </summary>
    public List<string> ItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Tokens granted by this exchange.
    /// </summary>
    public List<TokenCount> Tokens { get; set; } = new List<TokenCount>();

    /// <summary>
    /// Optional effects that trigger upon exchange completion.
    /// These could unlock new exchanges, modify NPC states, etc.
    /// </summary>
    public List<string> EffectIds { get; set; } = new List<string>();

    /// <summary>
    /// Gets a human-readable description of the rewards.
    /// </summary>
    public string GetDescription()
    {
        List<string> parts = new List<string>();

        foreach (ResourceAmount resource in Resources)
        {
            parts.Add($"{resource.Amount} {resource.Type}");
        }

        foreach (string item in ItemIds)
        {
            parts.Add($"1x {item}");
        }

        foreach (TokenCount token in Tokens)
        {
            if (token.Count > 0)
            {
                parts.Add($"{token.Count} {token.Type} Token{(token.Count > 1 ? "s" : "")}");
            }
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "Nothing";
    }

    /// <summary>
    /// Checks if this reward structure has any actual rewards.
    /// </summary>
    public bool HasRewards()
    {
        return (Resources != null && Resources.Count > 0) ||
               (ItemIds != null && ItemIds.Count > 0) ||
               (Tokens != null && Tokens.Count > 0) ||
               (EffectIds != null && EffectIds.Count > 0);
    }
}