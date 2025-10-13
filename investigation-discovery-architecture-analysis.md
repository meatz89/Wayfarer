# Investigation Discovery Architecture - From Cookie Clicker to Strategic Design

**Context**: This document chronicles the evolution from a fundamentally flawed investigation discovery system to the correct architectural pattern using GoalCard rewards.

**Date**: 2025-10-13
**Status**: Architectural Analysis & Learning Document

---

## Part 1: The Wrong Understanding - GoalCompletionTrigger

### What I Implemented (WRONG APPROACH)

I implemented an investigation discovery system triggered by goal completion:

**DiscoveryTriggerType.cs** - Added new enum:
```csharp
/// <summary>
/// Investigation revealed after completing specific goal
/// Prerequisites: goal_completed(X)
/// PROPER ARCHITECTURE: Checks actual game state (goal completion), not invisible knowledge tokens
/// </summary>
GoalCompletionTrigger
```

**Investigation.cs** - Added prerequisite property:
```csharp
/// <summary>
/// Required completed goal ID (GoalCompletionTrigger)
/// Checks actual game state - goal must be marked complete in GameWorld.Goals
/// PROPER ARCHITECTURE: Direct state checking, not invisible knowledge tokens
/// </summary>
public string CompletedGoalId { get; set; }
```

**InvestigationDiscoveryEvaluator.cs** - Added checking logic:
```csharp
private bool CheckGoalCompletionTrigger(InvestigationPrerequisites prereqs)
{
    // Check if required goal is completed
    if (!string.IsNullOrEmpty(prereqs.CompletedGoalId))
    {
        if (_gameWorld.Goals.TryGetValue(prereqs.CompletedGoalId, out Goal goal))
        {
            return goal.IsCompleted;  // Boolean gate check
        }
        return false;
    }
    return true;
}
```

**13_investigations.json** - Updated to use trigger:
```json
"intro": {
  "triggerType": "GoalCompletionTrigger",
  "triggerPrerequisites": {
    "completedGoalId": "martha_gather_information"
  },
  "actionText": "Search for safe entry to the mill",
  "locationId": "courtyard",
  "introNarrative": "Martha's story about her daughter haunts you..."
}
```

### What I Thought This Solved

I believed this was better than knowledge token string matching because:
- Checks actual game state (goal completion) instead of invisible knowledge tokens
- Type-safe lookup in GameWorld.Goals dictionary
- Direct state checking rather than fragile string ID matching
- Clearer architectural intent (goal leads to investigation)

### The Fundamental Flaw

**This is still Cookie Clicker design.**

The system creates a LINEAR PROGRESSION with BOOLEAN GATES:
1. Complete Goal A → unlock Investigation B
2. No resource cost
3. No strategic tension
4. No opportunity cost
5. No impossible choices

It's just moving the string matching problem to a different location. The real issue isn't STRING MATCHING vs TYPE-SAFE LOOKUPS - it's BOOLEAN GATES vs RESOURCE COMPETITION.

---

## Part 2: Why It Was Bad Game Design

### Cookie Clicker Pattern

**Cookie Clicker progression model:**
- Click cookies → gain cookies → unlock building → click more cookies → unlock next building
- Linear progression with boolean gates: "Have X? Unlock Y."
- No strategic depth, no trade-offs, just time investment

**My GoalCompletionTrigger was identical:**
- Complete goal → gain completion flag → unlock investigation → complete next goal
- Linear progression with boolean gates: "Goal completed? Unlock investigation."
- No strategic depth, no trade-offs, just playing through the chain

### What's Missing: Inter-Systemic Rules & Shared Resources

**The core principle I violated:**

> "The solution is INTER-SYSTEMIC RULES and SHARED RESOURCES."

**Why?**

Because meaningful strategy requires:
1. **Resource Competition**: Multiple systems competing for limited shared resources
2. **Opportunity Cost**: Resources spent on A cannot be spent on B
3. **Impossible Choices**: Multiple valid paths with genuine trade-offs
4. **Strategic Tension**: No optimal solution, only contextual decisions

**My boolean gate system had NONE of these:**
- No resources consumed during discovery
- No opportunity cost (completing goal costs nothing extra)
- No competing choices (just "do the thing" or "don't do the thing")
- No strategic tension (obvious correct path: complete the goal)

### The Example That Broke My Understanding

**Elena POC shows the correct pattern:**

Martha's conversation has THREE GoalCards with MOMENTUM THRESHOLDS:

