# Conversation System Refactor Implementation Plan

## Overview

This document outlines the complete implementation plan for refactoring Wayfarer's conversation system according to the specifications in `conversation-system-refactor.md`. The refactor transforms the system from stat-gated card access to momentum-based access with stat specialization bonuses.

## Current State Analysis

### Existing Implementation
- **4-Resource System**: Initiative, Momentum, Doubt, and Cadence already implemented
- **Card Depth System**: CardDepth enum (Depth1-10) exists
- **Persistence Types**: Statement and Echo types defined
- **SessionCardDeck**: Managing Deck, Mind (hand), and Spoken piles
- **ConversationFacade**: Main service handling conversation operations

### Key Issues to Address
1. **Static stat gates** create rigid progression
2. **Persistent doubt** creates death spirals
3. **Threshold-based cadence** creates optimization targets
4. **Foundation depletion** can cause deadlocks
5. **Uniform deck distributions** make all conversation types feel the same

## Implementation Phases

### Phase 1: Core System Changes (CRITICAL)

#### 1.1 LISTEN Action Refactor
**Objective**: Replace persistent doubt with clearable-but-costly system

**Files Modified**:
- `ConversationFacade.cs::ExecuteListen()`
- `ConversationSession.cs::ApplyCadenceFromListen()`

**Changes**:
```csharp
// OLD: Doubt persists with minimal reduction
if (session.CurrentDoubt > 0)
    session.ReduceDoubt(1);

// NEW: Complete doubt reset, momentum reduction
int doubtCleared = session.CurrentDoubt;
session.CurrentDoubt = 0;
session.CurrentMomentum = Math.Max(0, session.CurrentMomentum - doubtCleared);
```

**Formula Updates**:
- Doubt: Always resets to 0 on LISTEN
- Momentum: Reduced by amount of doubt cleared
- Cadence: Reduced by 3 (was 2)
- Card Draw: 3 + abs(min(cadence, 0))

#### 1.2 Momentum-Based Draw Pool
**Objective**: Dynamic card access based on conversation state

**Files Modified**:
- `SessionCardDeck.cs::DrawToHand()`
- `ConversationDeckBuilder.cs::FilterCardsByMomentum()`

**Logic**:
```csharp
bool CanAccessCard(ConversationCard card, int momentum, PlayerStats stats)
{
    // Basic momentum gate
    if (card.Depth <= momentum) return true;

    // Stat specialization bonus
    if (card.BoundStat.HasValue)
    {
        int statBonus = GetStatDepthBonus(stats.GetLevel(card.BoundStat.Value));
        return card.Depth <= momentum + statBonus;
    }

    return false;
}
```

#### 1.3 Foundation Card Echo Classification
**Objective**: Prevent Initiative generation depletion

**Files Modified**:
- `02_cards.json` - Update all Foundation card definitions
- `ConversationCardParser.cs` - Add validation rules

**Rules**:
- 70% of depth 1-2 cards must be Echo type
- ALL Initiative-generating cards must be Echo type
- 30% can be Statement for variety

### Phase 2: Stat Specialization System (HIGH)

#### 2.1 Stat Depth Bonuses
**Objective**: Preserve character identity without hard gates

**Files Modified**:
- `PlayerStats.cs` - Add GetDepthBonus() method
- `ConversationFacade.cs` - Apply bonuses to accessibility

**Bonus Progression**:
- Levels 1-3: +0 depth bonus
- Levels 4-6: +1 depth bonus
- Levels 7-9: +2 depth bonus
- Level 10: +3 depth bonus

**Implementation**:
```csharp
public int GetDepthBonus(PlayerStatType statType)
{
    int level = GetLevel(statType);
    return level switch
    {
        >= 10 => 3,
        >= 7 => 2,
        >= 4 => 1,
        _ => 0
    };
}
```

#### 2.2 Linear Cadence Scaling
**Objective**: Remove arbitrary thresholds for smooth progression

**Files Modified**:
- `ConversationSession.cs::GetDrawCount()`

**Formula Change**:
```csharp
// OLD: Threshold at -3
int GetDrawCount()
{
    int base = 3;
    return Cadence <= -3 ? base + 1 : base;
}

// NEW: Linear scaling
int GetDrawCount()
{
    int base = 3;
    int bonus = Math.Abs(Math.Min(0, Cadence));
    return base + bonus;
}
```

### Phase 3: Content Rebalancing (MEDIUM)

#### 3.1 Starting Momentum Formula
**Objective**: Meaningful but non-breaking stat progression reward

**Files Modified**:
- `ConversationFacade.cs::StartConversation()`

**Formula**:
```csharp
int startingMomentum = 2 + (playerStats.GetHighestStatLevel() / 3);
```

**Progression**:
- Stats 1-2: Start at momentum 2
- Stats 3-5: Start at momentum 3
- Stats 6-8: Start at momentum 4
- Stats 9-10: Start at momentum 5

#### 3.2 Card Depth Distribution by Type
**Objective**: Create mechanically distinct conversation types

**Files Modified**:
- `01_foundation.json` - Update conversation type definitions
- `ConversationDeckBuilder.cs` - Apply distribution rules

**Distributions**:
- **Support Conversations**: 40% Foundation, 30% Standard, 20% Advanced, 10% Decisive
- **Request Conversations**: 35% Foundation, 30% Standard, 25% Advanced, 10% Decisive
- **Investigation**: 30% Foundation, 35% Standard, 25% Advanced, 10% Decisive
- **Authority**: 25% Foundation, 30% Standard, 30% Advanced, 15% Decisive

### Phase 4: System Integration & Testing (LOW)

