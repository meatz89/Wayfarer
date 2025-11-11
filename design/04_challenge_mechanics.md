# Game Design Section 4: Challenge Mechanics (Tactical Systems)

## 4.1 Overview

Challenge mechanics form the **tactical layer** of Wayfarer's two-layer architecture. While the strategic layer handles narrative progression through scenes and situations with perfect information, the tactical layer introduces card-based gameplay with hidden complexity and victory thresholds.

**Three Parallel Challenge Systems:**
- **Mental Challenges**: Location-based investigations using observation and deduction
- **Physical Challenges**: Location-based obstacles using strength and precision
- **Social Challenges**: NPC-based conversations using rapport and persuasion

All three systems share a unified structural pattern while maintaining distinct session models that reflect their fictional reality. A single five-stat progression system governs all tactical gameplay, ensuring every card played contributes to unified character development.

---

## 4.2 Unified Five-Stat System

Wayfarer uses a **single unified progression system** across all three challenge types. Five stats govern all tactical gameplay:

- **Insight**: Pattern recognition, analysis, understanding
- **Rapport**: Empathy, connection, emotional intelligence
- **Authority**: Command, decisiveness, power
- **Diplomacy**: Balance, patience, measured approach
- **Cunning**: Subtlety, strategy, risk management

### 4.2.1 Card Binding and Depth Access

Every card in every challenge system is bound to one of these five stats. This binding determines:

**Depth Access**: Player's stat level determines which card depths they can access. Higher stats unlock more powerful card tiers, creating tangible progression feedback.

**XP Gain**: Playing any card grants experience to its bound stat. This creates a single unified progression loop - playing Mental challenge cards, Physical challenge cards, or Social challenge cards all feed the same stat advancement system.

### 4.2.2 Cross-System Stat Manifestation

The same stat manifests differently depending on challenge type, creating thematic coherence across systems:

**Insight Manifestations:**
- **Mental**: Pattern recognition in evidence, analyzing crime scenes, deductive reasoning
- **Physical**: Structural analysis, reading terrain features, finding weaknesses in obstacles
- **Social**: Understanding NPC motivations, reading between lines, seeing hidden agendas

**Rapport Manifestations:**
- **Mental**: Empathetic observation, understanding human element in investigations
- **Physical**: Flow state, body awareness, natural movement, athletic grace
- **Social**: Building emotional connection, creating trust, resonating with feelings

**Authority Manifestations:**
- **Mental**: Commanding the scene, decisive analysis, authoritative conclusions
- **Physical**: Decisive action, power moves, commanding environment through presence
- **Social**: Asserting position, directing conversation, establishing dominance

**Diplomacy Manifestations:**
- **Mental**: Balanced investigation approach, patient observation, measured conclusions
- **Physical**: Measured technique, controlled force, pacing endurance wisely
- **Social**: Finding middle ground, compromise, maintaining balanced conversation

**Cunning Manifestations:**
- **Mental**: Subtle investigation, covering tracks, tactical information gathering
- **Physical**: Risk management, tactical precision, adaptive technique
- **Social**: Strategic conversation flow, subtle manipulation, reading and responding

### 4.2.3 Why Unified Stats Matter

**Single Progression Path**: Playing any challenge type improves capabilities across all systems. A player who specializes in Social challenges still gains stats useful in Physical and Mental challenges when those stats appear in social cards.

**Thematic Coherence**: Stats represent fundamental character traits that manifest contextually. Insight isn't "just" for investigations - it's pattern recognition that applies to physical obstacles and social interactions differently.

**Build Variety**: Players can specialize (few stats to high levels) or generalize (all stats to moderate levels), creating distinct playstyles. The same five stats create different tactical capabilities based on distribution.

**No Wasted Effort**: Every card played contributes to character progression. There are no "challenge-specific currencies" that become worthless when switching between Mental, Physical, and Social challenges.

---

## 4.3 Strategic-Tactical Bridge

The strategic and tactical layers remain strictly separated, with an explicit bridge pattern routing between them.

### 4.3.1 Strategic Layer Flow

**Scene → Situation → Choice**

The strategic layer handles narrative progression and player decision-making with perfect information:

- **Scene**: Persistent narrative container in GameWorld, contains situations, tracks overall progression
- **Situation**: Narrative moment presenting 2-4 choices to player
- **Choice**: Player option with visible costs, requirements, and rewards

Players see all information before committing. Costs are visible (Stamina -2), requirements are visible (Rapport ≥ 5), rewards are visible (OnSuccess: Room unlocked, OnFailure: Pay extra 5 coins).

### 4.3.2 The Bridge: ActionType Property

Every choice has an ActionType property that determines execution flow:

**ActionType.Instant**: Stay in strategic layer, apply rewards immediately, advance situation
**ActionType.Navigate**: Stay in strategic layer, move player to new location or NPC
**ActionType.StartChallenge**: Cross to tactical layer, spawn challenge session

This property provides explicit, testable routing. There is no ambiguity about whether a choice stays strategic or crosses to tactical.

### 4.3.3 Tactical Layer Flow

**Challenge Session → Card Play → Resource Accumulation → Threshold Victory**

When ActionType.StartChallenge triggers, the system:

1. Extracts SituationCards from parent Situation (victory conditions with thresholds)
2. Spawns temporary challenge session (MentalSession, PhysicalSession, or SocialSession)
3. Player enters card-based gameplay with hidden complexity
4. Player plays cards, accumulates builder resource toward threshold
5. Threshold reached: Victory or failure outcome determined
6. Session destroyed (temporary entity)
7. Control returns to strategic layer with outcome

### 4.3.4 Bridge Design Guarantees

**Clear Separation**: Strategic layer shows WHAT to attempt, tactical layer handles HOW to execute
**Perfect Information at Entry**: Player knows costs and possible outcomes before crossing bridge
**Explicit Routing**: ActionType property makes transition unambiguous
**One-Way Flow**: Strategic spawns tactical, tactical returns outcome, no circular dependencies
**Layer Purity**: Situations are strategic entities, challenges are tactical entities, never conflate

---

## 4.4 Common Challenge Structure

All three challenge types share the same structural pattern:

### 4.4.1 Five Core Resources

**Builder Resource** (Persists or Session-Bounded):
- Primary progress toward victory
- **Mental**: Progress (persists across sessions)
- **Physical**: Breakthrough (resets each attempt)
- **Social**: Momentum (resets each conversation)

**Session Resource** (Always Resets):
- Tactical capacity for playing cards
- **Mental**: Attention (derived from permanent Focus stat)
- **Physical**: Exertion (derived from permanent Stamina stat)
- **Social**: Initiative (accumulated via Foundation cards)

**Threshold Resource** (Danger Toward Failure):
- Accumulating risk or frustration
- **Mental**: Exposure (investigative footprint, persists)
- **Physical**: Danger (accumulating harm, resets)
- **Social**: Doubt (NPC frustration, resets)

**Flow Mechanic** (System-Specific):
- Controls card availability and pacing
- **Mental**: Leads (investigative threads generated by ACT)
- **Physical**: Aggression (overcautious to reckless spectrum)
- **Social**: Cadence (conversation rhythm and pacing)

**Understanding** (Global Persistent):
- Shared tier-unlocking resource across all systems
- Represents growing mastery of tactical gameplay
- Unlocks higher card tiers in all three challenge types

### 4.4.2 Action Pair Pattern

Each challenge type has two primary actions creating tactical rhythm:

**Mental Challenges**: ACT / OBSERVE
- ACT: Take investigative action, spend Attention, generate Leads, build Progress
- OBSERVE: Follow investigative threads, draw Details equal to Leads count

**Physical Challenges**: EXECUTE / ASSESS
- EXECUTE: Lock Option as preparation, spend Exertion, build prepared sequence
- ASSESS: Evaluate situation, trigger all locked Options as combo

**Social Challenges**: SPEAK / LISTEN
- SPEAK: Advance conversation through Statements, build Momentum
- LISTEN: Reset and draw, take in new information, manage conversation flow

The action pair creates a natural rhythm: Primary action advances state, secondary action resets or triggers accumulated effects.

### 4.4.3 Victory and Failure Conditions

**Victory Threshold**: Builder resource reaches target value
- Mental: Progress ≥ threshold (may span multiple sessions)
- Physical: Breakthrough ≥ threshold (must complete in single attempt)
- Social: Momentum ≥ threshold (must complete in single conversation)

**Failure Conditions**: System-specific consequences of threshold resource
- Mental: High Exposure increases difficulty but doesn't force failure (can always continue)
- Physical: Danger ≥ MaxDanger causes immediate injury/failure (one-shot test)
- Social: Doubt ≥ MaxDoubt ends conversation (NPC forces exit)

**Both Outcomes Advance Progression**: Success and failure both apply rewards and advance the story. There are no dead-end failure states. This ensures forward progress (TIER 1 quality goal) while maintaining meaningful consequences.

---

## 4.5 Mental Challenges: Investigation at Locations

Mental challenges represent pauseable investigations at locations. They reflect the reality that **investigations take time** - you can leave, rest, return, and continue where you left off.

### 4.5.1 Session Model: Pauseable Static Puzzle

**Can Pause Anytime**: Player can leave location mid-investigation. All state persists exactly where they left off.

**Progress Persists**: Accumulates at location across multiple visits. Player may make small Progress in first visit, return later to complete.

**Exposure Persists**: Investigative "footprint" increases with each action. Higher Exposure makes investigation harder but doesn't force failure.

**Attention Resets**: Player returns with fresh mental energy after rest. Permanent Focus stat determines how much Attention available per session.

**No Forced Ending**: High Exposure makes cards more expensive or dangerous, but player can always continue if willing to accept costs.

**Incremental Victory**: Reach Progress threshold across multiple sessions. Unlike Physical challenges, Mental challenges respect fictional time pressure (investigations take days, not minutes).

### 4.5.2 Core Session Resources

**Progress** (Builder, Persists):
- Accumulates toward completion across sessions
- Represents concrete advancement in solving investigation
- Victory threshold: Reach target Progress value (scaled by investigation difficulty)

**Attention** (Session Budget, Resets):
- Mental capacity for ACT cards
- Derived from permanent Focus stat
- Resets to full when returning to investigation after rest
- Limits how many investigative actions possible in single visit