```
GoalCard 1: "Surface Conversation" (Threshold: 2 Momentum)
- Minimal depth, efficient time usage
- Reward: Basic knowledge about the mill

GoalCard 2: "Deeper Connection" (Threshold: 6 Momentum)
- Moderate depth, more time investment
- Reward: Reveals daughter's disappearance story + DISCOVER INVESTIGATION

GoalCard 3: "Trust Earned" (Threshold: 10 Momentum)
- Maximum depth, heavy time investment
- Reward: Full backstory + emotional connection + reputation gain
```

**The strategic tension:**
- **Scenario 1**: Player has 8 segments remaining, multiple other goals urgent
  - Push to 6 Momentum → discover investigation → move on (efficient)
  - Cost: 3-4 segments, moderate Focus
  - Trade-off: Miss deeper narrative and reputation, but save time for other goals

- **Scenario 2**: Player has abundant time, wants full narrative
  - Push to 10 Momentum → full backstory + reputation
  - Cost: 5-6 segments, significant Focus
  - Trade-off: Heavy time investment, but maximum relationship depth

**This creates IMPOSSIBLE CHOICE:**
- Both paths are valid
- No optimal solution (context-dependent)
- Resources (Time/Focus) compete with other urgent goals
- Player must make STRATEGIC DECISION based on current game state

**My GoalCompletionTrigger had ZERO of this depth.**

---

## Part 3: The Correct Design - GoalCard Reward System

### Core Architecture

**Investigation discovery happens IN-CHALLENGE through GoalCard rewards:**

```
Martha Conversation Goal
└─ GoalCard 1: "Surface Talk" (2 Momentum)
   └─ Rewards: { knowledge, progress }
└─ GoalCard 2: "Deep Talk" (6 Momentum)
   └─ Rewards: { knowledge, progress, discoverInvestigation: "mill_mystery" }
└─ GoalCard 3: "Trust Earned" (10 Momentum)
   └─ Rewards: { knowledge, progress, reputation }
```

**Discovery happens when player:**
1. Plays conversation cards to build Momentum
2. Reaches 6+ Momentum threshold
3. GoalCard 2 becomes playable
4. Player plays GoalCard 2 to complete goal
5. GoalCard reward includes `discoverInvestigation: "mill_mystery"`
6. Investigation moves from Potential → Discovered
7. Modal appears: "Begin Investigation: The Waterwheel Mystery"

### Why This Is Correct

**1. Resource Competition**
- Building Momentum costs Time (segments) and Focus (mental challenge cards)
- Time spent on Martha conversation cannot be spent on delivery obligations
- Focus spent pushing for discovery cannot be spent on other mental challenges

**2. Opportunity Cost**
- Reaching 6 Momentum costs ~3-4 segments
- Those segments could be spent: completing deliveries, exploring other locations, pursuing other goals
- Player must decide: "Is discovering this investigation worth 3-4 segments RIGHT NOW?"

**3. Impossible Choices**
- Stop at 2 Momentum (efficient, minimal depth)
- Push to 6 Momentum (discover investigation, moderate depth)
- Push to 10 Momentum (full narrative, maximum depth)
- No "correct" answer - depends on remaining time, competing obligations, player priorities

**4. Strategic Tension**
- All three challenge systems (Physical/Mental/Social) compete for player's limited Time
- Martha conversation competes with: delivery obligations, mill exploration, other NPC goals
- Player must prioritize based on current game state

**5. Perfect Information**
- Player sees momentum thresholds on GoalCards
- Player sees time cost (segments consumed)
- Player sees competing goals and remaining time
- Player can calculate: "Do I have resources to push for discovery?"
- No hidden gates, no surprise unlocks, no arbitrary requirements

### Technical Implementation

**GoalCardRewards.cs** (needs implementation):
```csharp
public class GoalCardRewards
{
    public int Progress { get; set; }
    public int Momentum { get; set; }
    public List<string> KnowledgeGranted { get; set; }
    public Dictionary<string, int> ReputationChanges { get; set; }

    // NEW: Investigation discovery reward
    public string DiscoverInvestigation { get; set; }
}
```

