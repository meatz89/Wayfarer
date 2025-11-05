# Perfect Information Implementation Plan
**Status**: Ready for Implementation (Plan Approved)
**Inspired By**: The Life and Suffering of Sir Brante choice display design
**Goal**: Show players EXACT requirements and cascading final values for strategic decision-making

---

## Problem Statement

**Current State**:
- ✅ Scene choice CSS cascade issue FIXED (removed `action-card` class pollution from location.css)
- ✅ Locked cards fully readable (no dark overlay)
- ✅ CONSEQUENCES section implemented (shows costs + rewards)
- ❌ Lock indicator shows vague "REQUIREMENTS NOT MET" (no current values, no gaps)
- ❌ Consequences don't show final values (player can't calculate cascading effects)

**Target State** (Sir Brante Pattern):
- Lock indicator shows: "Insight 3+ (have 1)" - player sees exact gap
- Consequences show: "💰 +10 Coins (will have 18)" - player plans cascading spending
- Relationship changes show: "💙 +1 Elena (have 3, will have 4)" - strategic bond management
- ALL information visible BEFORE selection (Perfect Information principle)

---

## Sir Brante Design Analysis

**What Makes It Excellent**:

1. **CONDITIONS MET Panel**:
   - Shows exact formula: "WILLPOWER > 0"
   - Shows current value: "(now 0)"
   - Player calculates gap mentally (need >0, have 0 = blocked)
   - Clean, compact, information-dense

2. **CONSEQUENCES Panel**:
   - Shows ALL consequences BEFORE selection
   - Resource changes with delta AND final value: "WILLPOWER -5 (now 0)"
   - Relationship changes: Qualitative + Quantitative: "LYDIA BRANTE: GRATEFUL" + "+1 (now 1)"
   - Organized by category: PERSONALITY, RELATIONS

3. **Visual Hierarchy**:
   - ALL CAPS section headers
   - Clear separation between CONDITIONS and CONSEQUENCES
   - Minimal decoration, maximum information density

---

## Architecture: 5-Layer Implementation

### Layer 1: Domain Fix (NumericRequirement.cs)

**File**: `src\GameState\NumericRequirement.cs`
**Line**: 66-72

**Current Code**:
```csharp
private bool CheckBondStrength(Player player, string npcId, int threshold)
{
    // Assuming NPC entities have BondStrength property
    // Will be implemented when NPC.BondStrength property exists
    // For now, return false
    return false; // TODO: Implement when NPC.BondStrength exists
}
```

**Replace With**:
```csharp
private bool CheckBondStrength(Player player, string npcId, int threshold)
{
    // Sum all token types for overall relationship strength
    NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.NpcId == npcId);
    if (entry == null) return false;

    int totalBond = entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow;
    return totalBond >= threshold;
}
```

**Why**: The requirement system checks "BondStrength" but the actual system tracks 4 separate ConnectionTypes (Trust, Diplomacy, Status, Shadow). This maps BondStrength to total tokens across all types, matching game design of "overall relationship strength."

---

### Layer 2: ViewModel Enhancement (GameViewModels.cs)

**File**: `src\ViewModels\GameViewModels.cs`

**2A. Add Final Values to BondChangeVM** (line 337-342):

```csharp
public class BondChangeVM
{
    public string NpcName { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; }
    public int CurrentBond { get; set; }    // NEW: Current bond before change
    public int FinalBond { get; set; }       // NEW: Final bond after change
}
```

**2B. Add Final Values to ScaleShiftVM** (line 344-349):

```csharp
public class ScaleShiftVM
{
    public string ScaleName { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; }
    public int CurrentScale { get; set; }    // NEW
    public int FinalScale { get; set; }       // NEW
}
```

**2C. Add Final Values to ActionCardViewModel** (after line 316):

```csharp
// Add these new properties to ActionCardViewModel
// Final values after this choice (for Sir Brante-style display)
public int FinalCoins { get; set; }
public int FinalResolve { get; set; }
public int FinalHealth { get; set; }
public int FinalStamina { get; set; }
public int FinalFocus { get; set; }
public int FinalHunger { get; set; }
```

