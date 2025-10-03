# Critical Design Decisions and Gaps

## Overview

This document identifies critical design decisions that must be made before or during V2 implementation, gaps in the current design that need addressing, and strategic questions about Wayfarer's identity and future. These decisions will fundamentally affect implementation priorities, content creation strategies, and player experience.

---

## Section 1: Core Identity Decisions

### Decision 1.1: What IS Wayfarer V2?

**The Fundamental Question**: Are we building a better version of current Wayfarer or a different game entirely?

#### Option A: Enhanced Current Game (30% change)
- **Core**: Conversations remain primary gameplay
- **Investigations**: Optional side content for variety
- **Travel**: Simplified obstacles, not core challenge
- **Audience**: Current players stay happy
- **Marketing**: "Wayfarer with more depth"
- **Risk**: Low - incremental improvement
- **Development**: 3-4 weeks

#### Option B: Investigation-Focused Game (70% change)
- **Core**: Investigations are primary gameplay
- **Conversations**: Support investigations with information
- **Travel**: Meaningful challenges requiring preparation
- **Audience**: New players, some current players leave
- **Marketing**: "Mystery exploration RPG"
- **Risk**: High - genre shift
- **Development**: 6-8 weeks

#### Option C: True Hybrid (50% change)
- **Core**: Both systems equally important
- **Players**: Choose their focus each session
- **Balance**: Harder to achieve
- **Audience**: Broader appeal potential
- **Marketing**: "Choose your adventure style"
- **Risk**: Medium - may satisfy neither group fully
- **Development**: 5-6 weeks

**RECOMMENDATION**: Option B with careful transition
- Current content suggests investigation focus
- Miller's Daughter IS an investigation game
- Conversations become tools for investigation
- Market as evolution, not replacement

### Decision 1.2: Genre and Setting Definition

**Current**: "Low fantasy historical like Frieren/Roadwarden"

**Need Specific Decisions**:

#### Time Period
- **Medieval (1100-1300)**: Water mills, limited technology, feudal structure
- **Late Medieval (1300-1500)**: More complex society, early Renaissance
- **Ambiguous Historical**: Vaguely pre-industrial, avoid specific dates
- **Fantasy Timeless**: Historical elements without real-world constraints

**RECOMMENDATION**: Late Medieval (1300-1500)
- Allows water mills, basic commerce, legal systems
- Supports investigation complexity
- Familiar to fantasy readers

#### Geographic Inspiration
- **Northern European**: Germanic names, forests, mills
- **Mediterranean**: Different architecture, social structure
- **Fantasy Amalgam**: Mix inspirations freely
- **Specific Region**: Model on real location

**RECOMMENDATION**: Northern European (Germanic/Nordic)
- Fits mill architecture
- Supports forest/creek environments
- Established fantasy convention

#### Magic Level
- **None**: Pure historical, no magical elements
- **Folklore**: Superstitions, herbs, folk medicine, no real magic
- **Subtle**: Magic exists but rare, mostly rumors
- **Present**: Magic is real but uncommon

**RECOMMENDATION**: Folklore level
- Maintains verisimilitude
- Allows mysterious atmosphere
- Doesn't require magic systems
- Investigations stay grounded

#### Technology Constraints
**Allowed**:
- Water mills, windmills
- Basic metalworking (iron, simple steel)
- Rope, pulleys, basic mechanisms
- Oil lamps, candles
- Parchment, early paper
- Simple locks, keys

**Forbidden**:
- Gunpowder
- Clocks (sundials OK)
- Printing press
- Glass windows (shutters, oiled cloth)
- Compasses (unless rare import)
- Modern concepts (crowbars → pry bars)

#### Naming Conventions
```
NPCs:
- Germanic base: Hans, Greta, Wilhelm, Martha
- Latin church names: Marcus, Elena, Sophia
- Occupational surnames: Miller, Smith, Cook
- Location surnames: Brookside, Hillcrest

Places:
- Descriptive: Old Mill, Widow's Creek
- Historical: Saint Elena's, King's Road
- Natural: Darkwood, Clearwater
```

---

## Section 2: System Integration Gaps

### Gap 2.1: The Investigation-Conversation Loop

**Problem**: Investigations find evidence, but why do I need to keep having conversations?

