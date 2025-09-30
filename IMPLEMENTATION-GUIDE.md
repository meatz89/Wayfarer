# Card System Implementation Guide
## From Specification to Working Implementation

This guide shows the complete pipeline from the COMPLETE-CARD-LIBRARY-SPECIFICATION.md through to working gameplay.

---

## Pipeline Overview

```
JSON Cards → Parser → CardEffectCatalog → Domain Models → Session Logic → UI Display
```

---

## 1. JSON Card Definition

**Location**: `src/Content/Core/02_cards.json`

**Example** (from INSIGHT-CARDS-REFERENCE.json):

```json
{
  "id": "insight_pattern_synthesis",
  "title": "Pattern Synthesis",
  "dialogueText": "Everything we've discussed reveals a clear pattern...",
  "type": "Conversation",
  "boundStat": "Insight",
  "depth": 3,
  "persistence": "Echo",
  "initiativeCost": 2,
  "category": "Realization",
  "requiredStat": "Insight",
  "requiredStatements": 3,
  "effectVariant": "Signature",
  "personalityTypes": ["ALL"]
}
```

**Key Fields**:
- `boundStat`: Which stat this card belongs to (Insight/Rapport/Authority/Commerce/Cunning)
- `depth`: 1-8, determines when accessible via tier unlocks
- `persistence`: Echo (repeatable) or Statement (one-time)
- `initiativeCost`: How much Initiative required to play
- `requiredStat` + `requiredStatements`: For signature cards only (null/0 for base cards)
- `effectVariant`: "Base" or "Signature"

**Critical Rules**:
- Foundation (depth 1-2): `requiredStatements` MUST be 0 or null
- Standard+ (depth 3+): Can have `requiredStatements` of 3, 4, 5, or 8
- `requiredStat` must match `boundStat` for signature cards

---

## 2. CardEffectCatalog

**Location**: `src/GameState/CardEffectCatalog.cs`

**Purpose**: Central definition of all card effects organized by stat and depth.

**Structure**:
```csharp
private static List<CardEffectFormula> GetInsightEffects(int depth)
{
    return depth switch
    {
        1 => new List<CardEffectFormula>
        {
            // Index 0: "Quick Scan" - Base Echo
            new CardEffectFormula { ... },
            // Index 1: "Ask Question" - Base Statement
            new CardEffectFormula { ... }
        },
        3 => new List<CardEffectFormula>
        {
            // Index 0: "Connect Evidence" - Base Echo
            new CardEffectFormula { ... },
            // Index 1: "Cross-Reference" - Base Statement
            new CardEffectFormula { ... },
            // Index 2: "Pattern Synthesis" - Signature Echo
            new CardEffectFormula { ... }
        },
        // ... more depths
    };
}
```

**Effect Formula Types**:

1. **Fixed**: Static value
   ```csharp
   new CardEffectFormula
   {
       FormulaType = EffectFormulaType.Fixed,
       TargetResource = ConversationResourceType.Cards,
       BaseValue = 2  // Draw 2 cards
   }
   ```

2. **Scaling**: Linear scaling from game state
   ```csharp
   new CardEffectFormula
   {
       FormulaType = EffectFormulaType.Scaling,
       TargetResource = ConversationResourceType.Cards,
       ScalingSource = ScalingSourceType.InsightStatements,
       ScalingMultiplier = 1.0m,
       ScalingMax = 4  // Draw 1 card per Insight Statement (max 4)
   }
   ```

3. **Compound**: Multiple effects in sequence
   ```csharp
   new CardEffectFormula
   {
       FormulaType = EffectFormulaType.Compound,
       CompoundEffects = new List<CardEffectFormula>
       {
           new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
           new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 }
       }
   }
   ```

4. **Trading**: Consume resource A to gain resource B
   ```csharp
   new CardEffectFormula
   {
       FormulaType = EffectFormulaType.Trading,
       TargetResource = ConversationResourceType.Doubt,
       TradeRatio = -3,
       ConsumeResource = ConversationResourceType.Momentum,
       ConsumeAmount = 2  // Consume 2 Momentum: -3 Doubt
   }
   ```

5. **Setting**: Set resource to absolute value
   ```csharp
   new CardEffectFormula
   {
       FormulaType = EffectFormulaType.Setting,
       TargetResource = ConversationResourceType.Cadence,
       SetValue = 0  // Set Cadence to 0
   }
   ```

