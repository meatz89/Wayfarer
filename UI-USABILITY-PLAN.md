# UI USABILITY IMPROVEMENT PLAN

## **CRITICAL PROBLEM STATEMENT**

The current UI fails to communicate basic game mechanics to players. Players cannot understand:
- When NPCs are available for services
- Which NPCs provide which services  
- Why markets are closed
- How to plan activities across time blocks

This is **not strategic information** - this is **basic game functionality** that must be clearly communicated.

## **IMPLEMENTATION PLAN**

### **PHASE 1: Core Information Display (CRITICAL)**

**Goal**: Make basic NPC availability and service information visible to players

#### **1.1 Enhanced NPC Schedule Display in LocationSpotMap**
- Show NPC availability schedules clearly
- Display which services each NPC provides
- Indicate current availability status

**Files to Modify:**
- `src/Pages/LocationSpotMap.razor` - Add schedule display
- `src/Pages/LocationSpotMap.razor.cs` - Add schedule helper methods

**Implementation:**
```html
<div class="npc-schedule-info">
    <div class="npc-name">@npc.Name</div>
    <div class="npc-profession">@npc.Profession.ToString().Replace("_", " ")</div>
    <div class="npc-availability">
        Available: @GetNPCScheduleDescription(npc.AvailabilitySchedule)
    </div>
    <div class="npc-services">
        Services: @string.Join(", ", npc.ProvidedServices.Select(s => s.ToString().Replace("_", " ")))
    </div>
    <div class="npc-current-status @(npc.IsAvailable(currentTime) ? "available" : "unavailable")">
        @(npc.IsAvailable(currentTime) ? "Available Now" : "Not Available")
    </div>
</div>
```

#### **1.2 Enhanced Market Status Display**
- Show which traders are available at current time
- Display when market will open if closed
- List all traders and their schedules

**Files to Modify:**
- `src/Pages/Market.razor` - Enhanced market status section
- `src/GameState/MarketManager.cs` - Add detailed market status methods

**Implementation:**
```html
<div class="market-trader-info">
    <h4>Trading Available</h4>
    <div class="current-traders">
        @foreach (var trader in GetCurrentTraders())
        {
            <div class="trader-status available">
                <span class="trader-name">@trader.Name</span>
                <span class="trader-schedule">(@GetScheduleDescription(trader.AvailabilitySchedule))</span>
            </div>
        }
    </div>
    
    @if (!GetCurrentTraders().Any())
    {
        <div class="no-traders">
            <div class="status-message">No traders available right now</div>
            <div class="next-availability">
                <h5>Traders will be available:</h5>
                @foreach (var trader in GetAllTraders())
                {
                    <div class="trader-schedule">
                        <span class="trader-name">@trader.Name</span>
                        <span class="next-time">Next available: @GetNextAvailableTime(trader)</span>
                    </div>
                }
            </div>
        </div>
    }
</div>
```

#### **1.3 Service Provider Information**
- Clearly connect NPCs to their functions
- Show which NPCs are required for which activities

**Files to Modify:**
- `src/GameState/GameWorldManager.cs` - Add NPC service query methods

### **PHASE 2: Service Availability Planning (HIGH PRIORITY)**

**Goal**: Allow players to plan activities across time blocks

#### **2.1 Time-Based Service Availability**
- Show what services are available at each time block
- Display when specific NPCs will be available

**Files to Modify:**
- `src/Pages/MainGameplayView.razor` - Add service availability display
- `src/GameState/GameWorldManager.cs` - Add time-based service queries

**Implementation:**
```html
<div class="time-block-services">
    <h4>Available This Time Block (@currentTime)</h4>
    <div class="available-services">
        @foreach (var service in GetAvailableServices(currentTime))
        {
            <div class="service-item">
                <span class="service-name">@service.Type.ToString().Replace("_", " ")</span>
                <span class="service-providers">
                    (@string.Join(", ", service.Providers.Select(p => p.Name)))
                </span>
            </div>
        }
    </div>
</div>
```

