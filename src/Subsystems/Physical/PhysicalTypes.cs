using System.Collections.Generic;

/// <summary>
/// PROJECTION PRINCIPLE: Card effect projection for Physical tactical system
/// Parallel to CardEffectResult in Conversation system
/// Shows what WILL happen before committing - perfect information for player
/// </summary>
public class PhysicalCardEffectResult
{
    public CardInstance Card { get; set; }

    // Builder resource (parallel to Initiative in Social)
    public int PositionChange { get; set; }

    // Victory resource (parallel to Momentum in Social)
    public int BreakthroughChange { get; set; }

    // Consequence resource (parallel to Doubt in Social)
    public int DangerChange { get; set; }

    // Balance resource (rhythm tracking)
    public int BalanceChange { get; set; }

    // Persistent progress resource
    public int ReadinessChange { get; set; }

    // Strategic resource costs
    public int HealthCost { get; set; }
    public int StaminaCost { get; set; }
    public int CoinsCost { get; set; }

    // Card draw
    public int CardsToDraw { get; set; }

    // Session control
    public bool EndsSession { get; set; }

    // UI display
    public string EffectDescription { get; set; }
}

public class PhysicalOutcome
{
    public bool Success { get; set; }
    public int FinalProgress { get; set; }
    public int FinalDanger { get; set; }
    public string EscapeCost { get; set; } = null; // Resources lost if escaped
}

public class PhysicalTurnResult
{
    public bool Success { get; set; }
    public string Narrative { get; set; }
    public int CurrentBreakthrough { get; set; }
    public int CurrentDanger { get; set; }
    public bool SessionEnded { get; set; }
}

public class PhysicalTier
{
    public int TierLevel { get; set; }
    public int ReadinessRequired { get; set; }
}
