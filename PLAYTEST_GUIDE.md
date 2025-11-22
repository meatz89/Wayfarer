# Wayfarer Playtest Guide

**Mission:** Test the core player experience flow to verify the game is playable, engaging, and free of critical issues.

**Status:** Building iteratively with verified facts...

---

## Why This Guide Exists

The next session will test Wayfarer holistically using Playwright automation and multiple player personas. This guide provides:
- Verified facts about what exists in the game RIGHT NOW
- Working commands and selectors tested against actual code
- Clear testing objectives with success criteria
- Specific issues to look for based on design intent

Everything here is verified against code or documentation. No assumptions.

---

## How to Run the Game

**Technology:** ASP.NET Core Blazor Server (verified: Program.cs contains `builder.Services.AddServerSideBlazor()`)

**Build Command:**
```bash
cd src
dotnet build
```

**Run Command:**
```bash
cd src
ASPNETCORE_URLS="http://localhost:5000" dotnet run
```

**What Happens on Startup:**
1. GameWorld initialization via `GameWorldInitializer.CreateGameWorld()` (verified: Program.cs contains `GameWorld gameWorld = GameWorldInitializer.CreateGameWorld()`)
2. Content loading from JSON files in `Content/Core/` directory
3. Serilog console logging starts
4. Server listens on http://localhost:5000
5. Navigate browser to http://localhost:5000

**Prerequisites:**
- **.NET 8.0 SDK required** (verified: Wayfarer.csproj specifies `net8.0`)
- Browser: Chrome, Firefox, or Edge (latest version)
- Disk space: ~500MB for .NET SDK + game assets

**Automated vs Manual Testing:**
- **Automated tests** (Wayfarer.Tests.Project/) verify structural guarantees:
  - Tutorial spawning (TutorialInnLodgingIntegrationTest.cs)
  - Soft-lock prevention (FallbackPathTests.cs)
  - Economic structure (AStoryPlayerExperienceTest.cs)
  - Run via: `cd Wayfarer.Tests.Project && dotnet test`

- **Manual playtesting** (this guide) tests experiential qualities:
  - Does scarcity FEEL emergent over 2 hours?
  - Do builds create DISTINCT experiences?
  - Is strategic pressure meaningful?

**Run automated tests FIRST** to catch breaking issues, THEN manual playtest for experience validation.

**Expected Console Output on Success:**
```
info: Content loading...
info: GameWorld initialized
info: Now listening on: http://localhost:5000
```

**If Startup Fails:**

**GameWorld initialization error:**
```bash
# Check Content/Core/ exists and contains JSON files
ls -la src/Content/Core/*.json
# Should show 19 files (01_foundation.json through 22_tutorial_challenges.json)

# Run automated tests to isolate issue
cd Wayfarer.Tests.Project && dotnet test --filter "Tutorial"
```

**Port conflict (Address already in use):**
```bash
# Use different port
ASPNETCORE_URLS="http://localhost:6000" dotnet run
```

**Tutorial scene doesn't spawn:**
- Automated test verifies this (run `dotnet test --filter "Tutorial_LoadsSceneTemplates"`)
- Check browser console for JavaScript errors
- Verify GameWorld.IsGameStarted = true in debugger
- Check GameFacade.StartGameAsync() was called

**Reset game state:**
- Restart server (Ctrl+C, then rerun)
- Clear browser localStorage: F12 → Application → Local Storage → Clear
- Delete generated content: `rm -rf src/Content/Dynamic/*`

**Report bugs:**
- GitHub Issues: https://github.com/meatz89/Wayfarer/issues
- Include: Console logs, browser console errors, steps to reproduce
- Label: `playtest` for issues found during playtesting

---

## What is Wayfarer?

**Genre:** Choice-picking narrative game like **The Life and Suffering of Sir Brante** with infinite journey structure like **Frieren: Beyond Journey's End**

**Core Experience:** Strategic depth through impossible choices, not mechanical complexity. (design/01_design_vision.md:11)