#### **2.2 NPC Schedule Helper Methods**
- Add methods to query NPC availability
- Provide schedule descriptions for UI display

**Files to Modify:**
- `src/GameState/GameWorldManager.cs` - Add NPC schedule query methods
- `src/Content/NPCRepository.cs` - Add schedule-based queries

### **PHASE 3: Comprehensive Availability Component (HIGH PRIORITY)**

**Goal**: Create reusable component for NPC availability information

#### **3.1 NPCAvailabilityComponent**
- Reusable component for displaying NPC schedules
- Consistent information across all UI screens

**Files to Create:**
- `src/Components/NPCAvailabilityComponent.razor`
- `src/Components/NPCAvailabilityComponent.razor.cs`

#### **3.2 Time Block Planning UI**
- Show what will be available in future time blocks
- Help players plan multi-step activities

**Files to Modify:**
- `src/Pages/MainGameplayView.razor` - Add time block planning section

### **PHASE 4: Visual Enhancements and Testing (MEDIUM PRIORITY)**

**Goal**: Polish and validate the UI improvements

#### **4.1 Visual Schedule Indicators**
- Add icons and color coding for availability
- Visual time block indicators

#### **4.2 Comprehensive Testing**
- Unit tests for new UI helper methods
- Integration tests for NPC availability display
- User experience validation

## **SUCCESS CRITERIA**

Players should be able to answer these questions WITHOUT guessing:

1. **"When can I trade?"** - Clear display of trading hours and which NPCs provide trading
2. **"Who do I need to talk to for X?"** - Clear connection between NPCs and services
3. **"Why is this closed?"** - Clear explanation of NPC availability requirements
4. **"When will this be available?"** - Clear schedule information for planning
5. **"What can I do right now?"** - Clear display of currently available services

## **TECHNICAL IMPLEMENTATION DETAILS**

### **New Methods Required:**

#### **GameWorldManager.cs**
```csharp
public List<NPC> GetCurrentlyAvailableNPCs(string locationId)
public List<NPC> GetNPCsProvidingService(ServiceTypes service)
public string GetNPCScheduleDescription(Schedule schedule)
public string GetNextAvailableTime(NPC npc)
public List<ServiceInfo> GetAvailableServices(TimeBlocks timeBlock)
```

#### **MarketManager.cs**
```csharp
public List<NPC> GetCurrentTraders(string locationId)
public List<NPC> GetAllTraders(string locationId)
public string GetDetailedMarketStatus(string locationId)
```

#### **NPCRepository.cs**
```csharp
public List<NPC> GetNPCsByService(ServiceTypes service)
public List<NPC> GetNPCsBySchedule(Schedule schedule)
public Dictionary<TimeBlocks, List<NPC>> GetNPCAvailabilityMap(string locationId)
```

### **New Data Structures:**

```csharp
public class ServiceInfo
{
    public ServiceTypes Type { get; set; }
    public List<NPC> Providers { get; set; }
    public bool IsCurrentlyAvailable { get; set; }
}

public class NPCAvailabilityInfo
{
    public NPC NPC { get; set; }
    public bool IsCurrentlyAvailable { get; set; }
    public List<TimeBlocks> AvailableTimes { get; set; }
    public TimeBlocks? NextAvailableTime { get; set; }
}
```

## **PRIORITY ORDER**

1. **CRITICAL (Phase 1)**: Basic NPC schedule display - players must understand when NPCs are available
2. **HIGH (Phase 2)**: Service availability planning - players must be able to plan activities
3. **HIGH (Phase 3)**: Comprehensive availability component - consistent information display
4. **MEDIUM (Phase 4)**: Visual enhancements and testing - polish and validation

## **VALIDATION PLAN**

After implementation, verify that a new player can:
1. Understand why the market is closed
2. See when trading will be available
3. Plan activities across time blocks
4. Connect NPCs to their services
5. Navigate the game without confusion about basic mechanics

This is fundamental game usability - not optional strategic information.