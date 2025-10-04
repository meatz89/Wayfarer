# Wayfarer: Strategic-Tactical Layer Architecture

## Executive Summary

Wayfarer integrates visual novel narrative presentation with simulator mechanics depth through a two-layer architecture inspired by XCOM. The strategic layer provides the world navigation hub where players exist at locations and initiate activities. The tactical layer provides card-based challenge resolution systems where moment-to-moment gameplay occurs. Three distinct tactical systems (Social, Mental, Physical) handle different challenge types while sharing unified mechanical patterns. Investigation and Travel function as strategic activities that compose tactical challenges rather than being systems themselves.

This architecture solves the fundamental design problem: how to make visual novel storytelling and simulation mechanics work together naturally rather than competing for player attention.

---

## Design Context and Motivation

### The Core Problem

Visual novels and simulation games have fundamentally different structures. Visual novels are narrative-driven with branching choices leading to story outcomes. Simulators are systems-driven with resource management and optimization loops. When combined naively, one overshadows the other - either mechanics become gates to story progression (narrative wins, mechanics feel perfunctory) or story becomes flavor text for optimization puzzles (mechanics win, narrative feels disconnected).

Wayfarer aims for genuine integration where mechanics express narrative and narrative contextualizes mechanics. The conversation system demonstrates this working: tactical card play isn't separate from dialogue, it IS the dialogue system. Initiative and Momentum aren't abstract numbers, they represent conversational dynamics. The challenge is extending this integration to investigation, travel, and other gameplay.

### The Visual Novel Constraint

The game presents like a visual novel: situation description or dialogue, followed by player choosing from possible responses, followed by resulting reaction and state change, then next situation. This flow must feel natural and maintain narrative immersion. However, underneath this presentation lives complex mechanical simulation tracking resources, stats, relationships, world state, and more.

For AI-generated text to work properly, every choice must have deterministic mechanical effects. The AI generates narrative flavor from game state, but the state changes themselves are mechanically defined. This means cards must carry both mechanical effects (what changes in game state) and narrative context (what this choice represents in story).

### The Integration Goal

Mechanics should function as the grammar of narrative choice. When you play a Cunning card in conversation, you're not thinking "I'll spend Initiative to gain Momentum" - you're thinking "I'll use an indirect approach to convince them." The mechanical layer structures what's possible narratively. The narrative layer makes mechanical choices meaningful contextually.

This integration must extend beyond conversations to investigations, travel, and any challenge the player faces. The architecture must support this unified vision.

---

## Two-Layer Architecture

### Strategic Layer: The World Hub

The strategic layer is where the player exists in the game world. This layer handles:

**Location and Movement:**
The player occupies a location spot within a location. Locations contain multiple spots representing different areas or contexts within that place. The town square is a location with spots like the fountain, the notice board, the market stalls. Each spot provides different NPCs and action opportunities.

Travel between location spots happens via routes. Routes contain obstacles and require time segments. The strategic layer tracks which routes exist, what obstacles they contain, and how much time travel costs. Routes can be improved through discovery (finding shortcuts) or deteriorate through world events.

**Time Management:**
The strategic layer tracks time segments consumed by all activities. Each day contains segments allocated to morning, midday, afternoon, and evening blocks. Time creates pressure through deadlines on obligations, weather changes affecting routes, and NPC availability at locations.

**Resource State:**
Strategic resources persist across all activities. Health and Stamina are the player's physical condition. Coins enable purchases. Equipment provides capabilities. Knowledge unlocks options. These resources are inputs and outputs for tactical challenges but exist strategically.

**Activities as Orchestrators:**
Investigation and Travel are strategic activities, not tactical systems. When you investigate a location, you enter a multi-phase activity that may spawn various tactical challenges. When you travel a route, you encounter obstacles that spawn appropriate tactical challenges. These activities track their own state and progression but delegate actual challenge resolution to tactical systems.

### Tactical Layer: Challenge Resolution

The tactical layer is where moment-to-moment gameplay happens through card-based systems. When the strategic layer presents a challenge (convince this NPC, solve this puzzle, overcome this physical obstacle), the appropriate tactical system spawns with a deck of cards representing available approaches.

The tactical layer receives strategic state as input (your stats, resources, equipment, knowledge) and outputs strategic state modifications (XP gained, resources spent, world changes made). During tactical play, transient resources exist only for that challenge - once you return to strategic layer, these disappear.

Three tactical systems handle different challenge types. Each has unique identity reflecting its domain while sharing core mechanical patterns. Cards never mix between systems - each manipulates its own transient resources.

---

## The Three Tactical Systems

### Social System: Influencing Through Conversation

The Social system resolves interpersonal challenges where the goal is reaching understanding or agreement with an NPC. This might be negotiation, persuasion, information gathering, or emotional connection.

**Core Identity:**
Conversations flow organically until natural endpoint. They have no fixed length - a conversation continues until consensus is reached or the relationship breaks down. You cannot pause mid-conversation and return later (this would be socially absurd). The system models the NPC's emotional state and receptiveness through resources that shift as you speak.

**Player Actions:**

Listen Action: Draw cards from the conversation deck into your hand. This represents listening to the NPC, giving them space to express themselves, and gathering conversational options. The deck contains cards representing things you could say in response. Drawing cards doesn't advance time within the conversation.

Speak Action: Play a card from your hand. This represents saying something, advancing your conversational goals, and shifting the emotional dynamic. Playing cards consumes resources and generates effects that move the conversation forward.

**Transient Resources:**

Initiative represents your capacity to guide conversation direction. You must build Initiative through Foundation cards (listening, small talk, building rapport) before you can play more significant cards. This creates builder-spender dynamics where you generate resources to enable powerful moves.

Momentum represents progress toward your conversational goal. Different conversation types have different Momentum thresholds. Friendly chat might need modest Momentum, desperate request needs significant Momentum, difficult negotiation needs extensive Momentum. Reaching threshold means successful conversation.

Doubt represents the NPC's patience wearing thin or losing interest. As Doubt accumulates, the conversation moves toward breakdown. Reaching maximum Doubt ends the conversation as failure. The NPC becomes harder to talk to afterward.

Cadence tracks the balance between speaking and listening. High Cadence (speaking too much) increases Doubt when you Listen next. Low Cadence (listening too much) grants bonus cards when you Listen next. This forces rhythm - you can't spam powerful cards without cost.

Statements in Spoken tracks conversation history. Some powerful cards require minimum Statements count, representing conversational context needed for sophisticated moves. This creates sequencing requirements.

**Strategic Persistence:**

Relationship Tokens accumulate with each NPC through successful conversations. Tokens unlock better cards in that NPC's deck for future conversations. Higher relationship means more sophisticated conversational options.

Burden Cards enter an NPC's conversation deck when conversations fail badly. These cards have negative effects when drawn, making future conversations harder. Relationship damage persists - the NPC remembers your failures.

Observation Cards are granted to NPC decks when you share discoveries with them. These cards unlock when you talk to that specific NPC again, representing shared knowledge creating conversational opportunities.

**Cards Bound to Stats:**

Social cards bind to all five stats based on conversational approach:
- Insight: Analytical questions, pattern recognition in speech, reading between lines
- Authority: Forceful statements, demands, assertions of will
- Rapport: Empathetic responses, emotional connection, reading feelings
- Diplomacy: Negotiation, finding common ground, mercantile thinking
- Cunning: Indirect implications, manipulation, subtle steering

### Mental System: Solving Through Analysis

The Mental system resolves intellectual challenges where the goal is understanding something through observation and reasoning. This might be solving puzzles, decoding patterns, analyzing documents, or figuring out mechanisms.

**Core Identity:**
Mental challenges have player-controlled pacing within deadline constraints. You can work on a puzzle intensely, step away to think, and return with the problem unchanged. The puzzle doesn't resist or react - it's static. However, your actions might disturb the location or draw attention, creating consequences. The system models your attention capacity and the location's tolerance for investigation.

**Player Actions:**

Observe Action: Draw cards from the challenge deck into your hand. This represents examining the puzzle, looking at different angles, and identifying approaches to try. Each observation takes time and might generate exposure if you're being obvious.

Act Action: Play a card from your hand. This represents testing a hypothesis, trying an approach, or making progress on understanding. Actions consume attention, advance progress, and might generate consequences.

**Transient Resources:**

Attention represents your mental capacity to focus on this challenge. You must generate Attention through Observe actions (looking carefully, taking notes, building understanding) before you can attempt complex analytical moves. This creates the same builder-spender pattern as Social but for intellectual effort.

Progress represents advancement toward understanding threshold. Different challenges have different Progress requirements. Simple puzzles need modest Progress, complex investigations need extensive Progress. Reaching threshold unlocks next phase or completes the challenge.

Exposure represents how much you've disturbed the location or drawn attention. Physical actions make noise, repeated observations look suspicious, aggressive approaches leave evidence. Reaching Exposure threshold triggers consequences - someone notices, structure becomes dangerous, or you must deal with social confrontation.

Observe/Act Balance tracks investigation rhythm. High balance (acting too much without observing) increases errors and exposure. Low balance (observing too much without acting) wastes time and attention. Optimal play alternates between observation and action.

Clue Types represent specific understanding accumulated. Structural clues are physical knowledge, Historical clues are documented facts, Environmental clues are contextual information, Social clues are behavioral patterns, Hidden clues are secrets discovered. Different phases require different clue combinations, not just total count.

**Strategic Persistence:**

Location Exposure accumulates across all investigation attempts at a location. If you were loud or obvious, that persists. Next time you investigate there, you start with elevated exposure - people are watching, structure is damaged, or evidence has been moved. This creates consequences for aggressive investigation.