**You are:** A traveler making difficult choices where your past decisions constrain current options. NOT a hero. You pick from available choices, invest in stats that unlock some paths while closing others, and live with cumulative consequences of specialization.

**CRITICAL DISTINCTION - Two Layers:**

### Tactical Layer (Individual Choices)
**What it is:** Each individual choice-picking moment you encounter

**How it feels:**
- Often has clear optimal path (not every choice is "hard")
- Some choices are all positive (pure rewards)
- Some choices are all negative (pick your poison)
- **You will NOT soft-lock from one bad choice**
- Perfect information lets you see exactly what you're getting

**NOT tested:** "Is this individual choice difficult enough?"

### Strategic Layer (Cumulative Consequences)
**What it is:** The pattern that emerges across 20+ choices over hours of play

**How it feels:**
- "I invested in Insight for 2 hours, NOW Social situations cost me dearly"
- "I spent coins on equipment, NOW I can't afford the NPC relationship"
- "I chose Route A ten times, NOW I'm specialized but vulnerable elsewhere"
- **The Essential Feeling:** "I can access path A OR path B based on my stats, not both. My past stat investments created this constraint."

**IS tested:** "Does resource scarcity emerge across hours? Do early choices constrain later options? Does specialization create distinct multi-hour experiences?"

---

## What Playtesters Should Understand

### 1. Test HOLISTICALLY (Hours, Not Minutes)
**Don't test:** Individual choice difficulty
**Do test:** Does scarcity emerge after 15 deliveries? Does my build (chosen across 20 stat investments) create distinct experience?

**Play for MINIMUM 2-4 hours** to see strategic patterns emerge.

### 2. Stat Gating Creates Constraint
**Don't test:** "This individual choice is easy"
**Do test:** "After 2 hours, do I see choices I WANT but can't take due to stat requirements? Am I blocked from entire content areas due to specialization?"

**Past stat allocation creates CURRENT constraints** - specialization matters over hours.

### 3. Specialization is EMERGENT (Not Immediate)
**Don't test:** "Insight stat doesn't immediately feel powerful"
**Do test:** "After investing in Insight 10 times across 3 hours, do Mental challenges feel easier AND Social situations feel more expensive?"

**Compare 3-hour Investigator playthrough vs 3-hour Diplomat playthrough** - distinct?

### 4. Infinite Journey (Frieren Principle)
**Don't test:** "Where's the ending?"
**Do test:** "Does it feel open-ended? Can I explore for hours without pressure to 'finish'?"

**No completion anxiety** - play at your own pace without fear of missing content.

### 5. Contemplation Not Urgency
**Don't test:** "Choices should be timed for pressure"
**Do test:** "Can I pause, calculate, and strategize? Do I trust the numbers?"

**Perfect information enables multi-step planning** - not artificial urgency.

---

## Testing Lens: STRATEGIC Not TACTICAL

**WRONG approach (tactical testing):**
- "Scene X doesn't create enough tension"
- "This choice has an obvious answer"
- "Individual encounter feels too easy"
- Testing each decision point in isolation

**RIGHT approach (strategic testing):**
- "After 2 hours, do I feel resource pressure from cumulative choices?"
- "Does Investigator build play differently than Diplomat after 3 hours?"
- "Can I afford everything I want, or am I forced to prioritize over time?"
- "Do choices made in hour 1 constrain options in hour 3?"

**Play the game holistically as a STRATEGY game about long-term resource allocation, not a tactics game about individual hard decisions.**

---

## What Actually Exists Right Now

**Verified against Content/Core JSON files (19 files total)**

### NPCs (3 verified)
- **Elena** (Innkeeper) - Notable, Obstacle, Informed - Common Room
- **Thomas** (Warehouse Foreman) - Commoner, Neutral, Ignorant - Needs work help
- **General Merchant** - Commoner, Neutral, Ignorant - Sells equipment

### Venues (3 verified)
- **The Brass Bell Inn** (Tavern) - Tutorial starting area
- **Town Square** (Market) - Trading hub
- **The Old Mill** (Wilderness) - Outskirts

