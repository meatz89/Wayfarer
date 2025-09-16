# Conversation UI Update Implementation Plan

## Objective
Update the conversation screen UI to match the mockup at `/mnt/c/git/wayfarer/UI-MOCKUPS/conversation-screen.html` while:
- Preserving all existing functionality
- Avoiding CSS redundancy and conflicts
- Ensuring proper Razor-to-CSS class mapping
- Following Wayfarer's architectural principles

## Prerequisites
- Read `/mnt/c/git/wayfarer/CLAUDE.md` for architectural principles
- Read `/mnt/c/git/wayfarer/PLAYER-STATS-IMPLEMENTATION-PLAN.md` for stats system
- Read `/mnt/c/git/wayfarer/wayfarer-poc-content-elenas-letter.md` for gameplay context
- Understand the parent-child relationship: GameScreen.razor provides the container

## Files to Modify
1. `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor`
2. `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor.cs`
3. `/mnt/c/git/wayfarer/src/wwwroot/css/conversation.css`

## Implementation Tasks

### Phase 1: Add Player Stats Display (NEW SECTION)
**Location**: After `.location-context` div in ConversationContent.razor

**Razor Structure**:
```razor
<!-- Player Stats Display -->
<div class="stats-display">
    @foreach (var stat in GetPlayerStats())
    {
        <div class="stat-item">
            <div class="stat-name">@stat.Name</div>
            <div class="stat-level">@stat.Level</div>
            <div class="stat-xp">@stat.CurrentXP/@stat.RequiredXP XP</div>
        </div>
    }
</div>
```

**Required Methods in .cs**:
- `GetPlayerStats()` - Returns list of PlayerStatInfo objects
- `PlayerStatInfo` class with Name, Level, CurrentXP, RequiredXP

**CSS Classes to Add**:
- `.stats-display` - Flex container with background
- `.stat-item` - Individual stat display
- `.stat-name`, `.stat-level`, `.stat-xp` - Typography styles

### Phase 2: Update NPC Bar with Personality Badge
**Changes**: Restructure existing `.npc-bar` content

**Razor Updates**:
```razor
<div class="npc-bar">
    <div class="npc-info">
        <div class="npc-name">@NpcName</div>
        <div class="npc-status">@GetNpcStatusLine()</div>
    </div>
    <div class="personality-badge">@GetPersonalityRuleShort()</div>
</div>
```

**Remove**: The separate `.personality-rule` section below

**CSS Updates**:
- Add `.personality-badge` styling
- Update `.npc-bar` to use `justify-content: space-between`

### Phase 3: Convert Patience Display to Visual Slots
**Replace**: Current `.patience-display` content

**Razor Structure**:
```razor
<div class="conditions-bar">
    <div class="patience-display">
        <span class="patience-label">Patience</span>
        <div class="patience-slots">
            @for (int i = 1; i <= Session.MaxPatience; i++)
            {
                <span class="patience-slot @GetPatienceSlotClass(i)"></span>
            }
        </div>
    </div>

    <div class="rapport-display">
        <span class="rapport-label">Rapport</span>
        <span class="rapport-counter">@(Session?.RapportManager?.CurrentRapport ?? 0)</span>
    </div>
</div>
```

**Method to Add**:
- `GetPatienceSlotClass(int slot)` - Returns "used", "current", or empty

**CSS Classes**:
- `.conditions-bar` - Container
- `.patience-slots` - Flex container for circles
- `.patience-slot`, `.patience-slot.used`, `.patience-slot.current`
- `.rapport-counter` - Large rapport number

### Phase 4: Restructure Flow State Bar
**Update**: Existing `.flow-display` structure

**Razor Structure**:
```razor
<div class="flow-state-bar">
    <div class="flow-container">
        <div class="flow-negative">
            @for (int i = -3; i <= -1; i++)
            {
                <span class="flow-segment @GetFlowSegmentClass(i)"></span>
            }
        </div>

        <div class="current-state">
            <div class="state-name">@GetConnectionStateDisplay()</div>
        </div>

        <div class="flow-positive">
            @for (int i = 1; i <= 3; i++)
            {
                <span class="flow-segment @GetFlowSegmentClass(i)"></span>
            }
        </div>
    </div>
</div>
```

**CSS Updates**:
- `.flow-state-bar` - Dark background
- `.flow-container` - Centered flex layout
- `.current-state` - Centered state name box

### Phase 5: Add Goals Section
**Location**: After flow state bar, before narrative section

**Razor Structure**:
```razor
@if (GetRequestGoals().Any())
{
    <div class="goals-section">
        <div class="goals-header">Request Goals - @GetRequestName()</div>
        <div class="goals-container">
            @foreach (var goal in GetRequestGoals())
            {
                <div class="goal-card @GetGoalCardClass(goal.Threshold)">
                    <div class="goal-threshold">@goal.Threshold</div>
                    <div class="goal-name">@goal.Name</div>
                    <div class="goal-reward">@goal.Reward</div>
                </div>
            }
        </div>
    </div>
}
```

**Methods to Add**:
- `GetRequestGoals()` - Returns list of goal thresholds
- `GetGoalCardClass(int threshold)` - Returns "achievable" or "active"

### Phase 6: Update Card Structure
**Modifications**: Add stat badges and XP indicators to existing cards

**Card Header Updates**:
```razor
<!-- Add before card-header -->
<div class="stat-badge @GetCardStatClass(convCard)">
    @GetCardStatName(convCard) Lv @GetCardLevel(convCard)
</div>

<!-- In card-name, add XP indicator -->
<div class="card-name">
    @GetProperCardName(convCard)
    <span class="xp-gain">+@GetXPGain() XP</span>
</div>
```

**CSS Classes to Add**:
- `.stat-badge` with stat-specific variants
- `.xp-gain` - Inline XP indicator

### Phase 7: CSS Selective Updates

**Color Palette Updates**:
```css
/* Update only these specific colors */
body background: linear-gradient(to bottom, #3d3429, #2e2720);
.game-container background: #e8dcc4;
.flow-state-bar background: #2c241a;
.current-state background: #e8dcc4;
```

**New Classes Only** (avoid duplicates):
- Stats display classes
- Personality badge
- Patience slots
- Goals section
- Stat badges for cards

**Positioning Adjustments**:
- `.persistence-tag` - Move to `top: 10px; right: 10px;`
- `.card-outcomes` - Ensure `width: 30%` for proper layout

## Testing Checklist
1. [ ] All player stats display with correct levels and XP
2. [ ] Personality badge appears on NPC bar
3. [ ] Patience shows as visual circles
4. [ ] Flow state has centered state name
5. [ ] Goals section shows request thresholds
6. [ ] Cards have stat badges and XP indicators
7. [ ] All CSS classes map correctly
8. [ ] No CSS conflicts or duplicates
9. [ ] Visual matches mockup
10. [ ] All functionality preserved

## Agent Execution Order
1. **UI Component Agent**: Update Razor structure
2. **Code-Behind Agent**: Add required methods
3. **CSS Agent**: Apply selective style updates
4. **Verification Agent**: Test all mappings and functionality

## Critical Requirements
- DO NOT duplicate existing CSS classes
- DO NOT break existing functionality
- DO NOT remove working features
- ENSURE all Razor classes have CSS definitions
- MAINTAIN architectural principles from CLAUDE.md