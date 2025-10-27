# Situation Spawn Templates

## Overview

Spawn rules create cascading narrative chains where completing one situation spawns child situations. This document defines common patterns for authoring spawn rules, inspired by obstacle templates but adapted for Sir Brante-style narrative progression.

---

## Understanding Templates: Two Meanings

### Design Pattern Templates (This Document)
Reusable **authoring patterns** for creating spawn rule chains. These are conceptual structures that guide content creation, similar to obstacle templates (Gauntlet, Fork, Lock, etc.).

### Prototype Instance Templates (JSON Implementation)
Fully-defined **situation definitions** in JSON that serve as prototypes for cloning. When a spawn rule executes:
1. Find template situation by `TemplateId`
2. Clone the template
3. Apply modifications (placement, requirement offsets)
4. Add to GameWorld as new situation

**Example:**
```json
{
  "id": "investigate_theft_phase1",
  "name": "Question the Witnesses",
  "systemType": "Social",
  "deckId": "investigation_social"
}
```
This situation definition becomes a **prototype template** when referenced by:
```json
{
  "SuccessSpawns": [
    {
      "TemplateId": "investigate_theft_phase1",
      "TargetPlacement": "LocationId:crime_scene"
    }
  ]
}
```

---

## Spawn Rule Pattern Catalog

### Pattern 1: Linear Progression
**Narrative Structure:** Sequential story beats, each unlocking the next

**Pattern:**
- Parent completes → spawns single child
- Child appears at same or related location
- No requirement changes (or small increases for pacing)
- Builds toward climax through accumulation

**JSON Structure:**
```json
{
  "id": "mystery_intro",
  "name": "Strange Occurrence",
  "SuccessSpawns": [
    {
      "TemplateId": "mystery_investigation",
      "TargetPlacement": "SameAsParent"
    }
  ]
}

{
  "id": "mystery_investigation",
  "name": "Investigate the Scene",
  "SuccessSpawns": [
    {
      "TemplateId": "mystery_revelation",
      "TargetPlacement": "SameAsParent"
    }
  ]
}
```

**Use Cases:**
- Quest progression (Act 1 → Act 2 → Act 3)
- Tutorial sequences (Learn A → Learn B → Learn C)
- Investigation phases (Discover → Investigate → Solve)

**Design Guidelines:**
- Each phase should feel like progression, not repetition
- Escalate stakes or revelations, not just difficulty
- Clear narrative arc from beginning to climax

---

### Pattern 2: Branching Consequences
**Narrative Structure:** Success and failure lead to different futures

**Pattern:**
- Parent completes → different spawns based on success/failure
- Success spawns represent positive consequence chain
- Failure spawns represent negative consequence chain
- Both paths viable but with different flavors

**JSON Structure:**
```json
{
  "id": "choose_faction",
  "name": "Pledge Allegiance",
  "SuccessSpawns": [
    {
      "TemplateId": "faction_a_mission1",
      "TargetPlacement": "LocationId:faction_a_base"
    }
  ],
  "FailureSpawns": [
    {
      "TemplateId": "faction_b_mission1",
      "TargetPlacement": "LocationId:faction_b_base"
    }
  ]
}
```

**Use Cases:**
- Faction choices (joining one locks out the other)
- Moral decisions (help vs betray spawns different consequences)
- Risk/reward decisions (ambitious path vs safe path)

**Design Guidelines:**
- Both branches should be interesting (not punishment vs reward)
- Failure spawns can be harder but equally valuable
- Divergent consequences should be narratively meaningful

---

### Pattern 3: Escalating Difficulty
**Narrative Structure:** Each phase harder than the last

**Pattern:**
- Parent completes → child with increased requirements
- RequirementOffsets make child more challenging
- Rewards scale with difficulty
- Tests player growth and preparation

**JSON Structure:**
```json
{
  "id": "prove_yourself_easy",
  "name": "First Trial",
  "CompoundRequirement": {
    "OrPaths": [
      {
        "NumericRequirements": [
          {"Type": "BondStrength", "Context": "mentor_npc", "Threshold": 5}
        ]
      }
    ]
  },
  "SuccessSpawns": [
    {
      "TemplateId": "prove_yourself_hard",
      "TargetPlacement": "SameAsParent",
      "RequirementOffsets": {
        "BondStrengthOffset": 5
      }
    }
  ]
}

{
  "id": "prove_yourself_hard",
  "name": "Final Trial",
  "CompoundRequirement": {
    "OrPaths": [
      {
        "NumericRequirements": [
          {"Type": "BondStrength", "Context": "mentor_npc", "Threshold": 5}
        ]
      }
    ]
  }
}
```
*Note: After spawning, "prove_yourself_hard" will require BondStrength 10 (5 base + 5 offset)*

