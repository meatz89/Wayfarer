IMPORTANT: Generate ONLY the raw content with no meta-commentary. DO NOT acknowledge this request, introduce your response (like "I'll create..." or "Here is..."), or end with questions to the reader. Your entire response should be exactly what will be shown to the player without requiring any editing.

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
- Current location: {currentLocation} (Depth: {currentDepth})
- Last hub depth: {lastHubDepth}
- Encounter outcome: {encounterOutcome} (Success/Partial/Failure)

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

- Active opportunities: 
{activeOpportunities}

## Forward Progression Rules
- New Locations MUST be connected to current location
- New locations MUST have depth = current_depth or current_depth + 1
- Difficulty scales with depth (base difficulty = depth/2 + 1, minimum 1)
- Hub locations should appear every 3-4 depth levels
- If current_depth - lastHubDepth >= 3, YOU MUST CREATE A HUB LOCATION at depth {currentDepth + 1}
- Hub locations require 3+ spots with Rest/Trade/Healing services
- Rest locations should be available every 2 depth levels
- Locations must be directly connected to the current location
- Locations must generally be small and in walkable distance

## Location Types
- Hub: Major settlement with multiple services (Rest, Trade, Healing)
- Connective: Minor location connecting other locations
- Landmark: Special location with unique encounters
- Hazard: Dangerous area with high risk/reward

## Format Requirements
- Character names: SIMPLE FIRST NAMES ONLY (e.g., "Giles", not "Giles the merchant")
- All actions require a known location spot, a goal and a complication
- Action Type must only be one of:
  * Rest, Labor, Gather, Fight (physical actions)
  * Discuss, Persuade, Perform (social actions)
  * Study, Investigate, Analyze (intellectual actions)
- Environmental properties must use standard values only:
  * Illumination: Bright, Roguey, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * Economic: Wealthy, Commercial, Humble
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
      "complication": "What makes this challenging",
      "actionType": "Discuss",
      "isRepeatable": true/false,
      "energyCost": 1,
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
  "opportunities": [
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