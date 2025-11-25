# DUAL-TIER ACTION ARCHITECTURE (CRITICAL: READ THIS)

**DATE:** 2025-01-23 (Updated 2025-11-23 with Fallback Scene concept)
**PURPOSE:** Document the TWO-TIER action system to prevent architectural misunderstandings

---

## FALLBACK SCENE ARCHITECTURE (Conceptual Model)

**Think of atmospheric actions as a "FALLBACK SCENE"** - when no active scene exists at a location/NPC, the player sees atmospheric actions as the default baseline. This unifies the mental model:

- **Active Scene** → Scene-based actions from Situations (dynamic narrative)
- **Fallback Scene** → Atmospheric actions (always-present baseline: Travel, Work, Rest)

Both are "scenes" conceptually, but with different implementation patterns (direct properties vs ChoiceTemplate). The overlay pattern means GameFacade returns EITHER active scene actions OR fallback scene actions, never both.

This keeps scene logic consistent while maintaining separate data patterns for performance/simplicity reasons.

---

## THE CRITICAL MISTAKE (Learn From This)

**What Happened:** Claude attempted to "clean up legacy code" by deleting ActionCosts/ActionRewards properties from LocationAction and PathCard entities, assuming ALL actions should use ChoiceTemplate exclusively.

**Why This Was Wrong:** The architecture has TWO INTENTIONAL action patterns serving different purposes. Deleting those properties broke the atmospheric action layer that prevents soft-locks.

**The Lesson:** LocationAction is a UNION TYPE supporting TWO patterns via pattern discrimination. Both patterns are CORRECT and INTENTIONAL architecture, not legacy code to delete.

---

## THE TWO-TIER ARCHITECTURE (Ground Truth)

### TIER 1: Atmospheric Action Layer (Parse-Time, Permanent)

**Purpose:** Persistent gameplay scaffolding preventing dead ends and soft-locks

**Source:** LocationActionCatalog generates at package load time

**Storage:** GameWorld.LocationActions (permanent collection, saved in save files)

**Properties Used:**
- `ActionCosts Costs` - Direct cost properties (coins, stamina, time, etc.)
- `ActionRewards Rewards` - Direct reward properties (coins, health, focus, etc.)
- `ChoiceTemplate` is **NULL** for atmospheric actions

**Examples:**
- Travel (opens route selection screen)
- Work (earn coins through labor)
- Rest (recover health/stamina)
- Intra-Venue Movement (walk to adjacent location)

**Characteristics:**
- Simple, fixed costs/rewards (no formulas needed)
- Always available at appropriate locations (based on capabilities)
- Never deleted or modified during gameplay
- Provide baseline player agency independent of narrative state

**Implementation Pattern:**
```csharp
// LocationActionCatalog.cs
actions.Add(new LocationAction
{
    Name = "Work",
    Costs = new ActionCosts { /* fixed costs */ },
    Rewards = new ActionRewards { CoinReward = 8 },
    ChoiceTemplate = null  // ← ATMOSPHERIC PATTERN
});
```

**Executor Pattern:**
```csharp
// LocationActionExecutor.cs
if (action.ChoiceTemplate == null)
{
    // Atmospheric action - use direct Costs/Rewards
    return ValidateAtmosphericAction(action, player);
}
```

---

### TIER 2: Scene-Based Action Layer (Query-Time, Ephemeral)

**Purpose:** Dynamic narrative content from Scene-Situation architecture

**Source:** ChoiceTemplate generates actions when Situation is active

**Storage:** **NOT STORED** - actions passed by object reference, discarded after execution

**Properties Used:**
- `ChoiceTemplate` - References template with CostTemplate/RewardTemplate/RequirementFormula
- `ChoiceTemplate.CostTemplate` - Formula-based costs
- `ChoiceTemplate.RewardTemplate` - Complex rewards (bonds, scales, scene spawns)
- `Situation` - Reference to source situation for cleanup

**Examples:**
- Investigate Missing Goods (scene at location)
- Converse with NPC (scene at NPC)
- Choose Path with Scene Encounter (scene on route)

**Characteristics:**
- Complex, dynamic costs/rewards (formulas, requirements, scaling)
- Conditional availability (only when parent scene is active)
- Created fresh on query, deleted after execution
- Provide narrative depth layered on atmospheric baseline