**Use Cases:**
- Training arcs (apprentice → journeyman → master)
- Proving worth (easy task → harder task → impossible task)
- Relationship depth (acquaintance → friend → confidant)

**Design Guidelines:**
- Offset increases should feel fair (+3 to +5 reasonable)
- Player should have time between phases to prepare
- Final phase should feel like earned achievement

---

### Pattern 4: Discovery Chain
**Narrative Structure:** Finding clues reveals new locations/opportunities

**Pattern:**
- Parent completes → spawns child at NEW location
- TargetPlacement changes to revealed area
- Child represents acting on discovered information
- Creates sense of world expansion

**JSON Structure:**
```json
{
  "id": "find_clue",
  "name": "Search the Archives",
  "placementLocationId": "library",
  "SuccessSpawns": [
    {
      "TemplateId": "investigate_lead",
      "TargetPlacement": "LocationId:hidden_vault"
    }
  ]
}

{
  "id": "investigate_lead",
  "name": "Explore the Vault",
  "placementLocationId": "hidden_vault"
}
```

**Use Cases:**
- Detective work (clue → lead → culprit)
- Exploration (rumor → map → secret location)
- Information gathering (contact → informant → hideout)

**Design Guidelines:**
- New location should feel like meaningful discovery
- Parent should clearly telegraph what will be revealed
- Child situation should follow naturally from parent

---

### Pattern 5: Time-Delayed Consequences
**Narrative Structure:** Deals with the devil come due later

**Pattern:**
- Parent completes → spawns child with Conditions
- MinResolve, RequiredState, or RequiredAchievement gates child
- Child appears only when conditions met
- Creates "wait for it..." narrative tension

**JSON Structure:**
```json
{
  "id": "make_dark_pact",
  "name": "Accept the Offer",
  "SuccessSpawns": [
    {
      "TemplateId": "pact_comes_due",
      "TargetPlacement": "SameAsParent",
      "Conditions": {
        "MinResolve": 20
      }
    }
  ]
}

{
  "id": "pact_comes_due",
  "name": "The Price is Called",
  "description": "They've come to collect on your promise."
}
```
*Note: Child spawns immediately but only appears when player has 20+ Resolve*

**Use Cases:**
- Deferred consequences (debt collection, promises kept)
- Condition-based reveals (only when player is strong enough)
- State-dependent events (only when Wounded, Desperate, etc.)

**Design Guidelines:**
- Condition should make narrative sense
- Player should have warning this is coming
- Consequence should feel earned (not arbitrary)

---

### Pattern 6: Multi-Phase Mystery
**Narrative Structure:** Obligation progression with multiple acts

**Pattern:**
- Intro situation → Phase 1 situations
- All Phase 1 complete → spawns Phase 2
- Progressive revelation through structured acts
- Uses both SuccessSpawns and ObligationId system

**JSON Structure:**
```json
{
  "id": "mystery_intro",
  "name": "Accept the Case",
  "ObligationId": "missing_grain_investigation",
  "isIntroAction": true,
  "SuccessSpawns": [
    {
      "TemplateId": "phase1_witness_a",
      "TargetPlacement": "LocationId:mill"
    },
    {
      "TemplateId": "phase1_witness_b",
      "TargetPlacement": "LocationId:tavern"
    },
    {
      "TemplateId": "phase1_scene",
      "TargetPlacement": "LocationId:warehouse"
    }
  ]
}
```
*Note: Completing intro spawns all three Phase 1 situations simultaneously*

**Use Cases:**
- Investigation mysteries (accept case → gather clues → solve)
- Multi-step quests (preparation → execution → resolution)
- Faction storylines (introduction → trials → membership)

**Design Guidelines:**
- Each phase should have clear goal (gather all clues, complete all trials)
- Phases can spawn multiple situations (parallel investigation)
- Final phase should bring threads together

---

### Pattern 7: Conditional Branching
**Narrative Structure:** Different outcomes based on player state

**Pattern:**
- Parent completes → spawns different children based on Conditions
- Multiple spawn rules, each with different conditions
- Player state determines which chain activates
- Creates reactive narrative

