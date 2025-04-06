# LOCATION CREATION

## FOCUS
This prompt is for LOCATION CREATION ONLY. Your task is to create a MINIMAL, FUNCTIONAL representation of the location the player just arrived at.
Do not acknowledge my request in your response.

## REQUIRED COMPONENTS
- 1-2 location spots that represent key areas of this location
- 1-2 actions per spot that give the player clear things to do
- Environmental properties that define this location's character
- Simple, focused descriptions that establish the location's identity

## LOCATION DUPLICATION PREVENTION
- BEFORE creating ANY new location, CHECK if its name appears in "{knownLocations}" 
- If it does, DO NOT include it in the "locations" array of your response
- DUPLICATE LOCATIONS WILL BREAK THE GAME

## MANDATORY REQUIREMENTS FOR ALL RESPONSES
- Every response MUST include at least one new actionDefinition
- All actionDefinitions MUST have spotName and locationName that match existing or newly created spots/locations
- FAILURE TO MEET THESE REQUIREMENTS WILL BREAK THE GAME

## Existing World Context
- Current location spots: 
{currentLocationSpots}

- All known location spots across all locations:
{allKnownLocationSpots}

- All existing actions:
{allExistingActions}

## Context
- Location name: {locationName}
- Location description: {locationDescription}
- Connected from: {originLocationName}
- Current depth: {locationDepth}
- Player archetype: {characterArchetype}
- Current player focus: {playerFocus}

## Context
- Current location: {currentLocation} (Depth: {currentDepth})
- Last hub depth: {lastHubDepth}
- Current player resources: Health {health}/{maxHealth}, Energy {energy}/{maxEnergy}
- Known characters: {knownCharacters}
- Active opportunities: {activeOpportunities}

## Minimal Spot Design
- Include only 1-2 key spots that represent distinct areas
- Each spot must have 1-2 actions that provide clear opportunities
- Spots should follow logical placement within the location type
- If this is a hub location, include spots for essential services (rest, trade, information)

## Action Design
- Each action should have a clear goal and complication
- Keep action types appropriate to the location's nature
- Include at least one action that advances the narrative
- Actions must have appropriate energy and time costs

## Environmental Properties
- Assign 3-5 properties that define this location's character:
  * Illumination: Bright, Shadowy, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * Economic: Wealthy, Commercial, Humble
  * Physical: Confined, Expansive, Hazardous

## Minimalist Approach
- Focus on QUALITY over QUANTITY
- Create just enough spots and actions for immediate play
- Future encounters will naturally expand this location
- Provide a foundation that can grow organically

## Response Format
You must provide your response ONLY as a valid JSON object with the following structure:

{
  "playerLocationUpdate": {
    "newLocationName": "{locationName}",
    "locationChanged": true
  },
  "locationSpots": [
    {
      "id": "unique_spot_id",
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "position": "Center/North/East/South/West",
      "locationName": "{locationName}",
      "environmentalProperties": {
        "illumination": "Bright/Shadowy/Dark",
        "population": "Crowded/Quiet/Isolated",
        "atmosphere": "Tense/Formal/Chaotic",
        "economic": "Wealthy/Commercial/Humble",
        "physical": "Confined/Expansive/Hazardous"
      }
    }
  ],
  "actionDefinitions": [
    {
      "id": "unique_action_id",
      "name": "Action name",
      "description": "What this action involves",
      "type": "Encounter/Direct/Travel",
      "locationName": "{locationName}",
      "locationSpotId": "unique_spot_id",
      "cost": {
        "energy": 1,
        "timeMinutes": 30,
        "money": 0
      },
      "encounterDefinition": {
        "goal": "The player's goal in this action",
        "complication": "What makes this challenging",
        "momentum": 0,
        "pressure": 0,
        "strategicTags": ["Tag1", "Tag2"]
      }
    }
  ]
}