# Documentation Separation Framework: Game Design vs Arc42 Technical

**Purpose:** Clear boundaries between player-facing design documentation and developer-facing technical documentation for Wayfarer.

**Last Updated:** 2025-01

**Status:** Authoritative

---

## Executive Summary

Wayfarer documentation serves two distinct audiences with different needs. This framework establishes clear separation boundaries, decision tests, and cross-reference strategies to prevent documentation drift and HIGHLANDER violations.

**Core Distinction:**
- **Game Design Docs**: Answer "WHY does this create strategic depth?" for designers/players
- **Arc42 Docs**: Answer "HOW is this implemented?" for developers/architects

---

## 1. What Belongs Where

### 1.1 Game Design Documentation (Player Experience & Design Intent)

**Primary Audience:** Game designers, content authors, future designers, players (potentially)

**Purpose:** Explain design decisions, player experience goals, strategic depth rationale, content creation philosophy

**Belongs Here:**

**Core Gameplay Loops:**
- Three-tier loop hierarchy (short/medium/long loops)
- Daily rhythm structure (morning/evening phases)
- Delivery cycle mechanics (accept job → route segments → earn coins → survival)
- Calendar pressure and time segment competition
- Resource pressure creating impossible choices

**Design Philosophy & Player Experience:**
- WHY design decisions create strategic depth
- Impossible choices framework (accept one cost to avoid another)
- Perfect information principle (player can calculate before commitment)
- No soft-lock architecture rationale
- Specialization creates identity AND vulnerability
- Optimization skill as core competency

**Content Archetypes as GAMEPLAY PATTERNS:**
- Four-choice archetype as GUARANTEED PROGRESSION mechanism
- WHY four choices prevent soft-locks (stat-gated, money-gated, challenge, guaranteed fallback)
- Archetype reusability philosophy (one pattern, infinite fictional contexts)
- Service flow composition (negotiate → execute → depart)
- Situation design patterns with player experience intent

**Narrative Design:**
- Infinite A-story philosophy (Frieren Principle)
- WHY never-ending story creates coherent experience
- Package cohesion as content organization philosophy
- AI narrative generation philosophy (augment hand-crafted content)

**Resource Economy as PLAYER CONSTRAINTS:**
- Tight economy design (earnings barely cover survival)
- WHY scarcity creates strategic decisions
- Resource layers (personal stats, relationships, permanent resources, time, context)
- Shared vs system-specific resources

**Challenge Mechanics as GAMEPLAY:**
- Mental/Physical/Social challenge session models from PLAYER PERSPECTIVE
- WHY pauseable vs one-shot vs session-bounded matters for player experience
- Card types as tactical options (not implementation)
- Resource accumulation patterns (builder/threshold/session resources)
- Action pairs as player rhythm (ACT/OBSERVE, EXECUTE/ASSESS, SPEAK/LISTEN)

**Progression & Balance:**
- Stat system philosophy (five stats, manifestation across challenge types)
- WHY unified stats create meaningful specialization
- Bond system as investment tension (short-term cost, long-term benefit)
- Equipment progression philosophy

**Atmospheric Layer Philosophy:**
- WHY persistent actions solve quiet location problem
- Navigation as scaffolding vs content as ephemeral layer
- Atmospheric actions categories from player perspective

**Example Files:**
- `/home/user/Wayfarer/DESIGN_PHILOSOPHY.md`
- `/home/user/Wayfarer/WAYFARER_CORE_GAME_LOOP.md`
- `/home/user/Wayfarer/HOW_TO_PLAY_WAYFARER.md`
- `/home/user/Wayfarer/REQUIREMENT_INVERSION_PRINCIPLE.md` (WHY resource arithmetic, not boolean gates)

---

### 1.2 Arc42 Technical Documentation (Developer Implementation)

**Primary Audience:** Software engineers, architects, future maintainers, AI assistants

**Purpose:** Explain system architecture, implementation patterns, code organization, technical decisions

**Belongs Here:**

