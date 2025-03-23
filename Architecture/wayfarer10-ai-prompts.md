# Wayfarer AI Integration: Prompt Templates

This document outlines all AI calls required for the Wayfarer dynamic world progression system, including detailed prompts with placeholders for the information that needs to be provided to the AI.

## 1. Encounter Generation

### Purpose
Generate narrative content when a player enters a location or interacts with a specific spot.

### Prompt Template

```
# Wayfarer Encounter Generation

## Location Context
Name: {{location.name}}
Description: {{location.detailedDescription}}
Current Time: {{timeOfDay}}
Current Environmental Properties: {{currentEnvironmentalProperties}}

## Characters Present
{{#each charactersPresent}}
- {{name}}: {{role}} - {{briefDescription}}
  - Relationship with player: {{relationshipWithPlayer}}
{{/each}}

## Available Opportunities
{{#each availableOpportunities}}
- {{name}} ({{type}}): {{briefDescription}}
{{/each}}

## Player Information
Name: {{player.name}}
Background: {{player.background}}
Notable skills: {{player.topSkills}}
Notable relationships in this area: {{player.keyRelationshipsInArea}}

## Player History with this Location
{{#if playerHasVisitedBefore}}
  {{player.name}} has visited {{location.name}} {{visitCount}} time(s) before.
  Previous interactions:
  {{#each previousInteractions}}
  - {{this}}
  {{/each}}
{{else}}
  This is {{player.name}}'s first visit to {{location.name}}.
{{/if}}

## Instructions
Generate an engaging encounter introduction (2-3 paragraphs) that:
1. Describes the location considering the time of day and current atmosphere
2. Introduces available characters naturally within the scene
3. References the player's history with this location (if any)
4. Subtly hints at possible opportunities
5. Sets up interesting choices for the player

Your response should feel like the opening of an interactive scene in a medieval life simulation. Focus on sensory detail and creating a strong sense of place.

## Response Format
Provide only the narrative text with no additional explanations or meta-commentary.
```

## 2. World Discovery Extraction

### Purpose
Extract mentions of new locations, characters, and opportunities from encounter narratives.

### Prompt Template

```
# Wayfarer World Discovery Extraction

## Context
The following is a narrative from a player encounter in the Wayfarer medieval life simulation. Please identify any new locations, characters, or opportunities that were mentioned or implied, following the strict quality guidelines below.

## Known Entities (Do Not Extract These)
Known Locations: {{knownLocationNames}}
Known Characters: {{knownCharacterNames}}

## Quality Requirements
For an entity to be extracted, it MUST meet ALL of these requirements:

Characters:
- Must have a proper name (not just "female baker" or "guard")
- Must have a defined role or occupation
- Must have at least one distinguishing characteristic or trait
- Must have a clear connection to a location

Locations:
- Must have a proper name (not just "nearby village" or "some ruins")
- Must have a clear geographic relationship to known locations
- Must have at least one distinguishing feature
- Must have a reason for the player to potentially visit

Opportunities:
- Must have a clear objective
- Must connect to at least one established character or location
- Must offer a hint at potential rewards

## Important Limitations
- Extract AT MOST 2 new locations and 2 new characters per encounter
- If more than this number appear, select only the most significant ones based on:
  - Frequency of mention
  - Level of detail provided
  - Importance to the narrative
  - Direct interaction with the player

## Encounter Narrative
{{encounterNarrative}}

## Response Format
Respond with a JSON object containing arrays for new entities only. Do not include any entities that don't meet ALL quality requirements.

```json
{
  "locations": [
    {
      "name": "Location name",
      "description": "Brief description",
      "distinguishingFeature": "What makes this place unique",
      "connectedTo": "Relationship to known locations",
      "reasonToVisit": "Why the player might go here"
    }
  ],
  "characters": [
    {
      "name": "Character's full name",
      "role": "Character's occupation or role",
      "description": "Brief physical and personality description",
      "location": "Where this character can be found",
      "distinguishingTrait": "What makes this character memorable"
    }
  ],
  "opportunities": [
    {
      "name": "Name of quest/job/mystery",
      "type": "Quest/Job/Mystery",
      "description": "Brief description of the opportunity",
      "location": "Where this opportunity takes place",
      "relatedCharacters": ["Characters involved"],
      "potentialReward": "Hint at what the player might gain"
    }
  ]
}
```
```

## 3. Location Development

### Purpose
Develop detailed information for a newly discovered location.

### Prompt Template

