using System.Collections.Generic;

/// <summary>
/// PROJECTION PRINCIPLE: Card effect projection for Mental tactical system
/// Parallel to CardEffectResult in Conversation system
/// Shows what WILL happen before committing - perfect information for player
///
/// PERFECT INFORMATION ENHANCEMENT: Tracks base values + all bonuses separately
/// so UI can display complete breakdown to player (e.g., "5 base + 2 specialist + 1 familiarity = 8 total")
/// </summary>
public class MentalCardEffectResult
{
    public CardInstance Card { get; set; }

    // Builder resource (parallel to Initiative in Social)
    public int AttentionChange { get; set; }
    public int BaseAttention { get; set; }
    public List<EffectBonus> AttentionBonuses { get; set; } = new List<EffectBonus>();

    // Victory resource (parallel to Momentum in Social)
    public int ProgressChange { get; set; }
    public int BaseProgress { get; set; }
    public List<EffectBonus> ProgressBonuses { get; set; } = new List<EffectBonus>();

    // Consequence resource (parallel to Doubt in Social)
    public int ExposureChange { get; set; }
    public int BaseExposure { get; set; }
    public List<EffectBonus> ExposureBonuses { get; set; } = new List<EffectBonus>();

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
