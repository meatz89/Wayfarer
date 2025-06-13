IMPORTANT: Generate ONLY the raw choice content with no meta-commentary, JSON formatting, or structural elements.

# WAYFARER'S RESOLVE CHOICE GENERATION
{PROMPT_CONTEXT}

## Period Authenticity:
- Medieval character perspective appropriate to the current situation
- Concrete reactions to visible, tangible elements of the scene
- Experience-based responses to the specific challenge at hand
- Avoid modern analytical terms or abstract strategic thinking

## CRITICAL REQUIREMENT: IMMEDIATE CONTEXT CONSTRAINTS
Before creating choices, analyze the narrative context INTERNALLY:
1. What is the character's current physical location and immediate surroundings?
2. What people, objects, or elements are physically present RIGHT NOW?
3. What can the character do in the NEXT FEW SECONDS in this exact spot?
4. What immediate interactions are available with current scene elements?

## TEMPORAL AND SPATIAL BOUNDARIES
- ALL choices must happen in the current location within the next few minutes
- NO choices about leaving the location, traveling elsewhere, or long-term planning
- NO choices about what to do "later" or "tomorrow" 
- Focus on immediate actions: speaking to someone present, examining something visible, moving to a different part of the same room
- Choices should be about HOW to engage with what's already established in the scene

## SITUATIONAL FLEXIBILITY  
- If a specific interaction or moment has naturally concluded, choices should reflect new available options within the same space
- Don't artificially prolong resolved tensions, but stay in the current physical context
- Allow the player to pivot between different elements/people in the current scene
- Choices should feel organic to what's immediately available

## Response Creation Requirements
Every choice must:
- Involve immediate actions with currently present elements (people, objects, opportunities)
- Stay within the current physical location and timeframe
- Feel like natural next steps someone would take in this exact moment
- Reference specific elements that are actively present in the scene
- Show immediate, short-term consequences rather than long-term outcomes

## Description Guidelines (1-2 sentences):
- Reference specific people or objects currently present in the scene
- Focus on immediate actions and their likely immediate results
- Show what happens in the next few seconds/minutes
- Use "might," "could," or "would" to express immediate potential consequences

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
      "requiresSkillCheck": "(true or false) Does this choice require a skill check?", 
      "successEffect": "On Success Effect",
      "failureEffect": "On Failure Effect"
      "skillOption": 
        {
            "skillName": "Brute Force",
            "difficulty": "Standard",
        }
    }
  ]
}