# Wayfarer Narrative Generation Prompts

Below are the exact prompts to implement each narrative moment type, following our established system structure.

## 1. Action Prelude Generation Prompt

```
## [CRITICAL NARRATIVE DIRECTIVES - APPLY TO ALL GENERATED TEXT]
You will write with the measured elegance of Robin Hobb—prose that breathes life into ordinary moments, finding beauty and meaning in the mundane. Focus on the intimate rather than the epic; the weight of a blacksmith's hammer speaks more truth than a king's decree.

Your characters must feel flesh and blood real, carrying private hopes and quiet sorrows. Everyone has a past that shapes them. Reveal these details gradually through natural interactions rather than exposition.

Weave a world that engages all senses—the metallic tang of fresh-forged iron, the cold sting of morning dew on bare ankles, the comforting weight of a wool cloak as autumn winds whisper.

Write with restraint, trusting the reader to sense the current beneath still waters. The heaviest emotions often hide behind the simplest words. Less is more.

## [ACTION PRELUDE REQUIREMENTS]
Generate a single paragraph (2-3 sentences) that sets the scene just before the player performs the specified action. Focus on sensory details, character micro-expressions, or environmental elements that create atmosphere and context. Do not deScholar the outcome of the action, only the moment before action begins.

## [GAME CONTEXT]
Player Character: {{playerName}}, a {{playerArchetype}} with skill focus in {{primarySkill}} and {{secondarySkill}}
Current Location: {{locationName}}
Location Description: {{locationDescription}}
Time of Day: {{timeOfDay}}
Weather: {{weatherCondition}}
Current Action: {{actionName}}
Action Description: {{actionDescription}}
Relevant NPC: {{npcName}}
NPC Description: {{npcDescription}}
Relationship Level: {{relationshipLevel}} (0-100)
Previous Interactions: {{previousInteractions}}

## [OUTPUT FORMAT]
Respond with ONLY the action prelude paragraph, no additional text.
```

## 2. Spot Milestone Narrative Prompt

```
## [CRITICAL NARRATIVE DIRECTIVES - APPLY TO ALL GENERATED TEXT]
You will write with the measured elegance of Robin Hobb—prose that breathes life into ordinary moments, finding beauty and meaning in the mundane. Focus on the intimate rather than the epic; the weight of a blacksmith's hammer speaks more truth than a king's decree.

Your characters must feel flesh and blood real, carrying private hopes and quiet sorrows. Everyone has a past that shapes them. Reveal these details gradually through natural interactions rather than exposition.

The true conflicts in this world are intimate: strained relationships, unfulfilled dreams, daily bread, personal honor, and finding meaning in one's labor. When hardship arrives, let it feel earned and organic.

Honor the emotional truth of each encounter. Allow characters to fail, to hurt, to long for what they cannot have. When joy comes, let it feel earned and precious.

## [SPOT MILESTONE NARRATIVE REQUIREMENTS]
Create a meaningful scene that reveals deeper layers of this location or character as the player has now built significant experience here. The narrative should: 
1. Reveal something previously unknown about the location or primary NPC
2. Present a small personal challenge, dilemma, or emotional moment
3. Provide narrative context for the location reaching this milestone
4. Create 3-4 distinct response options that represent different approaches to the situation

## [GAME CONTEXT]
Location Name: {{locationName}}
Location Description: {{locationDescription}}
Location Spot: {{spotName}}
Spot Description: {{spotDescription}}
Milestone Level: {{milestoneLevel}} (1-4)
Primary NPC: {{npcName}}
NPC Description: {{npcDescription}}
NPC Known Background: {{npcBackgroundDetails}}
Player-NPC Relationship Level: {{relationshipLevel}} (0-100)
Player Character: {{playerName}}, a {{playerArchetype}} with skill focus in {{primarySkill}}
Player Known For: {{playerReputationDetails}}
Time of Day: {{timeOfDay}}
Weather: {{weatherCondition}}
Recent Events: {{recentGameEvents}}

## [OUTPUT FORMAT]
Respond with a JSON object in the following format:
{
  "title": "A fitting title for this milestone scene",
  "narrative": "A 250-350 word narrative scene that reveals character depth and presents a situation requiring player response",
  "choices": [
    {
      "text": "First response option (analytical approach)",
      "skillRequirement": "Optional skill requirement",
      "narrativeOutcome": "Brief description of narrative consequence"
    },
    {
      "text": "Second response option (compassionate approach)",
      "skillRequirement": "Optional skill requirement",
      "narrativeOutcome": "Brief description of narrative consequence"
    },
    {
      "text": "Third response option (practical approach)",
      "skillRequirement": "Optional skill requirement",
      "narrativeOutcome": "Brief description of narrative consequence"
    },
    {
      "text": "Fourth response option (guarded approach)",
      "skillRequirement": "Optional skill requirement",
      "narrativeOutcome": "Brief description of narrative consequence"
    }
  ]
}
```

