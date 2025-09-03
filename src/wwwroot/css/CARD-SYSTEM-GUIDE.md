# Unified Card System Design Guide

## Overview
The unified card system provides consistent sizing and styling for all interactive card elements in the Wayfarer UI. This ensures visual consistency and reduces cognitive load by presenting all choices with uniform visual focus.

## Core Principles

### 1. **Single Source of Truth**
All card components inherit from `.card-base` which defines:
- Consistent padding (14px standard)
- Unified border treatment (2px solid)
- Standard shadows and transitions
- Hover/active/disabled states

### 2. **Three Size Variants**
- **Compact** (10px padding): For grids with many items (spots, NPC actions)
- **Standard** (14px padding): Default size for most cards
- **Large** (18px padding): For emphasis (route cards, major choices)

### 3. **Type Indicators**
Left border colors indicate card type:
- **Flow**: Green (#7a8b5a) - Positive/supportive cards
- **State**: Brown (#8b7355) - Neutral state changes
- **Crisis**: Orange (#8b4726) - Urgent/negative cards
- **Observation**: Blue (#5a7a8a) - Information cards
- **Exchange**: Green (#7a8b5a) - Trade/exchange cards
- **Burden**: Dark (#4a3a2a) - Permanent negative cards

## Usage Examples

### Basic Card
```html
<div class="card-base card-standard">
    <div class="card-header">
        <h3 class="card-title">Card Title</h3>
    </div>
    <div class="card-body">
        Card content goes here
    </div>
</div>
```

### Typed Card with Badge
```html
<div class="card-base card-standard card-flow">
    <span class="card-badge free">FREE</span>
    <div class="card-header">
        <h3 class="card-title">Flow Card</h3>
    </div>
    <div class="card-body">
        This card has no cost
    </div>
</div>
```

### Grid Layout
```html
<div class="cards-grid-2">
    <div class="card-base card-compact spot-card">Spot 1</div>
    <div class="card-base card-compact spot-card">Spot 2</div>
    <div class="card-base card-compact spot-card">Spot 3</div>
    <div class="card-base card-compact spot-card">Spot 4</div>
</div>
```

## Migration Path

### Old Classes → New Classes
- `.spot-card` → `.card-base card-compact`
- `.action-card` → `.card-base card-standard`
- `.npc-card` → `.card-base card-standard`
- `.dialog-card` → `.card-base card-standard`
- `.route-card` → `.card-base card-large`

### CSS Import Order
1. `common.css` (imports unified-cards.css)
2. Screen-specific CSS (game-screen.css, location.css, etc.)
3. `medieval-fixes.css` (adds texture overlays)

## Grid Systems

### Responsive Grid
```css
.cards-grid-responsive {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 12px;
    align-items: stretch; /* Forces equal heights */
}
```

### Fixed Columns
- `.cards-grid-2`: 2 columns
- `.cards-grid-3`: 3 columns
- `.cards-list`: Single column vertical list

## Visual Hierarchy

### Priority Levels
1. **Selected/Active**: 3px border, elevated shadow
2. **Hover**: Subtle lift and color shift
3. **Standard**: Base appearance
4. **Disabled**: Reduced opacity, dashed border

### Badges & Overlays
- Position: `top: -8px; right: 10px`
- Use for: FREE, URGENT, COMBINABLE markers
- Animation: `pulse` for attention

## Performance Considerations

- All transitions use GPU-accelerated properties (transform, opacity)
- Box-shadow instead of drop-shadow for better performance
- Single CSS file reduces HTTP requests
- Consistent sizing prevents layout shifts

## Accessibility

- Minimum touch target: 44x44px (achieved with padding)
- Color contrast ratios meet WCAG AA standards
- Focus states clearly visible
- Disabled states clearly indicated

## Future Extensions

To add new card types:
1. Define the type color in unified-cards.css
2. Add `.card-[type]` class with left border color
3. Optionally add specific background gradient
4. Document in this guide

## Testing Checklist

- [ ] All cards in grids have equal heights
- [ ] Hover states work consistently
- [ ] Selected states are visually distinct
- [ ] Disabled cards cannot be interacted with
- [ ] Cards respond appropriately on mobile
- [ ] Type indicators are clearly visible
- [ ] Badges don't overlap content
- [ ] Transitions are smooth