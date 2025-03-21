# Wayfarer Card-Based Progression System

## Overview

The Wayfarer encounter system has been refined with a card-based progression system that transforms choices into tiered ability cards of varying power levels. This system creates a meaningful progression path for character development while introducing strategic trade-offs between immediate power and long-term potential.

## Motivation

The card-based progression system addresses several design goals:

1. **Meaningful Progression**: Rather than all choices being equally powerful, the tiered card system creates a progression path where characters unlock more powerful abilities as they develop.

2. **Strategic Depth**: The system introduces meaningful decisions about when to use powerful cards versus conserving approach/focus values for future turns.

3. **Character Specialization**: The requirement system encourages specialization in specific approaches and focuses, creating distinct character builds.

4. **Elegant Integration**: The system works seamlessly with existing strategic and narrative tag mechanics, creating rich interactions without adding unnecessary complexity.

## Tiered Card Structure

Choices are now organized into five tiers of increasing power:

### Tier 1: Novice Cards
- **Momentum Type**: +0 momentum, +1 primary approach, +1 focus
- **Pressure Type**: -0 pressure, +1 primary approach, +1 focus
- **Requirements**: None
- **Purpose**: Build approach/focus values to unlock better cards

### Tier 2: Trained Cards
- **Momentum Type**: +2 momentum, +2 primary approach, +1 focus
- **Pressure Type**: -1 pressure, +1 primary approach, +1 focus
- **Requirements**: None (or minimal)

### Tier 3: Adept Cards
- **Momentum Type**: +3 momentum, +2 approach, +1 focus, -1 required approach/focus
- **Pressure Type**: -2 pressure, +1 approach, +1 focus, -1 required approach/focus
- **Requirements**: Either approach 3+ OR focus 2+ (single requirement only)

### Tier 4: Expert Cards
- **Momentum Type**: +4 momentum, +2 approach, +1 focus, -2 required approach/focus
- **Pressure Type**: -3 pressure, +1 approach, +1 focus, -2 required approach/focus
- **Requirements**: Either approach 5+ OR focus 3+ (single requirement only)

### Tier 5: Master Cards
- **Momentum Type**: +5 momentum, +3 approach, +1 focus, -3 required approach/focus
- **Pressure Type**: -4 pressure, +2 approach, +1 focus, -3 required approach/focus
- **Requirements**: Either approach 8+ OR focus 5+ (single requirement only)

## Core Mechanics

### 1. Requirement System

Higher-tier cards (Tier 2+) have requirements that must be met to use them:

- Each card requires EITHER a minimum approach value OR a minimum focus value (never both)
- Approach requirements are generally higher (3/5/8) than focus requirements (2/3/5) since focuses are harder to build
- Cards cannot be selected if their requirements are not met
- Requirements create natural progression paths as players develop their character

### 2. Permanent Reduction Cost

The key innovation of this system is that higher-tier cards come with a cost: they permanently reduce the required approach or focus when used:

- Tier 3 cards reduce the required approach/focus by 1
- Tier 4 cards reduce the required approach/focus by 2
- Tier 5 cards reduce the required approach/focus by 3

This creates significant strategic considerations:
- Using a powerful card provides immediate benefit but sacrifices future potential
- After using high-tier cards, players must rebuild their approach/focus values using lower-tier cards
- Players must carefully consider when to "spend" their high values on powerful effects

### 3. Environmental Property-Based Strategic Tag System

The system now uses dynamic environmental properties to create strategic tags that cards can synergize with:

#### Environmental & Situational Properties

Strategic tags are based on properties that can be combined to create varied environments:

**Illumination Properties**
- **Bright** - Well-lit areas with clear visibility
- **Shadowy** - Areas with mixed lighting and many shadows
- **Dark** - Poorly lit or nighttime environments

**Population Properties**
- **Crowded** - Densely populated areas with many people
- **Quiet** - Sparsely populated with few observers
- **Isolated** - Completely private or secluded environments

**Atmosphere Properties**
- **Tense** - High-pressure, stressful environments
- **Formal** - Rule-bound, ceremonial, or structured settings
- **Chaotic** - Unpredictable, disordered environments

**Economic Properties**
- **Wealthy** - Affluent areas with valuable resources
- **Commercial** - Trading environments with economic activity
- **Humble** - Simple environments with limited resources

