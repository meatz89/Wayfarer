# Complete Game Systems Implementation Plan

## Overview
This document outlines the implementation of all missing game systems to create the complete Wayfarer experience as designed.

## Implementation Status (2025-08-02)
- ✅ Phase 1: Special Letter Generation System - COMPLETE
- ✅ Phase 2: Information Letter Satchel System - COMPLETE  
- ✅ Phase 3: Endorsement-to-Seal System - COMPLETE
- ✅ Phase 4: Route Discovery System - COMPLETE
- ✅ Phase 5: Multi-Context Token Display - COMPLETE
- ✅ Phase 6: Time Cost System - COMPLETE
- ✅ Phase 7: Leverage Visibility - COMPLETE
- ✅ Phase 8: Standing Obligation Integration - COMPLETE

## Core Design Principles to Follow
1. **NO SILENT BACKEND ACTIONS** - Every change must be player-initiated or clearly messaged
2. **System Interactions Over Special Rules** - Create categorical mechanics, not exceptions
3. **Everything Has UI** - If it exists in backend, player must be able to see and interact with it

## Implementation Phases

### Phase 1: Special Letter Generation System
**Goal**: Allow players to exchange tokens for special letters at thresholds

#### 1.1 Create SpecialLetterService
- Check token thresholds for each NPC
- Generate appropriate special letter offers
- NO automatic generation - must be player-initiated

#### 1.2 Add UI Request Buttons
- Add "Request Special Letter" button in NPC interaction screen
- Show when player meets token thresholds:
  - Trust 5+ → "Request Introduction" 
  - Commerce 5+ → "Request Access Permit"
  - Status 5+ → "Request Endorsement"
  - Shadow 5+ → "Request Information"
- Display cost and requirements clearly

#### 1.3 Content Templates
Create special letter templates in letter_templates.json:
- introduction_letter_template (Trust, SpecialType: Introduction)
- access_permit_template (Commerce, SpecialType: AccessPermit)
- endorsement_letter_template (Status, SpecialType: Endorsement)
- information_letter_template (Shadow, SpecialType: Information)

### Phase 2: Information Letter Satchel System
**Goal**: Information letters take inventory space but not queue slots

#### 2.1 Modify Letter Queue Logic
- Check if letter.SpecialType == Information
- If yes, add to player.Inventory instead of LetterQueue
- Show in separate "Carried Information" UI section

#### 2.2 Create Information Display UI
- Show carried information letters in inventory
- Display what they reveal while carried
- Allow player to discard if needed

#### 2.3 Implement Reveal Mechanics
- While carried, information letters reveal hidden content
- Check InformationDiscoveryManager when rendering UI
- Show previously hidden NPCs/routes/actions

### Phase 3: Endorsement-to-Seal System
**Goal**: Complete the Status token progression path

#### 3.1 Create Guild Locations
- Add Guild locations to locations.json
- Each guild type (Merchant, Messenger, Scholar)
- Tier 2+ access requirements

#### 3.2 Endorsement Conversion UI
- Add "Visit Guild" action at guild locations
- Show "Convert Endorsements" button
- Display conversion rates (e.g., 3 endorsements = 1 seal)
- Show current endorsement count

#### 3.3 Seal Progression Logic
- Track endorsements delivered to guilds
- Convert at thresholds to seal tiers
- Update player.WornSeals when conversion happens
- Message player about new privileges

### Phase 4: Route Discovery System
**Goal**: Implement the two-phase discovery system

#### 4.1 Explore Action
- Add "Explore Area" button in location screen
- Costs time (2-4 hours based on location size)
- Reveals undiscovered routes from current location
- Shows discovery messages

#### 4.2 Information Integration
- NPCs mention routes in conversation
- Letters can contain route information
- Information letters reveal hidden routes
- All create InformationDiscoveryManager entries

#### 4.3 Access Requirements UI
- Show discovered but inaccessible routes differently
- Display requirements clearly:
  - "Requires: Commerce 3 + 10 coins"
  - "Requires: Merchant Seal + Cart"
  - "Requires: Information about safe passage"

