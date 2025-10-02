# Wayfarer Playtesting Personas

## Purpose

These personas represent distinct player types for testing Wayfarer's conversation-based RPG mechanics. Each persona focuses on different aspects of the system to ensure comprehensive coverage during playtesting sessions.

---

## Persona 1: The Optimizer - "Min/Max Morgan"

### Player Profile
- **Gaming Background**: Extensive CCG experience (Slay the Spire, Monster Train, Dominion)
- **Playstyle**: Analytical, spreadsheet-driven, seeks optimal paths
- **Primary Motivation**: System mastery and efficiency

### Testing Focus
- **Initiative Economy**: Will they discover the builder/spender rhythm naturally?
- **Stat Specialization**: Can they identify which stats to focus for their preferred approach?
- **Queue Management**: Will they optimize delivery routes to minimize displacement costs?
- **Weight Optimization**: Can they find the optimal load balance between obligations and tools?

### Expected Behaviors
- Extensively tests Foundation card ratios to maximize Initiative generation
- Calculates exact momentum thresholds before attempting conversations
- Maps all routes to find segment-optimal paths
- Tracks token gains meticulously to unlock signature cards efficiently

### Key Questions to Answer
1. Is the optimal path discoverable through play or does it require external calculation?
2. Do the formulas provide enough information for strategic planning?
3. Are there degenerate strategies that break the intended difficulty curve?
4. Does mastery feel rewarding or just mathematical?

---

## Persona 2: The Roleplayer - "Narrative Nora"