**JSON Structure:**
```json
{
  "id": "make_choice",
  "name": "Choose Your Path",
  "SuccessSpawns": [
    {
      "TemplateId": "honorable_path",
      "TargetPlacement": "SameAsParent",
      "Conditions": {
        "MinResolve": 25
      }
    },
    {
      "TemplateId": "desperate_path",
      "TargetPlacement": "SameAsParent",
      "Conditions": {
        "RequiredState": "Desperate"
      }
    }
  ]
}
```
*Note: If player has 25+ Resolve, honorable path spawns. If player is Desperate, desperate path spawns. If neither, no spawn.*

**Use Cases:**
- Reputation-based branches (honorable vs ruthless)
- Resource-based branches (wealthy path vs poor path)
- State-based branches (healthy vs wounded recovery)

**Design Guidelines:**
- Conditions should be mutually exclusive when appropriate
- Missing conditions can mean "no spawn" (valid outcome)
- State checks should make narrative sense

---

### Pattern 8: Hub-and-Spoke
**Narrative Structure:** Central situation spawns multiple parallel options

**Pattern:**
- Hub situation completes → spawns multiple children
- All children available simultaneously
- Player chooses which to pursue (order matters)
- Creates freedom within structure

**JSON Structure:**
```json
{
  "id": "establish_base",
  "name": "Set Up Camp",
  "SuccessSpawns": [
    {
      "TemplateId": "explore_north",
      "TargetPlacement": "LocationId:northern_woods"
    },
    {
      "TemplateId": "explore_south",
      "TargetPlacement": "LocationId:southern_marsh"
    },
    {
      "TemplateId": "explore_east",
      "TargetPlacement": "LocationId:eastern_ruins"
    }
  ]
}
```

**Use Cases:**
- Exploration (base camp → multiple expedition options)
- Investigation (interview → multiple witnesses to question)
- Preparation (planning → multiple tasks to complete)

**Design Guidelines:**
- All spokes should be roughly equal value
- Order shouldn't matter (parallel, not sequential)
- Completing all spokes can spawn convergence situation

---

### Pattern 9: Cascading Unlocks
**Narrative Structure:** Completing each reveals the next layer

**Pattern:**
- Child A has no spawns initially
- Completing A reveals it HAD hidden spawns
- Each layer peels back to show deeper content
- Progressive revelation through gameplay

**JSON Structure:**
```json
{
  "id": "surface_mystery",
  "name": "Strange Occurrence",
  "SuccessSpawns": [
    {
      "TemplateId": "deeper_mystery",
      "TargetPlacement": "SameAsParent"
    }
  ]
}

{
  "id": "deeper_mystery",
  "name": "Disturbing Discovery",
  "SuccessSpawns": [
    {
      "TemplateId": "truth_revealed",
      "TargetPlacement": "SameAsParent"
    }
  ]
}
```

**Use Cases:**
- Mystery revelations (each answer raises deeper question)
- Horror escalation (each investigation makes things worse)
- Conspiracy uncovering (each thread leads to bigger truth)

**Design Guidelines:**
- Each layer should feel like progression, not padding
- Revelations should be meaningful
- Final layer should justify the journey

---

### Pattern 10: Preparation Cascade
**Narrative Structure:** Completing preparations spawns improved final challenge

**Pattern:**
- Multiple preparation situations (no spawns)
- Final preparation spawns modified boss/challenge
- RequirementOffsets make final easier based on prep work
- Rewards preparation with easier climax

**JSON Structure:**
```json
{
  "id": "prepare_intel",
  "name": "Gather Intelligence"
}

{
  "id": "prepare_equipment",
  "name": "Acquire Tools"
}

{
  "id": "ready_signal",
  "name": "Signal Readiness",
  "SuccessSpawns": [
    {
      "TemplateId": "final_confrontation",
      "TargetPlacement": "LocationId:enemy_stronghold",
      "RequirementOffsets": {
        "NumericOffset": -10
      }
    }
  ]
}
```
*Note: If player completed all preparations, final confrontation is 10 points easier*

**Use Cases:**
- Heist planning (preparation → execution)
- Boss preparation (gather intel → easier fight)
- Diplomatic preparation (build relationships → easier negotiation)

**Design Guidelines:**
- Preparations should be optional but valuable
- Final challenge should be possible without prep (just harder)
- Offsets should scale with number of preps completed

---

## Authoring Guidelines

### Creating Reusable Templates

**Good Template Characteristics:**
1. **Generic Naming** - "investigate_lead" not "investigate_martha_lead"
2. **Contextual Flexibility** - Works at multiple locations via TargetPlacement
3. **Scalable Difficulty** - RequirementOffsets can adjust challenge level
4. **Clear Purpose** - Single narrative beat, not multi-purpose