Location State tracks permanent changes. Forced doors stay forced, cleared obstacles stay cleared, discovered passages stay discovered. Progress isn't lost between attempts - the location remembers what you've done.

Knowledge Facts are acquired when you solve parts of investigations. These facts persist forever, enable conversation topics, unlock investigation actions, and reveal travel shortcuts. Knowledge is the primary reward for investigation.

**Cards Bound to Stats:**

Mental cards bind to all five stats based on analytical approach:
- Insight: Logical deduction, systematic analysis, pattern recognition
- Authority: Forceful investigation, demanding access, breaking through obstacles
- Rapport: Intuitive leaps, sensing what matters, reading environment
- Diplomacy: Methodical documentation, organized approach, purchasing information
- Cunning: Lateral thinking, finding hidden elements, indirect methods

### Physical System: Overcoming Through Exertion

The Physical system resolves challenges requiring immediate physical commitment where retreat is costly or impossible. This might be combat, athletics under pressure, heavy labor under time constraint, or any situation where you must commit fully.

**Core Identity:**
Physical challenges demand immediate resolution. Once engaged, you cannot save and leave - you're committed until victory, defeat, or costly retreat. The challenge proceeds in rounds whether you're ready or not. This isn't about the NPC having agency (they don't, it's still a system) - it's about the situation having momentum. If you're lifting something heavy, you can't pause halfway. If you're in a fight, the opponent's responses are system-modeled but create time pressure.

**Player Actions:**

Assess Action: Draw cards from the challenge deck into your hand. This represents reading the situation, identifying options, and preparing responses. Assessment happens quickly but you can't assess forever - each round advances regardless.

Execute Action: Play a card from your hand. This represents physical commitment to an approach. Execution consumes resources, advances progress toward victory, and generates danger of failure consequences.

**Transient Resources:**

These need definition based on Physical system design, but following the pattern:

Position or Momentum represents your tactical advantage in the physical challenge. You build advantage through careful execution before attempting decisive moves.

Progress represents advancement toward completion or victory. Different challenges have different requirements - defeating an opponent needs enough Progress to overcome their resistance, completing heavy labor needs Progress to finish the task.

Danger or Exposure represents accumulating risk of failure consequences. Physical challenges directly threaten health and stamina. Reaching thresholds triggers injury, exhaustion, or defeat.

Assess/Execute Balance tracks physical rhythm. Hesitation (too much assessment) wastes stamina through prolonged exertion. Recklessness (too much execution) increases danger through lack of preparation.

**Strategic Persistence:**

Injury Cards enter player's deck when Physical challenges go badly. These cards persist across all future Physical challenges until healed, representing lasting damage. Injury makes future physical activity harder and more dangerous.

Equipment Degradation occurs through Physical use. Tools can break, armor can be damaged, weapons can dull. This creates maintenance costs and risk of losing capabilities mid-challenge.

Reputation changes through Physical outcomes. Victory increases respect or fear. Defeat damages standing. Witnesses spread word. This affects both Social and Physical encounters going forward.

**Cards Bound to Stats:**

Physical cards bind to all five stats based on tactical approach:
- Insight: Reading patterns, anticipating moves, finding weaknesses
- Authority: Aggressive tactics, overwhelming force, intimidation
- Rapport: Sensing timing, intuitive reactions, reading body language
- Diplomacy: Measured approach, conserving energy, seeking compromise
- Cunning: Dirty fighting, exploiting environment, underhanded tactics

---

## Universal Card Pattern

All three tactical systems share core mechanical structure while differing in identity and resource meanings.

### Two Actions

Every tactical system provides exactly two player actions:

The Draw Action (Listen/Observe/Assess): Draws cards from the system deck into your hand. This represents gathering options, building resources, and preparing for significant moves. Drawing is generally low-cost or free but has consequences through balance tracking.

The Play Action (Speak/Act/Execute): Plays a card from hand. This represents committing to an approach, spending resources, and advancing toward goals. Playing cards drives the challenge forward but depletes resources requiring management.

### Card Structure

Every card regardless of system has these properties:

Type determines resource flow. Foundation cards generate resources when played. Standard cards spend resources moderately for meaningful effects. Decisive cards spend resources heavily for powerful effects. This three-tier structure creates natural pacing and tactical depth.

Depth determines power level and stat requirements. Higher depth means stronger effects but requires higher bound stat to access. Depth creates progression - as your stats increase, you gain access to more powerful cards in all systems.

Bound Stat determines which stat gates access and receives XP. Each card binds to exactly one of the five stats. Playing that card grants experience toward that stat. Stat level determines maximum card depth accessible - higher stats unlock deeper tactical options.

Category determines thematic approach and specific costs. In Social this might be Empathy vs Logic. In Mental this might be Physical vs Analysis. In Physical this might be Aggressive vs Defensive. Categories affect which resources cards manipulate and what clue types they yield.

Tags provide additional properties enabling equipment gates and universal modifiers. A card tagged Climbing requires rope to play. A card tagged Loud doubles exposure generation. Tags compose - cards can have multiple tags applying multiple modifiers.

### Parser Rules

Cards don't manually specify every cost and effect. Instead, the parser looks at card properties and deterministically generates all mechanical values. This means content creators fill in categorical properties (Type, Depth, Stat, Category, Tags) and write narrative text (name, description). The system calculates costs, effects, requirements, and dangers automatically based on universal formulas.

This parser-driven approach dramatically reduces authoring burden. The same "Force Door" card template can be used across hundreds of investigations with only the description text changing. All mechanical behavior comes from Type=Standard, Depth=2, BoundStat=Authority, Category=Physical, Tags=[Loud, Risky].

---

## Stats as Universal Approaches

The five player stats do not represent domain-specific skills (social skill, mental skill, physical skill). They represent universal tactical approaches applicable across all challenge types.

**Insight: Analytical Awareness**
Understanding through systematic observation and logical deduction. In conversation, this is asking targeted questions and reading between lines. In investigation, this is methodical examination and pattern recognition. In physical challenges, this is studying opponent patterns and identifying weaknesses. Insight represents rational problem-solving.

**Authority: Forceful Approach**
Solving through assertion of will and direct action. In conversation, this is demands and forceful statements. In investigation, this is breaking through obstacles and forcing access. In physical challenges, this is aggressive overwhelming tactics. Authority represents power-based solutions.

**Rapport: Intuitive Connection**
Understanding through empathy and reading unstated cues. In conversation, this is emotional connection and sensing feelings. In investigation, this is intuitive leaps about what matters. In physical challenges, this is sensing timing and reacting to subtle signals. Rapport represents emotional intelligence.

**Diplomacy: Methodical Negotiation**
Solving through careful bargaining and finding mutual benefit. In conversation, this is negotiation and mercantile thinking. In investigation, this is purchasing information and organized documentation. In physical challenges, this is measured approach and seeking compromise. Diplomacy represents strategic patience.

**Cunning: Indirect Method**
Achieving goals through misdirection and unconventional thinking. In conversation, this is manipulation and subtle steering. In investigation, this is lateral thinking and finding hidden elements. In physical challenges, this is dirty fighting and environmental exploitation. Cunning represents clever subversion.

This unified approach system means every stat matters for every challenge type. You don't build "social character" vs "combat character" - you build "insightful character" who uses analysis in all contexts, or "cunning character" who uses indirect methods everywhere. This creates coherent character identity across challenge types rather than fragmented skill trees.

### Stat Progression Integration

Playing cards grants experience to the bound stat. A card bound to Cunning grants Cunning XP when played regardless of whether it's a Social, Mental, or Physical challenge. XP accumulates to thresholds where the stat level increases.

Stat level determines card depth access. Cunning 1 accesses depths 1-2 (basic cards). Cunning 2 accesses depths 1-3 (Foundation plus simple Standard). Cunning 3 accesses depths 1-4 (Standard cards). Higher levels progressively unlock more powerful card depths across all three systems.

This creates natural progression: playing challenges improves stats, improved stats unlock better cards, better cards make you more capable in all challenge types. Success in one domain improves capability in other domains through shared stat system.

---

## Resource Architecture

### Strategic Resources (Persist)

These resources exist in the strategic layer and persist across all activities:

**Health and Stamina:** Your physical condition. Health represents damage tolerance - injury and harm reduce health. Stamina represents energy for exertion - strenuous activity depletes stamina. Both regenerate slowly through rest or quickly through recovery actions. Low health restricts risky physical actions. Low stamina restricts strenuous actions in any system.

**Coins:** Economic resource for purchasing equipment, services, and information. Earned through work, obligation completion, and discoveries. Spent on preparation and access. Creates economic pressure through competing demands.

**Time Segments:** The limited resource of time. Each day contains a fixed number of segments. All activities consume segments. Obligations have deadlines. Weather changes on schedule. Time creates urgency without arbitrary timers.

**Equipment:** Physical items providing capabilities. Equipment enables card tags (rope enables climbing cards). Equipment provides universal modifiers (lantern adds visibility but increases exposure). Equipment can be consumed (oil, food) or degraded (tools break, armor damages). Creates preparation strategy and resource management.

**Knowledge:** Facts and understanding accumulated through investigation and conversation. Knowledge unlocks conversation topics, investigation actions, and travel approaches. Knowledge persists forever. This is the primary reward for thorough exploration.

**Player Stats:** The five stats (Insight, Authority, Rapport, Diplomacy, Cunning) gate card access and grow through play. Stats are the primary character progression mechanism.

### Transient Resources (Temporary)

These resources exist only during tactical challenges and disappear when returning to strategic layer:

