# Arc42 Section 11: Risks and Technical Debt

## 11.1 Overview

This section documents technical risks that could impact the system and known technical debt requiring future attention. Each risk includes likelihood, impact, and mitigation strategies. Technical debt items include root causes and remediation plans.

---

## 11.2 Technical Risks

### RISK-001: Procedural A-Story Quality Degradation

**Category**: Content Generation
**Likelihood**: Medium
**Impact**: High (affects core experience)

**Description:**
The infinite A-story (A11+) relies on procedural generation combining archetypes, categorical filtering, and AI narrative generation. If generation quality degrades:
- Players encounter repetitive or nonsensical content
- Structural guarantees violated (soft-locks possible)
- AI-generated narrative disconnected from game state
- Balance broken (too easy or impossibly hard)

**Specific Failure Modes:**
1. **Archetype Repetition**: Same archetype selected 3+ times in sequence (boring pattern)
2. **Entity Resolution Failure**: Categorical filters match no available entities (empty results)
3. **Structural Validation Bypass**: Generated scenes skip validation, violate no-soft-lock requirement
4. **AI Hallucination**: LLM generates narrative referencing non-existent game entities
5. **Scaling Imbalance**: Late-game content too easy (player overpowered) or too hard (impossible)

**Current Mitigation:**
- Archetype catalog includes 20-30 variants (reduces repetition)
- Categorical property scaling tested on authored content (A1-A3)
- Structural validation framework designed (not yet implemented)
- Marker resolution prevents reference errors

**Additional Mitigation Needed:**
- **Implement Validation Framework** (Priority: HIGH):
  - ContentValidator.ValidateSceneStructure() before spawn
  - Reject scenes violating no-soft-lock guarantee
  - Log validation failures for debugging
  - Fallback to safe archetype if generation fails

- **Add Archetype Selection Anti-Repetition** (Priority: MEDIUM):
  - Track last 5 spawned archetypes
  - Prefer unused archetypes, avoid repeats
  - Force variety after 3 consecutive similar archetypes

- **Implement Simulation Testing** (Priority: MEDIUM):
  - Automated playthrough of generated scenes
  - Verify all paths lead to next scene
  - Detect impossible resource requirements
  - Run nightly on 100+ generated scenes

- **Monitor Generation Metrics** (Priority: HIGH):
  - Telemetry: Archetype distribution over time
  - Player feedback: "Content felt repetitive" survey question
  - Validation pass rate: % of generated scenes passing validation
  - Target: <10% player reports of repetition, >95% validation pass rate

**Risk Owner**: Content Generation Team

---

### RISK-002: Performance Degradation with Scale

**Category**: Performance
**Likelihood**: Medium-High
**Impact**: Medium (affects UX)

**Description:**
As player progresses, GameWorld accumulates entities:
- 50+ hours gameplay = hundreds of scenes (some active, many expired)
- Thousands of locations generated via dependent resources
- NPCs with complex relationship histories
- Large card decks with many unlocked cards

Query-time operations may slow:
- SceneFacade.GetActionsAtLocation() iterates all active scenes
- LocationFacade filters locations by accessibility
- UI rendering large choice lists

**Specific Performance Concerns:**
1. **Scene Collection Bloat**: Active + expired scenes never pruned
2. **Action Instantiation Cost**: Creating actions from templates on every UI refresh
3. **Entity Lookup Overhead**: Linear search through GameWorld collections (no indexing)
4. **Save File Size**: Serializing thousands of entities takes seconds
5. **UI Rendering**: Blazor re-renders entire component tree on state changes

**Current Mitigation:**
- Query-time action instantiation (only current context, not all scenes)
- Expired scenes can be deleted (architecture supports)
- Three-tier timing model reduces memory footprint

**Additional Mitigation Needed:**
- **Implement Scene Cleanup** (Priority: MEDIUM):
  - Delete expired scenes after 7 days
  - Configurable retention policy
  - Keep only: Active, Provisional, recently completed
  - Target: Max 50 scenes in GameWorld.Scenes

- **Add Entity Indexing** (Priority: LOW):
  - Dictionary<locationId, List<Scene>> for location-based queries
  - Rebuild index on scene spawn/completion
  - Trade memory for query speed
  - Target: <10ms for GetActionsAtLocation()

