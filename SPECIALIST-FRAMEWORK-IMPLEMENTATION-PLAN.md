# Specialist Framework Implementation Plan

## Overview

This document provides a complete implementation plan for the Specialist with Universal Access framework across the entire Wayfarer codebase: from JSON content files through parsers, domain models, calculation logic, and UI display.

## Core Principle Recap

**Old System**: Hard 1:1 stat-to-resource mapping (Authority-only generates Momentum, Insight-only draws cards)

**New System**: Specialist with Universal Access
- Each stat SPECIALIZES in one resource (2-3x efficiency)
- All stats can ACCESS universal resources (Momentum/Initiative) at 1x rate
- Enables all deck compositions to reach goals while maintaining stat identity

## Implementation Roadmap

### Phase 1: Update CardEffectCatalog.cs (Code Changes)

**File**: `C:\Git\Wayfarer\src\GameState\CardEffectCatalog.cs`

**Current State**: Uses hard 1:1 mapping with simple fixed effects per stat and depth.

**Required Changes**: Update all effect formulas to use compound effects with specialist + universal + secondary resources.

#### 1.1 Insight Effects (Specialist: Cards | Universal: Momentum/Initiative)

**Foundation (Depth 1-2)**:
```csharp
// Type A: Draw 2 cards, +1 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
    }
}
```

**Standard (Depth 3-4)**:
```csharp
// Type A: Draw 3 cards, +2 Momentum, +1 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
    }
}
```

**Advanced (Depth 5-6)**:
```csharp
// Type A: Draw 4 cards, +3 Momentum, +2 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 4 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
    }
}
```

**Master (Depth 7-8)**:
```csharp
// Type A: Draw 6 cards, +5 Momentum, +3 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 6 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 }
    }
}
```

#### 1.2 Rapport Effects (Specialist: Cadence | Universal: Momentum/Initiative)

**Foundation (Depth 1-2)**:
```csharp
// Type A: -1 Cadence, +1 Momentum, +1 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -1 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 }
    }
}
```

**Standard (Depth 3-4)**:
```csharp
// Type A: -2 Cadence, +2 Momentum, +2 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 }
    }
}
```

**Advanced (Depth 5-6)**:
```csharp
// Type A: -3 Cadence, +3 Momentum, +3 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cadence, BaseValue = -3 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 }
    }
}
```

**Master (Depth 7-8)**:
```csharp
// Type A: Set Cadence to -5, +8 Momentum, +5 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.StateSetting, TargetResource = ConversationResourceType.Cadence, BaseValue = -5 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 5 }
    }
}
```

#### 1.3 Authority Effects (Specialist: Momentum | Universal: Initiative | Trade-off: Doubt)

**Foundation (Depth 1-2)**:
```csharp
// Type A: +2 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Fixed,
    TargetResource = ConversationResourceType.Momentum,
    BaseValue = 2
},

// Type B: +2 Momentum, +1 Doubt
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 1 }
    }
}
```

**Standard (Depth 3-4)**:
```csharp
// Type A: +5 Momentum, +1 Initiative, +2 Doubt
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 1 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 2 }
    }
}
```

**Advanced (Depth 5-6)**:
```csharp
// Type A: +8 Momentum, +2 Initiative, +3 Doubt
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 8 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 3 }
    }
}
```

**Master (Depth 7-8)**:
```csharp
// Type A: +12 Momentum, +3 Initiative, +4 Doubt
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 12 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 3 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = 4 }
    }
}
```

#### 1.4 Diplomacy Effects (Specialist: Doubt | Universal: Momentum | Trading)

**Foundation (Depth 1-2)**:
```csharp
// Type A: -1 Doubt
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Fixed,
    TargetResource = ConversationResourceType.Doubt,
    BaseValue = -1
},

// Type B: -1 Doubt, +1 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -1 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
    }
}
```