**Example: GOOD Template**
```json
{
  "id": "social_persuade_generic",
  "name": "Persuade",
  "description": "Convince them to help your cause.",
  "systemType": "Social",
  "deckId": "persuasion",
  "CompoundRequirement": {
    "OrPaths": [
      {
        "NumericRequirements": [
          {"Type": "BondStrength", "Context": "", "Threshold": 10}
        ]
      }
    ]
  }
}
```
*Note: Context is empty - will be populated by spawning parent*

**Example: BAD Template (Too Specific)**
```json
{
  "id": "convince_martha_help_find_grain",
  "name": "Ask Martha About the Grain",
  "description": "Martha knows something about the missing grain shipment."
}
```
*Too specific - can only be used once*

---

### Placement Strategies

#### SameAsParent
Use when child flows naturally from parent location.
```json
{"TargetPlacement": "SameAsParent"}
```

**Good for:**
- Sequential conversations at same location
- Escalating confrontations
- Multi-step location-based tasks

#### SpecificLocation
Use when parent reveals new location.
```json
{"TargetPlacement": "LocationId:hidden_vault"}
```

**Good for:**
- Discovery chains (find clue → explore revealed location)
- Location unlocking (hear rumor → travel to new place)
- Distributed consequences (action here → effect there)

#### SpecificNPC
Use when parent directs player to specific person.
```json
{"TargetPlacement": "NpcId:informant"}
```

**Good for:**
- NPC introductions (meet A → A directs you to B)
- Relationship chains (friend → friend of friend)
- Information networks (contact → informant → source)

---

### Requirement Offset Design

**Positive Offsets** (make harder):
```json
{
  "RequirementOffsets": {
    "BondStrengthOffset": 5,
    "ScaleOffset": 2,
    "NumericOffset": 3
  }
}
```

**Use when:**
- Escalating difficulty (each phase harder)
- Testing growth (player should have improved)
- Creating climax (final challenge most difficult)

**Guidelines:**
- +3 to +5 feels like progression
- +10+ feels like a wall (use sparingly)
- Test with actual gameplay (offsets can compound)

**Negative Offsets** (make easier):
```json
{
  "RequirementOffsets": {
    "NumericOffset": -5
  }
}
```

**Use when:**
- Rewarding preparation (multiple preps → easier final)
- Narrative logic (helping NPC makes them help you easier)
- Accessibility (providing multiple paths to same goal)

**Guidelines:**
- -5 to -10 meaningful but not trivializing
- Should still require engagement, not automatic
- Can scale with number of preparations completed

---

### Condition Design

#### MinResolve
Gates spawn until player has strategic resources.
```json
{
  "Conditions": {
    "MinResolve": 20
  }
}
```

