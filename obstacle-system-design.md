# Wayfarer Obstacle System Design Document

## Document Purpose

This document defines the Obstacle system for Wayfarer - the integration layer connecting strategic gameplay (investigations, travel, world exploration) with tactical gameplay (Mental, Physical, and Social challenge systems). This system enables dynamic narrative situations where players choose between multiple approaches to overcome barriers.

## Context: Existing Wayfarer Systems

### Three Tactical Challenge Systems

Wayfarer has three parallel tactical challenge systems, each using the unified 5-stat system (Insight, Rapport, Authority, Diplomacy, Cunning):

**Mental Challenges** - Investigations at locations
- Session model: Pauseable (can leave and return)
- Resources: Progress (builder), Attention (session budget from Focus), Exposure (threshold), Leads (observation flow)
- Actions: ACT (generate Leads) / OBSERVE (follow Leads)
- Properties: Location.InvestigationProfile (Delicate/Obscured/Layered/Time-Sensitive/Resistant)

**Physical Challenges** - Obstacles at locations
- Session model: One-shot (must complete in single attempt)
- Resources: Breakthrough (builder), Exertion (session budget from Stamina), Danger (threshold), Aggression (balance)
- Actions: EXECUTE (lock cards, build Aggression) / ASSESS (trigger combo, exhaust hand)
- Properties: Location.ChallengeType (Combat/Athletics/Finesse/Endurance/Strength)

**Social Challenges** - Conversations with NPCs
- Session model: Session-bounded (real-time conversation with patience limit)
- Resources: Momentum (builder), Initiative (session builder), Doubt (threshold), Cadence (balance), Statements (history)
- Actions: SPEAK (advance conversation) / LISTEN (draw cards)
- Properties: NPC.Personality (affects Doubt accumulation and tactics)

### Goal System Architecture

**Goals** are first-class entities that create action opportunities at locations or NPCs:
- SystemType: Mental/Physical/Social
- DeckId: Which ChallengeDeck to use
- Assignment: npcId OR locationId (mutually exclusive)
- GoalCards: Inline victory conditions (threshold + rewards)
- Requirements: Knowledge, equipment, stats, completed phases (optional)

**Storage:**
- GameWorld.Goals: List<Goal> (all goals, loaded at initialization)
- NPC.ActiveGoals: List<string> (goal IDs currently available at this NPC)
- Location.ActiveGoals: List<string> (goal IDs currently available at this location)

**Two goal lifecycle types:**
- Persistent goals: Added to entities at initialization, never removed, repeatable
- Investigation goals: Referenced in Investigation.Phases, dynamically added during progression, removed on success

### Investigation System

Investigations are multi-phase mysteries with structured progression:
- Each phase defines an obstacle that spawns at specific location/NPC
- Phases can require knowledge, equipment, stats, or completed previous phases
- When phase activates, its obstacle (with all goals) is added to target entity
- Resolving the obstacle progresses investigation to next phase
- Investigation states: unknown → pending → active → completed

### Design Principles

- Elegance over complexity
- Verisimilitude throughout
- Perfect information (no hidden mechanics)
- No soft-locks (always a path forward, just varying costs)
- No string ID matching or brittle references
- Rule-based mechanical systems using strongly typed properties
- Integration through mechanics changing game state

---

## The Problem: Strategic-Tactical Gap

The tactical layer (three challenge systems) and strategic layer (investigations, travel, world exploration) were disconnected. Players needed a way to encounter dynamic narrative situations offering multiple tactical approaches with meaningful consequences.

**Required behaviors:**
- Player encounters bandits → can confront (Physical), negotiate (Social), or outwit (Mental)
- Player needs locked room access → can break in (Physical), bribe guard (Social), or study mechanism (Mental)
- Not all approaches always available
- Past player choices affect which approaches appear
- Sometimes multiple challenges required before resolution
- Different approaches create different world consequences

**Rejected solutions:**
- String ID matching between systems (brittle, not mechanical)
- Status effects with context strings (still string matching)
- Binary gates (creates soft-locks, against design principles)
- Property-based difficulty modifiers (unnecessary complexity through formulas)

**Solution:** Obstacles as first-class entities with universal numerical properties that gate approach availability through simple thresholds.

---

## Core Concept: Obstacles

**Obstacle** = Strategic barrier requiring tactical resolution

Obstacles exist ON routes, locations, and NPCs. They block or complicate progress until overcome through tactical challenges.

### Obstacle Structure

```
Obstacle
{
    string Id;
    string Description; // AI-generated narrative
    
    // Universal properties (0-3 scale)
    int PhysicalDanger;    // Physical approach difficulty
    int SocialDifficulty;  // Social approach difficulty
    int MentalComplexity;  // Mental approach difficulty
    
    bool IsPermanent; // false = removed on resolution, true = persists
    
    List<string> ApproachGoalIds;    // Resolution goals
    List<string> PreparationGoalIds; // Property reduction goals
}
```

### Property Meanings

Each property represents how difficult that type of approach is:

**PhysicalDanger (0-3):**
- 0 = No physical threat, safe passage
- 1 = Low danger, minor risk
- 2 = Moderate danger, significant risk
- 3 = High danger, desperate/impossible

**SocialDifficulty (0-3):**
- 0 = Socially open, no resistance
- 1 = Low difficulty, minor negotiation
- 2 = Moderate difficulty, significant persuasion needed
- 3 = High difficulty, hostile/impossible

**MentalComplexity (0-3):**
- 0 = Mentally simple, obvious solution
- 1 = Low complexity, straightforward analysis
- 2 = Moderate complexity, careful study needed
- 3 = High complexity, extremely obscure/impossible

**Property level 3 = impossible/desperate** - approaches requiring this level are either unavailable or extremely costly.

### Storage and References

```
Investigation
{
    List<InvestigationPhase> Phases;
}

InvestigationPhase
{
    List<string> RequiredObstacleIds; // Must clear these obstacles before phase accessible
    string GoalId; // The phase's main goal
}

Obstacle
{
    string LocationId OR string RouteId OR string NpcId; // Where obstacle exists
    List<string> ApproachGoalIds;
    List<string> PreparationGoalIds;
}

GameWorld
{
    List<Obstacle> Obstacles; // All obstacles in game
    List<Goal> Goals; // All goals in game
}
```

**Hierarchy:** Investigation → Obstacle → Goals

Obstacles are assigned to location/route/NPC via their own properties. Goals target obstacles via TargetObstacleId.

---

## Two Goal Types at Obstacles

### Preparation Goals

**Purpose:** Provide tactical advantage OR reduce obstacle properties

**Two preparation types:**

