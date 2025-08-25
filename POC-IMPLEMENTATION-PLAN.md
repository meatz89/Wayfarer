# WAYFARER POC IMPLEMENTATION PLAN
**Created**: 2025-08-25
**Status**: IN PROGRESS

## 🎯 SUCCESS CRITERIA FOR POC

The POC is complete when a player can:
1. Start at Market Square Fountain spot
2. Move to Merchant Row spot (instant)
3. Quick Exchange with Marcus (3 coins → Hunger = 0)
4. Return to Fountain spot
5. Observe "Guards blocking north road" (gain Shadow card)
6. Travel to Copper Kettle Tavern (15 minutes)
7. Move to Corner Table spot
8. Start conversation with Elena (DESPERATE state)
9. Reach 10 comfort to generate letter
10. See letter in queue with correct deadline

## 📊 CURRENT STATE ASSESSMENT

### What's Actually Working ✅
- Basic conversation flow exists
- Observation cards DO appear in hand
- Letter generation triggers at thresholds
- Resource system displays values

### What's Broken ❌
- **UI**: Uses emoji (💰❤️🍞👁️) instead of styled icons
- **Cards**: Look like gray boxes, not the beautiful mockup cards
- **Emotional States**: Only DESPERATE/HOSTILE work (2 of 9)
- **Exchanges**: Unknown if implemented as cards

### Completion by Component
- Core Mechanics: 40%
- Emotional States: 20%
- UI/Visual Design: 10%
- Content: 30%
- Polish: 0%

## 📦 IMPLEMENTATION PACKAGES

### Package 1: UI COMPLETE OVERHAUL
**Agent**: ui-ux-designer-priya
**Time**: 8 hours
**Priority**: CRITICAL

#### Tasks:
1. **Replace ALL Emoji Icons**
   - Create CSS-based icons for coins, health, hunger, attention
   - Update GameScreen.razor to use CSS classes not emoji
   - Fix navigation icons (location, letters, travel)
   - Ensure icons display in all browsers

2. **Redesign Cards to Match Mockup**
   - Card structure with proper borders and gradients
   - Left border color coding by type
   - Header with name and weight
   - Tags section with proper styling
   - Success/failure outcomes clearly displayed
   - "FREE!" tag positioned inside card (not top: -8px)

---

### Package 2: EMOTIONAL STATE SYSTEM FIX
**Agent**: systems-architect-kai
**Time**: 6 hours
**Priority**: CRITICAL

#### Tasks:
1. **Debug State Initialization**
   - Why only DESPERATE/HOSTILE appear
   - Trace ConversationSession.StartConversation
   - Verify all 9 states can trigger
   - Check state transition cards in decks

2. **Fix State-Specific Mechanics**
   - EAGER: +3 bonus for playing 2+ same type
   - OVERWHELMED: Max 1 card only
   - CONNECTED: Auto-advance depth
   - TENSE: Weight limit 1
   - Each state: Correct draw counts

3. **State Transition Cards**
   - Add to conversation decks
   - Make visible in UI
   - Test transitions work

---

### Package 3: EXCHANGE SYSTEM AS CARDS
**Agent**: game-design-reviewer (Chen)
**Time**: 4 hours
**Priority**: HIGH

#### Tasks:
1. **Implement Exchanges as Cards**
   - Generate accept/decline cards
   - Use SPEAK action to select
   - NO special UI or buttons
   - Same card system as conversations

2. **Exchange Mechanics**
   - Draw from exchange deck
   - Display as conversation cards
   - Show costs/rewards clearly
   - Instant resolution

---

### Package 4: LETTER & OBLIGATION SYSTEM
**Agent**: narrative-designer-jordan
**Time**: 4 hours
**Priority**: HIGH

#### Tasks:
1. **Fix Letter Thresholds**
   - 5-9 comfort: Simple (24h, 5 coins)
   - 10-14: Important (12h, 10 coins)
   - 15-19: Urgent (6h, 15 coins)
   - 20+: Critical (2h, 20 coins)

2. **Fix Deadlines**
   - Verify time calculations
   - Test with actual game time
   - Ensure appears in queue

---

### Package 5: POC SCENARIO IMPLEMENTATION
**Agent**: change-validator
**Time**: 4 hours
**Priority**: FINAL

#### Tasks:
1. **Create POC Content**
   - Marcus at Merchant Row
   - Elena at Corner Table
   - Observation at Fountain
   - Routes between locations

2. **Test Complete Flow**
   - Use Playwright
   - Screenshot each step
   - Verify against mockups

---

## 🔄 EXECUTION ORDER

### Phase 1: Critical UI Fixes (Day 1)
1. ✅ Document plan (THIS FILE)
2. ⏳ Package 1: UI Overhaul with Priya
3. ⏳ Verify with screenshots

### Phase 2: Core Systems (Day 1-2)
4. ⏳ Package 2: Emotional States with Kai
5. ⏳ Package 3: Exchange System with Chen
6. ⏳ Test both systems work

### Phase 3: Complete POC (Day 2)
7. ⏳ Package 4: Letters with Jordan
8. ⏳ Package 5: POC Flow validation
9. ⏳ Final testing with Playwright

## 📋 TESTING CHECKLIST

Before claiming ANY task complete:
- [ ] Clean build: `dotnet build --no-incremental`
- [ ] Clear browser cache
- [ ] Launch with Playwright
- [ ] Take screenshots
- [ ] Compare to mockups
- [ ] Document what works/doesn't work

## 🚨 CRITICAL RULES

1. **NO EMOJI** - Replace ALL with CSS
2. **CARDS NOT BUTTONS** - Everything is a card
3. **TEST EVERYTHING** - No assumptions
4. **MATCH MOCKUPS** - Pixel perfect
5. **ALL 9 STATES** - Must work

## 📈 PROGRESS TRACKING

| Package | Agent | Status | Completion |
|---------|-------|--------|------------|
| UI Overhaul | Priya | COMPLETED | 100% |
| Emotional States | Kai | COMPLETED | 100% |
| Exchange System | Chen | COMPLETED | 100% |
| Letter System | Jordan | COMPLETED | 100% |
| POC Validation | Validator | IN PROGRESS | 0% |

## 🎮 FINAL DELIVERABLE

A working POC where:
- UI matches mockups exactly
- All 9 emotional states function
- Exchanges use card system
- Complete POC flow works
- Screenshots prove everything

**Current Status**: Starting Package 1