# Design Section 3: Progression Systems

## Purpose

This document defines how players progress in Wayfarer through five interconnected resource layers. It explains stat development, economic advancement, relationship building, content unlocking, and build specialization. The core principle: progression through resource arithmetic and entity spawning, never through boolean gates.

---

## 3.1 Resource Layer Architecture

Wayfarer uses a multi-layered resource system inspired by Sir Brante's impossible choice design. Five distinct resource layers create strategic depth through scarcity and competition.

### The Five Resource Layers

**Layer 1: Personal Stats (Capability Thresholds)**
- Insight, Rapport, Authority, Diplomacy, Cunning
- Numeric values (0-10 typical range)
- Arithmetic comparison for gating: `player.Insight >= choice.RequiredInsight`
- Each point precious, small scale creates weight

**Layer 2: Per-Entity Resources (Individual Capital)**
- StoryCubes per NPC (relationship depth)
- InvestigationCubes per Location (mental progress)
- Separate numeric score for EACH entity
- Build independently, spend independently

**Layer 3: Universal Resources (Shared Across Systems)**
- Health (permanent, carries between challenges)
- Stamina (permanent, carries between challenges)
- Focus (permanent, derives Mental challenge Attention)
- Coins (economic resource, survival pressure)
- Time Segments (daily budget, universal scarcity)

**Layer 4: Session Resources (Tactical Capacity)**
- Attention (Mental challenges, derived from Focus)
- Exertion (Physical challenges, derived from Stamina)
- Initiative (Social challenges, accumulated via Foundation cards)
- Progress/Breakthrough/Momentum (builder resources, session-only)
- Exposure/Danger/Doubt (threshold resources, session-only)

**Layer 5: Meta-Resources (System-Level Mechanics)**
- Understanding (tier unlocking across all challenge types)
- Equipment (permanent improvements, purchased with coins)
- Route Mastery (knowledge of fixed segments)

### Why Five Layers Matter

**Heterogeneous resource types create impossible choices:**
- Cannot convert Stats to Coins
- Cannot convert Time to Health directly
- Each resource serves distinct purpose
- Must manage all simultaneously

**Different layers operate at different scales:**
- Stats: Slow progression (days/weeks to increase)
- Session Resources: Fast cycle (minutes to build and spend)
- Universal Resources: Medium persistence (deplete during play, restore between sessions)
- Meta-Resources: Permanent improvements (never lost)

**Prevents optimization to single dimension:**
- Can't just "maximize stats" and win
- Can't just "maximize coins" and win
- All layers must be managed for success
- Strategic depth emerges from juggling multiple concerns

---

## 3.2 Stat Progression: The Five Core Stats

### Unified Stat System

Wayfarer uses **single unified progression system** across all three challenge types (Mental, Physical, Social). Five stats govern all tactical gameplay:

**Insight:**
- Pattern recognition, analysis, understanding
- Mental: Analyzing evidence, deduction, crime scenes
- Physical: Structural analysis, reading terrain, finding weaknesses
- Social: Understanding NPC motivations, reading between lines, hidden agendas

**Rapport:**
- Empathy, connection, emotional intelligence
- Mental: Empathetic observation, sensing emotions in clues
- Physical: Flow state, body awareness, natural movement
- Social: Building emotional connection, creating trust, resonating

**Authority:**
- Command, decisiveness, power
- Mental: Commanding the scene, decisive analysis, authoritative conclusions
- Physical: Decisive action, power moves, commanding environment
- Social: Asserting position, directing conversation, establishing dominance

**Diplomacy:**
- Balance, patience, measured approach
- Mental: Balanced investigation, patient observation, measured reasoning
- Physical: Measured technique, controlled force, pacing endurance
- Social: Finding middle ground, compromise, balanced conversation

**Cunning:**
- Subtlety, strategy, risk management
- Mental: Subtle investigation, covering tracks, tactical information gathering
- Physical: Risk management, tactical precision, adaptive technique
- Social: Strategic conversation, subtle manipulation, reading and responding

### Card Binding and Depth Access

Every card in every system is bound to one of these five stats. This binding determines:

**1. Depth Access:** Player's stat level determines which card depths they can access
- Stat 0-2: Depth 1 cards (basic)
- Stat 3-5: Depth 2 cards (intermediate)
- Stat 6-8: Depth 3 cards (advanced)
- Stat 9+: Depth 4 cards (expert)

