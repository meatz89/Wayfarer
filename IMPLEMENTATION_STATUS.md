# Wayfarer Implementation Status Matrix

## PURPOSE

This document tracks implementation status of all major features and systems in Wayfarer. Use this to understand what's real vs. planned.

**Last Updated:** 2025-01
**Verification Method:** Code inspection (Scene.cs, GameWorld.cs, SituationArchetypeCatalog.cs)

**Status Legend:**
- ✅ **IMPLEMENTED:** Fully working, tested, in production use
- 🚧 **IN PROGRESS:** Partially implemented, actively being developed
- 📋 **DESIGNED:** Architecture documented, implementation not started
- 💡 **PLANNED:** Intended future feature, no detailed design yet
- ⚠️ **DEPRECATED:** Still in code but being phased out
- ❌ **REMOVED:** Previously existed, now deleted

---

## CORE ARCHITECTURE

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Scene System | ✅ IMPLEMENTED | Replaces Goal/Obstacle architecture | Scene.cs, GameWorld.Scenes |
| Situation Embedding | ✅ IMPLEMENTED | Situations stored in Scene.Situations, NOT GameWorld | Scene.cs L75, No GameWorld.Situations |
| SceneTemplate System | ✅ IMPLEMENTED | Parse-time templates for scene generation | SceneTemplate.cs, GameWorld.SceneTemplates L159 |
| Provisional Scene State | ✅ IMPLEMENTED | Perfect information preview before finalization | Scene.cs L112-115, SceneState enum |
| Active/Completed/Expired States | ✅ IMPLEMENTED | Scene lifecycle fully implemented | Scene.cs L116, SceneState enum |
| Query-Time Action Instantiation | ✅ IMPLEMENTED | Actions created on-demand, not at spawn | Situation.cs L34, InstantiationState enum |
| Three-Tier Timing Model | ✅ IMPLEMENTED | Templates → Scenes/Situations → Actions | Architecture pattern confirmed |

---

## SPATIAL SYSTEMS

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Venue System | ✅ IMPLEMENTED | Clusters of locations | GameWorld.cs L8, Venue.cs |
| Location System | ✅ IMPLEMENTED | Specific places within venues | GameWorld.cs L9, Location.cs |
| Hex Map | ✅ IMPLEMENTED | Hex grid for spatial scaffolding | GameWorld.cs L16, HexMap.cs |
| Route System (JSON-authored) | ✅ IMPLEMENTED | Routes loaded from JSON packages | GameWorld.cs L190, RouteOption.cs |
| Route System (Hex-based procedural) | 📋 DESIGNED | Auto-generation from hex pathfinding | HEX_TRAVEL_SYSTEM.md L156-164 |
| Travel Between Venues | ✅ IMPLEMENTED | Route-based travel with scenes | Travel system operational |
| Instant Movement Within Venue | ✅ IMPLEMENTED | No cost for same-venue navigation | Confirmed in movement code |
| "Spot" Sub-Locations | ❌ REMOVED | Earlier architecture used "Spots" | No Spot.cs, docs mention legacy term |

---

## CONTENT GENERATION

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Situation Archetype Catalog | ✅ IMPLEMENTED | 21 archetypes for choice generation | SituationArchetypeCatalog.cs |
| 5 Core Archetypes | ✅ IMPLEMENTED | Confrontation, Negotiation, Investigation, Social Maneuvering, Crisis | SituationArchetypeCatalog.cs L94-229 |
| 10 Expanded Archetypes | ✅ IMPLEMENTED | Service Transaction through Recruitment | SituationArchetypeCatalog.cs L244-529 |
| 6 Specialized Service Archetypes | ✅ IMPLEMENTED | Rest Preparation, Service Negotiation, etc. | SituationArchetypeCatalog.cs L544-720 |
| Universal Property Scaling | ✅ IMPLEMENTED | PowerDynamic, Quality, NPCDemeanor scale difficulty | SituationArchetypeCatalog.cs L774-804 |
| Categorical Property Translation | 🚧 IN PROGRESS | Some properties implemented (Demeanor), others hardcoded | SCENE_DATA_FLOW_ANALYSIS.md GAP 3.1 |
| Marker Resolution System | ✅ IMPLEMENTED | Templates reference markers, resolved at finalization | Scene.cs L206-211, MarkerResolutionMap |
| Dependent Resource Generation | ✅ IMPLEMENTED | Scenes can create locations/items dynamically | Scene.cs L183-199, CreatedLocationIds |
| AI Narrative Generation | 🚧 IN PROGRESS | Architecture exists, integration partial | Situation.cs L214, GeneratedNarrative |

