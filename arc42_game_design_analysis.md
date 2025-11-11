# Arc42 Game Design Content Analysis

## Executive Summary

The arc42 documentation contains significant **GAME DESIGN** content that should be separated from pure architectural documentation. The confusion stems from: (1) game design GOALS driving architectural DECISIONS, (2) architectural patterns implementing game design PRINCIPLES, and (3) quality requirements validating both architecture AND player experience.

**Key Finding:** Approximately 30-40% of current arc42 content is game design that should either MOVE to game design docs or be DUPLICATED with architectural implications staying in arc42.

---

## SECTION 1: Introduction and Goals (01_introduction_and_goals.md)

### FINDING 1.1: Core Game Loop (Lines 9-40)

**Location:** Section 1.1 "Requirements Overview" → "Central Game Loop"

**Content Summary:**
- Three-tier loop hierarchy (SHORT/MEDIUM/LONG loops)
- 10-30 second encounter cycles
- 5-15 minute delivery job flow
- Hours-long progression cycles
- Resource pressure creating impossible choices

**Decision:** **MOVE to game design docs**

**Rationale:** This describes WHAT the player DOES and WHY it's fun, not HOW the system is built. The three-tier loop is a GAMEPLAY design pattern, not an architectural pattern. Arc42 should focus on system structure, not player activity flow.

**Recommendation:**
- Create `CORE_GAMEPLAY_LOOPS.md` in game design docs
- Replace arc42 section with: "The game implements three nested gameplay loops (see CORE_GAMEPLAY_LOOPS.md). Architecture supports this via Scene→Situation→Choice flow with resource state management."

---

### FINDING 1.2: Infinite A-Story (Frieren Principle) (Lines 42-52)

**Location:** Section 1.1 "Requirements Overview" → "The Eternal Journey"

**Content Summary:**
- Never-ending main storyline philosophy
- Narrative coherence rationale (travel as eternal state)
- Mechanical elegance arguments
- Player agency philosophy
- No arbitrary ending point

**Decision:** **MOVE to game design docs**

**Rationale:** This is NARRATIVE DESIGN philosophy explaining WHY the infinite story creates fun/engagement. The fact that it WORKS is player experience. The fact that it EXISTS requires architectural support (procedural generation), but the DESIGN DECISION belongs in game design.

**Recommendation:**
- Create `INFINITE_A_STORY_DESIGN.md` in game design docs (or enhance existing INFINITE_A_STORY_ARCHITECTURE.md with design rationale section)
- Arc42 should only reference: "System supports infinite procedural storyline generation (see ADR-001 and INFINITE_A_STORY_DESIGN.md for design rationale)"

---

### FINDING 1.3: Perfect Information Principle (Lines 54-63)

**Location:** Section 1.1 "Requirements Overview" → "Perfect Information Principle"

**Content Summary:**
- Players see exact costs/rewards before commitment
- Resource pressure philosophy (delivery earnings barely cover survival)
- Optimization skill provides advantage
- Every choice matters mechanically

**Decision:** **DUPLICATE (appears in both arc42 and game design docs)**

**Rationale:** Perfect Information is BOTH a player experience goal (game design) AND an architectural requirement (all costs must be calculable at UI layer). The PLAYER EXPERIENCE aspect belongs in game design. The IMPLEMENTATION aspect (how UI calculates and displays costs) belongs in arc42.

**Recommendation:**
- Game design doc: `PERFECT_INFORMATION_PRINCIPLE.md` - focuses on WHY (player empowerment, strategic planning, no gotchas)
- Arc42 doc: Keep section 1.2 quality goal but focus on WHAT this means architecturally (resource state always queryable, deterministic calculations, UI transparency requirements)

---

### FINDING 1.4: Quality Goal - Strategic Depth (Lines 91-97)

**Location:** Section 1.2 "Quality Goals" → TIER 2 item 3

