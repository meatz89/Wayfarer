# Conversation Redesign Implementation Plan

## Overview
This document outlines the implementation plan for completing the conversation system redesign from `wayfarer-conversation-redesign.md`. The implementation follows the proper pipeline: JSON → Parser → Entity → Mechanics → UI.

## Current Status

### ✅ Already Implemented
- **Player Conversation Deck**: Player owns a deck used in all conversations
- **NPC Progression Cards**: Token-based unlocking at thresholds (1, 3, 6, 10, 15)
- **Failure Forces LISTEN**: Failed SPEAK exhausts all focus, forcing LISTEN
- **Categorical Card System**: Cards use PersistenceType, SuccessEffectType, etc.

### ❌ Not Implemented
1. Card XP/Leveling System
2. Personality Conversation Modifiers
3. Updated Draw Counts (currently 1-3, should be 3-5)
4. NPC Progression Cards in JSON
5. Persistence Balance (should be 20% persistent, not 80%)

## Implementation Details

### Feature 1: Update Draw Counts (Simplest)

**Files to Modify:**
- `src/Content/Core/game_rules.json` - Add listenDrawCounts configuration
- `src/Services/GameRules.cs` - Add ListenDrawCounts property
- `src/Content/GameRulesParser.cs` - Parse the new property
- `src/Subsystems/Conversation/ConversationOrchestrator.cs` (lines 372-376) - Use config values

**Changes:**
```json
// game_rules.json
{
  "listenDrawCounts": {
    "Disconnected": 3,
    "Guarded": 4,
    "Neutral": 4,
    "Receptive": 5,
    "Trusting": 5
  }
}
```

### Feature 2: Adjust Persistence Balance

**Files to Modify:**
- `src/Content/Core/core_game_package.json` - Update starter deck cards

**Distribution:**
- 4 cards with `"persistence": "Thought"` (20%)
- 10 cards with `"persistence": "Impulse"` (50%)
- 6 cards with `"persistence": "Opening"` (30%)

### Feature 3: Add NPC Progression Cards

**Files to Modify:**
- `src/Content/Core/core_game_package.json` - Add progressionDeck to each NPC

**Example Structure:**
```json
"progressionDeck": [
  {
    "id": "marcus_bargain",
    "description": "Marcus's Bargain",
    "minimumTokensRequired": 1,
    "focus": 2,
    "difficulty": "Hard",
    "persistence": "Thought",
    "successType": "Rapport"
  },
  // ... 4 more cards at 3, 6, 10, 15 tokens
]
```

### Feature 4: Personality Rule Enforcer

**New Files:**
- `src/Subsystems/Conversation/PersonalityRuleEnforcer.cs`

**Files to Modify:**
- `src/GameState/Enums/PersonalityModifierType.cs` - Create enum
- `src/GameState/PersonalityModifier.cs` - Create class
- `src/Content/NPCParser.cs` - Parse personality modifiers
- `src/Subsystems/Conversation/ConversationOrchestrator.cs` - Integrate enforcer

**Personality Rules:**
- **Proud**: Cards must be played in ascending focus order (reset on LISTEN)
- **Devoted**: When rapport decreases, decrease it twice
- **Mercantile**: Highest focus card each turn gains +30% success
- **Cunning**: Playing same focus as previous costs -2 rapport
- **Steadfast**: All rapport changes capped at ±2

### Feature 5: Card XP/Leveling System

**Files to Modify:**
- `src/GameState/CardInstance.cs` - Add XP, Level properties
- `src/GameState/ConversationCard.cs` - Add LevelBonuses
- `src/GameState/CardLevelBonus.cs` - Create class
- `src/Content/ConversationCardParser.cs` - Parse level bonuses
- `src/Subsystems/Conversation/ConversationOrchestrator.cs` - Grant XP, apply bonuses
- `src/Pages/Components/ConversationContent.razor` - Display XP/Level