**Standard (Depth 3-4)**:
```csharp
// Type A: -2 Doubt, +2 Momentum, Consume 2 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, BaseValue = -2 }
    }
}
```

**Advanced (Depth 5-6)**:
```csharp
// Type A: -4 Doubt, +3 Momentum, Consume 3 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Doubt, BaseValue = -4 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, BaseValue = -3 }
    }
}
```

**Master (Depth 7-8)**:
```csharp
// Type A: Set Doubt to 0, +5 Momentum, Consume 4 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.StateSetting, TargetResource = ConversationResourceType.Doubt, BaseValue = 0 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
        new() { FormulaType = EffectFormulaType.Trading, TargetResource = ConversationResourceType.Momentum, BaseValue = -4 }
    }
}
```

#### 1.5 Cunning Effects (Specialist: Initiative | Universal: Momentum | Secondary: Cards)

**Foundation (Depth 1-2)**:
```csharp
// Type A: +2 Initiative
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Fixed,
    TargetResource = ConversationResourceType.Initiative,
    BaseValue = 2
},

// Type B: +2 Initiative, +1 Momentum
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 1 }
    }
}
```

**Standard (Depth 3-4)**:
```csharp
// Type A: +4 Initiative, +2 Momentum, Draw 1 card
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 4 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 2 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 1 }
    }
}
```

**Advanced (Depth 5-6)**:
```csharp
// Type A: +6 Initiative, +3 Momentum, Draw 2 cards
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 6 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 3 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 2 }
    }
}
```

**Master (Depth 7-8)**:
```csharp
// Type A: +10 Initiative, +5 Momentum, Draw 3 cards
new CardEffectFormula
{
    FormulaType = EffectFormulaType.Compound,
    CompoundEffects = new List<CardEffectFormula>
    {
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Initiative, BaseValue = 10 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Momentum, BaseValue = 5 },
        new() { FormulaType = EffectFormulaType.Fixed, TargetResource = ConversationResourceType.Cards, BaseValue = 3 }
    }
}
```

### Phase 2: Update 02_cards.json (Content Changes)

**File**: `C:\Git\Wayfarer\src\Content\Core\02_cards.json`

**Current State**: 60 cards with existing effect formulas that may not match the specialist framework.

**Required Changes**: Review all 60 cards and update their assigned effect formulas to match the new specialist patterns.

#### 2.1 Card Review Process

For each card in 02_cards.json:

1. **Identify Stat & Depth**: Check the card's `statType` and `depth` properties
2. **Find Matching Formula**: Locate the appropriate formula from CardEffectCatalog based on stat/depth
3. **Verify Effect Assignment**: Ensure the card uses the correct effect formula from the catalog
4. **Update if Needed**: If the card has a manually specified effect that doesn't match the catalog pattern, update it

#### 2.2 Example Card Updates

**Before (Old Pattern)**:
```json
{
  "id": "insight_notice_detail",
  "name": "Notice Detail",
  "statType": "Insight",
  "depth": 2,
  "persistence": "Statement",
  "initiativeCost": 0,
  "effects": [
    {
      "formulaType": "Fixed",
      "targetResource": "Cards",
      "baseValue": 2
    }
  ]
}
```

**After (New Pattern)**:
```json
{
  "id": "insight_notice_detail",
  "name": "Notice Detail",
  "statType": "Insight",
  "depth": 2,
  "persistence": "Statement",
  "initiativeCost": 0,
  "effects": [
    {
      "formulaType": "Compound",
      "compoundEffects": [
        {
          "formulaType": "Fixed",
          "targetResource": "Cards",
          "baseValue": 2
        },
        {
          "formulaType": "Fixed",
          "targetResource": "Momentum",
          "baseValue": 1
        }
      ]
    }
  ]
}
```

**IMPORTANT**: If cards use `useDefaultEffect: true`, they will automatically pull from CardEffectCatalog and no JSON changes are needed. Only update cards with explicitly defined effects.

### Phase 3: Update Deck Compositions (Content Changes)

