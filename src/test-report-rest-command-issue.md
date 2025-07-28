# Wayfarer Frontend Test Report - REST Command Execution Issue

## Test Date: 2025-07-28

## Executive Summary
The Wayfarer game server has a critical issue preventing REST API command execution. Commands returned by the `/api/tutorial/location-actions` endpoint cannot be executed because their IDs change between the get and execute calls.

## Server Startup Status
✅ Server running successfully on port 5200
- No startup errors
- All services initialized correctly
- Blazor ServerPrerendered mode functioning
- SignalR hubs accessible

## Test Results

### 1. Game Initialization
✅ **PASS** - `/api/tutorial/start-game`
- Game starts successfully
- Tutorial auto-starts for new players
- Player initialized at correct location (Lower Ward - Abandoned Warehouse)

### 2. Tutorial Status Check
✅ **PASS** - `/api/tutorial/status`
- Tutorial state correctly tracked
- Current step information available
- Player state properly reported

### 3. Location Actions Discovery
✅ **PASS** - `/api/tutorial/location-actions`
- Actions discovered correctly
- REST command available: `RestCommand_1h_plus2_stamina`
- Conversation commands listed but marked as unavailable

### 4. REST Command Execution
✅ **PASS** - `/api/tutorial/execute-action/RestCommand_1h_plus2_stamina`
- REST command executes successfully
- Player stamina increased from 4 to 6
- Debug output shows command found and executed

### 5. Conversation Command Execution
❌ **FAIL** - `/api/tutorial/execute-action/ConverseCommand_[id]`
- Command IDs don't match between get and execute calls
- Get returns: `ConverseCommand_617d0cf9-084e-44a5-9ecb-5d05c7cd8250`
- Execute looks for: `ConverseCommand_60ac0a99-366d-43d4-9f5b-7f76f1a7c445`
- Error: "Command not found"

## Root Cause Analysis

The issue is in the command ID generation system:

1. **BaseGameCommand** constructor generates a new GUID for each command instance:
   ```csharp
   protected BaseGameCommand()
   {
       CommandId = Guid.NewGuid().ToString();
   }
   ```

2. **DiscoveredCommand.UniqueId** uses this CommandId:
   ```csharp
   return $"{Command.GetType().Name}_{Command.CommandId}";
   ```

3. **Problem Flow**:
   - `GetLocationActionsViewModel` calls `DiscoverCommands` → creates commands with GUIDs
   - `ExecuteActionAsync` calls `DiscoverCommands` again → creates NEW commands with DIFFERENT GUIDs
   - Command lookup fails because IDs don't match

## Code Modifications Made
Added debug logging to LocationActionsUIService to diagnose the issue:
- Logs the command ID being searched for
- Logs all available commands and their IDs
- Error messages now include this debug information

## Recommendations

### Immediate Fix Required
The command ID generation needs to be deterministic. Options:
1. **Use stable identifiers** based on command properties (e.g., NPC ID for conversations)
2. **Cache discovered commands** between get and execute calls
3. **Pass command parameters** instead of command IDs

### Testing Improvements
1. Add integration tests that verify command IDs remain stable
2. Implement E2E tests that execute discovered commands
3. Add logging to track command lifecycle

### Additional Observations
- REST commands work because they use a deterministic ID format: `RestCommand_{hours}h_{reward}`
- Only commands with generated GUIDs (like ConverseCommand) are affected
- The tutorial flow is blocked because conversation commands cannot be executed

## Summary
- **Server Status**: ✅ Running correctly
- **API Endpoints**: ✅ Functioning
- **REST Commands**: ✅ Working
- **Conversation Commands**: ❌ Broken due to unstable IDs
- **Overall Frontend Status**: ❌ FAIL - Critical functionality broken

The game is currently unplayable through the REST API due to this command ID issue. The fix should be straightforward - either make command IDs deterministic or cache commands between discovery and execution.