```
# Wayfarer Location Development

## Basic Information
Name: {{location.name}}
Current Description: {{location.description}}
Connected to: {{connectedLocationNames}}

## Development Instructions
Develop this location for the Wayfarer medieval life simulation game. Create rich, detailed content that makes this location feel like a real place in a medieval world.

## Required Elements
1. Detailed Description (2-3 paragraphs describing the location's appearance, atmosphere, and character)
2. Brief History (how this place came to be, any notable events)
3. Points of Interest (4-6 specific spots within this location where interesting interactions could occur)
4. Atmosphere at different times of day (how the location changes from morning to night)
5. Environmental Properties (4-6 qualities that affect gameplay, like "Crowded", "Formal", "Shadowy")
6. Time-based Properties (how environmental properties change throughout the day)

## Points of Interest Requirements
Each point of interest must include:
- A specific name
- A clear purpose or function
- A brief description
- A type (Character, Feature, Shop, Service, Opportunity)

## Response Format
Respond with a JSON object containing all the developed elements.

```json
{
  "detailedDescription": "Multi-paragraph description",
  "history": "Brief history of the location",
  "pointsOfInterest": [
    {
      "name": "Name of the spot",
      "type": "Character/Feature/Shop/Service/Opportunity",
      "description": "Brief description",
      "purpose": "What players can do here"
    },
    {...}
  ],
  "atmosphereByTime": {
    "morning": "Description of morning atmosphere",
    "afternoon": "Description of afternoon atmosphere",
    "evening": "Description of evening atmosphere",
    "night": "Description of nighttime atmosphere"
  },
  "environmentalProperties": ["Property1", "Property2", ...],
  "timeProperties": {
    "morning": ["Property1", "Property2", ...],
    "afternoon": ["Property1", "Property2", ...],
    "evening": ["Property1", "Property2", ...],
    "night": ["Property1", "Property2", ...]
  }
}
```
```

## 4. Character Development

### Purpose
Develop detailed information for a newly discovered character.

### Prompt Template

```
# Wayfarer Character Development

## Basic Information
Name: {{character.name}}
Current Description: {{character.description}}
Role: {{character.role}}
Home Location: {{homeLocationName}}

## Development Instructions
Develop this character for the Wayfarer medieval life simulation game. Create a rich, detailed profile that makes this character feel like a real person with depth and personality.

## Required Elements
1. Detailed Physical Appearance (how they look, dress, carry themselves)
2. Personality (traits, mannerisms, quirks)
3. Background (brief history, how they came to their current situation)
4. Daily Routine (what they typically do and where they can be found)
5. Relationship Tendencies (how they generally interact with others)
6. Approach Preferences (which player approaches they respond best to)
7. Knowledge & Resources (what they know or have that might be valuable)
8. Speech Pattern (how they typically express themselves)

## Response Format
Respond with a JSON object containing all the developed elements.

```json
{
  "appearance": "Detailed physical description",
  "personality": "Description of personality traits and behaviors",
  "background": "Character's history and origins",
  "routine": "Character's typical daily activities",
  "relationshipTendencies": "How this character forms relationships",
  "approachPreferences": {
    "dominance": -2 to 2 (negative means responds poorly, positive means responds well),
    "rapport": -2 to 2,
    "analysis": -2 to 2,
    "precision": -2 to 2,
    "concealment": -2 to 2
  },
  "knowledge": ["Things this character knows about"],
  "resources": ["Resources this character has access to"],
  "speechPattern": "Description of how this character speaks"
}
```
```

## 5. Opportunity Development

### Purpose
Develop detailed information for a newly discovered opportunity (quest, job, or mystery).

### Prompt Template

```
# Wayfarer Opportunity Development

## Basic Information
Name: {{opportunity.name}}
Type: {{opportunity.type}}
Description: {{opportunity.description}}
Location: {{locationName}}
Related Characters: {{relatedCharacterNames}}

## Development Instructions
Develop this opportunity for the Wayfarer medieval life simulation game. Create an engaging, detailed challenge that provides meaningful player interaction.

## Required Elements
1. Detailed Description (what this opportunity involves)
2. Background (context for why this opportunity exists)
3. Challenges (what difficulties the player will face)
4. Stages (how this opportunity progresses)
5. Potential Rewards (what the player might gain)
6. Consequences (how this might affect the world)
7. Required Approaches (which player approaches would be most effective)

## Response Format
Respond with a JSON object containing all the developed elements.

```json
{
  "detailedDescription": "Multi-paragraph description",
  "background": "Context for this opportunity",
  "challenges": ["Challenge1", "Challenge2", ...],
  "stages": [
    {
      "name": "Stage name",
      "description": "What happens in this stage",
      "objectiveType": "Investigate/Retrieve/Defeat/Persuade/etc",
      "difficulty": 1-5 scale
    },
    {...}
  ],
  "rewards": {
    "money": suggested amount (0 if none),
    "items": ["Item1", "Item2", ...],
    "relationships": [
      {
        "character": "Character name",
        "change": suggested value change (-100 to +100)
      }
    ],
    "experience": ["Skill1", "Skill2", ...],
    "other": "Other rewards"
  },
  "consequences": "How completing this changes the world",
  "recommendedApproaches": ["Approach1", "Approach2", ...],
  "timeConstraint": "Any time constraints, or 'None'"
}
```
```

