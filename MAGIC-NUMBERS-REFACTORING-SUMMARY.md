# Magic Numbers Refactoring Summary

## Overview
Extracted magic numbers throughout the codebase into named constants to improve code readability and maintainability.

## Created Files

### `/src/GameState/Constants/GameConstants.cs`
A centralized location for all game constants that aren't part of the configurable GameConfiguration. Organized into logical sections:

- **LoadWeight**: Weight thresholds and stamina penalties
  - `LIGHT_LOAD_MAX = 3`
  - `MEDIUM_LOAD_MAX = 6`
  - Stamina penalties for each load category

- **UI**: User interface related constants
  - Wait option hours (2 and 4)
  - Streaming content estimation values
  - Display limits

- **Inventory**: Item and inventory constants
  - Default inventory capacity
  - Coins per weight unit (10 coins = 1 weight)
  - Heavy item threshold

- **Network**: HTTP client timeouts and retry settings

- **Patron**: Patron debt and leverage thresholds
  - Initial patron debt (-20)
  - Various debt thresholds

- **LetterQueue**: Queue position and deadline constants

- **Game**: Miscellaneous game constants like XP requirements

- **Validation**: Minimum counts for content validation

- **StringParsing**: String parsing separators and lengths

## Modified Files

### UI and Display Files
- `/src/Pages/TravelSelection.razor.cs` - Updated weight thresholds
- `/src/Services/TravelUIService.cs` - Updated weight thresholds and stamina penalties
- `/src/Pages/PlayerStatusView.razor.cs` - Updated weight status checks
- `/src/Pages/Inventory.razor.cs` - Updated weight calculations
- `/src/Services/RestUIService.cs` - Updated wait options and string parsing

### Game Mechanics Files
- `/src/GameState/TravelManager.cs` - Updated coin weight calculations and weight status
- `/src/Game/MainSystem/RouteOption.cs` - Updated weight penalty calculations
- `/src/GameState/MarketManager.cs` - Updated heavy item threshold
- `/src/GameState/PatronLetterService.cs` - Updated patron debt values
- `/src/GameState/Player.cs` - Updated XP to next level constant
- `/src/Content/LocationSpot.cs` - Updated XP to next level constant

### Service Files
- `/src/GameState/LetterCategoryService.cs` - Added GameConfiguration dependency for payment ranges
- `/src/UIHelpers/StreamingContentState.cs` - Updated streaming token estimation

### Test Files
- `/Wayfarer.E2ETests/E2E.Test.cs` - Updated HTTP client timeout and error display limit
- `/Wayfarer.Tests/PatronLetterLeverageTests.cs` - Fixed constructor to include GameConfiguration

## Key Benefits

1. **Centralized Constants**: All magic numbers are now in one place, making them easy to find and modify
2. **Self-Documenting Code**: Named constants explain their purpose better than raw numbers
3. **Consistency**: Using the same constant ensures consistent behavior across the codebase
4. **Maintainability**: Changing a game mechanic value now requires updating only one location

## Constants Still in Configuration

Some values remain in `GameConfiguration` as they are meant to be configurable:
- Letter queue size (8)
- Token thresholds for letter categories
- Stamina costs for various actions
- Time-related settings
- Payment ranges for letter categories

## Notes

- Some magic numbers in commands (stamina costs) were kept as literals with comments because injecting configuration into commands would require significant refactoring
- Starting values in Player class are kept as literals with explanatory comments as they're game balance decisions
- The GameConfiguration already handles many configurable values, so only truly constant values were extracted