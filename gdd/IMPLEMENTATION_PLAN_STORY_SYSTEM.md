# Implementation Plan: Story System Completion

## Overview

This document captures the implementation gaps and architectural decisions needed to complete the B-Story system and resolve the RhythmPattern architectural contradiction.

**Created:** Session ending 2025-12-06
**Status:** Planning document for next session

---

## Part 1: B-Story Implementation Gaps

### 1.1 Critical Blockers

| Issue | Impact |
|-------|--------|
| **Final situation detection stubbed** | Cannot identify which situation is final in any scene; all situations treated as final |
| **No B-Story enrichment** | MainStory scenes receive parse-time enrichment; SideStory receives none |
| **No B-Story validation** | A-Story consistency validation exits early for non-MainStory categories |

### 1.2 Missing B-Story Components

| Component | A-Story Status | B-Story Status | Required Action |
|-----------|---------------|----------------|-----------------|
| **Type Discrimination** | Clear `MainStory` category | Both B-types share `SideStory` | Add property to distinguish B-Consequence vs B-Sought |
| **Parse-time Enrichment** | Enriches final choices with spawn rewards | None | Create enrichment for B-Story final situations |
| **Validation Rules** | Validates template consistency | None | Create B-Story validation rules |
| **Payoff Guarantee** | N/A | Not validated | Validate final situation has resource rewards |
| **Completion Tracking** | Tracks sequence and intensity | None | Track B-Story completion state |
| **Procedural Generation** | Full service exists | None | Consider procedural B-Story generation |

### 1.3 B-Story Type Discrimination

**Current State:** Both B-Consequence and B-Sought use shared category with no distinguishing property.

**Design Decision Required:** How to distinguish at runtime?

| Option | Pros | Cons |
|--------|------|------|
| **A) New enum values** | Clean discrimination, type-safe | Breaking change, affects all parsers |
| **B) Add type property** to template | Non-breaking, explicit | Nullable for non-B stories, extra property |
| **C) Derive from spawn context** | No schema change | Harder to validate at parse-time |

**Recommendation:** Option B - Add explicit type property (nullable, only set for SideStory).

### 1.4 B-Story Validation Requirements

Per documentation, B-Stories MUST:

1. **End with payoff** - Final situation grants resources
2. **Follow correct rhythm:**
   - B-Consequence: Narrative → Payoff (no challenges in intermediate situations)
   - B-Sought: Challenge → Payoff (costs/tests in intermediate situations)
3. **Have proper placement** - B-Consequence uses categorical matching for continuity

**Missing Validation Rules:**

| Rule | Description |
|------|-------------|
| BSTORY_001 | SideStory template missing final situation resource reward |
| BSTORY_002 | B-Consequence intermediate situation has challenge (costs/tests) |
| BSTORY_003 | B-Sought missing challenge structure in intermediate situations |
| BSTORY_004 | B-Consequence missing narrative continuity placement filters |

---

## Part 2: RhythmPattern Architectural Contradiction

### 2.1 The Contradiction

| Source | Position |
|--------|----------|
| **arc42 Documentation** | "Structure is determined by situation position within the scene, not by an explicit RhythmPattern property" |
| **Code Reality** | RhythmPattern enum exists, is parsed from JSON, and actively branches choice generation |

### 2.2 RhythmPattern Inventory

**Where RhythmPattern exists:**
- Enum definition with Building, Crisis, Mixed values
- Property on SceneTemplate
- Property in generation context
- Property in spawn reward context
- Fields in DTOs
- Fields in JSON content
- Active branching in choice generation catalogs
- Active branching in procedural generation service
- Active branching in reward computation

### 2.3 Resolution Options

| Option | Description | Effort | Risk |
|--------|-------------|--------|------|
| **A) Document current reality** | Update arc42 to reflect that RhythmPattern IS used | Low | Documentation doesn't match original intent |
| **B) Implement position-based structure** | Remove RhythmPattern, derive from situation position | High | Major refactor, breaks existing content |
| **C) Hybrid approach** | RhythmPattern for A-Story only, derive for B/C | Medium | Complexity, two systems |

