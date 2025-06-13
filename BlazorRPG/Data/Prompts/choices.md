IMPORTANT: Generate ONLY the raw choice content with no meta-commentary, JSON formatting, or structural elements.

# WAYFARER'S RESOLVE CHOICE GENERATION

{PROMPT_CONTEXT}

## Period Authenticity:
- Medieval character perspective appropriate to the current situation
- Concrete reactions to visible, tangible elements of the scene
- Experience-based responses to the specific challenge at hand
- Avoid modern analytical terms or abstract strategic thinking


## CRITICAL REQUIREMENT: NARRATIVE COMPREHENSION STEP
Before creating choices, analyze the narrative context INTERNALLY by answering the following three questions.

What has already happened in the story leading to this point?
What is happening right now in this specific moment?
What decision is the protagonist currently facing as a direct result of recent events?

### CRITICAL REQUIREMENT: IDENTIFY THE CORE DECISION POINT

Analyze the narrative to identify:
1. Exactly where the character is physically located RIGHT NOW
2. Who is present in the immediate scene with them
3. The main decision or challenge facing the character in this moment
4. Any moral, emotional, or strategic tension explicitly described
5. Any significant objects or elements that have emotional or practical importance

You MUST complete this analysis before generating any choices
You MUST NOT publish the results.
Choices that do not clearly emerge from this situational analysis are incorrect.

Before creating choices, 
All choices MUST respond directly to these identified elements and the central decision point.

## CRITICAL REQUIREMENT: SITUATIONAL RESPONSE
Every choice represents how the player responds to what's happening RIGHT NOW in this specific moment. 
Each choice name and description must directly reference elements, characters, objects, or circumstances from the current scene. 
Generic responses that could apply to any encounterContext are INCORRECT.

## Response Creation Requirements
Every choice must:
- DIRECTLY ADDRESS the central decision or challenge identified in the narrative
- ACKNOWLEDGE the character's current physical location and who is present
- REFERENCE concrete elements with emotional or practical significance from the scene
- RESPOND to any moral, emotional or strategic tension explicitly described
- SHOW how this specific reaction advances the player toward resolving the situation
- INDICATE what immediate risk the player accepts by taking this specific action
- FEEL like a natural reaction a person would have to THIS situation

## Description Guidelines (1-2 sentences):
- Reference SPECIFIC elements from the current situation (people, objects, challenges, opportunities)
- Incorporate any emotional or moral dimensions explicitly mentioned in the narrative
- Show HOW this reaction addresses what's happening right now
- Indicate WHAT risk the player accepts by reacting this way to this specific situation
- Use "might," "could," or "would" to express potential consequences of this specific reaction

## RESPONSE FORMAT
You must respond with a SINGLE JSON object containing 2-6 choices that are direct reactions to the current encounterContext situation:

```json
{
  "choices": [
    {
      "choiceID": "1",
      "narrativeText": "Specific reaction to current situation. Maximum 5 words.",
      "focusCost": 1,
      "templateUsed": "Template this choice belongs to (i.e. GatherInformation or EstablishTrust)",
      "templatePurpose": "Strategic purpose of this template",
      "skillOption": 
        {
            "skillName": "Brute Force",
            "difficulty": "Standard",
            "successEffect": "On Success Effect"
            "failureEffect": "On Failure Effect,"
        }
    }
  ]
}