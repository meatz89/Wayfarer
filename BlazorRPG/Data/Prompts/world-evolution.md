# WORLD EVOLUTION

After analyzing the full encounter narrative, determine how the world should evolve based on the outcome.

## Character Context
- Current location: {currentLocation}
- Known locations: {knownLocations}
- Known characters: {knownCharacters}
- Active opportunities: {activeOpportunities}
- Encounter outcome: {encounterOutcome} (Success/Partial/Failure)

## Player Location Changes
- Carefully review the encounter narrative to determine if the player ended at a DIFFERENT LOCATION than where they started
- If player moved to a new location, specify this location name in your response
- If this new location does not exist in the known locations list, you MUST create it with spots and actions

## Resource Changes
- Extract ANY mention of coin transactions (gains or losses)
- Note inventory items ADDED during the encounter
- Note inventory items REMOVED or USED during the encounter
- Only include resources explicitly mentioned in the narrative

## World Evolution Focus
- Extract only the MOST SIGNIFICANT world elements (max 1-2 of each type)
- Focus on elements that enable FUTURE GAMEPLAY, not decorative details
- Prioritize elements directly connected to the encounter outcome
- IF SUCCESS: Create elements that advance the player's goals
- IF FAILURE: Create alternative paths to achieve similar goals

## World Structure Requirements
- EVERY new location MUST have at least one spot
- EVERY new spot MUST have at least one action
- EVERY action must either provide direct benefit or start an encounter
- Location → Spot → Action hierarchy must be maintained
- New elements must connect logically to existing world elements

## Format Standards
- Character names: SIMPLE FIRST NAMES ONLY (e.g., "Giles", not "Giles the merchant")
- Action types: Discuss, Travel, Persuade, Rest, Investigate only
- All actions require goal and complication fields
- Environmental properties must use standard values only:
  * Illumination: Bright, Shadowy, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * Economic: Wealthy, Commercial, Humble
  * Physical: Confined, Expansive, Hazardous

## Response Format
{
  "playerLocationUpdate": {
    "newLocationName": "Name of location player is now at (if changed)",
    "locationChanged": true/false
  },
  "resourceChanges": {
    "coinChange": 5,  // Positive for gains, negative for losses
    "itemsAdded": ["ItemName1", "ItemName2"],  // Items added to inventory
    "itemsRemoved": ["ItemName3"]  // Items removed from inventory
  },
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
      "name": "ActionName",
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
              "name": "ActionName",
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