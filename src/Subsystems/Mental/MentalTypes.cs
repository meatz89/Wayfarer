using System.Collections.Generic;

/// <summary>
/// PROJECTION PRINCIPLE: Card effect projection for Mental tactical system
/// Parallel to CardEffectResult in Conversation system
/// Shows what WILL happen before committing - perfect information for player
/// </summary>
public class MentalCardEffectResult
{
    public CardInstance Card { get; set; }

    // Builder resource (parallel to Initiative in Social)
    public int AttentionChange { get; set; }

    // Victory resource (parallel to Momentum in Social)
    public int ProgressChange { get; set; }

    // Consequence resource (parallel to Doubt in Social)
    public int ExposureChange { get; set; }

    // Balance resource (rhythm tracking)
    public int BalanceChange { get; set; }

    // Persistent progress resource
    public int UnderstandingChange { get; set; }

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

public class MentalOutcome
{
    public bool Success { get; set; }
    public int FinalProgress { get; set; }
    public int FinalExposure { get; set; }
    public bool SessionSaved { get; set; } = false; // True if player left investigation to return later
}

public class MentalTurnResult
{
    public bool Success { get; set; }
    public string Narrative { get; set; }
    public int CurrentProgress { get; set; }
    public int CurrentExposure { get; set; }
    public bool SessionEnded { get; set; }
}

public class MentalTier
{
    public int TierLevel { get; set; }
    public int UnderstandingRequired { get; set; }
}
