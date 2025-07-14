# WORLD EVOLUTION

## LOCATION DUPLICATION PREVENTION
- BEFORE creating ANY new location, CHECK if its name appears in "{knownLocations}" 
- If it does, DO NOT include it in the "locations" array of your response
- DUPLICATE LOCATIONS WILL BREAK THE GAME

## MANDATORY REQUIREMENTS FOR ALL RESPONSES
- Every response MUST include at least one new actionDefinition
- All actionDefinitions MUST have spotName and locationName that match existing or newly created spots/locations
- FAILURE TO MEET THESE REQUIREMENTS WILL BREAK THE GAME

## Context
- Current location: {currentLocation}
- EncounterContext outcome: {encounterOutcome} (Success/Partial/Failure)

## Existing World Context

- All known locations:
{allKnownLocations}
 
- Connected locations:
{connectedLocations}

- Current location spots: 
{currentLocationSpots}

- All existing actions:
{allExistingActions}

- All Known characters: 
{knownCharacters}

- Active contracts: 
{activeContracts}

## Location Types
- Hub: Major settlement with multiple services (Rest, Trade, Healing)
- Connective: Minor location connecting other locations
- Landmark: Special location with unique encounters
- Hazard: Dangerous area with high risk/reward

## Format Requirements
- Character names: SIMPLE FIRST NAMES ONLY (e.g., "Giles", not "Giles the merchant")
- All actions require a known location spot, a goal
- Action Type must only be one of:
  * Rest, Labor, Gather, Fight (physical actions)
  * Discuss, Persuade, Perform (social actions)
  * Study, Investigate, Analyze (intellectual actions)
- Environmental properties must use standard values only:
  * Illumination: Bright, Thiefy, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * : Wealthy, Commercial, Humble
  * Physical: Confined, Expansive, Hazardous
 
## Response Format
You must provide your response ONLY as a valid JSON object with the following structure:

{
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
  "actionDefinitions": [
    {
      "name": "ActionName",
      "description": "What this action involves",
      "goal": "The player's goal in this action",
      "actionType": "Discuss",
      "isRepeatable": true/false,
      "staminaCost": 1,
      "locationName": "{currentLocation}",
      "spotName": "Spot name", // MUST BE AN ALREADY KNOWN SPOT NAME OR DEFINED IN THIS RESPONSE 
    }
  ],
  "locationSpots": [
    {
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "locationName": "{currentLocation}"
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
  "characters": [
    {
      "name": "Giles",
      "role": "Merchant",
      "description": "Brief physical and personality description",
      "location": "Name of existing location where they can be found",
      "locationSpot": "Name of existing locationSpot where they can be found"
    }
  ],
  "contracts": [
    {
      "name": "Opportunity name",
      "type": "Quest/Job/Mystery/Investigation",
      "description": "Brief description",
      "location": "Name of existing location where it takes place",
      "locationSpot": "Name of existing locationSpot where it takes place",
      "relatedCharacter": "Name of existing character involved"
    }
  ]
}