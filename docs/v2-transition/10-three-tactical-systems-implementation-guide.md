# Wayfarer: Strategic-Tactical Layer Architecture

## Executive Summary

Wayfarer integrates visual novel narrative presentation with simulator mechanics depth through a two-layer architecture inspired by XCOM. The strategic layer provides the world navigation hub where players exist at locations and initiate activities. The tactical layer provides card-based challenge resolution systems where moment-to-moment gameplay occurs. Three distinct tactical systems (Social, Mental, Physical) handle different challenge types while sharing unified mechanical patterns.

Goals spawn engagement types which reference specific card decks. Investigation and Travel function as strategic activities that orchestrate multiple tactical engagements rather than being tactical systems themselves. Knowledge accumulates in signature decks attached to NPCs and locations, providing contextual advantages when engaging with those entities.

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

**Goals and Engagement Types:**
NPCs and locations define goals - actionable choices presented to the player in the UI. Each goal references exactly one engagement type. Engagement types are the bridge between strategic intentions and tactical execution - they define which tactical system spawns (Social/Mental/Physical) and which card deck to use for resolution.

**Activities as Orchestrators:**
Investigation and Travel are strategic activities, not tactical systems. When you investigate a location, you enter a multi-phase activity that presents goals at each phase, with each goal spawning its associated engagement type. When you travel a route, you encounter obstacles that present goals spawning appropriate tactical engagements. These activities track their own state and progression but delegate actual challenge resolution to tactical systems through the engagement type mechanism.

### Tactical Layer: Challenge Resolution

The tactical layer is where moment-to-moment gameplay happens through card-based systems. When the strategic layer presents a goal and the player selects it, the associated engagement type's tactical system spawns with its specific deck of cards representing available approaches.

The tactical layer receives strategic state as input (your stats, resources, equipment, knowledge) and outputs strategic state modifications (XP gained, resources spent, world changes made). During tactical play, transient resources exist only for that challenge - once you return to strategic layer, these disappear.

Three tactical systems handle different challenge types. Each has unique identity reflecting its domain while sharing core mechanical patterns. Cards are defined once and can appear in multiple engagement type decks, but within a single tactical instance, you draw from exactly one engagement deck plus any matching knowledge cards from the entity's signature deck.

---

## Goals and Engagement Types

### Goals as UI Actions

Goals are what the player clicks to initiate tactical challenges. NPCs and locations define the goals available to the player at any moment.