---

### Layer 3: Data Mapping (SceneContent.razor.cs)

**File**: `src\Pages\Components\SceneContent.razor.cs`

**3A. Add Requirement Gap Analysis Methods** (add after LoadChoices method):

```csharp
/// <summary>
/// Extract requirement gaps in Sir Brante format: "Insight 3+ (have 1)"
/// </summary>
private List<string> GetRequirementGaps(CompoundRequirement formula, Player player)
{
    List<string> gaps = new List<string>();

    foreach (OrPath orPath in formula.OrPaths)
    {
        if (!orPath.IsSatisfied(player, GameWorld))
        {
            foreach (NumericRequirement req in orPath.NumericRequirements)
            {
                if (!req.IsSatisfied(player, GameWorld))
                {
                    gaps.Add(FormatRequirementGap(req, player));
                }
            }
        }
    }

    return gaps;
}

/// <summary>
/// Format requirement gap: "Type threshold (have current)"
/// </summary>
private string FormatRequirementGap(NumericRequirement req, Player player)
{
    return req.Type switch
    {
        "PlayerStat" => FormatPlayerStatGap(req, player),
        "Resolve" => $"Resolve {req.Threshold}+ (have {player.Resolve})",
        "Coins" => $"{req.Threshold} Coins (have {player.Coins})",
        "Scale" => FormatScaleGap(req, player),
        "BondStrength" => FormatBondGap(req, player),
        "CompletedSituations" => $"Complete {req.Threshold} situations (have {player.CompletedSituationIds.Count})",
        "Achievement" => req.Threshold > 0 ? $"Need achievement: {req.Context}" : $"Cannot have: {req.Context}",
        "State" => req.Threshold > 0 ? $"Need state: {req.Context}" : $"Cannot have state: {req.Context}",
        "HasItem" => req.Threshold > 0 ? $"Need item: {req.Context}" : $"Cannot have item: {req.Context}",
        _ => "Unknown requirement"
    };
}

private string FormatPlayerStatGap(NumericRequirement req, Player player)
{
    StatType statType = Enum.Parse<StatType>(req.Context);
    int currentLevel = player.Stats.GetLevel(statType);
    return $"{req.Context} {req.Threshold}+ (have {currentLevel})";
}

private string FormatScaleGap(NumericRequirement req, Player player)
{
    int currentScale = req.Context switch
    {
        "Morality" => player.Scales.Morality,
        "Lawfulness" => player.Scales.Lawfulness,
        "Method" => player.Scales.Method,
        "Caution" => player.Scales.Caution,
        "Transparency" => player.Scales.Transparency,
        "Fame" => player.Scales.Fame,
        _ => 0
    };

    return $"{req.Context} {req.Threshold}+ (have {currentScale})";
}

private string FormatBondGap(NumericRequirement req, Player player)
{
    NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.NpcId == req.Context);
    int totalBond = 0;
    if (entry != null)
    {
        totalBond = entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow;
    }

    NPC npc = GameWorld.NPCs.FirstOrDefault(n => n.ID == req.Context);
    string npcName = npc?.Name ?? req.Context;

    return $"Bond {req.Threshold}+ with {npcName} (have {totalBond})";
}

/// <summary>
/// Get total bond strength across all connection types
/// </summary>
private int GetTotalBond(Player player, string npcId)
{
    NPCTokenEntry entry = player.NPCTokens.FirstOrDefault(t => t.NpcId == npcId);
    if (entry == null) return 0;

    return entry.Trust + entry.Diplomacy + entry.Status + entry.Shadow;
}
```

**3B. Update LoadChoices() Method** (lines 58-66 and 122-182):

**Replace requirement validation section** (lines 58-66):
```csharp
// Validate RequirementFormula
if (choiceTemplate.RequirementFormula != null && choiceTemplate.RequirementFormula.OrPaths.Count > 0)
{
    requirementsMet = choiceTemplate.RequirementFormula.IsAnySatisfied(player, GameWorld);
    if (!requirementsMet)
    {
        // NEW: Get specific requirement gaps instead of generic string
        List<string> gaps = GetRequirementGaps(choiceTemplate.RequirementFormula, player);
        lockReason = string.Join(" OR ", gaps);  // Show all gaps with OR between
    }
}
```

