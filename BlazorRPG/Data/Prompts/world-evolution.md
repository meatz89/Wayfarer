# WORLD EVOLUTION

Determine how the world should evolve in response to the player's choices and actions.

## Context
- Character Background: {{characterBackground}}
- Current location: {{currentLocation}}
- Known locations: {{knownLocations}}
- Known characters: {{knownCharacters}}
- Active opportunities: {{activeOpportunities}}

## Critical Requirements
1. EVERY new location MUST have at least one spot with at least one action
2. EVERY new location spot MUST have at least one action
3. All actions MUST have name, description, goal, complication, and actionType
4. New locations MUST connect to at least one existing location
5. All elements must have a clear purpose related to player choices

## Evolution Guidelines
- Create 0-2 new location spots at existing locations
- Create 0-3 new actions at existing spots
- Create 0-2 new characters at existing locations
- Create 0-1 new locations with required spots and actions
- Create 0-2 new opportunities linked to existing locations and characters

## Format Requirements
- Character names must be SIMPLE FIRST NAMES ONLY (e.g., "Giles", not "Giles the merchant")
- Action types must be one of: Discuss, Travel, Persuade, Rest, Investigate
- All actions must specify a goal and complication
- Environmental properties must use these exact values:
  * Illumination: Bright, Shadowy, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * Economic: Wealthy, Commercial, Humble
  * Physical: Confined, Expansive, Hazardous

## Response Format
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