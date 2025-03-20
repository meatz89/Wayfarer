Generate narrative choices

ENCOUNTER STATUS:
- Encounter Type: {ENCOUNTER_TYPE} (Physical/Social/Intellectual)
- Turn: {CURRENT_TURN} of {MAX_TURNS}
- Momentum: {CURRENT_MOMENTUM}/{MAX_MOMENTUM} (Success threshold: {SUCCESS_THRESHOLD})
- Pressure: {CURRENT_PRESSURE}/{MAX_PRESSURE} (Failure threshold)
- Health: {CURRENT_HEALTH}/{MAX_HEALTH}
- Confidence: {CURRENT_CONFIDENCE}/{MAX_CONFIDENCE}
- Concentration: {CURRENT_CONCENTRATION}/{MAX_CONCENTRATION}
- Proximity to End: {ENCOUNTER_STAGE} (Early/Middle/Late)

LOCATION STRATEGIC INFORMATION:
- Favored Approaches: {FAVORABLE_APPROACHES} (Increase momentum/Reduce pressure)
- Dangerous Approaches: {DANGEROUS_APPROACHES} (Increase pressure/Reduce momentum)
- Active Narrative Tags: {ACTIVE_TAGS}

CURRENT SITUATION:
- Player Character (PC) State: {INJURIES/RESOURCES/CONDITION}
- Character Goal: {CHARACTER_GOAL}

NPC CHARACTERIZATION:
- Each choice should consider how present NPCs will respond based on their established motivations
- Show present NPCs as independent actors with their own agendas, not just reactive elements

CHOICES TO TRANSFORM:
Below are mechanical representations of four choices. Transform these into narrative descriptions while preserving their mechanical essence:

{CHOICES_INFO}

CHOICE FORMAT GUIDELINES:
For each choice, create:
1. A NAME using the format based on encounter type:
   - Physical Encounters: Action-oriented thoughts ("I'll charge at him...")
   - Intellectual Encounters: Observational thoughts ("Those symbols match the ones...")
   - Social Encounters: Direct speech in quotation marks ("Let me help with that...")
2. A DESCRIPTION (1-2 sentences) explaining what would happen if the player took this action and how NPCs might respond

Format the output as a JSON object with four choice objects containing 'index', 'name', 'description', 'approach', and 'focus'.