**Update BondChange mapping** (around line 123-136):
```csharp
// Map relationship consequences (BondChanges) WITH FINAL VALUES
List<BondChangeVM> bondChanges = new List<BondChangeVM>();
if (reward?.BondChanges != null)
{
    foreach (BondChange bondChange in reward.BondChanges)
    {
        NPC npc = GameWorld.NPCs.FirstOrDefault(n => n.ID == bondChange.NpcId);
        int currentBond = GetTotalBond(player, bondChange.NpcId);

        bondChanges.Add(new BondChangeVM
        {
            NpcName = npc?.Name ?? bondChange.NpcId,
            Delta = bondChange.Delta,
            Reason = bondChange.Reason ?? "",
            CurrentBond = currentBond,                      // NEW
            FinalBond = currentBond + bondChange.Delta      // NEW
        });
    }
}
```

**Update ScaleShift mapping** (around line 138-151):
```csharp
// Map reputation consequences (ScaleShifts) WITH FINAL VALUES
List<ScaleShiftVM> scaleShifts = new List<ScaleShiftVM>();
if (reward?.ScaleShifts != null)
{
    foreach (ScaleShift scaleShift in reward.ScaleShifts)
    {
        int currentScale = scaleShift.ScaleType.ToString() switch
        {
            "Morality" => player.Scales.Morality,
            "Lawfulness" => player.Scales.Lawfulness,
            "Method" => player.Scales.Method,
            "Caution" => player.Scales.Caution,
            "Transparency" => player.Scales.Transparency,
            "Fame" => player.Scales.Fame,
            _ => 0
        };

        scaleShifts.Add(new ScaleShiftVM
        {
            ScaleName = scaleShift.ScaleType.ToString(),
            Delta = scaleShift.Delta,
            Reason = scaleShift.Reason ?? "",
            CurrentScale = currentScale,                    // NEW
            FinalScale = currentScale + scaleShift.Delta    // NEW
        });
    }
}
```

**Update ActionCardViewModel instantiation** (around line 183-221):
```csharp
ActionCardViewModel choice = new ActionCardViewModel
{
    Id = choiceTemplate.Id,
    Name = choiceTemplate.ActionTextTemplate,
    Description = "",
    RequirementsMet = requirementsMet,
    LockReason = lockReason,

    // All costs
    ResolveCost = resolveCost,
    CoinsCost = coinsCost,
    TimeSegments = timeSegments,
    HealthCost = healthCost,
    StaminaCost = staminaCost,
    FocusCost = focusCost,
    HungerCost = hungerCost,

    // All rewards
    CoinsReward = coinsReward,
    ResolveReward = resolveReward,
    HealthReward = healthReward,
    StaminaReward = staminaReward,
    FocusReward = focusReward,
    HungerChange = hungerChange,
    FullRecovery = fullRecovery,

    // NEW: Final values (Sir Brante pattern)
    FinalCoins = player.Coins + coinsReward - coinsCost,
    FinalResolve = player.Resolve + resolveReward - resolveCost,
    FinalHealth = player.Health + healthReward - healthCost,
    FinalStamina = player.Stamina + staminaReward - staminaCost,
    FinalFocus = player.Focus + focusReward - focusCost,
    FinalHunger = player.Hunger + hungerChange + hungerCost,

    // All consequences
    BondChanges = bondChanges,
    ScaleShifts = scaleShifts,
    StateApplications = stateApplications,

    // All progression unlocks
    AchievementsGranted = achievementsGranted,
    ItemsGranted = itemsGranted,
    LocationsUnlocked = locationsUnlocked,
    ScenesUnlocked = scenesUnlocked
};
```

---

### Layer 4: UI Rendering (SceneContent.razor)

**File**: `src\Pages\Components\SceneContent.razor`

**4A. Update Lock Indicator** (lines 30-36):

