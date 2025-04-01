# WORLD EVOLUTION

Determine how the world should evolve in response to the player's choices and actions.

## Context
- Character Background: {characterBackground}
- Current location: {currentLocation}
- Known locations: {knownLocations}
- Known characters: {knownCharacters}
- Active opportunities: {activeOpportunities}
- Encounter Outcome: {encounterOutcome} (Success/Partial/Failure)

## Guidelines
1. Identify player interests during this conversation
2. Apply "Purpose or Perish" - only include elements with clear purpose
3. Character names must be SIMPLE FIRST NAMES ONLY
4. Action types must be standardized (Discuss, Travel, Persuade, Rest, Investigate)
5. Use standard environmental property values

## World Structure Requirements:
- EVERY new location MUST have at least one spot
- EVERY new spot MUST have at least one action
- EVERY action must either provide direct benefit or start an encounter
- Location → Spot → Action hierarchy must be maintained
- If encounter failed, create ALTERNATIVE PATHS to same/similar goals

## Create (as appropriate):
- New location spots (0-2) at existing or new locations
- New actions (0-3) at existing or new spots
- New characters (0-2) at existing locations
- New locations (0-1) connected to known locations
- New opportunities (0-2) linked to existing elements

## For Failed Encounters:
- Create alternative paths (new actions at different spots/locations)
- These must offer different approaches to similar goals
- Should feel like natural world evolution, not artificial second chances

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