# Playtesting Fixes Implementation Plan

## Overview
This document outlines the comprehensive plan to fix 6 critical issues discovered during Session 1 playtesting with all 5 personas.

**Session Date:** October 2, 2025
**Playtest Results:** `test-results-session1.md`
**Priority Level:** HIGH - These fixes block meaningful playtesting

---

## Issue 3: Fix Starting Conditions to Match POC Scenario

### Problem
Current starting state doesn't match POC documentation:
- Health: 10 (expected: 100)
- Hunger: 5 (expected: 50)
- Coins: 50 (expected: 0)
- Time: Dawn Segment 1 (expected: Morning Segment 4)
- Missing: Viktor's Package obligation

### Root Cause
`Player.cs` constructor sets hardcoded defaults instead of using package starting conditions from `05_gameplay.json`.

### Solution

#### Step 1: Update Player Constructor
**File:** `src/GameState/Player.cs` (lines 180-197)

**Current Code:**
```csharp
public Player()
{
    Background = GameRules.StandardRuleset.Background;
    Inventory = new Inventory(10);

    Coins = 5;              // WRONG: Should be 0
    Level = 1;
    CurrentXP = 0;
    XPToNextLevel = 100;

    MaxHealth = 10;         // WRONG: Should be 100
    MaxHunger = 100;        // Correct
}
```

**New Code:**
```csharp
public Player()
{
    Background = GameRules.StandardRuleset.Background;
    Inventory = new Inventory(10);

    // Starting values will be set by ApplyInitialConfiguration
    // These are just safe defaults in case config isn't loaded
    Coins = 0;              // FIXED
    Level = 1;
    CurrentXP = 0;
    XPToNextLevel = 100;

    MaxHealth = 100;        // FIXED
    MaxHunger = 100;
    // Health and Hunger set via ApplyInitialConfiguration
}
```

#### Step 2: Add Starting Time Properties
**File:** `src/Models/PackageStartingConditions.cs`

**Add Properties:**
```csharp
public class PackageStartingConditions
{
    public PlayerInitialConfig PlayerConfig { get; set; }
    public string StartingSpotId { get; set; }
    public List<StandingObligationDTO> StartingObligations { get; set; }
    public Dictionary<string, NPCTokenRelationship> StartingTokens { get; set; }

    // NEW PROPERTIES
    public TimeBlocks StartingTimeBlock { get; set; } = TimeBlocks.Morning;
    public int StartingSegment { get; set; } = 1;
    public int StartingDay { get; set; } = 1;
}
```

#### Step 3: Load Starting Time in PackageLoader
**File:** `src/Content/PackageLoader.cs` (around line 290)

**Add After Line 298:**
```csharp
// Apply starting time if specified
if (conditions.StartingTimeBlock != default(TimeBlocks))
{
    _gameWorld.CurrentTimeBlock = conditions.StartingTimeBlock;
}

// Apply starting segment via WorldState
if (conditions.StartingSegment > 0)
{
    _gameWorld.WorldState.CurrentSegment = conditions.StartingSegment;
}

if (conditions.StartingDay > 0)
{
    _gameWorld.CurrentDay = conditions.StartingDay;
}
```

#### Step 4: Update JSON with Starting Time
**File:** `src/Content/Core/05_gameplay.json` (lines 10-34)

**Update startingConditions:**
```json
"startingConditions": {
  "playerConfig": {
    "coins": 0,
    "health": 100,
    "maxHealth": 100,
    "hunger": 50,
    "maxHunger": 100,
    "satchelCapacity": 10,
    "satchelWeight": 3
  },
  "startingSpotId": "central_fountain",
  "startingTimeBlock": "Morning",
  "startingSegment": 4,
  "startingDay": 1,
  "startingObligations": [
    {
      "id": "viktor_package",
      "type": "delivery",
      "position": 1,
      "deadline": 480,
      "recipientId": "marcus",
      "letterId": "viktor_package_letter",
      "payment": 7,
      "weight": 3,
      "description": "Viktor's package for Marcus at Market Square"
    }
  ]
}
```

### Verification
After fix, game should start with:
- ✅ Health: 100/100
- ✅ Hunger: 50/100
- ✅ Coins: 0
- ✅ Time: Morning, Segment 4/4
- ✅ Viktor's Package in Queue Position 1

---

## Issue 7: Fix Personality Display & Mechanics

### Problem
MERCANTILE personality shows "+30% success" (old system) instead of "+3 Momentum" (current system).

### Root Cause
1. `PersonalityRuleEnforcer.GetRuleDescription()` returns outdated text
2. Actual implementation still uses success rate modifier instead of momentum bonus
3. Success rates don't exist in current deterministic card system

### Solution

#### Step 1: Fix Display Text
**File:** `src/Subsystems/Conversation/PersonalityRuleEnforcer.cs` (line 170)

**Current:**
```csharp
PersonalityModifierType.HighestFocusBonus => "Mercantile: Your highest Initiative card gains +30% success",
```

**Fixed:**
```csharp
PersonalityModifierType.HighestFocusBonus => "Mercantile: Highest Initiative card gains +3 Momentum",
```

#### Step 2: Remove Obsolete Success Rate Method
**File:** `src/Subsystems/Conversation/PersonalityRuleEnforcer.cs` (lines 48-68)

