# WORLD EVOLUTION

After analyzing the full encounter narrative, determine how the world should evolve based on the outcome. Every encounter MUST add at least one action.

## Context
- Current location: {currentLocation} (Depth: {currentDepth})
- Last hub depth: {lastHubDepth}
- Current player resources: Health {health}/{maxHealth}, Energy {energy}/{maxEnergy}
- Known locations: {knownLocations}
- Known characters: {knownCharacters}
- Active opportunities: {activeOpportunities}
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

## Player Location Changes
- Carefully review the encounter narrative to determine if the player ended at a DIFFERENT LOCATION
- If player moved to a new location, specify this location name in your response
- If this new location does not exist in the known locations list, you MUST create it with spots and action

## Resource Changes
- Extract ANY mention of coin transactions (gains or losses)
- Note inventory items ADDED during the encounter
- Note inventory items REMOVED or USED during the encounter

## Discovery Rewards
- All new locations should have discoveryBonusXP (10 × depth) 
- All new locations should have discoveryBonusCoins (5 × depth)
- Hubs should have double these discovery bonuses

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

## World Structure Requirements
- EVERY new location MUST have at least one spot
- Location → Spot → Action hierarchy must be maintained
- New elements must connect logically to existing world elements

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