### Phase 5: Multi-Context Token Display
**Goal**: Show relationship complexity in UI

#### 5.1 Enhanced NPC Cards
Replace single token count with context breakdown:
```
Elena: 
Trust: 5 ❤️ (close friend)
Commerce: -2 💰 (owes you money)
Status: 1 ⭐ (acquaintance)
Shadow: 0 🌙 (no secrets)
```

#### 5.2 Token Transaction Preview
When taking actions, show which context is affected:
- "Deliver Letter: +1 Trust token with Elena"
- "Skip Letter: -3 Commerce tokens (debt leverage)"

#### 5.3 Relationship Screen Overhaul
- Tab for each token context
- Show all NPCs sorted by that context
- Highlight debts and leverage opportunities

### Phase 6: Time Cost System
**Goal**: Make time pressure visible on all actions

#### 6.1 Action Time Preview Component
Create reusable component showing:
- Clock icon + time cost
- Current time → resulting time
- Impact on deadlines

#### 6.2 Apply to All Actions
- Travel: Already shows hours
- Work: "Work at Shop (4 hours)"
- Socialize: "Chat with Elena (1 hour)"
- Explore: "Explore Market District (2 hours)"
- Special Letter Requests: "Negotiate Introduction (1 hour)"

#### 6.3 Deadline Impact Warnings
Before actions that consume time:
- Check letter deadlines
- Warn if action would cause expiration
- "⚠️ This action will cause 2 letters to expire!"

### Phase 7: Leverage Visibility
**Goal**: Make token debt consequences clear

#### 7.1 Queue Position Indicators
- Show WHY letters enter at certain positions
- "💸 Debt leverage: Position 2 → Position 1"
- "👑 Noble privilege: Position 3 → Position 2"

#### 7.2 Skip Cost Modifiers
- Show base cost and multipliers clearly
- "Skip Cost: 3 tokens (base) × 2 (obligation) = 6 tokens"
- Explain each modifier

#### 7.3 Debt Warning System
When player has negative tokens:
- Red highlighting on affected NPCs
- Warning icons in queue
- "⚠️ Your debt to Marcus gives him queue priority!"

### Phase 8: Standing Obligation Integration
**Goal**: Show permanent rule changes from token thresholds

#### 8.1 Active Obligations Display
- Dedicated UI panel showing active obligations
- Icon indicators on affected systems
- Tooltip explanations

#### 8.2 Threshold Warnings
Before crossing important thresholds:
- "⚠️ Reaching -5 tokens will trigger 'Creditor Priority'"
- "✨ Reaching 10 tokens will unlock 'Trusted Courier'"
- Give player choice to proceed

#### 8.3 Obligation Effects UI
- Highlight modified game rules
- Show in relevant contexts:
  - Queue: "📌 Patron letters locked to Position 1"
  - Skip costs: "💰 Double cost due to 'Noble Expectations'"

## Implementation Order (Updated)
1. ✅ Special Letter Generation (enables core token→letter loop)
2. ✅ Information Letter Satchel (completes special letter types)  
3. ✅ Multi-Context Token Display (makes relationships visible)
4. ⏳ Time Cost System (adds pressure visibility) - IN PROGRESS
5. Route Discovery (uses information system)
6. Endorsement-to-Seal (completes progression)
7. Leverage Visibility (explains queue behavior)
8. Standing Obligations (shows permanent changes)

Note: Phase 5 was prioritized over 3 & 4 for UI visibility needs.

## Testing Strategy
- Unit tests for each new service
- Integration tests for system interactions
- Manual playtesting of complete loops:
  - Earn tokens → Request special letter → Deliver → Gain benefit
  - Discover route → Meet requirements → Travel new path
  - Collect endorsements → Visit guild → Upgrade seal
  - Go into debt → See leverage → Experience consequences

## Success Criteria
- Player can see and interact with every system
- No silent state changes
- Clear cause-and-effect for all mechanics
- Complete information available to make decisions
- All four token contexts create unique gameplay loops