# Wayfarer Implementation Plan - Full Compliance Architecture

## Executive Summary

The Wayfarer codebase has functioning core mechanics but 0% UI compliance with design specifications. This plan provides a deterministic path to achieve 100% compliance while maintaining architectural integrity.

## Current State Analysis

### Working Components (Keep/Refactor)
- ✅ GameScreen as authoritative parent (correct SPA pattern)
- ✅ Queue logic and management system
- ✅ Token system and tracking
- ✅ Time block attention system
- ✅ Basic conversation flow
- ✅ Observation mechanics

### Critical Non-Compliance Issues
1. **Resources Bar**: Not always visible (violates Perfect Information)
2. **Player Choices**: Shown as buttons instead of cards
3. **Queue Display**: Missing from location screens
4. **Exchange System**: Not displayed as cards
5. **Observation Cards**: Don't persist in player hand
6. **Patience Calculations**: Missing hunger/spot modifiers
7. **Hospitality Mechanics**: Rest/food exchanges not implemented
8. **Letter Delivery**: No indicators during conversations

## Architectural Principles

### 1. Perfect Information (CRITICAL)
**Every mechanical value affecting gameplay MUST be visible at all times:**
- Resources (coins, health, hunger, attention) - ALWAYS in header
- Queue state with positions and deadlines
- Token counts and their effects
- Patience calculations showing all modifiers
- Card freshness/decay states

### 2. Card-Based Everything
**ALL player choices are cards, NEVER buttons:**
```css
/* WRONG - Button approach */
<button class="action-button">SPEAK</button>

/* CORRECT - Card approach */
<div class="card-base card-standard action-card">
    <div class="card-header">SPEAK</div>
    <div class="card-body">Play selected card</div>
    <div class="card-cost">1 patience</div>
</div>
```

### 3. Unified Screen Architecture
```
GameScreen (Always Visible)
├── ResourcesBar (FIXED TOP - Always Visible)
│   ├── Coins/Health/Hunger/Attention
│   └── Time Display
├── QueueBar (FIXED BELOW RESOURCES - Always Visible)
│   └── Active obligations with positions
├── DynamicContent (Changes per screen)
│   ├── LocationContent
│   ├── ConversationContent
│   ├── QueueManagementContent
│   └── TravelContent
└── NavigationFooter (FIXED BOTTOM - Always Visible)
    └── Location | Obligations buttons
```

## Implementation Phases

### Phase 1: Fix Critical Infrastructure (2 days)

#### 1.1 Unified Header Component
**File**: `Pages/Components/UnifiedHeader.razor`
```csharp
// Always visible, contains:
- Resources display with thresholds
- Time display with period
- Queue ticker showing position 1 deadline
```

#### 1.2 Card Component System
**File**: `Pages/Components/Cards/ActionCard.razor`
```csharp
@* Base card component for ALL actions *@
<div class="card-base @CardSize @(IsDisabled ? "disabled" : "") @(IsSelected ? "selected" : "")"
     @onclick="OnClick">
    <div class="card-header">
        @if (!string.IsNullOrEmpty(BadgeText))
        {
            <span class="card-badge">@BadgeText</span>
        }
        <span class="card-title">@Title</span>
    </div>
    <div class="card-body">
        @ChildContent
    </div>
    @if (ShowCost)
    {
        <div class="card-cost">
            @CostContent
        </div>
    }
</div>
```

#### 1.3 Queue Display Component
**File**: `Pages/Components/QueueTicker.razor`
```csharp
// Shows active queue across ALL screens
// Position 1 highlighted with deadline
// Positions 2-3 shown with time remaining
```

### Phase 2: Refactor Location Screen (2 days)

#### 2.1 NPC Interaction Cards
**Convert buttons to cards:**
```csharp
// OLD (Wrong)
<button @onclick="StartConversation">Talk to Marcus</button>

// NEW (Correct)
<ActionCard Title="Talk to Marcus"
            CardSize="card-standard"
            OnClick="() => StartConversation(npc.Id)">
    <div class="npc-state">@npc.EmotionalState</div>
    <CostContent>
        <span>2 attention • 7 patience</span>
    </CostContent>
</ActionCard>
```

