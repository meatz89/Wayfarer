# UI Architecture Fix Plan

## Current Problems

1. **MainGameplayView.razor has RenderFragment** - Should use `<LocationScreen />`
2. **LocationScreen.razor exists but is broken** - References non-existent LocationScreenBase
3. **NPCActionsView shows EVERYTHING** - Location actions mixed with NPC actions
4. **Wrong UI flow** - Shows time planning first, then NPCs with offers, then spot map, then all actions mixed

## Correct Architecture

### 1. Location Screen Flow
```
Location Screen
├── Location Header (name, type)
├── Spot Selection (grid of spots you can move between)
├── NPCs at Current Spot (list of NPCs at your current spot)
├── Selected NPC Actions (actions for the selected NPC only)
└── Location-wide Actions (Travel, Rest, Market buttons)
```

### 2. Component Structure
- MainGameplayView.razor - Uses `<LocationScreen />` component, NO RenderFragment
- LocationScreen.razor - Main location UI
- LocationScreen.razor.cs - Code-behind for LocationScreen
- NPCActionsView - Should show ONLY actions for selected NPC
- LocationActions - Should be location-wide actions only (Travel, Rest, Market)

## Implementation Steps

### Step 1: Create LocationScreen.razor.cs
- Move logic from RenderLocationScreen into proper code-behind
- Handle spot selection, NPC selection, action execution

### Step 2: Fix MainGameplayView
- Replace `@RenderLocationScreen()` with `<LocationScreen />`
- Remove the RenderLocationScreen method from @code block
- Move all @code block logic to MainGameplayView.razor.cs

### Step 3: Fix NPCActionsView
- Remove location actions (Travel, Rest, etc)
- Show ONLY actions for the selected NPC
- Pass selected NPC as parameter

### Step 4: Create LocationActionsBar component
- Shows Travel, Rest, Market buttons
- These navigate to dedicated screens
- Clear separation from NPC actions

### Step 5: Fix UI Flow in LocationScreen
1. Spots first (where can I go?)
2. NPCs at current spot (who is here?)
3. Actions for selected NPC (what can I do with them?)
4. Location actions bar at bottom (travel, rest, market)

## Benefits

1. **Clear Navigation** - Users understand spots → NPCs → actions
2. **Proper Component Separation** - Each component has one job
3. **No RenderFragments** - Proper components are testable and reusable
4. **Consistent Architecture** - All components use code-behind pattern
5. **Better UX** - Actions appear where they make sense