# How to Play Wayfarer (Agent Testing Guide)

## Purpose

This guide teaches AI agents how to understand and interact with Wayfarer for testing. It explains the game through principles and concepts, not implementation details.

## Core Game Concepts

**Wayfarer is a narrative RPG** where you navigate a world, interact with people, make meaningful choices, and manage your character's well-being. Every decision has transparent consequences shown before you commit.

**The Central Loop:**
You exist in a location. You have options for what to do. Actions lead to new situations. Some situations are simple (move somewhere, rest, wait). Others are complex narrative moments with branching choices that affect your character and relationships.

**Perfect Information Principle:**
Before making any meaningful choice, you see exactly what it will cost you and what you'll gain. No hidden consequences. This lets you make strategic decisions about how to spend your limited resources (time, energy, health, social capital).

## Understanding the World

**Space is Hierarchical:**
The world is organized as Venues containing multiple Spots. A tavern is a venue. The common room and upstairs rooms are spots within it. You navigate between spots instantly within a venue, or travel longer distances between venues.

**Time Flows in Blocks:**
Each day has four periods (morning, midday, afternoon, evening). Actions consume time blocks. When you exhaust a period, the next begins. When evening ends, you advance to the next day. Time is a precious resource - you can't do everything.

**Resources Define Your State:**
Your character tracks several resources that constrain your options:
- **Physical State**: Health and stamina limit your ability to act
- **Mental State**: Focus determines your ability to engage in complex challenges
- **Basic Needs**: Hunger increases over time and must be managed
- **Social Capital**: Resolve spent to influence others, rebuild through relationships
- **Economic**: Coins enable transactions and access to services

When resources deplete, options close. When you lack stamina, physically demanding actions become unavailable. When you lack resolve, you can't attempt social challenges. Resource management is central to strategy.

## Navigation Principles

**You Move Through States, Not Pages:**
The game doesn't reload or navigate to new URLs. Instead, the display transforms to show different views based on your actions. Understanding which view you're in is crucial for knowing what actions are available.

**The View Hierarchy:**

**Location View** - Your default state. You're standing somewhere and see what you can do here. Actions might include looking around to see who's present, moving to another spot, resting, waiting for time to pass, or traveling elsewhere.

**People View** - When you look around, you transition to seeing who's present and what interactions they offer. Each person might have ongoing narrative opportunities (scenes). Not everyone has something to offer at every moment.

**Scene View** - When you engage with someone's narrative opportunity, you enter a modal focus on that interaction. The world behind doesn't go away, but you're now making choices within this specific narrative moment.

**Discovery View** - Occasionally, achieving something triggers a narrative interruption (discovering an obligation, earning an achievement). These moments pause the flow until you acknowledge them.

**The Navigation Pattern:**
Location → People → Scene → Location (with consequences applied)

You always return to the location view after completing an action. From there, you decide your next move. The cycle continues.

## Understanding Scenes (Narrative Moments)

**Scenes Are Consequential Choices:**
When you engage with someone, you enter a scene - a focused narrative moment with choices. Each choice shows:
- What it costs (resources consumed immediately)
- What you gain (resources restored, relationships strengthened, items received)
- Current state and projected future state after this choice

**Locked vs Available:**
Some choices require prerequisites (relationship strength, skill level, possessing specific items). Locked choices show you exactly what you're missing - no mystery, just transparent gates telling you "come back when you have X."

**Choice Display Pattern:**
Every scene presents multiple options:
- Successful approaches (different ways to achieve the goal, each with different costs)
- Alternative outcomes (different types of success or compromise)
- Exit strategies (ways to back out, often with minor costs)

The game never traps you without options. There's always a way forward, even if costly.

**Perfect Information in Action:**
Before selecting a choice, you see resource changes displayed as:
- Costs as negative changes with before/after values
- Rewards as positive changes with projected outcomes
- Current state prominently displayed for comparison

Example: "Resolve -2 (now 30, will have 28)" tells you the exact cost, your current reserve, and what you'll have left. No guessing, no surprises.

## Multi-Situation Scene Architecture (Currently Broken)

**The Design Intent:**
Some scenes are meant to be multi-part story arcs spanning several situations across different locations. A complete arc might involve:
1. Negotiating access in one location
2. Traveling to the service location
3. Receiving the service there
4. Returning to conclude the transaction

**Context-Aware Progression:**
The architecture distinguishes between:
- **Same Context Transitions**: Consecutive situations at the same location should flow seamlessly without returning to the world
- **Different Context Transitions**: When the next situation requires a different location, you exit to the world, navigate there, and the scene resumes

**Current Reality:**
Multi-situation scenes currently end after the first situation completes. You return to the world and don't see the continuation. This is an architectural gap, not intended behavior.

**What Should Happen (Example: Lodging Service):**
1. Negotiate with innkeeper at common room → Complete → Exit to world
2. Navigate to upstairs room → Scene resumes automatically → Enter room
3. Rest in room → Seamlessly advance (same location) → Wake up
4. Return to common room → Scene resumes automatically → Conclude transaction

