# Playtest Documentation Index

**Status:** Active playtest documentation
**Last Updated:** 2025-11-30
**Test Suite:** 436 tests passing (97 architecture tests)

---

## Active Documentation

### Playtest Guides
- **[PLAYTEST_GUIDE.md](PLAYTEST_GUIDE.md)** - Complete playtest methodology and protocol
- **[PLAYTEST_LEARNINGS.md](PLAYTEST_LEARNINGS.md)** - Session learnings and findings

### Key Principles Tested (from gdd/01_vision.md)

1. **Impossible Choices** - Every decision forces sacrifice between valid alternatives. "I can afford A OR B, but not both."
2. **Perfect Information** - All costs visible before selection (no hidden gotchas). Strategic layer shows everything.
3. **Build Identity Through Constraint** - Specialization creates different viable paths. Cannot maximize all stats.
4. **Requirement Inversion** - Stats affect COST not ACCESS. High stats pay less, low stats pay more. Everyone progresses.
5. **Four-Choice Archetype** - Every A-story situation offers: stat-gated (free for specialists), resource (costs coins), challenge (skill test), fallback (always available)

### Design Tier Hierarchy (Conflict Resolution)

| Tier | Principle | Priority |
|------|-----------|----------|
| **TIER 1** | No Soft-Locks, Single Source of Truth | Never compromise |
| **TIER 2** | Playability, Perfect Information, Resource Scarcity | Compromise only for Tier 1 |
| **TIER 3** | Elegance, Verisimilitude | Compromise for Tier 1 or 2 |

### Architecture Compliance (HIGHLANDER Principles)

| Principle | Status | Description |
|-----------|--------|-------------|
| Service Statelessness | PASSING | Services contain logic, not state |
| Backend/Frontend Separation | PASSING | Backend returns domain values, frontend handles CSS |
| Entity Identity Model | PASSING | No IDs on domain entities (use object references) |
| Domain Collection Principle | PASSING | Use List<T> for domain collections |
| Single Source of Truth | PASSING | No dual storage of same data |

**Documented Exceptions:**
- `CardInstance.InstanceId` - Valid identity tracking for card instances of same template
- Pure utility services (NarrativeService, TimeBlockCalculator) - Don't need state parameters
- Enum-keyed Dictionary lookups - Configuration data, not identity collections
- `DisplayName` properties on Scene classes - Entity canonical names, not presentation

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

**Access:** Navigate to `/spawngraph` route while game is running (e.g., if game is at `http://localhost:<PORT>`, go to `http://localhost:<PORT>/spawngraph`)

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

### Game URL
**IMPORTANT:** The game is at the ROOT URL, NOT `/game`:
- **Correct:** `http://localhost:<PORT>/` (root path)
- **Wrong:** `http://localhost:<PORT>/game` (this route does not exist)

The spawn graph visualizer is at `/spawngraph` route.

### Fresh Game State
```bash
# Kill any zombie servers
taskkill //F //IM dotnet.exe

# Build and run (pick any available port in 5000-5999)
cd src && dotnet build
cd src && ASPNETCORE_URLS="http://localhost:$PORT" dotnet run --no-build

# Test (run BEFORE playtesting - 436 tests should pass)
cd Wayfarer.Tests.Project && dotnet test
```

**Port Selection:**
- Use any port in **5000-5999** range
- **Avoid 6000** - blocked by Chrome as "unsafe port"
- **Avoid 8000-9999** - may have zombie servers from previous sessions
- If port conflict: increment and retry

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
