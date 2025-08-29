# UI Implementation Plan - 1:1 Mockup Matching

## Current State
- Medieval theming has been removed (completed)
- Base CSS structure exists but doesn't match mockups
- Components have wrong structure and missing elements
- Fonts, colors, and layouts don't match mockup specifications

## Target State
Perfect 1:1 visual match with UI mockups where:
- All styling matches exactly (fonts, colors, spacing)
- All UI elements are present and correctly positioned
- All content is dynamically generated from JSON or game mechanics
- No hardcoded text or values

## Work Packages

### Package 1: CSS Foundation & Typography
**Agent: game-design-reviewer**
**Priority: CRITICAL**
**Status: PENDING**

Tasks:
1. Create `/src/wwwroot/css/mockup-clean.css` with exact mockup styles
2. Update font loading in `_Layout.cshtml` to use Garamond properly
3. Implement exact color palette from mockups:
   - Background: #faf4ea
   - Cards: #e8dcc4
   - Text: #2c241a
   - Secondary: #7a6250
4. Remove all gradients, shadows, and ornamental effects
5. Implement clean card layouts matching mockup exactly

Files to modify:
- Create: `/src/wwwroot/css/mockup-clean.css`
- Update: `/src/Pages/_Layout.cshtml`
- Update: `/src/wwwroot/css/game-base.css`
- Update: `/src/wwwroot/css/common.css`

### Package 2: Location Screen Structure
**Agent: content-integrator**
**Priority: HIGH**
**Status: PENDING**

Tasks:
1. Fix obligations queue panel styling
2. Implement proper NPC card layout with token strips
3. Fix observation card display with rewards
4. Ensure all NPC dialogue and descriptions come from JSON
5. Remove any remaining medieval class names

Files to modify:
- `/src/Pages/Components/LocationContent.razor`
- `/src/Pages/Components/LocationContent.razor.cs`
- `/src/wwwroot/css/location.css`

Key changes:
- Obligations panel: Show queue positions 1-3 with arrows
- NPC cards: Display Trust/Commerce/Status/Shadow tokens in strip
- Observations: Show "+Card" reward and attention cost
- All text from JSON files or game mechanics

### Package 3: Conversation Screen Structure
**Agent: narrative-designer**
**Priority: HIGH**
**Status: PENDING**

Tasks:
1. Add comfort dots visualization (-3 to +3 range)
2. Implement token strip showing current tokens
3. Fix card layout with side panels for success/failure
4. Add turn counter and state info display
5. Ensure all card text comes from JSON

Files to modify:
- `/src/Pages/Components/ConversationContent.razor`
- `/src/Pages/Components/ConversationContent.razor.cs`
- `/src/wwwroot/css/conversation.css`

Key structures needed:
```html
<!-- Comfort Dots -->
<div class="comfort-dots">
  <span class="dot negative"></span>
  <span class="dot negative"></span>
  <span class="dot negative"></span>
  <span class="dot neutral active"></span>
  <span class="dot positive"></span>
  <span class="dot positive"></span>
  <span class="dot positive"></span>
</div>

<!-- Token Strip -->
<div class="token-strip">
  <div class="token-item">Trust: 2</div>
  <div class="token-item">Commerce: 1</div>
  <div class="token-item">Status: 0</div>
  <div class="token-item">Shadow: 0</div>
</div>
```

### Package 4: Card Component Redesign
**Agent: game-mechanics-designer**
**Priority: HIGH**
**Status: PENDING**

Tasks:
1. Create unified card component matching mockup exactly
2. Implement side panels for success/failure outcomes
3. Show all costs and effects visibly (Perfect Information)
4. Ensure cards look like cards, not buttons
5. Implement proper hover and selection states

Files to modify:
- Create: `/src/Pages/Components/CardDisplay.razor`
- Update: `/src/wwwroot/css/unified-cards.css`

Card structure from mockup:
```html
<div class="dialog-card">
  <div class="card-header">
    <span class="card-type">Promise</span>
    <span class="card-cost">2 attention</span>
  </div>
  <div class="card-body">
    <div class="card-text">Card narrative text here</div>
    <div class="card-outcomes">
      <div class="outcome success">Success: +1 Trust</div>
      <div class="outcome failure">Failure: -1 Trust</div>
    </div>
  </div>
</div>
```

### Package 5: Dynamic Content Loading
**Agent: systems-architect**
**Priority: MEDIUM**
**Status: PENDING**

Tasks:
1. Ensure all NPC names, descriptions come from JSON
2. Load all card text from cards.json
3. Generate atmospheric descriptions from categorical properties
4. Remove ALL hardcoded text
5. Implement proper data flow from GameWorld to UI

Files to verify:
- `/src/Data/npcs.json`
- `/src/Data/cards.json`
- `/src/Data/locations.json`
- All component code-behind files

### Package 6: Resource Bar Standardization
**Agent: wayfarer-design-auditor**
**Priority: MEDIUM**
**Status: PENDING**

Tasks:
1. Implement consistent resource bar across all screens
2. Show Coins/Health/Hunger/Attention always visible
3. Use exact styling from mockup
4. Ensure values update dynamically from GameWorld
5. Fix positioning and layout

Files to modify:
- `/src/Pages/Components/ResourceBar.razor`
- Create if doesn't exist as separate component
- Update all screens to use this component

### Package 7: Final Validation
**Agent: change-validator**
**Priority: LOW**
**Status: BLOCKED (waiting for other packages)**

Tasks:
1. Take screenshots of all screens
2. Compare pixel-by-pixel with mockups
3. Verify all dynamic content works
4. Check no hardcoded text remains
5. Ensure build succeeds without errors

Validation checklist:
- [ ] Fonts are Garamond throughout
- [ ] Colors match exactly (#faf4ea, #e8dcc4, etc.)
- [ ] No medieval ornaments or textures
- [ ] All UI elements present from mockups
- [ ] Content loads from JSON/mechanics
- [ ] Cards display as cards, not buttons
- [ ] Resources always visible
- [ ] Perfect Information principle maintained

## Implementation Order
1. Package 1 (CSS Foundation) - MUST complete first
2. Packages 2-4 (Component structures) - Can run in parallel
3. Package 5 (Dynamic content) - After component work
4. Package 6 (Resource bar) - Can run anytime
5. Package 7 (Validation) - After all others complete

## Success Metrics
- Screenshots match mockups exactly
- No visual elements from medieval theme remain
- All content dynamically generated
- Clean, minimal aesthetic achieved
- Game mechanics clearly visible to player