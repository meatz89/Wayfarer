# Simplified AI-Driven Discovery System for Wayfarer POC

## Single Location Framework: The Dusty Flagon

For your proof of concept focusing on a single location with expanding depth rather than breadth, I recommend structuring around a medieval inn called "The Dusty Flagon." This provides a contained environment that naturally supports all your core mechanics while remaining extensible.

### Core Concept: Depth Through Discovery

The Dusty Flagon starts with a limited set of accessible spots but grows in complexity as the player:
- Develops relationships with NPCs
- Discovers hidden areas
- Unlocks new interactions through actions
- Manages afflictions effectively

## Deterministic Skeleton Structure

### 1. Spot Definition Layer

```json
{
  "id": "common_room",
  "name": "Common Room",
  "discovery_condition": "initial",
  "tags": [
    "function:social_hub", 
    "atmosphere:lively",
    "isolation_management:basic"
  ]
}
```

Additional spots include:
- Dining Area (hunger management)
- Basic Lodging (exhaustion management)
- Quiet Alcove (mental strain management)
- Innkeeper's Counter (services, information)

Hidden spots (discovered through play):
- Cellar (better resources, some risk)
- Private Room (superior rest)
- Kitchen (food quality improvement)
- Stables (alternative characters, opportunities)

### 2. NPC Definition Layer

```json
{
  "id": "innkeeper",
  "name": "Helena",
  "location_spot": "innkeeper_counter",
  "discovery_condition": "initial",
  "tags": [
    "role:proprietor",
    "disposition:businesslike",
    "service:provides_lodging",
    "story_hook:refugee_past"
  ],
  "relationship_progression": {
    "level_1_requirement": "basic_interaction:3",
    "level_2_requirement": "regular_customer:coins:15",
    "level_3_requirement": "special_task:message_delivery"
  }
}
```

NPCs range from always-present staff to travelers who appear based on deterministic conditions (day count, player actions, spot discoveries).

### 3. Action Definition Layer

```json
{
  "id": "order_meal",
  "location_spot": "dining_area",
  "visibility_condition": "dining_area.discovered == true",
  "availability_condition": "player.coins >= 2",
  "ap_cost": 1,
  "tags": [
    "hunger_management:standard",
    "approach:rapport",
    "quality_level:basic"
  ],
  "outcome_effect": {
    "hunger_reduction": 10,
    "coins_spent": 2
  }
}
```

## Deterministic Discovery Logic

All new content appears through predictable rules:

### 1. Relationship-Based Discovery

```
IF relationship(innkeeper) >= 1 THEN 
  unlock_spot(cellar)
  add_action(special_meal)
```

### 2. Action-Based Discovery

```
IF action_count(help_in_kitchen) >= 3 THEN
  unlock_npc(cook)
  improve_quality(meals)
```

### 3. Time-Based Discovery

```
IF day_count >= 3 AND player_condition(isolation > 50) THEN
  unlock_npc(bard)
  add_special_event(evening_performance)
```

### 4. Exploration-Based Discovery

```
IF explore(common_room) AND random_seeded(player_id + day_count) > 0.7 THEN
  discover_item(strange_key)
  add_action(investigate_locked_door)
```

## Simplified AI Prompt Framework

### 1. Base Location Prompt

```
Generate a description of the Dusty Flagon inn's [SPOT_NAME].

SPOT CONTEXT:
- Function: [FUNCTION_TAG]
- Atmosphere: [ATMOSPHERE_TAG]
- Time: [TIME_OF_DAY]

CHARACTER CONTEXT:
- Present NPCs: [PRESENT_NPC_LIST]
- Current activities: [ACTIVITY_TAGS]

PLAYER CONTEXT:
- Notable afflictions: [SIGNIFICANT_AFFLICTIONS]
- Recent discoveries: [RECENT_DISCOVERIES]

Write 2-3 sentences that establish the immediate sensory experience and highlight 1-2 potential interactions without explicitly mentioning game mechanics.
```

### 2. NPC Interaction Prompt

```
Generate an interaction with [NPC_NAME] at the Dusty Flagon inn.

NPC DETAILS:
- Role: [NPC_ROLE]
- Current activity: [CURRENT_ACTIVITY]
- Relationship level: [RELATIONSHIP_LEVEL]

CONTEXT:
- Location: [CURRENT_SPOT]
- Time: [TIME_OF_DAY]
- Recent events: [RECENT_RELEVANT_EVENTS]

Write a brief exchange that:
1. Shows the NPC's personality through speech and behavior
2. Hints at their function/services
3. Includes one subtle detail that changes based on relationship level
```

### 3. Discovery Moment Prompt