**Current Design**: NPCs give quests → Investigations need info → Different NPCs provide

**Missing**: What makes investigation discoveries drive back to conversations?

#### Solution A: Mystery Chains
```
Investigation A reveals → Clue needs interpretation →
NPC B can explain → Reveals Investigation C exists →
NPC D has access → Conversation unlocks new location
```

#### Solution B: Evidence Confrontations
```
Find criminal evidence → Must confront accused →
Special conversation type → Authority challenge →
Success = confession → Unlocks legal investigation
```

#### Solution C: Knowledge Trading
```
Discovery valuable to NPC A → Trade for information →
Information valuable to NPC B → Build web of exchanges →
Each trade reveals more → Investigations multiply
```

**RECOMMENDATION**: All three in different contexts
- Mystery chains for main narrative
- Confrontations for dramatic moments
- Knowledge trading for side content

### Gap 2.2: Discovery Cascade System

**Problem**: Completing investigation feels like ending, not beginning

**Missing**: How do discoveries create new content?

#### Cascade Types Needed:

**Narrative Cascades**:
```
Mill smuggling → Courthouse records investigation
Courthouse → Harbor investigation
Harbor → Noble estate investigation
```

**Social Cascades**:
```
Evidence found → NPCs take sides
Factions form → New conversation dynamics
Relationships shift → New requests appear
```

**Mechanical Cascades**:
```
Tool discovered → New approaches available
Route found → New locations accessible
Technique learned → Investigation methods improve
```

**RECOMMENDATION**: Design 3-5 cascade chains for launch
- Each investigation connects to 2-3 others
- Some connections obvious, some hidden
- Player choice determines which cascades trigger

### Gap 2.3: Obligation Selection Strategy

**Problem**: Without queue positions, why choose one obligation over another?

**Missing**: Strategic dimensions for obligation choice

#### Proposed Obligation Properties:
```
struct ObligationProfile {
    Urgency: 1-5 (deadline pressure)
    Reward: 1-5 (payment value)
    Difficulty: 1-5 (preparation needed)
    Narrative: 1-5 (story importance)
    Synergy: 1-5 (combines with others)
    Discovery: 1-5 (investigation potential)
}
```

#### Strategic Patterns:
- **Efficiency**: Choose high-synergy obligations
- **Profit**: Choose high-reward obligations
- **Story**: Choose high-narrative obligations
- **Explorer**: Choose high-discovery obligations

**RECOMMENDATION**: Display obligation profiles clearly
- Let players see all dimensions
- Different players value different aspects
- Creates meaningful choice without optimal strategy

---

## Section 3: Content Scalability Crisis

### Gap 3.1: Creation-to-Consumption Ratio

**Problem**: Miller's Daughter takes 30 hours to create, 45 minutes to play

**Math**:
- Ratio: 40:1 (unsustainable)
- Need: 20 investigations minimum
- Time: 600 hours = 15 weeks full-time
- Result: Content starvation

### Solution: Modular Investigation System

#### Component Library Structure:
```
BASE PATTERNS (5 types):
├── Lost & Found (find person/object)
├── Hidden Truth (uncover secret)
├── Structural Danger (navigate hazard)
├── Social Conflict (resolve dispute)
└── Mystery Object (understand artifact)

LOCATIONS (10 settings):
├── Abandoned Building (mill, manor, temple)
├── Natural Cave (bandits, hermit, monster)
├── Forest Grove (ritual site, hidden camp)
├── Riverside (crossing, hideout, wreck)
├── Ruins (ancient, recent, mysterious)
├── Occupied Building (shop, home, office)
├── Underground (cellar, tunnel, dungeon)
├── Highland (cliff, mountain, tower)
├── Wetland (marsh, swamp, bog)
└── Crossroads (meeting, ambush, marker)

COMPLICATIONS (10 types):
├── Someone Else Investigating (rival, ally, enemy)
├── Natural Hazard (weather, animals, terrain)
├── Structural Danger (collapse, trap, mechanism)
├── Time Pressure (deadline, race, decay)
├── Social Discovery Risk (witnessed, reported)
├── Resource Challenge (equipment, stamina)
├── Knowledge Requirement (language, history)
├── Moral Dilemma (help vs. profit, truth vs. peace)
├── Red Herring (false lead, misdirection)
└── Escalation (small → large discovery)

REVELATIONS (10 types):
├── Criminal Activity (theft, smuggling, murder)
├── Lost History (forgotten event, hidden truth)
├── Hidden Relationship (affair, family, alliance)
├── Valuable Discovery (treasure, knowledge, map)
├── Tragic Truth (accident, sacrifice, loss)
├── Conspiracy (plot, collaboration, betrayal)
├── Supernatural Element (curse, blessing, artifact)
├── Identity Reveal (disguise, imposter, heir)
├── Scientific Discovery (method, formula, technique)
└── Personal Secret (shame, pride, fear)
```

