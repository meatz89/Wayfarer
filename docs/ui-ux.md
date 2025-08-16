# Wayfarer: UI/UX Specialist Perspective

## Making Complex Systems Intuitive

### The Core UI Challenge

Wayfarer must simultaneously display:
- Queue state (8 slots, weights, deadlines)
- NPC locations and availability
- Conversation state (patience, comfort, cards)
- Route network and travel options
- Time pressure (multiple deadlines)
- Resource states (attention, coins, tokens)
- Relationship matrices (4 types × 20+ NPCs)

Without overwhelming the player or breaking immersion.

### The Cognitive Load Solution: Modal Focus

**Map Mode** (Primary Navigation)
- Full-screen medieval city map
- NPCs shown as portraits at locations
- Emotional states via portrait frames (red=desperate, yellow=anxious)
- Queue as minimal sidebar showing position 1 deadline
- Routes highlighted by availability
- Click location to see who's there
- Click NPC to initiate conversation
- Time/attention/coins in top bar

**Conversation Mode** (Deep Engagement)
- NPC portrait center, large and expressive
- Patience bar below portrait (drains as cards played)
- Comfort meter on left (builds toward thresholds)
- 5 cards drawn at bottom, requirements clearly shown
- Success percentage displayed on each card
- Queue minimized but deadline for position 1 still visible
- Peripheral awareness: Other NPCs at location shown as small portraits

**Queue Management Mode** (Tactical Planning)
- Full queue display with all 8 slots
- Drag and drop to reorder (shows token cost preview)
- Each letter shows: sender portrait, deadline timer, weight, type icon
- Visual weight indicator (bags filling up)
- Displacement preview: hovering shows which tokens would burn
- Filter by deadline urgency or sender

**Route Planning Mode** (Travel Optimization)
- Network view of city connections
- Travel times displayed on each route
- Locked routes shown grayed with requirement icons
- Current position highlighted
- Optimal path suggestion (dotted line)
- Cost/time trade-off for each transport type

### Information Hierarchy

**Always Visible** (Critical Info):
- Current attention points
- Coins
- Position 1 deadline
- Current time

**Context Visible** (Mode Relevant):
- In conversation: patience, comfort, cards
- In map: NPC locations, emotional states
- In queue: all letters, weights, deadlines
- In route: connections, times, costs

**Available on Demand** (Deep Dive):
- Full relationship matrix (hold Tab)
- NPC history log (right-click portrait)
- Letter contents (hover over letter)
- Route requirements (hover over locked route)

### Visual Language

**Color Coding**:
- Trust: Warm blue (personal, calming)
- Commerce: Gold (wealth, trade)
- Status: Purple (nobility, formality)
- Shadow: Dark green (secrets, hidden)
- Desperate: Red (urgent, crisis)
- Anxious: Orange (worried, time pressure)
- Neutral: Gray (calm, stable)
- Hostile: Dark red (angry, blocked)

**Icons**:
- Letter types: Sealed scroll (trust), Coin purse (commerce), Coat of arms (status), Hooded figure (shadow)
- Deadlines: Clock face with colored urgency
- Weight: Bag icons (1-3 bags)
- Routes: Footprints (walking), Wheel (cart), Carriage (premium)

**Animation Language**:
- Patience draining: Slow fade of bar
- Comfort building: Gentle fill with glow at thresholds
- Deadline pressure: Subtle pulse as time runs low
- Card success: Green highlight on success, red shake on failure
- Token burning: Visual flame effect when displacing

### Reducing Complexity Through Smart Defaults

**Conversation Cards**:
- Auto-sort by success probability
- Highlight cards you can afford
- Dim cards you can't play
- Auto-reveal requirements on hover

**Queue Management**:
- Default to optimal position based on relationships
- Warning before accepting letter that would overflow
- Auto-calculate displacement costs
- Suggest reordering for deadline optimization

**Route Planning**:
- Highlight fastest route by default
- Show alternative routes with trade-offs
- Remember preferred transport types
- Quick-travel to previously visited locations

### The Learning Curve

**Tutorial Integration**:
- First letter pre-placed teaches queue basics
- First conversation guided by comfort thresholds
- First displacement shows token cost clearly
- First route choice presents clear trade-off
- Each system introduced when relevant, not front-loaded

**Progressive Disclosure**:
- Start with basic conversation cards
- Introduce requirements as relationships build
- Show route options as they unlock
- Add queue complexity as more letters accepted

**Feedback Systems**:
- Clear success/failure states
- Relationship changes shown as +1/-1 floaters
- Deadline warnings at 2 hours, 1 hour, 30 minutes
- Queue overflow prevention warnings
- Route lock explanations on hover

### Accessibility Considerations

**Colorblind Modes**:
- Shapes in addition to colors
- Patterns for different token types
- Text labels option

**Difficulty Options**:
- Deadline timer speed adjustment
- Hint system for optimal routes
- Undo last action (with cost)
- Conversation success percentages shown/hidden

**Quality of Life**:
- Queue sorting options
- Batch delivery planning
- Favorite NPC marking
- Letter history log
- Relationship change history

### The Moment-to-Moment Experience

**Approaching an NPC**:
1. See emotional state from portrait border
2. Hover to preview patience level
3. Click to enter conversation
4. See cards draw with clear costs/requirements
5. Make informed decision
6. See immediate result
7. Understand consequences

**Managing Queue**:
1. See new letter entry position
2. Preview displacement cost
3. Drag to reorder if needed
4. Confirm token burn
5. See new arrangement
6. Understand trade-off

**Planning Route**:
1. See current position on map
2. View available destinations
3. Compare route options
4. See time/cost trade-offs
5. Select transport type
6. Watch travel progress

### The Power User Experience

**Keyboard Shortcuts**:
- Tab: Full relationship matrix
- Q: Queue management
- M: Map mode
- 1-5: Select conversation cards
- Space: Confirm action
- Esc: Cancel/Exit

**Advanced Information**:
- Optimal route calculator
- Deadline collision warnings
- Token burn predictions
- Conversation probability calculator
- Daily efficiency metrics

**Planning Tools**:
- Pin multiple destinations
- Route chain planning
- Queue forecast view
- Relationship goal tracking
- Note system for NPC details

### Making Failure Feel Fair

**Clear Warning Systems**:
- Deadline alerts escalate visually
- Overflow warnings before accepting
- Displacement costs shown clearly
- Success percentages transparent
- Requirements explained

**Recovery Visibility**:
- How to fix hostile relationships
- Emergency options highlighted
- Alternative paths shown
- Coin solutions available
- Shadow leverage explained

### The Aesthetic Wrapper

All UI elements wrapped in medieval aesthetic:
- Parchment textures for cards
- Wax seals for letters
- Hand-drawn map style
- Ink splatter transitions
- Quill pen cursor
- Period-appropriate fonts

But clarity never sacrificed for aesthetics:
- High contrast text
- Clear iconography
- Consistent visual language
- Responsive feedback
- Smooth animations

This UI/UX design ensures that despite Wayfarer's complex dual-layer systems, players always understand their options, the consequences of their choices, and how to achieve their goals. The interface guides without constraining, informs without overwhelming, and maintains immersion while providing critical gameplay information.