```
Generate a discovery moment for finding [DISCOVERED_ELEMENT] at the Dusty Flagon.

DISCOVERY TYPE:
- Element type: [new_spot|new_npc|hidden_item|secret]
- Found through: [exploration|relationship|special_action]

CONTEXT:
- Current location: [CURRENT_SPOT]
- Player action: [TRIGGERING_ACTION]
- Related characters: [RELATED_NPCS]

Describe this discovery in 2-3 sentences that capture:
1. The initial observation or revelation
2. One distinctive detail about what's discovered
3. A hint at its potential significance or usefulness
```

## Example Discovery Path

To illustrate how this works in practice:

1. **Day 1**: Player has access to basic inn spaces
   - Common Room (social interactions, isolation management)
   - Basic Lodging (simple rest, basic exhaustion management)
   - Dining Area (standard meals, basic hunger management)

2. **Initial Relationship Building**:
   - Player regularly speaks with innkeeper Helena
   - At relationship level 1, Helena mentions storage issues
   - This unlocks "Help Organize Cellar" action

3. **New Spot Discovery**:
   - Helping in cellar reveals hidden corner with herbs
   - AI generates description of discovery
   - Unlocks new "Brew Herbal Tea" action for mental strain
   - Opens relationship path with previously unnoticed herb collector NPC

4. **Resource Quality Improvement**:
   - Building relationship with herb collector
   - Unlocks better quality relaxation options
   - Reveals story thread about local medicinal traditions

5. **Expanding Investigation**:
   - Cellar exploration eventually reveals old sealed door
   - High enough Analysis approach allows investigation
   - Discovers entrance to forgotten storeroom
   - Opens entirely new narrative thread and resource options

## Technical Implementation

For this POC, I recommend:

1. **Central State Object**:
   - Tracks discovered spots, NPCs, actions
   - Maintains relationship levels
   - Records affliction states
   - Uses day count as primary progression metric

2. **Simple Triggers System**:
   - Checks discovery conditions after each action
   - Seeds random checks with player ID + day count for determinism
   - Queues discoveries for dramatic pacing

3. **Minimal AI Integration**:
   - Single prompt type with condition-based modifications
   - Maintains small memory of recent descriptions
   - Focuses on atmosphere and sensory details
   - Minimizes mechanical references

This approach gives you a solid proof of concept that demonstrates your core affliction management, relationship building, and discovery mechanics within a limited scope that can easily expand later.


# Wayfarer POC: The Dusty Flagon - Single Location Discovery System

## Core Structure: The Discovery Flow

The Wayfarer POC focuses on depth rather than breadth, centered around a single medieval inn called "The Dusty Flagon." The system follows a strict deterministic flow:

1. **Location Spot (Level 0)** → Initial One-Time Action → Approach Selection → **Location Spot (Level 1)** → Repeatable Action → XP Gain → **Location Spot (Level 2)** → New One-Time Action → New Location Spot Discovery

### Key Mechanics

- **Location Spots**: Either Features (physical spaces) or Characters (people)
- **Action Types**: Labor, Precision, Persuasion, Rapport, Observation, Contemplation
- **Action Categories**: One-time (choice-based) or Repeatable (resource-generating)
- **Progression**: Location Spot XP → Level Up → New Discoveries

## Location Spot Framework

Every location spot follows this exact pattern:

```
Location Spot (Level 0)
├── One-Time Action (2 mutually exclusive approaches)
    ├── Approach A (may require Skill X or Item Y)
    │   └── Unlocks Repeatable Action A
    └── Approach B (may require Skill Z or Item W)
        └── Unlocks Repeatable Action B

Location Spot (Level 1)
├── Repeatable Action (generates resources + Location XP)
    └── When XP threshold reached: Level Up

Location Spot (Level 2)
├── New One-Time Action (2 mutually exclusive approaches)
    ├── Approach C
    │   └── Unlocks New Location Spot C (Level 0)
    └── Approach D
        └── Unlocks New Location Spot D (Level 0)
```

## Initial Location Spots

The POC begins with two location spots:

### 1. Main Hall (Feature, Level 0)
- Initial One-Time Action: "Survey the Room" (Observation)
  - Approach 1: "Study the Patrons" (requires Observation 1+)
  - Approach 2: "Examine the Architecture" (requires Analysis 1+)

### 2. Innkeeper (Character, Level 0)
- Initial One-Time Action: "Meet the Innkeeper" (Rapport)
  - Approach 1: "Casual Introduction" (requires Rapport 1+)
  - Approach 2: "Formal Greeting" (requires Persuasion 1+)

## Action Definition Examples

