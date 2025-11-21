# Section 8: Balance Philosophy and Difficulty Scaling

## 8.1 Overview

Wayfarer creates strategic depth through impossible choices, not mechanical complexity. Balance emerges from resource scarcity forcing players to choose between multiple valid but mutually exclusive paths. Difficulty scaling happens through categorical properties and contextual variation, not through stat inflation or artificial gates.

This section documents:
- **Balance Principles**: How challenge creates meaningful choice without frustration
- **Difficulty Scaling Mechanisms**: Categorical properties, route opacity, economic pressure
- **Four-Choice Balance Pattern**: Orthogonal resource costs ensuring viable alternatives
- **Build Diversity**: Multiple specializations, no "correct" build
- **Progression Curve**: Tight margins throughout, mastery through optimization
- **Sweet Spots and Extremes**: Balance matters, extremes have consequences
- **AI Balance Enablement**: How categorical scaling enables infinite balanced content

---

## Reading Paths: Find What You Need Quickly

This document is comprehensive (2,150+ lines). Use these reading paths to navigate efficiently based on your goal.

### Path 1: "I need to design a balanced situation RIGHT NOW"

**Quick workflow (15 minutes):**
1. Read Section 8.2.0 "Intra-Situation Choice Balance" - The 8 fundamental rules
2. Jump to Section 8.9 "Designing Balanced Situations: Step-by-Step Methodology" - Complete 5-phase process
3. Use [VALIDATION_CHECKLIST.md](VALIDATION_CHECKLIST.md) before committing

**Then consult:**
- [DESIGN_GUIDE.md](DESIGN_GUIDE.md) - Extracted practical methodology with templates and flowcharts
- [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) - Concrete numeric values for costs/rewards

**Cross-reference for details:**
- Section 8.4 "Four-Choice Balance Pattern" for path design
- Section 8.10 "Stat Requirement Scaling" for progression-appropriate thresholds

### Path 2: "I need to understand WHY we balance this way"

**Philosophy deep-dive (30-45 minutes):**
1. Section 8.2 "Core Balance Principles" - The fundamental philosophy and 8 rules explained
2. Section 8.2.1-8.2.6 - Individual principle deep-dives:
   - 8.2.1: Perfect Information (no hidden gotchas)
   - 8.2.2: Resource Scarcity Creates Challenge (not stat checks)
   - 8.2.3: Specialization Creates Capability AND Vulnerability
   - 8.2.4: No Unwinnable States (always forward progress)
   - 8.2.5: Strategic Depth Through Impossible Choices
   - 8.2.6: Mastery Through Optimization (not power creep)
3. Section 8.5 "Build Diversity and Specialization" - Why all builds are viable

**Cross-reference:**
- [design/01_design_vision.md](01_design_vision.md) - Core design philosophy
- [design/13_player_experience_emergence_laws.md](13_player_experience_emergence_laws.md) - Psychological principles

### Path 3: "How do I set appropriate costs for tier X?"

**Numeric values lookup (5-10 minutes):**
1. Section 8.10 "Stat Requirement Scaling Across Progression" - Tables by tier (A1-A3, A4-A6, A7-A12, A13-A20, A21+)
2. Section 8.3 "Difficulty Scaling Mechanisms" - Categorical property multipliers
3. Section 8.6 "Progression Curve" - Expected player state by tier

**Authoritative source:**
- [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) - Complete authoritative numeric configuration with all costs, rewards, and multipliers

**Examples:**
- Section 8.4.4 "Example Balanced Four-Choice Situation" - Concrete tier 2 example
- Section 8.9.7 "Complete Design Example Output" - A8 mid-game situation

### Path 4: "I need to understand categorical scaling for AI content"

**AI balance enablement (20-30 minutes):**
1. Section 8.8 "AI Balance Enablement" - The complete AI content generation strategy
2. Section 8.8.2 "Categorical Property Architecture" - How AI writes descriptive properties
3. Section 8.8.3 "Dynamic Scaling via Universal Formulas" - How catalogues translate to balanced numbers
4. Section 8.8.5 "Infinite Variety Through Property Combinations" - 2,000+ variations from 21 archetypes

**Cross-reference:**
- Section 8.3.1 "Categorical Property Scaling" - NPCDemeanor, Quality, PowerDynamic, EnvironmentQuality multipliers
- [design/07_content_generation.md](07_content_generation.md) - 21 situation archetypes and AI workflow

### Path 5: "How do I design crisis situations?"

**Crisis-specific guidance (15 minutes):**
1. Section 8.2.0 Rule 5 - Crisis situations definition
2. Section 8.9.8 "Crisis Situation Variation" - Design methodology for damage control
3. Section 8.4.4 Example "Crisis Situation" subsection - Bandit ambush example

**Related:**
- Section 8.2.5 "Strategic Depth Through Impossible Choices" - Asymmetric costs philosophy
- [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) Section "Crisis Situation Baseline Values"

### Path 6: "I need to validate edge cases and scattered progression"

**Edge case validation (25-35 minutes):**
1. Section 8.5 "Build Diversity and Specialization" - Multiple viable builds
2. Section 8.11.3 "Edge Case Validation" - Worst-case player, specialists, generalist, scattered
3. Review examples in Section 8.9 showing how paths accommodate different builds

**Comprehensive edge case guide:**
- [DESIGN_GUIDE.md](DESIGN_GUIDE.md) Section "Scattered Stat Progression: Deep Analysis and Handling" - 5 progression patterns, 5 design strategies, 4 validation procedures, 5 common problems with fixes

### Path 7: "What are the common mistakes and how do I avoid them?"

**Anti-patterns and fixes (20 minutes):**
1. Section 8.2.0 Rules 1-8 with FORBIDDEN examples
2. Section 8.9.6 Phase 5 "Verify Verisimilitude" - Fiction must justify mechanics
3. [VALIDATION_CHECKLIST.md](VALIDATION_CHECKLIST.md) - Canonical pre-commit validation

**Common mistakes documented:**
- Same resource different amounts (Rule violation example in 8.2.0)
- Narrative-only choices (Rule 2 violation)
- Dominant strategies (Rule 3 violation)
- Arbitrary costs (verisimilitude failure)

**Anti-pattern catalog:**
- [DESIGN_GUIDE.md](DESIGN_GUIDE.md) Section "Common Patterns and Anti-Patterns" - ✓ DO THIS vs ✗ NEVER DO THIS examples

### Path 8: "I need the complete step-by-step design process"

**Full methodology (45-60 minutes):**
1. Section 8.9 "Designing Balanced Situations: Step-by-Step Methodology" - Read entire section
   - 8.9.1: Design Process Overview (5 phases)
   - 8.9.2: Phase 1 - Establish Context
   - 8.9.3: Phase 2 - Define Outcome
   - 8.9.4: Phase 3 - Design Four Paths
   - 8.9.5: Phase 4 - Validate Balance
   - 8.9.6: Phase 5 - Verify Verisimilitude
   - 8.9.7: Complete Design Example Output
   - 8.9.8: Crisis Situation Variation
2. Section 8.10 "Stat Requirement Scaling" - Progression-appropriate thresholds
3. [VALIDATION_CHECKLIST.md](VALIDATION_CHECKLIST.md) - Final validation

**Practical companion guide:**
- [DESIGN_GUIDE.md](DESIGN_GUIDE.md) - Same 5-phase process with templates, flowcharts, decision trees, and enhanced edge case coverage

### Path 9: "How does the economy stay tight throughout progression?"

**Economic balance deep-dive (25 minutes):**
1. Section 8.3.3 "Economic Pressure (Tight Margins)" - Margins never trivialize
2. Section 8.6 "Progression Curve" - Early/mid/late game economics
3. Section 8.4.3 "Resource Competition Across Systems" - Shared scarcity

**Numeric details:**
- Section 8.10.4 "Scaling Coin Costs Alongside Stat Requirements" - Coin-to-stat equivalence formula
- [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) Section "Economic Pressure Validation" - Player reserve expectations, spending patterns, B-to-A ratios

### Path 10: "I'm implementing catalogues and need the scaling formulas"

**Technical implementation (30 minutes):**
1. Section 8.3.1 "Categorical Property Scaling" - Universal scaling properties and multipliers
2. Section 8.8.3 "Dynamic Scaling via Universal Formulas" - Formula structure and calculation examples
3. Section 8.8.4 "Relative Consistency Across Progression" - Proportional scaling guarantees

**Cross-reference:**
- [arc42/08_crosscutting_concepts.md](../08_crosscutting_concepts.md) Section "Catalogue Pattern" - Technical implementation
- [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) Section "Categorical Property Multipliers" - Exact multiplier values

---

## 8.2 Core Balance Principles

### 8.2.0 Intra-Situation Choice Balance (The Fundamental Rule)

**All choices in a situation must be balanced RELATIVE TO EACH OTHER, not relative to choices in other situations.**

This is the most fundamental balance principle. Every comparison, every evaluation, every balance judgment happens WITHIN the same situation. Cross-situation balance is deliberately avoided.

**The Core Rules:**

**Rule 1: Every situation must have at least 2 choices minimum**
- No single-choice situations (that's not a choice, it's a cutscene)
- Typical A-story situations have 4 choices (stat/money/challenge/fallback)
- B-story situations may have 2-3 choices (simpler structure)

**Rule 2: Player must always juggle resources**
- NO choice should only progress narrative without resource consequence
- Every choice costs OR rewards something measurable (stats, coins, time, resources)
- Narrative progression is OUTCOME, not standalone value
- Example FORBIDDEN: "Listen to story" → Only narrative text, no mechanical effect

**Rule 3: Choices with requirements/costs MUST give better rewards**
- If Choice A costs coins and Choice B is free, Choice A MUST give better outcome
- If Choice A has stat requirement and Choice B doesn't, Choice A MUST give better outcome
- If Choice A costs more than Choice B (same resource type), Choice A MUST give proportionally better outcome
- NO choice should be notably better than another without ALSO having higher requirement or cost

**Rule 4: Balance within situation, NOT across situations**
- Situation A might give +2 Insight for 10 coins
- Situation B might give +1 Insight for 15 coins
- This is ACCEPTABLE because they're different situations (different contexts, different narrative weight)
- Within Situation A, if one choice gives +2 Insight for 10 coins, another choice should NOT give +2 Insight for 5 coins
- Compare choices ONLY to other choices in same situation

**Rule 5: Crisis situations = All choices are damage control**
- Some situations are CRISES where every choice is bad
- All choices may REDUCE stats or apply negative consequences
- Balance: Choices trade off which damage to accept
- Example: Choice A loses 2 Health, Choice B loses 15 coins, Choice C loses 1 Rapport
- NOT balanced by rewards being equal, but by different players valuing different costs

**Rule 6: Multi-stat effects create balance through trade-offs**
- Choices can affect multiple stats (gain one, lose another)
- Example: +2 Authority, -1 Rapport (commanding presence alienates some)
- Example: +2 Insight, -1 Health (intensive mental effort exhausts body)
- Balance emerges from asymmetric exchanges (gain in one area, lose in others)

**Multi-Stat Trade-Off Ratio Guidelines:**

**Symmetric Gains (Multiple Positives):**
When choice grants MULTIPLE stats, reduce individual gains to maintain balance.

Standard single-stat gain: +2 to one stat
Multi-stat symmetric gain: +1 to each of two stats (total +2 distributed)
- Example: "+1 Insight, +1 Cunning" (analytical and strategic thinking)
- Balanced against "+2 Insight" (pure analytical specialization)
- Player chooses: Depth in one area OR breadth across two areas

Three-stat gain: +1 to each of three stats (rare, represents broad learning)
- Example: "+1 Insight, +1 Rapport, +1 Diplomacy" (holistic growth)
- Balanced against "+2 Insight" (specialist gets more depth)
- Use sparingly (encourages generalist builds, dilutes specialization)

**Asymmetric Trades (Gain One, Lose Another):**
Exchanges create interesting tension between different character aspects.