#### Assembly Process:
1. Pick pattern + location + complication + revelation
2. Generate connecting narrative tissue
3. Create appropriate requirements/dangers
4. Link to existing NPCs
5. Output: Functional investigation in 1 hour

#### Variations from Same Components:
```
Example: Lost & Found + Abandoned Mill + Time Pressure + Criminal Activity
Version 1: Find missing child before storm, discover kidnapping
Version 2: Find stolen goods before owner returns, discover fraud
Version 3: Find evidence before trial, discover false accusation
```

**5 × 10 × 10 × 10 = 5,000 possible combinations**

#### Implementation Plan:
- Week 1: Build component templates
- Week 2: Create assembly tool
- Week 3: Generate 10 test investigations
- Week 4: Polish and integrate
- Result: 10 investigations in 40 hours instead of 300

### Gap 3.2: Authored vs. Generated Balance

**Question**: Which content deserves hand-crafting vs. procedural generation?

#### Bespoke Content (Hand-Crafted):
- Main story investigations (5-7 total)
- NPC introduction scenes
- Major revelations
- Tutorial content
- Climactic moments

**Time**: 20-30 hours each, 150-200 hours total

#### Modular Content (Semi-Procedural):
- Side investigations (15-20 total)
- Random events
- Routine deliveries
- Repeatable content

**Time**: 3-5 hours each, 60-100 hours total

#### AI-Assisted Content (Future):
- Variation descriptions
- NPC dialogue adaptation
- Investigation descriptions
- Dynamic reactions

**Time**: Minutes per generation

**RECOMMENDATION**: Launch with 5 bespoke + 10 modular
- Bespoke for identity and quality
- Modular for variety and replayability
- AI for personalization later

---

## Section 4: Economic System Gaps

### Gap 4.1: Money Becomes Meaningless

**Problem**: After buying basic equipment (5 coins), money has no purpose

**Current Economy**:
```
Income:
- Work: 5 coins/block
- Deliveries: 5-15 coins
- Discoveries: 0-10 coins
- Total: 10-30 coins/day possible

Expenses:
- Equipment: 3-10 coins (ONE TIME)
- Food: 2-3 coins (occasional)
- Nothing else...
```

### Solution: Ongoing Money Sinks

#### Services (Recurring):
```
Information:
- Rumors: 2 coins (random tips)
- Specific intel: 5 coins (targeted info)
- Decode documents: 10 coins (expert knowledge)

Access:
- Location permits: 5-10 coins/day
- Bribe officials: 10-20 coins
- Exclusive areas: 15-30 coins

Medical:
- Healing: 5 coins per 20 health
- Cure conditions: 10-15 coins
- Preventive care: 5 coins (immunity buff)

Social:
- Gifts for NPCs: 5-15 coins (relationship boost)
- Tavern rounds: 3-5 coins (gather rumors)
- Event participation: 10-20 coins
```

#### Consumables (Depleting):
```
Degrading Equipment:
- Lantern oil: 1 coin per 2 hours
- Tool maintenance: 2 coins per 5 uses
- Rope wear: Replace after 10 uses

Special Items:
- Investigation supplies: 5 coins/investigation
- Travel provisions: 3 coins/long journey
- Protective items: 5-10 coins (single use)
```

#### Investment Opportunities:
```
Long-term:
- Improve routes: 20-50 coins (permanent benefit)
- Establish safe houses: 30 coins (rest points)
- Information network: 25 coins (passive intel)
```

**RECOMMENDATION**: Add all three categories
- Services for immediate needs
- Consumables for preparation
- Investments for progression
- Result: 10-20 coins/day spending

