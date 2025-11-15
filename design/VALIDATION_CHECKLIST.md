# Wayfarer Situation Design: Pre-Commit Validation Checklist

## Purpose

This is the **canonical validation checklist** for all Wayfarer situation designs. Use this checklist before committing any situation (A-story or B-story) to ensure balance, playability, and consistency.

**Status:** Authoritative validation requirements
**Last Updated:** 2025-11

---

## How to Use This Checklist

1. **Complete every section** for every situation before committing
2. **ALL checkboxes must be checked** - if ANY fails, redesign before committing
3. **Document exceptions** - if a check doesn't apply, note why (e.g., "B-story, no progression requirement")
4. **Create comparison matrix** - for Reward Proportionality section, explicitly compare choice rewards

**Referenced by:**
- [08_balance_philosophy.md](08_balance_philosophy.md) Section 8.11
- [DESIGN_GUIDE.md](DESIGN_GUIDE.md) Pre-Commit Validation Checklist section
- [VALIDATION_TOOLING_PLAN.md](VALIDATION_TOOLING_PLAN.md) Automated validation targets

---

## Structural Validation

### Choice Count

- [ ] **Minimum 2 choices for ALL situations**
  - NO single-choice situations (that's a cutscene, not a choice)
  - FAIL if choices < 2

- [ ] **A-story: 4-6 choices typical**
  - Typical pattern: Stat-gated + Money-gated + Challenge + Fallback (+ optional alternatives)
  - WARN if A-story has < 4 choices (may lack variety)

- [ ] **B-story: 2-4 choices acceptable**
  - Simpler structure acceptable for side content
  - PASS if B-story has 2+ choices

### Progression Guarantee (A-story MANDATORY, B-story N/A)

- [ ] **At least one choice has ZERO requirements**
  - No stat requirement
  - No coin cost
  - No session resource cost
  - FAIL if A-story has no zero-requirement path

- [ ] **Fallback choice CANNOT fail**
  - Must be Instant action OR guaranteed outcome
  - NOT a Challenge path (challenges can fail)
  - FAIL if fallback can fail

- [ ] **Fallback choice SPAWNS next scene or advances story**
  - Must guarantee forward progression
  - Cannot dead-end or loop indefinitely
  - FAIL if fallback doesn't progress

- [ ] **Fallback clearly identified**
  - Labeled as fallback in design/code
  - Player can identify which path is "always available"

### Orthogonal Resource Costs

- [ ] **Each choice costs DIFFERENT resource type**
  - Resource types: Stat requirement, Coins, Session resources (Resolve/Stamina/Focus), Time, Multi-resource trade-offs
  - No two choices should cost the SAME resource with SIMILAR amounts
  - FAIL if two choices both cost 10 coins and 15 coins (false choice - cheaper always better)

- [ ] **If two choices cost same resource type:**
  - They must have PROPORTIONALLY different rewards
  - Higher cost = Better rewards
  - Example: 10 coins → +1 stat, 25 coins → +2 stats (acceptable, proportional)

---

## Rule Compliance Validation

### Rule 1: Minimum 2 Choices

- [ ] **Count >= 2**
  - Verified above in Structural Validation
  - FAIL if < 2 choices

### Rule 2: Resource Juggling

- [ ] **Every choice costs OR rewards something measurable**
  - Costs: Stats, coins, time, session resources, health, relationship
  - Rewards: Same categories
  - FORBIDDEN: Narrative-only choices with zero mechanical effect

- [ ] **No choices that ONLY progress narrative**
  - Example FORBIDDEN: "Listen to innkeeper's story" → Only narrative text, no mechanical effect
  - Example CORRECT: "Listen to innkeeper's story (2 time blocks)" → Narrative + relationship +1 + time cost
  - Progression is OUTCOME, not standalone value

### Rule 3: Requirements Justify Rewards

- [ ] **Higher stat requirement = Better rewards**
  - If Choice A requires Insight 5 and Choice B requires Insight 2, Choice A MUST have better rewards
  - If Choice A requires stat and Choice B doesn't, Choice A MUST have better rewards

- [ ] **Coin cost = Better rewards than free**
  - If Choice A costs coins and Choice B is free, Choice A MUST give better outcome
  - If Choice A costs 25 coins and Choice B costs 10 coins, Choice A MUST give proportionally better outcome

- [ ] **Challenge risk = Better potential rewards**
  - Challenge success should give better rewards than money-gated path
  - Challenge failure should give adequate rewards (not catastrophic)
  - Variable outcome justified by risk

- [ ] **Create explicit comparison matrix**
  - List all choices with costs and rewards
  - Verify proportionality (higher cost → better reward)
  - Document in situation design notes

### Rule 4: Intra-Situation Balance

- [ ] **All comparisons WITHIN this situation only**
  - Compare Choice A vs Choice B in THIS situation
  - NEVER compare to choices in OTHER situations

- [ ] **No references to other situations' costs/rewards**
  - "This costs more than situation X" is INVALID reasoning
  - "This gives less than situation Y" is INVALID reasoning

- [ ] **Cross-situation variance acceptable**
  - Situation A: +2 Insight for 10 coins
  - Situation B: +1 Insight for 15 coins
  - This is ACCEPTABLE (different contexts, different narrative weight)
  - Only compare within each situation independently

### Rule 5: Crisis Situations

- [ ] **If crisis situation: ALL choices involve losses**
  - Crisis = Damage control, not opportunity
  - Every choice loses SOMETHING (Health, coins, stats, relationship)
  - Balance through asymmetric costs (lose Health vs lose Coins vs lose Both)
  - FAIL if crisis situation has purely positive outcome

- [ ] **If normal situation: Positive outcomes allowed**
  - Most situations are NOT crisis
  - Choices grant rewards, build relationships, gain resources
  - PASS for normal situations

### Rule 6: Multi-Stat Trade-Offs (Optional)

- [ ] **If using multi-stat effects: Properly balanced**
  - Asymmetric: +2 one stat, -1 another stat (net +1, character shift)
  - Symmetric: +1 to each of two stats (total +2 distributed, breadth over depth)
  - Compound requirements: Stat A >= X AND Stat B >= Y (rewards EXCEPTIONAL)

- [ ] **Multi-stat effects not required**
  - Optional enhancement to balance
  - Frequency: 10-15% of situations
  - Not mandatory for every situation

### Rule 7: Verisimilitude in Costs

- [ ] **All costs reflect actual opportunity costs**
  - Time spent here cannot be spent elsewhere
  - Coins spent here cannot be spent elsewhere
  - Stats required were earned through XP investment

- [ ] **Costs make narrative sense**
  - "Bribe guard" costs 20 coins (makes sense)
  - "Bribe guard" costs 0 coins (FORBIDDEN - no bribe)
  - "Fast travel" costs 5 coins (makes sense)
  - "Fast travel" costs 0 coins but takes time (contradictory - not faster)

### Rule 8: Coins as Alternative

- [ ] **Coin path exists as alternative to stat path**
  - Money-gated path available alongside stat-gated path
  - Provides alternative for players without specialization

- [ ] **Coin cost valued appropriately**
  - See Scaling Validation section below
  - Must feel substantial (20-40% of typical reserve)
  - Coins function like Sir Brante willpower (meaningful choice to spend)

---

## Scaling Validation

### Stat Requirements Match Progression

- [ ] **A1-A3: No stat requirements (identity building)**
  - All choices FREE of stat gates
  - Offer stat GAINS (+1 or +2 to various stats)
  - Player builds identity through choices

- [ ] **A4-A6: Requirements 2-3 (early specialization)**
  - Player expected state: 1 stat at 2-3, others at 0-1
  - Stat-gated paths should require 2-3
  - Multiple stat options (Insight 2 OR Rapport 2 OR Authority 2)

- [ ] **A7-A12: Requirements 4-5 (moderate specialization)**
  - Player expected state: 1-2 stats at 4-5, others at 1-2
  - Stat-gated paths should require 4-5
  - Specialist players hit consistently, generalists sometimes

- [ ] **A13-A20: Requirements 6-7 (deep specialization)**
  - Player expected state: 1-2 stats at 6-7, 1 at 3-4, others 1-2
  - Primary paths: Requirement 6-7
  - Secondary paths: Requirement 4-5 (for balanced builds)

- [ ] **A21+: Requirements 7-8+ (mastery)**
  - Player expected state: 1-2 stats at 7-8, 1-2 at 4-5, others 2-3
  - Elite paths: Requirement 8+ (rare, true masters)
  - Primary paths: Requirement 6-7 (high specialists)
  - Secondary paths: Requirement 4-5 (balanced builds)

**Cross-reference:** [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) Section "Stat Requirement Scaling by Progression"

### Coin Costs Match Progression

- [ ] **Tier 1 (A4-A6): 10-15 coins**
  - Money-gated A-paths cost 10-15 coins
  - Equivalent to 1-2 B-story completions
  - Feels substantial (30-40% of 30-50 coin reserve)

- [ ] **Tier 2 (A7-A12): 20-30 coins**
  - Money-gated A-paths cost 20-30 coins
  - Equivalent to 1-2 B-story completions
  - Feels substantial (30-40% of 60-80 coin reserve)

- [ ] **Tier 3 (A13-A20): 35-50 coins**
  - Money-gated A-paths cost 35-50 coins
  - Equivalent to 1-2 B-story completions
  - Feels substantial (30-40% of 100-150 coin reserve)

- [ ] **Tier 4 (A21+): 60-100 coins**
  - Money-gated A-paths cost 60-100 coins
  - Equivalent to 1-2 B-story completions
  - Feels substantial (30-40% of 150-250 coin reserve)

**Cross-reference:** [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) Section "Money-Gated Path Costs by Tier"

### Session Resource Costs Match Progression

- [ ] **Tier 1 (A4-A6): 2-3 Resolve/Stamina/Focus**
  - Challenge paths cost 2-3 session resource
  - 20-30% of typical 10-point session pool
  - Player can attempt 2-4 challenges per session

- [ ] **Tier 2 (A7-A12): 3-4 Resolve/Stamina/Focus**
  - Challenge paths cost 3-4 session resource
  - 25-35% of typical 12-point session pool
  - Player can attempt 2-4 challenges per session

- [ ] **Tier 3 (A13-A20): 4-5 Resolve/Stamina/Focus**
  - Challenge paths cost 4-5 session resource
  - 30-40% of typical 15-point session pool
  - Player can attempt 2-3 challenges per session

- [ ] **Tier 4 (A21+): 5-6 Resolve/Stamina/Focus**
  - Challenge paths cost 5-6 session resource
  - 35-45% of typical 18-point session pool
  - Player can attempt 2-3 challenges per session

**Cross-reference:** [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) Section "Session Resource Costs by Tier"

### Time Costs (Universal, No Scaling)

- [ ] **Fallback paths: 3-5 time blocks**
  - Time cost doesn't scale with progression
  - Universal constraint (opportunity cost always meaningful)
  - 3-5 blocks ≈ 2-4 hours in-game time
  - Opportunity cost: 1-2 B-stories OR alternative A-scene progress

---

## Edge Case Validation

### Worst-Case Player

- [ ] **Player with 0 coins, 0 stats, 0 session resources can progress**
  - This is the ultimate test for A-story situations
  - Fallback path MUST be available (zero requirements)
  - FAIL if worst-case player cannot progress (soft-lock)

- [ ] **Fallback path clearly available to worst-case player**
  - No hidden requirements
  - No resource costs
  - Cannot fail

### Specialized Players

- [ ] **Insight specialist finds viable path**
  - In Insight-appropriate situations (investigation, analysis, deduction)
  - Insight stat-gated path should exist
  - Insight specialist rewarded for specialization

- [ ] **Authority specialist finds viable path**
  - In Authority-appropriate situations (command, intimidation, leadership)
  - Authority stat-gated path should exist
  - Authority specialist rewarded for specialization

- [ ] **Rapport specialist finds viable path**
  - In Rapport-appropriate situations (empathy, trust-building, emotional connection)
  - Rapport stat-gated path should exist
  - Rapport specialist rewarded for specialization

- [ ] **Diplomacy specialist finds viable path**
  - In Diplomacy-appropriate situations (negotiation, compromise, measured approach)
  - Diplomacy stat-gated path should exist
  - Diplomacy specialist rewarded for specialization

- [ ] **Cunning specialist finds viable path**
  - In Cunning-appropriate situations (subtlety, strategy, manipulation, risk management)
  - Cunning stat-gated path should exist
  - Cunning specialist rewarded for specialization

### Build Diversity

- [ ] **Balanced generalist has multiple viable options**
  - Player with all stats at 4-5 (moderate, not specialized)
  - Should have access to money, challenge, and fallback paths
  - May not unlock highest stat-gated paths (7+ requirements)
  - Still viable and fun to play

- [ ] **Scattered progression player never soft-locked**
  - Player with random stat distribution (e.g., Rapport 3, Cunning 2, Authority 6, others 1)
  - Money-gated path always available
  - Challenge path always available
  - Fallback always available (A-story)
  - May unlock occasional stat-gated path when their one high stat applies

- [ ] **Wealthy player can use economic power**
  - Money-gated path exists
  - Cost feels meaningful (not trivial)
  - Economic playstyle viable

- [ ] **Skilled player can demonstrate skill**
  - Challenge path exists
  - Success rewards better than money-gated path
  - Failure still acceptable (not catastrophic)
  - Skill expression valued

**Cross-reference:** [DESIGN_GUIDE.md](DESIGN_GUIDE.md) Section "Scattered Stat Progression: Deep Analysis and Handling"

---

## Verisimilitude Validation

### Narrative Context

- [ ] **Narrative context supports ALL paths**
  - Each path makes sense in this specific situation
  - NPC personality/demeanor supports paths
  - Location/environment supports paths
  - Stakes/urgency supports paths

- [ ] **No logical contradictions between paths**
  - "Fast travel" costs time (contradictory) - FORBIDDEN
  - "Bribe guard" costs 0 coins (contradictory) - FORBIDDEN
  - "Buy sword" is free (contradictory) - FORBIDDEN

### Requirements and Costs

- [ ] **Requirements justified by fiction**
  - High Authority requirement: NPC respects command presence
  - High Insight requirement: Challenge requires analytical thinking
  - High Rapport requirement: NPC values emotional connection
  - Coin cost: Actual payment/bribe/purchase
  - Time cost: Help/patience/waiting

- [ ] **Player can understand WHY requirement exists**
  - Not arbitrary numbers
  - Emerges from narrative context
  - Makes intuitive sense

### Rewards

- [ ] **Rewards match effort narratively**
  - Best mechanical rewards = Best narrative outcomes (deep relationship, exceptional result)
  - Good mechanical rewards = Good narrative outcomes (positive relationship, complete result)
  - Minimal mechanical rewards = Adequate narrative outcomes (functional relationship, basic result)

- [ ] **Relationship changes reflect interaction quality**
  - Stat-gated path (specialization): Relationship +2 (impressive, creates bond)
  - Money-gated path (transaction): Relationship +1 or +0 (transactional, acceptable)
  - Challenge success (skill): Relationship +2 (earned respect)
  - Challenge failure (attempt): Relationship +0 or -1 (tried but failed)
  - Fallback (patience): Relationship +1 (earned through time)

### Mechanics Emerge from Fiction

- [ ] **Categorical properties drive costs**
  - NPCDemeanor: Friendly (easier/cheaper) vs Hostile (harder/expensive)
  - Quality: Basic (cheap) vs Premium (expensive)
  - PowerDynamic: Dominant (Authority easier) vs Submissive (Rapport easier)
  - EnvironmentQuality: Affects restoration and comfort

- [ ] **Costs not arbitrary**
  - Can explain to player WHY this costs what it costs
  - Fiction justifies mechanics
  - No "because game balance" reasoning needed

---

## Reward Proportionality

### Create Comparison Matrix

For EVERY situation, create explicit comparison matrix:

| Path | Cost | Relationship | Other Rewards | Total Value | Rank |
|------|------|--------------|---------------|-------------|------|
| Stat-gated | [Stat X] | +2 | +2 Insight, bonus item | Highest | 1 |
| Money-gated | [Y] coins | +1 | +1 Insight | Good | 2 |
| Challenge (success) | [Z] Resolve | +2 | +2 Insight | Excellent | 1-2 |
| Challenge (failure) | [Z] Resolve | +0 | +1 Insight | Adequate | 3 |
| Fallback | [W] time | +1 | +1 Insight | Minimal | 4 |

### Verify Proportionality

- [ ] **Stat-gated path: Best rewards**
  - Highest relationship gain (+2 typical)
  - Best bonuses (items, extra stat gains, detailed information)
  - Justifies permanent XP investment

- [ ] **Money-gated path: Good rewards**
  - Moderate relationship gain (+1 typical)
  - Complete outcome (not degraded)
  - Reliable, instant
  - Justifies substantial coin cost

- [ ] **Challenge success: Excellent rewards**
  - Equal to or better than money-gated
  - Often equal to stat-gated
  - High relationship gain (+2 typical)
  - Justifies risk and skill requirement

- [ ] **Challenge failure: Adequate rewards**
  - Still progresses (NOT catastrophic)
  - Better than or equal to fallback
  - Lower relationship gain (+0 or -1 typical)
  - Risk justified by potential success

- [ ] **Fallback: Minimal rewards**
  - Base outcome achieved (progression guaranteed)
  - Moderate relationship gain (+1 typical from patience/help)
  - No bonuses
  - Slowest path (time cost)

### Reward Balance Rules

- [ ] **Higher cost = Better rewards**
  - Verified via comparison matrix
  - No exceptions (false choices if violated)

- [ ] **No dominant strategies**
  - No choice strictly better than another in all contexts
  - Different player builds prefer different paths
  - Stat specialist prefers stat path
  - Wealthy player prefers money path
  - Skilled player prefers challenge path
  - Worst-case player uses fallback

---

## Final Review Questions

### Impossible Choice

- [ ] **Does this situation create impossible choice?**
  - All paths genuinely viable in different contexts
  - Different player builds prefer different paths
  - Strategic tension in selection
  - Player feels trade-off pain (choosing means NOT choosing others)

### Playability

- [ ] **Would I want to play this?**
  - Does it feel fair? (No cheap gotchas, hidden penalties)
  - Does it feel interesting? (Meaningful choice, not trivial)
  - Does it feel appropriate? (Fits context, progression level, narrative)

### Justification

- [ ] **Can I justify this to a player asking "Why?"**
  - "Why does Path A cost more than Path B?" → Has clear answer
  - "Why does this NPC require this stat?" → Has fiction justification
  - "Why do all paths give same base outcome?" → Design philosophy (no false choices, forward progression)

---

## Validation Pass Criteria

**To commit a situation design:**
- [ ] **ALL checkboxes above are checked**
- [ ] **Comparison matrix created and verified**
- [ ] **No FAIL conditions triggered**
- [ ] **All three final review questions answered confidently**

**If ANY checkbox unchecked:**
- STOP - Do not commit
- Identify violation
- Redesign situation
- Re-validate with checklist
- Only commit when ALL boxes checked

**Exception documentation:**
- If check doesn't apply (e.g., "B-story, no fallback required"), note: "N/A - B-story"
- Document WHY check doesn't apply
- Ensure exception is legitimate

---

## Related Documentation

### Balance Philosophy and Methodology
- **[08_balance_philosophy.md](08_balance_philosophy.md)** Section 8.11 - Detailed validation explanations
- **[DESIGN_GUIDE.md](DESIGN_GUIDE.md)** Pre-Commit Validation Checklist - Practical guide with flowcharts
- **[VALIDATION_TOOLING_PLAN.md](VALIDATION_TOOLING_PLAN.md)** - Automated validation implementation plan

### Baseline Values
- **[BASELINE_ECONOMY.md](BASELINE_ECONOMY.md)** - Authoritative numeric values for all costs, rewards, scaling

### Core Design Concepts
- **[01_design_vision.md](01_design_vision.md)** - Perfect information, impossible choice philosophy
- **[05_resource_economy.md](05_resource_economy.md)** - Orthogonal resources, tight margins
- **[13_player_experience_emergence_laws.md](13_player_experience_emergence_laws.md)** - Psychological principles

---

**Document Status:** Canonical validation checklist (single source of truth)
**Last Updated:** 2025-11
**Maintained By:** Design team
**Version:** 1.0