## 6. Encounter Choice Generation

### Purpose
Generate choices for player during an encounter.

### Prompt Template

```
# Wayfarer Encounter Choice Generation

## Encounter Context
Location: {{location.name}}
Current Situation: {{currentSituation}}
Characters Present: {{charactersPresentNames}}
Player Goal: {{playerGoal}}

## Strategic Information
Current Momentum: {{momentum}}
Current Pressure: {{pressure}}
Active Strategic Tags:
{{#each activeStrategicTags}}
- {{approach}}: {{effect}} ({{description}})
{{/each}}

Active Narrative Tags:
{{#each activeNarrativeTags}}
- {{name}}: Blocks {{blockedFocus}} focus ({{description}})
{{/each}}

## Required Choices
Generate 4 distinct choices for the player in this situation. Each choice should:
1. Reflect a different approach (Dominance, Rapport, Analysis, Precision, Concealment)
2. Target a specific focus (Relationship, Information, Physical, Environment, Resource)
3. Suggest a narrative action the player could take
4. Feel natural within the current situation

## Response Format
Respond with a JSON array of 4 choice objects:

```json
[
  {
    "name": "Choice name",
    "approach": "Primary approach",
    "focus": "Target focus",
    "effectType": "Momentum/Pressure",
    "description": "Description of the action from player's perspective"
  },
  {...}
]
```
```

## 7. Encounter Outcome Generation

### Purpose
Generate narrative outcomes for player choices during encounters.

### Prompt Template

```
# Wayfarer Encounter Outcome Generation

## Player Choice
Player selected: {{selectedChoice.name}} ({{selectedChoice.approach}} + {{selectedChoice.focus}})
Choice description: {{selectedChoice.description}}

## Encounter Context
Location: {{location.name}}
Time: {{timeOfDay}}
Current Situation: {{currentSituation}}
Characters Involved: {{charactersInvolvedNames}}

## Mechanical Changes (Do not mention directly)
- Momentum: {{oldMomentum}} → {{newMomentum}} ({{momentumChange}})
- Pressure: {{oldPressure}} → {{newPressure}} ({{pressureChange}})
- {{selectedChoice.approach}} increased by {{approachIncrease}}
- {{selectedChoice.focus}} increased by {{focusIncrease}}
{{#if newNarrativeTags}}
- New narrative tag activated: {{newNarrativeTags}}
{{/if}}

## Instructions
Generate a narrative outcome (2-3 paragraphs) that:
1. Describes the result of the player's chosen action
2. Shows how the approach/focus increases manifest narratively
3. Reflects momentum and pressure changes through narrative developments
4. Includes character reactions with psychological depth
5. Sets up the next decision point

## Response Format
Structure your response in two clearly separated sections:
1. OUTCOME: The immediate result of the player's choice
2. NEW SITUATION: The evolved scenario that sets up the next decision

Do not mention mechanical terms like "momentum," "pressure," or "tags" directly in your narrative.
```

## 8. State Change Recommendations

### Purpose
Generate suggested changes to game state after encounters.

### Prompt Template

```
# Wayfarer State Change Recommendations

## Encounter Summary
{{encounterSummary}}

## Player Actions
{{playerActions}}

## Characters Involved
{{#each charactersInvolved}}
- {{name}} ({{role}}) - Current relationship: {{currentRelationship}}
{{/each}}

## Instructions
Based on this encounter, recommend appropriate changes to game state. Consider:
1. How resources should be affected (money, items, health, etc.)
2. How relationships should change based on interactions
3. What skills the player demonstrated and should gain experience in
4. Any world events that should be recorded

## Response Format
Respond with a JSON object containing recommended state changes.

```json
{
  "resourceChanges": {
    "money": amount change (positive or negative),
    "food": amount change,
    "health": amount change,
    "concentration": amount change,
    "confidence": amount change,
    "items": ["Item to add or remove"]
  },
  "relationshipChanges": {
    "CharacterName1": value change (-100 to +100),
    "CharacterName2": value change
  },
  "skillExperienceGained": ["Skill1", "Skill2", ...],
  "suggestedWorldEvents": [
    "Description of event to record"
  ]
}
```
```

