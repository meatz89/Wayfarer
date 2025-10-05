using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// PROJECTION PRINCIPLE: Pure projection function that returns what WOULD happen
/// without modifying any game state. The resolver NEVER modifies state directly.
/// Parallel to CategoricalEffectResolver in Conversation system.
/// </summary>
public class PhysicalEffectResolver
{
    private readonly PlayerExertionCalculator _exertionCalculator;

    public PhysicalEffectResolver(PlayerExertionCalculator exertionCalculator)
    {
        _exertionCalculator = exertionCalculator ?? throw new ArgumentNullException(nameof(exertionCalculator));
    }

    /// <summary>
    /// PROJECTION: Returns what WILL happen when card is played.
    /// Calculates all resource changes without modifying state.
    /// Perfect information for player decision-making.
    /// DUAL BALANCE SYSTEM: Combines action-based balance + approach-based balance
    /// </summary>
    public PhysicalCardEffectResult ProjectCardEffects(CardInstance card, PhysicalSession session, Player player, PhysicalActionType actionType)
    {
        PhysicalCardEffectResult result = new PhysicalCardEffectResult
        {
            Card = card,
            PositionChange = 0,
            BreakthroughChange = 0,
            DangerChange = 0,
            BalanceChange = 0,
            ReadinessChange = 0,
            HealthCost = 0,
            StaminaCost = 0,
            CoinsCost = 0,
            CardsToDraw = 1,  // Standard: draw 1 card after playing
            EndsSession = false,
            EffectDescription = ""
        };

        PhysicalCard template = card.PhysicalCardTemplate;
        if (template == null)
        {
            result.EffectDescription = "Invalid card";
            return result;
        }

        // Calculate player exertion state for dynamic costs
        PlayerExertionState exertion = _exertionCalculator.CalculateExertion(player);
        int costModifier = exertion.GetPhysicalCostModifier();

        // Builder resource: Position
        // Spend Position cost (modified by physical exertion)
        int modifiedPositionCost = Math.Max(0, template.PositionCost + costModifier);
        result.PositionChange -= modifiedPositionCost;

        // Generate Position from Foundation cards (Depth 1-2)
        int positionGen = template.GetPositionGeneration();
        result.PositionChange += positionGen;

        // DUAL BALANCE SYSTEM:
        // 1. Action-based balance (Assess vs Execute rhythm)
        int actionBalance = actionType switch
        {
            PhysicalActionType.Assess => -2,   // Drawing/assessing decreases balance
            PhysicalActionType.Execute => +1,  // Executing increases balance
            _ => 0
        };

        // 2. Approach-based balance (card approach)
        int approachBalance = template.Approach switch
        {
            Approach.Methodical => -1,
            Approach.Standard => 0,
            Approach.Aggressive => 1,
            Approach.Reckless => 2,
            _ => 0
        };

        // Combine both balance effects
        result.BalanceChange = actionBalance + approachBalance;

        // Victory resource: Breakthrough calculated from categorical properties via PhysicalCardEffectCatalog
        result.BreakthroughChange = PhysicalCardEffectCatalog.GetProgressFromProperties(template.Depth, template.Category);

        // Consequence resource: Danger calculated from categorical properties via PhysicalCardEffectCatalog
        int baseDanger = PhysicalCardEffectCatalog.GetDangerFromProperties(template.Depth, template.Approach);
        int riskModifier = exertion.GetRiskModifier();
        result.DangerChange = baseDanger + riskModifier;

        // Balance modifier: High positive balance increases Danger
        int projectedBalance = session.Commitment + result.BalanceChange;
        if (projectedBalance > 5)
        {
            result.DangerChange += 1;
        }

        // Strategic resource costs - PRE-CALCULATED at parse time via PhysicalCardEffectCatalog
        // Resolver just uses the values calculated during parsing (no runtime calculation)
        result.StaminaCost = template.StaminaCost;
        result.HealthCost = template.DirectHealthCost;
        result.CoinsCost = template.CoinCost;

        // Session end detection: Check if this card would end session
        int projectedProgress = session.CurrentBreakthrough + result.BreakthroughChange;
        int projectedDanger = session.CurrentDanger + result.DangerChange;
        result.EndsSession = projectedProgress >= 20 || projectedDanger >= session.MaxDanger;

        // Build effect description
        List<string> effects = new List<string>();

        if (result.PositionChange != 0)
            effects.Add($"Position {(result.PositionChange > 0 ? "+" : "")}{result.PositionChange}");
        if (result.BreakthroughChange > 0)
            effects.Add($"Breakthrough +{result.BreakthroughChange}");
        if (result.DangerChange > 0)
            effects.Add($"Danger +{result.DangerChange}");
        if (result.BalanceChange != 0)
            effects.Add($"Balance {(result.BalanceChange > 0 ? "+" : "")}{result.BalanceChange}");

        if (result.HealthCost > 0) effects.Add($"Health -{result.HealthCost}");
        if (result.StaminaCost > 0) effects.Add($"Stamina -{result.StaminaCost}");
        if (result.CoinsCost > 0) effects.Add($"Coins -{result.CoinsCost}");

        result.EffectDescription = effects.Count > 0 ? string.Join(", ", effects) : "No effect";

        return result;
    }
}