**Physical Properties**
- **Confined** - Tight spaces with limited movement
- **Expansive** - Open areas with room to maneuver
- **Hazardous** - Physically dangerous environments

#### Card-Property Synergies

Cards now include conditional effects based on environmental properties:

- Each card may have synergy effects with specific properties
- These effects only activate when the corresponding strategic tag is present
- The same card may have different value depending on the location's properties
- Cards can synergize with properties regardless of their approach/focus

#### Location Design

Locations combine multiple properties to create unique strategic environments:

- The same location can have different properties based on time of day, weather, or events
- Location properties activate corresponding strategic tags
- Players can strategize around visiting locations at optimal times
- Property combinations create distinct strategic landscapes for each encounter

### 4. Narrative Tag Interactions

Narrative tags create additional strategic considerations for the card system:

- When narrative tags activate and block focus choices, they potentially block powerful high-tier cards
- Using high-tier cards that reduce approach values might deactivate narrative tags, creating a benefit
- This creates a push-pull dynamic where high approach values enable powerful cards but might trigger limiting tags

## Character Development and Progression

Characters begin with access to Tier 1 and possibly Tier 2 cards. Through character development:

1. **Approach Building**: As players increase approach values, they unlock more powerful approach-requirement cards
2. **Focus Development**: As players invest in specific focuses, they unlock specialized focus-requirement cards
3. **Strategic Choices**: Players face meaningful decisions about which approaches or focuses to prioritize

## Gameplay Impact

The card-based progression system transforms gameplay in several ways:

1. **Power Curve**: Players experience a noticeable power increase as they unlock higher-tier cards
2. **Strategic Cycles**: Gameplay follows a pattern of building approach/focus values followed by "spending" them on powerful effects
3. **Specialization**: Players naturally specialize in approaches and focuses that unlock their preferred high-tier cards
4. **Card Collection**: Character development is represented by an expanding collection of available cards

## Integration with Existing Systems

The card system integrates seamlessly with existing Wayfarer mechanics:

- **Base Tag System**: Approach and focus tags remain fundamental to character capabilities
- **Encounter Layer**: Strategic and narrative tags interact naturally with the card system
- **Presentation Layer**: Cards are visually distinguished by tier with appropriate color-coding
- **Card Selection Algorithm**: The algorithm prioritizes higher-tier cards when requirements are met

## Dynamic Environment System

The property-based strategic tag system creates a living world where locations change based on time and circumstances:

### Example Location Combinations

**Market Square:**
- **Morning**: Bright + Crowded + Commercial + Chaotic
- **Afternoon**: Bright + Crowded + Commercial + Tense
- **Evening**: Shadowy + Crowded + Commercial + Chaotic
- **Night**: Dark + Quiet + Commercial + Tense

**Forest Path:**
- **Day**: Bright + Isolated + Expansive + Hazardous
- **Dusk**: Shadowy + Isolated + Expansive + Tense
- **Night**: Dark + Isolated + Expansive + Hazardous

### Property Variation

The same location can have different properties based on:
- Time of day
- Weather conditions
- Special events (festivals, emergencies)
- Player actions in previous encounters
- Season of the year

This creates a dynamic world where timing and context matter, encouraging players to revisit locations under different conditions.

## Design Rationale

The card-based progression system with environmental properties achieves several design goals:

1. **Depth Through Simplicity**: Creates strategic depth through the interaction of simple systems
2. **Meaningful Choices**: Every decision about which card to play has both immediate and long-term impacts
3. **Character Identity**: Card collections create distinct character identities and playstyles
4. **Natural Learning Curve**: Players master basic cards before gradually accessing more complex options
5. **Emergent Strategy**: The interaction between cards, properties, and narrative tags creates emergent strategic depth
6. **Dynamic World**: The property system creates a living world that changes with time and circumstances
7. **Contextual Value**: Cards vary in usefulness based on environmental properties, creating situational strategy

The system preserves what made the original Wayfarer system elegant—unified mechanics, tag-based interactions, and strategic depth—while adding a meaningful progression system that rewards character development with increased power and strategic options. The property-based strategic tag system adds another layer of depth, making the world feel alive and creating rich strategic possibilities that work with any approach or focus.