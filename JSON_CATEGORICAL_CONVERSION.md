# JSON CATEGORICAL PROPERTY CONVERSION

**PHASE 1: INPUT BOUNDARY** - Remove all entity instance IDs from JSON files, replace with categorical properties.

**Contract Boundaries First**: Fix JSON input contract BEFORE touching parsers/domain/services.

---

## CONVERSION STATUS

### ✅ COMPLETED (6 files - PHASE 1 COMPLETE)

**01_foundation.json** (Locations):
- ❌ REMOVED: `venueId` from all locations
- ✅ ADDED: Categorical dimensions (`privacy`, `safety`, `activity`, `purpose`) to all locations
- Status: All 4 locations have complete categorical properties
- Pattern: Locations matched by properties, not IDs

**03_npcs.json** (NPCs):
- ❌ REMOVED: `venueId`, `locationId` from all 3 NPCs
- ✅ ADDED: `spawnLocation` (PlacementFilterDTO) to all NPCs
  - Elena: `{ privacyLevels: ["SemiPublic"], safetyLevels: ["Safe"], activityLevels: ["Moderate"], purposes: ["Dwelling"] }`
  - Thomas: Same as Elena (shares common room)
  - Merchant: `{ privacyLevels: ["Public"], safetyLevels: ["Safe"], activityLevels: ["Busy"], purposes: ["Commerce"] }`
- Status: All NPCs use categorical spawn location matching
- Pattern: NPCs spawn via categorical location properties

**11_exchange_cards.json** (Exchange Cards):
- ❌ REMOVED: `npcId: "merchant_general"` from all 7 exchange cards
- ✅ ADDED: `providerFilter` (PlacementFilterDTO) to all exchanges
  - All cards: `{ placementType: "NPC", professions: ["Merchant"] }`
- Status: All 7 exchange cards use categorical provider matching
- Pattern: Exchange cards match ANY merchant via categorical properties

**15_conversation_trees.json** (Conversation Trees):
- ❌ REMOVED: `npcId` from all 4 conversation trees
- ✅ ADDED: `participantFilter` (PlacementFilterDTO) to all trees
  - Elena welcome/late night: `{ professions: ["Innkeeper"], socialStandings: ["Notable"], storyRoles: ["Obstacle"], knowledgeLevels: ["Informed"] }`
  - Thomas intro: `{ professions: ["Dock_Boss"], socialStandings: ["Commoner"], storyRoles: ["Neutral"], knowledgeLevels: ["Ignorant"] }`
  - Merchant small talk: `{ professions: ["Merchant"], socialStandings: ["Commoner"], storyRoles: ["Neutral"], knowledgeLevels: ["Ignorant"] }`
- Status: All 4 conversation trees use categorical participant matching
- Pattern: Conversation trees trigger based on NPC categorical properties

**16_observation_scenes.json** (Observation Scenes):
- ❌ REMOVED: `locationId` from all 4 observation scenes
- ✅ ADDED: `locationFilter` (PlacementFilterDTO) to all scenes
  - Common room scenes (2): `{ privacyLevels: ["SemiPublic"], safetyLevels: ["Safe"], activityLevels: ["Moderate"], purposes: ["Dwelling"] }`
  - Square center scenes (2): `{ privacyLevels: ["Public"], safetyLevels: ["Safe"], activityLevels: ["Busy"], purposes: ["Commerce"] }`
- Status: All 4 observation scenes use categorical location matching
- Pattern: Observation scenes trigger at ANY location matching categorical properties

**Test/01_test_lodging.json** (Test Data):
- ❌ REMOVED: `venueId` from location, `LocationId` from NPC
- ✅ ADDED: Categorical dimensions to location (privacy, safety, activity, purpose)
- ✅ ADDED: `spawnLocation` (PlacementFilterDTO) to NPC
  - Test innkeeper: `{ privacyLevels: ["SemiPublic"], safetyLevels: ["Safe"], activityLevels: ["Moderate"], purposes: ["Dwelling"] }`
- Status: Test data validates categorical matching architecture
- Pattern: Test content uses same categorical properties as production content

---

## ⚠️ SPECIAL CASE (1 file)

**02_hex_grid.json** (Hex Grid):
- Contains: `locationId` in hex records
- **NOT AN ENTITY INSTANCE ID**: This is a **derived lookup**
- Pattern: `Hex.LocationId` is derived from `Location.HexPosition` (source of truth)
- HIGHLANDER compliant: Location owns position, Hex has reverse lookup for routing
- **NO CONVERSION NEEDED**: Derived lookups are acceptable per HIGHLANDER principle

