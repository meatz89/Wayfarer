# Complete Card Library Specification
## Momentum Gates Access, Statements Gate Power

This document specifies the complete card library using a hybrid system where **momentum-based tier unlocking** provides the primary progression, while **Statement requirements** gate powerful signature variants for specialists.

---

## Table of Contents

1. [Core Design Philosophy](#core-design-philosophy)
2. [The Two-Tiered System](#the-two-tiered-system)
3. [Mathematical Foundation](#mathematical-foundation)
4. [Effect Formula System](#effect-formula-system)
5. [Card Structure](#card-structure)
6. [Complete Card Library](#complete-card-library)
7. [Implementation Guidelines](#implementation-guidelines)
8. [Strategic Implications](#strategic-implications)

---

## Core Design Philosophy

### **CRITICAL REFINEMENT: Specialist with Universal Access**

**Resource Generation Model**: Each stat SPECIALIZES in one resource (2-3x efficiency) but can ACCESS universal resources (Momentum/Initiative) at standard rates.

**Why This Change:**
- **Original Problem**: Hard 1:1 mapping (Authority-only generates Momentum) created impossible deck compositions
- **Mechanical Issue**: "Zero Authority" decks can't reach goals without Momentum generation
- **Solution**: All stats generate SOME Momentum; Authority just does it 2-3x more efficiently

**Stat Specialization Rates:**

| Stat | Specialist Resource | Universal Access | Trade-offs |
|------|---------------------|------------------|------------|
| **Insight** | Cards (2-6 draw) | Momentum (+1-3), Initiative (+1-2) | Information-focused |
| **Rapport** | Cadence (-1 to -3) | Momentum (+1-3), Initiative (+1-3) | Sustainable, lower momentum |
| **Authority** | Momentum (+2-12) | Initiative (+1-2) | 2-3x faster momentum but generates Doubt |
| **Commerce** | Doubt (-1 to -6) | Momentum (+1-3) | Often consumes Momentum to reduce Doubt |
| **Cunning** | Initiative (+2-6) | Momentum (+1-3) | Enables long action chains |

**Effect Pattern by Depth:**
- **Foundation (1-2)**: Specialist 2x, Universal 1x
- **Standard (3-4)**: Specialist 2.5x, Universal 1.5x
- **Advanced (5-6)**: Specialist 3x, Universal 2x
- **Master (7-8)**: Specialist 3-4x, Universal 2-3x

**Example:**
```
Authority Depth 1: +2 Momentum, +1 Doubt (specialist)
Insight Depth 1: Draw 2 cards, +1 Momentum (specialist + universal)
Cunning Depth 4: +4 Initiative, +2 Momentum, Draw 1 card (specialist + universals + secondary)
```

### The Hybrid Approach

**PRIMARY: Momentum-Based Tier Unlocking**
- Momentum thresholds (6/12/18) unlock depth tiers
- Stat specialization extends depth access for specialized cards
- Provides natural progression through conversations
- Works for all build types (generalist and specialist)

**SECONDARY: Statement Requirements for Power Variants**
- Foundation tier (depths 1-2): 100% base cards (no requirements)
- Standard+ tiers: ~75% base cards, ~25% signature variants
- Signature variants require 3-8 Statement counts (starts at Standard tier)
- Provides 50-80% more power for specialists who focus one stat

### Design Goals Achieved

✓ **Sustainable Economy**: Echo cards provide efficiency without blocking progression
✓ **Clear Progression**: Momentum thresholds unlock new options predictably
✓ **Specialist Rewards**: Statement variants AND resource specialization provide meaningful optimization
✓ **Verisimilitude**: All conversation approaches advance progress; commanding just does it faster/riskier
✓ **Mathematical Validity**: Requirements achievable within conversation length
✓ **Universal Foundation**: Everyone starts with same tools, specialization rewards come later
✓ **Playable Decks**: Every composition can generate Momentum to reach goals

---

## The Two-Tiered System

### Primary: Momentum Tier Unlocks

| Momentum | Tier Unlocked | Depths Available | Strategic Milestone |
|----------|---------------|------------------|---------------------|
| 0 | Tier 1 | 1-2 | Foundation cards only |
| 6+ | Tier 2 | 1-4 | Standard cards unlock |
| 12+ | Tier 3 | 1-6 | Advanced cards unlock (Enhanced Goal) |
| 18+ | Tier 4 | 1-8 | Master cards unlock |

**Stat Specialization Depth Extension:**

Stat specialization extends the maximum accessible depth for cards of that stat, regardless of tier limits.

```
Stat Level 3-5: Can access +1 depth for that stat's cards
Stat Level 6-8: Can access +2 depth for that stat's cards
Stat Level 9-10: Can access +3 depth for that stat's cards

Example: Momentum 8 (Tier 2, normally depths 1-4), Insight 7
- All cards: Depths 1-4 (Tier 2 baseline)
- Insight cards ONLY: Depths 1-6 (tier baseline + 2 stat bonus)
- Can access Insight Advanced cards early via specialization
```

### Secondary: Statement Signature Variants

**Foundation Tier (Depths 1-2): Universal Starting Point**
- 100% base cards with NO Statement requirements
- Everyone starts equal regardless of build
- Establishes the conversational foundation

**Standard+ Tiers (Depths 3+): Specialization Rewards**

**Base Cards** (~75% of Standard+ library):
- No Statement requirements
- Accessible immediately when tier unlocks
- Provide solid effects for all players

**Signature Variants** (~25% of Standard+ library):
- Require 3-8 Statement cards of matching stat type
- Provide 50-80% more power than base equivalents
- Only appear in Standard/Advanced/Master tiers (depths 3+)
- Reward specialists who focus one stat

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
- Cannot access signature variants (minimum 3 required)
- Uses base versions of all cards
- Flexible, adaptive gameplay

**Moderate Specialist** (70% one stat, 30% support stats):
- 9 Statements: 6-7 in primary, 2-3 in support
- Can access Standard signature variants (3-4 requirement)
- Can access some Advanced signature variants (5 requirement)
- Balanced optimization vs versatility

**Pure Specialist** (90% one stat):
- 9 Statements: 8+ in primary stat
- Can access ALL signature variants including Master (8+ requirement)
- Maximum power in specialized domain
- May struggle with resource diversity

### Requirement Thresholds by Tier

| Tier | Base Cards | Signature Requirement | Achievability |
|------|------------|----------------------|---------------|
| Foundation (1-2) | No requirement | N/A (no signatures) | Universal |
| Standard (3-4) | No requirement | 3-4 Statements | Moderate specialists |
| Advanced (5-6) | No requirement | 5 Statements | Focused specialists |
| Master (7-8) | No requirement | 8 Statements | Pure specialists only |

---

## Effect Formula System

All card effects use deterministic formulas. No randomness, no player choices, no percentage-based scaling.

### Formula Types

**1. Fixed (Static Value)**
```
Effect: +4 Initiative
Formula: BaseValue = 4
```

**2. Linear Scaling (State-Based)**
```
Effect: +1 Momentum per Doubt (max 8)
Formula: ScalingSource = Doubt, Multiplier = 1.0, Max = 8
Result = min(Doubt × 1.0, 8)
```

**3. Trading (Resource Consumption)**
```
Effect: Consume 2 Momentum: -3 Doubt
Formula: ConsumeResource = Momentum, ConsumeAmount = 2, Effect = -3 Doubt
Precondition: Momentum ≥ 2
```

**4. Setting (Absolute Value)**
```
Effect: Set Cadence to 0
Formula: SetValue = 0
Result: Cadence becomes 0 regardless of current value
```

**5. Compound (Multiple Effects)**
```
Effect: Draw 2 cards, +1 Momentum
Formula: [Fixed(Cards, 2), Fixed(Momentum, 1)]
Both effects execute in sequence
```

### Scaling Sources

Linear scaling formulas can reference these game state values:

- **TotalStatements**: Count of all Statement cards played (any stat)
- **StatStatements**: Count of Statement cards played for a specific stat (e.g., Insight only)
- **Doubt**: Current Doubt value (0-10)
- **PositiveCadence**: max(0, Cadence) - only positive values
- **NegativeCadence**: abs(min(0, Cadence)) - only negative values
- **MindCards**: Number of cards currently in hand
- **Momentum**: Current Momentum value

**All scaling is LINEAR**: Multiply source by multiplier, apply max cap.

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

**JSON Structure** (Categorical Properties Only):
```json
{
  "id": "stat_card_name",
  "title": "Card Name",
  "dialogueText": "What the character says...",
  "type": "Conversation",
  "boundStat": "StatName",
  "depth": 1-8,
  "persistence": "Echo|Statement",
  "initiativeCost": 0-8,
  "requiredStat": null or "StatName",
  "requiredStatements": 0 or 3-8,
  "effectVariant": "Base|Signature",
  "personalityTypes": ["ALL"]
}
```

**Effect Determination**: Effects are NOT in JSON. The parser looks up effects from `CardEffectCatalog` based on `boundStat` + `depth` + `effectVariant`.

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

**Master (Depths 7-8)**: 33% Echo
- Purpose: Climactic moments
- Mostly conversation-defining declarations

---

## Complete Card Library

### INSIGHT CARDS - Information Gathering → Analytical Conclusions

#### Foundation (Depth 1-2) - Universal Base Cards Only

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

**"Notice Detail" - Base**
```
Insight, Depth 2, Statement, 0 Initiative
Requirement: None
Effect: Draw 2 cards, +1 Momentum
Verisimilitude: Basic observation is the foundation of analysis
```

#### Standard (Depth 3-4) - Signatures Begin

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
Effect: Draw 1 card per 2 TotalStatements (max 3)
Verisimilitude: More conversation history reveals more connections
```

**"Cross-Reference" - Base**
```
Insight, Depth 3, Statement, 2 Initiative
Requirement: None
Effect: Draw 2 cards, +1 Momentum, +1 Initiative
Verisimilitude: Referencing prior information builds understanding
```

**"Complex Analysis" - Signature**
```
Insight, Depth 4, Statement, 3 Initiative
Requirement: 3+ Insight Statements
Effect: Draw 4 cards, +3 Momentum
Power Increase: +33% cards, +50% momentum
Verisimilitude: Deep analysis builds on substantial prior observations
```

**"Pattern Synthesis" - Signature**
```
Insight, Depth 3, Echo, 2 Initiative
Requirement: 3+ Insight Statements
Effect: Draw 1 card per InsightStatement (max 4)
Power Increase: +33% max draw
Verisimilitude: Synthesizing all prior analytical work
```

#### Advanced (Depth 5-6) - Specialist Territory

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
Effect: Draw 1 card per 2 TotalStatements (max 5)
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
Requirement: 5+ Insight Statements
Effect: Draw 6 cards, +6 Momentum
Power Increase: +50% cards, +20% momentum
Verisimilitude: Masterful deduction from comprehensive foundation
```

**"Analytical Mastery" - Signature**
```
Insight, Depth 5, Echo, 4 Initiative
Requirement: 5+ Insight Statements
Effect: Draw 1 card per InsightStatement (max 6)
Power Increase: +20% max draw
Verisimilitude: Mastery means leveraging all gathered information
```

#### Master (Depth 7-8) - Pure Specialist Only

**"Undeniable Logic" - Base**
```
Insight, Depth 8, Statement, 7 Initiative
Requirement: None
Effect: Draw 5 cards, +7 Momentum, +3 Initiative
Verisimilitude: Irrefutable arguments from logical foundation
```

**"Complete Understanding" - Signature**
```
Insight, Depth 8, Statement, 7 Initiative
Requirement: 8+ Insight Statements
Effect: Draw 8 cards, +10 Momentum, +4 Initiative
Power Increase: +60% cards, +43% momentum, +33% initiative
Verisimilitude: Total comprehension achieved through exhaustive analysis
```

---

### RAPPORT CARDS - Empathetic Connection → Emotional Breakthrough

#### Foundation (Depth 1-2) - Universal Base Cards Only

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

**"Active Listening" - Base**
```
Rapport, Depth 2, Statement, 0 Initiative
Requirement: None
Effect: Reduce Cadence by 1, +1 Momentum
Verisimilitude: Basic empathy begins building connection
```

#### Standard (Depth 3-4) - Signatures Begin

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
Effect: Reduce Cadence by 1 per 3 TotalStatements (max -3)
Verisimilitude: Reflection grows powerful with conversation depth
```

**"Profound Validation" - Signature**
```
Rapport, Depth 4, Statement, 3 Initiative
Requirement: 3+ Rapport Statements
Effect: Reduce Cadence by 3, +4 Momentum
Power Increase: +50% cadence reduction, +33% momentum
Verisimilitude: Deep validation from established trust
```

**"Emotional Resonance" - Signature**
```
Rapport, Depth 3, Echo, 2 Initiative
Requirement: 3+ Rapport Statements
Effect: Reduce Cadence by 1 per 2 RapportStatements (max -4)
Power Increase: +33% max cadence reduction
Verisimilitude: Perfect attunement to emotional state
```

#### Advanced (Depth 5-6) - Specialist Territory

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
Effect: Reduce Cadence by 2, +4 Momentum, Reduce Doubt by 1 per 2 Doubt (round down)
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
Requirement: 5+ Rapport Statements
Effect: Reduce Cadence by 5, +7 Momentum, +3 Initiative
Power Increase: +67% cadence reduction, +40% momentum
Verisimilitude: Perfect empathy from sustained relational work
```

**"Emotional Harmony" - Signature**
```
Rapport, Depth 5, Echo, 4 Initiative
Requirement: 5+ Rapport Statements
Effect: Set Cadence to -3
Power Increase: From neutral to strong negative (better outcome)
Verisimilitude: Deep harmony from trust foundation
```

#### Master (Depth 7-8) - Pure Specialist Only

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
Requirement: 8+ Rapport Statements
Effect: Set Cadence to -5, +11 Momentum, +4 Initiative
Power Increase: +38% momentum, +4 initiative
Verisimilitude: Ultimate empathetic connection from complete trust
```

---

### AUTHORITY CARDS - Positioning → Decisive Command

#### Foundation (Depth 1-2) - Universal Base Cards Only

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

**"Assert Position" - Base**
```
Authority, Depth 2, Statement, 1 Initiative
Requirement: None
Effect: +2 Momentum, +1 Doubt
Verisimilitude: Basic assertion establishes presence
```

#### Standard (Depth 3-4) - Signatures Begin

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
Effect: +1 Momentum per PositiveCadence (max +5), +2 Doubt
Verisimilitude: Leveraging conversational dominance
```

**"Overwhelming Demand" - Signature**
```
Authority, Depth 4, Statement, 4 Initiative
Requirement: 3+ Authority Statements
Effect: +7 Momentum, +2 Doubt, +2 Initiative
Power Increase: +40% momentum, +2 initiative
Verisimilitude: Commands backed by established authority
```

**"Calculated Pressure" - Signature**
```
Authority, Depth 3, Echo, 3 Initiative
Requirement: 3+ Authority Statements
Effect: +1 Momentum per AuthorityStatement (max +5), +2 Doubt
Power Increase: Scales with specialization
Verisimilitude: Leveraging accumulated authority
```

#### Advanced (Depth 5-6) - Specialist Territory

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
Effect: +1 Momentum per 2 TotalStatements (max +6), +3 Doubt
Verisimilitude: Leveraging entire conversation
```

**"Unquestionable Authority" - Signature**
```
Authority, Depth 6, Statement, 6 Initiative
Requirement: 5+ Authority Statements
Effect: +11 Momentum, +3 Doubt, +3 Initiative
Power Increase: +38% momentum, +3 initiative
Verisimilitude: Authority so established it cannot be challenged
```

**"Total Domination" - Signature**
```
Authority, Depth 5, Echo, 5 Initiative
Requirement: 5+ Authority Statements
Effect: +2 Momentum per AuthorityStatement (max +10), +3 Doubt
Power Increase: +67% max momentum
Verisimilitude: Complete conversational control
```

#### Master (Depth 7-8) - Pure Specialist Only

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
Requirement: 8+ Authority Statements
Effect: +16 Momentum, +4 Doubt, +4 Initiative
Power Increase: +33% momentum, +4 initiative
Verisimilitude: Authority so overwhelming it ends debates
```

---

### COMMERCE CARDS - Risk Management → Sealed Agreement

#### Foundation (Depth 1-2) - Universal Base Cards Only

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

**"Address Concern" - Base**
```
Commerce, Depth 2, Statement, 1 Initiative
Requirement: None
Effect: -1 Doubt, +1 Momentum
Verisimilitude: Basic risk mitigation
```

#### Standard (Depth 3-4) - Signatures Begin

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
Effect: Reduce Doubt by 1 per 3 Momentum consumed (max -3, requires 9 Momentum)
Verisimilitude: Trading progress for safety
```

**"Expert Negotiation" - Signature**
```
Commerce, Depth 4, Statement, 4 Initiative
Requirement: 3+ Commerce Statements
Effect: -3 Doubt, +4 Momentum, Consume 2 Momentum
Power Increase: +50% doubt reduction, +33% momentum
Verisimilitude: Masterful compromise from negotiation expertise
```

**"Strategic Exchange" - Signature**
```
Commerce, Depth 3, Echo, 3 Initiative
Requirement: 3+ Commerce Statements
Effect: Reduce Doubt by 1 per 2 Momentum consumed (max -4, requires 8 Momentum)
Power Increase: +33% efficiency, +33% max reduction
Verisimilitude: Efficient trading from experience
```

#### Advanced (Depth 5-6) - Specialist Territory

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
Effect: Reduce Doubt by 1 per 2 Doubt (round down)
Verisimilitude: Crisis management expertise
```

**"Perfect Terms" - Signature**
```
Commerce, Depth 6, Statement, 6 Initiative
Requirement: 5+ Commerce Statements
Effect: -5 Doubt, +7 Momentum, +3 Initiative, Consume 3 Momentum
Power Increase: +25% doubt reduction, +40% momentum, +3 initiative
Verisimilitude: Ideal terms from comprehensive negotiation
```

**"Master Negotiator" - Signature**
```
Commerce, Depth 5, Echo, 5 Initiative
Requirement: 5+ Commerce Statements
Effect: Reduce Doubt by 1 per CommerceStatement (max -6)
Power Increase: Direct scaling with specialization
Verisimilitude: Every negotiation builds expertise
```

#### Master (Depth 7-8) - Pure Specialist Only

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
Requirement: 8+ Commerce Statements
Effect: Set Doubt to 0, +11 Momentum, +4 Initiative, Consume 4 Momentum
Power Increase: +38% momentum, +4 initiative
Verisimilitude: Flawless agreement from negotiation mastery
```

---

### CUNNING CARDS - Tactical Setup → Springing the Trap

#### Foundation (Depth 1-2) - Universal Base Cards Only

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

**"Subtle Maneuver" - Base**
```
Cunning, Depth 2, Statement, 0 Initiative
Requirement: None
Effect: +2 Initiative, +1 Momentum
Verisimilitude: Basic tactical positioning
```

#### Standard (Depth 3-4) - Signatures Begin

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
Effect: +1 Initiative per 2 MindCards (max +5)
Verisimilitude: Options create opportunities
```

**"Masterful Setup" - Signature**
```
Cunning, Depth 4, Statement, 2 Initiative
Requirement: 3+ Cunning Statements
Effect: +5 Initiative, +3 Momentum, Draw 1 card
Power Increase: +25% initiative, +50% momentum, +1 card
Verisimilitude: Superior positioning from tactical mastery
```

**"Perfect Leverage" - Signature**
```
Cunning, Depth 3, Echo, 2 Initiative
Requirement: 3+ Cunning Statements
Effect: +1 Initiative per CunningStatement (max +5)
Power Increase: Direct scaling with specialization
Verisimilitude: Every setup creates more opportunities
```

#### Advanced (Depth 5-6) - Specialist Territory

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
Requirement: 5+ Cunning Statements
Effect: +8 Initiative, +7 Momentum, Draw 2 cards
Power Increase: +33% initiative, +40% momentum
Verisimilitude: Complete setup exploitation
```

**"Tactical Mastery" - Signature**
```
Cunning, Depth 5, Echo, 4 Initiative
Requirement: 5+ Cunning Statements
Effect: +2 Initiative per CunningStatement (max +10)
Power Increase: +25% max initiative
Verisimilitude: Every maneuver multiplies opportunities
```

#### Master (Depth 7-8) - Pure Specialist Only

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
Requirement: 8+ Cunning Statements
Effect: +13 Initiative, +11 Momentum, Draw 3 cards
Power Increase: +30% initiative, +38% momentum, +3 cards
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
  "type": "Conversation",
  "boundStat": "Insight",
  "depth": 4,
  "persistence": "Statement",
  "initiativeCost": 3,
  "category": "Realization",
  "requiredStat": null,
  "requiredStatements": 0,
  "effectVariant": "Base",
  "personalityTypes": ["ALL"]
}
```

**Signature Card Example:**
```json
{
  "id": "insight_complex_analysis",
  "title": "Complex Analysis",
  "dialogueText": "Based on everything I've observed...",
  "type": "Conversation",
  "boundStat": "Insight",
  "depth": 4,
  "persistence": "Statement",
  "initiativeCost": 3,
  "category": "Realization",
  "requiredStat": "Insight",
  "requiredStatements": 3,
  "effectVariant": "Enhanced",
  "personalityTypes": ["ALL"]
}
```

### Effect Formulas in CardEffectCatalog

Each card's effect is derived from CardEffectCatalog based on:
- `boundStat` (Insight/Rapport/Authority/Commerce/Cunning)
- `depth` (1-8)
- `effectVariant` (Base, Enhanced, Scaling_X, etc.)

The catalog returns a `CardEffectFormula` object with:
```csharp
public class CardEffectFormula {
    public EffectFormulaType FormulaType; // Fixed, Scaling, Trading, Setting, Compound
    public ConversationResourceType TargetResource; // Initiative, Momentum, Doubt, Cadence, Cards
    public int BaseValue; // For Fixed type
    public ScalingSourceType? ScalingSource; // For Scaling type
    public decimal ScalingMultiplier; // For Scaling type
    public int? ScalingMax; // For Scaling type
    public List<CardEffectFormula> CompoundEffects; // For Compound type
    // ... other fields
}
```

### UI Display Guidelines

**Statement Counter (Always Visible):**
```
Statement Counts:
Insight:   ████░ (5/8)
Rapport:   ██░░░ (2/8)
Authority: █░░░░ (1/8)
Commerce:  ░░░░░ (0/8)
Cunning:   ███░░ (3/8)
```

**Card Display with Locked Signature:**
```
┌─────────────────────────┐
│ Complex Analysis    [🔒] │  ← Lock icon indicates requirement not met
│ Insight • Depth 4       │
│ 3 Initiative            │
├─────────────────────────┤
│ Requires: 3 Insight     │  ← Red text, grayed out
│ Have: 2 Insight         │
├─────────────────────────┤
│ Draw 4 cards            │
│ +3 Momentum             │
└─────────────────────────┘
```

**Card Display with Available Signature:**
```
┌─────────────────────────┐
│ Complex Analysis    [⭐] │  ← Star icon indicates signature variant
│ Insight • Depth 4       │
│ 3 Initiative            │
├─────────────────────────┤
│ ✓ Signature Available   │  ← Green text
├─────────────────────────┤
│ Draw 4 cards            │
│ +3 Momentum             │
└─────────────────────────┘
```

### Parser Validation Rules

```csharp
// Foundation tier must have NO signature variants
if (card.Depth <= 2 && card.RequiredStatements > 0)
    throw new InvalidDataException($"Foundation card {card.Id} cannot have Statement requirements");

// Signature cards must start at Standard tier (depth 3+)
if (card.RequiredStatements > 0 && card.Depth < 3)
    throw new InvalidDataException($"Signature card {card.Id} must be depth 3 or higher");

// Statement requirements must match valid thresholds
var validThresholds = new[] { 3, 4, 5, 8 };
if (card.RequiredStatements > 0 && !validThresholds.Contains(card.RequiredStatements))
    throw new InvalidDataException($"Card {card.Id} has invalid requirement: {card.RequiredStatements}. Valid: 3, 4, 5, 8");

// RequiredStat must match BoundStat for signature variants
if (card.RequiredStatements > 0 && card.RequiredStat != card.BoundStat)
    throw new InvalidDataException($"Signature card {card.Id} must require statements of its own stat");
```

### Card Distribution Per Stat

**Foundation (Depths 1-2)**: 4 base cards
- 2-3 Echo, 1-2 Statement
- 0 signature variants

**Standard (Depths 3-4)**: 4 cards
- 3 base (accessible to all)
- 1 signature (requires 3-4 Statements)

**Advanced (Depths 5-6)**: 3 cards
- 2 base (accessible to all)
- 1 signature (requires 5 Statements)

**Master (Depths 7-8)**: 1 card
- 0 base (specialist-only tier)
- 1 signature (requires 8 Statements)

**Total per stat: 12 cards** (9 base, 3 signature = 75% base, 25% signature)

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
- Reaches 6-7 Statements in primary stat
- Accesses Standard signature variants (3-4 requirement)
- Maintains some tactical flexibility
- Balanced optimization vs versatility

**The Pure Specialist (90% One Stat)**
- Nearly exclusively plays one stat
- Reaches 8+ Statement threshold
- Accesses ALL signature variants including Master
- Maximum power in specialized domain
- Requires strong understanding of resource loops
- Risk: May struggle with resource diversity

### Deck Building Guidelines

**Per Stat in Conversation Type:**
- 75% Base cards (9 per stat, accessible to all)
- 25% Signature variants (3 per stat, specialist rewards)

**Example: Insight-Heavy Investigation Deck**
```
12 Insight cards total:
- 9 base cards (usable by everyone)
- 3 signature variants (3-8 Statement requirements)

Distribution by Tier:
- Foundation: 4 base, 0 signature (4 total)
- Standard: 3 base, 1 signature (4 total)
- Advanced: 2 base, 1 signature (3 total)
- Master: 0 base, 1 signature (1 total)
```

### Progression Feel

**Early Game (Momentum 0-6):**
- Playing Foundation cards only
- Building Statement counts organically
- All players use same Foundation cards
- Tier 2 unlocks Standard base cards

**Mid Game (Momentum 6-12):**
- Standard cards accessible
- Moderate specialists hit 3-4 Statement thresholds
- Signature variants differentiate specialists from generalists
- Meaningful optimization choices appear

**Late Game (Momentum 12+):**
- Advanced cards accessible
- Pure specialists hit 8+ Statement threshold
- Master signature variants provide dramatic power spikes
- Powerful climactic plays possible

### Draw Pool Management

**Signature Card Visibility:**

When drawing cards, signature variants appear in hand but may be unplayable if requirements aren't met. This is intentional friction that:

- Rewards specialists (they can play more cards)
- Provides visibility (you see what's possible)
- Creates aspirational goals (motivates specialization)
- Punishes indecision (generalists get dead draws)

**UI must clearly distinguish**:
- ✓ Playable cards (green highlight)
- 🔒 Locked signature cards (red/gray, show requirement)
- Base cards are always playable when tier unlocked

### Design Philosophy Benefits

✓ **No False Progression**: Base cards always work when tier unlocks
✓ **Optional Optimization**: Signature variants reward focus without blocking access
✓ **Clear Trade-offs**: Specialization vs flexibility is meaningful choice
✓ **Achievable Goals**: Statement thresholds actually reachable in play
✓ **Verisimilitude**: Powerful moves require conversational foundation (Standard+ only)
✓ **Universal Foundation**: Everyone starts equal, specialization rewards come later
✓ **Honest Power Scaling**: 50-80% more powerful accurately describes signature variants
✓ **Deterministic Effects**: All formulas are linear and predictable

---

## Summary

This hybrid system provides:

1. **Primary Progression** through momentum-based tier unlocking (works for everyone)
2. **Secondary Optimization** through Statement signature variants (rewards specialists)
3. **Mathematical Validity** (requirements achievable within conversation length)
4. **Strategic Depth** (meaningful choice between specialization and flexibility)
5. **Implementation Clarity** (75/25 base/signature split, Foundation is 100% base)
6. **Verisimilitude That Works** (signatures require foundation, starting at Standard tier)
7. **Deterministic Mechanics** (no randomness, no choices, pure linear scaling)

The system achieves the original goals while maintaining honest implementation details. Foundation provides universal starting point, Standard+ tiers introduce specialization rewards with 50-80% power increases for focused builds.