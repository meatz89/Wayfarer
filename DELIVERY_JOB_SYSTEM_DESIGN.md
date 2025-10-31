# Delivery Job System - Holistic Architecture Design

**Date:** 2025-10-31
**Purpose:** Core game loop - procedurally generated delivery jobs connecting Commercial locations

---

## I. THE CORE LOOP (Player Experience)

```
Morning at Commercial Location
  ↓
Player: "View Job Board" action
  ↓
Job Board Modal: Shows 3-5 available jobs
  - Destination location
  - Payment amount
  - Route danger/length
  - Cargo description (narrative flavor)
  ↓
Player selects job → Active job stored on Player
  ↓
Active job displayed in UI header ("Deliver grain to Town Square - 15 coins")
  ↓
Player: "Travel to Another Location" → selects destination route
  ↓
Travel happens (existing route system)
  ↓
Arrive at destination location
  ↓
Location action appears: "Complete Delivery: [job name]"
  ↓
Player executes completion action
  ↓
Rewards granted (+coins, clear active job)
  ↓
Evening: Player can secure room with earnings
  ↓
Next Morning: Repeat (sustainable infinite loop)
```

---

## II. DOMAIN ENTITY DESIGN

### DeliveryJob Entity

```csharp
/// <summary>
/// Represents a procedurally generated delivery job connecting two Commercial locations.
/// Jobs are generated at parse time from available routes.
/// </summary>
public class DeliveryJob
{
    // Identity
    public string Id { get; set; } = "";  // e.g., "delivery_square_to_inn_001"

    // Routing
    public string OriginLocationId { get; set; } = "";  // Where job is offered
    public string DestinationLocationId { get; set; } = "";  // Where delivery must be completed
    public string RouteId { get; set; } = "";  // Route connecting origin → destination

    // Economics
    public int Payment { get; set; }  // Coins rewarded on completion

    // Narrative
    public string CargoDescription { get; set; } = "";  // "A sack of grain", "Letter for the innkeeper"
    public string JobDescription { get; set; } = "";  // Full description shown in job board

    // Difficulty (categorical - translate via catalogue)
    public DifficultyTier DifficultyTier { get; set; }  // Simple, Moderate, Dangerous

    // Availability
    public bool IsAvailable { get; set; } = true;  // Can player accept this job?
    public List<TimeBlocks> AvailableAt { get; set; } = new();  // When is job offered
}

public enum DifficultyTier
{
    Simple,      // Short route, low danger, low pay
    Moderate,    // Medium route, medium danger, medium pay
    Dangerous    // Long route, high danger, high pay
}
```

**Why No Deadline?**
- Tutorial POC doesn't need time pressure
- Can add later: `public int DaysRemaining { get; set; }`

**Why Categorical Difficulty?**
- Catalogue Pattern: DifficultyTier translates to payment/description at parse time
- AI-friendly: Future procedural generation can specify "Moderate" without knowing payment scaling

---

## III. PARSE-TIME GENERATION (Catalogue Pattern)

### DeliveryJobCatalog

**Purpose:** Generate delivery jobs from available routes at parse time (not runtime!)

**Called By:** PackageLoader after routes are generated

