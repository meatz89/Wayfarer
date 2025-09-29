# Conversation System Refactor - Implementation Plan

## Overview
This document outlines the complete implementation of the conversation system refactor, correcting critical bugs and implementing the SteamWorld Quest-inspired Initiative system.

## Core Understanding (Corrected)
- **Initiative starts at 0** (like SteamWorld Quest Strike/Steam system)
- **Foundation cards (depth 1-2)** cost 0 Initiative and GENERATE Initiative (like Strike cards)
- **Higher depth cards** cost Initiative to play (like Skill cards)
- **Echo cards** should be repeatable (return to deck/hand for reuse)
- **Statement cards** should stay permanently in Spoken pile (one-time use)

---

## CRITICAL BUG DISCOVERED

### Echo/Statement Persistence is COMPLETELY BACKWARDS
**Current Wrong Implementation:**
- Statement cards reshuffle from Spoken back to deck ❌
- Echo cards stay permanently in Spoken pile ❌

**Correct Implementation:**
- Echo cards should return to deck for reuse ✅
- Statement cards should stay permanently in Spoken ✅

This breaks the entire sustainability model and must be fixed first!

---

## PHASE 1: FIX CRITICAL PERSISTENCE BUG

### 1.1 Fix Echo/Statement System
**Problem**: The current `ReshuffleSpokenPile()` method in `SessionCardDeck.cs` is completely backwards.

**Files to modify**:
- `SessionCardDeck.cs` - Complete rewrite of persistence logic
- `ConversationFacade.cs` - Update card playing mechanics

**Implementation Changes**:
```csharp
// WRONG (current):
// Statement cards reshuffle back to deck
// Echo cards stay in Spoken

// CORRECT (new):
// Echo cards return to deck for reuse
// Statement cards stay permanently in Spoken
```

### 1.2 New Persistence Flow
**Echo Cards (Repeatable Techniques)**:
1. When played → Go to temporary "Echo zone" or return to deck
2. Can be drawn again later in conversation
3. Examples: "Active Listening", "Encouraging Nod", "Thoughtful Pause"

**Statement Cards (One-time Content)**:
1. When played → Go permanently to Spoken pile
2. Never return to deck
3. Examples: "Introduce Myself", "State Purpose", "Make Promise"

---

## PHASE 2: CORE MECHANICS FIXES

### 2.1 Initiative System (Already Correct!)
**Current Implementation**: Starting Initiative = 0 ✅
- This is already correct - no changes needed
- Foundation cards cost 0 Initiative and generate 1-2 Initiative
- Perfect SteamWorld Quest Strike/Steam model

### 2.2 Implement 7-Card Hand Limit
**Files to modify**:
- `ConversationSession.cs` - Change `IsHandOverflowing()` from 10 to 7
- `SessionCardDeck.cs` - Add `DiscardDown(int maxSize)` method
- `ConversationFacade.cs` - Force discard to 7 after LISTEN

**Implementation**:
```csharp
public void DiscardDown(int maxSize)
{
    while (mindPile.Count > maxSize)
    {
        // Player chooses which cards to discard
        // Or auto-discard by some criteria
    }
}
```

### 2.3 Foundation Card Validation
**Requirements**:
- 70% of depth 1-2 cards must be Echo type
- ALL Initiative-generating cards must be Echo type
- Add parser validation to enforce this

**Files to modify**:
- `ConversationCardParser.cs` - Add validation method
- `PackageLoader.cs` - Call validation after loading cards

---

## PHASE 3: CONVERSATION TYPE SYSTEM

### 3.1 Define Conversation Types with Depth Distributions
**Files to modify**:
- `ConversationTypeDefinition.cs` - Add `DepthDistribution` property
- `ConversationTypeDefinitionDTO.cs` - Add depth distribution fields
- `02_cards.json` - Define conversation types

**Depth Distributions**:
```
Support Conversations:    40% Foundation, 30% Standard, 20% Advanced, 10% Decisive
Request Conversations:    35% Foundation, 30% Standard, 25% Advanced, 10% Decisive
Investigation:           30% Foundation, 35% Standard, 25% Advanced, 10% Decisive
Authority Conversations: 25% Foundation, 30% Standard, 30% Advanced, 15% Decisive
```

**Data Structure**:
```csharp
public class ConversationTypeDefinition
{
    // ... existing properties ...
    public DepthDistribution Distribution { get; set; }
}

public class DepthDistribution
{
    public float Foundation { get; set; } // Depth 1-2
    public float Standard { get; set; }   // Depth 3-4
    public float Advanced { get; set; }   // Depth 5-6
    public float Decisive { get; set; }   // Depth 7-8
}
```

### 3.2 Implement Depth-Based Deck Building
**Files to modify**:
- `ConversationDeckBuilder.cs` - Apply depth distributions when building decks
- Filter available cards based on conversation type's distribution ratios

