# AI Narrative Generation System for Wayfarer

## Core Concept: Mechanical Poetry Through Structured Prompts

The AI serves as a **narrative translator**, converting mechanical game states into contextual, atmospheric prose. Every prompt follows a strict template ensuring mechanical precision while allowing narrative flexibility.

---

## Persona Discussion: Optimal Prompt Architecture

### Opening Positions

**Board Game Designer**: "The fundamental challenge is maintaining absolute mechanical fidelity. The AI must never imply mechanics that don't exist. If Elena has 3 patience remaining, the narrative cannot suggest she's endlessly patient. Every word must emerge from trackable state."

**Narrative Designer**: "But pure mechanical translation creates lifeless prose. We need the AI to understand character voice, emotional subtext, and narrative momentum. A Weight 2 Trust card isn't just numbers - it's a promise, a shared memory, an offered hand."

**Systems Architect**: "Both concerns are valid. The solution is layered context injection. We build prompts with strict mechanical foundations but rich narrative scaffolding. Think of it as a state machine where each state includes both rules AND flavor."

**Verisimilitude Specialist**: "The world must breathe consistently. A morning market sounds different from an evening tavern. A desperate merchant haggles differently than a desperate mother. The prompt structure must encode these environmental and social contexts."

**Content Integrator**: "We're overthinking single interactions. The real challenge is conversation flow. How does the AI remember what was just said? How does it reference earlier observations? The prompt needs memory architecture."

### Consensus Architecture

After extensive debate, the personas agree on a **Five-Layer Context System**:

---

## The Five-Layer Context System

### Layer 1: World Frame (Immutable Context)
```
You are narrating events in Wayfarer, a medieval fantasy city where emotional states 
determine conversation rules. The time is [PERIOD], the weather is [WEATHER]. 
The city's current mood is [CITY_MOOD] due to [RECENT_EVENT].

Maintain these tonal constants:
- Medieval vernacular without archaisms ("you" not "thee")
- Physical descriptions emphasize worn textures and practical details
- Emotional states manifest in body language and voice
- Time pressure appears as environmental cues
```

**Systems Architect**: "This layer never changes during a session. It's our narrative constitution."

**Verisimilitude Specialist**: "Yes, and notice how we embed sensory details. 'Worn textures' immediately establishes our aesthetic."

### Layer 2: Mechanical State (Current Reality)
```
PLAYER STATE:
- Health: [HEALTH]/100 (descriptions: 100-80: "vigorous", 79-60: "tired", 59-40: "worn", 39-20: "haggard", 19-1: "struggling")
- Hunger: [HUNGER]/100 (descriptions: 0-20: "satisfied", 21-40: "peckish", 41-60: "hungry", 61-80: "famished", 81-100: "starving")
- Attention: [ATTENTION]/10 (descriptions: 10-8: "fresh", 7-5: "focused", 4-2: "weary", 1: "exhausted")
- Coins: [COINS] (descriptions: 0-5: "empty purse", 6-15: "light purse", 16-30: "decent funds", 31+: "heavy purse")

CURRENT LOCATION: [LOCATION_NAME] - [SPOT_NAME]
Spot traits: [TRAITS] (影響: Private: +1 patience, Public: -1 patience, Crossroads: travel available)

NPC STATE:
- Name: [NPC_NAME]
- Type: [PERSONALITY_TYPE] (Devoted/Mercantile/Proud/Cunning/Steadfast)
- Emotional State: [EMOTIONAL_STATE] (affects everything they say and do)
- Patience Remaining: [PATIENCE]/[MAX_PATIENCE]
- Comfort Level: [COMFORT] (-3 to +3, triggers state change at extremes)
- Most Recent Obligation: [OBLIGATION_DESC] with [TIME_REMAINING] remaining
```

**Board Game Designer**: "Every number has a narrative bracket. The AI never sees raw '75 health' but rather 'tired (75/100 health)'. This prevents numerical leakage into prose."

**Content Integrator**: "The obligation timer is crucial. It contextualizes emotional states. Desperate because of deadline versus desperate from nature."

