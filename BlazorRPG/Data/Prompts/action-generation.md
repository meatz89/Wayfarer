IMPORTANT: Generate ONLY the raw content with no meta-commentary. DO NOT acknowledge this request, introduce your response (like "I'll create..." or "Here is..."), or end with questions to the reader. Your entire response should be exactly what will be shown to the player without requiring any editing.

# ACTION AND ENCOUNTER GENERATION

Create a complete action and encounter template for '{ACTIONNAME}' at the location spot '{SPOT_NAME}' in '{LOCATION_NAME}'.

## Core Action Details
- Name: "{ACTIONNAME}"

## Encounter Design Task

Design a complete encounter that implements this action, providing all required mechanical values.

### ActionTemplate Values Needed
- **ActionType**: Choose between:
  * "Basic" - Direct action with immediate effects (rest, purchase, travel)
  * "Encounter" - Triggers the tag-based encounter system (most actions are this type)
- **CoinCost**: Any upfront coin cost to attempt the action (often 0)

## Response Format
Respond with a JSON object containing both action and encounter details:

{
  "action": {
    "name": "Negotiate with Merchants",
    "goal": "Secure favorable trade terms for your goods",
    "actionType": "Encounter",
    "coinCost": 5
  },
  "encounterTemplate": {
    "name": "NegotiateWithMerchantsEncounter",
    "duration": 3,
    "maxPressure": 14,
    "partialThreshold": 5,
    "standardThreshold": 7, 
    "exceptionalThreshold": 10,
    "hostility": "Neutral",
    "strategicTags": [
      {"name": "Market Daylight", "environmentalProperty": "Bright"},
      {"name": "Bustling Shoppers", "environmentalProperty": "Crowded"},
      {"name": "Trading Post", "environmentalProperty": "Commercial"},
      {"name": "Market Commotion", "environmentalProperty": "Chaotic"}
    ],
    "narrativeTags": ["SuperficialCharm", "ColdCalculation"]
  }
}