/// <summary>
/// Behavioral reputation scales (Sir Brante pattern)
/// Tracks player's emergent character identity across multiple dimensions
/// All scales range from -10 (negative pole) to +10 (positive pole), default 0 (neutral)
/// </summary>
public enum ScaleType
{
/// <summary>
/// Morality: Altruistic vs Exploitative
/// +10 = Selfless, helps others without compensation
/// 0 = Practical, balances self-interest with fairness
/// -10 = Ruthless, exploits others for personal gain
/// </summary>
Morality,

/// <summary>
/// Lawfulness: Law-abiding vs Outlaw
/// +10 = Strictly follows rules and authorities
/// 0 = Pragmatic, follows laws when convenient
/// -10 = Disregards laws, operates outside system
/// </summary>
Lawfulness,

/// <summary>
/// Method: Diplomatic vs Violent
/// +10 = Always seeks peaceful resolution
/// 0 = Uses appropriate force when needed
/// -10 = Resorts to violence first
/// </summary>
Method,

/// <summary>
/// Caution: Careful vs Reckless
/// +10 = Plans thoroughly, avoids risk
/// 0 = Balances caution with action
/// -10 = Impulsive, takes unnecessary risks
/// </summary>
Caution,

/// <summary>
/// Transparency: Honest vs Deceptive
/// +10 = Always tells truth, even when costly
/// 0 = Honest but pragmatic
/// -10 = Habitually lies and deceives
/// </summary>
Transparency,

/// <summary>
/// Fame: Renowned vs Anonymous
/// +10 = Famous, recognized everywhere
/// 0 = Known locally, anonymous elsewhere
/// -10 = Actively avoids attention, operates in shadows
/// </summary>
Fame
}