#### Informational Preparation (Grants Cards)

Actions that give you knowledge or tactical advantage without changing the objective situation.

**Characteristics:**
- Complete tactical challenges
- Grant knowledge that adds cards to specific challenge decks
- Don't modify obstacle properties
- Cards provide tactical advantage equivalent to lower difficulty
- AI can reference card content in narrative

**Effect structure:**
```
InformationalPreparation.Rewards
{
    int Coins;
    List<KnowledgeCardGrant> GrantedCards;
}

KnowledgeCardGrant
{
    string KnowledgeId;           // For player journal
    string Description;           // What player learned
    SystemType TargetSystemType;  // Mental/Physical/Social
    string TargetObstacleId;      // Which obstacle's challenges get this card
    Card GrantedCard;             // Full card definition with contextual text
}
```

**Example:**
"Ask Townsfolk About Miller" (Social challenge):
- Full Social challenge at Town Square
- Grants knowledge card "Mention Miller's Errand"
- Card added to all Social challenges at "Suspicious Gatekeeper" obstacle
- Card: +3 Momentum, -1 Doubt, text: "The townsfolk speak well of the miller"
- Guard's SocialDifficulty unchanged (still suspicious)
- Player draws card during challenge, uses it tactically

#### Structural Preparation (Manipulates Goals)

Actions that objectively change the situation by replacing approaches with better alternatives.

**Characteristics:**
- Complete tactical challenges
- Remove difficult approach goals, add easier ones
- New goals have lower property requirements and/or easier challenges
- Represent physical changes, authority gains, or discovered methods
- AI references goal context to explain what changed

**Effect structure:**
```
StructuralPreparation.Rewards
{
    int Coins;
    GoalManipulation Manipulation;
}

GoalManipulation
{
    string TargetObstacleId;
    List<string> RemoveGoalIds;  // Remove these approach goals
    List<string> AddGoalIds;      // Add these approach goals (already exist in GameWorld)
}
```

**Example:**
"Get Official Seal" (Social challenge at Town Hall):
- Full Social challenge convincing clerk
- Removes: "Negotiate Entry" goal from Gatekeeper obstacle
- Adds: "Show Official Credentials" goal to Gatekeeper obstacle
- New goal has easier challenge (lower Momentum threshold, simpler deck, lower property requirement)
- Objective change: You now have recognized authority
- Does NOT resolve gatekeeper obstacle

---

## Knowledge Card System

### The Verisimilitude Problem

