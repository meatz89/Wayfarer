# LOCATION CREATION TASK

Create a detailed '{LOCATION_TYPE}' location for the Wayfarer game.

## Location Requirements
- Difficulty Level: {DIFFICULTY} (on a scale of 1-3)
- Number of Spots: {REQUESTED_SPOT_COUNT} interaction areas within the location
- Environmental Properties: Select appropriate properties for this location type
- Connected Locations: Determine logical connections to other location types

## Location Spot Requirements
For each interaction spot, define:
- Name: Descriptive name for this area
- Description: Brief description of this specific area
- InteractionType: Character, Quest, Shop, Feature, or Travel
- InteractionDescription: What the player can do here
- Position: North, South, East, West, Center, etc.
- ActionNames: Possible actions at this location, i.e. TradeGoods, ForestTravel, RentRoom, MeetContact

## Output Format
Respond with a complete JSON object containing all location details.

Format the output as a JSON object with 
an object containing 'name', 'description', 'detailedDescription', 'history', 'pointsOfInterest', 'travelTimeMinutes', 'travelDescription', 'connectedLocationIds' and 'spots'
where spots is an array of objects containing 'name', 'description', 'interactionType', 'description', 'interactionDescription', 'description', 'position' and 'actionNames'
