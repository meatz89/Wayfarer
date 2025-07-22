# UI PRINCIPLES

This document defines the core UI/UX principles and patterns that must be maintained for visual consistency and user experience quality in Wayfarer.

## CORE UI PHILOSOPHY

### Letter Queue Centricity
**The Letter Queue is the heart of the game - it should always be one click away from any screen.**

- Primary navigation bar always visible
- Queue button highlighted when away from queue
- Visual reminders of queue pressure (deadlines, full slots)
- Queue state persists across navigation

### Visual Hierarchy
**Information importance drives visual prominence.**

1. **Critical Information** - Large, high contrast, centered
   - Letter deadlines approaching
   - Queue at capacity
   - Token debt warnings
   
2. **Primary Information** - Medium size, clear positioning
   - Current location
   - Available actions
   - Resource counts
   
3. **Secondary Information** - Smaller, muted colors
   - Flavor text
   - Historical data
   - Optional details

### Mechanical Transparency
**Every mechanical effect must be clearly communicated before and after actions.**

#### Before Actions
- Show exact costs (time, tokens, stamina)
- Display success chances if applicable
- Preview outcomes clearly
- Highlight risks and opportunities

#### After Actions
- Animate state changes
- Show numerical feedback (+3 coins, -1 token)
- Narrative context for mechanical changes
- Clear success/failure indication

## CSS ARCHITECTURE

### Variable-First Design
**Always use CSS variables for consistency.**

```css
/* ✅ CORRECT: Use CSS variables */
.letter-card {
    background: var(--bg-panel);
    color: var(--text-primary);
    border: 1px solid var(--border-primary);
}

/* ❌ WRONG: Hardcoded values */
.letter-card {
    background: #2a2a2a;
    color: #ffffff;
    border: 1px solid #444;
}
```

### Core Variables
- `--text-primary`: Main text color
- `--text-secondary`: Muted text
- `--text-success`: Positive outcomes
- `--text-danger`: Warnings and failures
- `--bg-primary`: Main background
- `--bg-panel`: Card/panel backgrounds
- `--border-primary`: Standard borders
- `--shadow-standard`: Consistent shadows

### File Organization
1. **Separate CSS files** - Never inline styles
2. **Component-based** - One CSS file per component
3. **Shared styles** - Common patterns in shared.css
4. **No !important** - Proper specificity instead

## COMPONENT PATTERNS

### Cards and Panels
**Consistent visual containers for grouped information.**

```css
.panel {
    background: var(--bg-panel);
    border-radius: 8px;
    padding: 1rem;
    box-shadow: var(--shadow-standard);
}
```

### Interactive Elements
**Clear affordances for all interactive elements.**

#### Buttons
- Hover states with color shift
- Active states with slight depression
- Disabled states with reduced opacity
- Clear primary/secondary distinction

#### Choice Lists
- Hover highlights entire choice
- Selected state clearly visible
- Disabled choices visually distinct
- Affordability indicators (✓/✗)

### Status Indicators
**Consistent visual language for game state.**

#### Token Display
- Color-coded by type (Trust=blue, Trade=gold, etc.)
- Debt shown as negative with red
- Animated on change

#### Time Display
- Current time block clearly shown
- Time progression animated
- Deadline warnings escalate visually

## NAVIGATION PRINCIPLES

### Three-Click Rule
**Any game action should be accessible within 3 clicks from anywhere.**

1. Return to queue (1 click)
2. Navigate to context (1 click)  
3. Perform action (1 click)

### Context Preservation
**Navigation maintains player context.**

- Selected NPC remains selected
- Scroll positions preserved
- Expanded/collapsed states maintained
- Filter settings persist

### Breadcrumb Clarity
**Player always knows where they are.**

- Visual breadcrumb trail
- Screen titles prominent
- Parent context visible
- Back button always available

## RESPONSIVE DESIGN

### Mobile-First Approach
**Design for smallest screens first.**

- Touch-friendly tap targets (min 44px)
- Scrollable containers
- Collapsible sections
- Bottom-sheet patterns for actions

### Breakpoint Strategy
```css
/* Mobile: < 768px */
/* Tablet: 768px - 1024px */
/* Desktop: > 1024px */
```

### Adaptive Layouts
- Stack vertically on mobile
- Side-by-side on tablet+
- Multi-column on desktop
- Maintain readability at all sizes

## ANIMATION PRINCIPLES

### Purpose-Driven Animation
**Every animation must serve a purpose.**

#### State Changes
- Fade transitions for content swaps
- Slide transitions for navigation
- Scale transitions for emphasis
- No animation > bad animation

#### Feedback Animation
- Number changes tick up/down
- Progress bars fill smoothly
- Success/failure pulses
- Token additions fly to counter

### Performance Considerations
- Use CSS transforms only
- Avoid layout thrashing
- Respect prefers-reduced-motion
- Keep durations under 300ms

## ACCESSIBILITY REQUIREMENTS

### Color and Contrast
- WCAG AA minimum contrast ratios
- Never rely on color alone
- Consistent color meanings
- High contrast mode support

### Keyboard Navigation
- All actions keyboard accessible
- Visible focus indicators
- Logical tab order
- Skip links for navigation

### Screen Reader Support
- Semantic HTML structure
- ARIA labels where needed
- Live regions for updates
- Descriptive button text

## ERROR HANDLING UI

### Graceful Degradation
**Errors should never break the UI.**

#### User Errors
- Clear error messages
- Suggest corrections
- Maintain form state
- Allow easy retry

#### System Errors
- Friendly error screens
- Maintain navigation
- Offer alternatives
- Log for debugging

## LOADING STATES

### Progressive Loading
**Show something immediately.**

1. **Skeleton screens** - Show layout structure
2. **Placeholder content** - Generic shapes
3. **Partial data** - Load critical first
4. **Complete data** - Fill in details

### Loading Indicators
- Spinner for short waits (<3s)
- Progress bar for long operations
- Cancel option when possible
- Time estimates when available

## ONBOARDING FLOW

### Progressive Disclosure
**Don't overwhelm new players.**

1. **Core mechanic first** - Letter queue basics
2. **Add complexity gradually** - Tokens, routes, etc.
3. **Context-sensitive help** - Tooltips on hover
4. **Always skippable** - Respect returning players

### Tutorial Integration
- Non-blocking hints
- Highlight relevant UI
- Practice in safe environment
- Achievement milestones

## TESTING CHECKLIST

Before implementing any UI:

- [ ] Uses CSS variables for all colors
- [ ] Follows visual hierarchy principles  
- [ ] Maintains navigation consistency
- [ ] Includes loading states
- [ ] Handles errors gracefully
- [ ] Keyboard accessible
- [ ] Mobile responsive
- [ ] Animations purposeful
- [ ] Mechanics clearly communicated
- [ ] Three-click rule maintained

## COMMON ANTI-PATTERNS TO AVOID

### ❌ Modal Overuse
- Modals interrupt flow
- Use inline editing instead
- Sheet patterns for mobile
- Preserve context

### ❌ Mystery Meat Navigation
- Icons need labels
- Unclear destinations
- Hidden functionality
- Gesture-only actions

### ❌ Walls of Text
- Break into chunks
- Use visual hierarchy
- Progressive disclosure
- Scannable formatting

### ❌ Inconsistent Patterns
- Mixed button styles
- Varying spacing
- Different interactions
- Conflicting colors

---

**Remember: The UI serves the letter queue optimization puzzle. Every element should help players make better decisions about their queue.**