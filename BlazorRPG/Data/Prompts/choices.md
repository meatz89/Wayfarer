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

Format the output as a JSON object with four choice objects containing 'index', 'name', 'description', 'approach', and 'focus'.

{CHOICE_STYLE_GUIDANCE}