**File**: `C:\Git\Wayfarer\src\Content\Core\02_cards.json` (cardDecks section)

**Current State**: Three conversation type decks (desperate_request, trade_negotiation, friendly_chat) with specific card distributions.

**Required Changes**: Review deck compositions to ensure they can generate Momentum to reach goals.

#### 3.1 desperate_request Deck

**Current Issue**: "Heavy Rapport/Insight, zero Authority" meant NO Authority cards, thus no Momentum specialists.

**Fix**: Keep the Rapport/Insight focus but ensure Momentum generation through:
- Rapport cards now generate Momentum (via universal access)
- Insight cards now generate Momentum (via universal access)
- Can still include 1-2 Authority cards if needed for faster progression

**Verification**: Test that deck can reach 8/12/16 Momentum goals within reasonable turn counts.

#### 3.2 trade_negotiation Deck

**Current Issue**: Needs to balance Momentum generation (Authority) with Doubt reduction (Diplomacy).

**Fix**: Already includes both Authority and Diplomacy. Verify that:
- Authority cards provide Momentum + Initiative (not just Momentum + Doubt)
- Diplomacy cards provide Momentum while reducing Doubt
- Deck has sustainable Momentum generation

#### 3.3 friendly_chat Deck

**Current Issue**: Balanced across all 5 stats, should have natural Momentum flow.

**Fix**: Verify all stats contribute to Momentum generation at appropriate rates.

### Phase 4: Parser Updates (Validation Changes)

**File**: `C:\Git\Wayfarer\src\Content\ConversationCardParser.cs`

**Current State**: Validates cards have effects and match expected patterns.

**Required Changes**: Update validation to allow compound effects with multiple resources.

#### 4.1 Update GeneratesInitiative() Method

**Current Implementation**: Already recursively checks compound effects (added earlier in conversation).

**Verification Needed**: Ensure it correctly handles nested compound effects with both Initiative and Momentum.

#### 4.2 Add Validation for Specialist Framework

Add new validation rule to verify cards follow specialist patterns:

```csharp
private static void ValidateSpecialistFramework(List<ConversationCardTemplate> cards, List<ValidationMessage> messages)
{
    foreach (var card in cards)
    {
        if (card.Effects == null || !card.Effects.Any()) continue;

        var primaryEffect = card.Effects.First();

        // Verify compound effects for depth 3+
        if (card.Depth >= 3 && primaryEffect.FormulaType != EffectFormulaType.Compound)
        {
            messages.Add(ValidationMessage.Warning(
                $"Card '{card.ID}' at depth {card.Depth} should use Compound effects for specialist + universal resources"
            ));
        }

        // Verify specialist resource matches stat type
        if (primaryEffect.FormulaType == EffectFormulaType.Compound && primaryEffect.CompoundEffects != null)
        {
            var specialistResource = GetExpectedSpecialistResource(card.StatType);
            var hasSpecialist = primaryEffect.CompoundEffects.Any(e => e.TargetResource == specialistResource);

            if (!hasSpecialist)
            {
                messages.Add(ValidationMessage.Warning(
                    $"Card '{card.ID}' ({card.StatType}) should include specialist resource '{specialistResource}'"
                ));
            }

            // Verify universal resources (Momentum/Initiative) are present for depth 3+
            if (card.Depth >= 3)
            {
                var hasMomentumOrInitiative = primaryEffect.CompoundEffects.Any(e =>
                    e.TargetResource == ConversationResourceType.Momentum ||
                    e.TargetResource == ConversationResourceType.Initiative
                );

                if (!hasMomentumOrInitiative)
                {
                    messages.Add(ValidationMessage.Warning(
                        $"Card '{card.ID}' at depth {card.Depth} should include universal resources (Momentum or Initiative)"
                    ));
                }
            }
        }
    }
}

private static ConversationResourceType GetExpectedSpecialistResource(StatType stat)
{
    return stat switch
    {
        StatType.Insight => ConversationResourceType.Cards,
        StatType.Rapport => ConversationResourceType.Cadence,
        StatType.Authority => ConversationResourceType.Momentum,
        StatType.Diplomacy => ConversationResourceType.Doubt,
        StatType.Cunning => ConversationResourceType.Initiative,
        _ => throw new ArgumentException($"Unknown stat type: {stat}")
    };
}
```

