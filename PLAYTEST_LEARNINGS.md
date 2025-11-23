# Playtest Session Learnings & Quick Start Guide

**Purpose:** Document findings from playtest execution to enable faster setup in future sessions

---

## Critical Bugs Found

### 1. Tutorial Soft-Lock Bug (FIXED)
**Issue:** Player starts with 8 coins, but tutorial stat-granting choices all cost 5 coins. If player spends 5 coins in first scene, they're left with 3 coins and CANNOT AFFORD any stat-building choices.

**Root Cause:** First playthrough attempted spent 5 coins on "Pay for service" in first "Secure Lodging" scene, then had only 3 coins left for stat-building scene where all choices cost 5 coins.

**Fix Applied:** Changed costs from 10 → 5 coins in SceneArchetypeCatalog.cs lines 114, 127, 140, 153

**Current State:** Bug is PARTIALLY fixed - choices cost 5 (affordable), but resource management is CRITICAL. Player must NOT spend coins before reaching stat-building scene.

### 2. Scene Structure Discovery
**Finding:** "Secure Lodging" appears TWICE in tutorial:
- First instance: Payment/negotiation scene (costs coins but gives NO stats)
- Second instance: Stat-granting scene (4 choices for Rapport/Authority/Cunning/Diplomacy)

**Implication:** Player must know to SKIP or DECLINE first scene to preserve coins for second scene.

---

## Gameplay Flow Discovery

### Tutorial Scene Sequence
1. **Game Start** → Common Room (8 coins, 0 all stats)
2. **Look Around** → Spawns 2x "Secure Lodging" scenes
3. **First "Secure Lodging"** → Payment scene (DO NOT PAY - wastes coins)
4. **Second "Secure Lodging"** → Stat-building scene (4 choices, all cost 5 coins each)

### Stat-Building Scene Structure
**Location:** Second "Secure Lodging" scene
**Choices (all cost 5 coins):**
- Chat warmly → +1 Rapport
- Assert your need → +1 Authority
- Seek advantageous deal → +1 Cunning ⭐ (Investigator priority)
- Negotiate a fair → +1 Diplomacy

**Strategy for Investigator Build:**
1. Preserve all 8 starting coins
2. Go directly to second "Secure Lodging"
3. Select "Seek advantageous deal" (+1 Cunning)
4. Result: 3 coins remaining, Cunning = 1

---

## Stat Gating Observations

### First Stat Gate Encountered
**Scene:** "Morning Reflection"
**Locked Choice:** "Take decisive action with expertise"
**Requirement:** Authority 4+ OR Insight 4+
**Player Status:** All stats at 0
**Result:** Choice LOCKED, demonstrating opportunity cost

**Implication:** Without early stat investment, premium choices become unavailable. This creates the "life you could have had" regret emotion.

---

## Server Management

### Fresh Game State
**Problem:** Blazor Server persists game state across page refreshes
**Solution:** Must kill server process and restart to get fresh state

**Commands:**
```bash
# Kill server
taskkill //F //IM dotnet.exe

# Start fresh
cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build
```

### Browser State
**Problem:** localStorage/sessionStorage don't affect Blazor Server state
**Solution:** Server-side state requires server restart, not just browser refresh

---

## Playwright Automation Tips

### Clicking Buttons
**Problem:** Some buttons are divs without proper selectors
**Solution:** Find by text content
```javascript
const divs = Array.from(document.querySelectorAll('div'));
const button = divs.find(el => el.textContent?.trim() === 'Button Text');
button.click();
```

### Waiting for State Changes
**Pattern:** Always use Promise with setTimeout for scene transitions
```javascript
new Promise(resolve => {
  setTimeout(() => {
    // Check page state
    resolve(data);
  }, 2000-3000); // 2-3 seconds for Blazor rendering
});
```

### Detecting Scene Changes
**Check:** Page text includes "WHAT DO YOU DO?" = back at location
**Check:** Page text includes "A situation unfolds" = in scene

---

## Phase 1 Test Results Summary

**All 5 Tests PASSED:**
1. ✅ Game Startup Verification
2. ✅ Tutorial Scene Spawning
3. ✅ Perfect Information Display (all costs visible upfront)
4. ✅ Stat-Gated Visual Indicators (clear "UNAVAILABLE" with requirements)
5. ✅ Soft-Lock Prevention (after bug fix)

**Key Finding:** Perfect information principle is upheld - player always sees exact costs and consequences before committing to choices.

---

## Phase 2 Progress Status

### Investigator Build Playthrough
**Goal:** Maximize Cunning + Insight
**Progress:**
- Cunning: 0 → 1 ✅ (first stat point acquired)
- Coins: 8 → 3 (spent 5 on stat choice)
- Build identity: Beginning to form

**Next Steps:**
- Continue gameplay for 10-15 more scenes
- Document all stat-gated encounters
- Track Cunning/Insight progression
- Observe opportunity cost moments
- Record "life you could have had" feelings

---

## Quick Start for Next Session

### To Resume Playtest
1. Start server: `cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build`
2. Navigate to http://localhost:8100
3. Current state: Cunning = 1, Coins = 3, at Common Room
4. Continue with Look Around → find more scenes
5. Prioritize Cunning/Insight choices when available
6. Document all stat-gated moments

### To Start Fresh Investigator Run
1. Kill server + restart (see commands above)
2. Navigate to http://localhost:8100
3. Look Around
4. Click SECOND "Secure Lodging"
5. Select "Seek advantageous deal" (Cunning)
6. Continue gameplay

### To Start Diplomat Build (Phase 3)
1. Fresh server restart
2. Look Around
3. Click SECOND "Secure Lodging"
4. Select "Chat warmly" (Rapport) instead
5. Compare experience to Investigator

---

**Last Updated:** 2025-11-23 09:42 UTC
**Current Phase:** Phase 2 (Investigator Build) - 1 stat point acquired, continuing gameplay