When preparation reduces obstacle properties abstractly, AI narrative loses context:
- Guard conversation difficulty reduced, but guard doesn't know why
- Cards can't reference specific preparation
- Breaks immersion (system knows, narrative doesn't)

### Solution: Knowledge as Tactical Cards

**Informational preparation grants cards** that appear in challenges:
- Card text provides narrative context
- Playing card = using that information tactically
- AI sees card, generates contextual dialogue
- Mechanical advantage through gameplay, not abstract bonuses

**Example flow:**

1. Player completes "Ask Townsfolk" (Social challenge)
2. Grants knowledge card: "Mention Miller's Errand"
   - Text: "The townsfolk speak well of the miller. You're on legitimate business."
   - Effect: +3 Momentum, -1 Doubt
   - Added to: Social challenges at "Suspicious Gatekeeper" obstacle

3. Player enters "Negotiate with Guard" challenge
4. Challenge deck includes standard guard cards PLUS "Mention Miller's Errand"
5. Player draws and plays card
6. AI generates: "Ah, you're going to see the miller? The baker mentioned someone would be coming. Go on through."

### Card Types by Preparation

**Social preparation cards:**
- Conversation topics, reputation, relationships
- Effect: Momentum gains, Doubt reduction
- Example: "Cite Common Friend" (+2 Momentum, -1 Doubt)

**Mental preparation cards:**
- Knowledge, observations, patterns
- Effect: Progress gains, Lead generation
- Example: "Exploit Pattern" (+3 Progress, generate 2 Leads)

**Physical preparation cards:**
- Tactics, weaknesses, equipment usage
- Effect: Breakthrough gains, Danger reduction
- Example: "Strike Weak Point" (+4 Breakthrough, -2 Danger this action)

### Implementation Details

**When creating challenge context:**
1. Check player's knowledge
2. For each knowledge, check if it grants cards to this challenge
3. Add granted cards to challenge deck
4. Player draws from augmented deck

**Card grants are obstacle-specific:**
- Knowledge about bandits helps with bandit challenges
- Knowledge about mill helps with mill challenges
- Knowledge about guard helps with that guard
- No global advantages (maintains specificity)

### Why Two Preparation Types

**Informational (cards):** Tactical advantage you use during challenge
- You learned something useful
- Must draw and play card (gameplay)
- AI sees card content (narrative hook)
- Obstacle and goals unchanged

**Structural (goal manipulation):** Objective situation change
- Something in world actually changed
- Replaces difficult goals with easier ones
- New goals unlock when their requirements met
- AI sees goal context (narrative hook)

**Example distinction:**

"Ask Townsfolk" (Informational):
- Grants card to use in guard conversation
- Guard still suspicious (SocialDifficulty=2)
- "Negotiate Entry" goal unchanged
- You have better talking points

"Get Official Seal" (Structural):
- Removes "Negotiate Entry" goal (difficult)
- Adds "Show Official Credentials" goal (easy)
- New goal requires SocialDifficulty ≤1 OR completed "Get Official Seal"
- You now have authority

---

### Approach Goals

**Purpose:** Resolve the obstacle through tactical challenge

**Characteristics:**
- Are complete tactical challenges (use full Mental/Physical/Social systems)
- Only appear when property requirements met
- Success resolves obstacle based on consequence type
- Can have additional requirements (knowledge, items, stats)

**Availability structure:**
```
ApproachGoal
{
    // Standard goal properties
    SystemType Type;
    string DeckId;
    List<GoalCard> GoalCards;
    
    // Approach-specific
    PropertyRequirements Requirements;
    ConsequenceType Consequence;
}

PropertyRequirements
{
    int MaxPhysicalDanger;   // -1 = no requirement
    int MaxSocialDifficulty; // -1 = no requirement  
    int MaxMentalComplexity; // -1 = no requirement
    List<string> RequiredCompletedGoals; // Must have completed these goals
    
    // Stat requirements (existing system)
    int MinInsight;
    int MinRapport;
    int MinAuthority;
    int MinDiplomacy;
    int MinCunning;
}
```

**Availability check:**
Approach goal is visible and selectable when:
- ALL property requirements met (obstacle properties ≤ required thresholds)
- AND all required goals completed
- AND all stat requirements met

**Example:**
"Tactical Strike" (Physical challenge):
- Requirements: PhysicalDanger ≤ 2, Knowledge "bandit_patrol_patterns"
- Full Physical challenge using location's ChallengeType
- Success resolves obstacle with Remove consequence
- Obstacle deleted from game

---

## Consequence Types

When player completes an approach goal, the obstacle resolves based on consequence type:

### Remove (Permanent Clearance)

**Effect:** Obstacle completely removed from game

**Use cases:**
- Defeated enemies
- Cleared physical barriers
- Destroyed obstacles
- One-time challenges overcome

**Implementation:**
- Remove obstacle ID from parent entity's ObstacleIds list
- Remove all goals associated with this obstacle from ActiveGoals
- Obstacle no longer exists in world

**Examples:**
- "Fight Through Bandits" → Bandits defeated, route safe
- "Clear Rubble" → Passage open permanently
- "Defeat Guard" → Guard no longer blocks access

### Transform (Change Nature)

**Effect:** Obstacle properties change, creating new relationship or ongoing cost

**Use cases:**
- Hostile becomes ally/neutral
- Creates tribute/toll relationship
- Establishes ongoing bargain
- Changes social dynamic

**Implementation:**
- Modify obstacle properties (often set to different values)
- Can add new goals or change existing ones
- Obstacle persists with new configuration
- Often creates repeatable interaction

**Examples:**
- "Negotiate Treaty" → Hostile bandits become toll-collectors (PhysicalDanger=0, SocialDifficulty=1, requires 5 coins per passage)
- "Befriend Guard" → Guard becomes contact (obstacle removed, NPC relationship created)
- "Make Deal" → Criminal becomes informant (Transform to ongoing relationship)

### Bypass (Create Alternative)

**Effect:** Discover alternate method, original obstacle unchanged

**Use cases:**
- Hidden paths around danger
- Secret entrances to locations
- Knowledge of alternate methods
- Workarounds that don't affect others

**Implementation:**
- Original obstacle unchanged
- Create new Route entity (if travel obstacle)
- Add knowledge or capability to player
- Player gains permanent alternative access

**Examples:**
- "Find Hidden Trail" → Creates new route avoiding bandits (bandits still threaten main road)
- "Discover Secret Entrance" → New access point to location (main door still guarded)
- "Learn Lock Mechanism" → Knowledge enables future bypasses (locks of this type now easier)

---

## Player Experience Flow

### 1. Encounter Phase

Player arrives at location/route with obstacle.

**UI displays:**
- Obstacle description (AI-generated narrative)
- Current property values (PhysicalDanger: 3, SocialDifficulty: 2, etc.)
- Available approach goals (meets requirements)
- Locked approach goals (with clear requirement explanation)
- Available preparation goals

**Player sees:**
```
BANDIT CAMP OBSTACLE

Armed bandits have set up camp on the road, demanding tribute from travelers.

PhysicalDanger: 3 (High)
SocialDifficulty: 3 (High)
MentalComplexity: 3 (High)

AVAILABLE APPROACHES:
→ "Force Through" (Physical challenge)
  Desperate assault, 50+ Health risk

LOCKED APPROACHES:
○ "Tactical Strike" (Requires PhysicalDanger ≤ 2)
○ "Negotiate Passage" (Requires SocialDifficulty ≤ 2)
○ "Find Hidden Trail" (Requires MentalComplexity ≤ 2)

PREPARATION:
→ "Scout Enemy Camp" (Mental) - Reduces PhysicalDanger
→ "Contact Leader" (Social) - Reduces SocialDifficulty
→ "Study Terrain" (Mental) - Reduces MentalComplexity
```

### 2. Strategic Decision

Player evaluates options:

**Option A: Commit immediately**
- Select available approach goal
- Enter tactical challenge right now
- Fast but potentially costly (desperate options usually worse)

**Option B: Prepare first**
- Select preparation goal(s)
- Complete tactical challenges to reduce properties
- Takes more time but unlocks better options
- Can prepare multiple aspects or focus on one

**Decision factors:**
- Current resources (Health, Stamina, Focus, Coins)
- Time pressure (deadlines, weather, urgency)
- Stat strengths (which challenge types can I handle well?)
- Desired outcome (quick vs thorough, violent vs peaceful)

### 3. Preparation Execution (Optional)

If player chooses preparation:

**Example: "Scout Enemy Camp"**
- Enter Mental challenge at this location
- Use full Investigation system (ACT/OBSERVE, Progress/Attention/Exposure)
- Victory: Complete goal, gain rewards
- Property reduction: PhysicalDanger 3→2
- Return to obstacle screen with updated properties

**Now sees:**
```
PhysicalDanger: 2 (Moderate) ✓ Changed
SocialDifficulty: 3 (High)
MentalComplexity: 3 (High)

NEWLY AVAILABLE:
→ "Tactical Strike" (Physical challenge)
  Prepared assault, ~30 Health risk
```

Player can prepare more or commit to now-available approach.

### 4. Multiple Preparations

Player can reduce multiple properties:

Complete "Study Terrain" (Mental challenge):
- MentalComplexity 3→2
- New approach unlocked: "Find Hidden Trail"

**Updated options:**
```
AVAILABLE APPROACHES:
→ "Force Through" (desperate, ~50 Health)
→ "Tactical Strike" (prepared, ~30 Health)
→ "Find Hidden Trail" (bypass, ~20 Focus)
```

Player now has meaningful choice between three viable approaches.

### 5. Approach Commitment

Player selects approach goal.

**Example: "Find Hidden Trail"**
- Enter Mental challenge (Investigation system)
- Complete tactical challenge
- Victory: Obstacle resolved with Bypass consequence

### 6. Resolution & Consequences

System executes consequence type:

**For "Find Hidden Trail" (Bypass):**
- Create new Route entity connecting same locations
- New route has different properties (safer, longer)
- Original Bandit Camp obstacle remains on main road
- Player gains permanent knowledge of alternate route

**AI generates contextual narrative:**
"Your careful study of the terrain revealed an old smuggler's path winding through the hills. Overgrown but passable, it avoids the bandit camp entirely. The main road remains under their control, but you've found your way through."

**Player gains:**
- Resolution of immediate barrier
- Permanent world change (new route, relationship, clearance)
- Challenge completion rewards (coins, XP, items from goal cards)
- Stat XP from cards played during challenges
- Knowledge/understanding from experience

---

## Strategic Depth & Player Decisions

### Time vs Resources Trade-off

**Fast approach:**
- Use desperate/available option immediately
- High resource cost (Health, Stamina, Coins)
- Minimal time investment
- Good when: deadline pressure, resources abundant

**Prepared approach:**
- Invest time in preparation challenges
- Lower resource cost for resolution
- Significant time investment
- Good when: resources scarce, time available

### Approach Selection

**Physical approaches:**
- Cost Health/Stamina during challenge
- Violent resolution (often Remove consequences)
- Good for: High combat stats, abundant Health, aggressive playstyle

**Social approaches:**
- Card costs during challenge (often coins)
- Relationship outcomes (often Transform consequences)
- Good for: High social stats, abundant Coins, diplomatic playstyle

**Mental approaches:**
- Cost Focus/Time during challenge
- Discovery outcomes (often Bypass consequences)
- Good for: High mental stats, patient playstyle, exploration focus

### Consequence Planning

**Remove consequences:**
- Permanent clearance
- Violent/destructive
- Benefits everyone (world permanently changed)
- May affect reputation/relationships

**Transform consequences:**
- Creates ongoing relationship
- Often has recurring costs (tolls, tributes)
- Personal benefit (others face original obstacle)
- Builds faction connections

**Bypass consequences:**
- Peaceful resolution
- Doesn't help others (personal knowledge)
- World unchanged for others
- Often reveals hidden content

### Property Reduction Paths

**Focus single property:**
- Unlock one good approach quickly
- Efficient for specific strategy
- Example: Reduce Physical 3→2→1 to get best combat option

**Spread preparation:**
- Unlock multiple approach types
- Maximum flexibility at resolution
- Choose cheapest based on current resources
- Example: Reduce Physical 3→2 AND Mental 3→2 to have options

**Deep preparation:**
- Reduce one property to 0
- Get absolute best approach in that category
- Time-intensive but potentially free resolution
- Example: Reduce Social 3→2→1→0 for free passage via diplomacy

---

## Obstacle Templates

Templates provide interesting decisions while working across situations. Most obstacles use one template, varying which properties and consequences apply.

### Template 1: The Gauntlet

**Pattern:** Single approach type, multiple preparations reduce cost

**Structure:**
- One approach goal available immediately at high property level
- Multiple preparation goals reduce same property
- Each preparation unlocks marginally better resolution
- Final state can reach property 0 (free resolution)

**Example: Flooded River Crossing**
```
Obstacle: Flooded River
PhysicalDanger: 3

Approach Goals:
- "Wade Across" (Physical ≤3, always available, ~60 Stamina cost risk)

Preparation Goals:
- "Find Shallow Point" (Mental) → Danger 3→2
- "Construct Rope Bridge" (Physical) → Danger 2→1  
- "Wait for Waters to Recede" (costs 4 time segments) → Danger 1→0

Resolution: All approaches Remove obstacle (cross successfully)
```

**Decision space:** How much time/effort to invest vs stamina cost? Can I afford to wait for free crossing or need to pay cost now?

**Use cases:** Physical barriers, natural obstacles, terrain challenges

### Template 2: The Fork

**Pattern:** Two approaches immediately available, no preparation

**Structure:**
- Two approach goals available from start
- Different property requirements (both ≤2 or ≤3)
- No preparation goals offered
- Forces immediate choice based on current resources

**Example: Suspicious Gatekeeper**
```
Obstacle: Guarded Gate
PhysicalDanger: 2, SocialDifficulty: 1

Approach Goals:
- "Force Past Guard" (Physical ≤2, ~30 Health risk, Remove)
- "Pay Entry Fee" (Social ≤1, 10-15 Coins cost in cards, Remove)

Preparation Goals: None

Resolution: Both Remove obstacle (gain entry)
```

**Decision space:** Spend Health or Coins? Choose based on current resource availability and stat strengths.

**Use cases:** Simple barriers, resource checks, binary choices

### Template 3: The Lock

**Pattern:** One terrible option available, one good option locked

**Structure:**
- One desperate approach (property ≤3) always available
- One much better approach (property ≤2 or ≤1) locked
- Single preparation unlocks better option
- Clear "prepare or suffer" choice

**Example: Sealed Vault Door**
```
Obstacle: Reinforced Vault
PhysicalDanger: 3, MentalComplexity: 3

Approach Goals:
- "Smash Door" (Physical ≤3, ~50 Health, loud/consequences, Remove)
- "Pick Lock" (Mental ≤2, ~15 Focus, clean, Remove)

Preparation Goals:
- "Study Lock Mechanism" (Mental) → Complexity 3→2

Resolution: Both Remove obstacle (door open)
```

**Decision space:** Accept brutal consequence now or invest time for clean approach?

**Use cases:** Locked doors, secured areas, technical barriers

### Template 4: The Puzzle

**Pattern:** Single approach requires multiple properties reduced

**Structure:**
- One approach goal requires two property types ≤ threshold
- Multiple preparation paths (different properties)
- Player must prepare both aspects
- Choice in which preparations to do

**Example: Ancient Mechanism**
```
Obstacle: Complex Machine
PhysicalDanger: 2, MentalComplexity: 2

Approach Goals:
- "Activate Mechanism" (requires Physical ≤1 AND Mental ≤1, Remove)

Preparation Goals:
- "Clear Debris" (Physical) → Danger 2→1
- "Decode Instructions" (Mental) → Complexity 2→1
- "Find Power Source" (Mental, alternate) → Complexity 2→1

Resolution: Remove obstacle (mechanism activated)
```

**Decision space:** Must prepare both Physical and Mental aspects. Choose which Mental prep to do. Order matters for resource management.

**Use cases:** Complex mechanical obstacles, multi-aspect barriers, investigation gates

### Template 5: The Escalation

**Pattern:** Multiple approaches at graduated property levels, better preparation yields better consequences

**Structure:**
- Several approach goals at different property thresholds
- Lower properties unlock better outcomes (not just easier challenges)
- Deep investment yields superior long-term results
- Consequence types improve with preparation

**Example: Hostile Territory**
```
Obstacle: Enemy Territory
PhysicalDanger: 3, SocialDifficulty: 2

Approach Goals:
- "Fight Through" (Physical ≤3, ~50 Health, Remove bandits)
- "Show Force" (Physical ≤2, ~25 Health, Transform to tribute relationship)
- "Diplomatic Entry" (Social ≤2, ~10 Coins, Transform to neutral)
- "Honored Guest" (Social ≤1, free, Transform to allies)

Preparation Goals:
- "Demonstrate Strength" (Physical) → Danger 3→2
- "Build Reputation" (Social) → Difficulty 2→1
- "Complete Favor" (Physical or Social) → properties reduced

Resolution: Consequence type improves with preparation depth
```

**Decision space:** How much to invest in better long-term consequences? Quick violence vs patient diplomacy vs deep relationship building?

**Use cases:** Faction territories, major social barriers, relationship gates

### Template 6: The Discovery

**Pattern:** Obvious approach + hidden bypass

**Structure:**
- One clear approach available (property ≤2)
- Hidden bypass approach (property ≤2) requires discovery
- Discovery comes from unrelated exploration/conversation
- Bypass typically cheaper/cleaner than obvious approach

**Example: Guarded Checkpoint**
```
Obstacle: Military Checkpoint
SocialDifficulty: 2

Approach Goals:
- "Show Papers" (Social ≤2, ~10 Coins, Remove)
- "Servant's Entrance" (Mental ≤2, ~5 Focus, Bypass) [HIDDEN]

Preparation Goals:
- "Bribe Official" (Social) → Difficulty 2→1
- "Observe Building" (Mental, at different location) → Reveals "Servant's Entrance" approach

Resolution: Show Papers removes checkpoint interaction, Servant's Entrance bypasses entirely
```

**Decision space:** Pay obvious cost or explore elsewhere for alternative? Thoroughness rewarded with discovery.

**Use cases:** Guarded locations, official barriers, locations with secrets

### Template 7: The Transformation

**Pattern:** Force approach vs relationship approach with divergent consequences

**Structure:**
- Two approaches at similar difficulty
- One aggressive (Remove consequence)
- One social (Transform consequence)
- Same immediate result, different long-term outcomes

**Example: Unfriendly Innkeeper**
```
Obstacle: Hostile Proprietor
PhysicalDanger: 2, SocialDifficulty: 2

Approach Goals:
- "Intimidate" (Physical ≤2, Remove, get room but create enemy)
- "Build Rapport" (Social ≤2, Transform to friendly contact, future benefits)

Preparation Goals:
- "Ask Around" (Social) → Difficulty 2→1
- "Demonstrate Capability" (Physical) → Danger 2→1

Resolution: Both grant access, but Intimidate creates enemy, Rapport creates ally
```

**Decision space:** Immediate access vs long-term relationship? Force vs friendship? Impact on future interactions?

**Use cases:** NPC obstacles, social barriers, relationship gates

### Template 8: The Expertise Gate

**Pattern:** Requires specific stat level or alternative preparation

**Structure:**
- One approach requires high stat level (shortcut for specialists)
- Alternative approach available with preparation (accessible to all)
- Preparation takes time but bypasses stat requirement
- Rewards specialization but doesn't lock out generalists

**Example: Scholar's Riddle**
```
Obstacle: Ancient Puzzle
MentalComplexity: 2

Approach Goals:
- "Solve Riddle" (Mental ≤2, requires Insight 4+, free if qualified)
- "Research Answer" (Mental ≤1, ~15 Focus, accessible to all)

Preparation Goals:
- "Study Ancient Texts" (Mental) → Complexity 2→1

Resolution: Both Remove obstacle (puzzle solved)
```

**Decision space:** Use high stats as shortcut or prepare to bypass requirement? Specialists rewarded but others not blocked.

**Use cases:** Knowledge gates, skill checks, specialist shortcuts

### Template 9: The Urgent Choice

**Pattern:** Fast dangerous option vs slow safe option

**Structure:**
- Two approaches with time/safety trade-off
- Fast approach costs more resources, no time
- Slow approach costs fewer resources, significant time
- Deadline pressure drives decision

**Example: Collapsing Structure**
```
Obstacle: Unstable Building
PhysicalDanger: 3, MentalComplexity: 2

Approach Goals:
- "Sprint Through" (Physical ≤3, ~40 Health risk, 0 time cost, Remove)
- "Navigate Safely" (Mental ≤2, ~20 Focus, 2 time segments, Remove)

Preparation Goals: None (time pressure prevents preparation)

Resolution: Both Remove obstacle (traversal complete)
```

**Decision space:** Speed vs safety based on deadline urgency and current Health/Focus levels.

**Use cases:** Time-sensitive obstacles, emergency situations, deadline pressure

### Template 10: The Staged Reveal

**Pattern:** Completing approaches unlocks new preparations with deeper outcomes

**Structure:**
- Initial approach available
- Completing first approach or preparation reveals new options
- Each stage unlocks deeper investment opportunities
- Final stage yields superior outcome but requires significant investment

**Example: Criminal Network**
```
Obstacle: Underground Organization
SocialDifficulty: 3

Approach Goals:
- "Prove Yourself" (Physical ≤3, violent entry, Remove)
- "Negotiate Entry" (Social ≤2, paid entry, Transform to contact) [Locked initially]
- "Invited Guest" (Social ≤1, friendly entry, Transform to allies) [Locked initially]

Preparation Goals:
- "Find Contact" (Social) → Difficulty 3→2, reveals "Negotiate Entry"
- "Complete Favor" (Physical, unlocked after Find Contact) → Difficulty 2→1, reveals "Invited Guest"

Resolution: Remove vs Transform to contact vs Transform to allies (each better than previous)
```

**Decision space:** How deep into relationship investment? Quick violence vs progressive social escalation? Time investment vs outcome quality?

**Use cases:** Faction progression, complex social structures, relationship depth systems

---

## Integration with Existing Systems

### Investigation System

**Investigations structure phases as obstacles:**

Each investigation phase IS an obstacle that gets spawned:

```
Investigation: "Mill Mystery"

Phase 1 (Obstacle at Mill Exterior):
  - PhysicalDanger=0, MentalComplexity=2
  - Approach: "Observe Mill Exterior" (Mental, Remove)
  
Phase 2 (Obstacle at Mill Owner NPC):
  - SocialDifficulty=2
  - Requirements: Phase 1 resolved
  - Approach: "Question Mill Owner" (Social, Remove)
  
Phase 3 (Obstacle at Mill Basement):
  - PhysicalDanger=2, MentalComplexity=1
  - Requirements: Phase 2 resolved
  - Approaches: "Clear Rubble" (Physical, Remove) OR "Find Alternate" (Mental, Bypass)
  
Phase 4 (Obstacle at Mill Basement):
  - MentalComplexity=3
  - Requirements: Phase 3 resolved
  - Approach: "Investigate Basement" (Mental, Remove)
```

**Phase activation:**
- Investigation phase becomes active (requirements met)
- Phase's obstacle added to target Location.ObstacleIds or NPC.ObstacleIds
- Obstacle's goals added to entity.ActiveGoals
- Player sees new obstacle with its approaches/preparations

**Phase completion:**
- Player resolves obstacle via approach goal
- Obstacle removed (if consequence = Remove/Bypass) or transformed
- Investigation checks: is this phase's obstacle resolved?
- If yes, next phase activates (if requirements met)

**Player experience:**
- Investigation progresses by resolving obstacles
- Each phase presents new obstacle with choices
- Investigation is chain of obstacles, not separate system

### Travel System

**Routes accumulate obstacles dynamically:**

Routes are paths between locations that can have multiple obstacles:

```
Route: "North Road to Farmstead"
ObstacleIds: ["bandit_camp", "flooded_crossing"]
```

Player attempting travel sees:
- Route base properties (distance, base time)
- All obstacles on route with their properties
- Can attempt travel by resolving obstacles in sequence
- Or find alternate routes (Bypass consequences create new routes)

**Dynamic obstacle addition:**
- Quest creates obstacle on route (bandit raid)
- Weather system adds temporary obstacle (flooding)
- Investigation reveals obstacle (hidden danger)
- World events spawn obstacles (patrols, blockades)

### Goals and Challenge Systems

**Preparation and Approach goals are standard goals:**

Both goal types use existing goal architecture:
- Have SystemType (Mental/Physical/Social)
- Reference ChallengeDeck
- Contain GoalCards with thresholds and rewards
- Use full tactical challenge systems

**The ONLY differences:**

Preparation goals:
- Rewards include ObstaclePropertyReduction
- Don't remove themselves from ActiveGoals on completion (unless investigation goal)

Approach goals:
- Have PropertyRequirements for visibility
- Have ConsequenceType for resolution behavior
- Remove themselves and related goals on completion

**Challenge execution is identical:**
- Player selects goal from Location/NPC
- Facade creates challenge context
- Player enters tactical challenge (Mental/Physical/Social system)
- Challenge resolves normally
- Additional obstacle logic executes based on goal type

### Location and NPC Properties

**Obstacles respect entity properties:**

When obstacle spawns at Location:
- Physical challenges use Location.ChallengeType
- Mental challenges use Location.InvestigationProfile

When obstacle involves NPC:
- Social challenges use NPC.Personality

**Example:**
Obstacle "Suspicious Guard" at Mill location:
- Mill has ChallengeType = Finesse (careful, precise interactions)
- Physical approach "Subdue Guard" uses Finesse challenge type
- Social approach "Persuade Guard" uses Guard NPC's personality

Properties ensure challenges feel appropriate to context.

---

## AI Content Generation

### Templates as Mechanical Frameworks

AI generates obstacle content within template constraints:

**Input to AI:**
- Template type (Gauntlet, Fork, Lock, etc.)
- Property distribution (which properties at what levels)
- Consequence types for approaches
- Context (location, investigation phase, quest, world event)

**AI generates:**
- Obstacle description (narrative flavor)
- Goal names and descriptions
- Challenge deck selections (appropriate to context)
- Knowledge/item names for requirements
- Reward details (coins, items, narrative consequences)

**AI cannot:**
- Change property mechanics
- Modify template structure
- Create approaches with invalid requirements
- Violate verisimilitude rules

### Example AI Generation

**Template: The Lock**
**Context: Investigation Phase 3 at Ancient Ruins**
**Property Distribution: PhysicalDanger=3, MentalComplexity=3**

AI generates:
```
Obstacle: "Sealed Chamber"
Description: "Heavy stone doors sealed with an ancient mechanism block the inner sanctum. The seals are intact but the chamber beyond shows signs of activity."

Approach Goals:
- "Force the Seals" (Physical ≤3)
  Description: "Breach the stone doors through brute force, risking damage to archaeological evidence."
  Challenge: Physical/Strength at Ancient Ruins
  Consequence: Remove
  
- "Decode Mechanism" (Mental ≤2)
  Description: "Study the locking mechanism to open the chamber without damage."
  Challenge: Mental/Resistant at Ancient Ruins  
  Consequence: Remove
  
Preparation Goals:
- "Study Seal Patterns" (Mental)
  Description: "Examine the geometric patterns on the seal for clues to its operation."
  Challenge: Mental/Layered at Ancient Ruins
  Effect: MentalComplexity 3→2
  Rewards: Knowledge "seal_geometry_understood", 10 coins
```

### Content Validation

All AI-generated obstacles validated at load time:
- Property values in valid range (0-3)
- Goal IDs reference existing goals
- Requirements reference existing knowledge/items/locations
- Consequence types are valid enum values
- Parent entity (Location/Route/NPC) exists

Parse failures are loud and immediate, not silent degradation.

---

## Implementation Notes

### Data Structure Summary

**Investigation entity:**
```
Investigation
{
    string Id;
    InvestigationState State; // Unknown, Pending, Active, Completed
    List<InvestigationPhase> Phases;
}

InvestigationPhase
{
    string ObstacleId;      // Which obstacle defines this phase
    string TargetLocationId; // OR
    string TargetNpcId;
    List<string> Requirements; // Knowledge, items, completed phases
}
```

**Obstacle entity:**
```
Obstacle
{
    string Id;
    string Description;
    int PhysicalDanger;      // 0-3
    int SocialDifficulty;    // 0-3
    int MentalComplexity;    // 0-3
    bool IsPermanent;
    List<string> ApproachGoalIds;
    List<string> PreparationGoalIds;
}
```

**Goal extensions:**
```
Goal
{
    // Existing properties
    SystemType Type;
    string DeckId;
    string NpcId;           // OR
    string LocationId;
    List<GoalCard> GoalCards;
    
    // NEW for obstacle system
    string TargetObstacleId;        // Which obstacle this affects (both types)
    PropertyRequirements Requirements; // Approach goals only
    ConsequenceType Consequence;       // Approach goals only
}

PropertyRequirements
{
    int MaxPhysicalDanger;   // -1 = no requirement
    int MaxSocialDifficulty;
    int MaxMentalComplexity;
    List<string> RequiredKnowledgeIds;
    List<string> RequiredItemIds;
    // Stat requirements (existing system)
}

enum ConsequenceType
{
    Remove,
    Transform,
    Bypass
}
```

**Goal rewards extension:**
```
GoalCard.Rewards
{
    int Coins;
    GoalManipulation Manipulation;            // Structural preparation only
    List<KnowledgeCardGrant> GrantedCards;   // Informational preparation only
}

GoalManipulation
{
    string TargetObstacleId;
    List<string> RemoveGoalIds;  // Remove these approach goals from obstacle
    List<string> AddGoalIds;      // Add these approach goals to obstacle
}

KnowledgeCardGrant
{
    string KnowledgeId;           // For player journal reference
    string Description;           // What player learned
    SystemType TargetSystemType;  // Mental/Physical/Social
    string TargetObstacleId;      // Which obstacle's challenges get this card
    Card GrantedCard;             // Full card with contextual text and effects
}

Card
{
    string Id;
    string Name;
    string Text;                  // Contextual description referencing preparation
    StatType BoundStat;           // Insight/Rapport/Authority/Diplomacy/Cunning
    int Depth;                    // Card depth (1-10)
    CardEffect Effect;            // Momentum/Progress/Breakthrough gains, resource costs
}
```

**Entity extensions:**
```
Location
{
    List<string> ObstacleIds; // NEW
    // Existing: ActiveGoals, InvestigationProfile, ChallengeType, etc.
}

Route
{
    List<string> ObstacleIds; // NEW
    // Existing: Origin, Destination, etc.
}

NPC
{
    List<string> ObstacleIds; // NEW
    // Existing: ActiveGoals, Personality, etc.
}
```

### Facade Responsibilities

**ObstacleFacade (new):**
- GetObstaclesAtLocation(locationId) → List<Obstacle>
- GetObstaclesAtRoute(routeId) → List<Obstacle>
- GetObstaclesAtNPC(npcId) → List<Obstacle>
- GetAvailableApproaches(obstacleId, player) → List<Goal> (filtered by requirements)
- GetPreparations(obstacleId) → List<Goal>
- ApplyInformationalPrep(goalId, obstacleId) → add granted cards to player knowledge
- ApplyStructuralPrep(goalId, obstacleId) → remove/add goals per manipulation
- ResolveObstacle(goalId, obstacleId, consequenceType) → execute consequence
- CreateObstacle(template, properties, location/route/npc) → spawn new obstacle

**InvestigationFacade:**
- ActivatePhase(investigationId, phaseIndex) → spawn phase obstacle at target entity
- CheckPhaseCompletion(investigationId, phaseIndex) → is obstacle resolved?
- ProgressInvestigation(investigationId) → check next phase requirements, activate if met

**GameFacade integration:**
- When player completes goal, check if it targets obstacle
- If informational prep: apply card grants to player
- If structural prep: manipulate obstacle goals
- If approach goal: execute consequence, check investigation progression

### Consequence Execution

**Remove:**
1. Remove obstacle.Id from parent entity.ObstacleIds
2. Remove all obstacle's goals from entity.ActiveGoals
3. Remove obstacle from GameWorld.Obstacles (if not persistent)
4. Trigger AI narrative generation
5. Update UI

**Transform:**
1. Modify obstacle properties per goal definition
2. Can change ApproachGoalIds/PreparationGoalIds
3. Can modify IsPermanent flag
4. Obstacle persists with new configuration
5. Trigger AI narrative generation
6. Update UI

**Bypass:**
1. Create new Route (if travel obstacle) with different properties
2. OR grant Knowledge enabling alternate access
3. OR add new goal to different location
4. Original obstacle unchanged
5. Trigger AI narrative generation
6. Update UI

### UI Requirements

**Obstacle overview screen:**
- Shows obstacle description
- Displays all three properties with visual indicators
- Lists available approaches (clickable to enter challenge)
- Lists locked approaches with clear requirement text
- Lists preparation options (clickable to enter challenge)
- Shows which property each preparation reduces

**Property change feedback:**
- Animate property value changes after preparation
- Highlight newly unlocked approaches
- Show player what changed and why

**Approach selection:**
- Clear indication of challenge type (Mental/Physical/Social)
- Resource cost estimates (based on property levels)
- Consequence type indication (Remove/Transform/Bypass)
- One-click commitment to approach

**No separate "clear obstacle" button** - selecting approach enters challenge, completing challenge resolves obstacle.

---

## Examples: Complete Obstacle Definitions

### Example 1: Simple Fork (Template 2)

```json
{
  "id": "town_gate_toll",
  "description": "A guard collects tolls at the town gate. You can pay the fee or attempt to bypass.",
  "physicalDanger": 0,
  "socialDifficulty": 1,
  "mentalComplexity": 0,
  "isPermanent": true,
  "approachGoalIds": ["pay_toll", "sneak_past"],
  "preparationGoalIds": []
}

Goals:
{
  "id": "pay_toll",
  "systemType": "Social",
  "deckId": "simple_negotiation",
  "targetObstacleId": "town_gate_toll",
  "requirements": {
    "maxSocialDifficulty": 1
  },
  "consequence": "Remove",
  "goalCards": [
    {
      "threshold": 8,
      "rewards": {
        "coins": -5
      }
    }
  ]
}

{
  "id": "sneak_past",
  "systemType": "Physical",
  "deckId": "finesse_challenge",
  "targetObstacleId": "town_gate_toll",
  "requirements": {
    "maxPhysicalDanger": 0
  },
  "consequence": "Bypass",
  "goalCards": [
    {
      "threshold": 10,
      "rewards": {
        "coins": 5
      }
    }
  ]
}
```

### Example 2: Complex Escalation (Template 5)

```json
{
  "id": "bandit_roadblock",
  "description": "Armed bandits control this stretch of road, demanding tribute from travelers.",
  "physicalDanger": 3,
  "socialDifficulty": 2,
  "mentalComplexity": 3,
  "isPermanent": false,
  "approachGoalIds": [
    "force_through_bandits",
    "tactical_strike",
    "negotiate_passage",
    "honored_passage"
  ],
  "preparationGoalIds": [
    "scout_bandit_camp",
    "sabotage_supplies",
    "contact_leader",
    "complete_favor"
  ]
}

Approach Goals:

{
  "id": "force_through_bandits",
  "systemType": "Physical",
  "deckId": "combat_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {
    "maxPhysicalDanger": 3
  },
  "consequence": "Remove",
  "description": "Fight through the bandit camp with overwhelming force.",
  "goalCards": [
    {
      "threshold": 15,
      "rewards": {
        "coins": 10,
        "reputation": "violent_reputation"
      }
    }
  ]
}

{
  "id": "tactical_strike",
  "systemType": "Physical",
  "deckId": "combat_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {
    "maxPhysicalDanger": 2,
    "requiredKnowledgeIds": ["bandit_patrol_patterns"]
  },
  "consequence": "Remove",
  "description": "Use scouted information to strike effectively.",
  "goalCards": [
    {
      "threshold": 12,
      "rewards": {
        "coins": 15
      }
    }
  ]
}

{
  "id": "negotiate_passage",
  "systemType": "Social",
  "deckId": "negotiation_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {
    "maxSocialDifficulty": 2,
    "requiredKnowledgeIds": ["bandit_leader_contact"]
  },
  "consequence": "Transform",
  "description": "Negotiate ongoing passage rights with the bandit leader.",
  "transformEffect": {
    "physicalDanger": 0,
    "socialDifficulty": 1,
    "newDescription": "The bandits now expect 5 coins tribute per passage."
  },
  "goalCards": [
    {
      "threshold": 10,
      "rewards": {
        "coins": 5
      }
    }
  ]
}

{
  "id": "honored_passage",
  "systemType": "Social",
  "deckId": "negotiation_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {
    "maxSocialDifficulty": 1,
    "requiredKnowledgeIds": ["bandit_favor_complete"]
  },
  "consequence": "Transform",
  "description": "Earn the bandits' respect through service.",
  "transformEffect": {
    "physicalDanger": 0,
    "socialDifficulty": 0,
    "newDescription": "The bandits now see you as an ally and allow free passage."
  },
  "goalCards": [
    {
      "threshold": 8,
      "rewards": {
        "coins": 10,
        "relationship": "bandit_alliance"
      }
    }
  ]
}

Preparation Goals:

{
  "id": "scout_bandit_camp",
  "systemType": "Mental",
  "deckId": "investigation_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {},
  "description": "Observe the bandit camp to learn their patterns and weaknesses.",
  "goalCards": [
    {
      "threshold": 10,
      "rewards": {
        "coins": 5,
        "grantedCards": [
          {
            "knowledgeId": "bandit_patterns",
            "description": "You've studied their patrol patterns and shift changes",
            "targetSystemType": "Physical",
            "targetObstacleId": "bandit_roadblock",
            "grantedCard": {
              "id": "strike_during_shift_change",
              "name": "Strike During Shift Change",
              "text": "You know when the guards are distracted during patrol rotation",
              "boundStat": "Cunning",
              "depth": 3,
              "effect": {
                "breakthrough": 4,
                "dangerReduction": 2
              }
            }
          }
        ]
      }
    }
  ]
}

{
  "id": "sabotage_supplies",
  "systemType": "Physical",
  "deckId": "finesse_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {
    "requiredCompletedGoals": ["scout_bandit_camp"]
  },
  "description": "Sneak into camp and sabotage their supplies.",
  "goalCards": [
    {
      "threshold": 12,
      "rewards": {
        "coins": 10,
        "manipulation": {
          "targetObstacleId": "bandit_roadblock",
          "removeGoalIds": ["force_through_bandits"],
          "addGoalIds": ["tactical_strike"]
        }
      }
    }
  ]
}

{
  "id": "contact_leader",
  "systemType": "Social",
  "deckId": "conversation_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {},
  "description": "Find a way to meet with the bandit leader.",
  "goalCards": [
    {
      "threshold": 10,
      "rewards": {
        "grantedCards": [
          {
            "knowledgeId": "leader_acquaintance",
            "description": "You've made contact with the bandit leader",
            "targetSystemType": "Social",
            "targetObstacleId": "bandit_roadblock",
            "grantedCard": {
              "id": "reference_meeting",
              "name": "Reference Your Meeting",
              "text": "You spoke with their leader. They know you're not just another traveler.",
              "boundStat": "Authority",
              "depth": 2,
              "effect": {
                "momentum": 3,
                "doubtReduction": 2
              }
            }
          }
        ]
      }
    }
  ]
}

{
  "id": "complete_favor",
  "systemType": "Physical",
  "deckId": "athletics_challenge",
  "targetObstacleId": "bandit_roadblock",
  "requirements": {
    "requiredCompletedGoals": ["contact_leader"]
  },
  "description": "Complete a task for the bandits to earn their trust.",
  "goalCards": [
    {
      "threshold": 15,
      "rewards": {
        "coins": 20,
        "manipulation": {
          "targetObstacleId": "bandit_roadblock",
          "removeGoalIds": ["negotiate_passage"],
          "addGoalIds": ["honored_passage"]
        }
      }
    }
  ]
}
```

### Example 3: Investigation Phase Gate (Template 4 variant)

```json
{
  "id": "collapsed_passage",
  "description": "A cave-in has blocked the passage to the mill basement. The rubble looks recent.",
  "physicalDanger": 2,
  "socialDifficulty": 0,
  "mentalComplexity": 1,
  "isPermanent": false,
  "approachGoalIds": ["clear_rubble", "find_alternate"],
  "preparationGoalIds": ["assess_stability"]
}

Goals:

{
  "id": "clear_rubble",
  "systemType": "Physical",
  "deckId": "strength_challenge",
  "targetObstacleId": "collapsed_passage",
  "requirements": {
    "maxPhysicalDanger": 2
  },
  "consequence": "Remove",
  "description": "Clear the rubble blocking the passage.",
  "goalCards": [
    {
      "threshold": 12,
      "rewards": {
        "coins": 5
      }
    }
  ]
}

{
  "id": "find_alternate",
  "systemType": "Mental",
  "deckId": "investigation_challenge",
  "targetObstacleId": "collapsed_passage",
  "requirements": {
    "maxMentalComplexity": 1
  },
  "consequence": "Bypass",
  "description": "Search for another way into the basement.",
  "goalCards": [
    {
      "threshold": 10,
      "rewards": {
        "grantKnowledgeIds": ["mill_secret_entrance"]
      }
    }
  ]
}

{
  "id": "assess_stability",
  "systemType": "Mental",
  "deckId": "investigation_challenge",
  "targetObstacleId": "collapsed_passage",
  "requirements": {},
  "description": "Study the collapse to understand safe clearing methods.",
  "goalCards": [
    {
      "threshold": 8,
      "rewards": {
        "reduction": {
          "targetObstacleId": "collapsed_passage",
          "reducePhysicalDanger": 1
        }
      }
    }
  ]
}
```

---

## Design Philosophy Summary

The obstacle system achieves integration through:

**Mechanical simplicity:**
- Three universal properties (0-3 scale)
- Simple threshold checks (property ≤ requirement)
- No formulas, no complex interactions
- Rules-based gating, not string matching

**Player agency:**
- Multiple valid approaches
- Preparation is optional, not mandatory
- Choose between time and resources
- Consequences reflect approach choices

**Verisimilitude:**
- Properties have natural world meaning
- Different approaches feel authentic
- Consequences match narrative logic
- World changes persist meaningfully

**Emergent complexity:**
- Simple rules create rich decision spaces
- Template variations provide diversity
- Multiple obstacles compose naturally
- Player expression through choice patterns

**Integration elegance:**
- Uses existing challenge systems unchanged
- Extends goal architecture minimally
- Respects entity properties (profiles, personalities)
- AI generates within mechanical constraints

The system creates 80 Days-style approach selection (clear options, meaningful trade-offs, simple commitment) while maintaining Wayfarer's mechanical depth and verisimilitude requirements.
