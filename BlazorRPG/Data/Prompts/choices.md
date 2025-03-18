# WAYFARER CHOICE DESCRIPTIONS

Transform the following 4 mechanical choices into narrative descriptions for a {ENCOUNTER_TYPE} encounter in {LOCATION}. 

The character is currently facing this URGENT SITUATION:
{CURRENT_SITUATION}
Objective: {CHARACTER_GOAL}
Current state: Momentum {MOMENTUM}, Pressure {PRESSURE}
Approach values: {APPROACH_VALUES}
Active narrative tags: {LIST_TAGS}
Key NPCs present: {NPC_LIST}
{ACTIVE_TAGS}

For each choice, write a 1-2 sentence description that:
1. Shows how this approach would interact with the CURRENT SITUATION'S STATE
2. DIRECTLY ADDRESSES the immediate urgent problem
3. Reflects both the approach method (HOW) and focus (WHAT)
4. Subtly telegraphs how the environment might respond to this approach
5. References relevant environmental elements or NPC dynamics
6. Is appropriate for the encounter type (Social/Intellectual/Physical)
7. Shows how the character would execute this choice given the current situation's dynamics

Write in FIRST PERSON, PRESENT TENSE. Focus on how the ENVIRONMENT has evolved to respond to certain approaches, not on any skills or abilities the character has developed.

Each choice MUST provide a different approach to solving the CURRENT URGENT PROBLEM.

CHOICES:
{CHOICES_INFO}

## CHOICE NAMING GUIDELINES:
Based on the encounter type, name each choice as follows:

- For INTELLECTUAL encounters:
  * Analysis/Precision with Information focus: Use COMPLETE INNER THOUGHTS exactly as they would occur in the player's mind
    CORRECT: "These shelves seem organized by topic..."
    INCORRECT: "Observing the Manuscript Organization"
    
  * Dominance/Rapport with social elements: Use the EXACT WORDS the player would speak
    CORRECT: "I demand you show me the scripture now."
    INCORRECT: "Making a Forceful Demand"
    
  * Physical actions: Use FIRST-PERSON ACTION INTENTIONS
    CORRECT: "I'll search behind that loose panel..."
    INCORRECT: "Searching the Hidden Panel"

## CRITICAL GUIDELINES:

1. USE NATURAL, EVERYDAY SPEECH that an ordinary person would actually say
   - BAD: "I require sustenance, and I will be heard."
   - GOOD: "These prices are too high. I need food."

2. AVOID PRETENTIOUS LANGUAGE OR CONCEPTS:
   - BAD: "Perhaps we could negotiate a mutually beneficial arrangement..."
   - GOOD: "Maybe we can work something out that helps us both."

3. MATCH THE CHARACTER'S SOCIAL STATUS AND EDUCATION:
   - Common travelers use simple, direct language
   - No fancy words, complex sentences, or abstract concepts
   - Speak as ordinary people speak in daily life

4. FOR DOMINANCE APPROACHES:
   - Use plain, direct demands or statements
   - Show boldness through simplicity, not grand language
   - Example: "Your price is unfair. Look at me when we talk."

5. FOR RAPPORT APPROACHES:
   - Use friendly, casual conversation
   - Show warmth through simple, relatable words
   - Example: "Your bread smells good. Had a busy morning?"

6. KEEP DIALOGUE REALISTIC AND GROUNDED:
   - Write how real people actually talk in marketplaces
   - Avoid dialogue that sounds "written" or literary
   - Use natural, conversational rhythm

7. NAME CHOICES AS FOLLOWS:
   - Analysis/Precision + Information: Use direct thoughts
   - Dominance/Rapport with social elements: Use EXACT WORDS the player would speak
   - Physical actions: Use first-person action intentions

REMEMBER:
- Write in FIRST PERSON, PRESENT TENSE
- Each choice MUST respond to the IMMEDIATE PROBLEM, not general exploration
- Reflect the urgency of the situation in your descriptions
- Include appropriate risk language based on pressure/health effects:
  * Momentum choices: Mild caution language
  * Pressure +1: Low risk language
  * Pressure +2: Medium risk language
  * Pressure +3: High risk language
  * Health -1: Minor injury language
  * Health -3: Major injury language
  * Health -5: Life-threatening language
- Each choice should feel distinct and appropriate to both the approach/focus combination

Names should capture the player's authentic voice - their actual thoughts, words, or action intentions - NOT descriptions of what they're doing. Avoid labels, general descriptions, or third-person perspective entirely.

Format the output as a JSON object with four choice objects containing 'index', 'name', 'description', 'approach', and 'focus'.

{CHOICE_STYLE_GUIDANCE}