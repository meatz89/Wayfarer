using System.Collections.Generic;

/// <summary>
/// Result of processing a card effect - COMPLETE projection of all resource changes
/// THREE PARALLEL SYSTEMS: Matches MentalCardEffectResult and PhysicalCardEffectResult structure
///
/// PERFECT INFORMATION ENHANCEMENT: Tracks base values + all bonuses separately
/// so UI can display complete breakdown to player (e.g., "5 base + 2 personality + 1 connection = 8 total")
/// </summary>
public class CardEffectResult
{
    public CardInstance Card { get; set; }

    // Builder resource
    public int InitiativeChange { get; set; }
    public int BaseInitiative { get; set; }
    public List<EffectBonus> InitiativeBonuses { get; set; } = new List<EffectBonus>();

    // Victory resource
    public int MomentumChange { get; set; }
    public int BaseMomentum { get; set; }
    public List<EffectBonus> MomentumBonuses { get; set; } = new List<EffectBonus>();

    // Consequence resource
    public int DoubtChange { get; set; }
    public int BaseDoubt { get; set; }
    public List<EffectBonus> DoubtBonuses { get; set; } = new List<EffectBonus>();

    // Cadence resource tracking (rarely used - most cadence comes from Delivery)
    public int CadenceChange { get; set; }

    // Understanding resource - unlocks tiers, persists through LISTEN
    public int UnderstandingChange { get; set; }

    // Card draw
    public int CardsToDraw { get; set; }
    public List<CardInstance> CardsToAdd { get; set; }

    // Session control
    public bool EndsConversation { get; set; }

    // Strategic resource costs (parallel to Mental and Physical systems)
    public int HealthCost { get; set; }
    public int StaminaCost { get; set; }
    public int CoinsCost { get; set; }

    // UI display
    public string EffectOnlyDescription { get; set; } // Card effect formula only (excludes Initiative generation property)
    public string EffectDescription { get; set; } // All resource changes (includes Initiative generation for SPEAK preview)
}

