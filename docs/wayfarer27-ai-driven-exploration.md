# AI Prompting Framework for Wayfarer's Dynamic World Generation

## Layered Prompting Architecture

After examining your example, I've developed a comprehensive prompting framework that integrates seamlessly with Wayfarer's deterministic procedural skeleton. This system employs a layered approach to narrative generation that mirrors the layered structure of the procedural skeleton itself.

### Core Prompt Templates

The framework consists of five specialized prompt types, each designed to generate specific content from the skeleton's semantic tags:

#### 1. Location Experience Prompt

```
Generate a medieval traveler's experience of [location_type: village|town|wilderness|etc.].

LOCATION CONTEXT:
- Name: [node.name]
- Type: [node.type_tag]
- Primary function: [node.function_tag]

ATMOSPHERIC ELEMENTS:
- Time: [current_time_of_day]
- Weather: [current_weather]
- Mood: [node.mood_tag]
- Sensory notes: [node.sensory_tags]

AFFLICTION CONTEXT:
- This location offers [node.resource_tags] for managing [hunger|exhaustion|mental_strain|isolation]
- The quality level is [resource_quality_level]

NARRATIVE ELEMENTS:
- Historical significance: [node.history_tag]
- Current situation: [node.current_event_tag]
- Social atmosphere: [node.social_tag]

Describe this location in vivid detail from a second-person perspective ("you"), emphasizing:
1. Initial visual impression upon arrival
2. Three distinct environmental features that might draw attention
3. Local activity that suggests available interactions
4. Subtle indications of the resources available for affliction management
5. Sensory details (sounds, smells, textures) that reinforce the mood

Write 2-3 paragraphs that immerse the player in this location without explicitly mentioning game mechanics.
```

#### 2. NPC Encounter Prompt

```
Generate an encounter with a [npc.role_tag] in a medieval [location_type].

CHARACTER DETAILS:
- Role: [npc.role_tag]
- Disposition: [npc.disposition_tag]
- Background hint: [npc.background_tag]
- Current state: [npc.current_state_tag]

RELATIONSHIP CONTEXT:
- Current relationship level: [relationship_level]
- Potential services: [npc.service_tags]
- Relationship progression gate: [relationship_next_level_requirement]

NARRATIVE CONTEXT:
- Location atmosphere: [node.mood_tag]
- Local situation: [node.current_event_tag]
- NPC's connection to location: [npc.location_connection_tag]

Create a brief encounter that includes:
1. A vivid physical description emphasizing one distinctive feature
2. Initial greeting or behavior that establishes character
3. A short dialogue snippet (2-3 lines) that hints at:
   - Their disposition toward the traveler
   - Their role in this location
   - A subtle indication of how deeper relationship might be established

Write in second-person present tense with period-appropriate language and dialogue that feels authentic to medieval life without modern colloquialisms.
```

#### 3. Resource Interaction Prompt

```
Generate a [resource_interaction_type: meal|rest|conversation|etc.] experience in a medieval [location_type].

RESOURCE CONTEXT:
- Resource type: [resource.type_tag]
- Quality level: [resource.quality_level]
- Associated affliction: [affliction_type]
- Effectiveness: [resource.effectiveness_value]

ENVIRONMENTAL CONTEXT:
- Location atmosphere: [node.mood_tag]
- Time of day: [current_time_of_day]
- Social setting: [resource.social_context_tag]

NARRATIVE ELEMENTS:
- Cultural aspect: [node.cultural_tag]
- Associated tradition: [resource.tradition_tag]
- Seasonal element: [current_season]

Describe the experience of using this resource in 1-2 paragraphs that include:
1. The sensory experience (tastes, smells, tactile sensations, sounds)
2. The immediate emotional or physical effect on the traveler
3. Social or environmental context surrounding the resource use
4. One subtle detail that distinguishes this particular experience from similar ones elsewhere

The description should evoke the quality level without explicitly mentioning numerical values, and should reflect medieval life authentically.
```

#### 4. Event Unfolding Prompt

