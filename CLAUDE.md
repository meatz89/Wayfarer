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

## PROJECT OVERVIEW: WAYFARER

**Wayfarer** is a medieval life simulation RPG built as a Blazor Server application. It features a sophisticated, AI-driven narrative system with turn-based resource management gameplay focused on economic strategy, travel optimization, and contract fulfillment.

### Core Game Concept
Players are medieval wayfarers who must manage limited resources (coins, stamina, time blocks) while completing contracts, trading items between locations, and making strategic decisions about travel routes. The genius of the design is that **everything costs something else** - creating interconnected decision trees where optimizing one resource affects others.

## CURRENT ARCHITECTURE OVERVIEW

### System Architecture Pattern: **GameWorld-Centric Architecture**

```
┌─────────────────────────────────┐
│         UI Layer (Blazor)        │  ← Pages/*.razor components
│                │                │
│                ▼                │
├─────────────────────────────────┤
│      GameWorldManager          │  ← SINGLE ACTION GATEWAY
│                │                │
│                ▼                │
├─────────────────────────────────┤
│         GameWorld               │  ← SINGLE SOURCE OF TRUTH (STATE ONLY)
│                ▲                │
├─────────────────────────────────┤
│    Managers & Repositories      │  ← Business Logic (STATELESS)
│                │                │  ← Injected GameWorld via DI
├─────────────────────────────────┤
│      JSON Content Pipeline      │  ← Initial state population
└─────────────────────────────────┘
```

### **🚨 CRITICAL ARCHITECTURAL RULES**

#### **1. UI → GameWorldManager Gateway Pattern**
- **ALL UI actions** must go through GameWorldManager methods
- UI components can inject GameWorld + GameWorldManager + specific Managers for read queries
- **NEVER** inject managers for actions - only for reading data

#### **2. GameWorld = Single Source of Truth (State Only)**
- GameWorld ONLY holds state properties
- GameWorld NEVER calls other components
- GameWorld NEVER contains business logic methods
- All state reads must go through GameWorld

#### **3. Managers/Repositories = Stateless (GameWorld DI)**
- All managers inject GameWorld via DI constructor
- Managers NEVER hold private state
- Managers NEVER cache data locally
- GameWorld NEVER passed as method parameter

#### **4. JSON → GameWorld Population (One-Time)**
- JSON content parsed once at startup into GameWorld
- All runtime reads from GameWorld.WorldState.*
- Repositories NEVER cache locally

### Directory Structure & Responsibilities

**`/src/Pages/`** - Blazor UI Components (MUST use GameWorldManager only)
**`/src/GameState/`** - State Container & Gateway
- `GameWorld.cs` - **SINGLE SOURCE OF TRUTH** (state only, no business logic)
- `GameWorldManager.cs` - **ACTION GATEWAY** (all UI actions route here)

**`/src/GameState/` Managers** - Business Logic Services (STATELESS)
- All managers inject GameWorld, no local state

**`/src/Content/`** - JSON Content Pipeline (Initialization Only)
- Populates GameWorld once at startup

**`/src/Content/` Repositories** - Data Access (STATELESS)
- Read from GameWorld.WorldState, never cache locally

## UI ↔ BACKEND INTEGRATION PATTERNS

### **🚨 MANDATORY UI Injection Pattern**
```csharp
// ✅ CORRECT: Only inject GameWorldManager in UI
@inject GameWorldManager GameManager

// ❌ INCORRECT: Never inject managers directly in UI for actions
```

### **ENFORCED Data Flow Architecture**
```
UI Component → GameWorldManager → Specific Manager → GameWorld State
     ↑                                                       ↓
StateHasChanged() ←──────────── State Change ───────────────┘
```

## LATEST SESSION HISTORY & STATUS

### ✅ COMPILATION FIXES & ARCHITECTURAL VALIDATION (2025-07-08 - Session 2)
**Status:** ✅ COMPLETE - Build successful, all 4 architectural rules maintained

#### **🎯 SESSION OBJECTIVES ACHIEVED**
1. **✅ Fixed compilation errors** from previous session's architectural refactor
2. **✅ Maintained architectural compliance** during compilation fixes  
3. **✅ Validated TDD readiness** - codebase ready for test-driven development
4. **✅ Updated documentation** with latest architectural findings

#### **📊 BUILD STATUS**
- **Compilation**: ✅ SUCCESS (0 errors, warnings only)
- **Architecture**: ✅ 100% COMPLIANT with all 4 rules
- **TDD Ready**: ✅ Codebase ready for test implementation

#### **🚀 NEXT STEPS IDENTIFIED**
1. **TDD Implementation**: Begin writing tests for core game flows
2. **End-to-end Testing**: Validate complete action flows work correctly  
3. **Integration Testing**: Verify UI ↔ GameWorldManager ↔ Managers ↔ GameWorld flow

---

### 🚨 MAJOR ARCHITECTURAL COMPLIANCE FIXES (2025-07-08 - Session 1)
**Status:** Critical architectural principles enforcement completed

**Major Violations Fixed:**
1. **UI Components**: Direct manager injections violating gateway pattern
2. **GameWorld**: Business logic methods violating state-only principle  
3. **Managers**: Local state caching violating stateless principle
4. **Repositories**: Local state caching instead of GameWorld reads

**Result:** All 4 architectural rules now enforced across entire codebase.

## DEVELOPMENT RECOMMENDATIONS

### **🚨 CRITICAL ARCHITECTURE ENFORCEMENT**

**MANDATORY PATTERNS:**
1. **UI Layer**: ONLY inject `GameWorldManager` - never inject other managers for actions
2. **GameWorld**: ONLY holds state - no business logic or calls to other components  
3. **Managers**: STATELESS with GameWorld injected via DI - never passed as parameters
4. **Repositories**: STATELESS - always read from GameWorld, never cache locally
5. **Initialization**: JSON → Parsers → GameWorld population (one-time only)

**CODE REVIEW CHECKLIST:**
- ✅ Does UI component only inject GameWorldManager for actions?
- ✅ Does GameWorld only contain state properties?  
- ✅ Do managers inject GameWorld via constructor DI?
- ✅ Do repositories read from GameWorld.WorldState?
- ✅ Are all actions routed through GameWorldManager?

**ANTI-PATTERNS TO AVOID:**
- ❌ Direct manager injection in UI components for actions
- ❌ GameWorld calling other components  
- ❌ Managers holding private state
- ❌ Repositories caching data locally
- ❌ Passing GameWorld as method parameters
- ❌ Reading JSON files during gameplay