Call this from the main validation method:
```csharp
ValidateSpecialistFramework(cards, messages);
```

### Phase 5: Domain Model Updates (No Changes Required)

**Files**:
- `C:\Git\Wayfarer\src\Domain\Conversation\CardEffectFormula.cs`
- `C:\Git\Wayfarer\src\Domain\Conversation\ConversationCardTemplate.cs`

**Status**: ✅ No changes needed.

**Reason**: The existing domain models already support:
- `EffectFormulaType.Compound` for multi-resource effects
- `CompoundEffects` list for nested effects
- All necessary `ConversationResourceType` values (Momentum, Initiative, Cards, Cadence, Doubt)

### Phase 6: Calculation Logic Updates (No Changes Required)

**Files**:
- `C:\Git\Wayfarer\src\Services\Conversation\ConversationEngine.cs`
- `C:\Git\Wayfarer\src\Services\Conversation\CardEffectResolver.cs`

**Status**: ✅ No changes needed.

**Reason**: The effect resolution system already:
- Processes compound effects recursively
- Applies each sub-effect in sequence
- Handles all resource types (Momentum, Initiative, Cards, Cadence, Doubt)
- Supports all formula types (Fixed, Trading, StateSetting, Compound)

### Phase 7: UI Updates (Display Text Changes)

**Files**:
- `C:\Git\Wayfarer\src\Pages\ConversationContent.razor`
- `C:\Git\Wayfarer\src\Services\UI\CardDisplayTextGenerator.cs` (if exists)

**Current State**: Card effect text displays simple effects like "+2 Momentum" or "Draw 2 cards".

**Required Changes**: Update effect text generation to show compound effects clearly.

#### 7.1 Update Card Effect Display

**Current Display**:
```
+2 Momentum
```

**New Display**:
```
Draw 2 cards, +1 Momentum
```

**Implementation**: Update the text generation logic to:

```csharp
public static string GetEffectDisplayText(CardEffectFormula effect)
{
    if (effect.FormulaType == EffectFormulaType.Compound && effect.CompoundEffects != null)
    {
        var parts = effect.CompoundEffects
            .Select(e => GetSingleEffectText(e))
            .Where(text => !string.IsNullOrEmpty(text));

        return string.Join(", ", parts);
    }

    return GetSingleEffectText(effect);
}

private static string GetSingleEffectText(CardEffectFormula effect)
{
    return effect.TargetResource switch
    {
        ConversationResourceType.Cards => $"Draw {effect.BaseValue} card{(effect.BaseValue != 1 ? "s" : "")}",
        ConversationResourceType.Momentum => FormatResourceChange("Momentum", effect.BaseValue),
        ConversationResourceType.Initiative => FormatResourceChange("Initiative", effect.BaseValue),
        ConversationResourceType.Cadence => FormatResourceChange("Cadence", effect.BaseValue),
        ConversationResourceType.Doubt => FormatResourceChange("Doubt", effect.BaseValue),
        _ => ""
    };
}

private static string FormatResourceChange(string resourceName, int value)
{
    if (value > 0) return $"+{value} {resourceName}";
    if (value < 0) return $"{value} {resourceName}";
    return "";
}
```

#### 7.2 Update Conversation Type Descriptions

**File**: `C:\Git\Wayfarer\src\Content\Core\05_gameplay.json`

**Current Descriptions**:
```json
{
  "id": "desperate_request",
  "description": "Someone in crisis needs help - heavy Rapport/Insight, zero Authority"
}
```

