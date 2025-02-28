# Wayfarer Encounter System: Revised Rules

## 0. Introduction and Game Context

"Wayfarer" is a medieval life simulation where you play as an ordinary traveler making their way in the world. No epic quests or chosen one narrative - just the authentic challenges of survival, relationships, and finding your place in a grounded medieval setting.

### Core Gameplay Loop
1. Players move between locations (inns, markets, docks, etc.)
2. At each location, they find spots where they can take actions (like the bar in a tavern)
3. Taking an action starts an encounter
4. Each encounter presents choices that determine success/failure
5. Success brings rewards (coins, knowledge, relationships)
6. These rewards enable accessing new locations and opportunities

### Design Philosophy
The encounter system represents a fresh approach to RPG interactions that:
- Replaces traditional combat mechanics with universal interaction rules
- Uses location properties to function like unique "enemy AI patterns"
- Creates authentic medieval challenges through mechanical systems
- Forces players to "read the room" like in real social situations
- Provides clear rules that allow for intentional strategy

## 1. Core Encounter Objectives and Structure

### 1.1. Success and Failure Conditions
- **Encounter Duration**: Each location specifies a fixed number of turns for encounters
- **Momentum Thresholds**:
  - 0-7 momentum = Failure (no reward)
  - 8-9 momentum = Partial success (minor reward)
  - 10-11 momentum = Standard success (normal reward)
  - 12+ momentum = Exceptional success (bonus reward)
- **Failure Condition**: Reach exactly 10 pressure to fail the encounter
- **Encounter Continuation**: Encounters always continue for their full duration unless 10 pressure is reached

### 1.2. Focus System
- **Starting Focus**: All encounters begin with 0 Focus
- **Focus Range**: Focus can range from 0 to 5
- **Focus Building**: Focus is gained through multiple approach characteristics
- **Focus Application**: Focus can be spent based on approach characteristics
- **Focus Preservation**: When Focus reaches 5, any further Focus gained is converted to -1 pressure
- **Focus returns to 0 when the encounter ends**

### 1.3. Encounter Core Without Tags
- **The encounter system functions completely without tags as a core strategic puzzle**
- **This core system consists of choice selection, state management, and Focus balancing**
- **Players can successfully navigate encounters using only the core mechanics**
- **Tags provide additional strategic depth but are not required for the core system to function**

### 1.4. Choice Structure
- **Four Choices**: Every turn presents exactly four choices
- **Single Decision**: Players select one choice per turn with no additional decisions
- **Choice Components**: Each choice has exactly:
  - One Action characteristic (what it accomplishes)
  - One Approach characteristic (how it functions)
  - A current State (Ready or Tired) determining effectiveness
- **Choice Effects**: Effects are determined entirely by characteristics and state
- **No Sub-choices**: Choices never present "OR" options or require additional selections

## 2. Choice Characteristics and Effects

### 2.1. Action Characteristics
Each action characteristic defines exactly what resource the choice affects, with effectiveness determined by state:

- **Develop**: Generate momentum
  - Ready State: Gain exactly 2 momentum.
  - Tired State: Gain exactly 1 momentum.

- **Prepare**: Generate Focus
  - Ready State: Gain exactly 1 Focus.
  - Tired State: Gain exactly 1 Focus (with a maximum cap of 3 instead of 5).

- **Stabilize**: Manage pressure
  - Ready State: Reduce pressure by exactly 1 point.
  - Tired State: Prevent pressure increase for one turn.

- **Intensify**: High impact effects with risk
  - Ready State: Gain exactly 3 momentum, add exactly 1 pressure.
  - Tired State: Gain exactly 2 momentum, add exactly 1 pressure.

### 2.2. Approach Characteristics
Each approach characteristic defines exactly how the action functions, with options determined by state:

- **Direct**: Resource amplification through Focus
  - Ready State: Automatically spend 1 Focus for +1 to primary effect (if available).
  - Tired State: No Focus spending occurs.

- **Flexible**: Resource conversion with Focus generation
  - Ready State: Convert 1 pressure to 1 of the Action's primary resource. Gain 1 Focus.
  - Tired State: No conversion effect. Gain 1 Focus.

- **Thorough**: State preservation with Focus generation
  - Ready State: This characteristic doesn't become Tired after use. Gain 1 Focus.
  - Tired State: No protection effect. Gain 1 Focus.

- **Perceptive**: State manipulation with Focus utility
  - Ready State: Improves exactly one characteristic following priority order: 1) The most recently tired characteristic, 2) A characteristic that shares a choice with the current selection, 3) The location's Protected Action characteristic. Additionally, if Focus is available, automatically spends 1 Focus to reduce pressure by 1.
  - Tired State: No improvement effect. No Focus spending occurs.