---

## 3. ConversationCardParser

**Location**: `src/Content/ConversationCardParser.cs`

**Flow**:
1. Read JSON DTO
2. Parse categorical properties (stat, depth, persistence)
3. Look up effect formula from CardEffectCatalog
4. Create ConversationCard domain model

**Key Logic** (lines 196-219):
```csharp
if (cardType == CardType.Conversation && boundStat.HasValue)
{
    string variantName = dto.EffectVariant ?? "Base";

    if (variantName == "Base")
    {
        var variants = CardEffectCatalog.GetEffectVariants(boundStat.Value, (int)depth);
        effectFormula = variants.FirstOrDefault();
    }
    else
    {
        effectFormula = CardEffectCatalog.GetEffectByVariantName(boundStat.Value, (int)depth, variantName);
    }
}
```

**Validation Rules** (to be added):
```csharp
// Foundation tier must have NO signature variants
if (card.Depth <= 2 && card.RequiredStatements > 0)
    throw new InvalidDataException($"Foundation card {card.Id} cannot have Statement requirements");

// Signature cards must start at Standard tier (depth 3+)
if (card.RequiredStatements > 0 && card.Depth < 3)
    throw new InvalidDataException($"Signature card {card.Id} must be depth 3 or higher");

// Statement requirements must match valid thresholds
var validThresholds = new[] { 3, 4, 5, 8 };
if (card.RequiredStatements > 0 && !validThresholds.Contains(card.RequiredStatements))
    throw new InvalidDataException($"Card {card.Id} has invalid requirement: {card.RequiredStatements}");
```

---

## 4. ConversationSession (Statement Tracking)

**Location**: `src/GameState/ConversationSession.cs`

**Statement Counter** (lines 42-49):
```csharp
public Dictionary<PlayerStatType, int> StatementCounts { get; set; } = new Dictionary<PlayerStatType, int>
{
    { PlayerStatType.Insight, 0 },
    { PlayerStatType.Rapport, 0 },
    { PlayerStatType.Authority, 0 },
    { PlayerStatType.Commerce, 0 },
    { PlayerStatType.Cunning, 0 }
};
```

**Increment Logic** (lines 233-246):
```csharp
public void IncrementStatementCount(PlayerStatType stat)
{
    if (StatementCounts.ContainsKey(stat))
    {
        StatementCounts[stat]++;
    }
    else
    {
        StatementCounts[stat] = 1;
    }

    Console.WriteLine($"Statement count for {stat}: {StatementCounts[stat]} (Total: {GetTotalStatements()})");
}
```

**Requirement Check** (ConversationCard.cs lines 167-176):
```csharp
public bool MeetsStatementRequirements(ConversationSession session)
{
    if (!RequiredStat.HasValue || RequiredStatements <= 0)
    {
        return true; // No requirements
    }

    int statementCount = session.GetStatementCount(RequiredStat.Value);
    return statementCount >= RequiredStatements;
}
```

---

## 5. SessionCardDeck (Tier + Statement Filtering)

**Location**: To be created/updated in `src/GameState/SessionCardDeck.cs`

**Filtering Logic** (to implement):

```csharp
public List<CardInstance> GetPlayableCards(ConversationSession session, int playerStatLevel)
{
    int currentTier = GetTierFromMomentum(session.CurrentMomentum);
    int maxDepth = GetMaxDepthForTier(currentTier);

    // Apply stat specialization bonus for specialized stat
    // (This logic depends on player's specialized stat)

    return allCards.Where(card =>
    {
        // Check tier/depth access
        if ((int)card.ConversationCardTemplate.Depth > maxDepth)
            return false;

        // Check Statement requirements
        if (!card.ConversationCardTemplate.MeetsStatementRequirements(session))
            return false;

        // Check Initiative cost
        if (card.ConversationCardTemplate.InitiativeCost > session.CurrentInitiative)
            return false;

        return true;
    }).ToList();
}

private int GetTierFromMomentum(int momentum)
{
    if (momentum >= 18) return 4; // Master
    if (momentum >= 12) return 3; // Advanced
    if (momentum >= 6) return 2;  // Standard
    return 1; // Foundation
}

private int GetMaxDepthForTier(int tier)
{
    return tier * 2; // Tier 1 = depth 2, Tier 2 = depth 4, etc.
}
```

