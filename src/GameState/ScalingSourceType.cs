/// <summary>
/// Sources of values for scaling formulas.
/// Defines what game state value to multiply by the scaling multiplier.
/// </summary>
public enum ScalingSourceType
{
// Core resources
Doubt,
PositiveCadence,
NegativeCadence,
Momentum,
Initiative,

// Card piles
MindCards,
SpokenCards,
DeckCards,

// Statement history
TotalStatements,
InsightStatements,
RapportStatements,
AuthorityStatements,
DiplomacyStatements,
CunningStatements
}