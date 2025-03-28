# LOCATION CREATION TASK

Create a detailed '{LOCATION_TYPE}' location for Wayfarer.

## Location Requirements
- Difficulty Level: {DIFFICULTY} (1-3)
- Spots: {REQUESTED_SPOT_COUNT} interaction areas
- Choose appropriate environmental properties from:
  * Illumination: Bright, Shadowy, Dark
  * Population: Crowded, Quiet, Isolated
  * Atmosphere: Tense, Formal, Chaotic
  * Economic: Wealthy, Commercial, Humble
  * Physical: Confined, Expansive, Hazardous
- Determine logical connections to other location types

## Each Spot Must Include:
- Name and description
- InteractionType: Character/Quest/Shop/Feature/Travel
- Position: North/South/East/West/Center
- Initial action possibility (TradeGoods, ForestTravel, RentRoom, etc.)

## Response Format
A JSON object with 'name', 'description', 'detailedDescription', 'history', 'pointsOfInterest', 'travelTimeMinutes', 'travelDescription', 'connectedLocationIds' and 'spots' array.