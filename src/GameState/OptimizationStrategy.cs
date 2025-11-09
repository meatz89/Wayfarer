/// <summary>
/// Defines the strategy for route optimization based on player priorities.
/// Used to determine which route characteristics to prioritize.
/// </summary>
public enum OptimizationStrategy
{
/// <summary>
/// Optimize for overall efficiency (balanced cost, time, and stamina)
/// </summary>
Efficiency,

/// <summary>
/// Minimize coin cost
/// </summary>
CheapestCost,

/// <summary>
/// Minimize stamina expenditure
/// </summary>
LeastStamina,

/// <summary>
/// Minimize travel time
/// </summary>
FastestTime,

/// <summary>
/// Maximize safety and reliability
/// </summary>
Safest
}