**Content Summary:**
- Impossible choices through resource scarcity
- All systems compete for same scarce resources
- No boolean gates for progression
- Specialization creates identity AND vulnerability

**Decision:** **DUPLICATE with different perspectives**

**Rationale:** "Strategic Depth" is a GAME DESIGN goal (creating fun through meaningful choices). It becomes an ARCHITECTURAL CONSTRAINT (systems must share resources, no boolean unlocks). Game design explains WHY it's fun. Arc42 explains HOW architecture enforces it.

**Recommendation:**
- Game design doc: `RESOURCE_SCARCITY_DESIGN.md` - focuses on player experience, trade-offs, specialization creating identity
- Arc42 doc: Keep as quality goal but emphasize architectural enforcement: "Systems connect via typed rewards, shared resource pools, arithmetic comparisons not boolean flags"

---

## SECTION 2: Constraints (02_constraints.md)

### FINDING 2.1: No Game Design Content

**Decision:** Section 2 is appropriately architectural.

**Rationale:** Technical constraints, type restrictions, lambda rules, package cohesion, code quality standards are all implementation concerns, not game design.

---

## SECTION 3: Context and Scope (03_context_and_scope.md)

### FINDING 3.1: Business Context - Three-Tier Loop Hierarchy (Lines 15-38)

**Location:** Section 3.1 "Business Context" → "Gameplay Scope - Three-Tier Loop Hierarchy"

**Content Summary:**
- Same as Finding 1.1 (three-tier loop)
- SHORT LOOP: situation-choice cycles
- MEDIUM LOOP: delivery jobs and survival
- LONG LOOP: accumulation and progression

**Decision:** **MOVE to game design docs, REFERENCE from arc42**

**Rationale:** Duplicate of Finding 1.1. This is GAMEPLAY flow, not system context. "Business context" in arc42 should describe WHAT the system does for stakeholders, not HOW players experience gameplay.

**Recommendation:**
- Remove detailed loop descriptions from section 3.1
- Replace with: "Wayfarer provides a resource management gameplay experience with nested decision loops. See CORE_GAMEPLAY_LOOPS.md for gameplay design. Architecture supports this via persistent GameWorld state and Scene→Situation→Choice progression."

---

### FINDING 3.2: Core Value Proposition (Lines 9-13)

**Location:** Section 3.1 "Business Context" → "Core Value Proposition"

**Content Summary:**
- "Strategic Depth Through Impossible Choices"
- "Infinite Content Without Resolution"

**Decision:** **REFERENCE from arc42 to game design docs**

**Rationale:** Value proposition is marketing/design, not architecture. Arc42 "business context" should describe system purpose, not player value proposition.

**Recommendation:**
- Create `VALUE_PROPOSITION.md` or integrate into `DESIGN_PHILOSOPHY.md`
- Arc42 section 3.1 should say: "Wayfarer implements a resource-constrained decision-making system supporting infinite procedural content. See DESIGN_PHILOSOPHY.md for design rationale and player value proposition."

---

## SECTION 4: Solution Strategy (04_solution_strategy.md)

### FINDING 4.1: Two-Layer Architecture - Player Experience Goals (Lines 7-13)

**Location:** Section 4.1 "Two-Layer Architecture Decision" → "Problem"

**Content Summary:**
- "Players need to make informed strategic decisions (WHAT to attempt) before experiencing tactical complexity (HOW to execute)"
- "Mixed concerns create confusion - players can't calculate risk before committing"

**Decision:** **DUPLICATE with different emphasis**

**Rationale:** The PROBLEM statement mixes player experience (game design) with architectural solution. Game design explains WHY separation matters for fun. Arc42 explains HOW architecture implements separation.

**Recommendation:**
- Game design doc: `TWO_LAYER_PLAYER_EXPERIENCE.md` - focuses on player decision-making, risk calculation, strategic vs tactical thinking
- Arc42 doc: Keep but refocus: "To support perfect information at strategic layer while preserving tactical complexity, architecture separates Strategic Layer (Scene/Situation/Choice) from Tactical Layer (Challenge Sessions) with explicit bridge pattern."

