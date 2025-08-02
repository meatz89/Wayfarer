# System Cleanup and UI Integration Plan

## Overview
This document outlines the plan to remove wrongly conceptualized systems and properly implement remaining UI gaps in Wayfarer RPG.

## Phase 1: Remove Wrongly Implemented Systems

### 1.1 Delete Resource Gathering System
**Rationale**: Contradicts core game premise of being a letter carrier
- [ ] Delete `GatherResourcesCommand.cs`
- [ ] Remove gathering logic from `CommandDiscoveryService`
- [ ] Remove any UI references to resource gathering
- [ ] Update location definitions to remove gathering spots

### 1.2 Delete Market Browsing Command
**Rationale**: Information display doesn't need a command system
- [ ] Delete `BrowseCommand.cs`
- [ ] Remove browse logic from `CommandDiscoveryService`
- [ ] Integrate price display directly into Market UI

### 1.3 Delete Keep Secret Command
**Rationale**: Should be conversation choice, not standalone action
- [ ] Delete `KeepSecretCommand.cs`
- [ ] Remove from `CommandDiscoveryService`
- [ ] Remove from NPCActionsView
- [ ] Plan: Add secret-keeping as conversation choices in future

### 1.4 Delete Share Lunch Command
**Rationale**: Mechanical token farming without meaningful gameplay
- [ ] Delete `ShareLunchCommand.cs`
- [ ] Remove from `CommandDiscoveryService`
- [ ] Remove food consumption for socializing

### 1.5 Delete Equipment Socialize Command
**Rationale**: Equipment shouldn't determine relationship quality
- [ ] Delete `EquipmentSocializeCommand.cs`
- [ ] Remove equipment-based social bonuses
- [ ] Simplify to single socialize action per NPC

## Phase 2: Implement Missing UI Systems

### 2.1 Debt Management System
**Current State**: BorrowMoneyCommand exists but lacks UI
**Implementation**:
- [ ] Create `DebtManagementScreen.razor`
- [ ] Show all active debts with NPCs
- [ ] Display interest accumulation
- [ ] Add repayment interface
- [ ] Integrate with token debt system
- [ ] Show consequences of defaulting

### 2.2 Personal Errand System
**Current State**: PersonalErrandCommand exists but no UI
**Implementation**:
- [ ] Create `ErrandBoard.razor` component
- [ ] Display available errands from NPCs
- [ ] Show requirements and rewards
- [ ] Track errand progress
- [ ] Integrate with relationship system

### 2.3 Transport Selection
**Current State**: Hardcoded to walking
**Implementation**:
- [ ] Update TravelSelection.razor to show transport options
- [ ] Display equipment requirements for each transport
- [ ] Show stamina/time cost differences
- [ ] Allow selection based on owned equipment

### 2.4 Seal Management System
**Current State**: Endorsement conversion exists but UI incomplete
**Implementation**:
- [ ] Create `SealProgressionScreen.razor`
- [ ] Show collected endorsements
- [ ] Display conversion thresholds
- [ ] Allow seal selection/equipment
- [ ] Show seal benefits and privileges

### 2.5 Information Discovery UI
**Current State**: Information letters exist but effects unclear
**Implementation**:
- [ ] Create `DiscoveryLog.razor` component
- [ ] Track discovered routes, NPCs, secrets
- [ ] Show information letter effects
- [ ] Display discovery notifications

## Phase 3: Fix Information Visibility Issues

### 3.1 NPC Schedule Display
**Problem**: Players can't see when NPCs are available
**Solution**:
- [ ] Update LocationSpotMap to show NPC schedules
- [ ] Add "Available: Dawn, Morning" indicators
- [ ] Show next availability time when unavailable
- [ ] Display services each NPC provides

### 3.2 Service Planning Interface
**Problem**: Can't plan activities across time blocks
**Solution**:
- [ ] Enhance time planning UI in MainGameplayView
- [ ] Show which services available in each time block
- [ ] Display NPC availability patterns
- [ ] Allow activity chaining visualization

## Phase 4: Architectural Cleanup

### 4.1 Simplify CommandDiscoveryService
- [ ] Reduce dependencies (target: <5 from current 15+)
- [ ] Separate discovery from validation
- [ ] Remove UI-specific logic
- [ ] Create focused command validators

### 4.2 Consistent Command Execution
- [ ] All commands through GameFacade
- [ ] Unified error handling
- [ ] Consistent UI feedback
- [ ] Remove direct command calls from UI

## Implementation Priority

1. **Immediate (Phase 1)**: Remove wrong systems
   - Prevents further confusion
   - Simplifies codebase
   - Focuses on core mechanics

2. **High Priority**: 
   - NPC Schedule Display (basic usability)
   - Debt Management UI (economic loop)
   - Transport Selection (strategic choices)

3. **Medium Priority**:
   - Personal Errand System (relationship depth)
   - Seal Management (progression visibility)
   - Service Planning (time optimization)

4. **Low Priority**:
   - Information Discovery UI (polish)
   - Architectural cleanup (technical debt)

## Success Criteria

After implementation:
1. No "mechanical" token farming actions exist
2. All economic systems have complete UI
3. Players can see NPC availability without guessing
4. Transport choices are strategic, not hardcoded
5. Debt and progression systems are transparent
6. Core focus remains on letter delivery and relationships

## Excluded Features

These features are intentionally NOT being implemented:
- Resource gathering of any kind
- Equipment-based social advantages
- Time-wasting information commands
- Mechanical relationship building

The game should focus on its core premise: delivering letters, building relationships through meaningful choices, and navigating the social/economic landscape of the world.