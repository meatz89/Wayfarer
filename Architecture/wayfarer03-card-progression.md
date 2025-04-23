# Wayfarer Card-Based Progression System

The Wayfarer card system represents the player's persistent skills and abilities. Unlike approach and focus tags (which are temporary encounter state variables), cards persist between encounters and form the core of character development.

## Motivation

The card-based progression system addresses several design goals:

1. **Meaningful Progression**: Cards represent skills and abilities the player permanently acquires through gameplay.
2. **Strategic Depth**: The system introduces meaningful decisions about which cards to use during encounters.
3. **Character Specialization**: Players develop a collection of cards that define their character's capabilities.
4. **Elegant Integration**: Cards interact with the temporary encounter state (approach/focus tags) without requiring complex persistence of those tags.

## Card Structure

Choice cards are organized into five tiers of increasing power. These represent the player's actual skills:

### Tier 1: Novice Cards
- Basic abilities available to all players
- Simple effects with no special requirements

### Tier 2: Trained Cards
- Standard abilities representing fundamental skills
- More powerful effects than Novice cards

### Tier 3-5: Advanced Cards
- Powerful abilities representing specialized skills
- Have requirements based on player level and skill development
- Provide significant tactical advantages in appropriate situations

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

## Player Development and Progression

Player progression in Wayfarer consists of:

1. Level: Overall character advancement
2. Card Collection: Unlocked skills / abilities that can be used in encounters
3. Relationships: Connections with NPCs (numerical values with defined states)
4. Resources: Money, food, items, and encounter resources (health, concentration, confidence)

Unlike the temporary approach/focus tags used in encounters, these elements persist between encounters and represent actual character development.


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