**Social uses:** Initiative, Momentum, Doubt, Cadence, Statements. These model conversation dynamics - your capacity to guide discussion, progress toward agreement, NPC patience wearing thin, speaking/listening balance, and conversational context.

**Mental uses:** Attention, Progress, Exposure, Observe/Act Balance, Clue Types. These model investigation dynamics - your mental focus, understanding advancement, location disturbance, investigation rhythm, and specific knowledge types.

**Physical uses:** (To be fully defined based on Physical system design, but following the pattern) Position/advantage in challenge, progress toward completion, danger accumulation, assessment/execution balance, and relevant tactical states.

The key distinction: strategic resources are what you bring in and take out. Transient resources are the gameplay happening inside. You enter Social challenge with high Stamina, play through conversation building Initiative and Momentum, and exit having consumed Time and gained Knowledge. The Initiative and Momentum existed only during conversation.

---

## Three-Part Persistence Pattern

Each tactical system affects strategic state through identical structural pattern:

### Consumables (Restored)

Resources spent during tactical challenges that regenerate in strategic layer. Stamina is the universal consumable - all challenges exhaust you. Stamina depletes through card costs, regenerates through rest. Low stamina restricts options across all systems.

### Debt (Failure Accumulation)

Negative persistent effects from failed challenges that make future attempts harder:

**Social Debt:** Burden cards enter NPC conversation decks. These cards have negative effects when appearing, making conversations harder. Burden accumulates through relationship damage. High burden makes consensus nearly impossible without relationship repair.

**Mental Debt:** Location exposure levels increase. High exposure means future investigation attempts start with elevated exposure baseline - less room for mistakes before consequences. Exposure accumulates through being noticed, making noise, or leaving evidence.

**Physical Debt:** Injury cards enter player deck. These cards persist across Physical challenges, creating debuffs and restricting options. Injuries accumulate through taking damage, making future physical activity more dangerous.

Debt creates downward spirals if not managed. Failed Social challenge creates burden, making next Social challenge with that NPC harder, leading to more burden. Failed Mental challenge increases exposure, making next Mental attempt more likely to fail, leading to more exposure. Failed Physical challenge causes injury, making next Physical challenge more dangerous, leading to more injury.

### Progress (Success Accumulation)

Positive persistent effects from successful challenges that make future attempts easier:

**Social Progress:** Relationship tokens accumulate with NPCs. Higher tokens unlock better cards in that NPC's deck. Strong relationships provide superior conversational options and access to exclusive information and support.

**Mental Progress:** Knowledge facts accumulate. Knowledge unlocks actions across all systems - conversation topics require knowledge, investigation actions require knowledge, travel approaches require knowledge. Knowledge represents mastery.

**Physical Progress:** Reputation increases through victory. Higher reputation affects both Social encounters (people respect or fear you) and Physical encounters (opponents are cautious or enemies seek revenge). Reputation is social capital from physical prowess.

Progress creates upward spirals when managed well. Successful Social challenges improve relationships, unlocking better cards for next Social challenge, making success easier. Successful Mental challenges grant knowledge, enabling better preparation for next Mental attempt, improving success chance. Successful Physical challenges build reputation, affecting future encounters favorably.

---

## Strategic Layer Activities: The Complete Picture

The strategic layer contains three categories of activities that interact with tactical challenges in different ways.

### Direct Strategic Actions (No Tactical Spawn)

These activities manipulate strategic resources directly without entering tactical layer gameplay. They're design abstractions where the outcome is deterministic or simple enough not to warrant moment-to-moment tactical decisions.

**Resource Management:**
Resting to recover health and stamina consumes time and restores resources without tactical gameplay. You make the strategic choice to rest, time advances, resources restore. The interesting decision already happened strategically - choosing to rest versus pursuing other activities. Similarly, shopping for equipment is direct transaction: coins exchange for items. The strategic choice of whether to spend coins is interesting; the exchange itself doesn't need tactical depth.

**Simple Work:**
Routine labor to earn coins can be strategic abstraction. You choose to work, time and stamina are consumed, coins are earned based on time investment. The interesting gameplay is the strategic choice of when to work versus pursue other activities, not the moment-to-moment labor execution. However, if work becomes complex (dangerous job, performance-based pay, negotiation required), it might spawn tactical challenges instead.

**Location Observations:**
Casually observing a location spot without deep investigation grants surface-level information strategically. You see who's present, what the general atmosphere is, what obvious features exist. This informs strategic decisions about which activities to pursue without spawning tactical gameplay. Casual observation is quick contextual information gathering.

**Time Manipulation:**
Choosing to wait or advance time to different day segments. Waiting for an NPC to arrive at a location, waiting for shops to open, advancing to evening for different social opportunities. This is strategic time management providing control over when activities happen without tactical depth in the waiting itself.

**Obligation Management:**
Accepting delivery requests or tasks from NPCs adds them to your obligation journal strategically. The acceptance moment is simple decision-making. The interesting gameplay happens when fulfilling the obligation through travel challenges, investigation challenges, or conversation challenges - not in the acceptance itself.

### Tactical Challenge Triggers

These activities spawn one of the three tactical systems for resolution:

**Formal Conversations:**
When you choose to have a meaningful conversation with an NPC - whether to gather information, negotiate, persuade, or build relationship - this spawns the Social tactical layer. You enter conversation with that NPC's deck, your cards based on stats and relationship level, and play through to consensus or breakdown. The conversation could be initiated by you approaching an NPC, or by the NPC approaching you based on game events.

**Deep Investigations:**
When you choose to investigate a location deeply, this spawns the Investigation strategic activity which orchestrates multiple tactical challenges across its phases. You might face Mental challenges for puzzle-solving, Physical challenges for obstacle removal, and Social challenges for gathering information from witnesses. This is different from casual observation - investigation commits resources and triggers tactical depth.

**Route Travel:**
When you choose to travel a route between locations, this spawns the Travel strategic activity which presents obstacles. Each obstacle spawns the appropriate tactical challenge - Physical for terrain difficulties, Mental for navigation puzzles, Social for checkpoints or encounters. Travel commits you to the journey with its associated challenges.

**Physical Contests:**
When you engage in combat, athletics challenges, or labor requiring tactical execution, this spawns Physical tactical layer. You play through rounds of physical commitment until victory, defeat, or costly retreat. This happens when situations demand immediate physical resolution that can't be abstracted.

### Hybrid Activities (Context-Dependent)

Some activities might spawn tactical challenges or be strategic abstractions depending on context, complexity, and design goals:

**Tavern Talk:**
Casual social interaction could work multiple ways. As strategic abstraction, you choose "chat with locals" and receive rumor information directly - quick and simple. As lightweight Social tactical, you enter simplified conversation with generic deck representing small talk. The design choice depends on whether casual socializing should have tactical depth or remain quick strategic flavor. A hybrid approach works well: tavern talk starts as strategic listening to gossip, but if you learn something interesting, you can choose to engage deeper with specific person, spawning full Social tactical. This provides onramp from casual to meaningful interaction.

**Mercantile Negotiations:**
Basic shopping at fixed prices is strategic abstraction - direct coin-to-item exchange. Complex negotiations with variable prices, limited trust, or special conditions might warrant Social tactical spawn focused on Diplomacy. The line between abstraction and tactical depends on transaction significance and narrative weight.

**Routine Investigations:**
Searching a room casually might be strategic abstraction - consume time, roll against Insight stat, gain item or information directly. Deep investigation of same room spawns Mental tactical with cards representing different search approaches. The player chooses investigation intensity based on available resources and expected payoff.

**Skilled Labor:**
Routine work with predictable outcomes can be strategic abstraction with modest pay. Dangerous or performance-based work might spawn Physical tactical representing the exertion challenge, with success earning better pay and failure causing injury. This creates strategic choice between safe-but-slow income and risky-but-lucrative engagement.

### Design Decision Framework

How to decide whether an activity should be strategic abstraction versus spawn tactical challenge:

**Complexity and Choice Depth:**
If the activity involves multiple interesting decisions with meaningful trade-offs, it warrants tactical depth. Shopping from fixed-price menu doesn't need tactics because the interesting choice was strategic (buy now versus save coins). Negotiating prices with suspicious merchant who has limited trust might warrant Social tactical with relationship consequences.

**Player Skill Expression:**
If execution skill in the moment affects outcomes meaningfully, use tactical. Simple work is abstraction because the interesting decision is strategic (work now versus investigate). Dangerous work where you must manage exertion and danger through card play might be Physical tactical where skilled play earns better outcomes.

**Failure Consequence Weight:**
If failure creates interesting consequences worth playing through, use tactical. Resting successfully restores resources - failure to rest just means you didn't, no interesting consequence. Failed conversation damages relationship with burden cards - interesting consequence worth tactical play to avoid.

**Narrative Significance:**
If the activity is narratively significant to current goals, consider tactical depth. Casual chat with random traveler might be abstraction providing quick information. Interrogating suspect in investigation mystery warrants full Social tactical depth because the outcome matters to player's goals.

**Pacing Considerations:**
Too many tactical spawns exhaust players. Strategic abstractions provide pacing relief between intense challenges. Not everything needs tactical depth - routine activities can be quick abstractions allowing focus on significant challenges. Balance tactical intensity with strategic breathing room.

### Location Action Menus

When the player is at a location spot, the strategic layer presents context-sensitive menu of possible actions based on what's present:

**NPC Interaction Options:**
If NPCs are present, options appear to initiate conversations with them. Each NPC might offer multiple conversation types depending on relationship and context - formal request, casual chat, trade negotiation, information gathering. Choosing one spawns Social tactical with appropriate deck. The menu shows which NPCs are available and what conversation types are possible with each.