**Implementation**:
```csharp
List<ConversationCard> BuildDeckWithDistribution(ConversationTypeDefinition type, List<ConversationCard> allCards)
{
    var foundation = allCards.Where(c => c.Depth <= 2).ToList();
    var standard = allCards.Where(c => c.Depth >= 3 && c.Depth <= 4).ToList();
    var advanced = allCards.Where(c => c.Depth >= 5 && c.Depth <= 6).ToList();
    var decisive = allCards.Where(c => c.Depth >= 7).ToList();

    // Apply distribution ratios to select cards
    // ...
}
```

---

## PHASE 4: COMPLETE SYSTEMS (Already Implemented ✅)

### 4.1 Momentum-Based Draw Pool ✅
- Cards filtered by momentum + stat bonuses
- Already implemented in previous work

### 4.2 LISTEN Action ✅
- Doubt resets to 0
- Momentum reduced by doubt cleared
- Cadence reduced by 3
- Already implemented

### 4.3 Starting Momentum ✅
- Formula: 2 + floor(highest_stat / 3)
- Already implemented

---

## PHASE 5: UI/FRONTEND UPDATES

### 5.1 Display Card Requirements
**Files to modify**:
- `ConversationContent.razor` - Show depth and Initiative requirements
- `ConversationContent.razor.cs` - Add helper methods
- `conversation.css` - Style indicators

**Display Elements**:
- Show card depth as badge (1-8)
- Show Initiative cost clearly
- Gray out cards that exceed current momentum
- Highlight specialist-accessible cards (stat bonus)
- Show "Requires Momentum X" tooltip

### 5.2 LISTEN Preview Enhancement
**Files to modify**:
- `ConversationContent.razor` - Update LISTEN button

**Show on LISTEN button**:
- "LISTEN: Clear doubt (-X momentum)"
- "Draw Y cards" where Y = 3 + abs(min(cadence, 0))
- "Hand limit 7 (will discard excess)" if hand > 7

**Example**: "LISTEN: Clear 3 doubt (-3 momentum), Draw 5 cards"

---

## PHASE 6: TESTING & VALIDATION

### 6.1 Create Core Validation Tests
**Test Scenarios**:
1. **Initiative Builder Loop**: Start at 0, use Foundation to build Initiative
2. **Echo Sustainability**: Verify Echo cards return, Statements stay permanent
3. **Hand Management**: Test 7-card limit and forced discard
4. **Conversation Types**: Different types have distinct depth distributions

### 6.2 Content Validation Tools
**New Utilities**:
- Echo/Statement ratio validator (ensure 70% Foundation are Echo)
- Initiative generation validator (all Initiative-gen cards are Echo)
- Depth distribution analyzer per conversation type
- Deadlock detection tool

---

## CRITICAL CODE TO REMOVE/FIX

### Must Fix Immediately:
1. **`ReshuffleSpokenPile()` method** - Completely backwards logic
   - Currently: Reshuffles Statement cards (should stay permanent)
   - Should: Handle Echo cards returning to deck

2. **Card persistence after playing** - Wrong flow
   - Echo cards must be repeatable
   - Statement cards must be permanent

3. **Hand limit validation** - Wrong value
   - Change from 10 to 7 cards maximum

4. **Missing validation** - No enforcement
   - Add Foundation card ratio validation
   - Validate Initiative-generation rules

---

## SUCCESS CRITERIA

### Core Loop Working:
- [ ] Start with 0 Initiative (SteamWorld Quest model)
- [ ] Foundation cards generate Initiative for free
- [ ] Higher depth cards cost Initiative to play
- [ ] Echo cards are repeatable throughout conversation
- [ ] Statement cards are permanent one-time use

### No Depletion/Deadlocks:
- [ ] 70% of Foundation cards are Echo type
- [ ] All Initiative-generating cards are Echo
- [ ] 20+ turn conversations remain playable
- [ ] No mechanical deadlocks possible

### Conversation Variety:
- [ ] Support conversations feel different from Authority
- [ ] Different depth distributions create distinct play patterns
- [ ] Natural momentum waves (3-5 peaks/valleys)

### Player Experience:
- [ ] Clear Initiative costs visible
- [ ] Momentum requirements shown
- [ ] LISTEN shows exact trade-offs
- [ ] Specialists have advantages without exclusion

---

## IMPLEMENTATION ORDER

1. **CRITICAL**: Fix Echo/Statement persistence (completely backwards!)
2. **CRITICAL**: Add Foundation card validation (70% Echo rule)
3. **CRITICAL**: Implement 7-card hand limit
4. **HIGH**: Add conversation type depth distributions
5. **HIGH**: Update UI to show requirements clearly
6. **MEDIUM**: Create testing and validation framework
7. **LOW**: Update personality rules for new system

---

## Risk Mitigation

### Hand Clogging Prevention
- 7-card limit forces regular hand refresh
- LISTEN discards excess cards
- Prevents permanent disadvantage from bad draws

### Foundation Depletion Prevention
- 70% Echo minimum at depth 1-2
- All Initiative generators are Echo
- Sustainable fuel generation guaranteed

### Momentum Stagnation Prevention
- Some Foundation cards generate small momentum
- Ensures slow steady progress always possible
- Prevents complete stagnation at low momentum

This implementation fixes the fundamental persistence bug while completing the SteamWorld Quest-inspired conversation system with proper resource management and sustainability.