**DELETE METHOD:**
```csharp
// REMOVE THIS - Success rates don't exist anymore
public int ModifySuccessRate(CardInstance card, int baseSuccessRate)
{
    // ... DELETE ENTIRE METHOD
}
```

#### Step 3: Implement Correct Momentum Bonus
**File:** `src/Subsystems/Conversation/PersonalityRuleEnforcer.cs`

**ADD NEW METHOD:**
```csharp
/// <summary>
/// Get Mercantile bonus for highest Initiative card (+3 Momentum)
/// </summary>
public int GetMercantileMomentumBonus(CardInstance card)
{
    if (_modifier.Type != PersonalityModifierType.HighestFocusBonus)
        return 0;

    int cardInitiative = GetCardInitiativeCost(card);

    // First card of turn OR highest Initiative so far gets the bonus
    if (_isFirstCardOfTurn || cardInitiative > _highestInitiativeThisTurn)
    {
        return 3; // +3 Momentum bonus
    }

    return 0;
}
```

**UPDATE ModifyMomentumChange (line 84):**
```csharp
public int ModifyMomentumChange(CardInstance card, int baseMomentumChange)
{
    int modifiedChange = baseMomentumChange;

    // MERCANTILE: Add +3 Momentum to highest Initiative card
    if (_modifier.Type == PersonalityModifierType.HighestFocusBonus)
    {
        modifiedChange += GetMercantileMomentumBonus(card);
    }

    switch (_modifier.Type)
    {
        case PersonalityModifierType.MomentumLossDoubled:
            // ... existing code
            break;
        // ... rest of switch
    }

    return modifiedChange;
}
```

#### Step 4: Update PersonalityModifier.cs
**File:** `src/GameState/PersonalityModifier.cs` (lines 37-40)

**Current:**
```csharp
case PersonalityType.MERCANTILE:
    modifier.Type = PersonalityModifierType.HighestFocusBonus;
    modifier.Parameters["bonusPercent"] = 30; // +30% success rate
    break;
```

**Fixed:**
```csharp
case PersonalityType.MERCANTILE:
    modifier.Type = PersonalityModifierType.HighestFocusBonus;
    modifier.Parameters["momentumBonus"] = 3; // +3 Momentum
    break;
```

### Verification
After fix:
- ✅ UI shows: "Mercantile: Highest Initiative card gains +3 Momentum"
- ✅ Playing highest Initiative card grants +3 Momentum
- ✅ Second card with same Initiative does NOT get bonus
- ✅ Playing higher Initiative card gets bonus again

---

## Issue 6: Add Tooltips for All Game Terms

### Problem
No explanations for game terms. First-timer persona completely lost.

### Solution

#### Step 1: Create Tooltip Component
**File:** `src/Pages/Components/TooltipWrapper.razor` (NEW)

```razor
@namespace Wayfarer.Pages.Components

<span class="tooltip-term" @onmouseenter="ShowTooltip" @onmouseleave="HideTooltip">
    @ChildContent
    @if (_showTooltip)
    {
        <div class="tooltip-popup @Position">
            <div class="tooltip-title">@Term</div>
            <div class="tooltip-description">@Description</div>
        </div>
    }
</span>

@code {
    [Parameter] public string Term { get; set; }
    [Parameter] public string Description { get; set; }
    [Parameter] public string Position { get; set; } = "top"; // top, bottom, left, right
    [Parameter] public RenderFragment ChildContent { get; set; }

    private bool _showTooltip = false;

    private void ShowTooltip() => _showTooltip = true;
    private void HideTooltip() => _showTooltip = false;
}
```

#### Step 2: Create Tooltip Content Provider
**File:** `src/Services/TooltipContentProvider.cs` (NEW)

