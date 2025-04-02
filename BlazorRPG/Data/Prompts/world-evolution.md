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

## Relationship Changes
- Identify ALL character relationship changes suggested in the narrative
- For EACH character the player interacted with, determine if the relationship improved or worsened
- Look for explicit statements about relationship changes
- Also infer changes from:
  * Player helping a character (+1 or +2)
  * Character providing valuable assistance to player (+1)
  * Player completing a task for a character (+1 or +2)
  * Player failing to help a character when promised (-1)
  * Player actions that upset or harm a character's interests (-1 or -2)
  * Player deceiving or betraying a character (-2)
- Only include characters explicitly named in the narrative
- Indicate relationship change as a positive or negative integer from -3 to +3

## World Evolution Focus
- Extract only the MOST SIGNIFICANT world elements (max 0-4 of each type)
- Focus on elements that enable FUTURE GAMEPLAY, not decorative details
- Prioritize elements directly connected to the encounter outcome
- IF SUCCESS: Create elements that advance the player's goals
- IF FAILURE: Create alternative paths to achieve similar goals

## Location Requirements
- Difficulty Level: {DIFFICULTY} (1-3)
- Choose appropriate environmental properties (Bright/Shadowy/Dark, Crowded/Quiet/Isolated, etc.)
- Determine logical connections to other location types

## Each Spot Must Include:
- Name and description
- InteractionType: Character/Quest/Shop/Feature/Travel
- Position: North/South/East/West/Center
- Initial action possibility (TradeGoods, ForestTravel, RentRoom, etc.)
- EACH SPOT MUST HAVE AT LEAST ONE ACTION

## Action Requirements
- Actions must be either:
  * Direct Actions: Provide immediate benefits with clear costs (time/money/energy)
  * Encounter Actions: Start encounters with specific goals and complications
- Direct examples: Sleep (restores energy, costs time), Purchase (costs money, provides items)
- Encounter examples: Gather Information, Negotiate Price, Search Area

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
- Action Type must only be one of:
  * physical: Travel, Rest, Labor, Gather, Fight
  * social: Discuss, Persuade, Perform
  * intellectual: Study, Investigate, Analyze
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
  "relationshipChanges": [
    {
      "characterName": "Giles",
      "changeAmount": 2,  // Positive for improved relationship, negative for worsened
      "reason": "Brief description of why the relationship changed"
    },
    {
      "characterName": "Marta",
      "changeAmount": -1,
      "reason": "Brief description of why the relationship worsened"
    }
  ],
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
      "difficulty": "Location Difficulty (number)",
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
      "type": "Quest/Job/Mystery/Investigation",
      "description": "Brief description",
      "location": "Name of existing location where it takes place",
      "relatedCharacter": "Name of existing character involved"
    }
  ]
}