**2. XP Gain:** Playing any card grants XP to its bound stat
- Single unified progression across all three challenge types
- Playing Mental Insight cards increases Insight
- Insight then improves Mental AND Physical AND Social performance

### Stat Progression Sources

**Challenge Success (Primary Source):**
- Successfully completing challenges grants stat XP
- XP awarded to stats of cards played during challenge
- Incremental growth through repeated success
- Play Mental Insight cards → Gain Insight XP
- Accumulate XP → Stat increases

**NPC Training (Secondary Source):**
- Bond Level 4 with specific NPCs grants stat training
- Permanent stat point increase (significant boost)
- One-time benefit per NPC
- Limited by NPC availability
- Must invest time in relationships to unlock

**Equipment Bonuses (Tertiary Source):**
- Some equipment provides temporary stat bonuses
- Active while equipped
- Lost if equipment sold/lost
- Provides flexibility to meet thresholds

### Stat Gating Effects

**Dialogue Choice Availability:**
Higher stats unlock additional dialogue choices in NPC scenes and route situations.

Example:
```
Choice: "Spot the hidden contradiction in their story"
Requires: Insight 3
Your Insight: 2
Status: DISABLED (shown greyed with requirement visible)
```

Player sees: "Requires Insight 3, you have 2" - exact gap of 1 point visible. Can plan to increase Insight before retrying.

**Challenge Difficulty:**
Higher relevant stat makes challenges easier:
- Reduces threshold resources (Exposure, Danger, Doubt)
- Increases builder resources (Progress, Breakthrough, Momentum)
- Better cards available at higher depths

**NPC Access:**
Some NPCs require minimum stat level to begin bonding.

Examples:
- Merchant needs Rapport 3 (must be personable)
- Guard captain needs Authority 2 (must command respect)
- Thief contact needs Cunning 3 (must prove subtlety)

**Choice Availability:**
Route encounter choices sometimes require stats.

Examples:
- "Spot hidden path" needs Insight 2
- "Intimidate bandits" needs Authority 3
- Locked choices visible with requirement shown

### Specialization is Intentional

**Cannot max all stats.** Limited XP forces specialization. Must choose which stats to develop, which approaches to enable.

**Numeric Constraint:**
- XP gain is slow (3-5 challenges to increase stat by 1)
- Five stats to develop
- Limited challenge opportunities per day
- Would take hundreds of challenges to max all five
- Game doesn't last long enough to max everything

**Strategic Consequence:**
Players naturally specialize in 2-3 stats, neglect others. This creates:

**Identity:** "I'm an Investigator" (high Insight/Cunning) or "I'm a Diplomat" (high Rapport/Diplomacy)

**Capability:** Excel in specialized challenges, dominate your domain

**Vulnerability:** Struggle with challenges requiring neglected stats, forced into suboptimal approaches

### Specialization Creates Build Variety

**Investigator Build (Insight + Cunning):**
- Dominates Mental challenges
- Excellent at observation, deduction, pattern recognition
- Weak in Social challenges (low Rapport/Authority)
- Struggles with direct confrontation
- Best for investigation-focused play

**Diplomat Build (Rapport + Diplomacy):**
- Dominates Social challenges
- Excellent at persuasion, empathy, compromise
- Weak in Mental challenges (low Insight)
- Misses subtle clues, struggles with complex analysis
- Best for relationship-focused play

**Commander Build (Authority + Diplomacy):**
- Balanced Social (directive and balanced approaches)
- Excellent at assertion and negotiation
- Weak in Insight and Cunning
- Struggles with subtle investigations
- Best for leadership-focused play

**Hybrid Builds:**
- Balanced across 3-4 stats (no specialization)
- Flexible, handles variety adequately
- Never dominates anything
- No distinct vulnerability or excellence
- Best for players who want options

### Sweet Spots Exist (Not Just "More Is Better")

**Optimal ranges for different contexts:**
- Stat 3-4: Comfortable for most mid-game content
- Stat 5-6: Excellent, dominates specialized challenges
- Stat 7+: Excessive, diminishing returns (rare to achieve)