---

## SECTION 8: Crosscutting Concepts (08_crosscutting_concepts.md)

### FINDING 8.1: Design Principle 4 - Inter-Systemic Rules (Lines 506-526)

**Location:** Section 8.3.2 "Core Design Principles" → "Principle 4"

**Content Summary:**
- "Systems connect via typed rewards, not boolean flag evaluation"
- FORBIDDEN: "If completed A, unlock B"
- REQUIRED: "Shared resource competition, opportunity costs"

**Decision:** **DUPLICATE with different perspectives**

**Rationale:** This is BOTH a game design principle (how to create strategic depth) AND an architectural pattern (how systems connect). The "no boolean gates" rule is a DESIGN DECISION driven by gameplay goals.

**Recommendation:**
- Game design doc: `PROGRESSION_DESIGN.md` - focuses on WHY boolean gates are bad for fun, resource arithmetic creates depth
- Arc42 doc: Keep but emphasize implementation: "Systems connect via typed rewards applied at completion boundaries. No continuous boolean flag evaluation. Scene completion applies SceneReward (LocationsToUnlock, CoinsToGrant, etc.)."

---

### FINDING 8.2: Design Principle 6 - Resource Scarcity (Lines 546-556)

**Location:** Section 8.3.2 "Core Design Principles" → "Principle 6"

**Content Summary:**
- "Shared resources (Time, Focus, Stamina, Health) force player to accept one cost to avoid another"
- Resource types: Shared vs System-Specific
- Test: "Can player pursue all options without trade-offs?"

**Decision:** **MOVE to game design docs**

**Rationale:** This is PURE GAME DESIGN. It describes HOW to create strategic depth through resource design. Not an architectural pattern.

**Recommendation:**
- Move entirely to `RESOURCE_SCARCITY_DESIGN.md`
- Arc42 should only reference: "Game design requires shared resource pools forcing trade-offs (see RESOURCE_SCARCITY_DESIGN.md). Architecture implements via ResourceFacade managing Time, Focus, Stamina, Health, Coins with all systems competing for same pools."

---

### FINDING 8.3: Design Principle 10 - Perfect Information (Lines 603-620)

**Location:** Section 8.3.2 "Core Design Principles" → "Principle 10"

**Content Summary:**
- "Strategic layer visible (costs, rewards, requirements)"
- "Tactical layer hidden (card draw, challenge flow)"
- Test: "Can player decide WHETHER to attempt before entering?"

**Decision:** **DUPLICATE with different perspectives**

**Rationale:** Same as Finding 1.3. Perfect Information is BOTH a player experience goal (game design) and an architectural requirement (UI must show all costs).

**Recommendation:**
- Game design doc: Focus on player empowerment, strategic planning, transparency philosophy
- Arc42 doc: Focus on implementation - how UI queries GameWorld, how costs are calculated, layer separation enforcement

---

### FINDING 8.4: Four-Choice Archetype (Lines 693-707)

**Location:** Section 8.4.2 "Domain Concepts" → "Four-Choice Archetype (Guaranteed Progression)"

**Content Summary:**
- Every A-story situation has 4 choice types
- Stat-Gated (Optimal), Money-Gated (Reliable), Challenge (Risky), Guaranteed (Patient)
- Purpose: Player chooses HOW to progress, not IF
- Ensures no soft-locks

**Decision:** **MOVE to game design docs**

**Rationale:** This is PURE CHOICE DESIGN. It's a mechanical pattern for creating player choices that ensure progression. This is GAME DESIGN, not architecture. The fact that the system VALIDATES this pattern (parse-time checks) is architectural, but the PATTERN ITSELF is design.

