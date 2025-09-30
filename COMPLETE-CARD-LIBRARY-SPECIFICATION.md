# Complete Card Library Specification
## Momentum Gates Access, Statements Gate Power

This document specifies the complete card library using a hybrid system where **momentum-based tier unlocking** provides the primary progression, while **Statement requirements** gate powerful signature variants for specialists.

---

## Table of Contents

1. [Core Design Philosophy](#core-design-philosophy)
2. [The Two-Tiered System](#the-two-tiered-system)
3. [Mathematical Foundation](#mathematical-foundation)
4. [Card Structure](#card-structure)
5. [Complete Card Library](#complete-card-library)
6. [Implementation Guidelines](#implementation-guidelines)
7. [Strategic Implications](#strategic-implications)

---

## Core Design Philosophy

### The Hybrid Approach

**PRIMARY: Momentum-Based Tier Unlocking**
- Momentum thresholds (6/12/18) unlock depth tiers
- Stat specialization bonuses (+1/+2/+3 depth) for high stat levels
- Provides natural progression through conversations
- Works for all build types (generalist and specialist)

**SECONDARY: Statement Requirements for Power Variants**
- Most cards (80%) have NO Statement requirements
- Rare "Signature" variants (20%) require Statement counts
- Provides bonus effects for specialists who focus one stat
- Optional optimization - casual players can ignore entirely

### Design Goals Achieved

✓ **Sustainable Economy**: Echo cards provide efficiency without blocking progression
✓ **Clear Progression**: Momentum thresholds unlock new options predictably
✓ **Specialist Rewards**: Statement variants provide meaningful optimization
✓ **Verisimilitude**: Powerful conclusions require conversational foundation
✓ **Mathematical Validity**: Requirements achievable within conversation length

---

## The Two-Tiered System

### Primary: Momentum Tier Unlocks

| Momentum | Tier Unlocked | Depths Available | Strategic Milestone |
|----------|---------------|------------------|---------------------|
| 0 | Tier 1 | 1-2 | Foundation cards only |
| 6+ | Tier 2 | 1-4 | Standard cards unlock |
| 12+ | Tier 3 | 1-6 | Advanced cards unlock (Enhanced Goal) |
| 18+ | Tier 4 | 1-8 | Master cards unlock |

**Stat Specialization Bonuses:**
```
Stat Level 3-5: +1 depth bonus for that stat's cards
Stat Level 6-8: +2 depth bonus for that stat's cards
Stat Level 9-10: +3 depth bonus for that stat's cards

Example: Momentum 8 (Tier 2), Insight 7
- All cards: Depths 1-4 (Tier 2 baseline)
- Insight cards: Depths 1-6 (baseline + 2 bonus)
```

### Secondary: Statement Signature Variants

**Base Cards** (80% of library):
- No Statement requirements
- Accessible immediately when tier unlocks
- Provide solid effects for all players

**Signature Variants** (20% of library):
- Require 2-8 Statement cards of matching stat type
- Provide enhanced effects (+20-30% more powerful)
- Reward specialists who focus one stat
- Optional optimization path

---

## Mathematical Foundation

### Conversation Length Analysis

**Typical 12-15 Turn Conversation:**

```
Phase 1: Foundation (Turns 1-3)
- Cards played: 6
- Echo ratio: 70% (4 Echo, 2 Statement)
- Statement count: 2

Phase 2: Standard (Turns 4-7)
- Cards played: 8
- Echo ratio: 50% (4 Echo, 4 Statement)
- Statement count: 6 total

Phase 3: Advanced/Master (Turns 8-12)
- Cards played: 8
- Echo ratio: 40% (5 Echo, 3 Statement)
- Statement count: 9 total

TOTAL: 22 cards played, 9 Statement cards
```

### Statement Accumulation Patterns

**Generalist Build** (playing 3-4 different stats):
- 9 Statements across 4 stats = ~2-3 per stat
- Can access Foundation signature variants (2+ requirement)
- Cannot access higher signature variants
- Uses base versions of Standard/Advanced cards

**Moderate Specialist** (70% one stat, 30% support stats):
- 9 Statements: 6-7 in primary, 2-3 in support
- Can access Standard signature variants (3-4 requirement)
- Can access some Advanced signature variants (5 requirement)
- Meaningful optimization available

**Pure Specialist** (90% one stat):
- 9 Statements: 8+ in primary stat
- Can access ALL signature variants including Master (8+ requirement)
- Maximum optimization, highest power ceiling
- May struggle with resource diversity

### Requirement Thresholds by Tier

| Tier | Base Cards | Signature Requirement | Achievability |
|------|------------|----------------------|---------------|
| Foundation (1-2) | No requirement | 2+ Statements | Easy (generalists) |
| Standard (3-4) | No requirement | 3-4 Statements | Moderate (specialists) |
| Advanced (5-6) | No requirement | 5 Statements | Hard (focused specialists) |
| Master (7-8) | No requirement | 8 Statements | Very Hard (pure specialists) |

---

## Card Structure

### Card Template

```
NAME - [Base/Signature]
Stat, Depth X, Persistence, Initiative Cost
Requirement: [None | X+ Stat Statements]
Effect: [Formula-based effect]
Verisimilitude: [Why this effect makes sense]
```

### Persistence Distribution by Tier

**Foundation (Depths 1-2)**: 70% Echo
- Purpose: Sustainable building blocks
- Repeatable tools for consistent play

**Standard (Depths 3-4)**: 50% Echo
- Purpose: Balanced toolkit
- Mix of repeatable and committed plays

**Advanced (Depths 5-6)**: 40% Echo
- Purpose: Building toward finishers
- More committed plays, fewer repeatable

**Master (Depths 7-8)**: 20% Echo
- Purpose: Climactic moments
- Conversation-defining declarations

---

## Complete Card Library

### INSIGHT CARDS - Information Gathering → Analytical Conclusions

#### Foundation (Depth 1-2) - Always Available

**"Notice Detail" - Base**
```
Insight, Depth 2, Statement, 0 Initiative
Requirement: None
Effect: Draw 2 cards, +1 Momentum
Verisimilitude: Basic observation is the foundation of analysis
```

**"Quick Scan" - Base**
```
Insight, Depth 1, Echo, 0 Initiative
Requirement: None
Effect: Draw 2 cards
Verisimilitude: Rapid information gathering, repeatable technique
```

**"Ask Question" - Base**
```
Insight, Depth 1, Statement, 0 Initiative
Requirement: None
Effect: Draw 1 card, +1 Initiative
Verisimilitude: Questions open conversational opportunities
```

**"Careful Analysis" - Base**
```
Insight, Depth 2, Echo, 0 Initiative
Requirement: None
Effect: Draw 2 cards
Verisimilitude: Methodical examination, repeatable process
```

**"Deep Observation" - Signature**
```
Insight, Depth 2, Statement, 0 Initiative
Requirement: 2+ Insight Statements in Spoken
Effect: Draw 3 cards, +1 Momentum
Verisimilitude: Deeper insights emerge from sustained observation
```

**"Analytical Method" - Signature**
```
Insight, Depth 1, Echo, 0 Initiative
Requirement: 2+ Insight Statements in Spoken
Effect: Draw 2 cards, +1 Initiative
Verisimilitude: Refined techniques emerge from practice
```

#### Standard (Depth 3-4) - Unlocked at Momentum 6+

**"Identify Pattern" - Base**
```
Insight, Depth 4, Statement, 3 Initiative
Requirement: None
Effect: Draw 3 cards, +2 Momentum
Verisimilitude: Patterns emerge from observation
```

**"Connect Evidence" - Base**
```
Insight, Depth 3, Echo, 2 Initiative
Requirement: None
Effect: Draw 1 card per 2 total Statements (max 3)
Verisimilitude: More conversation history reveals more connections
```

**"Cross-Reference" - Base**
```
Insight, Depth 3, Statement, 2 Initiative
Requirement: None
Effect: Draw 2 cards, +1 Momentum, +1 Initiative
Verisimilitude: Referencing prior information builds understanding
```

**"Analytical Reasoning" - Base**
```
Insight, Depth 4, Echo, 3 Initiative
Requirement: None
Effect: Draw 3 cards
Verisimilitude: Systematic reasoning process
```

**"Complex Analysis" - Signature**
```
Insight, Depth 4, Statement, 3 Initiative
Requirement: 3+ Insight Statements in Spoken
Effect: Draw 4 cards, +3 Momentum
Verisimilitude: Deep analysis builds on substantial prior observations
```

**"Pattern Synthesis" - Signature**
```
Insight, Depth 3, Echo, 2 Initiative
Requirement: 3+ Insight Statements in Spoken
Effect: Draw 1 card per Insight Statement (max 4)
Verisimilitude: Synthesizing all prior analytical work
```

#### Advanced (Depth 5-6) - Unlocked at Momentum 12+

**"Draw Conclusion" - Base**
```
Insight, Depth 6, Statement, 5 Initiative
Requirement: None
Effect: Draw 4 cards, +5 Momentum
Verisimilitude: Logical conclusions from accumulated information
```

**"Synthesize Information" - Base**
```
Insight, Depth 5, Echo, 4 Initiative
Requirement: None
Effect: Draw 1 card per 2 total Statements (max 5)
Verisimilitude: Bringing together all conversation threads
```

**"Reveal Implication" - Base**
```
Insight, Depth 5, Statement, 4 Initiative
Requirement: None
Effect: Draw 3 cards, +4 Momentum, +2 Initiative
Verisimilitude: Hidden implications become clear
```

**"Perfect Deduction" - Signature**
```
Insight, Depth 6, Statement, 5 Initiative
Requirement: 5+ Insight Statements in Spoken
Effect: Draw 6 cards, +6 Momentum
Verisimilitude: Masterful deduction from comprehensive foundation
```

**"Analytical Mastery" - Signature**
```
Insight, Depth 5, Echo, 4 Initiative
Requirement: 5+ Insight Statements in Spoken
Effect: Draw 1 card per Insight Statement (max 6)
Verisimilitude: Mastery means leveraging all gathered information
```

#### Master (Depth 7-8) - Unlocked at Momentum 18+

**"Undeniable Logic" - Base**
```
Insight, Depth 7, Statement, 6 Initiative
Requirement: None
Effect: Draw 5 cards, +6 Momentum, +3 Initiative
Verisimilitude: Irrefutable arguments from logical foundation
```

**"Complete Understanding" - Signature**
```
Insight, Depth 8, Statement, 7 Initiative
Requirement: 8+ Insight Statements in Spoken
Effect: Draw 8 cards, +8 Momentum
Verisimilitude: Total comprehension achieved through exhaustive analysis
```

---

### RAPPORT CARDS - Empathetic Connection → Emotional Breakthrough

#### Foundation (Depth 1-2) - Always Available

**"Active Listening" - Base**
```
Rapport, Depth 2, Statement, 0 Initiative
Requirement: None
Effect: Reduce Cadence by 1, +1 Momentum
Verisimilitude: Basic empathy begins building connection
```

**"Show Understanding" - Base**
```
Rapport, Depth 1, Echo, 0 Initiative
Requirement: None
Effect: Reduce Cadence by 1
Verisimilitude: Repeatable empathetic response
```

**"Gentle Encouragement" - Base**
```
Rapport, Depth 1, Statement, 0 Initiative
Requirement: None
Effect: +1 Initiative, +1 Momentum
Verisimilitude: Encouragement creates openings
```

**"Empathetic Response" - Base**
```
Rapport, Depth 2, Echo, 0 Initiative
Requirement: None
Effect: Reduce Cadence by 1, +1 Initiative
Verisimilitude: Sustained empathy technique
```

**"Deep Listening" - Signature**
```
Rapport, Depth 2, Statement, 0 Initiative
Requirement: 2+ Rapport Statements in Spoken
Effect: Reduce Cadence by 2, +1 Momentum
Verisimilitude: Profound listening emerges from practiced empathy
```

**"Sustained Empathy" - Signature**
```
Rapport, Depth 1, Echo, 0 Initiative
Requirement: 2+ Rapport Statements in Spoken
Effect: Reduce Cadence by 1, +1 Initiative
Verisimilitude: Empathy deepens with relationship foundation
```

#### Standard (Depth 3-4) - Unlocked at Momentum 6+

**"Validate Feelings" - Base**
```
Rapport, Depth 4, Statement, 3 Initiative
Requirement: None
Effect: Reduce Cadence by 2, +3 Momentum
Verisimilitude: Validation addresses emotional needs
```

**"Find Common Ground" - Base**
```
Rapport, Depth 3, Statement, 2 Initiative
Requirement: None
Effect: Reduce Cadence by 1, +2 Momentum, +1 Initiative
Verisimilitude: Shared understanding builds naturally
```

**"Reflect Emotions" - Base**
```
Rapport, Depth 3, Echo, 2 Initiative
Requirement: None
Effect: Reduce Cadence by 1 per 3 total Statements (max -3)
Verisimilitude: Reflection grows powerful with conversation depth
```

**"Empathetic Insight" - Base**
```
Rapport, Depth 4, Echo, 3 Initiative
Requirement: None
Effect: Reduce Cadence by 2
Verisimilitude: Understanding emotional dynamics
```

**"Profound Validation" - Signature**
```
Rapport, Depth 4, Statement, 3 Initiative
Requirement: 3+ Rapport Statements in Spoken
Effect: Reduce Cadence by 3, +4 Momentum
Verisimilitude: Deep validation from established trust
```

**"Emotional Resonance" - Signature**
```
Rapport, Depth 3, Echo, 2 Initiative
Requirement: 3+ Rapport Statements in Spoken
Effect: Reduce Cadence by 1 per 2 Rapport Statements (max -4)
Verisimilitude: Perfect attunement to emotional state
```

#### Advanced (Depth 5-6) - Unlocked at Momentum 12+

**"Deep Understanding" - Base**
```
Rapport, Depth 6, Statement, 5 Initiative
Requirement: None
Effect: Reduce Cadence by 3, +5 Momentum
Verisimilitude: Profound understanding transcends words
```

**"Emotional Support" - Base**
```
Rapport, Depth 5, Statement, 4 Initiative
Requirement: None
Effect: Reduce Cadence by 2, +4 Momentum, -1 Doubt per 2 current Doubt
Verisimilitude: Support addresses both emotions and concerns
```

**"Resonate" - Base**
```
Rapport, Depth 5, Echo, 4 Initiative
Requirement: None
Effect: Set Cadence to 0
Verisimilitude: Perfect balance achieved
```

**"Perfect Empathy" - Signature**
```
Rapport, Depth 6, Statement, 5 Initiative
Requirement: 5+ Rapport Statements in Spoken
Effect: Reduce Cadence by 5, +6 Momentum, +3 Initiative
Verisimilitude: Perfect empathy from sustained relational work
```

**"Emotional Harmony" - Signature**
```
Rapport, Depth 5, Echo, 4 Initiative
Requirement: 5+ Rapport Statements in Spoken
Effect: Set Cadence to -3
Verisimilitude: Deep harmony from trust foundation
```

#### Master (Depth 7-8) - Unlocked at Momentum 18+

**"Emotional Breakthrough" - Base**
```
Rapport, Depth 8, Statement, 7 Initiative
Requirement: None
Effect: Set Cadence to -5, +8 Momentum
Verisimilitude: Transformative emotional moment
```

**"Transcendent Understanding" - Signature**
```
Rapport, Depth 8, Statement, 7 Initiative
Requirement: 8+ Rapport Statements in Spoken
Effect: Set Cadence to -5, +10 Momentum, +4 Initiative
Verisimilitude: Ultimate empathetic connection from complete trust
```

---

### AUTHORITY CARDS - Positioning → Decisive Command

#### Foundation (Depth 1-2) - Always Available

**"Assert Position" - Base**
```
Authority, Depth 2, Statement, 1 Initiative
Requirement: None
Effect: +2 Momentum, +1 Doubt
Verisimilitude: Basic assertion establishes presence
```

**"State Firmly" - Base**
```
Authority, Depth 1, Echo, 0 Initiative
Requirement: None
Effect: +2 Momentum, +1 Doubt
Verisimilitude: Repeatable firm statement
```

**"Challenge" - Base**
```
Authority, Depth 1, Statement, 0 Initiative
Requirement: None
Effect: +1 Momentum, +1 Initiative
Verisimilitude: Challenges create opportunities
```

**"Direct Statement" - Base**
```
Authority, Depth 2, Echo, 1 Initiative
Requirement: None
Effect: +2 Momentum, +1 Doubt
Verisimilitude: Clear, direct communication
```

**"Commanding Presence" - Signature**
```
Authority, Depth 2, Statement, 1 Initiative
Requirement: 2+ Authority Statements in Spoken
Effect: +3 Momentum, +1 Doubt, +1 Initiative
Verisimilitude: Authority builds on established positioning
```

**"Forceful Rhetoric" - Signature**
```
Authority, Depth 1, Echo, 0 Initiative
Requirement: 2+ Authority Statements in Spoken
Effect: +3 Momentum, +1 Doubt
Verisimilitude: Rhetoric strengthens with confidence
```

#### Standard (Depth 3-4) - Unlocked at Momentum 6+

**"Direct Demand" - Base**
```
Authority, Depth 4, Statement, 4 Initiative
Requirement: None
Effect: +5 Momentum, +2 Doubt
Verisimilitude: Direct demands drive progress
```

**"Pressure Point" - Base**
```
Authority, Depth 3, Statement, 3 Initiative
Requirement: None
Effect: +4 Momentum, +1 Doubt, +1 Initiative
Verisimilitude: Identifying leverage points
```

**"Escalate Tension" - Base**
```
Authority, Depth 3, Echo, 3 Initiative
Requirement: None
Effect: +1 Momentum per positive Cadence point (max +5), +2 Doubt
Verisimilitude: Leveraging conversational dominance
```

**"Authoritative Statement" - Base**
```
Authority, Depth 4, Echo, 4 Initiative
Requirement: None
Effect: +5 Momentum, +2 Doubt
Verisimilitude: Power through declaration
```

**"Overwhelming Demand" - Signature**
```
Authority, Depth 4, Statement, 4 Initiative
Requirement: 3+ Authority Statements in Spoken
Effect: +7 Momentum, +2 Doubt, +2 Initiative
Verisimilitude: Commands backed by established authority
```

**"Calculated Pressure" - Signature**
```
Authority, Depth 3, Echo, 3 Initiative
Requirement: 3+ Authority Statements in Spoken
Effect: +1 Momentum per Authority Statement (max +5), +2 Doubt
Verisimilitude: Leveraging accumulated authority
```

#### Advanced (Depth 5-6) - Unlocked at Momentum 12+

**"Compelling Argument" - Base**
```
Authority, Depth 6, Statement, 6 Initiative
Requirement: None
Effect: +8 Momentum, +3 Doubt
Verisimilitude: Powerful arguments compel action
```

**"Overwhelming Presence" - Base**
```
Authority, Depth 5, Statement, 5 Initiative
Requirement: None
Effect: +7 Momentum, +2 Doubt, +2 Initiative
Verisimilitude: Presence dominates conversation
```

**"Dominate" - Base**
```
Authority, Depth 5, Echo, 5 Initiative
Requirement: None
Effect: +1 Momentum per 2 total Statements (max +6), +3 Doubt
Verisimilitude: Leveraging entire conversation
```

**"Unquestionable Authority" - Signature**
```
Authority, Depth 6, Statement, 6 Initiative
Requirement: 5+ Authority Statements in Spoken
Effect: +10 Momentum, +3 Doubt, +3 Initiative
Verisimilitude: Authority so established it cannot be challenged
```

**"Total Domination" - Signature**
```
Authority, Depth 5, Echo, 5 Initiative
Requirement: 5+ Authority Statements in Spoken
Effect: +2 Momentum per Authority Statement (max +10), +3 Doubt
Verisimilitude: Complete conversational control
```

#### Master (Depth 7-8) - Unlocked at Momentum 18+

**"Decisive Command" - Base**
```
Authority, Depth 8, Statement, 8 Initiative
Requirement: None
Effect: +12 Momentum, +4 Doubt
Verisimilitude: Commands that compel immediate action
```

**"Absolute Authority" - Signature**
```
Authority, Depth 8, Statement, 8 Initiative
Requirement: 8+ Authority Statements in Spoken
Effect: +15 Momentum, +4 Doubt, +4 Initiative
Verisimilitude: Authority so overwhelming it ends debates
```

---

### COMMERCE CARDS - Risk Management → Sealed Agreement

#### Foundation (Depth 1-2) - Always Available

**"Address Concern" - Base**
```
Commerce, Depth 2, Statement, 1 Initiative
Requirement: None
Effect: -1 Doubt, +1 Momentum
Verisimilitude: Basic risk mitigation
```

**"Reassure" - Base**
```
Commerce, Depth 1, Echo, 0 Initiative
Requirement: None
Effect: -1 Doubt
Verisimilitude: Repeatable reassurance technique
```

**"Propose Alternative" - Base**
```
Commerce, Depth 1, Statement, 0 Initiative
Requirement: None
Effect: -1 Doubt, +1 Initiative
Verisimilitude: Options reduce tension
```

**"Mitigate Risk" - Base**
```
Commerce, Depth 2, Echo, 1 Initiative
Requirement: None
Effect: -1 Doubt, +1 Initiative
Verisimilitude: Ongoing risk management
```

**"Thorough Reassurance" - Signature**
```
Commerce, Depth 2, Statement, 1 Initiative
Requirement: 2+ Commerce Statements in Spoken
Effect: -2 Doubt, +1 Momentum
Verisimilitude: Deep reassurance from negotiation foundation
```

**"Strategic Comfort" - Signature**
```
Commerce, Depth 1, Echo, 0 Initiative
Requirement: 2+ Commerce Statements in Spoken
Effect: -1 Doubt, +1 Initiative
Verisimilitude: Refined comfort techniques
```

#### Standard (Depth 3-4) - Unlocked at Momentum 6+

**"Find Middle Ground" - Base**
```
Commerce, Depth 4, Statement, 4 Initiative
Requirement: None
Effect: -2 Doubt, +3 Momentum, Consume 2 Momentum
Verisimilitude: Compromise requires giving ground
```

**"Calculate Risk" - Base**
```
Commerce, Depth 3, Statement, 3 Initiative
Requirement: None
Effect: -2 Doubt, +2 Momentum
Verisimilitude: Understanding risks reduces them
```

**"Trade Concession" - Base**
```
Commerce, Depth 3, Echo, 3 Initiative
Requirement: None
Effect: Consume X Momentum: -X Doubt (max 3)
Verisimilitude: Trading progress for safety
```

**"Risk Analysis" - Base**
```
Commerce, Depth 4, Echo, 4 Initiative
Requirement: None
Effect: -2 Doubt, +1 Initiative
Verisimilitude: Systematic risk assessment
```

**"Expert Negotiation" - Signature**
```
Commerce, Depth 4, Statement, 4 Initiative
Requirement: 3+ Commerce Statements in Spoken
Effect: -3 Doubt, +4 Momentum, Consume 2 Momentum
Verisimilitude: Masterful compromise from negotiation expertise
```

**"Strategic Exchange" - Signature**
```
Commerce, Depth 3, Echo, 3 Initiative
Requirement: 3+ Commerce Statements in Spoken
Effect: Consume X Momentum: -(X+1) Doubt (max 4)
Verisimilitude: Efficient trading from experience
```

#### Advanced (Depth 5-6) - Unlocked at Momentum 12+

**"Propose Terms" - Base**
```
Commerce, Depth 6, Statement, 6 Initiative
Requirement: None
Effect: -4 Doubt, +5 Momentum, Consume 3 Momentum
Verisimilitude: Formal terms require foundation
```

**"Strategic Concession" - Base**
```
Commerce, Depth 5, Statement, 5 Initiative
Requirement: None
Effect: -3 Doubt, +4 Momentum, +2 Initiative, Consume 2 Momentum
Verisimilitude: Strategic giving to gain
```

**"Mitigate Crisis" - Base**
```
Commerce, Depth 5, Echo, 5 Initiative
Requirement: None
Effect: Reduce Doubt by half (round down)
Verisimilitude: Crisis management expertise
```

**"Perfect Terms" - Signature**
```
Commerce, Depth 6, Statement, 6 Initiative
Requirement: 5+ Commerce Statements in Spoken
Effect: -5 Doubt, +6 Momentum, +3 Initiative, Consume 3 Momentum
Verisimilitude: Ideal terms from comprehensive negotiation
```

**"Master Negotiator" - Signature**
```
Commerce, Depth 5, Echo, 5 Initiative
Requirement: 5+ Commerce Statements in Spoken
Effect: -1 Doubt per Commerce Statement (max -6)
Verisimilitude: Every negotiation builds expertise
```

#### Master (Depth 7-8) - Unlocked at Momentum 18+

**"Seal Agreement" - Base**
```
Commerce, Depth 8, Statement, 8 Initiative
Requirement: None
Effect: Set Doubt to 0, +8 Momentum, Consume 4 Momentum
Verisimilitude: Final agreement removes all concerns
```

**"Perfect Agreement" - Signature**
```
Commerce, Depth 8, Statement, 8 Initiative
Requirement: 8+ Commerce Statements in Spoken
Effect: Set Doubt to 0, +10 Momentum, +4 Initiative, Consume 4 Momentum
Verisimilitude: Flawless agreement from negotiation mastery
```

---

### CUNNING CARDS - Tactical Setup → Springing the Trap

#### Foundation (Depth 1-2) - Always Available

**"Subtle Maneuver" - Base**
```
Cunning, Depth 2, Statement, 0 Initiative
Requirement: None
Effect: +2 Initiative, +1 Momentum
Verisimilitude: Basic tactical positioning
```

**"Feint" - Base**
```
Cunning, Depth 1, Echo, 0 Initiative
Requirement: None
Effect: +2 Initiative
Verisimilitude: Repeatable tactical tool
```

**"Bait" - Base**
```
Cunning, Depth 1, Statement, 0 Initiative
Requirement: None
Effect: +1 Initiative, +1 Momentum
Verisimilitude: Creating tactical openings
```

**"Quick Maneuver" - Base**
```
Cunning, Depth 2, Echo, 0 Initiative
Requirement: None
Effect: +2 Initiative
Verisimilitude: Rapid tactical adjustment
```

**"Calculated Setup" - Signature**
```
Cunning, Depth 2, Statement, 0 Initiative
Requirement: 2+ Cunning Statements in Spoken
Effect: +3 Initiative, +1 Momentum
Verisimilitude: Setup improves with tactical foundation
```

**"Refined Technique" - Signature**
```
Cunning, Depth 1, Echo, 0 Initiative
Requirement: 2+ Cunning Statements in Spoken
Effect: +2 Initiative, +1 Initiative if Doubt ≥ 5
Verisimilitude: Exploiting danger through practice
```

#### Standard (Depth 3-4) - Unlocked at Momentum 6+

**"Create Opening" - Base**
```
Cunning, Depth 4, Statement, 2 Initiative
Requirement: None
Effect: +4 Initiative, +2 Momentum
Verisimilitude: Manufactured opportunities
```

**"Position Advantage" - Base**
```
Cunning, Depth 3, Statement, 2 Initiative
Requirement: None
Effect: +3 Initiative, +2 Momentum, Draw 1 card
Verisimilitude: Positioning creates options
```

**"Tactical Leverage" - Base**
```
Cunning, Depth 3, Echo, 2 Initiative
Requirement: None
Effect: +1 Initiative per 2 cards in Mind (max +5)
Verisimilitude: Options create opportunities
```

**"Opportunistic Play" - Base**
```
Cunning, Depth 4, Echo, 2 Initiative
Requirement: None
Effect: +4 Initiative
Verisimilitude: Seizing moments
```

**"Masterful Setup" - Signature**
```
Cunning, Depth 4, Statement, 2 Initiative
Requirement: 3+ Cunning Statements in Spoken
Effect: +5 Initiative, +3 Momentum, Draw 1 card
Verisimilitude: Superior positioning from tactical mastery
```

**"Perfect Leverage" - Signature**
```
Cunning, Depth 3, Echo, 2 Initiative
Requirement: 3+ Cunning Statements in Spoken
Effect: +1 Initiative per Cunning Statement (max +5)
Verisimilitude: Every setup creates more opportunities
```

#### Advanced (Depth 5-6) - Unlocked at Momentum 12+

**"Exploit Opening" - Base**
```
Cunning, Depth 6, Statement, 4 Initiative
Requirement: None
Effect: +6 Initiative, +5 Momentum
Verisimilitude: Capitalizing on created opportunities
```

**"Perfect Timing" - Base**
```
Cunning, Depth 5, Statement, 3 Initiative
Requirement: None
Effect: +5 Initiative, +4 Momentum, Draw 2 cards
Verisimilitude: Timing multiplies effectiveness
```

**"Convert Tension" - Base**
```
Cunning, Depth 5, Echo, 4 Initiative
Requirement: None
Effect: +1 Initiative per Doubt (max +8)
Verisimilitude: Danger becomes opportunity
```

**"Total Exploitation" - Signature**
```
Cunning, Depth 6, Statement, 4 Initiative
Requirement: 5+ Cunning Statements in Spoken
Effect: +8 Initiative, +6 Momentum, Draw 2 cards
Verisimilitude: Complete setup exploitation
```

**"Tactical Mastery" - Signature**
```
Cunning, Depth 5, Echo, 4 Initiative
Requirement: 5+ Cunning Statements in Spoken
Effect: +2 Initiative per Cunning Statement (max +10)
Verisimilitude: Every maneuver multiplies opportunities
```

#### Master (Depth 7-8) - Unlocked at Momentum 18+

**"Spring the Trap" - Base**
```
Cunning, Depth 8, Statement, 6 Initiative
Requirement: None
Effect: +10 Initiative, +8 Momentum
Verisimilitude: The payoff from extensive setup
```

**"Overwhelming Advantage" - Signature**
```
Cunning, Depth 8, Statement, 6 Initiative
Requirement: 8+ Cunning Statements in Spoken
Effect: +12 Initiative, +10 Momentum, Draw 3 cards
Verisimilitude: Absolute tactical superiority from complete setup
```

---

## Implementation Guidelines

### JSON Structure

**Base Card Example:**
```json
{
  "id": "insight_identify_pattern",
  "title": "Identify Pattern",
  "dialogueText": "I'm seeing a pattern here...",
  "boundStat": "Insight",
  "depth": 4,
  "persistence": "Statement",
  "initiativeCost": 3,
  "requiredStat": null,
  "requiredStatements": 0,
  "effectVariant": "Base",
  "category": "Realization"
}
```

**Signature Card Example:**
```json
{
  "id": "insight_complex_analysis",
  "title": "Complex Analysis",
  "dialogueText": "Based on everything I've observed...",
  "boundStat": "Insight",
  "depth": 4,
  "persistence": "Statement",
  "initiativeCost": 3,
  "requiredStat": "Insight",
  "requiredStatements": 3,
  "effectVariant": "Enhanced",
  "category": "Realization"
}
```

### UI Display Guidelines

**Show Statement Progress:**
```
Conversation Resources:
Initiative: 5/10
Momentum: 8
Doubt: 3/10
Cadence: -2

Statement Counts:
Insight:   ████░ (5)
Rapport:   ██░░░ (2)
Authority: █░░░░ (1)
Commerce:  ░░░░░ (0)
Cunning:   ███░░ (3)
```

**Card Display with Requirements:**
```
[Base Card]
Identify Pattern
Initiative: 3
Draw 3 cards, +2 Momentum

[Signature Card - LOCKED]
Complex Analysis
Initiative: 3
Requires: 3 Insight Statements (Have: 2)
Draw 4 cards, +3 Momentum
```

### Parser Validation Rules

```csharp
// Base cards must have no requirements
if (card.EffectVariant == "Base" && card.RequiredStatements > 0)
    throw new InvalidDataException($"Base card {card.Id} cannot have Statement requirements");

// Signature cards must have requirements
if (card.EffectVariant == "Signature" && card.RequiredStatements == 0)
    throw new InvalidDataException($"Signature card {card.Id} must have Statement requirements");

// Statement requirements must match appropriate thresholds
var validThresholds = new[] { 2, 3, 4, 5, 8 };
if (card.RequiredStatements > 0 && !validThresholds.Contains(card.RequiredStatements))
    throw new InvalidDataException($"Card {card.Id} has invalid requirement threshold: {card.RequiredStatements}");
```

---

## Strategic Implications

### Build Archetypes

**The Generalist (Multiple Stats)**
- Plays diverse cards across 3-4 stats
- Uses base versions of all cards
- Flexible, adaptive gameplay
- Cannot access signature variants
- Good for learning and exploration

**The Moderate Specialist (70% One Stat)**
- Focuses primarily on one stat
- Reaches 3-5 Statement thresholds
- Accesses Standard/Advanced signature variants
- Maintains some tactical flexibility
- Balanced optimization vs versatility

**The Pure Specialist (90% One Stat)**
- Nearly exclusively plays one stat
- Reaches 8+ Statement threshold
- Accesses all signature variants including Master
- Maximum power in specialized domain
- Requires strong understanding of resource loops

### Deck Building Guidelines

**Per Stat in Conversation Type:**
- 60% Base cards (accessible to all)
- 20% Signature variants (specialist rewards)
- 20% situational cards (state-dependent effects)

**Example: Insight-Heavy Investigation Deck**
```
12 Insight cards total:
- 8 base cards (usable by everyone)
- 2 signature variants (3-5 Statement requirement)
- 2 master signatures (8+ Statement requirement)

Distribution by Tier:
- Foundation: 4 base, 1 signature (5 total)
- Standard: 3 base, 1 signature (4 total)
- Advanced: 1 base, 1 signature (2 total)
- Master: 0 base, 1 signature (1 total)
```

### Progression Feel

**Early Game (Momentum 0-6):**
- Playing Foundation cards
- Building Statement counts organically
- Signature variants appear but are locked
- Tier 2 unlocks new base options

**Mid Game (Momentum 6-12):**
- Standard cards accessible
- Specialists hit 3-5 Statement thresholds
- Signature variants become playable
- Meaningful optimization choices appear

**Late Game (Momentum 12+):**
- Advanced cards accessible
- Pure specialists hit 8+ Statement threshold
- Master signature variants available
- Powerful climactic plays possible

### Design Philosophy Benefits

✓ **No False Progression**: Base cards always work when tier unlocks
✓ **Optional Optimization**: Signature variants reward focus without blocking access
✓ **Clear Trade-offs**: Specialization vs flexibility is meaningful choice
✓ **Achievable Goals**: Statement thresholds actually reachable in play
✓ **Verisimilitude**: Powerful moves require conversational foundation

---

## Summary

This hybrid system provides:

1. **Primary Progression** through momentum-based tier unlocking (works for everyone)
2. **Secondary Optimization** through Statement signature variants (rewards specialists)
3. **Mathematical Validity** (requirements achievable within conversation length)
4. **Strategic Depth** (meaningful choice between specialization and flexibility)
5. **Implementation Simplicity** (80% base cards, 20% signature variants)

The system achieves the original goals while avoiding the mathematical impossibilities and perverse incentives of a Statement-only progression system. Players can ignore Statements entirely and still have full access to powerful cards, or they can optimize toward signature variants for maximum power in their specialized domain.