**What Actually Happens:**
1. Negotiate with innkeeper at common room → Complete → Exit to world
2. Navigate to upstairs room → Scene does NOT resume → Just regular location actions

## Resource Display Principles

**Everything Visible, Always:**
Your resources appear constantly at the top of the screen. No hidden stats, no surprises. What you see is what you have.

**Visual States Communicate Urgency:**
Resources display differently based on their level:
- Normal state (you're fine)
- Warning state (getting low, consider addressing)
- Critical state (immediate danger, must address soon)

**Time Visibility:**
The current day, period, and remaining segments in this period are always visible. You know exactly how much time you have left before transitions occur.

## Interaction Principles for Testing

**Observation Over Memorization:**
Don't rely on fixed positions or sequences. Instead, observe what's currently displayed and identify elements by their meaning (the action that says "Look Around", the person named "Elena", the choice about "Make a bold gambit").

**State Verification is Mandatory:**
After every action, verify the state changed as expected:
- Did resources change by the expected amounts?
- Did the view transition to what you anticipated?
- Did new options appear or old ones disappear?

Never assume an action succeeded without verification.

**Modal Interruptions Require Handling:**
Discovery moments can interrupt your planned flow. Always check if something is blocking the current view and handle it before proceeding with your original intention.

**Context Determines Available Actions:**
What you can do depends on where you are and what state you're in. The same location at different times of day might offer different opportunities. The same person in different relationship states offers different interactions.

## Testing Strategies

**Basic Flow Validation:**
Start at the default location. Verify you can access the people view. Verify you can engage with available narrative moments. Verify you can make choices and return to the world.

**Resource Consequence Verification:**
Make choices with known costs. Verify resources decreased by exactly those amounts. Make choices with known rewards. Verify resources increased as stated.

**Navigation Validation:**
Move between locations. Verify location headers update correctly. Verify time passes if expected. Verify available actions change appropriately.

**Perfect Information Validation:**
Enter scenes. Verify every choice shows consequences before selection. Verify locked choices explain exactly why they're locked. Verify the display format matches the principle (current value, change amount, resulting value).

**Multi-Situation Validation (Currently Failing):**
Engage with multi-situation scenes. Complete the first situation. Navigate to where the second situation should occur. Observe that it does NOT resume (this is the known bug). Document that the scene prematurely ended.

## Common Failure Modes

**Modal Blocking:**
You attempt an action but something blocks it. Check if a discovery modal appeared that needs acknowledgment before you can proceed.

**Premature Scene Termination:**
You complete the first part of what should be a multi-step scene, but it ends instead of continuing. This is the known architectural gap. Document it as a failure.

**Ambiguous Element Selection:**
You attempt to interact with something but get the wrong element (like clicking the first action when you meant a specific one). Use meaning-based identification instead of position-based.

**State Desynchronization:**
The displayed state doesn't match what you expect after an action. This indicates either the action failed silently or your expectation was wrong. Investigate which.

## Debugging Principles

**When Things Go Wrong:**

**Identify Current View:**
First, determine what view you're in. Are you at a location looking at actions? Are you looking at people? Are you in a scene? Are you blocked by a discovery modal? The view determines what actions are possible.

**Verify Prerequisites:**
If an action seems unavailable, check if prerequisites are met. Do you have the required resources? Are you in the right location? Is the right time of day?

**Check for Obstructions:**
If actions don't respond, check if something is covering them (a modal, an overlay, a loading state).

**Trace State History:**
If the current state doesn't match expectations, trace back through recent actions. Did each action verify correctly? Is there a point where state diverged from expectations?

## Summary for Agents

**Conceptual Understanding Checklist:**

- Wayfarer is about managing resources while navigating narrative opportunities
- Perfect Information means all consequences visible before commitment
- Navigation cycles through: Location → People → Scene → Location (changed)
- Resources constrain options - strategy is about resource allocation
- Time flows in blocks and is a precious resource itself
- Multi-situation scenes are architecturally intended but currently broken
- Verification after every action is mandatory, not optional
- Observation beats memorization - identify by meaning, not position
- Modal interruptions are normal and must be handled
- Current behavior may differ from architectural intent - document gaps

**Testing Mindset:**

You're not just clicking buttons. You're verifying that:
- The system displays information transparently
- Actions produce expected consequences
- State transitions follow logical rules
- Resource accounting is accurate
- The player could make informed strategic decisions

When something doesn't work as architecturally intended (like multi-situation scenes), that's not a testing failure - that's a successful identification of an implementation gap. Document it precisely.

**The Core Question:**

Can a human player, using only the information displayed, make strategic decisions about resource allocation across time to achieve their goals? If yes, the system works. If no, identify what information is missing or misleading.

This guide describes the conceptual model, not the implementation. Use it to understand WHAT the game is, not HOW it's built.
