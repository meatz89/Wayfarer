# Playtest Documentation Index

**Branch:** playtest-1
**Status:** Phase 1 & 3 complete, Phase 2 requires human execution
**Last Updated:** 2025-11-23

---

## Quick Start

**New to this playtest session?** Start here:
1. Read [SESSION_SUMMARY.md](SESSION_SUMMARY.md) - Overview of all work completed
2. Read [PLAYTEST_LEARNINGS.md](PLAYTEST_LEARNINGS.md) - Quick start guide and findings
3. Read [WHATS_MISSING_ANALYSIS.md](WHATS_MISSING_ANALYSIS.md) - Gap analysis and what remains

**Continuing Phase 2?** Go to:
- [PHASE_2_HANDOFF.md](PHASE_2_HANDOFF.md) - Human playtester handoff instructions

---

## Documentation by Phase

### Phase 1: Automated Smoke Tests ✅ COMPLETE
**Purpose:** Verify game mechanics function correctly
**Duration:** ~15 minutes
**Files:**
- **[PHASE_1_TEST_REPORT.md](PHASE_1_TEST_REPORT.md)** - Complete test results (all 5 tests passed)

**Key Findings:**
- All mechanical systems working correctly
- 2 critical bugs fixed (tutorial soft-lock, duplicate scene architecture)
- Perfect information principle upheld
- Stat gating displays clearly

---

### Phase 2: Emotional Arc Validation ⏸️ REQUIRES HUMAN
**Purpose:** Test subjective emotional experience over 3-4 hours
**Duration:** 3-4 hours continuous human gameplay
**Files:**
- **[PHASE_2_HANDOFF.md](PHASE_2_HANDOFF.md)** - START HERE for human execution
- **[PHASE_2_EMOTIONAL_ARC_LOG.md](PHASE_2_EMOTIONAL_ARC_LOG.md)** - Template for documenting emotional experience
- **[PHASE_2_INVESTIGATOR_LOG.md](PHASE_2_INVESTIGATOR_LOG.md)** - Initial gameplay documentation

**Why Human Required:**
AI can test mechanics, but cannot experience:
- Regret for unchosen paths (subjective emotion)
- Build identity formation (cumulative feeling over hours)
- "Life you could have had" emotion (Sir Brante model)
- Meaningful vulnerability from specialization (strategic feeling)

**Prerequisites:**
1. Read PLAYTEST_GUIDE.md completely
2. Understand Sir Brante emotional model
3. Commit 3-4 hours uninterrupted
4. Follow hour-by-hour documentation checkpoints

---

### Phase 3: Holistic Playability Fixes ✅ COMPLETE
**Purpose:** Fix all critical UX issues discovered in Phase 1
**Duration:** ~45 minutes
**Files:**
- Documented in [SESSION_SUMMARY.md](SESSION_SUMMARY.md) Phase 3 section
- Gap analysis in [WHATS_MISSING_ANALYSIS.md](WHATS_MISSING_ANALYSIS.md)

**Critical Fixes Implemented:**
1. ✅ **Stats Display** - Added to GameScreen header (stats were invisible)
2. ✅ **Scene Category Tags** - Added type badges ([Tutorial], [Service])
3. ✅ **CSS Polish** - Visual separation and professional styling

**Impact:** All discovered playability issues resolved. Game ready for human testing.

---

## Key Documentation Files

### Session Overview
- **[SESSION_SUMMARY.md](SESSION_SUMMARY.md)** - Complete session summary, work completed, findings
- **[PLAYTEST_LEARNINGS.md](PLAYTEST_LEARNINGS.md)** - Session learnings and quick start guide
- **[WHATS_MISSING_ANALYSIS.md](WHATS_MISSING_ANALYSIS.md)** - Holistic gap analysis after Phase 3

### Test Reports
- **[PHASE_1_TEST_REPORT.md](PHASE_1_TEST_REPORT.md)** - Automated smoke test results
- **[PHASE_2_EMOTIONAL_ARC_LOG.md](PHASE_2_EMOTIONAL_ARC_LOG.md)** - Emotional arc template (empty, requires human)
- **[PHASE_2_INVESTIGATOR_LOG.md](PHASE_2_INVESTIGATOR_LOG.md)** - Initial Investigator build documentation

