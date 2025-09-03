# ACTION AND CONVERSATION GENERATION

Create a complete action and conversationContext template for '{ACTIONNAME}' at the location spot '{SPOT_NAME}' in '{LOCATION_NAME}'.

## Core Action Details
- Name: "{ACTIONNAME}"

## ConversationContext Design Task

Design a complete conversationContext that implements this action, providing all required mechanical values.

### ActionTemplate Values Needed
- **ActionType**: Choose between:
  * "Basic" - Direct action with immediate effects (rest, purchase, travel)
  * "Conversation" - Triggers the tag-based conversationContext system (most actions are this type)
- **CoinCost**: Any upfront coin cost to attempt the action (often 0)

## Response Format
Respond with a JSON object containing both action and conversationContext details:

{
  "action": {
    "name": "Negotiate with Merchants",
    "request": "Secure favorable trade terms for your goods",
    "actionType": "Conversation",
    "coinCost": 5
  },
  "conversationTemplate": {
    "name": "NegotiateWithMerchantsConversation",
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