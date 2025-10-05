using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// PROJECTION PRINCIPLE: Pure projection function that returns what WOULD happen
/// without modifying any game state. The resolver NEVER modifies state directly.
/// Parallel to CategoricalEffectResolver in Conversation system.
/// </summary>
public class MentalEffectResolver
{
    private readonly PlayerExertionCalculator _exertionCalculator;

    public MentalEffectResolver(PlayerExertionCalculator exertionCalculator)
    {
        _exertionCalculator = exertionCalculator ?? throw new ArgumentNullException(nameof(exertionCalculator));
    }

    /// <summary>
    /// PROJECTION: Returns what WILL happen when card is played.
    /// Calculates all resource changes without modifying state.
    /// Perfect information for player decision-making.
    /// DUAL BALANCE SYSTEM: Combines action-based balance + method-based balance
    /// </summary>
    public MentalCardEffectResult ProjectCardEffects(CardInstance card, MentalSession session, Player player, MentalActionType actionType)
    {
        MentalCardEffectResult result = new MentalCardEffectResult
        {
            Card = card,
            AttentionChange = 0,
            ProgressChange = 0,
            ExposureChange = 0,
            BalanceChange = 0,
            UnderstandingChange = 0,
            HealthCost = 0,
            StaminaCost = 0,
            CoinsCost = 0,
            CardsToDraw = 1,  // Standard: draw 1 card after playing
            EndsSession = false,
            EffectDescription = ""
        };

        MentalCard template = card.MentalCardTemplate;
        if (template == null)
        {
            result.EffectDescription = "Invalid card";
            return result;
        }

        // Calculate player exertion state for dynamic costs
        PlayerExertionState exertion = _exertionCalculator.CalculateExertion(player);
        int costModifier = exertion.GetMentalCostModifier();

        // Builder resource: Attention
        // Spend Attention cost (modified by mental exertion)
        int modifiedAttentionCost = Math.Max(0, template.AttentionCost + costModifier);
        result.AttentionChange -= modifiedAttentionCost;

        // Generate Attention from Foundation cards (Depth 1-2)
        int attentionGen = template.GetAttentionGeneration();
        result.AttentionChange += attentionGen;

        // DUAL BALANCE SYSTEM:
        // 1. Action-based balance (Observe vs Act rhythm)
        int actionBalance = actionType switch
        {
            MentalActionType.Observe => -2,  // Drawing/observing decreases balance
            MentalActionType.Act => +1,       // Acting increases balance
            _ => 0
        };

        // 2. Method-based balance (card approach)
        int methodBalance = template.Method switch
        {
            Method.Careful => -1,
            Method.Standard => 0,
            Method.Bold => 1,
            Method.Reckless => 2,
            _ => 0
        };

        // Combine both balance effects
        result.BalanceChange = actionBalance + methodBalance;

        // Victory resource: Progress calculated from categorical properties via MentalCardEffectCatalog
        result.ProgressChange = MentalCardEffectCatalog.GetProgressFromProperties(template.Depth, template.Category);

        // Consequence resource: Exposure calculated from categorical properties via MentalCardEffectCatalog
        int baseExposure = MentalCardEffectCatalog.GetExposureFromProperties(template.Depth, template.Method);
        int riskModifier = exertion.GetRiskModifier();
        result.ExposureChange = baseExposure + riskModifier;

        // Balance modifier: High positive balance increases Exposure
        int projectedBalance = session.ObserveActBalance + result.BalanceChange;
        if (projectedBalance > 5)
        {
            result.ExposureChange += 1;
        }

        // Strategic resource costs - calculated from ExertionLevel categorical property
        // TODO: Implement stamina/health/coins cost calculation formulas from ExertionLevel
        result.HealthCost = 0;  // Placeholder
        result.StaminaCost = 0;  // Placeholder
        result.CoinsCost = 0;  // Placeholder

        // Session end detection: Check if this card would end session
        int projectedProgress = session.CurrentProgress + result.ProgressChange;
        int projectedExposure = session.CurrentExposure + result.ExposureChange;
        result.EndsSession = projectedProgress >= 20 || projectedExposure >= 10;

        // Build effect description
        List<string> effects = new List<string>();

        if (result.AttentionChange != 0)
            effects.Add($"Attention {(result.AttentionChange > 0 ? "+" : "")}{result.AttentionChange}");
        if (result.ProgressChange > 0)
            effects.Add($"Progress +{result.ProgressChange}");
        if (result.ExposureChange > 0)
            effects.Add($"Exposure +{result.ExposureChange}");
        if (result.BalanceChange != 0)
            effects.Add($"Balance {(result.BalanceChange > 0 ? "+" : "")}{result.BalanceChange}");

        if (result.HealthCost > 0) effects.Add($"Health -{result.HealthCost}");
        if (result.StaminaCost > 0) effects.Add($"Stamina -{result.StaminaCost}");
        if (result.CoinsCost > 0) effects.Add($"Coins -{result.CoinsCost}");

        result.EffectDescription = effects.Count > 0 ? string.Join(", ", effects) : "No effect";

        return result;
    }
}