**Balanced development beats min-maxing:**
- Two stats at 5 better than one at 8, one at 2
- Flexibility prevents vulnerability to crisis
- Extreme specialization creates catastrophic weaknesses

---

## 3.3 Economic Progression: Tight Margins and Strategic Investment

### Starting State

**Initial Resources:**
- 20-30 coins (varies by starting scenario)
- Basic equipment (no bonuses)
- Known routes: 1-2 safe routes
- Time segments: Full day available

**Immediate Pressure:**
- Must find income source (delivery jobs)
- Survival costs consume earnings
- Profit margin very small (5-10 coins per cycle)

### Income Sources

**Delivery Jobs (Primary Income):**
- Accept at taverns or job boards
- Earn 20-40 coins per delivery (varies by difficulty)
- Requires route travel (resource costs)
- Return trip adds costs
- Net profit: 5-15 coins typical

**Challenge Success Bonuses:**
- Some challenges offer coin rewards on success
- Gamble: Risk resources for potential extra income
- Not reliable (failure costs more than gain)

**Item Sales (Rare):**
- Find items during exploration
- Sell at shops for coins
- Infrequent, not sustainable income

### Expenditures (The Economic Pressure)

**Mandatory Survival Costs:**
- Food: 8-12 coins per day (restores hunger)
- Lodging: 12-18 coins per day (restores energy)
- Total: 20-30 coins per day just to survive
- Deliveries BARELY cover this

**Equipment Upgrades (Strategic Investment):**
- Basic equipment: 40-80 coins
- Advanced equipment: 100-200 coins
- Expert equipment: 300+ coins
- Permanent improvements to efficiency

**NPC Investment (Relationship Costs):**
- NPC scenes cost time segments (opportunity cost)
- Some NPCs require gifts (coin cost)
- Delayed payoff (benefits come at higher bond levels)

**Route Exploration (Discovery Costs):**
- Unknown routes riskier (higher resource costs)
- Potential to discover shortcuts or better routes
- Investment in route knowledge

### Tight Economy Design Philosophy

**Why margins are tight:**
- Every choice matters mechanically
- Optimization skill provides measurable advantage
- Investment decisions feel genuinely costly
- Resource management is core challenge
- Failure is possible but recoverable

**Example Economic Cycle:**
```
Morning:
  Accept delivery (30 coins reward)

Travel Outbound:
  6 route segments
  Encounter costs: 8 energy, 12 hunger, 5 coins, 3 time segments
  Arrive: Earn 30 coins

Travel Return:
  6 route segments (same costs roughly)
  Encounter costs: 7 energy, 10 hunger, 4 coins, 3 time segments

Evening:
  Food: 10 coins (restore hunger)
  Lodging: 15 coins (restore energy)

Net Profit Calculation:
  Earned: 30 coins
  Spent on route: 9 coins
  Spent on survival: 25 coins
  Net: -4 coins (LOSS!)
```

**This is intentional.** Some deliveries result in losses. Player must:
- Learn routes to optimize costs
- Develop stats to access better options
- Build relationships for route shortcuts
- Purchase equipment to reduce costs
- Calculate carefully which jobs to accept

### Equipment as Strategic Investment

**Equipment Types:**

**Stat-Boosting Equipment:**
- "+1 Insight Hat" - Provides temporary Insight bonus
- Enables meeting higher stat thresholds
- Lost if equipment sold
- Cost: 60-100 coins

**Resource Efficiency Equipment:**
- "Sturdy Boots" - Reduces energy cost of Physical challenges
- "Focus Charm" - Increases max Focus (more Attention per Mental challenge)
- Permanent efficiency improvement while owned
- Cost: 80-150 coins

**Challenge Capability Equipment:**
- "Investigation Kit" - Unlocks additional Mental challenge options
- "Climbing Gear" - Enables certain Physical challenge approaches
- Opens new strategic possibilities
- Cost: 100-200 coins

**The Investment Tension:**
- Equipment costs 60-200 coins (12-40 successful deliveries)
- Months of in-game time to afford
- But benefits compound forever
- Earlier purchase = more total benefit

**Strategic Question:**
Buy equipment now (delayed gratification, future efficiency) OR maintain resource buffer (immediate flexibility, safety net)?

### Economic Build Variety

