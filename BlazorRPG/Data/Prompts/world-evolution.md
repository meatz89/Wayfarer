# WORLD EVOLUTION

## HIGHEST PRIORITY INSTRUCTION FOR TRAVEL ENCOUNTERS
If this was a travel encounter ({wasTravelEncounter} = true) with destination "{travelDestination}":
1. YOU MUST set locationChanged = true and newLocationName = "{travelDestination}"
2. YOU MUST create AT LEAST ONE locationSpot AT "{travelDestination}" SPECIFICALLY describing the arrival point
3. YOU MUST create AT LEAST ONE actionDefinition at this arrival spot that represents the player's FIRST POSSIBLE ACTION after arriving
4. FAILURE TO DO THIS WILL BREAK THE GAME AND IS UNACCEPTABLE

## MANDATORY ACTION REQUIREMENT
Every encounter MUST result in at least one new actionDefinition being created
FAILURE TO INCLUDE AT LEAST ONE ACTION WILL BREAK THE GAME

## Context
- Current location: {currentLocation} (Depth: {currentDepth})
- Last hub depth: {lastHubDepth}
- Current player resources: Health {health}/{maxHealth}, Energy {energy}/{maxEnergy}
- Encounter outcome: {encounterOutcome} (Success/Partial/Failure)

## Forward Progression Rules
- New locations MUST have depth = current_depth or current_depth + 1
- Difficulty scales with depth (base difficulty = depth/2 + 1, minimum 1)
- Hub locations should appear every 3-4 depth levels
- If current_depth - lastHubDepth >= 3, YOU MUST CREATE A HUB LOCATION at depth {currentDepth + 1}
- Hub locations require 3+ spots with Rest/Trade/Healing services
- If player health or energy < 30%, include a rest or healing action
- Rest locations should be available every 2 depth levels

## Location Types
- Hub: Major settlement with multiple services (Rest, Trade, Healing)
- Connective: Minor location connecting other locations
- Landmark: Special location with unique encounters
- Hazard: Dangerous area with high risk/reward

## For Failed Encounters:
- Create alternative paths (new action at different spots/locations)
- These must offer different approaches to similar goals

- 
## Known Locations
{knownLocations}

## Known Characters 
{knownCharacters}

## Current Location Spots
{currentLocationSpots}

## Travel Information
- Was this a travel encounter: {wasTravelEncounter}
- Travel destination: {travelDestination}

## Resource Changes
- Extract ANY mention of coin transactions (gains or losses)
- Note inventory items ADDED during the encounter
- Note inventory items REMOVED or USED during the encounter

## CRITICAL INSTRUCTIONS FOR TRAVEL
When creating elements for a travel destination:
1. The FIRST spot you create MUST represent the ARRIVAL POINT (e.g., "Village Gate", "Forest Entrance", "Cave Mouth")
2. The FIRST action you create MUST be available at this arrival point
3. This action should represent the player's first interaction with the new location
4. The arrival spot and action MUST BE TAGGED with the EXACT destination location name

## Relationship Changes
- Identify ALL character relationship changes suggested in the narrative
- ONLY include characters explicitly named in the narrative
- For EACH character the player interacted with, determine if the relationship improved or worsened
- Look for explicit statements about relationship changes
- Also infer changes from:
  * Player helping a character (+1 or +2)
  * Character providing valuable assistance to player (+1)
  * Player completing a task for a character (+1 or +2)
  * Player failing to help a character when promised (-1)
  * Player actions that upset or harm a character's interests (-1 or -2)
  * Player deceiving or betraying a character (-2)
- Indicate relationship change as a positive or negative integer from -3 to +3

## World Evolution Focus
- Extract only the MOST SIGNIFICANT world elements (maximum 0-2 of each type)
- Focus on elements that enable FUTURE GAMEPLAY, not decorative details
- Prioritize elements directly connected to the encounter outcome
- IF SUCCESS: Create elements that advance the player's goals
- IF FAILURE: Create alternative paths to achieve similar goals
## Location Requirements
- Choose appropriate environmental properties (Bright/Shadowy/Dark, Crowded/Quiet/Isolated, etc.)
- Determine logical connections to other location types

## JSON VERIFICATION FOR TRAVEL ENCOUNTERS
If wasTravelEncounter = true, your response MUST include:
1. playerLocationUpdate.locationChanged = true
2. playerLocationUpdate.newLocationName = "{travelDestination}"
3. At least one locationSpot with locationName = "{travelDestination}"
4. At least one actionDefinition with locationName = "{travelDestination}" and spotName matching a created spot

## Format Requirements
- Character names: SIMPLE FIRST NAMES ONLY (e.g., "Giles", not "Giles the merchant")
- All actions require goal and complication fields
- Action Type must only be one of:
  * Travel, Rest, Labor, Gather, Fight (physical actions)
  * Discuss, Persuade, Perform (social actions)
  * Study, Investigate, Analyze (intellectual actions)
- Environmental properties must use standard values only:
  * Illumination: Bright, Shadowy, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * Economic: Wealthy, Commercial, Humble
  * Physical: Confined, Expansive, Hazardous

## FINAL VERIFICATION STEPS
Before submitting your response for a travel encounter:
1. Verify playerLocationUpdate.newLocationName EXACTLY matches "{travelDestination}"
2. Verify at least one locationSpot exists with locationName EXACTLY matching "{travelDestination}"
3. Verify at least one actionDefinition has:
   - locationName EXACTLY matching "{travelDestination}"
   - spotName matching one of your created spots
   - a meaningful description of the first possible interaction at this location
   
## Response Format
You must provide your response as a valid JSON object with the following structure:

{
  "playerLocationUpdate": {
    "newLocationName": "Name of location player is now at (if changed)",
    "locationChanged": true/false
  },
  "resourceChanges": {
    "coinChange": 5,
    "itemsAdded": ["ItemName1", "ItemName2"],
    "itemsRemoved": ["ItemName3"]
  },
  "relationshipChanges": [
    {
      "characterName": "Giles",
      "changeAmount": 2,
      "reason": "Brief description of why the relationship changed"
    }
  ],
  "locations": [
    {
      "name": "Location name",
      "description": "Brief description",
      "difficulty": 3,
      "depth": 2,
      "locationType": "Hub/Connective/Landmark/Hazard",
      "availableServices": ["Rest", "Trade", "Healing"],
      "discoveryBonusXP": 20,
      "discoveryBonusCoins": 10,
      "connectedTo": ["First Connected Location", "Second Connected Location"],
      "environmentalProperties": ["Bright", "Crowded", "Commercial", "Chaotic"]
    }
  ],
  "locationSpots": [
    {
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "locationName": "Location name"
    }
  ],
  "actionDefinitions": [
    {
      "name": "ActionName",
      "description": "What this action involves",
      "goal": "The player's goal in this action",
      "complication": "What makes this challenging",
      "actionType": "Discuss",
      "isRepeatable": true/false,
      "energyCost": 1,
      "spotName": "Spot name",
      "locationName": "Location name"
    }
  ],
  "characters": [
    {
      "name": "Giles",
      "role": "Merchant",
      "description": "Brief physical and personality description",
      "location": "Name of existing location where they can be found"
    }
  ],
  "opportunities": [
    {
      "name": "Opportunity name",
      "type": "Quest/Job/Mystery/Investigation",
      "description": "Brief description",
      "location": "Name of existing location where it takes place",
      "relatedCharacter": "Name of existing character involved"
    }
  ]
}