**Recommendation:**
- Create `CHOICE_DESIGN_PATTERNS.md` with comprehensive explanation of four-choice archetype
- Arc42 should only reference: "Content validation ensures A-story situations follow four-choice archetype pattern (see CHOICE_DESIGN_PATTERNS.md for design rationale). Parser validates at least one zero-requirement choice exists per A-story situation."

---

### FINDING 8.5: Two-Layer Architecture (Lines 673-691)

**Location:** Section 8.4.1 "Domain Concepts" → "Two-Layer Architecture"

**Content Summary:**
- Strategic Layer: Perfect information, state machine, persistent
- Tactical Layer: Hidden complexity, victory thresholds, temporary
- Bridge: ChoiceTemplate.ActionType

**Decision:** **KEEP in arc42, REFERENCE from game design docs**

**Rationale:** This is appropriately architectural (describes system structure). BUT game design docs should explain WHY this separation matters for player experience.

**Recommendation:**
- Keep in arc42 as-is (domain concept)
- Game design doc `TWO_LAYER_PLAYER_EXPERIENCE.md` should explain player perspective and link to arc42 for implementation

---

## SECTION 9: Architecture Decisions (09_architecture_decisions.md)

### FINDING 9.1: ADR-001 Infinite A-Story (Lines 9-87)

**Location:** ADR-001 complete

**Content Summary:**
- Never-ending main storyline decision
- Two-phase design (authored tutorial → procedural continuation)
- Narrative pattern: "You travel, arrive, meet people..."
- Consequences: No closure, infinite replayability, player agency

**Decision:** **CONTROVERSIAL - DUPLICATE with different formats**

**Rationale:** This ADR documents an ARCHITECTURAL DECISION (how to implement infinite content) driven by GAME DESIGN GOALS (infinite engagement, no arbitrary ending). The ADR FORMAT (Context, Decision, Consequences, Alternatives) is appropriate for arc42. But the CONTENT is 80% game design rationale.

**Recommendation:**
- Arc42: Keep ADR-001 but focus on ARCHITECTURAL aspects:
  - Context: System must support infinite content generation
  - Decision: Two-phase pipeline (authored → procedural) with scene archetype catalog
  - Consequences: Requires robust validation, no ceiling on difficulty/rewards
  - Alternatives: Traditional endings, repeatable loops, branching paths
- Game design doc: Create `INFINITE_A_STORY_DESIGN.md` with:
  - WHY infinite is better for engagement
  - Narrative coherence philosophy (travel as eternal state)
  - Player agency and pacing control
  - Comparison to traditional narrative structures

---

### FINDING 9.2: ADR-002 Resource Arithmetic (Lines 89-201)

**Location:** ADR-002 complete

**Content Summary:**
- Replace boolean gates with resource arithmetic
- Five Resource Layers (Personal Stats, Per-Person Relationships, Permanent Resources, Time, Ephemeral Context)
- Perfect information display with numeric gaps
- Consequences: Strategic depth, opportunity cost, specialization

**Decision:** **MOVE to game design docs, create architectural ADR**

**Rationale:** This is 90% GAME DESIGN (progression system design, resource economy). Only 10% is architectural (implementation pattern). The ADR documents a GAME DESIGN DECISION, not an architectural decision.

**Recommendation:**
- Game design doc: `RESOURCE_ARITHMETIC_DESIGN.md` with full content of current ADR-002
  - WHY boolean gates are shallow
  - HOW resource arithmetic creates depth
  - Five resource layers explained
  - Impossible choices philosophy
- Arc42: Create new ADR-002 "Requirement Inversion Pattern" focusing on IMPLEMENTATION:
  - Context: System must display requirements before content access
  - Decision: Entities spawn into world immediately, requirements filter visibility
  - Implementation: SpawnConditions.IsEligible(), arithmetic comparisons, UI shows gaps
  - Consequences: All content queryable, perfect information possible, parse-time validation

---

### FINDING 9.3: ADR-003 Two-Layer Architecture (Lines 203-328)

