# Conversation System Implementation Plan

## Overview

This document outlines the complete implementation of the redesigned conversation system inspired by Steamworld Quest's builder/spender mechanics. The new system features:

- **Initiative-based action economy** (replacing Focus, starts at 0 like Steamworld Quest steam)
- **Cadence conversation balance tracking** (-10 to +10)
- **Stat-gated depth access** for cards (depths 1-10)
- **Builder/spender card architecture** with alternative costs
- **Extended player progression** system (stats 1-8+)
- **Four distinct resources** with unique mechanical identities

## Core Design Principles

1. **Each Resource Has ONE Identity**: No overlap between Initiative, Momentum, Doubt, and Cadence (4 resources total)
2. **Visible State Only**: All scaling effects use visible game state, no hidden tracking
3. **Natural Progression**: Stats unlock deeper conversation tools maintaining verisimilitude
4. **Builder/Spender Dynamic**: Foundation cards enable Decisive cards through Initiative economy
5. **Save Compatibility**: Maintain existing save data while transitioning to new system

## Phase 1: Core Models & Data Structures

### 1.1 ConversationSession Model Updates

**File**: `src/GameState/ConversationSession.cs`

**Changes**:
```csharp
// Replace Focus with Initiative (starts at 0, no max like Steamworld Quest)
public int CurrentInitiative { get; set; } // Starts at 0, built through cards
public int Cadence { get; set; } // Range -10 to +10

// Remove old Focus AND Connection State properties
// public int CurrentFocus { get; set; } // DELETE
// public int MaxFocus { get; set; } // DELETE
// public ConnectionState CurrentState { get; set; } // DELETE
// public int FlowBattery { get; set; } // DELETE (replaced by Cadence)

// Add doubt tax calculation
public int GetEffectiveMomentumGain(int baseMomentum)
{
    decimal reduction = CurrentDoubt * 0.20m;
    return (int)(baseMomentum * (1 - reduction));
}

// Add cadence effects (corrected mechanics)
public bool ShouldApplyCadenceDoubtPenalty() => Cadence >= 6;
public int GetCadenceDoubtPenalty() => Math.Max(0, Cadence - 5);
public bool ShouldApplyCadenceBonusDraw() => Cadence <= -3;
```

**Migration Strategy**:
- Add new properties with default values
- Create migration method to convert existing saves
- Maintain backward compatibility during transition

### 1.2 ConversationCard & CardTemplate Model Updates

**Files**:
- `src/GameState/Enums/CardCategoricalProperties.cs`
- `src/GameState/ConversationCard.cs`
- `src/Subsystems/Conversation/Models/CardTemplate.cs`

**New Enums**:
```csharp
public enum CardDepth
{
    Depth1 = 1, Depth2 = 2, Depth3 = 3, // Foundation
    Depth4 = 4, Depth5 = 5, Depth6 = 6, // Standard
    Depth7 = 7, Depth8 = 8, Depth9 = 9, Depth10 = 10 // Decisive
}

public enum PersistenceType
{
    Standard,    // Goes to Spoken pile when played
    Echo,        // Returns to hand after playing if conditions met
    Persistent,  // Stays in hand until forcibly discarded
    Banish       // Removes itself from conversation entirely
}
```

**CardTemplate Updates**:
```csharp
public class CardTemplate
{
    // New properties
    public CardDepth Depth { get; set; }
    public int InitiativeCost { get; set; }
    public List<AlternativeCost> AlternativeCosts { get; set; }
    public PersistenceType Persistence { get; set; }
    public ScalingFormula ScalingEffect { get; set; }

    // Remove old properties
    // public int FocusCost { get; set; } // DELETE
}

public class AlternativeCost
{
    public string Condition { get; set; } // "Cadence >= 5", "Doubt >= 7"
    public int ReducedInitiativeCost { get; set; }
    public int MomentumCost { get; set; }
    public string Description { get; set; }
}

public class ScalingFormula
{
    public string ScalingType { get; set; } // "Cadence", "Doubt", "SpokeCards", "Momentum"
    public int BaseEffect { get; set; }
    public decimal Multiplier { get; set; }
    public string Formula { get; set; } // Human-readable formula
}
```

### 1.3 PlayerStats Model Extension

**File**: `src/GameState/PlayerStats.cs`

