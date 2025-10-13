# Boolean Gate Elimination: Board Game Resource Model

**Status:** Planning
**Date:** 2025-01-XX
**Principle:** Every `if (hasX)` check that HIDES content is a boolean gate and must be eliminated.

---

## Table of Contents

1. [The Problem: Boolean Gates Everywhere](#the-problem-boolean-gates-everywhere)
2. [The Solution: Board Game Resource Model](#the-solution-board-game-resource-model)
3. [Core Architectural Principles](#core-architectural-principles)
4. [Current System Analysis](#current-system-analysis)
5. [Target Architecture](#target-architecture)
6. [Implementation Plan](#implementation-plan)
7. [Files Affected](#files-affected)
8. [Verification Checklist](#verification-checklist)

---

## The Problem: Boolean Gates Everywhere

### What is a Boolean Gate?

A boolean gate is any system that uses binary true/false checks to HIDE content from players.

**Pattern:**
```
if (hasX) then unlock_Y else hide_Y
```

**Examples in current codebase:**

1. **Knowledge System (Pure Boolean Gate)**
```csharp
// PlayerKnowledge.cs - List<string> of IDs
if (player.Knowledge.HasKnowledge("clue_mill_entrance"))
    goal.IsAvailable = true;  // Show goal
else
    goal.IsAvailable = false;  // HIDE goal ← BOOLEAN GATE
```

2. **GoalRequirements (Multiple Boolean Gates)**
```csharp
// GoalRequirementsChecker.cs:30-36
bool knowledgeMet = CheckKnowledgeRequirements(...);      // Boolean
bool equipmentMet = CheckEquipmentRequirements(...);      // Boolean
bool statsMet = CheckStatsRequirements(...);              // Boolean
bool familiarityMet = CheckFamiliarityRequirement(...);   // Boolean
bool completedGoalsMet = CheckCompletedGoalsRequirements(...); // Boolean

// If ANY false: Goal HIDDEN (locked/unlocked pattern)
return knowledgeMet && equipmentMet && statsMet && familiarityMet && completedGoalsMet;
```

3. **Investigation Prerequisites (Chained Boolean Gates)**
```csharp
// Investigation.cs - PhaseDefinition with Requirements
Phase1.Requirements = null;  // Always available
Phase2.Requirements = { CompletedGoals: ["phase_1"] };  // Hidden until Phase 1 done
Phase3.Requirements = { CompletedGoals: ["phase_2"] };  // Hidden until Phase 2 done
```

### Why Boolean Gates Are Bad

**Cookie Clicker Progression:**
- "You can't do X. Complete Y. Now you can do X."
- Linear path, no strategic choice
- Binary state (locked/unlocked)
- No opportunity cost
- Players feel railroaded

**False Strategic Depth:**
```
Phase 2 requires Phase 1 complete
Phase 3 requires Phase 2 complete

Strategy? There is none. Do Phase 1, then 2, then 3. One path only.
```

---

## The Solution: Board Game Resource Model

### Board Game Insight

**Bad design (Video Game Boolean Gates):**
"You can't do X. Complete Y. Now you can do X."

**Good design (Board Game Resources):**
"X costs 5 wood. Get wood from multiple sources. Spend it on X or Z. Choose wisely."

### The Difference

| Boolean Gates | Resource Competition |
|--------------|---------------------|
| Binary state (locked/unlocked) | Continuous resource (0→1→2→3→4→5) |
| Single path to unlock | Multiple paths to accumulate |
| No opportunity cost | Resource competition creates trade-offs |
| Hidden until unlocked | Always visible, difficulty varies |
| One correct sequence | Strategic choices emerge |

### Example Transformation

**BEFORE (Boolean Gates):**
```
Investigation "Miller's Daughter"

Phase 1: "Examine Mill Exterior"
- Always available

Phase 2: "Explore Mill Interior"
- Requires: Phase 1 complete ← BOOLEAN GATE
- Requires: Has item "lantern" ← BOOLEAN GATE
- Hidden until requirements met

Phase 3: "Decipher Mill Records"
- Requires: Phase 2 complete ← BOOLEAN GATE
- Requires: Has knowledge "mill_structure" ← BOOLEAN GATE
- Hidden until requirements met

Strategy: Do Phase 1 → Do Phase 2 → Do Phase 3 (only path)
```

**AFTER (Board Game Resources):**
```
Investigation "Miller's Daughter"

Phase 1: "Examine Mill Exterior"
- Costs: 3 Time, 10 Focus
- Base Difficulty: Exposure 3 (easy)
- Modifiers: None
- Rewards: Understanding +1, Familiarity(mill) +1

Phase 2: "Explore Mill Interior"
- Costs: 3 Time, 20 Focus
- Base Difficulty: Exposure 10 (very hard)
- Modifiers:
  * Understanding ≥ 2: -3 Exposure
  * Familiarity(mill) ≥ 2: -2 Exposure
  * HasItemCategory(Light_Source): -2 Exposure
- Rewards: Understanding +2, Familiarity(mill) +1

Phase 3: "Decipher Mill Records"
- Costs: 4 Time, 25 Focus
- Base Difficulty: Exposure 12 (extremely hard)
- Modifiers:
  * Understanding ≥ 4: -4 Exposure
  * Familiarity(mill) ≥ 3: -3 Exposure
  * HasItemCategory(Translation_Tool): -3 Exposure
- Rewards: Understanding +3, Investigation complete

ALL THREE PHASES ALWAYS VISIBLE

Strategy Options:
- Path A: Phase 1 → Phase 2 → Phase 3 (safe, slow)
  * Phase 1: Exposure 3 (easy), gain Understanding 1, Familiarity 1
  * Phase 2: Still Exposure 10 (very hard) - modifiers don't apply yet
  * Need to do Phase 1 TWICE or find lantern before Phase 2 is safe

- Path B: Phase 1 twice → Phase 2 → Phase 3 (safer, slower)
  * Phase 1 twice: Understanding 2, Familiarity 2
  * Phase 2: Now Exposure 5 (moderate) with modifiers
  * Phase 3: Still very hard without more Understanding

- Path C: Do other Mental challenges first → Phase 1 → Phase 2
  * Build Understanding from other investigations
  * Phase 2 becomes easier with high Understanding
  * Faster but requires exploring other locations

- Path D: Attempt Phase 2 early (risky gambit)
  * Pay 20 Focus, face Exposure 10 (very high failure chance)
  * If succeed: Jump ahead in progression
  * If fail: Wasted resources, no reward

MULTIPLE PATHS. RESOURCE COMPETITION. STRATEGIC CHOICES.
```

---

## Core Architectural Principles

### Principle 1: All Goals Always Visible

Goals are NEVER hidden based on player state.

```csharp
// ❌ WRONG
if (!player.HasKnowledge("clue"))
    return; // Hide goal

// ✅ CORRECT
// Goal always visible
// Difficulty varies based on player.Understanding
```

### Principle 2: No ID Matching (Mechanical Properties Only)

Check mechanical properties, NEVER specific IDs.

```csharp
// ❌ WRONG - String/ID Matching (Boolean Gate)
if (player.Inventory.Contains("lantern"))
    difficulty -= 2;

// ✅ CORRECT - Mechanical Property
List<ItemCategory> categories = player.GetEquipmentCategories();
if (categories.Contains(ItemCategory.Light_Source))
    difficulty -= 2;
// Any item with Light_Source category works: Lantern, Torch, Candle, etc.
```

**Already implemented correctly in RouteOption.cs:305-323:**
```csharp
private List<ItemCategory> GetPlayerEquipmentCategories(ItemRepository itemRepository, Player player)
{
    List<ItemCategory> categories = new List<ItemCategory>();
    foreach (string itemId in player.Inventory.GetAllItems())
    {
        Item item = itemRepository.GetItemById(itemId);
        if (item != null)
        {
            categories.AddRange(item.Categories);  // MECHANICAL PROPERTIES
        }
    }
    return categories.Distinct().ToList();
}
```

### Principle 3: Resources Over Tokens

Use continuous numerical resources, not discrete tokens/flags.

```csharp
// ❌ WRONG - Discrete Token (Boolean Gate)
player.Knowledge.Add("clue_mill_structure");  // String ID token
if (player.Knowledge.HasKnowledge("clue_mill_structure"))
    // Binary check

// ✅ CORRECT - Continuous Resource
player.Understanding += 2;  // 0-10 scale, cumulative
if (player.Understanding >= 2)
    difficulty -= 3;  // Graduated benefit
```

### Principle 4: Costs + Difficulty + Modifiers (Not Requirements)

Goals have transparent costs and difficulty, never hard requirements.

```csharp
// ❌ WRONG - Hard Requirements (Boolean Gates)
Goal {
    Requirements {
        RequiredKnowledge: ["clue"],
        RequiredEquipment: ["lantern"],
        MinimumFamiliarity: 2
    }
}
// Result: Goal HIDDEN if requirements not met

// ✅ CORRECT - Costs + Difficulty + Modifiers
Goal {
    Costs {
        Time: 3,
        Focus: 15
    },
    BaseDifficulty: 10,
    DifficultyModifiers: [
        { Type: Understanding, Threshold: 2, Effect: -3 },
        { Type: Familiarity, Context: "mill", Threshold: 2, Effect: -2 },
        { Type: HasItemCategory, Context: "Light_Source", Effect: -2 }
    ]
}
// Result: Goal ALWAYS VISIBLE
// Without modifiers: Difficulty 10 (very hard, but attemptable)
// With modifiers: Difficulty 3-10 based on player state (transparent)
```

### Principle 5: Multiple Paths to Success

Resources can be accumulated through multiple routes.

```
Understanding (Mental expertise):
- Path A: Complete Investigation Phase 1 multiple times (+1 each)
- Path B: Complete other Mental challenges at other locations (+1-3 each)
- Path C: Mix of both based on resource availability (Focus, Time)

Familiarity (Location knowledge):
- Path A: Repeat challenges at same location
- Path B: Spend time observing location (low Focus cost)
- Path C: Talk to NPCs about location (Social challenges)

Strategic choice emerges: Which path maximizes resource efficiency?
```

---

## Current System Analysis

### Systems to Eliminate

#### 1. Knowledge System (18+ files)

**Core Files:**
- `src/GameState/Knowledge.cs` - Entity definition
- `src/GameState/PlayerKnowledge.cs` - Player's knowledge collection
- `src/Content/Parsers/KnowledgeParser.cs` - JSON parsing
- `src/GameState/KnowledgeRequirement.cs` - Requirement checking
- Related DTOs and parsers

**Usage Pattern (Boolean Gate):**
```csharp
// Investigation grants knowledge token
player.Knowledge.AddKnowledge("clue_mill_structure");

// Goal checks for knowledge token (boolean gate)
if (!player.Knowledge.HasKnowledge("clue_mill_structure"))
    return false; // Goal hidden

// This is Cookie Clicker progression
```

**Replacement:**
```csharp
// Investigation grants Understanding resource
player.Understanding += 2;  // 0-10 scale, cumulative

// Goal uses Understanding as difficulty modifier
int difficulty = baseDifficulty;
if (player.Understanding >= 2)
    difficulty -= 3;  // Graduated benefit

// Goal always visible, difficulty varies
```

#### 2. GoalRequirements System (5+ files)

**Core Files:**
- `src/GameState/Goal.cs:162-169` - GoalRequirements class
- `src/Services/GoalRequirementsChecker.cs` - 247 lines of boolean checks
- Related parsing and DTO code

**Usage Pattern (Multiple Boolean Gates):**
```csharp
// Goal.cs
public class Goal {
    public GoalRequirements Requirements { get; set; }
}

public class GoalRequirements {
    public List<string> RequiredKnowledge { get; set; }      // Boolean gate
    public List<string> RequiredEquipment { get; set; }      // Boolean gate
    public List<StatRequirement> RequiredStats { get; set; } // Boolean gate
    public int MinimumLocationFamiliarity { get; set; }      // Boolean gate
    public List<string> CompletedGoals { get; set; }         // Boolean gate
}

// GoalRequirementsChecker.cs
public bool CheckGoalRequirements(Goal goal) {
    return knowledgeMet && equipmentMet && statsMet && familiarityMet && completedGoalsMet;
    // If false: Goal HIDDEN
}
```

**Replacement:**
```csharp
// Goal.cs
public class Goal {
    public GoalCosts Costs { get; set; }
    public int BaseDifficulty { get; set; }
    public List<DifficultyModifier> DifficultyModifiers { get; set; }
}

// DifficultyCalculationService.cs (NEW)
public DifficultyResult CalculateDifficulty(Goal goal) {
    int finalDifficulty = goal.BaseDifficulty;
    foreach (var mod in goal.DifficultyModifiers) {
        if (CheckModifier(mod))
            finalDifficulty += mod.Effect;  // Graduated reduction
    }
    return finalDifficulty;  // Goal always visible
}
```

#### 3. NPC Deck Systems (10+ files)

**Core Files:**
- `src/GameState/NPC.cs:71-72` - ObservationDeck, BurdenDeck properties
- Deck builder services
- Card entity definitions
- Related parsers and DTOs

**Usage Pattern:**
- ObservationDeck: Narrative cards unlocked via investigations
- BurdenDeck: Negative cards from failed obligations
- Complex deck management system

**Why Eliminate:**
- ObservationCards are just narrative flavor (not game mechanics)
- Can be replaced with simple narrative text on investigation completion
- BurdenCards create negative spiral (bad game feel)
- ExchangeDeck is KEPT (transactional system, not boolean gates)

**Replacement:**
```csharp
// Investigation completion shows narrative text directly
// No card collection mini-game
// Focus on core gameplay loop: Resource management and challenge resolution
```

### Systems to Keep (Already Correct)

#### 1. Familiarity System ✅

**Current Implementation:**
```csharp
// Player.cs:71
public List<FamiliarityEntry> LocationFamiliarity { get; set; }

// 0-3 scale per location, cumulative resource
public int GetLocationFamiliarity(string locationId) {
    return LocationFamiliarity.GetFamiliarity(locationId);
}
```

**Why It's Correct:**
- Continuous resource (0-3), not boolean flag
- Accumulated through repeated challenges
- Never depletes (permanent growth)

**Current Problem:**
```csharp
// GoalRequirementsChecker.cs:175-184
if (requirements.MinimumLocationFamiliarity <= 0)
    return true;
int currentFamiliarity = player.GetLocationFamiliarity(locationId);
return currentFamiliarity >= requirements.MinimumLocationFamiliarity;
// This is STILL a boolean gate!
```

**Fix:**
```csharp
// Remove from Requirements
// Add to DifficultyModifiers instead:
{ Type: Familiarity, Context: "mill", Threshold: 2, Effect: -2 }
// Goal visible at Familiarity 0, but harder
// Goal easier at Familiarity 2+
```

#### 2. Mastery System ✅

**Current Implementation:**
```csharp
// Player.cs:84
public List<MasteryTokenEntry> MasteryTokens { get; set; }

// Per-type Physical expertise (Combat, Athletics, etc.)
```

**Why It's Correct:**
- Continuous resource per challenge type
- Accumulated through repeated Physical challenges
- Reduces Danger baseline for that type

**Usage:**
```csharp
// Already follows resource model
// Just needs integration with DifficultyModifier system
{ Type: Mastery, Context: "Combat", Threshold: 2, Effect: -3 }
```

#### 3. Connection Tokens ✅

**Current Implementation:**
```csharp
// Player.cs:51
public List<NPCTokenEntry> NPCTokens { get; private set; }

// 0-15 scale per NPC, accumulated through Social challenges
```

**Why It's Correct:**
- Continuous resource per NPC
- Can increase and decrease (obligation failures)
- Creates relationship progression

**Usage:**
```csharp
// Already follows resource model
// Just needs integration with DifficultyModifier system
{ Type: ConnectionTokens, Context: "martha", Threshold: 5, Effect: -4 }
```

---

## Target Architecture

### New Resource: Understanding

**Definition:**
```csharp
// Player.cs (add near line 90, near Focus property)
/// <summary>
/// Mental resource - Understanding cumulative expertise (0-10 scale)
/// Granted by ALL Mental challenges (+1 to +3 based on difficulty)
/// Used by DifficultyModifiers to reduce Exposure baseline
/// Never depletes - permanent player growth
/// Competition: Multiple investigations need it, limited Focus/Time to accumulate
/// </summary>
public int Understanding { get; set; } = 0;
```

**Accumulation:**
```csharp
// After completing Mental challenge (victory):
switch (challenge.Difficulty) {
    case "Easy":     player.Understanding += 1; break;
    case "Moderate": player.Understanding += 2; break;
    case "Hard":     player.Understanding += 3; break;
}
// Max: 10
```

**Usage:**
```csharp
// DifficultyModifier in goal:
{ Type: Understanding, Threshold: 2, Effect: -3 }

// If player.Understanding >= 2:
//   Exposure reduced by 3
// Else:
//   No modifier applied, base difficulty stands
```

### New System: DifficultyModifier

**Entity Definition:**
```csharp
// src/GameState/DifficultyModifier.cs (NEW FILE)

/// <summary>
/// Difficulty modifier - reduces challenge difficulty based on player resources
/// No boolean gates: All goals always visible, difficulty varies
/// </summary>
public class DifficultyModifier
{
    public ModifierType Type { get; set; }
    public string Context { get; set; }    // Entity ID or property name (if needed)
    public int Threshold { get; set; }     // Minimum resource value needed
    public int Effect { get; set; }        // Difficulty change (usually negative)
}

/// <summary>
/// Types of difficulty modifiers
/// NO ID MATCHING: Only mechanical properties and numerical resources
/// </summary>
public enum ModifierType
{
    /// <summary>
    /// Global Mental expertise (0-10 scale)
    /// Context: null (global resource)
    /// Threshold: Minimum Understanding needed
    /// Effect: Exposure reduction
    /// </summary>
    Understanding,

    /// <summary>
    /// Physical expertise per challenge type (0-3 scale per type)
    /// Context: Challenge type ("Combat", "Athletics", etc.)
    /// Threshold: Minimum Mastery needed for that type
    /// Effect: Danger reduction
    /// </summary>
    Mastery,

    /// <summary>
    /// Location understanding (0-3 scale per location)
    /// Context: Location ID ("mill", "tavern", etc.)
    /// Threshold: Minimum Familiarity needed
    /// Effect: Exposure or Doubt reduction
    /// </summary>
    Familiarity,

    /// <summary>
    /// NPC relationship strength (0-15 scale per NPC)
    /// Context: NPC ID ("martha", "elena", etc.)
    /// Threshold: Minimum Connection Tokens needed
    /// Effect: Doubt rate reduction
    /// </summary>
    ConnectionTokens,

    /// <summary>
    /// Obstacle property threshold check
    /// Context: Property name ("PhysicalDanger", "MentalComplexity", etc.)
    /// Threshold: Maximum property value (inverted logic: lower is better)
    /// Effect: Difficulty change (can be positive or negative)
    /// Example: If PhysicalDanger <= 5, reduce Danger by 3
    /// </summary>
    ObstacleProperty,

    /// <summary>
    /// Equipment category presence (MECHANICAL PROPERTY, NOT ID)
    /// Context: ItemCategory enum value ("Light_Source", "Navigation_Tools", etc.)
    /// Threshold: Not used (presence check only)
    /// Effect: Difficulty reduction
    /// Checks if player has ANY item with this category
    /// Example: HasItemCategory(Light_Source) matches: Lantern, Torch, Candle
    /// NEVER matches specific item IDs
    /// </summary>
    HasItemCategory
}
```

**Usage Example:**
```json
{
  "id": "goal_explore_mill_interior",
  "name": "Explore Mill Interior",
  "costs": {
    "time": 3,
    "focus": 20
  },
  "base_difficulty": 10,
  "difficulty_modifiers": [
    {
      "type": "Understanding",
      "threshold": 2,
      "effect": -3
    },
    {
      "type": "Familiarity",
      "context": "mill",
      "threshold": 2,
      "effect": -2
    },
    {
      "type": "HasItemCategory",
      "context": "Light_Source",
      "effect": -2
    }
  ]
}
```

**Difficulty Calculation:**
```csharp
// Base: 10
// Player has: Understanding 3, Familiarity(mill) 2, Lantern (has Light_Source category)
// Modifiers apply:
//   Understanding >= 2: -3 → difficulty 7
//   Familiarity(mill) >= 2: -2 → difficulty 5
//   HasItemCategory(Light_Source): -2 → difficulty 3
// Final: 3 (easy)

// Same goal, different player state:
// Player has: Understanding 0, Familiarity(mill) 0, no Light_Source item
// No modifiers apply
// Final: 10 (very hard, but STILL VISIBLE AND ATTEMPTABLE)
```

### New System: GoalCosts

**Entity Definition:**
```csharp
// src/GameState/GoalCosts.cs (NEW FILE)

/// <summary>
/// Resources player must pay to attempt goal
/// Transparent costs, no hidden fees
/// Resource competition creates strategic choices
/// </summary>
public class GoalCosts
{
    /// <summary>
    /// Time segments consumed by this goal
    /// Competition: Limited time until deadline
    /// </summary>
    public int Time { get; set; } = 0;

    /// <summary>
    /// Focus consumed by Mental challenges
    /// Competition: Limited Focus pool (0-100), shared by all Mental goals
    /// </summary>
    public int Focus { get; set; } = 0;

    /// <summary>
    /// Stamina consumed by Physical challenges
    /// Competition: Limited Stamina pool (0-100), shared by all Physical goals
    /// </summary>
    public int Stamina { get; set; } = 0;

    /// <summary>
    /// Coins spent on this goal (rare)
    /// Competition: Limited coins, needed for items, travel, etc.
    /// </summary>
    public int Coins { get; set; } = 0;
}
```

**Usage:**
```csharp
// Before attempting goal:
if (player.Focus < goal.Costs.Focus) {
    // Show "Insufficient Focus" message
    // Goal still visible, just can't afford it right now
    return;
}

// Attempt goal:
player.Focus -= goal.Costs.Focus;
// Start challenge...

// Resource competition:
// Player has 50 Focus
// Goal A costs 20 Focus, Goal B costs 30 Focus
// Can do A + B, or A twice, or B + smaller goal, etc.
// Strategic choice emerges
```

### New Service: DifficultyCalculationService

**Service Definition:**
```csharp
// src/Services/DifficultyCalculationService.cs (NEW FILE)

/// <summary>
/// Calculate final difficulty based on base difficulty and player state
/// Replaces GoalRequirementsChecker (which was boolean gate system)
/// </summary>
public class DifficultyCalculationService
{
    private readonly GameWorld _gameWorld;

    public DifficultyCalculationService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Calculate final difficulty for a goal
    /// Returns base difficulty plus/minus modifiers
    /// Goal ALWAYS visible regardless of difficulty
    /// </summary>
    public DifficultyResult CalculateDifficulty(Goal goal)
    {
        Player player = _gameWorld.GetPlayer();
        int finalDifficulty = goal.BaseDifficulty;
        List<string> appliedModifiers = new List<string>();

        foreach (DifficultyModifier mod in goal.DifficultyModifiers)
        {
            if (CheckModifier(mod, player, goal))
            {
                finalDifficulty += mod.Effect;  // Usually negative (reduction)
                appliedModifiers.Add(FormatModifier(mod));
            }
        }

        return new DifficultyResult
        {
            BaseDifficulty = goal.BaseDifficulty,
            FinalDifficulty = Math.Max(0, finalDifficulty),
            AppliedModifiers = appliedModifiers
        };
    }

    /// <summary>
    /// Check if modifier threshold is met
    /// </summary>
    private bool CheckModifier(DifficultyModifier mod, Player player, Goal goal)
    {
        switch (mod.Type)
        {
            case ModifierType.Understanding:
                return player.Understanding >= mod.Threshold;

            case ModifierType.Mastery:
                // Check Mastery for specific type
                int mastery = player.MasteryTokens.GetMastery(mod.Context);
                return mastery >= mod.Threshold;

            case ModifierType.Familiarity:
                // Check Familiarity for specific location
                int familiarity = player.GetLocationFamiliarity(mod.Context);
                return familiarity >= mod.Threshold;

            case ModifierType.ConnectionTokens:
                // Check Connection Tokens for specific NPC
                int tokens = player.NPCTokens.GetTokenCount(mod.Context, ConnectionType.Trust);
                return tokens >= mod.Threshold;

            case ModifierType.ObstacleProperty:
                // Check obstacle property threshold
                // Find parent obstacle for this goal
                Obstacle obstacle = FindParentObstacle(goal);
                if (obstacle == null) return false;

                int propertyValue = obstacle.GetPropertyValue(mod.Context);
                return propertyValue <= mod.Threshold;  // Inverted: lower is better

            case ModifierType.HasItemCategory:
                // Check if player has ANY item with this category
                List<ItemCategory> categories = GetPlayerEquipmentCategories(player);
                ItemCategory targetCategory = Enum.Parse<ItemCategory>(mod.Context);
                return categories.Contains(targetCategory);

            default:
                return false;
        }
    }

    /// <summary>
    /// Get all equipment categories from player inventory
    /// Same pattern as RouteOption.cs:305-323
    /// </summary>
    private List<ItemCategory> GetPlayerEquipmentCategories(Player player)
    {
        List<ItemCategory> categories = new List<ItemCategory>();
        foreach (string itemId in player.Inventory.GetAllItems())
        {
            Item item = _gameWorld.ItemRepository.GetItemById(itemId);
            if (item != null)
            {
                categories.AddRange(item.Categories);  // MECHANICAL PROPERTIES
            }
        }
        return categories.Distinct().ToList();
    }

    private string FormatModifier(DifficultyModifier mod)
    {
        return $"{mod.Type} ≥ {mod.Threshold}: {mod.Effect:+#;-#;0}";
    }
}

/// <summary>
/// Result of difficulty calculation (for UI display)
/// </summary>
public class DifficultyResult
{
    public int BaseDifficulty { get; set; }
    public int FinalDifficulty { get; set; }
    public List<string> AppliedModifiers { get; set; }
}
```

---

## Implementation Plan

### Phase 1: Create New Systems (Non-Breaking) ✅

**Goal:** Add new systems alongside old systems (parallel implementation)

#### 1.1 Create DifficultyModifier System
**New file:** `src/GameState/DifficultyModifier.cs`
- Define ModifierType enum (Understanding, Mastery, Familiarity, ConnectionTokens, ObstacleProperty, HasItemCategory)
- Define DifficultyModifier class
- See "Target Architecture" section above for complete code

#### 1.2 Create GoalCosts System
**New file:** `src/GameState/GoalCosts.cs`
- Define GoalCosts class (Time, Focus, Stamina, Coins)
- See "Target Architecture" section above for complete code

#### 1.3 Add Understanding Resource
**Modify:** `src/GameState/Player.cs`
- Add property after line 89 (near Focus property)
```csharp
// Mental resource - Understanding cumulative expertise (0-10 scale)
// Granted by ALL Mental challenges (+1 to +3 based on difficulty)
// Used by DifficultyModifiers to reduce Exposure baseline
// Never depletes - permanent player growth
public int Understanding { get; set; } = 0;
```

#### 1.4 Extend Goal Entity
**Modify:** `src/GameState/Goal.cs`
- Add after line 93 (before Requirements property)
```csharp
/// <summary>
/// Resources player must pay to attempt this goal
/// Transparent costs create resource competition
/// </summary>
public GoalCosts Costs { get; set; } = new GoalCosts();

/// <summary>
/// Base difficulty before any modifiers
/// Exposure for Mental, Danger for Physical, Doubt rate for Social
/// </summary>
public int BaseDifficulty { get; set; } = 0;

/// <summary>
/// Difficulty modifiers that reduce/increase difficulty based on player state
/// Goal ALWAYS visible, difficulty varies transparently
/// </summary>
public List<DifficultyModifier> DifficultyModifiers { get; set; } = new List<DifficultyModifier>();
```
- KEEP `public GoalRequirements Requirements` temporarily (parallel systems during migration)

#### 1.5 Create DifficultyCalculationService
**New file:** `src/Services/DifficultyCalculationService.cs`
- Implement full service (see "Target Architecture" section above)
- Create DifficultyResult class

#### 1.6 Create DTOs for New Systems
**New files:**
- `src/Models/GoalCostsDTO.cs`
- `src/Models/DifficultyModifierDTO.cs`

### Phase 2: Eliminate Knowledge System (SCORCHED EARTH) 🔥

**Goal:** Complete elimination of Knowledge entities and PlayerKnowledge

#### 2.1 Delete Knowledge Entities

**DELETE FILES:**
```
src/GameState/Knowledge.cs
src/GameState/PlayerKnowledge.cs
src/Content/Parsers/KnowledgeParser.cs (if exists)
src/GameState/KnowledgeRequirement.cs (if exists)
[Any Knowledge-related DTOs found via grep]
```

**Verification:**
```bash
grep -r "class Knowledge" --include="*.cs"
# Expected: Zero results
```

#### 2.2 Remove PlayerKnowledge from Player

**Modify:** `src/GameState/Player.cs:43`

**DELETE:**
```csharp
public PlayerKnowledge Knowledge { get; private set; } = new PlayerKnowledge();
```

**Compilation errors expected:**
- All `player.Knowledge.AddKnowledge()` calls will break
- All `player.Knowledge.HasKnowledge()` calls will break
- This is GOOD - we want compilation to force us to find all usages

#### 2.3 Remove Knowledge from Investigation Rewards

**Modify:** `src/GameState/Investigation.cs:65`

**PhaseCompletionReward class:**

**DELETE:**
```csharp
public List<string> KnowledgeGranted { get; set; } = new List<string>();
```

**ADD:**
```csharp
/// <summary>
/// Understanding points granted on phase completion (1-3)
/// Cumulative resource (0-10 max), reduces Exposure on Mental challenges
/// </summary>
public int UnderstandingReward { get; set; } = 0;
```

#### 2.4 Remove Knowledge from Investigation Prerequisites

**Modify:** `src/GameState/Investigation.cs:143`

**InvestigationPrerequisites class:**

**DELETE:**
```csharp
/// <summary>
/// Required knowledge IDs (ConversationalDiscovery)
/// DEPRECATED: Use GoalCompletionTrigger instead - knowledge tokens are fragile
/// </summary>
public List<string> RequiredKnowledge { get; set; } = new List<string>();
```

(Comment already says DEPRECATED!)

#### 2.5 Update InvestigationActivity

**Modify:** `src/Services/InvestigationActivity.cs`

**Find all occurrences of:**
```csharp
player.Knowledge.AddKnowledge(knowledgeId);
```

**Replace with:**
```csharp
player.Understanding += phaseReward.UnderstandingReward;
// Ensure Understanding doesn't exceed 10
player.Understanding = Math.Min(10, player.Understanding);
```

**Search pattern:**
```bash
grep -n "Knowledge.Add" src/Services/InvestigationActivity.cs
grep -n "Knowledge.Has" src/Services/InvestigationActivity.cs
```

#### 2.6 Grep and Destroy All Knowledge References

**Verification:**
```bash
# Find all remaining Knowledge references
grep -r "Knowledge" --include="*.cs" | grep -v "// " | grep -v "///

"

# Expected results:
# - Zero results for PlayerKnowledge
# - Zero results for KnowledgeGranted
# - Zero results for RequiredKnowledge
# - Zero results for HasKnowledge/AddKnowledge method calls

# If any found: Investigate and eliminate
```

### Phase 3: Eliminate NPC Deck Systems 🔥

**Goal:** Remove ObservationDeck and BurdenDeck (keep ExchangeDeck)

#### 3.1 Remove Decks from NPC

**Modify:** `src/GameState/NPC.cs:71-72`

**DELETE:**
```csharp
// NPC DECK ARCHITECTURE
public List<ObservationCard> ObservationDeck { get; internal set; }
public List<BurdenCard> BurdenDeck { get; internal set; }
```

**KEEP:**
```csharp
public List<ExchangeCard> ExchangeDeck { get; set; } = new();  // Transactional system OK
```

**Compilation errors expected:**
- All deck initialization code will break
- All deck access code will break
- This is GOOD

#### 3.2 Remove from Investigation

**Modify:** `src/GameState/Investigation.cs:38`

**DELETE:**
```csharp
/// <summary>
/// Observation cards unlocked on completion
/// </summary>
public List<ObservationCardReward> ObservationCardRewards { get; set; } = new List<ObservationCardReward>();
```

**Replace with:**
- Simple narrative text shown on completion (already exists: CompletionNarrative)
- Or additional Understanding reward
- No card collection needed

#### 3.3 Delete Card Entities

**Search for card definitions:**
```bash
grep -r "class ObservationCard" --include="*.cs"
grep -r "class BurdenCard" --include="*.cs"
```

**DELETE FILES:**
- Files containing ObservationCard class
- Files containing BurdenCard class
- Related deck builder services (search for "DeckBuilder")
- Related DTOs and parsers

#### 3.4 Remove from GameWorld

**Modify:** `src/GameState/GameWorld.cs`

**Search for:**
```bash
grep -n "ObservationCard\|BurdenCard" src/GameState/GameWorld.cs
```

**DELETE:**
- Any ObservationCard collections
- Any BurdenCard collections

**KEEP:**
- ExchangeCard infrastructure (transactional system)

#### 3.5 Verification

```bash
# Should return ZERO results:
grep -r "ObservationCard" --include="*.cs"
grep -r "BurdenCard" --include="*.cs"

# ExchangeCard should still exist:
grep -r "ExchangeCard" --include="*.cs"
# Expected: Multiple results (this is correct)
```

### Phase 4: Eliminate GoalRequirements (SCORCHED EARTH) 🔥

**Goal:** Replace Requirements with Costs + Difficulty + Modifiers

#### 4.1 Delete GoalRequirements Class

**Modify:** `src/GameState/Goal.cs:162-169`

**DELETE ENTIRE CLASS:**
```csharp
/// <summary>
/// Requirements for a goal to be available
/// </summary>
public class GoalRequirements
{
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public List<string> RequiredEquipment { get; set; } = new List<string>();
    public List<StatRequirement> RequiredStats { get; set; } = new List<StatRequirement>();
    public int MinimumLocationFamiliarity { get; set; } = 0;
    public List<string> CompletedGoals { get; set; } = new List<string>();
}
```

**DELETE FROM Goal CLASS (line ~94):**
```csharp
/// <summary>
/// Prerequisites for this goal to be available
/// </summary>
public GoalRequirements Requirements { get; set; }
```

**Compilation errors expected:**
- Massive compilation errors throughout codebase
- Every Requirements access will break
- This is GOOD - forces us to find all usages

#### 4.2 Delete GoalRequirementsChecker

**DELETE FILE:**
```
src/Services/GoalRequirementsChecker.cs
```

**File size:** 247 lines

**Replacement:** DifficultyCalculationService (already created in Phase 1)

#### 4.3 Update ObstacleGoalFilter

**Modify:** `src/Services/ObstacleGoalFilter.cs`

**Lines to change: 60, 115, 182**

**BEFORE:**
```csharp
// Line 60
bool accessRequirementsMet = _requirementsChecker.CheckGoalRequirements(goal);

if (propertyRequirementsMet && accessRequirementsMet)
{
    visibleGoals.Add(goal);
}
```

**AFTER:**
```csharp
// ALL GOALS ALWAYS VISIBLE (if property requirements met)
// Difficulty calculated by DifficultyCalculationService
// UI shows difficulty, not locked/unlocked

if (propertyRequirementsMet)
{
    visibleGoals.Add(goal);
}
```

**Repeat for lines 115 and 182 (same pattern)**

**Remove field:**
```csharp
// Line 11 - DELETE:
private readonly GoalRequirementsChecker _requirementsChecker;

// Constructor line 17 - DELETE:
_requirementsChecker = new GoalRequirementsChecker(gameWorld);
```

#### 4.4 Update Investigation PhaseDefinitions

**Modify:** `src/GameState/Investigation.cs:54`

**DELETE:**
```csharp
// Prerequisites for this phase to complete
public GoalRequirements Requirements { get; set; } = new GoalRequirements();
```

**Phases no longer have requirements**
- All phases always visible
- Difficulty varies based on player Understanding/Familiarity
- Strategic choice: Attempt hard phase early (risky) or build resources first (safe)

#### 4.5 Verification

```bash
# Should return ZERO results:
grep -r "GoalRequirements" --include="*.cs"
grep -r "RequiredKnowledge" --include="*.cs"
grep -r "RequiredEquipment" --include="*.cs"
grep -r "MinimumLocationFamiliarity" --include="*.cs"
grep -r "CheckGoalRequirements" --include="*.cs"
```

### Phase 5: Update All Parsers 🔧

**Goal:** Parse new Costs/Difficulty/Modifiers, remove old Requirements parsing

#### 5.1 Update GoalParser

**Modify:** `src/Content/Parsers/GoalParser.cs`

**Find Requirements parsing section, DELETE:**
```csharp
// Parse Requirements (OLD SYSTEM - DELETE ALL)
if (dto.Requirements != null)
{
    goal.Requirements = new GoalRequirements
    {
        RequiredKnowledge = dto.Requirements.RequiredKnowledge ?? new List<string>(),
        RequiredEquipment = dto.Requirements.RequiredEquipment ?? new List<string>(),
        RequiredStats = ...,
        MinimumLocationFamiliarity = ...,
        CompletedGoals = ...
    };
}
```

**ADD (NEW SYSTEM):**
```csharp
// Parse Costs
if (dto.Costs != null)
{
    goal.Costs = new GoalCosts
    {
        Time = dto.Costs.Time,
        Focus = dto.Costs.Focus,
        Stamina = dto.Costs.Stamina,
        Coins = dto.Costs.Coins
    };
}

// Parse BaseDifficulty
goal.BaseDifficulty = dto.BaseDifficulty;

// Parse DifficultyModifiers
if (dto.DifficultyModifiers != null)
{
    foreach (var modDto in dto.DifficultyModifiers)
    {
        goal.DifficultyModifiers.Add(new DifficultyModifier
        {
            Type = Enum.Parse<ModifierType>(modDto.Type),
            Context = modDto.Context,
            Threshold = modDto.Threshold,
            Effect = modDto.Effect
        });
    }
}
```

#### 5.2 Update InvestigationParser

**Modify investigation parsing files**

**DELETE:**
```csharp
// Parse KnowledgeGranted (OLD)
phaseReward.KnowledgeGranted = dto.KnowledgeGranted ?? new List<string>();

// Parse RequiredKnowledge prerequisite (OLD)
prerequisites.RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>();
```

**ADD:**
```csharp
// Parse UnderstandingReward (NEW)
phaseReward.UnderstandingReward = dto.UnderstandingReward;
```

#### 5.3 Create DTOs for New Systems

**New file:** `src/Models/GoalCostsDTO.cs`
```csharp
public class GoalCostsDTO
{
    public int Time { get; set; }
    public int Focus { get; set; }
    public int Stamina { get; set; }
    public int Coins { get; set; }
}
```

**New file:** `src/Models/DifficultyModifierDTO.cs`
```csharp
public class DifficultyModifierDTO
{
    public string Type { get; set; }        // Enum as string for JSON
    public string Context { get; set; }
    public int Threshold { get; set; }
    public int Effect { get; set; }
}
```

**Modify:** `src/Models/GoalDTO.cs`
```csharp
// ADD:
public GoalCostsDTO Costs { get; set; }
public int BaseDifficulty { get; set; }
public List<DifficultyModifierDTO> DifficultyModifiers { get; set; }

// KEEP temporarily (for migration):
public GoalRequirementsDTO Requirements { get; set; }
```

### Phase 6: Update All JSON Content 📝

**Goal:** Convert all content to new structure

#### 6.1 Goal JSON Transformation Pattern

**BEFORE (Boolean Gates):**
```json
{
  "id": "goal_explore_mill_interior",
  "name": "Explore Mill Interior",
  "requirements": {
    "required_knowledge": ["clue_mill_entrance"],
    "required_equipment": ["lantern"],
    "minimum_location_familiarity": 2
  }
}
```

**AFTER (Resource Modifiers):**
```json
{
  "id": "goal_explore_mill_interior",
  "name": "Explore Mill Interior",
  "costs": {
    "time": 3,
    "focus": 20
  },
  "base_difficulty": 10,
  "difficulty_modifiers": [
    {
      "type": "Understanding",
      "threshold": 2,
      "effect": -3
    },
    {
      "type": "Familiarity",
      "context": "mill",
      "threshold": 2,
      "effect": -2
    },
    {
      "type": "HasItemCategory",
      "context": "Light_Source",
      "effect": -2
    }
  ]
}
```

**Key Changes:**
- `requirements` → deleted
- `costs` → added (time, focus, stamina, coins)
- `base_difficulty` → added (numerical baseline)
- `difficulty_modifiers` → added (array of graduated reductions)
- `required_knowledge` → becomes `Understanding` threshold modifier
- `required_equipment` → becomes `HasItemCategory` modifier (MECHANICAL PROPERTY)
- `minimum_location_familiarity` → becomes `Familiarity` threshold modifier

#### 6.2 Investigation JSON Transformation Pattern

**BEFORE:**
```json
{
  "phase_definitions": [
    {
      "id": "phase_1",
      "completion_reward": {
        "knowledge_granted": ["clue_mill_structure"]
      }
    },
    {
      "id": "phase_2",
      "requirements": {
        "required_knowledge": ["clue_mill_structure"],
        "completed_goals": ["phase_1"]
      }
    }
  ]
}
```

**AFTER:**
```json
{
  "phase_definitions": [
    {
      "id": "phase_1",
      "completion_reward": {
        "understanding_reward": 2
      }
    },
    {
      "id": "phase_2",
      "costs": {
        "time": 3,
        "focus": 20
      },
      "base_difficulty": 10,
      "difficulty_modifiers": [
        {
          "type": "Understanding",
          "threshold": 2,
          "effect": -3
        }
      ]
    }
  ]
}
```

**Key Changes:**
- `knowledge_granted` → `understanding_reward` (1-3 points)
- `requirements.required_knowledge` → deleted
- `requirements.completed_goals` → deleted (no prerequisites)
- Phase 2 always visible, difficulty high without Understanding
- Player chooses: Attempt Phase 2 early (risky) or build Understanding first (safe)

#### 6.3 Files to Update

**Core Content:**
```
src/Content/Core/01_locations.json
src/Content/Core/02_npcs.json
src/Content/Core/03_cards.json
src/Content/Core/investigations/*.json (all investigation files)
```

**Package Content:**
```
src/Content/[PackageName]/*.json (all package files with goals/investigations)
```

**Documentation:**
```
docs/elena-poc-v2-millers-daughter.md
docs/obstacle-system-design.md
```

#### 6.4 Conversion Script (Optional)

Create temporary conversion script to help migrate JSON:

```csharp
// Pseudocode for JSON conversion
foreach (goal in allGoals)
{
    // Convert requirements to modifiers
    if (goal.requirements.required_knowledge.Any())
    {
        goal.difficulty_modifiers.Add({
            type: "Understanding",
            threshold: 2,  // Assume knowledge = Understanding 2
            effect: -3
        });
    }

    if (goal.requirements.required_equipment.Any())
    {
        foreach (item in requirements.required_equipment)
        {
            // Map item ID to ItemCategory
            ItemCategory category = MapItemToCategory(item);
            goal.difficulty_modifiers.Add({
                type: "HasItemCategory",
                context: category.ToString(),
                effect: -2
            });
        }
    }

    if (goal.requirements.minimum_location_familiarity > 0)
    {
        goal.difficulty_modifiers.Add({
            type: "Familiarity",
            context: goal.placement_location_id,
            threshold: goal.requirements.minimum_location_familiarity,
            effect: -2
        });
    }

    // Remove requirements
    goal.requirements = null;
}
```

### Phase 7: Build and Verification 🔨

**Goal:** Zero compilation errors, zero boolean gates

#### 7.1 Incremental Compilation

**After each phase, compile:**
```bash
cd src
dotnet build
```

**Expected progression:**
- Phase 1 complete: Clean build (new systems added, old systems still work)
- Phase 2 complete: Compilation errors (Knowledge references break)
- After fixing Phase 2 errors: Clean build
- Phase 3 complete: Compilation errors (Deck references break)
- After fixing Phase 3 errors: Clean build
- Phase 4 complete: MASSIVE compilation errors (Requirements references break)
- After fixing Phase 4 errors: Clean build
- Phases 5-6: Clean build (JSON changes, no code changes)

#### 7.2 Grep Verification (Zero Tolerance)

**After Phase 7 complete, ALL of these MUST return ZERO results:**

```bash
# Knowledge system eliminated:
grep -r "RequiredKnowledge" --include="*.cs"
grep -r "PlayerKnowledge" --include="*.cs"
grep -r "class Knowledge" --include="*.cs"
grep -r "HasKnowledge" --include="*.cs"
grep -r "AddKnowledge" --include="*.cs"

# Requirements system eliminated:
grep -r "GoalRequirements" --include="*.cs"
grep -r "GoalRequirementsChecker" --include="*.cs"
grep -r "CheckGoalRequirements" --include="*.cs"
grep -r "RequiredEquipment" --include="*.cs"
grep -r "RequiredStats" --include="*.cs"
grep -r "MinimumLocationFamiliarity" --include="*.cs"

# NPC deck systems eliminated:
grep -r "ObservationDeck" --include="*.cs"
grep -r "BurdenDeck" --include="*.cs"
grep -r "ObservationCard" --include="*.cs"
grep -r "BurdenCard" --include="*.cs"

# Boolean gate patterns eliminated:
grep -r "if.*HasKnowledge" --include="*.cs"
grep -r "if.*RequiredKnowledge" --include="*.cs"
grep -r "IsAvailable.*=.*false" --include="*.cs"  # Check for goal hiding
```

**Expected:** ZERO results for all searches (except ExchangeCard, which is correct)

#### 7.3 Architecture Compliance Checklist

**Manual verification:**

- [ ] All goals always visible in UI (no hidden goals based on player state)
- [ ] No boolean gates (`if (hasX) then unlock_Y` patterns eliminated)
- [ ] Goals have transparent costs (Time/Focus/Stamina/Coins shown in UI)
- [ ] Goals have transparent difficulty (BaseDifficulty + Modifiers shown in UI)
- [ ] Difficulty varies based on player resources (Understanding, Familiarity, etc.)
- [ ] Multiple paths to reduce difficulty exist for each goal
- [ ] Resource competition creates strategic choices (limited Focus/Time/Stamina)
- [ ] Understanding resource exists (0-10 scale)
- [ ] Understanding granted by Mental challenges (+1 to +3)
- [ ] Understanding reduces Exposure via DifficultyModifiers
- [ ] No ID matching for items (only ItemCategory mechanical properties)
- [ ] HasItemCategory checks category, not specific item IDs
- [ ] Zero compilation errors
- [ ] Zero warnings related to deleted systems
- [ ] All JSON content updated to new structure

#### 7.4 Playtesting Verification

**Test scenarios:**

1. **Investigation Progression Without Boolean Gates**
   - Start "Miller's Daughter" investigation
   - Verify Phase 2 visible immediately (not hidden)
   - Verify Phase 2 shows high difficulty (Exposure 10)
   - Complete Phase 1, gain Understanding +1
   - Verify Phase 2 difficulty still high (modifiers don't apply yet)
   - Complete Phase 1 again or other Mental challenges
   - Verify Phase 2 difficulty reduces when Understanding ≥ 2
   - Attempt Phase 2 before building Understanding
   - Verify challenge is very hard but completable (no blocking)

2. **Resource Competition**
   - Player has 50 Focus
   - Two Mental goals available: Goal A (20 Focus), Goal B (30 Focus)
   - Verify both goals visible
   - Verify can attempt either goal
   - Attempt Goal A, lose 20 Focus (30 remaining)
   - Verify Goal B still visible but now unaffordable
   - Verify UI shows "Insufficient Focus" message (not "Locked")
   - Rest to recover Focus
   - Verify strategic choice exists: A+A, or A+B, or B alone

3. **Equipment Mechanical Properties**
   - Goal has modifier: HasItemCategory(Light_Source): -2 Exposure
   - Player has no Light_Source items
   - Verify goal visible, difficulty 10
   - Player acquires "Lantern" (has Light_Source category)
   - Verify goal difficulty reduces to 8
   - Player acquires "Torch" (also has Light_Source category)
   - Verify goal difficulty still 8 (multiple items don't stack)
   - Player loses Lantern but keeps Torch
   - Verify goal difficulty still 8 (any Light_Source item works)

4. **Understanding Accumulation**
   - Player Understanding = 0
   - Complete easy Mental challenge
   - Verify Understanding increases to 1
   - Complete moderate Mental challenge
   - Verify Understanding increases to 3
   - Complete hard Mental challenge
   - Verify Understanding increases to 6
   - Verify goals with Understanding modifiers show reduced difficulty
   - Verify Understanding never decreases (permanent growth)
   - Verify Understanding caps at 10

5. **No Hidden Goals**
   - Visit any location
   - Verify ALL goals for obstacles at location are visible
   - Verify no goals hidden based on Knowledge/Requirements
   - Verify difficulty shown for each goal
   - Verify costs shown for each goal
   - Verify modifiers shown (which apply, which don't)

---

## Files Affected

### Files to DELETE (~20 files)

**Knowledge System (~8 files):**
```
src/GameState/Knowledge.cs
src/GameState/PlayerKnowledge.cs
src/Content/Parsers/KnowledgeParser.cs (if exists)
src/GameState/KnowledgeRequirement.cs (if exists)
src/Models/KnowledgeDTO.cs (if exists)
[Additional Knowledge-related DTOs found via grep]
```

**Requirements System (~5 files):**
```
src/Services/GoalRequirementsChecker.cs (247 lines)
src/Models/GoalRequirementsDTO.cs (if exists)
[Additional Requirements-related files found via grep]
```

**NPC Deck Systems (~7 files):**
```
[Files containing ObservationCard class definition]
[Files containing BurdenCard class definition]
[Deck builder services for these systems]
[Related DTOs and parsers]
```

### Files to CREATE (~5 files)

**New Systems:**
```
src/GameState/DifficultyModifier.cs (NEW - ~100 lines)
src/GameState/GoalCosts.cs (NEW - ~30 lines)
src/Services/DifficultyCalculationService.cs (NEW - ~150 lines)
src/Models/GoalCostsDTO.cs (NEW - ~20 lines)
src/Models/DifficultyModifierDTO.cs (NEW - ~20 lines)
```

### Files to MODIFY (~30 files)

**Core Game State:**
```
src/GameState/Player.cs
  - Line 43: DELETE PlayerKnowledge property
  - Line ~90: ADD Understanding property

src/GameState/Goal.cs
  - Line ~94: ADD Costs, BaseDifficulty, DifficultyModifiers properties
  - Line ~94: DELETE Requirements property (later phase)
  - Line 162-169: DELETE GoalRequirements class (later phase)

src/GameState/NPC.cs
  - Line 71-72: DELETE ObservationDeck, BurdenDeck properties
  - KEEP ExchangeDeck property

src/GameState/Investigation.cs
  - Line 38: DELETE ObservationCardRewards property
  - Line 54: DELETE Requirements property from PhaseDefinition
  - Line 65: DELETE KnowledgeGranted, ADD UnderstandingReward
  - Line 143: DELETE RequiredKnowledge from InvestigationPrerequisites

src/GameState/GameWorld.cs
  - Remove ObservationCard/BurdenCard collections (if any)
```

**Services:**
```
src/Services/ObstacleGoalFilter.cs
  - Line 11, 17: DELETE _requirementsChecker field and initialization
  - Line 60, 115, 182: DELETE accessRequirementsMet checks
  - Goals always visible (if property requirements met)

src/Services/InvestigationActivity.cs
  - Replace all player.Knowledge.AddKnowledge() with player.Understanding +=
  - Update phase reward handling
```

**Parsers:**
```
src/Content/Parsers/GoalParser.cs
  - DELETE Requirements parsing section
  - ADD Costs parsing
  - ADD BaseDifficulty parsing
  - ADD DifficultyModifiers parsing

src/Content/Parsers/InvestigationParser.cs
  - DELETE KnowledgeGranted parsing
  - DELETE RequiredKnowledge parsing
  - ADD UnderstandingReward parsing
```

**DTOs:**
```
src/Models/GoalDTO.cs
  - ADD Costs, BaseDifficulty, DifficultyModifiers properties
  - DELETE Requirements property (later phase)

src/Models/InvestigationDTO.cs
  - DELETE KnowledgeGranted property
  - ADD UnderstandingReward property
```

**JSON Content (~15+ files):**
```
src/Content/Core/investigations/*.json (all investigation files)
src/Content/[Packages]/*.json (all package files with goals)
docs/elena-poc-v2-millers-daughter.md
docs/obstacle-system-design.md
```

---

## Verification Checklist

### Code Verification ✅

- [ ] Compilation: `dotnet build` returns zero errors
- [ ] Grep: `grep -r "RequiredKnowledge" --include="*.cs"` returns zero results
- [ ] Grep: `grep -r "GoalRequirements" --include="*.cs"` returns zero results
- [ ] Grep: `grep -r "PlayerKnowledge" --include="*.cs"` returns zero results
- [ ] Grep: `grep -r "ObservationDeck" --include="*.cs"` returns zero results
- [ ] Grep: `grep -r "BurdenDeck" --include="*.cs"` returns zero results
- [ ] Grep: `grep -r "CheckGoalRequirements" --include="*.cs"` returns zero results
- [ ] Grep: `grep -r "HasKnowledge" --include="*.cs"` returns zero results
- [ ] Grep: `grep -r "AddKnowledge" --include="*.cs"` returns zero results

### Architecture Verification ✅

- [ ] All goals always visible (no hidden goals based on player state)
- [ ] No boolean gates (`if (hasX)` patterns that hide content eliminated)
- [ ] Goals have transparent costs (Time, Focus, Stamina, Coins)
- [ ] Goals have transparent difficulty (BaseDifficulty value)
- [ ] Goals have transparent modifiers (DifficultyModifiers list)
- [ ] Difficulty varies based on player resources (Understanding, Familiarity, etc.)
- [ ] Multiple paths to reduce difficulty exist
- [ ] Resource competition creates strategic choices
- [ ] No ID matching for items (only ItemCategory mechanical properties)
- [ ] HasItemCategory checks category, not specific IDs

### Resource Verification ✅

- [ ] Understanding resource exists (Player.Understanding property)
- [ ] Understanding range: 0-10
- [ ] Understanding granted by Mental challenges (+1 to +3)
- [ ] Understanding never decreases (permanent growth)
- [ ] Understanding used by DifficultyModifiers (reduces Exposure)
- [ ] Familiarity used by DifficultyModifiers (not Requirements)
- [ ] Mastery used by DifficultyModifiers (reduces Danger)
- [ ] Connection Tokens used by DifficultyModifiers (reduces Doubt)

### JSON Content Verification ✅

- [ ] All goals have `costs` object (not `requirements`)
- [ ] All goals have `base_difficulty` value
- [ ] All goals have `difficulty_modifiers` array
- [ ] No goals have `requirements` object
- [ ] No goals have `required_knowledge` field
- [ ] No goals have `required_equipment` field
- [ ] All investigations have `understanding_reward` (not `knowledge_granted`)
- [ ] No investigations have prerequisites blocking phases

### Gameplay Verification ✅

- [ ] Test: Investigation progression without boolean gates
- [ ] Test: Resource competition between goals
- [ ] Test: Equipment mechanical properties (ItemCategory)
- [ ] Test: Understanding accumulation and usage
- [ ] Test: No hidden goals anywhere
- [ ] Test: Multiple paths to reduce difficulty
- [ ] Test: Strategic choices emerge naturally
- [ ] Test: Attempting hard goals early (risky) vs building resources (safe)

---

## Success Metrics

### Code Metrics

**Eliminated:**
- ~20 files deleted (Knowledge, Requirements, NPC decks)
- ~500 lines of boolean gate logic removed
- Zero `if (hasX)` patterns that hide content
- Zero string/ID matching for game mechanics

**Created:**
- ~5 new files (DifficultyModifier, GoalCosts, DifficultyCalculationService)
- ~320 lines of transparent difficulty calculation
- Board game resource model fully implemented

**Modified:**
- ~30 files updated
- All parsers updated to new structure
- All JSON content converted to new structure

### Architecture Metrics

**Boolean Gates:**
- Before: 5+ boolean gate systems (Knowledge, Requirements, etc.)
- After: ZERO boolean gates

**Goal Visibility:**
- Before: Goals hidden based on Requirements
- After: ALL goals always visible

**Strategic Depth:**
- Before: Linear progression, one path only
- After: Multiple paths, resource competition, strategic choices

**Transparency:**
- Before: Hidden requirements, opaque unlocking
- After: Transparent costs, transparent difficulty, transparent modifiers

### Gameplay Metrics

**Player Experience:**
- Before: "Why can't I do this?" (frustration)
- After: "Should I do this now or later?" (strategic choice)

**Progression:**
- Before: Linear (Phase 1 → 2 → 3, only path)
- After: Non-linear (multiple paths, player choice)

**Risk/Reward:**
- Before: No risk (requirements block attempts)
- After: Transparent risk (can attempt hard goals early, high failure chance)

**Resource Management:**
- Before: Collect tokens to unlock
- After: Manage competing resources, make trade-offs

---

## Implementation Timeline

**Estimated Effort:** 20-30 hours

**Phase 1 (Create New Systems):** 3-4 hours
- New entity definitions: 1 hour
- DifficultyCalculationService: 2 hours
- DTOs and infrastructure: 1 hour

**Phase 2 (Eliminate Knowledge):** 3-4 hours
- Delete files: 30 minutes
- Fix compilation errors: 2 hours
- Update Investigation rewards: 1 hour
- Verification: 30 minutes

**Phase 3 (Eliminate NPC Decks):** 2-3 hours
- Delete files: 30 minutes
- Fix compilation errors: 1 hour
- Update Investigation rewards: 1 hour
- Verification: 30 minutes

**Phase 4 (Eliminate Requirements):** 4-6 hours
- Delete GoalRequirements: 30 minutes
- Fix MASSIVE compilation errors: 3-4 hours
- Update ObstacleGoalFilter: 1 hour
- Verification: 1 hour

**Phase 5 (Update Parsers):** 2-3 hours
- GoalParser updates: 1 hour
- InvestigationParser updates: 30 minutes
- DTO updates: 30 minutes
- Testing: 1 hour

**Phase 6 (Update JSON Content):** 6-8 hours
- Core investigations: 2-3 hours
- Package content: 2-3 hours
- Documentation: 1 hour
- Verification: 1 hour

**Phase 7 (Final Verification):** 2-3 hours
- Grep verification: 30 minutes
- Compilation verification: 30 minutes
- Playtesting: 1-2 hours

---

## Risk Assessment

### High Risk Items

**1. Massive Compilation Errors (Phase 4)**
- Risk: Deleting GoalRequirements will break hundreds of references
- Mitigation: Incremental fixes, grep to find all usages, systematic approach
- Rollback: Git commit before Phase 4

**2. JSON Content Migration (Phase 6)**
- Risk: Manual JSON editing error-prone, many files to update
- Mitigation: Pattern-based search/replace, JSON validation
- Rollback: Git commit before Phase 6, can revert individual files

**3. Gameplay Balance (Phase 7)**
- Risk: New difficulty values may be unbalanced
- Mitigation: Playtesting, iterative adjustment
- Rollback: Adjust BaseDifficulty and modifier Effect values

### Medium Risk Items

**1. Investigation Activity Updates (Phase 2)**
- Risk: Missing some Knowledge references, subtle bugs
- Mitigation: Grep verification, unit tests
- Rollback: Git commit before Phase 2

**2. Parser Updates (Phase 5)**
- Risk: JSON parsing breaks, content fails to load
- Mitigation: DTO validation, parser testing
- Rollback: Git commit before Phase 5

### Low Risk Items

**1. Creating New Systems (Phase 1)**
- Risk: Minimal, adding alongside existing systems
- Mitigation: No changes to existing code
- Rollback: Delete new files if needed

**2. NPC Deck Elimination (Phase 3)**
- Risk: Low usage, limited impact
- Mitigation: Isolated system
- Rollback: Git commit before Phase 3

---

## Appendix: Design Rationale

### Why Eliminate Knowledge?

**Problem:**
- Knowledge tokens are discrete boolean flags
- Create "Cookie Clicker" progression (collect X to unlock Y)
- No strategic depth (linear path only)
- No resource competition (Knowledge never competes with anything)
- Fragile (string ID matching, typos break system)

**Solution:**
- Understanding is continuous resource (0-10)
- Accumulated from ANY Mental challenge (multiple paths)
- Competes with Focus and Time (limited resources)
- Graduated benefits (Understanding 2 helps more than Understanding 1)
- Strongly typed (no string matching)

### Why Eliminate Requirements?

**Problem:**
- Requirements create boolean gates (all-or-nothing)
- Goals hidden if requirements not met (frustration)
- No risk/reward choice (can't attempt hard goals early)
- Linear progression enforced by system
- Opaque to player (why is this locked?)

**Solution:**
- Costs + Difficulty + Modifiers always transparent
- Goals always visible (player can evaluate)
- Risk/reward emerges (attempt hard goal early = risky but fast)
- Multiple paths to reduce difficulty (strategic choice)
- Transparent to player (exact difficulty calculation shown)

### Why Keep Familiarity/Mastery/Tokens?

**Reasoning:**
- Already continuous resources (not boolean flags)
- Already accumulated through gameplay (not granted via tokens)
- Already create strategic choices (where to focus effort)
- Just need integration with DifficultyModifier system
- Perfect board game pattern already in place

### Why ItemCategory Not Item IDs?

**Problem with Item IDs:**
- String matching creates boolean gates
- Specific item "lantern" required (no alternatives)
- Fragile (typos break system)
- No mechanical reasoning (why does THIS lantern work but not THAT torch?)

**Solution with ItemCategory:**
- Mechanical property check (does item provide light?)
- ANY item with Light_Source category works (Lantern, Torch, Candle, etc.)
- Strongly typed (enum, no typos possible)
- Logical reasoning (player understands WHY it works)
- Already implemented correctly in RouteOption.cs

### Why Board Game Model?

**Video Game Boolean Gates:**
- Collect 3 keys to unlock door
- Complete quest A to unlock quest B
- Linear progression, no strategic choice
- "Why can't I do this?" (frustration)

**Board Game Resource Competition:**
- Wheat costs 3 wood, Shield costs 5 wood
- Multiple paths to get wood (forest, trade, cards)
- Limited wood forces choice: Wheat or Shield?
- "Should I do X or Y?" (strategic depth)

**Wayfarer Application:**
- Investigation A costs 20 Focus, Investigation B costs 30 Focus
- Multiple paths to get Understanding (any Mental challenge)
- Limited Focus forces choice: A+A, or A+B, or B alone?
- "Should I investigate mill or tavern first?" (strategic depth)

This is the difference between Cookie Clicker and actual game design.

---

## Appendix: Example Conversions

### Example 1: Simple Investigation Phase

**BEFORE:**
```json
{
  "id": "phase_examine_exterior",
  "name": "Examine Mill Exterior",
  "description": "Survey the exterior of the mill for clues",
  "requirements": null,
  "completion_reward": {
    "knowledge_granted": ["clue_mill_entrance"]
  }
}
```

**AFTER:**
```json
{
  "id": "phase_examine_exterior",
  "name": "Examine Mill Exterior",
  "description": "Survey the exterior of the mill for clues",
  "costs": {
    "time": 3,
    "focus": 10
  },
  "base_difficulty": 3,
  "difficulty_modifiers": [],
  "completion_reward": {
    "understanding_reward": 1
  }
}
```

### Example 2: Complex Investigation Phase

**BEFORE:**
```json
{
  "id": "phase_explore_interior",
  "name": "Explore Mill Interior",
  "description": "Enter the mill and search for evidence",
  "requirements": {
    "required_knowledge": ["clue_mill_entrance"],
    "required_equipment": ["lantern"],
    "minimum_location_familiarity": 2,
    "completed_goals": ["phase_examine_exterior"]
  },
  "completion_reward": {
    "knowledge_granted": ["clue_mill_structure", "clue_mill_records"]
  }
}
```

**AFTER:**
```json
{
  "id": "phase_explore_interior",
  "name": "Explore Mill Interior",
  "description": "Enter the mill and search for evidence",
  "costs": {
    "time": 3,
    "focus": 20
  },
  "base_difficulty": 10,
  "difficulty_modifiers": [
    {
      "type": "Understanding",
      "threshold": 2,
      "effect": -3
    },
    {
      "type": "Familiarity",
      "context": "mill",
      "threshold": 2,
      "effect": -2
    },
    {
      "type": "HasItemCategory",
      "context": "Light_Source",
      "effect": -2
    }
  ],
  "completion_reward": {
    "understanding_reward": 2
  }
}
```

**Key Changes:**
- No requirements → Goal always visible
- Base difficulty 10 (very hard without modifiers)
- Understanding 2 reduces by 3 → difficulty 7
- Familiarity 2 reduces by 2 → difficulty 5
- Light_Source item reduces by 2 → difficulty 3
- All modifiers: difficulty 3 (easy)
- No modifiers: difficulty 10 (very hard but attemptable)
- Multiple paths: Build Understanding through other challenges, or visit mill multiple times for Familiarity, or find Light_Source item
- Knowledge tokens replaced with Understanding resource
- No completed_goals prerequisite → Player can attempt Phase 2 immediately (risky)

### Example 3: Obstacle Goal

**BEFORE:**
```json
{
  "id": "goal_negotiate_with_guard",
  "name": "Negotiate with Gate Guard",
  "requirements": {
    "required_knowledge": ["town_customs"],
    "required_stats": [
      { "stat": "Charm", "minimum": 2 }
    ]
  },
  "consequence_type": "Bypass"
}
```

**AFTER:**
```json
{
  "id": "goal_negotiate_with_guard",
  "name": "Negotiate with Gate Guard",
  "costs": {
    "time": 2
  },
  "base_difficulty": 8,
  "difficulty_modifiers": [
    {
      "type": "Understanding",
      "threshold": 3,
      "effect": -2
    },
    {
      "type": "ConnectionTokens",
      "context": "guard_captain",
      "threshold": 5,
      "effect": -3
    }
  ],
  "consequence_type": "Bypass"
}
```

**Key Changes:**
- Knowledge "town_customs" → Understanding resource (general social expertise)
- Stat requirement Charm 2 → ConnectionTokens modifier (specific relationship)
- Goal always visible (no hiding based on stats)
- Base difficulty 8 (moderate)
- Multiple paths: Build Understanding through Mental challenges, or build relationship with guard captain
- Strategic choice: Attempt early (moderate difficulty) or build resources first (easier)

---

**END OF DOCUMENT**