**New Descriptions**:
```json
{
  "id": "desperate_request",
  "description": "Someone in crisis needs help - heavy Rapport/Insight focus (both generate Momentum via empathy and analysis)"
}
```

Update all conversation type descriptions to remove "zero Authority" language and emphasize specialist focus instead of exclusivity.

### Phase 8: Testing & Verification

#### 8.1 Unit Tests

Create tests for:

1. **CardEffectCatalog Tests**: Verify each stat/depth combination returns compound effects with correct resources
2. **Effect Resolution Tests**: Verify compound effects apply all sub-effects correctly
3. **Validation Tests**: Verify specialist framework validation catches missing universal resources

#### 8.2 Integration Tests

Test complete card play scenarios:

1. **Insight Card Play**: Verify playing Insight depth-4 card draws 3 cards AND adds 2 Momentum AND adds 1 Initiative
2. **Rapport Card Play**: Verify Rapport cards reduce Cadence AND generate Momentum AND generate Initiative
3. **Authority Card Play**: Verify Authority cards generate Momentum AND Initiative AND Doubt
4. **Diplomacy Card Play**: Verify Diplomacy cards reduce Doubt AND generate Momentum (with trading)
5. **Cunning Card Play**: Verify Cunning cards generate Initiative AND Momentum AND draw cards

#### 8.3 E2E Playwright Tests

Test actual gameplay:

1. **Zero-Authority Deck**: Start conversation with desperate_request deck, verify can reach 8 Momentum goal through Rapport/Insight cards alone
2. **Momentum Tracking**: Verify UI shows Momentum increasing from non-Authority cards
3. **Effect Display**: Verify card tooltips show all compound effects (e.g., "Draw 3 cards, +2 Momentum, +1 Initiative")

Example test:
```csharp
[Test]
public async Task DesperateRequest_CanReachGoalWithoutAuthority()
{
    // Start conversation with Elena (desperate_request type)
    await NavigateTo("/");
    await ClickNPC("elena");
    await ClickConversationType("FriendlyChat");

    // Play only Rapport and Insight cards
    int momentum = 0;
    while (momentum < 8)
    {
        // Find a Rapport or Insight card in hand
        var card = await FindCardByStats(["Rapport", "Insight"]);
        Assert.IsNotNull(card, "Should have Rapport or Insight cards available");

        // Play the card
        await PlayCard(card);

        // Verify Momentum increased
        var newMomentum = await GetMomentumValue();
        Assert.Greater(newMomentum, momentum, "Momentum should increase from Rapport/Insight cards");
        momentum = newMomentum;

        // If we need more cards, LISTEN
        if (await GetHandSize() == 0)
        {
            await ClickListen();
        }
    }

    // Verify we reached goal
    Assert.GreaterOrEqual(momentum, 8, "Should reach Basic Goal (8 Momentum) with Rapport/Insight only");
}
```

## Implementation Checklist

- [ ] **Phase 1**: Update CardEffectCatalog.cs with all new compound effect formulas (5 stats × 4 depth tiers)
- [ ] **Phase 2**: Review and update 02_cards.json card effects if needed (60 cards)
- [ ] **Phase 3**: Verify deck compositions can reach goals (3 conversation types)
- [ ] **Phase 4**: Add specialist framework validation to ConversationCardParser.cs
- [ ] **Phase 5**: Verify domain models (no changes needed)
- [ ] **Phase 6**: Verify calculation logic (no changes needed)
- [ ] **Phase 7**: Update UI effect display text generation
- [ ] **Phase 8**: Create and run unit tests
- [ ] **Phase 9**: Create and run integration tests
- [ ] **Phase 10**: Create and run E2E Playwright tests
- [ ] **Phase 11**: Update conversation type descriptions in 05_gameplay.json
- [ ] **Phase 12**: Full playthrough test of all 3 conversation types

## Risk Assessment