**Generation Logic:**
```csharp
public static class DeliveryJobCatalog
{
    /// <summary>
    /// Generate delivery jobs from routes connecting Commercial locations.
    /// Called ONCE at parse time by PackageLoader.
    /// </summary>
    public static List<DeliveryJob> GenerateJobsFromRoutes(
        List<RouteOption> routes,
        List<Location> locations,
        GameWorld gameWorld)
    {
        List<DeliveryJob> jobs = new List<DeliveryJob>();

        // Find all Commercial locations (job origins and destinations)
        List<Location> commercialLocations = locations
            .Where(loc => loc.LocationProperties.Contains(LocationPropertyType.Commercial))
            .ToList();

        // Generate jobs for each route between Commercial locations
        foreach (RouteOption route in routes)
        {
            Location origin = gameWorld.GetLocation(route.OriginLocationSpot);
            Location destination = gameWorld.GetLocation(route.DestinationLocationSpot);

            // Both ends must be Commercial locations
            if (!commercialLocations.Contains(origin) || !commercialLocations.Contains(destination))
                continue;

            // Calculate difficulty tier from route properties
            DifficultyTier tier = CalculateDifficultyTier(route);

            // Calculate payment from route cost + profit margin
            int payment = CalculatePayment(route, tier);

            // Generate cargo description (narrative flavor)
            string cargo = GenerateCargoDescription(origin, destination, tier);

            jobs.Add(new DeliveryJob
            {
                Id = $"delivery_{origin.Id}_to_{destination.Id}",
                OriginLocationId = origin.Id,
                DestinationLocationId = destination.Id,
                RouteId = route.Id,
                Payment = payment,
                CargoDescription = cargo,
                JobDescription = $"Deliver {cargo} to {destination.Name}",
                DifficultyTier = tier,
                IsAvailable = true,
                AvailableAt = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday }
            });
        }

        return jobs;
    }

    private static DifficultyTier CalculateDifficultyTier(RouteOption route)
    {
        // Route length determines difficulty
        if (route.TravelTimeSegments <= 2) return DifficultyTier.Simple;
        if (route.TravelTimeSegments <= 4) return DifficultyTier.Moderate;
        return DifficultyTier.Dangerous;
    }

    private static int CalculatePayment(RouteOption route, DifficultyTier tier)
    {
        // Base payment covers: room (10) + travel costs + profit
        int basePay = 10;  // Room cost
        int travelCost = route.TravelTimeSegments * 2;  // Estimated stamina/food cost
        int profit = tier switch
        {
            DifficultyTier.Simple => 3,
            DifficultyTier.Moderate => 5,
            DifficultyTier.Dangerous => 10,
            _ => 3
        };

        return basePay + travelCost + profit;
    }

    private static string GenerateCargoDescription(Location origin, Location destination, DifficultyTier tier)
    {
        // Procedural cargo generation based on location types
        string[] cargos = tier switch
        {
            DifficultyTier.Simple => new[] { "a letter", "a small package", "bread for delivery" },
            DifficultyTier.Moderate => new[] { "a sack of grain", "barrels of ale", "trade goods" },
            DifficultyTier.Dangerous => new[] { "valuable documents", "rare spices", "a sealed chest" },
            _ => new[] { "a package" }
        };

        Random rng = new Random($"{origin.Id}{destination.Id}".GetHashCode());
        return cargos[rng.Next(cargos.Length)];
    }
}
```

**Parse-Time Call (PackageLoader):**
```csharp
// After routes are generated:
List<DeliveryJob> jobs = DeliveryJobCatalog.GenerateJobsFromRoutes(
    gameWorld.Routes,
    gameWorld.Locations,
    gameWorld);

gameWorld.AvailableDeliveryJobs = jobs;
```

---

## IV. GAMEWORLD INTEGRATION

### Add to GameWorld.cs

```csharp
public class GameWorld
{
    // Existing...
    public List<Location> Locations { get; set; } = new();
    public List<RouteOption> Routes { get; set; } = new();

    // NEW: Delivery job storage
    public List<DeliveryJob> AvailableDeliveryJobs { get; set; } = new();

    // Helper methods
    public List<DeliveryJob> GetJobsAvailableAt(string locationId, TimeBlocks currentTime)
    {
        return AvailableDeliveryJobs
            .Where(job => job.OriginLocationId == locationId)
            .Where(job => job.IsAvailable)
            .Where(job => job.AvailableAt.Count == 0 || job.AvailableAt.Contains(currentTime))
            .ToList();
    }

    public DeliveryJob GetJobById(string jobId)
    {
        return AvailableDeliveryJobs.FirstOrDefault(j => j.Id == jobId);
    }
}
```

### Add to Player.cs

```csharp
public class Player
{
    // Existing...
    public int Coins { get; set; }
    public int Health { get; set; }

    // NEW: Active delivery tracking
    public string ActiveDeliveryJobId { get; set; } = "";  // Empty = no active job

    // Helper methods
    public bool HasActiveDeliveryJob => !string.IsNullOrEmpty(ActiveDeliveryJobId);

    public void AcceptDeliveryJob(string jobId)
    {
        if (HasActiveDeliveryJob)
            throw new InvalidOperationException("Player already has an active delivery job");
        ActiveDeliveryJobId = jobId;
    }

    public void CompleteDeliveryJob()
    {
        if (!HasActiveDeliveryJob)
            throw new InvalidOperationException("No active delivery job to complete");
        ActiveDeliveryJobId = "";
    }
}
```