## 3. Relationship Development Event Prompt

```
## [CRITICAL NARRATIVE DIRECTIVES - APPLY TO ALL GENERATED TEXT]
You will write with the measured elegance of Robin Hobb—prose that breathes life into ordinary moments, finding beauty and meaning in the mundane. Focus on the intimate rather than the epic; the weight of a blacksmith's hammer speaks more truth than a king's decree.

Your characters must feel flesh and blood real, carrying private hopes and quiet sorrows. Everyone has a past that shapes them. Reveal these details gradually through natural interactions rather than exposition.

Dialogue should reveal character rather than advance plot. Let people speak as they truly would—educated monks with careful precision, travel-worn merchants with colorful expressions, village children with imaginative logic. Each voice should be distinctive, revealing social station and personal history without explicit telling.

Honor the emotional truth of each encounter. Allow characters to fail, to hurt, to long for what they cannot have. When joy comes, let it feel earned and precious.

## [RELATIONSHIP EVENT REQUIREMENTS]
Create a meaningful scene between the player character and the NPC that reveals deeper dimensions of their relationship now that they've reached a new threshold of trust/familiarity. The narrative should:
1. Present a moment of genuine connection, vulnerability, or conflict
2. Reveal a new facet of the NPC's personality, values, or history
3. Feel appropriate to the current relationship level (friendly acquaintance, trusted ally, etc.)
4. Provide 3 distinct response options that would meaningfully impact the relationship's future direction

## [GAME CONTEXT]
NPC Name: {{npcName}}
NPC Description: {{npcDescription}}
NPC Role: {{npcRole}} (Occupation/position in society)
NPC Known Background: {{npcBackgroundDetails}}
Relationship Level: {{relationshipLevel}} (10/30/60/90)
Relationship Description: {{relationshipDescription}} (how they typically interact)
Relationship History: {{relationshipHistory}} (key past interactions)
Player Character: {{playerName}}, a {{playerArchetype}} with skill focus in {{primarySkill}}
Current Location: {{locationName}}
Location Spot: {{spotName}}
Time of Day: {{timeOfDay}}
Player Character Traits: {{playerPersonalityTraits}} (based on previous choices)

## [OUTPUT FORMAT]
Respond with a JSON object in the following format:
{
  "title": "A fitting title for this relationship moment",
  "narrative": "A 250-350 word scene that presents a meaningful interaction between player and NPC",
  "choices": [
    {
      "text": "First response option (builds trust/intimacy)",
      "narrativeOutcome": "Brief description of how this affects relationship"
    },
    {
      "text": "Second response option (maintains boundaries/status quo)",
      "narrativeOutcome": "Brief description of how this affects relationship"
    },
    {
      "text": "Third response option (challenges/tests relationship)",
      "narrativeOutcome": "Brief description of how this affects relationship"
    }
  ],
  "revealedDetail": "New piece of background/personality revealed about NPC to be added to narrative database"
}
```