**GoalCompletionHandler.cs** (needs update):
```csharp
private async Task ApplyGoalCardRewards(GoalCardRewards rewards, Player player)
{
    // Existing reward application...

    // NEW: Investigation discovery
    if (!string.IsNullOrEmpty(rewards.DiscoverInvestigation))
    {
        Investigation investigation = _gameWorld.Investigations
            .FirstOrDefault(i => i.Id == rewards.DiscoverInvestigation);

        if (investigation != null)
        {
            _gameWorld.InvestigationJournal.DiscoverInvestigation(investigation.Id);

            // Prepare modal display
            InvestigationIntroResult result = new InvestigationIntroResult
            {
                InvestigationId = investigation.Id,
                InvestigationName = investigation.Name,
                IntroNarrative = investigation.IntroAction.IntroNarrative,
                IntroActionText = investigation.IntroAction.ActionText,
                LocationName = GetLocationName(investigation.IntroAction.LocationId),
                SpotName = GetSpotName(investigation.IntroAction.LocationId)
            };

            _gameWorld.InvestigationActivity.PendingIntroResult = result;
        }
    }
}
```

---

## Part 4: The POC Example - How Martha's Conversation Works

### Narrative Flow

**Player enters Market → sees Martha (NPC Goal available)**

**Player clicks Martha → enters Social Challenge:**
- 3 conversation cards available
- Each card costs time (1 segment) and builds momentum
- GoalCards display with momentum thresholds (2, 6, 10)

**Scenario 1: Efficient Path (Stop at 2 Momentum)**
- Player plays 2 basic conversation cards
- Reaches 2 Momentum threshold
- Plays GoalCard 1: "Surface Conversation"
- Reward: Basic knowledge about mill, no investigation discovery
- Exit conversation, move to next goal
- **Time cost: 2 segments** | **Discovery: NO**

**Scenario 2: Discovery Path (Push to 6 Momentum)**
- Player plays 5-6 conversation cards (some deeper, some basic)
- Reaches 6 Momentum threshold
- Plays GoalCard 2: "Deeper Connection"
- Reward: Daughter's disappearance story + **DISCOVER INVESTIGATION: mill_mystery**
- Modal appears: "Begin Investigation: The Waterwheel Mystery"
- Investigation now visible in player's Investigation Journal
- **Time cost: 3-4 segments** | **Discovery: YES**

**Scenario 3: Full Depth Path (Push to 10 Momentum)**
- Player plays 8-10 conversation cards (many deeper options)
- Reaches 10 Momentum threshold
- Plays GoalCard 3: "Trust Earned"
- Reward: Full backstory + emotional connection + reputation +3 with Martha
- Investigation already discovered at 6 Momentum
- **Time cost: 5-6 segments** | **Discovery: YES + MAX NARRATIVE**

### The Strategic Decision

**Context: Player has 8 segments remaining, 2 delivery obligations due**

**Option A: Efficient (2 Momentum)**
- Cost: 2 segments
- Leaves 6 segments for deliveries
- Discovery: No
- Safely complete deliveries, no investigation

**Option B: Discovery (6 Momentum)**
- Cost: 4 segments
- Leaves 4 segments for deliveries
- Discovery: Yes
- Tight time budget, investigation available

**Option C: Full Depth (10 Momentum)**
- Cost: 6 segments
- Leaves 2 segments for deliveries
- Discovery: Yes + Full narrative
- Risk: May fail deliveries (reputation penalty)

**This is an IMPOSSIBLE CHOICE:**
- Option A is safe but misses investigation content
- Option B balances discovery with delivery obligations
- Option C risks delivery failure for maximum narrative depth
- No "correct" answer - depends on player priorities

**This is what my GoalCompletionTrigger system lacked entirely.**

---

## Part 5: Design Principles - Why This Matters

### 1. Perfect Information Principle

**Bad Design (Hidden Gates):**
```
Complete goal → ???  → Investigation appears
```
Player has no visibility into WHY investigation appeared or WHAT triggered it.

**Good Design (Visible Mechanics):**
```
Martha Goal
├─ GoalCard (2 Mom): Surface → No discovery
├─ GoalCard (6 Mom): Deep → DISCOVER mill_mystery
└─ GoalCard (10 Mom): Trust → Reputation +3
```
Player sees EXACT thresholds, EXACT rewards, can CALCULATE resource cost.

### 2. Inter-Systemic Rules & Shared Resources

**Bad Design (Isolated Systems):**
```
Conversation System: Build momentum → complete goal
Investigation System: Check goal completion → unlock investigation
```
Systems don't compete, no resource tension.