**Updates**:
```csharp
public class PlayerStats
{
    // Extend XP requirements
    private static readonly Dictionary<int, int> XP_REQUIREMENTS = new()
    {
        [1] = 10, [2] = 25, [3] = 50, [4] = 100,
        [5] = 175, [6] = 275, [7] = 400 // Extended progression
    };

    // Add depth access methods
    public int GetMaxAccessibleDepth(PlayerStatType stat)
    {
        return GetLevel(stat);
    }

    public bool CanAccessCardDepth(PlayerStatType stat, CardDepth depth)
    {
        return GetLevel(stat) >= (int)depth;
    }

    // Update success bonus calculation
    public decimal GetSuccessBonus(PlayerStatType stat)
    {
        return GetLevel(stat) * 0.10m; // 10% per level
    }
}
```

## Phase 2: JSON Content Migration

### 2.1 Card Template Migration

**Files**: `src/Content/Core/01_cards.json`, `02_cards.json`, `03_cards.json`

**Migration Tasks**:

1. **All Card Effects Must Be Redesigned** for 4-Resource System:

**Foundation Cards (0 Initiative Cost - Always Playable)**:
```json
{
  "Id": "rapport_active_listening",
  "Depth": 1,
  "InitiativeCost": 0,
  "BoundStat": "Rapport",
  "Effects": {
    "Success": {
      "Initiative": 2
    }
  }
},
{
  "Id": "insight_quick_observation",
  "Depth": 1,
  "InitiativeCost": 0,
  "BoundStat": "Insight",
  "Effects": {
    "Success": {
      "Initiative": 1,
      "DrawCards": 1
    }
  }
}
```

**Standard Cards (Medium Initiative Cost)**:
```json
{
  "Id": "commerce_fair_trade",
  "Depth": 4,
  "InitiativeCost": 3,
  "BoundStat": "Commerce",
  "Effects": {
    "Success": {
      "Momentum": 2,
      "Initiative": 1
    }
  }
}
```

**Decisive Cards (High Initiative Cost)**:
```json
{
  "Id": "authority_commanding_presence",
  "Depth": 7,
  "InitiativeCost": 8,
  "BoundStat": "Authority",
  "Effects": {
    "Success": {
      "Momentum": 6,
      "Doubt": -2
    }
  }
}
```

2. **Each Stat Needs 0-Initiative Foundation Cards**:
```json
{
  "Id": "commerce_quick_trade",
  "InitiativeCost": 0,
  "BoundStat": "Commerce",
  "Effects": {
    "Success": {
      "Momentum": 1,
      "Initiative": 1
    }
  }
},
{
  "Id": "cunning_deflection",
  "InitiativeCost": 0,
  "BoundStat": "Cunning",
  "Effects": {
    "Success": {
      "Initiative": 2,
      "Doubt": -1
    }
  }
},
{
  "Id": "authority_direct_statement",
  "InitiativeCost": 0,
  "BoundStat": "Authority",
  "Effects": {
    "Success": {
      "Initiative": 1,
      "Momentum": 2,
      "Doubt": 1
    }
  }
}
```

3. **Scaling Effects Must Use Only 4 Resources** (Initiative, Cadence, Momentum, Doubt):
```json
{
  "Id": "rapport_perfect_understanding",
  "Depth": 10,
  "InitiativeCost": 10,
  "BoundStat": "Rapport",
  "Effects": {
    "Success": {
      "MomentumMultiplier": 2.0,
      "Doubt": -999
    }
  },
  "ScalingEffect": {
    "ScalingType": "Cadence",
    "Formula": "+1 Initiative per negative Cadence point"
  }
},
{
  "Id": "cunning_perfect_timing",
  "Depth": 9,
  "InitiativeCost": 9,
  "BoundStat": "Cunning",
  "Effects": {
    "Success": {
      "Momentum": "2 + Doubt"
    }
  },
  "ScalingEffect": {
    "ScalingType": "Doubt",
    "Formula": "More effective when under pressure"
  }
}
```

### 2.1.1 Allowed Card Effects in 4-Resource System

**Available Effects** (all others must be removed):
- **Initiative**: Generate or spend Initiative points
- **Momentum**: Gain momentum toward goals (subject to doubt tax)
- **Doubt**: Increase or decrease doubt pressure
- **Cadence**: Modify conversation balance (rare, most cards just trigger normal ±1/±3)
- **DrawCards**: Draw additional cards from deck
- **MomentumMultiplier**: Scale existing momentum (×2, ×1.5, etc.)

**REMOVED Effects** (delete all references):
- Flow generation/spending (replaced by Cadence)
- Connection State transitions (deleted system)
- Focus restoration/modification (replaced by Initiative)
- MaxFocus effects (no max Initiative)
- Any Connection State bonuses