```razor
@if (!choice.RequirementsMet && !string.IsNullOrEmpty(choice.LockReason))
{
    <div class="scene-locked-indicator">
        <div class="lock-header">🔒 LOCKED</div>
        @foreach (var gap in choice.LockReason.Split(" OR "))
        {
            <div class="requirement-gap">@gap</div>
        }
    </div>
}
```

**4B. Update Resource Rewards with Final Values** (around lines 97-120):

```razor
@* Resource rewards WITH FINAL VALUES (Sir Brante style) *@
@if (choice.FullRecovery)
{
    <div class="consequence-item">✨ Full Recovery (all resources restored)</div>
}
@if (choice.CoinsReward != 0)
{
    <div class="consequence-item">
        💰 @(choice.CoinsReward > 0 ? "+" : "")@choice.CoinsReward Coins
        <span class="final-value">(will have @choice.FinalCoins)</span>
    </div>
}
@if (choice.ResolveReward != 0)
{
    <div class="consequence-item">
        🎯 @(choice.ResolveReward > 0 ? "+" : "")@choice.ResolveReward Resolve
        <span class="final-value">(will have @choice.FinalResolve)</span>
    </div>
}
@if (choice.HealthReward != 0)
{
    <div class="consequence-item">
        ❤️ @(choice.HealthReward > 0 ? "+" : "")@choice.HealthReward Health
        <span class="final-value">(will have @choice.FinalHealth)</span>
    </div>
}
@if (choice.StaminaReward != 0)
{
    <div class="consequence-item">
        💪 @(choice.StaminaReward > 0 ? "+" : "")@choice.StaminaReward Stamina
        <span class="final-value">(will have @choice.FinalStamina)</span>
    </div>
}
@if (choice.FocusReward != 0)
{
    <div class="consequence-item">
        🔷 @(choice.FocusReward > 0 ? "+" : "")@choice.FocusReward Focus
        <span class="final-value">(will have @choice.FinalFocus)</span>
    </div>
}
@if (choice.HungerChange != 0)
{
    <div class="consequence-item">
        🍽️ @(choice.HungerChange > 0 ? "+" : "")@choice.HungerChange Hunger
        <span class="final-value">(will have @choice.FinalHunger)</span>
    </div>
}
```

**4C. Update Relationship Consequences** (around lines 123-144):

```razor
@* Relationship consequences WITH FINAL VALUES *@
@foreach (var bondChange in choice.BondChanges)
{
    <div class="consequence-item consequence-bond">
        💙 @(bondChange.Delta > 0 ? "+" : "")@bondChange.Delta Bond with @bondChange.NpcName
        <span class="final-value">(have @bondChange.CurrentBond, will have @bondChange.FinalBond)</span>
        @if (!string.IsNullOrEmpty(bondChange.Reason))
        {
            <div class="consequence-reason">@bondChange.Reason</div>
        }
    </div>
}

@* Reputation consequences WITH FINAL VALUES *@
@foreach (var scaleShift in choice.ScaleShifts)
{
    <div class="consequence-item consequence-scale">
        ⚖️ @(scaleShift.Delta > 0 ? "+" : "")@scaleShift.Delta @scaleShift.ScaleName
        <span class="final-value">(have @scaleShift.CurrentScale, will have @scaleShift.FinalScale)</span>
        @if (!string.IsNullOrEmpty(scaleShift.Reason))
        {
            <div class="consequence-reason">@scaleShift.Reason</div>
        }
    </div>
}
```

---

### Layer 5: CSS Styling (scene.css)

**File**: `src\wwwroot\css\scene.css`

**Add after .lock-text styling** (around line 212):

```css
/* Lock indicator - Sir Brante compact style */
.lock-header {
    font-size: 10px;
    font-weight: 700;
    color: #d4704a;
    letter-spacing: 0.8px;
    margin-bottom: 6px;
    text-transform: uppercase;
}

.requirement-gap {
    font-size: 11px;
    color: #7a6250;
    line-height: 1.4;
    margin-bottom: 3px;
}

/* Final values - muted but visible (Sir Brante style) */
.final-value {
    font-size: 10px;
    color: #8b7355;
    font-style: italic;
    margin-left: 4px;
}
```

