IMPORTANT: Generate ONLY the raw content with no meta-commentary. DO NOT acknowledge this request, introduce your response (like "I'll create..." or "Here is..."), or end with questions to the reader. Your entire response should be exactly what will be shown to the player without requiring any editing.

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
- Location name: {locationName}
- Location description: {locationDescription}
- Connected from: {originLocationName}
- Current depth: {locationDepth}
- Player archetype: {characterArchetype}
- Current player focus: {playerFocus}
- Current player resources: Health {health}/{maxHealth}, Energy {energy}/{maxEnergy}
- Known characters: {knownCharacters}
- Active opportunities: {activeOpportunities}

## Existing World Context
- Current location spots: 
{currentLocationSpots}

- All known location spots across all locations:
{allKnownLocationSpots}

- All existing actions:
{allExistingActions}

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
  
  "strategicTags": [
    {
      "approachType": "Analysis/Dominance/Rapport/Precision/Concealment",
      "effectType": "IncreasesMomentum/DecreasesPressure/DecreasesMomentum/IncreasesPressure",
      "description": "Description of how this approach affects encounters here"
    }
  ],
  
  "narrativeTags": [
    {
      "approachType": "Analysis/Dominance/Rapport/Precision/Concealment",
      "focusType": "Relationship/Information/Physical/Environment/Resource",
      "threshold": 3,
      "description": "Description of how this approach affects focus availability"
    }
  ],
  
  "locationSpots": [
    {
      "id": "unique_spot_id",
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "interactionDescription": "Description of what happens on basic interaction",
      "position": "Center/North/East/South/West",
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
      "goal": "The player's goal in this action",
      "complication": "What makes this challenging",
      "actionType": "Discuss/Persuade/Perform/Study/Investigate/Analyze/Rest/Labor/Gather/Fight",
      "isRepeatable": true,
      "energyCost": 1,
      "cost": {
        "energy": 1,
        "timeMinutes": 30,
        "money": 0
      },
      "encounterDefinition": {
        "goal": "The player's goal in this encounter",
        "complication": "What makes this challenging",
        "momentum": 0,
        "pressure": 0,
        "strategicTags": ["Tag1", "Tag2"]
      }
    }
  ]
}

## FINAL CHECK
BEFORE RETURNING THE JSON, VERIFY ONE LAST TIME that you have included at least one locationSpot and at least one actionDefinition with a matching locationSpotId. This is REQUIRED to prevent the player from being stuck.