---

## V. LOCATION ACTIONS

### A. "View Job Board" Action

**Generated By:** LocationActionCatalog at parse time for Commercial locations

**Add to LocationActionType enum:**
```csharp
public enum LocationActionType
{
    Travel,
    Rest,
    SecureRoom,
    Work,
    ViewJobBoard,  // NEW
    CompleteDelivery  // NEW
}
```

**Add to LocationActionCatalog.GeneratePropertyBasedActions():**
```csharp
// Commercial property → Work action + Job Board action
if (location.LocationProperties.Contains(LocationPropertyType.Commercial))
{
    // Existing Work action...

    // NEW: Job Board action
    actions.Add(new LocationAction
    {
        Id = $"view_job_board_{location.Id}",
        SourceLocationId = location.Id,
        Name = "View Job Board",
        Description = "Browse available delivery jobs. Accept a job to earn coins by transporting goods.",
        ActionType = LocationActionType.ViewJobBoard,
        Costs = ActionCosts.None(),
        Rewards = ActionRewards.None(),
        RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Commercial },
        Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday },
        Priority = 120
    });
}
```

### B. "Complete Delivery" Action

**Dynamically Generated:** LocationActionManager checks if player has active job and is at destination

**NOT from LocationActionCatalog** - this is query-time conditional action

**Add to LocationActionManager.GetLocationActions():**
```csharp
public List<LocationActionViewModel> GetLocationActions(Venue venue, Location location)
{
    List<LocationActionViewModel> actions = GetDynamicLocationActions(venue.Id, location.Id);

    // NEW: Add "Complete Delivery" action if player at destination
    DeliveryJob activeJob = GetActiveDeliveryJob();
    if (activeJob != null && activeJob.DestinationLocationId == location.Id)
    {
        actions.Add(new LocationActionViewModel
        {
            Id = "complete_delivery",
            ActionType = "completedelivery",
            Title = $"Complete Delivery: {activeJob.CargoDescription}",
            Detail = $"Deliver {activeJob.CargoDescription} and receive {activeJob.Payment} coins",
            Cost = "Free!",
            IsAvailable = true,
            EngagementType = "Instant"
        });
    }

    return actions;
}

private DeliveryJob GetActiveDeliveryJob()
{
    Player player = _gameWorld.GetPlayer();
    if (!player.HasActiveDeliveryJob) return null;
    return _gameWorld.GetJobById(player.ActiveDeliveryJobId);
}
```

---

## VI. UI IMPLEMENTATION

### A. Job Board Modal Component

**Path:** `src/Pages/Components/JobBoard.razor`

```razor
@* Job Board Modal - Shows available delivery jobs *@
@if (IsVisible)
{
    <div class="modal-overlay" @onclick="CloseModal">
        <div class="modal-content job-board-modal" @onclick:stopPropagation="true">
            <div class="modal-header">
                <h2>Job Board - Available Deliveries</h2>
                <button class="close-button" @onclick="CloseModal">×</button>
            </div>

            <div class="modal-body">
                @if (AvailableJobs.Any())
                {
                    <div class="jobs-grid">
                        @foreach (var job in AvailableJobs)
                        {
                            <div class="job-card @GetDifficultyClass(job.DifficultyTier)">
                                <div class="job-header">
                                    <div class="job-title">@job.JobDescription</div>
                                    <div class="job-payment">@job.Payment coins</div>
                                </div>
                                <div class="job-details">
                                    <div class="job-route">
                                        Route: @GetLocationName(job.OriginLocationId) → @GetLocationName(job.DestinationLocationId)
                                    </div>
                                    <div class="job-difficulty">
                                        Difficulty: @job.DifficultyTier
                                    </div>
                                </div>
                                <button class="accept-job-btn"
                                        @onclick="() => OnAcceptJob.InvokeAsync(job.Id)">
                                    Accept Job
                                </button>
                            </div>
                        }
                    </div>
                }
                else
                {
                    <div class="no-jobs-message">
                        No delivery jobs available at this time. Check back later.
                    </div>
                }
            </div>
        </div>
    </div>
}
```

### B. Active Job Display (UI Header)