public class PhysicalNarrativeService
{
    public PhysicalNarrativeService() { }

    public string GenerateActionNarrative(CardInstance card, PhysicalSession session)
    {
        return card.PhysicalCardTemplate?.Description ?? "You face the challenge.";
    }
}

public class PhysicalDeckBuilder
{
    private readonly GameWorld _gameWorld;

    public PhysicalDeckBuilder(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Build deck from engagement type's specific deck (parallel to Social system)
    /// Signature deck knowledge cards added to starting hand (NOT shuffled into deck)
    /// Returns (deck to draw from, starting hand with knowledge and injury cards)
    /// </summary>
    public (List<CardInstance> deck, List<CardInstance> startingHand) BuildDeckWithStartingHand(PhysicalEngagementType engagementType, string locationId, Player player)
    {
        List<CardInstance> startingHand = new List<CardInstance>();

        // THREE PARALLEL SYSTEMS: Get Physical engagement deck from engagement type
        if (!_gameWorld.PhysicalEngagementDecks.TryGetValue(engagementType.DeckId, out PhysicalEngagementDeck deckDefinition))
        {
            throw new InvalidOperationException($"[PhysicalDeckBuilder] Physical engagement deck '{engagementType.DeckId}' not found in GameWorld.PhysicalEngagementDecks");
        }

        // Build card instances from engagement deck (parallel to ConversationDeckBuilder pattern)
        List<CardInstance> deck = deckDefinition.BuildCardInstances(_gameWorld);

        // Equipment category filtering: Remove cards requiring equipment player doesn't have
        List<EquipmentCategory> playerEquipmentCategories = GetPlayerEquipmentCategories(player);
        deck = deck.Where(instance =>
        {
            PhysicalCard template = instance.PhysicalCardTemplate;
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

        // Add injury cards to deck (debuffs from past failures - Physical debt system)
        foreach (string injuryCardId in player.InjuryCardIds)
        {
            PhysicalCard injuryTemplate = _gameWorld.PhysicalCards
                .FirstOrDefault(e => e.Card.Id == injuryCardId)?.Card;

            if (injuryTemplate != null)
            {
                CardInstance injuryInstance = new CardInstance
                {
                    InstanceId = Guid.NewGuid().ToString(),
                    PhysicalCardTemplate = injuryTemplate
                };
                deck.Add(injuryInstance);
            }
        }

        // Get location and add signature deck knowledge cards to STARTING HAND
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId);
        if (location?.SignatureDeck != null)
        {
            List<SignatureKnowledgeCard> knowledgeCards = location.SignatureDeck.GetCardsForTacticalType(TacticalSystemType.Physical);
            foreach (SignatureKnowledgeCard sigCard in knowledgeCards)
            {
                PhysicalCard knowledgeTemplate = _gameWorld.PhysicalCards
                    .FirstOrDefault(e => e.Card.Id == sigCard.CardId)?.Card;

                if (knowledgeTemplate != null)
                {
                    CardInstance knowledgeInstance = new CardInstance
                    {
                        InstanceId = Guid.NewGuid().ToString(),
                        PhysicalCardTemplate = knowledgeTemplate
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

public class PhysicalContextBase
{
    public PhysicalSession Session { get; set; }
    public Location Location { get; set; }
}
