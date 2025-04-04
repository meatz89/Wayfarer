# NARRATIVE CHOICE GENERATION

## Encounter Status
- Type: {ENCOUNTER_TYPE} | Turn: {CURRENT_TURN}/{MAX_TURNS} | Stage: {ENCOUNTER_STAGE}
- Momentum: {CURRENT_MOMENTUM}/{MAX_MOMENTUM} (Success: {SUCCESS_THRESHOLD})
- Pressure: {CURRENT_PRESSURE}/{MAX_PRESSURE}
- Resources: Health {CURRENT_HEALTH}, Confidence {CURRENT_CONFIDENCE}, Concentration {CURRENT_CONCENTRATION}

## Player Character Context
- Archetype: {CHARACTER_ARCHETYPE} (Avoid announcing directly)
- Natural Approaches: {NATURAL_APPROACHES}
- Dangerous Approaches: {DANGEROUS_APPROACHES}

## Strategic Information
- Active Narrative Tags: {ACTIVE_TAGS}

## Current Situation
- Character Goal: {CHARACTER_GOAL}
- Character State: {INJURIES/RESOURCES/CONDITION}

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
   - Social: Direct speech in quotation marks ("Perhaps we could come to an arrangement...")

2. DESCRIPTION (1-2 sentences):
   - Must CREATE a NEW SITUATION or CHANGE the existing one
   - Show potential CONSEQUENCES of the action
   - Match the URGENCY level of the encounter situation
   - Physical encounters require IMMEDIATE, CONSEQUENTIAL actions
   - Express uncertainty using "might," "could," or "hope to"
   - For natural archetype approaches: Show greater confidence
   - For dangerous archetype approaches: Show hesitation

3. DYNAMIC ACTION requirements:
   - Each choice must CHANGE something in the scene
   - AVOID choices that only gather information without using it
   - TRANSFORM static observation into action with immediate effects
   - Show how each choice creates a NEW NARRATIVE BRANCH
   - Analysis should lead to IMMEDIATE insight and advantage, not just information
   - For tense situations, all choices must maintain or escalate tension

4. PERIOD AUTHENTICITY:
   - Use language true to a medieval character of the archetype
   - Describe observations through concrete sensory details, not abstract systems
   - Frame tactics through experience and instinct, not modern analytical terms
   - Avoid modern concepts like "scanning," "monitoring," or "surveillance"

5. Approach manifestation:
   - Analysis: Noticing crucial details that create IMMEDIATE advantage, spotting a weakness to exploit NOW
   - Precision: Swift, targeted movements that change the encounter state
   - Rapport: Emotional appeals that provoke immediate reactions
   - Evasion: Deceptive actions that create new opportunities
   - Dominance: Forceful interventions that alter power dynamics

6. Focus manifestation:
   - Relationship: Provoking specific reactions from NPCs that change the encounter
   - Information: Using knowledge to CREATE an advantage, not just gather more data
   - Physical: Manipulating bodies or objects to transform the situation
   - Environment: Changing terrain, creating new paths, or altering surroundings
   - Resource: Transforming objects into tools that change encounter dynamics

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