### Gap 4.2: Work-Play Balance

**Problem**: Working for coins is boring but necessary

**Current**: Flat 5 coins per block, no variety

#### Enhanced Work System:
```
Work Types:
- Manual labor: 3-5 coins (based on stamina)
- Skilled work: 5-8 coins (based on stats)
- Dangerous work: 8-12 coins (risk involved)
- Special requests: 10-20 coins (mini-investigations)
```

#### Work Integration:
- Work reveals rumors and knowledge
- Work builds NPC relationships
- Work can trigger events
- Work provides equipment access

**RECOMMENDATION**: Make work mechanically interesting
- Not just time-for-coins exchange
- Provides secondary benefits
- Remains optional for skilled players

---

## Section 5: Player Experience Gaps

### Gap 5.1: Tutorial and Onboarding

**Problem**: New systems need teaching, but tutorials interrupt flow

#### Required Teaching:
1. Investigations have phases
2. Phases save progress
3. Equipment enables actions
4. Knowledge gates choices
5. Failure teaches requirements
6. Preparation matters

#### Solution: Integrated Tutorial Investigation

**"The Lost Cat" (Tutorial)**:
```
Phase 1: Search Garden
- Choice A: Look in bushes (easy)
- Choice B: Climb tree (needs stamina)
- Teaches: Basic choices

Phase 2: Ask Neighbors
- Choice A: Talk to child (easy)
- Choice B: Convince adult (needs rapport)
- Teaches: Stat requirements

Phase 3: Retrieve Cat
- Choice A: Use ladder (needs equipment)
- Choice B: Lure with food (needs item)
- Teaches: Equipment enables

Completion:
- Martha grateful
- Free meal reward
- Knowledge gained
- Opens real investigations
```

**Design Principles**:
- Low stakes (cat, not murder)
- Quick completion (10 minutes)
- All mechanics shown
- Failure is safe
- Success feels good

### Gap 5.2: Learning Curve

**Problem**: Failure should teach, not frustrate

#### Graduated Failure Information:
```
Attempt 1 Failure:
"The creek is too dangerous to cross"
→ Learn: Need different approach

Attempt 2 Failure:
"You need something to help with crossing"
→ Learn: Equipment helps

Attempt 3 Failure:
"Rope would make this much safer"
→ Learn: Specific equipment

Attempt 4 Success:
"Using rope, you cross safely"
→ Learn: Preparation worked
```

**RECOMMENDATION**: Three-tier revelation
1. Vague hint (something wrong)
2. Category hint (need equipment)
3. Specific hint (need rope)

### Gap 5.3: Choice Paralysis

**Problem**: Too many investigation choices overwhelm

#### Solution: Choice Presentation Hierarchy
```
Primary Choices (Always Visible):
- Direct approach (usually hardest)
- Cautious approach (usually safest)
- Retreat (always available)

Secondary Choices (If Requirements Met):
- Equipment-enabled options
- Knowledge-enabled options
- Stat-enabled options

Hidden Choices (Discovered):
- Secret approaches
- Creative solutions
- Combined methods
```

**UI Design**:
- Show 3-4 choices maximum initially
- Expand to show more if equipped/knowledgeable
- Gray out impossible choices
- Tooltip explains requirements

---

## Section 6: Technical Implementation Gaps

### Gap 6.1: State Persistence Complexity

**Problem**: Investigations need complex state tracking

#### What Must Persist:
```
Per Investigation:
- Current phase
- Completed phases
- Choices made
- Knowledge gained
- World changes
- Attempt count
- Custom data

Per Player:
- All knowledge
- All equipment
- All reputation
- All progress

Per World:
- NPC states
- Location states
- Route improvements
- Global flags
```

#### Solution: Hierarchical State System
```
SaveGame
├── PlayerState
│   ├── Knowledge[]
│   ├── Equipment[]
│   └── Stats
├── WorldState
│   ├── NPCStates{}
│   ├── LocationStates{}
│   └── GlobalFlags{}
└── ProgressState
    ├── Investigations{}
    ├── Routes{}
    └── Obligations{}
```

**RECOMMENDATION**: Implement incrementally
- Start with investigation progress only
- Add world state as needed
- Full persistence by launch