**Minimalist Build:**
- Avoid equipment purchases
- Maximize relationship investment
- High risk tolerance (no equipment safety nets)
- Relies on stat development and NPC benefits

**Equipment-Focused Build:**
- Prioritize equipment purchases
- Reduce relationship investment
- Lower risk tolerance (equipment provides safety)
- Relies on mechanical advantages

**Balanced Economic Build:**
- Moderate equipment investment
- Moderate relationship investment
- Medium risk tolerance
- Flexibility through diversification

---

## 3.4 Relationship Progression: NPC Bonds and Mechanical Benefits

### Bond Level System

NPCs have bond levels representing relationship depth. Spending time with NPC advances bond through conversation scenes. Each bond level is one scene with 2-4 choices.

**Bond Progression:**
- Bond Level 0: Haven't met (NPC unknown)
- Bond Level 1: Initial meeting, basic introduction
- Bond Level 2: Deeper conversation, personal story
- Bond Level 3: Meaningful connection, trust established
- Bond Level 4: Close relationship, mutual respect
- Bond Level 5: Deep bond, special access

### Advancing Bond Levels

**Time Investment Required:**
- Each bond scene costs 2-4 time segments
- Opportunity cost (could be doing delivery instead)
- Immediate cost for delayed benefit

**Scene Structure:**
Each bond level scene presents 2-4 dialogue choices. Choices affect relationship flavor (friendly vs professional, humorous vs serious) but **all paths advance bond**. No failure states. The investment is time/coins, not correct dialogue selection.

**No Optimal Path:**
Different choices create different relationship personalities, but all mechanically advance bond level. Player chooses based on character preference, not optimization.

### Mechanical Benefits (Game Design First)

**Bond Level 2 Unlock: Route Information**

Benefit: NPC reveals hidden shortcut on specific route
- Permanently reduces segment count on that route
- Example: "There's a path through the old mill that cuts 3 segments off the eastern route"
- Information persists forever
- Applies to all future travels

Mechanical Impact:
- Time segment savings (3 segments = ~10 minutes real-time)
- Fewer encounters = less resource expenditure
- Faster delivery completion = more deliveries per day
- Compounds: Benefit applied every time route traveled

**Bond Level 3 Unlock: Economic Advantage**

Benefit Options (NPC-dependent):
- Discount at NPC's shop (10-20% off equipment)
- Reduced costs for specific resource type (cheaper food from innkeeper)
- Special delivery jobs with better pay (25% higher rewards)

Mechanical Impact:
- Improved coin efficiency
- Faster equipment acquisition
- Better profit margins on deliveries
- Economic pressure reduced

**Bond Level 4 Unlock: Stat Training**

