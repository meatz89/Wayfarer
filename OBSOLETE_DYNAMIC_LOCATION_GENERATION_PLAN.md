# OBSOLETE: Dynamic Location Generation Plan

**STATUS: OBSOLETE - Architecture was rejected in favor of "entities persist forever" principle**

**Why Obsolete:**
This plan was based on cleanup/temporary entities model where:
- Locations could be deleted/cleaned up based on "significance"
- Cleanup services would remove temporary entities
- Routes would be deleted when locations removed

**Current Architecture:**
- ALL entities persist forever within session (Locations, NPCs, Venues, Routes)
- No entity removal methods exist
- Skeleton replacement uses update-in-place pattern
- Capacity budget prevents unlimited generation (not cleanup)

**For Current Dynamic Generation Implementation:**
See:
- `design/07_content_generation.md` - Capacity budget system
- `08_crosscutting_concepts.md` - Entity persistence principles
- Commit b31939e - Entity removal violations eliminated

**This file preserved for historical context only. Do NOT implement anything from this plan.**