**NPC Goals:**
Each NPC defines multiple goals representing different interaction possibilities:
- "Request urgent delivery" (Elena's desperate request goal)
- "Chat casually" (Martha's friendly conversation goal)  
- "Negotiate price" (Marcus's mercantile exchange goal)
- "Confront about evidence" (Elder's accusation goal)

**Location Goals:**
Each location spot defines goals representing possible actions at that place:
- "Investigate interior" (Mill's investigation goal)
- "Search for clues" (Ancient ruins exploration goal)
- "Force entry" (Locked building breach goal)

**Conditional Goals:**
Goals can have prerequisites - knowledge facts, equipment, stats, time of day, relationship levels. Martha's "Ask about daughter" goal only appears after learning she had a daughter. Mill's "Climb to window" goal only appears if you have climbing equipment.

### Engagement Types Define Tactical Resolution

Each goal references exactly one engagement type. The engagement type specifies:

**Tactical System:** Which of the three systems (Social, Mental, Physical) handles this engagement

**Card Deck:** Which specific deck of cards is used for this engagement

**Victory Threshold:** How much progress resource needed to succeed

**Engagement Type Examples:**
- "Desperate Request" (Social system, Desperate Request deck, Momentum threshold 12)
- "Mill Interior Investigation" (Mental system, Mill Interior deck, Progress threshold 10)
- "Creek Crossing" (Physical system, Creek Crossing deck, Breakthrough threshold 8)
- "Information Gathering" (Social system, Information Gathering deck, Momentum threshold 8)
- "Confrontation" (Social system, Confrontation deck, Momentum threshold 14)

The engagement type is the authoritative definition of how a tactical challenge works. When player clicks goal, the system loads that engagement type's deck and spawns the appropriate tactical system.

### Deck Ownership

**Engagement types own decks, not NPCs or locations.** This is critical for understanding the architecture.

A single NPC might offer goals that reference multiple engagement types:
- Elena's "Request delivery" goal → Desperate Request engagement → Desperate Request deck
- Elena's "Chat about weather" goal → Casual Chat engagement → Casual Chat deck  
- Elena's "Discuss legal matters" goal → Professional Conversation engagement → Professional Conversation deck

These are different decks with different tactical dynamics. Elena doesn't own these decks - the engagement types do. Multiple NPCs might offer goals using the same engagement type and thus the same deck.

Similarly, a location might offer goals referencing multiple engagement types:
- Mill's "Survey exterior" goal → Exterior Assessment engagement → Exterior Assessment deck
- Mill's "Investigate interior" goal → Interior Investigation engagement → Interior Investigation deck
- Mill's "Search for evidence" goal → Evidence Gathering engagement → Evidence Gathering deck

The Mill doesn't own these decks. Each engagement type has its own deck. The Mill just defines which engagement types are relevant to investigating it.

### Card Reuse Across Decks

Cards are defined once in JSON and can appear in multiple engagement type decks. "Careful Observation" Foundation card might appear in:
- Exterior Assessment deck
- Interior Investigation deck  
- Evidence Gathering deck
- Crime Scene Analysis deck

It's the same card definition, instantiated in multiple decks. When you draw "Careful Observation" from Interior Investigation deck, it behaves identically to drawing it from Evidence Gathering deck - same properties, same mechanical effects.

This enables efficient content creation: define powerful card templates once, include them in all relevant engagement type decks. Changing the card definition updates it everywhere it appears.

---

## Signature Decks: Knowledge Repository

While engagement types own the tactical decks, each NPC and location has one signature deck that accumulates knowledge about that entity.

### Signature Deck Structure

**One Per Entity:**
- Elena has Elena's signature deck
- Marcus has Marcus's signature deck
- The Mill has the Mill's signature deck
- Ancient Ruins has Ancient Ruins' signature deck

**Contains Knowledge Cards Only:**
Signature decks don't contain base tactical cards. They contain only knowledge cards earned through successful engagements with that entity. When you learn something about Elena through conversation, a knowledge card gets added to Elena's signature deck. When you discover something about the Mill through investigation, a knowledge card gets added to the Mill's signature deck.

**Tactical Type Declaration:**
Each knowledge card in a signature deck declares which tactical type it applies to:
- Social type: applies when engaging socially with this entity
- Mental type: applies when engaging mentally with this entity  
- Physical type: applies when engaging physically with this entity

A signature deck can contain cards of multiple tactical types. The Mill's signature deck might contain Mental knowledge cards (structural weaknesses discovered) and Physical knowledge cards (safe traversal routes found).

### Knowledge Card Application

When a tactical engagement spawns, the system:

1. Loads the engagement type's base deck
2. Checks the relevant entity's signature deck for matching tactical type cards
3. Adds matching knowledge cards directly to player's starting hand (not shuffled into deck)

**Example - Social with Elena:**
- Player clicks Elena's "Request urgent delivery" goal
- Loads Desperate Request engagement deck
- Checks Elena's signature deck for Social type cards
- Player has "Elena's personal struggles" knowledge card (Social type) from prior conversation
- Card added to starting hand
- Player begins with Desperate Request deck to draw from + knowledge card in hand

**Example - Mental at Mill:**
- Player clicks Mill's "Investigate interior" goal  
- Loads Interior Investigation engagement deck
- Checks Mill's signature deck for Mental type cards
- Player has "Wheel mechanism broken" knowledge card (Mental type) from exterior survey
- Card added to starting hand
- Player begins with Interior Investigation deck + knowledge card in hand

**Example - Physical at Creek:**
- Player encounters creek crossing obstacle during travel
- Loads Creek Crossing engagement deck
- Creek is generic obstacle with no signature deck
- Player begins with only Creek Crossing deck, no knowledge cards

Knowledge cards in hand provide immediate tactical advantage. They're typically stronger than base cards - lower costs, higher yields, or reduced risks. This represents understanding providing leverage.

### Knowledge Card Persistence

Knowledge cards have two persistence modes defined at creation:

**Persistent Knowledge:**
Card remains in signature deck after being played. It represents understanding that continues to be valuable. "Mill structural weakness" remains useful every time you investigate the Mill - it's not consumed by use. These cards appear in hand for every future engagement of matching tactical type with this entity.

**Consumable Knowledge:**
Card is removed from signature deck after being played. It represents a one-time opportunity or knowledge that becomes irrelevant once acted upon. "Elder's corruption evidence" might only be playable once - after you use it to confront him, the knowledge is spent. The card disappears from his signature deck permanently.

The card definition specifies persistence mode when created.

### Signature Deck Growth

Signature decks grow through successful engagements:

**After Successful Social Engagement:**
Might earn knowledge about the NPC's motivations, secrets, relationships, or vulnerabilities. These become Social type knowledge cards in their signature deck.

**After Successful Mental Engagement:**  
Might discover location features, historical facts, structural details, or hidden elements. These become Mental type knowledge cards (and possibly Physical type) in the location's signature deck.

**After Successful Physical Engagement:**
Might learn opponent patterns, environmental advantages, or tactical weaknesses. These become Physical type knowledge cards in the relevant entity's signature deck.

The signature deck is visible character/location progression. Elena's signature deck starting with 0 cards, growing to 5 cards over multiple conversations shows your deepening understanding of her. The Mill's signature deck growing from 0 to 8 cards shows your accumulated investigation discoveries.

---

## The Three Tactical Systems

### Social System: Influencing Through Conversation

The Social system resolves interpersonal challenges where the goal is reaching understanding or agreement with an NPC. This might be negotiation, persuasion, information gathering, or emotional connection.

**Core Identity:**
Conversations flow organically until natural endpoint. They have no fixed length - a conversation continues until consensus is reached or the relationship breaks down. The system models the NPC's emotional state and receptiveness through resources that shift as you speak.

**Player Actions:**

**Listen Action:** Draw cards from the engagement deck into your hand. This represents listening to the NPC, giving them space to express themselves, and gathering conversational options. The deck contains cards representing things you could say in response. Drawing cards doesn't advance time within the conversation.

**Speak Action:** Play a card from your hand. This represents saying something, advancing your conversational goals, and shifting the emotional dynamic. Playing cards consumes resources and generates effects that move the conversation forward.

**Leave Action:** End conversation immediately. If Momentum threshold not reached, this is conversation failure - you didn't achieve your goal. The NPC remembers this failure. Use when conversation going badly and you want to cut losses before relationship damage worsens.

**Transient Resources:**

**Initiative** represents your capacity to guide conversation direction. You must build Initiative through Foundation cards (listening, small talk, building rapport) before you can play more significant cards. This creates builder-spender dynamics where you generate resources to enable powerful moves. Starts at 0 each conversation.

**Momentum** represents progress toward your conversational goal. Different engagement types have different Momentum thresholds. Casual Chat might need Momentum 6, Desperate Request needs Momentum 12, Confrontation needs Momentum 16. Reaching threshold means successful conversation.

**Doubt** represents the NPC's patience wearing thin or losing interest. As Doubt accumulates, the conversation moves toward breakdown. Reaching Doubt 10 ends the conversation as failure. The NPC becomes harder to talk to afterward through burden cards added to their signature deck.

**Cadence** tracks the balance between speaking and listening. Starts at 0. Speaking increases Cadence (+1), Listening decreases Cadence (-2). High Cadence (speaking too much without listening) increases Doubt generation when you speak. Low Cadence (listening too much) grants bonus cards when you Listen. This forces rhythm - you can't spam powerful cards without cost.

**Statements** tracks conversation history as count of cards played. Some powerful Decisive cards require minimum Statements count (e.g., "requires 4+ statements"), representing conversational context needed for sophisticated moves. This creates sequencing requirements.

**Strategic Persistence:**

**Relationship Tokens** accumulate with each NPC through successful conversations. Tokens affect future engagement possibilities - higher tokens might unlock more sophisticated engagement types with better outcomes.

**Burden Cards** enter the NPC's signature deck (as Social type) when conversations fail badly. These cards appear in future Social engagements with that NPC and have negative effects when drawn - they increase Doubt, reduce Momentum gain, or cost extra Initiative. Relationship damage persists - the NPC remembers your failures.

**Knowledge Cards** are granted to the NPC's signature deck when conversations succeed and you learn something valuable about them. These cards appear in future Social engagements providing tactical advantages.

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

**Observe Action:** Draw cards from the engagement deck into your hand. This represents examining the puzzle, looking at different angles, and identifying approaches to try. Each observation takes time and might generate exposure if you're being obvious.

**Act Action:** Play a card from your hand. This represents testing a hypothesis, trying an approach, or making progress on understanding. Actions consume attention, advance progress, and might generate consequences.

**Leave Action:** Exit the mental challenge and return to strategic layer. Progress and exposure state saves - when you return, you continue where you left off. Use when you need different equipment, more resources, or time to think. Unlike Social, leaving Mental challenges doesn't mean failure.

**Transient Resources:**

**Attention** represents your mental capacity to focus on this challenge. You must generate Attention through Observe actions (looking carefully, taking notes, building understanding) before you can attempt complex analytical moves. This creates the same builder-spender pattern as Social but for intellectual effort. Starts at 0 each engagement.

**Progress** represents advancement toward understanding threshold. Different engagement types have different Progress requirements. Simple puzzles need Progress 6, complex investigations need Progress 12, advanced mysteries need Progress 18. Reaching threshold completes the challenge successfully.

**Exposure** represents how much you've disturbed the location or drawn attention. Physical actions make noise, repeated observations look suspicious, aggressive approaches leave evidence. Reaching Exposure 10 triggers consequences - someone notices, structure becomes dangerous, or you must deal with social confrontation.

**Observe/Act Balance** tracks investigation rhythm. Starts at 0. Acting increases balance (+1), Observing decreases balance (-2). High balance (acting too much without observing) increases Exposure generation and risk of mistakes. Low balance (observing too much without acting) wastes time and Attention. Optimal play alternates between observation and action.

**Clue Types** represent specific understanding accumulated. Cards generate different clue types: Structural (physical knowledge), Historical (documented facts), Environmental (contextual information), Social (behavioral patterns), Hidden (secrets discovered). Some later-phase engagement types require specific clue combinations, not just total Progress. This creates investigation depth beyond single number optimization.

**Strategic Persistence:**

**Location Exposure** accumulates across all Mental engagements at a location. If you were loud or obvious during first investigation, that exposure persists. Next Mental engagement at this location starts with elevated exposure baseline - people are watching, structure is damaged, or evidence has been moved. This creates consequences for aggressive investigation and rewards careful approach.

**Location State** tracks permanent changes to physical locations. Forced doors stay forced, cleared obstacles stay cleared, discovered passages stay discovered. Progress isn't lost between Mental engagements - the location remembers what you've done.

**Knowledge Facts** are acquired when you complete Mental engagements. These facts persist forever, enable conversation topics, unlock new engagement goals, and become knowledge cards in signature decks. Knowledge is the primary reward for investigation.

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
Physical challenges demand immediate resolution. Once engaged, retreat is difficult and costly. The challenge proceeds in rounds whether you're ready or not. This isn't about opponent agency (they don't have any, it's still a system) - it's about the situation having momentum. If you're lifting something heavy, you can't pause halfway. If you're in a fight, the opponent's responses are system-modeled but create time pressure through accumulating danger.

**Player Actions:**

**Assess Action:** Draw cards from the engagement deck into your hand. This represents reading the situation, identifying options, and preparing responses. Assessment happens quickly but you can't assess forever - commitment must follow.

**Execute Action:** Play a card from your hand. This represents physical commitment to an approach. Execution consumes resources, advances progress toward victory, and generates danger of failure consequences.

**Escape Action:** Attempt to flee the Physical challenge. Requires playing special Escape cards or meeting specific conditions. Failure means continued engagement with penalties. Success means returning to strategic layer but might mean the obstacle/opponent remains undefeated, blocking progress. This is costly retreat option.

**Transient Resources:**

**Position** represents tactical advantage, readiness, and preparation. You must build Position through Assess actions (watching carefully, finding footing, reading patterns) before you can execute decisive physical actions. Foundation cards generate Position. Starts at 0 each engagement.

**Breakthrough** represents advancement toward completion or victory. Different engagement types have different Breakthrough requirements. Simple physical obstacles need Breakthrough 6, challenging athletics need Breakthrough 10, combat encounters need Breakthrough 12-16. Reaching threshold means successful resolution.

**Danger** represents accumulating risk of injury or failure consequences. Physical challenges directly threaten health and stamina. As Danger accumulates, probability of negative outcomes increases. Reaching Danger 10 triggers consequences: injury cards added to player deck, health/stamina loss, or forced defeat.

**Commitment** tracks Assess vs Execute rhythm. Starts at 0. Executing increases Commitment (+1), Assessing decreases Commitment (-2). High Commitment (too much execution without assessment) increases Danger generation significantly. Low Commitment (too much hesitation) wastes stamina through prolonged exertion. Balance required.

**Approach History** tracks physical tactics attempted as count of Execute actions taken. Some powerful Decisive cards require minimum Approach History (e.g., "requires 3+ approaches"), representing learning opponent/situation through engagement. This creates dynamic where you build understanding through action.

**Strategic Persistence:**

**Injury Cards** enter player's Physical deck when challenges go badly and Danger threshold reached. These cards persist across all future Physical engagements until healed through rest or medicine. They have debuff effects when drawn - reduce Position generation, increase Danger, or prevent certain cards from being played. Injury accumulation makes future physical activity progressively more dangerous.

**Equipment Degradation** occurs through Physical use. Tools can break, armor can be damaged, weapons can dull when used in harsh Physical engagements. This creates maintenance costs and risk of losing capabilities mid-activity. Degraded equipment might cease to enable its card category until repaired.

**Reputation** changes through Physical outcomes. Victory increases respect or fear. Defeat damages standing. Witnesses spread word. This affects both Social engagements (NPCs respond differently based on your physical reputation) and future Physical encounters (opponents might be more cautious or more aggressive based on your record).

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

Every tactical system provides exactly two primary player actions:

**The Draw Action (Listen/Observe/Assess):** Draws cards from the engagement deck into your hand. This represents gathering options, building resources, and preparing for significant moves. Drawing is generally low-cost but has consequences through balance tracking. Cards go from deck to hand.

**The Play Action (Speak/Act/Execute):** Plays a card from hand. This represents committing to an approach, spending resources, and advancing toward goals. Playing cards drives the challenge forward but depletes resources requiring management. Cards go from hand to discard.

Both systems also have Leave/Escape action for exiting tactical engagement with varying consequences.

### Card Structure

Every card regardless of system has these properties:

**Type** determines resource flow. Foundation cards generate primary resource when played (Initiative/Attention/Position). Standard cards spend primary resource moderately for meaningful progress. Decisive cards spend primary resource heavily for powerful effects. This three-tier structure creates natural pacing and tactical depth.

**Depth** determines power level and stat requirements. Integer 1-10. Higher depth means stronger effects but requires higher bound stat to access. Depth creates progression - as your stats increase, you gain access to more powerful cards. Depth 1-2 accessible with stat level 1, depth 3-4 requires stat level 2, depth 5-6 requires stat level 3, continuing this pattern.

**BoundStat** determines which stat gates access and receives XP. Each card binds to exactly one of the five stats (Insight/Authority/Rapport/Diplomacy/Cunning). Playing that card grants experience toward that stat. Stat level determines maximum card depth accessible - higher stats unlock deeper tactical options.

**Category** determines thematic approach and specific effects. Categories are system-specific enums:
- Social: Empathetic, Logical, Assertive, Mercantile, Deceptive  
- Mental: Analytical, Physical, Observational, Social, Synthesis
- Physical: Aggressive, Defensive, Tactical, Evasive, Endurance

Category determines which progress resource the card emphasizes, which clue types it yields (Mental), or which tactical dynamics it manipulates. Parser maps categories to specific resource effects deterministically.

**Tags** provide additional properties enabling equipment requirements and universal modifiers. List of enum values. A card might be tagged [Climbing, Risky]. Tags compose - cards can have multiple tags applying multiple effects.

### Universal Card Properties

These properties affect cards across all three tactical systems:

**RiskLevel** (enum: Safe, Cautious, Risky, Dangerous):
Determines base danger probability regardless of system. Safe has near-zero failure consequences. Dangerous has high probability of negative outcomes. In Social, risk means relationship damage. In Mental, risk means injury from investigation hazards or exposure spikes. In Physical, risk means direct health damage. Higher BoundStat reduces danger probability but doesn't change the card's base RiskLevel classification.

**Visibility** (enum: Subtle, Moderate, Obvious, Loud):
Determines how noticeable the action is regardless of system. Affects Exposure generation in Mental (obvious actions disturb locations faster), Doubt increase in Social (loud conversational moves increase Doubt), and enemy awareness in Physical (obvious actions alert opponents). Subtle generates minimal exposure/attention. Loud generates significant exposure/attention.

**ExertionLevel** (enum: Minimal, Light, Moderate, Heavy, Extreme):
Determines stamina cost regardless of system. Even Social and Mental cards have exertion - intense negotiations drain stamina through stress, sustained concentration exhausts mentally. Heavy exertion cards cost significant stamina and become unavailable when player stamina drops below threshold. This creates stamina management across all challenge types.

**MethodType** (enum: Direct, Analytical, Intuitive, Negotiated, Deceptive):
Categorizes the approach being taken, determining which knowledge facts provide bonuses. This is orthogonal to system-specific Category. A card with MethodType.Analytical benefits from analytical knowledge facts regardless of which tactical system it's in. Knowledge matching MethodType provides cost reduction or effect enhancement.

### Requirements Structure

The Requirements property is strongly typed structure containing explicit prerequisites:

**EquipmentCategory** (enum: None, Climbing, Mechanical, Documentation, Illumination, Force, Precision, Medical, Securing):
Declares required equipment category. Default is None (no equipment needed). Cards requiring equipment can only appear in engagement decks when player has equipment providing that category. A card requiring Climbing only exists in deck if player has rope, grappling hook, or other climbing equipment.

Equipment items provide one or more categories. Rope provides both Climbing and Securing. Crowbar provides both Force and Mechanical. Scholar's kit provides Documentation. Lantern provides Illumination.

**StatThresholds** (Dictionary<StatType, int>):
Minimum stat levels required beyond the BoundStat requirement. A card might require Insight 3 (as BoundStat) AND Authority 2 (as threshold), representing combined capability needs. Empty dictionary means only BoundStat requirement applies.

**MinimumHealth** and **MinimumStamina** (integers):
Resource gates preventing card play when too damaged or exhausted. A card requiring MinimumStamina 30 becomes unplayable when stamina drops below 30. Defaults to 0 (always playable regardless of resources).

All requirements must be met simultaneously for card to be playable. UI shows unmet requirements clearly when displaying unusable cards.

### Parser Rules

Cards don't manually specify costs and effects. The parser generates all mechanical values from categorical properties using deterministic formulas.

**Parser Inputs:** Type, Depth, BoundStat, Category, RiskLevel, Visibility, ExertionLevel, MethodType, Tags

**Parser Outputs:** Primary resource cost, primary resource yield, progress resource effect, timer resource effect, balance effect, danger probability, stamina cost, clue types (Mental only)

The same "Force Door" card template works across hundreds of engagements with only description text changing. All mechanical behavior derives from: Type=Standard, Depth=2, BoundStat=Authority, Category=Physical, RiskLevel=Risky, Visibility=Loud, ExertionLevel=Heavy.

Parser formulas are configurable for balancing without code changes. During playtesting, formula tuning adjusts all cards using affected properties proportionally.

---

## Stats as Universal Approaches

The five player stats do not represent domain-specific skills (social skill, mental skill, physical skill). They represent universal tactical approaches applicable across all challenge types.

**Insight: Analytical Awareness**
Understanding through systematic observation and logical deduction. In Social engagements, this is asking targeted questions and reading between lines. In Mental engagements, this is methodical examination and pattern recognition. In Physical engagements, this is studying opponent patterns and identifying weaknesses. Insight represents rational problem-solving.

**Authority: Forceful Approach**
Solving through assertion of will and direct action. In Social engagements, this is demands and forceful statements. In Mental engagements, this is breaking through obstacles and forcing access. In Physical engagements, this is aggressive overwhelming tactics. Authority represents power-based solutions.

**Rapport: Intuitive Connection**
Understanding through empathy and reading unstated cues. In Social engagements, this is emotional connection and sensing feelings. In Mental engagements, this is intuitive leaps about what matters. In Physical engagements, this is sensing timing and reacting to subtle signals. Rapport represents emotional intelligence.

**Diplomacy: Methodical Negotiation**
Solving through careful bargaining and finding mutual benefit. In Social engagements, this is negotiation and mercantile thinking. In Mental engagements, this is purchasing information and organized documentation. In Physical engagements, this is measured approach and seeking compromise. Diplomacy represents strategic patience.

**Cunning: Indirect Method**
Achieving goals through misdirection and unconventional thinking. In Social engagements, this is manipulation and subtle steering. In Mental engagements, this is lateral thinking and finding hidden elements. In Physical engagements, this is dirty fighting and environmental exploitation. Cunning represents clever subversion.

This unified approach system means every stat matters for every challenge type. You don't build "social character" vs "combat character" - you build "insightful character" who uses analysis in all contexts, or "cunning character" who uses indirect methods everywhere. This creates coherent character identity across challenge types rather than fragmented skill trees.

### Stat Progression Integration

Playing cards grants experience to the bound stat. A card bound to Cunning grants Cunning XP when played regardless of whether it's Social, Mental, or Physical engagement. XP accumulates to thresholds where the stat level increases.

Stat level determines card depth access across all systems:
- Stat Level 1: Access depths 1-2 (Foundation + basic Standard)
- Stat Level 2: Access depths 1-4 (Foundation + Standard)
- Stat Level 3: Access depths 1-6 (Foundation + Standard + basic Decisive)
- Stat Level 4: Access depths 1-8 (all card types)
- Stat Level 5+: Access depths 1-10 (master level)

Higher stat level also reduces danger probability for cards bound to that stat and improves resource efficiency when playing those cards.

This creates natural progression: playing challenges improves stats, improved stats unlock better cards, better cards make you more capable in all challenge types. Success in one domain improves capability in other domains through shared stat system.

---

## Resource Architecture

### Strategic Resources (Persist)

These resources exist in the strategic layer and persist across all activities:

**Health and Stamina:** Your physical condition. Health represents damage tolerance - injury and harm reduce health. Stamina represents energy for exertion - strenuous activity depletes stamina. Both regenerate slowly through rest or quickly through recovery actions. Low health restricts risky physical actions. Low stamina restricts high-exertion actions in any system.

**Coins:** Economic resource for purchasing equipment, services, and information. Earned through work, obligation completion, and discoveries. Spent on preparation and access. Creates economic pressure through competing demands.

**Time Segments:** The limited resource of time. Each day contains fixed segments. All activities consume segments. Obligations have deadlines. Weather changes on schedule. NPCs are available at different times. Time creates urgency without arbitrary timers.

**Equipment:** Physical items providing capabilities. Equipment items declare which categories they provide (Climbing, Mechanical, etc.). During deck composition for engagement types, cards requiring equipment categories are only included if player has matching equipment. Equipment also has durability that degrades through use, especially in Physical engagements.

**Knowledge Facts:** Discrete pieces of understanding accumulated through engagements. Each fact has properties defining where it applies (which NPCs, which locations, which situations). Knowledge facts unlock new goals, enable conversation topics, and become knowledge cards in signature decks.

**Player Stats:** The five stats (Insight, Authority, Rapport, Diplomacy, Cunning) gate card access and grow through play. Stats are the primary character progression mechanism.

**Signature Decks:** Each NPC and location has signature deck containing knowledge cards earned about them. These decks grow through successful engagements and provide tactical advantages in future engagements.

### Transient Resources (Temporary)

These resources exist only during tactical challenges and disappear when returning to strategic layer:

**Social uses:** Initiative, Momentum, Doubt, Cadence, Statements. These model conversation dynamics - your capacity to guide discussion, progress toward agreement, NPC patience wearing thin, speaking/listening balance, and conversational context.

**Mental uses:** Attention, Progress, Exposure, Observe/Act Balance, Clue Types. These model investigation dynamics - your mental focus, understanding advancement, location disturbance, investigation rhythm, and specific knowledge types.

**Physical uses:** Position, Breakthrough, Danger, Commitment, Approach History. These model physical challenge dynamics - your tactical advantage, advancement toward victory, accumulating injury risk, action rhythm, and learned patterns.

The key distinction: strategic resources are what you bring in and take out. Transient resources are the gameplay happening inside. You enter Social engagement with high Stamina, play through conversation building Initiative and Momentum, and exit having consumed Time and gained Knowledge. The Initiative and Momentum existed only during conversation.

---

## Three-Part Persistence Pattern

Each tactical system affects strategic state through identical structural pattern:

### Consumables (Restored)

Resources spent during tactical engagements that regenerate in strategic layer. Stamina is the universal consumable - all challenges exhaust you. Stamina depletes through card ExertionLevel costs, regenerates through rest. Low stamina restricts high-exertion options across all systems.

Time is also consumable in the sense that segments spent don't return, but time advances naturally rather than regenerating.

### Debt (Failure Accumulation)

Negative persistent effects from failed challenges that make future attempts harder:

**Social Debt:** Burden cards enter NPC signature decks (as Social type cards). These cards have negative effects when appearing in future Social engagements - they increase Doubt, reduce Momentum gain, or cost extra Initiative. Burden accumulates through relationship damage. High burden makes consensus nearly impossible without relationship repair.

**Mental Debt:** Location exposure levels increase permanently. High exposure means future Mental engagements at that location start with elevated Exposure baseline - less room for mistakes before consequences. Exposure accumulates through being noticed, making noise, or leaving evidence. Location remembers your intrusive investigation methods.

**Physical Debt:** Injury cards enter player's Physical deck (not signature deck, but persistent tactical deck used across all Physical engagements). These cards appear when drawing in future Physical challenges, creating debuffs and restricting options. Injuries accumulate through taking Danger threshold damage. Multiple injuries make future physical activity progressively more dangerous until healed.

Debt creates downward spirals if not managed. Failed Social creates burden, making next Social with that NPC harder, leading to more burden. Failed Mental increases exposure, making next Mental attempt more dangerous, leading to more exposure. Failed Physical causes injury, making next Physical more likely to fail, leading to more injury.

Strategic choices to repair debt become critical: work on relationship repair with NPC (special engagement types), investigate cautiously to let exposure decay over time, rest and heal to remove injury cards.

### Progress (Success Accumulation)

Positive persistent effects from successful challenges that make future attempts easier:

**Social Progress:** Relationship tokens accumulate with NPCs. Higher tokens unlock better engagement types - NPCs trust you enough for deeper conversations. Tokens also affect starting conditions in Social engagements (might start with lower Doubt, or bonus Initiative).

**Mental Progress:** Knowledge facts accumulate in player's collection and as cards in location signature decks. Knowledge unlocks goals across all systems - conversation topics require knowledge, investigation actions require knowledge, travel approaches require knowledge. Location knowledge cards provide Mental and Physical advantages when engaging with that location. Knowledge represents mastery.

**Physical Progress:** Reputation increases through victory. Higher reputation affects both Social engagements (people respect or fear you based on physical prowess) and Physical engagements (opponents might be more cautious, or aggressive rivals might seek you out). Reputation is social capital from physical capability.

Progress creates upward spirals when managed well. Successful Social improves relationships, unlocking better engagement types, making success easier. Successful Mental grants knowledge, enabling better preparation, improving success chance. Successful Physical builds reputation, affecting future encounters favorably.

---

## Equipment System

### Equipment as Deck Filters

Equipment does not provide universal modifiers during tactical play. Equipment determines which cards can exist in engagement decks at deck composition time.

**Equipment Categories:**
Each equipment item provides one or more categories (enum values):
- Rope: provides [Climbing, Securing]
- Crowbar: provides [Force, Mechanical]  
- Scholar's Kit: provides [Documentation]
- Lantern: provides [Illumination]
- Lockpicks: provides [Mechanical, Precision]
- Medicine: provides [Medical]

**Card Requirements:**
Cards declare required equipment category in their Requirements property. Most cards have EquipmentCategory=None (no requirement). Specialized cards require specific categories:
- "Climb to Upper Window" requires Climbing
- "Pick the Lock" requires Mechanical
- "Read Ancient Texts" requires Documentation
- "Force the Mechanism" requires Force

**Deck Composition:**
When engagement type loads its deck, the system filters cards:
- If player has equipment providing card's required category: include card in deck
- If player lacks required equipment: exclude card from deck entirely

Cards requiring equipment only appear in deck when you have the tools. This creates visible progression: acquire rope, suddenly climbing cards appear in relevant engagement decks. The deck expansion shows your growing capabilities.

**No Runtime Modifiers:**
Equipment doesn't modify card costs, dangers, or effects during play. A card requiring Mechanical tools works identically whether you have basic lockpicks or master precision tools - both provide Mechanical category, both enable the card. Equipment is binary: either enables cards or doesn't.

This simplicity maintains elegance: equipment expands tactical options by including more cards, not by adding modifier calculations during play.

### Equipment Degradation

Physical engagements can damage equipment. When Danger threshold triggers in Physical challenge, might cause equipment degradation instead of or in addition to injury. Degraded equipment stops providing its categories until repaired, immediately removing cards requiring those categories from future engagement decks.

This creates maintenance pressure: aggressive Physical play risks losing equipment capabilities, reducing tactical options in all systems.

---

## Strategic Layer Activities

The strategic layer contains three categories of activities that interact with tactical challenges in different ways.

### Direct Strategic Actions (No Tactical Spawn)

These activities manipulate strategic resources directly without entering tactical layer:

**Resource Management:**
Resting to recover health and stamina consumes time and restores resources without tactical gameplay. You make strategic choice to rest, time advances, resources restore. Shopping for equipment is direct transaction: coins for items.

**Simple Work:**
Routine labor to earn coins can be strategic abstraction. Choose to work, time and stamina consumed, coins earned based on time investment. The interesting decision is strategic (when to work versus pursue other activities), not tactical execution details.

**Casual Observation:**
Looking around location spot without deep engagement grants surface information strategically. You see who's present, general atmosphere, obvious features. This informs strategic decisions about which goals to pursue.

**Time Advancement:**
Waiting or advancing to different day segments. Waiting for NPC arrival, waiting for shops to open, advancing to evening for different social opportunities. Strategic time management.

**Obligation Management:**
Accepting delivery requests adds them to obligation journal strategically. The interesting gameplay happens when fulfilling obligations through travel and investigation, not in acceptance itself.

### Tactical Engagement Triggers

These activities spawn one of the three tactical systems:

**Goals from NPCs:**
Clicking NPC goal spawns Social tactical with the goal's referenced engagement type and deck. Might be negotiation, persuasion, information gathering, or relationship building.

**Goals from Locations:**
Clicking location goal spawns Mental or Physical tactical with referenced engagement type and deck. Mental for puzzles and investigation, Physical for obstacles and challenges.

**Travel Obstacles:**
Attempting route travel encounters obstacles. Each obstacle presents a goal that spawns appropriate tactical engagement - Physical for terrain, Mental for navigation, Social for checkpoints.

All tactical spawns follow same pattern: goal → engagement type → tactical system with specific deck.

### Activities as Goal Orchestrators

**Investigation Activity:**
Multi-phase framework presenting goals at each phase. Phase 1 might offer "Survey exterior" goal. Phase 2 might offer conditional goals based on Phase 1 discoveries - "Force entry" if found weak point, "Climb window" if found access route. Each goal spawns appropriate tactical engagement. Investigation tracks phase completion, discoveries, and exposure accumulation across phases.

**Travel Activity:**
Route framework presenting obstacle goals in sequence. "Cross creek" goal spawns Physical engagement. "Navigate dense forest" goal spawns Mental engagement. "Pass checkpoint" goal spawns Social engagement. Travel tracks progress along route, time consumed, and obstacle completion. Successful navigation might improve route permanently.

Activities orchestrate goal presentation and track macro-state, but delegate resolution to tactical systems through engagement types.

---

## Cross-System Integration

### Knowledge Enables Goals

Knowledge facts unlock new goals at NPCs and locations:

**Social Goal Unlocking:**
Learning "Martha had daughter" knowledge fact unlocks Martha's "Ask about daughter" goal, which references Sensitive Topic engagement type. The knowledge doesn't directly affect tactical play - it unlocks access to different engagement with different deck and dynamics.

Learning "Elder's corruption evidence" unlocks Elder's "Confront about ledger" goal, referencing Accusation engagement type. Different deck, different tactical challenge.

**Mental Goal Unlocking:**
Learning "Mill has hidden passage" knowledge fact unlocks Mill's "Investigate passage" goal, referencing Secret Area Investigation engagement type. Different engagement, different deck from main investigation.

**Physical Goal Unlocking:**
Learning "Guard patrol schedule" might unlock "Infiltrate during gap" goal, referencing Stealth Approach engagement type instead of direct confrontation engagement.

Knowledge shapes which tactical engagements become available, creating strategic branching based on what you've learned.

### Knowledge Cards in Hand

When tactical engagement spawns, matching knowledge from signature decks goes to starting hand:

**Example - Social with Marcus:**
- Goal: "Negotiate price" (Mercantile Exchange engagement)
- Loads Mercantile Exchange deck
- Checks Marcus's signature deck for Social type cards
- Player has "Marcus's hidden debt" card from prior conversation
- Card added to starting hand
- Tactical play: can reference debt for leverage, reducing Initiative costs

**Example - Mental at Mill:**
- Goal: "Investigate interior" (Interior Investigation engagement)
- Loads Interior Investigation deck
- Checks Mill's signature deck for Mental type cards
- Player has "Structural weakness location" card from exterior survey
- Card added to starting hand
- Tactical play: card reveals safe approach, reducing Exposure

**Example - Physical at Mill:**
- Goal: "Climb to loft" (Climbing Challenge engagement)
- Loads Climbing Challenge deck
- Checks Mill's signature deck for Physical type cards
- Player has "Safe handholds" card from prior investigation
- Card added to starting hand
- Tactical play: card reduces Danger when climbing

Knowledge application is automatic based on tactical type matching. The system doesn't need complex targeting - if spawning Social with Marcus, check Marcus's signature deck for Social cards. Simple and deterministic.

### Cross-Type Knowledge

A single NPC or location signature deck can contain knowledge cards of multiple tactical types:

**Mill's Signature Deck might contain:**
- "Wheel mechanism broken" (Mental type) - helps investigation
- "Safe traversal route" (Physical type) - helps climbing/navigation
- "Miller's ghost story" (Social type) - helps conversations about the Mill

When you investigate the Mill (Mental engagement), you get Mental type cards from Mill's signature deck in hand. When you physically navigate the Mill (Physical engagement), you get Physical type cards in hand. When you discuss the Mill with NPCs (Social engagement about the Mill as topic), relevant cards might apply based on engagement design.

This creates multifaceted understanding: thoroughly investigating a location provides advantages across multiple engagement types at that location.

### Reputation Cross-System Effects

Physical success builds reputation (strategic resource). Reputation affects Social engagements:

**Reputation Levels:**
- Unknown (0): No reputation effects
- Capable (10): NPCs with "Respects Competence" disposition start Social engagements with -1 Doubt
- Formidable (25): NPCs with "Fears Strength" disposition start with -2 Doubt, "Challenges Strong" disposition start with +2 Doubt
- Legendary (50): Major effects on Social starting conditions across NPC types

Reputation also affects Physical engagements:
- Higher reputation: some opponents more cautious (start with lower Danger threshold)
- Higher reputation: aggressive rivals seek you out (trigger special Physical engagement goals)

Reputation creates circular integration: Physical success improves Social options, Social success might lead to Physical opportunities.

---

## AI Text Generation Integration

### The Visual Novel Presentation

Player experiences game as visual novel: situation descriptions → choice selections → outcome descriptions. The UI presents narrative text with choices, hiding or minimally showing mechanical details.

### Mechanical State Drives Generation

AI receives game state and generates narrative for three contexts:

**Situation Descriptions:** Scene-setting before player draws or plays cards. AI receives: current location, environmental conditions, NPC emotional state (from transient resources like Initiative/Doubt), recent action history. Generates description establishing tactical challenge context.

**Card Narrative Text:** Flavor text making choices feel narrative. AI receives: card properties (Type, Depth, BoundStat, Category, etc.), current tactical state, engagement context. Instead of showing mechanical values, displays narrative description. "Shoulder the door hard, damn the noise" instead of "Force Door: Standard, Depth 2, +3 Progress, +2 Exposure."

**Outcome Descriptions:** Results of card play explained narratively. AI receives: mechanical outcome (resources changed, progress made, dangers triggered), current state after resolution. Generates appropriate narrative: "The door splinters with loud crack. You're through, but the sound echoes - anyone nearby would have heard that."

### Mechanical Constraint

AI generates all narrative text but cannot modify mechanical effects. Card effects are deterministically defined by parser rules from card properties. AI decorates mechanics with narrative but doesn't invent consequences.

The separation ensures mechanical consistency while allowing narrative flexibility. Parser determines "playing this Standard/Depth 2/Risky card generates +3 Progress, +2 Exposure, costs 2 Attention, 15% danger of injury." AI writes: "You force the mechanism open, metal screeching against metal. It gives way with effort, but the noise carries through empty halls. Your hands ache from the strain."

Same mechanical outcome, different narrative phrasing based on context and AI variation. But the mechanical impact is fixed by card properties and parser.

### Engagement Deck Context

AI knows which engagement deck is active, providing crucial generation context. Drawing from "Desperate Request" deck means generating desperate dialogue. Drawing from "Mill Interior Investigation" deck means generating Mill-specific investigation descriptions.

The engagement type provides domain context. "Recall His Kindness" card appears in Desperate Request deck (with NPCs who have shown kindness). AI generates text referencing shared history: "Remember when Marcus helped with the broken cart? Bring that up - remind him you haven't forgotten."

Deck identity ensures narrative coherence. Cards only appear in contextually appropriate decks, so AI can reference deck-specific details without ambiguity.

### Knowledge Cards Get Special Treatment

AI emphasizes that knowledge cards represent understanding you earned. "You remember the structural weakness you identified in west wall - that would be easiest entry point."

Differentiates knowledge cards narratively while treating them mechanically identical to base cards. Players understand these advantages derive from their investigation/conversation history.

---

## Content Authoring Workflow

### Creating Cards

Content creators define cards using strongly-typed properties. Each card is reusable component appearing in multiple engagement decks.

**Card Definition:**
1. Type: Foundation/Standard/Decisive
2. Depth: 1-10
3. BoundStat: Insight/Authority/Rapport/Diplomacy/Cunning
4. Category: System-specific enum
5. RiskLevel: Safe/Cautious/Risky/Dangerous
6. Visibility: Subtle/Moderate/Obvious/Loud
7. ExertionLevel: Minimal/Light/Moderate/Heavy/Extreme
8. MethodType: Direct/Analytical/Intuitive/Negotiated/Deceptive
9. Requirements:
   - EquipmentCategory: None or specific category
   - StatThresholds: Additional stat requirements
   - MinimumHealth/Stamina: Resource gates
10. Name: Short title
11. Description Template: Narrative text with context placeholders

Parser generates all costs, effects, dangers from categorical properties. Content creator just selects enums and writes narrative text.

Cards stored in library, included in multiple engagement decks by reference. "Careful Observation" Foundation card appears in dozens of Mental engagement decks. Update card definition once, all decks reflect change.

### Creating Engagement Types

Engagement types are the bridge between goals and tactical resolution:

**Engagement Type Definition:**
1. TacticalSystem: Social/Mental/Physical
2. CardDeck: List of card references (pulls from card library)
3. VictoryThreshold: Progress resource amount needed (Momentum/Progress/Breakthrough)
4. StartingConditions: Initial transient resource values
5. Name and Description: For UI display

**Deck Composition Guidelines:**
- Include 40-60 cards total
- 30-40% Foundation (resource generation)
- 40-50% Standard (main tactics)
- 10-20% Decisive (powerful finishers)
- Mix of BoundStats (don't over-concentrate single stat)
- Include cards with varied Categories for tactical diversity
- Roughly 5% of cards require equipment (specialized options)

**Examples:**
- "Desperate Request" engagement: Social system, 45 cards emphasizing Empathetic/Assertive categories, Momentum threshold 12
- "Mill Interior Investigation" engagement: Mental system, 50 cards emphasizing Physical/Analytical categories, Progress threshold 10
- "Creek Crossing" engagement: Physical system, 40 cards emphasizing Tactical/Endurance categories, Breakthrough threshold 8

### Creating NPCs and Locations

**NPC Definition:**
1. Goals: List of available goals player can click
2. Each goal references an engagement type
3. Goals can have prerequisites (knowledge facts, relationship tokens, time of day)
4. Signature deck: Initially empty, grows through play

**Location Definition:**
1. Location spots within location (different contexts/areas)
2. Each spot has goals: List of available investigation/interaction options
3. Each goal references an engagement type
4. Signature deck: Initially empty, grows through discoveries

**Example - Elena NPC:**
```
Goals:
  "Request urgent delivery" → Desperate Request engagement (always available)
  "Chat about weather" → Casual Chat engagement (always available)
  "Discuss legal matters" → Professional Conversation engagement (requires relationship tokens 5+)
  "Ask about personal struggles" → Sensitive Topic engagement (requires "Elena's troubles" knowledge fact)
SignatureDeck: []
```

**Example - The Mill Location:**
```
Spot: Mill Exterior
  Goals:
    "Survey structure" → Exterior Assessment engagement (always available)
    "Listen at door" → Eavesdropping engagement (requires evening time)
    
Spot: Mill Interior
  Goals:
    "Investigate interior" → Interior Investigation engagement (requires entry method)
    "Search for evidence" → Evidence Gathering engagement (requires "mill mystery" knowledge)
    "Force mechanism" → Mechanical Challenge engagement (requires tools)
    
SignatureDeck: []
```

### Knowledge Card Generation

Knowledge cards are created dynamically when earned, not pre-authored:

**Generation Triggers:**
- Successful Social engagement: might create knowledge about NPC
- Successful Mental engagement: might create knowledge about location
- Successful Physical engagement: might create knowledge about tactics/patterns

**Generation Process:**
1. Determine target signature deck (which NPC or location)
2. Determine tactical type (Social/Mental/Physical based on what was learned)
3. Determine persistence (persistent understanding vs one-time opportunity)
4. Generate card properties (tends toward better values than base cards)
5. AI generates narrative description based on discovery context
6. Add card to signature deck

**Example:**
Player completes difficult Social engagement with Marcus, learning about his hidden debt. System generates:
- Target: Marcus's signature deck
- TacticalType: Social
- Persistence: Persistent
- Properties: Type=Standard, Depth=2, BoundStat=Rapport, Category=Empathetic, lower Initiative cost than base
- Name: "Acknowledge His Burden"
- AI generates description referencing the debt discovery
- Card added to Marcus's signature deck

Next Social engagement with Marcus: this card appears in starting hand, providing tactical advantage.

### Creating Activities

**Investigation Activity:**
1. Location: Which location this investigation occurs at
2. Phases: 2-5 sequential phases
3. For each phase:
   - Goals available this phase
   - Completion conditions (specific Progress thresholds or discoveries)
   - Conditional goals based on prior phase results
4. Rewards: Knowledge facts gained, items found, relationship tokens earned
5. Consequences: What happens if Exposure too high

**Example - Mill Investigation:**
```
Phase 1: Exterior
  Goals: "Survey structure" → Exterior Assessment (Mental)
  Completion: Progress 8 reached
  Yields: "Structural weakness" knowledge, unlocks Phase 2 goals
  
Phase 2: Entry (conditional based on Phase 1)
  If found weakness:
    Goal: "Force weak wall" → Breaking Through (Physical)
  If have climbing gear:
    Goal: "Climb to window" → Climbing Access (Physical)
  Completion: Successfully enter interior
  
Phase 3: Interior Investigation
  Goal: "Search interior" → Interior Investigation (Mental)
  Completion: Progress 12 reached
  Yields: "Elder's ledger" knowledge, completes investigation
  
Exposure Trigger (if Exposure reaches 10 during any phase):
  Spawns "Deal with witnesses" → Confrontation (Social)
```

**Travel Route:**
1. Start and end location spots
2. Base time cost
3. Obstacles in sequence:
   - Each obstacle is a goal
   - Each goal references engagement type
   - Physical for terrain, Mental for navigation, Social for checkpoints
4. Route improvement conditions (what makes future travel easier)

**Example - Mill Path Route:**
```
Start: Town Square → End: Farmstead
BaseTime: 6 segments

Obstacle 1: "Navigate dense forest" → Forest Navigation (Mental, Progress 6)
Obstacle 2: "Cross creek" → Creek Crossing (Physical, Breakthrough 8)  
Obstacle 3: "Pass through mill area" → Stealth Passage (Mental, Progress 8)

Improvements:
  If "Forest path" knowledge gained: reduce Obstacle 1 to Progress 4
  If rope used at creek: permanent rope remains, reduce Obstacle 2 to Breakthrough 4
```

---

## Implementation Priorities

### Phase 1: Core Tactical Systems

1. Implement card property system with parser
2. Build Social tactical system with transient resources
3. Build Mental tactical system with transient resources
4. Build Physical tactical system with transient resources
5. Create basic engagement types for testing
6. Test tactical flow: goal → engagement type → deck loading → tactical play → outcomes

### Phase 2: Persistence and Knowledge

1. Implement signature deck system
2. Build knowledge card generation
3. Implement debt systems (burden/exposure/injury)
4. Test knowledge flow: earn knowledge → signature deck → next engagement advantage
5. Test debt accumulation and consequences

### Phase 3: Strategic Integration

1. Build Investigation activity framework
2. Build Travel activity framework
3. Implement goal unlocking based on knowledge
4. Test activity orchestration: phases → goals → engagements → outcomes
5. Test cross-system integration: Social → Mental → Physical cycles

### Phase 4: AI and Presentation

1. Integrate AI text generation for situation descriptions
2. Generate card narrative text from mechanical properties
3. Generate outcome descriptions from mechanical results
4. Polish UI for visual novel presentation
5. Test narrative coherence with mechanical determinism

---

## Design Principles Achieved

This architecture achieves the core integration goals:

**Mechanics Express Narrative:** Card types represent conversational dynamics (Foundation = building rapport), investigative methods (Observe = examining carefully), and physical tactics (Execute = committing to action). Players think narratively, act mechanically.

**Systems Create Impossible Choices:** Builder-spender dynamics mean you can't do everything. Spend resources now or save for better opportunity? Risk high exposure for faster progress or proceed cautiously? Every decision has meaningful trade-offs.

**Verisimilitude Through Structure:** The three systems behave differently because their domains behave differently. Social engagements end when leaving without goal achievement. Mental engagements allow leaving and returning with state saved. Physical engagements make escape costly. Structure matches reality.

**Unified Progression:** Stats as universal approaches mean all challenges contribute to all capabilities. You don't build fragmented skill trees - you build coherent tactical identity expressed across all challenge types.

**Circular Integration:** Systems feed each other through strategic resources. Social success provides knowledge for Mental challenges. Mental success creates knowledge cards for Social advantages. Physical success builds reputation affecting both. No system exists in isolation.

**Preparation Matters:** Strategic resource management determines tactical options. Good preparation (right equipment, relevant knowledge) opens superior approaches. Poor preparation restricts to difficult methods. Success requires thinking beyond immediate challenge.

**Failure Teaches:** The debt systems accumulate handicaps from failure, but state persistence (especially in Mental) and multiple engagement approaches mean failed attempts aren't wasted. You learn what you need, retreat, prepare, and return stronger.

**Elegant State Management:** Goals → Engagement Types → Decks. Signature decks accumulate knowledge. Equipment filters deck composition. Clear separation between what persists (strategic) and what's transient (tactical). No runtime complexity.

The architecture solves the core problem: making simulator depth and visual novel narrative work together naturally through shared mechanical grammar expressing different narrative contexts.

---

## Conclusion

This strategic-tactical layer architecture provides the foundation for integrating complex simulation mechanics with visual novel narrative presentation. The two-layer structure separates world navigation (strategic) from challenge resolution (tactical). 

Goals spawn engagement types which own specific card decks. The three tactical systems (Social, Mental, Physical) handle different challenge types while sharing unified card-based patterns. Activities (Investigation, Travel) orchestrate tactical challenges through goal presentation rather than being systems themselves. 

Stats as universal approaches create coherent character identity across challenge types. Equipment filters deck composition at load time. Knowledge accumulates in signature decks providing contextual advantages. The three-part persistence pattern (consumables, debt, progress) creates meaningful consequences and progression. 

Circular interconnection between systems creates preparation loops and strategic depth. AI generates narrative presentation while mechanical parsers ensure deterministic gameplay. The result is a game where mechanics function as the grammar of narrative choice - players think in story terms while the underlying simulation tracks complex state, resources, and consequences.

Neither mechanics nor narrative overshadow each other because they're the same thing expressed at different layers of abstraction.