**Implementation Pattern:**
```csharp
// SceneFacade generates from ChoiceTemplate
LocationAction action = new LocationAction
{
    Name = template.Name,
    ChoiceTemplate = template,  // ← SCENE-BASED PATTERN
    Situation = situation,
    Costs = null,  // Not used - template has costs
    Rewards = null  // Not used - template has rewards
};
```

**Executor Pattern:**
```csharp
// LocationActionExecutor.cs
if (action.ChoiceTemplate != null)
{
    // Scene-based action - use ChoiceTemplate
    return ValidateChoiceTemplate(action.ChoiceTemplate, player, gameWorld);
}
```

---

## PATTERN DISCRIMINATION (How Systems Coexist)

**LocationAction entity is a UNION TYPE:**

```csharp
public class LocationAction
{
    // PATTERN DISCRIMINATOR
    public ChoiceTemplate ChoiceTemplate { get; set; }

    // TIER 1: Atmospheric Pattern (used when ChoiceTemplate == null)
    public ActionCosts Costs { get; set; }
    public ActionRewards Rewards { get; set; }

    // TIER 2: Scene-Based Pattern (used when ChoiceTemplate != null)
    public Situation Situation { get; set; }

    // Shared properties (both patterns)
    public Location SourceLocation { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    // ... etc
}
```

**Runtime Logic:**
1. Executor checks `action.ChoiceTemplate`
2. IF null → Atmospheric action → use `action.Costs` and `action.Rewards`
3. IF not null → Scene-based action → use `action.ChoiceTemplate.CostTemplate` and `RewardTemplate`

**This is NOT a violation of HIGHLANDER** - these are TWO DIFFERENT concepts sharing ONE entity class via pattern discrimination, not duplicate representations of the SAME data.

---

## WHY BOTH PATTERNS EXIST (Design Rationale)

### Why Not Use ChoiceTemplate for Everything?

**Atmospheric actions are simple and permanent:**
- Work always costs time, always gives 8 coins
- Rest always costs time, always recovers 1 health + 1 stamina
- Travel always opens route screen with zero cost

**ChoiceTemplate would be OVERKILL:**
- Formula system for constants: `CostTemplate.Coins = 0` (why not just `Costs.Coins = 0`?)
- Requirement system for always-true: `RequirementFormula = null` (simpler: no check)
- Template indirection: Load template → read CostTemplate → read Coins (vs direct: read Costs.Coins)

**Atmospheric actions are generated at parse-time:**
- LocationActionCatalog creates complete LocationAction entities immediately
- Storing template reference adds complexity for no benefit
- Direct properties are self-contained (no external dependencies)

### Why Not Use Direct Properties for Everything?

**Scene-based actions are complex and dynamic:**
- Costs/rewards vary by context (NPC personality, location tier, player stats)
- Requirements use OR paths (need stat X OR stat Y OR item Z)
- Rewards spawn scenes, modify relationships, trigger events

**Direct properties would be INSUFFICIENT:**
- Can't express formula scaling: "Cost scales with location tier"
- Can't express OR requirements: "Need Insight 3 OR Cunning 2"
- Can't express complex rewards: "Spawn scene at location matching filter"

**Scene-based actions are ephemeral:**
- Created fresh each query from templates
- Same template used across many situations with different entities
- Template reference enables reusability and data-driven content

---

## PATHCARD FOLLOWS SAME PATTERN

PathCard also has dual patterns:

**Static Route PathCards (Atmospheric):**
- Defined in route JSON files
- Use direct properties: `StaminaCost`, `TravelTimeSegments`, `CoinReward`, etc.
- Persistent route options (part of authored content)

**Scene-Based PathCards (Ephemeral):**
- Generated from ChoiceTemplate when scene active on route
- Use `ChoiceTemplate` property
- Temporary narrative encounters on travel

---

## NOMENCLATURE CLARITY

**AVOID "LEGACY" terminology** - this implies code to delete. Use correct terms:

✅ **Atmospheric Actions** - Parse-time permanent scaffolding
✅ **Scene-Based Actions** - Query-time ephemeral narrative
✅ **Direct Properties** - Costs/Rewards on entity (atmospheric pattern)
✅ **Template Properties** - ChoiceTemplate reference (scene-based pattern)

❌ **Legacy Actions** - Misleading, implies deprecated code
❌ **Old System** - Wrong, atmospheric system is current and correct
❌ **Compatibility Layer** - Wrong, this is core architecture

