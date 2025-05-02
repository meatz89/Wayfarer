# Wayfarer: Core Progression Systems Documentation

## Overview

Wayfarer implements a dynamic progression system where both the player character and the world evolve through gameplay. This document explains the core progression mechanics, clarifying how players navigate the game world and how that world responds to their choices.

## World Structure

### Locations

Locations are the primary nodes in the game world's geography. Each location represents a distinct place that the player can visit.

- **Travel Requirements**: Moving between locations requires:
  - Time (measured in minutes)
  - Energy expenditure
  - Potential risk of encounters during travel
- **Connections**: Locations connect to other locations through logical geographical relationships
- **Examples**: Village, Forest Path, Ancient Ruins, Crown and Rose Inn

### Location Spots

Location spots are specific areas within a location that the player can interact with.

- **Navigation**: Unlike locations, spots can be freely visited without cost once the player is at the location
- **Limitation**: Each location should have 1-4 spots, each representing a distinct area
- **Discovery**: Not all spots at a location may be initially visible; some are discovered through actions and encounters
- **Examples**: Market Stall (in Village), Bar (in Crown and Rose Inn), Clearing (in Forest Path)

### Actions

Actions are specific activities the player can perform at location spots.

- **Visibility**: Each location spot displays its available actions to the player
- **Types**:
  - **Direct Actions**: Provide immediate benefits/costs without encounters (e.g., Sleep, Buy Supplies)
  - **Encounter Actions**: Trigger encounter sequences (e.g., Gather Information, Negotiate, Search Area)
- **Limitation**: Each location spot should have at least 1 action but generally no more than 3-4
- **Examples**: Sleep (at Private Room), Haggle (at Market Stall), Local News (at Bar)

### Encounters

Encounters are interactive scenarios that use the tag-based system to determine outcomes.

- **Trigger**: Most (but not all) actions start encounters
- **Structure**: 3-7 turns where player makes choices that affect approach/focus tags
- **Resolution**: Determined by final momentum vs. success threshold
- **Purpose**: Primary method for unlocking new content and progressing the narrative

## Player Progression

### Resources and States

The player manages several resources that constrain their activities:

- **Energy**: Expended during travel and certain actions
- **Time**: Advances through travel, rest, and certain activities
- **Money**: Used for purchases, services, and some actions
- **Physical Resources**:
  - **Health**: Affected by physical encounters, reduced by pressure
  - **Concentration**: Affected by intellectual encounters, reduced by pressure
  - **Confidence**: Affected by social encounters, reduced by pressure

### Knowledge and Access

Player progression is primarily measured through:

- **Location Discovery**: Expanding the map of known and accessible locations
- **Character Relationships**: Building connections with NPCs that unlock new opportunities
- **Quest Progress**: Advancing through narrative-based quest sequences
- **Equipment and Items**: Acquiring tools that enable new actions or improve existing ones

## Action and Encounter System

### Direct Actions

Some actions provide immediate benefits without triggering encounters:

- **Structure**: 
  ```
  Action: Sleep
  Cost: 8 hours of time
  Benefit: Restore energy
  Location Spot: Private Room (at Crown and Rose Inn)
  Requirement: Must have paid for room
  ```

- **Processing**: Applied immediately, updating player state and advancing time
- **Purpose**: Provide maintenance mechanics and basic gameplay loops

### Encounter Actions

Most substantive actions trigger encounters:

- **Structure**:
  ```
  Action: Local News
  Goal: Gather information about recent events
  Complication: Innkeeper is busy with other patrons
  Type: Rapport Encounter
  Location Spot: Bar (at Crown and Rose Inn)
  ```

- **Processing**: Triggers the tag-based encounter system
- **Purpose**: Primary method for world discovery and narrative progression

### Encounter Resolution

The outcome of encounters determines player and world progression:

- **Success** (Momentum ≥ Threshold):
  - Unlocks new knowledge, locations, characters, or opportunities
  - Advances quest narratives
  - May provide direct rewards (items, money, reputation)

- **Failure** (Pressure too high or turns exhausted):
  - Creates alternative paths to similar goals
  - Introduces new challenges that provide second chances
  - Never results in complete dead-ends

## World Evolution System

### Content Unlocking