**Update .choice-section-title** (line 223-229):

```css
.choice-section-title {
    font-size: 10px;
    font-weight: 700;
    color: #7a6250;
    letter-spacing: 1px;
    margin-bottom: 6px;
    text-transform: uppercase;  /* ADD: Sir Brante ALL CAPS style */
}
```

---

## Implementation Checklist

### Layer 2: Domain (NumericRequirement.cs)
- [ ] Replace CheckBondStrength TODO with total bond calculation (line 66-72)

### Layer 1: ViewModel (GameViewModels.cs)
- [ ] Add CurrentBond + FinalBond to BondChangeVM (line 337-342)
- [ ] Add CurrentScale + FinalScale to ScaleShiftVM (line 344-349)
- [ ] Add Final resource properties to ActionCardViewModel (after line 316)

### Layer 3: Data Mapping (SceneContent.razor.cs)
- [ ] Add GetRequirementGaps() method
- [ ] Add FormatRequirementGap() method
- [ ] Add helper methods (FormatPlayerStatGap, FormatScaleGap, FormatBondGap, GetTotalBond)
- [ ] Update requirement validation to use GetRequirementGaps() (line 58-66)
- [ ] Update BondChange mapping with current/final values (line 123-136)
- [ ] Update ScaleShift mapping with current/final values (line 138-151)
- [ ] Update ActionCardViewModel instantiation with final values (line 183-221)

### Layer 4: UI (SceneContent.razor)
- [ ] Update lock indicator to display specific gaps (line 30-36)
- [ ] Add final values to resource rewards (line 97-120)
- [ ] Add current/final values to bond changes (line 123-132)
- [ ] Add current/final values to scale shifts (line 135-144)

### Layer 5: CSS (scene.css)
- [ ] Add .lock-header styling (uppercase, compact)
- [ ] Add .requirement-gap styling (readable gaps)
- [ ] Add .final-value styling (muted italics)
- [ ] Update .choice-section-title with text-transform uppercase

### Testing
- [ ] Build project: `cd src && dotnet build`
- [ ] Run application and navigate to tutorial scene
- [ ] Verify lock indicator shows: "Insight 3+ (have 1)" format
- [ ] Verify consequences show: "💰 +10 Coins (will have 18)" format
- [ ] Verify bond changes show: "💙 +1 Elena (have 3, will have 4)" format
- [ ] Test all 9 requirement types display correctly
- [ ] Test all consequence types display final values

---

## Expected Behavior After Implementation

**Locked Choice Display**:
```
🔒 LOCKED
Insight 3+ (have 1)
OR 15 Coins (have 8)
```

**Available Choice Consequences**:
```
COSTS:
🎯 2 Resolve
⏱️ 1 Segments

CONSEQUENCES:
💰 +10 Coins (will have 18)
🎯 -2 Resolve (will have 6)
💙 +1 Bond with Elena (have 3, will have 4)
⚖️ +2 Morality (have -1, will have 1)
```

---

## Architecture Principles Applied

✅ **Perfect Information**: Players see exact gaps and cascading final values
✅ **Single Source of Truth**: Player state for current values, rewards for deltas
✅ **Strong Typing**: No Dictionary lookups, all typed properties
✅ **Separation of Concerns**: Domain evaluation + Display formatting
✅ **Sir Brante Inspiration**: Compact, information-dense, strategic depth

---

## Notes for Next Session

1. **Bond System**: CheckBondStrength now sums all 4 token types (Trust + Diplomacy + Status + Shadow) for "overall relationship strength"

2. **OR Logic Display**: Multiple requirement gaps shown with " OR " between them. UI displays them vertically for clarity.

3. **Final Value Calculation**: Simple addition/subtraction (current ± delta). Handles costs and rewards in single calculation.

4. **CSS Aesthetic**: Match Sir Brante's compact, ALL CAPS style for section headers and lock indicators.

5. **Testing Priority**: Start with tutorial scene (Elena innkeeper) - tests PlayerStat, Coins, and BondStrength requirements.