---

## VALIDATION CHECKLIST (Before Deleting Properties)

Before removing ANY property from LocationAction or PathCard:

1. ✅ Search codebase for ALL usages (Grep exhaustively)
2. ✅ Check LocationActionCatalog - does it use this property?
3. ✅ Check PathCard generation - does route system use this?
4. ✅ Read architecture/12_glossary.md entries for Action types
5. ✅ Read design/12_design_glossary.md for Atmospheric Action Layer
6. ✅ Verify pattern discrimination logic in executors
7. ✅ Confirm property is truly unused in BOTH patterns
8. ✅ Ask: Does deleting this break atmospheric scaffolding?

**IF IN DOUBT, ASK BEFORE DELETING. Atmospheric system is CRITICAL.**

---

## THE COMPILATION ERRORS WERE THE WARNING

When you see:
```
LocationActionCatalog.cs: error CS0117: "LocationAction" enthält keine Definition für "Costs"
```

This is NOT "legacy code to update" - this is **ATMOSPHERIC SYSTEM BROKEN**.

LocationActionCatalog is the HEART of the atmospheric layer. Breaking it breaks soft-lock prevention.

---

## EXECUTOR ARCHITECTURE (SRP-Compliant Refactoring - 2025-11-23)

**The executor layer was refactored to fix SRP and HIGHLANDER violations.**

### Previous Architecture (WRONG - SRP Violation)

**LocationActionExecutor** handled BOTH atmospheric AND scene-based patterns:
- Pattern discrimination inside executor (if/else on ChoiceTemplate)
- Single class with two responsibilities (validation of two different patterns)

**NPCActionExecutor** and **PathCardExecutor** both contained duplicate ChoiceTemplate validation logic (HIGHLANDER violation).

### Current Architecture (CORRECT - SRP Compliant)

**Two specialized executors, each with one responsibility:**

1. **LocationActionExecutor** - Validates ONLY atmospheric (fallback scene) actions
   - Input: LocationAction with null ChoiceTemplate OR PathCard with null ChoiceTemplate
   - Validates: Direct Costs/Rewards properties
   - Method: `ValidateAndExtract(LocationAction, Player)` and `ValidateAtmosphericPathCard(PathCard, Player)`
   - NO ChoiceTemplate logic

2. **SituationChoiceExecutor** - Validates ALL ChoiceTemplate-based actions (HIGHLANDER)
   - Input: ChoiceTemplate from any source (LocationAction, NPCAction, PathCard)
   - Validates: RequirementFormula, CostTemplate, RewardTemplate
   - Method: `ValidateAndExtract(ChoiceTemplate, actionName, Player, GameWorld)`
   - Single source of truth for ChoiceTemplate validation

**Pattern routing happens in GameFacade:**
- GameFacade checks `action.ChoiceTemplate != null` to determine pattern
- Calls appropriate executor based on pattern (discrimination moved from executor to caller)
- Executors are pattern-specific (no internal discrimination)

**NPCActionExecutor and PathCardExecutor DELETED:**
- All NPC actions are scene-based → use SituationChoiceExecutor directly
- PathCards follow dual-pattern → route to LocationActionExecutor or SituationChoiceExecutor based on ChoiceTemplate

### Benefits of Refactoring

- **SRP Compliance**: Each executor has exactly one responsibility
- **HIGHLANDER Compliance**: ChoiceTemplate validation exists in exactly ONE place
- **Reduced Code Duplication**: ValidateChoiceTemplate logic unified (was duplicated 3 times)
- **Clear Separation**: Atmospheric vs scene-based is enforced by class structure, not if/else

---

## SUMMARY FOR CLAUDE (Read This Every Time)

**LocationAction has TWO patterns via union type:**

1. **Atmospheric** (ChoiceTemplate == null) → Use Costs/Rewards directly
2. **Scene-Based** (ChoiceTemplate != null) → Use ChoiceTemplate.CostTemplate/RewardTemplate

**BOTH patterns are CORRECT and INTENTIONAL.**

**DO NOT delete ActionCosts/ActionRewards properties** - they are REQUIRED for atmospheric actions.

**DO NOT assume "legacy" means "delete"** - verify architectural intent first.

**DO read glossaries BEFORE refactoring** - architecture is documented.

**This document exists because Claude made this mistake. Don't repeat it.**
