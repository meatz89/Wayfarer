# Modal Scene Engagement + Archetype-Based Situation Generation

**Design Document - High-Level Conceptual Architecture**

---

## Executive Summary

This document defines two interconnected systems that transform Wayfarer from a browsing-based menu system into a Sir Brante-style forced moment decision system with learnable mechanical patterns.

**System 1: Modal Scene Engagement**
- Scenes take over full screen when player enters location
- Player forced into situation, must make choice
- No atmospheric menu visible until scene completes
- Creates "you're IN this moment NOW" pressure

**System 2: Archetype-Based Situation Generation**
- Mechanical archetypes define choice structure (stat-gated + money + challenge + fallback)
- Consistent patterns players can learn and prepare for
- Manual narrative wrapping (AI generation deferred to Phase 3)
- Domain-appropriate stat testing creates strategic preparation

---

## Part 1: The Core Problems

### Problem 1: Browsing Instead of Experiencing

**Current State:**
- Player arrives at location
- Sees atmospheric menu with many options
- Scenes are menu items among other menu items
- Feels like browsing a catalog, not living moments

**What This Creates:**
- No pressure or urgency
- Choices feel hypothetical ("I could do this")
- No forced confrontation with consequences
- Easy to ignore important content

**What We Need:**
- Sir Brante's "you're IN this situation NOW" feeling
- Forced moments where player must engage
- Immediate visibility of resource constraints
- Cannot browse away without making choice

### Problem 2: Unpredictable Mechanics Without Learnable Patterns

**Current State:**
- Each situation is hand-authored and unique
- No patterns to learn or prepare for
- Player can't develop strategic literacy
- Resource allocation feels arbitrary

