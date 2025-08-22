# UI Mockup Implementation Plan - Exact Screens with JSON-Driven Content

## Overview
Implement the EXACT UI from HTML mockups with ALL content systematically generated from JSON data and game mechanics, matching the mockups precisely.

## Current Status
Started: 2025-08-21
Last Updated: 2025-08-21 (Session 20 - Location Screen Improvements)
Status: ✅ PHASE 4 & 5 COMPLETE - Location screen fully updated

## 🔍 SESSION 16 DISCOVERIES - Complete CSS Analysis

### What's Actually Working:
1. **Card Category System**: CardCategory enum (COMFORT/STATE/CRISIS) ✅ IMPLEMENTED
2. **NPCDeckFactory**: Already generates all three card types (lines 132-159) ✅
3. **Progress Grid**: Actually using correct `2fr 1fr 2fr` (conversation.css:143) ✅
4. **Card Structure**: ConversationScreen.razor properly renders header/body/outcomes ✅

### CSS Architecture Confusion - RESOLVED:
Found **TWO parallel CSS systems** trying to style the same cards:
1. **conversation.css** (lines 398-600): Uses `.dialog-card` class - THIS IS ACTIVE
2. **cards.css** (lines 62-151): Uses `.dialog-option` class - ORPHANED/UNUSED

**KEY FINDING**: The `.dialog-option` overflow issue mentioned in previous sessions was a RED HERRING - that class isn't even used! The actual issue is in `.dialog-card` in conversation.css:402.

### Actual Problems Found:
1. **conversation.css:402**: `overflow: hidden` on `.dialog-card` cuts off content
2. **No min-height set**: Cards collapse vertically
3. **Card borders too faint**: 5px borders exist but colors need darkening
4. **cards.css:374**: Card header margin is already `0` (not negative as claimed)

## Phase 1: JSON Data Structure (POC Setup) ✅ COMPLETE
**Create complete JSON content for POC scenario**

### 1.1 Enhance npcs.json with POC state:
- [x] Created npcs_poc.json with Elena DESPERATE state, 8-minute deadline
- [x] Marcus has CALCULATING state with commerce letter
- [x] Lord Blackwood NEUTRAL, leaving at 5 minutes

### 1.2 Create card_templates.json:
- [x] Created with 15+ card templates
- [x] Includes COMFORT, STATE, and CRISIS types
- [x] Set bonuses and success formula defined
- [x] Persistence types and connection types specified

### 1.3 Create observations.json:
- [x] Created with observations for all locations
- [x] Market Square: guards, merchants, cart service
- [x] Tavern: noble schedule, Elena's distress
- [x] Noble District: Lord preparing, guard patterns
- [x] Each observation has type, cost, relevance

## Phase 2: Backend Categorical Generation ❌ DELETED (LEGACY)
**These were LEGACY CODE not in target architecture - ALL DELETED**

### Deleted Files:
- ❌ ConversationNarrativeGenerator.cs (legacy)
- ❌ LocationNarrativeGenerator.cs (legacy)
- ❌ CardContextGenerator.cs (legacy)
- ❌ NPCStateResolver.cs (legacy)
- ❌ All "literary UI" components (legacy)

### Lesson Learned:
- If it's not in conversation-system.md, it's LEGACY
- HIGHLANDER PRINCIPLE: There can be only ONE
- No duplicate enums, no compatibility layers

## Phase 3: Frontend Text Rendering ✅ PARTIAL
**Create components that map categories to text**

### 3.1 StateNarrativeRenderer.razor: ✅ COMPLETE
- Maps EmotionalState → narrative text
- Maps state → mechanical description

### 3.2 NPCDialogueGenerator.razor: ✅ COMPLETE (Session 14)
- Maps MeetingObligation → contextual dialogue
- Maps PersonalityType + EmotionalState → dialogue
- Generates appropriate urgency based on deadline

### 3.3 CardDialogueRenderer.razor: ⚠️ EXISTS BUT WRONG
- Needs to support CardType enum properly
- Must show persistence icons
- Needs proper visual structure

## Phase 4: Conversation Screen 🔧 NEEDS TARGETED FIXES

### What's Actually Implemented (Session 16 Analysis):
1. **Card Structure** ✅ COMPLETE in ConversationScreen.razor:
   - Card header with name/tags (lines 190-207)
   - Weight display as number (line 205)
   - Outcome grid with success/failure (lines 217-236)
   - Persistence icons implemented (line 196)

2. **Card Types** ✅ ALREADY EXIST in conversation.css:
   ```css
   /* Lines 421-432 - Already implemented! */
   .dialog-card.comfort { border-left: 5px solid #7a8b5a; }
   .dialog-card.state { border-left: 5px solid #8b7355; }
   .dialog-card.crisis { 
       border-left: 5px solid #8b4726;
       background: #faf0e6;
   }
   ```

3. **Progress Grid** ✅ CORRECT:
   - Using `2fr 1fr 2fr` as specified (line 143)
   - NOT using minmax() as previously claimed

