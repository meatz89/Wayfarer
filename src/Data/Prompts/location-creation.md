# LOCATION CREATION

## ‼️ CRITICAL REQUIREMENT ‼️
YOU MUST ALWAYS GENERATE AT LEAST ONE ACTION WITH A CORRESPONDING LOCATION SPOT.
FAILURE TO DO SO WILL PREVENT THE PLAYER FROM INTERACTING WITH THE GAME AND CREATE A HARD LOCK.
THIS IS THE MOST IMPORTANT REQUIREMENT.

## FOCUS
This prompt is for LOCATION CREATION ONLY. Your task is to create a MINIMAL, FUNCTIONAL representation of the location the player just arrived at.
Do not acknowledge my request in your response.

## REQUIRED COMPONENTS
- 1-2 location spots that represent key areas of this location
- 1-2 actions per spot that give the player clear things to do
- Environmental properties that define this location's character
- Simple, focused descriptions that establish the location's identity

## VERIFICATION CHECKLIST (ESSENTIAL)
Before submitting your response, verify that:
1. You have created AT LEAST ONE location spot
2. You have created AT LEAST ONE action linked to a location spot
3. Every action has a valid locationSpotId that matches an existing spot
4. You have included all required fields in your JSON

## Context
- Current Location name: {locationName}
- Arrived after travel from: {originLocationName}
- Player archetype: {characterArchetype}

## Existing World Context

- All known locations
{allKnownLocations}
 
- All Known characters: 
{knownCharacters}

- Active contracts: 
{activeContracts}

## Minimal Design Philosophy
- Focus on QUALITY over QUANTITY
- Create just enough content for immediate play
- Future encounters will naturally expand this location
- Provide a foundation that can grow organically

## Response Format
You must provide your response ONLY as a valid JSON object with the following structure:

{
  "name": "{locationName}",
  "description": "Brief description of the location",
  "detailedDescription": "More detailed paragraph about this location",
  "history": "Brief history of this location",
  "pointsOfInterest": "Notable features or landmarks",
  "travelTimeMinutes": 60,
  "travelDescription": "Description of the journey to this location",
  "connectedLocationIds": ["Name of connected location"],
  
  "playerLocationUpdate": {
    "newLocationName": "{locationName}",
    "locationChanged": true
  },
  
  "locationSpots": [
    {
      "name": "Spot name",
      "locationName": "{locationName}",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "interactionDescription": "Description of what happens on basic interaction",
      "position": "Center/North/East/South/West",
      "environmentalProperties": {
        "illumination": "Bright/Thiefy/Dark",
        "population": "Crowded/Quiet/Isolated",
        "atmosphere": "Guarded/Formal/Chaotic",
        "": "Wealthy/Commercial/Humble",
        "physical": "Confined/Expansive/Hazardous"
      }
    }
  ],
  
  "actionDefinitions": [
    {
      "name": "Action name",
      "description": "What this action involves",
      "type": "Encounter/Direct/Travel",
      "locationName": "{locationName}",
      "locationSpot": "Spot name", // MUST BE AN ALREADY KNOWN SPOT NAME OR DEFINED IN THIS RESPONSE,
      "request": "The player's request in this action",
      "actionType": "Discuss/Persuade/Perform/Study/Investigate/Analyze/Rest/Labor/Gather/Fight",
      "isRepeatable": true,
      "staminaCost": 1,
      "cost": {
        "stamina": 1,
        "timeMinutes": 30,
        "money": 0
      },
      "encounterDefinition": {
        "request": "The player's request in this encounter",
        "strategicTags": ["Tag1", "Tag2"]
      }
    }
  ]
}

## FINAL CHECK
BEFORE RETURNING THE JSON, VERIFY ONE LAST TIME that you have included at least one locationSpot and at least one actionDefinition with a matching locationSpotId. This is REQUIRED to prevent the player from being stuck.