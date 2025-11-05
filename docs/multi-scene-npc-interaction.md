# Multi-Scene NPC Interaction Architecture

## Core Concept

A single NPC can have multiple independent active scenes simultaneously. Each scene represents a distinct narrative thread with its own lifecycle, situations, and completion state. Players must see ALL active scenes as separate interaction options.

## Physical Presence vs Interactive Opportunities

**Physical Presence (Always Visible):**
NPCs exist in the game world as physical entities. When an NPC is present at a location, the player always sees them listed. This represents the fiction: "Elena is standing near the fireplace."

**Interactive Opportunities (Conditional):**
Interaction buttons appear only when the NPC has at least one active scene. Each active scene spawns a separate button with a descriptive label. This represents available conversation topics or interaction contexts.

**Example:**
- Elena is at the Common Room (physical presence - always shown)
- Elena has 2 active scenes: "Secure Lodging" and "Inn Trouble Brewing"
- UI shows Elena's card with TWO buttons: "Secure Lodging" and "Discuss Inn Trouble"
- Thomas is at the Common Room (physical presence - always shown)
- Thomas has 0 active scenes
- UI shows Thomas's card with NO buttons

## Scene Independence

Each scene maintains independent lifecycle and completion state:

- **Scene A**: "Secure Lodging" (Tutorial) - 4 situations cascading sequentially
- **Scene B**: "Inn Trouble Brewing" (Investigation) - 3 situations cascading sequentially

Completing Scene A does not affect Scene B. Both scenes remain visible and independently playable until each reaches its own completion criteria.

## Sequential Situations Within Scenes

Within a single scene, situations flow sequentially without interruption:

1. Player clicks "Secure Lodging" button
2. Scene activates, shows Situation 1: "Enter"
3. Player completes Situation 1
4. Scene auto-advances to Situation 2: "Lodging" (no return to location view)
5. Player completes Situation 2
6. Scene auto-advances to Situation 3: "Leave"
7. Player completes Situation 3
8. Scene completes, returns to location view

The scene state machine manages CurrentSituationId and AdvanceToNextSituation() for seamless narrative flow.

## Perfect Information Principle

Players must see ALL available interaction options to make strategic decisions. Hiding scenes because they lack aesthetic labels violates this principle. The architecture prioritizes functionality over cosmetics:

- Scene exists + active situation → Show button (even with placeholder label)
- No active scene → No button (nothing to engage with)

## Multi-Scene Display Pattern

The architecture shift from single-scene to multi-scene display:

**Before (Single Scene):**
- Query: FirstOrDefault() - find first active scene
- ViewModel: InteractionLabel (string) - single label
- UI: One button per NPC maximum

**After (Multi Scene):**
- Query: Where() - find ALL active scenes
- ViewModel: AvailableScenes (List<NpcSceneViewModel>) - multiple scene descriptors
- UI: Loop rendering one button per scene

## Label Derivation Hierarchy

When deriving button labels for scenes, use this fallback hierarchy:

1. Scene.DisplayName (explicit authored label)
2. First Situation.Name in scene (derived from situation content)
3. Placeholder "Talk to [NPC Name]" (functional default)

Never hide a functional scene because it lacks a pretty label. Playability trumps aesthetics.

## Navigation Routing

When player clicks scene button, navigation must route to SPECIFIC scene, not just NPC:

**Before:**
- Button click → npcId only
- Navigation searches GameWorld.Scenes for any active scene at this NPC
- Ambiguous when multiple scenes exist

**After:**
- Button click → (npcId, sceneId) pair
- Navigation uses sceneId for direct lookup
- Explicit routing to intended scene

## Spawn Independence

Scenes spawn independently from different sources:

- Tutorial scenes spawn at parse-time (concrete npcId binding)
- Obligation scenes spawn at runtime (categorical filters)
- Multiple obligations can spawn scenes at same NPC simultaneously
- Each scene operates independently until completion

This architectural pattern supports rich narrative branching where NPCs serve as hubs for multiple concurrent story threads.