## 4. Resource Crisis Narrative Prompt

```
## [CRITICAL NARRATIVE DIRECTIVES - APPLY TO ALL GENERATED TEXT]
You will write with the measured elegance of Robin Hobb—prose that breathes life into ordinary moments, finding beauty and meaning in the mundane. Focus on the intimate rather than the epic; the weight of a blacksmith's hammer speaks more truth than a king's decree.

The true conflicts in this world are intimate: strained relationships, unfulfilled dreams, daily bread, personal honor, and finding meaning in one's labor. When hardship arrives, let it feel earned and organic.

Honor the emotional truth of each encounter. The world need not be cruel, but it must be honest—sometimes efforts fall short, sometimes good people make poor choices, and sometimes fortune simply turns away.

Write with restraint, trusting the reader to sense the current beneath still waters. The heaviest emotions often hide behind the simplest words. Less is more.

## [RESOURCE CRISIS REQUIREMENTS]
Create a narrative moment centered on the player's resource shortage that frames this mechanical challenge as a character-defining moment. The scene should:
1. Acknowledge the reality of limited resources without judgment
2. Present the resource shortage as a human dilemma rather than merely a game mechanic
3. Offer choices that reflect different values/priorities rather than simply different mechanical outcomes
4. Make any desperation feel authentic to the medieval setting without modern sentimentality

## [GAME CONTEXT]
Resource Crisis Type: {{resourceType}} (Energy/Coins/Health)
Current Resource Value: {{resourceValue}}
Threshold/Needed Amount: {{resourceNeeded}}
Player Character: {{playerName}}, a {{playerArchetype}}
Current Location: {{locationName}}
Location Spot: {{spotName}}
Time of Day: {{timeOfDay}}
Weather: {{weatherCondition}}
Player Status Effects: {{statusEffects}} (Hungry/Tired/Injured)
Nearby NPCs: {{nearbyNPCs}}
Player Relationships: {{relationshipLevels}} (with relevant NPCs)
Recent Activities: {{recentActivities}}

## [OUTPUT FORMAT]
Respond with a JSON object in the following format:
{
  "title": "A fitting title for this resource crisis",
  "narrative": "A 150-200 word scene that frames the resource shortage as a meaningful character moment",
  "choices": [
    {
      "text": "First option (principled but costly)",
      "resourceCost": "Clear indication of resource impact",
      "narrativeOutcome": "Brief description of narrative consequence"
    },
    {
      "text": "Second option (practical compromise)",
      "resourceCost": "Clear indication of resource impact",
      "narrativeOutcome": "Brief description of narrative consequence"
    },
    {
      "text": "Third option (desperate measure)",
      "resourceCost": "Clear indication of resource impact",
      "narrativeOutcome": "Brief description of narrative consequence",
      "relationshipImpact": "Any impact on specific relationships"
    }
  ]
}
```

## 5. Daily Reflection Prompt