---

## 6. UI Display

### Statement Counter Component

**Location**: To be added to ConversationContent.razor

```razor
<div class="statement-counter">
    <h4>Statement Progress</h4>
    @foreach (var stat in new[] {
        PlayerStatType.Insight,
        PlayerStatType.Rapport,
        PlayerStatType.Authority,
        PlayerStatType.Commerce,
        PlayerStatType.Cunning
    })
    {
        var count = Session.GetStatementCount(stat);
        <div class="statement-bar">
            <span class="stat-name">@stat:</span>
            <div class="progress-bar">
                @for (int i = 0; i < 8; i++)
                {
                    <span class="pip @(i < count ? "filled" : "empty")">█</span>
                }
            </div>
            <span class="count">(@count/8)</span>
        </div>
    }
</div>
```

### Card Display with Lock Indicator

**Location**: CardDisplay component or inline in conversation UI

```razor
@if (!card.ConversationCardTemplate.MeetsStatementRequirements(Session))
{
    <div class="card locked-signature">
        <div class="card-header">
            <span class="card-title">@card.ConversationCardTemplate.Title</span>
            <span class="lock-icon">🔒</span>
        </div>
        <div class="requirement-not-met">
            Requires: @card.ConversationCardTemplate.RequiredStatements @card.ConversationCardTemplate.RequiredStat Statements
            <br/>
            Have: @Session.GetStatementCount(card.ConversationCardTemplate.RequiredStat.Value)
        </div>
        <div class="card-effect">
            @GetEffectDescription(card)
        </div>
    </div>
}
else if (card.ConversationCardTemplate.RequiredStatements > 0)
{
    <div class="card signature-available">
        <div class="card-header">
            <span class="card-title">@card.ConversationCardTemplate.Title</span>
            <span class="signature-icon">⭐</span>
        </div>
        <div class="requirement-met">
            ✓ Signature Available
        </div>
        <div class="card-effect">
            @GetEffectDescription(card)
        </div>
    </div>
}
else
{
    <div class="card base">
        <div class="card-header">
            <span class="card-title">@card.ConversationCardTemplate.Title</span>
        </div>
        <div class="card-effect">
            @GetEffectDescription(card)
        </div>
    </div>
}
```

### CSS Styling

**Location**: wwwroot/css/conversation.css

```css
.statement-counter {
    background: rgba(0, 0, 0, 0.3);
    padding: 1rem;
    border-radius: 8px;
    margin-bottom: 1rem;
}

.statement-bar {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin: 0.5rem 0;
}

.progress-bar {
    flex: 1;
    display: flex;
    gap: 2px;
}

.pip {
    font-size: 0.8rem;
}

.pip.filled {
    color: #4CAF50;
}

.pip.empty {
    color: #333;
}

.card.locked-signature {
    opacity: 0.6;
    border: 2px solid #ff4444;
    background: rgba(255, 68, 68, 0.1);
}

.card.signature-available {
    border: 2px solid #FFD700;
    background: rgba(255, 215, 0, 0.1);
    box-shadow: 0 0 10px rgba(255, 215, 0, 0.3);
}

.card.base {
    border: 2px solid #666;
}

.lock-icon {
    font-size: 1.2rem;
    color: #ff4444;
}

.signature-icon {
    font-size: 1.2rem;
    color: #FFD700;
    animation: pulse 2s infinite;
}

@keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.5; }
}

.requirement-not-met {
    color: #ff4444;
    font-size: 0.9rem;
    padding: 0.5rem;
    background: rgba(255, 68, 68, 0.2);
    border-radius: 4px;
    margin: 0.5rem 0;
}

.requirement-met {
    color: #4CAF50;
    font-weight: bold;
    padding: 0.5rem;
    background: rgba(76, 175, 80, 0.2);
    border-radius: 4px;
    margin: 0.5rem 0;
}
```

---

## 7. Effect Execution

**Location**: `src/Subsystems/Conversation/CategoricalEffectResolver.cs`

The effect resolver already handles all formula types. For Statement cards, we just need to increment the counter after successful play:

