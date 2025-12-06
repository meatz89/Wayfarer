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
| **Structure-based Detection** | Position determines Building vs Crisis | Not implemented | Detect B-Consequence vs B-Sought from choice requirements |
| **Parse-time Enrichment** | Enriches final choices with spawn rewards | None | Create enrichment for B-Story final situations |
| **Validation Rules** | Validates template consistency | None | Create B-Story validation rules |
| **Payoff Guarantee** | N/A | Not validated | Validate final situation has resource rewards |
| **Completion Tracking** | Tracks sequence and intensity | None | Track B-Story completion state |
| **Procedural Generation** | Full service exists | None | Consider procedural B-Story generation |

### 1.3 B-Story Type Detection (Structure-Based)

**Design Decision (RESOLVED):** A/B/C are meta game design definitions. In code, distinguish by STRUCTURE:

| B-Story Type | Intermediate Situation Choices | Detection |
|--------------|-------------------------------|-----------|
| **B-Consequence** | NO requirements | All choices have null/empty Requirement |
| **B-Sought** | HAS requirements | At least one choice has non-null Requirement |

No new enum or property needed. Validation detects the pattern from template structure.

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
2. **Create B-Story enrichment path** - Ensure final situations have payoff (resource rewards)

### Phase 2: B-Story Validation

3. **Add B-Story validation rules** - BSTORY_001 through BSTORY_004
4. **Validate payoff guarantee** - Final situation must have resource rewards
5. **Validate rhythm compliance** - B-Consequence has no choice requirements, B-Sought has requirements

### Phase 3: RhythmPattern Migration (DECIDED: Remove It)

6. **Update choice generation** - Use position + story category instead of RhythmPattern
7. **Remove from GenerationContext** - Delete Rhythm property
8. **Remove from SceneTemplate** - Delete RhythmPattern property
9. **Remove from DTOs** - Delete RhythmPattern fields
10. **Remove from JSON** - Delete rhythmPattern from tutorial scenes
11. **Delete enum** - Remove RhythmPattern.cs

### Phase 4: B-Story Completion

10. **B-Story completion tracking** - State updates when B-Story completes
11. **Consider procedural B-Story generation** - Templates for B-Consequence and B-Sought
12. **Integration tests** - Full B-Story lifecycle testing

---

## Part 4: Design Decisions (RESOLVED)

### Q1: B-Story Type Discrimination ✓ DECIDED

**Decision:** A/B/C story categories are meta game design definitions. In code, use different STRUCTURES - no new enum needed.

**Implication:** B-Consequence vs B-Sought is distinguished by template structure:
- B-Consequence templates: Intermediate situations have choices with NO requirements
- B-Sought templates: Intermediate situations have choices WITH requirements

Validation rules detect the pattern from structure, not from an explicit type property.

### Q2: RhythmPattern Direction ✓ DECIDED

**Decision:** Plan migration to position-based structure. Remove RhythmPattern.

**Migration Strategy:**
1. Implement position-based detection (IsFinalSituation)
2. Update choice generation to use position + story category instead of RhythmPattern
3. Remove RhythmPattern from GenerationContext
4. Remove RhythmPattern from SceneTemplate
5. Remove RhythmPattern from DTOs
6. Remove rhythmPattern from JSON content
7. Delete RhythmPattern enum

### Q3: B-Story Procedural Generation (OPEN)

**Still to decide:** Do we need procedural B-Story generation?

- B-Consequence: Could use templates with placement filters for continuity
- B-Sought: Could use job board templates with categorical scaling
- Or: Hand-author B-Story templates only

### Q4: B-Consequence Challenge Definition ✓ DECIDED

**Decision:** A "challenge" is defined by **choices having requirements**.

**B-Consequence intermediate situations:** Choices must have NO requirements (no stat tests, no resource costs). Player already proved themselves in A-story Crisis.

**B-Sought intermediate situations:** Choices SHOULD have requirements (stat tests, resource costs). Player is working for income.

**Validation Rule:** Check `ChoiceTemplate.Requirement` - if any non-null requirement exists in intermediate B-Consequence situation, validation fails.

---

## Part 5: Components to Modify

### Validation & Detection
- Scene template validator - Fix final situation detection, add B-Story rules
- Add structure-based B-Story type detection (check choice requirements)

### Enrichment
- Scene template parser - Add B-Story enrichment path for final situations

### RhythmPattern Migration (Phase 3)
- Choice generation catalogs - Use position + story category
- Generation context - Remove Rhythm property
- Scene template entity - Remove RhythmPattern property
- Scene template DTO - Remove RhythmPattern field
- Spawn reward DTO - Remove RhythmPatternContext field
- Tutorial JSON - Remove rhythmPattern fields
- RhythmPattern enum - DELETE

### Services
- Situation completion handler - Add B-Story completion tracking

### Tests
- New B-Story validation tests
- Update existing scene template tests
- Remove/update RhythmPattern compliance tests

---

## Summary

**Documentation is now consistent** - All GDD and arc42 documents correctly distinguish:
- A-Story: Building → Crisis
- B-Consequence: Narrative → Payoff (NO choice requirements)
- B-Sought: Challenge → Payoff (HAS choice requirements)

**Design decisions resolved:**
- Q1: B-Story types distinguished by STRUCTURE (choice requirements), not enum
- Q2: MIGRATE away from RhythmPattern to position-based
- Q4: "Challenge" = choices with requirements

**Implementation gaps:**
- 2 critical blockers (final situation stub, no B-Story enrichment)
- 5 missing components (structure detection, validation, payoff, tracking, generation)

**Next session should:**
1. Implement Phase 1: Fix IsFinalSituation, create B-Story enrichment
2. Implement Phase 2: B-Story validation rules
3. Begin Phase 3: RhythmPattern migration