**Location Feature Interactions:**
If the location has notable features (locked door, suspicious area, interesting mechanism, hidden alcove), options appear to interact with them. Casual examination might be strategic abstraction. Choosing deep investigation spawns Investigation activity with its orchestrated tactical challenges. The menu reveals features you've noticed through observation or prior knowledge.

**Travel Options:**
Routes leading to other location spots appear as travel choices showing estimated time cost and known difficulty. Selecting travel spawns Travel activity with its obstacle challenges. The menu shows all available routes from current location spot.

**Rest and Recovery:**
Options to rest, eat, or wait for time passage. These are typically strategic abstractions consuming time and resources to restore health and stamina. Rest might be interrupted by random events if the design includes such elements, but the rest action itself doesn't spawn tactical challenges.

**Economic Actions:**
If merchants present, shopping options appear for purchasing equipment and supplies. If work available, labor options appear offering coin payment for time investment. These might be strategic abstractions (fixed transactions) or spawn tactical challenges (complex negotiations, dangerous work) depending on context.

**Social Opportunities:**
Listening to tavern gossip, observing crowds, joining group activities. These could be strategic information gathering granting rumors and knowledge directly, or spawn simplified Social tactical depending on design goals and narrative significance.

The strategic layer menu is dynamic and contextual. You're not choosing from abstract actions - you're responding to what's physically and socially present at your current location spot. The menu updates based on time of day (shops close, NPCs arrive/leave), game state (obligations active, investigations in progress), and world events (weather changes, reputation effects).

### Information Gathering Spectrum

Notice how information gathering exists on spectrum from strategic to deeply tactical, allowing players to choose intensity:

**Casual Observation (Strategic):** Walk through market, see who's present, notice general atmosphere. Quick contextual information consuming minimal time.

**Tavern Gossip (Strategic or Light Tactical):** Listen to rumors, gather surface information. Might hear about investigation opportunities or NPC locations without tactical depth commitment.

**Targeted Questions (Full Social Tactical):** Have meaningful conversation with NPC specifically to gather information. Full tactical depth with relationship consequences and sophisticated information extraction.

**Investigation (Strategic Activity with Tactical Challenges):** Deep exploration of physical location through orchestrated challenges. Maximum information gain requiring maximum commitment and resource expenditure.

This spectrum allows players to calibrate information-gathering investment based on their needs, available resources, and risk tolerance. Not every information need requires deep tactical engagement.

---

## Investigation as Strategic Activity

Investigation is not a tactical system - it's a strategic activity that orchestrates tactical challenges.

### Investigation Structure

When you investigate a location, you enter a multi-phase activity framework. Each phase presents challenges that must be overcome to advance. Phases unlock sequentially as you meet requirements. The investigation tracks overall state including completed phases, accumulated discoveries, and consequences from your methods.

### Phase Composition

Each investigation phase contains one or more challenges. Each challenge spawns the appropriate tactical system based on challenge type:

**Mental Challenges:** Require puzzle-solving, analysis, or deduction. These spawn the Mental tactical system with a deck of cards representing investigative approaches. Success yields understanding, clues, or access to next phase.

**Physical Challenges:** Require overcoming physical obstacles like locked doors, climbing barriers, or moving heavy objects. These spawn the Physical tactical system with cards representing exertion methods. Success grants physical access or removes obstacles.

**Social Challenges:** Require gathering information from people, convincing someone to help, or confronting suspects. These spawn the Social tactical system with conversation cards. Success yields information or cooperation.

### Example: Miller's Daughter Investigation

**Phase 1: Exterior Assessment**
- Mental Challenge: "Survey the mill structure" - spawns Mental tactical with observation cards
- Goal: Gather structural and environmental clues to understand entry options
- Success yields knowledge: weak wall location, window access possibility, time when location empty

**Phase 2: Gaining Entry**
- Physical Challenge: "Climb to upper window" - spawns Physical tactical with climbing cards
- OR Mental Challenge: "Pick the lock" - spawns Mental tactical with mechanism cards
- OR Physical Challenge: "Force the door" - spawns Physical tactical with breaking cards
- Multiple valid approaches based on equipment, stats, and preferred method

**Phase 3: Interior Investigation**
- Mental Challenge: "Search for evidence" - spawns Mental tactical with analysis cards
- Goal: Find clues about the mystery through careful examination
- Success yields historical clues: old ledgers, hidden compartments, incriminating documents

**Phase 4: Confrontation (optional, triggered by exposure)**
- Social Challenge: "Deal with witnesses" - spawns Social tactical with confrontation cards
- Triggered if exposure threshold exceeded during earlier phases
- Must convince witnesses not to report you or face reputation consequences

**Phase 5: Extraction**
- Choice of Physical or Mental depending on what you discovered
- Mental: Piece together the full story from clues
- Physical: Escape with evidence before caught

The investigation activity orchestrates these challenges, tracks which phases are complete, remembers what you've discovered, and determines what challenges appear based on your previous choices and performance.

### Investigation State Persistence

Investigation state saves between sessions. If you complete Phase 1 and 2, then leave to rest and gather better equipment, when you return you resume at Phase 3. Your discoveries remain known. Your exposure level persists. The door you forced stays forced. This enables iterative investigation: attempt, learn what you need, retreat, prepare, return with advantage.

This persistence is unique to Mental challenges reflecting reality - you can think about a puzzle, walk away, and return unchanged. Physical challenges in investigation (forcing doors, climbing walls) are static obstacles you can prepare for, unlike dynamic Physical challenges (combat) where retreat is costly.

---

## Travel as Strategic Activity

Travel is not a tactical system - it's a strategic activity that orchestrates obstacle encounters.

### Travel Structure

When you travel a route between locations, you encounter obstacles based on route difficulty and conditions. Each obstacle presents a challenge that must be overcome to continue. The travel activity tracks progress along route, resources expended, and consequences from obstacle resolution.

### Obstacle Composition

Each travel obstacle spawns appropriate tactical system based on obstacle nature:

**Physical Obstacles:** Rough terrain, river crossings, climbing sections, heavy weather. These spawn Physical tactical with cards representing traversal methods. Success means passage, failure means injury or route abandonment.

**Mental Obstacles:** Navigation puzzles, reading signs, understanding local customs, solving access riddles. These spawn Mental tactical with cards representing problem-solving approaches. Success reveals paths, failure wastes time or leads astray.

**Social Obstacles:** Checkpoints requiring permission, travelers needing help, locals demanding toll, suspicious witnesses. These spawn Social tactical with conversation cards. Success grants passage, failure creates reputation problems or must find alternate route.

### Example: Creek Crossing Obstacle

**Physical Obstacle: "Swollen Creek"**
- Description: Heavy rains have swollen the creek to dangerous levels
- Spawns Physical tactical system
- Cards available based on equipment and stats:
  - "Wade Across" (Authority + Stamina) - direct but dangerous
  - "Use Rope to Secure Crossing" (Cunning + Rope equipment) - safer with preparation
  - "Find Shallow Spot" (Insight + time investment) - careful but time-consuming

Success means crossing with varying time and resource costs based on method. Failure means injury, equipment loss, or must abandon route and try different path. The obstacle remembers if you improved it (set up rope permanently) making future crossings easier.

### Route Improvement

Travel obstacles can be permanently improved through good performance. If you carefully set up rope crossing with low exposure, that rope stays for future trips - the crossing becomes easier. If you discover hidden game trail, that knowledge persists - you can use faster route next time. Travel activities create permanent world improvements rewarding thorough exploration.

---

## System Interconnection

The three tactical systems create circular dependencies through strategic resource manipulation:

### Social Feeds Other Systems

Successful conversations grant Knowledge. Knowledge unlocks Mental investigation actions (you know what to look for) and Physical approach options (you know the weakness). Conversations also grant Observation cards - when you share investigation discoveries, NPCs give you special conversation options in return.

Relationships provide practical support. High relationship NPCs offer equipment, information about dangers, or access to restricted areas. Social success makes other challenges easier through preparation advantages.

### Mental Feeds Other Systems