- **Optimize Save/Load** (Priority: LOW):
  - Incremental saves (only changed entities)
  - Compression for large collections
  - Background save thread (non-blocking)
  - Target: <2 seconds for full save

- **Profile Critical Paths** (Priority: HIGH):
  - Identify slowest 10% of operations
  - Add performance telemetry
  - Establish baselines: Max 50ms for UI interactions
  - Monitor 95th percentile response times

**Risk Owner**: Performance Engineering

---

### RISK-003: Catalogue Scaling Imbalance

**Category**: Game Balance
**Likelihood**: Medium
**Impact**: Medium (affects difficulty curve)

**Description:**
Universal scaling formulas in catalogues must work across:
- Early game (Level 1, 50 coins total wealth)
- Mid game (Level 10, 500 coins wealth)
- Late game (Level 20, 2000+ coins wealth, max stats)

If formulas scale incorrectly:
- Early game too hard (players can't progress)
- Late game too easy (no challenge, boring)
- Stat sweet spots violated (16+ still optimal instead of 14)

**Specific Balance Concerns:**
1. **Linear Scaling**: Base × playerLevel works early, breaks late (exponential growth needed?)
2. **Coin Inflation**: Economy breaks if income scales faster than costs
3. **Stat Threshold Ceilings**: Max stat 20, but requirements exceed 20 (impossible)
4. **Quality Multipliers**: Premium (1.6×) too expensive early, trivial late
5. **Cross-System Inconsistency**: Social challenges scale differently than Mental

**Current Mitigation:**
- Inspired by Sir Brante (proven resource arithmetic model)
- Authored content (A1-A3) manually balanced as baseline
- Sweet spots documented (14 comfortable, 16 excessive)

**Additional Mitigation Needed:**
- **Implement Balance Testing Suite** (Priority: HIGH):
  - Simulate player at Level 1, 5, 10, 15, 20
  - Generate 100 scenes per level
  - Verify stat thresholds achievable (player.Stat ≥ requirement possible)
  - Check coin costs affordable (≤50% of typical wealth)
  - Target: 95% of generated content solvable

- **Add Dynamic Difficulty Adjustment** (Priority: MEDIUM):
  - Track player success rate per archetype
  - If <40% success: Reduce multipliers 10%
  - If >80% success: Increase multipliers 10%
  - Adjust per-player, not globally
  - Target: 50-70% challenge success rate

- **Monitor Economy Telemetry** (Priority: HIGH):
  - Player wealth over time (median, 95th percentile)
  - Coin income vs expenses ratio
  - Stat distribution at Level 10, 20
  - Target: Players maintain 1-2× daily expenses in reserves

- **Establish Scaling Baselines** (Priority: HIGH):
  - Document formula: BaseValue × (1 + 0.15 × playerLevel)
  - Test on authored content first
  - Iterate until A1-A3 feel balanced
  - Apply tested formulas to procedural generation

**Risk Owner**: Game Design Team

---

### RISK-004: AI Narrative Generation Unreliability

**Category**: AI Integration
**Likelihood**: Medium
**Impact**: Low-Medium (degrades UX, not blocking)

**Description:**
AI narrative generation (via Ollama LLM) can fail:
- **Service Unavailable**: Ollama offline, network issues
- **Timeout**: LLM takes >30 seconds, blocks gameplay
- **Hallucination**: Generated text references non-existent entities
- **Tone Mismatch**: AI generates inappropriate or inconsistent narrative
- **Context Window**: Player history exceeds LLM token limit

**Current Mitigation:**
- AI is enhancement, not requirement (fallback to templates)
- Narrative hints constrain AI output (tone, theme)
- Entity properties provided as context (npcName, locationName)

**Additional Mitigation Needed:**
- **Implement Graceful Fallback** (Priority: HIGH):
  - If AI call fails: Use template text with placeholders
  - If AI times out (>5s): Cancel, use fallback
  - Log failures for debugging
  - Target: Zero gameplay blocking

- **Add AI Output Validation** (Priority: MEDIUM):
  - Parse AI response for placeholder completion
  - Reject responses with "{npcName}" still present
  - Validate tone (no profanity, no modern references)
  - Retry once if validation fails, fallback if retry fails

- **Implement Context Pruning** (Priority: LOW):
  - Summarize player history if >2000 tokens
  - Include only recent 5 scenes
  - Focus on current location/NPC relationships
  - Target: Stay under LLM context limit

- **Monitor AI Performance** (Priority: MEDIUM):
  - Telemetry: AI call success rate, latency
  - User feedback: "Narrative felt disconnected" survey
  - Target: >90% success rate, <3s average latency

**Risk Owner**: AI Integration Team

---

### RISK-005: Save/Load State Corruption

**Category**: Data Integrity
**Likelihood**: Low
**Impact**: Critical (player progress loss)

**Description:**
Save/load system serializes entire GameWorld:
- Complex object graphs (Scenes → Situations → ChoiceTemplates)
- Ephemeral references (Template object cached, not saved)
- Marker resolution maps (GUIDs generated at spawn)

If serialization fails or data corrupts:
- Player loses 50+ hours of progress
- Game unloadable (crashes on load)
- Template references break (TemplateId doesn't resolve)

**Specific Corruption Scenarios:**
1. **Version Mismatch**: Old save, new code (properties renamed/removed)
2. **Circular References**: JSON serializer stack overflows
3. **Ephemeral Data Saved**: Template objects serialized (shouldn't be)
4. **MarkerResolutionMap Loss**: GUIDs lost, dependent resources unreachable
5. **Partial Write**: Save interrupted, file truncated

**Current Mitigation:**
- Template references: Save TemplateId (string), restore Template (object) on load
- Marker resolution: MarkerResolutionMap persisted with Scene
- Actions NOT saved (ephemeral, recreated from templates on load)

**Additional Mitigation Needed:**
- **Implement Save Validation** (Priority: HIGH):
  - After save: Deserialize and verify structure
  - Check all TemplateIds resolve
  - Validate GameWorld integrity (no null critical properties)
  - Target: Zero corrupt saves

- **Add Save File Versioning** (Priority: HIGH):
  - Save file header: { version: "1.0", schemaHash: "abc123" }
  - On load: Check version compatibility
  - If incompatible: Show clear error, don't corrupt game
  - Migration path for schema changes

- **Implement Backup System** (Priority: MEDIUM):
  - Keep last 3 saves (auto-rotate)
  - Player can manually create backup
  - If load fails: Try previous save
  - Target: Zero catastrophic progress loss

- **Add Save Integrity Tests** (Priority: MEDIUM):
  - Unit tests: Serialize → Deserialize → Verify equality
  - Integration tests: Full gameplay → Save → Load → Continue
  - Test with 10-hour save files (realistic scale)
  - Run on every build

**Risk Owner**: Save System Team

---

## 11.3 Technical Debt

### DEBT-001: Incomplete Categorical Property Translation

**Status**: 🚧 IN PROGRESS
**Priority**: HIGH
**Introduced**: Initial architecture implementation

**Description:**
The catalogue pattern requires ALL properties to be categorical (translated at parse-time), but some properties remain hardcoded:
- ServiceType enum values hardcoded in logic (should use categorical properties)
- Quality enum partially implemented (some contexts use it, others don't)
- EnvironmentQuality defined but not universally applied

**Impact:**
- Inconsistent scaling (some systems scale, others don't)
- AI generation limited (can't describe all properties categorically)
- Manual tuning required (defeats catalogue purpose)

**Root Cause:**
- Incremental implementation (catalogues added gradually)
- Legacy code not refactored (pre-catalogue patterns remain)
- Missing catalogue types (no ServiceTypeCatalogue)

**Remediation Plan:**
1. **Audit All Numeric Properties** (1 week):
   - Search codebase for hardcoded values (StatThreshold: 8, CoinCost: 15)
   - Identify which should be categorical
   - Create list of missing catalogues

2. **Create Missing Catalogues** (2 weeks):
   - ServiceTypeCatalogue (lodging, healing, bathing)
   - EnvironmentQualityCatalogue (safe, dangerous, sacred)
   - PowerDynamicCatalogue (already defined, ensure universal usage)

3. **Refactor Hardcoded Values** (3 weeks):
   - Replace hardcoded numbers with categorical properties
   - Update JSON files (add categorical properties)
   - Update parsers (call catalogues)
   - Update entities (store translated values)

4. **Validation** (1 week):
   - Verify ALL properties translated
   - No hardcoded values except design constants
   - Test balance with new catalogues

**Estimated Effort**: 7 weeks
**Risk**: Medium (breaking changes to JSON format)
**Assigned**: Content Pipeline Team

---

### DEBT-002: AI Narrative Integration Partial

**Status**: 🚧 IN PROGRESS
**Priority**: HIGH
**Introduced**: AI generation feature started

**Description:**
AI narrative generation architecture exists (Situation.GeneratedNarrative property, Ollama integration), but:
- Not all situations use AI (some still use template text)
- AI context building incomplete (doesn't include full player history)
- No quality validation (AI output not checked)
- Fallback inconsistent (some code paths don't handle AI failure)

**Impact:**
- Inconsistent narrative quality (some scenes rich, others generic)
- Potential gameplay blocking if AI fails and no fallback
- Player confusion (why does some content feel different?)

**Root Cause:**
- Feature implemented incrementally (not all scenes converted)
- Ollama integration added late (some code paths bypass it)
- No comprehensive testing of AI failure modes

**Remediation Plan:**
1. **Audit AI Integration Points** (1 week):
   - Identify all situations using AI vs templates
   - Document fallback behavior per code path
   - Create AI integration checklist

2. **Implement Universal Fallback** (2 weeks):
   - Centralize AI calls in NarrativeGenerator service
   - Wrap ALL calls with try-catch and timeout
   - Always provide template fallback
   - Log failures for monitoring

3. **Add AI Output Validation** (2 weeks):
   - Parse AI response structure
   - Verify placeholder replacement
   - Tone/length validation
   - Reject and fallback if invalid

4. **Convert Remaining Situations** (3 weeks):
   - Update situations still using only templates
   - Ensure AI hints provided for all
   - Test generated vs template quality
   - Target: 80% of situations use AI (20% templates acceptable)

**Estimated Effort**: 8 weeks
**Risk**: Low (fallback ensures functionality)
**Assigned**: AI Integration Team

---

### DEBT-003: Perfect Information Display Format Inconsistent

**Status**: 🚧 IN PROGRESS
**Priority**: MEDIUM
**Introduced**: UI implementation

**Description:**
Strategic layer perfect information principle requires showing exact costs/requirements, but:
- Some choices show "Stamina -3", others show "(Stamina cost)"
- Requirements format varies: "Requires Rapport 6" vs "Rapport ≥ 6" vs "Rapport 6+"
- Gap display inconsistent: "Need 2 more" vs "(You have 4)" vs not shown
- Reward visibility varies by context (some OnSuccess/OnFailure shown, others hidden)

**Impact:**
- Player confusion (can't rely on format)
- Accessibility issues (screen readers get inconsistent info)
- Quality goal QS-005 not fully met

**Root Cause:**
- UI components implemented independently
- No shared formatting library
- Incremental UI refactoring incomplete

**Remediation Plan:**
1. **Define Standard Formats** (1 week):
   - Document canonical formats:
     - Costs: "Stamina -3"
     - Requirements: "Requires Rapport 6 (you have 4)"
     - Gaps: "Need 2 more Rapport"
     - Rewards: "Gains: Coins +10, Understanding +1"
   - Create style guide for UI team

2. **Create Shared Formatting Service** (2 weeks):
   - DisplayFormatterService with methods:
     - FormatResourceCost(resourceType, amount, playerCurrent)
     - FormatRequirement(stat, threshold, playerStat)
     - FormatRewards(rewardTemplate)
   - Centralize ALL formatting logic

3. **Refactor UI Components** (3 weeks):
   - Update all choice display components
   - Replace custom formatting with service calls
   - Test visual consistency across all screens

4. **Add Accessibility Tests** (1 week):
   - Screen reader testing
   - Verify consistent aria-labels
   - Ensure keyboard navigation

**Estimated Effort**: 7 weeks
**Risk**: Low (visual changes only)
**Assigned**: UI/UX Team

---

### DEBT-004: ExplorationCubes Designed But Not Implemented

**Status**: 📋 DESIGNED
**Priority**: LOW
**Introduced**: Route mastery system design

**Description:**
ExplorationCubes intended to track route mastery (similar to InvestigationCubes for locations, StoryCubes for NPCs, MasteryCubes for decks):
- Methods exist: `GameWorld.cs` references ExplorationCubes
- Property NOT found on Route entity
- Functionality not implemented (no gain/spend mechanics)

**Impact:**
- Missing mastery system for route travel
- Asymmetry in mastery mechanics (locations/NPCs have cubes, routes don't)
- Dead code (methods referencing non-existent property)

**Root Cause:**
- Designed but deprioritized (other features more critical)
- Route travel works without mastery (optional enhancement)

**Remediation Plan:**
1. **Decide: Implement or Remove** (1 week):
   - Evaluate gameplay value: Does route mastery add depth?
   - If NO: Remove ExplorationCubes references, document decision
   - If YES: Continue to step 2

2. **If Implementing** (3 weeks):
   - Add ExplorationCubes property to Route entity
   - Implement gain mechanics (cubes earned per successful route travel)
   - Implement spend mechanics (cubes unlock route shortcuts? reduce danger?)
   - Update UI to display route mastery

3. **If Removing** (1 week):
   - Delete ExplorationCubes methods from GameWorld
   - Remove references from documentation
   - Document in ADR: "Route mastery deprioritized for simplicity"

**Estimated Effort**: 1-4 weeks (depends on decision)
**Risk**: None (optional feature)
**Assigned**: Game Systems Team

---

### DEBT-005: A4-A10 Tutorial Scenes Need Authoring

**Status**: 📋 DESIGNED
**Priority**: MEDIUM
**Introduced**: Infinite A-story design

**Description:**
Tutorial phase (A1-A10) designed with:
- A1-A3: Authored and implemented ✅
- A4-A10: Designed but not authored 📋

Players currently jump from A3 (30-60 minutes) directly to procedural generation (A11+). This skips:
- Advanced mechanics introduction (complex situations, multi-phase scenes)
- Narrative setup for infinite continuation
- Calibration for procedural quality expectations

**Impact:**
- Jarring transition (hand-crafted → procedural feels abrupt)
- Missed teaching opportunities (advanced patterns not tutorialized)
- Procedural quality comparison (A11+ compared to A1-A3, not A10)

**Root Cause:**
- Content authoring bottleneck (requires narrative design + JSON creation)
- Procedural system prioritized (needed for infinite content)
- Tutorial extension lower priority than core systems

**Remediation Plan:**
1. **Narrative Design for A4-A10** (3 weeks):
   - Design 7 scenes bridging A3 → A11+
   - Introduce: Multi-situation scenes, complex transitions, dependent resources
   - Establish: Pursuit narrative, regional unlocking, procedural setup

2. **JSON Authoring** (4 weeks):
   - Create SceneTemplates for A4-A10
   - Test each scene in isolation
   - Validate structural guarantees (no soft-locks)

3. **Integration Testing** (2 weeks):
   - Playthrough: A1 → A10 full sequence
   - Verify: Narrative coherence, difficulty curve, teaching progression
   - Polish: Adjust balance, fix continuity issues

4. **Procedural Transition** (1 week):
   - Trigger A11+ after A10 completion
   - Ensure seamless hand-off
   - Test: A10 → A11 feels natural

**Estimated Effort**: 10 weeks
**Risk**: Low (doesn't block procedural system)
**Assigned**: Content Authoring Team

---

## 11.4 Risk Mitigation Summary

### Immediate Actions (Next 3 Months)

| Risk/Debt | Action | Owner | Priority |
|-----------|--------|-------|----------|
| RISK-001 (Procedural Quality) | Implement validation framework | Content Gen | HIGH |
| RISK-003 (Catalogue Balance) | Balance testing suite | Game Design | HIGH |
| RISK-005 (Save Corruption) | Save validation + versioning | Save System | HIGH |
| DEBT-001 (Categorical Props) | Create missing catalogues | Content Pipeline | HIGH |
| DEBT-002 (AI Integration) | Universal fallback | AI Integration | HIGH |

### Monitoring & Telemetry (Ongoing)

- **Performance**: 95th percentile response times for critical operations
- **Quality**: Player feedback surveys, archetype distribution analysis
- **Balance**: Wealth curves, success rates, stat distributions
- **AI**: Generation success rate, latency, fallback frequency
- **Integrity**: Save success rate, load failure rate, corruption reports

### Quarterly Review

- Re-evaluate risk likelihood/impact based on telemetry
- Adjust mitigation priorities based on player feedback
- Review technical debt remediation progress
- Update risk register with new identified risks

---

## Related Documentation

- **10_quality_requirements.md** - Quality scenarios validating risk mitigation
- **09_architecture_decisions.md** - ADRs addressing some risks through design
- **01_introduction_and_goals.md** - Quality goals affected by risks
- **IMPLEMENTATION_STATUS.md** - Current implementation state (debt tracking)