### Handoff Materials
- **[PHASE_2_HANDOFF.md](PHASE_2_HANDOFF.md)** - Human playtester handoff documentation

---

## Current Status Summary

### Completed Work ✅
- **Phase 1:** All 5 automated tests passed
- **Phase 3:** All critical playability issues fixed
- **Bugs Fixed:**
  - Tutorial soft-lock (costs adjusted)
  - Duplicate scene architecture (old tutorials disabled)
  - Stats invisible in UI (stats display added)
  - Scene type ambiguity (category tags added)

### Ready for Execution ⏸️
- **Phase 2:** Emotional arc validation (requires 3-4 hours human gameplay)

### Optional Enhancements (Low Priority)
- Documentation further iteration
- Icon color theming
- Responsive layout testing
- Additional CSS polish

---

## Reading Order by Goal

### Goal: "I want to run Phase 2 emotional testing"
1. Read [PHASE_2_HANDOFF.md](PHASE_2_HANDOFF.md) - Complete handoff instructions
2. Read [PLAYTEST_LEARNINGS.md](PLAYTEST_LEARNINGS.md) - Session findings and setup
3. Read [PLAYTEST_GUIDE.md](PLAYTEST_GUIDE.md) - Original playtest design (if not already familiar)
4. Use [PHASE_2_EMOTIONAL_ARC_LOG.md](PHASE_2_EMOTIONAL_ARC_LOG.md) - Document your experience

### Goal: "I want to understand what was done this session"
1. Read [SESSION_SUMMARY.md](SESSION_SUMMARY.md) - Complete overview
2. Read [PHASE_1_TEST_REPORT.md](PHASE_1_TEST_REPORT.md) - Test results
3. Read [WHATS_MISSING_ANALYSIS.md](WHATS_MISSING_ANALYSIS.md) - What remains

### Goal: "I want to continue development"
1. Read [SESSION_SUMMARY.md](SESSION_SUMMARY.md) - Current state
2. Read [WHATS_MISSING_ANALYSIS.md](WHATS_MISSING_ANALYSIS.md) - Identified gaps
3. Review git commits on playtest-1 branch

### Goal: "I want to understand design decisions"
1. Read [PLAYTEST_LEARNINGS.md](PLAYTEST_LEARNINGS.md) - Discoveries and findings
2. Read [WHATS_MISSING_ANALYSIS.md](WHATS_MISSING_ANALYSIS.md) - Architectural analysis
3. Read [PLAYTEST_GUIDE.md](PLAYTEST_GUIDE.md) - Original design intent

---

## Technical Setup

### Fresh Game State
```bash
# Kill server
taskkill //F //IM dotnet.exe

# Start fresh
cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build
```

### Build Commands
```bash
# Build
cd src && dotnet build

# Run (after build)
cd src && ASPNETCORE_URLS="http://localhost:8100" dotnet run --no-build

# Test
cd src && dotnet test
```

### Game State Notes
- Blazor Server persists state server-side
- Page refresh does NOT reset game
- Must restart server for fresh state

---

## Commit History

**Latest Commits (playtest-1 branch):**
- **0a3f0a49** - Add CSS styling polish for stats display and scene category tags
- **705f3176** - Update SESSION_SUMMARY.md: Document Phase 3 playability fixes
- **71c286f2** - Fix critical playability issues: Add stats display and scene type tags
- **b30dc161** - Fix critical tutorial soft-lock bug in Secure Lodging scene

---

## Next Steps

**Immediate:** Execute Phase 2 emotional arc validation (human playtester, 3-4 hours)

**After Phase 2:**
- Phase 3: Diplomat build comparison (2-3 hours)
- Generate PLAYTEST_COMPLETE_REPORT.md (final deliverable)
- Answer: Does stat specialization create meaningfully different experiences?

---

**For questions or issues, refer to [SESSION_SUMMARY.md](SESSION_SUMMARY.md) Technical Notes section or git commit history.**
