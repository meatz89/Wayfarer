# Debug Test Instructions

## Testing NPC Visibility and Interaction

1. **Start the game**
   - Run `cd src && dotnet run`
   - Navigate to http://localhost:5011

2. **Check initial state**
   - You should start at Millbrook Village at Dawn (6 AM)
   - Look for the location spots: Market, Tavern, Workshop

3. **Test NPC visibility**
   - Click on each location spot card
   - You should see NPCs listed under "People:" section
   - Workshop Master should show at Workshop spot
   - Market Trader should show at Market spot  
   - Tavern Keeper should show at Tavern spot
   - NPCs should have red/green status indicators

4. **Test NPC interaction**
   - Click on an NPC card (it should highlight)
   - If NPC is available (green dot), you should see available actions
   - If NPC is unavailable (red dot), you should see when they're next available

5. **Debug output**
   - Check console for debug messages
   - Look for:
     - "Found X NPCs at spot Y"
     - "Of those, Z are available at [timeblock]"
     - "Selected NPC: [name]"

## Expected NPCs and Schedules

- **Workshop Master**: Available during Workshop_Hours (Dawn, Morning, Afternoon)
- **Market Trader**: Available during Market_Hours (Morning, Afternoon)  
- **Tavern Keeper**: Available Always (all time periods)

At Dawn, only Workshop Master and Tavern Keeper should be available (green).