```csharp
public static class TooltipContentProvider
{
    public static readonly Dictionary<string, string> Definitions = new()
    {
        // Conversation Resources
        ["Initiative"] = "Action currency for playing cards. Generated by Remarks (+1) and Observations (+1). Required to play higher-cost cards.",
        ["Momentum"] = "Progress toward conversation goals. Reach 8 (Basic), 12 (Priority), or 16 (Immediate) to complete delivery.",
        ["Doubt"] = "NPC's uncertainty. Conversation fails at 10 Doubt. Reduced by Observations (-1 Doubt each).",
        ["Cadence"] = "Conversation rhythm. +1 per SPEAK, -1 per LISTEN. Positive Cadence adds Doubt when you LISTEN.",
        ["Understanding"] = "Unlocks deeper conversation cards. Gain Understanding by playing Statement cards.",

        // Card Types
        ["Statement"] = "Cards that go to Spoken pile and add to conversation time cost (1 segment per Statement).",
        ["Echo"] = "Cards that return to deck after played. Can be drawn and played multiple times.",
        ["Thought"] = "Cards that persist for the entire conversation and provide ongoing benefits.",

        // ConversationalMoves
        ["Remark"] = "Authority-focused cards that generate Initiative (+1) and Momentum. Direct statements.",
        ["Observation"] = "Perception cards that generate Initiative (+1) and reduce Doubt. Analytical insights.",
        ["Argument"] = "High-cost cards requiring multiple Statements in Spoken. Powerful conclusion moves.",

        // Stats
        ["Insight"] = "Analytical intelligence. Better with observational cards. Unlocks systematic investigation (Lv2) and scholar shortcuts (Lv2).",
        ["Rapport"] = "Emotional intelligence. Better with empathetic cards. Unlocks local inquiry (Lv2) and community paths (Lv2).",
        ["Authority"] = "Command presence. Better with directive cards. Unlocks demand access (Lv2) and noble gates (Lv2).",
        ["Diplomacy"] = "Trade acumen. Better with negotiation cards. Unlocks purchase information (Lv2) and merchant caravans (Lv2).",
        ["Cunning"] = "Subtlety and misdirection. Better with indirect cards. Unlocks covert search (Lv2) and shadow paths (Lv3).",

        // Player Resources
        ["Health"] = "Physical wellbeing. Restored by rest and medical care. Death at 0.",
        ["Hunger"] = "Food need. Increases over time. High hunger reduces work efficiency and causes health damage.",
        ["Coins"] = "Currency for purchases, exchanges, and services.",
        ["Stamina"] = "Energy for travel. Restored by rest. Required for long journeys.",
        ["Attention"] = "Daily mental energy for investigations and deep work. Refreshes each time block.",

        // Location Mechanics
        ["Familiarity"] = "Location knowledge (0-3). Investigate to increase. Unlocks observations at specific levels.",
        ["Investigation"] = "Spend 1 Attention + 1 Segment to increase Familiarity. Different approaches use different stats.",
        ["Observation"] = "Location-specific knowledge card that helps in conversations with specific NPCs.",

        // Queue & Obligations
        ["Queue Position"] = "Letter delivery priority. Must complete Position 1 before others. Can be reordered with tokens.",
        ["Deadline"] = "Time limit for delivery (in segments). Miss deadline = obligation fails and relationship damage.",
        ["Weight"] = "Physical burden. Letters and items have weight. Exceeding capacity slows travel.",

        // Time System
        ["Time Block"] = "Day divided into 6 blocks: Dawn, Morning, Afternoon, Evening, Night, Late Night. Attention refreshes per block.",
        ["Segment"] = "Smallest time unit. 4 segments per time block. Conversations and work consume segments.",

        // Tokens
        ["Trust"] = "Personal connection token. Earned from Basic goals. Used for personal favors.",
        ["Diplomacy"] = "Trade relationship token. Earned from Priority goals. Used for business dealings.",
        ["Status"] = "Social standing token. Earned from Immediate goals. Used for exclusive access.",
        ["Shadow"] = "Covert connection token. Earned from secret dealings. Used for underground favors.",

        // Personality Types
        ["MERCANTILE"] = "Business-focused. Highest Initiative card gains +3 Momentum.",
        ["DEVOTED"] = "Emotionally invested. All Momentum losses doubled, +1 Doubt on failure.",
        ["PROUD"] = "Status-conscious. Cards must be played in ascending Initiative order.",
        ["CUNNING"] = "Information-focused. Repeat Initiative costs -2 Momentum.",
        ["STEADFAST"] = "Duty-bound. All Momentum changes capped at ±2.",

        // Travel
        ["Route"] = "Path between locations. Face-down until discovered. Familiarity reduces time/risk.",
        ["Path Card"] = "Choice during travel (continue, investigate, rest, etc.). Some routes have fixed sequences.",
        ["Event"] = "Random encounter during travel. Drawn from event pools based on route type.",
    };

    public static string Get(string term)
    {
        return Definitions.TryGetValue(term, out string description) ? description : "No description available.";
    }
}
```

#### Step 3: Add Tooltip CSS
**File:** `src/wwwroot/css/tooltips.css` (NEW)

```css
.tooltip-term {
    position: relative;
    border-bottom: 1px dotted #7a6250;
    cursor: help;
    display: inline-block;
}

.tooltip-popup {
    position: absolute;
    background: #2a2419;
    border: 2px solid #7a6250;
    border-radius: 4px;
    padding: 12px;
    width: 300px;
    z-index: 1000;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.5);
    pointer-events: none;
}

.tooltip-popup.top {
    bottom: 100%;
    left: 50%;
    transform: translateX(-50%);
    margin-bottom: 8px;
}

.tooltip-popup.bottom {
    top: 100%;
    left: 50%;
    transform: translateX(-50%);
    margin-top: 8px;
}

.tooltip-title {
    font-weight: bold;
    color: #f4e4c1;
    margin-bottom: 6px;
    font-size: 14px;
}

.tooltip-description {
    color: #d4c4a1;
    font-size: 12px;
    line-height: 1.4;
}
```

#### Step 4: Update GameScreen with Tooltips
**File:** `src/Pages/GameScreen.razor`

**Add to resource bars:**
```razor
<div class="resource-item">
    <TooltipWrapper Term="Health" Description="@TooltipContentProvider.Get("Health")">
        <span class="resource-label">Health</span>
    </TooltipWrapper>
    <span class="resource-value">@CurrentHealth/@MaxHealth</span>
</div>

<div class="resource-item">
    <TooltipWrapper Term="Hunger" Description="@TooltipContentProvider.Get("Hunger")">
        <span class="resource-label">Hunger</span>
    </TooltipWrapper>
    <span class="resource-value">@CurrentHunger/@MaxHunger</span>
</div>

<!-- Repeat for Coins, Attention, etc. -->
```

