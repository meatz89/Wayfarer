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

**Current Wrong Pattern**:
```json
{
  "id": "buy_basic_supplies",
  "npcId": "merchant_general",
  "cardType": "Purchase",
  "items": ["basic_supplies"]
}
```

**Target Correct Pattern**:
```json
{
  "id": "buy_basic_supplies",
  "providerFilter": {
    "placementType": "NPC",
    "professions": ["Merchant"],
    "purposes": ["Commerce"]
  },
  "cardType": "Purchase",
  "items": ["basic_supplies"]
}
```

**EntityResolver Integration**:
- ExchangeCard parser calls `entityResolver.FindOrCreateNPC(providerFilter)`
- Returns NPC object reference (not ID string)
- ExchangeCard.Provider property holds object reference
- Same exchange card works with any merchant matching filter

### ConversationTreeDTO Conversion

**Current Wrong Pattern**:
```json
{
  "id": "elena_greeting",
  "npcId": "elena_innkeeper",
  "rootNodeId": "greeting_start"
}
```

**Target Correct Pattern**:
```json
{
  "id": "elena_greeting",
  "participantFilter": {
    "placementType": "NPC",
    "professions": ["Innkeeper"],
    "storyRoles": ["Obstacle"],
    "socialStandings": ["Notable"]
  },
  "rootNodeId": "greeting_start"
}
```

**EntityResolver Integration**:
- ConversationTree parser calls `entityResolver.FindOrCreateNPC(participantFilter)`
- Returns NPC object reference
- ConversationTree.Participant property holds object reference
- Same conversation tree works with any innkeeper matching filter

### ObservationSceneDTO Conversion

**Current Wrong Pattern**:
```json
{
  "id": "observe_common_room",
  "locationId": "common_room",
  "observations": [...]
}
```

**Target Correct Pattern**:
```json
{
  "id": "observe_common_room",
  "locationFilter": {
    "placementType": "Location",
    "privacyLevels": ["SemiPublic"],
    "purposes": ["Dwelling"],
    "activityLevels": ["Moderate"]
  },
  "observations": [...]
}
```

**EntityResolver Integration**:
- ObservationScene parser calls `entityResolver.FindOrCreateLocation(locationFilter)`
- Returns Location object reference
- ObservationScene.Location property holds object reference
- Same observation scene works at any inn common room matching filter

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