### Player Profile
- **Gaming Background**: Story-focused RPGs (Disco Elysium, Baldur's Gate 3, visual novels)
- **Playstyle**: Character-driven, makes choices based on narrative consistency
- **Primary Motivation**: Emergent storytelling and character relationships

### Testing Focus
- **Verisimilitude**: Do conversations feel like real social interactions?
- **Personality Rules**: Are NPC personalities distinct and memorable?
- **Burden Cards**: Does relationship damage create meaningful narrative consequences?
- **ConversationalMove System**: Do Remarks/Observations/Arguments feel contextually appropriate?

### Expected Behaviors
- Chooses conversation options based on "what my character would say"
- Builds relationships with favorite NPCs rather than optimizing token spread
- Accepts suboptimal obligations to help NPCs they care about
- Values narrative coherence over mechanical efficiency

### Key Questions to Answer
1. Can players ignore optimal paths and still complete scenarios through character choices?
2. Do the mechanics support emergent narratives or feel like spreadsheet management?
3. Are personality rules intuitive or do they feel like arbitrary combat modifiers?
4. Does the conversation system create memorable moments or just resource puzzles?

---

## Persona 3: The Puzzle Solver - "Tactical Tessa"

### Player Profile
- **Gaming Background**: Puzzle games, tactical RPGs (Into the Breach, XCOM, puzzle-focused roguelikes)
- **Playstyle**: Methodical, enjoys solving discrete challenges with perfect information
- **Primary Motivation**: Mastering individual conversation puzzles

### Testing Focus
- **Cadence Management**: Can they discover the rhythm of SPEAK/LISTEN timing?
- **Doubt Timer**: Does the pressure create tension without feeling arbitrary?
- **Personality Puzzles**: Are personality rules distinct enough to change approach meaningfully?
- **Requirement Chains**: Can they build Statements in Spoken to enable powerful cards?

### Expected Behaviors
- Treats each conversation as a standalone puzzle to solve optimally
- Experiments with different card sequences to find efficient paths
- Maps out personality rule implications before attempting conversations
- Uses negative Cadence deliberately for card draw advantages

### Key Questions to Answer
1. Are conversations solvable with perfect play or does RNG interfere?
2. Do personality rules create distinct puzzles or just parameter tweaks?
3. Is the feedback loop clear enough to learn from mistakes?
4. Does Doubt create urgency or just feel like an artificial timer?

---

## Persona 4: The Explorer - "Discovery Dan"

### Player Profile
- **Gaming Background**: Open-world games, exploration-focused titles (Outer Wilds, Zelda)
- **Playstyle**: Curious, methodical, wants to discover all content
- **Primary Motivation**: Finding hidden systems and content

### Testing Focus
- **Investigation System**: Is location familiarity rewarding to build?
- **Observation Cards**: Do discoveries feel meaningful or like checklists?
- **Travel Path Discovery**: Is revealing face-down paths satisfying?
- **Stat-Gated Content**: Can they discover which stats unlock what content?

### Expected Behaviors
- Investigates every location to maximum familiarity before progressing
- Experiments with different stats to discover gated approaches
- Tries to find all observation cards and stranger encounters
- Tests edge cases and system boundaries

### Key Questions to Answer
1. Is there enough content to reward thorough exploration?
2. Are stat-gated paths discoverable or do they feel hidden?
3. Do observations provide meaningful advantages or just completionist satisfaction?
4. Does exploration compete healthily with time pressure or feel punishing?

---

## Persona 5: The Struggler - "First-Timer Felix"

### Player Profile
- **Gaming Background**: Limited CCG/deckbuilder experience, plays casually
- **Playstyle**: Learning as they go, makes intuitive rather than calculated choices
- **Primary Motivation**: Completing the scenario, experiencing the story

### Testing Focus
- **Tutorial Effectiveness**: Can they understand core mechanics without external help?
- **Soft-Lock Prevention**: Do escape valves work when they make mistakes?
- **Difficulty Curve**: Is the learning curve too steep for casual players?
- **UI Clarity**: Are resources and options clearly communicated?

### Expected Behaviors
- Mismanages Initiative in early conversations (tries to play high-cost cards without buildup)
- Accepts too many heavy obligations without considering weight
- Misses deadlines due to poor time management
- Doesn't optimize stat development or token distribution

### Key Questions to Answer
1. Can struggling players still complete scenarios or do they hit unwinnable states?
2. Are failure states clear or do players feel confused about what went wrong?
3. Does the work system provide enough bailout options for resource mistakes?
4. Are foundation mechanics (Foundation cards, basic work, simple exchanges) accessible enough?

---

## Testing Protocol

### Session Structure

**Phase 1: Cold Start (30 minutes)**
- No tutorial or instructions beyond basic UI controls
- Observe natural discovery of mechanics
- Note confusion points and intuitive leaps

**Phase 2: Guided Play (45 minutes)**
- Answer questions but don't provide optimal strategies
- Let players develop their own approaches
- Track which mechanics click vs which remain opaque

**Phase 3: Reflection (15 minutes)**
- Structured interview about experience
- Identify pain points and moments of delight
- Gather suggestions for clarity improvements

### Key Metrics to Track

**For All Personas:**
- Time to understand Initiative builder/spender dynamic
- Number of failed conversations before first success
- Whether they discover LISTEN's card draw benefit
- If they manage weight capacity proactively or reactively

**Optimizer-Specific:**
- Time to discover optimal Foundation card ratios
- Whether they calculate exact paths or iterate
- If they find unintended exploits

**Roleplayer-Specific:**
- Whether they reference NPC personalities in decision-making
- Frequency of narrative-based choices over optimal ones
- Emotional reactions to relationship damage

**Puzzle Solver-Specific:**
- Conversations attempted before first perfect solve
- Whether they map out personality rules explicitly
- Use of Cadence manipulation for advantage

**Explorer-Specific:**
- Percentage of content discovered before scenario completion
- Whether they prioritize investigation over efficiency
- Discovery of stat-gated content without hints

**Struggler-Specific:**
- Frequency of hitting soft-lock prevention systems
- Whether they use work system as intended bailout
- Understanding of cause-effect for failures

---

## Success Criteria

### Critical (Must Pass for All Personas)
- No unwinnable states encountered (soft-lock prevention works)
- Core loop (Foundation → Standard → Goals) understood within 2 conversations
- At least one successful obligation completion
- No complete confusion about resource purposes

### Important (Should Pass for Most Personas)
- Personality rules recognized as meaningful (not just flavor)
- Weight management understood before hitting capacity
- Time pressure felt but not overwhelming
- Token system value recognized

### Nice-to-Have (Persona-Dependent)
- Optimal paths discovered (Optimizer, Puzzle Solver)
- Emergent narratives created (Roleplayer)
- Content discovery satisfying (Explorer)
- Struggles overcome through learning (Struggler)

---

## Post-Test Analysis Questions

### Mechanical Clarity
1. Which resources were immediately understandable vs confusing?
2. Did the conversation flow feel like real dialogue or card combat?
3. Were personality rules intuitive or did they feel arbitrary?
4. Could players predict card effects before playing them?

### Strategic Depth
1. Did players discover multiple valid approaches or converge on one?
2. Were trade-offs meaningful or were some choices clearly superior?
3. Did stat progression feel like character growth or just power creep?
4. Were failures instructive or just frustrating?

### Pacing and Difficulty
1. Did time pressure create tension or just stress?
2. Were early conversations appropriately tutorial-like?
3. Did difficulty ramp smoothly or spike unpredictably?
4. Could struggling players find paths forward?

### Emotional Engagement
1. Did players care about NPCs or just see them as mechanics?
2. Were conversation victories satisfying?
3. Did relationship damage feel meaningful?
4. Were there memorable emergent moments?

### UI and Presentation
1. Could players find needed information without hunting?
2. Were calculations visible enough for planning?
3. Did feedback clearly communicate success/failure?
4. Were options well-organized or overwhelming?

---

## Iteration Priorities Based on Persona Feedback

### If Optimizer finds exploits:
- Review formulas for unintended synergies
- Adjust caps or costs to prevent degenerate strategies
- Ensure multiple builds remain viable

### If Roleplayer feels disconnected:
- Enhance personality rule presentation
- Add more narrative context to mechanical choices
- Strengthen conversation verisimilitude

### If Puzzle Solver can't find solutions:
- Improve feedback on why cards failed
- Clarify personality rule implications
- Better telegraph optimal sequencing

### If Explorer finds content lacking:
- Add more observations and hidden paths
- Ensure stat-gated content is discoverable
- Balance exploration rewards with time costs

### If Struggler hits walls:
- Strengthen tutorial messaging
- Improve soft-lock prevention visibility
- Add more graduated difficulty in early content
- Clarify cause-effect for failures

---

These personas ensure comprehensive coverage of Wayfarer's systems while representing realistic player approaches to the game's unique conversation-based mechanics.
