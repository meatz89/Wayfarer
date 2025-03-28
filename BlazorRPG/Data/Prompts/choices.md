# NARRATIVE CHOICE GENERATION

## Encounter Status
- Type: {ENCOUNTER_TYPE} | Turn: {CURRENT_TURN}/{MAX_TURNS} | Stage: {ENCOUNTER_STAGE}
- Momentum: {CURRENT_MOMENTUM}/{MAX_MOMENTUM} (Success: {SUCCESS_THRESHOLD})
- Pressure: {CURRENT_PRESSURE}/{MAX_PRESSURE}
- Resources: Health {CURRENT_HEALTH}, Confidence {CURRENT_CONFIDENCE}, Concentration {CURRENT_CONCENTRATION}

## Strategic Information
- Favorable Approaches: {FAVORABLE_APPROACHES}
- Dangerous Approaches: {DANGEROUS_APPROACHES}
- Active Narrative Tags: {ACTIVE_TAGS}

## Current Situation
- Character Goal: {CHARACTER_GOAL}
- Character State: {INJURIES/RESOURCES/CONDITION}

## Choices to Transform
{CHOICES_INFO}

## Format Requirements
For each choice, create:
1. NAME based on encounter type:
   - Physical: Action thoughts without quotation marks (I'll charge at him...)
   - Intellectual: Observational thoughts without quotation marks (Those symbols match...)
   - Social: Direct speech in quotation marks ("Let me help with that...")
2. DESCRIPTION (1-2 sentences) showing what would happen and NPC responses
3. Each choice must reflect its approach and focus combination
4. Consider NPC responses based on their established motivations
5. Show present NPCs as independent actors with their own agendas

## RESPONSE FORMAT
You must respond with a SINGLE JSON object containing a "choices" array:
```json
{
  "choices": [
    {
      "index": 1,
      "name": "First choice name",
      "description": "First choice description"
    },
    {
      "index": 2,
      "name": "Second choice name",
      "description": "Second choice description"
    },
    {
      "index": 3,
      "name": "Third choice name",
      "description": "Third choice description"
    },
    {
      "index": 4,
      "name": "Fourth choice name",
      "description": "Fourth choice description"
    }
  ]
}

Do NOT add approach and focus fields to the JSON - they are provided for your reference only.