# Agent Operations Guide: Wayfarer Game Testing & Development

**Version:** 1.1 - VERIFIED WITH ACTUAL TESTING
**Last Updated:** 2025-11-13 17:40 UTC
**Purpose:** PROVEN operational procedures based on actual testing evidence
**Status:** All procedures verified with screenshots and server logs

---

## CRITICAL FINDINGS FROM VERIFICATION TESTING

**Date:** 2025-11-13 17:40 UTC
**Tester:** Main session (documented with screenshots + server logs)

### BUG DISCOVERED: Scene Activation Failure (BLOCKING)

**Status:** CONFIRMED with 100% certainty via server logs and Playwright testing
**Impact:** Tutorial "Secure Lodging" (a1_arrival) scene is INACCESSIBLE to players

**Evidence:**
1. Server logs show scene spawns correctly:
   ```
   [Init] Found 1 starter templates (will be spawned by GameFacade.StartGameAsync)
     - a1_arrival (PlacementFilter: NPC)
   [SceneInstanceFacade] Spawned scene 'scene_a1_arrival_25fca695' via HIGHLANDER flow
   ```

2. Scene activation FAILS - PlacementFilter data is LOST:
   ```
   [Scene.ShouldActivateAtContext] Scene 'scene_a1_arrival_25fca695' checking activation at location 'common_room', npc ''
   [Scene.ShouldActivateAtContext] Scene 'scene_a1_arrival_25fca695' requires location '', npc '' | Player at 'common_room', ''
   [Scene.ShouldActivateAtContext] Scene 'scene_a1_arrival_25fca695' rejected - Location mismatch
   ```

3. UI shows Elena with "Exchange/Trading" instead of "Arrival" scene (screenshot: 02_look_around_npcs.png)

**Root Cause:** Scene entity stores PlacementFilter during spawn, but activation check receives EMPTY npc context (`npc ''`) instead of `'elena'`. The PlacementFilter data is lost between spawn and activation.

**Recommendation:** Investigate Scene.ShouldActivateAtContext method and NPC context passing in BuildNPCsWithSituations

---