**Good Design (Competing Systems):**
```
Shared Resource: TIME (segments)
├─ Conversation with Martha (3-6 segments for discovery)
├─ Delivery Obligations (4-5 segments required)
├─ Mill Exploration (2-3 segments initial investigation)
└─ Other NPC Goals (variable segments)
```
All systems compete for limited Time, creating strategic tension.

### 3. Opportunity Cost

**Bad Design (Free Unlocks):**
```
Complete goal → unlock investigation (no cost)
```
No reason NOT to unlock, no strategic decision.

**Good Design (Resource Cost):**
```
Push for 6 Momentum → costs 4 segments
Those 4 segments could be spent:
- Completing delivery obligations
- Exploring mill location
- Building reputation with other NPCs
- Reducing route danger
```
Every choice has COST, creating meaningful decisions.

### 4. Impossible Choices

**Bad Design (Linear Progression):**
```
Do you want to unlock investigation?
├─ Yes: Complete goal (obvious choice)
└─ No: Don't complete goal (why would you?)
```
No real choice, just "do the thing" or "don't do the thing."

**Good Design (Contextual Decisions):**
```
Which momentum threshold do you target?
├─ 2 Mom: Efficient, no discovery, safe deliveries
├─ 6 Mom: Discovery, moderate cost, tight time budget
└─ 10 Mom: Full depth, high cost, risk delivery failure

Context matters:
- Remaining time
- Competing obligations
- Player priorities (narrative vs efficiency)
- Current resource state
```
Multiple valid paths, no optimal solution, genuine strategic decisions.

### 5. String ID Matching - When Is It Appropriate?

**VALID USES:**
- Static content references (locations, NPCs, items defined in same JSON package)
- AI narrative content (dynamic text generation)
- Single JSON object with direct children (guaranteed to exist at parse time)

**INVALID USES:**
- Inter-system connection (use shared resources instead)
- Boolean gates (use resource thresholds instead)
- Discovery triggers (use in-challenge rewards instead)
- Prerequisite checking (use property thresholds instead)

**Why the distinction?**

Static content: "Which location does this NPC spawn at?" → `locationId: "market_square"`
- Content relationship, not game mechanic
- Validated at parse time (package cohesion)
- No runtime state checking

Dynamic mechanics: "When does investigation discover?" → ❌ `completedGoalId: "martha_goal"`
- Game mechanic requiring runtime state
- Creates boolean gate (Cookie Clicker pattern)
- No resource competition, no strategic depth

**Correct pattern:** `goalCard.rewards.discoverInvestigation: "mill_mystery"`
- Discovery happens IN-CHALLENGE as typed reward
- Resource cost (momentum building consumes time)
- Opportunity cost (time spent here vs elsewhere)
- Strategic decision (which threshold to target)

---

## Part 6: Implementation Roadmap

### Phase 1: Remove Wrong Implementation

**Files to modify:**
1. `DiscoveryTriggerType.cs` - Remove `GoalCompletionTrigger` enum value
2. `Investigation.cs` - Remove `CompletedGoalId` property
3. `InvestigationDiscoveryEvaluator.cs` - Remove `CheckGoalCompletionTrigger()` method
4. `13_investigations.json` - Revert to proper trigger type (or remove trigger entirely)

### Phase 2: Add GoalCard Reward System

**Files to create/modify:**
1. `GoalCardRewards.cs` - Add `string DiscoverInvestigation { get; set; }`
2. `GoalCompletionHandler.cs` - Add investigation discovery reward application
3. `InvestigationActivity.cs` - Verify `PendingIntroResult` storage exists
4. `GameScreen.razor` - Verify modal detection for investigation intro

### Phase 3: Update Content

**Files to modify:**
1. `13_investigations.json` - Remove intro trigger system (discovery via rewards only)
2. Martha NPC goal JSON (when created) - Add GoalCards with tiered rewards:
   ```json
   {
     "id": "martha_gather_information",
     "goalCards": [
       {
         "id": "surface_talk",
         "name": "Surface Conversation",
         "threshold": 2,
         "rewards": {
           "progress": 1,
           "knowledgeGranted": ["martha_mill_basics"]
         }
       },
       {
         "id": "deeper_connection",
         "name": "Deeper Connection",
         "threshold": 6,
         "rewards": {
           "progress": 1,
           "knowledgeGranted": ["martha_daughter_story"],
           "discoverInvestigation": "waterwheel_mystery"
         }
       },
       {
         "id": "trust_earned",
         "name": "Trust Earned",
         "threshold": 10,
         "rewards": {
           "progress": 1,
           "knowledgeGranted": ["martha_full_backstory"],
           "reputation": {"martha": 3}
         }
       }
     ]
   }
   ```