---

## A-STORY (MAIN STORY)

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| A1-A3 Tutorial Scenes | ✅ IMPLEMENTED | First 3 authored tutorial scenes | Scene.Category = MainStory, MainStorySequence 1-3 |
| A4-A10 Tutorial Scenes | 📋 DESIGNED | Designed but not authored | INFINITE_A_STORY_ARCHITECTURE.md |
| A11+ Procedural Continuation | 📋 DESIGNED | Infinite procedural A-story | INFINITE_A_STORY_ARCHITECTURE.md L447-494 |
| Procedural Phase Validation | 📋 DESIGNED | Runtime validation for generated scenes | INFINITE_A_STORY_ARCHITECTURE.md L447-475 |
| Simulation Validation | 📋 DESIGNED | Optional playthrough simulation | INFINITE_A_STORY_ARCHITECTURE.md L476-494 |

---

## TACTICAL CHALLENGE SYSTEMS

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Social Challenge System | ✅ IMPLEMENTED | Social tactical gameplay | GameWorld.cs L61-62, SocialSession |
| Mental Challenge System | ✅ IMPLEMENTED | Mental tactical gameplay | GameWorld.cs L63, MentalSession |
| Physical Challenge System | ✅ IMPLEMENTED | Physical tactical gameplay | GameWorld.cs L64, PhysicalSession |
| Social Cards | ✅ IMPLEMENTED | Card deck for social challenges | GameWorld.cs L55, SocialCard.cs |
| Mental Cards | ✅ IMPLEMENTED | Card deck for mental challenges | GameWorld.cs L56, MentalCard.cs |
| Physical Cards | ✅ IMPLEMENTED | Card deck for physical challenges | GameWorld.cs L58, PhysicalCard.cs |
| Challenge Outcome Rewards | ✅ IMPLEMENTED | Rewards applied after successful challenges | Challenge context tracking in GameWorld |

---

## RESOURCE SYSTEMS

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Strategic Resources | ✅ IMPLEMENTED | Coins, Health, Stamina, Focus, Hunger | Player.cs properties |
| Tactical Resources (Mental) | ✅ IMPLEMENTED | Progress, Attention, Exposure | MentalSession.cs |
| Tactical Resources (Physical) | ✅ IMPLEMENTED | Breakthrough, Danger, Momentum | PhysicalSession.cs |
| Tactical Resources (Social) | ✅ IMPLEMENTED | Doubt, Rapport, Trust | SocialSession.cs |
| InvestigationCubes (per-Location) | ✅ IMPLEMENTED | Localized mastery per location | Location.cs, InvestigationCubes property |
| StoryCubes (per-NPC) | ✅ IMPLEMENTED | Localized relationship depth | NPC.cs, StoryCubes property |
| ExplorationCubes (per-Route) | 📋 DESIGNED | Designed but property not found in Route | GameWorld.cs L759-763 mentions, not implemented |
| MasteryCubes (per-Deck) | ✅ IMPLEMENTED | Physical challenge mastery | Player.cs, MasteryCubes.GetMastery() |

---

## TIME & PROGRESSION

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Day/TimeBlock System | ✅ IMPLEMENTED | Day counter, 4 TimeBlocks per day | GameWorld.cs L6-7, TimeBlocks enum |
| Time Segment Tracking | ✅ IMPLEMENTED | Granular time tracking within blocks | TimeManager.cs |
| Scene Expiration | ✅ IMPLEMENTED | Scenes expire after ExpirationDays | Scene.cs L135, ExpiresOnDay |
| Deadline Tracking (Obligations) | ✅ IMPLEMENTED | NPCCommissioned obligations have deadlines | GameWorld.cs L502-520, DeadlineSegment |
| State Duration Tracking | ✅ IMPLEMENTED | States have DurationSegments | ActiveState.cs, DurationSegments property |

---

## UI & PRESENTATION

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Atmospheric Presentation Mode | ✅ IMPLEMENTED | Scenes as menu options | Scene.cs L57, PresentationMode.Atmospheric |
| Modal Presentation Mode | ✅ IMPLEMENTED | Full-screen scene takeover | Scene.cs L57, PresentationMode.Modal |
| Breathe Progression Mode | ✅ IMPLEMENTED | Return to menu after each situation | Scene.cs L65, ProgressionMode.Breathe |
| Cascade Progression Mode | ✅ IMPLEMENTED | Continue to next situation immediately | Scene.cs L65, ProgressionMode.Cascade |
| Multi-Situation Scene Resumption | ✅ IMPLEMENTED | Scenes resume after navigation | Scene.cs L344-390, ShouldActivateAtContext() |
| Context-Aware Routing | ✅ IMPLEMENTED | ContinueInScene vs ExitToWorld vs SceneComplete | Scene.cs L436-457, SceneRoutingDecision enum |
| Perfect Information Display | 🚧 IN PROGRESS | Costs/rewards visible, but format inconsistent | Requirement formula evaluation exists |

