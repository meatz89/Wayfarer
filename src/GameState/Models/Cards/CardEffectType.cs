public enum CardEffectType
{
    FixedComfort,      // +1 to +5 or -1 to -3 comfort
    ScaledComfort,     // +X where X varies
    DrawCards,         // Draw 1 or 2 cards
    AddWeight,         // Add 1 or 2 to weight pool
    SetAtmosphere,     // Change atmosphere
    ObservationEffect, // Unique observation effects
    GoalEffect        // End conversation with outcome
}