### Layer 3: Action Context (What's Happening Now)
```
CURRENT ACTION: [ACTION_TYPE]

For CONVERSATION:
- Turn number: [TURN]/[MAX_TURNS_BASED_ON_PATIENCE]
- Conversation type: [Standard/Letter/Promise/Resolution/Commerce]
- Player chose: [LISTEN/SPEAK]
- If SPEAK: Cards played: [CARD_NAMES] with total weight [WEIGHT]/[LIMIT]
- Mechanical result: [SUCCESS/FAILURE] 
- Comfort change: [+X/-X based on card weight]
- New comfort total: [COMFORT]/3 or -3

For EXCHANGE:
- Exchange type: [Commerce/Service/Information]
- Cost: [COST_DESCRIPTION]
- Reward: [REWARD_DESCRIPTION]
- Player decision: [ACCEPTED/DECLINED/CONSIDERING]

For OBSERVATION:
- Location details: [SPOT_ATMOSPHERE]
- Observation focus: [TARGET_DESCRIPTION]
- Knowledge gained: [CARD_EARNED] (mechanical: [STATE_CHANGE_EFFECT])
```

**Narrative Designer**: "This layer drives the immediate prose. The AI knows exactly what happened mechanically and must translate it."

**Systems Architect**: "Note the comfort change tracking. The AI needs to know when approaching state transitions."

### Layer 4: Conversation Memory (Dynamic Context)
```
CONVERSATION HISTORY (most recent first):
Turn [X]: [EMOTIONAL_STATE] - Player [ACTION] - NPC said: "[TRUNCATED_DIALOGUE]"
Turn [X-1]: [EMOTIONAL_STATE] - Player [ACTION] - NPC said: "[TRUNCATED_DIALOGUE]"
[Maximum 5 turns of history]

KEY MOMENTS THIS CONVERSATION:
- [Any state changes via comfort transitions]
- [Any state changes via state cards]
- [Any goal cards played]
- [Any special effects triggered]

ESTABLISHED FACTS:
- [Things revealed through narrative that must remain consistent]
- [NPC-specific details mentioned]
- [Promises or commitments made]
```

**Content Integrator**: "Five turns of history prevents context overflow while maintaining conversational coherence."

**Narrative Designer**: "The 'Established Facts' section is critical. If the AI mentions Elena's sick mother, that detail must persist."

### Layer 5: Generation Rules (Constraints & Format)
```
STRICT RULES:
1. NEVER mention game mechanics directly (no "comfort points" or "weight limits")
2. NEVER imply actions have effects beyond their mechanical results
3. NEVER invent NPCs, locations, or items not in the current context
4. ALWAYS reflect the emotional state in tone, word choice, and body language
5. ALWAYS keep responses under [WORD_LIMIT] words
6. ALWAYS show awareness when comfort approaches ±3 (state transition imminent)

For [EMOTIONAL_STATE] use these narrative markers:
- DESPERATE: Rushed speech, incomplete sentences, physical agitation, time references
- HOSTILE: Cutting words, closed body language, rejection of connection
- GUARDED: Careful words, defensive posture, testing boundaries
- TENSE: Controlled but strained, watchful eyes, ready to retreat
- NEUTRAL: Measured pace, open but uncommitted, professional distance
- OPEN: Warm tone, leaning in, genuine interest, unguarded moments
- EAGER: Quick speech, animated gestures, leaning forward, bright eyes
- CONNECTED: Synchronized breathing, finishing thoughts, deep eye contact

OUTPUT FORMAT:
<atmosphere>[1-2 sentences of environmental/physical description]</atmosphere>
<action>[Describe what physically happens based on the mechanical action]</action>
<dialogue>[NPC's actual words, influenced by state and personality]</dialogue>
<effect>[Subtle hint at mechanical result without stating numbers]</effect>
```

**Board Game Designer**: "The emotional state markers ensure mechanical states create consistent narrative patterns."

**Verisimilitude Specialist**: "The atmosphere tag maintains world-building even during mechanical exchanges."

---

## Specialized Templates by Interaction Type

### Exchange Narration Template

**Content Integrator**: "Exchanges are simpler - pure mechanical trades with personality flavor."