### 2.2 Conversation Deck Restructuring

**Files**: Various conversation type JSON files

**Organization Strategy**:
- **Depth 1-3**: 60% of deck (Foundation cards)
- **Depth 4-6**: 30% of deck (Standard cards)
- **Depth 7-10**: 10% of deck (Decisive cards)

**Example Conversation Type**:
```json
{
  "ConversationType": "FriendlyChat",
  "Cards": {
    "Depths1-3": ["friendly_greeting", "active_listening", "show_interest"],
    "Depths4-6": ["thoughtful_response", "deep_connection", "emotional_support"],
    "Depths7-10": ["perfect_understanding", "breakthrough_moment"]
  }
}
```

## Phase 3: Game Logic Implementation

### 3.1 ConversationFacade Complete Rewrite

**File**: `src/Subsystems/Conversation/ConversationFacade.cs`

**New Methods**:
```csharp
public class ConversationFacade
{
    // Initiative Management
    public bool CanPlayCard(ConversationCard card)
    {
        return session.CurrentInitiative >= card.GetEffectiveInitiativeCost() ||
               HasAlternativeCostAvailable(card);
    }

    public bool PayCardCost(ConversationCard card, AlternativeCost alternativeCost = null)
    {
        if (alternativeCost != null)
        {
            return PayAlternativeCost(card, alternativeCost);
        }

        if (session.CurrentInitiative >= card.InitiativeCost)
        {
            session.CurrentInitiative -= card.InitiativeCost;
            return true;
        }
        return false;
    }

    // Cadence System (corrected mechanics)
    public void ApplyCadenceFromCardPlay()
    {
        session.Cadence -= 1; // -1 per card played (corrected)
    }

    public void ApplyCadenceFromListen()
    {
        session.Cadence += 3; // +3 per LISTEN (corrected)
    }

    public void ProcessCadenceEffectsOnListen()
    {
        if (session.ShouldApplyCadenceDoubtPenalty())
        {
            int penalty = session.GetCadenceDoubtPenalty();
            session.CurrentDoubt += penalty;
        }
    }

    // Doubt Tax Implementation
    public int ApplyDoubtTax(int baseMomentum)
    {
        return session.GetEffectiveMomentumGain(baseMomentum);
    }
}
```

### 3.2 Stat-Gated Depth Filtering

**File**: `src/GameFacade.cs`

**New Methods**:
```csharp
public List<CardTemplate> GetAccessibleCards(string conversationType)
{
    var allCards = GetConversationTypeCards(conversationType);
    var playerStats = GetPlayerStats();

    return allCards.Where(card =>
        playerStats.CanAccessCardDepth(card.BoundStat, card.Depth)
    ).ToList();
}

public ConversationDeck BuildDeckForPlayer(string conversationType)
{
    var accessibleCards = GetAccessibleCards(conversationType);
    return new ConversationDeck
    {
        Cards = accessibleCards,
        MaxDepthByStats = CalculateMaxDepths(GetPlayerStats())
    };
}
```

### 3.3 LISTEN Action Implementation

**Method**: `ConversationFacade.ExecuteListen()`

**Sequence**:
```csharp
public async Task ExecuteListen()
{
    // 1. Apply Cadence Effects
    ProcessCadenceEffectsOnListen();
    ApplyCadenceFromListen();

    // 2. Apply Doubt Tax on Momentum (handled in card effects)

    // 3. Handle Card Persistence
    ProcessCardPersistence();

    // 4. Calculate Card Draw
    int cardsToDraw = CalculateCardDraw();

    // 5. NO Initiative refresh (must be earned through cards like Steamworld Quest)

    // 6. Check Goal Card Activation
    CheckGoalCardActivation();

    // 7. Draw Cards
    await DrawCards(cardsToDraw);
}

private int CalculateCardDraw()
{
    int baseDraw = 4; // Fixed number (no Connection State)
    int cadenceBonus = session.ShouldApplyCadenceBonusDraw() ? 1 : 0;

    return baseDraw + cadenceBonus;
}
```

### 3.4 SPEAK Action Implementation

**Method**: `ConversationFacade.ExecuteSpeak(ConversationCard card)`