**System Architecture (HOW systems connect):**
- Two-layer architecture IMPLEMENTATION (Scene/Situation entities vs Challenge session objects)
- GameWorld as single source of truth (zero dependencies architecture)
- Facade pattern responsibilities (GameFacade orchestrates specialized facades)
- Service & subsystem organization (Social/Mental/Physical parallel structure)
- Dependency inversion (all dependencies flow toward GameWorld)

**Parse-Time vs Spawn-Time vs Query-Time (Three-Tier Timing Model):**
- WHEN entities instantiate (parse/spawn/query)
- HOW lazy instantiation works (InstantiationState tracking)
- WHY deferred action creation reduces memory
- Template → Instance → Action lifecycle
- Complete flow examples with concrete code

**Catalogues and Property Translation (IMPLEMENTATION):**
- HOW catalogues translate categorical → concrete
- WHEN translation happens (parse-time ONLY)
- Formula implementation (`scaledValue = base × multiplier`)
- FORBIDDEN runtime patterns (no catalogue calls, no string matching)
- Existing catalogue locations and usage

**Scene/Situation/Choice Entity Structure (CODE ORGANIZATION):**
- Scene owns embedded Situations collection (not separate GameWorld collection)
- Situation contains ChoiceTemplates (NOT actions yet)
- InstantiationState enum tracking (Deferred → Instantiated)
- Entity relationships (ownership vs placement vs reference)
- Property naming and types

**Blazor ServerPrerendered Double-Rendering:**
- Idempotence requirements
- StaticWebAssets configuration
- NavigationManager quirks
- Component lifecycle (prerender → interactive)

**Save/Load Serialization:**
- JSON serialization patterns
- GameWorld state persistence
- Save file format
- Load validation

**Performance Considerations:**
- Memory optimization (lazy instantiation rationale)
- Query optimization (GameWorld collection access patterns)
- Action cleanup lifecycle
- UI rendering efficiency

**Data Flow Patterns:**
- JSON → DTO → Parser → Domain → GameWorld → Facade → UI
- Context object creation (ConversationContext, MentalSession, etc.)
- Request/response flow (user click → facade → state update → UI refresh)
- Marker resolution flow (logical marker → spawn-time GUID → runtime usage)

**Example Files:**
- `/home/user/Wayfarer/ARCHITECTURE.md`
- `/home/user/Wayfarer/ARCHITECTURAL_PATTERNS.md`
- `/home/user/Wayfarer/CODING_STANDARDS.md`
- `/home/user/Wayfarer/01_introduction_and_goals.md`
- `/home/user/Wayfarer/04_solution_strategy.md`
- `/home/user/Wayfarer/08_crosscutting_concepts.md`

---

### 1.3 Overlapping Concepts (Appears in Both with Different Angles)

Some concepts appear in both documentation sets but serve different purposes:

#### Two-Layer Architecture

**Game Design Perspective:**
- "Strategic perfect information vs tactical hidden complexity"
- WHY separation enables informed player decisions
- Player can calculate WHETHER to attempt before entering
- Perfect information principle application
- Strategic depth through calculated risk-taking

**Arc42 Perspective:**
- "Scene/Situation (persistent) vs Challenge sessions (temporary)"
- HOW ChoiceTemplate.ActionType bridges layers
- Entity ownership hierarchy (Scene owns Situations, Challenge extracts SituationCards)
- Forbidden patterns (layer confusion, wrong collections)
- Complete flow examples with code

**Cross-Reference:** Game design explains player experience goal, arc42 explains implementation achieving that goal.

---

#### Resource Arithmetic

**Game Design Perspective:**
- WHY numbers create strategic depth (not boolean gates)
- "I can afford A OR B, not both" impossible choice
- Resource scarcity philosophy
- Shared resource competition
- Opportunity costs and trade-offs

**Arc42 Perspective:**
- HOW catalogues scale numeric values
- Parse-time translation formulas (`statThreshold = base × multiplier`)
- Categorical properties → concrete values
- RequirementFormula evaluation at runtime
- FORBIDDEN boolean flag patterns

