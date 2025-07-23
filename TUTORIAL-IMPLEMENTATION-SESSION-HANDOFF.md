# Tutorial Implementation Session Handoff

## Completed Tasks (7/12)

### ✅ 1. Integrate NarrativeManager into LocationActionManager
- Already integrated and working
- Action filtering through narrative system functional
- Conversation introductions can be overridden by narrative

### ✅ 2. Create NarrativeOverlay.razor component
- Created at `/src/Pages/Components/NarrativeOverlay.razor`
- Shows current objective, guidance, and progress
- Minimizable interface with mobile responsiveness
- Added `GetNarrativeDefinition()` method to NarrativeManager

### ✅ 3. Add tutorial auto-start to GameWorldManager
- Tutorial auto-starts on new game via `InitializeTutorialIfNeeded()`
- Removed patron obligation from game start (moved to Day 10)
- Checks for tutorial completion flag

### ✅ 4. Add narrative state to save/load system
- Complete serialization implemented via Task agents
- Added FlagServiceState and NarrativeManagerState
- Extended SerializablePlayerState with letter queue, tokens, obligations
- Full integration with existing save/load system

### ✅ 5. Create tutorial locations in locations.json
- Added: lower_ward, docks, merchants_rest
- Created spots: abandoned_warehouse, lower_ward_square, wharf, private_room
- Added routes connecting all tutorial locations
- Updated tutorial starting location in NarrativeBuilder

### ✅ 6. Create tutorial NPCs in npcs.json
- Added all 5 tutorial NPCs:
  - tam_beggar (Tam the Beggar)
  - martha_docks (Martha the Docker)
  - elena_scribe (Elena the Scribe)
  - fishmonger (Fishmonger Giles)
  - patron_intermediary (The Intermediary)

### ✅ 7. Fix movement tutorial to use proper travel
- Removed Rest action placeholders
- Added FlagService and NarrativeManager to TravelManager
- Tutorial flags set on location arrival:
  - `tutorial_first_movement` when player first travels
  - `tutorial_docks_visited` when reaching docks
- Updated narrative steps to be location-based

## In-Progress Tasks

### 🔄 8. Implement UI visibility controls during tutorial
**Current State**: Started modifying NavigationBar.razor
- Added conditional rendering for Letter Management section
- Need to add visibility check methods based on flags:
  - Queue: Show after `tutorial_first_letter_accepted`
  - Relations: Show after `first_token_earned`
  - Obligations: Show after `tutorial_patron_met` or first obligation
- **Important**: User specified to use player actions/knowledge, NOT day numbers

**Next Steps**:
1. Add the visibility check methods to NavigationBar.razor @code section
2. Check flags like `first_token_earned`, `tutorial_first_letter_accepted`, etc.
3. Apply same visibility controls to other UI elements

## Remaining Tasks (4)

### 📋 9. Add patron obligation creation during tutorial Day 10
- Remove patron obligation from game start ✅
- Add obligation creation when `patron_contact_met` flag is set
- Inject StandingObligationManager into NarrativeManager
- Add `ObligationToCreate` property to NarrativeStep

### 📋 10. Implement NPC scheduling for tutorial
- Martha should only appear in mornings
- Patron intermediary only at noon on Day 10
- Modify `NPC.IsAvailable()` to check narrative state
- Use flags rather than time-based scheduling

### 📋 11. Add stamina collapse mechanic
- Check for 0 stamina in TimeManager.AdvanceTime()
- Force rest action when stamina hits 0
- Implement "robbed while unconscious" consequence
- Add narrative message for collapse

### 📋 12. Test tutorial flow end-to-end
- Run through complete 10-day tutorial
- Verify all branches work (save money vs eat, burn token vs don't)
- Check save/load at each stage
- Ensure no soft locks possible

## Key Implementation Notes

1. **Movement System**: Travel is handled through TravelManager, not LocationAction
2. **Flag-Based Progress**: Tutorial uses flags for progression, not day numbers
3. **No Special Rules**: Following game design principle - leverage system handles priority
4. **UI Visibility**: Should be based on player achievements, not arbitrary days

## Critical Files Modified
- `/src/GameState/NarrativeBuilder.cs` - Tutorial narrative definition
- `/src/GameState/TravelManager.cs` - Added tutorial flag tracking
- `/src/GameState/GameWorldManager.cs` - Removed patron obligation from start
- `/src/Pages/Components/NarrativeOverlay.razor` - New tutorial UI component
- `/src/Content/Templates/locations.json` - Added tutorial locations
- `/src/Content/Templates/location_spots.json` - Added tutorial spots
- `/src/Content/Templates/routes.json` - Added tutorial routes
- `/src/Content/Templates/npcs.json` - Added tutorial NPCs

## Current Git Status
- Branch: letters-ledgers
- Multiple files modified but not committed
- Serialization changes made by Task agents but not detailed here