### Phase 4: Testing

**Validation scenarios:**
1. Complete Martha conversation at 2 Momentum → No investigation discovered
2. Complete Martha conversation at 6 Momentum → Investigation discovered, modal appears
3. Complete Martha conversation at 10 Momentum → Investigation already discovered, no duplicate modal
4. Verify time costs (segments consumed) match design intent
5. Verify competing goals create resource tension

---

## Part 7: Summary - The Core Insight

### What I Got Wrong

I thought the problem was:
- ❌ String matching bad, type-safe lookups good
- ❌ Knowledge tokens fragile, goal completion robust
- ❌ Invisible state bad, visible state good

I implemented "better" trigger system:
- ❌ Check goal completion instead of knowledge tokens
- ❌ Type-safe dictionary lookup instead of string matching
- ❌ Direct state checking instead of invisible flags

### What I Missed Completely

The problem was never STRING MATCHING vs TYPE-SAFE LOOKUPS.

The problem was **BOOLEAN GATES vs RESOURCE COMPETITION**.

My GoalCompletionTrigger was still Cookie Clicker design:
- Linear progression (complete A → unlock B)
- No strategic depth (obvious correct path)
- No resource competition (systems isolated)
- No opportunity cost (unlocks free)
- No impossible choices (just do the thing)

### The Correct Pattern

**GoalCard reward system provides:**
- ✅ Discovery happens IN-CHALLENGE (tactical gameplay integration)
- ✅ Resource cost (momentum building consumes Time/Focus)
- ✅ Opportunity cost (time spent here vs other goals)
- ✅ Inter-systemic rules (all challenges compete for shared Time)
- ✅ Impossible choices (multiple valid thresholds, no optimal solution)
- ✅ Perfect information (visible thresholds, calculable costs)
- ✅ Strategic tension (context-dependent decisions)

### The Teaching Moment

**User feedback progression:**
1. "STRING ID MATCHING IS NEVER CORRECT" → Don't use knowledge tokens
2. "ENUMS matching only slightly better" → You're still doing boolean gates
3. "It's COOKIE CLICKER level lazy design" → No strategic depth
4. "INTER-SYSTEMIC RULES and SHARED RESOURCES" → Systems must compete
5. "How does it unlock? I hope not by string matching" → Discovery must cost resources

**The learning:**

Good game design isn't about:
- Type safety
- Compile-time validation
- Direct state checking

Good game design IS about:
- Resource competition
- Opportunity cost
- Impossible choices
- Strategic tension
- Inter-systemic rules

**My boolean gate system had perfect technical architecture and zero strategic depth.**

**The GoalCard reward system has simple implementation and rich strategic gameplay.**

That's the difference between COOKIE CLICKER and WAYFARER.

---

## Appendix: Relevant Architecture Documentation

### From Architecture.md

**GameWorld as Single Source of Truth:**
> GameWorld contains all game state. Services operate on GameWorld but don't store state. All state changes flow through GameWorld.

**Three Parallel Tactical Systems:**
> Physical, Mental, Social challenges all compete for player's time and resources. This creates strategic tension through resource competition.

### From wayfarer-design-document-v2.md

**Core Design Philosophy:**
> "Wayfarer creates strategic depth through impossible choices - decisions where multiple valid paths exist with genuine trade-offs."

**Perfect Information Principle:**
> "All formulas visible. All calculations transparent. Players can compute exact outcomes. No hidden gates, no arbitrary requirements."

**Strategic-Tactical Architecture:**
> "Strategic layer (goals) defines WHERE. Tactical layer (challenges) defines WHEN. Bridge pattern: Goals create context, GoalCards create victory conditions."

### From obstacle-system-design.md

**Property Thresholds for Goal Availability:**
> "Simple gating: Property thresholds and knowledge requirements. No formulas, no string matching. Goals check obstacle properties directly."

**Shared State Through Properties:**
> "Obstacles have properties (physicalDanger, mentalComplexity, socialDifficulty). Goals check these properties. Multiple goals can react to same obstacle state."

---

**END DOCUMENT**

**Status**: This document captures the architectural learning from investigation discovery system design. GoalCompletionTrigger implementation should be removed and replaced with GoalCard reward system as described above.

**Key Takeaway**: Good game design isn't about perfect technical architecture - it's about resource competition, opportunity cost, and impossible choices that create strategic depth.
