# UI Design and Implementation Principles

**CRITICAL**: This document captures the essential design and implementation principles for Wayfarer's UI system, discovered and refined through implementation.

## Core UI Design Principles

### 1. Progressive Disclosure Pattern
**Principle**: Show essential information first, details on demand.

**Implementation**:
```razor
<!-- Essential info always visible -->
<div class="resource-main">@currentStamina stamina</div>
<!-- Details shown contextually -->
<span class="resource-detail">Available for travel</span>
```

**Why**: Players need quick access to key data without UI clutter. Details should support decisions, not overwhelm.

### 2. Contextual Information Architecture
**Principle**: Display ONLY information relevant to the current activity.

**Examples**:
- **Market Screen**: Show coins, inventory space, weight impact on travel
- **Travel Screen**: Show stamina, equipment capabilities, weight penalties
- **Player Status**: Comprehensive view when explicitly requested

**Anti-Pattern**: Showing all information everywhere (creates cognitive overload)

### 3. No Strategic Analysis or Automation
**Principle**: NEVER calculate or display optimization hints.

**Violations to Avoid**:
```csharp
// ❌ WRONG: Strategic analysis
public RouteStrategicAnalysis AnalyzeRouteAccessibility()
public List<EquipmentInvestmentOpportunity> CalculateEquipmentInvestmentOpportunities()

// ✅ RIGHT: Simple capability check
public bool HasClimbingEquipment()
```

**Why**: Gameplay happens in the player's head. The UI provides data, players provide strategy.

### 4. Category Visibility
**Principle**: All game-mechanical categories MUST be visible in UI.

**Implementation**:
```razor
@if (item.Categories.Any())
{
    <span class="item-categories">
        (@item.AllCategoriesDescription)
    </span>
}
```

**Why**: Players cannot strategize about invisible systems. Categories drive gameplay.

## Technical Implementation Patterns

### 1. Repository-Mediated Access Pattern
**Principle**: ALL game state access MUST go through repositories.

```csharp
// ❌ WRONG: Direct access
GameWorld.WorldState.Items.Add(item);

// ✅ RIGHT: Repository-mediated
ItemRepository.AddItem(item);
```

**UI Access Patterns**:
- **Actions**: UI → GameWorldManager → Specific Manager → Repository
- **Queries**: UI → Repository → GameWorld.WorldState

### 2. Blazor Component Architecture
**Principle**: Razor components with code-behind base classes.

**Structure**:
```
PlayerStatusView.razor       (UI markup)
PlayerStatusView.razor.cs    (Logic in base class)
```

**Benefits**:
- Clean separation of markup and logic
- Testable business logic
- Type-safe parameter handling

### 3. Dependency Injection Pattern
**Principle**: Inject services, never create or locate them.

```csharp
[Inject] public GameWorld GameWorld { get; set; }
[Inject] public ItemRepository ItemRepository { get; set; }
```

### 4. Stateless UI Components
**Principle**: Components should be reactive, not stateful.

```csharp
// ❌ WRONG: Caching in component
private List<Item> _cachedItems;

// ✅ RIGHT: Always query fresh data
public List<Item> GetFilteredMarketItems()
{
    return GameManager.GetAvailableMarketItems(Location.Id);
}
```

## CSS Organization Principles

### 1. Semantic Class Names
**Principle**: CSS classes describe purpose, not appearance.

```css
/* ✅ RIGHT: Semantic */
.resource-block { }
.travel-context { }
.weight-warning { }

/* ❌ WRONG: Appearance-based */
.red-text { }
.big-font { }
```

### 2. Component-Scoped Styles
**Principle**: Group related styles by component/feature.

```css
/* Travel Planning Styles */
.travel-status { }
.travel-resources { }
.travel-context { }

/* Market Trading Styles */
.trading-context { }
.trading-resources { }
```

### 3. State-Based Styling
**Principle**: Use CSS classes to reflect game state.

```csharp
public string GetWeightClass(int totalWeight)
{
    if (totalWeight <= 3) return "weight-light";
    if (totalWeight <= 6) return "weight-medium";
    return "weight-heavy";
}
```

## Game Design Constraints in UI

### 1. No Automated Conveniences
**Principle**: Never solve the puzzle for the player.

**Examples of What NOT to Do**:
- Trading opportunity calculators
- Optimal route suggestions
- Profit margin displays
- "Best equipment for route" hints

### 2. Discovery Through Exploration
**Principle**: Information is revealed through player action, not given freely.

**Implementation**:
- Prices visible only at current location
- Route requirements shown, but player discovers equipment needs
- NPC schedules shown, but player learns patterns

### 3. Meaningful Visual Hierarchy
**Principle**: Important != Prominent. Context determines prominence.

**Example**: 
- Stamina is HUGE on travel screen (critical for travel decisions)
- Stamina is small on market screen (less relevant for trading)

## Navigation and Screen Management

### 1. Enum-Based Navigation
**Principle**: Use type-safe enums for screen management.

```csharp
public enum CurrentViews
{
    LocationScreen,
    MapScreen,
    MarketScreen,
    PlayerStatusScreen  // Added for dedicated status view
}
```

### 2. Context-Preserving Navigation
**Principle**: Maintain game context when switching views.

```csharp
[Parameter] public EventCallback OnClose { get; set; }

public async Task ClosePlayerStatus()
{
    if (OnClose.HasDelegate)
        await OnClose.InvokeAsync();
    else
        NavigationManager.NavigateTo("/");
}
```

## Error Prevention Patterns

### 1. Null-Safe Data Access
**Principle**: Always check for null when accessing game data.

```csharp
Item item = ItemRepository.GetItemByName(itemName);
if (item != null)
{
    categories.AddRange(item.Categories);
}
```

### 2. State Validation
**Principle**: Validate state before operations.

```csharp
bool canBuy = GameManager.CanBuyMarketItem(item.Id, Location.Id);
bool canSell = playerState.Inventory.HasItem(item.Name);
```

## Performance Considerations

### 1. Minimize Re-Renders
**Principle**: Use StateHasChanged() judiciously.

```csharp
private void BuyItem(Item item)
{
    GameManager.ExecuteTradeAction(item.Id, "buy", Location.Id);
    StateHasChanged(); // Only after state-changing operations
}
```

### 2. Efficient Queries
**Principle**: Filter data at the source, not in the UI.

```csharp
// Let repository/manager handle filtering
var availableRoutes = TravelManager.GetAvailableRoutes(currentId, destId);
```

## Testing Patterns

### 1. UI Logic in Base Classes
**Principle**: Business logic in base classes enables unit testing.

### 2. Repository Mocking
**Principle**: Mock repositories for isolated UI testing.

### 3. Integration Through GameWorldManager
**Principle**: Test complex flows through the central coordinator.

## Summary

These principles ensure:
1. **Player Agency**: UI provides information, players provide strategy
2. **Clean Architecture**: Clear separation of concerns and data flow
3. **Maintainability**: Consistent patterns across all UI components
4. **Performance**: Reactive updates without caching complexity
5. **Game Design Integrity**: No automation or strategic hints

The UI is a window into the game world, not a strategy guide. It should empower discovery and decision-making while maintaining the mystery and challenge that makes games engaging.