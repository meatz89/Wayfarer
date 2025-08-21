# UI Mockup Implementation Plan - Exact Screens with JSON-Driven Content

## Overview
Implement the EXACT UI from HTML mockups with ALL content systematically generated from JSON data and game mechanics, matching the mockups precisely.

## Current Status
Started: 2025-08-21
Last Updated: 2025-08-21 (Session 14)
Status: ⚠️ PARTIAL PROGRESS - Mechanics work, UI completely wrong

## 🔥 CRITICAL DISCOVERY (Session 14)
**Found actual mockup**: `/mnt/c/git/wayfarer/UI-MOCKUPS/conversation-screen.html`
- Current CSS is COMPLETELY WRONG
- Cards missing type system (COMFORT/STATE/CRISIS)
- Progress containers have wrong dimensions
- No visual hierarchy or proper styling

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

## Phase 4: Conversation Screen ❌ NEEDS COMPLETE REWRITE

### Current Problems:
1. **CSS is WRONG**:
   - Progress: `minmax(350px, 1fr)` should be `2fr 1fr 2fr`
   - Cards: No type-based styling (comfort/state/crisis)
   - Heights: Everything vertically squished
   - Width: 900px+ minimum explodes screen

2. **Card Structure Missing**:
   - No card header with name/tags
   - No weight display (should be dots or number)
   - No outcome grid (success/failure columns)
   - No persistence icons (♻⚡→⚫)

3. **Card Types Not Implemented**:
   ```css
   /* FROM MOCKUP - NEEDED */
   .dialog-card.comfort { border-left: 5px solid #7a8b5a; }
   .dialog-card.state { border-left: 5px solid #8b7355; }
   .dialog-card.crisis { 
       border-left: 5px solid #8b4726;
       background: #faf0e6;
   }
   ```

### What Needs to be Done:

#### 4.1 Fix ConversationCard.cs Model:
- [ ] Add CardType enum (COMFORT, STATE, CRISIS)
- [ ] Ensure Type property maps to CardType
- [ ] Add methods for CSS class generation

#### 4.2 Rewrite conversation.css:
- [ ] Copy EXACT values from mockup HTML (lines 1-600)
- [ ] Remove all minmax() nonsense
- [ ] Fix grid dimensions to match mockup
- [ ] Add proper card type styling

#### 4.3 Update ConversationScreen.razor:
- [ ] Apply card type CSS classes
- [ ] Add persistence icons
- [ ] Structure cards with header/body/footer
- [ ] Create outcome grid display

#### 4.4 Visual Requirements:
Each card MUST have:
- Type-specific left border (5px solid color)
- Header: Name, type tag, persistence icon, weight
- Body: Dialogue text with proper padding
- Footer: Success/failure grid with percentages
- Hover effects per type

## Phase 5: Location Screen ⚠️ PARTIALLY WORKING
- Actions display correctly
- Observations work
- NPCs show emotional states
- Missing some visual polish

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

## Root Cause Analysis:
```css
.dialog-option {
    overflow: hidden;  /* ← THIS IS THE KILLER */
    min-height: 100px; /* ← TOO SMALL (needs 280px) */
}
.card-header {
    margin: -15px -15px 10px; /* ← PUSHES HEADER OUTSIDE VISIBLE AREA */
}
```

## Session 16 Implementation Plan:
1. **FIX OVERFLOW** - Remove `overflow: hidden` from `.dialog-option`
2. **FIX HEIGHT** - Change `min-height: 100px` to `min-height: 280px`  
3. **FIX MARGINS** - Change card-header margin to `0 0 10px`
4. **FIX BORDERS** - Make 5px colored borders more prominent
5. **ADD CARD TYPES** - Create STATE and CRISIS cards in NPCDeckFactory
6. **TEST ALL TYPES** - Verify COMFORT, STATE, CRISIS all display correctly

## Success Criteria:
- [ ] Cards visually distinct by type (colored borders)
- [ ] Persistence icons display correctly
- [ ] Progress containers readable (proper height)
- [ ] Screen doesn't explode horizontally
- [ ] Outcome grids show success/failure
- [ ] Hover effects work per card type
- [ ] Matches mockup EXACTLY