# CSS Guide for 4-Resource Conversation System

## Overview

This document outlines the CSS changes made to support the new 4-resource conversation system (Initiative, Cadence, Momentum, Doubt) that replaces the old 5-resource system (Focus, Flow, Connection State, Momentum, Doubt).

## Key Changes Made

### 1. Removed Old System Styles

**Deleted CSS classes:**
- `.focus-display-section`, `.focus-content`, `.focus-bar`
- `.flow-display`, `.flow-bar`, `.flow-segment`
- `.flow-state-bar`, `.flow-state-wrapper`
- `.card-focus` (renamed to `.card-initiative`)

### 2. Added New Resource Displays

#### Initiative Display
- **Class:** `.initiative-display-section`
- **Description:** Simple counter showing current Initiative (starts at 0, no maximum)
- **HTML Structure:**
```html
<div class="initiative-display-section">
    <div class="initiative-content">
        <span class="initiative-label">INITIATIVE</span>
        <div class="initiative-counter">
            <span class="initiative-value">@Initiative</span>
        </div>
        <span class="initiative-info">Build with Foundation cards</span>
    </div>
</div>
```

#### Cadence Meter
- **Class:** `.cadence-display`
- **Description:** Horizontal balance meter showing range -10 to +10
- **JavaScript Helper:** Use `GetCadencePosition()` to calculate indicator position
- **HTML Structure:**
```html
<div class="cadence-display">
    <span class="cadence-label">Conversation Balance</span>
    <div class="cadence-meter">
        <div class="cadence-track">
            <div class="cadence-indicator" style="left: @GetCadencePosition()%"></div>
        </div>
        <div class="cadence-scale">
            <span>-10</span><span>0</span><span>+10</span>
        </div>
    </div>
    <span class="cadence-value">@Cadence</span>
</div>
```

### 3. Card Depth Visual Hierarchy

Cards now show their strategic depth through color-coded borders and backgrounds:

- **Foundation (Depth 1-3):** `.depth-foundation` - Green (#4CAF50)
- **Standard (Depth 4-6):** `.depth-standard` - Blue (#2196F3)
- **Decisive (Depth 7-10):** `.depth-decisive` - Orange (#FF9800)

**HTML Structure:**
```html
<div class="card @GetCardDepthClass(card)">
    <div class="card-depth-marker">@GetDepthDisplayName(card.Depth)</div>
    <!-- card content -->
</div>
```

### 4. Alternative Cost Options

Cards with multiple payment options display interactive cost alternatives:

**HTML Structure:**
```html
@if (HasAlternativeCosts(card))
{
    <div class="alternative-costs">
        <div class="alt-cost-header">Alternative Costs:</div>
        @foreach (var altCost in GetAvailableAlternativeCosts(card))
        {
            <div class="alt-cost-option @(IsAltCostAvailable(altCost) ? "available" : "")"
                 @onclick="() => PlayCardWithAltCost(card, altCost)">
                <div class="alt-cost-description">@altCost.Description</div>
                <div class="alt-cost-requirements">@altCost.Requirements</div>
            </div>
        }
    </div>
}
```

### 5. Updated Resource Bar

The resource bar now accommodates exactly 4 resources with improved spacing and visual indicators:

- **Initiative:** Green icon (#4CAF50)
- **Cadence:** Gradient icon with mini-meter
- **Momentum:** Green icon (#7a8b5a)
- **Doubt:** Red icon (#8b4726) with critical state animations

## JavaScript Helper Functions Needed

To properly use these CSS classes, implement these helper functions:

```csharp
// Cadence position calculation (0-100% for CSS left positioning)
public string GetCadencePosition()
{
    // Range -10 to +10 maps to 0-100%
    return $"{((Session.Cadence + 10) / 20.0) * 100}";
}

// Card depth classification
public string GetCardDepthClass(ConversationCard card)
{
    return card.Depth switch
    {
        <= 3 => "depth-foundation",
        <= 6 => "depth-standard",
        _ => "depth-decisive"
    };
}

// Depth display name
public string GetDepthDisplayName(int depth)
{
    return depth switch
    {
        <= 3 => "Foundation",
        <= 6 => "Standard",
        _ => "Decisive"
    };
}

// Alternative cost availability
public bool HasAlternativeCosts(ConversationCard card) => card.AlternativeCosts?.Any() == true;
public bool IsAltCostAvailable(AlternativeCost cost) => /* check conditions */;
```

## Responsive & Accessibility Features

### Mobile Support
- Cadence meter scales down on smaller screens
- Initiative counter reduces size appropriately
- Alternative cost options adjust padding/font-size

### Accessibility
- **Reduced Motion:** All animations disabled with `prefers-reduced-motion: reduce`
- **High Contrast:** Enhanced border widths and contrast in `prefers-contrast: high`
- **Screen Readers:** Proper semantic structure with labels and descriptions

## Migration Notes

1. **Remove references** to Focus, Flow, and Connection State in HTML templates
2. **Update Razor components** to use Initiative instead of Focus
3. **Implement Cadence meter** with proper position calculation
4. **Add depth classes** to card elements based on card.Depth property
5. **Test thoroughly** across different screen sizes and accessibility settings

## Color Scheme Consistency

All new colors maintain the existing Wayfarer design language:
- **Primary palette:** #2c241a, #faf4ea, #d4a76a, #8b7355
- **Resource colors:** Green (#4CAF50), Blue (#2196F3), Orange (#FF9800), Red (#8b4726)
- **Background gradients:** Subtle use of rgba() overlays for depth indication

This CSS system supports the complete 4-resource conversation redesign while maintaining visual coherence with the existing Wayfarer aesthetic.