**Sequence**:
```csharp
public async Task<bool> ExecuteSpeak(ConversationCard card)
{
    // 1. Validate Initiative/Alternative Costs
    if (!CanPlayCard(card)) return false;

    // 2. Check Personality Restrictions
    if (!ValidatePersonalityRules(card)) return false;

    // 3. Pay Card Cost
    if (!PayCardCost(card)) return false;

    // 4. Decrease Cadence (corrected)
    ApplyCadenceFromCardPlay();

    // 5. Calculate Success
    bool success = await CalculateSuccess(card);

    // 6. Process Results
    if (success)
    {
        await ProcessSuccess(card);
    }
    else
    {
        ProcessFailure(card);
    }

    // 7. Handle Card Persistence
    ProcessCardAfterPlay(card);

    return success;
}
```

## Phase 4: UI Implementation

### 4.1 ConversationContent.razor Updates

**Key Changes**:

1. **Replace Focus Display with Initiative (no max, starts at 0)**:
```html
<div class="initiative-display-section">
    <div class="initiative-content">
        <span class="initiative-label">INITIATIVE</span>
        <div class="initiative-counter">
            <span class="initiative-value">@Session.CurrentInitiative</span>
        </div>
        <span class="initiative-info">Build with Foundation cards</span>
    </div>
</div>
```

2. **Add Cadence Meter**:
```html
<div class="cadence-display">
    <span class="cadence-label">Conversation Balance</span>
    <div class="cadence-meter">
        <div class="cadence-track">
            <div class="cadence-indicator" style="left: @GetCadencePosition()%"></div>
        </div>
        <span class="cadence-value">@Session.Cadence</span>
    </div>
</div>
```

3. **Update Card Display for Depth**:
```html
<div class="card @GetCardDepthClass(convCard) @GetCardNarrativeClass(convCard)">
    <div class="card-depth-marker">@GetDepthDisplayName(convCard.Depth)</div>
    <!-- existing card content -->
    @if (HasAlternativeCosts(convCard))
    {
        <div class="alternative-costs">
            @foreach (var altCost in GetAvailableAlternativeCosts(convCard))
            {
                <div class="alt-cost-option" @onclick="() => PlayCardWithAltCost(convCard, altCost)">
                    @altCost.Description
                </div>
            }
        </div>
    }
</div>
```

### 4.2 CSS Updates

**File**: `src/wwwroot/css/conversation.css`

**New Styles**:
```css
/* Initiative Display */
.initiative-display-section {
    /* Similar to focus display but distinct styling */
}

/* Cadence Meter */
.cadence-display {
    display: flex;
    align-items: center;
    gap: 10px;
}

.cadence-meter {
    position: relative;
    width: 200px;
    height: 20px;
    background: #333;
    border-radius: 10px;
}

.cadence-track {
    width: 100%;
    height: 100%;
    position: relative;
    background: linear-gradient(to right, #ff6b6b 0%, #ffd93d 50%, #6bcf7f 100%);
}

.cadence-indicator {
    position: absolute;
    top: 0;
    width: 4px;
    height: 100%;
    background: white;
    border-radius: 2px;
    transition: left 0.3s ease;
}

/* Depth-based Card Styling */
.card.depth-foundation {
    border-left: 4px solid #4CAF50; /* Green for Foundation */
}

.card.depth-standard {
    border-left: 4px solid #2196F3; /* Blue for Standard */
}

.card.depth-decisive {
    border-left: 4px solid #FF9800; /* Orange for Decisive */
}

.card-depth-marker {
    position: absolute;
    top: 5px;
    right: 5px;
    font-size: 10px;
    font-weight: bold;
    color: rgba(255, 255, 255, 0.7);
}

/* Alternative Costs */
.alternative-costs {
    margin-top: 10px;
    padding: 5px;
    background: rgba(0, 0, 0, 0.2);
    border-radius: 3px;
}

.alt-cost-option {
    cursor: pointer;
    padding: 3px 6px;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 2px;
    font-size: 11px;
    margin: 2px 0;
    transition: background 0.2s;
}

.alt-cost-option:hover {
    background: rgba(255, 255, 255, 0.2);
}
```

## Phase 5: Testing Strategy

### 5.1 Unit Tests
- Initiative calculation accuracy
- Cadence tracking correctness
- Doubt tax application
- Alternative cost validation
- Stat-gated depth filtering

### 5.2 Integration Tests
- Full LISTEN action sequence
- Full SPEAK action sequence
- Personality rule enforcement
- Save/load compatibility

### 5.3 User Experience Tests
- Early game progression (stats 1-2)
- Mid game complexity (stats 3-5)
- Late game mastery (stats 6-8+)
- Builder/spender satisfaction

