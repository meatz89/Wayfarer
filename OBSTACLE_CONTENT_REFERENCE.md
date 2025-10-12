# Obstacle System - Content Reference

**Version:** 1.0
**Last Updated:** 2025-01-13
**Target Audience:** Content creators, designers, and developers

---

## Table of Contents

1. [Core Concepts](#core-concepts)
2. [Complete JSON Schema](#complete-json-schema)
3. [Consequence Types](#consequence-types)
4. [Property-Based Gating](#property-based-gating)
5. [Distributed Interaction Pattern](#distributed-interaction-pattern)
6. [Examples](#examples)
7. [Common Patterns](#common-patterns)
8. [Validation Rules](#validation-rules)

---

## Core Concepts

### What is an Obstacle?

An **Obstacle** is a challenge with **multiple resolution paths** and **property-based gating**. It represents:
- Physical danger (collapsing floors, locked doors, bandits)
- Mental complexity (puzzles, mechanisms, research)
- Social difficulty (tense conversations, negotiations)

### Key Principles

**1. One Obstacle, Many Goals**
- One obstacle can have 2-5 goals representing different approaches
- Goals are **contained inline** within the obstacle (not separate entities)
- Each goal targets a specific tactical system (Physical/Mental/Social)

**2. Property-Based Gating**
- Goals become visible when obstacle properties meet thresholds
- `maxPhysicalDanger: 2` → Goal visible only if obstacle's PhysicalDanger ≤ 2
- NO string matching for prerequisites (spawning IS the gate)

**3. Distributed Interaction**
- One obstacle can place goals at **multiple locations/NPCs**
- `PlacementLocationId`/`PlacementNpcId` → Where goal's button appears in UI
- Example: One "Mill Entrance Blocked" obstacle → Force door at entrance + Find alternate entry at courtyard

**4. Three Properties Only**
- `PhysicalDanger`: 0-3 (structural stability, combat danger)
- `MentalComplexity`: 0-3 (puzzle difficulty, information density)
- `SocialDifficulty`: 0-3 (conversation tension, relationship stakes)

---

## Complete JSON Schema

### Obstacle in Package File

Obstacles can be defined in two locations:

**1. Package-Level (01_foundation.json, etc.)**
```json
{
  "content": {
    "obstacles": [
      {
        "id": "unique_obstacle_id",
        "name": "Display Name",
        "description": "Narrative description of the challenge",
        "physicalDanger": 2,
        "mentalComplexity": 1,
        "socialDifficulty": 0,
        "isPermanent": false,
        "goals": [
          {
            "id": "unique_goal_id",
            "name": "Goal Action Name",
            "description": "What player does",
            "placementLocationId": "location_id",
            "placementNpcId": "npc_id",
            "systemType": "Physical",
            "deckId": "physical_challenge",
            "consequenceType": "Resolution",
            "setsResolutionMethod": "Violence",
            "setsRelationshipOutcome": "Neutral",
            "investigationId": "investigation_id",
            "propertyRequirements": {
              "maxPhysicalDanger": 2,
              "maxMentalComplexity": 1,
              "maxSocialDifficulty": 0
            },
            "propertyReduction": {
              "reducePhysicalDanger": 1,
              "reduceMentalComplexity": 0,
              "reduceSocialDifficulty": 0
            },
            "isAvailable": true,
            "deleteOnSuccess": true,
            "goalCards": [
              {
                "id": "card_id",
                "name": "Card Name",
                "description": "Card description",
                "threshold": 8,
                "rewards": {
                  "progress": 1
                }
              }
            ]
          }
        ]
      }
    ]
  }
}
```

**2. Investigation Phase Rewards (13_investigations.json)**
```json
{
  "phases": [
    {
      "id": "phase_1",
      "completionReward": {
        "knowledgeGranted": ["knowledge_id"],
        "obstaclesSpawned": [
          {
            "targetType": "Location",
            "targetEntityId": "location_id",
            "obstacle": {
              // Same obstacle structure as above
            }
          }
        ]
      }
    }
  ]
}
```

### Location Reference

Locations reference obstacles via `obstacleIds` array:

```json
{
  "locations": [
    {
      "id": "mill_entrance",
      "venueId": "old_mill",
      "name": "Mill Entrance",
      "obstacleIds": ["mill_entrance_blocked"]
    }
  ]
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | ✅ | Globally unique obstacle ID |
| `name` | string | ✅ | Display name shown to player |
| `description` | string | ✅ | Narrative description of challenge |
| `physicalDanger` | int (0-3) | ✅ | Structural/combat danger level |
| `mentalComplexity` | int (0-3) | ✅ | Puzzle/information complexity |
| `socialDifficulty` | int (0-3) | ✅ | Conversation tension level |
| `isPermanent` | bool | ✅ | If true, obstacle persists after resolution |
| `goals` | array | ✅ | 1-5 goals representing approaches |

### Goal Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `id` | string | ✅ | Globally unique goal ID |
| `name` | string | ✅ | Action button text |
| `description` | string | ✅ | What player does |
| `placementLocationId` | string | ⚠️ | Where goal button appears (Location) |
| `placementNpcId` | string | ⚠️ | Where goal button appears (NPC) |
| `systemType` | enum | ✅ | Physical/Mental/Social |
| `deckId` | string | ✅ | Challenge deck to use |
| `consequenceType` | enum | ✅ | See Consequence Types section |
| `setsResolutionMethod` | enum | ⚠️ | Resolution approach for AI context |
| `setsRelationshipOutcome` | enum | ⚠️ | Relationship outcome for AI context |
| `investigationId` | string | ⚠️ | Links goal to investigation |
| `propertyRequirements` | object | ❌ | Visibility thresholds |
| `propertyReduction` | object | ❌ | Property reduction on success |
| `isAvailable` | bool | ❌ | Immediate visibility (default: true) |
| `deleteOnSuccess` | bool | ❌ | Remove goal after completion |
| `goalCards` | array | ✅ | Challenge cards for this goal |

⚠️ = Required in specific contexts
❌ = Optional

---

## Consequence Types

### 1. Resolution
**Completes the obstacle entirely.**

```json
{
  "consequenceType": "Resolution",
  "setsResolutionMethod": "Violence",
  "setsRelationshipOutcome": "Neutral"
}
```

**When to Use:**
- Final approach that solves the obstacle
- Player breaks through door, defeats bandits, solves puzzle
- Investigation phase completion goals

**ResolutionMethod Enum:**
- `Violence` - Forceful approach
- `Diplomacy` - Negotiation, conversation
- `Cleverness` - Outsmarting, puzzle solving
- `Preparation` - Careful planning, resource gathering
- `Boldness` - Risk-taking, daring action

**RelationshipOutcome Enum:**
- `Positive` - Improves relationship
- `Neutral` - No relationship change
- `Negative` - Damages relationship

---

### 2. Bypass
**Avoids the obstacle without resolving it.**

```json
{
  "consequenceType": "Bypass",
  "setsResolutionMethod": "Cleverness",
  "setsRelationshipOutcome": "Neutral"
}
```

**When to Use:**
- Player finds alternate path around obstacle
- Sneak past guards instead of fighting
- Climb window instead of forcing door

**Key Difference from Resolution:**
- Obstacle remains but player progresses
- May have narrative consequences later
- NPCs/world remember obstacle wasn't resolved

---

### 3. Transform
**Changes obstacle into a different form.**

```json
{
  "consequenceType": "Transform",
  "transformsIntoObstacleId": "new_obstacle_id"
}
```

**When to Use:**
- Obstacle evolves into new challenge
- Player actions escalate/de-escalate situation
- Example: "Suspicious Guards" → "Hostile Guards"

**Implementation:**
- Original obstacle deleted
- New obstacle spawned at same location/NPC
- Progression tracked through transformation chain

---

### 4. Modify
**Reduces obstacle properties without completing it.**

```json
{
  "consequenceType": "Modify",
  "propertyReduction": {
    "reducePhysicalDanger": 1,
    "reduceMentalComplexity": 0,
    "reduceSocialDifficulty": 0
  }
}
```

**When to Use:**
- Preparation actions that make challenge easier
- Example: Get rope → Reduces loft danger from 3 to 2
- Unlocks new goals with lower property requirements

**Mechanic:**
- Obstacle properties reduced by specified amounts
- Goals with lower requirements become visible
- Can be stacked (multiple Modify goals)

---

### 5. Grant
**Provides reward without changing obstacle state.**

```json
{
  "consequenceType": "Grant",
  "rewards": {
    "coins": 10,
    "items": ["rope"],
    "knowledge": ["hidden_path"]
  }
}
```

**When to Use:**
- Information gathering that doesn't resolve obstacle
- Resource collection that doesn't change challenge
- Observation goals

**Key Feature:**
- Obstacle remains unchanged
- Goal remains available (unless `deleteOnSuccess: true`)
- Can be repeated for farming resources

---

## Property-Based Gating

### How It Works

Goals become visible based on **mechanical properties**, NOT string matching.

**Example:**
```json
{
  "obstacle": {
    "physicalDanger": 3,
    "goals": [
      {
        "name": "Climb to loft (dangerous)",
        "propertyRequirements": {
          "maxPhysicalDanger": 3
        }
      },
      {
        "name": "Climb to loft (safe)",
        "propertyRequirements": {
          "maxPhysicalDanger": 2
        }
      }
    ]
  }
}
```

**Initial State:**
- PhysicalDanger = 3
- Only "Climb to loft (dangerous)" visible (maxPhysicalDanger: 3)
- "Climb to loft (safe)" **hidden** (requires ≤ 2)

**After Getting Rope (Modify goal):**
- PhysicalDanger = 2 (reduced by 1)
- "Climb to loft (safe)" **now visible** (2 ≤ 2)
- "Climb to loft (dangerous)" still visible

**No String Matching:**
- ❌ `"requirements": { "knowledge": ["has_rope"] }`
- ✅ Property reduction makes goal visible mechanically

---

### Property Thresholds

**PhysicalDanger (0-3):**
- 0: No danger (safe environment)
- 1: Minor risk (scratches, bruises)
- 2: Moderate danger (injury possible)
- 3: High danger (death possible)

**MentalComplexity (0-3):**
- 0: Simple (obvious solution)
- 1: Moderate (requires thought)
- 2: Complex (multi-step reasoning)
- 3: Expert (specialized knowledge)

**SocialDifficulty (0-3):**
- 0: Casual (friendly chat)
- 1: Tense (delicate topic)
- 2: Difficult (high stakes)
- 3: Critical (relationship-defining)

---

## Distributed Interaction Pattern

### One Obstacle, Multiple Locations

**Pattern:**
One obstacle can place goals at different locations, creating a **spatial puzzle**.

**Example: "Mill Entrance Blocked"**

```json
{
  "id": "mill_entrance_blocked",
  "name": "Boarded Door",
  "physicalDanger": 2,
  "mentalComplexity": 1,
  "goals": [
    {
      "id": "force_door",
      "name": "Force the door open",
      "placementLocationId": "mill_entrance",
      "systemType": "Physical",
      "consequenceType": "Resolution"
    },
    {
      "id": "find_alternate_entry",
      "name": "Find another way in",
      "placementLocationId": "courtyard",
      "systemType": "Mental",
      "consequenceType": "Bypass"
    }
  ]
}
```

**Result:**
- Player sees "Force the door open" button at **mill_entrance**
- Player sees "Find another way in" button at **courtyard**
- Both goals resolve the same obstacle
- Player must **explore** to find all options

**Why This Matters:**
- Encourages exploration (goals aren't all in one place)
- Creates spatial tactics (where you engage obstacle matters)
- Prevents "goal spam" (distributed across world)

---

### PlacementLocationId vs PlacementNpcId

**Semantics:**
- **Where goal's UI button appears**
- NOT ownership (obstacle owns goal)
- NOT mechanical dependency

**Physical/Mental Goals:**
```json
{
  "placementLocationId": "workshop"
  // Button appears at workshop location screen
}
```

**Social Goals:**
```json
{
  "placementNpcId": "elena"
  // Button appears in Elena's conversation screen
}
```

**One Goal = One Placement:**
- ✅ `placementLocationId` OR `placementNpcId`
- ❌ NOT both simultaneously

---

## Examples

### Example 1: Simple Physical Obstacle

```json
{
  "id": "locked_door",
  "name": "Locked Door",
  "description": "Heavy oak door with a rusted lock",
  "physicalDanger": 1,
  "mentalComplexity": 1,
  "socialDifficulty": 0,
  "isPermanent": false,
  "goals": [
    {
      "id": "force_lock",
      "name": "Force the lock",
      "description": "Break the lock with brute strength",
      "placementLocationId": "storage_room",
      "systemType": "Physical",
      "deckId": "physical_challenge",
      "consequenceType": "Resolution",
      "setsResolutionMethod": "Violence",
      "setsRelationshipOutcome": "Neutral",
      "propertyRequirements": {
        "maxPhysicalDanger": 1
      },
      "deleteOnSuccess": true,
      "goalCards": [
        {
          "id": "force_lock_card",
          "name": "Break Lock",
          "description": "Force the lock open",
          "threshold": 6,
          "rewards": {
            "progress": 1
          }
        }
      ]
    },
    {
      "id": "pick_lock",
      "name": "Pick the lock",
      "description": "Carefully work the mechanism",
      "placementLocationId": "storage_room",
      "systemType": "Mental",
      "deckId": "mental_challenge",
      "consequenceType": "Resolution",
      "setsResolutionMethod": "Cleverness",
      "setsRelationshipOutcome": "Neutral",
      "propertyRequirements": {
        "maxMentalComplexity": 1
      },
      "deleteOnSuccess": true,
      "goalCards": [
        {
          "id": "pick_lock_card",
          "name": "Pick Lock",
          "description": "Manipulate the lock mechanism",
          "threshold": 8,
          "rewards": {
            "progress": 1
          }
        }
      ]
    }
  ]
}
```

---

### Example 2: Multi-Stage Obstacle with Modify

```json
{
  "id": "unstable_loft",
  "name": "Unstable Loft Access",
  "description": "The loft floor is dangerously unstable",
  "physicalDanger": 3,
  "mentalComplexity": 1,
  "socialDifficulty": 0,
  "isPermanent": false,
  "goals": [
    {
      "id": "get_rope",
      "name": "Get rope from workshop",
      "description": "Find rope to secure the floor",
      "placementLocationId": "workshop",
      "systemType": "Mental",
      "deckId": "mental_challenge",
      "consequenceType": "Modify",
      "setsResolutionMethod": "Preparation",
      "setsRelationshipOutcome": "Neutral",
      "propertyReduction": {
        "reducePhysicalDanger": 1
      },
      "deleteOnSuccess": true,
      "goalCards": [
        {
          "id": "find_rope_card",
          "name": "Find Rope",
          "description": "Locate suitable rope",
          "threshold": 6,
          "rewards": {
            "progress": 1
          }
        }
      ]
    },
    {
      "id": "climb_loft_dangerous",
      "name": "Climb to loft (risky)",
      "description": "Climb without safety measures",
      "placementLocationId": "main_hall",
      "systemType": "Physical",
      "deckId": "physical_challenge",
      "consequenceType": "Resolution",
      "setsResolutionMethod": "Boldness",
      "setsRelationshipOutcome": "Neutral",
      "propertyRequirements": {
        "maxPhysicalDanger": 3
      },
      "deleteOnSuccess": true,
      "goalCards": [
        {
          "id": "climb_risky_card",
          "name": "Risky Climb",
          "description": "Climb without preparation",
          "threshold": 10,
          "rewards": {
            "progress": 1
          }
        }
      ]
    },
    {
      "id": "climb_loft_safe",
      "name": "Climb to loft (secured)",
      "description": "Climb with rope securing the floor",
      "placementLocationId": "main_hall",
      "systemType": "Physical",
      "deckId": "physical_challenge",
      "consequenceType": "Resolution",
      "setsResolutionMethod": "Preparation",
      "setsRelationshipOutcome": "Neutral",
      "propertyRequirements": {
        "maxPhysicalDanger": 2
      },
      "deleteOnSuccess": true,
      "goalCards": [
        {
          "id": "climb_safe_card",
          "name": "Safe Climb",
          "description": "Climb with safety measures",
          "threshold": 7,
          "rewards": {
            "progress": 1
          }
        }
      ]
    }
  ]
}
```

**Flow:**
1. Initial state: PhysicalDanger = 3
   - "Get rope" visible (Modify goal, always shown)
   - "Climb (risky)" visible (requires ≤ 3)
   - "Climb (secured)" **hidden** (requires ≤ 2)
2. Player completes "Get rope"
   - PhysicalDanger reduced to 2
   - "Climb (secured)" **now visible** (2 ≤ 2)
3. Player completes either climb goal
   - Obstacle resolved

---

### Example 3: Social Obstacle on NPC

```json
{
  "id": "elena_guarded_secret",
  "name": "Elena's Guarded Secret",
  "description": "Elena knows more than she's admitting",
  "physicalDanger": 0,
  "mentalComplexity": 0,
  "socialDifficulty": 2,
  "isPermanent": false,
  "goals": [
    {
      "id": "confront_elena",
      "name": "Confront Elena about the evidence",
      "description": "Show Elena the smuggling ledgers",
      "placementNpcId": "elena",
      "systemType": "Social",
      "deckId": "social_challenge",
      "consequenceType": "Resolution",
      "setsResolutionMethod": "Diplomacy",
      "setsRelationshipOutcome": "Positive",
      "investigationId": "waterwheel_mystery",
      "propertyRequirements": {
        "maxSocialDifficulty": 2
      },
      "deleteOnSuccess": true,
      "goalCards": [
        {
          "id": "confront_elena_card",
          "name": "Diplomatic Approach",
          "description": "Present evidence carefully",
          "threshold": 8,
          "rewards": {
            "progress": 1
          }
        }
      ]
    }
  ]
}
```

---

## Common Patterns

### Pattern 1: The Gauntlet
**One approach, no alternatives.**

```json
{
  "physicalDanger": 2,
  "goals": [
    {
      "name": "Force through",
      "consequenceType": "Resolution"
    }
  ]
}
```

**Use When:** No narrative alternatives (locked in sequence).

---

### Pattern 2: The Fork
**Two equal approaches.**

```json
{
  "physicalDanger": 2,
  "mentalComplexity": 2,
  "goals": [
    {
      "name": "Force door",
      "systemType": "Physical",
      "consequenceType": "Resolution"
    },
    {
      "name": "Pick lock",
      "systemType": "Mental",
      "consequenceType": "Resolution"
    }
  ]
}
```

**Use When:** Player chooses playstyle (brute force vs. finesse).

---

### Pattern 3: The Preparation Ladder
**Modify goals unlock easier Resolution.**

```json
{
  "physicalDanger": 3,
  "goals": [
    {
      "name": "Get rope",
      "consequenceType": "Modify",
      "propertyReduction": { "reducePhysicalDanger": 1 }
    },
    {
      "name": "Risky climb",
      "propertyRequirements": { "maxPhysicalDanger": 3 },
      "consequenceType": "Resolution"
    },
    {
      "name": "Safe climb",
      "propertyRequirements": { "maxPhysicalDanger": 2 },
      "consequenceType": "Resolution"
    }
  ]
}
```

**Use When:** Preparation is optional but beneficial.

---

### Pattern 4: The Distributed Choice
**Same obstacle, goals at different locations.**

```json
{
  "goals": [
    {
      "placementLocationId": "entrance",
      "name": "Force door"
    },
    {
      "placementLocationId": "courtyard",
      "name": "Find window"
    }
  ]
}
```

**Use When:** Encouraging exploration.

---

## Validation Rules

### Duplicate ID Protection

**Rule:** Obstacle IDs must be globally unique across all packages.

**Validation Locations:**
1. `LocationParser.cs:190` - Package-level obstacles
2. `NPCParser.cs:133` - Package-level obstacles on NPCs
3. `InvestigationActivity.cs:403` - Runtime obstacle spawning (Location)
4. `InvestigationActivity.cs:449` - Runtime obstacle spawning (NPC)
5. `PackageLoader.cs:631` - Package-level obstacle loading

**Error Message:**
```
Duplicate obstacle ID 'obstacle_id' found in package 'package_id'.
Obstacle IDs must be globally unique across all packages.
```

---

### Property Ranges

**All properties must be 0-3:**
- PhysicalDanger: 0-3
- MentalComplexity: 0-3
- SocialDifficulty: 0-3

**NPCs can ONLY have SocialDifficulty obstacles:**
- Physical/Mental properties must be 0
- Validated in `NPCParser.cs:123-130`

---

### Goal Placement

**Each goal must have exactly one placement:**
- ✅ `placementLocationId` (Physical/Mental goals)
- ✅ `placementNpcId` (Social goals)
- ❌ Both simultaneously (invalid)

---

### Consequence Type Requirements

**Resolution:**
- Must have `setsResolutionMethod`
- Must have `setsRelationshipOutcome`
- Should have `deleteOnSuccess: true`

**Modify:**
- Must have `propertyReduction` with at least one reduction
- Should NOT have `deleteOnSuccess: true` (multiple uses)

**Transform:**
- Must have `transformsIntoObstacleId`
- Referenced obstacle must exist

---

## Architecture Principles

### NO String Matching

**❌ FORBIDDEN:**
```json
{
  "requirements": {
    "knowledge": ["has_rope"],
    "completedGoals": ["phase_1"]
  }
}
```

**✅ CORRECT:**
```json
{
  "completionReward": {
    "obstaclesSpawned": [/* Phase 2 obstacle */]
  }
}
```

**Why:** Spawning IS the gate. Phase 2 obstacle doesn't exist until Phase 1 completes.

---

### Knowledge is Metadata Only

**Knowledge grants are for:**
- ✅ UI visibility (show player what they learned)
- ✅ Narrative context (conversations reference discoveries)
- ❌ NOT mechanical gating (use properties for that)

---

### Single Source of Truth

**Obstacles are stored in:**
- `GameWorld.Obstacles` (single list, globally unique IDs)

**Referenced by:**
- `Location.ObstacleIds` (string array)
- `NPC.ObstacleIds` (string array)
- `RouteOption.Obstacles` (direct reference for travel obstacles)

**Goals are:**
- Contained inline within obstacles
- NOT separate entities
- NOT stored in separate collections

---

## Summary

**Key Takeaways:**

1. **Three Properties Only:** PhysicalDanger, MentalComplexity, SocialDifficulty (0-3)
2. **Five Consequence Types:** Resolution, Bypass, Transform, Modify, Grant
3. **Property-Based Gating:** Goals visible based on properties, NOT string matching
4. **Distributed Interaction:** Goals at multiple locations/NPCs for spatial tactics
5. **Spawning IS the Gate:** Investigation progression through obstacle spawning
6. **Containment Pattern:** Goals live inside obstacles, not as separate entities
7. **Single Source of Truth:** GameWorld.Obstacles with references from locations/NPCs
8. **No String Matching:** Use mechanical properties for all behavior

---

**For More Information:**
- `obstacle-system-design.md` - Design philosophy
- `OBSTACLE_SYSTEM_IMPLEMENTATION_PLAN.md` - Technical implementation
- `obstacle-templates.md` - Narrative patterns
- `poc-investigation-example.md` - Complete investigation example