```csharp
// In ConversationFacade.ExecuteSpeak() or similar
if (card.ConversationCardTemplate.Persistence == PersistenceType.Statement &&
    card.ConversationCardTemplate.BoundStat.HasValue)
{
    session.IncrementStatementCount(card.ConversationCardTemplate.BoundStat.Value);
}
```

---

## 8. Testing

### Unit Tests

**Test Statement Tracking**:
```csharp
[Test]
public void StatementCounter_IncrementsCorrectly()
{
    var session = new ConversationSession();

    session.IncrementStatementCount(PlayerStatType.Insight);
    session.IncrementStatementCount(PlayerStatType.Insight);
    session.IncrementStatementCount(PlayerStatType.Rapport);

    Assert.AreEqual(2, session.GetStatementCount(PlayerStatType.Insight));
    Assert.AreEqual(1, session.GetStatementCount(PlayerStatType.Rapport));
    Assert.AreEqual(3, session.GetTotalStatements());
}
```

**Test Signature Card Filtering**:
```csharp
[Test]
public void SignatureCard_BlockedWithoutRequirements()
{
    var session = new ConversationSession();
    var card = new ConversationCard
    {
        RequiredStat = PlayerStatType.Insight,
        RequiredStatements = 3
    };

    Assert.IsFalse(card.MeetsStatementRequirements(session));

    session.IncrementStatementCount(PlayerStatType.Insight);
    session.IncrementStatementCount(PlayerStatType.Insight);
    session.IncrementStatementCount(PlayerStatType.Insight);

    Assert.IsTrue(card.MeetsStatementRequirements(session));
}
```

### E2E Tests (Playwright)

```csharp
[Test]
public async Task PlayConversation_SignatureCardsUnlockWithStatements()
{
    // Start conversation
    await NavigateTo("/");
    await StartConversationWithElena();

    // Verify Statement counter is visible
    await Expect(Page.Locator(".statement-counter")).ToBeVisibleAsync();

    // Play 3 Insight Statement cards
    await PlayCard("Quick Scan"); // Base Echo
    await ClickListen();
    await PlayCard("Ask Question"); // Base Statement - count: 1
    await ClickListen();
    await PlayCard("Notice Detail"); // Base Statement - count: 2
    await ClickListen();
    await PlayCard("Cross-Reference"); // Base Statement - count: 3
    await ClickListen();

    // Verify Insight counter shows 3
    var insightCount = await Page.Locator(".statement-bar:has-text('Insight') .count").TextContentAsync();
    Assert.That(insightCount, Does.Contain("3"));

    // Verify Pattern Synthesis (3+ requirement) is now unlocked
    var patternSynthesis = Page.Locator(".card:has-text('Pattern Synthesis')");
    await Expect(patternSynthesis).Not.ToHaveClassAsync("locked-signature");
    await Expect(patternSynthesis).ToHaveClassAsync("signature-available");

    // Verify it has the signature icon
    await Expect(patternSynthesis.Locator(".signature-icon")).ToBeVisibleAsync();
}
```

---

## 9. Implementation Checklist

- [x] Update CardEffectCatalog with all Insight formulas (9 base + 3 signature)
- [ ] Add parser validation for Foundation signature ban
- [ ] Create complete JSON cards for Insight (reference created)
- [ ] Update SessionCardDeck tier + Statement filtering
- [ ] Add Statement counter UI component
- [ ] Add locked/signature card styling
- [ ] Implement Statement increment on card play
- [ ] Write unit tests for Statement tracking
- [ ] Write E2E test for signature unlock flow
- [ ] Complete remaining stats (Rapport, Authority, Commerce, Cunning)

---

## 10. Next Steps

1. **Validation**: Add parser rules to enforce Foundation has no signatures
2. **Filtering**: Implement SessionCardDeck playable card filtering
3. **UI**: Add Statement counter and card lock indicators
4. **Testing**: Write comprehensive tests for the pipeline
5. **Content**: Complete JSON cards for all 5 stats (60 cards total)

The architecture is now in place. The remaining work is:
- **Code**: Parser validation + deck filtering + UI components (~200 lines)
- **Content**: 48 more JSON cards (Rapport, Authority, Commerce, Cunning)
- **Testing**: Unit + E2E tests (~300 lines)

**Total implementation**: ~500 lines of code + 48 JSON cards.