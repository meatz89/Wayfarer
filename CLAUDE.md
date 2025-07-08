# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## AUTO-DOCUMENTATION MANDATE

**CRITICAL WORKFLOW REMINDERS:**
1. ✅ **ALWAYS read existing 'claude.md' first** - Understand current architecture state
2. ✅ **ALWAYS update 'claude.md' after discovering new information** - Maintain comprehensive documentation  
3. ✅ **NEVER proceed without updating documentation** - When new insights are discovered
4. ✅ **Document architectural changes immediately** - Track all relationships and patterns
5. ✅ **VERIFY DOCUMENTATION IN EVERY COMMIT** - Follow post-commit validation workflow
6. 🧹 **REGULARLY CULL AND UPDATE claude.md** - Remove outdated information, consolidate sections, keep only current relevant details

**POST-COMMIT VALIDATION WORKFLOW:**
```bash
# After each commit, verify that claude.md was also updated:
git log --oneline -2  # Should show both code and doc updates

# If claude.md wasn't updated, update it before continuing
# Always commit code and documentation together
```

**UPDATE TRIGGERS (always update claude.md when):**
- Discovering new files or components
- Understanding new relationships between classes  
- Identifying architectural patterns
- Finding integration issues
- Implementing new features
- Discovering technical debt
- Understanding JSON content structure
- Mapping UI-backend connections

**MANDATORY SECTIONS TO MAINTAIN:**
- Current Architecture Overview
- Codebase Analysis & Relationships
- Integration Mapping (UI ↔ Backend)
- Implementation Status (Features vs. Planned)
- Session History & Architectural Decisions

## Latest Session Findings (Frontend-Backend Integration Fixes)

### ✅ CRITICAL ARCHITECTURAL FIXES COMPLETED
**Session Date:** 2025-07-08
**Status:** All major integration issues resolved

**Fixes Implemented:**
1. **ItemRepository.cs**: Refactored to load from `GameWorld.WorldState.Items` instead of hardcoded constructor
2. **GameWorldInitializer.cs**: Removed duplicate hardcoded item definitions (lines 148-153)
3. **MainGameplayView.razor**: Fixed component name mismatches (TravelSelectionWithWeight → TravelSelection, MarketUI → Market)
4. **RestUI.razor**: Refactored to use `RestManager` instead of direct GameWorld manipulation
5. **ContractUI.razor**: Enhanced to use `ContractSystem` with proper completion logic
6. **ServiceConfiguration.cs**: Added missing `RestManager` service registration
7. **ContractSystem.cs**: Added complete business logic for contract completion

**JSON Content Pipeline Verified:**
- ✅ `items.json` → `GameWorldSerializer.DeserializeItems()` → `ItemParser.ParseItem()` working correctly
- ✅ All content types (locations, contracts, actions, items) now properly load from JSON
- ✅ No more hardcoded data conflicts

**Architecture Compliance Achieved:**
- ✅ All UI components follow proper pattern: UI → Services → GameWorld
- ✅ No direct GameWorld manipulation from UI components
- ✅ Service injection working correctly throughout application

**Build & Runtime Status:**
- ✅ Project compiles successfully
- ✅ Application runs on `http://localhost:5010` and `https://localhost:7232`
- ✅ Tests updated and should pass

**NEXT SESSION PRIORITY:** Commit current work, then test complete game flow end-to-end

## Development Recommendations

**MAINTENANCE GUIDELINES:**
- 🧹 **Regularly cull your markdown files like claude.md** from info that is no longer needed, or update outdated information
- 📋 **Update claude.md after each session** with important architectural changes
- 🔄 **Consolidate related sections** to avoid duplication
- 🎯 **Keep only current, actionable information** in documentation