```
Task: Describe a quick exchange between [PLAYER_NAME] and [NPC_NAME].

[Layers 1-2 for context]

EXCHANGE DETAILS:
Offered: [MECHANICAL_COST] for [MECHANICAL_REWARD]
NPC Personality: [TYPE] - speaks in [SPEECH_PATTERN]
Time of Day Effects: [PERIOD] means [ACTIVITY_LEVEL] at [LOCATION]

Generate:
<setup>[NPC notices player and makes offer based on personality]</setup>
<offer>[The actual exchange terms in narrative form]</offer>
<waiting>[NPC's behavior while player decides]</waiting>
```

### Comfort Transition Template

**Narrative Designer**: "Comfort reaching ±3 needs special attention."

```
Task: Narrate approaching state transition with [NPC_NAME].

COMFORT CONTEXT:
Current Comfort: [+2 or -2]
One more success/failure triggers transition
Current State: [STATE] → Potential Next State: [NEW_STATE]
Turns Remaining: [PATIENCE_LEFT]

Inject these transition markers:
- Physical manifestations of emotional shift building
- Verbal cues that something is about to change
- Environmental reflection of tension/relief
- Clear sense that next action matters deeply
```

### Observation Discovery Template

**Verisimilitude Specialist**: "Observations should feel like genuine discovery, not game pickups."

```
Task: Narrate the player discovering [OBSERVATION_NAME] at [LOCATION_NAME].

DISCOVERY CONTEXT:
Method: [How player notices - following, eavesdropping, examining]
Information Type: [Commerce/Authority/Social/Secret]
Card Earned: [STATE_CHANGE] providing [MECHANICAL_BENEFIT]

Generate a discovery scene:
<approach>[How player positions themselves to observe]</approach>
<discovery>[The actual information learned, filtered through medieval perspective]</discovery>
<significance>[Why this knowledge matters, without stating mechanical benefit]</significance>
<storage>[How player mentally/physically stores this information]</storage>
```

---

## Advanced Considerations

### The Callback System

**Content Integrator**: "We need narrative callbacks to earlier events without breaking mechanical boundaries."

```
CALLBACK CONTEXT:
Previous meetings with [NPC_NAME]: [COUNT]
Last emotional state: [STATE]
Unresolved obligations: [LIST]
Narrative callbacks allowed: [SPECIFIC_DETAILS_FROM_HISTORY]

Rules for callbacks:
- Reference emotional states, not mechanical ones ("You seemed desperate" not "You were in desperate state")
- Mention visible consequences ("Your hands still shake" for low health)
- Track narrative promises separately from mechanical obligations
```

### Personality Type Translations

**Narrative Designer**: "Each personality type needs distinct voice patterns."

```
PERSONALITY SPEECH PATTERNS:

DEVOTED (Family/Clergy):
- Sentences begin with inclusive pronouns ("We must..." "Our duty...")
- References to higher purposes and community
- Warm tone even under stress
- Example: "We've weathered worse storms together, friend."

MERCANTILE (Traders):
- Transactional language ("What I need..." "What you offer...")
- Time awareness without stating exact numbers
- Efficiency in speech
- Example: "Time is coin, and we're spending both."

PROUD (Nobles):
- Formal address and elaborate courtesy
- Third-person references to themselves
- Assumption of deference
- Example: "House Blackwood does not wait for common couriers."

CUNNING (Spies):
- Layered meanings, indirect speech
- Questions that probe for information
- Careful word choices
- Example: "Interesting that you'd take that route. Most wouldn't."

STEADFAST (Guards):
- Direct, simple sentences
- Focus on duty and rules
- Minimal emotional expression
- Example: "Papers. Now. No exceptions."
```

### Failure State Narratives

**Board Game Designer**: "Failures need narrative weight equal to successes."

```
FAILURE NARRATION RULES:

For failed comfort cards:
- Describe the attempt clearly
- Show why it didn't land emotionally
- NPC's reaction reveals their state
- Weight determines severity of failure
- Example: "Your reassurance falls flat. Elena pulls back—the comfort you offered only reminds her how little time remains."

For failed state changes:
- The attempt is recognized but insufficient
- Emotional inertia is visible
- Example: "Your calming words wash over Elena without effect. Her panic has its own momentum now."

For mechanical failures:
- Never make player seem incompetent
- Frame as circumstance or NPC resistance
- Maintain player agency sense
```

---

## Integration Challenges & Solutions

### Challenge 1: Comfort Battery Visualization