**Level System:**
- Thresholds: 3, 7, 15, 30, 50, 75, 100, 150, 200... (infinite progression)
- Level 2: +10% success rate
- Level 3: Gains persistence (becomes Thought type)
- Level 4: +10% success rate
- Level 5: Ignores forced LISTEN on failure
- Levels 6+: Continue pattern (+10% success every even level)

## Implementation Order

### Phase 1: Configuration Changes (1 hour)
1. ✅ Update draw counts in game_rules.json
2. ✅ Parse and apply new draw counts
3. ✅ Adjust persistence balance in starter deck
4. ✅ Verify changes work

### Phase 2: Content Addition (1 hour)
1. ✅ Add progression cards for Marcus
2. ✅ Add progression cards for Elena
3. ✅ Add progression cards for other NPCs
4. ✅ Test token unlocking works

### Phase 3: Personality System (3 hours)
1. ✅ Create PersonalityRuleEnforcer
2. ✅ Implement each personality rule
3. ✅ Integrate with ConversationOrchestrator
4. ✅ Update UI to show rules
5. ✅ Test each personality

### Phase 4: Card Leveling (3 hours)
1. ✅ Add XP/Level to CardInstance
2. ✅ Parse level bonuses from JSON
3. ✅ Implement XP gain on success
4. ✅ Apply level bonuses
5. ✅ Update UI to show progression
6. ✅ Test save/load with XP

### Phase 5: Integration Testing (1 hour)
1. ✅ Test all features together
2. ✅ Verify no regressions
3. ✅ Check save game compatibility
4. ✅ Run full conversation flow

## Success Criteria

### Draw Counts ✅ COMPLETE
- [x] Disconnected state draws 3 cards
- [x] Guarded state draws 4 cards
- [x] Neutral state draws 4 cards
- [x] Receptive state draws 5 cards
- [x] Trusting state draws 5 cards

### Persistence Balance ✅ COMPLETE
- [x] Only 20% of starter cards are Thought type (4 of 20)
- [x] 80% of cards lost on LISTEN
- [x] Creates proper urgency in conversations

### Progression Cards ✅ COMPLETE
- [x] Each NPC has 5 unique progression cards
- [x] Cards unlock at 1, 3, 6, 10, 15 tokens
- [x] Cards have NPC-specific names and flavor

### JSON Refactoring ✅ COMPLETE
- [x] Cards and decks separated into cards_and_decks_package.json
- [x] Core package reduced from 1591 to 409 lines
- [x] Clean separation of world definition from card content

### Personality Rules ⏳ PENDING
- [ ] Each personality has distinct mechanical effect
- [ ] Rules clearly displayed in UI
- [ ] Violations prevent illegal plays
- [ ] Rules create different strategic puzzles

### Card Leveling ⏳ PENDING
- [ ] Cards gain 1 XP per successful play
- [ ] Levels calculated from XP thresholds
- [ ] Bonuses apply cumulatively
- [ ] No level cap exists
- [ ] XP persists between conversations

## Testing Strategy

### Unit Tests
- Test PersonalityRuleEnforcer validation logic
- Test XP to Level calculations
- Test draw count configuration

### Integration Tests
- Full conversation with each personality type
- Card leveling across multiple conversations
- Token-based progression unlocking

### Manual Testing
- Play through Elena's Letter scenario
- Verify all UI elements display correctly
- Check save/load compatibility

## Risk Mitigation

### Save Game Compatibility
- Card XP defaults to 0 for existing saves
- Level bonuses only apply if defined
- Personality rules gracefully handle missing data

### Performance
- XP calculations cached per conversation
- Level bonuses computed once per card
- Personality rules check only on play attempt

### Balance
- Draw counts configurable in game_rules.json
- XP thresholds adjustable
- Personality modifiers tunable

## Completion Checklist

- [ ] All JSON changes committed
- [ ] All parsers updated
- [ ] All entities extended
- [ ] All mechanics implemented
- [ ] All UI updated
- [ ] All tests passing
- [ ] Documentation updated
- [ ] Save compatibility verified
- [ ] Performance acceptable
- [ ] Balance feels good

This implementation will complete the conversation redesign, making conversations the true core progression loop where players build and improve their deck over time.