### Low Risk
- Domain model updates (none needed)
- Calculation logic (already supports compound effects)
- UI display (simple text generation change)

### Medium Risk
- Card effect updates in JSON (60 cards to review, but most use default effects)
- Validation updates (new rules, may catch unexpected issues)

### High Risk
- CardEffectCatalog changes (20 effect formulas × 4 depth tiers = 80+ formula objects to update)
- Deck composition balance (may need multiple iterations to feel right)

### Mitigation Strategies
1. **Update CardEffectCatalog incrementally**: One stat at a time, test after each
2. **Use feature flags**: Add config flag to enable/disable specialist framework for testing
3. **Comprehensive test coverage**: Write tests BEFORE changing CardEffectCatalog
4. **Backup current state**: Commit all current JSON before changes
5. **Parallel comparison**: Keep old CardEffectCatalog.cs as CardEffectCatalog.Old.cs for reference

## Estimated Timeline

- **Phase 1** (CardEffectCatalog): 3-4 hours (careful, high-stakes)
- **Phase 2** (JSON cards): 1-2 hours (mostly review, few actual changes)
- **Phase 3** (Deck compositions): 30 minutes (verification)
- **Phase 4** (Validation): 1 hour (new validation rules)
- **Phase 7** (UI display): 1 hour (text generation)
- **Phase 8-10** (Testing): 2-3 hours (comprehensive test suite)
- **Phase 11-12** (Polish & playthrough): 1-2 hours

**Total**: 9-13 hours of focused implementation work

## Success Criteria

1. ✅ All 60 cards show compound effects in UI (specialist + universal + secondary)
2. ✅ desperate_request conversation can reach 8 Momentum without Authority cards
3. ✅ All validation passes with no errors
4. ✅ E2E tests pass showing Momentum generation from all stats
5. ✅ CardEffectCatalog has 20 formulas × 4 depth tiers with proper scaling (2x → 2.5x → 3x → 3-4x)
6. ✅ Deck compositions feel balanced and can reach all goal tiers (8/12/16)
7. ✅ UI clearly displays "Draw 3 cards, +2 Momentum, +1 Initiative" style effect text

## Post-Implementation

After implementation is complete and tested:

1. **Remove Old Code**: Delete any backup files or commented-out old patterns
2. **Update Architecture Docs**: If any architectural assumptions changed, update ARCHITECTURE.md
3. **Create Migration Guide**: Document the before/after for future reference
4. **Performance Check**: Verify no performance regression from compound effect processing
5. **User Testing**: Playthrough by actual players to verify the feel is correct

---

## Notes from User's Detailed Framework Explanation

From the user's latest message, key concrete examples to implement:

### Foundation (Depth 1-2) - Exact Values

```
Authority: +2 Momentum, +1 Doubt
Insight: Draw 2 cards, +1 Momentum
Rapport: -1 Cadence, +1 Momentum, +1 Initiative
Diplomacy: -1 Doubt, +1 Momentum
Cunning: +2 Initiative, +1 Momentum
```

### Standard (Depth 3-4) - Exact Values

```
Authority: +5 Momentum, +2 Doubt, +1 Initiative
Insight: Draw 3 cards, +2 Momentum, +1 Initiative
Rapport: -2 Cadence, +2 Momentum, +2 Initiative
Diplomacy: -2 Doubt, +2 Momentum, Consume 2 Momentum
Cunning: +4 Initiative, +2 Momentum, Draw 1 card
```

### Advanced (Depth 5-6) - Exact Values

```
Authority: +8 Momentum, +3 Doubt, +2 Initiative
Insight: Draw 4 cards, +3 Momentum, +2 Initiative
Rapport: -3 Cadence, +3 Momentum, +3 Initiative
Diplomacy: -4 Doubt, +3 Momentum, Consume 3 Momentum
Cunning: +6 Initiative, +3 Momentum, Draw 2 cards
```

These exact values should be used when implementing Phase 1 (CardEffectCatalog updates).