## 9. Travel Encounter Generation

### Purpose
Generate encounters that happen during travel between locations.

### Prompt Template

```
# Wayfarer Travel Encounter Generation

## Travel Context
Starting from: {{startLocation.name}}
Traveling to: {{destinationLocation.name}}
Travel method: {{travelMethod}}
Travel time: {{travelTimeMinutes}} minutes
Time of day: {{timeOfDay}}
Terrain: {{terrainType}}

## Player Context
Player Name: {{player.name}}
Resources: Money: {{player.money}}, Food: {{player.food}}
Health: {{player.health}}/{{player.maxHealth}}
Notable Skills: {{player.topSkills}}

## Special Conditions
{{#if dangerousRoute}}This route is known to be dangerous.{{/if}}
{{#if playerFirstTimeOnRoute}}This is the player's first time traveling this route.{{/if}}
{{#if recentWorldEvents}}Recent relevant events: {{recentWorldEvents}}{{/if}}

## Instructions
Generate a travel encounter that:
1. Fits the journey context and terrain
2. Provides meaningful choices for the player
3. Has potential consequences for resources or future opportunities
4. Feels appropriate to the medieval setting

## Response Format
Provide the narrative setup for the travel encounter followed by 2-3 possible player choices.

```json
{
  "encounterDescription": "Description of what happens during travel",
  "choices": [
    {
      "name": "Name of choice",
      "description": "Description of this option",
      "potentialOutcome": "Hint at possible consequences"
    },
    {...}
  ]
}
```
```

## 10. Opportunity Progression

### Purpose
Generate the next stage of an ongoing opportunity (quest, job, or mystery).

### Prompt Template

```
# Wayfarer Opportunity Progression

## Opportunity Context
Name: {{opportunity.name}}
Type: {{opportunity.type}}
Current Status: {{opportunity.status}}
Current Stage: {{currentStage.name}}

## Progress Information
{{progressDescription}}

## Related Entities
Location: {{location.name}}
Characters:
{{#each relatedCharacters}}
- {{name}} ({{role}})
{{/each}}

## Player Context
Player Actions: {{playerActions}}
Current Relationships: {{relevantRelationships}}

## Instructions
Based on the player's progress, generate the next stage of this opportunity. Consider:
1. How the situation evolves based on player actions
2. How involved characters react
3. What new challenges emerge
4. Whether the opportunity advances, concludes, or takes a detour

## Response Format
Respond with a JSON object containing the updated opportunity details.

```json
{
  "updatedDescription": "Updated narrative description",
  "newStatus": "Updated status (In Progress/Completed/Failed)",
  "nextStage": {
    "name": "Name of next stage",
    "description": "What happens in this stage",
    "objective": "What the player needs to do",
    "location": "Where this stage takes place",
    "involvedCharacters": ["Character1", "Character2"]
  },
  "stateChanges": {
    "resourceChanges": {...},
    "relationshipChanges": {...},
    "worldEffects": "How this affects the world"
  }
}
```
```

---

## Implementation Notes

### Critical Quality Controls

1. **Remember to implement extraction limits** - Enforce the maximum of 1-2 new locations and 1-2 new characters per encounter.

2. **Apply entity quality requirements strictly** - Reject any entities that don't meet all quality criteria.

3. **Design fallback mechanisms** - Have default behaviors when AI responses don't meet expected format or quality.

4. **Include context constraints** - Only provide relevant information to the AI to focus its responses.

5. **Parse responses carefully** - Use robust parsing with fallback options when JSON structure is unexpected.

### Information Management

1. **Maintain history summaries** - Keep concise summaries of past interactions rather than full text for context.

2. **Prioritize recent information** - Weight recent interactions more heavily in context.

3. **Localize context** - Only provide information relevant to the current location/situation.

4. **Clarify boundaries** - Make it clear what information is established vs. what needs to be generated.

### Content Quality

1. **Review AI outputs** - Implement review mechanisms for quality assurance.

2. **Maintain consistent style** - Ensure prompts encourage consistent tone and style.

3. **Adjust specificity** - Fine-tune prompt detail level based on observed outcomes.

4. **Handle edge cases** - Design prompts to handle unusual situations gracefully.