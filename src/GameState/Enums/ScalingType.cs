/// <summary>
/// Scaling types for deterministic card effect calculations using only 4-resource system
/// All scaling effects must use ONLY: Initiative, Cadence, Momentum, Doubt + visible piles
/// </summary>
public enum ScalingType
{
    None = 0,

    // Standing Obligation scaling types
    Linear,        // Linear scaling: effect = base + (tokens * scalingFactor)
    Stepped,       // Stepped scaling: effect changes at specific thresholds
    Threshold,     // Threshold scaling: effect only applies above/below threshold

    // Visible resource scaling (Initiative, Cadence, Momentum, Doubt)
    CurrentInitiative,     // = current initiative
    CurrentCadence,        // = current cadence value
    CurrentMomentum,       // = current momentum
    CurrentDoubt,          // = current doubt
    DoubleMomentum,        // Double current momentum

    // Visible pile scaling (Mind, Spoken, Deck)
    CardsInMind,           // = cards in mind (hand)
    CardsInSpoken,         // = cards in spoken pile
    CardsInDeck,           // = cards remaining in deck

    // Resource conversion (Momentum spending)
    SpendMomentumForDoubt, // Spend momentum → reduce doubt
    SpendMomentumForInitiative, // Spend momentum → gain initiative

    // Conditional scaling based on visible state
    DoubtMultiplier,       // Effect scales with current doubt level
    CadenceBonus,          // Effect scales with cadence (positive/negative)
    InitiativeThreshold    // Effect only applies above/below initiative threshold
}