## Migration & Compatibility

### Save File Migration
```csharp
public static ConversationSession MigrateLegacySession(LegacyConversationSession legacy)
{
    return new ConversationSession
    {
        // Convert Focus to Initiative (but start at 0 like Steamworld Quest)
        CurrentInitiative = 0, // Always start at 0, regardless of old Focus

        // Initialize new properties
        Cadence = 0,

        // Preserve existing data
        CurrentMomentum = legacy.CurrentMomentum,
        CurrentDoubt = legacy.CurrentDoubt,
        // ... other properties
    };
}
```

### Gradual Rollout Strategy
1. **Phase 1**: Backend models and JSON (invisible to users)
2. **Phase 2**: Game logic with fallback support
3. **Phase 3**: UI updates with feature flags
4. **Phase 4**: Full activation after thorough testing

## Updated Formulas for 4-Resource System

### Initiative System (Steamworld Quest Style)
- **Starting Initiative**: 0 (always, regardless of stats/connection)
- **Initiative Generation**: Only through 0-cost Foundation cards
- **No Initiative Refresh**: Must be earned each turn through card play
- **No Maximum**: Initiative can accumulate without limit

### Cadence System (Replaces Flow)
- **Range**: -10 to +10
- **SPEAK Effect**: Cadence -= 1 (corrected mechanic)
- **LISTEN Effect**: Cadence += 3 (corrected mechanic)
- **High Cadence Penalty**: If Cadence ≥ 6, +1 Doubt per point above 5 on LISTEN
- **Low Cadence Bonus**: If Cadence ≤ -3, +1 card draw on LISTEN

### Card Draw (Fixed System)
- **Base Cards per LISTEN**: 4 (fixed, no Connection State modifier)
- **Cadence Bonus**: +1 if Cadence ≤ -3
- **Total Formula**: 4 + Cadence Bonus

### Success Calculations
- **Card Success Rate**: Base% + (2% × Current Momentum) + (10% × Bound Stat Level)
- **Doubt Tax**: Momentum gains reduced by 20% per Doubt point
- **Example**: At 3 Doubt, momentum gains reduced by 60%

### Resource Limits and Ranges
- **Initiative**: 0 to unlimited
- **Cadence**: -10 to +10
- **Momentum**: 0 to unlimited (goal thresholds at 8/12/16)
- **Doubt**: 0 to 10 (conversation ends at 10)

### Personality Rule Updates
- **Proud**: Cards must be played in ascending Initiative order (not Focus)
- **Mercantile**: Highest Initiative card gets +30% success (not highest Focus)
- **Cunning**: Same Initiative cost as previous card costs -2 momentum
- **Devoted**: +1 additional Doubt on failure
- **Steadfast**: All momentum changes capped at ±2

### Card Cost Structure
- **Foundation (Depth 1-3)**: 0-2 Initiative, often generate Initiative
- **Standard (Depth 4-6)**: 3-5 Initiative, balanced effects
- **Decisive (Depth 7-10)**: 6-12 Initiative, powerful effects requiring buildup

## Success Criteria

### Technical Success
- [ ] All tests passing
- [ ] No performance regression
- [ ] Save compatibility maintained
- [ ] Clean code architecture

### Gameplay Success
- [ ] Builder/spender dynamic feels satisfying
- [ ] Stat progression creates meaningful choices
- [ ] Resource management is strategic but not overwhelming
- [ ] Conversation flow feels natural and engaging

### User Experience Success
- [ ] UI clearly communicates new systems
- [ ] Learning curve is manageable
- [ ] Advanced players have meaningful depth
- [ ] Visual feedback supports understanding

## Timeline Estimate

- **Phase 1**: 3-4 days (Models & JSON)
- **Phase 2**: 4-5 days (Game Logic)
- **Phase 3**: 2-3 days (UI Implementation)
- **Phase 4**: 2-3 days (Testing & Polish)

**Total**: ~12-15 days for complete implementation

## Risk Mitigation

### High Risk Areas
- **Save Compatibility**: Implement thorough migration testing
- **Performance**: Profile stat filtering and depth calculations
- **User Experience**: Conduct early user testing of UI changes
- **Complexity**: Provide clear visual feedback for new systems

### Rollback Plan
- Maintain feature flags for easy rollback
- Keep legacy code paths during transition period
- Comprehensive testing before each phase deployment

---

This implementation plan transforms Wayfarer's conversation system into a deep, strategic experience while maintaining the game's design principles and existing player experience.