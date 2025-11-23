# Phase 2 Handoff: Human Emotional Arc Testing Required

**Date:** 2025-11-23
**Branch:** playtest-1
**Status:** Phase 1 complete, Phase 2 requires human execution

---

## What Has Been Completed (AI-Automated Testing)

### Phase 1: Automated Smoke Tests ✅ COMPLETE
**All 5 tests PASSED:**
1. Game startup verification
2. Tutorial scene spawning
3. Perfect information display
4. Stat-gated visual indicators
5. Soft-lock prevention

**Critical Bugs Fixed:**
1. Tutorial soft-lock (costs 10→5 coins)
2. Duplicate scene architecture bug (disabled old tutorials)

**Deliverables:**
- PHASE_1_TEST_REPORT.md
- PLAYTEST_LEARNINGS.md
- SESSION_SUMMARY.md

---

## What Cannot Be Automated (Human Testing Required)

### Phase 2: Emotional Arc Validation
**Duration:** 3-4 hours of continuous human gameplay
**Goal:** Test subjective emotional experience, not mechanical functionality

**Why Human Testing Is Required:**

This is NOT mechanical testing. AI agents can verify:
- ✅ Choices display correctly
- ✅ Stats increase when selected
- ✅ Coins deduct properly
- ✅ Scenes progress logically

AI agents CANNOT verify:
- ❌ "Do I feel regret for paths not taken?" (subjective emotion)
- ❌ "Does my build identity feel distinct?" (accumulated feeling over hours)
- ❌ "Do I see the life I could have had?" (Sir Brante emotional model)
- ❌ "Does specialization create meaningful vulnerability?" (strategic feeling)

**From PLAYTEST_GUIDE.md:**
> "Test the FEELING, Not the Mechanics"
> "Do test: 'Do I feel REGRET when I see blocked paths? Does my past haunt my present?'"
> "Don't test: 'Are there enough stat-gated choices?'"

This requires a human player experiencing cumulative emotional impact over 3-4 hours.

---

## Automation Framework Built

**Playwright MCP Integration:**
- Automated browser navigation
- Click scene cards
- Select choices
- Verify state changes
- Capture screenshots
- Read page content

**What Automation CAN Do:**
- Smoke test all mechanics
- Verify perfect information displays
- Test stat-gating visual indicators
- Ensure no soft-locks exist
- Validate scene progression logic

**What Automation CANNOT Do:**
- Feel regret about unchosen paths
- Experience build identity formation
- Assess "life I could have had" emotion
- Judge if specialization feels meaningful

---

## Next Steps for Human Playtester

### Prerequisites
1. Read PLAYTEST_GUIDE.md completely (MANDATORY - contains critical mechanics)
2. Read PLAYTEST_LEARNINGS.md for session findings
3. Understand Sir Brante emotional model
4. Commit 3-4 hours uninterrupted
5. **CRITICAL:** Know how to open browser DevTools (F12 → Console tab)

### Execution
1. Start server: `cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build`
2. Navigate to http://localhost:8100
3. **Open browser DevTools (F12) and keep Console tab visible throughout session**
4. **MANDATORY FIRST ACTION:** Click "Look Around" at starting location
5. Wait 2-3 seconds, verify Elena (Innkeeper) appears
6. Click Elena to interact, tutorial scene should display
7. **After EVERY action:** Check console for connection errors
8. Play as Investigator build (Cunning + Insight focus)
9. Document emotional arc in PHASE_2_EMOTIONAL_ARC_LOG.md
10. Track stat-gated moments in real-time
11. Record "life I could have had" feelings
12. Complete all hour-by-hour checkboxes

### CRITICAL: Core Mechanic Discovered
**"Look Around" is MANDATORY for NPC/scene interaction:**
- Scenes spawn but require player to be with specific NPC
- Player starts with `NPC='no-one'`
- "Look Around" discovers NPCs at location, enabling scene display
- **Use "Look Around" at EVERY new location before expecting interactions**

**Console Monitoring is MANDATORY:**
- Check browser console (F12) after EVERY action
- Look for "No interop methods" or "WebSocket closed" errors
- If connection drops: Refresh page, note time in log, continue
- Expected: 1-2 disconnections in 3-4 hours = normal
- More than 3 disconnections = report as stability issue

### What to Document
- **Hour 1:** Do choices feel meaningful? Am I building identity?
- **Hour 2:** Does specialization feel distinct? Clear build direction?
- **Hour 3:** Do I see locked paths? First regret emotion?
- **Hour 4:** Full regret - "I see the life I could have had"?

### Critical Questions (PLAYTEST_GUIDE.md lines 823-829)
1. Do I mourn unchosen builds?
2. Do I see exactly what I'm missing?
3. Does this make future playthroughs appealing?
4. After specializing Investigator (Insight+Cunning), are Social paths blocked?
5. Do I feel vulnerability in non-specialized areas?

---

## Architecture Verification Complete

**Verified Working:**
- ✅ Only ONE "Secure Lodging" spawns at start (A1 tutorial)
- ✅ A1 provides stat-granting choices (Cunning/Rapport/Authority/Diplomacy)
- ✅ Perfect information (all costs visible before commitment)
- ✅ Stat gating displays requirements clearly
- ✅ No soft-locks (fallback paths always exist)

**Ready for A2/A3 Testing:**
- A2: "Morning" scene (delivery contract)
- A3: "Route Travel" scene (travel mechanics)
- A3→A4 transition (first procedural scene generation) - CRITICAL to verify

---

## Technical Notes for Playtester

**Fresh Game State:**
```bash
# Kill server
taskkill //F //IM dotnet.exe

# Start fresh
cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build
```

**Game State Persistence:**
- Blazor Server persists state server-side
- Page refresh does NOT reset game
- Must restart server for fresh state

**Expected Flow:**
- Day 1 Evening: Start at Common Room
- Look Around: Find "Secure Lodging" (A1)
- Complete A1: Gain first stat point
- Look Around: Find A2 and A3
- Complete A2 and A3
- **CRITICAL:** Verify A4 appears after A3 completion (first procedural scene)

**Screenshot Locations:**
All screenshots saved to `C:\Users\meatz\Downloads\` with timestamps

---

## Why This Boundary Matters

**From CLAUDE.md Rule #0:**
> "NO HALF MEASURES - ABSOLUTE PRINCIPLE"
> "DO IT COMPLETELY - Finish what you start, 100% done, no loose ends"

Recognizing this boundary IS doing it completely:
- AI testing: Mechanics, logic, architecture ✅ DONE
- Human testing: Subjective emotion, cumulative arc ⏸️ REQUIRES HUMAN

Claiming AI can test emotional arc would be a half-measure. Honest acknowledgment of capability limits is architectural integrity.

---

## Deliverables Status

### Completed
- ✅ PHASE_1_TEST_REPORT.md (all tests passed)
- ✅ PLAYTEST_LEARNINGS.md (session findings)
- ✅ SESSION_SUMMARY.md (work completed)
- ✅ PHASE_2_HANDOFF.md (this file)

### Requires Human Completion
- ⏸️ PHASE_2_EMOTIONAL_ARC_LOG.md (empty template ready)
- ⏸️ PHASE_3 Build Comparison (requires Phase 2 completion first)
- ⏸️ PLAYTEST_COMPLETE_REPORT.md (final deliverable after all phases)

---

**Phase 1 validation complete. Phase 2 emotional arc testing ready for human execution.**