### Locations (4+ verified from 01_foundation.json)
- **Town Square Center** - Start location, Public/Safe/Busy/Commerce, "You've been walking since dawn..."
- **Common Room** - Start location, SemiPublic/Safe/Moderate/Dwelling, Elena's location
- **Fountain Plaza** - Public/Safe/Busy/Commerce, "Courier's board posts delivery opportunities"
- **Mill Courtyard** - Public/Neutral/Quiet/Transit, Old mill entrance

### Content Files
1. 01_foundation.json - Venues, locations, player config
2. 02_hex_grid.json - Spatial scaffolding
3. 03_npcs.json - Elena, Thomas, Merchant
4. 04_connections.json - NPC relationship tokens
5. 06_gameplay.json - Game rules, time config
6. 07_equipment.json - Items
7. 08_social_cards.json - Conversation cards
8. 09_mental_cards.json - Investigation cards
9. 10_physical_cards.json - Obstacle cards
10. 11_exchange_cards.json - Trading inventory
11. 12_challenge_decks.json - Challenge configurations
12. 14_npc_decks.json - NPC observation decks
13. 15_conversation_trees.json - Dialogue trees
14. 16_observation_scenes.json - Investigation content
15. 18_states.json - Temporary conditions
16. 19_achievements.json - Milestones
17. 21_tutorial_scenes.json - Tutorial starters
18. 22_a_story_tutorial.json - Main story tutorial (3 scenes)
19. 22_tutorial_challenges.json - Tutorial challenge decks

---

## Tutorial Flow (First 15 Minutes)

**What Actually Happens** (verified from 22_a_story_tutorial.json):

### Scene a1: "Secure Lodging" (isStarter: true)
- **Where:** Common Room (inn with Commercial + Restful properties)
- **Who:** Elena (Innkeeper)
- **Narrative:** "Evening approaches...you're a newly arrived traveler needing shelter..."
- **Archetype:** inn_lodging (cascading situations)
- **Goal:** Learn choice system, resource costs, perfect information

### Scene a2: "First Delivery"
- **Where:** Town Square area (Commercial + Public properties)
- **Who:** General Merchant
- **Narrative:** "Morning arrives...seeking work...delivery contract awaits"
- **Archetype:** delivery_contract
- **Goal:** Accept first delivery job, understand economic loop

### Scene a3: "Journey Beyond Town"
- **Where:** Route travel (Quiet + Outdoor properties)
- **Archetype:** route_segment_travel
- **Narrative:** "The road stretches before you..."
- **Goal:** Experience route segments, resource management, route learning

**Testing Priority:** Verify all three scenes spawn, progress correctly, and teach core systems.

---

## How to Test Using Playwright

**Verified Selectors** (from actual GameScreen.razor code):

### Resource Display
```javascript
// Verified from ResourceBar.razor (line 4: .resource-item is container)
// Structure: .resource-item > .resource-label + .resource-value + .resource-bar
const health = await page.locator('.resource-item .resource-value').nth(0).textContent();
const hunger = await page.locator('.resource-item .resource-value').nth(1).textContent();
const focus = await page.locator('.resource-item .resource-value').nth(2).textContent();
const coins = await page.locator('.resource-item .resource-value').nth(4).textContent();

// Alternative: target by label (more resilient)
const health = await page.locator('.resource-item:has(.resource-label:text("Health")) .resource-value').textContent();
```

### Time Display
```javascript
// Verified: time-header contains day-display and period-name
const day = await page.locator('.day-display').textContent();
const period = await page.locator('.period-name').textContent();
```

### Active Delivery
```javascript
// Verified: active-job-banner appears when delivery active
const hasActiveJob = await page.locator('.active-job-banner').isVisible();
const payment = await page.locator('.active-job-payment').textContent();
```

### Navigation
```javascript
// Verified: fixed-buttons-container has journal-btn and map-btn
await page.click('.journal-btn');
await page.click('.map-btn');
```