2-for-1 trade: +2 in one stat, -1 in another stat
- Example: "+2 Authority, -1 Rapport" (assertive leadership alienates)
- Net gain: +1 total stats (same as pure +1 single stat)
- BUT: Directional shift (trading social warmth for command presence)
- Use when: Choice represents philosophical shift or character transformation

3-for-1 trade: +3 in one stat, -1 in another stat
- Example: "+3 Authority, -1 Cunning" (direct confrontation, abandon subtlety)
- Net gain: +2 total stats (standard gain amount)
- Larger shift: Significant character evolution
- Use when: Dramatic choice, major character moment

Even trade: +2 in one stat, -2 in another stat
- Example: "+2 Insight, -2 Health" (intensive study causes exhaustion and neglect)
- Net gain: 0 total stats
- Pure trade-off: No power gain, only character shift
- Use when: Choice represents sacrifice or cost of obsession

**Multi-Stat Requirements:**

Single requirement: Stat ≥ 5
Multi-requirement (2 stats): Both stats ≥ 3 (total 6 points required)
- Lower individual thresholds, higher total investment
- Rewards balanced builds
- Example: "Diplomacy ≥ 3 AND Rapport ≥ 3" (empathetic negotiation)

Multi-requirement (2 stats, asymmetric): Primary ≥ 5, Secondary ≥ 2 (total 7 points)
- Specialist with secondary competence
- Example: "Insight ≥ 5 AND Cunning ≥ 2" (strategic investigation)

**Concrete Multi-Stat Choice Examples:**

**Example 1: Confrontation with Authority Figure**

Choice A (Pure Authority): "Assert your position forcefully"
- Requirement: Authority ≥ 5
- Outcome: Target backs down, you gain respect, relationship +0 (professional distance)
- Stat gain: +1 Authority
- Pure specialist path

Choice B (Balanced Approach): "Balance firmness with understanding"
- Requirement: Authority ≥ 3 AND Diplomacy ≥ 3
- Outcome: Target accepts middle ground, mutual respect, relationship +2
- Stat gain: +1 Diplomacy
- Balanced specialist path (requires total 6 points across two stats)

Choice C (Transformative Shift): "Force the issue, consequences be damned"
- Requirement: None
- Outcome: Target capitulates, others fear you, relationship -2
- Stat gain: +3 Authority, -1 Rapport
- Available to anyone, transforms character (gain command, lose warmth)

**Example 2: Intensive Study Session**

Choice A (Standard Study): "Study normally"
- Requirement: None
- Cost: 2 time blocks
- Outcome: +1 Insight
- Baseline

Choice B (Intense Focus): "Push yourself to exhaustion"
- Requirement: None
- Cost: 3 time blocks
- Outcome: +2 Insight, -2 Health (mental gains, physical toll)
- Trade-off: Knowledge for wellbeing

Choice C (Balanced Learning): "Integrate multiple disciplines"
- Requirement: None
- Cost: 3 time blocks
- Outcome: +1 Insight, +1 Diplomacy (analytical and interpersonal learning)
- Breadth over depth

**Example 3: Leadership Opportunity**

Choice A (Command): "Take charge decisively"
- Requirement: Authority ≥ 4
- Outcome: Group follows, task succeeds efficiently
- Stat gain: +1 Authority
- Specialist path

Choice B (Inspire): "Lead through connection and vision"
- Requirement: Rapport ≥ 4
- Outcome: Group enthusiastically commits, task succeeds with energy
- Stat gain: +1 Rapport
- Alternative specialist path

Choice C (Balanced Leadership): "Combine direction with empathy"
- Requirement: Authority ≥ 3 AND Rapport ≥ 3
- Outcome: Group trusts and follows, task succeeds with commitment and efficiency
- Stat gain: +1 Authority, +1 Rapport (total +2, distributed)
- Balanced path (requires more total investment: 6 vs 4 points)

**When to Use Multi-Stat Effects:**

Use symmetric multi-stat GAINS when:
- Representing holistic learning or growth
- Validating balanced builds
- Offering breadth as alternative to depth
- Frequency: 10-15% of stat-granting choices

Use asymmetric TRADES when:
- Choice represents philosophical shift
- Character transformation or sacrifice
- Dramatic moments with consequences
- Costs of specialization or obsession
- Frequency: 5-10% of stat-affecting choices

Use multi-stat REQUIREMENTS when:
- Genuinely complex challenges
- Validating balanced builds
- Representing sophisticated capability
- Offering exceptional rewards
- Frequency: 10-15% of stat-gated choices

**Balance Validation for Multi-Stat Choices:**

Test 1: Does total value match single-stat alternatives?
- "+1 Insight, +1 Cunning" (total +2) = "+2 Insight" ✓

Test 2: Is there genuine trade-off for player?
- "+2 Authority, -1 Rapport": Gain command, lose warmth ✓
- Different builds value differently ✓

Test 3: Do requirements justify rewards?
- "Authority 3 AND Rapport 3" (total 6) gives better rewards than "Authority 5" (total 5) ✓
- More investment = better outcome ✓

Test 4: Does fiction support the multi-stat effect?
- "Intensive study" causing +Insight, -Health: Yes, exhaustion is narratively appropriate ✓
- "Balanced leadership" requiring Authority AND Rapport: Yes, combining directive and empathetic ✓

**Rule 7: Versimilitude in costs (opportunity cost accuracy)**
- Costs must reflect actual opportunity cost, not just exist on paper
- Example: Resting in private room costs Time segment, but if staying the night anyway, time passes regardless
- Therefore: Alternative to resting must give reward for NOT resting (since not actually saving time)
- Example: "Study while resting" gives +1 Understanding but restores less Health
- The cost is reduced restoration, NOT time (time already spent)

**More Verisimilitude Examples:**

**Travel Costs Example:**
INCORRECT: "Fast travel" costs 5 coins, "Slow travel" costs 0 coins but takes 3 time blocks
- Problem: Player traveling either way, time passes regardless of which route
- Both routes should cost TIME, difference is in risk/encounters

CORRECT: "Safe route" costs 5 time blocks (longer, more segments), "Dangerous shortcut" costs 3 time blocks but higher risk
- Both cost time (real opportunity cost)
- Trade-off: Safety vs speed, not money vs time
- Verisimilitude: Longer route is actually longer (more segments to traverse)

**Persuasion Costs Example:**
INCORRECT: "Intimidate" costs Authority 5, "Bribe" costs 0 coins but takes longer
- Problem: Bribing with 0 coins makes no sense narratively
- Time cost alone doesn't justify success

CORRECT: "Intimidate" costs Authority 5 (stat requirement), "Bribe" costs 20 coins (actual payment)
- Both have real costs
- Trade-off: Specialization vs economic resource
- Verisimilitude: Intimidation requires commanding presence, bribery requires money

**Investigation Costs Example:**
INCORRECT: "Thorough investigation" costs 4 time blocks, finds ALL clues. "Quick investigation" costs 2 time blocks, finds SOME clues. Both free.
- Problem: No reason to ever choose quick investigation (time not that scarce, all clues strictly better)
- Dominant strategy (thorough always better)

CORRECT: "Thorough investigation" costs 4 time blocks + 3 Focus (mentally exhausting). "Quick investigation" costs 2 time blocks, no Focus (surface level).
- Both have real costs (time + mental energy vs time only)
- Trade-off: Completeness vs resource preservation
- Thorough path finds more but depletes Focus (might need that Focus for other challenges today)
- Quick path preserves Focus for other uses
- Verisimilitude: Deep investigation is mentally draining

**Equipment Purchase Example:**
INCORRECT: "Buy sword" costs 50 coins. "Borrow sword" costs 0 coins but must return later.
- Problem: Borrowing has no real cost if player never needs to return it
- "Return later" is vague promise, not mechanical cost

CORRECT: "Buy sword" costs 50 coins, permanent ownership. "Rent sword" costs 10 coins, must return after 5 days OR pay 50 coins.
- Both have real costs (purchase vs rental)
- Trade-off: Upfront investment vs short-term access
- Rental deadline creates real pressure (must complete task or lose item)
- Verisimilitude: Rental has lower upfront cost but creates obligation