#### Step 5: Update ConversationContent with Tooltips
**File:** `src/Pages/Components/ConversationContent.razor`

**Lines 54-78 (resources):**
```razor
<div class="resource-item">
    <TooltipWrapper Term="Momentum" Description="@TooltipContentProvider.Get("Momentum")">
        <div class="resource-label">Momentum</div>
    </TooltipWrapper>
    <div class="resource-value">@(Session?.CurrentMomentum ?? 0)</div>
    <div class="resource-bar">
        <div class="resource-fill" style="width: @GetMomentumPercentage()%;"></div>
    </div>
</div>

<!-- Repeat for Initiative, Doubt, Understanding, Cadence -->
```

### Verification
After fix:
- ✅ Hover over "Health" shows: "Physical wellbeing. Restored by rest..."
- ✅ All 40+ game terms have tooltips
- ✅ Tooltips appear on hover, disappear on mouse leave
- ✅ Positioning works (top/bottom/left/right)

---

## Issue 11: Show Stat-Gated Content with Requirements

### Problem
Locked investigation approaches are hidden or show no requirements. Explorer and Optimizer personas can't plan stat development.

### Solution

#### Step 1: Update LocationContent.razor
**File:** `src/Pages/Components/LocationContent.razor` (lines 115-132)

**Current Code:**
```razor
@foreach (var approach in AvailableInvestigationApproaches)
{
    bool canAfford = CanAffordInvestigation(approach);
    <div class="approach-btn @(canAfford ? "" : "approach-locked")"
         @onclick="async () => await InvestigateWithApproach(approach)">
        <div class="approach-name">@GetApproachDisplayName(approach)</div>
        <div class="approach-cost">1 attention • 1 segment</div>
        <div class="approach-description">@GetApproachDescription(approach)</div>
        @if (!canAfford)
        {
            <div class="approach-requirement">@GetApproachRequirement(approach)</div>
        }
    </div>
}
```

**Problem:** `GetApproachRequirement()` doesn't exist yet and doesn't show stat requirements.

**NEW CODE:**
```razor
@* Show ALL possible investigation approaches, not just available ones *@
@foreach (var approach in GetAllInvestigationApproaches())
{
    bool canAfford = CanAffordInvestigation(approach);
    bool meetsStatRequirement = MeetsStatRequirement(approach);
    bool isLocked = !canAfford || !meetsStatRequirement;

    <div class="approach-btn @(isLocked ? "approach-locked" : "")"
         @onclick="async () => await (isLocked ? Task.CompletedTask : InvestigateWithApproach(approach))">
        <div class="approach-header">
            <div class="approach-name">
                @if (isLocked) { <span class="lock-icon">🔒</span> }
                @GetApproachDisplayName(approach)
            </div>
            @if (!isLocked)
            {
                <div class="approach-cost">1 attention • 1 segment</div>
            }
        </div>
        <div class="approach-description">@GetApproachDescription(approach)</div>
        @if (isLocked)
        {
            <div class="approach-requirement">
                @if (!meetsStatRequirement)
                {
                    <span class="stat-requirement">Requires @GetRequiredStat(approach) Level @GetRequiredLevel(approach)</span>
                }
                @if (!canAfford)
                {
                    <span class="resource-requirement">Not enough Attention</span>
                }
            </div>
        }
    </div>
}
```

#### Step 2: Add Helper Methods to LocationContent.razor.cs
**File:** `src/Pages/Components/LocationContent.razor.cs`

**ADD METHODS:**
```csharp
protected List<string> GetAllInvestigationApproaches()
{
    // Return all 5 stat-based approaches
    return new List<string>
    {
        "systematic_observation",    // Insight 2+
        "local_inquiry",              // Rapport 2+
        "demand_access",              // Authority 2+
        "purchase_information",       // Diplomacy 2+
        "covert_search"               // Cunning 2+
    };
}

protected bool MeetsStatRequirement(string approach)
{
    Player player = GameWorld.GetPlayer();

    return approach switch
    {
        "systematic_observation" => player.Stats.Insight.Level >= 2,
        "local_inquiry" => player.Stats.Rapport.Level >= 2,
        "demand_access" => player.Stats.Authority.Level >= 2,
        "purchase_information" => player.Stats.Diplomacy.Level >= 2,
        "covert_search" => player.Stats.Cunning.Level >= 2,
        _ => true // Default approach (basic investigation) always available
    };
}

protected string GetRequiredStat(string approach)
{
    return approach switch
    {
        "systematic_observation" => "Insight",
        "local_inquiry" => "Rapport",
        "demand_access" => "Authority",
        "purchase_information" => "Diplomacy",
        "covert_search" => "Cunning",
        _ => "None"
    };
}

protected int GetRequiredLevel(string approach)
{
    return approach switch
    {
        "covert_search" => 3, // Cunning requires level 3
        _ => 2 // All others require level 2
    };
}
```

#### Step 3: Add CSS for Locked Approaches
**File:** `src/wwwroot/css/location.css`