### One-Time Action Example
```
{
  "id": "survey_room",
  "type": "one_time",
  "category": "observation",
  "location_spot": "main_hall",
  "name": "Survey the Room",
  "description": "Take a moment to observe the inn's main hall.",
  "approaches": [
    {
      "id": "study_patrons",
      "name": "Study the Patrons",
      "skill_requirement": {"skill": "observation", "level": 1},
      "outcome": {
        "location_level_up": true,
        "unlocks_action": "listen_conversations",
        "narrative": "You notice several distinct groups among the patrons - locals sharing drinks by the hearth, travelers keeping to themselves at corner tables, and what appears to be a merchant discussing something in hushed tones with a cloaked figure."
      }
    },
    {
      "id": "examine_architecture",
      "name": "Examine the Architecture",
      "skill_requirement": {"skill": "analysis", "level": 1},
      "outcome": {
        "location_level_up": true,
        "unlocks_action": "search_features",
        "narrative": "The hall shows signs of multiple reconstructions over the years. The oldest beams appear to be from an original structure at least a century old, while newer stonework suggests recent renovations. You notice unusual carvings in the corner support posts."
      }
    }
  ]
}
```

### Repeatable Action Example
```
{
  "id": "listen_conversations",
  "type": "repeatable",
  "category": "observation",
  "location_spot": "main_hall",
  "name": "Listen to Conversations",
  "description": "Position yourself to overhear discussions among the patrons.",
  "requirements": {
    "location_level_min": 1,
    "unlocked_by_approach": "study_patrons"
  },
  "cost": {
    "ap": 1,
    "energy": 1
  },
  "outcome": {
    "base_resources": {"information": 1, "isolation_reduction": 1},
    "scaling": {"skill": "observation", "resource": "information", "bonus": 1},
    "location_xp": 2,
    "narrative_pool": [
      "You catch fragments of conversation about road conditions to the north.",
      "Two farmers discuss unusual weather patterns affecting their crops.",
      "A traveler mentions strange lights seen in the nearby forest at night."
    ]
  }
}
```

## Complete Discovery Tree Example

This example shows one complete branch of progression:

1. **Main Hall (Level 0)**
   - One-Time: "Survey the Room"
   - Player chooses "Study the Patrons" approach
   - Main Hall reaches Level 1
   - Unlocks Repeatable: "Listen to Conversations"

2. **Main Hall (Level 1)**
   - Player repeatedly performs "Listen to Conversations"
   - Each action grants Information and Location XP
   - Information scales with Observation skill
   - After reaching XP threshold, Main Hall reaches Level 2

3. **Main Hall (Level 2)**
   - New One-Time: "Investigate Unusual Patron"
   - Player chooses "Subtle Observation" approach
   - Unlocks new Location Spot: "Mysterious Traveler" (Character, Level 0)

4. **Mysterious Traveler (Level 0)**
   - One-Time: "Approach Traveler"
   - Player chooses "Casual Greeting" approach
   - Mysterious Traveler reaches Level 1
   - Unlocks Repeatable: "Exchange Travel Stories"

This creates a continuously expanding discovery system within the single Dusty Flagon location.

## AI Implementation

For this simplified POC, the AI's role focuses on three specific functions:

### 1. Narrative Enrichment
The AI generates descriptive text for:
- Initial location spot descriptions
- Narrative outcomes of approaches
- Flavor text for repeatable actions
- Character introductions and responses

Example prompt:
```
Generate a description for a traveler's first impression of a medieval inn's main hall.
Focus on sensory details (sights, sounds, smells) and atmosphere.
Keep the description under 3 sentences and appropriate for a medieval setting.
```

### 2. Discovery Generation
When new location spots are unlocked, the AI generates:
- Names and basic descriptions for new spots
- Initial one-time actions appropriate to the spot
- Logical approaches based on the context
- Narrative justifications for the discovery

Example prompt:
```
Generate a new discoverable location spot within a medieval inn.
The spot should be connected to the main hall and be discovered after the player
notices unusual architectural features. Include:
1. Spot name (simple, memorable)
2. Brief description (1-2 sentences)
3. Initial one-time action name
4. Two approaches for investigating this spot
```

### 3. Conversation Generation
For character interactions, the AI generates:
- Dialogue appropriate to the character's role
- Responses that reflect relationship level
- Conversation options that hint at potential discoveries
- Information relevant to the inn and other NPCs

Example prompt:
```
Create a brief dialogue exchange when a traveler first meets an innkeeper.
The innkeeper should be businesslike but not unfriendly.
Include the innkeeper's greeting, a simple question about accommodations,
and a hint about recent unusual events at the inn.
Keep the dialogue under 4 lines total and appropriate for a medieval setting.
```

## Technical Implementation Considerations

For the POC, the system requires:

1. **State Tracking**
   - Current level of each location spot
   - XP accumulated for each spot
   - Completed one-time actions and chosen approaches
   - Unlocked repeatable actions
   - Discovered location spots

2. **Deterministic Triggers**
   - XP thresholds for each level
   - Clear conditions for all unlocks
   - Skill/item requirements for approaches

3. **Simple AI Integration**
   - Pre-generated narrative elements where possible
   - On-demand generation for new discoveries
   - Minimal context needed in prompts

This framework creates a complete system for a depth-focused single location that fulfills all your requirements while remaining elegantly simple and expandable.