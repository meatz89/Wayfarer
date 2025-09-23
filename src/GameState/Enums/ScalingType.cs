namespace Wayfarer.GameState.Enums
{
    /// <summary>
    /// Scaling types for deterministic card effect calculations
    /// Based on the conversation-screen.html UI mockup formulas
    /// Also used for standing obligations
    /// </summary>
    public enum ScalingType
    {
        None = 0,

        // Standing Obligation scaling types
        Linear,        // Linear scaling: effect = base + (tokens * scalingFactor)
        Stepped,       // Stepped scaling: effect changes at specific thresholds
        Threshold,     // Threshold scaling: effect only applies above/below threshold

        // Conversation card scaling formulas
        CardsInHand,           // = cards in hand (5)
        CardsInHandDivided,    // = cards in hand ÷ 2 (rounded up)
        DoubtReduction,        // = (10 - doubt)
        DoubtHalved,           // = (10 - doubt) / 2
        DoubleCurrent,         // Double current momentum
        PatienteDivided        // = patience/3 (using focus as proxy)
    }
}