```
Generate a [event.type_tag] event occurring in a medieval [location_type].

EVENT CONTEXT:
- Event type: [event.type_tag]
- Trigger: [event.trigger_tag]
- Stakes: [event.stakes_tag]
- Related NPCs: [event.npc_involvement_tags]

PLAYER CONTEXT:
- Current affliction states: [player.affliction_levels]
- Relevant skills: [player.relevant_skill_tags]
- Relationship with involved NPCs: [player.npc_relationship_levels]

NARRATIVE ELEMENTS:
- Environmental factor: [node.environmental_tag]
- Time constraint: [event.time_constraint_tag]
- Underlying tension: [node.tension_tag]

Create a dynamic event unfolding that includes:
1. How the event initially draws the traveler's attention
2. The immediate situation and apparent stakes
3. 2-3 clear potential responses with implied consequences
4. How this event relates to the location's ongoing situation

The event should create natural opportunities for using different approaches (Dominance, Rapport, Analysis, Precision, Evasion) without explicitly naming these game mechanics. Write in present tense with immediate urgency.
```

#### 5. Discovery Moment Prompt

```
Generate a [discovery.type_tag: hidden_resource|local_secret|historical_revelation|etc.] in a medieval [location_type].

DISCOVERY CONTEXT:
- Type: [discovery.type_tag]
- Found at/in: [discovery.specific_location_tag]
- Significance: [discovery.significance_tag]
- Condition: [discovery.condition_tag]

KNOWLEDGE CONTEXT:
- Connects to lore about: [discovery.lore_connection_tag]
- Reveals information about: [discovery.information_subject_tag]
- Historical period: [discovery.historical_era_tag]

GAMEPLAY RELEVANCE:
- Aids with affliction: [discovery.affliction_benefit_tag]
- Unlocks access to: [discovery.unlock_information_tag]
- Reveals potential resource: [discovery.resource_revelation_tag]

Describe the moment of discovery in 1-2 paragraphs that include:
1. The initial observation that leads to the discovery
2. The process of examining or uncovering the discovery
3. What makes this discovery significant or valuable
4. A hint at how this knowledge might be applied

Write from second-person perspective with an emphasis on the traveler's perceptiveness and the satisfaction of discovery. Include one sensory detail that makes this moment memorable.
```

## Integration with Semantic Tag System

Your semantic tag table provides an excellent foundation. I've expanded it to specifically support Wayfarer's core mechanics:

### Affliction Management Tags

These tags directly connect procedural locations to Wayfarer's four affliction systems:

| Tag Category | Example Tags | AI Interpretation |
|--------------|--------------|-------------------|
| hunger_management | simple_rations, hearty_meal, feast_preparation, preserved_food | Generate descriptions of basic sustenance; Describe nourishing, filling food; Detail elaborate food preparation; Mention food designed for travel |
| exhaustion_recovery | outdoor_rest, basic_lodging, comfortable_bed, luxury_accommodations | Describe makeshift rest areas; Generate simple inn room descriptions; Detail quality sleeping arrangements; Create opulent resting quarters experiences |
| isolation_reduction | passing_conversation, tavern_gathering, community_celebration, deep_connection | Brief social moments; Lively social hub activities; Festive community interactions; Meaningful personal exchanges |
| mental_strain_relief | quiet_corner, scenic_viewpoint, meditation_space, scholarly_discussion | Peaceful retreat descriptions; Calming natural beauty; Contemplative atmosphere; Intellectually stimulating exchanges |

### Approach Affinity Tags

These tags help the AI generate content that naturally encourages different approach styles:

| Tag Category | Example Tags | AI Interpretation |
|--------------|--------------|-------------------|
| dominance_opportunity | physical_challenge, authority_test, intimidation_moment, courage_requirement | Situations requiring strength or command; Encounters testing leadership; Confrontations where force might work; Scenarios requiring bravery |
| rapport_opportunity | diplomatic_situation, emotional_appeal, trust_building, charm_opening | Social situations requiring tact; Moments for empathy; Trust-dependent interactions; Opportunities for winning people over |
| analysis_opportunity | puzzle_element, investigation_scene, knowledge_application, observation_test | Intellectual challenges; Mystery elements; Situations requiring education; Details requiring careful notice |
| precision_opportunity | delicate_task, timing_critical, accuracy_needed, careful_approach | Tasks requiring fine control; Time-sensitive actions; Situations requiring exactness; Methodical problem-solving moments |
| evasion_opportunity | stealth_advantage, deception_option, escape_route, hidden_approach | Situations for sneaking; Opportunities for misdirection; Quick exit possibilities; Concealed alternatives |