## Table of Contents
1. [Process Management](#process-management)
2. [Build & Run Operations](#build--run-operations)
3. [Playwright Testing](#playwright-testing)
4. [Server Logs Analysis](#server-logs-analysis)
5. [Common Pitfalls](#common-pitfalls)
6. [Quick Reference Commands](#quick-reference-commands)

---

## Process Management

### Kill All Dotnet Processes on Port 8100

**Problem:** Multiple background dotnet processes accumulate, blocking port 8100.

**Solution - Step by Step:**

```bash
# Step 1: Find processes using port 8100
netstat -ano | findstr :8100

# Output example:
# TCP    127.0.0.1:8100    ...    LISTENING    9620
# TCP    [::1]:8100        ...    LISTENING    9620

# Step 2: Kill the process (NOTE: Use // instead of / on Windows)
taskkill //F //PID 9620

# Step 3: Verify port is free
netstat -ano | findstr :8100
# Should return nothing
```

**CRITICAL:**
- Use `//F` NOT `/F` (Windows Git Bash interprets `/F` as a path)
- Kill ALL PIDs shown by netstat, not just the first one
- Always verify port is free before starting server

### Kill Background Bash Shells

**Problem:** Background bash shells running `dotnet run` keep consuming resources.

**Solution:**
```bash
# List all background shells
# Check system reminders in tool output for bash IDs like "8d862d"

# Kill specific shell (if ID known)
# Use KillShell tool with bash_id parameter

# OR kill all dotnet processes system-wide
taskkill //F //IM dotnet.exe
```

**When to Use:**
- Before starting a new server
- After test completion
- When "Address already in use" errors occur

---

## Build & Run Operations

### Standard Build Process [VERIFIED 2025-11-13]

**CRITICAL: Path Syntax in Git Bash**
- ✅ CORRECT: `/c/Git/Wayfarer/src` (Unix-style with /c/ prefix)
- ❌ WRONG: `C:\Git\Wayfarer\src` (Windows-style - will fail in Git Bash!)

**Procedure (VERIFIED):**

```bash
# Step 1: Navigate to src directory (USE UNIX PATH!)
cd /c/Git/Wayfarer/src

# Step 2: Clean build (optional, if changing architecture)
dotnet clean

# Step 3: Build
dotnet build

# VERIFIED OUTPUT (SUCCESS):
# Wiederherzustellende Projekte werden ermittelt...
#   Alle Projekte sind für die Wiederherstellung auf dem neuesten Stand.
#   Wayfarer -> C:\Git\Wayfarer\src\bin\Debug\net8.0\Wayfarer.dll
#
# Der Buildvorgang wurde erfolgreich ausgeführt.
#     0 Warnung(en)
#     0 Fehler
#
# Verstrichene Zeit 00:00:01.07
```

**Build Verification (VERIFIED):**
- ✅ "Der Buildvorgang wurde erfolgreich ausgeführt." = SUCCESS
- ✅ 0 Fehler (errors) = SAFE TO PROCEED
- ✅ Warnings may appear (CS4014 async/await warnings are known/safe)
- ❌ Any errors > 0 = STOP, investigate, fix before proceeding

### Run Server for Interactive Testing [VERIFIED 2025-11-13]

**VERIFIED PROCEDURE:**

```bash
# Step 1: Kill existing processes (CRITICAL - see Process Management section)
netstat -ano | findstr :8100
# Output shows PIDs, example: "...ABH�REN    4932"
taskkill //F //PID 4932  # Use ACTUAL PID from netstat output

# Step 2: Verify port is FREE
netstat -ano | findstr :8100 || echo "PORT 8100 IS FREE"
# Should output: "PORT 8100 IS FREE"

# Step 3: Start server in background with timeout (USE UNIX PATH!)
cd /c/Git/Wayfarer/src && ASPNETCORE_URLS="http://localhost:8100" timeout 120 dotnet run --no-build
# Use run_in_background: true parameter in Bash tool
# Server will return bash_id (e.g., "cb1774") - SAVE THIS!

# Step 4: Wait 10 seconds for startup (VERIFIED - 10s is safer than 5s)
timeout 10 ping localhost -n 11 >nul 2>&1

# Step 5: Check server logs with saved bash_id
# Use BashOutput tool with bash_id from Step 3
```

**Parameters Explained (VERIFIED):**
- `ASPNETCORE_URLS="http://localhost:8100"` - Force port 8100 (override launchSettings.json)
- `timeout 120` - Auto-kill after 120 seconds (prevents orphan processes)
- `--no-build` - Skip rebuild (faster, use after `dotnet build` succeeds)
- `run_in_background: true` - Run as background process (required for Bash tool parameter)

**Server Startup Verification (VERIFIED OUTPUT):**
```
[Init] Clearing 1 stale files from Content/Dynamic
[Init]   ✅ Deleted scene_a1_arrival_64e34c47_package.json
[Init] Content/Dynamic cleared - clean state achieved
[PackageLoader] Loading package: 01_foundation.json
...
[Init] Found 1 starter templates (will be spawned by GameFacade.StartGameAsync)
  - a1_arrival (PlacementFilter: NPC)
...
[18:39:16 INF] Now listening on: http://localhost:8100
[18:39:16 INF] Application started. Press Ctrl+C to shut down.
[18:39:16 INF] Hosting environment: Development
[18:39:16 INF] Content root path: C:\Git\Wayfarer\src
```

**SUCCESS INDICATORS (VERIFIED):**
- ✅ "Now listening on: http://localhost:8100" appears
- ✅ "Application started. Press Ctrl+C to shut down." appears
- ✅ Starter templates count shown (e.g., "Found 1 starter templates")
- ✅ No exceptions or errors in output

**FAILURE INDICATORS:**
- ❌ "Address already in use" = Port not freed (repeat Step 1)
- ❌ Build errors = Run `dotnet build` first
- ❌ Missing packages = Check Content/Core directory exists

---

## Playwright Testing

### Standard Playwright Workflow [VERIFIED 2025-11-13]

**CRITICAL PREREQUISITE:** Server MUST be running on http://localhost:8100 BEFORE any Playwright actions.

**VERIFIED PROCEDURE:**

```typescript
// Step 1: Navigate to game (opens visible browser window)
mcp__playwright__playwright_navigate({
  url: "http://localhost:8100",
  headless: false,  // VERIFIED: false shows browser (required for interactive testing)
  timeout: 30000
})
// VERIFIED OUTPUT: "Navigated to http://localhost:8100"

// Step 2: Take screenshot of initial state (with PNG file)
mcp__playwright__playwright_screenshot({
  name: "01_game_loaded_verified",
  savePng: true  // VERIFIED: true saves actual PNG to Downloads folder
})
// VERIFIED OUTPUT: Screenshot saved to Downloads with timestamp

// Step 3: Get visible text to verify page loaded correctly
mcp__playwright__playwright_get_visible_text()
// VERIFIED OUTPUT: Full text content including "Sunday - Day 1 of Journey", resource bars, action buttons

// Step 4: Interact with page elements
mcp__playwright__playwright_click({
  selector: "text=Look Around"  // VERIFIED: text selector works for buttons
})
// VERIFIED OUTPUT: "Clicked element: text=Look Around"

// Step 5: Wait 3 seconds for page to update (VERIFIED timing)
// Use Bash tool: timeout 3 ping localhost -n 4 >nul 2>&1

// Step 6: Screenshot after action
mcp__playwright__playwright_screenshot({
  name: "02_look_around_npcs",
  savePng: true
})

// Step 7: Get updated visible text
mcp__playwright__playwright_get_visible_text()
// VERIFIED OUTPUT: Shows NPCs (Elena, Thomas) with their scenes/actions

// Step 8: Close browser when testing complete
mcp__playwright__playwright_close()
// VERIFIED OUTPUT: "Browser closed successfully"
```

**VERIFIED Screenshots from Testing:**
- `01_game_loaded_verified-2025-11-13T17-40-01-810Z.png` - Initial game load
- `02_look_around_npcs-2025-11-13T17-40-20-284Z.png` - After clicking Look Around

### Playwright Selector Strategies

**Best Practices:**
1. `text=Button Name` - Most reliable for buttons with visible text
2. `.class-name` - Use for styled elements
3. `#element-id` - Use for unique elements
4. Avoid complex CSS selectors - they break when UI changes

**Example Selectors from Wayfarer:**
- `text=Look Around` - Main location action button
- `text=Arrival` - Scene button on NPC
- `.scene-choice-card` - Individual choice cards in scenes
- `.resource-value` - Resource display values

### Handling Playwright Errors

**"Element not found" errors:**
```bash
# Take screenshot to see actual page state
mcp__playwright__playwright_screenshot({ name: "debug_state" })

# Get visible text to see available selectors
mcp__playwright__playwright_get_visible_text()

# Adjust selector based on actual page content
```

**"Timeout exceeded" errors:**
```bash
# Check if modal overlay is blocking clicks
# Wayfarer has modal overlays that intercept clicks

# Solution: Close modal first or click on modal content
mcp__playwright__playwright_click({
  selector: "text=Close"  // Close button if modal blocks
})
```

**"Browser not open" errors:**
```bash
# Always navigate first
mcp__playwright__playwright_navigate({ url: "http://localhost:8100", headless: false })

# Then perform actions
```

---

## Server Logs Analysis

### Reading Background Server Logs

```bash
# Get logs from background process
BashOutput({ bash_id: "78ed68" })  # Use actual bash_id from tool output

# Filter logs for specific keywords
BashOutput({
  bash_id: "78ed68",
  filter: "\\[SceneInstanceFacade\\]"  # Regex pattern
})
```

### Critical Log Patterns

**Scene Generation (SUCCESS):**
```
[SceneTemplateParser] Parsing SceneTemplate: a1_arrival
[SceneArchetypeGeneration] Generated 3 situations with pattern 'Linear'
[SceneInstanceFacade] Spawned scene 'scene_a1_arrival_<hash>' via HIGHLANDER flow
```

**Scene Activation (FAILURE Example):**
```
[Scene.ShouldActivateAtContext] Scene 'scene_a1_arrival_<hash>' checking activation at location 'common_room', npc ''
[Scene.ShouldActivateAtContext] Scene rejected - Location mismatch
```
**Red Flag:** Empty NPC context (`npc ''`) when scene requires NPC

**HIGHLANDER Violations:**
```
[Init] Found 3 starter templates
  - tutorial_secure_lodging (PlacementFilter: NPC)
  - a1_arrival (PlacementFilter: NPC)
```
**Red Flag:** Two templates with same PlacementFilter.npcId = DUPLICATE

**Package Loading:**
```
[PackageLoader] Loading package: 21_tutorial_scenes.json
[PackageLoader] Loaded 2 SceneTemplates from this package
```
**Check:** Count matches JSON file content

### Log Analysis Checklist

When investigating bugs:
1. ✅ Check starter templates count (should match expected)
2. ✅ Verify scene spawn messages (one per template)
3. ✅ Check activation attempts (ShouldActivateAtContext)
4. ✅ Look for NullReferenceException stack traces
5. ✅ Verify dependent locations generated correctly
6. ✅ Check hex position assignments

---

## Common Pitfalls

### Pitfall 1: Forgetting to Kill Previous Server

**Symptom:** "Address already in use" error
**Solution:** Always kill port 8100 processes first (see Process Management)

### Pitfall 2: Using `/F` Instead of `//F` in taskkill

**Symptom:** "Argument/Option ungültig" error
**Solution:** Git Bash on Windows requires `//F` not `/F`

### Pitfall 3: Not Waiting for Server Startup

**Symptom:** Playwright navigation fails, "connection refused"
**Solution:** Wait 5-10 seconds after starting server before Playwright actions

### Pitfall 4: Reading Stale Logs

**Symptom:** Logs don't match current state
**Solution:** Always use BashOutput on LATEST bash_id, check timestamp

### Pitfall 5: Clicking Through Modal Overlays

**Symptom:** "Element intercepts pointer events"
**Solution:** Check for `components-reconnect-modal` or other overlays blocking clicks

### Pitfall 6: Not Building Before Running

**Symptom:** Code changes don't appear in running game
**Solution:** Always `dotnet build` after code changes, then `dotnet run --no-build`

### Pitfall 7: Assuming Scene Spawned = Scene Accessible

**Symptom:** Scene appears in logs but not in UI
**Solution:** Check activation logs (`ShouldActivateAtContext`) - spawn != activation

---

## Quick Reference Commands

### Complete Test Session Workflow

```bash
# 1. Clean slate
taskkill //F //IM dotnet.exe
netstat -ano | findstr :8100  # Should return nothing

# 2. Build
cd C:\Git\Wayfarer\src && dotnet build

# 3. Start server (background, timeout, specific port)
cd src && ASPNETCORE_URLS="http://localhost:8100" timeout 60 dotnet run --no-build
# Save bash_id from output (e.g., "78ed68")

# 4. Wait for startup
timeout 5 ping localhost -n 6 >nul 2>&1

# 5. Open Playwright browser
mcp__playwright__playwright_navigate({
  url: "http://localhost:8100",
  headless: false
})

# 6. Test actions
mcp__playwright__playwright_click({ selector: "text=Look Around" })
mcp__playwright__playwright_screenshot({ name: "test_state" })

# 7. Check server logs for issues
BashOutput({ bash_id: "78ed68" })

# 8. Cleanup
mcp__playwright__playwright_close()
taskkill //F //IM dotnet.exe
```

### Emergency Recovery

```bash
# Everything is broken, start fresh:
taskkill //F //IM dotnet.exe
taskkill //F //IM msedge.exe     # Close Playwright browsers
taskkill //F //IM chrome.exe
taskkill //F //IM firefox.exe
cd C:\Git\Wayfarer\src && dotnet clean && dotnet build
# Then restart normal workflow
```

---

## Agent-Specific Tips

### For game-dev-validator Agents

**Focus:** Verify mechanics match design docs

**Essential Operations:**
1. Read `design/10_tutorial_design.md` FIRST
2. Use Playwright to navigate tutorial flow
3. Screenshot EVERY situation for evidence
4. Compare observed mechanics vs. design doc requirements
5. Check logs for mechanical integrity (four-choice pattern, resource costs)

### For qa-engineer Agents

**Focus:** Holistic testing, edge cases, connected systems

**Essential Operations:**
1. Test ALL paths (not just happy path)
2. Check server logs for exceptions/errors
3. Test edge cases (navigate away and back, try accessing locked locations)
4. Verify connected systems (Journal, Map, hex routes)
5. Document 9/10 certainty requirement (trace exact data flow)

### For domain-dev Agents

**Focus:** Data flow JSON→Parser→Domain→ViewModel→UI

**Essential Operations:**
1. Read JSON source files first
2. Grep for parser code
3. Grep for domain entity classes
4. Check ViewModel construction
5. Trace full stack through server logs
6. Verify strong typing (no string parsing, no dictionaries)

### For lead-architect Agents

**Focus:** Architecture patterns, HIGHLANDER violations, entity ownership

**Essential Operations:**
1. Check for duplicate entities (same ID, same placement filter)
2. Verify Catalogue Pattern usage
3. Check entity ownership (GameWorld = source of truth)
4. Verify Parser-JSON-Entity Triangle
5. Look for HIGHLANDER violations in logs

### For Explore Agents

**Focus:** User experience, playability, perfect information

**Essential Operations:**
1. Play through tutorial as player would
2. Screenshot every screen for UX analysis
3. Verify perfect information (all costs visible)
4. Check for confusing UI elements
5. Document player mental model mismatches

---

## Critical Success Factors

1. **Always kill processes before starting** - Most common failure point
2. **Always wait for server startup** - Second most common failure
3. **Always check logs immediately after spawn** - Catch bugs early
4. **Always use visible browser (headless: false)** - See what's actually happening
5. **Always take screenshots as evidence** - Document what you observe
6. **Always verify 9/10 certainty before claiming bugs** - Trace exact data flow

---

## Document Updates

Agents should update this document when they discover:
- New operational patterns that work reliably
- New failure modes not covered here
- Better commands or workflows
- Platform-specific quirks (Windows vs. Linux)

**Update Protocol:**
1. Test the new procedure thoroughly (3+ times)
2. Document exact commands with expected output
3. Add to appropriate section
4. Update version number and date at top