#### 4.1 UI Updates
**Files Modified**:
- `ConversationContent.razor.cs` - Update displays
- Card tooltips to show depth requirements
- LISTEN preview showing momentum cost

#### 4.2 Personality Rule Updates
**Files Modified**:
- `PersonalityRuleEnforcer.cs` - Adapt to Initiative system

**Updates**:
- Proud: Cards must be in ascending Initiative order (not Focus)
- Mercantile: Highest Initiative card gets bonus (not highest Focus)

## Risk Mitigation Strategies

### 1. Hand Clogging Prevention
**Issue**: Drawing expensive cards at high momentum that become unplayable
**Solution**: Hand size limit of 7, must discard down at LISTEN

**Implementation**:
```csharp
void ProcessListen()
{
    // ... existing logic ...

    // Force discard if hand too large
    if (session.Deck.HandSize > 7)
    {
        session.Deck.DiscardDown(7);
    }
}
```

### 2. Foundation Depletion Prevention
**Issue**: Statement Foundations could deplete Initiative generation
**Solution**: Minimum 70% of Foundations as Echoes

**Validation**:
```csharp
void ValidateFoundationCards(List<ConversationCard> cards)
{
    var foundations = cards.Where(c => (int)c.Depth <= 2);
    var echoes = foundations.Where(c => c.Persistence == PersistenceType.Echo);
    var initiatives = foundations.Where(c => c.GetInitiativeEffect() > 0);

    if (echoes.Count() < foundations.Count() * 0.7)
        throw new InvalidOperationException("Foundation cards must be 70% Echo type");

    if (initiatives.Any(c => c.Persistence != PersistenceType.Echo))
        throw new InvalidOperationException("All Initiative-generating cards must be Echo type");
}
```

### 3. Momentum Stagnation Prevention
**Issue**: If all Foundations only generate Initiative, momentum might never grow
**Solution**: Some Foundations generate small momentum amounts

## Testing & Validation Plan

### Test Scenario 1: Greedy Player Trap
**Purpose**: Verify greed creates self-inflicted difficulty

**Steps**:
1. Start conversation (momentum 2-3)
2. Rush momentum to 8 without building Initiative
3. Verify hand clogs with expensive unplayable cards
4. Confirm forced LISTEN drops player back
5. Validate slow recovery required

**Success Criteria**: Player recognizes greed caused situation

### Test Scenario 2: Specialist Advantage
**Purpose**: Confirm stat specialization provides meaningful advantage

**Steps**:
1. Create Rapport 10 character
2. Start conversation at momentum 3
3. Verify access to depth 6 Rapport cards (3 + 3 bonus)
4. Confirm other cards limited to depth 3
5. Test meaningful specialization advantage

**Success Criteria**: Specialist executes style even when struggling

### Test Scenario 3: Foundation Sustainability
**Purpose**: Ensure no mechanical deadlock

**Steps**:
1. Play 20+ turn conversation
2. Verify Initiative generation never depletes
3. Confirm Echo Foundations remain drawable
4. Test Statement accumulation in Spoken
5. Validate no deadlock occurs

**Success Criteria**: Long conversations remain playable

### Test Scenario 4: Conversation Flow Arc
**Purpose**: Verify natural dramatic arcs

**Steps**:
1. Track momentum curve over full conversation
2. Verify natural wave pattern from doubt pressure
3. Confirm Foundation → Standard → Advanced progression
4. Test LISTEN creates meaningful reset points
5. Validate climax possibility at high momentum

**Success Criteria**: Momentum shows 3-5 natural peaks and valleys

## Implementation Timeline

### Critical Path (Must Complete First)
1. **LISTEN Refactor** - Foundation for new system
2. **Momentum Draw Pool** - Core progression mechanic
3. **Foundation Echo Classification** - Deadlock prevention

### High Priority (Core Features)
4. **Stat Specialization** - Character differentiation
5. **Linear Cadence** - Remove complexity

### Medium Priority (Polish)
6. **Starting Momentum** - Progression reward
7. **Depth Distribution** - Content variety

### Low Priority (Fine-tuning)
8. **Personality Updates** - System adaptation
9. **UI Improvements** - Quality of life

## Success Metrics

### Mechanical Validation
- [ ] Conversations create 3-5 natural momentum waves
- [ ] Players can reach Premium goals (16 momentum) with skill
- [ ] Greedy play creates recoverable but punishing situations
- [ ] Specialists feel meaningfully different without exclusion
- [ ] No mechanical deadlocks possible
- [ ] Average conversation length 12-20 turns
- [ ] Foundation cards played throughout entire conversation

### Design Philosophy Validation
- [ ] **Elegance Over Complexity**: Each system serves exactly one purpose
- [ ] **Verisimilitude Throughout**: Every mechanic makes narrative sense
- [ ] **Perfect Information**: All calculations visible to players
- [ ] **No Soft-Lock Architecture**: Always a path forward
- [ ] **Deterministic Systems**: No hidden randomness

## Post-Implementation Tasks

### Content Audit
1. Review all conversation card definitions for consistency
2. Validate Echo/Statement classifications
3. Test conversation type distributions
4. Balance momentum thresholds for goal cards

### Performance Testing
1. Stress test long conversations (50+ turns)
2. Validate memory usage with large decks
3. Test UI responsiveness with complex calculations

### Integration Testing
1. Test with all personality types
2. Validate NPC signature card integration
3. Test observation card system compatibility
4. Verify exchange system still functions

This implementation plan ensures the conversation system refactor maintains architectural integrity while achieving the design goals of dynamic progression, character specialization, and strategic depth through resource management.