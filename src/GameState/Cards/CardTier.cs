/// <summary>
/// Card tier determines resource flow patterns across all tactical systems
/// Foundation: Generate builder resource, don't generate victory resource
/// Standard: Cost builder resource, generate victory resource
/// Decisive: High cost, high impact, unlocked by depth tiers
/// </summary>
public enum CardTier
{
Foundation,  // Generate builder resource (Initiative/Attention/Position), no victory resource
Standard,    // Cost builder resource, generate victory resource (Momentum/Progress/Breakthrough)
Decisive     // Unlocked by Understanding/depth tiers, powerful effects, high costs
}