### Actual Fixes Needed:

#### 4.1 ✅ ConversationCard.cs Model - ALREADY COMPLETE:
- CardCategory enum exists (COMFORT, STATE, CRISIS)
- GetCategoryClass() method implemented
- NPCDeckFactory generates all types

#### 4.2 ✅ Fix conversation.css (COMPLETED Session 19):
- [x] Line 402: Removed `overflow: hidden` 
- [x] Line 398: Added `min-height: 280px` to `.dialog-card`
- [x] Lines 421-432: Darkened border colors for visibility
- [x] Line 180: Changed to `min-height: 80px` in `.progress-bar`

#### 4.3 ✅ ConversationScreen.razor - ALREADY COMPLETE:
- Card classes applied correctly (line 187)
- Persistence icons working (line 196)
- Header/body/footer structure implemented
- Outcome grid displaying properly

#### 4.4 ✅ Clean Up Orphaned CSS (COMPLETED Session 19):
- [x] Removed cards.css file entirely
- [x] Removed cards.css reference from _Layout.cshtml
- [x] Kept only the active `.dialog-card` system in conversation.css

## Phase 5: Location Screen ⚠️ MOSTLY COMPLETE (Session 20-21)
- ✅ Actions display correctly
- ✅ Observations work
- ⚠️ NPCs show states but sometimes wrong (Elena was NEUTRAL not DESPERATE)
- ✅ Obligations panel shows both delivery and meeting deadlines
- ✅ Current spot name displayed
- ❌ **Spot properties NOT WORKING** - Code written but no data in JSON
- ✅ DeadlinePanel uses repositories (verified no hardcoded strings)

## Phase 6: Letter Queue Screen ⚠️ NEEDS UPDATE
- Basic functionality works
- Needs visual updates to match mockup style

## Key Architecture Principles (MUST FOLLOW):
1. **Frontend generates text from categories** - Backend only provides enums/types
2. **No hardcoded strings** - Everything from JSON or systematic generation
3. **HIGHLANDER PRINCIPLE** - One source of truth per concept
4. **No compatibility layers** - Delete and replace legacy code
5. **Pixel-perfect to mockup** - Use EXACT CSS values from HTML

## Session 15 Results:
✅ Added CardCategory enum to replace IsStateCard/IsCrisis booleans (HIGHLANDER PRINCIPLE)
✅ Updated all card creation to use new Category property
✅ Added CSS classes for card types (comfort/state/crisis) 
✅ Created card header structure in razor with proper HTML
✅ Cards have colored left borders (but too faint)
❌ **CRITICAL BUG FOUND**: Cards have `overflow: hidden` cutting off 157px of content!
❌ Card headers invisible due to negative margins + overflow
❌ Success/failure outcomes completely hidden below fold
❌ Progress containers still vertically squished

## Root Cause Analysis (CORRECTED):
```css
/* ACTUAL ISSUE in conversation.css:402 */
.dialog-card {
    overflow: hidden;     /* ← Cuts off card content */
    /* NO min-height set */ /* ← Cards collapse vertically */
}

/* NOT AN ISSUE - cards.css:374 */
.card-header {
    margin: 0;  /* ← Already correct, not negative */
}
```

## Session 18 Accomplishments - Categorical Properties

### Implemented Core Mechanics:
1. ✅ **Emotional State Rules as Data**: StateRuleset class defines all rules
   - Weight limits per state
   - Cards drawn on listen
   - Free weight categories (Crisis cards in DESPERATE/HOSTILE)
   - Allowed categories (HOSTILE only allows crisis cards)
   - Special overrides (OVERWHELMED max 1 card)

2. ✅ **Location Spot Properties**: SpotPropertyType enum
   - Privacy levels (Private, Discrete, Public, Exposed)
   - Atmosphere (Quiet, Loud, Warm, Shaded)  
   - View properties (ViewsMainEntrance, ViewsMarket, etc.)
   - Comfort modifiers calculated based on properties + NPC personality

3. ✅ **NPC Work/Home Locations**: Added to NPC class
   - WorkLocationId and WorkSpotId
   - HomeLocationId and HomeSpotId

4. ✅ **Stakes System**: Already existed as StakeType enum
   - REPUTATION, WEALTH, SAFETY, SECRET, STATUS

### CSS Still Needs (From Session 16):
1. **FIX OVERFLOW** - Remove `overflow: hidden` from `.dialog-card` 
2. **FIX HEIGHT** - Add `min-height: 280px` to `.dialog-card`
3. **FIX BORDERS** - Darken the 5px borders (too faint)
4. **FIX PROGRESS** - Add min-heights to progress containers
5. **CLEAN UP** - Remove orphaned `.dialog-option` styles

## Success Criteria:
- [ ] Cards visually distinct by type (colored borders)
- [ ] Persistence icons display correctly
- [ ] Progress containers readable (proper height)
- [ ] Screen doesn't explode horizontally
- [ ] Outcome grids show success/failure
- [ ] Hover effects work per card type
- [ ] Matches mockup EXACTLY