### Modals
```javascript
// Verified: modal-overlay wraps modal-content
const modalOpen = await page.locator('.modal-overlay').isVisible();
// Close modal - click overlay (stopPropagation on content)
await page.click('.modal-overlay');
```

**Critical Test:** Verify perfect information - all costs must be visible BEFORE player commits to choice.

---

## Critical Issues to Test For

**Based on verified design goals** (from design/01_design_vision.md and design/02_core_gameplay_loops.md):

### 1. Soft-Locks (BREAKING)
**Why Critical:** Infinite game cannot restart. Unwinnable states = permanent failure.

**Test:**
- Deplete all resources to zero
- Verify fallback path always exists (no-requirement option)
- Check every location has exit action
- Confirm scenes don't trap player

**Expected:** Always forward progress, even if suboptimal.

### 2. Perfect Information Violation (MAJOR)
**Why Critical:** Core pillar = "see all costs before committing" (design/01_design_vision.md:95)

**Test:**
- Every choice must show exact costs BEFORE selection
- No hidden consequences that surprise player
- Requirements shown with current player value
- Final resource values displayed (Current → Final format)

**Expected:** Can calculate multi-step strategies. Trust the numbers.

### 3. Stat Gating Doesn't Constrain (MAJOR)
**Why Critical:** Stat requirements should block desired choices, creating tension (design/03_progression_systems.md:154-182)

**Test:**
- Do I see stat-gated choices I WANT but can't take?
- After specializing for 2 hours, am I blocked from non-specialized content?
- Do requirements show exact gap ("Need Insight 5, you have 3")?
- Am I forced to invest in unwanted stats to unlock gated content?

**Expected:** Specialization creates vulnerability. Investigator blocked from Social paths. Diplomat blocked from Mental paths. Requirements visible with exact gaps.

**Red Flags:**
- All choices available regardless of stats (gating not working)
- Can't see what's required to unlock choices (perfect information broken)
- Never feel constrained by past stat investments (consequences not cumulative)

### 4. Scarcity Disappears (MAJOR)
**Why Critical:** "No power fantasy" - resources must stay scarce (design/01_design_vision.md:93)

**Test:**
- Do costs still matter after 20 deliveries?
- Does player reach "unstoppable" state?
- Are late-game challenges still challenging?

**Expected:** Economic pressure persists. Never trivial.

### 5. Strategic Constraints Don't Emerge (MAJOR)
**Why Critical:** "Impossible choices" emerge from cumulative stat consequences over hours (design/01_design_vision.md:19)

**Test (STRATEGIC - requires 2-4 hour playthrough):**
- After 2 hours of Insight investment: Are Social choices NOW blocked due to low Rapport?
- After 3 hours: Do early stat allocations create CURRENT vulnerability?
- Across playthrough: Do I see choices I WANT but can't afford (stat requirements)?
- Do I feel forced to pick stat increases I don't want to unlock gated content?

**Expected:** Stat specialization creates constraint. Past stat investment blocks current desired paths. "I can access A OR B based on my stats, not both" emerges.

**Economic Pressure** (secondary to stat gating):
- After 10 deliveries, can I afford occasional equipment? (Should be tight but possible)
- Do coins create tension but NOT dominant constraint? (Stats primary, coins secondary)

**DON'T test:** Whether individual choices feel hard tactically (they often won't - and that's CORRECT for Sir Brante model).

### 6. Route Segments Don't Learn (MEDIUM)
**Why Critical:** Route mastery = core skill progression (design/02_core_gameplay_loops.md:84)

**Test:**
- First travel: Segments face-down (unknown)
- Second travel: Fixed segments face-up showing exact costs
- Third travel: Can plan perfect resource allocation

**Expected:** Learning routes enables profitable optimization.

### 7. Tutorial Doesn't Spawn (BREAKING)
**Why Critical:** a1_secure_lodging has `isStarter: true` - must appear (22_a_story_tutorial.json)

