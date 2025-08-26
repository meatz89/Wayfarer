# Remaining Bugs Analysis & Solutions

## 1. Quick Exchange Not Updating Resources Immediately

### Problem
Resources (coins, attention, health) don't visually update when Quick Exchange cards are played.

### Root Cause
The `ExecuteExchange` method in GameFacade.cs and ConversationManager.cs IS updating the player resources correctly, but the UI components aren't refreshing.

### Solution Needed
After executing exchange in ConversationContent.razor.cs:
1. Call `StateHasChanged()` on the component
2. Also need to call refresh on parent GameScreen component
3. May need to trigger `RefreshResourceDisplay()` in GameScreen

### Files to Fix
- `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor.cs`
- `/mnt/c/git/wayfarer/src/Pages/GameScreen.razor.cs`

## 2. Async/Delayed Conversation Ending

### Problem
Conversations don't end immediately when completed, showing delay or async behavior.

### Likely Cause
The conversation ending logic is probably using async navigation which isn't awaited properly.

### Files to Check
- `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor.cs` - HandleConversationEnd method
- `/mnt/c/git/wayfarer/src/Pages/GameScreen.razor.cs` - HandleConversationEnd method

## 3. Inconsistent Card Styling (Free vs No Tag)

### Problem
Some cards display "Free" tag, others show nothing for cost.

### Solution Needed
Standardize card cost display logic to always show cost (even if 0).

### Files to Fix
- Card rendering components in `/mnt/c/git/wayfarer/src/Pages/Components/`
- Look for where card cost is displayed

## 4. Missing Success/Failure Notifications

### Problem
No visual feedback when cards are played.

### Solution  
The MessageSystem is being called (e.g., `_messageSystem.AddSystemMessage()`) but messages aren't displayed.

### Files to Check
- `/mnt/c/git/wayfarer/src/Pages/Components/MessageDisplay.razor`
- Ensure MessageDisplay component is included in GameScreen

## 5. No Exit Conversation Button

### Problem
Players can't manually exit conversations.

### Solution
Add exit button to ConversationContent.razor that calls HandleConversationEnd.

### Files to Fix
- `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor`

## 6. Travel Not Advancing Time

### Problem
Moving between locations doesn't advance game time.

### Location
Check travel execution in:
- `/mnt/c/git/wayfarer/src/Services/GameFacade.cs` - ExecuteTravel method
- `/mnt/c/git/wayfarer/src/GameState/TravelManager.cs`

## 7. Elena's Missing Quick Exchange

### Problem
Elena doesn't have a Quick Exchange card, causing crashes.

### Solution
Add Quick Exchange card to Elena's deck in:
- `/mnt/c/git/wayfarer/src/Game/ConversationSystem/Factories/ExchangeCardFactory.cs`
- Or in the JSON content files where NPCs are defined

## 8. Blazor Event Binding Issue (Displace Button)

### Core Problem
Blazor Server isn't binding @onclick handlers to dynamically rendered buttons.

### Symptoms
- Buttons render without any event attributes
- No onclick or Blazor internal attributes (_bl_) attached
- Affects Displace buttons and potentially others

### Potential Solutions
1. Replace inline async lambdas with EventCallback properties
2. Add @key attributes to force proper re-rendering
3. Use explicit method groups instead of lambdas
4. Consider using @onclick:preventDefault and @onclick:stopPropagation

### Technical Details
The issue is in ObligationQueueContent.razor where:
```razor
@onclick="@(async () => await DisplaceLetter(letter))"
```
Is not being compiled into actual event handlers by Blazor.

This might be due to:
- Components being inside CascadingValue
- Async lambda compilation issues
- Missing component lifecycle calls

## RECOMMENDED APPROACH

1. **Quick Fixes First**: 
   - Add Exit button
   - Fix resource refresh after exchange
   - Add Elena's Quick Exchange card

2. **UI State Management**:
   - Ensure StateHasChanged() is called after all game state updates
   - Consider implementing INotifyPropertyChanged pattern
   - May need to use InvokeAsync(() => StateHasChanged())

3. **Event Binding Fix**:
   - Research Blazor Server event binding limitations
   - Consider refactoring to use EventCallback<T> pattern
   - Test with simpler onclick handlers first

## TEST VERIFICATION

After fixes, verify:
1. Play Quick Exchange card → Resources update immediately
2. Complete conversation → Returns to location instantly  
3. All cards show cost consistently
4. Messages appear when actions succeed/fail
5. Exit button visible and functional in conversations
6. Travel consumes time based on distance
7. Elena has Quick Exchange available
8. Displace buttons actually work