**ADD:**
```css
.approach-locked {
    opacity: 0.6;
    cursor: not-allowed;
    background: #1a1410;
}

.approach-locked .approach-name {
    color: #7a6250;
}

.lock-icon {
    margin-right: 6px;
    opacity: 0.7;
}

.approach-requirement {
    margin-top: 8px;
    padding-top: 8px;
    border-top: 1px solid #3a3020;
    font-size: 11px;
    color: #9a8270;
}

.stat-requirement {
    display: block;
    margin-bottom: 4px;
}

.resource-requirement {
    display: block;
    color: #ca6c4c;
}
```

### Verification
After fix:
- ✅ All 5 investigation approaches visible
- ✅ Locked approaches show: "Requires Insight Level 2"
- ✅ Lock icon visible on locked approaches
- ✅ Greyed out and unclickable
- ✅ Unlocked approaches fully functional

---

## Issue 8: Add Discovery Journal/Tracking

### Problem
No way to track what you've discovered. Explorer persona felt completion wasn't recognized.

### Solution

#### Step 1: Create Discovery Journal Component
**File:** `src/Pages/Components/DiscoveryJournal.razor` (NEW)

```razor
@inherits DiscoveryJournalBase
@namespace Wayfarer.Pages.Components

<div class="discovery-journal">
    <div class="journal-header">
        <h2>Discovery Journal</h2>
        <button class="close-btn" @onclick="CloseJournal">✕</button>
    </div>

    <div class="journal-content">
        <!-- Locations Section -->
        <div class="journal-section">
            <h3>Locations Discovered (@GetDiscoveredLocationCount()/@GetTotalLocationCount())</h3>
            <div class="location-list">
                @foreach (var location in GetDiscoveredLocations())
                {
                    <div class="location-entry">
                        <div class="location-name">@location.Name</div>
                        <div class="familiarity-bar">
                            <span class="familiarity-label">Familiarity: @GetFamiliarity(location.Id)/@location.MaxFamiliarity</span>
                            <div class="progress-bar">
                                <div class="progress-fill" style="width: @GetFamiliarityPercent(location.Id, location.MaxFamiliarity)%"></div>
                            </div>
                        </div>
                        @if (GetFamiliarity(location.Id) >= location.MaxFamiliarity)
                        {
                            <span class="complete-badge">✓ Fully Explored</span>
                        }
                    </div>
                }
            </div>
        </div>

        <!-- Observations Section -->
        <div class="journal-section">
            <h3>Observations Collected (@GetCollectedObservationCount()/@GetTotalObservationCount())</h3>
            <div class="observation-list">
                @foreach (var obs in GetCollectedObservations())
                {
                    <div class="observation-entry">
                        <div class="observation-name">@obs.Name</div>
                        <div class="observation-effect">@obs.Effect</div>
                        <div class="observation-target">Useful with: @obs.TargetNpcName</div>
                    </div>
                }
            </div>
        </div>

        <!-- Routes Section -->
        <div class="journal-section">
            <h3>Routes Explored (@GetExploredRouteCount()/@GetTotalRouteCount())</h3>
            <div class="route-list">
                @foreach (var route in GetKnownRoutes())
                {
                    <div class="route-entry">
                        <div class="route-path">@route.OriginName → @route.DestinationName</div>
                        <div class="route-familiarity">
                            Familiarity: @GetRouteFamiliarity(route.Id)/5
                            @if (GetRouteFamiliarity(route.Id) >= 5)
                            {
                                <span class="mastered-badge">★ Mastered</span>
                            }
                        </div>
                    </div>
                }
            </div>
        </div>

        <!-- Path Cards Section -->
        <div class="journal-section">
            <h3>Path Cards Revealed (@GetRevealedPathCardCount()/@GetTotalPathCardCount())</h3>
            <div class="path-card-summary">
                <div>Face-up cards: @GetFaceUpCardCount()</div>
                <div>Face-down cards: @GetFaceDownCardCount()</div>
            </div>
        </div>
    </div>
</div>
```

**File:** `src/Pages/Components/DiscoveryJournal.razor.cs` (NEW)