**Location:** ADR-003 complete

**Content Summary:**
- Strategic vs Tactical layer separation
- Player experience goal: informed strategic decisions before tactical commitment
- Bridge pattern via ActionType
- Consequences: Clear separation, one-way flow, layer purity

**Decision:** **KEEP in arc42, REFERENCE from game design docs**

**Rationale:** This is appropriately architectural. Lines 209-213 mention player experience, but the bulk is about SYSTEM STRUCTURE. The player experience rationale is brief and supports the architectural decision.

**Recommendation:**
- Keep ADR-003 in arc42 as-is (well-balanced between rationale and architecture)
- Game design doc `TWO_LAYER_PLAYER_EXPERIENCE.md` should reference ADR-003 for implementation details

---

### FINDING 9.4: ADR-006 Principle Priority Hierarchy (Lines 539-658)

**Location:** ADR-006 complete

**Content Summary:**
- Three-tier priority: TIER 1 (No Soft-Locks, Single Source of Truth), TIER 2 (Playability, Perfect Information, Resource Scarcity), TIER 3 (Elegance, Verisimilitude)
- Conflict resolution framework
- Decision examples

**Decision:** **DUPLICATE with different emphasis**

**Rationale:** The priority hierarchy governs BOTH game design decisions AND architectural decisions. "No Soft-Locks" is a GAME DESIGN requirement. "Single Source of Truth" is an ARCHITECTURAL requirement. The framework is valuable for both docs.

**Recommendation:**
- Game design doc: `DESIGN_PRINCIPLE_HIERARCHY.md` with TIER 1-2 emphasizing player experience
  - TIER 1: No Soft-Locks (always forward progress for player)
  - TIER 2: Perfect Information (player empowerment), Resource Scarcity (strategic depth), Playability (testable content)
  - Examples of game design conflicts (Perfect Info vs Surprise, Resource Scarcity vs No Soft-Locks)
- Arc42: Keep ADR-006 with TIER 1 emphasizing system integrity
  - TIER 1: Single Source of Truth (data integrity), No Soft-Locks (system validation)
  - TIER 2: Playability (reachable code), Perfect Information (calculable UI), Resource Scarcity (typed rewards)
  - Examples of architectural conflicts (HIGHLANDER vs Performance, Elegance vs Complexity)

---

## SECTION 10: Quality Requirements (10_quality_requirements.md)

### FINDING 10.1: QS-001 No Soft-Locks (Lines 32-90)

**Location:** Quality Scenario 001

**Content Summary:**
- Zero-resource A-story progression validation
- Challenge failure still progresses
- Infinite generation never soft-locks
- Four-choice archetype enforcement

**Decision:** **KEEP in arc42 (validation scenarios appropriate)**

**Rationale:** While "No Soft-Locks" is a game design goal, the QUALITY SCENARIOS here are about SYSTEM VALIDATION (parsing, automated tests, validation rules). This is appropriate for arc42.

**Recommendation:**
- Keep in arc42 as-is (focuses on testing/validation)
- Game design doc should explain DESIGN PATTERNS that prevent soft-locks (four-choice archetype, guaranteed fallbacks, failure-progresses-differently)

---

### FINDING 10.2: QS-004 Strategic Depth (Lines 204-265)

**Location:** Quality Scenario 004

