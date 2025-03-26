Using the player choices and actions from our conversation, determine how the world should evolve in response.

CHARACTER BACKGROUND:
{{characterBackground}}

CURRENT WORLD STATE:
- Current location: {{currentLocation}}
- Known locations: {{knownLocations}}
- Known characters: {{knownCharacters}}
- Active opportunities: {{activeOpportunities}}

INSTRUCTIONS:
1. Identify what interested the player during this conversation
2. Apply "Purpose or Perish" - only include elements that advance the plot OR reinforce tone
3. Create world evolutions maintaining proper references:
   - New location spots (0-2) can only be added to existing locations or new locations you define
   - New actions (0-3) can only be added to existing spots or new spots you define
   - New characters (0-2) must be placed at existing locations
   - New locations (0-1) must connect to known locations
   - New opportunities (0-2) must connect to existing locations and characters

FOR EACH NEW ELEMENT:
- Make it require player interaction
- Connect it to the player's choices or background
- Keep descriptions brief but evocative
- Ensure it has a clear purpose

CHARACTER NAMING:
- Character names should be SIMPLE FIRST NAMES ONLY (e.g., "Giles", "Marta")
- Do NOT include titles or occupations in the name field (NO "Giles the merchant" or "widow Marta")
- Put occupations, titles, or roles in the dedicated "role" field instead

IMPORTANT FORMAT REQUIREMENTS:
- ActionNames must use predefined types: VillageGathering, TradeGoods, ForestTravel, SecretMeeting, SecretDeal, RentRoom, or FindQuests
- Action types must be one of: Discuss, Travel, Persuade, Rest, Investigate
- Environmental properties must specifically use these exact values: Bright, Shadowy, Dark, Crowded, Quiet, Isolated, Tense, Formal, Chaotic, Wealthy, Commercial, Humble, Confined, Expansive, or Hazardous
- ConnectedTo should be an array of existing location names
- Every action must belong to an existing spot OR a new spot defined in this response
- Every spot must belong to an existing location OR a new location defined in this response

RESPONSE FORMAT:
Respond with ONLY a valid JSON object following this exact structure:

{
  "newLocationSpots": [
    {
      "name": "Spot name",
      "description": "Brief description",
      "interactionType": "Character/Shop/Feature/Service",
      "locationName": "Name of existing location this spot belongs to",
      "actions": [
        {
          "name": "TradeGoods",
          "description": "What this action involves",
          "goal": "The player's goal in this action",
          "complication": "What makes this challenging", 
          "actionType": "Persuade"
        }
      ]
    }
  ],
  "newActions": [
    {
      "spotName": "Name of existing spot this action belongs to",
      "locationName": "Name of the location containing this spot",
      "name": "SecretMeeting",
      "description": "What this action involves",
      "goal": "The player's goal in this action",
      "complication": "What makes this challenging",
      "actionType": "Discuss"
    }
  ],
  "newCharacters": [
    {
      "name": "Giles",
      "role": "Merchant",
      "description": "Brief physical and personality description",
      "location": "Name of existing location where they can be found"
    }
  ],
  "newLocations": [
    {
      "name": "Location name",
      "description": "Brief description",
      "connectedTo": ["First Connected Location", "Second Connected Location"],
      "environmentalProperties": ["Bright", "Crowded", "Commercial", "Chaotic"],
      "spots": [
        {
          "name": "Spot name",
          "description": "Brief description",
          "interactionType": "Character/Shop/Feature/Service",
          "actions": [
            {
              "name": "VillageGathering",
              "description": "What this action involves",
              "goal": "The player's goal in this action",
              "complication": "What makes this challenging",
              "actionType": "Discuss"
            }
          ]
        }
      ]
    }
  ],
  "newOpportunities": [
    {
      "name": "Opportunity name",
      "type": "Quest/Job/Mystery",
      "description": "Brief description",
      "location": "Name of existing location where it takes place",
      "relatedCharacter": "Name of existing character involved"
    }
  ]
}