```csharp
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components
{
    public class DiscoveryJournalBase : ComponentBase
    {
        [Inject] protected GameWorld GameWorld { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        protected async Task CloseJournal()
        {
            await OnClose.InvokeAsync();
        }

        protected int GetDiscoveredLocationCount()
        {
            Player player = GameWorld.GetPlayer();
            return player.LocationFamiliarity.Count;
        }

        protected int GetTotalLocationCount()
        {
            return GameWorld.Locations.Count;
        }

        protected List<Location> GetDiscoveredLocations()
        {
            Player player = GameWorld.GetPlayer();
            return GameWorld.Locations
                .Where(l => player.LocationFamiliarity.Any(f => f.Id == l.Id))
                .OrderBy(l => l.Name)
                .ToList();
        }

        protected int GetFamiliarity(string locationId)
        {
            return GameWorld.GetPlayer().GetLocationFamiliarity(locationId);
        }

        protected double GetFamiliarityPercent(string locationId, int max)
        {
            return (double)GetFamiliarity(locationId) / max * 100.0;
        }

        protected int GetCollectedObservationCount()
        {
            return GameWorld.GetPlayer().CollectedObservations.Count;
        }

        protected int GetTotalObservationCount()
        {
            return GameWorld.Observations.Count;
        }

        protected List<ObservationInfo> GetCollectedObservations()
        {
            Player player = GameWorld.GetPlayer();
            return GameWorld.Observations
                .Where(o => player.CollectedObservations.Contains(o.Id))
                .Select(o => new ObservationInfo
                {
                    Name = o.Name,
                    Effect = o.DisplayText,
                    TargetNpcName = GetNpcName(o.TargetNpcId)
                })
                .ToList();
        }

        protected string GetNpcName(string npcId)
        {
            return GameWorld.NPCs.FirstOrDefault(n => n.ID == npcId)?.Name ?? "Unknown";
        }

        protected int GetExploredRouteCount()
        {
            return GameWorld.GetPlayer().KnownRoutes.Sum(kr => kr.Routes.Count);
        }

        protected int GetTotalRouteCount()
        {
            // Count all routes in the game
            // This is a placeholder - actual implementation depends on route storage
            return 20; // TODO: Get actual total from GameWorld
        }

        protected List<RouteInfo> GetKnownRoutes()
        {
            Player player = GameWorld.GetPlayer();
            List<RouteInfo> routes = new List<RouteInfo>();

            foreach (var entry in player.KnownRoutes)
            {
                foreach (var route in entry.Routes)
                {
                    routes.Add(new RouteInfo
                    {
                        Id = $"{route.OriginLocationSpot}_{route.DestinationLocationSpot}",
                        OriginName = route.OriginLocationSpot,
                        DestinationName = route.DestinationLocationSpot,
                        Familiarity = player.GetRouteFamiliarity($"{route.OriginLocationSpot}_{route.DestinationLocationSpot}")
                    });
                }
            }

            return routes.OrderBy(r => r.OriginName).ToList();
        }

        protected int GetRouteFamiliarity(string routeId)
        {
            return GameWorld.GetPlayer().GetRouteFamiliarity(routeId);
        }

        protected int GetRevealedPathCardCount()
        {
            return GameWorld.PathCardDiscoveries.Count(p => p.IsRevealed);
        }

        protected int GetTotalPathCardCount()
        {
            return GameWorld.PathCardDiscoveries.Count;
        }

        protected int GetFaceUpCardCount()
        {
            return GameWorld.PathCardDiscoveries.Count(p => p.IsRevealed);
        }

        protected int GetFaceDownCardCount()
        {
            return GameWorld.PathCardDiscoveries.Count(p => !p.IsRevealed);
        }
    }

    public class ObservationInfo
    {
        public string Name { get; set; }
        public string Effect { get; set; }
        public string TargetNpcName { get; set; }
    }

    public class RouteInfo
    {
        public string Id { get; set; }
        public string OriginName { get; set; }
        public string DestinationName { get; set; }
        public int Familiarity { get; set; }
    }
}
```

#### Step 2: Add Journal Button to GameScreen
**File:** `src/Pages/GameScreen.razor`

**Add button to header:**
```razor
<div class="game-header">
    <div class="header-buttons">
        <button class="header-btn" @onclick="ToggleJournal">
            📖 Journal
        </button>
    </div>
</div>

@if (_showJournal)
{
    <div class="modal-overlay" @onclick="ToggleJournal">
        <div class="modal-content" @onclick:stopPropagation>
            <DiscoveryJournal OnClose="ToggleJournal" />
        </div>
    </div>
}

@code {
    private bool _showJournal = false;

    private void ToggleJournal()
    {
        _showJournal = !_showJournal;
    }
}
```

#### Step 3: Add CSS
**File:** `src/wwwroot/css/discovery-journal.css` (NEW)

```css
.discovery-journal {
    background: #1a1410;
    border: 2px solid #7a6250;
    border-radius: 8px;
    width: 800px;
    max-height: 80vh;
    overflow-y: auto;
    padding: 24px;
}

.journal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
    border-bottom: 2px solid #7a6250;
    padding-bottom: 12px;
}

.journal-header h2 {
    color: #f4e4c1;
    margin: 0;
}

.close-btn {
    background: transparent;
    border: none;
    color: #f4e4c1;
    font-size: 24px;
    cursor: pointer;
}

.journal-section {
    margin-bottom: 32px;
}

.journal-section h3 {
    color: #d4c4a1;
    margin-bottom: 16px;
}

.location-entry, .observation-entry, .route-entry {
    background: #2a2419;
    padding: 12px;
    margin-bottom: 8px;
    border-radius: 4px;
}

.familiarity-bar {
    margin-top: 8px;
}

.progress-bar {
    width: 100%;
    height: 8px;
    background: #3a3020;
    border-radius: 4px;
    overflow: hidden;
}

.progress-fill {
    height: 100%;
    background: #7a6250;
    transition: width 0.3s;
}

.complete-badge, .mastered-badge {
    display: inline-block;
    margin-top: 8px;
    padding: 4px 8px;
    background: #4a7050;
    color: #f4e4c1;
    border-radius: 4px;
    font-size: 12px;
}
```

### Verification
After fix:
- ✅ Journal button visible in header
- ✅ Clicking opens modal with discovery journal
- ✅ Shows all discovered locations with familiarity
- ✅ Shows all collected observations
- ✅ Shows all explored routes
- ✅ Progress tracking visible (X/Y format)