**Recommendation:** Depends on design intent. If position-based is the target architecture, create a phased migration plan. If RhythmPattern is the intended design, update documentation.

### 2.4 Position-Based Alternative (If Chosen)

If we remove RhythmPattern and use position:

**For A-Story scenes:**
- Situations 1 to N-1: Generate Building choices (stat grants)
- Situation N (final): Generate Crisis choices (stat-gated tests)

**For B-Consequence scenes:**
- Situations 1 to N-1: Generate Narrative choices (no costs)
- Situation N (final): Generate Payoff choices (resource grants)

**For B-Sought scenes:**
- Situations 1 to N-1: Generate Challenge choices (costs/tests)
- Situation N (final): Generate Payoff choices (resource grants)

**Final situation detection:** Check if situation index equals template situation count minus one.

---

## Part 3: Implementation Roadmap

### Phase 1: Fix Critical Blockers (Required First)

1. **Implement final situation detection** - Replace stub with actual position detection
2. **Add B-Story type discrimination** - Property to distinguish B-Consequence from B-Sought
3. **Create B-Story enrichment path** - Ensure final situations have payoff

### Phase 2: B-Story Validation

4. **Add B-Story validation rules** - BSTORY_001 through BSTORY_004
5. **Validate payoff guarantee** - Final situation must have resource rewards
6. **Validate rhythm compliance** - B-Consequence has no challenges, B-Sought has challenges

### Phase 3: Resolve RhythmPattern

7. **Decision point:** Keep RhythmPattern or migrate to position-based?
8. **If keeping:** Update arc42 documentation to match reality
9. **If removing:** Phased migration (JSON, DTOs, parsers, catalogs, services)

### Phase 4: B-Story Completion

10. **B-Story completion tracking** - State updates when B-Story completes
11. **Consider procedural B-Story generation** - Templates for B-Consequence and B-Sought
12. **Integration tests** - Full B-Story lifecycle testing

---

## Part 4: Design Questions to Resolve

### Q1: B-Story Type Discrimination

**How should we distinguish B-Consequence from B-Sought at runtime?**

Options:
- A) New type enum property on template
- B) Separate enum values in story category
- C) Derive from spawn context (who spawned this scene)

### Q2: RhythmPattern Direction

**Is the target architecture position-based or RhythmPattern-based?**

- If position-based: Plan migration away from RhythmPattern
- If RhythmPattern-based: Update documentation to match code

### Q3: B-Story Procedural Generation

**Do we need procedural B-Story generation?**

- B-Consequence: Could use templates with placement filters for continuity
- B-Sought: Could use job board templates with categorical scaling
- Or: Hand-author B-Story templates only

### Q4: B-Consequence Challenge Definition

**What exactly constitutes a "challenge" that B-Consequence must NOT have?**

- Resource costs (Time, Coins, Stamina)?
- Stat tests (Insight check, Rapport check)?
- Both?
- Or: Any requirement with non-zero costs?

---

## Part 5: Components to Modify

### Validation & Detection
- Scene template validator - Fix final situation detection, add B-Story rules

### Enrichment
- Scene template parser - Add B-Story enrichment path

### Types
- Story-related enums - Consider B-Story type discrimination
- Scene template entity - Add type property if needed
- Scene template DTO - Add field for type

### Services
- Situation completion handler - Add B-Story completion tracking

### Tests
- New B-Story validation tests
- Update existing scene template tests

---

## Summary

**Documentation is now consistent** - All GDD and arc42 documents correctly distinguish:
- A-Story: Building → Crisis
- B-Consequence: Narrative → Payoff (NO challenges)
- B-Sought: Challenge → Payoff

**Implementation gaps identified:**
- 3 critical blockers (final situation stub, no enrichment, no validation)
- 6 missing components (enum, enrichment, validation, payoff, tracking, generation)
- 1 architectural contradiction (RhythmPattern in code vs position-based in docs)

**Next session should:**
1. Resolve design questions (Q1-Q4 above)
2. Implement Phase 1 critical blockers
3. Decide RhythmPattern direction