**Add to GameScreen.razor header:**
```razor
@if (HasActiveJob)
{
    <div class="active-job-banner">
        <span class="job-icon">📦</span>
        <span class="job-text">Active Delivery: @ActiveJobDescription</span>
        <span class="job-payment">@ActiveJobPayment coins</span>
    </div>
}
```

### C. Modal Trigger (Landing.razor already handles this)

When player clicks "View Job Board" action, GameScreen opens job board modal.

---

## VII. GAME FACADE HANDLERS

### Add to GameFacade.cs

```csharp
/// <summary>
/// Open job board modal showing available jobs at current location
/// </summary>
public async Task OpenJobBoard()
{
    Player player = _gameWorld.GetPlayer();
    Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
    TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();

    if (player.HasActiveDeliveryJob)
    {
        _messageSystem.AddSystemMessage("You already have an active delivery job", SystemMessageTypes.Warning);
        return;
    }

    List<DeliveryJob> availableJobs = _gameWorld.GetJobsAvailableAt(currentLocation.Id, currentTime);

    // Trigger modal open event (UI layer handles display)
    await OnJobBoardOpened.InvokeAsync(availableJobs);
}

/// <summary>
/// Player accepts a delivery job
/// </summary>
public void AcceptDeliveryJob(string jobId)
{
    Player player = _gameWorld.GetPlayer();

    if (player.HasActiveDeliveryJob)
    {
        _messageSystem.AddSystemMessage("You already have an active delivery job", SystemMessageTypes.Warning);
        return;
    }

    DeliveryJob job = _gameWorld.GetJobById(jobId);
    if (job == null)
        throw new InvalidOperationException($"Job not found: {jobId}");

    player.AcceptDeliveryJob(jobId);
    _messageSystem.AddSystemMessage($"Accepted delivery job: {job.JobDescription}", SystemMessageTypes.Success);
}

/// <summary>
/// Player completes active delivery job at destination
/// </summary>
public void CompleteDeliveryJob()
{
    Player player = _gameWorld.GetPlayer();

    if (!player.HasActiveDeliveryJob)
    {
        _messageSystem.AddSystemMessage("No active delivery job to complete", SystemMessageTypes.Warning);
        return;
    }

    DeliveryJob job = _gameWorld.GetJobById(player.ActiveDeliveryJobId);
    if (job == null)
        throw new InvalidOperationException($"Active job not found: {player.ActiveDeliveryJobId}");

    Location currentLocation = _gameWorld.GetPlayerCurrentLocation();
    if (currentLocation.Id != job.DestinationLocationId)
    {
        _messageSystem.AddSystemMessage("You must be at the delivery destination to complete this job", SystemMessageTypes.Warning);
        return;
    }

    // Grant rewards
    player.Coins += job.Payment;

    // Clear active job
    player.CompleteDeliveryJob();

    _messageSystem.AddSystemMessage($"Delivery completed! Earned {job.Payment} coins.", SystemMessageTypes.Success);
}
```

---

## VIII. ECONOMIC BALANCE

### Payment Formula

```
Payment = RoomCost + TravelCost + ProfitMargin

RoomCost = 10 coins (fixed)
TravelCost = RouteSegments * 2 coins (stamina/food estimate)
ProfitMargin = Tier-based bonus

Simple Tier: +3 coins
Moderate Tier: +5 coins
Dangerous Tier: +10 coins
```

### Example Jobs (Current World)

**Current Routes:**
- common_room ↔ square_center (2 segments)

**Generated Jobs:**
```
Job 1: "Deliver letter to Town Square"
- Route: common_room → square_center (2 segments)
- Payment: 10 + (2*2) + 3 = 17 coins
- Profit: 17 - 10 (room) - 4 (travel) = 3 coins
- DifficultyTier: Simple

Job 2: "Deliver bread to Brass Bell Inn"
- Route: square_center → common_room (2 segments)
- Payment: 17 coins
- Profit: 3 coins
- DifficultyTier: Simple
```

**Sustainable Loop:**
1. Morning: Accept job (17 coins promised)
2. Travel to destination (costs ~4 coins equivalent in stamina/food)
3. Complete delivery (+17 coins = 13 net after travel)
4. Evening: Secure room (-10 coins = 3 coins remaining)
5. Next morning: Repeat with 3+ coins buffer