---

## Issue 12: Add Stranger Schedules/Hints

### Problem
Time-gated NPCs are invisible. No hints when/where they appear.

### Solution

#### Step 1: Add Schedule Properties to NPC
**File:** `src/GameState/NPC.cs`

**ADD PROPERTIES:**
```csharp
public class NPC
{
    // ... existing properties

    /// <summary>
    /// Time blocks when this NPC is available
    /// </summary>
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();

    /// <summary>
    /// Location IDs where this NPC can be encountered
    /// </summary>
    public List<string> AvailableLocationIds { get; set; } = new List<string>();

    /// <summary>
    /// Human-readable schedule description
    /// </summary>
    public string ScheduleDescription { get; set; }

    /// <summary>
    /// Check if NPC is currently available based on time and location
    /// </summary>
    public bool IsAvailableNow(TimeBlocks currentTime, string currentLocationId)
    {
        bool rightTime = AvailableTimeBlocks.Count == 0 || AvailableTimeBlocks.Contains(currentTime);
        bool rightPlace = AvailableLocationIds.Count == 0 || AvailableLocationIds.Contains(currentLocationId);
        return rightTime && rightPlace;
    }
}
```

#### Step 2: Create Stranger Schedule Display
**File:** `src/Pages/Components/StrangerScheduleDisplay.razor` (NEW)

```razor
@inherits StrangerScheduleDisplayBase
@namespace Wayfarer.Pages.Components

<div class="stranger-schedule-panel">
    <h3>Known Strangers</h3>

    @foreach (var timeBlock in GetAllTimeBlocks())
    {
        var strangers = GetStrangersForTimeBlock(timeBlock);
        if (strangers.Any())
        {
            <div class="time-block-section">
                <div class="time-block-header">@timeBlock</div>
                @foreach (var stranger in strangers)
                {
                    bool isAvailableNow = IsStrangerAvailableNow(stranger);
                    <div class="stranger-entry @(isAvailableNow ? "available-now" : "")">
                        <div class="stranger-name">
                            @stranger.Name
                            @if (isAvailableNow)
                            {
                                <span class="available-badge">Available Now!</span>
                            }
                        </div>
                        <div class="stranger-personality">@stranger.PersonalityType</div>
                        <div class="stranger-schedule">@stranger.ScheduleDescription</div>
                    </div>
                }
            </div>
        }
    }
</div>

@code {
    [Inject] protected GameWorld GameWorld { get; set; }

    protected List<TimeBlocks> GetAllTimeBlocks()
    {
        return new List<TimeBlocks>
        {
            TimeBlocks.Dawn,
            TimeBlocks.Morning,
            TimeBlocks.Afternoon,
            TimeBlocks.Evening,
            TimeBlocks.Night,
            TimeBlocks.LateNight
        };
    }

    protected List<NPC> GetStrangersForTimeBlock(TimeBlocks timeBlock)
    {
        return GameWorld.NPCs
            .Where(npc => npc.AvailableTimeBlocks.Contains(timeBlock))
            .ToList();
    }

    protected bool IsStrangerAvailableNow(NPC stranger)
    {
        Player player = GameWorld.GetPlayer();
        string currentLocationId = player.CurrentLocationSpot?.LocationId;
        return stranger.IsAvailableNow(GameWorld.CurrentTimeBlock, currentLocationId);
    }
}
```

#### Step 3: Update JSON with Stranger Schedules
**Option A:** Update existing NPCs in `03_npcs.json`
**Option B:** Create new `06_strangers.json`

**Example Entry:**
```json
{
  "id": "traveling_merchant",
  "name": "Traveling Merchant",
  "personalityType": "MERCANTILE",
  "level": 2,
  "description": "A shrewd trader with goods from distant lands",
  "availableTimeBlocks": ["Morning", "Afternoon"],
  "availableLocationIds": ["market_square", "copper_kettle_tavern"],
  "scheduleDescription": "Mornings and afternoons at Market Square and Copper Kettle",
  "conversationDeckId": "trade_negotiation"
}
```

#### Step 4: Add Schedule Hints to LocationContent
**File:** `src/Pages/Components/LocationContent.razor`

**Add section after atmosphere:**
```razor
@if (GetExpectedStrangers().Any())
{
    <div class="expected-strangers">
        <div class="section-subheader">Expected Visitors</div>
        <div class="stranger-hints">
            @foreach (var stranger in GetExpectedStrangers())
            {
                <div class="stranger-hint">
                    <span class="stranger-name">@stranger.Name</span>
                    <span class="stranger-times">(@GetTimeBlockList(stranger))</span>
                </div>
            }
        </div>
    </div>
}
```

**Add helper method to LocationContent.razor.cs:**
```csharp
protected List<NPC> GetExpectedStrangers()
{
    if (CurrentLocation == null) return new List<NPC>();

    return GameWorld.NPCs
        .Where(npc => npc.AvailableLocationIds.Contains(CurrentLocation.Id))
        .OrderBy(npc => npc.Name)
        .ToList();
}

protected string GetTimeBlockList(NPC stranger)
{
    if (!stranger.AvailableTimeBlocks.Any()) return "Anytime";
    return string.Join(", ", stranger.AvailableTimeBlocks.Select(tb => tb.ToString()));
}
```