Successful investigations grant Knowledge. Knowledge unlocks Social conversation topics (you have information to share) and Physical tactical advantages (you know the opponent's pattern). Investigations also grant Observation cards directly - discoveries about people enable new conversation approaches with those people.

Investigations reveal route improvements. Finding shortcuts or hidden paths makes Travel faster. Understanding location dangers enables better Physical preparation.

### Physical Feeds Other Systems

Physical success builds Reputation. Reputation affects Social encounters (people respect or fear you) and Mental encounters (access granted based on standing). Being known as capable fighter changes how people interact with you.

Physical challenges through labor earn Coins. Coins purchase Equipment. Equipment enables tactical options across all systems - rope for climbing in Mental investigations, tools for forcing entry, gifts for Social relationship building.

### Circular Preparation Loops

The interconnection creates preparation loops spanning multiple systems:

Investigate ruins (Mental) → discover knowledge about bandit weakness → use knowledge in conversation with guard (Social) → gain information about bandit schedule → use timing knowledge in confrontation (Physical) → earn reputation from victory → reputation helps negotiate with merchant (Social) → purchase better equipment → equipment enables deeper investigation (Mental)

Success in one system provides advantages in other systems. Failure in one system creates disadvantages propagating across systems. This creates strategic depth where diverse skill development is valuable - narrow optimization makes you vulnerable when facing your weak domain.

---

## AI Text Generation Integration

### The Visual Novel Presentation

The player experiences the game as visual novel: situation descriptions followed by choice selections followed by outcome descriptions. The UI might show only narrative text without exposing resource counts or mechanical details, though this is a design choice - the game could show both narrative presentation and underlying mechanics.

### Mechanical State Drives Generation

AI receives game state and generates appropriate narrative:
- Current location and environmental conditions
- NPC emotional state (from Social resources like Initiative, Momentum, Doubt)
- Investigation progress (from Mental resources like Attention, Progress, Exposure)
- Physical condition (from Physical resources and dangers)
- Recent history of player actions
- Which deck is being drawn from (Marcus's deck, Mill's deck, etc)
- Cards currently in hand

The AI produces three types of content:

**Situation Descriptions:** Scene-setting narrative explaining current context before player draws or plays cards. "The mill looms before you, its broken windows like dead eyes. The door is secured with a rusty lock, and you hear the creek rushing behind the structure." This establishes the challenge context.

**Card Narrative Text:** Flavor text for each card making the choice feel narrative rather than mechanical. Instead of showing "Force Door: Type=Standard, Depth=2, Cost 2 Attention, +3 Progress, +2 Exposure", the card displays "Shoulder the door hard, damn the noise" with mechanical effects hidden or minimally visible. The AI generates this narrative text based on card properties and current context.

**Outcome Descriptions:** Results of card play explained narratively. "The door splinters with a loud crack. You're through, but the sound echoes - anyone nearby would have heard that." The AI receives mechanical outcome (door forced, Progress gained, Exposure increased) and generates appropriate narrative description.

### Mechanical Constraint

While AI generates all narrative text, it cannot modify mechanical effects. Card effects are deterministically defined by parser rules based on card properties. AI decorates mechanics with appropriate narrative but doesn't invent consequences. This separation ensures mechanical consistency while allowing narrative flexibility.

The AI sees mechanical state: "Player played card with Type=Standard, Depth=2, Category=Physical, Visibility=Loud from Mill investigation deck, generating +3 Progress, +2 Exposure, costing 2 Attention, danger roll succeeded." The AI writes: "You throw your weight against the old door, feeling the wood strain and crack. The sound echoes through the empty mill - subtle was never an option. You're through, but your heart pounds knowing that noise carried far."

The mechanical outcome is fixed by card properties and parser rules. The AI's job is making that outcome feel narrative and appropriate to context. Different AI generation might phrase things differently, but the mechanical impact is identical - deterministic and rule-based.

### Deck Context Informs Generation

Because challenges draw from specific decks, AI knows exactly what context it's generating for. Drawing from Marcus's conversation deck means generating Marcus-appropriate dialogue. Drawing from the Mill's investigation deck means generating Mill-appropriate investigation descriptions.

The deck identity provides crucial context for AI generation. The AI can reference deck-specific details because it knows which deck is active. When generating card text for a card in Marcus's deck, the AI knows this card only appears when talking to Marcus, so it can reference Marcus-specific details without ambiguity.

When you draw "Recall His Kindness" card from Marcus's deck, the AI knows this is Marcus-specific knowledge you accumulated through prior conversations with him. It generates text referencing your shared history: "Remember when Marcus helped you with the broken cart? Bring that up - remind him you haven't forgotten his generosity."

This deck-specific generation creates coherent narrative without requiring complex property matching or abstract targeting. The card lives in Marcus's deck, therefore it's about Marcus. The AI generation reflects that direct relationship.

### Knowledge Cards Get Special Treatment

Knowledge cards in decks are cards you earned through play rather than authored base cards. When generating text for knowledge cards, AI emphasizes that this represents understanding you gained: "You remember the structural weakness you identified in the west wall - that would be the easiest entry point."

The AI differentiates knowledge cards narratively while treating them mechanically identical to base cards. Players understand these are advantages they earned through prior investigation or conversation success. The narrative presentation reinforces that knowledge provides tactical advantage grounded in player history with this deck's owner.

---

## Universal Card Properties System

All cards across all three tactical systems use strongly typed properties with explicit mechanical effects. These properties are enums and structured types, never strings, ensuring clear compile-time guarantees and consistent behavior.

### Core Card Properties

**Type** (enum: Foundation, Standard, Decisive):
Determines the card's resource flow pattern and power level. Foundation cards generate primary tactical resource when played (Initiative in Social, Attention in Mental, Position in Physical). Standard cards spend moderate tactical resources for meaningful progress. Decisive cards spend heavy tactical resources for powerful effects. The parser uses Type to determine base costs and yields across all value calculations.

**Depth** (integer: 1-10):
Determines effect magnitudes and stat requirements. Higher depth means stronger effects but requires higher bound stat level to access. Depth 1-2 accessible with stat level 1, depth 3-4 requires stat level 2, depth 5-6 requires stat level 3, and so on. The parser scales all costs and effects based on depth following formulaic progression.

**BoundStat** (enum: Insight, Authority, Rapport, Diplomacy, Cunning):
Determines which player stat gates access to this card and receives XP when the card is played. A card bound to Cunning requires Cunning stat at appropriate level for the card's depth, and grants Cunning XP when successfully played. This creates stat progression feeding back into card access across all systems.

**Category** (system-specific enum):
Determines thematic approach and which strategic resources the card manipulates. Categories differ by system but follow consistent pattern:

Social Categories: Empathetic, Logical, Assertive, Mercantile, Deceptive
Mental Categories: Analytical, Physical, Observational, Social, Synthesis  
Physical Categories: Aggressive, Defensive, Tactical, Evasive, Endurance

Category determines which clue types cards yield in Mental system, which conversational dynamics cards emphasize in Social system, and which tactical approaches cards represent in Physical system. The parser maps categories to specific resource effects deterministically.

### Universal Card Properties (Affect All Systems)

**RiskLevel** (enum: Safe, Cautious, Risky, Dangerous):
Determines base danger probability and consequence severity regardless of system. This property represents inherent danger in the action itself, not who performs it or their capabilities. RiskLevel.Safe has minimal failure consequences. RiskLevel.Dangerous has high probability of negative outcomes on failure.

In Social system, high risk means relationship damage through burden cards. In Mental system, high risk means injury from investigation hazards or major exposure spikes. In Physical system, high risk means direct health damage. Equipment, stats, and knowledge can mitigate danger probability but cannot change the card's base risk classification.

The parser calculates danger probability from RiskLevel: Safe has probability near zero, Cautious has low probability, Risky has moderate probability, Dangerous has high probability. The bound stat reduces probability (higher stat = lower danger), and specific equipment can provide additional reductions, but the base classification comes from RiskLevel property.

**Visibility** (enum: Subtle, Moderate, Obvious, Loud):
Determines how noticeable the action is to observers or environment regardless of system. This property affects exposure generation in Mental system (obvious actions disturb locations faster), attention drawn in Social system (loud conversational moves increase Doubt faster), and enemy awareness in Physical system (obvious actions alert opponents sooner).

A card with Visibility.Subtle generates minimal exposure or attention across all systems - quiet conversation, careful investigation, stealthy physical movement. A card with Visibility.Loud generates significant exposure or attention - forceful declarations, breaking through obstacles, aggressive attacks.

The parser multiplies base exposure/attention generation by visibility modifier: Subtle reduces generation, Moderate keeps baseline, Obvious increases generation, Loud significantly increases generation. Equipment can modify effective visibility (stealth tools reduce, lantern increases) but the card's base visibility classification remains.

**ExertionLevel** (enum: Minimal, Light, Moderate, Heavy, Extreme):
Determines stamina cost and availability gates regardless of system. This property represents physical and mental exhaustion the action causes. Even Social and Mental cards have exertion - intense negotiations drain stamina through stress, sustained concentration exhausts mentally, careful observation requires focus energy.

A card with ExertionLevel.Heavy costs significant stamina and becomes unavailable when player stamina drops below threshold. ExertionLevel.Minimal costs almost nothing and remains available even when exhausted. This creates strategic stamina management across all challenge types - you cannot simply spam powerful cards without considering exhaustion accumulation.

The parser calculates stamina cost from ExertionLevel and card Depth: higher exertion and higher depth both increase cost. Low stamina gates out Heavy and Extreme exertion cards completely, forcing use of lower-exertion alternatives or retreat to recover. Equipment can reduce effective exertion (tools make physical work less tiring) but cannot eliminate it.

**MethodType** (enum: Direct, Analytical, Intuitive, Negotiated, Deceptive):
Categorizes the approach being taken, determining which knowledge facts and equipment provide bonuses. This property is orthogonal to Category (which determines system-specific effects) and represents universal tactical approach types.

A card with MethodType.Analytical benefits from analytical knowledge facts (understanding patterns, documented information, logical deductions) regardless of which system. Analytical knowledge provides cost reduction or effect enhancement. A card with MethodType.Physical benefits from physical equipment and stamina optimization.

The parser checks MethodType against player's accumulated knowledge and equipment to determine applicable modifiers. Multiple knowledge facts can apply to single card if they match the method type. This creates strategic value in knowledge accumulation - diverse knowledge helps diverse method types across all systems.

### Structured Requirements Object

The Requirements property is strongly typed structure containing explicit prerequisites for playing the card:

**EquipmentCategory** (enum within Requirements object):
Each card optionally requires an equipment category, not a specific item. Categories include: Climbing, Mechanical, Documentation, Illumination, Force, Precision, Medical, Securing. 

Equipment items provide one or more categories. A sturdy rope provides both Climbing and Securing categories. A scholar's kit provides Documentation. A crowbar provides both Force and Mechanical. A lantern provides Illumination. 

At deck composition time, cards requiring categories are included only if player equipment satisfies that category. A card requiring Climbing category is in the deck if any equipped item provides Climbing - whether that's rope, grappling hook, or climbing harness doesn't matter to the card.

This abstraction means roughly one in twenty cards has category requirements. Most cards are universally available. Equipment expands options rather than gates basic functionality.

**Knowledge as Playable Cards:**
Knowledge is not a requirement that cards check for. Instead, knowledge facts become cards in your hand during applicable challenges. When you learn something through conversation or investigation, you acquire a knowledge card. That card enters your starting hand in future challenges where it applies.

Knowledge cards declare what they apply to using properties, not specific IDs. A knowledge card might state: "Applies to LocationProperty.Ancient, ChallengeType.Mental, Provides Effect.BonusHistoricalClues." When you're in a Mental challenge at an ancient location, this knowledge card is automatically in your hand and playable.

Knowledge cards are typed by properties:
- Challenge type they apply to (Social/Mental/Physical enum)
- Location properties they apply to (Ancient/Unstable/Public/Dark/Guarded enum)
- Effect they provide when played (various effect enums)

This means knowledge is context-sensitive without referencing specific locations. The same "Structural Weakness Understanding" knowledge card works at any location with Unstable property during Physical challenges. Content creators never specify which exact locations - the property system handles applicability.

Knowledge cards go directly in hand at challenge start, not in deck to draw. This reflects that knowledge is something you bring to situations, not something you randomly discover mid-challenge. You know what you know from the beginning.

**StatThresholds** (Dictionary<StatType, int>):
Minimum stat levels required beyond the BoundStat requirement. Some cards might require Insight 3 (as BoundStat) AND Authority 2 (as threshold), representing combined capability needs. This allows complex skill requirements without string parsing.

**MinimumHealth** and **MinimumStamina** (integers):
Resource gates preventing playing cards when too damaged or exhausted. A card requiring MinimumStamina of 30 becomes unplayable when stamina drops below this threshold. This forces strategic resource management and prevents desperate gambits when too weakened.

All requirements must be met simultaneously for card to be playable. The UI shows unmet requirements clearly when displaying unusable cards, explaining exactly what the player needs to acquire or develop.

### Equipment Makes Cards Playable

Equipment doesn't modify decks or compose them at runtime. Equipment makes cards playable that would otherwise be unplayable when drawn. Every location deck and NPC deck contains cards representing all possible approaches at that location or with that NPC. Some of those cards require equipment. Drawing them without equipment means they're grayed out and unusable.

**Equipment Categories:**
Equipment items provide strongly-typed categories using enums: Climbing, Mechanical, Documentation, Illumination, Force, Precision, Medical, Securing. A sturdy rope provides both Climbing and Securing. A crowbar provides both Force and Mechanical. A scholar's kit provides Documentation. A lantern provides Illumination.

**Requirement Matching:**
Cards in decks declare required equipment categories in their Requirements object. "Climb to Upper Window" requires Climbing category. "Pick the Lock" requires Mechanical category. "Read Ancient Texts" requires Documentation category. When you draw these cards, the game checks: does your equipped gear provide this category?

If yes, the card is playable. If no, the card appears grayed out with clear indication: "Requires: Climbing Equipment." You can see the card exists and what it does, but you cannot play it without the tools.

**Universal Benefits:**
Beyond enabling specific cards, equipment provides universal modifiers that apply to all cards when played:

Climbing equipment reduces effective danger level for cards involving heights or traversal. If you play a risky climbing card while equipped with rope, the danger probability is reduced as if the card were cautious instead. The equipment provides safety margin.

Mechanical equipment reduces effective exertion level for manipulation cards. Precision tools make mechanical work less tiring, lowering stamina costs. This makes repeated mechanical actions sustainable.

Illumination equipment increases clue yields for observation cards but also increases visibility level for all cards. Better vision helps investigation but makes you more noticeable. The lantern is double-edged advantage.

Documentation equipment reduces attention costs for analytical cards. Proper note-taking and reference materials make mental work more efficient. This enables sustained intellectual effort.

These modifiers apply when playing cards, not when composing decks. The deck is unchanged by equipment. What changes is how effective your card plays are when you have better tools.

**Practical Example:**
The Old Mill's investigation deck contains forty cards including "Climb to Upper Window" (requires Climbing), "Force the Door" (requires Force), and "Examine Documents" (requires Documentation). 

Without any equipment, you might draw any of these cards, but the ones with requirements are grayed out. You can only play the cards without requirements. Your tactical options are limited.

With rope equipped (provides Climbing), when you draw "Climb to Upper Window" it's playable. You have that option now. But "Force the Door" and "Examine Documents" remain grayed out because you lack their required equipment.

With rope and crowbar equipped (provides Climbing, Force, Mechanical), now both "Climb to Upper Window" and "Force the Door" are playable when drawn. Your tactical flexibility increased. But "Examine Documents" still requires scholar's kit you don't have.

The deck never changed. Those cards were always in the Mill's deck. Equipment just determined which drawn cards you could actually use. This creates visible aspiration - you see powerful approaches you can't currently attempt, motivating equipment acquisition for future visits.



### Stat Effects on Properties

Beyond gating card depth access, stats reduce danger and improve efficiency:

**High BoundStat Level:**
- Reduces RiskLevel effective danger probability significantly for cards bound to this stat
- Provides resource cost reduction for playing cards bound to this stat at lower depths
- Increases progress generation for successful card plays

**Related Stat Thresholds:**
- Cards requiring multiple stats benefit from all contributing stats being high
- Cross-stat synergies emerge naturally from threshold requirements

This creates smooth progression where improving stats makes existing cards safer and more efficient while unlocking new higher-depth cards.

### Property Interaction Examples

**Example Card 1: "Force the Mechanism"**
```
Type: Standard
Depth: 2
BoundStat: Authority
Category: Physical (Mental system)
RiskLevel: Risky
Visibility: Loud
ExertionLevel: Heavy
MethodType: Direct
Requirements:
  EquipmentCategory: Mechanical
  MinimumStamina: 40
```

Parser generates: Moderate Attention cost, significant Progress yield, high Exposure generation (Loud visibility), significant stamina cost (Heavy exertion), moderate danger probability (Risky with Authority 2), requires Mechanical category equipment to be in deck.

If player has Climbing equipment (not Mechanical), this card never appears in deck. If player has Mechanical equipment (tools, precision instruments, etc) and 50 stamina, card appears in deck and is playable. High Authority reduces danger. Playing this card progresses investigation but creates noise (Loud) drawing attention.

**Example Card 2: "Careful Observation"**
```
Type: Foundation  
Depth: 1
BoundStat: Insight
Category: Observational (Mental system)
RiskLevel: Safe
Visibility: Subtle
ExertionLevel: Minimal
MethodType: Analytical
Requirements: {}
```

Parser generates: Zero Attention cost, generates Attention when played, minimal Progress yield, minimal Exposure generation (Subtle visibility), minimal stamina cost (Minimal exertion), no danger (Safe risk).

Always playable with no requirements. Safe, quiet, low-cost economy card. Lantern provides bonus clue yield. Analytical knowledge provides Attention generation bonus. Foundation pattern in Mental system.

**Example Card 3: "Heartfelt Appeal"**
```
Type: Decisive
Depth: 3
BoundStat: Rapport
Category: Empathetic (Social system)
RiskLevel: Cautious
Visibility: Moderate
ExertionLevel: Moderate
MethodType: Intuitive
Requirements:
  StatThresholds: {Rapport: 3}
  MinimumStamina: 20
```

Parser generates: High Initiative cost, major Momentum yield, moderate Doubt risk (Cautious with Rapport 3), moderate stamina cost, requires Rapport 3 stat level to access this depth 3 card.

This card appears in deck only when you have Rapport 3+. In your starting hand, you might also have "Personal Struggles Understanding" knowledge card (if you learned about NPC's history). Playing the knowledge card first, then playing Heartfelt Appeal creates synergy - the knowledge card reduces Doubt accumulation, making the high-Initiative Decisive card safer to deploy.

### Implementation Benefits

This strongly typed property system provides:

**Compile-Time Safety:** Enum values catch typos and invalid combinations during compilation. No runtime string matching failures.

**Maintainability:** Changing property effects happens in parser rules, automatically affecting all cards with that property. No per-card updates needed.

**Consistency:** Same properties behave identically across all three systems. Risk is always risk, exertion is always exertion, visibility is always visibility.

**Extensibility:** Adding new equipment or knowledge types requires defining their property-based rules once, automatically applying to all applicable cards.

**Content Velocity:** Card creation is selecting enum values from dropdowns and writing narrative text. No manual mechanical specification needed.

**Balance Tuning:** Parser formula adjustments affect all cards using those properties proportionally. System-wide rebalancing is formulaic, not card-by-card.

The properties provide the grammar for mechanical effects, content creators provide the narrative vocabulary, and the parser composes them into functional cards.

---

## Decks as Source of Truth

The fundamental architectural principle is that decks are the authoritative source of what's possible in any challenge. Cards exist in specific decks. When you draw from a deck, what you draw is what you get. There is no runtime composition, no property matching, no abstract card inclusion logic. The deck is the deck.

### Deck Ownership and Persistence

Every challenge-spawning entity owns a specific deck that persists across all encounters:

**NPC Conversation Decks:** Each NPC has their own conversation deck containing all cards representing possible things you could say to them. Marcus's deck contains Marcus-specific conversational approaches. Elena's deck contains Elena-specific approaches. These decks never mix. When you talk to Marcus, you draw from Marcus's deck. When you talk to Elena, you draw from Elena's deck.

**Location Investigation Decks:** Each location has its own investigation deck containing all cards representing possible investigation approaches at that location. The Old Mill's deck contains Mill-specific investigation actions. The Ancient Tower's deck contains Tower-specific investigation actions. When you investigate the Mill, you draw from the Mill's deck.

**Deck Persistence:** Decks persist permanently across all encounters. If you talk to Marcus three times over three days, you're drawing from the same Marcus deck each time. If you investigate the Mill twice, you're drawing from the same Mill deck each time. The deck is the persistent state container.

### Authored Base Decks

When content creators author an NPC or location, they define that entity's base deck. This is the starting point - the cards that exist in this deck from the beginning before any player interaction.

**Creating NPC Base Deck:**
The author selects cards appropriate to this NPC's personality, knowledge, and relationship with the player. A merchant's deck contains cards about trade and negotiation. A guard's deck contains cards about authority and regulation. A friend's deck contains cards about personal connection and trust. The deck composition reflects who this NPC is and what kinds of conversations are possible with them.

If the NPC is suspicious by nature, their deck contains more cards with high Doubt risk. If the NPC is knowledgeable about history, their deck contains cards referencing historical facts. The author composes the deck to create the intended challenge character.

**Creating Location Base Deck:**
The author selects cards appropriate to this location's physical nature and narrative context. An ancient ruin's deck contains cards about examining old structures, historical analysis, and navigating decay. A bustling market's deck contains cards about social observation, crowd navigation, and public investigation challenges. A dark cave's deck contains cards about careful movement and limited visibility challenges.

If the location is unstable, the deck contains cards with structural danger. If the location is well-lit, it contains cards emphasizing visual observation. If the location is socially complex, it contains cards about reading social dynamics. The deck reflects what investigating this specific location entails.

### Requirements Gate Playability

Cards in decks may have requirements - equipment categories, stat thresholds, minimum resources. These requirements don't prevent the card from being in the deck. They prevent the card from being playable when drawn.

**Drawing Unplayable Cards:**
You draw from the deck normally. Some cards you draw might be unplayable because you don't meet their requirements. The UI shows these cards grayed out with clear indication of what's needed. "Climb to Upper Window - Requires: Climbing Equipment." The card exists, you can see it, but you cannot play it without the tools.

This creates visible aspiration. You see powerful approaches that exist at this location but you cannot currently attempt. This motivates acquiring equipment, building stats, or gathering resources to enable these approaches in future attempts.

**Equipment Effect:**
Equipment doesn't change what's in decks. Equipment makes cards playable that would otherwise be grayed out. When you equip rope, any card requiring Climbing equipment category becomes playable when drawn. The Mill's investigation deck always contained "Climb to Upper Window" card - equipping rope just makes it playable instead of grayed out.

**Stat Effect:**
Stats don't change what's in decks. Stats gate card depth access. When you draw a depth 4 Cunning card but only have Cunning 2, it's unplayable - grayed out with "Requires: Cunning 3+". Advancing to Cunning 3 makes this card playable when drawn. The card was always in the deck, you just couldn't use it yet.

**Resource Effect:**
Low health or stamina doesn't change what's in decks. Resource gates make cards unplayable when drawn if you lack sufficient resources. A card requiring minimum 40 stamina becomes grayed out when your stamina drops below 40. Resting to restore stamina makes the card playable again.

This gating system means deck size is stable and deterministic. You know exactly which deck you're drawing from. What varies is which drawn cards you can actually play based on your current equipment, stats, and resources.

### Knowledge Adds Cards to Specific Decks

Knowledge is the primary mechanism for permanent deck modification. When you learn something, a new card gets created and added to a specific deck. This is the tangible reward for investigation and conversation success.

**Learning About NPCs:**
Through conversation or investigation, you learn something about Marcus - perhaps his hidden motivation, his personal struggle, or his secret connection to someone. The game creates a knowledge card representing this understanding and adds it permanently to Marcus's conversation deck. 

Next time you talk to Marcus, this knowledge card exists in his deck and might be drawn. It's a better-than-normal card because knowledge provides advantage - maybe it costs less Initiative, maybe it yields more Momentum, maybe it reduces Doubt risk significantly. The specific knowledge determines the card's properties.

This knowledge card is Marcus-specific. It lives in Marcus's deck forever. It won't appear when talking to Elena or investigating the Mill. Knowledge is contextual and permanent.

**Learning About Locations:**
Through investigation, you discover something about the Old Mill - perhaps a structural weakness, a hidden passage, or historical information about its construction. The game creates a knowledge card representing this discovery and adds it permanently to the Mill's investigation deck.

Next time you investigate the Mill, this knowledge card exists in its deck and might be drawn. It provides advantage - maybe it reveals safer approaches, maybe it enables bypassing danger, maybe it yields bonus clues. The discovery determines the card's effect.

This knowledge card is Mill-specific. It lives in the Mill's deck forever. It won't appear when investigating the Tower or talking to Marcus. Knowledge is bound to where it applies.

**Knowledge Card Properties:**
Knowledge cards are just cards with the same properties as any other card: Type, Depth, BoundStat, Category, RiskLevel, Visibility, ExertionLevel, MethodType, Requirements. What makes them "knowledge" is simply that they were added to the deck through learning rather than being authored in the base deck.

Knowledge cards tend to have better properties than base cards - lower costs, higher yields, reduced risks. This reflects that understanding provides advantage. But mechanically they're handled identically to base cards during gameplay.

**Visual Distinction:**
Knowledge cards might be visually marked in the UI to remind players these are cards they earned through play rather than baseline options. But this is presentation choice, not mechanical distinction. The game systems treat knowledge cards exactly like base cards once they're in the deck.

### Deck Size Shows Mastery

Because knowledge adds cards to decks, deck size grows as you learn more. Marcus's conversation deck might start with twenty cards. After multiple conversations where you learned about him, his deck contains twenty-five cards. After thorough investigation, it contains thirty cards. The growing deck size is visible indicator of your accumulated understanding.

The Mill's investigation deck might start with thirty cards. After your first investigation where you discovered structural weaknesses, it contains thirty-two cards. After finding hidden passages, it contains thirty-four cards. The deck expansion shows your investigation progress.

This creates satisfying progression. Your history with NPCs and locations is literally represented by additional cards in their decks. You're not gaining abstract "relationship points" - you're earning specific tactical advantages that manifest as better cards in future encounters.

### Deck Modification as State Change

The primary way game state changes through challenges is deck modification:

**Successful Conversation:** Might add relationship token, might add burden card to NPC's deck (if you damaged relationship), might add observation card to NPC's deck (if you shared discovery), might add knowledge card to your copy of NPC's deck (if you learned something).

**Successful Investigation:** Adds knowledge cards to location's deck representing discoveries made. Might add danger cards if you damaged the location. Might remove obstacle cards if you permanently cleared obstacles.

**Failed Encounters:** Might add burden cards to NPC decks (damaged relationships), might add exposure cards to location decks (location now suspicious of investigators), might add injury cards to player's Physical challenge deck (persistent debuffs).

Deck modification is the persistence mechanism. The game state is literally "which cards are in which decks." Save files store deck compositions. Loading a save restores deck states. All progress is represented through deck changes.

### No Runtime Composition

This architecture eliminates runtime deck composition entirely. There is no logic checking "does player have equipment X, does location have property Y, therefore include cards Z." The deck exists. You draw from it. Some cards might be unplayable due to requirements, but they're in the deck either way.

This provides:

**Clarity:** You always know which deck you're drawing from. No hidden composition logic affecting what you might draw.

**Determinism:** The same deck state produces the same possible draws. No variability from property-matching algorithms.

**Performance:** No composition calculation at challenge start. The deck is ready to use immediately.

**Simplicity:** Content creation is deck authoring. No property systems, no abstract targeting, no inclusion rules. Just: here are the cards in this deck.

**Debuggability:** Deck contents are inspectable. You can see exactly what's in Marcus's deck or the Mill's deck at any time. No emergent composition to troubleshoot.

The elegance is that decks are both the game state container and the gameplay mechanism. What you draw from determines what you can do, and your actions modify what's in decks, which affects what you'll draw next time. The circular relationship between gameplay and state is direct and tangible.

---

## Content Authoring Workflow

### Creating Cards

Content creators define individual cards using strongly-typed properties. Each card is a reusable component that can exist in multiple decks.

**Card Creation:**
1. Choose Type: Foundation, Standard, or Decisive (enum selection)
2. Choose Depth: 1-3 for accessible content, higher for advanced (integer)
3. Choose BoundStat: Which stat gates this card and grants XP (enum: Insight/Authority/Rapport/Diplomacy/Cunning)
4. Choose Category: System-specific approach type (enum varies by tactical system)
5. Choose RiskLevel: How dangerous (enum: Safe/Cautious/Risky/Dangerous)
6. Choose Visibility: How noticeable (enum: Subtle/Moderate/Obvious/Loud)
7. Choose ExertionLevel: How exhausting (enum: Minimal/Light/Moderate/Heavy/Extreme)
8. Choose MethodType: Approach category (enum: Direct/Analytical/Intuitive/Negotiated/Deceptive)
9. Define Requirements (optional, only for specialized cards):
   - EquipmentCategory: Which category if any (enum: Climbing/Mechanical/Documentation/Illumination/Force/Precision/Medical/Securing)
   - StatThresholds: Additional stat requirements beyond BoundStat
   - MinimumHealth/MinimumStamina: Resource gates
10. Write Name: Short card title
11. Write Description Template: Narrative flavor text with context placeholders for AI generation

The parser generates all costs, effects, danger probabilities, and resource flows from these categorical choices. Most cards have no equipment requirements - only specialized cards (roughly one in twenty) need specific equipment categories.

Cards are stored in a card library and can be included in multiple decks. "Careful Observation" Foundation card might appear in dozens of investigation decks because it's universally applicable. "Force the Mechanism" Standard card appears in decks for locations with mechanical elements.

### Creating NPC Base Decks

When authoring an NPC, the creator defines that NPC's conversation deck by selecting appropriate cards from the card library.

**NPC Deck Composition:**
1. Choose twenty to forty cards from Social card library representing possible conversational approaches with this NPC
2. Include mix of Foundation (40%), Standard (40%), and Decisive (20%) for pacing
3. Include cards matching NPC personality - suspicious NPCs get doubt-generating cards, helpful NPCs get rapport-building cards
4. Include cards reflecting NPC knowledge - historians get historical reference cards, merchants get negotiation cards
5. Ensure deck includes cards bound to various stats - don't over-concentrate on single stat

The resulting deck reflects who this NPC is and what kinds of conversations are possible with them. Marcus the merchant's deck emphasizes Diplomacy cards about trade. Elena the scholar's deck emphasizes Insight cards about research. The deck composition creates the NPC's character mechanically.

**Growth Slots:**
When creating NPC deck, author marks "growth slots" - places where knowledge cards will be added when player learns about this NPC. These aren't pre-authored cards, they're placeholders indicating "successful deep conversation will add card here." The actual knowledge cards get generated procedurally or by AI when player earns them.

### Creating Location Investigation Decks

When authoring a location, the creator defines that location's investigation deck by selecting appropriate cards from the card library.

**Location Deck Composition:**
1. Choose thirty to fifty cards from Mental card library representing possible investigation approaches at this location
2. Include mix of Foundation (30%), Standard (50%), and Decisive (20%) for challenge pacing
3. Include cards reflecting location's physical nature - dark locations get illumination-requiring cards, unstable locations get danger cards, ancient locations get historical analysis cards
4. Include cards for multiple approach types - physical methods, analytical methods, social methods if location is populated
5. Ensure cards span stat requirements - don't make location only accessible via single stat path

The resulting deck reflects what investigating this location entails. The Old Mill's deck emphasizes structural investigation, historical research, and physical navigation cards because that's what the Mill requires. The bustling marketplace deck emphasizes social observation and crowd navigation because that's what markets require.

**Growth Slots:**
Mark places where knowledge cards will be added when player discovers things about this location. Successful investigation phases add knowledge cards to these growth slots. Like NPC decks, these cards may be procedurally generated or AI-created based on what the player discovered.

### Knowledge Card Generation

Knowledge cards are not pre-authored. They're generated when earned through successful challenges. The generation system receives context about what was learned and creates appropriate card.

**Generation Inputs:**
- Which deck the knowledge card goes into (Marcus's deck, Mill's deck, etc)
- What was discovered (structural weakness, personal secret, historical fact, etc)
- Challenge difficulty and player performance (determines knowledge card power level)

**Generation Outputs:**
- Card properties (tends toward lower costs, higher yields, reduced risks than base cards)
- Card description (AI-generated based on discovery context and deck owner)
- Card name (reflects the specific knowledge gained)

The generated knowledge card gets added to the specified deck permanently. Next time player engages with that deck's owner, the knowledge card is in the deck and might be drawn.

**Example:**
Player completes difficult conversation with Marcus, learning about his hidden debt. The system generates knowledge card: Type=Standard, Depth=2, BoundStat=Rapport, Category=Empathetic, reduced Initiative cost, name="Acknowledge His Burden", description generated by AI referencing the debt. This card gets added to Marcus's conversation deck forever.

### Creating Strategic Activities

**Investigation Creation:**
1. Identify which location this investigation takes place at (determines which investigation deck is used)
2. Define number of phases (2-5 typically)
3. For each phase, define Progress threshold to advance
4. Define rewards for completion (relationship tokens, items, Observation cards added to NPC decks)
5. Define consequences for high Exposure (confrontation triggers, reputation changes)

The investigation framework uses the location's existing deck. It doesn't have its own cards. When you investigate the Mill, you draw from the Mill's investigation deck. The investigation activity just tracks phase progression and rewards.

**Travel Route Creation:**
1. Define start and end location spots
2. Define base time cost
3. List obstacles encountered on this route
4. For each obstacle, define which challenge type (Social/Mental/Physical) and difficulty
5. Define route improvement conditions (what permanently makes travel easier)

Travel obstacles spawn tactical challenges using appropriate decks. A Physical obstacle at a creek might use a generic "wilderness traversal" deck. A Social obstacle at a checkpoint might use the guard NPC's conversation deck. The route doesn't have its own cards - it references existing decks for challenges.

### Authoring Efficiency

This deck-based authoring provides significant efficiency benefits:

**Card Reuse:** Create "Careful Observation" card once, include it in fifty investigation decks. Update the card template once, all decks reflect the update. No duplication of mechanical definitions.

**Deck Templates:** Create "Suspicious NPC Template" deck composition, reuse it for multiple suspicious NPCs with different narrative descriptions. Mechanical structure is consistent, narrative varies.

**Knowledge Generation:** Don't pre-author hundreds of knowledge cards. Generate them procedurally when earned based on context. Ensures knowledge cards are contextually appropriate without explosive content creation burden.

**Clear Scope:** Each authored entity (NPC, location) has clear scope - one deck. You're not managing abstract property systems or inclusion rules. Just: here are the cards in this deck.

The authoring tools would provide deck composition interface showing card library, allowing drag-and-drop deck assembly, with clear indication of deck balance and coverage across stats and approaches. Authors compose decks like building playlists, selecting appropriate cards for the challenge they're creating.
4. Define obstacle improvement conditions (what makes future travel easier)
5. Assign card decks to each obstacle

The travel activity framework handles obstacle encounters, route improvement tracking, and tactical system spawning.

---

## Implementation Guidance

### Separation of Concerns

The strategic layer knows about locations, routes, resources, and activities. It doesn't know about cards or tactical systems. When investigation needs Mental challenge, strategic layer calls "SpawnMentalTactical(deck, context)" and receives results back.

The tactical layer knows about cards, transient resources, and resolution mechanics. It doesn't know about investigation phases or travel routes. It receives a deck and context, provides gameplay, and returns outcome to caller.

Activities (Investigation, Travel) are strategic constructs that orchestrate tactical spawning. They maintain their own state (phase progression, discoveries, obstacles overcome) and translate between strategic context and tactical requirements.

### State Management

Strategic state must be serializable for save/load. This includes: player location, resources, stats, equipment, knowledge, time, NPC relationship states, location states, route states, and activity progress.

Tactical state is transient and discards after challenge resolution. However, activities maintain their own persistent state (investigation progress, travel progress) that must serialize.

The separation ensures save files don't need tactical system internals - just strategic state and activity frameworks.

### Parser Implementation

The card parser takes categorical properties and generates mechanical values using formulas. These formulas should be tunable through configuration rather than hardcoded. This allows balancing without changing parser code.

Parser ruleset defines:
- Type effects (Foundation generates, Standard spends moderately, Decisive spends heavily)
- Depth scaling (how much power increases with depth)
- Category mappings (which resource types each category affects)
- Tag modifiers (how tags transform base values)
- Stat-depth gates (which stat levels unlock which depths)

### AI Integration Points

AI generation happens at specific points:
- Situation descriptions before player action choice
- Card narrative text (can be generated once and cached)
- Outcome descriptions after card resolution
- Transition text between phases or obstacles

The AI service receives structured state data and returns formatted text. The game logic never depends on AI output content - only uses it for presentation.

---

## Design Principles Achieved

This architecture achieves the original integration goals:

**Mechanics Express Narrative:** Card types represent conversational dynamics (Foundation = building rapport), investigative methods (Observe = examining carefully), and physical tactics (Execute = committing to action). Players think narratively, act mechanically.

**Systems Create Impossible Choices:** Builder-spender dynamics mean you can't do everything. Spend Attention now or save for better opportunity? Risk high Exposure for faster progress or investigate cautiously? Every decision has meaningful trade-offs.

**Verisimilitude Through Structure:** The three systems behave differently because their domains behave differently. Conversations can't pause mid-flow. Investigations allow stepping away. Physical confrontations demand commitment. Structure matches reality.

**Unified Progression:** Stats as universal approaches mean all challenges contribute to all capabilities. You don't build fragmented skill trees - you build coherent tactical identity expressed across all challenge types.

**Circular Integration:** Systems feed each other through strategic resources. Social success provides knowledge for Mental challenges. Mental success creates observation cards for Social challenges. Physical success builds reputation affecting both. No system is silo.

**Preparation Matters:** Strategic resource management determines tactical options. Good preparation opens superior approaches. Poor preparation restricts to difficult methods. Success requires thinking beyond immediate challenge.

**Failure Teaches:** The debt systems accumulate handicaps from failure, but state persistence and multiple approaches mean failed attempts aren't wasted. You learn what you need, retreat, prepare, and return stronger.

The architecture solves the core problem: making simulator depth and visual novel narrative work together naturally through shared mechanical grammar expressing different narrative contexts.

---

## Conclusion

This strategic-tactical layer architecture provides the foundation for integrating complex simulation mechanics with visual novel narrative presentation. The two-layer structure separates world navigation (strategic) from challenge resolution (tactical). The three tactical systems handle different challenge types while sharing unified card-based patterns. Activities orchestrate tactical challenges rather than being systems themselves. Stats as universal approaches create coherent character identity across challenge types. The three-part persistence pattern (consumables, debt, progress) creates meaningful consequences and progression. Circular interconnection between systems creates preparation loops and strategic depth.

The result is a game where mechanics function as the grammar of narrative choice - players think in story terms while the underlying simulation tracks complex state, resources, and consequences. Neither mechanics nor narrative overshadow each other because they're the same thing expressed at different layers of abstraction.