## 3. Characteristic State Progression

### 3.1. State Degradation
When a characteristic is used, its state changes according to these exact rules:
- **Ready State** → **Tired State**: Any characteristic used while in Ready State immediately becomes Tired for the next turn, unless protected.
- **Tired State** remains **Tired State**: A characteristic used while already in Tired State stays Tired.

Exception: Characteristics with the Thorough approach in Ready state don't become Tired when used.

### 3.2. State-Based Protection
Protected characteristics have special degradation rules:
- Protected characteristics never become Tired when used
- Each location defines one protected Action and one protected Approach
- Resonant characteristics (from environmental factors) always function as if Ready even when Tired
- Dissonant characteristics (from environmental factors) always function as if Tired even when Ready

### 3.3. State Improvement
States improve through these specific mechanisms:
- **Memory-Based Recovery**: Any characteristic in Tired State that is NOT used for exactly one complete turn automatically improves to Ready state in the following turn.
- **Choice-Based Improvement**: Choices with the Perceptive approach in Ready state can improve the state of other characteristics when selected.
- **Improvement Target Selection**: Automatic improvement follows this priority:
  1. First, improve the most recently tired characteristic
  2. If no characteristic was recently tired, improve the characteristic that shares a choice with the current characteristic
  3. If neither applies, improve the characteristic with the location's Protected Action

## 4. Pressure System

### 4.1. Pressure Accumulation
Pressure accumulates through these exact rules:
- **Base Accumulation**: +1 pressure is automatically added at the end of every turn
- **Characteristic Modifiers**:
  - Each location's audience property defines which two characteristics add +1 pressure when used
  - Each location's audience property defines which two characteristics reduce pressure by 1 when used
- **Minimum Pressure**: Pressure can never be reduced below 0
- **Tired States**: Choices using a characteristic in Tired state generate +1 additional pressure when used
- **Order of Operations**:
  1. First apply choice-specific pressure relief (-1 from specified characteristics)
  2. Then apply choice-specific pressure increases (+1 from specified characteristics)
  3. Then apply the additional pressure from Tired state
  4. Finally apply the base +1 pressure for the turn

### 4.2. Pressure Effects on Momentum
Pressure affects momentum gains through a linear reduction:
- Each point of pressure reduces all momentum gains by 1 (minimum gain of 1)
- This reduction applies after all other calculations
- At 10 pressure, the encounter fails

## 5. Location Properties as Adversaries

### 5.1. Location Definition Components
Each location defines exactly:
- **Available Choices**: Which four specific Action+Approach pairs are available
- **Environmental Factors**: Which characteristics are resonant/dissonant (two of each)
- **Audience Property**: Which characteristics generate/reduce pressure (two of each)
- **Protected Characteristics**: One Action and one Approach that never become Tired when used
- **Time Slots**: Available times of day with varying parameters
- **Encounter Duration**: The fixed number of turns for encounters at this location
- **Momentum Thresholds**: The exact thresholds for different success tiers (if different from standard)

### 5.2. Location Examples

#### Harbor Tavern
- **Available Choices**:
  1. Develop + Direct
  2. Prepare + Thorough  
  3. Stabilize + Flexible
  4. Intensify + Perceptive
- **Environmental Factors**:
  - Noisy: Direct is resonant, Thorough is dissonant
  - Crowded: Flexible is resonant, Prepare is dissonant
- **Audience Property**: Public (Intensify and Direct generate pressure; Develop and Thorough reduce it)
- **Protected Characteristics**: Develop (Action), Flexible (Approach)
- **Time Slots**:
  - Evening (6 turns, thresholds: 9/11/13)
  - Night (5 turns, thresholds: 8/10/12)
- **Momentum Thresholds**: 0-8 (Fail), 9-10 (Partial), 11-12 (Standard), 13+ (Exceptional)

#### Merchant Guild
- **Available Choices**:
  1. Develop + Thorough
  2. Prepare + Direct
  3. Stabilize + Flexible
  4. Intensify + Perceptive
- **Environmental Factors**:
  - Formal: Thorough is resonant, Intensify is dissonant
  - Private: Perceptive is resonant, Direct is dissonant
- **Audience Property**: Semi-Public (Prepare and Perceptive generate pressure; Develop and Thorough reduce it)
- **Protected Characteristics**: Develop (Action), Thorough (Approach)
- **Time Slots**:
  - Morning (5 turns, standard thresholds)
  - Afternoon (6 turns, higher thresholds)
- **Momentum Thresholds**: 0-8 (Fail), 9-11 (Partial), 12-14 (Standard), 15+ (Exceptional)

