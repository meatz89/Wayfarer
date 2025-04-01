# ACTION AND ENCOUNTER GENERATION

Create an action and matching encounter for '{SPOT_NAME}' at '{LOCATION_NAME}'.

## Context
- Location: {LOCATION_NAME} - {LOCATION_DESCRIPTION}
- Spot: {SPOT_NAME} - {SPOT_DESCRIPTION}
- Interaction Type: {INTERACTION_TYPE}
- Environmental Properties: {ENVIRONMENTAL_PROPERTIES}

## Create an Action with:
- Name: A brief, descriptive name (3-4 words maximum)
- Goal: What the player aims to achieve
- Complication: What challenge makes this interesting
- ActionType: Travel/Rest/Investigate/Discuss/Persuade

## Action Type Clarification:
- DIRECT ACTION: Provides immediate benefits (rest, purchase, etc.) with defined costs
- ENCOUNTER ACTION: Triggers an encounter with the tag-based system
- Specify which type this action will be
- For Direct Actions, define exact costs and benefits
- For Encounter Actions, define exact success outcomes

## Create a Matching Encounter Template with:
1. Parameters:
   - Duration: How many turns (3-7)
   - MaxPressure: Failure threshold (8-15)
   - Success thresholds: Partial, Standard, Exceptional (typically spaced 4 points apart)
   - Hostility: Friendly, Neutral, or Hostile

2. Approach specifications:
   - PressureReducingFocuses: Which focuses reduce pressure (pick 1-2)
   - MomentumReducingFocuses: Which focuses reduce momentum (pick 1-2 different)

3. Strategic Tags (4-5):
   - Must use standard environmental properties exactly as listed:
     * Illumination: Bright, Shadowy, Dark
     * Population: Crowded, Quiet, Isolated
     * Atmosphere: Tense, Formal, Chaotic
     * Economic: Wealthy, Commercial, Humble
     * Physical: Confined, Expansive, Hazardous

4. Narrative Tags (2-3):
   - Use existing tags like: IntimidatingPresence, BattleRage, SuperficialCharm, 
     SocialAwkwardness, DetailFixation, Overthinking, ShadowVeil, ParanoidMindset
   - Consider which approaches might be overused in this encounter

## Success and Failure Paths:
- Define SPECIFIC success outcome (new location, spot, character, knowledge, etc.)
- Define ALTERNATIVE PATH if encounter fails (new action elsewhere that achieves similar goal)
- Failure alternatives must provide different approach to similar objective

## Response Format
Respond with a JSON object containing both action and encounter details:

{
  "action": {
    "name": "Negotiate with Merchants",
    "goal": "Secure favorable trade terms for your goods",
    "complication": "Established traders are suspicious of newcomers",
    "actionType": "Persuade",
    "coinCost": 5
  },
  "encounterTemplate": {
    "duration": 5,
    "maxPressure": 10,
    "partialThreshold": 10,
    "standardThreshold": 14, 
    "exceptionalThreshold": 18,
    "hostility": "Neutral",
    "pressureReducingFocuses": ["Relationship", "Resource"],
    "momentumReducingFocuses": ["Physical", "Environment"],
    "strategicTags": [
      {"name": "Market Daylight", "environmentalProperty": "Bright"},
      {"name": "Bustling Shoppers", "environmentalProperty": "Crowded"},
      {"name": "Trading Post", "environmentalProperty": "Commercial"},
      {"name": "Market Commotion", "environmentalProperty": "Chaotic"}
    ],
    "narrativeTags": ["SuperficialCharm", "ColdCalculation"]
  }
}