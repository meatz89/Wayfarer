# Implementation Plan for Remaining Features

## Analysis Summary
Most core UI components exist and are integrated. The main gaps are:
1. Time costs not applied to all actions
2. Information leverage UI flow incomplete
3. Skip cost obligation multipliers not explained in UI

## Priority 1: Complete Time Cost Integration (1 day)

### Actions Missing Time Costs:
1. **Market Transactions**
   - `BuyItemAsync` - Add 30 minutes (0.5 hours)
   - `SellItemAsync` - Add 30 minutes (0.5 hours)
   
2. **Item Usage**
   - `UseItemAsync` - Add time based on item type (15-60 minutes)
   - `ReadLetterAsync` - Add 15 minutes (0.25 hours)
   
3. **Information Actions**
   - `UnlockInformationAccessAsync` - Add 1 hour
   - `UseInformationAsLeverageAsync` - Add 1 hour
   
4. **Letter Management**
   - `AcceptLetterOfferAsync` - Add 30 minutes (0.5 hours)
   - Queue management actions - Keep free (instant UI actions)

### Implementation Steps:
1. Add `_timeManager.AdvanceTime()` calls to each action
2. Show ActionTimePreview component before confirming actions
3. Add deadline impact warnings when applicable

## Priority 2: Information Leverage UI Flow (1 day)

### Current Gap:
- Information letters can be used as leverage but no UI to select target NPC
- TODO comment at InformationDiscoveryScreen.razor:153

### Implementation:
1. Create `NPCLeverageSelectionDialog.razor` component
2. Show NPCs where information would be valuable
3. Display potential leverage power
4. Connect to `UseInformationAsLeverageAsync`

## Priority 3: Skip Cost Transparency (0.5 day)

### Current Gap:
- Skip costs show final amount but not breakdown
- Obligation multipliers hidden from player

### Implementation:
1. Enhance skip cost display in LetterQueueDisplay
2. Show base cost + multipliers breakdown
3. Explain each multiplier source (debt, obligations, etc.)

## Priority 4: Route Discovery from Information Letters (0.5 day)

### Current Gap:
- Information letters should reveal routes but connection not implemented

### Implementation:
1. In `InformationRevealService`, add route discovery when information type is "route"
2. Update `ExecuteExplore` to check for information-revealed routes
3. Show information source when displaying discovered routes

## Priority 5: Clean Up Legacy Code (0.5 day)

### Potential Legacy Code to Remove:
1. Check for unused command patterns
2. Remove any NarrativeManager references
3. Clean up TODO comments for completed features
4. Remove compatibility shims

## Implementation Order

### Day 1: Time Cost Integration
- Morning: Add time costs to market and item actions
- Afternoon: Add time preview UI integration
- Evening: Test deadline warnings

### Day 2: Information System
- Morning: Create NPC leverage selection dialog
- Afternoon: Connect information to route discovery
- Evening: Test information flow end-to-end

### Day 3: Polish and Cleanup
- Morning: Implement skip cost breakdown UI
- Afternoon: Remove legacy code
- Evening: Final testing

## Success Criteria
1. All player actions show time costs (except instant UI actions)
2. Information letters have clear UI for leverage usage
3. Skip costs show transparent breakdown
4. No legacy code remains
5. All systems visible to player