---

## SAVE/LOAD SYSTEM

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| GameWorld Serialization | ✅ IMPLEMENTED | Save entire game state | GameFacade save methods |
| Action Reconstruction | ✅ IMPLEMENTED | Actions recreated from Templates on load | Actions ephemeral, not saved |
| Template ID References | ✅ IMPLEMENTED | Save TemplateId, restore Template reference | Scene.cs L21-30, TemplateId |
| Marker Resolution Persistence | ✅ IMPLEMENTED | MarkerResolutionMap saved with Scene | Scene.cs L211, Dictionary property |

---

## LEGACY/DEPRECATED FEATURES

| Feature | Status | Notes | Evidence |
|---------|--------|-------|----------|
| Goal/Obstacle Architecture | ❌ REMOVED | Replaced by Scene/Situation | No Goal.cs, Obstacle.cs in codebase |
| Separate Situation Collection | ❌ REMOVED | Situations now embedded in Scenes | No GameWorld.Situations property |
| "Spot" Terminology | ❌ REMOVED | Replaced with "Location" | No Spot.cs, GLOSSARY.md clarifies |
| "Encounter" Terminology | ⚠️ DEPRECATED | Use "Situation" instead | WAYFARER_CORE_GAME_LOOP.md uses legacy term |

---

## UPCOMING FEATURES (PLANNED)

| Feature | Status | Priority | Notes |
|---------|--------|----------|-------|
| Hex-Based Route Generation | 📋 DESIGNED | Medium | HEX_TRAVEL_SYSTEM.md L156-164 |
| A4-A10 Tutorial Scenes | 📋 DESIGNED | High | Need authoring |
| A11+ Procedural A-Story | 📋 DESIGNED | Medium | Architecture complete, needs implementation |
| AI Narrative Finalization | 🚧 IN PROGRESS | High | Core integration exists, needs robustness |
| Categorical Property Completion | 🚧 IN PROGRESS | High | Some properties hardcoded (ServiceType, Quality, etc.) |
| ExplorationCubes Implementation | 📋 DESIGNED | Low | Methods exist, property not on Route |
| Procedural Scene Validation | 📋 DESIGNED | Medium | Validation framework designed |

---

## HOW TO UPDATE THIS DOCUMENT

**When to Update:**
1. Feature moves from one status to another (Designed → In Progress → Implemented)
2. New major feature added
3. Feature deprecated or removed
4. Implementation discovered during code review

**Update Process:**
1. Change status marker (✅/🚧/📋/💡/⚠️/❌)
2. Update Notes column with specific details
3. Update Evidence column with file/line references
4. Update "Last Updated" date at top
5. Commit with message: "docs: update implementation status for [feature]"

**Evidence Format:**
- File reference: `Scene.cs`
- Specific property: `Scene.cs L116, State property`
- Method: `Scene.cs L225-264, AdvanceToNextSituation()`
- Collection: `GameWorld.cs L165, Scenes collection`
- Absence: `No GameWorld.Situations property`

---

## VERIFICATION CHECKLIST

When updating status to IMPLEMENTED, verify:
- [ ] Feature exists in codebase (class/property/method found)
- [ ] Feature has unit tests (if applicable)
- [ ] Feature used in actual gameplay flow (not just defined)
- [ ] Feature documented in at least one doc file
- [ ] No TODOs or "// Not implemented" comments in relevant code

When marking feature as DESIGNED:
- [ ] Architecture document exists
- [ ] Entity relationships defined
- [ ] Data flow documented
- [ ] Integration points identified
- [ ] No implementation code exists yet

---

## RELATED DOCUMENTATION

- **GLOSSARY.md:** Canonical term definitions
- **ARCHITECTURE.md:** Overall system architecture
- **SCENE_INSTANTIATION_ARCHITECTURE.md:** Scene lifecycle details
- **INFINITE_A_STORY_ARCHITECTURE.md:** A-story sequencing
- **DESIGN_PHILOSOPHY.md:** Core design principles