#### 2.2 Observation Card System
```csharp
public class ObservationCardManager
{
    // Observations generate cards that persist
    // Cards have freshness decay over time
    // Fresh > Stale > Expired states
    // Player can hold multiple observation cards
}
```

#### 2.3 Spot Movement Cards
```csharp
<ActionCard Title="@spot.Name"
            CardSize="card-compact"
            BadgeText="@(spot.IsCrossroads ? "CROSSROADS" : null)">
    <div>@GetSpotOccupants(spot)</div>
    <CostContent>
        <span>Move (free)</span>
    </CostContent>
</ActionCard>
```

### Phase 3: Fix Conversation System (3 days)

#### 3.1 Conversation Action Cards
**File**: `Pages/Components/ConversationActions.razor`
```csharp
// Display SPEAK/LISTEN/OBSERVE as cards
<div class="cards-grid-3col">
    <ActionCard Title="LISTEN" 
                IsDisabled="@(!CanListen())"
                OnClick="ExecuteListen">
        <div>Let them speak</div>
        <CostContent>
            <span>+1 comfort • -1 patience</span>
        </CostContent>
    </ActionCard>
    
    <ActionCard Title="SPEAK"
                IsDisabled="@(!HasSelectedCard())"
                IsSelected="@(SelectedCard != null)">
        <div>@GetSelectedCardName()</div>
        <CostContent>
            <span>Play card • -1 patience</span>
        </CostContent>
    </ActionCard>
</div>
```

#### 3.2 Player Hand Display
```csharp
public class PlayerHandComponent
{
    // Shows available cards (base + observations + letters)
    // Cards are selectable for SPEAK action
    // Selected card highlighted
    // Shows card effects/depth/freshness
}
```

#### 3.3 Patience Calculation Display
```csharp
public string GetPatienceCalculation(NPC npc)
{
    var parts = new List<string>();
    parts.Add($"{npc.BasePatience} base");
    
    if (HungerPenalty > 0)
        parts.Add($"-{HungerPenalty} hunger");
    
    if (SpotModifier != 0)
        parts.Add($"{SpotModifier:+#;-#} spot");
    
    return string.Join(" ", parts);
}
```

### Phase 4: Implement Exchange System (2 days)

#### 4.1 Exchange Card Display
```csharp
public class ExchangeCardComponent
{
    // Exchanges shown as selectable cards
    // Clear cost → reward visualization
    // Success rate displayed
    // Token bonuses shown
}
```

#### 4.2 Exchange Resolution
```csharp
// Use same SPEAK action to accept exchanges
// Decline is EXIT conversation
// No special buttons - cards only
```

### Phase 5: Add Missing Mechanics (2 days)

#### 5.1 Hospitality Exchanges
```csharp
public class HospitalitySpot
{
    // Taverns/Inns offer rest exchanges
    // Cost: Coins → Effect: Reduce hunger
    // Cost: Time → Effect: Restore health
}
```

#### 5.2 Letter Delivery Indicators
```csharp
// During conversations, show if NPC has letters
// Visual indicator on NPC card
// Automatic delivery on conversation start
```

#### 5.3 Verisimilitude Breakpoints
```csharp
public class PatienceBreakpoints
{
    // < 3 patience: Crisis behavior
    // < 5 patience: Impatient state
    // < 7 patience: Normal conversation
}
```

### Phase 6: Polish & Testing (1 day)

#### 6.1 CSS Refinement
- Ensure all cards have consistent styling
- Fix any layout issues
- Verify responsive behavior

#### 6.2 E2E Testing with Playwright
- Test all card interactions
- Verify queue display updates
- Check resource visibility
- Test exchange flows

## File Structure Changes

