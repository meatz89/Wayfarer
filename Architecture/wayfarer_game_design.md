# Wayfarer Encounter System: Complete Design Document

## Table of Contents
1. [Introduction](#1-introduction)
2. [System Architecture](#2-system-architecture)
3. [Base Tag System](#3-base-tag-system)
4. [Encounter Tag System](#4-encounter-tag-system)
5. [Narrative Presentation Layer](#5-narrative-presentation-layer)
6. [Choice System](#6-choice-system)
7. [Card Selection Algorithm](#7-card-selection-algorithm)
8. [Location System](#8-location-system)
9. [Example Encounters](#9-example-encounters)
10. [Implementation Guide](#10-implementation-guide)

---

## 1. Introduction

Wayfarer is a medieval life simulation where players navigate the world as ordinary travelers rather than epic heroes. The encounter system serves as the core interaction mechanism for all situations, replacing traditional combat with a universal approach for social interactions, exploration, trading, and even fighting.

The system is built around strategic tag management, where players must carefully choose which tags to build and when to use pressure choices to gain specific tag effects at the cost of increased complications. The only way to reduce pressure is through strategic tags that activate when certain tag thresholds are reached.

### Core Concept

The encounter system functions like a card-based board game:

- Each turn, the player receives 4 "cards" (choices)
- The player selects one choice, discarding the rest
- Their selection modifies base tags that track their approach and focus
- These base tags trigger encounter tags that provide mechanical effects
- The player's goal is to accumulate sufficient momentum to succeed while managing pressure

### Key Resources

- **Momentum**: Progress toward success (only increases through momentum choices)
- **Pressure**: Risk or complications (only increases through pressure choices, 10 = failure)
- **Base Tags**: Values on a 0-5 scale tracking approach and focus
- **Encounter Tags**: Triggered effects that determine mechanics (can reduce pressure)

### Key Principles

## Key Principles of the Pressure/Momentum System

1. **Choices Build, Never Reduce**: Choices either BUILD momentum OR BUILD pressure - never reduce pressure
2. **Pressure Reduction**: Pressure can ONLY be reduced through active, advantageous strategic encounter tags
3. **Strategic Pressure Choices**: Players may deliberately choose pressure-building options for:
   - Gaining specific tag modifications when they're strategically valuable
   - Taking advantage of active encounter tags that provide benefits to pressure choices
   - Having no viable momentum option due to narrative tag restrictions
4. **Tag Management**: Success depends on building the right tags to activate beneficial strategic tags
5. **Risk vs. Reward**: Players must weigh the cost of increased pressure against the strategic value of tag effects

---

## 2. System Architecture

The Wayfarer encounter system consists of three distinct layers that work together:

### Base Layer
- Tracks fundamental state variables (approach and focus tags)
- These tags are modified by player choices
- They have no direct mechanical effects themselves
- Values range from 0 to 5 for each tag

### Encounter Layer
- Provides actual gameplay mechanics through two types of tags:
  - **Narrative Tags**: Control which choices appear
  - **Strategic Tags**: Determine how effective choices are
- Tags activate at standard thresholds (2+ for minor, 4+ for major)
- In friendly locations, these tags usually benefit the player
- In hostile locations, these tags often restrict options or reduce effectiveness

### Presentation Layer
- Determines how encounters are narratively presented:
  - **Social Encounters**: Direct speech dialogue
  - **Intellectual Encounters**: Internal monologue and observations
  - **Physical Encounters**: Action descriptions
- Changes presentation based on context while mechanics remain consistent

The separation allows each system to handle different aspects while working together to create a cohesive experience.

---

## 3. Base Tag System

### Approach Tags (HOW)
These track how the player approaches encounters:

| Approach Tag  | Description                               | Associated Approach |
|---------------|-------------------------------------------|---------------------|
| Dominance     | Force, authority, intimidation            | Force               |
| Rapport       | Social connections, charm, persuasion     | Charm               |
| Analysis      | Intelligence, observation, problem-solving| Wit                 |
| Precision     | Careful execution, finesse, accuracy      | Finesse             |
| Concealment   | Stealth, hiding, subterfuge               | Stealth             |

### Focus Tags (WHAT)
These track what the player focuses on:

| Focus Tag     | Description                               |
|---------------|-------------------------------------------|
| Relationship  | Connections with others, social dynamics  |
| Information   | Knowledge, facts, understanding           |
| Physical      | Bodies, movement, physical objects        |
| Environment   | Surroundings, spaces, terrain             |
| Resource      | Items, money, supplies, valuables         |

### Natural Tag Relationships

| Approach Tag | Related Focus Tags          | Complementary Tags |
|--------------|-----------------------------|--------------------|
| Dominance    | Physical, Environment       | Precision          |
| Rapport      | Relationship, Information   | Analysis           |
| Analysis     | Information, Resource       | Works with all     |
| Precision    | Physical, Resource          | Concealment        |
| Concealment  | Environment, Information    | Precision          |

---

## 4. Encounter Tag System

The encounter tag system provides the actual gameplay mechanics through two types of tags.

### Standardized Tag Thresholds
All encounter tags use consistent activation thresholds:
- **Minor Effects**: Activate at 2+ in relevant tag
- **Major Effects**: Activate at 4+ in relevant tag

### Narrative Layer Tags
These tags control which choices can appear in the player's hand:

- **Activation**: Triggered when specific approach/focus tag thresholds are reached
  - Example: "Confrontation" activates at Dominance 4+
  - Example: "Social Scandal" activates if Force is used in Noble Court

- **Effect**: Binary states that restrict approaches
  - Example: "Confrontation" blocks Stealth approaches from appearing
  - Example: "Public Setting" blocks Concealment approaches

- **Persistence**: Once activated, they remain active for the duration of the encounter unless explicitly removed

### Strategic Layer Tags
These tags modify the effectiveness of choices:

- **Activation**: Also triggered at standardized tag thresholds
  - Example: "Social Currency" activates at Rapport 2+
  - Example: "Master Analyst" activates at Analysis 4+

- **Effect**: Provide consistent mechanical bonuses
  - Example: "Social Currency" adds +1 momentum to Resource choices
  - Example: "Network Leverage" reduces pressure by 1 at the end of each turn
  - Example: "Informed Traveler" adds +1 momentum to Information choices

- **Rules**:
  - All locations have a predefined set of potential strategic tags
  - Tags remain active as long as tag level stays above threshold
  - All momentum/pressure modifiers are standardized to +1/-1

### Thematic Tag Names
- Tag names should be thematically relevant to the location 
  - Example: "Warm Hearth" for an inn
  - Example: "Merchant's Respect" for a market
- The effects should be purely mechanical
  - Example: +1 momentum to Charm approaches
  - Example: -1 pressure from Resource choices

---

## 5. Narrative Presentation Layer

The presentation layer determines how encounters are narratively presented without affecting the underlying mechanics.

### Social Encounters - Direct Speech
All choices and outcomes presented as dialogue between characters.

**Example Choices:**
1. **Friendly Inquiry** (Charm+Information, Momentum)
   - *"Good evening! I'm new to these parts. What can you tell me about the road conditions ahead?"*
2. **Build Rapport** (Charm+Relationship, Momentum)
   - *"This is excellent ale! Is it brewed locally? You must have quite the stories after all your years here."*

### Intellectual Encounters - Internal Monologue/Observations
All choices and outcomes presented as the character's thoughts and analytical observations.

**Example Choices:**
1. **Process Evidence** (Wit+Information, Momentum)
   - *I need to connect these disparate facts. The ledger entries from winter don't match the storehouse inventory...*
2. **Environmental Assessment** (Wit+Environment, Momentum)
   - *This room's architecture suggests recent modifications. The newer stonework doesn't quite match the older foundation.*

### Physical Encounters - Action Descriptions
All choices and outcomes presented as physical movements and environmental interactions.

**Example Choices:**
1. **Silent Movement** (Stealth+Physical, Momentum)
   - *You carefully place each foot between fallen leaves, testing the ground before shifting your weight to avoid making any sound.*
2. **Display of Strength** (Force+Physical, Momentum)
   - *You grip the heavy oak beam with both hands, muscles straining as you lift it clear of the debris pile.*

---

## 6. Choice System

### Choice Structure
Each choice consists of:
- **Approach**: Force, Charm, Wit, Finesse, or Stealth
- **Focus**: Relationship, Information, Physical, Environment, or Resource
- **Effect Type**: Momentum (builds progress) or Pressure (builds complications)
- **Tag Effects**: How the choice modifies base tag values
- **Narrative Description**: Context-sensitive description of the action

Importantly, choices themselves have no special effects beyond tag modification and momentum/pressure changes. All special effects come from encounter tags that are defined by the location and activated when certain tag thresholds are reached.

**Critical Rule**: Choices either BUILD momentum OR BUILD pressure - they never reduce pressure directly. Strategic tags may reduce pressure (e.g., "-1 pressure at end of turn"), but choices themselves always either advance progress (momentum) or increase complications (pressure).

### Base Choice Effects
- **Momentum Choices**: Build 2 momentum (standard) or 3 momentum (special)
- **Pressure Choices**: Build 2 pressure (standard) or varying pressure (emergency)
- **Critical Rule**: Choices NEVER reduce pressure directly - they either build momentum OR build pressure
- **Strategic Value**: Players choose pressure-building choices for:
  - Specific tag effects when strategically valuable
  - Taking advantage of encounter tags that benefit pressure choices
  - When preferred momentum choices are blocked by narrative tags

### Core Choices (50 Base Options)

#### Force Approach Choices

**Force + Relationship**
1. **Command Respect** (Momentum)
   - Tags: +2 Dominance, -1 Rapport
   - Effect: Add 2 momentum

2. **Escalate Demands** (Pressure)
   - Tags: +2 Dominance, -1 Rapport
   - Effect: Add 2 pressure
   - *Strategic value: Rapidly builds Dominance tags when needed*

**Force + Information**
3. **Direct Questioning** (Momentum)
   - Tags: +1 Dominance, +1 Analysis
   - Effect: Add 2 momentum

4. **Intimidating Interrogation** (Pressure)
   - Tags: +2 Dominance, -1 Rapport
   - Effect: Add 2 pressure
   - *Strategic value: Maximizes Dominance growth while reducing Rapport*

**Force + Physical**
5. **Display of Strength** (Momentum)
   - Tags: +2 Dominance, -1 Concealment
   - Effect: Add 2 momentum

6. **Aggressive Posturing** (Pressure)
   - Tags: +2 Dominance, -1 Rapport
   - Effect: Add 2 pressure
   - *Strategic value: Useful when you need Dominance but don't want to lose Concealment*

**Force + Environment**
7. **Control Territory** (Momentum)
   - Tags: +1 Dominance, +1 Analysis
   - Effect: Add 2 momentum

8. **Force Through Obstacles** (Pressure)
   - Tags: +2 Dominance, -1 Precision
   - Effect: Add 2 pressure
   - *Strategic value: High Dominance gain when reducing Precision is acceptable*

**Force + Resource**
9. **Seize Resources** (Momentum)
   - Tags: +2 Dominance, -1 Rapport
   - Effect: Add 2 momentum

10. **Forceful Requisition** (Pressure)
    - Tags: +1 Dominance, +1 Rapport
    - Effect: Add 2 pressure
    - *Strategic value: Rare combination of Dominance and Rapport increase*

#### Charm Approach Choices

**Charm + Relationship**
11. **Build Rapport** (Momentum)
    - Tags: +2 Rapport, -1 Dominance
    - Effect: Add 2 momentum

12. **Express Vulnerability** (Pressure)
    - Tags: +2 Rapport, -1 Dominance
    - Effect: Add 2 pressure
    - *Strategic value: Identical tag effects to Build Rapport when momentum is blocked*

**Charm + Information**
13. **Friendly Inquiry** (Momentum)
    - Tags: +1 Rapport, +1 Analysis
    - Effect: Add 2 momentum

14. **Deflecting Explanation** (Pressure)
    - Tags: +2 Rapport, -1 Dominance
    - Effect: Add 2 pressure
    - *Strategic value: Maximizes Rapport while reducing Dominance*

**Charm + Physical**
15. **Graceful Display** (Momentum)
    - Tags: +1 Rapport, +1 Precision
    - Effect: Add 2 momentum

16. **Hesitant Approach** (Pressure)
    - Tags: +2 Rapport, -1 Dominance
    - Effect: Add 2 pressure
    - *Strategic value: Builds Rapport at the cost of Dominance and pressure*

**Charm + Environment**
17. **Create Ambiance** (Momentum)
    - Tags: +1 Rapport, +1 Precision
    - Effect: Add 2 momentum

18. **Secluded Conversation** (Pressure)
    - Tags: +2 Rapport, +1 Concealment
    - Effect: Add 2 pressure
    - *Strategic value: Rare combination of Rapport and Concealment increase*

**Charm + Resource**
19. **Negotiate Terms** (Momentum)
    - Tags: +1 Rapport, +1 Analysis
    - Effect: Add 2 momentum

20. **Overextend Resources** (Pressure)
    - Tags: +2 Rapport
    - Effect: Add 2 pressure
    - *Strategic value: Maximizes Rapport growth with no negative tag effects*

#### Wit Approach Choices

**Wit + Relationship**
21. **Analyze Motives** (Momentum)
    - Tags: +2 Analysis, +1 Rapport
    - Effect: Add 2 momentum

22. **Overthink Social Dynamics** (Pressure)
    - Tags: +2 Analysis, +1 Rapport
    - Effect: Add 2 pressure
    - *Strategic value: Identical tag effects to Analyze Motives when momentum is blocked*

**Wit + Information**
23. **Process Evidence** (Momentum)
    - Tags: +2 Analysis, +1 Precision
    - Effect: Add 2 momentum

24. **Deep Investigation** (Pressure)
    - Tags: +2 Analysis, +1 Precision
    - Effect: Add 2 pressure
    - *Strategic value: Identical tag effects to Process Evidence when momentum is blocked*

**Wit + Physical**
25. **Analyze Weaknesses** (Momentum)
    - Tags: +2 Analysis, +1 Dominance
    - Effect: Add 2 momentum

26. **Complex Maneuvering** (Pressure)
    - Tags: +1 Analysis, +1 Concealment
    - Effect: Add 2 pressure
    - *Strategic value: Builds both Analysis and Concealment simultaneously*

**Wit + Environment**
27. **Environmental Assessment** (Momentum)
    - Tags: +2 Analysis, +1 Concealment
    - Effect: Add 2 momentum

28. **Complex Navigation** (Pressure)
    - Tags: +2 Analysis, +1 Concealment
    - Effect: Add 2 pressure
    - *Strategic value: Identical tag effects to Environmental Assessment when momentum is blocked*

**Wit + Resource**
29. **Optimize Usage** (Momentum)
    - Tags: +2 Analysis, +1 Precision
    - Effect: Add 2 momentum

30. **Resource Analysis** (Pressure)
    - Tags: +2 Analysis
    - Effect: Add 2 pressure
    - *Note: This choice allows you to gain Analysis tags when you need them, even at the cost of added pressure*

#### Finesse Approach Choices

**Finesse + Relationship**
31. **Subtle Influence** (Momentum)
    - Tags: +1 Precision, +1 Rapport
    - Effect: Add 2 momentum

32. **Calculated Social Risk** (Pressure)
    - Tags: +1 Precision, +1 Rapport
    - Effect: Add 2 pressure
    - *Strategic value: Identical tag effects to Subtle Influence when momentum is blocked*

**Finesse + Information**
33. **Precise Questioning** (Momentum)
    - Tags: +1 Precision, +1 Analysis
    - Effect: Add 2 momentum

34. **Read Between Lines** (Pressure)
    - Tags: +1 Precision, +1 Analysis
    - Effect: Add 2 pressure

**Finesse + Physical**
35. **Precise Movement** (Momentum)
    - Tags: +2 Precision, +1 Concealment
    - Effect: Add 2 momentum

36. **Controlled Force** (Pressure)
    - Tags: +1 Precision, +1 Dominance
    - Effect: Add 2 pressure

**Finesse + Environment**
37. **Environmental Manipulation** (Momentum)
    - Tags: +1 Precision, +1 Analysis
    - Effect: Add 2 momentum

38. **Circumvent Obstacles** (Pressure)
    - Tags: +1 Precision, +1 Concealment
    - Effect: Add 2 pressure

**Finesse + Resource**
39. **Careful Allocation** (Momentum)
    - Tags: +2 Precision, +1 Analysis
    - Effect: Add 2 momentum

40. **Minimal Resource Use** (Pressure)
    - Tags: +1 Precision, +1 Analysis
    - Effect: Add 2 pressure

#### Stealth Approach Choices

**Stealth + Relationship**
41. **Observe Interactions** (Momentum)
    - Tags: +1 Concealment, +1 Analysis
    - Effect: Add 2 momentum

42. **Avoid Detection** (Pressure)
    - Tags: +2 Concealment, -1 Rapport
    - Effect: Add 2 pressure

**Stealth + Information**
43. **Eavesdrop** (Momentum)
    - Tags: +1 Concealment, +1 Analysis
    - Effect: Add 2 momentum

44. **Hidden Research** (Pressure)
    - Tags: +1 Concealment, +2 Analysis
    - Effect: Add 2 pressure

**Stealth + Physical**
45. **Silent Movement** (Momentum)
    - Tags: +2 Concealment, +1 Precision
    - Effect: Add 2 momentum

46. **Conceal Presence** (Pressure)
    - Tags: +2 Concealment, -1 Dominance
    - Effect: Add 2 pressure

**Stealth + Environment**
47. **Use Cover** (Momentum)
    - Tags: +2 Concealment, +1 Precision
    - Effect: Add 2 momentum

48. **Create Diversion** (Pressure)
    - Tags: +1 Concealment, -1 Dominance
    - Effect: Add 2 pressure

**Stealth + Resource**
49. **Secret Acquisition** (Momentum)
    - Tags: +1 Concealment, +1 Precision
    - Effect: Add 2 momentum

50. **Hide Resources** (Pressure)
    - Tags: +2 Concealment, +1 Analysis
    - Effect: Add 2 pressure

### Transitional Choices (10 Additional)

These choices bridge between approaches, allowing more organic transitions:

1. **Quiet Threat** (Force + Concealment, Momentum)
   - Tags: +1 Dominance, +1 Concealment
   - Effect: Add 2 momentum

2. **Tactical Force** (Force + Analysis, Momentum)
   - Tags: +1 Dominance, +1 Analysis, +1 Precision
   - Effect: Add 2 momentum

3. **Charming Diversion** (Charm + Concealment, Pressure)
   - Tags: +1 Rapport, +1 Concealment
   - Effect: Add 2 pressure

4. **Analytical Conversation** (Charm + Analysis, Momentum)
   - Tags: +1 Rapport, +1 Analysis
   - Effect: Add 2 momentum

5. **Precision Strike** (Force + Precision, Momentum)
   - Tags: +1 Dominance, +2 Precision
   - Effect: Add 2 momentum

6. **Calculated Risk** (Analysis + Concealment, Momentum)
   - Tags: +1 Analysis, +1 Concealment
   - Effect: Add 2 momentum

7. **De-escalate Tension** (Charm + Dominance, Pressure)
   - Tags: +1 Rapport, -2 Dominance
   - Effect: Add 2 pressure

8. **Fade from Attention** (Concealment + Rapport, Pressure)
   - Tags: +2 Concealment, -1 Rapport
   - Effect: Add 2 pressure

9. **Forceful Insight** (Analysis + Dominance, Momentum)
   - Tags: +1 Analysis, +1 Dominance
   - Effect: Add 2 momentum

10. **Precise Social Cues** (Precision + Rapport, Momentum)
    - Tags: +1 Precision, +1 Rapport
    - Effect: Add 2 momentum

### Special Choices

Special choices are additional options in the player's hand that can be unlocked through tag thresholds:

- **Tag Requirements**: Appear when specific tag values are reached (typically 2+ or 4+)
- **Enhanced Effects**: Generate +3 momentum (vs. +2 for standard choices)
- **Multiple Tag Effects**: Affect more tag categories simultaneously
- **Unique to Location**: Each location defines its own special choices

### Emergency Choices

Emergency choices provide alternative approaches when normal choices are blocked by narrative tags:

1. **Desperate Appeal** (Charm emergency option)
   - Tags: +1 Rapport
   - Effect: Build 1 momentum, build 2 pressure
   - *Available when Charm approaches are blocked*

2. **Firm Stance** (Force emergency option)
   - Tags: +2 Dominance
   - Effect: Build 1 momentum, build 2 pressure
   - *Available when Force approaches are blocked*

3. **Quick Withdrawal** (Stealth emergency option)
   - Tags: +2 Concealment
   - Effect: Build 1 momentum, build 2 pressure
   - *Available when Stealth approaches are blocked*

4. **Careful Analysis** (Wit emergency option)
   - Tags: +2 Analysis, +1 to lowest tag
   - Effect: Build 1 momentum, build 1 pressure
   - *Available when Wit approaches are blocked*

5. **Precise Adjustment** (Finesse emergency option)
   - Tags: +2 Precision
   - Effect: Build 1 momentum, build 1 pressure
   - *Available when Finesse approaches are blocked*

These emergency choices give players a fallback option when their preferred approach is blocked, but at the cost of lower momentum and higher pressure than standard choices.

---

## 7. Card Selection Algorithm

The card selection algorithm determines which 4 choices appear in the player's hand each turn.

**Selection Process**

The card selection algorithm builds a hand of 4 choices each turn designed for strategic depth:

1. **Apply Narrative Tag Filters**:
   - Remove choices that are blocked by active narrative tags

2. **Calculate Base Scores**:
   - Base score: 10 points for all approaches
   - Location preference: +3 for favored approaches, -2 for disfavored
   - Tag effects:
     * Each point in matching approach tag: +1 point
     * Each point in matching focus tag: +1 point

3. **Select Strategic Diverse Hand**:
   - One momentum choice from highest-scoring approach
   - One momentum choice from a complementary approach
   - One pressure choice (for tag effects when specific tag changes are needed)
   - One special or situational choice (when requirements are met)

4. **Ensure Hand Diversity**:
   - At least 3 different approaches
   - At least one momentum option
   - No more than 2 choices with the same approach or focus

5. **Apply Strategic Tag Effects**:
   - Modify choice effectiveness based on active strategic tags
   - These don't change which choices appear, only their effects when selected

This process ensures players always have meaningful strategic decisions, balancing progress (momentum) against advantageous tag effects (pressure choices) while adapting to the evolving encounter state.

### Emergency Options

Emergency options may appear when an approach is blocked by narrative tags if:
- The player has no viable choices otherwise
- The player has high values in the blocked approach
- A relevant emergency option would provide strategic value

---

## 8. Location System

### Location Definition Components
- **Favored/Disfavored Approaches**: Which approaches receive bonuses/penalties
- **Favored/Disfavored Focuses**: Which focuses receive bonuses/penalties
- **Narrative Tags**: Which narrative layer tags can activate
- **Strategic Tags**: Which strategic layer tags can activate
- **Momentum Thresholds**: Success breakpoints (Failure, Partial, Standard, Exceptional)
- **Duration**: How many turns the encounter lasts
- **Hostility Level**: Balance of beneficial vs. restrictive tags

### Unified Location Design
- **All Locations Have Both**: Each location includes both beneficial and restrictive tags
- **Hostility Balance**: The ratio determines if a location is friendly, neutral, or hostile
- **Consistent Mechanics**: All locations use the same activation thresholds and effect types

### Friendly Locations
Friendly locations have more beneficial tags than restrictive ones:
- **Beneficial Narrative Tags**: Few restrictions on player approaches
- **Positive Strategic Tags**: Multiple ways to gain momentum bonuses
- **Minimal Pressure**: Few or no automatic pressure increases

### Hostile Locations
Hostile locations have more restrictive tags than beneficial ones:
- **Restrictive Narrative Tags**: Multiple blocks on player approaches
- **Defensive Strategic Tags**: Reduce effectiveness of certain approaches
- **Pressure Mechanics**: May add pressure automatically each turn
- **Exploitable Weaknesses**: Specific tag combinations that provide advantages

### Example Location: Village Market (Friendly)

```
Location: Village Market

Core Properties:
- Favored Approaches: Charm, Finesse
- Disfavored Approaches: Force, Stealth
- Favored Focuses: Relationship, Resource
- Disfavored Focuses: Physical
- Duration: 5 turns
- Momentum Thresholds: 0-7 (Failure), 8-9 (Partial), 10-11 (Standard), 12+ (Exceptional)
- Hostility Level: Friendly (more beneficial than restrictive tags)

Narrative Tags:
- "Open Marketplace" (Active from start): Blocks Concealment approaches
- "Market Suspicion" (At Concealment 2+): Blocks Charm approaches
- "Guard Presence" (At Dominance 4+): Blocks Stealth approaches

Strategic Tags (Beneficial):
- "Merchant's Respect" (At Rapport 2+): +1 momentum to Resource-focused choices
- "Haggler's Eye" (At Precision 2+): -1 pressure from Resource-focused choices
- "Market Wisdom" (At Analysis 4+): +1 momentum to Information-focused choices
- "Trading Network" (At Relationship 4+): -1 pressure at end of each turn

Unique Location Choices:
- "Negotiate Better Price" (requires Rapport 2+ and Resource 2+)
- "Find Rare Goods" (requires Analysis 2+ and Resource 2+)
- "Build Trade Network" (requires Rapport 2+ and Relationship 2+)
```

### Example Location: Bandit Ambush (Hostile)

```
Location: Bandit Ambush

Core Properties:
- Favored Approaches: Force, Stealth
- Disfavored Approaches: Charm, Wit
- Favored Focuses: Physical, Environment
- Disfavored Focuses: Relationship
- Duration: 4 turns
- Momentum Thresholds: 0-8 (Failure), 9-11 (Partial), 12-14 (Standard), 15+ (Exceptional)
- Hostility Level: Hostile (more restrictive than beneficial tags)

Narrative Tags (Restrictive):
- "Surrounded" (Active from start): Blocks Concealment approaches
- "Threat of Violence" (Active from start): Blocks Charm approaches
- "Drawn Weapons" (At Dominance 4+): Blocks Wit approaches
- "Fight Started" (At Physical 4+): Blocks all non-Force approaches

Strategic Tags (Restrictive):
- "Numerical Advantage" (Active from start): +1 pressure at end of each turn
- "Territorial Knowledge" (Active from start): -1 momentum from Environment-focused choices
- "Battle Ready" (At Dominance 4+): -1 momentum from Force approaches

Strategic Tags (Beneficial - Weaknesses):
- "Superstitious" (At Wit 2+): -1 pressure at end of each turn
- "Poorly Coordinated" (At Precision 4+): +1 momentum to Force approaches
- "Easily Distracted" (At Concealment 2+): +1 momentum to Stealth approaches

Unique Location Choices:
- "Coordinated Defense" (requires Precision 2+ and Physical 2+)
- "Exploit Terrain" (requires Analysis 2+ and Environment 2+)
- "Find Escape Route" (requires Concealment 2+ and Analysis 2+)
```

---

## 9. Example Encounters

### Example: Roadside Inn Encounter (Social)

#### Initial State
- **Location**: Roadside Inn (Favors Charm, Wit, Relationship, Information)
- **Objective**: Secure comfortable lodging and gather useful travel information
- **Approach/Focus Tags**: All at 0 (no approaches established yet)
- **Active Encounter Tags**: 
  - "Common Room" (Narrative): Blocks Concealment approaches
  - "Tavern Regulations" (Narrative): Blocks Force approaches
- **Selection focuses on location preferences**: First hand favors Charm and Information but includes diverse options

#### Turn 1: Initial Arrival

**Available Choices:**
1. **Friendly Inquiry** (Charm+Information, Momentum)
   - *"Good evening! I'm new to these parts. What can you tell me about the road conditions ahead?"*
2. **Environmental Assessment** (Wit+Environment, Momentum)
   - *"This seems like a well-kept establishment. How long have you been running this inn?"*
3. **Smooth Over Tensions** (Charm+Relationship, Pressure)
   - *"I hope I'm not disturbing your evening. I promise I won't be any trouble."*
4. **Minimal Resource Use** (Finesse+Resource, Pressure)
   - *"What's your most affordable room for the night? I'm trying to stretch my coins."*

**Player Selects: "Friendly Inquiry"**
- Tag Effects: +1 Rapport, +1 Analysis, +2 Information
- New Tag States: 
  - Approach Tags: Concealment 0, Dominance 0, Rapport 1, Analysis 1, Precision 0
  - Focus Tags: Relationship 0, Information 2, Physical 0, Environment 0, Resource 0
- Momentum Gained: 2 (base momentum from choice)

#### Turn 2: Social Integration

**Player Selects: "Build Rapport"**
- Tag Effects: +2 Rapport, +2 Relationship
- New Tag States: 
  - Approach Tags: Concealment 0, Dominance 0, Rapport 3, Analysis 1, Precision 0
  - Focus Tags: Relationship 2, Information 2, Physical 0, Environment 0, Resource 0
- Activated Strategic Tags: "Warm Hearth" (at Rapport 2+)
  - Effect: +1 momentum to Relationship-focused choices
- Momentum Gained: 2

#### Turn 3: Information Gathering

**Player Selects: "Exchange Travel News"**
- Tag Effects: +1 Rapport, +2 Analysis, +2 Information
- New Tag States: 
  - Approach Tags: Concealment 0, Dominance 0, Rapport 4, Analysis 3, Precision 0
  - Focus Tags: Relationship 2, Information 4, Physical 0, Environment 0, Resource 0
- Activated Strategic Tags: "Traveler's Rest" (at Information 4+)
  - Effect: +1 momentum to Information-focused choices
- Momentum Gained: 3 (special choice)

#### Turn 4: Securing Accommodations

**Player Selects: "Share Stories"**
- Tag Effects: +2 Rapport, +1 Analysis, +2 Relationship
- New Tag States: 
  - Approach Tags: Concealment 0, Dominance 0, Rapport 6, Analysis 4, Precision 0
  - Focus Tags: Relationship 4, Information 4, Physical 0, Environment 0, Resource 0
- Activated Strategic Tags: 
  - "Innkeeper's Favor" (at Rapport 4+): -1 pressure at end of each turn
  - "Regular Patron" (at Relationship 4+): +1 momentum to Charm approaches
- Momentum Gained: 3 (special choice) + 1 (from Warm Hearth) = 4
- Pressure Reduced: 1 (from Innkeeper's Favor)

#### Final State After 4 Turns:
- Momentum: 11 (Standard Success)
- Pressure: 0
- Outcome: Player builds strong connections at the inn, gathers valuable travel information, and secures comfortable accommodations with multiple strategic advantages.

### Example: Bandit Ambush (Physical)

#### Turn 1: Initial Confrontation

**Player Selects: "Environmental Assessment"**
- *You carefully scan the surroundings, noting the positions of the bandits and potential escape routes.*
- Tag Effects: +2 Analysis, +1 Concealment
- New Tag States: 
  - Approach Tags: Concealment 1, Dominance 0, Rapport 0, Analysis 2, Precision 0
  - Focus Tags: Relationship 0, Information 0, Physical 0, Environment 2, Resource 0
- Activated Strategic Tags: "Easily Distracted" (at Concealment 2+)
  - Effect: +1 momentum to Stealth approaches
- Momentum Gained: 2

#### Turn 2: Strategic Positioning

**Player Selects: "Tactical Movement"**
- *You shift your position to place a large tree between yourself and the largest group of bandits.*
- Tag Effects: +1 Analysis, +1 Concealment
- New Tag States: 
  - Approach Tags: Concealment 2, Dominance 0, Rapport 0, Analysis 3, Precision 0
  - Focus Tags: Relationship 0, Information 0, Physical 0, Environment 2, Resource 0
- Activated Strategic Tags: "Superstitious" (at Wit 2+)
  - Effect: -1 pressure at end of each turn
- Momentum Gained: 2
- Pressure: +1 (from "Numerical Advantage") - 1 (from "Superstitious") = 0 net change

#### Turn 3: Exploiting Weakness

**Player Selects: "Silent Movement"**
- *You carefully slip between the shadows, moving to a better position while avoiding detection.*
- Tag Effects: +2 Concealment, +1 Precision
- New Tag States: 
  - Approach Tags: Concealment 4, Dominance 0, Rapport 0, Analysis 3, Precision 1
  - Focus Tags: Relationship 0, Information 0, Physical 1, Environment 2, Resource 0
- Momentum Gained: 2 + 1 (from "Easily Distracted") = 3
- Pressure: +1 (from "Numerical Advantage") - 1 (from "Superstitious") = 0 net change

#### Turn 4: Final Maneuver

**Player Selects: "Find Escape Route"** (Special Choice)
- *You've identified a narrow ravine the bandits aren't watching. With careful timing, you can slip away unnoticed.*
- Tag Effects: +1 Concealment, +2 Analysis, +1 Environment
- New Tag States: 
  - Approach Tags: Concealment 5, Dominance 0, Rapport 0, Analysis 5, Precision 1
  - Focus Tags: Relationship 0, Information 0, Physical 1, Environment 3, Resource 0
- Momentum Gained: 3 + 1 (from "Easily Distracted") = 4
- Pressure: +1 (from "Numerical Advantage") - 1 (from "Superstitious") = 0 net change

#### Final State:
- Momentum: 11 (Partial Success)
- Pressure: 0
- Outcome: Player successfully escapes the bandit ambush without direct confrontation, using environmental awareness and stealth.

### Example Strategic Use of Pressure Choices

Let's walk through a detailed mechanical example that shows why a player would deliberately choose a pressure option:

**Current Player State:**
- Location: Village Market
- Current Base Tags: Rapport 1, Analysis 1, Precision 1, all others at 0
- Current Momentum: 3
- Current Pressure: 2
- Goal: Need to reach Resource 2+ to unlock "Negotiate Better Price" special choice

**Active Location Tags:**
- Narrative Tag: "Open Marketplace" (blocks Concealment approaches)
- Strategic Tag: None yet (needs Rapport 2+ to activate "Merchant's Respect")

**Available Choices in Hand:**
1. **Negotiate Terms** (Charm+Resource, Momentum)
   - Tag Effects: +1 Rapport, +1 Analysis
   - Mechanical Effect: Build 2 momentum
   - Resulting State If Chosen: Rapport 2, Analysis 2, Resource 0, Momentum 5, Pressure 2
   - Strategic Result: Activates "Merchant's Respect" but doesn't build Resource

2. **Overextend Resources** (Charm+Resource, Pressure)
   - Tag Effects: +2 Rapport, +1 Resource
   - Mechanical Effect: Build 2 pressure
   - Resulting State If Chosen: Rapport 3, Resource 1, Momentum 3, Pressure 4
   - Strategic Result: Activates "Merchant's Respect", builds toward Resource 2+

3. **Display of Strength** (Force+Physical, Momentum)
   - Tag Effects: +2 Dominance, -1 Concealment
   - Mechanical Effect: Build 2 momentum
   - Resulting State If Chosen: Dominance 2, Concealment -1, Momentum 5, Pressure 2
   - Strategic Result: Wrong direction for tag strategy, doesn't help with Resource

4. **Environmental Assessment** (Wit+Environment, Momentum)
   - Tag Effects: +2 Analysis, +1 Concealment
   - Mechanical Effect: Build 2 momentum
   - Resulting State If Chosen: Analysis 3, Concealment 1, Momentum 5, Pressure 2
   - Strategic Result: Doesn't help with Resource or activating "Merchant's Respect"

**Player's Strategic Decision:**
The player deliberately chooses **Overextend Resources** (pressure choice) because:

1. **Tag Building Priority**: They need Resource 2+ to unlock the powerful "Negotiate Better Price" special choice, and this is the only option that builds Resource.

2. **Strategic Tag Activation**: By building Rapport from 1 to 3, it activates "Merchant's Respect" strategic tag, which will give +1 momentum to all future Resource choices.

3. **Multi-turn Strategy**: 
   - Turn 1: Choose "Overextend Resources" to reach Rapport 3, Resource 1
   - Turn 2: Choose another Resource-focused choice to reach Resource 2+
   - Turn 3: Unlock and use "Negotiate Better Price" with bonus momentum from "Merchant's Respect"

4. **Pressure Management**: The 2 pressure cost is acceptable because:
   - Current pressure is only 2, so they'll be at 4/10 after this choice
   - Once they activate "Merchant's Respect", future choices will be more efficient
   - Building Resource quickly is worth the short-term pressure increase

This example clearly demonstrates how a pressure choice can be strategically superior to momentum choices when the tag effects are more valuable than the immediate momentum gain.### Strategic Tag Effects on Pressure Choices

Strategic tags can transform pressure choices from risky options into valuable selections:

1. **Pressure Reduction Tags**: Tags like "Innkeeper's Favor" (-1 pressure at end of turn) can offset the pressure cost
2. **Enhanced Momentum**: Tags like "Poorly Coordinated" (+1 momentum to Force approaches) can add momentum to pressure choices
3. **Enhanced Tag Effects**: Some strategic tags enhance the tag effects of certain choices
4. **Tag Synergies**: Strategic combinations of tags can create powerful effects

For example, if a location has the strategic tag "Merchant's Understanding" (Force pressure choices build 1 less pressure), a player might deliberately choose "Escalate Demands" to gain its tag effects with reduced risk.

---

## 10. Implementation Guide

### Development Process

1. **Define Base Tag System**: Implement the 10 base tags as numeric values (0-5)
2. **Create Choice Repository**: Implement all 50 base choices with tag effects
3. **Design Location Templates**: Create several location templates with narrative and strategic tags
4. **Implement Card Selection Algorithm**: Build the logic for selecting 4 choices each turn
5. **Create Tag Activation Logic**: Implement threshold checking for encounter tags
6. **Build Turn Processing**: Handle player selection, tag updates, and tag activation
7. **Add Narrative Layer**: Integrate the appropriate narrative style based on encounter type

### Key Code Components

1. **BaseTagSystem**: Tracks and updates the 10 base tag values
2. **EncounterTagSystem**: Manages activation of narrative and strategic tags
3. **ChoiceRepository**: Contains all available choices with their effects
4. **LocationDefinition**: Defines location properties and available tags
5. **CardSelectionAlgorithm**: Selects appropriate choices for each hand
6. **TurnProcessor**: Handles player choice selection and outcome calculation
7. **NarrativePresenter**: Formats choices according to encounter type

### Testing Recommendations

1. **Tag Progression Test**: Verify tag values update correctly after choices
2. **Tag Activation Test**: Confirm encounter tags activate at correct thresholds
3. **Card Selection Test**: Ensure appropriate diversity in selection algorithm
4. **Full Encounter Test**: Run complete encounters to verify progression and outcomes
5. **Balance Testing**: Check that all approaches and focuses are viable
6. **Hostile Location Test**: Verify hostile locations correctly apply negative effects
7. **Narrative Style Test**: Confirm presentation changes based on encounter type

---

## Conclusion

The Wayfarer Encounter System creates a universal framework for all interactions in the game, replacing traditional specialized systems with an elegant tag-based approach. Key strengths include:

- **Unified Mechanics**: The same system handles social, intellectual, and physical encounters
- **Strategic Tag Management**: Players must balance tag building to activate beneficial strategic tags
- **Risk vs. Reward**: Pressure choices offer valuable tag effects at the cost of increased complications
- **Narrative Integration**: Mechanical systems reflect the evolving narrative state
- **Consistent Framework**: Standardized thresholds and effects make the system easy to learn
- **Scalable Design**: The system works for both friendly and hostile encounters

The system's elegant design comes from its clear separation of responsibilities:
- **Base Layer**: Choices modify tags and build either momentum OR pressure
- **Encounter Layer**: Encounter tags determine all special effects, including pressure reduction
- **Presentation Layer**: The narrative presentation adapts to encounter type

This architecture creates emergent gameplay where strategic choices about which tags to build and when to accept pressure lead to meaningful player agency and varied encounter experiences.