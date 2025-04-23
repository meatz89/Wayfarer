# Practical Implementation: World Evolution System

## Core Prompts

### 1. World Evolution Prompt

```
Using the encounter narrative below, determine how the world should evolve in response to the player's choices and interests.

ENCOUNTER NARRATIVE:
{{encounterNarrative}}

CHARACTER BACKGROUND:
{{characterBackground}}

CURRENT WORLD STATE:
- Current location: {{currentLocation}}
- Known locations: {{knownLocations}}
- Known characters: {{knownCharacters}}
- Active opportunities: {{activeOpportunities}}

INSTRUCTIONS:
1. Identify what interested the player during this encounter
2. Apply "Purpose or Perish" - only include elements that advance the plot OR reinforce tone
3. Create 0-2 new location spots at the current location
4. Create 0-3 new actions at existing spots
5. Create 0-2 new characters with clear purpose
6. Create 0-1 new locations connected to current ones
7. Create 0-2 new opportunities from player actions

FOR EACH NEW ELEMENT:
- Make it require player interaction
- Connect it to the player's choices or background
- Keep descriptions brief but evocative
- Ensure it has a clear purpose

RESPONSE FORMAT:
Respond with ONLY a valid JSON object following this exact structure:

{
  "newLocationSpots": [
    {
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "actions": [
        {
          "name": "Action name",
          "description": "What this action involves"
        }
      ]
    }
  ],
  "newActions": [
    {
      "spotName": "Existing spot name",
      "name": "Action name",
      "description": "What this action involves"
    }
  ],
  "newCharacters": [
    {
      "name": "Character name",
      "role": "Character's occupation",
      "description": "Brief physical and personality description",
      "location": "Where they can be found"
    }
  ],
  "newLocations": [
    {
      "name": "Location name",
      "description": "Brief description",
      "connectedTo": "Name of location it connects to",
      "environmentalProperties": ["Property1", "Property2"],
      "spots": [
        {
          "name": "Spot name",
          "description": "Brief description",
          "interactionType": "Character/Shop/Feature/Service",
          "actions": [
            {
              "name": "Action name",
              "description": "What this action involves"
            }
          ]
        }
      ]
    }
  ],
  "newOpportunities": [
    {
      "name": "Opportunity name",
      "type": "Quest/Job/Mystery",
      "description": "Brief description",
      "location": "Where it takes place",
      "relatedCharacter": "Character name"
    }
  ]
}
```

### 2. Memory Consolidation Prompt

```
Create a concise memory record of this encounter:

ENCOUNTER NARRATIVE:
{{encounterNarrative}}

INSTRUCTIONS:
Generate a single paragraph (2-3 sentences) that captures:
- Key player decisions
- Critical information discovered
- Significant character interactions
- Major world developments

This memory will be referenced in future encounters.

RESPONSE FORMAT:
Respond with ONLY the memory paragraph, no additional text.
```

## Data Models

### Input Models

```csharp
// Simple input model for World Evolution
public class WorldEvolutionInput
{
    public string EncounterNarrative { get; set; }
    public string CharacterBackground { get; set; }
    public string CurrentLocation { get; set; }
    public string KnownLocations { get; set; }
    public string KnownCharacters { get; set; }
    public string ActiveOpportunities { get; set; }
}

// Simple input model for Memory Consolidation
public class MemoryConsolidationInput
{
    public string EncounterNarrative { get; set; }
}
```

## Implementation Steps

1. **Create the Base Classes**: Start with implementing the data models and service classes.

2. **Set Up AI Integration**: Configure your API connection and test the basic prompt handling.

3. **Implement Post-Encounter Processing**:
   ```csharp
   // Example usage
   var encounterSystem = new EncounterSystem(worldStateManager, evolutionService);
   
   // After encounter completes
   string encounterNarrative = "The full narrative of the encounter...";
   await encounterSystem.ProcessEncounterOutcome(encounterNarrative);
   ```

4. **Add UI Notifications**: Implement simple UI notifications for new discoveries.