The world evolves based on encounter outcomes and player discoveries:

- **New Locations**: Connected to existing ones based on narrative logic
- **New Location Spots**: Added to existing locations as player learns about them
- **New Characters**: Introduced at appropriate locations based on narrative context
- **New Actions**: Become available as player gains knowledge or reputation

### Critical Rules

To maintain playability and logical progression:

1. **Every new location must have at least one spot with at least one action**
2. **Every new location spot must have at least one action**
3. All new elements must have clear purpose related to player's experiences
4. New content should be proportional to encounter significance

### Evolution Types

World evolution follows distinct patterns based on encounter outcomes:

#### After Successful Encounters

When players succeed in encounters, the world evolves to provide direct progress:

- **Direct Progression**: New locations/spots/actions that directly advance player goals
- **Reward Access**: Unlocking beneficial characters, services, or opportunities
- **Knowledge Expansion**: Revealing new information that opens multiple paths forward

#### After Failed Encounters

When players fail encounters, the world evolves to provide alternative paths:

- **Alternative Routes**: New locations/spots/actions that offer different approaches
- **Second Chances**: New characters or opportunities that provide ways to retry
- **Narrative Divergence**: Storylines that adapt to account for the failure

## Encounter Mechanics Detail

### Approach and Focus Tags

During encounters, player choices build temporary tags:

- **Approach Tags** (HOW players tackle challenges):
  - Dominance: Force, authority, intimidation
  - Rapport: Rapport connections, charm, persuasion
  - Analysis: Intelligence, observation, problem-solving
  - Precision: Careful execution, finesse, accuracy
  - Evasion: Precision, hiding, subterfuge

- **Focus Tags** (WHAT players concentrate on):
  - Relationship: Connections with others, social dynamics
  - Information: Knowledge, facts, understanding
  - Physical: Bodies, movement, physical objects
  - Environment: Surroundings, spaces, terrain
  - Resource: Items, money, supplies, valuables

These tags exist ONLY during encounters and reset afterward.

### Core Resources

Encounters track two primary resources:

- **Momentum**: Progress toward the character's goal
  - Increases through effective choices
  - Must reach threshold for success
  - Affected by location's strategic tags

- **Pressure**: Complications, stress, and risk
  - Increases through ineffective choices
  - Causes failure if too high
  - Affects secondary resources (Health/Confidence/Concentration)

### Encounter Flow

1. Player chooses an action at a location spot
2. Encounter begins with a specific goal and complication
3. Each turn, player selects from 4 choices representing different approaches and focuses
4. Choices modify approach/focus tags, momentum, and pressure
5. After 3-7 turns, encounter concludes based on momentum vs. threshold

### Outcome Processing

After encounters conclude:

1. Narrative outcome is presented
2. World evolution is processed
3. Player state is updated
4. New content is integrated into the game world

## Working Example: The Innkeeper's Information

### Initial State
- Player is at **Crown and Rose Inn**
- Has access to the **Bar** location spot
- The **Local News** action is available
- Goal: Learn about a missing merchant

### Action Selection
- Player selects **Local News**
- Triggers a social encounter with the innkeeper

### Encounter
- Player builds momentum through effective Rapport and Analysis approaches
- Reaches sufficient momentum to succeed
- Narrative describes innkeeper revealing key information

### World Evolution
1. A new location **Old Forest Road** is unlocked, connected to the Village
2. A new character **Woodcutter** is added to the Old Forest Road location
3. A new action **Ask About Merchant** is added at the Woodcutter's location spot
4. The player's journal is updated with information about the missing merchant

### Alternative: Encounter Failure
If the player fails the encounter:
1. A new character **Barmaid** might appear at the Bar location spot
2. A new action **Eavesdrop on Patrons** becomes available
3. This provides an alternative path to discover similar information

## Implementation Requirements

1. Location and location spot creation must ensure action availability
2. Each new location must have at least one functional spot with actions
3. World evolution must maintain narrative consistency
4. Player should always have multiple paths forward

## Conclusion

The progression system in Wayfarer creates a dynamic, responsive world that evolves based on player choices. By ensuring that new content maintains proper structure (locations → spots → actions → encounters) and provides both direct progression and alternative paths, the game maintains both narrative coherence and player agency throughout the experience.