---

## IX. TESTING CHECKLIST

### Minimum Viable Loop
- [ ] Jobs generated at parse time from routes
- [ ] Job board action appears at Commercial locations (Morning/Midday only)
- [ ] Player can open job board modal
- [ ] Player can accept a job
- [ ] Active job displayed in UI header
- [ ] "Complete Delivery" action appears at destination
- [ ] Player can complete delivery and receive payment
- [ ] Active job cleared after completion
- [ ] Player can accept new job after completing previous

### Economic Validation
- [ ] Job payment covers room cost + travel cost + profit
- [ ] Player can sustain infinite loop (work → earn → sleep → repeat)
- [ ] No soft-locks (player never trapped without coins/options)

### Edge Cases
- [ ] Cannot accept job if already has active job
- [ ] Cannot complete delivery if not at destination
- [ ] Cannot complete delivery if no active job
- [ ] Job board empty if no jobs available (graceful message)

---

## X. IMPLEMENTATION ORDER

**Phase 1: Domain Entities (Foundation)**
1. Create DeliveryJob.cs entity
2. Add DeliveryJobDTO if needed (likely not - procedural generation)
3. Add DifficultyTier enum
4. Add to GameWorld (AvailableDeliveryJobs property + helper methods)
5. Add to Player (ActiveDeliveryJobId + helper methods)

**Phase 2: Parse-Time Generation (Catalogue)**
1. Create DeliveryJobCatalog.cs
2. Implement GenerateJobsFromRoutes()
3. Implement payment calculation logic
4. Implement cargo generation logic
5. Call from PackageLoader after routes are generated

**Phase 3: Location Actions (Query Time)**
1. Add ViewJobBoard and CompleteDelivery to LocationActionType enum
2. Add job board action generation to LocationActionCatalog
3. Add completion action to LocationActionManager (conditional query-time action)
4. Update GetLocationActions() to include completion action when applicable

**Phase 4: Game Facade Handlers (Backend Logic)**
1. Add OpenJobBoard() method to GameFacade
2. Add AcceptDeliveryJob() method to GameFacade
3. Add CompleteDeliveryJob() method to GameFacade
4. Wire up to action execution flow

**Phase 5: UI Components (Frontend)**
1. Create JobBoard.razor modal component
2. Add active job display to GameScreen.razor header
3. Wire modal open/close events
4. Style job cards and modal

**Phase 6: Integration Testing**
1. Test complete loop end-to-end
2. Verify economic balance
3. Test edge cases
4. Adjust payment formula if needed

---

## XI. ARCHITECTURE COMPLIANCE

### HIGHLANDER Principle ✅
- ONE source of delivery jobs: GameWorld.AvailableDeliveryJobs
- ONE active job: Player.ActiveDeliveryJobId
- No duplicate job storage, no parallel systems

### Catalogue Pattern ✅
- Parse-time generation via DeliveryJobCatalog
- Categorical properties (DifficultyTier) translate to concrete values
- No runtime catalogue calls

### Strong Typing ✅
- List<DeliveryJob> not Dictionary<string, object>
- DifficultyTier enum not string
- No dictionaries anywhere

### Parser-JSON-Entity Triangle ✅
- Jobs generated procedurally (no JSON templates for POC)
- If JSON templates added later: JSON → DTO → Parser → Entity → GameWorld
- All three layers match

### Perfect Information ✅
- Player sees all job details before accepting (payment, destination, difficulty)
- No hidden costs or surprise outcomes
- Deterministic rewards

### Fail Fast ✅
- Throw exceptions for missing jobs/locations
- No silent defaults
- Invalid operations caught immediately

---

## XII. FUTURE ENHANCEMENTS (Post-POC)

**Not Needed for Tutorial:**
- Job deadlines (time pressure)
- Job reputation system (unlock better jobs)
- Multiple concurrent jobs (currently ONE active job only)
- Job templates in JSON (currently procedural only)
- Failed delivery consequences
- Job expiration (jobs cycle in/out of board)
- Quest-based deliveries (tied to Obligations)

**Keep It Simple For POC:**
- Procedural generation from routes
- Single active job
- No time pressure
- Economic sustainability is enough

---

**END OF DESIGN DOCUMENT**

This design is complete, architecturally sound, and ready for implementation.