**Verified Code Path:**
1. GameFacade.StartGameAsync() called when game starts (Services/GameFacade.cs)
2. Queries `_gameWorld.SceneTemplates.Where(t => t.IsStarter)` (GameFacade.cs:1600)
3. For each starter template, calls SceneInstantiator to create Scene instance
4. Scene placed at location/NPC matching template's PlacementFilter
5. Scene becomes active, choices appear in UI

**Automated Test:** `TutorialInnLodgingIntegrationTest.Tutorial_LoadsSceneTemplates()` verifies IsStarter scenes exist

**Manual Test:**
- Game start → Scene a1 appears at Common Room with Elena
- Can complete all three tutorial scenes (a1, a2, a3)
- Each scene teaches intended system

**If fails:** Run `dotnet test --filter "Tutorial_LoadsSceneTemplates"` to isolate issue

**Expected:** Smooth onboarding, no confusion.

---

## Player Personas for Testing

**Based on verified build specializations** (from design/02_core_gameplay_loops.md, design/03_progression_systems.md):

### Persona 1: The Economic Optimizer
**Focus:** Resource management and profit maximization

**Playstyle:**
- Learn routes to face-up all fixed segments (route mastery)
- Calculate multi-step economic strategies
- Minimize costs, maximize earnings
- Avoid risky challenges unless expected value positive

**What to Test:**
- Can calculate delivery profitability accurately?
- Do margins feel tight enough to require optimization?
- Does route learning provide measurable advantage?
- Is equipment affordable within reasonable deliveries (10-15)?

**Success:** Maintains positive coins over 10 cycles through skill, not luck.

### Persona 2: The Investigator Build (Insight + Cunning)
**Focus:** Mental challenge specialization

**Playstyle:**
- Prioritize Insight and Cunning stat increases
- Seek out Mental challenges and investigations
- Expect to dominate Mental, struggle in Social
- Forced into expensive/slow paths when lacking social stats

**What to Test:**
- Do Insight-gated paths appear frequently?
- Are Social situations genuinely harder with low Rapport?
- Does specialization create distinct experience?
- Is vulnerability meaningful but not punishing?

**Success:** Mental challenges feel easy, Social situations force costly workarounds.

### Persona 3: The Diplomat Build (Rapport + Diplomacy)
**Focus:** Social challenge specialization

**Playstyle:**
- Prioritize Rapport and Diplomacy stat increases
- Build NPC relationships for mechanical benefits
- Expect to dominate Social, struggle in Mental
- Relationship investment over equipment

**What to Test:**
- Do Rapport-gated paths provide genuine advantage?
- Are Mental situations genuinely harder with low Insight?
- Does NPC relationship progression feel rewarding?
- Do bond benefits (shortcuts, discounts) justify investment?

**Success:** Social challenges feel easy, Mental situations force expensive choices.

### Persona 4: The Stress Tester (Edge Cases)
**Focus:** Breaking the game through suboptimal play

**Playstyle:**
- Deliberately make worst choices
- Deplete resources to zero
- Choose only fallback paths
- Test boundary conditions

**What to Test:**
- Does game soft-lock with zero resources?
- Do fallback paths always exist?
- Can recover from near-bankruptcy?
- Do edge cases crash or corrupt state?

**Success:** Game never breaks, always has forward path (even if slow).

**Testing Strategy:** Use different personas in parallel to cover all critical systems. Optimizer tests economy, Builds test specialization, Stress Tester tests robustness.

---

## Testing Session Quick Start

**CRITICAL: This is STRATEGIC testing requiring MINIMUM 2-4 hour playthroughs.**

Individual choices/scenes tested in isolation = WRONG. You're testing cumulative patterns across time.

**Mindset:**
- Not: "Is this choice hard?"
- But: "After 2 hours, do my choices create constraints?"

**Time commitment:**
- Minimum: 2 hours (to see economic patterns)
- Recommended: 3-4 hours (to see build specialization)
- Per persona: Run separate 3-hour sessions as Optimizer, Investigator, Diplomat