Benefit: NPC trains player in their expertise area
- Permanent stat point increase in specific stat
- One-time benefit (can't repeat)
- Significant impact (saves 10-15 challenges worth of XP)

Example:
- Veteran guard trains Authority (+1 Authority)
- Scholar trains Insight (+1 Insight)
- Merchant trains Diplomacy (+1 Diplomacy)

Mechanical Impact:
- Unlocks new choices requiring higher stat
- Easier challenges in that stat's domain
- Accelerates build specialization
- Major power spike

**Bond Level 5 Unlock: Exclusive Access**

Benefit Options (NPC-dependent):
- Unique delivery opportunities (high-risk, high-reward)
- Access to special routes or locations (previously hidden)
- Special equipment purchase options (unique items)
- Emergency resource assistance (safety net in crisis)

Mechanical Impact:
- New opportunities unavailable elsewhere
- Reduced risk (emergency backup)
- World expansion (new locations)
- Strategic flexibility

### The Investment Tension

**Immediate Costs:**
- Time segments spent on NPC scenes = time NOT spent earning coins
- Some NPCs require gifts (direct coin cost)
- Delays equipment purchases
- Increases short-term resource pressure

**Delayed Benefits:**
- Benefits appear only at Level 2+ (after 2+ scenes)
- Full benefits require Level 4-5 (after 4-5 scenes)
- 8-20 time segments total investment per NPC
- Equivalent to 2-4 delivery cycles

**Strategic Calculation:**
- Route shortcut saves time on every future delivery
  - After 10 uses, investment pays off
  - After 50 uses, massive net gain
- Stat increase valuable forever
  - Unlocks content immediately
  - Enables new strategies permanently
- Economic benefits compound
  - Every transaction improved
  - Months of cumulative savings

**The Impossible Choice:**
Spend today's resources on NPC for future benefit (delayed gratification, long-term thinking) OR take delivery for immediate survival needs (immediate gratification, short-term necessity)?

### NPC Availability and Stat Requirements

**NPCs exist at specific locations:**
- Merchant at market location
- Guard captain at garrison location
- Scholar at library location
- Must travel to location to interact

**Some NPCs require stat thresholds to begin bonding:**
- Cannot approach merchant without minimum Rapport 2
- Cannot approach guard captain without minimum Authority 2
- Cannot approach thief contact without minimum Cunning 3

**Why stat gates on NPCs:**
- Creates progression gates (can't access all NPCs immediately)
- Rewards stat development (unlocks NPC access)
- Specialization matters (different builds access different NPCs first)
- Verisimilitude (merchant won't talk to unfriendly stranger)

### Relationship Build Variety

**NPC-Focused Build:**
- Prioritize relationship investment
- Accept short-term economic pressure
- High time segment investment in conversations
- Benefits: Route shortcuts, stat training, exclusive access
- Drawbacks: Delayed equipment acquisition, economic struggle early

**Equipment-Focused Build:**
- Minimize relationship investment
- Prioritize economic accumulation
- Low time segment investment in NPCs
- Benefits: Mechanical advantages from equipment
- Drawbacks: No route shortcuts, no stat training, harder challenges

**Balanced Build:**
- Moderate both investments
- Flexible approach
- Benefits: Some of both advantages
- Drawbacks: Neither maximized, longer to achieve full benefits

---

## 3.5 Content Unlocking: Entity Spawning Over Boolean Gates

### The Requirement Inversion Principle

**Traditional approach (WRONG):**
- All content exists from start, hidden behind gates
- Completion sets boolean flags
- Flags unlock hidden content
- "If player completed Phase 1, reveal Phase 2"

**Wayfarer approach (CORRECT):**
- Content doesn't exist initially
- Completion spawns new content entities
- New entities added to world collections
- "Phase 1 completion creates Phase 2 entity"

### How Content Unlocking Works

**Scene Completion Rewards Spawn New Scenes:**

Example:
```
Scene: "Investigate the Mill - Phase 1"
Completion Reward: SpawnedScenes = ["investigate_mill_phase_2"]

Effect:
  1. Phase 2 scene template exists in catalogue
  2. CreateSceneFromTemplate("investigate_mill_phase_2")
  3. New scene added to GameWorld.Scenes
  4. Scene appears at location specified in template
  5. Player sees new button at location

Result: Phase 2 NOW EXISTS (didn't before)
```

**A-Story Progression Unlocks Regions:**

Example:
```
A-Story Phase 5 Completion
Reward: SpawnedLocations = ["eastern_region_hub"]

Effect:
  1. Eastern region template exists
  2. CreateLocationsFromTemplate("eastern_region_*")
  3. New locations added to GameWorld.Locations
  4. New routes created connecting to existing world
  5. Routes appear in travel screen

Result: Eastern region NOW ACCESSIBLE (didn't exist before)
```

**NPC Bonds Unlock Special Scenes:**

Example:
```
NPC Bond Level 4 Achieved
Reward: SpawnedScenes = ["npc_personal_crisis"]

Effect:
  1. Personal crisis scene template exists
  2. CreateSceneFromTemplate with NPC context
  3. Scene added at NPC's location
  4. Appears as special opportunity

Result: Personal storyline NOW AVAILABLE (wasn't before)
```

### Why Entity Spawning Is Superior

**Single Source of Truth:**
- If it's in GameWorld.Scenes, it exists and is visible
- No dual state (exists but hidden)
- No synchronization bugs
- Clear semantics: Empty collection = no content here

**Enables Dynamic Content:**
- AI can generate scenes at runtime
- Investigations can spawn varied challenges
- Rewards can create unique opportunities
- Not limited to pre-authored content

**Simpler UI Code:**
- Just render what's in collection
- No filtering logic
- No "show if unlocked" checks
- Direct mapping: Collection → UI display

**Perfect Information Maintained:**
- Player sees what exists (no mystery gates)
- Requirements are resource arithmetic, not boolean checks
- "Can I afford this?" not "Have I unlocked this?"

### Content Discovery Patterns

**Linear Progression (A-Story):**
- Phase 1 completion spawns Phase 2
- Phase 2 completion spawns Phase 3
- Continues indefinitely (infinite A-story)
- Provides structure without end

**Branching Discovery (Exploration):**
- Investigating location spawns multiple follow-up scenes
- Player chooses which to pursue
- Different branches lead to different content
- No single "correct" path

**Emergent Opportunities (NPC Relationships):**
- High bond levels spawn personal storylines
- Different NPCs unlock different content types
- Relationship choices create unique narratives
- Player-driven content emergence

**World Expansion (Regions):**
- A-story phases unlock new regions
- New regions contain new venues
- New venues contain new locations
- New locations have new NPCs
- World expands organically

---

## 3.6 Build Variety: Specialization Creates Identity

### Stat-Focused Builds

**Investigator (Insight + Cunning):**

Primary Stats: Insight 5+, Cunning 4+
Secondary Stats: Rapport 2, Authority 1, Diplomacy 2

Strengths:
- Dominates Mental challenges
- Spots hidden paths in exploration
- Excels at deduction and analysis
- High depth card access in Mental system

Weaknesses:
- Struggles with Social challenges
- Poor at direct confrontation
- Limited Authority choices disabled
- Forced into suboptimal approaches in conversations

Playstyle:
- Investigation-focused
- Prefers Mental challenges over Social
- Explores for clues and patterns
- Avoids direct conflict

**Diplomat (Rapport + Diplomacy):**

Primary Stats: Rapport 5+, Diplomacy 4+
Secondary Stats: Insight 2, Authority 1, Cunning 2

Strengths:
- Dominates Social challenges
- Befriends NPCs easily
- Excellent at negotiation and persuasion
- High depth card access in Social system

Weaknesses:
- Struggles with Mental challenges
- Misses subtle clues
- Poor at complex investigations
- Limited Insight choices disabled

Playstyle:
- Relationship-focused
- Prioritizes NPC bonds
- Solves problems through conversation
- Explores through social connections

**Commander (Authority + Diplomacy):**

Primary Stats: Authority 5+, Diplomacy 4+
Secondary Stats: Insight 2, Rapport 2, Cunning 1

Strengths:
- Balanced Social (directive and balanced)
- Commands respect in confrontations
- Good at negotiation from position of strength
- Access to Authority-gated content

Weaknesses:
- Weak in Insight and Cunning
- Struggles with subtle investigations
- Poor at deception and misdirection
- Limited strategic complexity

Playstyle:
- Leadership-focused
- Direct approach to problems
- Formal solutions through authority
- Commands rather than persuades

### Resource-Focused Builds

**Economic Specialist:**

Focus: Maximize coin generation and efficiency
Strategies:
- Learn all route shortcuts (minimize costs)
- Purchase efficiency equipment early (compound savings)
- Optimize delivery selection (highest profit routes)
- Minimize non-essential spending

Strengths:
- High coin reserves (resource buffer)
- Can afford expensive options
- Equipment advantages
- Economic crises manageable

Weaknesses:
- Lower stat development (fewer challenges)
- Weaker NPC relationships (less time investment)
- Relies on coins to solve problems
- Vulnerable when coins depleted

**Relationship Specialist:**

Focus: Maximize NPC bonds and social benefits
Strategies:
- Prioritize NPC scenes over deliveries
- Accept economic pressure short-term
- Build comprehensive NPC network
- Leverage relationship benefits

Strengths:
- Route shortcuts (all major routes optimized)
- Stat training bonuses (higher stats than normal)
- Economic advantages (discounts, better jobs)
- Exclusive access (special content)

Weaknesses:
- Economic struggle early game
- Delayed equipment acquisition
- High time investment required
- Benefits come late

### Challenge-Focused Builds

**Challenge Master:**

Focus: Maximize tactical challenge success
Strategies:
- Develop stats for challenge system mastery
- Accept challenge options frequently (high risk)
- Build Understanding quickly (tier unlocking)
- Master all three challenge types

Strengths:
- Excellent challenge success rate
- High reward efficiency (challenge bonuses)
- Flexible (can handle all challenge types)
- Tactical skill demonstration

Weaknesses:
- High resource costs (challenges expensive)
- Riskier progression (failures costly)
- Time-intensive (challenges take longer)
- Requires mechanical skill

### Hybrid Builds

**Balanced Generalist:**

Stats: All stats at 3-4 (no specialization)
Resources: Moderate coin reserves, moderate relationships
Challenges: Occasional engagement (not focused)

Strengths:
- Flexible (multiple approaches available)
- Handles variety adequately
- No catastrophic vulnerabilities
- Adaptable to situations

Weaknesses:
- Never dominates anything
- No distinct advantages
- Average at all content types
- No strong identity

---

## 3.7 Progression Pacing and Long-Term Goals

### Short-Term Goals (1-3 hours)

**Immediate survival:**
- Earn enough coins for food and lodging
- Complete delivery without major losses
- Restore depleted resources

**Incremental stat growth:**
- Complete 3-5 challenges to increase single stat by 1
- Visible progress within single session
- Unlocks small improvements

**Initial relationship building:**
- Meet 2-3 NPCs
- Advance 1 NPC to Bond Level 2
- Unlock first route shortcut

### Medium-Term Goals (5-15 hours)

**First equipment purchase:**
- Accumulate 60-100 coins
- Buy first major equipment upgrade
- Feel power increase

**Stat specialization emergence:**
- Two stats reach 4-5 (specialized)
- Three stats remain 1-2 (neglected)
- Build identity becomes clear

**NPC network development:**
- 3-5 NPCs at Bond Level 2-3
- Multiple route shortcuts unlocked
- Economic advantages active

**A-Story Phase 2-3:**
- Pursue main storyline
- Unlock second region
- Expand world significantly

### Long-Term Goals (20+ hours)

**Build optimization:**
- Primary stats reach 6-7 (mastery)
- Equipment set complete (3-4 major pieces)
- Route network fully optimized

**Deep NPC relationships:**
- 2-3 NPCs at Bond Level 4-5
- Stat training bonuses acquired
- Exclusive content accessed

**Challenge mastery:**
- Understanding at high levels
- Depth 3-4 cards accessible
- Tactical system mastered

**World exploration:**
- Multiple regions accessible
- Dozens of locations discovered
- Complex route networks mapped

### Infinite Horizon (50+ hours)

**A-Story continues:**
- Procedural generation provides endless progression
- New regions continuously unlocked
- World expands indefinitely

**Build refinement:**
- Fine-tune stat distribution
- Experiment with different approaches
- Explore alternative build paths

**Challenge excellence:**
- Master edge cases
- Optimize tactical play
- Demonstrate skill consistently

**Social network expansion:**
- Meet all NPCs
- Maximize key relationships
- Build comprehensive social capital

---

## 3.8 Related Documentation

### Other Design Sections

- **[01_design_vision.md](01_design_vision.md)** - Core experience statement, specialization philosophy
- **[02_core_gameplay_loops.md](02_core_gameplay_loops.md)** - How progression integrates with loops
- **04_choice_design_patterns.md** - How stats gate choices, requirement patterns
- **05_challenge_systems.md** - Stat manifestation in challenges, XP gain mechanics
- **06_world_structure.md** - Region unlocking, content discovery patterns

### Technical Architecture (arc42)

- **[arc42/05_building_block_view.md](../05_building_block_view.md)** - Player entity, stat tracking, resource management
- **[arc42/08_concepts.md](../08_concepts.md)** - Progression persistence, save system
- **[arc42/09_architecture_decisions.md](../09_architecture_decisions.md)** - Why entity spawning over boolean gates

### Core Philosophy

- **[REQUIREMENT_INVERSION_PRINCIPLE.md](../REQUIREMENT_INVERSION_PRINCIPLE.md)** - Complete pedagogical explanation of resource systems
- **[DESIGN_PHILOSOPHY.md](../DESIGN_PHILOSOPHY.md)** - Principle 4 (Inter-Systemic Rules), Principle 6 (Resource Scarcity), Principle 12 (Unified Stats)
- **[GLOSSARY.md](../GLOSSARY.md)** - StoryCube, InvestigationCube, Understanding, Bond Level definitions

---

**Document Status:** Production-ready game design specification
**Last Updated:** 2025-11
**Maintained By:** Design team
