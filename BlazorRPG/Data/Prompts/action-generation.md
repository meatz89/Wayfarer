IMPORTANT: Generate ONLY the raw content with no meta-commentary. DO NOT acknowledge this request, introduce your response (like "I'll create..." or "Here is..."), or end with questions to the reader. Your entire response should be exactly what will be shown to the player without requiring any editing.

# ACTION AND ENCOUNTER GENERATION

Create a complete action and encounter template for '{ACTIONNAME}' at the location spot '{SPOT_NAME}' in '{LOCATION_NAME}'.

## Core Action Details
- Name: "{ACTIONNAME}"
- Goal: "{GOAL}"
- Complication: "{COMPLICATION}" 
- BasicActionType: "{ACTION_TYPE}"

## Encounter Design Task

Design a complete encounter that implements this action, providing all required mechanical values.

### ActionTemplate Values Needed
- **ActionType**: Choose between:
  * "Basic" - Direct action with immediate effects (rest, purchase, travel)
  * "Encounter" - Triggers the tag-based encounter system (most actions are this type)
- **CoinCost**: Any upfront coin cost to attempt the action (often 0)

### EncounterTemplate Values Needed
- **Name**: Unique identifier for this encounter (typically ActionName + "Encounter")
- **Duration**: Number of turns (3-7) based on complexity
- **MaxPressure**: Failure threshold (usually 10-15)
- **PartialThreshold**: Minimum momentum for success (usually 8-12)
- **StandardThreshold**: Momentum needed for standard success (usually 12-16)
- **ExceptionalThreshold**: Momentum needed for exceptional success (usually 16-20)
- **Hostility**: "Friendly", "Neutral", or "Hostile" - affects starting pressure and momentum
- **PressureReducingFocuses**: 1-2 focus tags that are effective at reducing pressure
- **MomentumReducingFocuses**: 1-2 focus tags that are ineffective (reduce momentum)
- **StrategicTags**: 4-5 environmental properties that affect which approaches work well:
  * Must use EXACT standard property names:
    - Illumination: Bright, Shadowy, Dark
    - Population: Crowded, Quiet, Isolated
    - Atmosphere: Tense, Formal, Chaotic
    - Economic: Wealthy, Commercial, Humble
    - Physical: Confined, Expansive, Hazardous
  * Each strategic tag needs a descriptive name and environmental property
- **NarrativeTags**: 2-3 tags that activate when approaches reach thresholds:
  * Must use EXACT tag names from: IntimidatingPresence, BattleRage, BruteForceFixation, 
    TunnelVision, DestructiveImpulse, SuperficialCharm, SocialAwkwardness, HesitantPoliteness, 
    PublicAwareness, GenerousSpirit, ColdCalculation, AnalysisParalysis, Overthinking, 
    DetailFixation, TheoreticalMindset, MechanicalInteraction, NarrowFocus, PerfectionistParalysis, 
    DetailObsession, InefficientPerfectionism, ShadowVeil, ParanoidMindset, CautiousRestraint, 
    HidingPlaceFixation, HoardingInstinct

## Encounter Balance Guidelines
- Duration should match complexity (simple=3-4 turns, complex=5-7 turns)
- Thresholds should be spaced 2-3 points apart (e.g., 10, 14, 18)
- MaxPressure should be roughly 2× StandardThreshold
- Strategic tags should include 2 beneficial and 2 detrimental approach effects
- Narrative tags should activate at approach values 3+ or 4+
- PressureReducingFocuses and MomentumReducingFocuses should be different focus tags

## Response Format
Respond with a JSON object containing both action and encounter details:

{
  "action": {
    "name": "Negotiate with Merchants",
    "goal": "Secure favorable trade terms for your goods",
    "complication": "Established traders are suspicious of newcomers",
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