### Gap 6.2: AI Integration Architecture

**Problem**: AI needs context but can't have authority

#### Context Requirements:
```
For Dialogue Generation:
- NPC personality
- Relationship level
- Recent events
- Current emotions
- Knowledge possessed

For Description Generation:
- Location details
- Time/weather
- Player history
- Investigation state
- Equipment carried
```

#### Authority Boundaries:
```
AI CAN:
- Generate flavor text
- Vary descriptions
- Create dialogue
- Add atmosphere
- Personalize responses

AI CANNOT:
- Change requirements
- Modify outcomes
- Create mechanics
- Alter progressions
- Invent content
```

#### Implementation Architecture:
```
AIService
├── ContextBuilder (gathers game state)
├── PromptGenerator (creates structured prompts)
├── ResponseParser (ensures compliance)
├── CacheManager (stores generated content)
└── FallbackProvider (mechanical descriptions)
```

**RECOMMENDATION**: Phase AI integration
- Phase 1: No AI (mechanical descriptions)
- Phase 2: Basic AI (dialogue only)
- Phase 3: Full AI (all flavor text)

---

## Section 7: Missing Design Elements

### Element 7.1: Metaprogression System

**Current**: Nothing carries between playthroughs

**Options**:
1. **Knowledge Persistence**: Player remembers solutions
2. **Relationship Echoes**: Some reputation carries over
3. **Unlocked Content**: New starting options
4. **Mechanical Bonuses**: Small stat increases
5. **Achievement Rewards**: Cosmetic or mechanical

**RECOMMENDATION**: Knowledge + Unlocked Content
- Player knowledge persists (can skip tutorials)
- Completed investigations unlock new starts
- No mechanical advantages (preserve challenge)

### Element 7.2: Partial Success States

**Current**: Binary success/failure

**Enhanced Outcomes**:
```
Complete Success (100%):
- All objectives achieved
- Minimal resource cost
- Maximum rewards

Partial Success (60-99%):
- Main objective achieved
- Some complications
- Reduced rewards

Costly Success (40-59%):
- Objective achieved
- Major complications
- Minimal rewards

Failure with Progress (20-39%):
- Objective failed
- Some progress made
- Learning gained

Complete Failure (0-19%):
- No progress
- Resources lost
- Must retry
```

**RECOMMENDATION**: Implement for major investigations
- Adds nuance to outcomes
- Reduces failure frustration
- Creates more stories

### Element 7.3: Dynamic World Elements

**Current**: Static world state

**Potential Dynamics**:
- NPC schedules shift based on events
- Locations change after investigations
- Routes improve/degrade over time
- Economic fluctuations change prices

**RECOMMENDATION**: Start with event-based changes
- Investigations trigger world updates
- NPCs react to discoveries
- Keep predictable for players
- Add time-based changes later

---

## Section 8: Strategic Priorities

### Must Resolve Before Implementation

1. **Core Identity** (Investigation-primary vs. Conversation-primary)
2. **Content Strategy** (Modular system design)
3. **Economic Balance** (Money sink implementation)
4. **Genre Details** (Setting, technology, terminology)

### Must Resolve During Week 1

1. **State Persistence** (What/how to save)
2. **Knowledge System** (Structure and UI)
3. **Equipment Enables** (Specific mappings)
4. **Tutorial Design** (How to teach)

### Can Resolve During Development

1. **AI Integration** (Start without, add later)
2. **Metaprogression** (Post-launch feature)
3. **Dynamic World** (Enhancement)
4. **Partial Success** (Refinement)
5. **Procedural Generation** (Content expansion)

---

## Conclusion

These critical decisions and gaps represent the difference between a successful V2 transition and a confused half-measure. By addressing these systematically:

1. **Define identity clearly** - Investigation-primary with conversation support
2. **Build content pipeline** - Modular system for sustainability
3. **Fix economy** - Multiple money sinks for ongoing purpose
4. **Teach gradually** - Integrated tutorial and graduated learning
5. **Plan incrementally** - Core first, enhance later

The V2 transition is ambitious but achievable if these decisions are made consciously and implemented systematically. The key is maintaining focus on the core experience (investigations driving discoveries) while ensuring all systems support rather than compete with that core.