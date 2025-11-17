# JSON CATEGORICAL PROPERTY CONVERSION

**PHASE 1: INPUT BOUNDARY** - Remove all entity instance IDs from JSON files, replace with categorical properties.

**Contract Boundaries First**: Fix JSON input contract BEFORE touching parsers/domain/services.

---

## CONVERSION STATUS

### ✅ COMPLETED (2 files)

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

---

## ⚠️ SPECIAL CASE (1 file)

**02_hex_grid.json** (Hex Grid):
- Contains: `locationId` in hex records
- **NOT AN ENTITY INSTANCE ID**: This is a **derived lookup**
- Pattern: `Hex.LocationId` is derived from `Location.HexPosition` (source of truth)
- HIGHLANDER compliant: Location owns position, Hex has reverse lookup for routing
- **NO CONVERSION NEEDED**: Derived lookups are acceptable per HIGHLANDER principle

---

## 📋 PENDING CONVERSION (6 files)

### High Priority (Tutorial Content)

**11_exchange_cards.json** (Exchange Cards):
- Found: 7 instances of `"npcId": "merchant_general"`
- Violation: Hardcoded entity instance ID reference
- Target Pattern: `"npcFilter": { "professions": ["Merchant"], "purposes": ["Commerce"] }`
- Impact: Exchange cards should match NPCs categorically, not by ID
- Why: Same exchange card must work with ANY merchant across procedural worlds

**15_conversation_trees.json** (Conversation Trees):
- Found: 4 instances of `npcId` (elena_innkeeper, thomas_foreman, merchant_general)
- Violation: Hardcoded entity instance ID references
- Target Pattern: `"participantFilter": { "professions": ["Innkeeper"], "storyRoles": ["Obstacle"] }`
- Impact: Conversation trees should trigger based on NPC categorical properties
- Why: Same conversation tree must work with ANY innkeeper across procedural worlds

**16_observation_scenes.json** (Observation/Mental Scenes):
- Found: 4 instances of `locationId` (common_room, square_center)
- Violation: Hardcoded entity instance ID references
- Target Pattern: `"locationFilter": { "privacyLevels": ["SemiPublic"], "purposes": ["Dwelling"] }`
- Impact: Observation scenes should trigger at locations matching categorical properties
- Why: Same observation scene must work at ANY inn common room across procedural worlds

### Medium Priority (Narrative Content)

**Narratives/conversation_narratives.json** (Conversation Narratives):
- Found: Unknown count of npcId references (grep shows matches)
- Violation: Hardcoded entity instance ID references
- Target Pattern: Embed narrative templates in conversation trees (no separate ID reference)
- Impact: Narratives coupled to categorical conversation triggers
- Why: Narrative follows conversation pattern, not specific NPC

### Low Priority (Test Content)

**Test/01_test_lodging.json** (Test Data):
- Found: Unknown count of locationId/npcId/venueId references
- Violation: Test data using old ID pattern
- Target Pattern: Convert to categorical properties for consistency
- Impact: Test data should validate categorical matching, not ID lookups
- Why: Tests verify correct architecture patterns

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

## EXECUTION ORDER

1. **ExchangeCardDTO** + 11_exchange_cards.json (7 violations)
2. **ConversationTreeDTO** + 15_conversation_trees.json (4 violations)
3. **ObservationSceneDTO** + 16_observation_scenes.json (4 violations)
4. **ConversationNarrativeDTO** + conversation_narratives.json (unknown count)
5. **Test data** + 01_test_lodging.json (low priority)

**Phase 1 (Input Boundary) Complete When**: All JSON files use categorical properties, zero entity instance ID references remain.

**Then Proceed To**: Phase 2 (Parser Transformation) - Update parsers to use EntityResolver with new JSON structure.
