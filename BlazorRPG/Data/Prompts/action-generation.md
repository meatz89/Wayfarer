# ACTION AND ENCOUNTER GENERATION TASK

Create a complete action and matching encounter for the spot '{SPOT_NAME}' at location '{LOCATION_NAME}'.

## Context Information
- Location Name: {LOCATION_NAME}
- Location Description: {LOCATION_DESCRIPTION}
- Spot Name: {SPOT_NAME}
- Spot Description: {SPOT_DESCRIPTION}
- Spot Interaction Type: {INTERACTION_TYPE}
- Environmental Properties: {ENVIRONMENTAL_PROPERTIES}

## Action Requirements
- Name: A brief, descriptive name for this action (3-4 words maximum)
- Goal: What the player aims to achieve through this action
- Complication: What challenge or obstacle makes this interesting
- ActionType: Must be one of: Travel, Rest, Investigate, Discuss, Persuade
- CoinCost: The amount of coins required to perform this action (0 if none)

## Encounter Template Requirements
Each action needs a matching encounter template with:

1. Basic Parameters:
   - Duration: How many turns the encounter lasts (3-7)
   - MaxPressure: Maximum pressure before failure (8-15)
   - PartialThreshold: Momentum needed for partial success (typically MaxPressure - 3)
   - StandardThreshold: Momentum needed for standard success (typically PartialThreshold + 4)
   - ExceptionalThreshold: Momentum needed for exceptional success (typically StandardThreshold + 4)
   - Hostility: Friendly, Neutral, or Hostile

2. Approach Specifications:
   - MomentumBoostApproaches: Which approaches increase momentum (pick 1-2 from Dominance, Rapport, Analysis, Precision, Evasion)
   - DangerousApproaches: Which approaches have negative effects (pick 1-2 different from above)
   - PressureReducingFocuses: Which focuses reduce pressure (pick 1-2 from Relationship, Information, Physical, Environment, Resource)
   - MomentumReducingFocuses: Which focuses reduce momentum (pick 1-2 different from above)

3. Strategic Tags (4-5 tags describing the environment):
   Each strategic tag needs:
   - Name: Descriptive name for this environmental aspect
   - Environmental Property: Must use one of the following (match to the location's properties when possible):
     * Illumination: Bright, Shadowy, Dark
     * Population: Crowded, Quiet, Isolated
     * Atmosphere: Tense, Formal, Chaotic
     * Economic: Wealthy, Commercial, Humble
     * Physical: Confined, Expansive, Hazardous

4. Narrative Tags (2-3 tags that activate at approach thresholds):
   Each narrative tag should:
   - Use an existing tag from: IntimidatingPresence, BattleRage, SuperficialCharm, SocialAwkwardness, DetailFixation, Overthinking, ShadowVeil, ParanoidMindset (or others you see appropriate)
   - Consider which approach might be overused in this encounter

## Output Format
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
    "momentumBoostApproaches": ["Rapport", "Analysis"],
    "dangerousApproaches": ["Dominance"],
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