### Relationship Development Tags

These tags guide the AI in generating content that supports Wayfarer's relationship progression:

| Tag Category | Example Tags | AI Interpretation |
|--------------|--------------|-------------------|
| relationship_requirement | basic_introduction, service_exchange, trust_demonstration, personal_favor | Initial meeting content; Transactional interaction details; Tests of reliability; Significant assistance requests |
| relationship_gate | requires_payment, needs_reputation, demands_specific_skill, expects_evidence | Coin-based progression; Standing-based requirements; Skill check opportunities; Item/information requirements |
| relationship_opportunity | mutual_benefit, shared_enemy, common_interest, personal_connection | Win-win scenarios; Alliance against threats; Interest-based bonding; Emotional connection moments |

## Practical Implementation

To implement this system effectively, I recommend:

### 1. Contextual Concatenation

For any given player interaction, dynamically assemble a comprehensive prompt by combining:

```
[Basic Game State Context]
+ [Relevant Prompt Template]
+ [Location's Tags Formatted for Template]
+ [NPC Tags if relevant]
+ [Player State Context]
+ [Consistency Instructions]
```

### 2. Memory Management

Maintain a concise "memory document" that tracks:

- Key locations visited and their significant features
- Important NPCs encountered and relationship status
- Major discoveries and their implications
- Recurring descriptive elements for consistency

This document gets appended to prompts periodically to maintain consistent descriptions and references.

### 3. Deterministic Variation

To achieve consistent but non-repetitive descriptions:

```
variationSeed = hash(global_seed + location_id + visit_count)
```

Include this seed in the prompt with instructions like:
"Using variation seed [variationSeed], generate a slightly different description than previous visits while maintaining core elements."

### 4. Safeguards and Validation

Implement simple validation rules checking that the AI output:

- References the correct tags provided in the prompt
- Maintains medieval terminology (no modern concepts)
- Includes required elements (sensory details, dialogue if requested)
- Avoids contradicting established world facts

## Example Full Prompt

Here's how a complete integrated prompt might look:

```
You are generating content for Wayfarer, a medieval life simulation focusing on resource management and survival.

WORLD CONTEXT:
- Current season: Late autumn
- Recent events: Poor harvest, tensions with neighboring barony
- Player is a traveler seeking shelter for the approaching winter

PLAYER CONTEXT:
- Affliction levels: Hunger (moderate), Exhaustion (high), Mental Strain (low), Isolation (moderate)
- Archetype: Scholar with Analysis and Precision as natural approaches
- Current focus: Finding sustainable food source and adequate lodging

LOCATION CONTEXT:
- Name: Oakvale
- Type: small_farming_village
- Primary function: agricultural_settlement
- Mood: wary_of_strangers
- Current situation: preparing_for_winter_scarcity

AFFLICTION MANAGEMENT:
- This location offers simple_rations (quality 2) for hunger management
- Basic_lodging (quality 1) is available for exhaustion recovery
- Community_gathering (quality 2) opportunities exist for isolation reduction

Generate a medieval traveler's experience of this small farming village.

Describe this location in vivid detail from a second-person perspective ("you"), emphasizing:
1. Initial visual impression upon arrival
2. Three distinct environmental features that might draw attention
3. Local activity that suggests available interactions
4. Subtle indications of the resources available for affliction management
5. Sensory details (sounds, smells, textures) that reinforce the mood

Write 2-3 paragraphs that immerse the player in this location without explicitly mentioning game mechanics.

CONSISTENCY NOTES:
- Descriptions should reflect late autumn (harvest completed, preparation for winter)
- Language should be period-appropriate for medieval setting
- Food should be portrayed as limited but available
- Villagers should be described as cautious but not hostile
```

This approach creates a robust framework for generating rich, contextually appropriate content that directly supports Wayfarer's core mechanics while maintaining the deterministic nature of your procedural skeleton.