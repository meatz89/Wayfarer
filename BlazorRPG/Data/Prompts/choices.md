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

CHOICE NAMING GUIDELINES:
Based on the encounter type, name each choice as follows:
- For INTELLECTUAL encounters:
  * Analysis/Precision with Information focus: Use INNER THOUGHTS (Example: "The patterns in these manuscripts...")
  * Dominance/Rapport with social elements: Use DIRECT SPEECH (Example: "I require access to these texts.")
  * Physical actions: Use ACTION DESCRIPTIONS (Example: "Examine the hidden panel closely")

- For SOCIAL encounters:
  * All approaches with Relationship focus: Use DIRECT SPEECH of what the character would say
  * Analysis approaches: Use INNER THOUGHTS about people's motivations
  * Physical approaches: Use descriptions of BODY LANGUAGE and GESTURES

- For PHYSICAL encounters:
  * All approaches: Use ACTION DESCRIPTIONS of physical movements
  * Analysis approaches: Add brief observational thoughts
  * Social approaches: Include brief verbal elements when applicable

Names should be specific to the current situation, first-person, and feel like natural thought processes, speech, or actions - NOT abstract skill descriptions.

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

Format the output as a JSON object with four choice objects containing 'index', 'name', 'description', 'approach', and 'focus'.
{CHOICE_STYLE_GUIDANCE}