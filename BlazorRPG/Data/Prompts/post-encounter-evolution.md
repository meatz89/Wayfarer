# WORLD EVOLUTION

## CRITICAL TRAVEL DESTINATION HANDLING
- If this was a travel encounter ({wasTravelEncounter} = true):
  1. DO NOT CREATE A NEW LOCATION for destination "{travelDestination}" if it ALREADY EXISTS in "{knownLocations}"
  2. Simply set playerLocationUpdate.locationChanged = true and newLocationName = "{travelDestination}"
  3. If NO SPOTS EXIST at this destination, create new locationSpots with locationName EXACTLY matching "{travelDestination}"
  4. If NO ACTIONS EXIST at this destination, create new actionDefinitions with locationName EXACTLY matching "{travelDestination}" 
  5. ONLY create a new location object if "{travelDestination}" does NOT appear in "{knownLocations}"

## LOCATION DUPLICATION PREVENTION
- BEFORE creating ANY new location, CHECK if its name appears in "{knownLocations}" 
- If it does, DO NOT include it in the "locations" array of your response
- DUPLICATE LOCATIONS WILL BREAK THE GAME

## MANDATORY REQUIREMENTS FOR ALL RESPONSES
- Every response MUST include at least one new actionDefinition
- All actionDefinitions MUST have spotName and locationName that match existing or newly created spots/locations
- FAILURE TO MEET THESE REQUIREMENTS WILL BREAK THE GAME

## Context
- Current location: {currentLocation} (Depth: {currentDepth})
- Last hub depth: {lastHubDepth}
- Current player resources: Health {health}/{maxHealth}, Energy {energy}/{maxEnergy}
- Known locations: {knownLocations}
- Known characters: {knownCharacters}
- Active opportunities: {activeOpportunities}
- Encounter outcome: {encounterOutcome} (Success/Partial/Failure)

## Existing World Context
- Current location spots: 
{currentLocationSpots}

- All known location spots across all locations:
{allKnownLocationSpots}

- All existing actions:
{allExistingActions}

## Travel Information
- Was this a travel encounter: {wasTravelEncounter}
- Travel destination: {travelDestination}

## RESPONSE VALIDATION FOR TRAVEL ENCOUNTERS
Before submitting, manually check that:
1. If "{travelDestination}" appears in "{knownLocations}":
   - "locations" array DOES NOT contain an entry with name="{travelDestination}"
   - At least one locationSpot has locationName="{travelDestination}"
   - At least one actionDefinition has locationName="{travelDestination}"

2. If "{travelDestination}" DOES NOT appear in "{knownLocations}":
   - "locations" array DOES contain an entry with name="{travelDestination}"
   - This location has at least one connected location
   - At least one locationSpot has locationName="{travelDestination}"
   - At least one actionDefinition has locationName="{travelDestination}"

## Forward Progression Rules
- New locations MUST have depth = current_depth or current_depth + 1
- Difficulty scales with depth (base difficulty = depth/2 + 1, minimum 1)
- Hub locations should appear every 3-4 depth levels
- If current_depth - lastHubDepth >= 3, YOU MUST CREATE A HUB LOCATION at depth {currentDepth + 1}
- Hub locations require 3+ spots with Rest/Trade/Healing services
- If player health or energy < 30%, include a rest or healing action
- Rest locations should be available every 2 depth levels

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
  * 
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