**Content Summary:**
- Competing time blocks scenario (delivery vs investigation vs rest vs relationship)
- Stat specialization pressure scenario (can't max all stats)
- Resource competition across loops scenario (coins for lodging vs equipment vs travel)

**Decision:** **MOVE to game design docs**

**Rationale:** These are GAMEPLAY VALIDATION SCENARIOS testing if the game is FUN, not if the architecture is CORRECT. "Average player completes 40-60% of available daily actions" is a GAME DESIGN metric (measuring strategic pressure), not an architectural metric.

**Recommendation:**
- Game design doc: Create `STRATEGIC_DEPTH_VALIDATION.md` with gameplay scenarios
  - How to test if resource pressure creates meaningful choices
  - Telemetry metrics for strategic engagement (completion rates, stat variance)
  - Playtesting protocols for impossible choices
- Arc42: Remove QS-004 or replace with architectural scenario:
  - "System correctly applies resource costs across multiple systems"
  - "Resource state remains consistent during concurrent operations"
  - "UI displays accurate resource calculations"

---

### FINDING 10.3: QS-005 Perfect Information (Lines 267-328)

**Location:** Quality Scenario 005

**Content Summary:**
- Choice cost visibility before selection
- Reward transparency (no mystery boxes)
- Challenge entry transparency (both success/failure outcomes shown)

**Decision:** **KEEP in arc42 (UI requirements appropriate)**

**Rationale:** While Perfect Information is a game design goal, the quality scenarios here focus on UI IMPLEMENTATION (what must be displayed, where, when). This is architectural validation.

**Recommendation:**
- Keep in arc42 as-is (focuses on UI implementation requirements)
- Game design doc should explain WHY perfect information creates better player experience

---

## FINDINGS SUMMARY TABLE

| Section | Lines | Content | Decision | Priority |
|---------|-------|---------|----------|----------|
| 01 - Core Game Loop | 9-40 | Three-tier loop hierarchy | **MOVE** to game design | HIGH |
| 01 - Infinite A-Story | 42-52 | Frieren Principle philosophy | **MOVE** to game design | HIGH |
| 01 - Perfect Information | 54-63 | Resource pressure philosophy | **DUPLICATE** | MEDIUM |
| 01 - Strategic Depth Goal | 91-97 | Impossible choices via scarcity | **DUPLICATE** | MEDIUM |
| 03 - Three-Tier Loop | 15-38 | Duplicate of 01 content | **MOVE** to game design | HIGH |
| 03 - Value Proposition | 9-13 | Player value statement | **REFERENCE** | LOW |
| 04 - Two-Layer Problem | 7-13 | Player experience rationale | **DUPLICATE** | MEDIUM |
| 08 - Principle 4 | 506-526 | Inter-systemic rules design | **DUPLICATE** | MEDIUM |
| 08 - Principle 6 | 546-556 | Resource scarcity principle | **MOVE** to game design | HIGH |
| 08 - Principle 10 | 603-620 | Perfect information principle | **DUPLICATE** | MEDIUM |
| 08 - Four-Choice Archetype | 693-707 | Choice design pattern | **MOVE** to game design | HIGH |
| 08 - Two-Layer Domain | 673-691 | Strategic/Tactical separation | **KEEP** in arc42 | N/A |
| 09 - ADR-001 | 9-87 | Infinite A-Story decision | **DUPLICATE** (different format) | HIGH |
| 09 - ADR-002 | 89-201 | Resource arithmetic decision | **MOVE** to game design | HIGH |
| 09 - ADR-006 | 539-658 | Principle priority hierarchy | **DUPLICATE** | MEDIUM |
| 10 - QS-004 | 204-265 | Strategic depth validation | **MOVE** to game design | MEDIUM |

---

## RECOMMENDED NEW GAME DESIGN DOCUMENTS

### HIGH PRIORITY (Create First)

1. **CORE_GAMEPLAY_LOOPS.md**
   - Three-tier loop hierarchy detailed explanation
   - SHORT LOOP: 10-30s encounter-choice cycles
   - MEDIUM LOOP: 5-15min delivery job flow
   - LONG LOOP: Hours-long progression arc
   - How loops nest and interact
   - Resource flow between loops

2. **INFINITE_A_STORY_DESIGN.md**
   - Narrative design philosophy (Frieren Principle)
   - WHY infinite is better than traditional endings
   - Two-phase structure (authored → procedural)
   - Player agency and pacing control
   - Scope escalation over time (local → continental → cosmic)

3. **RESOURCE_SCARCITY_DESIGN.md**
   - Five resource layers explained
   - Shared resources (Time, Focus, Stamina, Health, Coins)
   - How scarcity creates impossible choices
   - Specialization and character identity
   - Economy balance philosophy

4. **CHOICE_DESIGN_PATTERNS.md**
   - Four-choice archetype comprehensive guide
   - Stat-Gated (optimal path)
   - Money-Gated (reliable path)
   - Challenge (risky path)
   - Guaranteed (patient path)
   - How archetype prevents soft-locks
   - Authoring guidelines for content creators

5. **RESOURCE_ARITHMETIC_DESIGN.md**
   - Full content from current ADR-002
   - WHY boolean gates fail (Cookie Clicker critique)
   - HOW arithmetic creates strategic depth
   - Perfect information display philosophy
   - Numeric gap visibility ("need 2 more Valor")

### MEDIUM PRIORITY (Create Second)

6. **PERFECT_INFORMATION_PRINCIPLE.md**
   - Player empowerment philosophy
   - Strategic calculation before commitment
   - UI transparency requirements (design perspective)
   - Comparison to hidden-consequence games
   - Player trust and respect

7. **TWO_LAYER_PLAYER_EXPERIENCE.md**
   - Strategic thinking vs Tactical execution
   - Risk calculation at strategic layer
   - Tactical surprise and emergent gameplay
   - How separation improves player experience

8. **PROGRESSION_DESIGN.md**
   - No boolean gates philosophy
   - Typed rewards as system boundaries
   - Opportunity cost and trade-offs
   - Stat specialization encouraging builds

9. **DESIGN_PRINCIPLE_HIERARCHY.md**
   - TIER 1: No Soft-Locks (player perspective)
   - TIER 2: Perfect Information, Resource Scarcity, Playability
   - TIER 3: Verisimilitude, Elegance
   - Conflict resolution examples
   - Game design vs architecture priorities

### LOW PRIORITY (Nice to Have)

10. **STRATEGIC_DEPTH_VALIDATION.md**
    - Gameplay testing scenarios
    - Telemetry metrics (completion rates, stat variance)
    - Playtesting protocols
    - How to validate "fun" and strategic pressure

11. **VALUE_PROPOSITION.md** (or integrate into DESIGN_PHILOSOPHY.md)
    - Core value proposition explanation
    - Target audience and playstyle
    - Competitive differentiation
    - Design pillars

---

## ARC42 MODIFICATIONS REQUIRED

### Section 01 - Introduction and Goals

**REMOVE:**
- Lines 13-40: Detailed three-tier loop (replace with reference)
- Lines 42-52: Infinite A-Story philosophy (replace with reference)
- Lines 54-63: Perfect Information detailed philosophy (keep brief architectural implication)

**REPLACE WITH:**
- "Wayfarer implements three nested gameplay loops (SHORT: 10-30s encounter cycles, MEDIUM: 5-15min delivery cycles, LONG: hours-long progression). See CORE_GAMEPLAY_LOOPS.md for gameplay design. Architecture supports this via Scene→Situation→Choice flow with resource state management."
- "The game supports infinite procedural storyline generation without narrative resolution (see INFINITE_A_STORY_DESIGN.md for design rationale, ADR-001 for architectural implementation)."
- "Perfect Information principle requires all strategic costs/rewards be calculable at UI layer before player commitment (see PERFECT_INFORMATION_PRINCIPLE.md for design rationale)."

### Section 03 - Context and Scope

**REMOVE:**
- Lines 15-38: Duplicate three-tier loop detail

**REPLACE WITH:**
- Brief summary: "Wayfarer provides nested gameplay loops supporting resource management under pressure. See CORE_GAMEPLAY_LOOPS.md for gameplay design."

### Section 08 - Crosscutting Concepts

**REMOVE:**
- Lines 546-556: Principle 6 (Resource Scarcity) - move to game design

**MODIFY:**
- Lines 506-526: Principle 4 (Inter-Systemic Rules) - keep but add reference: "See PROGRESSION_DESIGN.md for game design rationale."
- Lines 603-620: Principle 10 (Perfect Information) - keep brief architectural version, reference game design doc
- Lines 693-707: Four-Choice Archetype - move to game design, replace with: "Content validation enforces four-choice archetype pattern (see CHOICE_DESIGN_PATTERNS.md). Parser validates zero-requirement fallback exists."

### Section 09 - Architecture Decisions

**MODIFY:**
- ADR-001: Refocus on ARCHITECTURAL aspects (two-phase pipeline, validation requirements, procedural generation architecture). Add prominent reference: "For game design rationale, see INFINITE_A_STORY_DESIGN.md"

**REPLACE:**
- ADR-002: Move current content to RESOURCE_ARITHMETIC_DESIGN.md. Create new ADR-002 "Requirement Inversion Pattern" focused on implementation (SpawnConditions, IsEligible(), parse-time validation, UI display requirements)

**MODIFY:**
- ADR-006: Keep but add reference: "For game design perspective on principle priorities, see DESIGN_PRINCIPLE_HIERARCHY.md"

### Section 10 - Quality Requirements

**REMOVE:**
- QS-004 (Strategic Depth validation scenarios) - move to game design

**REPLACE WITH:**
- New QS-004 focused on architectural validation: "Resource state consistency across systems", "Accurate cost calculations", "UI displays correct resource values"

---

## IMPLEMENTATION PRIORITY

### Phase 1: Critical Separation (Do First)
1. Create CORE_GAMEPLAY_LOOPS.md
2. Create CHOICE_DESIGN_PATTERNS.md
3. Modify arc42 sections 01 and 08 to reference these docs
4. Move four-choice archetype content out of arc42

**Rationale:** These are the most obviously misplaced game design content in architecture docs.

### Phase 2: Philosophy Documents (Do Second)
1. Create INFINITE_A_STORY_DESIGN.md
2. Create RESOURCE_SCARCITY_DESIGN.md
3. Create RESOURCE_ARITHMETIC_DESIGN.md (move from ADR-002)
4. Modify arc42 ADR-001 and ADR-002

**Rationale:** These provide philosophical foundation for game design decisions.

### Phase 3: Principle Documents (Do Third)
1. Create PERFECT_INFORMATION_PRINCIPLE.md
2. Create TWO_LAYER_PLAYER_EXPERIENCE.md
3. Create PROGRESSION_DESIGN.md
4. Create DESIGN_PRINCIPLE_HIERARCHY.md
5. Modify arc42 section 08 to reference these docs

**Rationale:** These unify scattered design principles into coherent game design philosophy.

### Phase 4: Validation Documents (Do Last)
1. Create STRATEGIC_DEPTH_VALIDATION.md
2. Modify arc42 section 10 QS-004

**Rationale:** Nice to have but lower priority than core design documentation.

---

## GUIDING PRINCIPLE FOR SEPARATION

**Arc42 (HOW the system is built):**
- System structure and components
- Implementation patterns and technology choices
- Architectural constraints and trade-offs
- Testing and validation of SYSTEM CORRECTNESS

**Game Design (WHAT the player experiences and WHY it's fun):**
- Gameplay loops and player activity flow
- Progression systems and resource economy
- Choice design patterns and player agency
- Narrative structure and engagement philosophy
- Testing and validation of PLAYER EXPERIENCE

**When in doubt:** If it describes PLAYER BEHAVIOR or DESIGN GOALS → game design. If it describes SYSTEM STRUCTURE or IMPLEMENTATION → arc42.

**Cross-references are good:** Game design docs should reference arc42 for "how it's implemented." Arc42 docs should reference game design for "why this design drives this architecture."

---

## END OF ANALYSIS