---

### 1. Setup (5 min)
```bash
cd /home/user/Wayfarer/src
dotnet build
ASPNETCORE_URLS="http://localhost:5000" dotnet run
```

Open browser: http://localhost:5000

### 2. Verify Startup (2 min)
**Check console logs:**
- Content loading messages
- GameWorld initialized
- Server listening

**Check browser:**
- Game container loads (`.game-container`)
- Time header visible (`.time-header`)
- Resources bar visible (`.resources-bar`)
- No JavaScript errors in console

### 3. Tutorial Verification (10 min)
**Scene a1 must spawn automatically:**
- Common Room location loads
- Elena (Innkeeper) present
- "Secure Lodging" scene appears
- Verify 4 choice pattern visible
- Check EVERY choice shows costs before selection

**Complete a1 → a2 → a3 tutorial sequence**

### 4. Core Experience Test: Stat Gating and Choice Consequences (2-3 hours)

**THE PRIMARY EXPERIENCE TO TEST:**

**NOT:**  "Can I calculate profit margins accurately?"
**YES:** "I can't take the choice I WANT because my Rapport is too low"

**Play for 2-3 HOURS minimum to feel cumulative stat consequences.**

**What to Track:**

**Stat Gating Moments** (write these down as they happen):
- "I saw a choice I wanted but it was blocked - needed Insight 5, I have 3"
- "I was forced to take the expensive coin path because I lack the stat-gated free path"
- "This situation has 4 choices but only 2 are available to me"
- "I picked a stat increase I didn't want just to unlock gated content I need"

**Cumulative Consequences** (after 2 hours):
- "I invested in Insight for 2 hours, NOW Social situations block me constantly"
- "My past stat choices created vulnerability I'm feeling NOW"
- "I can access Mental content easily but Social content forces expensive workarounds"

**Questions after 2-3 hours:**
1. **Do I see choices I want but can't afford (stat requirements)?**
   - How often do I encounter stat-gated choices?
   - Can I see WHAT I need to unlock them (exact stat gap visible)?
   - Do I feel motivated to invest in stats to unlock paths?

2. **Do my past stat investments constrain current options?**
   - After specializing in Investigator (Insight+Cunning), are Social paths blocked?
   - Does specialization create FELT vulnerability in non-specialized areas?
   - Do I regret past stat choices when facing current constraints?

3. **Am I forced to pick stat increases I don't want?**
   - Do I ever choose "increase Rapport" even though I'd rather increase Insight?
   - Why? (To unlock gated content, to recover from being locked out, etc.)
   - Does this feel strategic or frustrating?

4. **Do choices create strategic pressure through REQUIREMENTS not ECONOMICS?**
   - Is the tension "I can't meet the stat requirement" or "I can't afford the coins"?
   - Primary should be stat gating, economics should be secondary