#### Step 5: Add System Messages for Stranger Arrivals
**File:** `src/GameState/NPCVisibilityService.cs` (or wherever stranger visibility is managed)

**Add notification when stranger becomes available:**
```csharp
public void OnTimeBlockChange(TimeBlocks newTimeBlock)
{
    Player player = _gameWorld.GetPlayer();
    string currentLocationId = player.CurrentLocationSpot?.LocationId;

    // Check for newly available strangers
    var newlyAvailable = _gameWorld.NPCs
        .Where(npc => npc.IsAvailableNow(newTimeBlock, currentLocationId))
        .Where(npc => !_previouslyAvailable.Contains(npc.ID))
        .ToList();

    foreach (var stranger in newlyAvailable)
    {
        _gameWorld.MessageSystem.AddSystemMessage(
            $"A stranger has arrived: {stranger.Name}",
            SystemMessageTypes.Information
        );
    }

    _previouslyAvailable = newlyAvailable.Select(s => s.ID).ToList();
}
```

### Verification
After fix:
- ✅ Stranger schedules visible in UI
- ✅ "Available Now!" badge for current strangers
- ✅ Location shows "Expected Visitors" with time blocks
- ✅ System message when stranger arrives
- ✅ Schedule descriptions loaded from JSON

---

## Implementation Order

Execute in this sequence:

### Phase 1: Critical Fixes (Blocks POC Testing)
1. ✅ Fix starting conditions (Player.cs + PackageStartingConditions)
2. ✅ Fix MERCANTILE personality display & mechanics
3. ✅ Build and test - verify POC scenario loads correctly

### Phase 2: Accessibility (Helps All Personas)
4. ✅ Create TooltipWrapper component
5. ✅ Create TooltipContentProvider
6. ✅ Add tooltips to GameScreen, ConversationContent, LocationContent
7. ✅ Build and test - verify tooltips work

### Phase 3: Discoverability (Helps Explorer & Optimizer)
8. ✅ Add stat requirement display to investigation approaches
9. ✅ Build and test - verify locked approaches show requirements

### Phase 4: Tracking (Nice-to-Have)
10. ✅ Create DiscoveryJournal component
11. ✅ Add journal button to GameScreen
12. ✅ Build and test - verify journal displays correctly

### Phase 5: Stranger System (Nice-to-Have)
13. ✅ Add schedule properties to NPC class
14. ✅ Create StrangerScheduleDisplay component
15. ✅ Add schedule hints to LocationContent
16. ✅ Add system messages for stranger arrivals
17. ✅ Build and test - verify stranger schedules work

### Phase 6: Final Testing
18. ✅ Complete smoke test of all 6 fixes
19. ✅ Run abbreviated playtest with Optimizer persona
20. ✅ Document any remaining issues

---

## Success Criteria

All fixes considered complete when:

1. **Starting Conditions:**
   - ✅ Health: 100/100
   - ✅ Hunger: 50/100
   - ✅ Coins: 0
   - ✅ Time: Morning Segment 4 (or as specified in JSON)
   - ✅ Viktor's Package in queue

2. **Personality Display:**
   - ✅ MERCANTILE shows: "Highest Initiative card gains +3 Momentum"
   - ✅ Playing highest Initiative card grants +3 Momentum
   - ✅ Subsequent cards with same Initiative don't get bonus

3. **Tooltips:**
   - ✅ 40+ game terms have tooltips
   - ✅ Hovering shows clear explanations
   - ✅ Present on all major screens

4. **Stat Requirements:**
   - ✅ All investigation approaches visible
   - ✅ Locked approaches show requirements
   - ✅ Lock icons present
   - ✅ Unclickable when locked

5. **Discovery Journal:**
   - ✅ Journal button in header
   - ✅ Shows discovered locations with familiarity
   - ✅ Shows collected observations
   - ✅ Shows explored routes
   - ✅ Progress tracking visible

6. **Stranger Schedules:**
   - ✅ Schedule properties on NPCs
   - ✅ "Available Now!" badge works
   - ✅ Location shows expected visitors
   - ✅ System messages on arrivals

---

## Rollback Plan

If issues arise:

1. **Git branch:** Create `playtesting-fixes` branch before starting
2. **Commit frequently:** After each completed fix
3. **Rollback:** `git checkout master` if critical bugs appear
4. **Selective merge:** Can merge individual commits if some fixes work

---

## Post-Implementation

After all fixes complete:

1. Update `test-results-session1.md` with "FIXED" status
2. Create `test-results-session2.md` for retest
3. Spawn same 5 personas for verification testing
4. Document any new issues discovered
5. Commit with message: "Fix 6 critical playtesting issues from Session 1"

---

## Estimated Timeline

- **Phase 1 (Critical):** 2 hours
- **Phase 2 (Tooltips):** 3 hours
- **Phase 3 (Stat Gates):** 1 hour
- **Phase 4 (Journal):** 2 hours
- **Phase 5 (Strangers):** 2 hours
- **Phase 6 (Testing):** 2 hours

**Total:** ~12 hours implementation + testing

---

This plan addresses all 6 critical issues identified by the 5 playtesting personas, prioritized by impact and implementation complexity.