public class MentalNarrativeService
{
    public MentalNarrativeService() { }

    public string GenerateActionNarrative(CardInstance card, MentalSession session)
    {
        return card.MentalCardTemplate?.Description ?? "You investigate further.";
    }
}

public class MentalDeckBuilder
{
    private readonly GameWorld _gameWorld;

    public MentalDeckBuilder(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Build deck from engagement type's specific deck (parallel to Social system)
    /// Signature deck knowledge cards added to starting hand (NOT shuffled into deck)
    /// Returns (deck to draw from, starting hand with knowledge cards)
    /// </summary>
    public (List<CardInstance> deck, List<CardInstance> startingHand) BuildDeckWithStartingHand(MentalEngagementType engagementType, string locationId, Player player)
    {
        List<CardInstance> startingHand = new List<CardInstance>();

        // THREE PARALLEL SYSTEMS: Get Mental engagement deck from engagement type
        if (!_gameWorld.MentalEngagementDecks.TryGetValue(engagementType.DeckId, out MentalEngagementDeck deckDefinition))
        {
            throw new InvalidOperationException($"[MentalDeckBuilder] Mental engagement deck '{engagementType.DeckId}' not found in GameWorld.MentalEngagementDecks");
        }

        // Build card instances from engagement deck (parallel to ConversationDeckBuilder pattern)
        List<CardInstance> deck = deckDefinition.BuildCardInstances(_gameWorld);

        // Equipment category filtering: Remove cards requiring equipment player doesn't have
        List<EquipmentCategory> playerEquipmentCategories = GetPlayerEquipmentCategories(player);
        deck = deck.Where(instance =>
        {
            MentalCard template = instance.MentalCardTemplate;
            if (template == null) return false;

            if (template.EquipmentCategory != EquipmentCategory.None)
            {
                if (!playerEquipmentCategories.Contains(template.EquipmentCategory))
                {
                    return false;
                }
            }

            return true;
        }).ToList();

        // Get location and add signature deck knowledge cards to STARTING HAND
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location?.SignatureDeck != null)
        {
            List<SignatureKnowledgeCard> knowledgeCards = location.SignatureDeck.GetCardsForTacticalType(TacticalSystemType.Mental);
            foreach (SignatureKnowledgeCard sigCard in knowledgeCards)
            {
                MentalCard knowledgeTemplate = _gameWorld.MentalCards
                    .FirstOrDefault(e => e.Card.Id == sigCard.CardId)?.Card;

                if (knowledgeTemplate != null)
                {
                    CardInstance knowledgeInstance = new CardInstance
                    {
                        InstanceId = Guid.NewGuid().ToString(),
                        MentalCardTemplate = knowledgeTemplate
                    };
                    startingHand.Add(knowledgeInstance);
                }
            }
        }

        return (deck, startingHand);
    }


    private List<EquipmentCategory> GetPlayerEquipmentCategories(Player player)
    {
        List<EquipmentCategory> categories = new List<EquipmentCategory>();
        foreach (string itemId in player.Inventory.GetAllItems())
        {
            Item item = _gameWorld.WorldState.Items?.FirstOrDefault(i => i.Id == itemId);
            if (item?.ProvidedEquipmentCategories != null)
            {
                categories.AddRange(item.ProvidedEquipmentCategories);
            }
        }
        return categories.Distinct().ToList();
    }
}

public class MentalContextBase
{
    public MentalSession Session { get; set; }
    public Location Location { get; set; }
}
