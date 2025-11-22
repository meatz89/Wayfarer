# Wayfarer Playtest Guide

**Purpose:** Comprehensive guide for testing the player experience flow and core gameplay loop.

**Session Goal:** Test holistic player experience from game start through multiple gameplay loops, verify all systems work together, identify friction points and accessibility issues.

---

## Table of Contents

1. [What is Wayfarer?](#what-is-wayfarer)
2. [How to Run the Game](#how-to-run-the-game)
3. [Quick Start: First 15 Minutes](#quick-start-first-15-minutes)
4. [Core Gameplay Loops](#core-gameplay-loops)
5. [Game World Structure](#game-world-structure)
6. [UI Navigation Guide](#ui-navigation-guide)
7. [How to Play Using Playwright](#how-to-play-using-playwright)
8. [Content Generation Systems](#content-generation-systems)
9. [Player Personas for Testing](#player-personas-for-testing)
10. [Testing Agents and Responsibilities](#testing-agents-and-responsibilities)
11. [Decision Points and Options](#decision-points-and-options)
12. [What to Look Out For](#what-to-look-out-for)
13. [Testing Scenarios by Loop](#testing-scenarios-by-loop)
14. [Known Content Inventory](#known-content-inventory)

---

## What is Wayfarer?

### Elevator Pitch

Wayfarer is a low-fantasy resource management game where you play as a working traveler navigating a harsh world through careful planning, relationships, and strategic choices. You are not a hero—you're a courier accepting delivery jobs to earn coins, managing scarce resources across dangerous routes, and barely affording food and shelter. Every decision forces you to choose between multiple valid but suboptimal paths, revealing your character through what you're willing to sacrifice.

The game never ends. Inspired by "Frieren: Beyond Journey's End," Wayfarer embraces infinite journey design where the road continues forever. Success is not reaching a destination—it's the person you become along the way.

### Core Design Pillars

1. **Perfect Information at Strategic Layer** - See exact costs, requirements, and rewards before committing
2. **Meaningful Consequence Through Scarcity** - Resources spent are genuinely spent; tight economic margins
3. **Journey Over Destination** - No "beating the game"; explore at your own pace
4. **Specialization Creates Identity** - Limited resources force specialization in stats, creating distinct builds
5. **Verisimilitude Throughout** - Systems mirror real-world dynamics; low-magic setting
6. **No Power Fantasy** - Resources remain scarce; challenges remain challenging

### The Core Experience

**"I can afford to do A OR B, but not both. Both paths are valid. Both have genuine costs. Which cost will I accept?"**

Wayfarer creates strategic depth through impossible choices, not mechanical complexity. Every decision presents multiple suboptimal paths with insufficient resources to pursue all.

---

## How to Run the Game

### Build and Run Commands

```bash
# Navigate to source directory
cd src

# Build the project (verify compilation)
dotnet build

# Run the game on localhost:5000
ASPNETCORE_URLS="http://localhost:5000" dotnet run

# Run tests (if testing code changes)
dotnet test
```

### Expected Startup Behavior

1. **Console Output:**
   - Content loading messages (parsing 19 JSON files)
   - GameWorld initialization complete
   - Application started on http://localhost:5000

2. **Browser:**
   - Navigate to http://localhost:5000
   - Game UI loads (Blazor WebAssembly)
   - Initial location screen appears (starting at Town Square or inn)

3. **Initialization Time:**
   - First load: 2-5 seconds (parsing 3,405 lines of JSON content)
   - Subsequent loads: 1-2 seconds (cached)

### Troubleshooting

**Build errors:**
- Ensure .NET SDK 7.0+ installed
- Check for missing dependencies: `dotnet restore`

**Port conflicts:**
- Change port: `ASPNETCORE_URLS="http://localhost:6000" dotnet run`

**Content loading failures:**
- Check `/src/Content/Core/` directory exists with JSON files
- Verify JSON syntax (parsers will throw on malformed data)

---

## Quick Start: First 15 Minutes

### What You'll Experience (Tutorial Flow)

**Scene 1: "Secure Lodging at Inn" (5 minutes)**
1. Game starts at inn common room
2. Meet Elena the innkeeper (NPC)
3. See 4 choice options for securing lodging:
   - Pay coins (instant, -15 coins)
   - Use Rapport stat (instant, requires Rapport 3)
   - Social challenge (conversation card game)
   - Sleep in common room (fallback, poor rest)
4. Choose option and see consequences apply
5. Navigate to private room (if unlocked)
6. Rest to restore energy
7. Depart room (scene completes)

**Scene 2: "Find First Delivery" (3 minutes)**
1. Morning arrives at town square
2. Meet merchant NPC
3. View available delivery contracts (job board)
4. Accept delivery job (earn 15-25 coins on completion)
5. Delivery becomes active (shown in header banner)

**Scene 3: "Travel to Destination" (7 minutes)**
1. Select "Travel to Another Venue" action
2. Choose route from route list
3. Navigate route segments:
   - Face-down cards reveal as you progress
   - Each segment presents choices (pay cost or find alternative)
   - Fixed segments flip face-up (learnable on repeat travel)
   - Event segments stay face-down (random each time)
4. Reach destination
5. Complete delivery (earn coins)
6. Experience economic pressure (survival costs consume earnings)

**Key Lessons:**
- Resource scarcity (barely profitable after food/lodging)
- Perfect information (all costs visible before commitment)
- Route learning (segments become predictable)
- Specialization value (stat-gated paths offer best outcomes)

---

## Core Gameplay Loops

### SHORT LOOP: Encounter-Choice-Consequence (10-30 seconds)

**Rhythm:** Encounter situation → Evaluate options with visible costs → Make informed choice → See immediate consequences → Progress forward

**Example - Muddy Road Encounter:**
- Choice 1: "Push through" (3 energy, 1 time) - High energy cost, reliable
- Choice 2: "Find detour" (1 energy, 2 time) - Saves energy, costs time
- Choice 3: "Hire guide" (5 coins, 1 time) - Monetary cost, fast
- Choice 4: "Athletic leap" (Physical stat required) - Spawns challenge

**No correct answer:** Each choice optimizes different resources based on your situation.

### MEDIUM LOOP: Delivery Cycle (5-15 minutes)

**Complete daily cycle:**
1. **Morning:** Wake at location → Review resources/time → Choose primary activity
2. **Activity Options:**
   - Accept Delivery (primary income: 15-25 coins, 3-5 time segments)
   - NPC Interaction (build relationship for future benefits)
   - Local Exploration (discover new routes/NPCs/content)
   - Equipment Purchase (spend accumulated coins)
3. **Route Travel:** Sequential segments with fixed/event types
4. **Evening:** Purchase food (8-12 coins) + lodging (7-15 coins)
5. **Sleep:** Advance to next day

**Economic Pressure Example:**
- Delivery: +20 coins
- Route costs: various energy/hunger
- Food: -10 coins
- Lodging: -15 coins
- **Net: -5 coins (operating at loss without optimization)**

### LONG LOOP: Character Development (hours)

**Progression Vectors:**
1. **Stat Development:** Challenge success grants XP; NPC training provides boosts
2. **Economic Advancement:** Accumulate profits → Equipment → Better efficiency
3. **Relationship Building:** Invest time in NPCs → Bond levels → Mechanical benefits
4. **World Discovery:** New regions → New routes → New locations → World expands

**Build Specialization Examples:**
- Investigator (Insight + Cunning): Dominates Mental challenges, weak in Social
- Diplomat (Rapport + Diplomacy): Dominates Social challenges, weak in Mental
- Leader (Authority + Diplomacy): Balanced Social, weak in investigations
- Hybrid Builds: Flexible but never dominant

---

## Game World Structure

### Spatial Hierarchy (Four Tiers)

```
Hex Grid (invisible backend scaffolding)
  ↓
Venue (cluster of 2-7 locations)
  Examples: "The Brass Bell Inn", "Town Square", "The Old Mill"
  Movement within venue: Instant (no travel cost)
  ↓
Location (specific place)
  Examples: "Common Room", "Upper Floor", "Town Square Center"
  This is where you ARE, where NPCs appear, where scenes spawn
  Movement between venues: Requires route travel
  ↓
Route (travel path between venue hubs)
  Examples: "Forest Path" connecting Inn to Mill
  Contains: Sequence of segments with challenges/encounters
```

### Core Entities

#### NPCs (Non-Player Characters)
- **Stored in:** GameWorld.NPCs collection
- **Key Properties:** Name, Profession, Demeanor (Friendly/Neutral/Hostile), Location
- **Progression:** StoryCubes (0-10 relationship progression per NPC)
- **Role:** Placement context for scenes, conversation partners, trading

**Example NPCs:**
- Elena (Innkeeper, Friendly) - Tutorial NPC for lodging scenes
- Various merchants, officials, travelers

#### Locations
- **Stored in:** GameWorld.Locations collection
- **Key Properties:** Name, Venue, HexPosition, IsVenueTravelHub
- **Progression:** InvestigationCubes (0-10 mastery per location)
- **Role:** Where player can be, where NPCs appear, where scenes spawn

**Current Locations (9+):**
- Town Square Center (starting location)
- Common Room (social hub)
- Fountain Plaza (commerce)
- Mill Courtyard (wilderness entrance)

#### Routes
- **Stored in:** GameWorld.Routes collection
- **Key Properties:** Origin/Destination, HexPath, DangerRating, TimeSegments
- **Progression:** ExplorationCubes (0-10 familiarity per route)
- **Role:** Enable travel between venues, trigger encounters

#### Scenes (Strategic Layer Progression)
- **Stored in:** GameWorld.Scenes collection
- **Structure:** Multi-situation narrative containers (2-4 situations per scene)
- **State Machine:** Provisional → Active → Completed/Expired
- **Placement:** At Location, NPC, or Route

**Example Scene Flow:**
```
Scene: "Secure Lodging"
  Situation 1: Negotiate with innkeeper (4 choices)
    → Choose "Pay coins"
    → Costs: -15 coins
    → Rewards: Unlock private room
  Situation 2: Rest in private room (3 choices)
    → Choose "Sleep 8 hours"
    → Costs: +8 time segments
    → Rewards: Restore stamina, advance day
  Situation 3: Depart room (2 choices)
    → Choose "Depart carefully"
    → Scene completes
```

#### Situations (Embedded in Scenes)
- **Stored in:** Scene.Situations (NOT separate collection)
- **Structure:** Individual decision moments with 2-4 choices
- **Bridge:** Can escalate to tactical challenges or provide instant results

#### Choices/Actions
- **Two Types:**
  1. **Atmospheric Actions:** Always available (Travel, Work, Rest, Movement)
  2. **Ephemeral Actions:** Temporary scene-based choices
- **Action Types:**
  - Instant: Apply costs/rewards immediately
  - StartChallenge: Bridge to tactical layer (Social/Mental/Physical)
  - Navigate: Change location/screen

### Templates vs Instances

**Templates (Immutable Blueprints):**
- SceneTemplate: Defines multi-situation structure
- SituationTemplate: Defines choice patterns
- ChoiceTemplate: Defines costs/requirements/rewards
- Card Templates: Social/Mental/Physical cards for challenges

**Instances (Mutable Runtime State):**
- Scene: Active gameplay container spawned from template
- Situation: Current decision moment within scene
- Action: Executable choice created on-demand

**Key Distinction:** Templates are "the blueprint" (what CAN exist). Instances are "the reality" (what ACTUALLY exists in your game).

---

## UI Navigation Guide

### Main Container: GameScreen.razor

**Always-Visible Header:**
- Time Display: Day number, time period (Morning/Afternoon/Evening/Night), segment dots
- Resources Bar: Health, Hunger, Focus (current/max), Stamina, Coins (color-coded warnings)
- Active Delivery Banner: Shows active job with payment (if applicable)
- Fixed Buttons: Journal, Map

**Dynamic Content Area (Screen Switching):**
Based on CurrentScreen enum, renders different screens. Player navigates by clicking actions/buttons.

### Main Screens and Navigation Flow

#### 1. Location Screen (Hub)

**View States:**

**Landing View:**
- "What do you do?" with action cards grid
- Player actions: Check Belongings, Look Around
- Travel actions: Go to Another Venue
- Location-specific: Rest, Work, Get Food, Rent Room
- Each card shows: Title, Detail, Cost, Lock reason (if unavailable)

**Looking Around View:**
- Atmosphere text (location description)
- People Here: NPC cards with interaction buttons (Talk, Exchange)
- Available Conversations: Dialogue trees
- Investigate: Observation scenes
- Back button to Landing

**Navigation:**
- Landing → Look Around → Select NPC/Scene → Scene Screen
- Landing → Travel → Travel Screen
- Landing → Work → Job Board View

#### 2. Scene Screen (Choice-Based Narrative)

**Layout:**
- Scene title/name
- Situation description (current narrative state)
- Choice cards (2-4 options) with:
  - Choice name and description
  - **CONSEQUENCES section** (always visible):
    - Costs: Resolve, Coins, Health, Stamina, Focus, Hunger, Time
    - Rewards: Resource gains, Stat gains, Relationship changes, Unlocks
    - Final values shown for all resources
  - Lock indicator (if unavailable): Shows missing requirements

**Example Choice Card:**
```
Pay Premium Rate
  Pay 15 coins upfront for guaranteed lodging

CONSEQUENCES:
  Costs:
    - Coins: 27 → 12 (-15)
    - Time: 1 segment
  Rewards:
    - Unlock: Private Room
    - Elena relationship: +1
```

#### 3. Challenge Screens (Tactical Layer)

**Social Challenge (Conversation):**
- NPC bar: Name, bond level, personality
- Resources: Momentum (0-16 progress), Cadence (-5 to +5 balance), Doubt (consequence)
- Thresholds: 3 cards showing targets (8, 12, 16) with rewards
- Card grid: Available conversation cards with Initiative costs
- Actions: SPEAK (play cards), LISTEN (draw cards)

**Mental Challenge (Investigation):**
- Resources: Progress (0-20 builder), Exposure (consequence)
- Thresholds: 2 cards (10 partial, 20 complete)
- Pile: Details cards to draw
- Actions: ACT (investigate), OBSERVE (draw details)

**Physical Challenge (Obstacle):**
- Resources: Breakthrough (0-20 builder), Aggression (-10 to +10 balance), Danger (consequence)
- Thresholds: 2 cards showing targets
- Card grid: Physical action cards
- Actions: EXECUTE (lock combos), ASSESS (trigger)

#### 4. Travel Screen

**Route Selection:**
- Header: Current location
- Route list by type (Walking, Special)
- Route cards show: Name, Transport badge, Time cost (segment blocks), Costs, Requirements

**Travel Path View:**
- Sequential segments (player progresses through)
- Face-down segments reveal on encounter
- Fixed segments flip face-up permanently (learnable)
- Event segments stay face-down (random)

#### 5. Other Screens

**Exchange/Trading:**
- NPC header with connection state
- Exchange grid: Items with costs/effects
- TRADE and LEAVE buttons

**Inventory:**
- Header: "Belongings"
- Weight status bar
- Items list with descriptions and weights

**Observation:**
- Scene description
- Examination points (affordable vs blocked)
- Progress tracker

**Conversation Tree:**
- NPC dialogue
- Response options with costs
- Focus display

### Modal Dialogs (Overlays)

**Journal Modal:**
- Tabs: Active Obligations, Discovered, Completed, Discoveries
- Each obligation shows: Name, Progress bar, Situation list, Location hints
- Close button (X)

**Map Modal:**
- Hex grid viewport with terrain colors
- Player position marked (★)
- Location markers at hex coordinates
- Legend showing terrain types
- Close button

**Obligation Modals:**
- Discovery: "New obligation available" with Begin/Not Now
- Intro: Details before starting
- Progress: Current status
- Complete: Results and rewards

---

## How to Play Using Playwright

### Prerequisites

```javascript
// Install Playwright
npm install -D @playwright/test

// Install browsers
npx playwright install
```

### Basic Navigation Pattern

```javascript
const { test, expect } = require('@playwright/test');

test('navigate game world', async ({ page }) => {
  // Navigate to game
  await page.goto('http://localhost:5000');

  // Wait for game to initialize
  await page.waitForSelector('.location-content', { timeout: 10000 });

  // Initial location should be visible
  await expect(page.locator('.location-header')).toBeVisible();
});
```

### Interaction Patterns

#### 1. Location Actions (Click Action Cards)

```javascript
// Look around at location
await page.click('text=Look Around');
await page.waitForSelector('.looking-around-view');

// Interact with NPC
await page.click('text=Elena'); // NPC name
// Or
await page.click('button:has-text("Talk")');
```

#### 2. Scene Choices (Select from Available Options)

```javascript
// Wait for scene to load
await page.waitForSelector('.scene-content');

// Read choice consequences
const consequences = await page.locator('.choice-consequences').textContent();
console.log('Choice consequences:', consequences);

// Click choice
await page.click('text=Pay Premium Rate');

// Verify resources updated
const coinsAfter = await page.locator('.resource-coins').textContent();
expect(coinsAfter).toContain('12'); // Expected final value
```

#### 3. Travel (Select Route and Navigate Segments)

```javascript
// Start travel
await page.click('text=Go to Another Venue');
await page.waitForSelector('.travel-content');

// Select route
await page.click('.route-card:has-text("Forest Path")');

// Navigate segments
await page.waitForSelector('.travel-path-content');

// For each segment, make choices
await page.click('text=Push Through'); // Example segment choice
// Repeat until route complete
```

#### 4. Challenges (Play Cards and Build Resources)

```javascript
// Social challenge example
await page.waitForSelector('.conversation-content');

// Check current momentum
const momentum = await page.locator('.momentum-bar').textContent();

// Play a card
await page.click('.social-card:has-text("Remark")');

// Click SPEAK or LISTEN action
await page.click('button:has-text("SPEAK")');

// Verify momentum increased
const newMomentum = await page.locator('.momentum-bar').textContent();
// Assert newMomentum > momentum
```

#### 5. Modals (Open/Close Overlays)

```javascript
// Open journal
await page.click('button:has-text("Journal")');
await page.waitForSelector('.discovery-journal-modal');

// Check active obligations
const obligations = await page.locator('.active-obligations').textContent();

// Close modal
await page.click('.modal-close-button'); // X button
// Or click outside modal
```

### Key Selectors Reference

**Resource Display:**
- `.resource-health` - Health value
- `.resource-coins` - Coin value
- `.resource-hunger` - Hunger value
- `.resource-focus` - Focus value
- `.time-display` - Day and time period

**Location Elements:**
- `.location-content` - Main location container
- `.action-card` - Clickable action cards
- `.npc-card` - NPC interaction cards

**Scene Elements:**
- `.scene-content` - Main scene container
- `.scene-choice-card` - Individual choice cards
- `.choice-consequences` - Costs/rewards section
- `.scene-locked-indicator` - Unavailable choice marker

**Challenge Elements:**
- `.conversation-content` - Social challenge
- `.mental-content` - Mental challenge
- `.physical-content` - Physical challenge
- `.social-card`, `.mental-card`, `.physical-card` - Challenge cards
- `.momentum-bar`, `.progress-bar`, `.breakthrough-bar` - Resource bars

**Travel Elements:**
- `.travel-content` - Travel screen
- `.route-card` - Route selection cards
- `.travel-path-content` - Active route path
- `.segment-card` - Individual route segments

### Testing Workflow Example

```javascript
test('complete delivery cycle', async ({ page }) => {
  await page.goto('http://localhost:5000');

  // 1. Start at location
  await page.waitForSelector('.location-content');

  // 2. Look around
  await page.click('text=Look Around');

  // 3. Accept delivery from merchant
  await page.click('text=View Deliveries'); // Or NPC interaction
  await page.waitForSelector('.job-board');
  await page.click('.job-card >> button:has-text("Accept")');

  // 4. Verify delivery appears in header
  await expect(page.locator('.active-delivery-banner')).toBeVisible();

  // 5. Travel to destination
  await page.click('text=Go to Another Venue');
  await page.click('.route-card:has-text("Mill")'); // Destination

  // 6. Navigate route segments
  // (Loop through segments based on route structure)
  let segmentCount = 0;
  while (await page.locator('.segment-card').count() > 0 && segmentCount < 10) {
    // Choose segment option (first available)
    await page.click('.segment-choice >> button >> nth=0');
    segmentCount++;
  }

  // 7. Arrive at destination
  await page.waitForSelector('.location-content');

  // 8. Complete delivery (automatic or via scene)
  // Check coins increased
  const coinsText = await page.locator('.resource-coins').textContent();
  // Verify payment received

  console.log('Delivery cycle completed');
});
```

### Advanced: Verifying Perfect Information

```javascript
test('verify all costs visible before choice', async ({ page }) => {
  await page.goto('http://localhost:5000');
  await page.waitForSelector('.scene-content');

  // For each choice card
  const choiceCards = await page.locator('.scene-choice-card').count();

  for (let i = 0; i < choiceCards; i++) {
    const card = page.locator('.scene-choice-card').nth(i);

    // Verify consequences section exists
    await expect(card.locator('.choice-consequences')).toBeVisible();

    // Verify costs shown
    const consequences = await card.locator('.choice-consequences').textContent();

    // Log for inspection
    console.log(`Choice ${i} consequences:`, consequences);

    // Could assert specific format, presence of keywords
    // expect(consequences).toContain('Costs:');
    // expect(consequences).toMatch(/\d+/); // Contains numbers
  }
});
```

### Playwright Tips for Wayfarer

1. **Wait for async state updates:** Game uses Blazor, DOM updates may be async
2. **Use text selectors:** Card titles/action names are stable identifiers
3. **Check resource values:** Always verify resources updated after actions
4. **Handle modals carefully:** Click outside to close, or use close button
5. **Route segments vary:** Route length may differ, use loops with max iterations
6. **Consequences are rich:** Parse consequence text to verify costs/rewards match expectations
7. **Screen transitions:** Wait for content class selectors after navigation

---

## Content Generation Systems

### Overview: Archetype-Based Infinite Variety

Wayfarer generates effectively infinite content from finite foundation using **mechanical patterns (archetypes)** that scale contextually based on **entity properties**.

**Core Innovation:** Separating mechanical structure from narrative context. The "negotiation" pattern works identically whether securing lodging, gaining passage, or purchasing information—but each feels different based on entities involved.

### Scene & Situation Generation

**Scene Archetypes (Multi-Situation Structures):**
- Define how multiple situations flow together (2-4 situations per scene)
- Example: "service_with_location_access" creates negotiate → access → service → depart
- Same archetype generates inn lodging, bathhouse visits, healer treatments

**Situation Archetypes (4-Choice Patterns):**
- 21 total archetypes (5 core, 10 expanded, 6 specialized)
- Each defines: stat relevance, base costs, challenge types, PathType distribution
- Example: NEGOTIATION archetype generates same 4-choice structure across contexts

**What Playtesters Notice:**
- Consistent strategic patterns (recognizable four-choice structure)
- Different thematic context and difficulty scaling
- Learning one archetype helps recognize it in new contexts

### NPC Creation & Personalization

**Three Orthogonal Dimensions (3×3×3 = 27 archetypes):**

1. **NPCSocialStanding:** Commoner / Notable / Authority
2. **NPCStoryRole:** Obstacle / Neutral / Facilitator
3. **NPCKnowledgeLevel:** Ignorant / Informed / Expert

**Composition Examples:**
- Notable + Obstacle + Informed = Gatekeeper
- Authority + Facilitator + Expert = Wise Mentor
- Commoner + Neutral + Ignorant = Peasant Bystander

**Personality Scaling:**
- Friendly: 0.6× threshold (easier to persuade)
- Neutral: 1.0× threshold (baseline)
- Hostile: 1.4× threshold (harder to persuade)

**What Playtesters Notice:**
- NPCs have distinct personalities emerging from dimensional properties
- Hostile guard captain feels completely different from friendly innkeeper
- Same negotiation requires different stat levels based on demeanor

### Location Placement & Connection

**Four Orthogonal Dimensions (3×3×3×4 = 108 archetypes):**

1. **LocationPrivacy:** Public / SemiPublic / Private
2. **LocationSafety:** Dangerous / Neutral / Safe
3. **LocationActivity:** Quiet / Moderate / Busy
4. **LocationPurpose:** Transit / Dwelling / Commerce / Civic

**Composition Examples:**
- SemiPublic + Safe + Moderate + Dwelling = Inn Common Room
- Public + Dangerous + Quiet + Transit = Dark Alley
- Public + Neutral + Busy + Commerce = Merchant Square

**Dynamic Generation (FindOrCreate Pattern):**
1. Scene needs location with properties (e.g., "Private + Safe + Dwelling")
2. System searches existing locations
3. If found: Reuse (prefer authored content)
4. If not found: Generate new location with exact properties
5. Generated location persists forever

**What Playtesters Notice:**
- World expands organically as you play
- Early game mostly pre-authored (stable tutorial)
- Late game more procedural (infinite variety)
- Generated locations indistinguishable from hand-authored

### Route Generation

**Two Segment Types:**

**Fixed Environmental Segments (Learnable):**
- Face-down on first travel, reveals when encountered
- Consistent each time (Steep Hill always presents climbing challenge)
- Face-up on repeat travel showing exact costs
- Rewards route mastery through learning

**Event Segments (Always Random):**
- Always face-down, random encounter each time
- Never predictable, never learned
- Creates uncertainty requiring resource buffers
- Tests flexibility

**What Playtesters Notice:**
- Routes have distinct personalities (known hills, tollgates, alleys)
- Random events keep routes unpredictable
- New routes feel risky until learned
- Mastering routes increases profitability

### Choice Generation: Categorical Property Scaling

**Mathematical Foundation:**
Base archetype values × scaling multipliers = contextually appropriate difficulty

**Nine Universal Categorical Properties:**

**Core Scaling (affects costs/thresholds):**
1. NPCDemeanor (Friendly 0.6× / Neutral 1.0× / Hostile 1.4×)
2. Quality (Basic 0.7× / Standard 1.0× / Premium 1.6× / Luxury 2.5×)
3. PowerDynamic (Dominant 0.6× / Equal 1.0× / Submissive 1.4×)
4. EnvironmentQuality (Basic 1.0× / Standard 2.0× / Premium 3.0×)

**Specialized Scaling (affects consequences):**
5. DangerLevel (Safe / Risky) - crisis damage
6. SocialStakes (Private / Witnessed / Public) - reputation impact
7. TimePressure (Leisurely / Urgent) - available choices
8. EmotionalTone (Cold / Warm / Passionate) - relationship progression
9. MoralClarity (Clear / Ambiguous) - narrative framing

**Example: Negotiation Across Contexts**

Base negotiation: StatThreshold 5, CoinCost 10

- **Friendly innkeeper + Basic inn:** 0.6× × 0.7× = Stat 3, Cost 7 (easy, cheap)
- **Hostile merchant + Luxury shop:** 1.4× × 2.5× = Stat 7, Cost 25 (hard, expensive)
- **Neutral official + Standard office:** 1.0× × 1.0× = Stat 5, Cost 10 (baseline)

**Mathematical Variety:**
- 21 archetypes × 3 demeanor × 4 quality × 3 power × 3 environment
- = 2,268 base mechanical variations
- × Infinite entity-specific narrative
- = Effectively infinite content

### Dynamic vs Static Content

**Dynamic Characteristics:**
- Property-driven variation (same archetype, different entities)
- Procedural entity resolution (categorical requirements)
- Contextual scaling (relationships affect difficulty)

**Static Characteristics:**
- Authored tutorial content (A1-A3 hand-crafted)
- Fixed environmental features (learned route segments)
- Specific tutorial NPCs (Elena always present)

**Bootstrap Gradient:**
- Early game: 95% authored, 5% generated (stability)
- Mid game: 60% authored, 40% generated (variety)
- Late game: 20% authored, 80% generated (infinite)

### Variety & Replayability

**Build-Driven Content Access:**
- Investigation Specialist: Dominates Mental, sees Insight-gated options
- Social Diplomat: Dominates Social, unlocks rapport-gated branches
- Different builds experience content through different lenses

**Procedural Entity Variation:**
- "Morning Conversation" needs Facilitator + Informed NPC
- Playthrough 1: Elena (innkeeper)
- Playthrough 2: Thomas (foreman)
- Playthrough 3: Generated NPC with properties

**Emergent Narrative:**
- Your story emerges from choices under scarcity
- Relationship prioritization
- Build specialization
- Resource trade-offs
- Strategic priorities

---

## Player Personas for Testing

### Persona 1: The Optimizer

**Playstyle:**
- Focuses on economic efficiency and resource management
- Studies route costs to maximize profit margins
- Prioritizes stat-gated paths for best rewards
- Avoids risky challenges unless expected value is positive

**Testing Focus:**
- Economic balance (are profit margins too tight/generous?)
- Route learning curve (how many travels to master a route?)
- Stat investment ROI (do stat increases provide meaningful advantages?)
- Resource tracking clarity (can player calculate multi-step strategies?)

**Scenarios:**
- Optimize delivery cycle to net maximum profit
- Learn 3 routes to face-up all fixed segments
- Calculate whether NPC relationship investment pays off
- Min-max stat distribution for specific build

**Success Metrics:**
- Can maintain positive coin balance over 10 days
- Accurately predicts resource costs before committing
- Identifies optimal paths through situations
- Understands compound benefits of early investments

### Persona 2: The Storyteller

**Playstyle:**
- Prioritizes narrative coherence and character development
- Chooses options that fit character concept over optimization
- Invests in NPC relationships for story, not mechanics
- Explores all dialogue options and observation scenes

**Testing Focus:**
- Narrative consistency (do choices feel thematically coherent?)
- Character voice (do NPCs feel distinct and believable?)
- Emotional resonance (do impossible choices create meaningful tension?)
- World believability (does fiction justify mechanics?)

**Scenarios:**
- Play as "noble traveler" always choosing honorable paths
- Play as "pragmatic survivor" always choosing cheap/fast paths
- Build deep relationship with one NPC across multiple interactions
- Explore all conversation trees before progressing main story

**Success Metrics:**
- Choices support consistent character concept
- NPC personalities remain consistent across interactions
- World lore feels coherent and internally consistent
- Narrative pacing feels appropriate (not rushed, not dragging)

### Persona 3: The Tactician

**Playstyle:**
- Seeks out tactical challenges (Social/Mental/Physical)
- Experiments with card combinations and strategies
- Studies resource management within challenges
- Pushes limits of challenge systems

**Testing Focus:**
- Challenge balance (too easy/hard? Solvable with skill?)
- Card variety and interestingness
- Resource tension (do thresholds create pressure?)
- Failure consequences (punishing but fair?)

**Scenarios:**
- Complete Social challenge with minimal Doubt accumulation
- Win Mental challenge while managing Exposure < 50%
- Discover optimal card combos in Physical challenges
- Intentionally fail challenge to test failure paths

**Success Metrics:**
- Challenges feel skillful (not pure luck)
- Resource management creates interesting decisions
- Card effects are clear and understandable
- Victory feels earned, failure feels fair

### Persona 4: The Explorer

**Playstyle:**
- Prioritizes discovering new locations and content
- Travels unknown routes despite risks
- Interacts with all NPCs to discover new scenes
- Explores all locations thoroughly before moving on

**Testing Focus:**
- World traversability (can reach all locations?)
- Content discovery pacing (how quickly do players find content?)
- Procedural generation quality (does generated content feel hand-crafted?)
- Navigation clarity (always know where to go?)

**Scenarios:**
- Visit every location in starting region
- Travel every available route at least once
- Interact with every NPC discovered
- Find all observation scenes at a location

**Success Metrics:**
- All locations accessible from game start (no soft-locks)
- Generated content feels appropriate and polished
- Navigation hints are sufficient (not lost/confused)
- Exploration feels rewarding (discovery of interesting content)

### Persona 5: The Struggler (Stress Tester)

**Playstyle:**
- Makes suboptimal choices intentionally
- Accepts poor outcomes (fallback paths)
- Depletes resources to near-zero
- Tests edge cases and boundary conditions

**Testing Focus:**
- Soft-lock prevention (always forward progress?)
- Failure state handling (what happens at 0 coins/health?)
- Fallback path viability (can progress with worst choices?)
- Edge case bugs (off-by-one, null references, etc.)

**Scenarios:**
- Spend all coins, attempt actions requiring payment
- Deplete health/hunger to critical levels
- Choose only fallback paths for entire session
- Fail all challenges deliberately
- Navigate with minimal/no stats

**Success Metrics:**
- Game never soft-locks (always has valid action)
- Failure states are clear and recoverable
- Fallback paths are slow but viable
- Edge cases don't crash or break game state

---

## Testing Agents and Responsibilities

### Agent 1: Economic Analyst

**Responsibility:** Verify economic balance and resource scarcity

**Tasks:**
- Track all coin inflows (delivery payments, quest rewards)
- Track all coin outflows (food, lodging, equipment, services)
- Calculate net profit margins per delivery cycle
- Verify tight economic pressure exists (margins are razor-thin)
- Test equipment upgrade affordability (requires many successful deliveries)
- Verify emergency costs can threaten savings

**Automated Checks:**
```javascript
// Track resources over 10 delivery cycles
let cycles = [];
for (let i = 0; i < 10; i++) {
  let startCoins = getCurrentCoins();
  await completeDeliveryCycle();
  let endCoins = getCurrentCoins();
  cycles.push({ start: startCoins, end: endCoins, net: endCoins - startCoins });
}

// Verify economic pressure
let averageNet = cycles.reduce((sum, c) => sum + c.net, 0) / cycles.length;
expect(averageNet).toBeLessThan(10); // Tight margins
expect(averageNet).toBeGreaterThan(-5); // But not punishing
```

**Report Format:**
- Average delivery earnings
- Average survival costs
- Net profit per cycle
- Time to afford equipment upgrade
- Economic pressure rating (1-10)

### Agent 2: Narrative Consistency Checker

**Responsibility:** Verify story coherence and character consistency

**Tasks:**
- Track NPC personalities across interactions (demeanor consistency)
- Verify categorical property scaling makes narrative sense
- Check that location descriptions match properties
- Ensure choice consequences align with fiction
- Verify relationship progression feels earned

**Automated Checks:**
```javascript
// Track NPC demeanor consistency
let npcInteractions = {};
await interactWithNPC('Elena');
npcInteractions['Elena'] = { demeanor: extractDemeanor(), interactions: 1 };

// Later interaction
await interactWithNPC('Elena');
let newDemeanor = extractDemeanor();
expect(newDemeanor).toBe(npcInteractions['Elena'].demeanor); // Consistent
```

**Report Format:**
- NPCs with inconsistent characterization
- Narrative contradictions found
- Implausible mechanical scaling (friendly NPC harder than hostile)
- Recommendation: Content needing revision

### Agent 3: UI/UX Flow Tester

**Responsibility:** Verify player-facing interface clarity and usability

**Tasks:**
- Verify all costs visible before commitment (perfect information)
- Check that locked choices show clear unlock requirements
- Ensure resource displays update in real-time
- Test modal overlays don't break navigation
- Verify all screens have clear "back" or "exit" paths
- Check that error states are handled gracefully

**Automated Checks:**
```javascript
// Verify perfect information on all choices
await navigateToScene();
let choices = await getAllChoiceCards();

for (let choice of choices) {
  let consequences = await choice.getConsequences();
  expect(consequences).toBeDefined();
  expect(consequences).toContain('Costs:'); // Costs visible

  if (await choice.isLocked()) {
    let lockReason = await choice.getLockReason();
    expect(lockReason).toBeTruthy(); // Lock reason shown
  }
}
```

**Report Format:**
- UI elements missing information
- Navigation dead ends (screens without exit)
- Unclear cost/reward displays
- Broken modals or overlays
- Accessibility issues

### Agent 4: Challenge System Validator

**Responsibility:** Verify tactical challenge balance and playability

**Tasks:**
- Complete Social challenges with various strategies
- Test Mental challenge resource management (Attention/Exposure)
- Validate Physical challenge risk/reward balance
- Verify threshold achievements grant correct rewards
- Test failure paths (MaxDoubt, MaxExposure, MaxDanger)

**Automated Checks:**
```javascript
// Social challenge - verify momentum progression
await startSocialChallenge();
let initialMomentum = getMomentum();

await playCard('Remark'); // +momentum card
await clickAction('SPEAK');

let newMomentum = getMomentum();
expect(newMomentum).toBeGreaterThan(initialMomentum); // Momentum increased

// Test threshold achievement
while (getMomentum() < 16 && getDoubt() < getMaxDoubt()) {
  await playOptimalCard();
  await clickAction('SPEAK');
}

if (getMomentum() >= 16) {
  let rewards = await getThresholdRewards();
  expect(rewards).toBeDefined();
  await verifyRewardsApplied(rewards);
}
```

**Report Format:**
- Challenge difficulty ratings (too easy/hard per tier)
- Card effectiveness rankings
- Broken victory/failure conditions
- Reward application bugs
- Balance recommendations

### Agent 5: Progression & State Tester

**Responsibility:** Verify scene progression and state management

**Tasks:**
- Test multi-situation scene flows (situations advance correctly)
- Verify scene state transitions (Provisional → Active → Completed)
- Test context-based auto-activation (situation starts when context matches)
- Verify scene completion cleanup (scenes removed from GameWorld)
- Test cross-scene dependencies (unlock one scene enables another)

**Automated Checks:**
```javascript
// Multi-situation progression
await startScene('a1_secure_lodging');
let situation1 = await getCurrentSituation();
expect(situation1.name).toContain('Negotiate');

await selectChoice('Pay coins');
await waitForProgression();

let situation2 = await getCurrentSituation();
expect(situation2.name).toContain('Rest'); // Advanced to next

await completeScene();
let sceneStillExists = await checkSceneInGameWorld('a1_secure_lodging');
expect(sceneStillExists).toBe(false); // Cleaned up
```

**Report Format:**
- Scene progression bugs (stuck situations)
- State transition errors
- Orphaned scenes (not cleaned up)
- Auto-activation failures
- Dependency chain breaks

### Agent 6: Spatial Navigation Tester

**Responsibility:** Verify world traversability and spatial coherence

**Tasks:**
- Test movement within venues (instant, free)
- Test route travel between venues (costs apply)
- Verify all locations reachable from starting position
- Test route segment progression (fixed vs event)
- Verify hex map accuracy (positions match actual locations)

**Automated Checks:**
```javascript
// Verify all locations reachable
let startLocation = getCurrentLocation();
let allLocations = await getAllLocations();
let reachable = [];

for (let location of allLocations) {
  if (await canReachLocation(location, maxDepth=5)) {
    reachable.push(location);
  }
}

// All authored locations should be reachable
expect(reachable.length).toBe(allLocations.length);

// Verify route travel costs time
let initialTime = getCurrentTimeSegment();
await selectRoute('Forest Path');
await completeRoute();
let finalTime = getCurrentTimeSegment();
expect(finalTime).toBeGreaterThan(initialTime); // Time consumed
```

**Report Format:**
- Unreachable locations (soft-locks)
- Route traversal bugs
- Spatial inconsistencies (hex positions wrong)
- Movement cost errors
- Navigation clarity issues

### Agent 7: Content Generation Observer

**Responsibility:** Monitor procedural generation quality

**Tasks:**
- Track which content is authored vs generated
- Verify generated NPCs have appropriate properties
- Verify generated locations match categorical requirements
- Test FindOrCreate pattern (reuses before generating)
- Check generated content persists correctly

**Automated Checks:**
```javascript
// Track content sources
let contentLog = { authored: [], generated: [] };

await triggerSceneNeedingLocation({ privacy: 'Private', safety: 'Safe' });
let location = await getScenePlacementLocation();

if (await isAuthoredLocation(location)) {
  contentLog.authored.push(location);
} else {
  contentLog.generated.push(location);
  // Verify properties match requirements
  expect(location.privacy).toBe('Private');
  expect(location.safety).toBe('Safe');
}

// Verify reuse preference
await triggerSameSceneAgain();
let location2 = await getScenePlacementLocation();
expect(location2.id).toBe(location.id); // Reused, not regenerated
```

**Report Format:**
- Authored vs generated content ratio
- Generated content quality assessment
- Categorical match failures
- Persistence bugs (generated content lost)
- Reuse pattern violations

---

## Decision Points and Options

### Key Decision Categories

#### 1. Resource Allocation (Constant)

**"What do I spend my limited resources on?"**

**Coin Decisions:**
- Equipment upgrades (60+ coins, permanent improvement)
- Emergency fund (buffer for unexpected costs)
- NPC gifts (relationship building)
- Services (healing, information, passage)

**Time Decisions:**
- Deliveries (primary income, 3-5 segments)
- NPC interactions (relationship investment, 2 segments)
- Exploration (discovery, 2 segments)
- Equipment shopping (1 segment)

**Energy/Hunger Decisions:**
- Push through encounters (spend resources)
- Rest and recover (spend time/coins)
- Optimize route choices (minimize consumption)

**Stat Points:**
- Specialize 2-3 stats (peaks of excellence, valleys of weakness)
- Generalize (adequate everywhere, excel nowhere)

#### 2. Risk Assessment (Per Encounter)

**"Should I gamble on optimal path or take safe path?"**

**Stat-Gated Path:**
- Best rewards, zero cost (if stat requirement met)
- Investment opportunity (1 point short, invest for future?)
- Progression question (will I use this stat again?)

**Money-Gated Path:**
- Reliable, expensive
- Economic question (can I afford this?)
- Opportunity cost (coins not available elsewhere)

**Challenge Path:**
- Variable rewards based on performance
- Risk question (success likely? Failure affordable?)
- Skill question (confident in tactical play?)

**Fallback Path:**
- Guaranteed progression, poor outcome
- Efficiency question (acceptable outcome?)
- Desperation option (only choice with no resources)

#### 3. Build Identity (Long-term)

**"What kind of traveler am I becoming?"**

**Investigator (Insight + Cunning):**
- Dominates Mental challenges
- Solves mysteries easily
- Struggles in social situations
- Forced into expensive paths when lacking social stats

**Diplomat (Rapport + Diplomacy):**
- Dominates Social challenges
- Befriends NPCs easily
- Weak in investigations
- Misses Mental-gated content

**Leader (Authority + Diplomacy):**
- Balanced Social capabilities
- Commands respect
- Vulnerable in subtle investigations
- Can't spot hidden details

**Economic Optimizer:**
- Focuses on route mastery
- Maximizes profit margins
- Minimal stat investment
- Financial flexibility

**Relationship Specialist:**
- Invests heavily in NPC bonds
- Unlocks shortcuts and discounts
- Long-term compounding benefits
- Short-term resource pressure

#### 4. Temporal Priority (Daily)

**"What should I do today with limited time?"**

**Morning Planning:**
- Accept delivery (income priority)
- Build NPC relationship (investment priority)
- Explore location (discovery priority)
- Purchase equipment (advancement priority)
- Must reserve time for survival needs

**Example Decision:**
```
Morning: 6 time segments available
- Delivery option: 4 segments, earn 20 coins
- NPC scene: 2 segments, relationship +1
- Survival needs: Food + lodging = must do before sleep

Choice A: Delivery (4) + Survival (evening) = income focus
Choice B: NPC (2) + Delivery (4) = balanced but tight
Choice C: NPC (2) + NPC (2) + Explore (2) = investment/discovery, no income
```

#### 5. Investment Timing (Strategic)

**"Invest now for future benefit or prioritize immediate survival?"**

**Equipment Upgrade:**
- Cost: 60 coins
- Requires: 12 successful deliveries
- Benefit: Permanent stat boost or resource efficiency
- Question: Delay other purchases for how long?

**NPC Relationship:**
- Cost: Multiple time investments (2-4 scenes)
- Benefit: Level 2 route shortcuts, Level 3 discounts, Level 4 stat training
- Question: Worth delaying deliveries for compound benefits?

**Route Learning:**
- Cost: Risky first travels with potential failures
- Benefit: Known segments enable perfect resource planning
- Question: Accept short-term losses for long-term profit?

**Emergency Fund:**
- Cost: Delayed equipment purchases
- Benefit: Safety net for unexpected events
- Question: How much buffer is enough?

#### 6. Route Selection (Per Delivery)

**"Which route should I travel?"**

**Known Route (Learned):**
- Fixed segments face-up (exact costs visible)
- Can plan perfect resource allocation
- Predictable profit calculation
- Safe, reliable, possibly slower

**Unknown Route (Risky):**
- Higher reward payment
- Uncertain challenges
- Risk of resource depletion
- Potential for failure
- Exploration benefit (learning new route)

**Long Safe Route:**
- More segments, easier challenges
- Reliable but time-consuming
- Lower resource consumption
- Lower time efficiency

**Short Risky Route:**
- Fewer segments, harder challenges
- Gamble for efficiency
- Higher resource consumption possible
- Higher time efficiency if successful

### Choice Presentation Pattern

**All choices follow this display format:**

```
CHOICE NAME
  Brief description of option

CONSEQUENCES:
  Costs:
    - Resolve: X → Y (-Z)
    - Coins: X → Y (-Z)
    - Health: X → Y (-Z)
    - [Other resources...]
    - Time: X segments

  Rewards:
    - Coins: +X (final: Y)
    - [Resource name]: +X (final: Y)
    - [Stat name]: +X XP (final: Y)
    - Relationship: [NPC] +X (final: Y)
    - Unlock: [Location/Scene/Item]

  [If locked]
  LOCKED:
    Requires:
      - [Stat] ≥ X (you have Y)
      OR
      - [Item] in inventory
    Insufficient resources for cost
```

**Perfect Information Guarantee:**
- All costs shown with current and final values
- All requirements shown with gap to player's current state
- All rewards shown with immediate and long-term effects
- All locks show alternative unlock paths (OR operators)

---

## What to Look Out For

### Critical Issues (Break Playability)

**Soft-Locks (Unwinnable States):**
- Situation with no affordable choices and no fallback
- Location with no exit actions
- Scene that won't complete/advance
- Resources depleted with no recovery path

**State Corruption:**
- Resources become negative or NaN
- Scenes duplicated or orphaned
- NPCs appear at wrong locations
- Time progresses incorrectly

**Navigation Breaks:**
- Screens don't transition
- Buttons unresponsive
- Modals won't close
- Back/exit paths missing

**Progression Blockers:**
- Required scenes don't spawn
- Tutorial won't advance
- Unlocked content stays locked
- Completed scenes reappear

### Major Issues (Significantly Degrade Experience)

**Economic Imbalance:**
- Impossible to maintain positive coin balance
- Equipment upgrades unaffordable within reasonable time
- Survival costs too high/low relative to earnings
- Profit margins don't create tension

**Challenge Balance:**
- Challenges trivially easy or impossibly hard
- Victory resources accumulate too quickly/slowly
- Consequence resources threaten failure too early/late
- Card effects unclear or broken

**UI/UX Problems:**
- Costs/rewards not visible before commitment (violates perfect information)
- Lock reasons unclear or missing
- Resource displays don't update
- Important information hidden or hard to find

**Content Issues:**
- NPCs behave inconsistently
- Generated content feels inappropriate
- Locations unreachable from start
- Routes have broken segments

### Minor Issues (Polish & Quality)

**Narrative Inconsistencies:**
- NPC dialogue contradicts earlier statements
- Location descriptions don't match properties
- Choice outcomes don't match expectations
- Tone shifts inappropriately

**Visual/Presentation:**
- Icons missing or wrong
- CSS classes not applying
- Layout breaks on certain screens
- Text truncated or overflows

**Performance:**
- Slow load times
- Laggy UI updates
- Memory leaks over time
- Browser console errors

**Edge Cases:**
- Boundary conditions not handled (0 resources, max values)
- Rare sequences cause unexpected behavior
- Unusual player choices break assumptions

### Positive Signals (What Good Looks Like)

**Economic Tension:**
- Close calls between profit and loss
- Meaningful decisions about spending vs saving
- Equipment upgrades feel earned
- Emergency costs create genuine pressure

**Strategic Depth:**
- Multiple valid approaches to situations
- Build specialization creates distinct experiences
- Route mastery provides measurable advantage
- Impossible choices feel genuinely difficult

**Tactical Engagement:**
- Challenges require thought and strategy
- Card combinations create interesting decisions
- Resource management adds tension
- Victory feels earned, failure feels fair

**Narrative Coherence:**
- NPCs feel like consistent characters
- World lore makes sense
- Choice consequences align with fiction
- Emotional resonance from impossible choices

**Player Guidance:**
- Always know what actions are available
- Always know costs before committing
- Always know why something is locked
- Always have path to progress (even if suboptimal)

---

## Testing Scenarios by Loop

### SHORT LOOP Testing (Encounter-Choice-Consequence)

**Scenario 1: Basic Choice Execution**
- Encounter muddy road segment
- Verify 4 choices visible (push through, detour, hire guide, athletic leap)
- Verify costs shown for each (energy, time, coins, stat requirement)
- Select "Push through" (-3 energy, -1 time)
- Verify resources deduct immediately
- Verify progression to next segment

**Scenario 2: Stat-Gated Path**
- Encounter choice requiring Rapport 3
- Verify shows "Rapport ≥ 3 (you have 2)" if insufficient
- Verify choice is locked with clear indicator
- Increase Rapport to 3 (via training or challenge)
- Return to choice, verify now unlocked
- Select stat-gated path, verify best rewards granted

**Scenario 3: Challenge Escalation**
- Encounter choice with "StartChallenge" ActionType
- Select challenge option
- Verify transition to tactical layer (Social/Mental/Physical screen)
- Verify challenge resources initialize correctly
- Play challenge to completion
- Verify return to strategic layer with outcomes applied

**Scenario 4: Fallback Path Viability**
- Deplete resources to near-zero (minimal coins, energy, stats)
- Encounter choice with fallback option
- Verify fallback has no requirements
- Select fallback, verify progression despite poor outcome
- Confirm game doesn't soft-lock with no resources

### MEDIUM LOOP Testing (Delivery Cycle)

**Scenario 5: Complete Delivery Cycle**
1. Wake at starting location (morning)
2. Look around, find merchant/job board
3. Accept delivery contract (note payment: 20 coins)
4. Verify active delivery banner appears in header
5. Select travel to destination
6. Navigate route (3-6 segments)
7. Arrive at destination
8. Complete delivery (earn 20 coins)
9. Purchase food (-10 coins)
10. Purchase lodging (-15 coins)
11. Sleep to advance day
12. Calculate net: +20 -10 -15 = -5 coins (confirm economic pressure)

**Scenario 6: Route Learning**
1. Select unknown route (segments face-down)
2. Navigate first travel, encountering unknown challenges
3. Possibly fail some segments (insufficient resources)
4. Complete route, note which segments were fixed vs event
5. Travel same route again
6. Verify fixed segments now face-up showing costs
7. Plan perfect resource allocation
8. Complete route successfully with optimized choices
9. Confirm profit improved due to learning

**Scenario 7: NPC Relationship Investment**
1. Identify NPC with valuable bond rewards (route shortcut at Level 2)
2. Invest time in NPC interaction scene (2 segments)
3. Verify relationship cube increases (+1)
4. Repeat until Level 2 reached
5. Verify mechanical benefit unlocked (route shortcut available)
6. Use shortcut on next delivery
7. Calculate time saved per delivery
8. Confirm investment pays for itself over multiple deliveries

**Scenario 8: Economic Crisis Recovery**
1. Deplete coins to near-zero through poor choices
2. Cannot afford food or lodging
3. Identify survival options:
   - Cheap food alternative (lower quality, less hunger restoration)
   - Free shelter (common room, poor energy restoration)
   - Desperate delivery (risky route for quick coins)
4. Execute recovery plan
5. Rebuild coin buffer
6. Confirm crisis was challenging but recoverable

### LONG LOOP Testing (Character Development)

**Scenario 9: Build Specialization (Investigator)**
1. Start new game
2. Prioritize Insight and Cunning stat increases
3. Accept Mental challenges whenever offered
4. Track win rate in Mental challenges (should be high)
5. Encounter Social challenge
6. Attempt with low Rapport/Diplomacy
7. Confirm difficulty (struggle or forced to expensive paths)
8. Track content accessibility:
   - Insight-gated choices frequently available
   - Social-gated choices often locked
9. Evaluate distinct experience from specialization

**Scenario 10: Build Specialization (Diplomat)**
1. Start new game
2. Prioritize Rapport and Diplomacy stat increases
3. Accept Social challenges whenever offered
4. Track win rate in Social challenges (should be high)
5. Encounter Mental challenge
6. Attempt with low Insight/Cunning
7. Confirm difficulty (struggle or multiple attempts)
8. Track content accessibility:
   - Social-gated choices frequently available
   - Insight-gated choices often locked
9. Compare experience to Investigator (should feel distinctly different)

**Scenario 11: Equipment Progression**
1. Track starting resources and capabilities
2. Execute optimized delivery cycles
3. Save profits toward equipment upgrade (60 coins)
4. Count deliveries required (expected: 10-15 with optimization)
5. Purchase equipment
6. Verify permanent benefit applied (stat boost or efficiency gain)
7. Measure impact on subsequent deliveries
8. Confirm progression feels earned and impactful

**Scenario 12: World Expansion**
1. Start in tutorial region (Brass Bell Inn, Town Square)
2. Complete A-story tutorial scenes (a1, a2, a3)
3. Progress to next A-story phase
4. Verify new region unlocks
5. Explore new region:
   - New locations discovered
   - New NPCs introduced
   - New routes generated
6. Confirm authored + procedural content mix
7. Verify seamless integration (no "seams" between authored/generated)

### TACTICAL LAYER Testing (Challenge Systems)

**Scenario 13: Social Challenge (Full Session)**
1. Enter conversation with NPC
2. Initial state: Momentum 0, Cadence 0, Doubt 0
3. Play SPEAK cards to build Momentum
4. Monitor Cadence (Listen/Speak balance)
5. Monitor Doubt accumulation (consequence)
6. Reach first threshold (8 Momentum)
7. Verify rewards applied
8. Continue to second threshold (12 Momentum)
9. Achieve third threshold (16 Momentum) or MaxDoubt
10. Exit challenge, verify scene progression

**Scenario 14: Mental Challenge (Investigation)**
1. Enter investigation scene at location
2. Initial state: Progress 0, Exposure 0, Attention full
3. ACT actions to generate Leads
4. OBSERVE to draw Details
5. Play Details to build Progress
6. Monitor Exposure accumulation
7. Reach partial threshold (10 Progress)
8. Verify partial rewards
9. Leave investigation mid-session
10. Return later with refreshed Attention
11. Complete full threshold (20 Progress)

**Scenario 15: Physical Challenge (Obstacle)**
1. Enter obstacle scene
2. Initial state: Breakthrough 0, Aggression 0, Danger 0
3. EXECUTE to lock Options as combo
4. ASSESS to trigger combo
5. Monitor Aggression scale (balance)
6. Monitor Danger accumulation (risk)
7. Reach breakthrough threshold (20)
8. Verify success rewards
9. Test failure path (let Danger reach max)
10. Verify failure consequences apply

**Scenario 16: Challenge Failure Recovery**
1. Enter Social challenge
2. Deliberately play poorly (wrong cards, bad rhythm)
3. Allow Doubt to reach MaxDoubt
4. Verify failure state triggers
5. Exit challenge with failure outcomes
6. Verify failure rewards/penalties applied
7. Check scene progression (does failure path exist?)
8. Confirm failure is setback, not game-over

### CONTENT GENERATION Testing

**Scenario 17: Procedural NPC Generation**
1. Trigger scene requiring specific NPC properties
   - Example: Facilitator + Informed + Commoner
2. Verify system finds appropriate NPC from existing pool (if authored match exists)
3. If no match: verify system generates new NPC with exact properties
4. Interact with generated NPC
5. Verify properties manifest correctly (friendly demeanor = lower thresholds)
6. Confirm generated NPC persists in GameWorld
7. Return to NPC later, verify consistency

**Scenario 18: Procedural Location Generation**
1. Trigger scene requiring specific location properties
   - Example: Private + Safe + Dwelling
2. Verify system searches existing locations first
3. If no match: verify new location generated with properties
4. Visit generated location
5. Verify categorical properties reflected in description/atmosphere
6. Confirm location persists permanently
7. Verify location accessible via navigation

**Scenario 19: Categorical Scaling Verification**
1. Negotiate with Friendly NPC at Basic venue
   - Stat requirement: expect ~3 (0.6× × 0.7× scaling)
   - Coin cost: expect ~7
2. Negotiate with Hostile NPC at Luxury venue
   - Stat requirement: expect ~7 (1.4× × 2.5× scaling)
   - Coin cost: expect ~25
3. Verify scaling makes narrative sense
4. Confirm mathematical consistency across situations

**Scenario 20: Route Segment Learning**
1. Select unknown route (all segments face-down)
2. First travel: Encounter 5 segments
   - 3 fixed environmental (reveal, become face-up)
   - 2 event segments (remain face-down)
3. Note which segments flipped face-up
4. Second travel: Same route
   - Verify 3 fixed segments show costs before entry
   - Verify 2 event segments still face-down
   - Verify event segments present different encounters
5. Third travel: Confirm route personality established

---

## Known Content Inventory

### Starting Region Locations

**The Brass Bell Inn (Venue):**
- Common Room (SemiPublic, Safe, Moderate, Dwelling) - Tutorial starting location
- Upper Floor (Private, Safe, Quiet, Dwelling) - Lodging destination
- Possibly: Cellar, Kitchen, Private Room (generated as needed)

**Town Square (Venue):**
- Square Center (Public, Neutral, Busy, Transit) - Crossroads hub
- Fountain Plaza (Public, Neutral, Moderate, Commerce) - Merchant area
- Possibly: Alleyways, Market Stalls (generated)

**The Old Mill (Venue):**
- Mill Courtyard (Public, Neutral, Moderate, Commerce) - Entrance
- Riverside Dock (Public, Neutral, Quiet, Transit) - Travel point

### Tutorial NPCs

**Elena (Innkeeper):**
- Profession: Innkeeper
- Demeanor: Friendly (0.6× thresholds)
- Social Standing: Notable
- Story Role: Facilitator
- Location: Common Room (The Brass Bell Inn)
- Tutorial Scene: "a1_secure_lodging" - Negotiate lodging, rest, depart

**Merchant NPCs:**
- Various merchants for delivery contracts
- Location: Town Square, Fountain Plaza
- Tutorial Scene: "a2_morning" - Accept first delivery

### Tutorial Scenes (A-Story)

**Scene a1: "Secure Lodging at Inn"**
- Archetype: inn_lodging (service_with_location_access)
- Placement: Inn location + Innkeeper NPC
- Situations:
  1. Negotiate lodging (4 choices: pay coins, use Rapport, social challenge, fallback)
  2. Rest in private room (3 choices: sleep 8hr, nap 2hr, examine)
  3. Depart room (2 choices: careful departure, rush out)

**Scene a2: "Find First Delivery"**
- Archetype: delivery_contract
- Placement: Commerce location + Merchant NPC
- Situations:
  1. Review available contracts (job board)
  2. Accept delivery (commitment)

**Scene a3: "Travel to Destination"**
- Archetype: route_travel
- Placement: Route context
- Situations: Sequential route segments (varies by route chosen)

### Card Inventory

**Social Cards (25+ cards):**
Core conversation mechanics for Social challenges
- Remark (basic momentum builder)
- Appeal to Sympathy (emotional resonance)
- Charm (rapport building)
- Intimidate (conflict escalation)
- Deflect (defensive)
- Compliment (positive interaction)
- [19+ more cards with various effects]

**Mental Cards (15+ cards):**
Investigation mechanics for Mental challenges
- Examine (basic progress builder)
- Deduce (reasoning)
- Recall (memory)
- Cross-Reference (connection finding)
- [11+ more cards]

**Physical Cards (15+ cards):**
Obstacle mechanics for Physical challenges
- Strike (basic breakthrough)
- Feint (tactical)
- Brace (defensive)
- Rush (aggressive)
- [11+ more cards]

### Equipment/Items

**Starting Equipment:**
- Basic clothing
- Small coin purse
- Travel rations (initial food)

**Available for Purchase:**
- Better equipment (stat boosts, 60+ coins)
- Food items (hunger restoration, 8-12 coins)
- Lodging services (energy restoration, 7-15 coins)
- Various trade goods

### Routes (Initially Available)

Routes generated procedurally between venues based on hex pathfinding.

**Expected Routes:**
- Inn ↔ Town Square (short, safe)
- Town Square ↔ Mill (medium, moderate danger)
- Inn ↔ Mill (longer, wilderness route)

### Progression Systems

**Player Stats (Five Stats):**
- Insight (Mental challenges, investigation)
- Rapport (Social challenges, friendly interactions)
- Authority (Social challenges, commanding respect)
- Diplomacy (Social challenges, balanced approach)
- Cunning (Mental challenges, deception/clever solutions)

**NPC Relationships (StoryCubes 0-10):**
- Level 0-1: Disconnected/Guarded
- Level 2: Route shortcuts unlocked
- Level 3: Merchant discounts unlocked
- Level 4: Stat training available
- Level 5: Exclusive access granted

**Location Mastery (InvestigationCubes 0-10):**
- Discover hidden details
- Unlock observation scenes
- Faster investigation at familiar locations

**Route Familiarity (ExplorationCubes 0-10):**
- Reveal fixed segments
- Better cost prediction
- Potential shortcuts discovered

### States (Temporary Conditions)

**Positive States:**
- Inspired (stat bonus)
- Well-Rested (energy efficiency)
- Fed (hunger buffer)

**Negative States:**
- Wounded (health penalty)
- Exhausted (energy efficiency penalty)
- Hungry (stat penalties)
- Demoralized (social penalty)

### Achievements

**Early Achievements:**
- First Steps (complete tutorial)
- Silver Tongue (win Social challenge)
- Sharp Mind (win Mental challenge)
- Steady Hand (win Physical challenge)
- Profitable (complete delivery with profit)
- Befriended (reach relationship Level 2)

---

## Summary for Next Session

### Quick Reference

**How to Run:**
```bash
cd src && dotnet build
ASPNETCORE_URLS="http://localhost:5000" dotnet run
```

**First 15 Minutes:**
1. Secure lodging at inn (negotiate → rest → depart)
2. Find delivery contract (accept job)
3. Travel to destination (navigate route segments)
4. Experience economic pressure (barely profitable)

**Key Testing Focus:**
- Economic balance (tight margins, equipment affordable?)
- Perfect information (all costs visible?)
- No soft-locks (always forward path?)
- Build specialization (distinct experiences?)
- Content generation quality (appropriate and polished?)
- Challenge balance (skillful, not luck-based?)

**Player Personas:**
1. The Optimizer (economic efficiency)
2. The Storyteller (narrative coherence)
3. The Tactician (challenge mastery)
4. The Explorer (world discovery)
5. The Struggler (edge case testing)

**Testing Agents:**
1. Economic Analyst (resource balance)
2. Narrative Consistency Checker (story coherence)
3. UI/UX Flow Tester (interface clarity)
4. Challenge System Validator (tactical balance)
5. Progression & State Tester (scene flow)
6. Spatial Navigation Tester (world traversability)
7. Content Generation Observer (procedural quality)

**Critical Success Metrics:**
- Can complete 10 delivery cycles without soft-lock
- Economic pressure creates tension but not frustration
- Different builds experience distinctly different content
- All choices show perfect information before commitment
- Generated content feels hand-crafted
- Challenges feel skillful, victories earned

---

**Document Version:** 1.0
**Last Updated:** Session Preparation
**Next Steps:** Execute comprehensive playtest with all personas and agents