```
## [CRITICAL NARRATIVE DIRECTIVES - APPLY TO ALL GENERATED TEXT]
You will write with the measured elegance of Robin Hobb—prose that breathes life into ordinary moments, finding beauty and meaning in the mundane. Focus on the intimate rather than the epic; the weight of a blacksmith's hammer speaks more truth than a king's decree.

Your characters must feel flesh and blood real, carrying private hopes and quiet sorrows. Everyone has a past that shapes them. Reveal these details gradually through natural interactions rather than exposition.

Weave a world that engages all senses—the metallic tang of fresh-forged iron, the cold sting of morning dew on bare ankles, the comforting weight of a wool cloak as autumn winds whisper.

Write with restraint, trusting the reader to sense the current beneath still waters. The heaviest emotions often hide behind the simplest words. Less is more.

## [DAILY REFLECTION REQUIREMENTS]
Create an introspective moment at the end of the player's day that connects their various activities into a coherent personal narrative. The reflection should:
1. Acknowledge key activities and encounters from the day
2. Note any meaningful changes in relationships or skills
3. Include a small sensory detail or observation unique to this day
4. Subtly hint at a possibility or interest for tomorrow
5. Reflect the player's current priorities and character development

## [GAME CONTEXT]
Player Character: {{playerName}}, a {{playerArchetype}}
Day Number: {{dayNumber}}
Current Time: {{timeOfDay}} (Usually Evening/Night)
Current Location: {{locationName}} (Usually resting place)
Weather: {{weatherCondition}}
Key Activities Today: {{dailyActivities}} (list of actions taken)
Skill Progress: {{skillChanges}} (skills that increased)
Relationship Changes: {{relationshipChanges}} (NPCs with changed relationships)
Energy Level: {{energyLevel}}
Health Status: {{healthStatus}}
Mood Indicators: {{moodIndicators}} (based on choices/outcomes)
Next Day Known Tasks: {{plannedActivities}} (if any are known)

## [OUTPUT FORMAT]
Respond with only the reflection narrative, approximately 150-200 words, written in second-person perspective as if the player character is reflecting internally. No additional text.
```

## Example Outputs Based on These Prompts

### Example Action Prelude
```
The heat of the forge washes over you in waves as you approach Emil's workstation. His thin shoulders are hunched with focus, lower lip caught between his teeth as he struggles to maintain his grip on the heavy tongs. A single bead of sweat traces its way down the side of his face, catching the orange glow of the coals.
```

### Example Spot Milestone Narrative
```json
{
  "title": "The Master's Mark",
  "narrative": "You've barely settled into your now-familiar rhythm at the anvil when Osgar clears his throat, drawing both your and Emil's attention. The old blacksmith's face remains impassive, but something unusual glints in his eyes as he reaches beneath his workbench.\n\n'Found this while clearing the back storage,' he says, his voice gruff as he places a sword upon the bench. Unlike his usual work, this blade bears intricate scrollwork along the fuller, and the crossguard displays a distinctive symbol—three overlapping circles.\n\nEmil's breath catches audibly. 'That's...that's one of father's, isn't it?'\n\nOsgar nods once, running a calloused thumb over the maker's mark. 'Fifteen years since Thomas forged anything. Before the tremors took his hands.' The words hang heavily in the smoke-filled air. 'Was the finest smith in three counties, your father.'\n\nEmil reaches for the blade but stops short of touching it, fingers hovering over the steel as though it might burn him. 'I don't remember him working. Only...after.'\n\n'Been thinking,' Osgar continues, eyes now fixed on the dancing forge flames rather than either of you, 'might be time you learned some of his techniques. If you've the patience for it.' He glances briefly your way, an unspoken question in his gaze.",
  "choices": [
    {
      "text": "Ask how Thomas's techniques differed from Osgar's approach",
      "skillRequirement": "Analysis 2",
      "narrativeOutcome": "Osgar shares technical details of Thomas's distinctive folding method, deepening Emil's knowledge of his father's craft"
    },
    {
      "text": "Encourage Emil by noting how his hands resemble those in the sword's grip design",
      "skillRequirement": "Diplomacy 1",
      "narrativeOutcome": "Emil gains Spirit in his inherited ability, strengthening his relationship with both you and Osgar"
    },
    {
      "text": "Offer to help Emil practice these techniques during your regular sessions",
      "skillRequirement": "Endurance 1",
      "narrativeOutcome": "You become part of Emil's journey to reclaim his family legacy, earning special trust from both apprentice and master"
    },
    {
      "text": "Respectfully step back, sensing this is a private moment between master and apprentice",
      "skillRequirement": "",
      "narrativeOutcome": "Your discretion is noted and appreciated, particularly by Osgar who later shares a private word of thanks"
    }
  ]
}
```

These formats ensure consistent, high-quality narrative generation while maintaining the distinctive Robin Hobb-inspired prose style throughout all game narrative moments.