# Conversation Card System - Complete Architecture

This document describes the complete conversation card system architecture including card properties, formulas, effects, conversation types, deck management, parsing, projection, and execution.

---

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Card Properties](#card-properties)
3. [Effect Formula System](#effect-formula-system)
4. [Statement Requirements](#statement-requirements)
5. [Conversation Resources](#conversation-resources)
6. [Tier Unlock System](#tier-unlock-system)
7. [Card Catalog](#card-catalog)
8. [Parsing Pipeline](#parsing-pipeline)
9. [Projection Principle](#projection-principle)
10. [Effect Execution](#effect-execution)
11. [Conversation Types](#conversation-types)
12. [Deck Management](#deck-management)

---

## Core Concepts

### Stat-to-Resource Mapping

**CRITICAL REFINEMENT**: **Specialist with Universal Access** model, NOT hard exclusivity.

Each stat SPECIALIZES in one resource (2-3x efficiency) but can ACCESS universal resources (Momentum/Initiative) at standard rates.

| Stat | Specialist Resource | Universal Access | Trade-offs |
|------|---------------------|------------------|------------|
| **Insight** | Cards (2-6 draw) | Momentum (+1-3), Initiative (+1-2) | Information-focused |
| **Rapport** | Cadence (-1 to -3) | Momentum (+1-3), Initiative (+1-3) | Sustainable, lower momentum |
| **Authority** | Momentum (+2-12) | Initiative (+1-2) | High momentum but generates Doubt |
| **Commerce** | Doubt (-1 to -6) | Momentum (+1-3) | Often consumes Momentum to reduce Doubt |
| **Cunning** | Initiative (+2-6) | Momentum (+1-3) | Enables long action chains |

**Why This Works:**
- **Verisimilitude**: All conversation approaches advance progress (momentum), not just commands
- **Gameplay**: Every deck can reach goals; specialists just excel in their domain
- **Mechanical**: Prevents impossible deck compositions (e.g., "zero Authority" can't win without Momentum)

**Pattern by Depth:**
- **Foundation (1-2)**: Specialist 2x rate, Universal 1x rate
- **Standard (3-4)**: Specialist 2.5x rate, Universal 1.5x rate
- **Advanced (5-6)**: Specialist 3x rate, Universal 2x rate
- **Master (7-8)**: Specialist 3-4x rate, Universal 2-3x rate

### Formula Types

Six formula types for different calculation patterns:

1. **Fixed** (Type A): Simple, predictable values
2. **Scaling** (Type B): Effect = base × state_value
3. **Conditional** (Type C): Effect depends on threshold
4. **Trading** (Type D): Consume X of A, gain Y of B
5. **Setting** (Type E): Transform state directly
6. **Compound**: Multiple fixed effects combined

---

## Card Properties

### Core Properties

Every card has these fundamental properties:

```csharp
public class ConversationCard
{
    // Identity
    public string Id { get; init; }
    public string Title { get; init; }
    public string DialogueText { get; init; }

    // Stat Binding
    public PlayerStatType? BoundStat { get; init; }

    // Depth & Category
    public DepthTier? Depth { get; init; }      // 1-8
    public CardCategory Category { get; init; }  // Expression/Realization/Regulation

    // Cost & Persistence
    public int InitiativeCost { get; init; }
    public PersistenceType Persistence { get; init; }  // Echo/Statement

    // Requirements
    public PlayerStatType? RequiredStat { get; init; }
    public int RequiredStatements { get; init; }

    // Effects
    public CardEffectFormula EffectFormula { get; init; }
}
```

### Card Categories

Three categories represent conversational moves:

**Expression** - Outward moves (statements, challenges, offers)
- Insight: Questions, observations revealed
- Rapport: Encouragement, validation
- Authority: Demands, assertions
- Commerce: Proposals, alternatives
- Cunning: Baits, maneuvers

**Realization** - Understanding moves (patterns, opportunities)
- Insight: Conclusions, deductions
- Rapport: (Not used - Rapport is Expression/Regulation)
- Authority: (Not used - Authority is Expression)
- Commerce: (Not used - Commerce is Regulation)
- Cunning: Openings, advantages

**Regulation** - Control moves (balance, safety, rhythm)
- Insight: (Not used - Insight is Expression/Realization)
- Rapport: Listening, understanding
- Authority: (Not used - Authority drives, not regulates)
- Commerce: Risk mitigation, reassurance
- Cunning: (Not used - Cunning creates, not regulates)

### Persistence Types

**Echo** - Returns to hand after play (sustainable tools)
**Statement** - Goes to Spoken pile, counts toward requirements

**Distribution by Depth**:
- Foundation (1-2): 70-80% Echo, 20-30% Statement
- Standard (3-4): 50-60% Echo, 40-50% Statement
- Advanced (5-6): 40-50% Echo, 50-60% Statement
- Master (7-8): 20-30% Echo, 70-80% Statement

---

## Effect Formula System

### Formula Structure

```csharp
public class CardEffectFormula
{
    public EffectFormulaType FormulaType { get; set; }
    public ConversationResourceType TargetResource { get; set; }

    // Fixed
    public int? BaseValue { get; set; }

    // Scaling
    public ScalingSourceType? ScalingSource { get; set; }
    public decimal ScalingMultiplier { get; set; }
    public int? ScalingMax { get; set; }

    // Conditional
    public ConversationResourceType? ConditionResource { get; set; }
    public int? ConditionThreshold { get; set; }
    public int? ConditionMetValue { get; set; }
    public int? ConditionUnmetValue { get; set; }

    // Trading
    public ConversationResourceType? ConsumeResource { get; set; }
    public int? ConsumeAmount { get; set; }
    public int? TradeRatio { get; set; }

    // Setting
    public int? SetValue { get; set; }

    // Compound
    public List<CardEffectFormula> CompoundEffects { get; set; }
}
```

### Formula Examples

**Type A: Fixed**
```csharp
// Draw 2 cards
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Fixed,
    TargetResource = ConversationResourceType.Cards,
    BaseValue = 2
}
```

**Type B: Scaling**
```csharp
// Draw 1 card per 2 Statements in Spoken (max 4)
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Scaling,
    TargetResource = ConversationResourceType.Cards,
    ScalingSource = ScalingSourceType.TotalStatements,
    ScalingMultiplier = 0.5m,
    ScalingMax = 4
}
```

**Type C: Conditional**
```csharp
// If Doubt ≥ 5: +6 Initiative, else +2 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Conditional,
    TargetResource = ConversationResourceType.Initiative,
    ConditionResource = ConversationResourceType.Doubt,
    ConditionThreshold = 5,
    ConditionMetValue = 6,
    ConditionUnmetValue = 2
}
```

**Type D: Trading**
```csharp
// Consume 2 Momentum: -3 Doubt
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Trading,
    TargetResource = ConversationResourceType.Doubt,
    ConsumeResource = ConversationResourceType.Momentum,
    ConsumeAmount = 2,
    TradeRatio = -3
}
```

**Type E: Setting**
```csharp
// Set Cadence to -5
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Setting,
    TargetResource = ConversationResourceType.Cadence,
    SetValue = -5
}
```

**Compound**
```csharp
// Draw 1 card, +1 Initiative, +1 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
    }
}
```

### Scaling Sources

Available scaling sources for Type B formulas:

```csharp
public enum ScalingSourceType
{
    Doubt,                    // Current doubt level
    PositiveCadence,          // Positive cadence points
    NegativeCadence,          // Negative cadence points (absolute value)
    Momentum,                 // Current momentum
    Initiative,               // Current initiative
    MindCards,                // Cards in hand
    SpokenCards,              // Cards in Spoken pile
    DeckCards,                // Cards remaining in deck
    TotalStatements,          // All Statement cards played
    InsightStatements,        // Insight Statements played
    RapportStatements,        // Rapport Statements played
    AuthorityStatements,      // Authority Statements played
    CommerceStatements,       // Commerce Statements played
    CunningStatements         // Cunning Statements played
}
```

---

## Statement Requirements

### Tracking System

**ConversationSession** tracks Statement cards by stat type:

```csharp
public Dictionary<PlayerStatType, int> StatementCounts { get; set; } = new()
{
    { PlayerStatType.Insight, 0 },
    { PlayerStatType.Rapport, 0 },
    { PlayerStatType.Authority, 0 },
    { PlayerStatType.Commerce, 0 },
    { PlayerStatType.Cunning, 0 }
};

public int GetStatementCount(PlayerStatType stat)
public int GetTotalStatements()
public void IncrementStatementCount(PlayerStatType stat)
```

### Requirement Validation

**ConversationCard** validates requirements:

```csharp
public bool MeetsStatementRequirements(ConversationSession session)
{
    if (!RequiredStat.HasValue || RequiredStatements <= 0)
        return true; // No requirement

    int statementCount = session.GetStatementCount(RequiredStat.Value);
    return statementCount >= RequiredStatements;
}
```

### Requirement Thresholds

| Depth Tier | Requirement | Verisimilitude |
|------------|-------------|----------------|
| Foundation (1-2) | None | Always accessible building blocks |
| Standard (3-4) | 2-3 Statements | Light foundation needed |
| Advanced (5-6) | 5 Statements | Moderate specialization required |
| Master (7-8) | 8 Statements | Heavy commitment necessary |

### Hybrid Requirements (Rare)

Some Master cards require multiple stat types:

```json
{
  "id": "empathetic_conclusion",
  "requiredStat": "Insight",
  "requiredStatements": 5,
  "secondaryRequiredStat": "Rapport",
  "secondaryRequiredStatements": 4
}
```

Currently not implemented - would need code extension.

---

## Conversation Resources

### The Five Resources

**Initiative** (0-10+)
- Action economy within turn
- Spent to play cards
- ACCUMULATES between LISTEN actions (never resets)
- Generated by Cunning cards

**Momentum** (0-∞)
- Goal progress tracker
- Unlocks depth tiers (6/12/18)
- Required for goal cards (8/12/16)
- Generated primarily by Authority cards

**Doubt** (0-10)
- Failure timer
- Conversation ends at 10
- Reduced by Commerce cards
- Added by Authority cards

**Cadence** (-10 to +10)
- Conversation rhythm/balance
- Positive = Player dominating (adds doubt on LISTEN)
- Negative = NPC dominating (bonus cards on LISTEN)
- Managed by Rapport cards

**Cards** (hand/deck/spoken)
- Information/options available
- Draw count: 3 base + negative cadence bonus
- Generated by Insight cards

### Resource Interactions

**SPEAK Action**:
```
1. Pay Initiative cost
2. Cadence +1 (player speaking)
3. Apply card effects (via formula)
4. If Statement: Increment stat counter
5. Process persistence (Echo→hand, Statement→spoken)
```

**LISTEN Action**:
```
1. Clear all Doubt to 0
2. Reduce Momentum by Doubt cleared
3. If Cadence > 0: Add Cadence to Doubt (domination penalty)
4. Draw cards (3 + |negative cadence|)
5. Cadence -3 (listening reduces tension)
6. Check goal card activation (momentum thresholds)
```

---

## Tier Unlock System

### Momentum Thresholds

Depth tiers unlock at momentum thresholds:

| Tier | Depths | Momentum | Cards Unlocked |
|------|--------|----------|----------------|
| Tier 1 | 1-2 | 0 (always) | Foundation cards |
| Tier 2 | 3-4 | 6 | Standard cards |
| Tier 3 | 5-6 | 12 | Advanced cards |
| Tier 4 | 7-8 | 18 | Master cards |

### Stat Specialization Bonus

High stat levels provide depth bonuses:

| Stat Level | Depth Bonus | Example |
|------------|-------------|---------|
| 3-5 | +1 depth | Insight 4 → depths 1-5 accessible |
| 6-8 | +2 depth | Insight 7 → depths 1-6 accessible |
| 9-10 | +3 depth | Insight 10 → depths 1-8 accessible |

**Combined Example**:
```
Momentum: 12 (Tier 3 unlocked → depths 1-6 baseline)
Insight Level: 7 (+2 depth bonus)
Result: Insight cards depths 1-8 accessible (full range)
       Other stats depths 1-6 accessible (baseline only)
```

### Implementation

```csharp
public class ConversationSession
{
    public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 };

    public void CheckAndUnlockTiers()
    {
        if (CurrentMomentum >= 6) UnlockedTiers.Add(2);
        if (CurrentMomentum >= 12) UnlockedTiers.Add(3);
        if (CurrentMomentum >= 18) UnlockedTiers.Add(4);
    }

    public int GetUnlockedMaxDepth()
    {
        if (UnlockedTiers.Contains(4)) return 8;
        if (UnlockedTiers.Contains(3)) return 6;
        if (UnlockedTiers.Contains(2)) return 4;
        return 2;
    }
}
```

---

## Card Catalog

### CardEffectCatalog

Centralized catalog of all effect formulas organized by stat and depth:

```csharp
public static class CardEffectCatalog
{
    public static List<CardEffectFormula> GetEffectVariants(PlayerStatType stat, int depth)
    {
        return stat switch
        {
            PlayerStatType.Insight => GetInsightEffects(depth),
            PlayerStatType.Rapport => GetRapportEffects(depth),
            PlayerStatType.Authority => GetAuthorityEffects(depth),
            PlayerStatType.Commerce => GetCommerceEffects(depth),
            PlayerStatType.Cunning => GetCunningEffects(depth),
            _ => new List<CardEffectFormula>()
        };
    }

    public static CardEffectFormula GetEffectByVariantName(PlayerStatType stat, int depth, string variantName)
    {
        var variants = GetEffectVariants(stat, depth);
        return variants.FirstOrDefault(v => GetVariantName(v) == variantName)
            ?? variants.FirstOrDefault();
    }
}
```

### Effect Variants

Multiple effect variants available per stat/depth combination for content variety:

**Example: Insight Depth 3-4**
```csharp
private static List<CardEffectFormula> GetInsightEffects(int depth)
{
    return depth switch
    {
        3 or 4 => new List<CardEffectFormula>
        {
            // Variant "Base": Draw 3 cards
            new CardEffectFormula
            {
                FormulaType = EffectFormulaType.Fixed,
                TargetResource = ConversationResourceType.Cards,
                BaseValue = 3
            },
            // Variant "Scaling_TotalStatements": Draw per Statements
            new CardEffectFormula
            {
                FormulaType = EffectFormulaType.Scaling,
                TargetResource = ConversationResourceType.Cards,
                ScalingSource = ScalingSourceType.TotalStatements,
                ScalingMultiplier = 0.5m,
                ScalingMax = 4
            },
            // Variant "Compound": Draw 2, +1 Momentum
            new CardEffectFormula
            {
                FormulaType = EffectFormulaType.Compound,
                CompoundEffects = new List<CardEffectFormula>
                {
                    new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 },
                    new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
                }
            }
        }
    };
}
```

### Initiative Cost Curves

Each stat has characteristic costs based on efficiency:

```csharp
public static int GetInitiativeCost(PlayerStatType stat, int depth)
{
    return stat switch
    {
        PlayerStatType.Cunning => depth switch  // Cheapest (generates Initiative)
        {
            1 or 2 => 0,
            3 or 4 => 2,
            5 or 6 => 4,
            7 or 8 => 6,
            _ => 0
        },
        PlayerStatType.Rapport => depth switch  // Cheap (sustainable)
        {
            1 or 2 => 0,
            3 or 4 => 2,
            5 or 6 => 4,
            7 or 8 => 6,
            _ => 0
        },
        PlayerStatType.Insight => depth switch  // Moderate
        {
            1 or 2 => 0,
            3 or 4 => 3,
            5 or 6 => 5,
            7 or 8 => 7,
            _ => 0
        },
        PlayerStatType.Commerce => depth switch  // Expensive (safety costs)
        {
            1 or 2 => 1,
            3 or 4 => 4,
            5 or 6 => 6,
            7 or 8 => 8,
            _ => 0
        },
        PlayerStatType.Authority => depth switch  // Most expensive (power costs)
        {
            1 or 2 => 1,
            3 or 4 => 4,
            5 or 6 => 6,
            7 or 8 => 8,
            _ => 0
        },
        _ => 0
    };
}
```

---

## Parsing Pipeline

### JSON to Domain Objects

**JSON Structure**:
```json
{
  "id": "insight_notice_detail",
  "title": "Notice Detail",
  "dialogueText": "I see something interesting here...",
  "boundStat": "Insight",
  "depth": 2,
  "persistence": "Statement",
  "initiativeCost": 0,
  "requiredStat": null,
  "requiredStatements": 0,
  "effectVariant": "Base",
  "category": "Expression"
}
```

### ConversationCardParser

**Parser responsibility**: Transform JSON DTOs into strongly-typed domain objects.

```csharp
public class ConversationCardParser
{
    public List<ConversationCard> ParseCards(string jsonContent)
    {
        // 1. Deserialize JSON to DTOs
        var dto = JsonSerializer.Deserialize<ConversationCardPackageDTO>(jsonContent);

        // 2. For each card DTO, create domain object
        foreach (var cardDto in dto.Cards)
        {
            var card = new ConversationCard
            {
                Id = cardDto.Id,
                Title = cardDto.Title,
                DialogueText = cardDto.DialogueText,
                BoundStat = ParseStat(cardDto.BoundStat),
                Depth = ParseDepth(cardDto.Depth),
                Persistence = ParsePersistence(cardDto.Persistence),
                InitiativeCost = cardDto.InitiativeCost ?? 0,
                RequiredStat = ParseStat(cardDto.RequiredStat),
                RequiredStatements = cardDto.RequiredStatements ?? 0,
                Category = ParseCategory(cardDto.Category),

                // 3. Get effect formula from catalog
                EffectFormula = GetEffectFormula(cardDto)
            };

            cards.Add(card);
        }
    }

    private CardEffectFormula GetEffectFormula(CardDTO dto)
    {
        if (!dto.BoundStat.HasValue || !dto.Depth.HasValue)
            return null;

        string variant = dto.EffectVariant ?? "Base";
        return CardEffectCatalog.GetEffectByVariantName(
            dto.BoundStat.Value,
            (int)dto.Depth.Value,
            variant
        );
    }
}
```

**CRITICAL**: Parser must convert JSON to fully-typed domain objects. No `JsonElement` or `Dictionary<string, object>` in domain layer.

### Validation

Parser validates Foundation sustainability rules:

```csharp
// Foundation cards (depth 1-2) must be 70% Echo for sustainability
if (card.Depth <= 2)
{
    double echoRatio = foundationEchoCount / (double)foundationTotalCount;
    if (echoRatio < 0.7)
        throw new InvalidDataException("Foundation tier must be at least 70% Echo cards");
}

// ALL Initiative-generating cards MUST be Echo
if (card.EffectFormula?.GeneratesInitiative() == true && card.Persistence != PersistenceType.Echo)
    throw new InvalidDataException($"Card {card.Id} generates Initiative but is not Echo");
```

---

## Projection Principle

### The Core Principle

**PROJECTION PRINCIPLE**: The `CategoricalEffectResolver` is a pure projection function that returns what WOULD happen without modifying any game state.

Both UI (for preview display) and game logic (for actual execution) call the resolver to get projections.

The resolver NEVER modifies state directly - it only returns `CardEffectResult` projections that describe what changes would occur.

### CategoricalEffectResolver

```csharp
public class CategoricalEffectResolver
{
    private readonly TokenMechanicsManager tokenManager;
    private readonly GameWorld gameWorld;

    /// <summary>
    /// PROJECTION: Returns what WOULD happen on card success without modifying state.
    /// </summary>
    public CardEffectResult ProcessSuccessEffect(CardInstance card, ConversationSession session)
    {
        var result = new CardEffectResult
        {
            Card = card,
            InitiativeChange = 0,
            MomentumChange = 0,
            DoubtChange = 0,
            CadenceChange = 0,
            CardsToDraw = 0,
            CardsToAdd = new List<CardInstance>(),
            EffectDescription = "",
            EndsConversation = false
        };

        var effects = new List<string>();
        CardEffectFormula formula = card.ConversationCardTemplate?.EffectFormula;

        if (formula.FormulaType == EffectFormulaType.Compound)
        {
            foreach (var subFormula in formula.CompoundEffects)
                ApplyFormulaToResult(subFormula, session, result, effects);
        }
        else
        {
            ApplyFormulaToResult(formula, session, result, effects);
        }

        result.EffectDescription = string.Join(", ", effects);
        return result;
    }

    private void ApplyFormulaToResult(CardEffectFormula formula, ConversationSession session,
        CardEffectResult result, List<string> effects)
    {
        // Calculate effect value using formula
        int effectValue = formula.CalculateEffect(session);

        // Accumulate into result (NOT session state)
        switch (formula.TargetResource)
        {
            case ConversationResourceType.Initiative:
                result.InitiativeChange += effectValue;
                effects.Add($"+{effectValue} Initiative");
                break;
            case ConversationResourceType.Momentum:
                result.MomentumChange += effectValue;
                effects.Add($"+{effectValue} Momentum");
                break;
            case ConversationResourceType.Doubt:
                result.DoubtChange += effectValue;
                effects.Add($"{effectValue} Doubt");
                break;
            case ConversationResourceType.Cadence:
                result.CadenceChange += effectValue;
                effects.Add($"{effectValue} Cadence");
                break;
            case ConversationResourceType.Cards:
                result.CardsToDraw += effectValue;
                effects.Add($"Draw {effectValue} cards");
                break;
        }
    }
}
```

### CardEffectResult

Complete projection of all resource changes:

```csharp
public class CardEffectResult
{
    public CardInstance Card { get; set; }
    public int InitiativeChange { get; set; }
    public int MomentumChange { get; set; }
    public int DoubtChange { get; set; }
    public int CadenceChange { get; set; }
    public int CardsToDraw { get; set; }
    public List<CardInstance> CardsToAdd { get; set; }
    public bool EndsConversation { get; set; }
    public string EffectDescription { get; set; }
}
```

---

## Effect Execution

### Single Source of Truth

**ConversationFacade** gets projections from resolver, then applies them to session:

```csharp
private CardPlayResult ProcessInitiativeCardPlay(CardInstance selectedCard, bool success, ConversationSession session)
{
    if (success)
    {
        // Get projection from resolver (single source of truth)
        CardEffectResult projection = _effectResolver.ProcessSuccessEffect(selectedCard, session);

        // Apply projection to session state
        ApplyProjectionToSession(projection, session);
    }

    return new CardPlayResult { /* ... */ };
}
```

### ApplyProjectionToSession

**This is the ONLY place where projections become reality**:

```csharp
private void ApplyProjectionToSession(CardEffectResult projection, ConversationSession session)
{
    // Apply Initiative changes
    if (projection.InitiativeChange != 0)
    {
        session.AddInitiative(projection.InitiativeChange);
    }

    // Apply Momentum changes
    if (projection.MomentumChange != 0)
    {
        session.CurrentMomentum = Math.Max(0, session.CurrentMomentum + projection.MomentumChange);
        session.CheckAndUnlockTiers(); // Side effect: tier unlocks
    }

    // Apply Doubt changes
    if (projection.DoubtChange > 0)
        session.AddDoubt(projection.DoubtChange);
    else if (projection.DoubtChange < 0)
        session.ReduceDoubt(-projection.DoubtChange);

    // Apply Cadence changes
    if (projection.CadenceChange != 0)
    {
        session.Cadence = Math.Clamp(session.Cadence + projection.CadenceChange, -10, 10);
    }

    // Apply card draw
    if (projection.CardsToDraw > 0)
    {
        Player player = _gameWorld.GetPlayer();
        session.Deck.DrawToHand(projection.CardsToDraw, session, player.Stats);
    }

    // Add specific card instances (legacy support)
    if (projection.CardsToAdd != null && projection.CardsToAdd.Any())
    {
        session.Deck.AddCardsToMind(projection.CardsToAdd);
    }
}
```

### UI Preview Usage

UI gets same projection for preview display:

```csharp
// ConversationContent.razor.cs
protected string GetSuccessEffect(CardInstance card)
{
    // PROJECTION PRINCIPLE: Get projection without modifying state
    CardEffectResult projection = EffectResolver.ProcessSuccessEffect(card, Session);

    // Display projection to player
    return projection.EffectDescription;
}
```

### Architecture Benefits

1. **Single source of truth** for effect calculations
2. **No code duplication** between preview and execution
3. **UI can accurately preview** what will happen
4. **Game logic gets consistent** effect calculations
5. **No side effects** during preview operations

---

## Conversation Types

### Type Structure

```csharp
public class ConversationTypeEntry
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public int AttentionCost { get; init; }
    public int MaxInitiative { get; init; } = 10;
}
```

### Standard Types

**Request** - Player-initiated requests for NPC help
```json
{
  "id": "request",
  "name": "Request",
  "description": "Ask the NPC for assistance",
  "attentionCost": 2,
  "maxInitiative": 10
}
```

**Delivery** - Deliver letters from obligation queue
```json
{
  "id": "delivery",
  "name": "Delivery",
  "description": "Deliver a letter to this NPC",
  "attentionCost": 1,
  "maxInitiative": 10
}
```

**Friendly Chat** - Social conversations
```json
{
  "id": "friendly_chat",
  "name": "Friendly Chat",
  "description": "Casual conversation",
  "attentionCost": 1,
  "maxInitiative": 10
}
```

### Type-Specific Mechanics

Some conversation types have unique mechanics:

**Request Conversations**:
- Display requestText when started
- Request cards in RequestPile
- Activate when momentum thresholds met
- End conversation when request card played successfully

**Delivery Conversations**:
- Check obligation queue for letters to this NPC
- Special delivery cards generated
- Rewards on successful delivery

---

## Deck Management

### SessionCardDeck

**HIGHLANDER PRINCIPLE**: ONE deck manages ALL card state.

```csharp
public class SessionCardDeck
{
    // The four piles
    private List<CardInstance> DeckPile { get; set; }      // Undrawn cards
    private List<CardInstance> HandPile { get; set; }      // Active hand (Mind)
    private List<CardInstance> SpokenPile { get; set; }    // Played cards
    private List<CardInstance> RequestPile { get; set; }   // Goal cards awaiting activation

    // Read-only access
    public IReadOnlyList<CardInstance> HandCards => HandPile.AsReadOnly();
    public IReadOnlyList<CardInstance> SpokenCards => SpokenPile.AsReadOnly();
    public IReadOnlyList<CardInstance> RequestCards => RequestPile.AsReadOnly();

    public int HandSize => HandPile.Count;
    public int RemainingDeckCards => DeckPile.Count;
    public int SpokenPileCount => SpokenPile.Count;
    public int RequestPileSize => RequestPile.Count;
}
```

### Card Flow

**Initialization**:
```
ConversationDeckBuilder.CreateConversationDeck()
  → NPC conversation deck (by stat distribution)
  → Request cards (if applicable)
  → Returns (SessionCardDeck, List<RequestCards>)
```

**Drawing**:
```
DrawToHand(count, session, playerStats)
  1. Filter by tier (session.GetUnlockedMaxDepth())
  2. Filter by stat level (player specialization bonus)
  3. Shuffle filtered pool
  4. Draw count cards
  5. Add to HandPile
```

**Playing**:
```
PlayCard(card)
  1. Remove from HandPile
  2. Check persistence type:
     - Echo → return to HandPile (immediately reusable)
     - Statement → move to SpokenPile (counts toward requirements)
     - Persistent → stays in HandPile (never leaves)
     - Banish → removed entirely (not returned)
```

**Request Activation**:
```
CheckRequestThresholds(currentMomentum)
  1. Check each card in RequestPile
  2. If momentum >= card.MomentumThreshold:
     - Move from RequestPile to HandPile
     - Mark as IsPlayable = true
     - Notify player
```

### ConversationDeckBuilder

Builds decks based on NPC stat distributions and conversation types:

```csharp
public (SessionCardDeck deck, List<CardInstance> requestCards)
    CreateConversationDeck(NPC npc, string requestId, List<CardInstance> observationCards)
{
    // 1. Get NPC's conversation deck (by stat distribution)
    var npcCards = GetNPCConversationCards(npc);

    // 2. Get request cards (if applicable)
    var requestCards = GetRequestCards(npc, requestId);

    // 3. Create deck with cards
    var deck = new SessionCardDeck();
    deck.Initialize(npcCards, requestCards, observationCards);

    return (deck, requestCards);
}

private List<ConversationCard> GetNPCConversationCards(NPC npc)
{
    // NPC has ConversationStatDistribution
    // Example: { Insight: 40%, Rapport: 30%, Authority: 20%, Commerce: 10% }

    var cards = new List<ConversationCard>();

    foreach (var (stat, percentage) in npc.ConversationStatDistribution)
    {
        int cardCount = (int)(TOTAL_DECK_SIZE * percentage);
        var statCards = GetCardsForStat(stat, cardCount, npc.Tier);
        cards.AddRange(statCards);
    }

    return cards;
}
```

### Deck Size & Distribution

**Total Deck**: ~30-40 cards per conversation
**Distribution Example** (Investigation type):
```
Insight: 40% (12 cards)
  - Depth 1-2: 6 cards (3 Echo, 3 Statement)
  - Depth 3-4: 4 cards (2 Echo, 2 Statement)
  - Depth 5-6: 2 cards (1 Echo, 1 Statement)

Cunning: 30% (9 cards)
  - Depth 1-2: 5 cards (4 Echo, 1 Statement)
  - Depth 3-4: 3 cards (2 Echo, 1 Statement)
  - Depth 5-6: 1 card (Echo)

Rapport: 20% (6 cards)
  - Depth 1-2: 4 cards (3 Echo, 1 Statement)
  - Depth 3-4: 2 cards (1 Echo, 1 Statement)

Other: 10% (3 cards)
  - Mixed depths and stats
```

---

## Summary

### Data Flow

```
JSON Files
  ↓
ConversationCardParser
  ↓
CardEffectCatalog (formulas)
  ↓
ConversationCard (domain objects)
  ↓
ConversationDeckBuilder
  ↓
SessionCardDeck
  ↓
ConversationSession (tracking)
  ↓
CategoricalEffectResolver (projection)
  ↓
ConversationFacade (execution)
  ↓
ApplyProjectionToSession (state mutation)
```

### Key Principles

1. **Projection Principle**: Resolver projects, Facade applies
2. **Single Source of Truth**: All effects from CardEffectCatalog
3. **No Code Duplication**: Preview and execution use same projection
4. **Highlander Principle**: ONE deck manages all cards
5. **Statement Requirements**: Logical progression through conversation
6. **Tier Unlocks**: Momentum gates powerful cards
7. **Stat Specialization**: High stat levels unlock deeper cards
8. **Perfect Information**: All effects calculable and visible

### Architecture Benefits

- **Maintainable**: All formulas in one catalog
- **Testable**: Pure projection functions
- **Predictable**: Perfect information for players
- **Extensible**: Easy to add new formulas/cards
- **Verisimilitudinous**: Requirements match real conversation logic