### New Components Required
```
Pages/Components/
├── Cards/
│   ├── ActionCard.razor           # Base card component
│   ├── NPCCard.razor              # NPC interaction card
│   ├── ObservationCard.razor     # Observation display
│   ├── ExchangeCard.razor        # Exchange offer card
│   └── ConversationCard.razor    # Hand card display
├── UnifiedHeader.razor            # Always-visible resources
├── QueueTicker.razor              # Queue position display
├── PlayerHand.razor               # Card hand in conversations
└── PatienceCalculator.razor      # Shows patience math
```

### Files to Refactor
```
Pages/Components/
├── LocationContent.razor          # Convert buttons to cards
├── ConversationContent.razor      # Add card-based actions
└── GameScreen.razor               # Ensure header always visible
```

### Files to Delete
```
Pages/Components/
├── UnifiedChoice.razor            # Wrong pattern - delete
└── Any button-based action components
```

## Success Metrics

### Must Have (100% Required)
- [ ] Resources ALWAYS visible
- [ ] All actions are cards, not buttons
- [ ] Queue visible on location screen
- [ ] Exchanges shown as cards
- [ ] Observation cards persist
- [ ] Patience shows all modifiers

### Should Have (Expected)
- [ ] Hospitality exchanges work
- [ ] Letter indicators in conversations
- [ ] Verisimilitude breakpoints
- [ ] Card freshness decay

### Nice to Have (If Time)
- [ ] Animation on card selection
- [ ] Sound effects on card play
- [ ] Visual queue manipulation preview

## Testing Strategy

### Unit Tests
```csharp
[Test]
public void ResourcesBar_AlwaysVisible_OnAllScreens()
{
    // Test resources visible on location
    // Test resources visible during conversation
    // Test resources visible in queue management
}

[Test]
public void AllActions_DisplayedAsCards_NotButtons()
{
    // Scan all razor files for <button> elements
    // Verify only navigation uses buttons
    // All game actions use card components
}
```

### E2E Tests with Playwright
```javascript
// Test card selection flow
await page.click('.observation-card');
await expect(page.locator('.selected')).toBeVisible();

// Test exchange display
await page.click('.npc-card.has-exchange');
await expect(page.locator('.exchange-card')).toHaveCount(3);
```

## Migration Path

### Day 1-2: Infrastructure
1. Create UnifiedHeader component
2. Implement base ActionCard
3. Add QueueTicker
4. Update GameScreen layout

### Day 3-4: Location Screen
1. Convert NPC interactions to cards
2. Implement observation persistence
3. Fix spot navigation cards

### Day 5-7: Conversation System
1. Replace action buttons with cards
2. Add player hand display
3. Show patience calculations

### Day 8-9: Exchange System
1. Create exchange card components
2. Integrate with conversation flow
3. Add token bonus display

### Day 10-11: Missing Mechanics
1. Add hospitality exchanges
2. Implement letter indicators
3. Add patience breakpoints

### Day 12: Testing & Polish
1. Run all E2E tests
2. Fix any UI issues
3. Final compliance check

## Risk Mitigation

### Risk: Breaking existing functionality
**Mitigation**: Create new components alongside old, switch atomically

### Risk: CSS conflicts
**Mitigation**: Use scoped CSS classes, test each change

### Risk: State management issues
**Mitigation**: Keep GameScreen as single source of truth

## Definition of Done

A feature is COMPLETE when:
1. ✅ UI matches mockup exactly
2. ✅ All interactions use cards
3. ✅ Perfect Information maintained
4. ✅ E2E tests pass
5. ✅ No console errors
6. ✅ Responsive layout works

## Next Steps

1. **Immediate**: Fix resources bar visibility (Phase 1.1)
2. **Today**: Create ActionCard component (Phase 1.2)
3. **Tomorrow**: Begin location screen refactor (Phase 2)

---

**Remember**: 
- NO BUTTONS for game actions
- Resources ALWAYS visible
- Everything is a CARD
- Perfect Information at all times