**Use when:**
- Deferred consequences (devil's bargain comes due)
- Resource gates (need strength to pursue)
- Pacing (delay high-intensity content)

#### RequiredState
Gates spawn based on player condition.
```json
{
  "Conditions": {
    "RequiredState": "Wounded"
  }
}
```

**Use when:**
- Conditional branches (healthy path vs wounded path)
- Reactive content (consequences of being desperate)
- State-driven narrative (only when certain conditions met)

#### RequiredAchievement
Gates spawn based on player accomplishments.
```json
{
  "Conditions": {
    "RequiredAchievement": "first_blood"
  }
}
```

**Use when:**
- Reputation-based content (only for killers/heroes)
- Achievement unlocks (prove yourself first)
- Milestone content (only after major accomplishment)

---

## Complete Example: Investigation Mystery

```json
{
  "id": "missing_grain_intro",
  "name": "The Miller's Plea",
  "description": "The miller begs for help. His grain shipment vanished without a trace.",
  "systemType": "Social",
  "placementNpcId": "miller_thomas",
  "deckId": "friendly_chat",
  "ObligationId": "missing_grain_investigation",
  "isIntroAction": true,
  "InteractionType": "Instant",
  "SuccessSpawns": [
    {
      "TemplateId": "grain_witness_martha",
      "TargetPlacement": "NpcId:martha"
    },
    {
      "TemplateId": "grain_scene_warehouse",
      "TargetPlacement": "LocationId:warehouse"
    },
    {
      "TemplateId": "grain_scene_docks",
      "TargetPlacement": "LocationId:docks"
    }
  ]
}

{
  "id": "grain_witness_martha",
  "name": "Question Martha",
  "description": "Martha was working late the night the grain disappeared.",
  "systemType": "Social",
  "placementNpcId": "martha",
  "deckId": "investigation_social",
  "ProjectedBondChanges": [
    {"NpcId": "martha", "Change": 2}
  ],
  "SuccessSpawns": [
    {
      "TemplateId": "grain_phase2_followup",
      "TargetPlacement": "SameAsParent",
      "RequirementOffsets": {
        "BondStrengthOffset": 3
      }
    }
  ]
}

{
  "id": "grain_scene_warehouse",
  "name": "Search the Warehouse",
  "description": "Look for clues in the empty warehouse.",
  "systemType": "Mental",
  "placementLocationId": "warehouse",
  "deckId": "investigation_mental"
}

{
  "id": "grain_scene_docks",
  "name": "Investigate the Docks",
  "description": "Check if anyone at the docks saw anything suspicious.",
  "systemType": "Mental",
  "placementLocationId": "docks",
  "deckId": "investigation_mental"
}

{
  "id": "grain_phase2_followup",
  "name": "Martha's Secret",
  "description": "Martha is willing to reveal more, but you'll need her deeper trust.",
  "systemType": "Social",
  "placementNpcId": "martha",
  "deckId": "deep_conversation",
  "CompoundRequirement": {
    "OrPaths": [
      {
        "NumericRequirements": [
          {"Type": "BondStrength", "Context": "martha", "Threshold": 10}
        ]
      }
    ]
  }
}
```

**Narrative Flow:**
1. Accept case (intro) → spawns 3 parallel investigations (hub-and-spoke)
2. Question Martha → if successful, spawns followup requiring BondStrength 13 (10 base + 3 offset)
3. Player must complete other investigations and build relationship to unlock Martha's secret
4. Completing all Phase 1 situations would trigger Phase 2 (via obligation system)

---

## Anti-Patterns to Avoid

### Anti-Pattern 1: Infinite Loops
**Problem:** Situation spawns itself
```json
{
  "id": "repeating_work",
  "SuccessSpawns": [
    {"TemplateId": "repeating_work"}
  ]
}
```
**Fix:** Use `Repeatable: true` on situation instead, or spawn different situations

### Anti-Pattern 2: Orphan Templates
**Problem:** Template exists but no situation spawns it
```json
{
  "id": "unused_template"
}
```
**Fix:** Either add spawn rules that reference it, or delete template

### Anti-Pattern 3: Circular Dependencies
**Problem:** A spawns B, B spawns A
```json
{
  "id": "situation_a",
  "SuccessSpawns": [{"TemplateId": "situation_b"}]
}
{
  "id": "situation_b",
  "SuccessSpawns": [{"TemplateId": "situation_a"}]
}
```
**Fix:** Design linear or tree-like progression, not circular

### Anti-Pattern 4: Unreachable Content
**Problem:** Spawn conditions impossible to meet
```json
{
  "Conditions": {
    "MinResolve": 100,
    "RequiredState": "Dead"
  }
}
```
**Fix:** Test all spawn paths, ensure conditions are achievable

### Anti-Pattern 5: Spawn Spam
**Problem:** Situation spawns 10+ children simultaneously
```json
{
  "SuccessSpawns": [
    {"TemplateId": "child1"},
    {"TemplateId": "child2"},
    ...
    {"TemplateId": "child12"}
  ]
}
```
**Fix:** Limit to 2-4 spawns per completion, use phases for structure

---

## Testing Checklist

When authoring spawn rules, verify:

- [ ] Template situation exists in JSON with correct ID
- [ ] TargetPlacement makes narrative sense
- [ ] RequirementOffsets are achievable (not too high)
- [ ] Conditions are reachable (test in game)
- [ ] Spawn doesn't create circular dependency
- [ ] Child flows naturally from parent (narrative coherence)
- [ ] All spawned situations have clear purpose
- [ ] No orphan templates (unused template situations)
- [ ] Spawn count reasonable (2-4 per completion)
- [ ] Tested end-to-end in running game

---

## Summary

**Spawn Rule Patterns:**
1. Linear Progression - Sequential story beats
2. Branching Consequences - Success vs failure paths
3. Escalating Difficulty - Each phase harder
4. Discovery Chain - Reveals new locations
5. Time-Delayed Consequences - Conditional appearance
6. Multi-Phase Mystery - Obligation progression
7. Conditional Branching - State-based outcomes
8. Hub-and-Spoke - Parallel options
9. Cascading Unlocks - Progressive revelation
10. Preparation Cascade - Prep rewards easier climax

**Key Principles:**
- Templates are prototypes, patterns guide authoring
- SameAsParent for continuity, Specific for discovery
- Positive offsets for escalation, negative for reward
- Conditions for gating, not blocking
- Test all spawn paths end-to-end
