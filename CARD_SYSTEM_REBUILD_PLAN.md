# Card System Rebuild Implementation Plan

## Executive Summary
Complete rebuild of the Wayfarer card system to implement the desperate plea card specification and fix the broken conversation system where no cards are drawn.

## Current System Problems
1. **Broken Card References**: Conversation decks reference fictional card IDs (`commerce_thought_1`, `cunning_thought_1`, etc.) that don't exist
2. **Empty Decks**: All conversation attempts result in 0 cards being drawn during LISTEN actions
3. **Missing Mechanics**: No momentum/doubt system implemented despite being specified
4. **Elena Conversation Broken**: Only shows "make elena my priority" card, missing all other conversation cards

## Implementation Strategy

### Phase 1: Legacy System Removal
**Task**: Remove all existing broken cards from 02_cards.json
**Goal**: Clear the deck for complete rebuild
**Files Modified**:
- `C:\Git\Wayfarer\src\Content\Core\02_cards.json` - Remove entire "cards" array

### Phase 2: Desperate Plea Card Implementation
**Task**: Implement exactly 24 cards per desperate_plea_cards.md specification
**Goal**: Create functional card system with momentum/doubt mechanics

#### Expression Cards (11 total)
1. `soft_agreement` (x2) - 1 focus, Thought, Rapport, +2 momentum
2. `careful_analysis` (x2) - 1 focus, Thought, Insight, +2 momentum
3. `show_understanding` - 2 focus, Thought, Rapport, momentum = cards in hand ÷ 2
4. `build_pressure` - 3 focus, Thought, Authority, momentum = (10 - current doubt)
5. `emotional_outburst` (x2) - 1 focus, Impulse, Rapport, +3 momentum, -1 draw penalty
6. `critical_moment` - 2 focus, Impulse, Authority, +5 momentum, -1 draw penalty
7. `burning_insight` (x2) - 1 focus, Impulse, Insight, +3 momentum, -1 draw penalty

#### Realization Cards (7 total)
1. `clear_confusion` (x2) - 2 focus, Thought, Insight, spend 3 momentum → reduce doubt by 2
2. `establish_trust` - 1 focus, Thought, Rapport, spend 2 momentum → +1 flow
3. `deep_investment` - 2 focus, Thought, Commerce, spend 4 momentum → permanent +1 card on LISTEN
4. `all_or_nothing` - 3 focus, Impulse, Authority, spend 6 momentum → gain 12 momentum
5. `reset_stakes` - 2 focus, Impulse, Cunning, spend ALL momentum → set doubt to 0
6. `force_understanding` - 4 focus, Impulse, Authority, spend 5 momentum → +3 flow

#### Regulation Cards (6 total)
1. `mental_reset` - 0 focus, Thought, Insight, +2 focus this turn only
2. `careful_words` (x2) - 1 focus, Thought, Cunning, discard up to 2 cards → gain 1 momentum per card
3. `patience` - 2 focus, Thought, Commerce, prevent next doubt increase
4. `racing_mind` (x2) - 1 focus, Impulse, Insight, draw 2 cards immediately

### Phase 3: Game Mechanics Updates
**Task**: Add new scaling types and momentum/doubt mechanics
**Files Modified**:
- Card parsers to handle new scaling formulas
- Effect resolvers for momentum spend/gain
- Impulse penalty system for unplayed cards

### Phase 4: Conversation Deck Updates
**Task**: Update deck references to use new card IDs
**Files Modified**:
- `02_cards.json` - Update `deck_desperate_empathy` to reference all 24 new cards
- Verify `elena_priority_promise` exists and works

### Phase 5: Testing & Verification
**Task**: Comprehensive testing of Elena's conversation
**Verification Points**:
- All 24 cards appear in conversation
- LISTEN draws 4 cards properly
- Momentum/doubt mechanics work
- "Make Elena my priority" card appears
- No "card not found" errors

## Implementation Agents

### Agent 1: Legacy Card Removal
- Remove all existing cards from JSON
- Clear broken deck references
- Prepare clean slate for rebuild

### Agent 2: Card Implementation
- Implement all 24 desperate plea cards
- Add proper JSON structure with new mechanics
- Ensure card IDs match specification exactly

### Agent 3: Mechanics Integration
- Add momentum/doubt scaling to parsers
- Implement impulse penalty system
- Update effect resolvers

### Agent 4: Deck Configuration
- Update conversation decks to reference new cards
- Verify Elena's desperate_request conversation type
- Test card circulation

### Agent 5: Verification
- Test Elena conversation end-to-end
- Verify all cards appear and function
- Validate momentum/doubt mechanics
- Confirm no errors in logs

## Success Criteria
1. ✅ Elena's conversation shows all 24 desperate plea cards
2. ✅ LISTEN actions draw cards successfully (no 0 cards drawn)
3. ✅ Momentum/doubt mechanics function as specified
4. ✅ "Make Elena my priority" card appears and works
5. ✅ No "card not found" warnings in logs
6. ✅ Cards have proper focus costs, persistence, and effects

## Risk Mitigation
- **Multiple verification rounds** with different agents
- **Incremental testing** after each phase
- **Rollback plan** if any phase fails
- **Cross-validation** between specification and implementation

This plan ensures complete transformation of the broken card system into a functional momentum/doubt-based conversation system.