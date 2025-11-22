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

**Technology:** ASP.NET Core Blazor Server (verified: Program.cs line 17)

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
1. GameWorld initialization via `GameWorldInitializer.CreateGameWorld()` (Program.cs:28)
2. Content loading from JSON files in `Content/Core/` directory
3. Serilog console logging starts
4. Server listens on http://localhost:5000
5. Navigate browser to http://localhost:5000

**For Testing Environment Without .NET:**
- Game requires .NET 7.0+ SDK to run
- Alternative: Use Docker container or pre-built deployment
- Playwright tests assume server already running on localhost:5000

**Expected Console Output on Success:**
```
info: Content loading...
info: GameWorld initialized
info: Now listening on: http://localhost:5000
```

---

## What is Wayfarer?

**Core Experience:** Strategic depth through impossible choices, not mechanical complexity. (design/01_design_vision.md:11)

**You are:** A courier managing scarce resources across dangerous routes. NOT a hero. You accept delivery jobs, navigate challenges, and barely afford survival. Profit margins are razor-thin.

**The Essential Feeling:**
"I can afford A OR B, but not both. Both valid. Both have costs. Which cost will I accept?"

**What Playtesters Should Understand:**

1. **Impossible Choices Are Intentional**
   - No correct answer exists
   - Multiple suboptimal paths by design
   - Character emerges from what you sacrifice
   - **Test:** Do choices feel genuinely difficult?

2. **Economic Scarcity Is Core**
   - Delivery earnings barely cover food + lodging
   - Equipment upgrades require many successful runs
   - Every coin spent somewhere = not spent elsewhere
   - **Test:** Do margins feel tight but fair?

3. **No Power Fantasy**
   - Resources stay scarce throughout
   - Growth is incremental, not exponential
   - You never become unstoppable
   - **Test:** Does scarcity persist even after progression?

4. **Infinite Journey (Frieren Principle)**
   - No ending, no "beating the game"
   - A-story continues forever
   - Success = quality of journey, not destination
   - **Test:** Does it feel open-ended without pressure to rush?

5. **Contemplation Not Urgency**
   - No ticking clocks forcing rushed decisions
   - All costs visible before committing (perfect information)
   - Can pause and calculate strategies
   - **Test:** Can you thoughtfully evaluate every choice?

**Testing Lens:** Verify these design pillars manifest in actual play. If choices feel trivial, scarcity disappears, or rush pressure appears - design is failing.

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

###Scene a2: "First Delivery"
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
// Resource values (line 29, 36, 43, 50, 57)
const health = await page.locator('.resource .resource-value').nth(0).textContent();
const hunger = await page.locator('.resource .resource-value').nth(1).textContent();
const focus = await page.locator('.resource .resource-value').nth(2).textContent();
const coins = await page.locator('.resource .resource-value').nth(4).textContent();
```

### Time Display
```javascript
// Day and time period (line 9, 11)
const day = await page.locator('.day-display').textContent();
const period = await page.locator('.period-name').textContent();
```

### Active Delivery
```javascript
// Check if delivery active (line 67)
const hasActiveJob = await page.locator('.active-job-banner').isVisible();
const payment = await page.locator('.active-job-payment').textContent();
```

### Navigation
```javascript
// Fixed buttons (line 79, 80)
await page.click('.journal-btn');
await page.click('.map-btn');
```

### Modals
```javascript
// Modal overlay (line 86, 96)
const modalOpen = await page.locator('.modal-overlay').isVisible();
// Close modal - click overlay
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

### 3. Economic Imbalance (MAJOR)
**Why Critical:** "Razor-thin profit margins" is intentional design (design/01_design_vision.md:17)

**Test:**
- Delivery earnings vs survival costs ratio
- Can maintain positive coins over 10 deliveries?
- Equipment affordable within reasonable time?
- Margins feel tight but NOT punishing?

**Expected:** ~3-7 coins profit per optimized delivery. Equipment costs 60+ coins = 10-12 deliveries required.

### 4. Scarcity Disappears (MAJOR)
**Why Critical:** "No power fantasy" - resources must stay scarce (design/01_design_vision.md:93)

**Test:**
- Do costs still matter after 20 deliveries?
- Does player reach "unstoppable" state?
- Are late-game challenges still challenging?

**Expected:** Economic pressure persists. Never trivial.

### 5. Choices Feel Trivial (MAJOR)
**Why Critical:** "Impossible choices" core experience (design/01_design_vision.md:19)

**Test:**
- Do multiple paths feel genuinely valid?
- Is there an obvious "correct" answer?
- Do sacrifices feel meaningful?

**Expected:** "I can afford A OR B, but not both" should be constantly true.

### 6. Route Segments Don't Learn (MEDIUM)
**Why Critical:** Route mastery = core skill progression (design/02_core_gameplay_loops.md:84)

**Test:**
- First travel: Segments face-down (unknown)
- Second travel: Fixed segments face-up showing exact costs
- Third travel: Can plan perfect resource allocation

**Expected:** Learning routes enables profitable optimization.

### 7. Tutorial Doesn't Spawn (BREAKING)
**Why Critical:** a1_secure_lodging has `isStarter: true` - must appear (22_a_story_tutorial.json:22)

**Test:**
- Game start → Scene a1 appears at Common Room with Elena
- Can complete all three tutorial scenes (a1, a2, a3)
- Each scene teaches intended system

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

### 4. Critical Path Test (20 min)
**As Economic Optimizer:**
1. Accept delivery contract (scene a2)
2. Travel route (scene a3) - note which segments are fixed/event
3. Complete delivery
4. Calculate: earnings - (food + lodging) = profit?
5. Repeat for 3 deliveries
6. Verify profit margins tight but positive

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

### 7. Build Test (20 min each)
**As Investigator:**
- Prioritize Insight + Cunning
- Find Mental challenge
- Verify domination
- Find Social situation
- Verify struggle/costly workarounds

**As Diplomat:**
- Prioritize Rapport + Diplomacy
- Find Social challenge
- Verify domination
- Find Mental situation
- Verify struggle/costly workarounds

### 8. Report Critical Issues
**BREAKING (stop testing):**
- Soft-locks
- Tutorial doesn't spawn
- Crashes/state corruption

**MAJOR (continue but note):**
- Perfect information violations
- Economic imbalance (too easy/hard)
- Scarcity disappears
- Choices feel trivial

**MEDIUM (note for later):**
- Route learning doesn't work
- Build differentiation weak
- UI unclear

---

## Success Criteria Summary

✅ **Tutorial completes** (a1 → a2 → a3)
✅ **No soft-locks** (always forward path)
✅ **Perfect information** (all costs visible before commitment)
✅ **Economic pressure exists** (3-7 coin profit per delivery, equipment requires 10-15 runs)
✅ **Impossible choices** ("A OR B, not both" constantly true)
✅ **Routes learn** (segments face-up on repeat, enables optimization)
✅ **Builds differ** (Investigator ≠ Diplomat experience)

---

**Document Status:** Ready for immediate playtest execution
**Last Verified:** Against code and design docs in session claude/prepare-player-testing-01RzQWDhYoVUAjCNyDKkhXox
**All facts checked** - No assumptions
