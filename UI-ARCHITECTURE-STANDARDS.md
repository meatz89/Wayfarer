# Wayfarer UI Architecture Standards

## Mandatory Rules

### 1. **NEVER Use @code Blocks**
- ALL component logic goes in `.razor.cs` code-behind files
- The `.razor` file is ONLY for markup
- Even simple components get a code-behind file

### 2. **NEVER Use Inline Styles**
- ALL styling goes in CSS classes
- Use `<style>` blocks in `.razor` files for component-specific CSS
- Or use separate `.css` files for shared styles

### 3. **NEVER Use RenderFragment**
- Create proper Razor components instead
- Components are composable, testable, and reusable
- RenderFragment is an anti-pattern

### 4. **MainGameplayView Architecture**
- MainGameplayView uses a switch/if-else to display the active screen
- Each screen is a proper Razor component (LocationScreen.razor, RestScreen.razor, etc.)
- NO inline RenderFragments

### 5. **Component Separation**
- Each screen is its own component
- Components are self-contained with their own code-behind
- Shared logic goes in services, not parent components

## Example Structure

### Correct Component Structure:
```
MainGameplayView.razor:
@if (CurrentScreen == Screens.Location)
{
    <LocationScreen />
}
else if (CurrentScreen == Screens.Rest)
{
    <RestScreen />
}
else if (CurrentScreen == Screens.Market)
{
    <MarketScreen />
}

MainGameplayView.razor.cs:
public partial class MainGameplayView : ComponentBase
{
    // Logic here, NO RenderFragments
}
```

### Correct Location Screen Structure:
```
LocationScreen.razor:
<div class="location-screen">
    <LocationHeader />
    <LocationSpotView />
    <NPCListView />
</div>

<style>
    .location-screen {
        /* CSS here */
    }
</style>

LocationScreen.razor.cs:
public partial class LocationScreen : ComponentBase
{
    // Component logic here
}
```

## Current Violations to Fix

1. **MainGameplayView.razor** - Has massive @code block with RenderFragments
2. **29 components** - Using @code blocks instead of code-behind
3. **LocationActions.razor** - Has @code block and inline component logic
4. **NPCActionsView.razor** - 594 lines with @code block
5. **Mixed patterns everywhere** - No consistency

## Migration Plan

### Phase 1: Fix MainGameplayView
- Remove ALL RenderFragments
- Create LocationScreen.razor component
- Create SidebarPanel.razor component
- Move @code block to MainGameplayView.razor.cs

### Phase 2: Fix Location Flow
- Create proper LocationScreen with:
  - LocationSpotSelector (choose spots)
  - NPCListAtSpot (see NPCs at current spot)
  - NPCActionPanel (actions for selected NPC)
- Remove inline Rest/Market actions
- Actions navigate to dedicated screens

### Phase 3: Standardize All Components
- Convert all @code blocks to code-behind files
- Ensure all components follow the pattern
- Remove any remaining RenderFragments

## Benefits

1. **Consistency** - One pattern everywhere
2. **Testability** - Components can be unit tested
3. **Maintainability** - Logic and markup separated
4. **Performance** - Proper component boundaries for rendering
5. **Clarity** - Easy to understand where code lives