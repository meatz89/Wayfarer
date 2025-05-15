IMPORTANT: Generate ONLY the raw content with no meta-commentary. DO NOT acknowledge this request, introduce your response (like "I'll create..." or "Here is..."), or end with questions to the reader. Your entire response should be exactly what will be shown to the player without requiring any editing.

# NARRATIVE CHOICE GENERATION

## Encounter State 
- Encounter Type: {ENCOUNTER_TYPE}
- Current Turn: {CURRENT_TURN}/{MAX_TURNS}
- Momentum: {MOMENTUM}/{MAX_MOMENTUM} (Success at {SUCCESS_THRESHOLD})
- Pressure: {PRESSURE}/{MAX_PRESSURE}
- Active Narrative Tags: {ACTIVE_TAGS}
- Goal: {ENCOUNTER_GOAL}

## Player Character Status
{PLAYER_STATUS}

## Choices to Transform
{CHOICES_INFO}

## Core Narrative Principles
- EVERY choice must CHANGE the encounter state and advance the narrative
- Choices should introduce NEW elements or transform existing ones
- AVOID passive observation, static analysis, or waiting
- Create meaningful CONSEQUENCES for each potential choice
- Each choice should feel like a true BRANCH in the story

## Format Requirements
For each choice, create:

1. NAME based on encounter type:
   - Physical: Action thoughts without quotation marks (I'll create a distraction with that barrel...)
   - Intellectual: Insight thoughts without quotation marks (That merchant's ledger has a hidden meaning...)
   - Rapport: Direct speech in quotation marks ("Perhaps we could come to an arrangement...")

2. DESCRIPTION (1-2 sentences):
   - Must CREATE a NEW SITUATION or CHANGE the existing one
   - Show potential CONSEQUENCES of the action
   - Match the URGENCY level of the encounter situation
   - Physical encounters require IMMEDIATE, CONSEQUENTIAL actions
   - Express uncertainty using "might," "could," or "hope to"

3. DYNAMIC ACTION requirements:
   - Each choice must CHANGE something in the scene
   - AVOID choices that only gather information without using it
   - TRANSFORM static observation into action with immediate effects
   - Show how each choice creates a NEW NARRATIVE BRANCH
   - Analysis should lead to IMMEDIATE insight and advantage, not just information
   - For tense situations, all choices must maintain or escalate tension

4. PERIOD AUTHENTICITY:
   - Use language true to a medieval character of the archetype
   - DeScholar observations through concrete sensory details, not abstract systems
   - Frame tactics through experience and instinct, not modern analytical terms
   - Avoid modern concepts like "scanning," "monitoring," or "surveillance"

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
```