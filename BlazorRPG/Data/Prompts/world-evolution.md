# WORLD EVOLUTION

Determine how the world should evolve in response to the player's choices and actions.

## Context
- Character Background: {{characterBackground}}
- Current location: {{currentLocation}}
- Known locations: {{knownLocations}}
- Known characters: {{knownCharacters}}
- Active opportunities: {{activeOpportunities}}

## Guidelines
1. Identify what interested the player during this conversation
2. Apply "Purpose or Perish" - only include elements that advance the plot OR reinforce tone
3. Create world evolutions maintaining proper references:
   - New location spots (0-2) can only be added to existing locations or new locations
   - New actions (0-3) can only be added to existing spots or new spots
   - New characters (0-2) must be placed at existing locations
   - New locations (0-1) must connect to known locations
   - New opportunities (0-2) must connect to existing locations and characters

## Critical Format Requirements
- Character names must be SIMPLE FIRST NAMES ONLY (e.g., "Giles", not "Giles the merchant")
- ActionNames must use predefined types: VillageGathering, TradeGoods, ForestTravel, etc.
- Action types must be one of: Discuss, Travel, Persuade, Rest, Investigate
- Environmental properties must specifically use these exact values:
  * Illumination: Bright, Shadowy, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * Economic: Wealthy, Commercial, Humble
  * Physical: Confined, Expansive, Hazardous

## Response Format
Respond with ONLY a valid JSON object following the exact structure in the example.

{
  "newLocationSpots": [
    {
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "locationName": "Name of existing location this spot belongs to",
      "actions": [
        {
          "name": "TradeGoods",
          "description": "What this action involves",
          "goal": "The player's goal in this action",
          "complication": "What makes this challenging", 
          "actionType": "Persuade"
        }
      ]
    }
  ],
  "newActions": [
    {
      "spotName": "Name of existing spot this action belongs to",
      "locationName": "Name of the location containing this spot",
      "name": "SecretMeeting",
      "description": "What this action involves",
      "goal": "The player's goal in this action",
      "complication": "What makes this challenging",
      "actionType": "Discuss"
    }
  ],
  "newCharacters": [
    {
      "name": "Giles",
      "role": "Merchant",
      "description": "Brief physical and personality description",
      "location": "Name of existing location where they can be found"
    }
  ],
  "newLocations": [
    {
      "name": "Location name",
      "description": "Brief description",
      "connectedTo": ["First Connected Location", "Second Connected Location"],
      "environmentalProperties": ["Bright", "Crowded", "Commercial", "Chaotic"],
      "spots": [
        {
          "name": "Spot name",
          "description": "Brief description",
          "interactionType": "Character/Shop/Feature/Service",
          "actions": [
            {
              "name": "VillageGathering",
              "description": "What this action involves",
              "goal": "The player's goal in this action",
              "complication": "What makes this challenging",
              "actionType": "Discuss"
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
      "location": "Name of existing location where it takes place",
      "relatedCharacter": "Name of existing character involved"
    }
  ]
}