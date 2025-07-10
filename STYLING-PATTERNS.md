# STYLING PATTERNS - WAYFARER UI DESIGN SYSTEM

This document describes the CSS styling patterns and UI design conventions used in the Wayfarer codebase for consistent visual implementation.

## CSS FILE ORGANIZATION

### Directory Structure
```
src/wwwroot/css/
├── game.css              # Core variables and base styles
├── ui-components.css     # Reusable UI component styles
├── containers.css        # Layout containers
├── items.css            # Item and route display styles
├── time-system.css      # Time and weather display
├── actions.css          # Action buttons and interactions
├── cards.css            # Card UI components
├── grids.css            # Grid layouts
├── stats-resources.css  # Player stats and resources
├── tooltip.css          # Tooltip styles
├── text-styles.css      # Typography
└── ...
```

## DESIGN VARIABLES

### Color Palette (from game.css)
```css
:root {
    /* Primary Colors */
    --parchment: #F2E8D5;      /* Light background */
    --oak-dark: #5D4037;       /* Dark borders */
    --oak-mid: #8D6E63;        /* Mid-tone borders */
    --leather: #A87D5F;        /* Primary buttons */
    --charcoal: #3E3E3E;       /* Dark text */
    
    /* Secondary Colors */
    --moss: #606C38;           /* Positive/success */
    --clay: #BC6C25;           /* Hover/special */
    --rust: #9E2A2B;           /* Negative/danger */
    --slate: #556573;          /* Neutral */
    --parchment-dark: #E8DCBE; /* Darker parchment */
    
    /* UI Element Colors */
    --bg-dark: #2C2622;        /* Dark background */
    --bg-panel: #3D3229;       /* Panel background */
    --bg-card: #4d3c36;        /* Card background */
    --bg-active: #6E5C4E;      /* Active state */
    --border-active: #8D7766;  /* Active border */
    
    /* Text Colors */
    --text-primary: #F2E8D5;   /* Light text on dark */
    --text-secondary: #DCD3BC; /* Secondary light text */
    --text-muted: #857D6B;     /* Muted text */
    
    /* Status Colors */
    --positive: var(--moss);   /* Success/allowed */
    --negative: #9E2A2B;       /* Error/blocked */
    --neutral: #8D7766;        /* Neutral state */
    --special: #BC6C25;        /* Special/important */
}
```

### Typography
```css
--font-primary: 'Cinzel', serif;      /* Headers, buttons */
--font-secondary: 'Lora', 'Georgia', serif;  /* Body text */
--font-tertiary: 'EB Garamond', serif;       /* Special text */
```

## COMPONENT PATTERNS

### Info Panels
Used for displaying categorical information about game entities:

```css
.location-info-panel {
    background-color: var(--bg-card);
    border: 1px solid var(--oak-mid);
    border-radius: 4px;
    padding: 12px 15px;
    margin-bottom: 15px;
    display: flex;
    flex-wrap: wrap;
    gap: 15px;
    font-family: var(--font-secondary);
}
```

### Info Display Pattern
Standard pattern for label-value pairs:

```css
.location-access-info {
    display: flex;
    align-items: center;
    gap: 8px;
}

.info-label {
    color: var(--text-muted);
    font-size: 0.9rem;
}

.info-value {
    color: var(--text-primary);
    font-weight: 500;
}
```

### Color-Coded Status Classes
Use semantic color coding for categorical values:

```css
/* Access Levels */
.access-Public { color: var(--positive); }
.access-Semi_Private { color: var(--neutral); }
.access-Private { color: var(--special); }
.access-Restricted { color: var(--negative); }

/* Social Expectations */
.social-Any { color: var(--positive); }
.social-Merchant_Class { color: var(--resource-coins); }
.social-Noble_Class { color: var(--special); }
.social-Professional { color: var(--neutral); }
```

### Button Patterns
Standard button styling:

```css
.location-button {
    background-color: var(--leather);
    color: var(--parchment);
    border: none;
    padding: 8px 15px;
    border-radius: 4px;
    cursor: pointer;
    font-family: var(--font-primary);
    letter-spacing: 0.05em;
    transition: all 0.2s ease;
}

.location-button:hover {
    background-color: var(--clay);
    transform: translateY(-2px);
}
```

## LAYOUT PATTERNS

### Container Structure
```css
.location-container {
    background-color: var(--bg-panel);
    flex: 1;
    padding: 15px;
}

.location-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 15px;
    padding-bottom: 10px;
    border-bottom: 1px solid var(--oak-mid);
}
```

### Responsive Flex Layouts
Use flexbox with wrap for responsive info displays:

```css
.info-panel {
    display: flex;
    flex-wrap: wrap;
    gap: 15px;
}
```

## UI CONVENTIONS

### Categorical Display Rules
1. **Always show entity categories** - Equipment categories, terrain requirements, social classes
2. **Use color coding** - Visual indicators for different categorical values
3. **Group related information** - Keep categorical info together in panels
4. **Maintain consistency** - Use same patterns across all UI components

### Accessibility Considerations
1. **Color not only indicator** - Always include text labels with color coding
2. **Sufficient contrast** - Text colors meet WCAG standards on backgrounds
3. **Clear hierarchies** - Use font size and weight to establish importance

### Component Naming
- **Containers**: `.location-container`, `.market-container`, etc.
- **Headers**: `.location-header`, `.section-header`
- **Info displays**: `.location-info-panel`, `.route-access-info`
- **Status classes**: `.access-{Level}`, `.social-{Expectation}`

## IMPLEMENTATION EXAMPLES

### Location UI Display (MainGameplayView.razor)
```razor
<div class="location-info-panel">
    <div class="location-access-info">
        <span class="info-label">Access Level:</span>
        <span class="info-value access-@currentLocation.AccessLevel">
            @currentLocation.AccessLevelDescription
        </span>
    </div>
    <div class="location-social-info">
        <span class="info-label">Social Expectation:</span>
        <span class="info-value social-@currentLocation.SocialExpectation">
            @currentLocation.SocialExpectationDescription
        </span>
    </div>
</div>
```

### Weather Display (time-system.css integration)
```css
.weather-display {
    display: flex;
    align-items: center;
    gap: 8px;
    color: var(--text-primary);
}

.weather-icon {
    font-size: 1.2em;
}
```

## BEST PRACTICES

1. **Use CSS Variables** - Always reference color variables instead of hardcoding
2. **Semantic Classes** - Name classes by purpose, not appearance
3. **Consistent Spacing** - Use standard padding/margin values (8px, 12px, 15px)
4. **Transition Effects** - Add subtle transitions for interactive elements
5. **Border Radius** - Use consistent radius values (3px for small, 4px for medium, 5px for large)

## ADDING NEW STYLES

When adding new UI components:

1. Check existing patterns in `ui-components.css` first
2. Use established color variables from `game.css`
3. Follow flex layout patterns for responsive design
4. Add categorical color coding for game entity displays
5. Maintain font family consistency
6. Test on dark background (game uses dark theme)

## CATEGORICAL VISIBILITY PRINCIPLE

All game systems that affect gameplay must be visible in UI:
- Equipment categories must show their effects
- Route requirements must be clearly displayed
- Social restrictions must be indicated
- Time-based availability must be shown
- Weather effects on terrain must be visible

This supports the game design principle that "Players cannot strategize about systems they cannot see or understand."