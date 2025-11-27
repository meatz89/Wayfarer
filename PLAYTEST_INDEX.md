# Playtest Documentation Index

**Status:** Active playtest documentation
**Last Updated:** 2025-11-25

---

## Active Documentation

### Playtest Guides
- **[PLAYTEST_GUIDE.md](PLAYTEST_GUIDE.md)** - Complete playtest methodology and protocol
- **[PLAYTEST_LEARNINGS.md](PLAYTEST_LEARNINGS.md)** - Session learnings and findings

### Key Principles Tested

1. **Impossible Choices** - Every decision forces sacrifice between valid alternatives
2. **Perfect Information** - All costs visible before selection (no hidden gotchas)
3. **Build Identity Through Constraint** - Specialization creates different viable paths
4. **Stat Gating via Cost (Not Access)** - High stats pay less, low stats pay more

---

## Playtest Protocol Summary

### Phase 1: Automated Smoke Tests
Verify game mechanics function correctly through automated testing.

### Phase 2: Emotional Arc Validation
Test subjective emotional experience over 3-4 hours of human gameplay.

**Why Human Required:**
AI can test mechanics, but cannot experience:
- Regret for unchosen paths (subjective emotion)
- Build identity formation (cumulative feeling over hours)
- "Life you could have had" emotion (Sir Brante model)
- Meaningful vulnerability from specialization

### Phase 3: Holistic Playability Fixes
Fix critical UX issues discovered during playtesting.

---

## Debugging Tools

### Spawn Graph Visualizer (`/spawngraph`)
**Purpose:** Interactive visual debugger for inspecting procedurally generated content - scenes, situations, choices, and entity dependencies.

**Access:** Navigate to `/spawngraph` route while game is running (e.g., if game is at `http://localhost:5000`, go to `http://localhost:5000/spawngraph`)

**Key Features:**
- **Node Types:** Scenes, Situations, Choices, Entities (NPCs, Locations, Routes)
- **Filters:** By type, story category (Main/Side/Service), and state (Active/Completed/Deferred)
- **Search:** Find nodes by name
- **Detail Panel:** Click any node to see full details and navigate to related nodes
- **Zoom Controls:** Double-click scene to zoom to subtree, "Fit to View" button

**When to Use:**
- Debug why a scene didn't activate
- Verify scene cascade flow (A1 → A2 → A3)
- Check which choices were made and their consequences
- Trace entity dependencies (which NPC is at which location)
- Understand situation routing decisions

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

## Related Documentation

- **[gdd/01_vision.md](gdd/01_vision.md)** - Design pillars and philosophy
- **[gdd/03_core_loop.md](gdd/03_core_loop.md)** - Gameplay loops being tested
- **[arc42/10_quality_requirements.md](arc42/10_quality_requirements.md)** - Quality goals

---

**Note:** Session-specific artifacts (test reports, emotional arc logs) are created and archived per playtest session.