**Cross-Reference:** Game design explains why resource arithmetic matters for depth, arc42 explains how catalogue scaling implements it.

---

#### Four-Choice Archetype

**Game Design Perspective:**
- Guaranteed progression design philosophy
- WHY four choices prevent soft-locks
- Player choice types (optimal/reliable/risky/patient)
- Resource orthogonality (each choice costs DIFFERENT resource)
- Specialization enables some paths, blocks others

**Arc42 Perspective:**
- HOW SituationArchetypeCatalogue generates four choices
- ChoiceTemplate.PathType enum values
- Parse-time archetype expansion
- Archetype composition (Tier 1 situation + Tier 2 scene)
- Code examples of generation logic

**Cross-Reference:** Game design explains player experience and design rationale, arc42 explains procedural generation implementation.

---

#### Archetype Reusability

**Game Design Perspective:**
- WHY same mechanics apply to different fictional contexts
- Content author efficiency (write once, use infinitely)
- AI generation enablement (categorical descriptions scale automatically)
- Mathematical variety (21 archetypes × property combinations = infinite content)

**Arc42 Perspective:**
- HOW categorical properties drive scaling
- GenerationContext structure
- Parse-time catalogue translation
- Concrete formula examples
- SceneArchetypeCatalogue vs SituationArchetypeCatalogue separation

**Cross-Reference:** Game design explains content creation philosophy, arc42 explains catalogue implementation.

---

#### Challenge Session Models