---

## 📋 OPTIONAL CLEANUP (1 file)

**Narratives/conversation_narratives.json** (Conversation Narratives):
- Found: 12 instances of `"npcId": null`
- **NOT A VIOLATION**: All npcId values are null (categorical wildcards, not hardcoded IDs)
- Current Pattern: Narrative templates match ANY NPC based on flow/rapport ranges
- Optional Cleanup: Remove npcId field entirely (serves no purpose when always null)
- Impact: None - already using correct categorical architecture
- Status: **NO CONVERSION NEEDED** (optional field removal only)

---

## CONVERSION STRATEGY (Per File)

### ExchangeCardDTO Conversion

**Current Pattern (Violation)**:

Exchange cards use npcId property to reference specific merchant NPCs. Each card (buy_rope, buy_food) hardcodes one specific merchant.

**Target Pattern (Correct)**:

Exchange cards use providerFilter property containing categorical dimensions (placementType: "NPC", professions: ["Merchant"], purposes: ["Commerce"]). ExchangeCard parser calls EntityResolver.FindOrCreateNPC(providerFilter), which returns any NPC matching the filter. Same exchange card works with any merchant NPC in the game world.

**Integration**: ExchangeCard.Provider stores the object reference returned by EntityResolver, not an ID string. The parser resolves categorical filter to concrete NPC object once during initialization.

### ConversationTreeDTO Conversion

**Current Pattern (Violation)**:

Conversation trees use npcId property to reference specific character NPCs. Each tree (elena_greeting, thomas_intro) hardcodes one specific NPC.

**Target Pattern (Correct)**:

Conversation trees use participantFilter property containing categorical dimensions (placementType: "NPC", professions: ["Innkeeper"], storyRoles: ["Obstacle"], socialStandings: ["Notable"]). ConversationTree parser calls EntityResolver.FindOrCreateNPC(participantFilter). Same conversation tree works with any NPC matching the filter (not just Elena, but any innkeeper with Obstacle role).

**Integration**: ConversationTree.Participant stores the object reference returned by EntityResolver, not an ID string.

### ObservationSceneDTO Conversion

**Current Pattern (Violation)**:

Observation scenes use locationId property to reference specific location entities. Each scene (observe_common_room) hardcodes one specific location.

**Target Pattern (Correct)**:

Observation scenes use locationFilter property containing categorical dimensions (placementType: "Location", privacyLevels: ["SemiPublic"], purposes: ["Dwelling"], activityLevels: ["Moderate"]). ObservationScene parser calls EntityResolver.FindOrCreateLocation(locationFilter). Same observation scene works at any location matching the filter (not just common_room, but any semi-public dwelling location).

**Integration**: ObservationScene.Location stores the object reference returned by EntityResolver, not an ID string.

---

## VERIFICATION CRITERIA

After conversion, verify:

✅ **Zero entity instance IDs in JSON** (grep returns no matches for `npcId`, `locationId`, `venueId` except hex grid derived lookup)
✅ **All entity references use PlacementFilterDTO** (categorical properties only)
✅ **Parsers use EntityResolver.FindOrCreate** (no ID lookups)
✅ **Domain entities have object references** (no ID properties)
✅ **Same content works across procedural worlds** (template independence verified)

---

## ✅ PHASE 1: JSON INPUT BOUNDARY (COMPLETE)

**Converted Files** (6 total):
1. ✅ **01_foundation.json** - Locations with categorical dimensions
2. ✅ **03_npcs.json** - NPCs with spawnLocation filters
3. ✅ **11_exchange_cards.json** - Exchange cards with providerFilter (7 cards)
4. ✅ **15_conversation_trees.json** - Conversation trees with participantFilter (4 trees)
5. ✅ **16_observation_scenes.json** - Observation scenes with locationFilter (4 scenes)
6. ✅ **Test/01_test_lodging.json** - Test data with categorical properties

**Verification**:
- ✅ Zero entity instance IDs in JSON (except Hex.LocationId derived lookup)
- ✅ All entity references use PlacementFilterDTO (categorical properties only)
- ✅ 15 entity instance ID violations eliminated in Phase 1

**Phase 1 Status**: COMPLETE

**Next Step**: Phase 2 (Parser Transformation) - Update DTOs and parsers to use EntityResolver with new JSON structure.