## 6. System Reaction Sequence

### 6.1. Turn Sequence
Each turn follows this exact sequence:
1. **Player Choice Selection**: Player selects one of the four available choices
2. **Synergy Processing**:
   - If the choice shares exactly one characteristic with previous turn's choice, gain +1 Focus
   - The bonus applies only when exactly one characteristic matches (not both)
   - The selected choice cannot be identical to the previous turn's choice
3. **Initial Effect Resolution**: 
   - System applies the base effect of the choice based on its Action, Approach, and State
   - Effects resolve in this order: momentum changes, Focus changes, state changes, pressure changes
4. **Focus Application**:
   - System applies any Focus effects based on the Approach characteristic
   - Focus is spent according to the Approach's rules
   - If Focus would exceed 5, convert excess to -1 pressure per point
5. **State Degradation Processing**:
   - System identifies which characteristics are eligible for degradation
   - Applies state changes one at a time
   - Protected and resonant characteristics don't become Tired
6. **Pressure Accumulation**:
   - Applies pressure modifications in the specified order of operations
7. **Preparation for Next Stage**:
   - Updates all characteristic states and resources
   - Performs victory and failure checks

## 7. Encounter Tags System (Extension to Core System)

### 7.1. Tag Independence
- **Independent Entities**: All encounter tags are independent entities with no inherent relationships
- **No Tag Pairs**: There are no "positive/negative pairs" of tags; each tag is a distinct entity
- **Tag Variety**: Each location has its own specific set of possible tags
- **Multi-Characteristic Effects**: Multiple tags can affect the same choice through its different characteristics

### 7.2. Tag Effects
Each tag affects exactly ONE characteristic in a specific way:

- **Negative Tags** modify choices to create challenges:
  - Some lock characteristics in Tired state
  - Some add pressure costs to choices
  - Some disable specific effects of choices
  - Some prevent certain characteristics from functioning normally

- **Positive Tags** enhance choices:
  - Some improve characteristic states automatically
  - Some reduce pressure costs
  - Some enhance Focus effects
  - Some provide bonus effects to specific characteristics

### 7.3. Tag Addition and Location Triggers
Each location defines exactly three trigger rules:
- **Primary Trigger**: When a choice with this characteristic is made, add the specified negative tag
- **Counter Trigger**: When a choice with this characteristic is made, remove the specified negative tag. When a choice with this characteristic is made while the specified negative tag is not present, add the specified positive tag

### 7.4. Tag Duration and Management
- **Tag Persistence**: All tags remain active until explicitly removed
- **Tag Removal**: Negative Tags can be replaced only through specific choice effects

## 8. Design Implementation Notes

### 8.1. Core System First
The implementation should:
- First build and test the core system without tags
- Ensure the base system creates meaningful strategic choices
- Verify that different location properties create distinct strategic puzzles
- Only add the tag system after the core mechanics are solid

### 8.2. System Boundaries and Integration
This system is designed to work as a complete self-contained module with these characteristics:
- **Deterministic**: Every state change can be predicted with complete certainty
- **Bounded**: Clear entry and exit conditions (momentum/pressure thresholds)
- **Systematic**: All mechanics follow consistent, rule-based patterns
- **Coherent**: Mechanics align with narrative concepts they represent

### 8.3. Learning Curve and Player Mastery
The system rewards player mastery through:
- **Learning location behaviors**: Each location has consistent, predictable patterns
- **Strategic planning**: Managing characteristic states for multi-turn strategies
- **Resource balancing**: Trading between momentum gain and pressure management
- **Focus optimization**: Building and spending Focus at optimal moments
- **Choice sequencing**: Planning turns to benefit from the Choice Synergy system

### 8.4. Default Initial Configuration
For implementation purposes, these defaults should be used:
- **Default States**: All characteristics start in Ready State unless specified otherwise
- **Default Focus**: All encounters start with 0 Focus
- **Default Pressure**: All encounters start with 1 Pressure
- **Default Choice Distribution**: Each location defines its own specific set of four choices
- **Default Duration**: 5 turns unless specified otherwise by the location
- **Default Thresholds**: 0-7 (Fail), 8-9 (Partial), 10-11 (Standard), 12+ (Exceptional)

### 8.5. Encounter Flow Diagram
The encounter system follows this flow:

```
Current Stage → Choice Selection → Synergy Processing → Effect Resolution → 
Focus Application → State Update → Pressure Accumulation → End Turn Checks → Next Stage
```

This definition creates a self-contained "game board" for the encounter, with all possible states and transitions clearly specified. The system is elegant in its rules while creating rich strategic depth through the interactions of its simple components.