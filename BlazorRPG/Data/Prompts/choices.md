IMPORTANT: Generate ONLY the raw content with no meta-commentary. DO NOT acknowledge this request, introduce your response (like "I'll create..." or "Here is..."), or end with questions to the reader. Your entire response should be exactly what will be shown to the player without requiring any editing.

# WAYFARER'S RESOLVE CHOICE GENERATION

## Current Encounter Situation
- Encounter Type: {ENCOUNTER_TYPE}
- Current Stage: {CURRENT_STAGE}/5 ({ENCOUNTER_TIER} Tier)
- Progress: {CURRENT_PROGRESS}/{SUCCESS_THRESHOLD}

## Player Character Status
{PLAYER_STATUS}

## CRITICAL REQUIREMENT: IDENTIFY THE CORE DECISION POINT
Before creating choices, analyze the narrative to identify:
1. Exactly where the character is physically located RIGHT NOW
2. Who is present in the immediate scene with them
3. The main decision or challenge facing the character in this moment
4. Any moral, emotional, or strategic tension explicitly described
5. Any significant objects or elements that have emotional or practical importance

All choices MUST respond directly to these identified elements and the central decision point.

## CRITICAL REQUIREMENT: SITUATIONAL RESPONSE
Every choice represents how the player responds to what's happening RIGHT NOW in this specific moment. Each choice name and description must directly reference elements, characters, objects, or circumstances from the current scene. Generic responses that could apply to any encounter are INCORRECT.

## Response Creation Requirements
Every choice must:
- DIRECTLY ADDRESS the central decision or challenge identified in the narrative
- ACKNOWLEDGE the character's current physical location and who is present
- REFERENCE concrete elements with emotional or practical significance from the scene
- RESPOND to any moral, emotional or strategic tension explicitly described
- SHOW how this specific reaction advances the player toward resolving the situation
- INDICATE what immediate risk the player accepts by taking this specific action
- FEEL like a natural reaction a person would have to THIS situation

## Choice Naming by Encounter Type:
- **Physical**: Action thoughts referencing the current physical challenge
- **Intellectual**: Insight thoughts referencing the current mental puzzle
- **Social**: Direct speech referencing the current person or social dynamic

## Description Guidelines (1-2 sentences):
- Reference SPECIFIC elements from the current situation (people, objects, challenges, opportunities)
- Incorporate any emotional or moral dimensions explicitly mentioned in the narrative
- Show HOW this reaction addresses what's happening right now
- Indicate WHAT risk the player accepts by reacting this way to this specific situation
- Use "might," "could," or "would" to express potential consequences of this specific reaction

## Period Authenticity:
- Medieval character perspective appropriate to the current situation
- Concrete reactions to visible, tangible elements of the scene
- Experience-based responses to the specific challenge at hand
- Avoid modern analytical terms or abstract strategic thinking

## Current Encounter Context for Direct Response
{CHOICES_INFO}

## RESPONSE FORMAT
You must respond with a SINGLE JSON object containing exactly 6 choices that are direct reactions to the current encounter situation:

```json
{
  "choices": [
    {
      "index": 1,
      "name": "Specific reaction to current situation",
      "description": "How this reaction addresses what's happening now and what risk it carries"
    }
  ]
}