**What This Creates:**
- Can't prepare correctly (don't know what to expect)
- Stat building feels random (don't know what matters where)
- No "Oh, I should have built Diplomacy for Economic zones"
- Each situation is surprise, not learned pattern

**What We Need:**
- Five mechanical archetypes that repeat
- Domain associations (Economic → Negotiation common)
- Learnable stat requirements
- Strategic preparation payoff

---

## Part 2: Design Principles

### Principle 1: Freedom at Meta-Level, Force at Immediate Level

**Meta-Level Freedom (Strategic):**
- Choose which location to visit
- Choose which scene to engage (if multiple)
- Choose when to engage (can leave and return)
- Choose to exit scene mid-way (via "Leave" choice with cost)

**Immediate-Level Force (Tactical):**
- Once in situation: Must make choice
- Cannot access atmospheric menu
- Cannot browse other options
- Cannot navigate away without explicit "Leave" choice

**Result:** Player navigates freely between locations but experiences forced moments within scenes.

### Principle 2: Mechanical Consistency, Narrative Variety

**Mechanical Layer (Consistent):**
- Five archetypes: Confrontation, Negotiation, Investigation, Social Maneuvering, Crisis
- Always same structure: stat-gated + money + challenge + fallback
- Always 4 choices per archetype situation
- Learnable through repetition

**Narrative Layer (Variable):**
- Same archetype, different stories
- "Negotiation" at merchant = price haggling
- "Negotiation" at guard post = permit discussion
- Mechanical structure identical, narrative wrapper different

**Result:** Players learn the game system without memorizing content.

### Principle 3: Perfect Information with Locked Visibility

**What Player Always Sees:**
- All four choices (available + locked)
- Exact requirements for locked choices
- Exact costs for available choices
- Current resource state

**What Player Discovers:**
- "I need Diplomacy 3+ for best option"
- "I spent coins in Situation 1, can't afford Situation 2's coin option"
- "My earlier choice locked future options"
- Compounding consequences visible immediately

**Result:** Sir Brante's tight feedback loop where consequences are immediately visible.

### Principle 4: Scenes Control Their Own Pacing

**Two Progression Modes:**

**Cascade Mode (High Pressure):**
- Complete Situation 1 → Immediately see Situation 2
- No menu between situations
- Forced through entire scene sequence
- Use for: Tutorial sequences, quest critical paths, crisis scenarios

**Breathe Mode (Low Pressure):**
- Complete Situation 1 → Return to atmospheric menu
- Scene remains active at location
- Next entry resumes at Situation 2
- Use for: Ambient scenes, optional content, exploratory scenes

**Result:** Different content types have appropriate pacing without player configuration.

### Principle 5: Archetype Defines Structure, Placement Defines Context

**Archetype Contribution:**
- Which stats are tested
- Cost structure (0/low/high)
- Challenge types
- Fallback penalty
- MECHANICS ONLY

**Placement Contribution:**
- Which NPC or location
- Narrative description
- Placeholder replacement ({NPCName}, {LocationName})
- Story context
- NARRATIVE ONLY

**Result:** One archetype works at any placement with appropriate narrative context.

---

## Part 3: Modal Scene Engagement Architecture

### 3.1 Scene Properties

**Every Scene Has:**

**PresentationMode enum:**
- `Atmospheric`: Normal menu display (existing behavior)
- `Modal`: Takes over full screen on location entry

**ProgressionMode enum:**
- `Breathe`: Return to menu after each situation
- `Cascade`: Continue to next situation immediately

**Defaults:** Atmospheric + Breathe (backward compatible)

### 3.2 Location Entry Flow

**Current Flow:**
```
Player arrives at location
  → Show Landing view (atmospheric menu)
  → Player clicks "Look Around"
  → Show scenes as menu options
```

**New Flow:**
```
Player arrives at location
  ↓
CHECK: Modal scenes at location?
  ↓
YES → Display scene immediately (full screen, no menu)
NO  → Show Landing view (normal atmospheric menu)
```

### 3.3 Modal Scene Display

**Full-Screen Presentation:**
- Scene header (name, progress indicator)
- Current situation narrative
- Player resource display (stats/coins/energy relevant to choices)
- 4 choices with locked/unlocked state
- Lock reasons visible for locked choices
- NO back button (choices control exit)

**No Atmospheric Menu Visible:**
- Cannot see other location actions
- Cannot navigate to other spots
- Cannot access inventory/journal
- Must engage with situation

**Exit Conditions:**
- Scene completes (all situations resolved)
- Player selects "Leave" choice (with cost)
- Scene marks itself as completed via reward

### 3.4 Situation Progression Within Scene

**Cascade Mode Behavior:**
```
Player in Situation 1
  → Selects choice
  → Apply costs/rewards
  → Scene advances to Situation 2
  → Immediately display Situation 2 (no menu)
  → Player forced through sequence
```

**Breathe Mode Behavior:**
```
Player in Situation 1
  → Selects choice
  → Apply costs/rewards
  → Scene advances to Situation 2
  → Return to atmospheric menu
  → Scene remains active
  → Next entry: Resume at Situation 2
```

### 3.5 Multiple Modal Scenes at Location

**Scene Selection Screen:**
```
Player arrives at location with 3 modal scenes
  ↓
Show scene selection (not situations yet):
  - Scene A: "Merchant Dispute" (3/4 - Crisis)
  - Scene B: "Trade Negotiation" (2/4)
  - Scene C: "Rumor Investigation" (1/3)
  - [Leave Location]
  ↓
Player picks Scene B
  ↓
Enter Scene B modal presentation
```

**Maintains Freedom:** Player chooses which scene to tackle, but once committed, forced through that scene's situations.

### 3.6 Integration with Existing Systems

**Scene State Tracking:**
- `Scene.CurrentSituationId` tracks progress
- Scene remains at location until completed
- Player can leave and return (scene persists in state)

**Action Execution:**
- Same executors (NPCActionExecutor, LocationActionExecutor)
- Same reward application (RewardApplicationService)
- Same validation (requirement checking, cost deduction)
- Only difference: Where player goes after execution

**Backward Compatibility:**
- Existing scenes default to Atmospheric (no modal takeover)
- Hand-authored situations continue working
- Tutorial scenes can opt into Modal without rewriting

---

## Part 4: Archetype-Based Situation Generation

### 4.1 The Five Core Archetypes

#### Archetype 1: CONFRONTATION
**When Used:** Authority challenges, physical barriers, intimidation moments

**Choice Structure:**
1. **Stat-Gated (Authority/Intimidation 3+):** Command, assert dominance, overpower
   - Cost: 0 resources
   - Outcome: Best result, fast resolution

2. **Money (10-15 coins):** Pay off, bribe, avoid conflict
   - Cost: Significant coins
   - Outcome: Decent result, expensive

3. **Challenge (Physical):** Fight, endure, physically compete
   - Cost: Stamina
   - Outcome: Variable (success/failure), risky

4. **Fallback (Always Available):** Submit, back down, lose face
   - Cost: Time or reputation
   - Outcome: Poor result, scene continues with penalty

**Example Contexts:**
- Guard blocks path → Assert authority vs pay entry vs force through vs turn back
- Gang threatens → Intimidate vs bribe vs fight vs flee
- Official questions authority → Show credentials vs pay fine vs argue vs submit

#### Archetype 2: NEGOTIATION
**When Used:** Price disputes, deal-making, compromise seeking

**Choice Structure:**
1. **Stat-Gated (Diplomacy/Rapport 3+):** Persuade, charm, find fair compromise
   - Cost: 0 resources
   - Outcome: Best deal, mutual benefit

2. **Money (15-20 coins):** Pay premium, sweeten deal, offer incentive
   - Cost: Large coins
   - Outcome: Get what you want, expensive

3. **Challenge (Mental):** Debate, cite regulations, out-logic opponent
   - Cost: Resolve
   - Outcome: Variable, cerebral approach

4. **Fallback (Always Available):** Accept unfavorable terms, lose advantage
   - Cost: Coins + reputation
   - Outcome: Poor deal, scene continues

**Example Contexts:**
- Merchant demands higher price → Negotiate vs pay premium vs cite regulations vs accept markup
- Contract terms unfavorable → Compromise vs pay extra vs legal argument vs sign anyway
- Resource allocation dispute → Mediate vs compensate vs debate vs concede

#### Archetype 3: INVESTIGATION
**When Used:** Mysteries, puzzles, information gathering, deduction

**Choice Structure:**
1. **Stat-Gated (Insight/Cunning 3+):** Deduce, analyze, spot hidden patterns
   - Cost: 0 resources
   - Outcome: Full understanding, correct solution

2. **Money (10 coins):** Pay informant, buy clues, hire expert
   - Cost: Moderate coins
   - Outcome: Get answer, don't learn method

3. **Challenge (Mental):** Work through puzzle, complex deduction, trial-and-error
   - Cost: Time + Focus
   - Outcome: Variable, learn if successful

4. **Fallback (Always Available):** Guess, give up, miss critical information
   - Cost: Miss clues, scene continues incomplete
   - Outcome: Proceed without full understanding

**Example Contexts:**
- Cryptic message → Decode vs hire scholar vs attempt decryption vs ignore
- Crime scene → Spot clues vs pay investigator vs search thoroughly vs leave
- Hidden mechanism → Understand vs pay engineer vs tinker vs abandon

#### Archetype 4: SOCIAL MANEUVERING
**When Used:** Reputation management, relationship building, social hierarchy

**Choice Structure:**
1. **Stat-Gated (Rapport/Cunning 3+):** Read people, empathize, subtle manipulation
   - Cost: 0 resources
   - Outcome: Strong connection, social capital gained

2. **Money (10 coins):** Gift, favor, social currency, expensive gesture
   - Cost: Coins or item
   - Outcome: Obligation created, transactional

3. **Challenge (Social):** Risk reputation, bold statement, gamble on reaction
   - Cost: Potential reputation loss
   - Outcome: Variable, high risk/reward

4. **Fallback (Always Available):** Alienate, offend, miss connection, awkward exit
   - Cost: Reputation or opportunity
   - Outcome: Relationship damaged, scene continues

**Example Contexts:**
- Noble takes offense → Smooth over vs offer gift vs double down vs walk away awkwardly
- Social gathering misstep → Recover gracefully vs compensate vs bold stance vs leave
- Faction allegiance questioned → Navigate carefully vs prove loyalty vs challenge authority vs stay neutral

#### Archetype 5: CRISIS DECISION
**When Used:** Emergencies, high-stakes moments, moral dilemmas, scene climaxes

**Choice Structure:**
1. **Stat-Gated (Variable stat 4+):** Heroic action, expert solution, leadership
   - Cost: 0 resources
   - Outcome: Optimal resolution, saves everyone

2. **Money (25+ coins):** Expensive emergency solution, hire professionals, buy way out
   - Cost: Massive coins
   - Outcome: Problem solved, financially ruined

3. **Challenge (High-Stakes):** Personal risk, gamble everything, desperation move
   - Cost: Health/Stamina risk
   - Outcome: Save some/lose some, heroic attempt

4. **Fallback (Always Available):** Flee, let it happen, accept severe consequence
   - Cost: Permanent guilt, scene fails, major penalty
   - Outcome: Worst outcome, live with failure

**Example Contexts:**
- Building collapses → Organize rescue vs hire rescuers vs attempt yourself vs flee
- Violence erupts → Stop it vs pay for peace vs physically intervene vs escape
- Innocent accused → Defend them vs bribe judge vs testify (risk self) vs stay silent

### 4.2 Domain-Archetype Associations

**Domain = Where situations occur (location type/theme)**

**Economic Domain (Merchant quarters, markets, guilds):**
- Negotiation (common) - Price disputes, contract terms, trade deals
- Social Maneuvering (common) - Reputation with merchants, guild standing
- Confrontation (uncommon) - Business rivals, protection rackets
- Investigation (uncommon) - Fraud, theft, supply chain mysteries
- Crisis (rare) - Market collapse, guild war, economic disaster

**Authority Domain (Government buildings, guard posts, courts):**
- Confrontation (common) - Authority challenges, permit disputes, power displays
- Negotiation (common) - Legal agreements, official permissions, compromise
- Social Maneuvering (uncommon) - Political alliances, faction navigation
- Investigation (uncommon) - Legal research, corruption, evidence gathering
- Crisis (rare) - Riots, coups, legal emergencies

**Mental Domain (Libraries, laboratories, scholarly societies):**
- Investigation (common) - Research, puzzles, mysteries, knowledge seeking
- Negotiation (common) - Intellectual debates, research funding, academic disputes
- Social Maneuvering (uncommon) - Academic politics, mentor relationships
- Confrontation (uncommon) - Intellectual challenges, proof disputes
- Crisis (rare) - Dangerous discovery, knowledge that threatens society

**Social Domain (Taverns, noble estates, social gatherings):**
- Social Maneuvering (common) - Reputation building, relationship navigation, faux pas recovery
- Negotiation (common) - Social contracts, favor trading, alliance building
- Investigation (uncommon) - Gossip networks, secret discovery, social mysteries
- Confrontation (uncommon) - Social challenges, duels of wit, public disputes
- Crisis (rare) - Scandal, betrayal revealed, social collapse

**Physical Domain (Wilderness, dangerous areas, physical challenges):**
- Confrontation (common) - Physical barriers, combat, survival challenges
- Investigation (common) - Tracking, finding paths, reading terrain
- Crisis (common) - Life-threatening emergencies, natural disasters
- Negotiation (uncommon) - Bargaining with dangerous entities
- Social Maneuvering (rare) - Rare in pure wilderness

### 4.3 Archetype Template Structure

**What an Archetype Defines:**

**Identity:**
- Archetype ID (string: "confrontation", "negotiation", etc.)
- Archetype Name (display: "Confrontation", "Negotiation", etc.)
- Domain Association (Economic, Authority, Mental, Social, Physical)

**Choice Structure (4 choices always):**

**Choice 1 - Stat-Gated:**
- Which stat(s) required (Authority, Diplomacy, Insight, Rapport, Cunning, Intimidation)
- Stat threshold (3+ typical, 4+ for crisis)
- Zero resource cost
- Best outcome

**Choice 2 - Money:**
- Coin cost (10-25 depending on archetype)
- No stat requirement
- Guaranteed success but expensive
- Decent outcome

**Choice 3 - Challenge:**
- Challenge type (Physical, Mental, Social)
- Resource cost (Stamina, Focus, or Reputation risk)
- Variable outcome (success/failure)
- Risky approach

**Choice 4 - Fallback:**
- No requirements, always available
- Often costs time or reputation
- Poor outcome but allows progress
- Escape valve

**Narrative Template Structure:**
- Situation description with placeholders
- Choice labels (generic, get replaced at instantiation)
- Placeholder types: {NPCName}, {LocationName}, {PlayerName}, {Domain-specific terms}

### 4.4 Archetype Catalog as Parse-Time Catalogue (CRITICAL ARCHITECTURAL ALIGNMENT)

**⚠️ IMPORTANT: SituationArchetypeCatalog IS A CATALOGUE in the CLAUDE.md sense.**

**CATALOGUE PATTERN COMPLIANCE:**
- Lives in `src/Content/Catalogues/` (standard location)
- Called ONLY from SceneTemplateParser at PARSE TIME
- NEVER called from facades, managers, or runtime code
- NEVER imported in any file except parsers
- Generates complete entities (ChoiceTemplates) from categorical properties (archetypeId)
- Translation happens ONCE at game initialization, NEVER during gameplay

**Data Flow:**
```
JSON (archetypeId: "confrontation")
  → SceneTemplateParser reads at parse time
  → SituationArchetypeCatalog.GetArchetype() called
  → Catalogue returns archetype with choice structure
  → Parser generates 4 ChoiceTemplates
  → Store in SituationTemplate
  → GameWorld.SituationTemplates (complete, no further generation)
```

**Runtime Behavior:**
- Facades query GameWorld.Situations (pre-populated)
- SceneFacade instantiates actions from ChoiceTemplates (normal flow)
- NO catalogue calls - all entities already generated at parse time
- Pure data access, no procedural generation at runtime

**Verification:**
- If you see `using Wayfarer.Content.Catalogues;` anywhere except SceneTemplateParser → ARCHITECTURAL VIOLATION

### 4.5 Stat Selection (Fixed Approach)

**Principle:** Archetype defines exactly which stats apply.

**Confrontation Always Tests:**
- Primary: Authority (commanding, asserting dominance)
- Secondary: Intimidation (threatening, physical presence)

**Negotiation Always Tests:**
- Primary: Diplomacy (persuasion, fair dealing)
- Secondary: Rapport (charm, relationship building)

**Investigation Always Tests:**
- Primary: Insight (pattern recognition, deduction)
- Secondary: Cunning (cleverness, lateral thinking)

**Social Maneuvering Always Tests:**
- Primary: Rapport (empathy, reading people)
- Secondary: Cunning (manipulation, subtle influence)

**Crisis Varies:**
- Depends on crisis type
- Leadership crisis → Authority
- Moral dilemma → Insight
- Social scandal → Rapport
- Physical danger → (no stat, physical challenge only)

**Result:** Players learn "Economic zones test Diplomacy, Authority zones test Authority" → Strategic preparation becomes meaningful.

### 4.6 Instantiation Process (Parse Time)

**⚠️ CRITICAL: This is PARSE-TIME ONLY, not runtime generation.**

**Template Declares Archetype:**
```json
{
  "situationTemplates": [{
    "id": "merchant_price_dispute",
    "archetypeId": "negotiation",
    "narrativeTemplate": "The merchant crosses arms. 'Price went up. Take it or leave it.'",
    "placementContext": {
      "npcId": "merchant_guild_rep",
      "locationId": "merchant_quarter"
    }
  }]
}
```

**Parser Processing:**
1. Read `archetypeId: "negotiation"`
2. Fetch archetype from SituationArchetypeCatalog
3. Generate 4 ChoiceTemplates from archetype structure:
   - Choice 1: Diplomacy 3+ requirement, 0 cost
   - Choice 2: 15 coins cost, no requirement
   - Choice 3: Mental challenge, Resolve cost
   - Choice 4: Fallback, accept bad terms
4. Apply narrative template with placeholder context
5. Store as SituationTemplate with generated ChoiceTemplates

**At Query Time (Player Enters):**
- SceneFacade activates situation
- Instantiates actions from ChoiceTemplates (normal flow)
- Evaluates requirements (locked/unlocked)
- Presents to UI with lock reasons

**Result:** Same archetype can spawn infinite situations with different narrative contexts.

### 4.7 Learning Curve Design

**First Encounter (Discovery):**
- Player enters Economic zone → Negotiation situation
- Sees Diplomacy option locked (has Diplomacy 2)
- Learns: "Need Diplomacy 3+ for best option"
- Pays coin alternative (15 coins)

**Second Encounter (Pattern Recognition):**
- Later situation in same zone → Negotiation again
- Recognizes: "Oh, this is same structure as before"
- Still lacks Diplomacy 3+ → Must pay coins again
- Resources depleting, realizes preparation needed

**Third Encounter (Strategic Response):**
- Takes delivery job that rewards Diplomacy training
- Builds Diplomacy to 3+
- Returns to Economic zone → Negotiation situation
- NOW has Diplomacy 3+ → Free option available
- Feels smart for preparing correctly

**Fourth Encounter (Mastery):**
- Recognizes Negotiation archetype immediately
- Knows Diplomacy matters in Economic zones
- Pre-prepared, takes free option confidently
- System literacy achieved

**Domain Association Learning:**
- After 3-5 Economic situations: "Diplomacy matters here"
- After 3-5 Authority situations: "Authority matters here"
- Strategic stat building becomes informed choice

---

## Part 5: Integration and Data Flow

### 5.1 Three-Tier Timing Model (Unchanged)

**Tier 1 - Load Time:**
- Parse JSON → SceneTemplates, SituationTemplates
- Archetype-based templates generate ChoiceTemplates here
- Store in GameWorld.SceneTemplates

**Tier 2 - Spawn Time:**
- SceneInstantiator creates Scene + Situations
- Scene.PresentationMode/ProgressionMode set from template
- Situations remain Dormant (no actions yet)
- Store in GameWorld.Scenes, GameWorld.Situations

**Tier 3 - Query Time:**
- Player enters location
- Check for modal scenes (new)
- If modal: Display immediately
- SceneFacade activates situations (Dormant → Active)
- Create actions from ChoiceTemplates
- Evaluate requirements (locked/unlocked)

### 5.2 Choice Execution Flow

**Player Selects Choice (Modal Scene):**
1. LocationContent.HandleModalChoiceSelected(actionId)
2. GameFacade.ExecuteLocationAction(actionId)
3. Executor validates requirements + costs
4. Apply costs (deduct coins/resolve/etc.)
5. Apply rewards (advance scene, spawn new scenes, etc.)
6. Check scene state:
   - Scene completed? → Exit modal, return to Landing
   - Scene continues + Cascade mode? → Load next situation immediately
   - Scene continues + Breathe mode? → Exit modal, scene persists

**Scene Progression:**
- Scene.CurrentSituationId advances to next ID
- Next situation becomes "current"
- If Cascade: Query next situation immediately
- If Breathe: Exit modal, next entry resumes here

### 5.3 Backward Compatibility

**Existing Content Unaffected:**
- Default PresentationMode = Atmospheric (no modal takeover)
- Default ProgressionMode = Breathe (return to menu)
- No ArchetypeId = use hand-authored ChoiceTemplates
- All existing scenes continue working unchanged

**Optional Opt-In:**
- Tutorial scenes can set PresentationMode: Modal
- Quest scenes can set ProgressionMode: Cascade
- New content can use Archetypes
- Old content remains functional

### 5.4 Perfect Information Principle

**Player Always Sees:**
- All modal scenes at location (scene selection if multiple)
- All 4 choices in situation (locked + unlocked)
- Exact requirements for locked choices ("Requires Diplomacy 3+")
- Exact costs for available choices ("15 coins", "2 Resolve", "Physical challenge")
- Current resource state (stats, coins, energy, etc.)

**Player Never Surprised By:**
- Hidden scenes that suddenly appear
- Locked choices without explanation
- Costs that weren't visible upfront
- Requirements that change mid-scene

**Result:** Sir Brante pattern - all information visible, player makes informed decisions, sees consequences immediately.

---

## Part 6: Success Criteria

### Phase 1 Success (Modal UI)

**Functional Requirements:**
✅ Modal scene displays full-screen on location entry
✅ Atmospheric menu NOT visible during modal scene
✅ Cascade scenes advance to next situation without menu
✅ Breathe scenes return to menu after each situation
✅ Scene completion exits modal and returns to Landing view
✅ Multiple modal scenes show selection screen first
✅ Locked choices display with visible requirements

**User Experience:**
✅ Player feels "forced into moment"
✅ Cannot browse away without explicit choice
✅ Clear what choices cost and require
✅ Progression feels pressured (cascade) or relaxed (breathe)

**Technical Quality:**
✅ No compilation errors
✅ Backward compatible (existing scenes work)
✅ State management clean (no leaks between modal/normal)
✅ Navigation stack not corrupted

### Phase 2 Success (Archetypes)

**Functional Requirements:**
✅ 5 archetypes defined in catalog
✅ Archetype-based situations generate 4 choices correctly
✅ Stat requirements assigned from archetype specs
✅ Coin costs match archetype structure
✅ Challenge types (Physical/Mental/Social) assigned correctly
✅ Fallback choices always available

**Pattern Learning:**
✅ Player recognizes archetype after 2-3 encounters
✅ Player learns domain associations (Economic → Negotiation)
✅ Player prepares stats appropriately
✅ Free stat-gated option feels rewarding when unlocked

**Content Quality:**
✅ Manual narrative templates feel appropriate
✅ Placeholders replaced correctly ({NPCName} → actual name)
✅ Same archetype works at different placements
✅ Narrative variety despite mechanical consistency

**Technical Quality:**
✅ Parser validates archetype IDs (fail fast on invalid)
✅ Generated ChoiceTemplates match hand-authored structure
✅ Archetype catalog extensible (easy to add 6th archetype)
✅ Mix of archetype and hand-authored situations works

---

## Part 7: Future Expansion (Phase 3 - Not Implemented This Session)

### AI Generation Integration

**Concept:** AI generates narrative from archetype + placement context.

**AI Receives:**
- Archetype ID ("negotiation")
- Domain (Economic)
- Placement context (NPC name, location name, scene theme)
- Narrative hints (tone, stakes, style)

**AI Generates:**
- Situation description (2-3 sentences)
- Choice 1 text (stat-gated approach, one sentence)
- Choice 2 text (money approach, one sentence)
- Choice 3 text (challenge approach, one sentence)
- Choice 4 text (fallback approach, one sentence)

**Caching:**
- First display: Generate + cache in Situation.GeneratedNarrative
- Subsequent: Use cached value
- Persistent across saves

**Fallback:**
- AI fails? Use manual template as fallback
- Always have manual template as safety net

**Phase 3 Deferred Because:**
- Need to prove mechanical foundation first
- External AI dependency adds complexity/cost
- Manual narrative sufficient for pattern validation

---

## Part 8: Content Authoring Guidelines

### Creating Modal Scenes

**When to Use Modal:**
- Tutorial sequences (teach mechanics under pressure)
- Quest critical paths (player must engage)
- Crisis scenarios (emergency demands immediate response)
- Dramatic narrative moments (story beats that shouldn't be skipped)

**When to Use Atmospheric:**
- Ambient exploration (player discovers at own pace)
- Optional side content (not critical to progression)
- Repeatable activities (daily routines, grinding)
- Background flavor (world-building without pressure)

### Choosing Progression Mode

**Use Cascade When:**
- Teaching sequence (one lesson builds on next)
- Dramatic scene (momentum must continue)
- Time-sensitive crisis (no time for breaks)
- Single narrative arc (situations are chapters of one story)

**Use Breathe When:**
- Exploration content (player should have exit opportunities)
- Multi-session scenes (player might need to stop and return)
- Resource-intensive (player might need to prepare between situations)
- Separate vignettes (situations are standalone within theme)

### Selecting Archetypes

**Ask These Questions:**

1. **What is player trying to overcome?**
   - Authority barrier → Confrontation
   - Unfair deal → Negotiation
   - Mystery/puzzle → Investigation
   - Social mistake → Social Maneuvering
   - Emergency → Crisis

2. **What domain is this?**
   - Merchant zone → Economic (Negotiation likely)
   - Guard post → Authority (Confrontation likely)
   - Library → Mental (Investigation likely)
   - Social gathering → Social (Social Maneuvering likely)

3. **What should player have prepared?**
   - If Economic domain → Diplomacy/Rapport matter
   - If Authority domain → Authority/Intimidation matter
   - If Mental domain → Insight/Cunning matter
   - Archetype + Domain = stat expectations

4. **What's the cost structure?**
   - High-stakes (Crisis) → 25+ coins, 4+ stat
   - Medium-stakes (Negotiation/Confrontation) → 15 coins, 3+ stat
   - Low-stakes (Investigation/Social) → 10 coins, 3+ stat

### Writing Narrative Templates

**Good Template Characteristics:**
- 2-3 sentences describing immediate situation
- Clear problem statement
- Emotional tone appropriate to archetype
- Placeholders for context ({NPCName}, {LocationName})
- No solution stated (choices provide solutions)

**Example Good Templates:**

**Confrontation:**
> "The guard's hand moves to his weapon. 'Papers. Now.' His eyes narrow as he examines your worn boots and road-stained cloak."

**Negotiation:**
> "The merchant waves a dismissive hand. 'Prices went up this morning. Twenty coins, not fifteen. Guild decision, nothing I can do about it.'"

**Investigation:**
> "Three sets of tracks lead from the scene. One heads toward the docks, one to the market, one vanishes into an alley. Blood on the cobblestones suggests someone was injured."

**Social Maneuvering:**
> "Lady Ashford's fan snaps shut. The entire salon has gone quiet, every eye now on you. Your last comment clearly touched a nerve."

**Crisis:**
> "Flames engulf the upper floor. Children's screams echo from inside. The crowd watches, frozen. The fire brigade is blocks away."

---

## Part 9: Implementation Checkpoints

### Checkpoint 1: Modal UI Foundation (Day 1-2)

**Deliverables:**
- Scene properties (PresentationMode, ProgressionMode) added
- Enums defined and parser updated
- LocationFacade.GetModalSceneAtLocation() implemented
- LocationContent.OnInitializedAsync() checks for modal scenes
- ModalSceneView component created and rendering

**Test:** Tutorial scene displays full-screen on entry

### Checkpoint 2: Modal Progression (Day 2-3)

**Deliverables:**
- Cascade mode: Advances to next situation automatically
- Breathe mode: Returns to menu after each situation
- Scene completion exits modal
- Multiple modal scenes show selection screen

**Test:** Create test scene with 3 situations, verify both cascade and breathe modes work

### Checkpoint 3: Archetype Catalog (Day 3-4)

**Deliverables:**
- 5 archetype definitions in catalog
- Domain enum defined
- PlayerStatType enum defined
- Choice archetype structure defined
- Narrative templates for each archetype

**Test:** Catalog returns archetype by ID, throws on invalid ID

### Checkpoint 4: Archetype Generation (Day 4-5)

**Deliverables:**
- Parser detects archetypeId field
- Parser generates 4 ChoiceTemplates from archetype
- Requirements assigned correctly (stat thresholds)
- Costs assigned correctly (coins, resolve, challenges)
- Narrative template applied with placeholders

**Test:** Create archetype-based scene, verify 4 choices generated with correct structure

### Checkpoint 5: Integration Testing (Day 5-6)

**Deliverables:**
- Mix of modal and atmospheric scenes works
- Mix of archetype and hand-authored situations works
- Tutorial scenes converted to modal + archetype
- Test content demonstrates all patterns
- Locked choices display correctly in modal scenes

**Test:** Play through complete scene sequence, verify progression, requirements, costs all work

### Checkpoint 6: Polish and Documentation (Day 6-7)

**Deliverables:**
- CSS styling for modal scene view
- Loading states and transitions
- Error handling for edge cases
- Content authoring guidelines
- Test coverage for new components

**Test:** User acceptance - does it feel like Sir Brante forced moments?

---

## Part 10: Architecture Constraints and Reminders

### What Must NOT Change

**Three-Tier Timing:**
- Actions still created at query time (Tier 3)
- NOT at scene spawn time
- SceneFacade creates actions when situation activates

**HIGHLANDER Principle:**
- GameWorld.Scenes single source of truth
- No duplicate scene storage
- Scene references by ID only

**Backend Authority:**
- All logic in facades
- UI just displays and collects input
- No game logic in .razor components

**Strong Typing:**
- Use enums (PresentationMode, ProgressionMode, Domain)
- NOT strings that get matched at runtime
- Parser validates enum values at load time

**Fail Fast:**
- Invalid archetype ID → throw at parse time
- Missing required fields → throw at parse time
- No silent failures or defaults for critical data

### What Must Be Preserved

**Existing Scene System:**
- Atmospheric scenes continue working
- Hand-authored situations continue working
- Tutorial content backward compatible
- No breaking changes to existing JSON

**Choice Execution:**
- Same validators (requirement checking)
- Same executors (cost application)
- Same reward system (scene advancement)
- Only navigation flow changes

**Perfect Information:**
- All choices visible (locked + unlocked)
- Lock reasons displayed
- Costs visible before selection
- Player always informed

---

## Conclusion

This document defines a complete transformation of Wayfarer's scene system from browsing-based menus to Sir Brante-style forced moments with learnable mechanical patterns.

**Key Takeaways:**

1. **Modal scenes force engagement** - No browsing, must make choice
2. **Archetypes create learnable patterns** - Five mechanical structures that repeat
3. **Domain associations enable preparation** - Know what stats matter where
4. **Tight feedback loop** - See consequences immediately (locked options)
5. **Freedom at meta-level** - Choose which scene, when to engage
6. **Force at immediate level** - Must respond to situation once in it

**Implementation is two-phase:**
- Phase 1: Modal UI (forced moments)
- Phase 2: Archetypes (learnable patterns)
- Phase 3: AI generation (deferred)

**Success means:**
- Player feels pressure to respond
- Player learns game systems through repetition
- Player prepares strategically for domains
- Player sees "I should have built Diplomacy" when locked out

This creates the Sir Brante experience: informed strategic choices under pressure with immediate visibility of compounding consequences.
