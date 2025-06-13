IMPORTANT: Generate ONLY the raw choice content with no meta-commentary, JSON formatting, or structural elements.

# WAYFARER'S RESOLVE CHOICE GENERATION
{PROMPT_CONTEXT}

## Period Authenticity:
- Medieval character perspective appropriate to the current situation
- Concrete reactions to visible, tangible elements of the scene
- Experience-based responses to the specific challenge at hand
- Avoid modern analytical terms or abstract strategic thinking

## CRITICAL REQUIREMENT: CURRENT SITUATION ANALYSIS
Before creating choices, analyze the narrative context INTERNALLY:
1. What is the character's current physical location and immediate surroundings?
2. What opportunities or elements are currently available to interact with?
3. Has the previous dramatic moment concluded or moved on?
4. What would a person naturally do next in this specific setting?

## SITUATIONAL FLEXIBILITY
- If a specific interaction or moment has naturally concluded, choices should reflect new available options
- Don't artificially prolong resolved tensions or force continued focus on elements that have moved on
- Allow the player to naturally pivot to other opportunities in the current environment
- Choices should feel organic to what's actually happening NOW, not what happened moments ago

## Response Creation Requirements
Every choice must:
- Reflect what's currently available and relevant in the immediate scene
- Feel like natural next steps a person would consider in this environment
- Reference elements that are still active and present
- Allow for organic scene progression rather than forced continuation
- Show realistic options for advancing the character's goals

## Description Guidelines (1-2 sentences):
- Reference elements that are currently relevant and available
- Focus on forward momentum rather than dwelling on concluded moments
- Show practical next steps appropriate to the setting
- Use "might," "could," or "would" to express potential consequences

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