**Exposure** (Threshold, Persists):
- Starts at 0, accumulates as investigative footprint
- Makes investigation harder (card costs increase, risks escalate)
- Does NOT force failure - player can always continue
- Represents suspicion, trail cooling, evidence degradation

**Leads** (Observation Flow, Persists):
- Investigative threads generated by ACT cards
- Determines how many Detail cards drawn by OBSERVE
- Persists between sessions (threads don't evaporate)
- Represents concrete investigation avenues discovered

**Understanding** (Global, Persistent):
- Shared tier-unlocking resource across all challenge types
- Higher Understanding unlocks advanced Mental cards
- Gained through tactical mastery, not story progression

### 4.5.3 Action Pair: ACT / OBSERVE

**ACT: Take Investigative Action**
- Spend Attention (session resource)
- Play ACT card with specific investigation approach
- Generate Leads (observation threads)
- Build Progress toward solution
- Increase Exposure (investigation leaves traces)

**Example ACT Card:**
```
"Interview Witnesses" (ACT, Insight-bound)
Cost: 3 Attention
Effect: +2 Progress, +2 Leads, +1 Exposure
```

**OBSERVE: Follow Investigative Threads**
- No cost (recovery action)
- Draw Detail cards equal to current Leads count
- Details provide additional Progress, clues, or tactical advantages
- Cannot observe what you haven't investigated (requires Leads first)

**Key Mechanic**: You cannot observe what you haven't investigated. ACT generates Leads, OBSERVE follows them. This creates natural investigation rhythm - take action, observe results, take action, observe results.

**Tactical Depth**: Player must balance ACT (spending Attention to make progress) with OBSERVE (capitalizing on generated Leads). High Leads count makes OBSERVE powerful, but requires having spent Attention on ACT first.

### 4.5.4 Why Pauseable Session Model

**Fictional Reality**: Investigations take time. You don't solve a murder in one sitting. You investigate, rest, return with fresh perspective, continue.

**Resource Management**: Attention is finite per visit. Player must decide: Push forward now (exhaust Attention) or return later (restore Attention)?

**Strategic Planning**: Player can assess Progress, calculate remaining threshold, plan future visits. Perfect information enables optimization.

**No Punishment for Rest**: Pausing doesn't lose Progress. Only Exposure accumulates, making future actions harder but not impossible.

**Matches Player Mental Model**: "I'll investigate a bit, then go rest and come back tomorrow" feels natural for detective work.

---

## 4.6 Physical Challenges: Obstacles at Locations

Physical challenges represent immediate tests of current capability. They reflect the reality that **physical obstacles are one-shot tests** - you can't pause halfway up a cliff and come back tomorrow.

### 4.6.1 Session Model: One-Shot Test

**Single Attempt**: Must complete or fail in one session. No pausing mid-challenge.

**No Challenge Persistence**: Each attempt starts fresh. Previous attempts don't carry over state.

**Personal State Carries**: Health and Stamina persist between challenges. Injury from one Physical challenge affects next attempt.

**Danger Threshold = Consequences**: Reaching MaxDanger causes injury/failure immediately. Threshold resource has teeth.

**Must Complete Now**: Cannot pause halfway and return later. Commit to attempt or don't start.

### 4.6.2 Core Session Resources

**Breakthrough** (Builder):
- Progress in single session toward victory
- Does NOT persist - each attempt starts at 0
- Victory threshold: Reach target Breakthrough value in one session
- Represents overcoming physical obstacle through accumulated effort

**Exertion** (Session Budget):
- Physical capacity for EXECUTE cards
- Derived from permanent Stamina stat
- Limited per attempt (no restoration mid-challenge)
- Forces rationing: Which actions are worth Exertion cost?

**Danger** (Threshold):
- Starts at 0, accumulates from risky actions
- Reaching MaxDanger causes immediate injury/failure
- Creates genuine risk: Push too hard, consequences follow
- Represents physical harm, exhaustion, dangerous positioning

**Aggression** (Balance):
- Overcautious to Reckless spectrum
- Affects card costs and effects
- Balanced Aggression (neutral) is safe but slow
- High Aggression (reckless) is fast but dangerous
- Low Aggression (cautious) is safe but inefficient

**Understanding** (Global, Persistent):
- Shared tier-unlocking resource across all challenge types
- Higher Understanding unlocks advanced Physical cards
- Gained through tactical mastery across all systems

### 4.6.3 Action Pair: EXECUTE / ASSESS

**EXECUTE: Lock Option as Preparation**
- Spend Exertion (session resource)
- Play EXECUTE card (specific physical action)
- Lock Option without triggering effect yet
- Build prepared sequence of multiple locked Options
- Exertion committed, waiting for ASSESS

**Example EXECUTE Card:**
```
"Power Grip" (EXECUTE, Authority-bound)
Cost: 3 Exertion
Effect: Lock [+5 Breakthrough, +2 Danger]
```

**ASSESS: Evaluate Situation and Trigger Combo**
- No cost (resolution action)
- Triggers ALL locked Options simultaneously
- Executes prepared sequence as single combo
- Resets state for next EXECUTE cycle

**Key Mechanic**: Options lock during EXECUTE phase, trigger during ASSESS phase. This creates two-phase rhythm - prepare sequence, execute combo, prepare next sequence.

**Tactical Depth**: Player must plan combo sequences. Which EXECUTE cards complement each other? How much Danger accumulates if triggering all locked Options? Can player reach Breakthrough threshold before Danger threshold?

**Strategic Timing**: When to ASSESS? Too early (few locked Options) wastes opportunity. Too late (Danger accumulating) risks threshold breach.

### 4.6.4 Why One-Shot Test Model

**Fictional Reality**: Physical obstacles are immediate tests. You can't pause halfway up a cliff. You either complete the climb or fall.

**High Stakes**: One-shot model creates genuine tension. No safety net of "pause and come back tomorrow."

**Resource Commitment**: Starting Physical challenge commits Stamina. Failed attempt means resources spent with no Progress carried forward.

**Distinct From Mental**: Mental challenges respect investigation time pressure (pauseable). Physical challenges respect physical reality (one-shot).

**Matches Player Mental Model**: "I'm attempting this obstacle right now" implies commitment, not leisurely pauseable exploration.

---

## 4.7 Social Challenges: Conversations with NPCs

Social challenges represent real-time interactions with entities that have agency. They reflect the reality that **conversations happen in moments** - you can't pause mid-discussion and resume tomorrow.

### 4.7.1 Session Model: Session-Bounded Dynamic Interaction

**Session-Bounded**: Must complete in single interaction. Cannot leave and return.

**MaxDoubt Ends Conversation**: NPC frustration forces conversation end when Doubt reaches maximum. This is not player choice - NPC has agency.

**No Pause/Resume**: Cannot pause mid-conversation and return later. Conversations happen in continuous flow.

**Can Leave Early**: Player can voluntarily end conversation before reaching Momentum threshold. This has consequences to relationship but guarantees forward progress.

**Relationship Persists**: StoryCubes (relationship state) and relationship scores remember between conversations. Though conversation is session-bounded, relationship is persistent.

### 4.7.2 Core Session Resources

**Momentum** (Builder):
- Progress toward goal in single conversation session
- Does NOT persist - each conversation starts at 0
- Victory threshold: Reach target Momentum value before MaxDoubt
- Represents advancing conversational goal (persuasion, negotiation, information gathering)

**Initiative** (Session):
- Action economy currency
- Accumulated via Foundation cards (cheap setup actions)
- Spent on Statement cards (expensive conversational moves)
- Creates two-phase rhythm: Gather Initiative, spend on Statements

**Doubt** (Threshold):
- NPC frustration toward conversation end
- Starts at base value (varies by NPC demeanor)
- Accumulates from poor conversational choices
- Reaching MaxDoubt: NPC ends conversation (forced exit)
- Represents NPC patience wearing thin

**Cadence** (Balance):
- Conversation rhythm and pacing
- Affects card costs and NPC responses
- Balanced Cadence maintains smooth conversation
- Disrupted Cadence increases Doubt faster
- Represents conversational flow quality

**Understanding** (Global, Persistent):
- Shared tier-unlocking resource across all challenge types
- Higher Understanding unlocks advanced Social cards
- Gained through tactical mastery across all systems

### 4.7.3 Action Pair: SPEAK / LISTEN

**SPEAK: Advance Conversation Through Statements**
- Spend Initiative (session resource)
- Play Statement card (significant conversational move)
- Build Momentum toward goal
- Risk increasing Doubt if poorly chosen
- Drives conversation forward

**Example Statement Card:**
```
"Appeal to Authority" (SPEAK, Authority-bound)
Cost: 5 Initiative
Effect: +6 Momentum, +3 Doubt
Requires: Authority ≥ 4
```

**LISTEN: Reset and Draw**
- No cost (recovery action)
- Draw new cards to hand
- Slight Cadence adjustment (calming conversation pace)
- No Momentum progress (tactical pause)
- Represents taking in new information, reassessing approach

**Key Mechanic**: SPEAK advances Momentum but risks Doubt. LISTEN provides new tactical options without advancing goal. Player must balance progress with safety.

**Tactical Depth**: When to SPEAK? When to LISTEN? Speaking builds Momentum toward victory but accumulates Doubt toward NPC-forced exit. Listening provides options but doesn't advance goal.

**Initiative Economy**: Foundation cards generate Initiative cheaply. Statement cards spend Initiative expensively. Efficient play sequences Foundation → Foundation → Statement → Listen → repeat.

### 4.7.4 Why Session-Bounded Model

**Fictional Reality**: Conversations happen in real-time with another person. You can't pause mid-sentence and walk away, returning tomorrow to resume.

**NPC Agency**: Other person has patience limits. They will end conversation if frustrated (MaxDoubt). Player doesn't control this.

**Relationship vs Session**: Though session is bounded, relationship persists. Poor conversation performance damages relationship, affecting future conversations.

**Distinct From Mental**: Mental challenges are static puzzles (pauseable). Social challenges are dynamic interactions (cannot pause another person).

**Matches Player Mental Model**: "I'm talking to this NPC right now" implies continuous interaction, not something resumable at leisure.

---

## 4.8 Challenge Integration with Strategic Layer

### 4.8.1 When Challenges Spawn

Challenges spawn when ChoiceTemplate.ActionType = StartChallenge. The system:

1. Stores pending context (parent Scene, parent Situation, rewards)
2. Extracts SituationCards from parent Situation (victory conditions)
3. Spawns temporary session (Mental/Physical/Social)
4. Player enters tactical layer

**Strategic Visibility Before Entry**: Player sees challenge difficulty, cost to enter, possible outcomes (OnSuccess reward, OnFailure reward) before committing. Perfect information preserved at bridge crossing.

### 4.8.2 Success and Failure Outcomes

**Both Outcomes Advance Story**: Success applies OnSuccessReward, failure applies OnFailureReward. Neither creates dead-end state.

**Example Success Outcome**:
- Momentum ≥ threshold in Social challenge
- OnSuccessReward: Unlock private room, advance scene to next situation

**Example Failure Outcome**:
- Doubt ≥ MaxDoubt (NPC ends conversation)
- OnFailureReward: Pay extra 5 coins, advance scene to next situation (different path)

**Reward Asymmetry**: Success rewards are typically better (unlocks, reduced costs, NPC favor). Failure rewards are typically worse (extra costs, reduced options, NPC coolness). But both advance progression.

### 4.8.3 Challenge Optionality

Not all choices route to challenges. Standard four-choice pattern:

1. **Stat-Gated Success Path**: InstantSuccess (no challenge)
2. **Money-Gated Success Path**: InstantSuccess (no challenge)
3. **Challenge Path**: StartChallenge (routes to tactical layer)
4. **Fallback Path**: InstantSuccess (no challenge, poor outcome)

Player can often avoid challenges entirely by using stat-gated or money-gated paths. Challenges represent the "demonstrate skill" option, not forced engagement.

**Why Optionality Matters**: Player chooses whether to engage tactical layer. High stats enable bypassing challenges entirely. Low stats force challenge engagement or money spending or fallback acceptance.

### 4.8.4 Perfect Information at Bridge

Before crossing to tactical layer, player sees:

**Entry Cost**: Stamina -2, Focus -3, Resolve -1 (resource deduction before entering)
**Challenge Type**: Mental/Physical/Social (which tactical system)
**Difficulty Indicator**: Categorical difficulty (Simple, Moderate, Complex, Intricate)
**Success Threshold**: Required builder resource value (Momentum ≥ 8)
**Success Reward**: Exact reward applied if victorious
**Failure Reward**: Exact reward applied if defeated

Player can calculate: "Is this challenge worth attempting given my current resources?" If low on Stamina, Physical challenge may not be wise. If low on Focus, Mental challenge risky.

---

## 4.9 Tactical Resource Management

### 4.9.1 Session Resources as Universal Costs

All challenge types derive session resources from permanent stats:

**Mental**: Attention = Function(Focus)
**Physical**: Exertion = Function(Stamina)
**Social**: Initiative = Generated by Foundation cards (influenced by stats)

This creates strategic resource pressure. Permanent stat depletion affects tactical capability:
- Low Focus → Low Attention → Fewer ACT cards in Mental challenges
- Low Stamina → Low Exertion → Fewer EXECUTE cards in Physical challenges
- Low Resolve → Cannot enter some Social challenges

### 4.9.2 Understanding as Cross-System Currency

Understanding is the ONLY resource shared across all three challenge types. It represents growing tactical mastery.

**How Understanding Increases**:
- Victory in any challenge type grants Understanding
- Tactical excellence (high efficiency, low danger) grants bonus Understanding
- Cross-system play encouraged (all three types contribute)

**What Understanding Unlocks**:
- Higher card tiers in ALL systems (not system-specific)
- Advanced tactics available across Mental, Physical, Social
- Represents player becoming skilled at tactical gameplay generally

**Why Cross-System Currency**: Understanding encourages engagement with all three challenge types. Specializing in only Social challenges limits Understanding growth, gates higher Physical and Mental tactics.

### 4.9.3 Victory Threshold Tuning

Victory thresholds scale by categorical difficulty:

**Simple**: Low threshold, easy victory (tutorial-level challenges)
**Moderate**: Medium threshold, requires decent tactical play
**Complex**: High threshold, requires efficient play or strong deck
**Intricate**: Very high threshold, demands optimization or specialization

Catalogues translate categorical difficulty to concrete thresholds at parse-time. AI-generated challenges specify "Complex Social challenge" without knowing exact Momentum threshold. Catalogue applies universal formula: Base × Difficulty × Quality × PowerDynamic = Threshold.

### 4.9.4 Persistent vs Ephemeral State

**Persistent State** (survives session destruction):
- Player stats (Insight, Rapport, Authority, Diplomacy, Cunning)
- Health, Stamina, Focus, Resolve
- Understanding
- NPC relationships (StoryCubes, relationship scores)
- Mental challenge Progress and Exposure (location-specific)

**Ephemeral State** (destroyed when session ends):
- Challenge session object itself
- Session resources (Attention, Exertion, Initiative remaining)
- Session-specific builder resources (Momentum, Breakthrough)
- Session-specific threshold resources (Doubt, Danger)
- Hand state, deck state, flow mechanics

This separation ensures challenges are truly tactical. No persistent "challenge progression" beyond Mental investigations. Physical and Social challenges are fresh tests each attempt.

---

## 4.10 Card Design Principles

### 4.10.1 Every Card Bound to Stat

Every tactical card binds to exactly one of five stats:
- Insight
- Rapport
- Authority
- Diplomacy
- Cunning

**Stat Binding Determines**:
- Which stat gains XP when card played
- Which stat gates access to higher card tiers
- Thematic coherence (Authority cards feel commanding, Rapport cards feel connective)

**No Unbound Cards**: Every card contributes to stat progression. No "neutral" cards that waste tactical plays.

### 4.10.2 Depth Tiers and Unlocking

Cards have depth tiers representing power level:

**Tier 1 (Depth 0-1)**: Basic cards, available from start
**Tier 2 (Depth 2-3)**: Intermediate cards, requires moderate stat levels
**Tier 3 (Depth 4-5)**: Advanced cards, requires high stat investment
**Tier 4 (Depth 6+)**: Mastery cards, requires specialization

Player stat levels gate access:
- Insight 5 unlocks Insight-bound Tier 2 cards
- Insight 10 unlocks Insight-bound Tier 3 cards
- Insight 15 unlocks Insight-bound Tier 4 cards

Specialization creates deck power. Generalist has many Tier 1-2 cards. Specialist has few Tier 3-4 cards (devastating but narrow).

### 4.10.3 Cross-System Card Philosophy

Cards in different systems bound to same stat feel thematically similar:

**Authority Cards Across Systems**:
- Mental: "Decisive Analysis" (make authoritative conclusion about evidence)
- Physical: "Power Move" (dominant physical action commanding environment)
- Social: "Assert Position" (establish conversational dominance)

All three feel like Authority manifesting in different contexts. Player with high Authority has powerful options across all challenge types.

### 4.10.4 Cost-Effect Balance

Cards balance cost against effect:

**Low-Cost Cards**: Small effects, high efficiency, build resources slowly
**High-Cost Cards**: Large effects, low efficiency, dramatic swings

**Example Low-Cost Mental Card**:
```
"Careful Observation" (Insight)
Cost: 1 Attention
Effect: +1 Progress, +1 Lead
```

**Example High-Cost Mental Card**:
```
"Reconstruct Crime Scene" (Insight)
Cost: 5 Attention
Effect: +8 Progress, +4 Leads, +3 Exposure
```

High-cost cards are powerful but risky. Low-cost cards are safe but slow. Deck composition determines playstyle.

---

## 4.11 Challenge Design Examples

### 4.11.1 Mental Challenge Example: "The Missing Merchant"

**Context**: Local merchant vanished three days ago. Scene spawns investigation at marketplace.

**Challenge Details**:
- **Type**: Mental challenge at Market Square location
- **Difficulty**: Moderate (Progress threshold = 15)
- **Starting Exposure**: 0 (fresh investigation)
- **Player Resources**: Attention = 12 (derived from Focus 8)

**Tactical Gameplay**:

Turn 1: Player plays "Interview Witnesses" (3 Attention, +2 Progress, +2 Leads)
- Progress: 0 → 2
- Leads: 0 → 2
- Attention: 12 → 9

Turn 2: Player uses OBSERVE action (no cost, draw 2 Detail cards equal to Leads)
- Draws "Suspicious Timing" (+1 Progress) and "Known Associates" (+2 Leads)
- Progress: 2 → 3
- Leads: 2 → 4

Turn 3: Player plays "Search Records" (4 Attention, +3 Progress, +1 Exposure)
- Progress: 3 → 6
- Exposure: 0 → 1
- Attention: 9 → 5

Turn 4: Player uses OBSERVE action (draw 4 Detail cards)
- Accumulates Progress and Leads from Details

Player realizes Attention running low (5 remaining). Chooses to leave location, rest, return tomorrow with fresh Attention. Progress (6/15) and Exposure (1) persist. Next visit continues from current state.

**Why This Works**: Pauseable session model matches investigation fiction. Player makes tactical progress but doesn't need to complete in one session. Can return with restored Attention.

### 4.11.2 Physical Challenge Example: "Crumbling Wall Climb"

**Context**: Route blocked by collapsed wall. Must climb unstable rubble to proceed.

**Challenge Details**:
- **Type**: Physical challenge on route segment
- **Difficulty**: Moderate (Breakthrough threshold = 12)
- **MaxDanger**: 8
- **Player Resources**: Exertion = 10 (derived from Stamina 7)

**Tactical Gameplay**:

Turn 1: Player plays "Assess Structure" (EXECUTE, 2 Exertion, lock [+3 Breakthrough])
- Locked Options: [+3 Breakthrough]
- Exertion: 10 → 8

Turn 2: Player plays "Careful Footing" (EXECUTE, 3 Exertion, lock [+4 Breakthrough, +1 Danger])
- Locked Options: [+3 Breakthrough], [+4 Breakthrough, +1 Danger]
- Exertion: 8 → 5

Turn 3: Player uses ASSESS action (trigger all locked Options)
- Breakthrough: 0 → 7 (3 + 4)
- Danger: 0 → 1
- Locked Options cleared

Turn 4: Player plays "Power Through" (EXECUTE, 4 Exertion, lock [+6 Breakthrough, +3 Danger])
- Locked Options: [+6 Breakthrough, +3 Danger]
- Exertion: 5 → 1

Turn 5: Player uses ASSESS action (trigger locked Option)
- Breakthrough: 7 → 13 (reached threshold!)
- Danger: 1 → 4 (below MaxDanger 8)
- **VICTORY**

Session ends, OnSuccessReward applied (route segment completed, proceed to next segment).

**Why This Works**: One-shot session model matches physical obstacle fiction. Player must commit to completion. Failed attempt would reset Breakthrough but cost Stamina permanently.

### 4.11.3 Social Challenge Example: "Negotiate Inn Lodging"

**Context**: Player needs room at inn. NPC innkeeper (Neutral demeanor, Premium quality inn).

**Challenge Details**:
- **Type**: Social challenge with innkeeper NPC
- **Difficulty**: Moderate (Momentum threshold = 8)
- **MaxDoubt**: 6
- **Starting Doubt**: 2 (Neutral demeanor)

**Tactical Gameplay**:

Turn 1: Player plays "Establish Rapport" (Foundation, 0 Initiative cost, +3 Initiative)
- Initiative: 0 → 3

Turn 2: Player plays "Common Ground" (Foundation, 0 Initiative cost, +3 Initiative)
- Initiative: 3 → 6

Turn 3: Player plays "Appeal to Sympathy" (Statement, 5 Initiative cost, +5 Momentum, +1 Doubt)
- Momentum: 0 → 5
- Doubt: 2 → 3
- Initiative: 6 → 1

Turn 4: Player uses LISTEN action (draw new cards, reset hand)
- Slight Cadence adjustment (calming)

Turn 5: Player plays "Offer Fair Payment" (Foundation, 0 Initiative cost, +2 Initiative)
- Initiative: 1 → 3

Turn 6: Player plays "Demonstrate Reliability" (Statement, 3 Initiative cost, +4 Momentum)
- Momentum: 5 → 9 (reached threshold!)
- **VICTORY**

Session ends, OnSuccessReward applied (unlock private room at reduced cost).

**Why This Works**: Session-bounded model matches conversation fiction. NPC has patience limit (MaxDoubt 6). Player must reach Momentum threshold before NPC frustration forces exit.

---

## 4.12 Challenge Difficulty Tuning

### 4.12.1 Categorical Difficulty Properties

Challenges specify categorical difficulty, NOT absolute thresholds:

**AI-Authoring**:
```json
{
  "situationArchetype": "investigation_gathering",
  "challengeType": "Mental",
  "difficulty": "Moderate",
  "npcDemeanor": "Friendly",
  "environmentQuality": "Standard"
}
```

**Catalogue Translation (Parse-Time)**:
```
Base Progress Threshold = 10 (archetype baseline)
Difficulty Multiplier = 1.0 (Moderate)
Demeanor Multiplier = 0.8 (Friendly = easier)
Environment Multiplier = 1.0 (Standard)

Final Threshold = 10 × 1.0 × 0.8 × 1.0 = 8
```

**Result**: AI generates "Moderate investigation with Friendly NPC" without knowing exact Progress threshold. Catalogue translates categorically to 8 Progress required. Same archetype with Hostile NPC = 14 Progress required.

### 4.12.2 Dynamic Scaling Benefits

**Universal Formulas**: One archetype scales to all contexts via categorical properties

**AI Generation**: AI describes RELATIVE difficulty ("Harder than average") without global balance knowledge

**Balance Tuning**: Adjust multipliers globally, all challenges rebalance automatically

**Hand-Authoring Minimal**: Authors specify entity context, not 50 numeric values

**Contextually Appropriate**: Premium inn + Hostile innkeeper = Hard negotiation automatically

### 4.12.3 Difficulty Indicators at Strategic Layer

Before entering challenge, player sees categorical difficulty:

**Simple**: Tutorial-level, low threshold
**Moderate**: Standard difficulty, reasonable threshold
**Complex**: Hard difficulty, high threshold
**Intricate**: Very hard difficulty, very high threshold

Player cannot see EXACT threshold (hidden complexity), but sees categorical indicator enabling strategic decision. "Complex Physical challenge" signals high commitment required.

---

## 4.13 Tactical Mastery and Player Skill Expression

### 4.13.1 Skill Expression Through Efficiency

Player skill determines tactical efficiency:

**Novice Play**: Wasteful card usage, high session resource consumption, barely reaches threshold
**Expert Play**: Efficient sequencing, minimal session resource waste, reaches threshold with resources to spare

Same challenge, same deck, different outcomes based on play quality.

**Example Efficiency Comparison**:
- Novice: Reaches Momentum 8 with 1 Exertion remaining, MaxDoubt nearly reached
- Expert: Reaches Momentum 8 with 4 Exertion remaining, Doubt at safe level

Expert play conserves resources, enabling more challenges per day.

### 4.13.2 Build Specialization vs Generalization

**Specialist Playstyle**:
- High investment in few stats (Insight 15, Cunning 12, others 3)
- Access to Tier 3-4 cards in specialized stats
- Devastating in specific challenge types
- Vulnerable when forced into non-specialized challenges

**Generalist Playstyle**:
- Moderate investment in all stats (all stats 7-8)
- Access to Tier 2 cards across all stats
- Consistent performance across all challenge types
- Never devastating, never helpless

**Hybrid Playstyle**:
- High in two stats, low in others
- Tier 3 access in two specializations
- Flexibility with power spikes
- Strategic choice about which challenges to attempt

Player builds create identity through stat distribution.

### 4.13.3 Understanding as Mastery Metric

Understanding grows through tactical excellence:

**Low Understanding** (0-5): Tier 1 cards only, basic tactics
**Moderate Understanding** (6-10): Tier 2 cards available, intermediate tactics
**High Understanding** (11-15): Tier 3 cards available, advanced tactics
**Master Understanding** (16+): Tier 4 cards available, mastery tactics

Understanding is CROSS-SYSTEM. High Understanding unlocks advanced cards in ALL challenge types. Represents player becoming skilled at tactical gameplay generally.

**How Understanding Grows**:
- Victory in any challenge: +1 Understanding
- Efficient victory (low resource waste): +2 Understanding
- Intricate challenge victory: +3 Understanding

Encourages mastery across all three challenge types.

---

## 4.14 Integration with Permanent Progression

### 4.14.1 Stats as Gating and Progression

Five stats serve dual purpose:

**Gating Function**: Stat thresholds gate challenge entry or strategic choices
- Rapport ≥ 5 required to attempt certain Social challenges
- Insight ≥ 8 required to access advanced investigation options

**Progression Function**: Stats unlock higher card tiers in tactical layer
- Cunning 10 unlocks Cunning-bound Tier 3 cards
- Playing Cunning cards grants Cunning XP

This creates feedback loop: Strategic layer gates on stats → Tactical layer advances stats → Strategic layer unlocks more options.

### 4.14.2 Permanent Resources as Challenge Costs

Entering challenges costs permanent resources:

**Mental Challenges**: May cost Focus (mental energy)
**Physical Challenges**: May cost Stamina (physical energy)
**Social Challenges**: May cost Resolve (emotional energy)

This creates strategic resource pressure. Low Stamina makes Physical challenges risky (both entry cost AND session resource limitation).

**Resource Restoration**:
- Rest restores Focus and Stamina (costs time)
- Food restores Health (costs coins)
- Social rest restores Resolve (costs time)

Player must balance challenge engagement with resource restoration. Cannot chain endless challenges without rest.

### 4.14.3 Equipment as Tactical Modifiers

Equipment provides permanent tactical benefits:

**Mental Equipment** (Investigation Tools): Increase Attention cap, reduce Exposure accumulation
**Physical Equipment** (Climbing Gear, Weapons): Increase Exertion cap, reduce Danger accumulation
**Social Equipment** (Fine Clothing, Gifts): Increase Initiative generation, reduce starting Doubt

Equipment purchased with coins (strategic resource) improves tactical performance. Investment pays off across multiple challenges.

---

## 4.15 Why Three Parallel Challenge Systems

### 4.15.1 Thematic Coverage

Three systems cover all conflict resolution types:

**Mental**: Puzzles, mysteries, investigations, analysis
**Physical**: Obstacles, threats, exertion, environmental challenges
**Social**: Persuasion, negotiation, relationship navigation, conversation

Any narrative situation routes to one of three systems. No mechanical gaps.

### 4.15.2 Playstyle Variety

Players can specialize in preferred challenge type:

**Investigation Specialist**: High Insight/Cunning, seeks Mental challenges
**Physical Adept**: High Authority/Cunning, prefers Physical challenges
**Social Butterfly**: High Rapport/Diplomacy, focuses Social challenges

Same game accommodates different player preferences through stat specialization.

### 4.15.3 Unified Progression

Despite three systems, progression is unified:

- Same five stats across all systems
- Same Understanding currency
- Same card depth tier structure
- Same builder/session/threshold resource pattern

Player learns one system, understands all three. No separate progression currencies or conflicting mechanics.

### 4.15.4 Fiction-Gameplay Alignment

Three systems align with fictional reality:

**Mental = Static**: Investigations are pauseable puzzles, persistence matches fiction
**Physical = Dynamic**: Obstacles are one-shot tests, commitment matches fiction
**Social = Interactive**: Conversations are bounded by NPC agency, session model matches fiction

Mechanics reinforce theme. Player mental model matches mechanical model.

---

## Related Documentation

**Technical Implementation**:
- Arc42 05_building_block_view.md - Challenge session architecture
- Arc42 06_runtime_view.md - Challenge execution flow
- Arc42 08_crosscutting_concepts.md - Strategic-tactical bridge pattern

**Game Design**:
- Design 01_core_pillars.md - Perfect information principle
- Design 02_progression_systems.md - Stat advancement and specialization
- Design 03_strategic_layer.md - Scene/Situation/Choice flow
- Design 05_resource_economy.md - Permanent resource management
- Design 06_content_generation.md - Challenge archetype catalogues

**Source Documents**:
- DESIGN_PHILOSOPHY.md (lines 499-668) - Challenge system design patterns
- DESIGN_PHILOSOPHY.md (lines 110-247) - Resource scarcity and balanced choices
- arc42 08_crosscutting_concepts.md (lines 593-668) - Design principles
- arc42 09_architecture_decisions.md - ADR-003 Two-layer architecture
