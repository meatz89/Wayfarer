
/// <summary>
/// Result of processing a card effect - COMPLETE projection of all resource changes
/// THREE PARALLEL SYSTEMS: Matches MentalCardEffectResult and PhysicalCardEffectResult structure
/// </summary>
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int InitiativeChange { get; set; }
    public int MomentumChange { get; set; }
    public int DoubtChange { get; set; }
    public int CadenceChange { get; set; } // Cadence resource tracking (rarely used - most cadence comes from Delivery)
    public int UnderstandingChange { get; set; } // NEW: Understanding resource - unlocks tiers, persists through LISTEN
    public int CardsToDraw { get; set; } // Number of cards to draw
    public List<CardInstance> CardsToAdd { get; set; }
    public bool EndsConversation { get; set; }

    // Strategic resource costs (parallel to Mental and Physical systems)
    public int HealthCost { get; set; }
    public int StaminaCost { get; set; }
    public int CoinsCost { get; set; }

    public string EffectOnlyDescription { get; set; } // Card effect formula only (excludes Initiative generation property)
    public string EffectDescription { get; set; } // All resource changes (includes Initiative generation for SPEAK preview)
}