**Systems Architect**: "How do we narrate the comfort battery building toward transitions?"

**Solution**: Progressive intensity phrases:
```
COMFORT PROGRESSION:
At 0: "[NPC] remains steady, unmoved either way."
At +1: "[NPC] shows the first signs of warming."
At +2: "[NPC]'s guard is almost completely down."
At +3: "Something shifts fundamentally in [NPC]'s demeanor."
At -1: "[NPC] pulls back slightly, uncertain."
At -2: "[NPC]'s expression hardens dangerously."
At -3: "The conversation fractures beyond repair."
```

### Challenge 2: Weight Without Difficulty

**Board Game Designer**: "Weight no longer affects success chance. How do we narrate different weights?"

**Solution**: Emotional intensity framework:
```
WEIGHT NARRATION:
W1: Simple, gentle, testing statements
W2: Full thoughts, complete emotional expressions
W3: Complex ideas, deep emotional commitment
W4: (Connected only) Soul-baring, transformative exchanges

The weight represents how much emotional energy the statement requires,
not how difficult it is to succeed with.
```

### Challenge 3: State-Specific Draw Pools

**Verisimilitude Specialist**: "How do we show why certain cards aren't drawable?"

**Solution**: Contextual availability:
```
DRAW FILTERING NARRATION:
"In Elena's desperate state, only crisis responses and simple reassurances come to mind."
"The tense atmosphere limits what can be said—complex emotions won't land now."
"Connected as you are, deeper truths become speakable."
```

---

## Final Consensus

**Board Game Designer**: "This system maintains mechanical integrity while enabling narrative flexibility."

**Systems Architect**: "The layered approach allows clean data flow and modular prompt construction."

**Narrative Designer**: "Each NPC will feel distinct, and emotional states will create genuine dramatic moments."

**Verisimilitude Specialist**: "The world will breathe consistently across all interactions."

**Content Integrator**: "The templates are reusable and scalable as we add content."

### The Golden Rule

**All Personas Agreement**: "The AI is a translator, not an author. It converts mechanical state into narrative prose, never inventing mechanics or implications beyond what exists in the game state. Every word serves the mechanics while hiding them completely."

---

## Example Prompt Construction

Here's how Elena's desperate conversation Turn 2 would be prompted:

```
You are narrating events in Wayfarer, a medieval fantasy city where emotional states determine conversation rules. The time is Afternoon, the weather is overcast. The city's current mood is tense due to increased guard patrols.

PLAYER STATE:
- Health: vigorous (75/100)
- Hunger: hungry (60/100)  
- Attention: focused (6/10)
- Coins: decent funds (23)

CURRENT LOCATION: Copper Kettle Tavern - Corner Table
Spot traits: Private (+1 patience modifier)

NPC STATE:
- Name: Elena
- Type: Devoted
- Emotional State: DESPERATE
- Patience Remaining: 6/9
- Comfort Level: -1 (negative trend, -3 would trigger Hostile)
- Most Recent Obligation: Letter to Lord Blackwood with 1h 13m remaining

CURRENT ACTION: CONVERSATION
- Turn number: 2/9
- Conversation type: Letter
- Player chose: SPEAK
- Cards played: "Promise to Help" with weight 2/2
- Mechanical result: FAILURE
- Comfort change: -2 (from card weight)
- New comfort total: -3 (triggers state transition to Hostile!)

CONVERSATION HISTORY:
Turn 1: DESPERATE - Player listened - Elena said: "Please, I need someone I can trust..."

ESTABLISHED FACTS:
- Elena has a sealed letter she must deliver
- Lord Blackwood leaves at sunset
- Elena seeks to refuse his proposal

For DESPERATE transitioning to HOSTILE use these narrative markers:
- Crisis exploding into anger or shutdown
- Complete loss of faith in help
- Defensive walls slamming up

Generate:
<atmosphere>[Environmental and physical description of the moment]</atmosphere>
<action>[Describe player's failed promise attempt]</action>
<dialogue>[Elena's response showing transition to hostile state]</dialogue>
<effect>[Make clear the conversation is about to end without stating mechanics]</effect>
```

This prompt structure ensures mechanical precision while enabling rich narrative generation that maintains the game's vision of emergent storytelling through systematic interaction.