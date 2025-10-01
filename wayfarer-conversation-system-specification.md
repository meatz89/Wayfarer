# Wayfarer Conversation System - Complete Specification

## Table of Contents
1. [Design Philosophy](#design-philosophy)
2. [Core System Overview](#core-system-overview)
3. [The Five Resources](#the-five-resources)
4. [Depth Tier Unlock System](#depth-tier-unlock-system)
5. [Stat-to-Resource Mapping](#stat-to-resource-mapping)
6. [Card Architecture](#card-architecture)
7. [Statement History Requirements](#statement-history-requirements)
8. [Turn Actions](#turn-actions)
9. [Complete Formulas Reference](#complete-formulas-reference)
10. [Strategic Patterns](#strategic-patterns)
11. [Example Gameplay](#example-gameplay)
12. [Integration Systems](#integration-systems)

---

## Design Philosophy

### Core Principles

**Elegance Over Complexity**: Every mechanic serves exactly one purpose with clear, calculable effects.

**Verisimilitude Throughout**: All mechanics mirror how real conversations work. You can't skip to conclusions without building analytical foundation. Dominating creates lasting tension. Patient listening yields understanding.

**Perfect Information**: No hidden probabilities. Players can calculate exact outcomes before committing to actions.

**Impossible Choices**: Every decision presents multiple suboptimal paths. Success comes from strategic resource management, not optimal solutions.

**Deterministic Systems**: If you have the Initiative and meet requirements, cards work. Period.

### Design Goals Achieved

1. **Dynamic Conversations**: Available card depths change based on conversational momentum
2. **Character Identity**: Stats provide specialization advantages without exclusion
3. **Natural Pacing**: Foundation → Standard → Advanced → Master progression feels earned
4. **Sustainable Gameplay**: Echo/Statement distinction prevents resource depletion
5. **Strategic Depth**: Three interlocking systems (Depth Unlocks, Stat Bonuses, Statement Requirements) create complex decision spaces

---

## Core System Overview

### What Are Conversations?

Conversations are Wayfarer's primary gameplay challenge - the "combat encounters" expressed through social dynamics. They use a deterministic card-based system where:

- **Cards represent conversational moves** (observations, assertions, questions, demands)
- **Resources track conversation state** (momentum, tension, balance, history)
- **Success comes from resource optimization** (not dice rolls or luck)

### The Builder/Spender Foundation

Inspired by Steamworld Quest, conversations use an Initiative economy:

1. **Initiative starts at 0** each conversation
2. **Foundation cards (depth 1-2)** generate Initiative at 0 cost
3. **Higher depth cards** spend Initiative to create powerful effects
4. **You must build before you can spend** - creating natural conversation rhythm

### Three Interlocking Systems

**System 1: Depth Tier Unlocks** (via Momentum Thresholds)
- Momentum 6+: Unlock Tier 2 (depths 3-4)
- Momentum 12+: Unlock Tier 3 (depths 5-6)
- Momentum 18+: Unlock Tier 4 (depths 7-8)
- **Once unlocked, tiers persist** regardless of momentum changes

**System 2: Stat Specialization Bonuses** (via Character Stats)
- Stats 3-5: +1 depth for that stat's cards
- Stats 6-8: +2 depth for that stat's cards
- Stats 9-10: +3 depth for that stat's cards
- **Specialists access deeper cards sooner** than generalists

**System 3: Statement History Requirements** (via Spoken Pile Tracking)
- Standard cards: Require 2-3 matching Statement cards played
- Advanced cards: Require 4-5 matching Statement cards played
- Master cards: Require 7-8 matching Statement cards played
- **Powerful plays must be earned** through conversational foundation

---

## The Five Resources

Each resource has exactly ONE mechanical purpose with no overlap.

### 1. Initiative - Action Economy

**What It Represents**: Conversational energy and attention - your ability to make substantive contributions.

**Mechanical Role**:
- Determines how many cards you can play in sequence
- Starts at 0, must be built through Foundation cards (SteamWorld Quest pattern)
- Persists between LISTEN actions
- Can accumulate without limit

**Starting Value**:
```
Starting Initiative = 0

You must build before you can spend:
- Foundation cards (depth 1-2) cost 0 Initiative and generate +1 to +2 Initiative
- Higher depth cards cost Initiative to play
- This creates the natural "build then spend" rhythm
```

**How It Changes**:
- Foundation cards generate +1 to +2 Initiative (primarily Cunning cards)
- Playing cards costs their Initiative value
- Never decreases except through card play

### 2. Momentum - Conversational Progress

**What It Represents**: The sophistication and depth of the conversation. As momentum builds, more complex exchanges become possible.

**Mechanical Role**:
- Determines which depth tiers are unlocked
- Tracks progress toward conversation goals (8/12/16)
- Can be consumed by card costs for powerful effects
- Primary victory condition

**Starting Value**:
```
Starting Momentum = 0

All conversations start at momentum 0.
Stats only matter for their specific card types, not for starting bonuses.
```

**How It Changes**:
- Card effects add momentum (primarily Authority cards)
- LISTEN action subtracts momentum (cost = doubt cleared)
- Some cards consume momentum as a cost
- Minimum value: 0 (no negative momentum)

**Critical Thresholds**:
- **6 Momentum**: Unlock Tier 2 (depths 3-4)
- **8 Momentum**: Basic Goal threshold
- **12 Momentum**: Unlock Tier 3 (depths 5-6) AND Enhanced Goal threshold
- **16 Momentum**: Premium Goal threshold
- **18 Momentum**: Unlock Tier 4 (depths 7-8)

### 3. Doubt - Failure Timer

**What It Represents**: Conversational tension, miscommunication, and disengagement. The mounting pressure that threatens to end the conversation.

**Mechanical Role**:
- Timer that ends conversation at 10 points
- Creates urgency and forces action
- Binary threat (10 = failure)
- Can be cleared completely through LISTEN

**Starting Value**: 0

**How It Changes**:
- Card effects add doubt (primarily Authority cards with "+X Doubt" costs)
- Positive cadence refills doubt after LISTEN (see Cadence section)
- Diplomacy cards reduce doubt
- LISTEN action clears ALL doubt temporarily
- **Conversation ends immediately at 10 doubt**

### 4. Cadence - Conversation Balance

**What It Represents**: Who dominates the conversation. Positive values mean you're talking over others. Negative values mean you're listening well.

**Mechanical Role**:
- Tracks conversation rhythm
- Affects LISTEN action results (refills doubt or grants bonus cards)
- Creates mechanical pressure to balance speaking and listening
- Range: -5 (maximum listening) to +10 (maximum domination)

**Starting Value**: 0 (balanced conversation)

**How It Changes**:
- Every SPEAK action: +1 Cadence
- Every LISTEN action: -3 Cadence (after other effects)
- Rapport cards reduce cadence
- Floor at -5, ceiling at +10

**Effects on LISTEN**:
- **Positive Cadence**: Refills doubt equal to cadence value (punishment for dominating)
- **Negative Cadence**: Grants bonus cards equal to absolute value (reward for listening)

### 5. Statements in Spoken - Conversation History

**What It Represents**: The accumulated substance of the conversation. Specific points made, arguments stated, evidence presented.

**Mechanical Role**:
- Tracks Statement cards (not Echo) by stat type
- Enables requirements for powerful cards
- Used for scaling effects ("per Statement in Spoken")
- Determines conversation time cost (1 segment + total Statements)

**Starting Value**: 0 for each stat type

**Tracking**:
```
Spoken Pile contains:
- Insight Statements: 0
- Rapport Statements: 0
- Authority Statements: 0
- Diplomacy Statements: 0
- Cunning Statements: 0
```

**How It Changes**:
- Playing a Statement card adds 1 to its stat type counter
- Echo cards do NOT add to counters (they return to Deck)
- Counters never decrease during a conversation
- Counters reset to 0 when conversation ends

---

## Depth Tier Unlock System

### Problem Statement

**Original System**: Stats directly gated card depth access. If you had Authority 5, you could ALWAYS access Authority cards up to depth 5, regardless of conversational state.

**Issues**:
1. Binary gating felt rigid (either you had access or didn't)
2. Conversations felt static (same options regardless of flow)
3. Ignored conversational dynamics (doing well vs struggling felt the same)
4. No progression feeling within individual conversations

### Solution: Momentum-Based Tier Unlocks

**Core Concept**: Current momentum determines which depth tiers are accessible. As you build momentum in the conversation, more sophisticated options unlock.

**Verisimilitude**: In real conversations, your ability to make sophisticated arguments depends on the conversational foundation you've built. You can't jump straight to complex points - you build toward them.

### The Four Tiers

**Tier 1 (Depths 1-2): Foundation**
- **Unlock Condition**: Always available from conversation start
- **Contains**: Foundation cards that generate Initiative and begin building
- **Purpose**: Starting point for all players, core building blocks
- **Characteristics**: 
  - 0 Initiative cost
  - Generate Initiative (Cunning, Rapport)
  - Simple effects
  - No requirements

**Tier 2 (Depths 3-4): Standard**
- **Unlock Condition**: Reach 6 momentum
- **Contains**: Standard cards with moderate effects
- **Purpose**: Core conversational toolkit
- **Characteristics**:
  - 1-5 Initiative cost
  - Moderate effects (3-4x Foundation power)
  - May require 2-3 matching Statements
  - Accessible early-mid conversation

**Tier 3 (Depths 5-6): Advanced**
- **Unlock Condition**: Reach 12 momentum (coincides with Enhanced Goal)
- **Contains**: Advanced cards with powerful effects
- **Purpose**: Sophisticated arguments and significant moves
- **Characteristics**:
  - 3-6 Initiative cost
  - Powerful effects (5-7x Foundation power)
  - Often require 4-5 matching Statements
  - Mid-late conversation tools

**Tier 4 (Depths 7-8): Master**
- **Unlock Condition**: Reach 18 momentum (beyond Premium Goal at 16)
- **Contains**: Master cards with conversation-defining effects
- **Purpose**: Climactic finishers and ultimate plays
- **Characteristics**:
  - 5-9 Initiative cost
  - Explosive effects (10-15x Foundation power)
  - Require 7-8 matching Statements
  - Late conversation only, require mastery

### Persistence Rule

**Critical**: Once unlocked, depth tiers remain available for the entire conversation, regardless of momentum changes.

**Example**:
```
Momentum 15, Tier 3 unlocked (depths 1-6 available)
Player LISTENs, clearing 7 doubt
Momentum drops to 8
Tier 3 REMAINS UNLOCKED
Draw pool still includes depths 1-6
```

**Why**: In real conversations, once you've established sophistication, you don't lose access to complex topics just because tension spikes. The depth of discourse persists.

### Strategic Implications

**Early Pressure**: Must build to 6 momentum to unlock Standard cards (depths 3-4). This is your first mini-goal before pursuing actual goals.

**Mid-Game Decision**: Push to 12 momentum unlocks Tier 3 AND enables Enhanced Goal. Do you take the goal immediately or unlock the tier first for better tools?

**Late Game Aspiration**: Tier 4 at 18 momentum requires pushing PAST Premium goal (16). Only the most skilled or specialized builds reach Master tier.

**LISTEN Becomes Less Punishing**: Losing momentum doesn't lose depth access, making LISTEN a tactical tool rather than desperate escape.

---

## Stat-to-Resource Mapping

### Problem Statement

**Original System**: Stats provided depth bonuses but weren't tied to specific effects. All cards at depth X had similar power regardless of stat.

**Issues**:
1. Stats felt interchangeable (no mechanical identity)
2. Specialization didn't create distinct playstyles
3. Card effects were arbitrary rather than predictable
4. Players couldn't reliably plan resource flows

### Solution: Specialist with Universal Access

**CRITICAL REFINEMENT**: Resources use a **Specialist with Universal Access** model, NOT hard exclusivity.

**Core Principle**: Each stat SPECIALIZES in one resource (generating 2-3x more efficiently) but can ACCESS universal resources at weaker rates.

**Why This Works**:
- **Verisimilitude**: In real conversations, insightful analysis DOES advance conversation (momentum), just not as forcefully as commands
- **Gameplay**: Every deck can progress toward goals, but specialists excel in their domain
- **Identity**: Authority is still "the momentum stat" - it's just 2-3x better at it, not exclusive

**Resource Tiers**:

**Tier 1: Universal Resources** (all stats can generate)
- **Momentum**: Progression gate, must be accessible to all stats
- **Initiative**: Action economy, should be accessible to all stats

**Tier 2: Specialist Resources** (only specialists generate efficiently)
- **Cards** (Insight specialty): Others might get +1 card occasionally, Insight gets +2-6
- **Cadence** (Rapport specialty): Others rarely touch it, Rapport specializes in reduction
- **Doubt** (Diplomacy specialty): Others rarely reduce it, Diplomacy specializes

**Efficiency Pattern by Depth**:
```
Foundation (Depth 1-2): Specialist 2x rate, Universal 1x rate
Standard (Depth 3-4): Specialist 2.5x rate, Universal 1.5x rate
Advanced (Depth 5-6): Specialist 3x rate, Universal 2x rate
Master (Depth 7-8): Specialist 3-4x rate, Universal 2-3x rate
```

This creates:
- Clear mechanical identity per stat (via specialization)
- Predictable resource flows (specialists excel in their domain)
- Distinct playstyles through specialization (specialists are 2-3x better)
- Verisimilitude (all approaches advance progress, some more efficiently)

### The Five Mappings

#### Insight → Cards (Information Advantage)

**Thematic Identity**: Analytical thinking, observation, pattern recognition

**Mechanical Role**: Card draw engine, cycling deck, seeing options

**Verisimilitude**: Observant, analytical people gather more information and see more possibilities.

**Specialization**: Cards (2-6 draw) + Universal Access to Momentum/Initiative

**Effect Patterns (Specialist + Universal)**:
- Foundation: Draw 2 cards (specialist 2x), +1 Momentum (universal 1x)
- Standard: Draw 3 cards (specialist 2.5x), +2 Momentum + 1 Initiative (universal 1.5x)
- Advanced: Draw 4 cards (specialist 3x), +3 Momentum + 2 Initiative (universal 2x)
- Master: Draw 6 cards (specialist 3-4x), +5 Momentum + 3 Initiative (universal 2-3x)

**Example Cards**:
```
"Notice Detail" - Insight Depth 2 (Foundation)
Statement, 0 Initiative
Effect: Draw 2 cards, +1 Momentum
(Specialist: Draw 2 | Universal: +1 Momentum)

"Identify Pattern" - Insight Depth 4 (Standard)
Statement, 3 Initiative
Requirement: 2+ Insight Statements in Spoken
Effect: Draw 3 cards, +2 Momentum, +1 Initiative
(Specialist: Draw 3 | Universal: +2M +1I)

"Draw Conclusion" - Insight Depth 6 (Advanced)
Statement, 5 Initiative
Requirement: 5+ Insight Statements in Spoken
Effect: Draw 4 cards, +3 Momentum, +2 Initiative
(Specialist: Draw 4 | Universal: +3M +2I)
```

#### Rapport → Cadence (Rhythm Management)

**Thematic Identity**: Empathy, connection, conversation balance

**Mechanical Role**: Prevents death spiral, enables sustainable play, manages tension

**Verisimilitude**: Empathetic people balance conversation flow, neither dominating nor being dominated.

**Specialization**: Cadence (-1 to -3 reduction) + Universal Access to Momentum/Initiative

**Effect Patterns (Specialist + Universal)**:
- Foundation: -1 Cadence (specialist 2x), +1 Momentum + 1 Initiative (universal 1x)
- Standard: -2 Cadence (specialist 2.5x), +2 Momentum + 2 Initiative (universal 1.5x)
- Advanced: -3 Cadence (specialist 3x), +3 Momentum + 3 Initiative (universal 2x)
- Master: Set Cadence to -5 (specialist transform), +8 Momentum + 5 Initiative (universal 2-3x)

**Example Cards**:
```
"Active Listening" - Rapport Depth 2 (Foundation)
Echo, 0 Initiative
Effect: -1 Cadence, +1 Momentum, +1 Initiative
(Specialist: -1 Cad | Universal: +1M +1I)

"Validate Feelings" - Rapport Depth 4 (Standard)
Statement, 3 Initiative
Requirement: 3+ Rapport Statements in Spoken
Effect: -2 Cadence, +2 Momentum, +2 Initiative
(Specialist: -2 Cad | Universal: +2M +2I)

"Emotional Breakthrough" - Rapport Depth 8 (Master)
Statement, 7 Initiative
Requirement: 8+ Rapport Statements in Spoken
Effect: Set Cadence to -5, +8 Momentum, +5 Initiative
(Specialist: Set Cad -5 | Universal: +8M +5I)
```

#### Authority → Momentum (Direct Progress)

**Thematic Identity**: Command, force, directness, power

**Mechanical Role**: Goal rushing, aggressive momentum generation, high risk/reward

**Verisimilitude**: Commanding people drive conversations toward conclusions through force of will.

**Specialization**: Momentum (+2-12 generation) + Universal Access to Initiative + Doubt Trade-off

**Effect Patterns (Specialist + Universal + Trade-off)**:
- Foundation: +2 Momentum (specialist 2x), +1 Doubt (trade-off)
- Standard: +5 Momentum (specialist 2.5x), +2 Doubt, +1 Initiative (universal 1.5x)
- Advanced: +8 Momentum (specialist 3x), +3 Doubt, +2 Initiative (universal 2x)
- Master: +12 Momentum (specialist 3-4x), +4 Doubt, +3 Initiative (universal 2-3x)

**Example Cards**:
```
"Assert Position" - Authority Depth 2 (Foundation)
Statement, 1 Initiative
Effect: +2 Momentum, +1 Doubt
(Specialist: +2M | Trade-off: +1D | No universal at depth 2)

"Direct Demand" - Authority Depth 4 (Standard)
Statement, 4 Initiative
Requirement: 3+ Authority Statements in Spoken
Effect: +5 Momentum, +2 Doubt, +1 Initiative
(Specialist: +5M | Trade-off: +2D | Universal: +1I)

"Decisive Command" - Authority Depth 8 (Master)
Statement, 8 Initiative
Requirement: 8+ Authority Statements in Spoken
Effect: +12 Momentum, +4 Doubt, +3 Initiative
(Specialist: +12M | Trade-off: +4D | Universal: +3I)
```

#### Diplomacy → Doubt (Risk Mitigation)

**Thematic Identity**: Negotiation, risk management, tension relief

**Mechanical Role**: Safety valve, doubt cleansing, enables aggressive strategies

**Verisimilitude**: Negotiators address concerns and manage risks, reducing tension.

**Specialization**: Doubt (-1 to -6 reduction) + Universal Access to Momentum + Trading Pattern

**Effect Patterns (Specialist + Universal + Trading)**:
- Foundation: -1 Doubt (specialist 2x), +1 Momentum (universal 1x)
- Standard: -2 Doubt (specialist 2.5x), +2 Momentum (universal 1.5x), Consume 2 Momentum (trade)
- Advanced: -4 Doubt (specialist 3x), +3 Momentum (universal 2x), Consume 3 Momentum (trade)
- Master: Set Doubt to 0 (specialist transform), +5 Momentum (universal 2-3x), Consume 4 Momentum (trade)

**Example Cards**:
```
"Address Concern" - Diplomacy Depth 2 (Foundation)
Statement, 1 Initiative
Effect: -1 Doubt, +1 Momentum
(Specialist: -1D | Universal: +1M | No trade at depth 2)

"Find Common Ground" - Diplomacy Depth 4 (Standard)
Statement, 4 Initiative
Requirement: 3+ Diplomacy Statements in Spoken
Effect: -2 Doubt, +2 Momentum, Consume 2 Momentum
(Specialist: -2D | Universal: +2M | Trade: Consume 2M)

"Seal Agreement" - Diplomacy Depth 8 (Master)
Statement, 8 Initiative
Requirement: 8+ Diplomacy Statements in Spoken
Effect: Set Doubt to 0, +5 Momentum, Consume 4 Momentum
(Specialist: Set D→0 | Universal: +5M | Trade: Consume 4M)
```

#### Cunning → Initiative (Action Economy)

**Thematic Identity**: Tactics, opportunity creation, setup

**Mechanical Role**: Engine stat, enables combinations, chain enabler

**Verisimilitude**: Tactical people create opportunities for multiple moves.

**Specialization**: Initiative (+2-10 generation) + Universal Access to Momentum + Secondary Cards

**Effect Patterns (Specialist + Universal + Secondary)**:
- Foundation: +2 Initiative (specialist 2x), +1 Momentum (universal 1x)
- Standard: +4 Initiative (specialist 2.5x), +2 Momentum (universal 1.5x), Draw 1 card (secondary)
- Advanced: +6 Initiative (specialist 3x), +3 Momentum (universal 2x), Draw 2 cards (secondary)
- Master: +10 Initiative (specialist 3-4x), +5 Momentum (universal 2-3x), Draw 3 cards (secondary)

**Example Cards**:
```
"Subtle Maneuver" - Cunning Depth 2 (Foundation)
Echo, 0 Initiative
Effect: +2 Initiative, +1 Momentum
(Specialist: +2I | Universal: +1M)

"Create Opening" - Cunning Depth 4 (Standard)
Statement, 2 Initiative
Requirement: 3+ Cunning Statements in Spoken
Effect: +4 Initiative, +2 Momentum, Draw 1 card
(Specialist: +4I | Universal: +2M | Secondary: Draw 1)

"Spring the Trap" - Cunning Depth 8 (Master)
Statement, 6 Initiative
Requirement: 8+ Cunning Statements in Spoken
Effect: +10 Initiative, +5 Momentum, Draw 3 cards
(Specialist: +10I | Universal: +5M | Secondary: Draw 3)
```

### Stat Bonus System

**Problem**: With momentum gating depth access, stats risked becoming irrelevant.

**Solution**: Stats provide depth bonuses for cards of their type, allowing specialists early access.

**Bonus Progression**:
```
Stats 1-2: +0 depth bonus
Stats 3-5: +1 depth bonus
Stats 6-8: +2 depth bonus
Stats 9-10: +3 depth bonus
```

**How It Works**:
```
Available Draw Pool Formula:

For each card in conversation deck:
  IF (card.depth <= current_unlocked_tier_max_depth) 
  OR (card.stat_type matches specialized_stat 
      AND card.depth <= current_unlocked_tier_max_depth + stat_bonus):
    Add to available draw pool
```

**Example**:
```
Current State:
- Tier 2 unlocked (momentum 8, depths 1-4 baseline)
- Player has Diplomacy 7 (+2 bonus)

Available Depths:
- ANY card depths 1-4
- COMMERCE cards depths 1-6
- Diplomacy specialist can access Advanced Diplomacy cards earlier
```

**Strategic Implication**: Specialists maintain advantage even when struggling (low momentum), preserving character identity through mechanical benefit.

---

## Card Architecture

### Card Structure

Every card has exactly these properties:

```
1. Name (thematic, flavor)
2. Stat Type (Insight/Rapport/Authority/Diplomacy/Cunning)
3. Depth (1-8, determines tier and power level)
4. Persistence Type (Echo or Statement)
5. Initiative Cost (varies by stat and depth)
6. Requirement (none, Statement count, or state check)
7. Effect (stat-appropriate resource manipulation)
```

### Persistence Types

**Echo Cards**: Return to Deck after playing
- Represent **repeatable techniques** and conversational tools
- Can be played multiple times in one conversation
- Do NOT add to Statement count in Spoken
- Examples: "Thoughtful Pause," "Deflection," "Active Listening"

**Statement Cards**: Go to Spoken pile after playing
- Represent **specific content** and one-time declarations
- Build the Statement count for requirements
- Cannot be played again in the same conversation
- Examples: "Share Discovery," "Make Promise," "State Terms"

**Verisimilitude**: You can ask clarifying questions repeatedly (Echo), but you can only reveal a specific secret once (Statement). You can pause thoughtfully many times (Echo), but making a specific promise happens once (Statement).

### Echo/Statement Ratios

**By Stat Identity**:
```
Cunning: 80% Echo (tactics are inherently repeatable)
Rapport: 75% Echo (empathy techniques repeat)
Diplomacy: 65% Echo (negotiation tools repeat)
Insight: 60% Echo (analysis repeats, insights don't)
Authority: 50% Echo (commands repeat, declarations don't)
```

**By Depth Tier**:
```
Foundation (1-2): 70-80% Echo (sustainable tools)
Standard (3-4): 50-60% Echo (balanced)
Advanced (5-6): 40-50% Echo (building history)
Master (7-8): 20-30% Echo (climactic moments)
```

**Why Ratios Invert**: Foundation needs sustainability. Masters represent conversation-defining moments that are inherently one-time declarations.

### Initiative Cost Curves

Each stat has characteristic costs based on efficiency:

**Cunning** (Cheapest - generates Initiative):
```
Depth 1-2: 0 Initiative
Depth 3-4: 1-2 Initiative
Depth 5-6: 3-4 Initiative
Depth 7-8: 5-6 Initiative
```

**Rapport** (Cheap - sustainable rhythm):
```
Depth 1-2: 0-1 Initiative
Depth 3-4: 2-3 Initiative
Depth 5-6: 3-4 Initiative
Depth 7-8: 5-7 Initiative
```

**Insight** (Moderate - information value):
```
Depth 1-2: 0-1 Initiative
Depth 3-4: 2-3 Initiative
Depth 5-6: 4-5 Initiative
Depth 7-8: 6-7 Initiative
```

**Diplomacy** (Moderate-Expensive - safety costs):
```
Depth 1-2: 0-1 Initiative
Depth 3-4: 3-4 Initiative
Depth 5-6: 5-6 Initiative
Depth 7-8: 7-8 Initiative
```

**Authority** (Most Expensive - power costs):
```
Depth 1-2: 0-1 Initiative
Depth 3-4: 3-5 Initiative
Depth 5-6: 5-7 Initiative
Depth 7-8: 7-9 Initiative
```

**Design Logic**: Resource generators (Cunning) are cheap. Power plays (Authority) are expensive. This creates natural resource loops.

### Effect Formula Types

**Type A: Fixed Value**
```
Simple, predictable effects
Example: "Draw 2 cards"
Use: Primarily Foundation tier
```

**Type B: Linear Scaling**
```
Effect scales with game state
Example: "Draw 1 card per 2 Statements in Spoken (max 4)"
Use: Standard and Advanced tiers
```

**Type C: State-Conditional**
```
Effect depends on threshold
Example: "If Doubt ≥ 5: +6 Initiative"
Use: Advanced tier, rewards/requires specific situations
```

**Type D: Conversion/Trading**
```
Exchange one resource for another
Example: "Consume 3 Momentum: -5 Doubt"
Use: Diplomacy cards primarily, various tiers
```

**Type E: State-Setting**
```
Not additive, but transformative
Example: "Set Cadence to -3"
Use: High depths only, powerful resets
```

---

## Statement History Requirements

### Problem Statement

**Context**: Without logical prerequisites, powerful cards could be played without foundation. An Insight character could "draw a complex conclusion" in turn 1 without any prior analysis.

**Issue**: Broke verisimilitude. Real conversations require building to sophisticated points.

### Solution: Track Statement History by Stat

**Core Concept**: The Spoken pile tracks how many Statement cards of each stat type have been played. Powerful cards require having built conversational foundation in that domain.

**Tracking System**:
```
Spoken Pile Tracking:
- Insight Statements in Spoken: 0
- Rapport Statements in Spoken: 0
- Authority Statements in Spoken: 0
- Diplomacy Statements in Spoken: 0
- Cunning Statements in Spoken: 0

When Statement card played:
- Card goes to Spoken pile
- Counter for card's stat type increments by 1

When Echo card played:
- Card returns to Deck
- No counter increment
```

### Requirement Thresholds by Depth

**Foundation (Depth 1-2): No Requirements**
- Purpose: Build foundations for everything else
- Always playable
- Statement versions begin accumulating counts

**Standard (Depth 3-4): Light Requirements**
```
Typically require: 2-3 Statements of matching stat
Example: "Requires 2+ Insight Statements in Spoken"
```
- Accessible early-mid conversation
- Reward for consistent stat usage

**Advanced (Depth 5-6): Moderate Requirements**
```
Typically require: 4-5 Statements of matching stat
Example: "Requires 5+ Insight Statements in Spoken"
```
- Mid-late conversation payoffs
- Require committed specialization

**Master (Depth 7-8): Heavy Requirements**
```
Typically require: 7-8 Statements of matching stat
Example: "Requires 8+ Insight Statements in Spoken"
```
- Late conversation climax
- Require total commitment to approach

### Verisimilitude Examples

#### Insight Path (Information → Conclusion)

```
Turn 1-2: "Notice Detail" (Foundation Statement)
Verisimilitude: Basic observations
Mechanical: Builds Insight Statement count to 2-3

Turn 3-4: "Identify Pattern" (Standard, requires 2+ Insight Statements)
Verisimilitude: Connecting observations into patterns
Mechanical: Count reaches 4-5

Turn 5-6: "Draw Conclusion" (Advanced, requires 5+ Insight Statements)
Verisimilitude: Logical conclusions from substantial analysis
Mechanical: Count reaches 7-8

Turn 7+: "Perfect Deduction" (Master, requires 8+ Insight Statements)
Verisimilitude: Masterful deduction from comprehensive foundation
Mechanical: Climactic analytical moment
```

#### Authority Path (Positioning → Command)

```
Turn 1-2: "Assert Position" (Foundation Statement)
Verisimilitude: Establishing authoritative presence
Mechanical: Builds Authority Statement count, generates doubt

Turn 3-4: "Direct Demand" (Standard, requires 3+ Authority Statements)
Verisimilitude: Demands require established authority
Mechanical: Count reaches 5-6, doubt accumulating

Turn 5: Diplomacy cards cleanse doubt
Verisimilitude: Must address concerns before continuing pressure
Mechanical: Doubt reduced, momentum cost paid

Turn 6-7: "Compelling Argument" (Advanced, requires 5+ Authority Statements)
Verisimilitude: Compelling force requires authority foundation
Mechanical: Count reaches 8+

Turn 8+: "Decisive Command" (Master, requires 8+ Authority Statements)
Verisimilitude: Commands that compel action
Mechanical: Explosive momentum finisher (+12M)
```

#### Rapport Path (Building → Breakthrough)

```
Turn 1-3: "Active Listening" (Foundation Statements)
Verisimilitude: Beginning empathetic engagement
Mechanical: Builds Rapport count, reduces cadence

Turn 4-5: "Validate Feelings" (Standard, requires 3+ Rapport Statements)
Verisimilitude: Validation requires established empathy
Mechanical: Count reaches 5-6, cadence staying low

Turn 6-7: "Deep Understanding" (Advanced, requires 5+ Rapport Statements)
Verisimilitude: Profound understanding requires relational foundation
Mechanical: Count reaches 8+

Turn 8+: "Emotional Breakthrough" (Master, requires 8+ Rapport Statements)
Verisimilitude: Transformative moments require deep trust
Mechanical: Set Cadence to -5, massive momentum
```

### Strategic Implications

**Specialization Rewards**: Focusing on one stat type builds Statement count faster, unlocking powerful cards earlier.

**Hybrid Challenges**: Multi-stat builds must allocate Statements across types, reaching high counts in any single stat later or not at all.

**Clear Progression**: Players feel themselves building toward powerful plays. Each Statement is progress toward unlocking finishers.

**Impossible Choices**: Play varied cards for flexibility vs focused cards to reach requirements faster?

---

## Turn Actions

### The Two Actions

Players alternate between two actions:

1. **SPEAK**: Play one card from hand
2. **LISTEN**: Draw cards, reset doubt, modify resources

### SPEAK Action

**Complete Sequence**:
```
1. Increase Cadence by +1 (you're speaking)

2. Select one card from Mind (hand)

3. Check Initiative Cost
   - Must have sufficient Initiative to pay cost
   - If insufficient, card cannot be played

4. Check Requirements
   - Statement count requirements (if any)
   - State requirements (if any)
   - If not met, card cannot be played

5. Pay Initiative Cost
   - Subtract card's Initiative cost from current Initiative

6. Apply Card Effect
   - Deterministic resolution
   - No dice, no randomness
   - Perfect information

7. Card to Appropriate Pile
   - Statement: Goes to Spoken pile, increments stat counter
   - Echo: Returns to Deck

8. Check for Personality Rule Effects
   - Devoted: +1 additional Doubt if effect added Doubt
   - Mercantile: Highest Initiative card gets bonus
   - Proud: Cards must be ascending Initiative order
   - Cunning: Same Initiative as previous costs -2 Momentum
   - Steadfast: Momentum changes capped at ±2

9. Continue or Must LISTEN
   - Can SPEAK again if Initiative remains
   - Or choose to LISTEN
   - Or conversation may end if goal reached or doubt = 10
```

**Critical Notes**:
- Can play multiple cards per turn if sufficient Initiative
- Initiative persists across SPEAKs (doesn't reset until LISTEN)
- Cadence increases by 1 PER card played
- Playing 3 cards = Cadence +3 that turn

### LISTEN Action

**Critical Sequencing**: Order matters significantly.

**Complete Sequence**:
```
1. Calculate doubt_cleared
   doubt_cleared = current_doubt

2. Clear All Doubt
   current_doubt = 0

3. Reduce Momentum by doubt_cleared
   current_momentum = max(0, current_momentum - doubt_cleared)
   Note: Depth tier unlocks persist regardless

4. Check Cadence for Refill (CRITICAL)
   IF current_cadence > 0:
     current_doubt += current_cadence
     IF current_doubt >= 10:
       Conversation ENDS (failure)
       STOP (skip remaining steps)

5. Calculate Cards to Draw
   IF current_cadence < 0:
     bonus_cards = abs(current_cadence)
   ELSE:
     bonus_cards = 0
   
   cards_to_draw = 3 + bonus_cards

6. Draw Cards
   Draw cards_to_draw from available depth tier pool
   Add to Mind (hand)

7. Reduce Cadence
   current_cadence = max(-5, current_cadence - 3)
   Floor at -5 (maximum listening state)
```

### Why Sequencing Matters

**The Cadence Trap**:
```
Example: M12, D7, Cad+6

LISTEN:
1. Clear doubt: D→0, M→5 (paid 7 momentum)
2. Cadence refill: D→6 (still in danger!)
3. Draw 3 cards (no bonus, cadence was positive)
4. Reduce cadence: Cad→3

Result: Paid 7 momentum to reduce doubt from 7 to 6
Barely improved situation, still have positive cadence
```

**The Death Spiral Endpoint**:
```
Example: M10, D1, Cad+10 (maximum domination)

LISTEN:
1. Clear doubt: D→0, M→9
2. Cadence refill: D→10 (conversation ENDS)
3. Never draw cards or reduce cadence

Result: Conversation lost. At Cadence +10, cannot safely LISTEN with any doubt.
```

**The Reward for Patience**:
```
Example: M8, D3, Cad-4

LISTEN:
1. Clear doubt: D→0, M→5 (paid 3 momentum)
2. Cadence is negative: No refill, D stays 0
3. Draw 3 + 4 = 7 cards (huge hand refresh)
4. Reduce cadence: Cad→-5 (capped at floor)

Result: Cleared all doubt, drew 7 cards, positioned for next turn
Patient listeners are rewarded
```

### Conversation End Conditions

**Failure**:
- Doubt reaches 10 (immediate loss)
- Cadence refill during LISTEN pushes doubt to 10

**Success**:
- Request card played (goal achieved at 8/12/16 momentum)
- Player chooses to leave (forfeit goals but safe exit)

**Cleanup After End**:
- All piles cleared
- NPC persistent decks unchanged (Signature, Observation, Burden remain)
- XP awarded: 1 XP per card played × conversation difficulty level
- Tokens awarded based on goal achieved
- Time cost: 1 segment + total Statement cards in Spoken pile

---

## Complete Formulas Reference

### Starting Values

```
Starting Initiative = 0 (must build through Foundation cards)
Starting Momentum = 0 (build through card play)
Starting Doubt = 0
Starting Cadence = 0
Starting Statements = 0 for all stat types
```

### Depth Tier Unlocks

```
Tier 1 (depths 1-2): Always unlocked
Tier 2 (depths 3-4): Unlocks when momentum reaches 6
Tier 3 (depths 5-6): Unlocks when momentum reaches 12
Tier 4 (depths 7-8): Unlocks when momentum reaches 18

Once unlocked, tiers persist regardless of momentum changes
```

### Stat Bonus Progression

```
Stats 1-2: +0 depth bonus
Stats 3-5: +1 depth bonus to specialized stat's cards
Stats 6-8: +2 depth bonus to specialized stat's cards
Stats 9-10: +3 depth bonus to specialized stat's cards
```

### Draw Pool Calculation

```
For each card in conversation type deck:
  tier_max_depth = current_unlocked_tier × 2
  
  IF (card.depth <= tier_max_depth)
  OR (card.stat_type matches specialized_stat 
      AND card.depth <= tier_max_depth + stat_bonus):
    Add to available draw pool
```

### SPEAK Action Effects

```
Per SPEAK:
- Cadence += 1
- Initiative -= card.initiative_cost
- Apply card.effect (deterministic)
- IF card is Statement: stat_statement_count += 1
- IF card is Echo: card returns to Deck
```

### LISTEN Action Effects

```
1. doubt_cleared = current_doubt
2. current_doubt = 0
3. current_momentum = max(0, current_momentum - doubt_cleared)
4. IF current_cadence > 0: 
     current_doubt += current_cadence
     IF current_doubt >= 10: END CONVERSATION
5. bonus_cards = (current_cadence < 0) ? abs(current_cadence) : 0
6. cards_to_draw = 3 + bonus_cards
7. Draw cards_to_draw from available pool
8. current_cadence = max(-5, current_cadence - 3)
```

### Goal Thresholds

```
Basic Goal: 8 momentum (rewards: standard)
Enhanced Goal: 12 momentum (rewards: improved)
Premium Goal: 16 momentum (rewards: maximum)
```

### Cadence Ranges

```
Minimum: -5 (maximum listening, bonus +5 cards on LISTEN)
Neutral: 0 (balanced conversation)
Maximum: +10 (maximum domination, +10 doubt on LISTEN)
```

### Doubt Mechanics

```
Range: 0 to 10
At 10: Conversation ends (failure)
Cleared: LISTEN sets to 0, then applies cadence refill
```

### Statement Requirements

```
Foundation (depths 1-2): No requirements
Standard (depths 3-4): 2-3 matching Statement cards
Advanced (depths 5-6): 4-5 matching Statement cards
Master (depths 7-8): 7-8 matching Statement cards
```

---

## Strategic Patterns

### Archetype 1: The Analyst (Insight Specialist)

**Build**: Insight 10 (+3 bonus), Cunning 7 (+2 bonus)

**Philosophy**: "I gather information until I can prove my point"

**Turn Pattern**:
```
Early (Turns 1-3):
- Play Insight Foundation Statements (0-1I cost)
- Use Cunning Echoes to generate Initiative
- Draw many cards, minimal momentum
- Build Insight Statement count to 4-5

Mid (Turns 4-6):
- Unlock Tier 2 at momentum 6
- Access Insight Standard cards (require 2-3 Statements)
- Stat bonus gives access to Insight Advanced (depths 5-6)
- Continue drawing cards and building Statement count

Late (Turns 7+):
- Unlock Tier 3 at momentum 12
- Statement count at 7-8, unlock Master Insights
- "Perfect Deduction" (Draw 6 cards, +8 Momentum)
- Chain multiple high-depth Insight cards
- Achieve Premium goal through information advantage
```

**Win Condition**: Overwhelming card advantage → superior options → optimal plays

**Strengths**:
- Sees many more cards than opponents
- Finds combinations consistently
- Adapts to any situation
- Stat bonus gives early access to Advanced Insights

**Weaknesses**:
- Doesn't directly build momentum
- Must convert card advantage into plays
- Requires planning and sequencing

### Archetype 2: The Commander (Authority Specialist)

**Build**: Authority 9 (+2 bonus), Diplomacy 7 (+2 bonus)

**Philosophy**: "I establish dominance then compel action"

**Turn Pattern**:
```
Early (Turns 1-3):
- Aggressive Authority Foundation Statements
- Accept doubt accumulation (+1 per Authority card)
- Build momentum rapidly (+2 per Foundation)
- Build Authority Statement count to 3-4
- Doubt at 4-5 by turn 3

Turn 3-4:
- Play Diplomacy cards to cleanse doubt
- Trade momentum for safety
- Continue Authority pressure

Mid (Turns 5-7):
- Unlock Tier 2, access Authority Standards
- Stat bonus gives access to Authority Advanced
- Authority Statement count at 6-7
- Burst → Cleanse → Burst pattern
- Doubt management through Diplomacy

Late (Turn 8+):
- Statement count reaches 8
- "Decisive Command" available (+12M, +4D)
- Achieve Premium goal (16M) explosively
- Accept final doubt, end conversation before failure
```

**Win Condition**: Overwhelming force → Premium goals through power

**Strengths**:
- Fastest momentum generation
- Can hit Premium goals earlier than others
- Stat bonuses in both Authority and Diplomacy
- Clear win condition

**Weaknesses**:
- Generates significant doubt
- Requires Diplomacy investment for safety
- High risk if misplayed
- Expensive Initiative costs

### Archetype 3: The Diplomat (Rapport Specialist)

**Build**: Rapport 10 (+3 bonus), Insight 6 (+1 bonus)

**Philosophy**: "I build trust then reach mutual understanding"

**Turn Pattern**:
```
Early (Turns 1-4):
- Play Rapport Foundation Statements
- Reduce Cadence consistently (-1 per card)
- Maintain Cadence at -1 to -3
- Build Rapport Statement count to 4-5
- Slow momentum but safe

Turn 5:
- LISTEN at Cadence -3
- Draw 6 cards (3 base + 3 bonus)
- Doubt cleared for 3 momentum cost
- Cadence → -5 (capped)

Mid (Turns 6-8):
- Huge hand (6 cards)
- Play 3-4 cards per turn with Initiative
- Statement count reaches 6-7
- Never accumulate doubt
- LISTEN every other turn with 8-card draws

Late (Turn 9+):
- Statement count at 8+
- "Emotional Breakthrough" (Set Cad to -5, +8M)
- Sustainable engine with 8-card LISTENs
- Achieve Enhanced/Premium through patience
```

**Win Condition**: Sustainable engine → consistent progress → patient victory

**Strengths**:
- Never trapped in death spiral
- Massive card advantage through negative cadence
- Very safe, forgiving gameplay
- Stat bonus gives early access to Rapport Advanced

**Weaknesses**:
- Slower momentum generation
- Longer conversations
- No explosive finishers like Authority
- Requires discipline to maintain patience

### Archetype 4: The Chain Artist (Cunning Specialist)

**Build**: Cunning 10 (+3 bonus), Authority 6 (+1 bonus)

**Philosophy**: "I create opportunities then exploit them"

**Turn Pattern**:
```
Early (Turns 1-2):
- Cunning Foundation Echoes (+2 Initiative each)
- Can replay same cards multiple times
- Generate 6-8 Initiative quickly
- Play 3-4 cards per turn

Mid (Turns 3-5):
- Unlock Tier 2
- Cunning Standards (+4 Initiative)
- Chain 5-6 cards per turn
- Build Cunning Statement count through Statements
- Use Authority finishers with accumulated Initiative

Late (Turns 6+):
- Cunning Statement count at 8+
- "Spring the Trap" (+10 Initiative, +8 Momentum)
- Can play entire hand in one turn
- End with Authority finishers
```

**Win Condition**: Initiative advantage → multi-card chains → overwhelming turns

**Strengths**:
- Most cards played per turn
- Long combination chains
- Spectacular explosive turns
- Stat bonus gives access to Cunning Advanced

**Weaknesses**:
- Chains don't directly win (need finishers)
- Requires careful hand management
- Must sequence correctly
- Can overkill on Initiative generation

---

## Example Gameplay

### Complete Conversation Example

**Setup**:
- **Player**: All stats Level 3 (+1 bonus to all stats)
- **NPC**: Marcus (Mercantile personality, Level 2 conversation)
- **Conversation Type**: Trade Negotiation
- **Goal**: Deliver letter (Basic: 8M, Enhanced: 12M, Premium: 16M)

**Initial State**:
```
Momentum: 3 (started at 2 + floor(3/3) = 3)
Initiative: 4 (started at 3 + floor(3/3) = 4)
Doubt: 0
Cadence: 0
Tier Unlocked: 1 (depths 1-2)
Statement Counts: All 0
```

**Hand (5 cards drawn at start)**:
1. "Subtle Maneuver" (Cunning D2, Statement, 0I, +2I +1M)
2. "Assert Position" (Authority D2, Statement, 1I, +2M +1D)
3. "Active Listening" (Rapport D2, Echo, 0I, -1Cad +1M)
4. "Thoughtful Pause" (Cunning D1, Echo, 0I, +2I)
5. "Notice Detail" (Insight D2, Statement, 0I, Draw 2, +1M)

---

**Turn 1**:

**SPEAK: "Thoughtful Pause" (Cunning Echo)**
- Cost: 0 Initiative
- Effect: +2 Initiative
- Cadence: +1 (now 1)
- State: M3, I6, D0, Cad1

**SPEAK: "Subtle Maneuver" (Cunning Statement)**
- Cost: 0 Initiative
- Effect: +2 Initiative, +1 Momentum
- Goes to Spoken: Cunning Statements = 1
- Cadence: +1 (now 2)
- State: M4, I8, D0, Cad2, Cunning:1

**SPEAK: "Assert Position" (Authority Statement)**
- Cost: 1 Initiative
- Effect: +2 Momentum, +1 Doubt
- Goes to Spoken: Authority Statements = 1
- Cadence: +1 (now 3)
- Mercantile Rule: Highest Initiative card gets bonus? Only played one card with cost, no comparison yet
- State: M6, I7, D1, Cad3, Cunning:1, Authority:1

**Tier 2 Unlocks!** (Momentum reached 6)
- Available depths: 1-4 baseline
- Plus stat bonuses: All stats +1, so depths 1-5 for specialized cards

**LISTEN**:
1. Doubt cleared: 1
2. Doubt → 0
3. Momentum: 6 - 1 = 5
4. Cadence check: +3, so Doubt += 3 → Doubt = 3
5. Cards to draw: 3 + 0 = 3 (cadence positive, no bonus)
6. Draw 3 cards from Tier 2 pool
7. Cadence: 3 - 3 = 0

**State**: M5, I7, D3, Cad0, Tier 2 unlocked
**Hand**: 7 cards (2 remaining + 3 drawn + 2 from "Notice Detail" unplayed)

---

**Turn 2**:

**New cards available**: Standards (depths 3-4) now in draw pool

**SPEAK: "Active Listening" (Rapport Echo)**
- Cost: 0 Initiative
- Effect: Reduce Cadence by 1, +1 Momentum
- Returns to Deck (Echo)
- Cadence: 0 + 1 (from speaking) - 1 (from effect) = 0
- State: M6, I7, D3, Cad0

**Tier 2 Re-Unlocks** (Momentum back to 6)

**SPEAK: "Find Common Ground" (Diplomacy D4 Standard, requires 3+ Diplomacy Statements - DON'T HAVE)**
- Cannot play yet, need Statement foundation first

**SPEAK: "Address Concern" (Diplomacy D2, Statement)**
- Cost: 1 Initiative
- Effect: -1 Doubt, +1 Momentum
- Goes to Spoken: Diplomacy Statements = 1
- Cadence: +1 (now 1)
- State: M7, I6, D2, Cad1, Diplomacy:1

**SPEAK: "Create Opening" (Cunning D4 Standard, requires 3+ Cunning Statements)**
- Have 1 Cunning Statement, need 2 more - CANNOT PLAY

**SPEAK: Another Foundation to build counts**
- Continue building...

**Analysis**: Player is building toward Standard cards but needs more Statement foundations. The Tier 2 unlock gave access but requirements gate the powerful plays.

---

**Turn 5** (skipping ahead):

**State**: M10, I5, D4, Cad2, Tier 2 unlocked
**Statement Counts**: Cunning:3, Authority:2, Diplomacy:2, Rapport:1, Insight:2

**SPEAK: "Create Opening" (Cunning D4, NOW PLAYABLE)**
- Requires: 3+ Cunning Statements ✓ (have 3)
- Cost: 2 Initiative
- Effect: +4 Initiative, +2 Momentum
- Goes to Spoken: Cunning Statements = 4
- Cadence: +1 (now 3)
- State: M12, I7, D4, Cad3, Cunning:4

**Tier 3 Unlocks!** (Momentum reached 12)
**Enhanced Goal Available!** (12 Momentum threshold)

**Choice Point**: 
- Play request card now for Enhanced Goal?
- Or continue building for Premium (16M)?

**Decision**: Continue building, player wants Premium

**SPEAK: Diplomacy card to cleanse doubt**
- Effect: -2 Doubt
- State: M12, I5, D2, Cad4

**LISTEN**:
1. Doubt cleared: 2
2. Doubt → 0  
3. Momentum: 12 - 2 = 10
4. Cadence +4, so Doubt += 4 → Doubt = 4
5. Draw 3 cards
6. Cadence: 4 - 3 = 1

**State**: M10, I5, D4, Cad1, Tier 3 still unlocked (persists)

---

**Turn 8** (skipping to climax):

**State**: M14, I8, D3, Cad2, Tier 3 unlocked
**Statement Counts**: Authority:5 (built up through mid-game)

**SPEAK: "Compelling Argument" (Authority D6 Advanced)**
- Requires: 5+ Authority Statements ✓ (have 5)
- Cost: 6 Initiative
- Effect: +8 Momentum, +3 Doubt
- Goes to Spoken: Authority Statements = 6
- Mercantile Rule: Highest Initiative card this turn gets bonus? Track it
- Cadence: +1 (now 3)
- State: M22, I2, D6, Cad3

**SPEAK: Request Card "Accept Letter Delivery"**
- Premium Goal: 16 Momentum ✓ (have 22)
- Effect: Accept obligation, gain +3 tokens
- Conversation Success!

**Result**:
- Tokens: +3 Diplomacy with Marcus
- Payment: 12 coins (Premium reward)
- XP: 15 cards played × 2 (Level 2 conversation) = 30 XP distributed
- Time: 1 segment + 10 Statements = 11 segments

---

## Integration Systems

### Personality Rules

Each NPC personality applies one rule that modifies conversation mechanics:

**Proud**: Cards must be played in ascending Initiative order each turn
- Resets when you LISTEN
- Verisimilitude: Respectful escalation required
- Impact: Constrains card sequencing

**Devoted**: When card effects add Doubt, add +1 additional Doubt
- Applies to card effects, not LISTEN cadence refill
- Verisimilitude: Catastrophizes, tension escalates faster
- Impact: Doubles doubt pressure from Authority plays

**Mercantile**: Your highest Initiative cost card each turn gains +3 Momentum bonus
- Compare costs of all cards played before LISTEN
- Verisimilitude: Rewards getting to the point
- Impact: Favors playing one expensive card per turn

**Cunning**: Playing same Initiative cost as previous card costs -2 Momentum
- Verisimilitude: Repetitive patterns are suspicious
- Impact: Encourages varied Initiative costs

**Steadfast**: All Momentum changes from card effects capped at ±2
- Does NOT apply to LISTEN momentum cost
- Verisimilitude: Steady, unmoved by extremes
- Impact: Makes explosive plays ineffective, favors consistent building

### Conversation Types

Each conversation type has a predefined deck with specific stat distribution:

**Support Conversation**:
```
Distribution: 45% Rapport, 25% Insight, 20% Diplomacy, 10% Other
Archetype: The Empath
Foundation Density: 45% (very sustainable)
Feels: Patient, forgiving, many safety nets
```

**Trade Negotiation**:
```
Distribution: 30% Diplomacy, 30% Authority, 25% Rapport, 15% Other
Archetype: The Negotiator
Foundation Density: 35% (balanced)
Feels: Push/pull dynamic, risk and mitigation
```

**Investigation**:
```
Distribution: 40% Insight, 30% Cunning, 20% Rapport, 10% Other
Archetype: The Analyst
Foundation Density: 35% (balanced)
Feels: Information-focused, requires conversion
```

**Authority Challenge**:
```
Distribution: 40% Authority, 30% Cunning, 20% Diplomacy, 10% Other
Archetype: The Commander
Foundation Density: 30% (challenging)
Feels: Aggressive, high stakes, dangerous
```

### NPC Persistent Decks

Each NPC maintains three persistent decks that enhance conversations:

**Signature Deck** (5 cards, token-unlocked):
```
0 tokens: No signature cards
1-2 tokens: 1 signature card mixed in
3-5 tokens: 2 signature cards
6-9 tokens: 3 signature cards
10-14 tokens: 4 signature cards
15+ tokens: All 5 signature cards
```

**Observation Deck** (location discoveries):
- Cards from completing observations at locations
- Automatically mixed into draw pile at conversation start
- Consumed when played
- Example: "Safe Passage Knowledge" unlocks route options

**Burden Deck** (relationship damage):
- Cards from failed obligations and broken promises
- Makes "Make Amends" conversation type harder
- Must be cleared through successful resolution conversations

### Goal System

**Multi-Threshold Structure**:
```
Basic Goal (8 Momentum):
- Standard rewards
- Achievable with one reset
- +1 token

Enhanced Goal (12 Momentum):
- Improved rewards
- Requires clean play or specialization
- +2 tokens

Premium Goal (16 Momentum):
- Maximum rewards
- Requires mastery or risk-taking
- +3 tokens
```

**Request Card Mechanics**:
- Fixed terms (no negotiation)
- Multiple goal cards in same Request bundle
- Can play when momentum reaches threshold
- Playing Request card ends conversation with success

### XP and Progression

**XP Award Formula**:
```
XP per card played = 1 × conversation_difficulty

Conversation Difficulties:
- Level 1 (Strangers, simple NPCs): 1× multiplier
- Level 2 (Named NPCs, standard): 2× multiplier
- Level 3 (Nobles, complex NPCs): 3× multiplier

Total XP = cards_played × difficulty
Distributed to stats based on cards played
```

**Stat Progression**:
```
Level 1→2: 10 XP
Level 2→3: 25 XP
Level 3→4: 50 XP
Level 4→5: 100 XP
Level 5→6: 175 XP
Level 6→7: 275 XP
Level 7→8: 400 XP
Level 8→9: 550 XP
Level 9→10: 750 XP
```

**Stat Level Benefits**:
- All levels: Unlock deeper card depths via bonus
- Level 2+: Unlock investigation approaches (world interaction)
- Level 3+: +1 depth bonus (first specialization payoff)
- Level 5+: +4 starting Initiative (from formula)
- Level 6+: +2 depth bonus (advanced specialist)
- Level 9+: +3 depth bonus (master level)

---

## Design Verification

### Does It Achieve Core Goals?

**✓ Elegance**: Three clean systems (Depth Unlocks, Stat Bonuses, Statement Requirements) work together without overlapping mechanics.

**✓ Verisimilitude**: 
- Can't skip to conclusions without premises
- Dominating creates lasting tension
- Patient listening yields understanding
- Stats represent actual competencies

**✓ Perfect Information**: All formulas visible, no hidden randomness, deterministic effects.

**✓ Impossible Choices**:
- LISTEN timing (cheap early vs expensive late)
- Stat specialization (depth in one vs breadth in many)
- Statement allocation (build finishers vs flexibility)
- Tier unlock vs goal (12M unlocks Tier 3 AND Enhanced Goal)

**✓ Strategic Depth**:
- 4+ distinct archetypes with different win patterns
- Cadence management creates death spiral avoidance game
- Statement requirements create earned progression
- Depth unlocks create satisfying breakthrough moments

### Remaining Questions for Playtest

1. **Conversation Length**: How many turns does a typical 10-15 minute conversation take? Are Statement thresholds (2/5/8) appropriately calibrated?

2. **Difficulty Curve**: Do conversation types feel appropriately different? Is Authority (30% Foundation) too punishing?

3. **Specialist vs Generalist**: Is the +1/+2/+3 depth bonus strong enough to make specialization worthwhile?

4. **Cadence Balance**: Is the -5 to +10 range appropriate? Does the death spiral at +8-10 cadence happen often enough to matter?

5. **Goal Accessibility**: Can players consistently reach Enhanced (12M) goals? Are Premium (16M) goals appropriately rare/difficult?

---

## Conclusion

The Wayfarer conversation system creates strategic depth through three interlocking mechanics that each serve a clear purpose:

1. **Depth Tier Unlocks**: Momentum-based progression creates dynamic conversations with satisfying breakthrough moments
2. **Stat Bonuses**: Character specialization provides mechanical advantages without exclusion
3. **Statement Requirements**: Logical prerequisites ensure powerful plays feel earned

Combined with the stat-to-resource mapping (Insight→Cards, Rapport→Cadence, Authority→Momentum, Diplomacy→Doubt, Cunning→Initiative), the system achieves perfect verisimilitude while maintaining deterministic, calculable outcomes.

Players must manage five distinct resources (Initiative, Momentum, Doubt, Cadence, Statements) toward clear goals, making impossible choices with perfect information every turn. This is conversation as strategic gameplay.
