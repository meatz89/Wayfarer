# NARRATIVE CHOICE GENERATION

## Encounter Status
- Type: {ENCOUNTER_TYPE} | Turn: {CURRENT_TURN}/{MAX_TURNS} | Stage: {ENCOUNTER_STAGE}
- Momentum: {CURRENT_MOMENTUM}/{MAX_MOMENTUM} (Success: {SUCCESS_THRESHOLD})
- Pressure: {CURRENT_PRESSURE}/{MAX_PRESSURE}
- Resources: Health {CURRENT_HEALTH}, Confidence {CURRENT_CONFIDENCE}, Concentration {CURRENT_CONCENTRATION}

## Player Character Context
- Archetype: {CHARACTER_ARCHETYPE}
- Natural Approaches: {NATURAL_APPROACHES}
- Dangerous Approaches: {DANGEROUS_APPROACHES}

## Strategic Information
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

2. DESCRIPTION (1-2 sentences):
   - Match the URGENCY level of the encounter situation
   - Physical encounters with threats require IMMEDIATE actions, not contemplative ones
   - Analysis in physical encounters must be QUICK calculations, not careful study
   - Express uncertainty using "might," "could," or "hope to"
   - For natural archetype approaches: Show greater confidence
   - For dangerous archetype approaches: Show hesitation

3. Context requirements:
   - HIGH THREAT situations require choices that acknowledge the immediate danger
   - Choices must be PLAUSIBLE within the timeframe available
   - Even intellectual approaches must manifest as QUICK THINKING during urgent situations
   - INTELLECTUAL actions during physical encounters should focus on split-second assessments
   - SOCIAL actions during physical encounters should be brief and immediate

4. Approach manifestation:
   - Analysis: Quick calculations, rapid pattern recognition, instant recall
   - Precision: Careful but swift movements, targeted actions
   - Rapport: Immediate emotional appeals, quick social reads
   - Evasion: Fast, stealthy movements, deception
   - Dominance: Forceful presence, intimidation, direct confrontation

5. Focus manifestation:
   - Relationship: Reading intentions, leveraging trust, exploiting social bonds, targeting loyalties
   - Information: Recalling crucial knowledge, spotting patterns, applying expertise, identifying weaknesses
   - Physical: Assessing bodily capabilities, targeting vulnerable points, exploiting physical properties
   - Environment: Utilizing terrain advantages, spotting environmental hazards, creating positional leverage
   - Resource: Deploying carried items, identifying valuable objects, weaponizing available resources
 
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