**Rule 8: Coins as alternative resource (inspired by Sir Brante's willpower)**
- Coins can function like willpower: alternative to stat requirements OR way to gain greater rewards
- Example: Choice A requires Insight 5, Choice B costs 20 coins (similar value paths)
- Example: Choice A gives +1 Rapport (free), Choice B costs 15 coins and gives +2 Rapport (better but costly)
- Spending coins for better outcome is valid IF coin cost is substantial relative to current economy
- This creates "do I spend limited economic resource now for advantage, or save for future needs?"

**Practical Examples:**

**CORRECT Intra-Situation Balance (Lodging Negotiation):**
- Choice 1: Rapport 4 → Room for 3 coins, relationship +1 (stat requirement, best deal)
- Choice 2: 8 coins → Room for 8 coins, relationship 0 (money cost, neutral)
- Choice 3: Social challenge (costs Resolve) → Success: Room for 5 coins, relationship +2 | Failure: Room for 8 coins (risk/reward, skill expression)
- Choice 4: Help for 3 time blocks → Room for free, relationship +1 (time cost, guaranteed)

**Balance Analysis:**
- Choice 1 requires stat investment (permanent cost), best economic outcome
- Choice 2 requires money (depletable cost), reliable but expensive
- Choice 3 requires tactical resource + skill, variable outcome, both results acceptable
- Choice 4 requires time (opportunity cost), zero money but slow
- ALL balanced within this situation: Different costs, proportional rewards

**INCORRECT Intra-Situation Balance (FORBIDDEN):**
- Choice 1: 10 coins → +1 Insight
- Choice 2: 5 coins → +1 Insight
- Choice 3: Free → +1 Insight
- Choice 4: Insight 3 → +1 Insight

**Why Wrong:** All give same reward but costs vary wildly. Choice 3 strictly dominates all others (same reward, zero cost). Choice 1 and 2 both cost coins but 2 is cheaper (dominates 1). No reason to ever pick Choice 1 or 4. False choices.

**CORRECT Crisis Situation (Damage Control):**

Situation: "Bandit ambush - unavoidable conflict"

- Choice 1: Fight back (Authority 4) → Lose 3 Health, keep 20 coins, gain +1 Authority (stat requirement, defend property)
- Choice 2: Pay them off → Lose 20 coins, keep Health, relationship with bandits +1 (economic cost)
- Choice 3: Physical challenge → Success: Lose 2 Health, keep 15 coins | Failure: Lose 5 Health, lose 10 coins (risky)
- Choice 4: Run away → Lose all coins, lose 1 Cunning (shame), preserve Health (guaranteed escape, terrible outcome)

**Balance Analysis:**
- ALL choices involve losses (crisis = damage control)
- Different players value different resources
- Wealthy player picks Choice 2 (has coins to spare)
- Healthy player picks Choice 1 (can afford Health loss)
- Poor player forced into Choice 3 or 4 (devastating but survivable)
- Balance through asymmetric costs, not equal rewards

### 8.2.1 Perfect Information Enables Optimization, Not Artificial Difficulty

Players see exact costs, rewards, and requirements before every decision. Challenge emerges from resource allocation strategy, not from hidden information or surprise penalties.

**What Players Can Calculate**:
- Exact stat thresholds: "Need Authority 5, you have 3"
- Exact resource costs: "Costs 15 coins, you have 12"
- Exact gap to qualification: "Need 2 more Rapport to unlock this choice"
- Exact outcomes: "Success grants +5 Understanding, failure costs 10 Health"

**Challenge Emerges From**:
- Limited resources (can't afford all options)
- Opportunity costs (spending here prevents spending elsewhere)
- Build specialization (strong in some areas, weak in others)
- Time pressure (limited time blocks per day)

**Not From**:
- Hidden requirements (mystery gates)
- Surprise consequences (gotchas after commitment)
- RNG failure (unpredictable outcomes)
- Stat checks with unknown thresholds (black boxes)

**Example**: Player sees lodging costs 8 coins. They have 12 coins. They know food costs 5 coins. They calculate: "If I pay 8 for lodging, I'll have 4 left, not enough for food tomorrow." Strategic choice, perfect information.

### 8.2.2 Resource Scarcity Creates Challenge, Not Stat Checks

Difficulty comes from managing finite resources across competing demands, not from arbitrary thresholds you can't meet.

**Shared Scarcity Resources**:
- **Time**: Fixed segments per day, every action consumes time, deadlines approach
- **Health**: Permanent depletion risk, slow recovery, limited by availability
- **Stamina**: Session capacity for Physical challenges, restored by rest
- **Focus**: Session capacity for Mental challenges, restored by rest
- **Resolve**: Social challenge capacity, rebuilt through relationships
- **Coins**: Economic transactions, earned slowly, depleted by costs

**The Impossible Choice Pattern**:
"I can afford option A OR option B, but not both. Both are valid strategic choices. Both have genuine costs I must accept. Which cost will I choose to pay?"

**Not**:
"I need stat X at threshold Y, but I can't get there, so I'm blocked."

**Example**: Player has 15 Focus remaining. Mental investigation requires 12 Focus to make meaningful progress. But they also need to rest soon (Health low). Spending Focus now means delaying rest. Delaying rest risks injury. Both options viable, both have costs. Player chooses based on priorities, not gatekeeping.

### 8.2.3 Specialization Creates Both Capability AND Vulnerability

Building high in some stats inherently means being weak in others. Every build has strengths and weaknesses. No build dominates all contexts.

**Specialization Emerges From**:
- Limited total XP across entire game
- Five stats competing for investment (Insight, Rapport, Authority, Diplomacy, Cunning)
- Situations requiring different stat combinations
- No way to max everything

**Capability**: High Insight unlocks investigative approaches
- "Analyze evidence systematically" path available
- Mental challenges easier
- Investigative scenes have optimal paths

**Vulnerability**: Low Authority means social confrontation difficult
- "Assert command" path locked
- Authority-gated situations force suboptimal choices
- Confrontation scenes become crises

**Build Identity Through Constraints**:
- Investigator (Insight + Cunning): Dominates investigations, weak in direct confrontations
- Diplomat (Rapport + Diplomacy): Dominates conversations, weak in subtle manipulation
- Leader (Authority + Diplomacy): Commands respect, weak in analysis and subterfuge

**Example**: Scene presents "Confront corrupt magistrate." High Authority build: Intimidate path unlocked (best outcome). Low Authority build: Must use expensive money path, risky challenge path, or patient fallback path. Specialization created advantage, but elsewhere that player will be weak.

### 8.2.4 No Unwinnable States - Always Forward Progress

In an infinite game, a single soft-lock is catastrophic. Every A-story situation MUST have guaranteed path forward, even if suboptimal.

**Guaranteed Path Requirements**:
1. Zero stat requirements (always visible)
2. Zero coin requirements (always affordable)
3. Cannot fail (Instant action or guaranteed challenge)
4. Advances progression (spawns next scene or transitions)

**Four-Choice Pattern Ensures This**:
- Stat-gated path: Best rewards IF qualified
- Money-gated path: Good rewards IF affordable
- Challenge path: Variable outcome, both success/failure advance
- Fallback path: ALWAYS available, minimal rewards, ALWAYS advances

Player chooses HOW to progress (optimal/reliable/risky/patient), never IF they progress.

**Example**: Player arrives at next A-story scene with zero coins, low stats, depleted resources. Challenge path might fail, causing setback. But fallback path always exists: "Wait patiently, they'll come around eventually" or "Help with something they need first." Takes time (opportunity cost), minimal rewards (narrative cost), but ALWAYS progresses story.

### 8.2.5 Strategic Depth Through Impossible Choices

The best choices create genuine dilemmas where all options are valid but mutually exclusive. Player reveals values through what they sacrifice.

**What Creates Impossible Choices**:
- Orthogonal resource costs (different choices cost different resources)
- Multi-resource trade-offs (gain in one area, lose in others)
- Asymmetric exchanges (gain 2 in X, lose 5 in Y)
- Time as universal constraint (every action closes time-dependent options)

**Not Impossible Choices**:
- One option strictly better (dominates)
- One option strictly worse (never chosen)
- Fake choices (all lead to same outcome)
- Irrelevant choices (no meaningful consequences)

**Example Impossible Choice**:
- Option A: Pay 15 coins → Instant success, preserve time, deplete economy
- Option B: Spend 3 time blocks helping → Free, build relationship, lose time
- Option C: Authority 5 path → Free IF qualified, instant, requires specialization
- Option D: Physical challenge → Risk injury, demonstrate capability, variable outcome

All four viable depending on: Current resources, build specialization, time pressure, player priorities. No universally correct answer. Player chooses what they value most and accepts the cost.

### 8.2.6 Mastery Through Optimization, Not Power Creep

Skilled players improve at resource management and strategic planning, not by grinding stats until invulnerable.

**Mastery Rewards**:
- **Route Learning**: Known routes show exact segment count, player calculates precisely
- **Economic Efficiency**: Tight margins reward skillful resource allocation
- **Tactical Improvement**: Better card play increases challenge success rates
- **Relationship Investment**: Building bonds unlocks shortcuts and better options
- **Build Synergy**: Understanding stat combinations unlocks optimal paths

**Not Mastery**:
- Stat grinding until all thresholds trivial
- Accumulating so much wealth that coin costs meaningless
- Finding exploits that bypass intended costs
- Power creep making earlier challenges obsolete

**Example**: Expert player knows Northern Road has 4 segments, costs 8 stamina, 2 time blocks, contains 1 dangerous segment. They plan: "I have 20 stamina, I'll arrive with 12, enough for one Physical challenge. I'll time arrival for evening, rest immediately, wake refreshed for investigation." New player: "I'll travel and figure it out." Both reach destination, but expert optimized resources and time through knowledge, not power.

## 8.3 Difficulty Scaling Mechanisms

### 8.3.1 Categorical Property Scaling

Entity properties drive contextual difficulty automatically. Same archetype + different entity properties = appropriate challenge for context.

**Universal Scaling Properties**:

**NPCDemeanor** (Friendly/Neutral/Hostile):
- Friendly: 0.6× stat threshold (easier to persuade)
- Neutral: 1.0× stat threshold (baseline)
- Hostile: 1.4× stat threshold (harder to persuade)

**Quality** (Basic/Standard/Premium/Luxury):
- Basic: 0.6× coin cost (cheap services)
- Standard: 1.0× coin cost (baseline)
- Premium: 1.6× coin cost (expensive services)
- Luxury: 2.4× coin cost (very expensive services)

**PowerDynamic** (Dominant/Equal/Submissive):
- Dominant: 0.6× authority check (player has power)
- Equal: 1.0× authority check (balanced)
- Submissive: 1.4× authority check (NPC has power)

**EnvironmentQuality** (Basic/Standard/Premium):
- Basic: 1.0× restoration (minimal comfort)
- Standard: 2.0× restoration (good comfort)
- Premium: 3.0× restoration (exceptional comfort)

**How Scaling Works**:

Archetype defines base values for stat threshold, coin cost, and challenge threshold as starting points for scaling.

Entity properties scale at parse-time. Friendly innkeepers with friendly demeanor multiplier produce easier stat thresholds. Hostile merchants with hostile demeanor multiplier produce harder stat thresholds. Premium inns with premium quality multiplier produce expensive coin costs. Basic shelters with basic quality multiplier produce cheap coin costs.

**Compound Scaling**:

Friendly innkeeper at premium inn combines friendly demeanor reducing stat threshold with premium quality increasing coin cost, creating low social barrier but high economic barrier for access.

Hostile merchant at basic stall combines hostile demeanor increasing stat threshold with basic quality reducing coin cost, creating high social barrier but low economic barrier if negotiation succeeds.

**Why This Works**:
- Same archetype reused infinitely
- Different property combinations = contextually appropriate difficulty
- AI authors descriptive properties ("friendly", "premium"), not numbers
- Catalogues handle math via universal formulas
- Relative consistency: Friendly ALWAYS easier than Hostile regardless of base values

### 8.3.2 Route Opacity (Known vs Unknown)

Routes have two states: Known (explored previously) and Unknown (never traveled). Known routes provide perfect information, Unknown routes require estimation and carry uncertainty.

**Known Route Display**:

A previously traveled northern road shows exact information: precise segment count, specific positions of dangerous segments, exact time requirement, precise stamina cost per segment, and known random event location. Player calculates exactly based on complete information, enabling precise resource planning and preparation decisions.

**Unknown Route Display**:

An unexplored northern road shows estimated ranges: distance ranging across multiple possible segment counts, general wilderness danger expectations, time range spanning multiple blocks, and stamina cost range. Player estimates conservatively, bringing extra supplies as safety buffer. Uncertainty increases difficulty through requiring buffer resources.

**Learning Through Travel**:
First journey on Northern Road: Unknown, player over-prepares, inefficient
Second journey on Northern Road: Known, player packs exactly what's needed, efficient

Mastery = Route knowledge enabling optimization.

### 8.3.3 Economic Pressure (Tight Margins)

Economy stays tight throughout game. Early game = barely scraping by. Late game = comfortable but not trivial. Margins never become so large that coin costs meaningless.

**Early Game Economics** (Tier 1: A1-A6):
- Earn 5-10 coins per B-story completion (typical 7-8)
- Food costs 5 coins standard meal (half your earnings)
- Lodging costs 8 coins standard room (full B-story earnings)
- Money-gated A-paths cost 10-15 coins (1-2 B-stories required)
- Margins: Tight, every choice matters

**Mid Game Economics** (Tier 2: A7-A12):
- Earn 15-25 coins per B-story completion (typical 18-20)
- Food costs 6 coins standard meal (smaller percentage)
- Lodging costs 12 coins standard room (affordable but still meaningful)
- Money-gated A-paths cost 20-30 coins (1-2 B-stories required)
- Margins: More comfortable, but still require planning

**Late Game Economics** (Tier 3: A13-A20):
- Earn 30-50 coins per high-tier B-story completion (typical 35-40)
- Food costs 7 coins standard meal (minor expense)
- Standard lodging costs 18 coins, premium lodging costs 35 coins (still significant)
- Money-gated A-paths cost 35-50 coins (1-2 B-stories required)
- Margins: Comfortable buffer, but premium options expensive

**Very Late Game Economics** (Tier 4: A21+):
- Earn 50-80 coins per high-tier B-story completion (typical 60-65)
- Food costs 8 coins standard meal (minor expense)
- Standard lodging costs 25 coins, premium lodging costs 50 coins
- Money-gated A-paths cost 60-100 coins (1-2 B-stories required)
- Margins: High numbers, but proportionally tight

**Why Margins Stay Tight**:
- Premium options scale with progression (luxury inn 50 coins vs basic inn 8 coins)
- Multiple simultaneous needs (lodging + food + equipment)
- Optional high-value purchases (training, rare items, information)
- Money-gated paths compete with economic security

**Example Late Game** (Tier 3): Player has 125 coins (comfortable by early game standards). But upcoming A-scene money-gated path costs 45 coins. Premium lodging costs 35 coins. Training session costs 60 coins. Still making trade-offs, just at higher numerical scale. Margins never trivialize into infinite wealth.

### 8.3.4 Stat Gating (Qualitative Difference, Not Just Easier)

Higher stats don't make game easier by lowering difficulty. Higher stats unlock DIFFERENT game by opening alternative paths unavailable to other builds.

**Stat Gating Philosophy**:
- NOT: "High Authority makes all social checks easier"
- YES: "High Authority unlocks directive approaches unavailable to low-Authority builds"

**Example Scene**: "Negotiate access to restricted archives"

**Low Authority Build** (Authority 2):
- Stat-gated "Command entry" path: LOCKED (requires Authority 5)
- Available options:
  - Money path: Pay 25 coins bribe (reliable but expensive)
  - Challenge path: Social challenge (risky, variable outcome)
  - Fallback path: Wait days for bureaucratic approval (slow, guaranteed)

**High Authority Build** (Authority 6):
- Stat-gated "Command entry" path: UNLOCKED (free, instant, best outcome)
- Available options:
  - Authority path: Assert official authority (free, immediate access)
  - Money path: Still available (unnecessary but option remains)
  - Challenge path: Still available (unnecessary but option remains)
  - Fallback path: Still available (unnecessary but option remains)

**What Changed**:
- High Authority build gained NEW option (command entry)
- Other options unchanged in difficulty
- Different playstyle, not easier playstyle
- Strategic choice remains: Use Authority path OR preserve alternative approaches

**Build Variety Through Stat Gating**:

Investigator Build (Insight 7, Authority 2):
- Investigation scenes: Multiple stat-gated paths unlocked
- Social confrontation scenes: Only expensive/risky/slow paths available
- Plays as cerebral investigator, struggles with authority figures

Leader Build (Authority 7, Insight 2):
- Social confrontation scenes: Multiple stat-gated paths unlocked
- Investigation scenes: Only expensive/risky/slow paths available
- Plays as commanding presence, struggles with complex analysis

**Not Power Creep**: Specialization creates asymmetric capability, not universal superiority.

### 8.3.5 Time Pressure (Forced Prioritization)

Time is universal constraint. Every action consumes time blocks. Days advance, expirations approach, opportunities close. Player forced to prioritize: Which fires to fight? Which opportunities to pursue?

**Time Structure**:
- 4 periods per day (Morning, Midday, Afternoon, Evening)
- 4 segments per period (16 total segments per day)
- Most actions cost 1-3 segments
- When period exhausted, advance to next
- Evening ends, day advances

**Time as Strategic Constraint**:

Scene A: Available for 5 days, high value
Scene B: Available for 3 days, moderate value
B-story: Available indefinitely, resource income

Player has 2 days remaining before Scene B expires.

**Strategic Calculation**:
"If I pursue Scene A now, Scene B expires (lose opportunity). If I pursue Scene B now, I preserve opportunity but delay Scene A. If I do B-story first, both scenes remain but I don't earn resources. Which do I prioritize?"

**Time Pressure Creates Impossible Choices**:
- Can't do everything (must choose)
- Optimal path unknowable (depends on future opportunities)
- Specialization matters (some paths faster than others)
- Resource accumulation competes with progression

**Example**: Player has 8 segments remaining in afternoon period.
- Mental investigation requires 6 segments to complete
- Eating requires 2 segments
- Period ends at 12 segments

Options:
- Investigate now (6 segments), eat after (2 segments), advance to evening
- Eat first (2 segments), investigate (6 segments), advance to evening
- Skip investigation, pursue different goal, eat later

Time forces decision. Cannot do both investigation AND other meaningful action. Prioritization required.

## 8.4 Four-Choice Balance Pattern

### 8.4.1 Pattern Structure

Every A-story situation presents 4 choice types with orthogonal resource costs. Pattern ensures guaranteed progression while maintaining meaningful strategic choice.

**The Four Paths**:

1. **Stat-Gated Path** (PathType.InstantSuccess)
   - Requirement: Player stat ≥ threshold
   - Cost: Free (no consumable resources)
   - Outcome: Best rewards
   - Purpose: Reward build specialization and character investment

2. **Money-Gated Path** (PathType.InstantSuccess)
   - Requirement: None (always visible)
   - Cost: Coins (expensive but affordable via B-story earnings)
   - Outcome: Good rewards (reliable, efficient)
   - Purpose: Reward economic accumulation from side content

3. **Challenge Path** (PathType.Challenge)
   - Requirement: None (always visible)
   - Cost: Resolve/Stamina/Focus (tactical session resource)
   - Outcome: Variable (success = excellent, failure = setback BUT STILL ADVANCES)
   - Purpose: Tactical gameplay integration, skill expression

4. **Guaranteed Path** (PathType.Fallback)
   - Requirement: None (always visible, always selectable)
   - Cost: Time (wait days, help with their needs, persistent effort)
   - Outcome: Minimal rewards, poor efficiency, but GUARANTEED progression
   - Purpose: Prevent soft-locks, reflect real relationship building

### 8.4.2 Orthogonal Resource Costs (The Key to Balance)

Balance requires each choice costing DIFFERENT resource type. If two choices cost same resource, one dominates and creates false choice.

**Test for Orthogonality**: Does each choice consume different resource?

**CORRECT Example**:
- Choice 1 (Stat path): Costs permanent character build (XP invested earlier)
- Choice 2 (Money path): Costs consumable coins (earned from side content)
- Choice 3 (Challenge path): Costs session resource (Resolve/Stamina) + time
- Choice 4 (Fallback path): Costs time (multiple days, opportunity cost)

All four cost DIFFERENT resources. Player selects based on availability and strategic priorities. No universal best choice.

**INCORRECT Example** (FORBIDDEN):
- Choice 1: Pay 5 coins → Success
- Choice 2: Pay 8 coins → Better success
- Choice 3: Challenge → Variable success
- Choice 4: Free → Poor success

Choices 1 and 2 both cost coins. If player has 8 coins, Choice 2 strictly dominates Choice 1. False choice. One option never used.

**Why Orthogonality Creates Balance**:

Shared resource = Direct comparison:
- "Option A costs 5 coins, Option B costs 8 coins, B gives better rewards"
- Player with 8 coins: B dominates (better rewards, affordable)
- A becomes dead option (never chosen)

Orthogonal resources = Strategic trade-off:
- "Option A costs 15 coins, Option B costs 3 time blocks"
- Player compares: "I need coins for lodging later, but I have time to spare. I'll choose B."
- Different player: "I'm on deadline, I have coins. I'll choose A."
- Both options viable depending on player situation

### 8.4.3 Resource Competition Across Systems

Four-choice pattern interacts with shared resource systems. Player must allocate finite resources across competing demands.

**Coins Compete Across**:
- Money-gated A-story paths (advance plot)
- Lodging and food (survival needs)
- Equipment purchases (preparation)
- Training sessions (stat improvement)
- Information purchases (shortcut unlocks)

**Time Competes Across**:
- A-story progression (main plot)
- B-story completion (resource income)
- Rest and recovery (health/stamina restoration)
- Relationship building (NPC conversations)
- Route exploration (map knowledge)

**Stamina/Focus/Resolve Compete Across**:
- Challenge paths in A-stories (plot advancement)
- B-story challenges (resource earning)
- Mental investigations (gathering information)
- Physical obstacles (accessing locations)
- Social conversations (building relationships)

**Example Competition**: Player has 15 coins, 8 segments remaining.

Option 1: Pursue A-story money-gated path now
- Cost: 15 coins (depletes economy)
- Benefit: Plot advances, new region unlocked
- Consequence: No money for food tomorrow, must earn via B-story

Option 2: Do B-story for income first
- Cost: 4 segments (time)
- Benefit: Earn 12 coins (comfortable buffer)
- Consequence: Delay A-story, less time for other goals today

Option 3: Rest to restore resources
- Cost: 4 segments (time)
- Benefit: Health/stamina restored (prepares for challenges)
- Consequence: A-story delayed, no income earned

All three options viable. No universal best choice. Strategic priorities determine selection. THIS is what creates strategic depth.

### 8.4.4 Example Balanced Four-Choice Situation

**Context**: Inn negotiation scene, player needs lodging for rest

**Choice 1: Rapport Path** (Stat-Gated)
- Requirement: Rapport ≥ 4
- Cost: Free
- Outcome: Innkeeper offers room at discount (3 coins instead of 8), relationship +1
- Player Value: High Rapport build gets best deal, reinforces specialization

**Choice 2: Money Path** (Reliable)
- Requirement: None
- Cost: 8 coins (standard room price)
- Outcome: Room secured immediately, no relationship change
- Player Value: Accessible to all builds if affordable, reliable but expensive

**Choice 3: Social Challenge** (Risky)
- Requirement: None
- Cost: 2 Resolve (enter Social challenge session)
- Outcome Success: Room for 5 coins, relationship +2, Understanding +1
- Outcome Failure: Room for 8 coins (normal price), relationship unchanged
- Player Value: Skill expression, potential savings, both outcomes progress

**Choice 4: Help Path** (Guaranteed)
- Requirement: None
- Cost: 3 time blocks (help clean common room, serve customers)
- Outcome: Room for free, innkeeper grateful, relationship +1
- Player Value: Always available, time-costly but zero coin cost, builds relationship

**Balance Analysis**:

Orthogonal costs:
- Choice 1: Character build (permanent investment)
- Choice 2: Coins (consumable resource)
- Choice 3: Resolve (session resource) + tactical skill
- Choice 4: Time (opportunity cost)

Resource trade-offs:
- High Rapport player: Choice 1 obvious (best deal, reinforces build)
- Wealthy player on deadline: Choice 2 fast and affordable
- Skilled player with time: Choice 3 for best outcome if successful
- Poor player with time: Choice 4 zero coin cost, builds relationship for future

No universal best choice. Context determines optimal selection. All four viable in different situations. THIS is balanced design.

## 8.5 Build Diversity and Specialization

### 8.5.1 Multiple Viable Specializations

Five stats provide foundation for diverse builds. Each stat unlocks different gameplay approaches and creates different vulnerability patterns.

**Five Core Stats**:
- **Insight**: Pattern recognition, analysis, deduction
- **Rapport**: Empathy, connection, emotional intelligence
- **Authority**: Command, decisiveness, power projection
- **Diplomacy**: Balance, patience, measured approach
- **Cunning**: Subtlety, strategy, risk management

**Investigative Specialist** (Insight 7, Cunning 6):
- Strengths: Mental investigations, pattern recognition, strategic information gathering
- Weaknesses: Direct confrontation, commanding respect, emotional connection
- Gameplay: Dominates investigation scenes, struggles in authority-based confrontations
- Viable: Yes, entire game completable with this build

**Social Specialist** (Rapport 7, Diplomacy 6):
- Strengths: Building relationships, emotional persuasion, balanced conversation
- Weaknesses: Analysis, subtle manipulation, command
- Gameplay: Dominates relationship-building, struggles with complex investigations
- Viable: Yes, relationship paths unlock alternatives to investigation

**Leadership Specialist** (Authority 7, Diplomacy 5):
- Strengths: Command respect, directive approach, balanced authority
- Weaknesses: Insight-based analysis, subtle cunning, emotional rapport
- Gameplay: Commands situations directly, struggles when subtlety required
- Viable: Yes, authority paths bypass need for investigation or rapport

**Subtle Manipulator** (Cunning 7, Rapport 5):
- Strengths: Strategic conversation, risk management, emotional reading
- Weaknesses: Direct authority, complex analysis, patience
- Gameplay: Navigates challenges through manipulation, struggles with direct confrontation
- Viable: Yes, cunning paths and emotional connection cover most situations

**Balanced Generalist** (All stats 4-5):
- Strengths: Flexibility, handles most situations adequately
- Weaknesses: Never dominates, always uses suboptimal paths
- Gameplay: Moderate capability everywhere, excellence nowhere
- Viable: Yes, money-gated and challenge paths available when stat paths locked

### 8.5.2 No "Correct" Build

Game validates all specializations as viable. No build required for completion. Different builds experience different game via different paths.

**Design Validation**:
- Every A-story situation has non-stat-gated paths (money, challenge, fallback)
- Specialization unlocks optimal paths, doesn't gate progression
- Different stats unlock different narrative perspectives
- Trade-offs exist for all builds (strengths and weaknesses)

**Example Validation**:

Scene: "Gain access to restricted area"

High Authority Build (Authority 7):
- Unlocked: "Command entry with authority" (free, instant)
- Available: Money path (unnecessary), challenge path (unnecessary), fallback path (unnecessary)
- Experience: Commanding presence opens doors directly

Low Authority Build (Authority 2):
- Locked: Authority path (requires Authority 5)
- Available: "Pay official bribe" (20 coins), Social challenge (risky), "Wait for bureaucratic approval" (3 days)
- Experience: Must use resources or time to achieve what Authority build gets free

Both builds complete scene. Different experiences. Different costs. Neither "correct." Authority build saved coins and time. Low Authority build might have coins from B-stories or time from efficient earlier routing. Trade-offs exist, neither build strictly superior.

### 8.5.3 Player Skill Expression

Beyond build specialization, player skill improves via resource optimization, route learning, and tactical execution.

**Resource Optimization**:
- Expert identifies minimum costs for goals
- Knows when to spend coins vs time vs tactical resources
- Maintains economic buffer through strategic B-story selection
- Times rest periods to minimize waste

**Route Learning**:
- Known routes show exact segment counts and costs
- Expert calculates precise arrival resources
- Plans multi-leg journeys accounting for intermediate rest
- Identifies shortcuts through route network knowledge

**Tactical Improvement**:
- Challenge success rates improve with card play skill
- Expert players maximize resource accumulation in sessions
- Better understanding of Action/Foundation/Discard patterns
- Identifies when to push for victory vs when to retreat safely

**Economic Efficiency**:
- Tight margins reward careful spending
- Expert knows which premium purchases worth cost
- Identifies high-value B-stories for coin income
- Maintains strategic reserve for unexpected expenses

**Relationship Investment**:
- Building NPC bonds unlocks shortcuts
- Expert prioritizes relationships strategically
- Knows which NPCs provide most valuable help
- Balances relationship building with other progression

**Example Skill Expression**:

Novice player:
- Travels unknown route, over-prepares, arrives with excess resources (wasteful)
- Does B-story whenever low on coins (reactive)
- Challenges when resources available (opportunistic)
- Result: Completes game but inefficiently, tight margins throughout

Expert player:
- Travels known routes with exact resources (optimized)
- Does B-stories proactively when high-value scenes available (strategic)
- Challenges when resources adequate AND failure manageable (calculated risk)
- Result: Completes game efficiently, maintains comfortable buffer

Both complete game. Expert experience smoother through optimization, not power. Skill expression through strategy, not grinding.

## 8.6 Progression Curve

### 8.6.1 Early Game: Tight Margins, Limited Options, Tutorial Safety Nets

**Characteristics**:
- Economy: 5-10 coins per B-story, food 5 coins, lodging 8-12 coins
- Stats: Starting values 1-2, limited stat-gated paths unlocked
- Routes: All unknown, uncertainty increases difficulty
- Challenges: Easier thresholds (tutorial calibration)
- Safety Nets: Higher rewards relative to costs, generous recovery rates

**Player Experience**:
"Every coin matters. I can afford food OR lodging, not both easily. I need to plan carefully. But when I succeed, rewards feel meaningful."

**Design Intent**:
- Teach resource management through meaningful scarcity
- Make victories feel earned through struggle
- Avoid frustration via safety nets (easier challenges, higher rewards)
- Establish pattern: Tight margins are normal, not failure state

### 8.6.2 Mid Game: Specialization Emerges, Economic Buffer, Tactical Depth

**Characteristics**:
- Economy: 15-25 coins per B-story, comfortable buffer possible
- Stats: 4-6 in specialized stats, 2-3 in neglected stats
- Routes: Core routes known, planning more precise
- Challenges: Full tactical complexity available
- Build Identity: Specialization creates distinctive playstyle

**Player Experience**:
"My high Insight build dominates investigations. But when Authority-based scenes appear, I struggle. I'm managing trade-offs between specialization advantage and vulnerability."

**Design Intent**:
- Specialization creates build identity
- Economic buffer allows strategic choices (not survival choices)
- Tactical depth fully unlocked (all three challenge types accessible)
- Margins still tight enough that costs matter

### 8.6.3 Late Game: Tier Escalation, Complexity Deepens, Margins Stay Tight

**Characteristics**:
- Economy: 30-50 coins per high-tier B-story, but premium options expensive (50-100 coins)
- Stats: 6-8 in specialized stats, enables consistent stat-path access in focused areas
- Routes: Most routes known, optimization mastery
- Challenges: Tactical mastery visible in success rates
- Scope: Local → Regional → Continental progression

**Player Experience**:
"I have 80 coins (wealthy by early standards), but upcoming money-gated path costs 75 coins. Premium lodging costs 40 coins. Training costs 30 coins. Still making hard choices, just at higher scale."

**Design Intent**:
- Numerical scale increases (30 coins vs 8 coins)
- Margins stay proportionally tight (premium options expensive)
- Mystery never resolves (infinite procedural A-story)
- Mastery shows in optimization, not trivial difficulty

### 8.6.4 Progression via Scope, Not Power

Player advancement measured by geographic reach and narrative scope, not statistical dominance.

**Early Game Scope** (A1-A20, Tier 1):
- Small towns and inns
- Local people with local concerns
- Simple negotiations and introductions
- Learning the world

**Mid Game Scope** (A21-A40, Tier 2):
- Larger towns and established venues
- Regional figures with broader influence
- Complex social situations
- Deepening relationships

**Late Game Scope** (A41-A60+, Tier 3-4):
- Cities and major centers
- Continental or cosmic-tier figures
- Long-term consequences visible
- Player's reputation precedes them

**What Changes**: Scale and complexity of situations, not statistical superiority.

**What Doesn't Change**: Margins stay tight, choices matter, specialization creates advantage and vulnerability.

**Example**: Early game inn negotiation costs 8 coins (tight when earning 10 coins per B-story). Late game palace lodging costs 40 coins (tight when earning 50 coins per B-story). Proportional difficulty maintained.

## 8.7 Sweet Spots and Extremes

### 8.7.1 Sweet Spots Exist (Not Just "More is Better")

Optimal stat ranges exist. Too low = weak. Too high = diminishing returns or negative consequences.

**Stat Sweet Spots**:
- Stats 4-6: Comfortable range, unlocks most stat-gated paths
- Stats 7-8: Specialization territory, dominates focused area
- Stats 9-10: Excessive, minimal additional benefit, resource inefficiency

**Why Sweet Spots**:
- Most stat-gated paths require 3-6 stat value
- Specializing to 7-8 enables consistent access in focused areas
- Beyond 8: Rare situations need 9+, diminishing return on XP investment
- Better to have 7/7 in two stats than 10/4 in two stats (flexibility)

**Example**:
- Player A: Insight 10, Cunning 3
- Player B: Insight 7, Cunning 6

Investigation scene with two paths:
- Path 1: Insight 7 required
- Path 2: Cunning 5 required

Player A: Path 1 unlocked (overkill, Insight 10 when 7 sufficient), Path 2 locked
Player B: Both paths unlocked, strategic choice available

Player B has BETTER options despite lower max stat. Balance beats maximization.

### 8.7.2 Extreme Stats Trigger Consequences

Very high values in certain contexts can trigger automatic consequences or threshold systems.

**Challenge Threshold Auto-Triggers**:

Physical challenge: Danger threshold = 10
- Player plays risky cards building Danger
- Danger reaches 10 → AUTOMATIC injury, challenge fails, return to location
- No player choice, threshold consequence

Social challenge: Doubt threshold = 10
- NPC patience depletes during conversation
- Doubt reaches 10 → AUTOMATIC conversation ends, relationship damaged
- No player choice, threshold consequence

**Specialization Extremes**:

Authority 10 (maximum):
- Benefit: All authority-gated paths unlocked
- Risk: NPCs respond with hostility (heavy-handed approach visible)
- Consequence: Some relationships difficult to build (seen as authoritarian)

Insight 10 (maximum):
- Benefit: All investigation paths unlocked
- Risk: Over-analysis paralysis (spend excess time on details)
- Consequence: Time efficiency suffers (thoroughness costs time)

**Design Philosophy**: Moderation often better than maximization. Extremes have costs.

### 8.7.3 Balanced Development Matters

Spreading XP across multiple stats creates flexibility. Min-maxing creates excellence and helplessness.

**Balanced Build** (5/5 split in two stats):
- Moderate capability in both areas
- Handles most situations adequately
- Rarely dominates, rarely helpless
- Flexibility over optimization

**Min-Maxed Build** (8/2 split in two stats):
- Excellence in one area (8 enables most stat paths)
- Helplessness in other area (2 locks most stat paths)
- Dominates specialized situations, struggles elsewhere
- Optimization over flexibility

**Which is Better**: Depends on player preference and playstyle goals.

**Balanced build advantages**:
- Handles varied content smoothly
- Fewer situations force expensive alternatives
- More strategic options available

**Min-maxed build advantages**:
- Dominates chosen specialty
- Creates distinctive playstyle and identity
- Challenge and mastery in overcoming weaknesses

Both viable. Neither "correct." Trade-offs exist. Game validates both approaches.

**Example**: Scene requires Insight 5 OR Rapport 4.

Balanced build (Insight 5, Rapport 4): Both paths unlocked, strategic choice
Min-maxed build (Insight 8, Rapport 2): Only Insight path unlocked

Min-maxed build: Overkill on Insight (8 when 5 sufficient), locked from Rapport path.
Balanced build: Efficient (exactly enough for both), but never dominates.

## 8.8 AI Balance Enablement

### 8.8.1 The Balance Problem for AI Content Generation

Traditional balance requires knowing:
- Player progression level (early game vs late game)
- Player economy (how many coins they have)
- Player stat ranges (what thresholds are challenging)
- Global difficulty curve (how hard should this be)

AI generating infinite content CAN'T know these. Solution: Categorical properties with universal scaling.

### 8.8.2 Categorical Property Architecture

**AI Writes Descriptive Properties**: The AI describes entities using categorical properties like Friendly demeanor, Lodging service type, Standard quality, and Standard environment quality. No numbers appear in the authored content - only self-documenting, fiction-appropriate descriptors like "Friendly innkeeper at standard-quality inn."

**Catalogues Translate to Balanced Numbers**: The service negotiation archetype defines base stat threshold of 5 and base coin cost of 8. Context scaling applies the Friendly demeanor multiplier of 0.6 and Standard quality multiplier of 1.0, producing final stat threshold of 3 for easy negotiation and coin cost of 8 for standard pricing. The catalogue applies universal formulas resulting in contextually appropriate difficulty.

### 8.8.3 Dynamic Scaling via Universal Formulas

**Formula Structure**: The final value equals base value multiplied by property multiplier one, times property multiplier two, times progression multiplier. This multiplicative structure enables compound scaling from multiple categorical dimensions.

**Base Value**: Archetype defines (service_negotiation: BaseStatThreshold = 5)
**Property Multipliers**: Categorical properties scale (Friendly = 0.6×, Hostile = 1.4×)
**Progression Multiplier**: Optional tier-based scaling (Tier 1 = 1.0×, Tier 4 = 1.5×)

**Example Calculation**:

AI generates: "Hostile merchant at luxury shop" (Tier 1)

Base stat threshold from archetype multiplied by hostile demeanor and tier one progression produces hard stat threshold. Base coin cost multiplied by luxury quality and tier one produces very expensive cost. Result: Challenging negotiation, expensive purchase. Appropriate for hostile luxury merchant.

AI generates: "Friendly innkeeper at basic shelter" (Tier 1)

Base stat threshold multiplied by friendly demeanor and tier one progression produces easy stat threshold. Base coin cost multiplied by basic quality and tier one produces cheap cost. Result: Easy negotiation, affordable lodging. Appropriate for friendly basic inn.

### 8.8.4 Relative Consistency Across Progression

Key guarantee: Friendly ALWAYS easier than Hostile, regardless of progression tier.

**Early Game** (Tier 1, BaseStatThreshold = 5):

Friendly demeanor produces easier threshold, neutral produces baseline threshold, hostile produces harder threshold.

**Late Game** (Tier 3, BaseStatThreshold = 7, Progression = 1.2×):

With higher base threshold and progression multiplier, friendly still produces easiest threshold, neutral produces moderate threshold, hostile produces hardest threshold. Absolute values increase but relative ordering preserved.

**Relative Differences Preserved**:

Friendly always easier than Neutral across all progression tiers. Hostile always harder than Neutral across all progression tiers. Proportional gaps maintained through consistent multiplier application.

**Why This Matters**: AI authors "Friendly innkeeper" knowing it will ALWAYS be easier than "Hostile merchant," regardless of when player encounters them. Consistency through universal formulas.

### 8.8.5 Infinite Variety Through Property Combinations

**21 Situation Archetypes** (reusable mechanical patterns)

**Categorical Properties**:
- 3 NPCDemeanor values (Friendly, Neutral, Hostile)
- 4 Quality values (Basic, Standard, Premium, Luxury)
- 3 PowerDynamic values (Dominant, Equal, Submissive)
- 3 EnvironmentQuality values (Basic, Standard, Premium)

**Mathematical Variety**:

Twenty-one situation archetypes multiplied by all categorical property combinations produces over two thousand distinct mechanical variations.

Add narrative variety from NPC personality and background, location atmosphere and history, player's journey context, and previous scene outcomes.

Result: Effectively infinite content, all balanced via universal formulas.

### 8.8.6 What AI Doesn't Need to Know

**AI Doesn't Know**:
- Player current coin count
- Player current stat values
- Global difficulty curve
- Economy balance targets
- How many hours player has played

**AI Only Knows**:
- Entity categorical properties (descriptive, fiction-appropriate)
- Archetype to use (selected by system based on anti-repetition)
- Player narrative context (for story coherence)

**System Handles**:
- Numerical balance via catalogues
- Difficulty scaling via progression multipliers
- Resource costs via universal formulas
- Mechanical guarantees via archetype structure

**Result**: AI generates infinite balanced content by describing entities categorically. System translates to numbers. Perfect division of labor.

## 8.9 Designing Balanced Situations: Step-by-Step Methodology

This section provides comprehensive methodology for designing balanced situations from scratch. Follow this process to ensure all 8 core balance rules are satisfied.

### 8.9.1 The Design Process Overview

**Five-Phase Design Process:**

1. **Establish Context** - What is the situation about? What does the player need?
2. **Define Outcome** - What happens when situation resolves? What does player gain?
3. **Design Four Paths** - Create orthogonal resource costs for same outcome
4. **Validate Balance** - Check all 8 rules, test edge cases
5. **Verify Verisimilitude** - Ensure fiction justifies mechanics

Each phase has specific methodology and validation criteria.

### 8.9.2 Phase 1: Establish Context

**Questions to Answer:**

1. **What does the player need to accomplish?**
   - Information (learn where to go next)
   - Access (enter restricted location)
   - Service (rest, healing, equipment)
   - Relationship (build NPC trust)

2. **What is the narrative context?**
   - Who is involved? (NPC personality, power dynamic)
   - Where is this happening? (location type, atmosphere)
   - What are the stakes? (urgent vs casual, high vs low importance)

3. **What entry state does player bring?**
   - Tags from previous choices (reputation, relationships)
   - Current resource levels (coins, health, stats)
   - Build specialization (high Insight vs high Authority)

4. **Is this A-story or B-story?**
   - A-story: Progression fallback required (must advance)
   - B-story: Verisimilitude fallback acceptable (can walk away)

**Example Context Establishment:**

**Situation**: "Gain innkeeper's trust to learn about mysterious stranger"

1. Need: Information about stranger's destination
2. Context: Small town inn, friendly innkeeper, casual stakes
3. Entry state: Just arrived (no prior relationship), moderate coins, varied stats
4. Type: A-story (critical path, must progress)

**Result**: This is an A-story information-gathering situation at a friendly inn with a willing NPC. Progression fallback required.

### 8.9.3 Phase 2: Define Outcome

**Questions to Answer:**

1. **What is the BASE outcome all choices share?**
   - Same fundamental result regardless of path
   - Difference is in HOW player gets there, not WHETHER

2. **What are PROPORTIONAL REWARDS for harder paths?**
   - Stat-gated path: Best rewards (validates specialization)
   - Money-gated path: Good rewards (efficient but costly)
   - Challenge path: Variable rewards (skill expression)
   - Fallback path: Minimal rewards (guaranteed but inefficient)

3. **What tags should be applied?**
   - Entry state for next scene
   - Relationship changes with NPCs
   - World state alterations

4. **What scenes spawn next?**
   - A-story: ALL four choices spawn same next scene (different entry states)
   - B-story: Choices may spawn different follow-ups OR none

**Example Outcome Definition:**

**Base Outcome (All Paths):**
- Learn stranger went north to Grimwood
- Next scene spawns: "Travel to Grimwood"

**Proportional Rewards:**

**Stat-gated path (Rapport 4):**
- Tag: "InnkeeperTrusts" (best relationship)
- Innkeeper relationship +2
- Bonus: Free meal (5 coin value)
- Information quality: Detailed (exact route, warnings about dangers)

**Money-gated path (15 coins):**
- Tag: "InnkeeperPaid" (transactional relationship)
- Innkeeper relationship +1
- Cost: 15 coins
- Information quality: Complete (destination and general route)

**Challenge path (Social challenge, 3 Resolve):**
- Success: Tag "InnkeeperConvinced", relationship +2, detailed information
- Failure: Tag "InnkeeperReluctant", relationship +0, basic information
- Both outcomes progress (information received either way)

**Fallback path (4 time blocks helping):**
- Tag: "InnkeeperHelped" (earned through patience)
- Innkeeper relationship +1
- Cost: 4 time blocks (opportunity cost)
- Information quality: Complete but slowly gained

**Result**: All four paths give information and spawn next scene. Rewards scale with difficulty. Fallback guarantees progression.

### 8.9.4 Phase 3: Design Four Paths

This is the critical phase where orthogonal resource costs are designed.

**Path 1: Stat-Gated (Optimal Specialization Path)**

**Design Questions:**

1. **Which stat makes narrative sense?**
   - Rapport for social connection
   - Insight for analytical observation
   - Authority for commanding presence
   - Diplomacy for balanced negotiation
   - Cunning for strategic manipulation

2. **What threshold is appropriate for progression level?**
   - Early game (A1-A3): No requirements (identity building)
   - Early-mid game (A4-A6): Stat 2-3 (first specialization)
   - Mid game (A7-A12): Stat 4-5 (moderate specialization)
   - Late game (A13-A20): Stat 6-7 (deep specialization)
   - Very late game (A21+): Stat 7-8 (mastery)

3. **What are the BEST rewards?**
   - Highest relationship gains (+2 vs +1)
   - Bonus tangible rewards (free items, extra coins, time saved)
   - Best information quality (most detailed, most useful)
   - Optimal entry state for next scene

4. **Why should this path be FREE?**
   - Player invested permanent resources (XP) to reach stat threshold
   - Build specialization should be rewarded
   - Validates character investment decisions

**Example Path 1 Design:**

Friendly innkeeper scenario, mid-game (A7):

**Choice: "Connect with innkeeper through genuine empathy"**
- Requirement: Rapport ≥ 4 (mid-game threshold)
- Cost: Free (validates Rapport specialization)
- Outcome: Innkeeper shares detailed info willingly, relationship +2, free meal
- Tag: "InnkeeperTrusts"
- Verisimilitude: High Rapport character naturally builds trust quickly

**Path 2: Money-Gated (Reliable Economic Path)**

**Design Questions:**

1. **What coin cost is SUBSTANTIAL but AFFORDABLE?**
   - Must feel meaningful (not trivial)
   - Must be achievable via 1-2 B-story completions
   - Scales with progression (higher costs later)

2. **What is baseline coin availability at this progression level?**
   - Early game (A1-A3): 5-15 coins per B-story, 20-40 coins typical reserve
   - Mid game (A7-A12): 15-25 coins per B-story, 40-80 coins typical reserve
   - Late game (A13-A20): 30-50 coins per B-story, 80-150 coins typical reserve

3. **What percentage of typical reserve is meaningful?**
   - 20-40% feels substantial but not devastating
   - Mid-game: 40-80 coin reserve → 15-25 coin cost range
   - This example: 15 coins (about 30% of 50 coin midpoint)

4. **What rewards justify the cost?**
   - Good rewards (not as good as stat path, better than fallback)
   - Reliable outcome (no variance, instant resolution)
   - Efficient (faster than time-based paths)

**Example Path 2 Design:**

**Choice: "Offer generous tip for information"**
- Requirement: None (always visible)
- Cost: 15 coins (meaningful but affordable)
- Outcome: Innkeeper shares complete info, relationship +1
- Tag: "InnkeeperPaid"
- Verisimilitude: Money greases wheels, transactional but effective

**Validation**: 15 coins is 60% of typical B-story reward (25 coins), requires player to have completed 1 B-story recently or saved from previous earnings. Substantial enough to matter, affordable enough to be viable.

**Path 3: Challenge (Risky Skill Expression Path)**

**Design Questions:**

1. **Which challenge type makes narrative sense?**
   - Social challenge: Conversations, negotiations, persuasion
   - Mental challenge: Analysis, investigation, deduction
   - Physical challenge: Action, confrontation, endurance

2. **What session resource cost is appropriate?**
   - Early game: 2-3 Resolve/Stamina/Focus
   - Mid game: 3-4 Resolve/Stamina/Focus
   - Late game: 4-5 Resolve/Stamina/Focus

3. **What are SUCCESS and FAILURE outcomes?**
   - Both must advance progression (no soft-lock)
   - Success: Excellent rewards (better than money path)
   - Failure: Adequate rewards (worse than money path, better than fallback)
   - Variance creates risk/reward trade-off

4. **Is failure genuinely acceptable?**
   - Player still gets information (base outcome)
   - Relationship might be neutral instead of positive
   - Next scene still spawns (progression guaranteed)
   - Failure is SETBACK not DISASTER

**Example Path 3 Design:**

**Choice: "Engage in friendly conversation, persuade gently"**
- Requirement: None (always visible)
- Cost: 3 Resolve (mid-game standard)
- Challenge: Social challenge session
- Success outcome: Detailed information, relationship +2, Understanding +1
- Success tag: "InnkeeperConvinced"
- Failure outcome: Basic information, relationship +0
- Failure tag: "InnkeeperReluctant"
- Verisimilitude: Skilled conversation can build trust OR fall flat

**Validation**: Both outcomes progress story (information received). Success gives best total value (relationship +2, Understanding +1, detailed info). Failure gives base outcome only. Both acceptable.

**Path 4: Fallback (Guaranteed Progression Path)**

**Design Questions:**

1. **What zero-requirement path makes narrative sense?**
   - Patient approach (wait, help, build trust slowly)
   - Alternative method (find different source, work around)
   - Brute force (inefficient but reliable)

2. **What is the opportunity cost?**
   - Time blocks (most common)
   - Alternative resources (less common)
   - Never coins (overlaps with Path 2)
   - Never stats (contradicts zero-requirement principle)

3. **How much time is MEANINGFUL but not PUNISHING?**
   - 3-5 time blocks typical (2-4 hours in-game)
   - Enough to feel costly (could do B-story instead)
   - Not so much that it feels like punishment

4. **What are MINIMAL rewards?**
   - Base outcome only (information received)
   - Small relationship gain (+1 typical)
   - No bonus rewards
   - Adequate entry state for next scene

5. **Does this GUARANTEE progression?**
   - Zero requirements (any player can select)
   - Cannot fail (Instant action or guaranteed challenge)
   - Spawns next scene (A-story advances)
   - This is NON-NEGOTIABLE for A-story

**Example Path 4 Design:**

**Choice: "Help with inn chores, build trust over time"**
- Requirement: None (zero requirements, always available)
- Cost: 4 time blocks (meaningful opportunity cost)
- Outcome: Complete information, relationship +1
- Tag: "InnkeeperHelped"
- Cannot fail: Instant action (not a challenge)
- Progresses: Spawns next scene
- Verisimilitude: Patient work builds trust naturally

**Validation**: Zero requirements ✓, Cannot fail ✓, Spawns next scene ✓. Progression guaranteed even for player with 0 stats, 0 coins, 0 Resolve.

### 8.9.5 Phase 4: Validate Balance

Now apply all 8 core balance rules systematically.

**Rule 1: Every situation must have at least 2 choices minimum**

Test: Count choices.
- Path 1, Path 2, Path 3, Path 4 = 4 choices
- Result: ✓ Pass

**Rule 2: Player must always juggle resources**

Test: Does every choice cost OR reward something measurable?
- Path 1: Costs stat investment (permanent resource)
- Path 2: Costs 15 coins (consumable resource)
- Path 3: Costs 3 Resolve (session resource)
- Path 4: Costs 4 time blocks (opportunity cost)
- All choices have mechanical consequences
- Result: ✓ Pass

**Rule 3: Choices with requirements/costs MUST give better rewards**

Test: Do higher costs = better rewards?

Comparison matrix:
| Path | Cost | Relationship | Info Quality | Bonus | Total Value |
|------|------|--------------|--------------|-------|-------------|
| 1 | Stat 4 | +2 | Detailed | Free meal | Highest |
| 2 | 15 coins | +1 | Complete | None | Good |
| 3 (success) | 3 Resolve | +2 | Detailed | +1 Understanding | Excellent |
| 3 (failure) | 3 Resolve | +0 | Basic | None | Minimal |
| 4 | 4 time | +1 | Complete | None | Minimal |

Analysis:
- Path 1 (stat requirement): Best relationship, best info, bonus reward
- Path 2 (coin cost): Middle relationship, adequate info, no bonus
- Path 3 (success): Best total (relationship + Understanding)
- Path 3 (failure): Minimal (acceptable setback)
- Path 4 (time cost): Minimal but adequate

Validation:
- Stat requirement gives best rewards ✓
- Money cost gives good rewards ✓
- Challenge success gives excellent rewards ✓
- Challenge failure gives minimal but acceptable rewards ✓
- Fallback gives minimal rewards ✓
- Result: ✓ Pass

**Rule 4: Balance within situation, NOT across situations**

Test: Are we comparing to choices in OTHER situations?
- No cross-situation comparisons made
- All balance judgments relative to THIS situation's four choices
- Result: ✓ Pass

**Rule 5: Crisis situations = All choices are damage control**

Test: Is this a crisis situation?
- No, this is standard information-gathering
- Not all choices involve losses
- Most choices give positive rewards
- Result: ✓ N/A (not crisis situation)

**Rule 6: Multi-stat effects create balance through trade-offs**

Test: Do any choices affect multiple stats asymmetrically?
- Current design: Single-stat gains only
- Opportunity: Could add multi-stat trade-offs
- Not required but could enhance

Potential enhancement:
- Path 1 could give +2 Rapport, -1 Authority (too friendly, lose commanding presence)
- Path 2 could give relationship +1, but tag "TransactionalRelationship" (limits future options)

Decision: Keep simple for this example, but multi-stat effects are valid tool.
- Result: ✓ Pass (not required, could enhance)

**Rule 7: Versimilitude in costs (opportunity cost accuracy)**

Test: Do costs reflect actual opportunity cost?

Path 1 (Stat requirement):
- Real cost: XP invested over many challenges to reach Rapport 4
- Opportunity cost: Could have invested in different stats
- Verisimilitude: ✓ (specialization should be rewarded)

Path 2 (15 coins):
- Real cost: 15 coins from player's reserve
- Opportunity cost: Could have bought food, lodging, equipment
- Verisimilitude: ✓ (money is fungible, can be spent elsewhere)

Path 3 (3 Resolve):
- Real cost: 3 Resolve from session pool
- Opportunity cost: Could use Resolve for other Social challenges today
- Verisimilitude: ✓ (social energy is finite)

Path 4 (4 time blocks):
- Real cost: 4 time blocks from day's budget
- Opportunity cost: Could do B-story for coins, other scenes, rest
- Verisimilitude: ✓ (time spent here cannot be spent elsewhere)

Result: ✓ Pass (all costs reflect real opportunity costs)

**Rule 8: Coins as alternative resource (inspired by Sir Brante's willpower)**

Test: Do coins function as alternative to stat requirements OR enhancement?

Current design:
- Path 1: Stat requirement (Rapport 4)
- Path 2: Coin cost (15 coins)
- These are ALTERNATIVES (either high Rapport OR coins achieves good outcome)

Is 15 coins appropriately valued relative to Rapport 4?

Valuation analysis:
- Rapport 4 requires ~12-15 challenges worth of XP
- Each challenge takes 3-5 segments (average 4)
- Total time investment: 48-60 segments
- Alternative: Do 1-2 B-stories (8-12 segments) to earn 15 coins
- Time ratio: Stat path takes 4-5× longer than coin path
- BUT stat path is permanent, coin path is consumable
- Balance: Coins are more accessible but depletable, stats are harder but permanent

Conclusion: 15 coins is appropriately valued as alternative to Rapport 4.

Result: ✓ Pass

### 8.9.6 Phase 5: Verify Verisimilitude

Final check: Does fiction justify mechanics?

**Test Questions:**

1. **Does the narrative context support all four paths?**
   - Friendly innkeeper willing to help
   - Can be convinced through empathy (Path 1) ✓
   - Can be paid for information (Path 2) ✓
   - Can be persuaded through conversation (Path 3) ✓
   - Will share info after building trust through work (Path 4) ✓

2. **Do requirements make sense in fiction?**
   - Rapport 4 for instant trust: Yes, empathetic people build connection quickly
   - 15 coins for information: Yes, generous tip encourages sharing
   - Social challenge: Yes, persuasion can work but might fail
   - 4 time blocks helping: Yes, actions build trust over time

3. **Do rewards match effort in narrative terms?**
   - High Rapport gets best relationship: Yes, natural connection
   - Money gets transactional outcome: Yes, business relationship
   - Skilled persuasion gets appreciation: Yes, NPC respects ability
   - Patient work gets gratitude: Yes, innkeeper values help

4. **Are there any logical contradictions?**
   - Scan all paths for narrative inconsistencies
   - Path 1 instant trust vs Path 4 slow trust: Consistent (Rapport skill accelerates natural process)
   - Path 2 payment vs Path 1 free: Consistent (empathy bypasses need for payment)
   - Path 3 risk of failure: Consistent (even skilled conversation can fail)

Result: ✓ Pass (all paths make narrative sense)

### 8.9.7 Complete Design Example Output

**Situation: "Gain Innkeeper's Trust" (A7 mid-game)**

**Context:**
- Location: Small town inn, common room
- NPC: Friendly innkeeper, willing to help but cautious
- Goal: Learn where mysterious stranger went
- Type: A-story (progression required)

**Outcome (All Paths):**
- Information: Stranger went north to Grimwood
- Progression: Next scene "Travel to Grimwood" spawns

**Choice 1: Connect through empathy (Stat-Gated)**
- Requirement: Rapport ≥ 4
- Cost: Free
- Outcome: Detailed information (exact route, danger warnings), relationship +2, free meal (5 coin value), Understanding +0
- Tag: "InnkeeperTrusts"
- Narrative: "Your genuine warmth puts the innkeeper at ease. They share everything willingly, even insisting you take provisions for the journey."

**Choice 2: Offer generous tip (Money-Gated)**
- Requirement: None
- Cost: 15 coins
- Outcome: Complete information (destination and general route), relationship +1, Understanding +0
- Tag: "InnkeeperPaid"
- Narrative: "The coin purse lands on the bar with a satisfying clink. The innkeeper's eyes light up, and they're suddenly quite helpful."

**Choice 3: Persuade gently (Challenge)**
- Requirement: None
- Cost: 3 Resolve (Social challenge)
- Success: Detailed information, relationship +2, Understanding +1
- Success tag: "InnkeeperConvinced"
- Failure: Basic information (destination only), relationship +0, Understanding +0
- Failure tag: "InnkeeperReluctant"
- Narrative Success: "Your careful words and attentive listening draw out the whole story. The innkeeper appreciates your genuine interest."
- Narrative Failure: "Despite your efforts, the innkeeper remains guarded. They provide the basic facts but little else."

**Choice 4: Help with chores (Fallback)**
- Requirement: None
- Cost: 4 time blocks
- Outcome: Complete information, relationship +1, Understanding +0
- Tag: "InnkeeperHelped"
- Narrative: "You spend the afternoon helping in the common room. The innkeeper warms to you gradually, eventually sharing what they know over evening stew."

**Balance Validation:**
- ✓ Four choices (Rule 1)
- ✓ All choices cost resources (Rule 2)
- ✓ Higher costs = better rewards (Rule 3)
- ✓ Balanced within situation (Rule 4)
- ✓ Verisimilitude in all costs (Rule 7)
- ✓ Coins as willpower alternative (Rule 8)
- ✓ Progression guaranteed (Choice 4 zero requirements, cannot fail)

### 8.9.8 Crisis Situation Variation

The above methodology applies to STANDARD situations. Crisis situations require modifications.

**Crisis Situation Definition:**
- All choices involve LOSSES or damage
- No purely positive outcomes
- Player chooses WHICH harm to accept
- Balance through asymmetric costs, not equal rewards

**When to Use Crisis Situations:**

1. **Narrative Justification Required:**
   - Fiction must support unavoidable bad outcome
   - Examples: Ambush, disaster, betrayal, crisis point
   - Never arbitrary (player should understand WHY all options bad)

2. **Frequency Guidelines:**
   - Rare (5-10% of A-story situations)
   - Clustered in dramatic moments (not evenly distributed)
   - Followed by recovery opportunities (not back-to-back crises)

3. **Progression Level Appropriate:**
   - Early game (A1-A6): Very rare, lower stakes
   - Mid game (A7-A12): Occasional, moderate stakes
   - Late game (A13+): More frequent, higher stakes

**Crisis Situation Design Modifications:**

**Path 1: Stat-Gated (Minimize Loss Path)**
- Requirement: Stat threshold
- Cost: Smaller loss than alternatives
- Outcome: Least bad option (still negative)
- Purpose: Specialization reduces damage

**Path 2: Money-Gated (Economic Damage Path)**
- Requirement: None
- Cost: Substantial coins (larger than normal)
- Outcome: Preserve other resources
- Purpose: Wealth as damage mitigation

**Path 3: Challenge (Risk Management Path)**
- Requirement: None
- Cost: Session resource
- Success: Moderate loss
- Failure: Severe loss
- Both outcomes negative, variance in severity

**Path 4: Fallback (Accept Maximum Loss Path)**
- Requirement: None
- Cost: Catastrophic loss (worst outcome)
- Outcome: Survive but devastated
- Purpose: Progression guaranteed even in disaster

**Example Crisis Situation:**

**Situation: "Bandit Ambush" (A9, late mid-game)**

**Context:**
- Unavoidable ambush on route
- Bandits demand valuables
- No peaceful resolution available
- All choices involve losses

**Choice 1: Fight back (Authority 5) - Stat-Gated**
- Requirement: Authority ≥ 5
- Outcome: Lose 4 Health, keep all coins, gain +1 Authority (learning)
- Tag: "FoughtBandits"
- Verisimilitude: Commanding presence deters some attackers, reduces violence

**Choice 2: Pay them off (25 coins) - Money-Gated**
- Requirement: None
- Cost: 25 coins (substantial, ~60% of typical mid-game reserve)
- Outcome: Lose 25 coins, keep Health, bandit relationship +1 (they remember you paid)
- Tag: "PaidBandits"
- Verisimilitude: Bandits take payment and leave

**Choice 3: Physical challenge (4 Stamina) - Challenge**
- Requirement: None
- Cost: 4 Stamina
- Success: Lose 3 Health, keep 20 coins (they grab some), gain +1 Cunning
- Failure: Lose 6 Health, lose 15 coins, gain nothing
- Tags: "EscapedBandits" (success), "BeatenByBandits" (failure)
- Verisimilitude: Attempting escape risky, might work or fail badly

**Choice 4: Surrender everything - Fallback**
- Requirement: None
- Outcome: Lose ALL coins, lose 2 Health (beaten), lose 1 Rapport (humiliation)
- Tag: "SurrenderedToBandits"
- Verisimilitude: Complete capitulation, maximum loss but survive

**Crisis Balance Analysis:**

Validation:
- All choices negative ✓
- Different players value different resources ✓
- Wealthy player picks Path 2 (has coins to spare) ✓
- Healthy player picks Path 1 (can afford Health loss) ✓
- Specialized fighter picks Path 3 (skills improve odds) ✓
- Desperate player picks Path 4 (survives, loses everything) ✓
- Progression guaranteed (Path 4 available, cannot soft-lock) ✓

Asymmetric costs create balance:
- 4 Health vs 25 coins vs (3-6 Health + 0-15 coins) vs (all coins + 2 Health + 1 Rapport)
- No universally best choice
- Player situation determines optimal selection

## 8.10 Stat Requirement Scaling Across Progression (A4-A20+)

Early game scenes (A1-A3) use identity building (no requirements). From A4 onward, stat requirements emerge and scale with player progression.

### 8.10.1 Progression Curve Philosophy

**Key Principles:**

1. **Player has built foundation by A4:**
   - A1-A3 choices offered stat gains
   - Typical player has 2-4 total stat points distributed
   - At least one stat has reached 2-3
   - Ensures first stat-gated paths become accessible

2. **Requirements scale with expected capability:**
   - Early requirements match early player capability
   - Later requirements assume specialization has occurred
   - Never assume player has ALL stats high (specialization expected)

3. **Always provide alternatives:**
   - Stat-gated path is OPTIMAL, not REQUIRED
   - Money/Challenge/Fallback paths always available
   - Specialization unlocks best path, doesn't gate progression

### 8.10.2 Stat Requirement Scaling Table

**A4-A6 (Early Specialization Begins):**

Typical Player State:
- Total XP invested: 3-5 stat points
- Specialization: 1 stat at 2-3, others at 0-1
- Expected capability: Can meet ONE stat-2 requirement consistently

Recommended Stat Requirements:
- Primary stat path: Requirement 2-3
- Alternative stat path: Requirement 2-3 (different stat)
- Ensures 1-2 stat paths accessible based on player's chosen specialization
- Money/Challenge/Fallback always available

Example (A5 situation):
- Choice 1: Insight 2 → Optimal outcome (analytical players)
- Choice 2: Rapport 2 → Optimal outcome (social players)
- Choice 3: 12 coins → Good outcome (economic players)
- Choice 4: Social challenge → Variable outcome (skill players)
- Choice 5: 3 time blocks → Minimal outcome (fallback)

Note: Five choices valid when offering multiple stat paths. Validates different specializations.

**A7-A12 (Moderate Specialization):**

Typical Player State:
- Total XP invested: 8-12 stat points
- Specialization: 1-2 stats at 4-5, others at 1-2
- Expected capability: Dominates ONE stat domain, adequate in another

Recommended Stat Requirements:
- Primary stat path: Requirement 4-5
- Alternative stat path: Requirement 4-5 (different stat)
- Specialist players (focused on one stat) hit primary path consistently
- Balanced players (spread across 2-3 stats) hit one path sometimes

Example (A9 situation):
- Choice 1: Authority 5 → Best outcome (leadership specialists)
- Choice 2: Cunning 4 → Best outcome (strategic specialists)
- Choice 3: 20 coins → Good outcome (economic path)
- Choice 4: Mental challenge → Variable outcome (skill expression)
- Choice 5: 4 time blocks → Minimal outcome (fallback)

**A13-A20 (Deep Specialization):**

Typical Player State:
- Total XP invested: 15-25 stat points
- Specialization: 1-2 stats at 6-7, 1 stat at 3-4, others at 1-2
- Expected capability: Mastery in primary domain, competent in secondary

Recommended Stat Requirements:
- Primary stat path: Requirement 6-7
- Secondary stat path: Requirement 4-5
- Specialist players hit primary consistently, secondary sometimes
- Balanced players hit secondary consistently, primary rarely

Example (A15 situation):
- Choice 1: Diplomacy 7 → Best outcome (master diplomats only)
- Choice 2: Insight 5 → Good outcome (moderate specialists)
- Choice 3: 35 coins → Good outcome (economic path, higher costs)
- Choice 4: Social challenge → Variable outcome
- Choice 5: 5 time blocks → Minimal outcome (fallback)

**A21+ (Mastery and Extremes):**

Typical Player State:
- Total XP invested: 30+ stat points
- Specialization: 1-2 stats at 7-8, 1-2 stats at 4-5, others at 2-3
- Expected capability: Mastery in primary, competence in secondary/tertiary

Recommended Stat Requirements:
- Elite stat path: Requirement 8+ (rare, exceptional specialists only)
- Primary stat path: Requirement 6-7 (standard high specialists)
- Secondary stat path: Requirement 4-5 (moderate capability)
- Provides challenges even for masters while maintaining alternatives

Example (A23 situation):
- Choice 1: Rapport 8 → Exceptional outcome (true masters)
- Choice 2: Rapport 6 → Best outcome (high specialists)
- Choice 3: Diplomacy 5 → Good outcome (balanced social builds)
- Choice 4: 70 coins → Good outcome (economic path, scaling costs)
- Choice 5: Mental challenge → Variable outcome
- Choice 6: 6 time blocks → Minimal outcome (fallback)

Note: Later situations may offer MORE than four choices to provide variety while maintaining guaranteed progression.

### 8.10.3 Multi-Stat Requirements (Compound Gates)

**When to Use:**
- Rare (10-15% of situations)
- Represents genuinely complex challenges requiring multiple capabilities
- Always provide single-stat alternatives

**Design Pattern:**

Compound requirement path:
- Requirement: Stat A ≥ X AND Stat B ≥ Y
- Rewards: EXCEPTIONAL (justifies compound requirement)
- Purpose: Reward balanced builds, create distinct path

Alternative paths:
- Single stat paths (higher threshold, good rewards)
- Money/Challenge/Fallback paths (standard alternatives)

**Example (A12 situation - Complex Negotiation):**

- Choice 1: Diplomacy 5 AND Authority 4 → Exceptional outcome (balanced leadership)
- Choice 2: Diplomacy 6 → Best outcome (pure diplomat, no authority needed)
- Choice 3: Authority 6 → Best outcome (pure commander, no diplomacy needed)
- Choice 4: 30 coins → Good outcome (economic path)
- Choice 5: Social challenge → Variable outcome
- Choice 6: 5 time blocks → Minimal outcome (fallback)

Analysis:
- Compound path (Diplomacy 5 + Authority 4): Requires 9 total points across two stats
- Single stat paths (Diplomacy 6 OR Authority 6): Requires 6 points in one stat
- Specialist beats compound for single-stat approach
- Balanced build unlocks compound for exceptional rewards
- Both build types validated

### 8.10.4 Scaling Coin Costs Alongside Stat Requirements

As stat requirements increase, coin costs should scale proportionally to maintain balance.

**Coin-to-Stat Equivalence Formula:**

**Early Game (A4-A6):**
- Stat requirement 2-3 ≈ 10-15 coins
- Reasoning: 1 B-story worth of earnings

**Mid Game (A7-A12):**
- Stat requirement 4-5 ≈ 20-30 coins
- Reasoning: 1-2 B-stories worth of earnings

**Late Game (A13-A20):**
- Stat requirement 6-7 ≈ 35-50 coins
- Reasoning: 1-2 B-stories worth of earnings (higher B-story rewards)

**Very Late Game (A21+):**
- Stat requirement 8+ ≈ 60-100 coins
- Reasoning: 2-3 B-stories worth of earnings

**Why This Scaling:**

Stat path cost:
- Reaching Stat 5 requires ~15-20 challenges worth of XP
- Each challenge takes 3-5 segments
- Total: 60-80 segments invested over time
- Permanent benefit (keeps stat forever)

Coin path cost:
- Earning 25 coins requires 1-2 B-stories
- Each B-story takes 8-12 segments
- Total: 10-20 segments invested immediately
- Consumable cost (coins spent, gone forever)

Balance:
- Coin path is 3-4× faster than stat path
- BUT coins are consumable (one-time use)
- Stats are permanent (reusable forever)
- Fair trade-off: Speed vs permanence

## 8.11 Balance Validation Checklist

**For the complete authoritative checklist, see:**
**[VALIDATION_CHECKLIST.md](VALIDATION_CHECKLIST.md) - Canonical Pre-Commit Validation Checklist**

Use this checklist before committing any situation design to ensure balance. The canonical version in VALIDATION_CHECKLIST.md is the single source of truth, maintained centrally and referenced by all design documents.

**Quick summary of validation areas:**

### 8.11.1 Structural Validation

- Choice count (minimum 2, typically 4-6 for A-story)
- Progression guarantee (A-story must have zero-requirement fallback)
- Orthogonal resource costs (each choice costs different resource type)

### 8.11.2 Rule Compliance Validation

- All 8 balance rules verified
- Resource juggling (every choice has mechanical effect)
- Requirements justify rewards (higher cost = better rewards)
- Intra-situation balance only (no cross-situation comparisons)
- Crisis vs normal situations properly balanced
- Verisimilitude in all costs

### 8.11.3 Edge Case Validation

- Worst-case player (0 coins, 0 stats, 0 resources) can progress
- All specialist builds find viable paths (Insight, Authority, Rapport, Diplomacy, Cunning)
- Balanced generalist has options
- Scattered progression player never soft-locked
- Wealthy and skilled players can use their advantages

### 8.11.4 Scaling Validation

- Stat requirements match progression tier (2-3 for A4-A6 → 7-8+ for A21+)
- Coin costs match progression tier (10-15 for A4-A6 → 60-100 for A21+)
- Session resource costs match progression tier (2-3 → 5-6)
- Time costs appropriate (3-5 blocks typical for fallback)

### 8.11.5 Verisimilitude Validation

- Narrative context supports all paths
- Requirements justified by fiction
- Rewards match effort narratively
- No arbitrary gates or logical contradictions

### 8.11.6 Reward Proportionality

- Create comparison matrix for all choices
- Verify: Stat-gated (best) > Money-gated (good) > Challenge success (excellent) > Challenge failure (adequate) > Fallback (minimal)
- Higher costs yield better rewards

### 8.11.7 Final Review Questions

- Does this create impossible choice? (Strategic tension)
- Would I want to play this? (Fair, interesting, appropriate)
- Can I justify this to players? (Clear reasoning for all costs/requirements)

**See [VALIDATION_CHECKLIST.md](VALIDATION_CHECKLIST.md) for complete checkbox-by-checkbox validation procedure.**

## 8.12 Summary

Wayfarer's balance philosophy:

**Perfect Information**: Players calculate strategic decisions with exact costs, rewards, requirements visible before commitment.

**Resource Scarcity**: Shared resources (time, coins, stamina, focus, health) force impossible choices between valid alternatives.

**Specialization**: Builds create identity through strengths and vulnerabilities. No universal best build. Trade-offs exist for all approaches.

**Guaranteed Progression**: Four-choice pattern ensures forward progress via orthogonal resource costs. Player chooses HOW to progress (optimal/reliable/risky/patient), never IF.

**Difficulty via Context**: Categorical properties scale challenge appropriately (Friendly vs Hostile, Basic vs Luxury). Same archetype + different properties = balanced contextual difficulty.

**Tight Margins**: Economy stays proportionally tight throughout. Early game: 10 coins challenging. Late game: 80 coins challenging. Scale increases, margins remain meaningful.

**Mastery via Optimization**: Skilled players improve resource management, route learning, tactical execution. Not power creep or stat grinding.

**AI-Enabled Balance**: Categorical properties + universal formulas = infinite balanced content without AI needing balance knowledge.

**Methodology Provided**: Step-by-step process for designing balanced situations, comprehensive validation checklist, stat/coin scaling formulas across all progression levels.

The result: Strategic depth through resource competition, build diversity through specialization, meaningful progression through scope escalation, and infinite content through categorical scaling. Challenge emerges from impossible choices, not from gates or grind.
