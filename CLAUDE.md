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

## COMMON INITIALIZATION BUGS & SOLUTIONS

### ❌ **Bug Pattern: Parameter Not Assigned in Setter Methods**
**Symptom**: Method receives parameters but doesn't assign them to class properties
**Example**: `SetCurrentLocation(location, spot)` receives `spot` but doesn't set `CurrentLocationSpot = spot`
**Root Cause**: Missing assignment statement in setter method
**Solution**: Always verify parameters are actually assigned to intended properties

### ❌ **Bug Pattern: Incomplete State Initialization**
**Symptom**: Game starts but critical state properties remain null/empty
**Common Locations**: 
- `WorldState.SetCurrentLocation()` - must set both location AND spot
- `GameWorldManager.StartGame()` - must fully initialize all required state
- JSON deserialization - must handle all required properties
**Solution**: Follow complete initialization checklist for all state objects

### ❌ **Bug Pattern: Initialization Order Dependencies**
**Symptom**: Components depend on other components being initialized first
**Example**: LocationSpot depends on Location being set, Actions depend on LocationSpot
**Solution**: 
1. Initialize base objects first (Location)
2. Initialize dependent objects second (LocationSpot) 
3. Initialize derived objects last (Actions)
4. Use null checks and defensive programming

### ✅ **Initialization Architecture Guidelines**
1. **State-First Initialization**: Set all core state before creating dependent objects
2. **Null-Safe Progression**: Check for null state at each initialization step
3. **Complete Object Creation**: Don't leave objects in partially-initialized states
4. **Validation After Initialization**: Verify all required properties are set
5. **Clear Error Messages**: Log specific initialization failures for debugging

### 🔍 **Initialization Debugging Checklist**
When debugging initialization issues:
- ✅ Check all setter methods actually assign parameters
- ✅ Verify initialization order follows dependencies  
- ✅ Confirm JSON templates contain all required data
- ✅ Validate GameWorld state after each major initialization step
- ✅ Ensure UI polling receives complete, valid state objects