**Simple Economic Check** (don't track spreadsheets):
- After 10 deliveries, do you have enough coins for survival? (Yes = working)
- Can you afford occasional equipment? (Should take ~10-15 deliveries = working)
- Do you ever run completely out of coins? (Should be rare but recoverable = working)

### 5. Perfect Information Audit (10 min)
**Every single choice across all screens:**
- Costs visible BEFORE selection?
- Requirements shown with player's current value?
- Final values displayed (Current → Final)?
- No surprise consequences?

**Test with Playwright:**
```javascript
// Verify every choice card has consequences section
const choices = await page.locator('.scene-choice-card').count();
for (let i = 0; i < choices; i++) {
  const card = page.locator('.scene-choice-card').nth(i);
  await expect(card.locator('.choice-consequences')).toBeVisible();
}
```

### 6. Soft-Lock Test (10 min)
**As Stress Tester:**
1. Spend all coins on bad choices
2. Deplete energy/hunger to critical
3. Verify fallback paths always exist
4. Confirm no unwinnable states
5. Test recovery from near-zero resources

### 7. Build Specialization Test: Stat Gating Creates Distinct Experiences (3-4 hours PER build)
**CRITICAL: Run SEPARATE 3-hour playthroughs for each build to compare stat gating patterns.**

**Session A - As Investigator (3 hours):**
1. **Stat investment:** Always pick Insight + Cunning increases when offered
2. **After 2-3 hours of cumulative stat allocation:**

   **Stat Gating Observations:**
   - How many Mental choices are stat-gated to me? (Should be HIGH - I meet requirements)
   - How many Social choices are stat-gated to me? (Should be HIGH - I DON'T meet requirements)
   - Am I forced to use expensive coin paths in Social situations because I lack Rapport?
   - Do I see choices I WANT but can't take due to low Rapport/Diplomacy?

   **The Feeling:**
   - Do I feel POWERFUL in Mental situations (many free stat-gated paths)?
   - Do I feel BLOCKED in Social situations (desired choices locked)?
   - Does my past Insight investment NOW constrain my Social options?

**Session B - As Diplomat (3 hours):**
1. **Stat investment:** Always pick Rapport + Diplomacy increases when offered
2. **After 2-3 hours of cumulative stat allocation:**

   **Stat Gating Observations:**
   - How many Social choices are stat-gated to me? (Should be HIGH - I meet requirements)
   - How many Mental choices are stat-gated to me? (Should be HIGH - I DON'T meet requirements)
   - Am I forced to use expensive coin paths in Mental situations because I lack Insight?
   - Do I see choices I WANT but can't take due to low Insight/Cunning?

   **The Feeling:**
   - Do I feel POWERFUL in Social situations (many free stat-gated paths)?
   - Do I feel BLOCKED in Mental situations (desired choices locked)?
   - Does my past Rapport investment NOW constrain my Mental options?

**Compare after BOTH 3-hour sessions:**
- **Stat gating pattern:** Did Investigator see different locked choices than Diplomat?
- **Vulnerability:** Did specialization create DISTINCT constraints?
- **Choice pressure:** Did I feel "forced to pick stat increases I don't want" in different situations?
- **Experience:** Did the game FEEL different based on what I can/can't access?

**Success = Different builds see different constraints through stat gating, not just different numbers.**

### 8. Report Critical Issues
**BREAKING (stop testing):**
- Soft-locks
- Tutorial doesn't spawn
- Crashes/state corruption

**MAJOR (continue but note):**
- Perfect information violations
- Economic imbalance over 2 hours (too easy/hard)
- Scarcity doesn't emerge across multiple playthroughs
- Strategic constraints don't appear after cumulative choices

**MEDIUM (note for later):**
- Route learning doesn't work
- Build differentiation weak
- UI unclear

---

## Success Criteria Summary

**REMEMBER: Test STRATEGICALLY across 2-4 hour playthroughs, not tactically per individual choice.**

✅ **Tutorial completes** (a1 → a2 → a3 teaches core systems)
✅ **No soft-locks** (always forward path, even with zero resources)
✅ **Perfect information** (all costs AND stat requirements visible before commitment)
✅ **Stat gating creates tension** (I see choices I WANT but can't take due to stat requirements)
✅ **Past stats constrain current choices** (2 hours of Insight investment NOW blocks Social paths)
✅ **Forced stat allocation** (picking stat increases I don't want to unlock gated content I need)
✅ **Strategic constraints EMERGE** ("I can access A OR B based on my stats, not both")
✅ **Builds differ through gating** (Investigator sees different blocked choices than Diplomat)

**Economic pressure** (secondary):
- Coins tight enough to matter but NOT dominant constraint
- Equipment affordable ~10-15 deliveries
- Can survive but feel resource pressure

**Timeframe matters:** Stat consequences emerge over HOURS through cumulative allocation, not individual encounters.

---

**Document Status:** Ready for immediate playtest execution
**Last Verified:** Against code and design docs in session claude/prepare-player-testing-01RzQWDhYoVUAjCNyDKkhXox
**All facts checked** - No assumptions