**Game Design Perspective (Player Experience):**
- **Mental**: Pauseable investigation (leave and return)
- **Physical**: One-shot test (must complete now)
- **Social**: Session-bounded conversation (NPC has agency)
- WHY different models create different player experiences
- Verisimilitude justification (investigations take time, obstacles don't wait, conversations are real-time)

**Arc42 Perspective (Implementation):**
- Session state persistence patterns
- Progress/Attention/Exposure tracking for Mental
- Breakthrough/Exertion/Danger tracking for Physical
- Momentum/Initiative/Doubt tracking for Social
- Session cleanup on completion/abandonment
- Temporary vs persistent state management

**Cross-Reference:** Game design explains player experience and fiction justification, arc42 explains state management implementation.

---

## 2. Decision Framework

### 2.1 Quick Test Questions

When writing documentation or classifying existing content, ask these questions:

#### Question 1: Who Needs This Information?

**If answer = "Game designer creating new content"** → Game Design Docs
**If answer = "Software engineer implementing features"** → Arc42 Docs
**If answer = "Both"** → Appears in both, different angles (see section 1.3)

#### Question 2: Does It Explain WHY or HOW?

**WHY this creates strategic depth** → Game Design Docs
**WHY this design decision over alternatives** → Game Design Docs
**HOW this is implemented in code** → Arc42 Docs
**HOW systems connect architecturally** → Arc42 Docs

#### Question 3: Is It About Player Experience or Code Organization?

**Player experience, tactical feel, strategic depth** → Game Design Docs
**Code organization, entity structure, data flow** → Arc42 Docs
**Both** → Split (player experience in design, implementation in arc42)

#### Question 4: Does It Use Game Terms or Code Terms?

**Game terms:** "Investigation is pauseable, player leaves and returns" → Game Design
**Code terms:** "MentalSession persists Progress/Exposure across query-time instantiations" → Arc42
**Mixed:** "Pauseable investigations persist MentalSession state" → Needs separation

#### Question 5: Would a Non-Programmer Understand It?

**Yes (uses game language, design philosophy)** → Likely Game Design
**No (requires code knowledge, technical patterns)** → Likely Arc42
**Depends on section** → Check if splitting improves clarity

---

### 2.2 Audience Test (Concrete Personas)

#### Persona 1: Game Designer (No Code Experience)

**Reads:** DESIGN_PHILOSOPHY.md, WAYFARER_CORE_GAME_LOOP.md
**Needs:** Understanding WHY choices matter, HOW to create balanced content, WHAT archetypes exist
**Avoids:** ARCHITECTURE.md (too technical), CODING_STANDARDS.md (irrelevant)

**Test:** If this persona can't understand a section in game design docs, it belongs in arc42.

#### Persona 2: Software Engineer (No Game Design Experience)

**Reads:** ARCHITECTURE.md, ARCHITECTURAL_PATTERNS.md, 04_solution_strategy.md
**Needs:** Understanding HOW systems connect, WHEN entities instantiate, WHERE business logic lives
**Avoids:** WAYFARER_CORE_GAME_LOOP.md (provides context but not required for implementation)

**Test:** If this persona can't implement a feature without a section in arc42, it's correctly placed.

#### Persona 3: Technical Game Designer (Both Backgrounds)

**Reads:** Both sets, cross-references frequently
**Needs:** Understanding WHY design creates depth AND HOW implementation achieves it
**Benefits:** Sees complete picture, makes informed content decisions

**Test:** Can this persona find information quickly in appropriate doc? If searching both docs for same concept, cross-reference needed.

---

## 3. Example Classifications (Tricky Concepts)

### 3.1 "Catalogue Pattern"

**Game Design Angle:** PROCEDURAL_CONTENT_GENERATION.md section on "WHY categorical properties"
- AI can generate without knowing game state
- Content authors describe entities categorically
- Dynamic scaling enables infinite variations
- Balance maintained automatically

**Arc42 Angle:** ARCHITECTURAL_PATTERNS.md + 08_crosscutting_concepts.md
- Three-phase pipeline (JSON → Parse → Runtime)
- FORBIDDEN patterns (runtime catalogue calls)
- Parse-time ONLY translation
- Code examples, formula specifics

**Belongs:** Primarily Arc42 (implementation pattern). Brief mention in game design with cross-reference.

---

### 3.2 "Perfect Information Principle"

**Game Design Angle:** DESIGN_PHILOSOPHY.md Principle 10
- Player can calculate strategic decisions before commitment
- All costs/rewards/requirements visible at strategic layer
- Hidden complexity belongs in tactical layer only
- Creates informed decision-making experience

**Arc42 Angle:** 01_introduction_and_goals.md Quality Goal, 04_solution_strategy.md
- Strategic layer entities (Scene/Situation/ChoiceTemplate)
- ChoiceTemplate properties visible before execution
- Tactical layer hides card draw order
- Implementation of requirement display in UI

**Belongs:** Both. Design philosophy explains WHY it matters, arc42 explains HOW it's implemented.

---

### 3.3 "Scene Lifecycle"

**Game Design Angle:** Brief mention in game loop docs
- Scenes provide narrative content
- Scenes progress through situations
- Scenes complete when story finishes

**Arc42 Angle:** ARCHITECTURE.md + 06_runtime_view.md (arc42 section)
- SceneState enum (Provisional/Active/Completed/Expired)
- State transitions (spawn → finalize → progress → complete)
- InstantiationState tracking
- Action cleanup on completion
- Marker resolution at spawn

**Belongs:** Primarily Arc42. Game design mentions scenes conceptually, arc42 explains full lifecycle technically.

---

### 3.4 "Impossible Choices"

**Game Design Angle:** DESIGN_PHILOSOPHY.md + WAYFARER_CORE_GAME_LOOP.md
- Core design philosophy (two valid options, insufficient resources for both)
- Resource pressure creates meaningful decisions
- Examples: energy vs time vs coins, delivery vs NPC bond vs exploration
- WHY this creates strategic depth

**Arc42 Angle:** None (pure game design concept)

**Belongs:** Game Design ONLY. No arc42 counterpart needed (implementation is just resource arithmetic which has its own arc42 docs).

---

### 3.5 "Three-Tier Timing Model"

**Game Design Angle:** None (pure implementation detail)

**Arc42 Angle:** ARCHITECTURAL_PATTERNS.md + ARCHITECTURE.md
- Parse time (templates), Spawn time (scenes), Query time (actions)
- WHY lazy instantiation reduces memory
- InstantiationState tracking
- Complete flow examples
- Forbidden patterns

**Belongs:** Arc42 ONLY. No game design counterpart (players never see this).

---

### 3.6 "NPC Bond System"

**Game Design Angle:** WAYFARER_CORE_GAME_LOOP.md
- Investment tension (short-term cost, long-term benefit)
- Bond level rewards (route shortcuts, discounts, stat training, exclusive access)
- Opportunity cost vs deliveries
- WHY mechanical benefits compound
- Player decision framework

**Arc42 Angle:** Brief implementation notes in ARCHITECTURE.md
- Bond level tracking (int property)
- Scene spawning based on bond level
- Reward application (typed rewards at level-up)

**Belongs:** Primarily Game Design. Arc42 mentions implementation briefly with cross-reference to design rationale.

---

## 4. Cross-Reference Strategy

### 4.1 When to Cross-Reference

**Always cross-reference when:**
1. Overlapping concept appears in both docs (see section 1.3)
2. Game design rationale informs implementation decision
3. Implementation pattern enables design philosophy
4. Reader would benefit from seeing other perspective

**Never cross-reference when:**
1. Concept purely design (impossible choices) OR purely technical (three-tier timing)
2. Audiences don't overlap
3. Creates circular dependency

### 4.2 Cross-Reference Format

**From Game Design to Arc42:**
```markdown
**Implementation:** See ARCHITECTURAL_PATTERNS.md (Catalogue Pattern) for HOW categorical properties translate to concrete values at parse-time.
```

**From Arc42 to Game Design:**
```markdown
**Design Rationale:** See DESIGN_PHILOSOPHY.md (Principle 10: Perfect Information) for WHY strategic layer requires all costs/rewards visible before player commitment.
```

**Bidirectional (Overlapping Concept):**

In Game Design Doc:
```markdown
### Two-Layer Architecture (Player Experience)

Strategic layer provides perfect information (all costs visible). Tactical layer hides complexity (card draw order unknown). This separation enables informed strategic decisions.

**Technical Implementation:** See 04_solution_strategy.md (Section 4.1) for entity structure and bridging pattern.
```

In Arc42 Doc:
```markdown
### 4.1 Two-Layer Architecture Decision

**Design Goal:** Enable perfect information at strategic layer while preserving tactical surprise. See DESIGN_PHILOSOPHY.md (Principle 10) for player experience rationale.

**Solution:** Strict separation via ChoiceTemplate.ActionType bridge...
```

### 4.3 Cross-Reference Maintenance

**When updating concept in one doc:**
1. Check if overlapping concept exists in other doc set
2. Update both docs if concept spans both
3. Verify cross-references still accurate
4. Update cross-reference if section numbers/locations changed

**Automated check:**
- Search for concept name across all docs
- Verify cross-references use current section numbers
- Flag broken links

---

## 5. Validation Checklist

### 5.1 For Game Design Documents

**Content Validation:**
- [ ] Explains WHY design creates strategic depth
- [ ] Uses player-facing language (not code terms)
- [ ] Focuses on player experience, design intent, balance philosophy
- [ ] Examples use gameplay scenarios (not code)
- [ ] Non-programmer can understand most content

**Cross-Reference Validation:**
- [ ] Technical implementation details cross-reference arc42
- [ ] No code examples (delegate to arc42)
- [ ] No entity property details (delegate to arc42)

**Audience Test:**
- [ ] Game designer persona can create content from this
- [ ] Explains design decisions clearly
- [ ] Provides enough context for content authoring

### 5.2 For Arc42 Documents

**Content Validation:**
- [ ] Explains HOW systems are implemented
- [ ] Uses technical terms (entities, facades, parsers)
- [ ] Focuses on architecture, code organization, data flow
- [ ] Examples use code snippets
- [ ] Requires programming knowledge to fully understand

**Cross-Reference Validation:**
- [ ] Design rationale cross-references game design docs
- [ ] No player experience justification (delegate to design docs)
- [ ] No balance philosophy (delegate to design docs)

**Audience Test:**
- [ ] Software engineer persona can implement features from this
- [ ] Architecture decisions clearly documented
- [ ] Provides enough detail for maintenance

---

## 6. Migration Path for Misplaced Content

### 6.1 Identifying Misplaced Content

**Symptoms:**
1. Game design doc contains code examples
2. Arc42 doc explains player experience at length
3. Same content duplicated in both docs (no cross-reference)
4. Audience confusion ("I need X but it's in technical doc")

### 6.2 Migration Process

**Step 1: Identify Content Type**
- Use decision framework (section 2.1)
- Use audience test (section 2.2)
- Classify as: Game Design, Arc42, or Overlapping

**Step 2: Extract and Move**
- **If purely game design:** Move to DESIGN_PHILOSOPHY.md or WAYFARER_CORE_GAME_LOOP.md
- **If purely arc42:** Move to ARCHITECTURE.md or appropriate arc42 section
- **If overlapping:** Split into two sections (design angle + technical angle)

**Step 3: Add Cross-References**
- Use cross-reference format (section 4.2)
- Verify bidirectional links
- Test that readers can find both perspectives

**Step 4: Delete Duplicates**
- Remove original content from misplaced location
- Leave breadcrumb cross-reference if readers might look there
- Update table of contents

---

## 7. Summary: Quick Decision Table

| If Content Is... | Belongs In... | Example Topic |
|------------------|---------------|---------------|
| Player experience rationale | Game Design | WHY impossible choices create depth |
| Design philosophy | Game Design | Perfect information principle |
| Gameplay loop structure | Game Design | Three-tier loop hierarchy |
| Resource economy philosophy | Game Design | Tight economy creates pressure |
| Archetype as player pattern | Game Design | Four-choice guaranteed progression |
| Implementation architecture | Arc42 | Two-layer entity structure |
| Code organization | Arc42 | Scene owns Situations collection |
| Parse/spawn/query timing | Arc42 | Three-tier timing model |
| Data flow patterns | Arc42 | JSON → Parser → GameWorld → Facade |
| Catalogue translation | Arc42 | Parse-time categorical → concrete |
| Technical patterns | Arc42 | HIGHLANDER, Let It Crash |
| Overlapping (WHY + HOW) | BOTH (cross-ref) | Two-layer architecture, Resource arithmetic |

---

## 8. Examples of Correct Separation

### 8.1 Resource Scarcity Concept

**In DESIGN_PHILOSOPHY.md (Game Design):**
```markdown
### Principle 6: Resource Scarcity Creates Impossible Choices

Shared resources (Time, Focus, Stamina, Health, Coins) force player to accept one cost to avoid another. This creates strategic depth through opportunity costs and trade-offs.

**The Impossible Choice:** "I can afford to do A OR B, but not both. Both paths are valid. Both have genuine costs. Which cost will I accept?"

**Test:** Can player pursue all interesting options without trade-offs? If yes, no strategic depth exists.
```

**In 04_solution_strategy.md (Arc42):**
```markdown
### 4.2 Parse-Time Translation Strategy (Catalogue Pattern)

**Problem:** AI content generation requires balance without knowing global game state.

**Solution:** Catalogues translate categorical properties to concrete numeric values using universal formulas.

Formula: `ScaledCost = BaseCost × QualityMultiplier × DemeanorMultiplier`

**Design Rationale:** See DESIGN_PHILOSOPHY.md (Principle 6) for why resource arithmetic creates strategic depth through scarcity.
```

**Result:** Game design explains WHY scarcity matters for player experience. Arc42 explains HOW catalogues implement resource costs. Clear separation, cross-referenced.

---

### 8.2 Challenge Session Models

**In WAYFARER_CORE_GAME_LOOP.md (Game Design):**
```markdown
### Mental Challenges - Investigation at Locations

Mental investigations can be paused and resumed, respecting the reality that investigations take time:

**Session Model: Pauseable Static Puzzle**
- Can pause anytime: Leave location, state persists exactly where you left off
- Progress persists: Accumulates at location across multiple visits
- Exposure persists: Investigative "footprint" increases difficulty
- No forced ending: High Exposure makes investigation harder but doesn't force failure

**Why Pauseable:** Real investigations take time. Player naturally breaks engagement to pursue other priorities. Forcing completion in single session creates artificial pressure inconsistent with investigative fiction.
```

**In ARCHITECTURE.md (Arc42):**
```markdown
### Service & Subsystem Layer

**MentalFacade**: Mental challenges (investigations) at locations
- Progress/Attention/Exposure/Leads resource system
- Pauseable session model (can leave and return)
- Session state persists in GameWorld.MentalSessions collection
- Location properties (Delicate, Obscured, Layered, Time-Sensitive, Resistant)

**Design Rationale:** See WAYFARER_CORE_GAME_LOOP.md (Mental Challenges) for why pauseable model creates appropriate investigative experience.
```

**Result:** Game design explains player experience and fiction justification. Arc42 explains implementation details and state management. Clear separation, cross-referenced.

---

## 9. Enforcement and Maintenance

### 9.1 Documentation Review Checklist

When creating or updating documentation:

**For Each Section:**
1. [ ] Is primary audience clear? (Designer vs Developer)
2. [ ] Does it pass decision framework tests? (section 2.1)
3. [ ] Cross-references added where appropriate? (section 4)
4. [ ] No duplication without cross-reference?
5. [ ] Validation checklist satisfied? (section 5)

**For Overlapping Concepts:**
1. [ ] Both perspectives documented? (design + technical)
2. [ ] Bidirectional cross-references exist?
3. [ ] Each doc uses appropriate terminology for audience?
4. [ ] No implementation details in design doc?
5. [ ] No player experience justification in arc42?

### 9.2 Periodic Audits

**Monthly:** Scan for new content, verify placement
**Quarterly:** Full documentation review using this framework
**After major features:** Update cross-references

### 9.3 Red Flags (Requires Immediate Attention)

- ❌ Code examples in game design docs (extract to arc42)
- ❌ Player experience narratives in arc42 (extract to design)
- ❌ Same content in both docs without cross-reference (consolidate)
- ❌ Broken cross-references (update section numbers)
- ❌ Mixed terminology (code terms in design, game terms in arc42)

---

## 10. Conclusion

Clear separation between game design and arc42 documentation prevents HIGHLANDER violations, reduces maintenance burden, and serves both audiences effectively.

**Core Principles:**
1. **Game Design** = WHY (player experience, strategic depth, design philosophy)
2. **Arc42** = HOW (implementation, architecture, technical patterns)
3. **Overlapping** = BOTH (different perspectives, cross-referenced)
4. **Audience Test** = Can target audience understand and use this?

**When in Doubt:**
- Ask: "Who needs this?" (Designer vs Developer)
- Ask: "WHY or HOW?" (Design vs Technical)
- Use decision framework (section 2)
- Apply audience test (section 2.2)

**Maintenance:**
- Use validation checklists (section 5)
- Add cross-references (section 4)
- Audit periodically (section 9.2)
- Fix red flags immediately (section 9.3)

**Result:** Each documentation set serves its audience optimally. Designers learn design philosophy without wading through technical details. Developers understand implementation without parsing game balance rationale. Both can cross-reference when needed.

---

## Related Documentation

- **DOCUMENTATION_STRUCTURE_ANALYSIS.md**: HIGHLANDER violation analysis (CLAUDE.md conflation)
- **DESIGN_PHILOSOPHY.md**: Game design principles (authoritative source for WHY)
- **ARCHITECTURE.md**: System architecture (authoritative source for HOW)
- **ARCHITECTURAL_PATTERNS.md**: Implementation patterns (arc42 technical)
- **CODING_STANDARDS.md**: Code-level conventions (arc42 technical)
- **Arc42 Sections**: Developer-facing technical documentation structure
