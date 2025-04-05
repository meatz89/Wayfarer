# LOCATION CREATION TASK

Create a detailed '{LOCATION_TYPE}' location for Wayfarer.

## Location Requirements
- Difficulty Level: {DIFFICULTY} (1-3)
- Spots: {REQUESTED_SPOT_COUNT} interaction areas
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

## Response Format
A JSON object with 'name', 'description', 'detailedDescription', 'history', 'pointsOfInterest', 'travelTimeMinutes', 'travelDescription', 'connectedLocationIds' and 'spots' array.