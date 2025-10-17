using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Venue screen component that displays the current location, available Locations, NPCs, and actions.
    ///
    /// VISUAL NOVEL SCENE CONTROLLER PATTERN:
    /// =======================================
    /// LocationContent manages internal sub-scene navigation with progressive disclosure.
    /// Mirrors the proven TravelContent pattern: internal view state with conditional rendering.
    ///
    /// Navigation Structure:
    /// - Landing: High-level navigation hub (3-5 choices)
    /// - LookingAround: List NPCs at spot
    /// - ApproachNPC: One NPC's details and goals
    /// - LocationChallenges: Ambient Mental/Physical goals at location
    /// - GoalDetail: Full goal info before commitment
    /// - Spots: Movement within venue
    ///
    /// CRITICAL: BLAZOR SERVERPRERENDERED CONSEQUENCES
    /// ================================================
    /// This component renders TWICE due to ServerPrerendered mode:
    /// 1. During server-side prerendering (static HTML generation)
    /// 2. After establishing interactive SignalR connection
    ///
    /// ARCHITECTURAL PRINCIPLES:
    /// - OnParametersSetAsync() runs TWICE - RefreshLocationData is read-only and safe
    /// - All data fetching operations are read-only (safe for double execution)
    /// - User interactions (clicks) only happen after interactive connection
    /// - State is maintained in GameWorld singleton (persists across renders)
    /// - Navigation state (ViewState) is NOT persisted - always resets to Landing
    ///
    /// IMPLEMENTATION REQUIREMENTS:
    /// - RefreshLocationData() fetches display data only (no mutations)
    /// - All actions go through GameScreen parent via CascadingValue pattern
    /// - Venue state read from GameWorld.GetPlayer().CurrentLocation
    /// - NPCs, actions, observations fetched fresh each render (read-only)
    /// - ViewState always starts at Landing when component initializes
    /// </summary>
    public class LocationContentBase : ComponentBase
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected DevModeService DevMode { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected ObstacleGoalFilter ObstacleGoalFilter { get; set; }
        [Inject] protected InvestigationActivity InvestigationActivity { get; set; }
        [Inject] protected DifficultyCalculationService DifficultyService { get; set; }
        [Inject] protected ItemRepository ItemRepository { get; set; }

        [Parameter] public EventCallback OnActionExecuted { get; set; }

        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        // VISUAL NOVEL NAVIGATION STATE MACHINE
        protected LocationViewState ViewState { get; set; } = LocationViewState.Landing;
        protected Stack<LocationViewState> NavigationStack { get; set; } = new();
        protected string SelectedNpcId { get; set; }
        protected Goal SelectedGoal { get; set; }

        // DATA PROPERTIES
        protected Location CurrentSpot { get; set; }
        protected List<NpcViewModel> AvailableNpcs { get; set; } = new();
        protected List<LocationObservationViewModel> AvailableObservations { get; set; } = new();
        // TakenObservations eliminated - observation system removed
        protected List<SpotViewModel> AvailableSpots { get; set; } = new();
        protected bool CanTravel { get; set; }
        protected bool CanWork { get; set; }
        protected List<LocationActionViewModel> LocationActions { get; set; } = new();
        protected List<WorkAction> AvailableWorkActions { get; set; } = new();
        protected TimeBlocks CurrentTime { get; set; }
        protected List<NPC> NPCsAtSpot { get; set; } = new();
        protected Venue CurrentLocation { get; set; }
        protected List<Goal> AvailableSocialGoals { get; set; } = new();
        protected List<Goal> AvailableMentalGoals { get; set; } = new();
        protected List<Goal> AvailablePhysicalGoals { get; set; } = new();
        protected List<Investigation> DiscoveredInvestigationsAtLocation { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("[LocationContent] OnInitializedAsync - Resetting to Landing view");
            ResetNavigation();
            await RefreshLocationData();
        }

        protected override async Task OnParametersSetAsync()
        {
            Console.WriteLine("[LocationContent] OnParametersSetAsync - Resetting to Landing view");
            ResetNavigation();
            await RefreshLocationData();
        }

        private async Task RefreshLocationData()
        {
            // Evaluate investigation discovery when Venue refreshes
            GameFacade.EvaluateInvestigationDiscovery();

            Venue venue = GameFacade.GetCurrentLocation();
            CurrentLocation = venue;
            Location? location = GameFacade.GetCurrentLocationSpot();
            CurrentSpot = location;
            TimeInfo timeInfo = GameFacade.GetTimeInfo();
            CurrentTime = timeInfo.TimeBlock;

            // Get NPCs at current location
            AvailableNpcs.Clear();
            NPCsAtSpot.Clear();
            if (CurrentSpot != null)
            {
                // Get NPCs at the current location for the current time
                List<NPC> npcsAtSpot = GameFacade.GetNPCsAtCurrentSpot();
                NPCsAtSpot = npcsAtSpot ?? new List<NPC>();
                foreach (NPC npc in NPCsAtSpot)
                {
                    // Get actual connection state
                    ConnectionState connectionState = GameFacade.GetNPCConnectionState(npc.ID);

                    // Display connection state
                    string stateDisplay = connectionState.ToString();

                    AvailableNpcs.Add(new NpcViewModel
                    {
                        Id = npc.ID,
                        Name = npc.Name,
                        PersonalityType = npc.PersonalityType.ToString(),
                        ConnectionState = stateDisplay
                    });
                }
            }

            // Get available observations
            AvailableObservations.Clear();
            // TakenObservations eliminated - observation system removed

            // Get other Locations in this Venue from GameWorld
            AvailableSpots.Clear();
            if (venue != null && GameWorld != null)
            {
                IEnumerable<Location> allSpots = GameWorld.Locations
                    .Where(s => s.VenueId == venue.Id);
                foreach (Location spot in allSpots)
                {
                    List<NPC> npcsAtSpot = GetNPCsAtSpot(spot.Name);
                    AvailableSpots.Add(new SpotViewModel
                    {
                        Id = spot.Name, // Use name as ID
                        Name = spot.Name,
                        IsCurrentSpot = (spot == location),
                        NPCs = npcsAtSpot.Select(npc => new NPCAtSpotViewModel
                        {
                            Name = npc.Name,
                            State = GetNPCStateDisplay(npc)
                        }).ToList()
                    });
                }
            }

            // Check if can travel from this location
            CanTravel = location?.LocationProperties?.Contains(LocationPropertyType.Crossroads) ?? false;
            Console.WriteLine($"[LocationContent] location: {location?.Name}, Properties: {string.Join(", ", location?.LocationProperties ?? new List<LocationPropertyType>())}, CanTravel: {CanTravel}");

            // Check if can work at this location
            CanWork = location?.LocationProperties?.Contains(LocationPropertyType.Commercial) ?? false;

            // Get dynamic Venue actions
            LocationActions.Clear();
            if (venue != null && location != null)
            {
                try
                {
                    LocationActionManager locationActionManager = GameFacade.GetLocationActionManager();
                    if (locationActionManager != null)
                    {
                        List<LocationActionViewModel> actions = locationActionManager.GetLocationActions(venue, location);
                        LocationActions = actions ?? new List<LocationActionViewModel>();
                        Console.WriteLine($"[LocationContent] Got {LocationActions.Count} Venue actions");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LocationContent] Error getting Venue actions: {ex.Message}");
                    LocationActions = new List<LocationActionViewModel>();
                }
            }

            // Get discovered investigations with intro actions at this location
            DiscoveredInvestigationsAtLocation.Clear();
            if (CurrentSpot != null && GameWorld.InvestigationJournal != null)
            {
                foreach (string investigationId in GameWorld.InvestigationJournal.DiscoveredInvestigationIds)
                {
                    Investigation investigation = GameWorld.Investigations.FirstOrDefault(i => i.Id == investigationId);
                    if (investigation != null &&
                        investigation.IntroAction != null &&
                        investigation.IntroAction.LocationId == CurrentSpot.Id)
                    {
                        DiscoveredInvestigationsAtLocation.Add(investigation);
                    }
                }
                Console.WriteLine($"[LocationContent] Got {DiscoveredInvestigationsAtLocation.Count} discovered investigations at this location");
            }

            // Get Social, Mental, and Physical investigation goals available at current location
            // THREE PARALLEL SYSTEMS: LocationGoals can spawn any tactical system type
            // Aggregates: ambient goals (location.ActiveGoals) + obstacle-specific goals (filtered by property requirements)
            AvailableSocialGoals.Clear();
            AvailableMentalGoals.Clear();
            AvailablePhysicalGoals.Clear();
            if (CurrentSpot != null)
            {
                // Use ObstacleGoalFilter to aggregate ambient + filtered obstacle goals
                List<Goal> allVisibleGoals = ObstacleGoalFilter.GetVisibleLocationGoals(CurrentSpot, GameWorld);

                AvailableSocialGoals = allVisibleGoals
                    .Where(g => g.SystemType == TacticalSystemType.Social)
                    .Where(g => g.IsAvailable && !g.IsCompleted)
                    .Where(g => string.IsNullOrEmpty(g.InvestigationId)) // Hide ALL investigation goals (they're in obstacles now)
                    .ToList();

                AvailableMentalGoals = allVisibleGoals
                    .Where(g => g.SystemType == TacticalSystemType.Mental)
                    .Where(g => g.IsAvailable && !g.IsCompleted)
                    .Where(g => string.IsNullOrEmpty(g.InvestigationId)) // Hide ALL investigation goals (they're in obstacles now)
                    .ToList();

                AvailablePhysicalGoals = allVisibleGoals
                    .Where(g => g.SystemType == TacticalSystemType.Physical)
                    .Where(g => g.IsAvailable && !g.IsCompleted)
                    .Where(g => string.IsNullOrEmpty(g.InvestigationId)) // Hide ALL investigation goals (they're in obstacles now)
                    .ToList();

                Console.WriteLine($"[LocationContent] Got {AvailableSocialGoals.Count} Social, {AvailableMentalGoals.Count} Mental, and {AvailablePhysicalGoals.Count} Physical goals available");
            }
        }

        protected async Task StartExchange(string npcId)
        {
            Console.WriteLine($"[LocationContent] Starting exchange with NPC ID: '{npcId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartExchange(npcId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for exchange with NPC '{npcId}'");
            }
        }

        protected async Task StartConversationWithRequest(string npcId, string requestId)
        {
            Console.WriteLine($"[LocationContent] Starting   conversation with NPC ID: '{npcId}', RequestId: '{requestId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartConversationSession(npcId, requestId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for conversation with NPC '{npcId}'");
            }
        }

        protected async Task StartSocialGoal(Goal goal)
        {
            Console.WriteLine($"[LocationContent] Starting Social goal: '{goal.Name}' (ID: '{goal.Id}') with NPC: '{goal.PlacementNpcId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartConversationSession(goal.PlacementNpcId, goal.Id);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for Social goal '{goal.Name}'");
            }
        }

        protected async Task StartMentalGoal(Goal goal)
        {
            Console.WriteLine($"[LocationContent] Starting Mental goal: '{goal.Name}' with deckId: '{goal.DeckId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartMentalSession(goal.DeckId, GameWorld.GetPlayer().CurrentLocation.Id, goal.Id, goal.InvestigationId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for Mental goal '{goal.Name}'");
            }
        }

        protected async Task StartPhysicalGoal(Goal goal)
        {
            Console.WriteLine($"[LocationContent] Starting Physical goal: '{goal.Name}' with deckId: '{goal.DeckId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartPhysicalSession(goal.DeckId, GameWorld.GetPlayer().CurrentLocation.Id, goal.Id, goal.InvestigationId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for Physical goal '{goal.Name}'");
            }
        }

        protected async Task StartInvestigationIntro(Investigation investigation)
        {
            Console.WriteLine($"[LocationContent] Setting pending intro for: '{investigation.Name}'");

            // Set pending intro action (doesn't activate yet - just prepares modal)
            InvestigationActivity.SetPendingIntroAction(investigation.Id);

            // Refresh UI so modal can detect pending result
            await OnActionExecuted.InvokeAsync();
        }

        protected async Task MoveToSpot(string LocationId)
        {
            Console.WriteLine($"[LocationContent] Moving to location: {LocationId}");

            // Call GameFacade to move to the location (free movement within location)
            bool success = GameFacade.MoveToSpot(LocationId);

            if (success)
            {
                Console.WriteLine($"[LocationContent] Successfully moved to location {LocationId}");
                // Reset navigation to Landing - location context has changed
                ResetNavigation();
                // Refresh the UI to show the new location
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                Console.WriteLine($"[LocationContent] Failed to move to location {LocationId}");
            }
        }

        protected async Task ExecuteLocationAction(LocationActionViewModel action)
        {
            Console.WriteLine($"[LocationContent] Executing Venue action: {action.ActionType}");

            // Special handling for travel action
            if (action.ActionType == "travel")
            {
                if (GameScreen != null)
                {
                    await GameScreen.HandleNavigation("travel");
                }
                else
                {
                    Console.WriteLine("[LocationContent] GameScreen not available for travel navigation");
                }
            }
            // Investigation action type - handled by InvestigationActivity orchestrator
            // UI integration for Investigation strategic activity not yet implemented
            else if (action.ActionType == "investigate")
            {
                await InvestigateLocation();
            }
            else
            {
                // Other action types not supported in current game design
                Console.WriteLine($"[LocationContent] Action type {action.ActionType} not supported");
            }
        }

        /// <summary>
        /// Investigate the current Venue to gain familiarity. Costs 1 attention and 1 segment.
        /// Familiarity gain depends on location properties.
        /// </summary>
        protected async Task InvestigateLocation()
        {
            if (CurrentLocation == null || CurrentSpot == null)
            {
                Console.WriteLine("[LocationContent] Cannot investigate - no current Venue or location");
                return;
            }

            Console.WriteLine($"[LocationContent] Starting investigation of {CurrentLocation.Name} at {CurrentSpot.Name}");


            // Call LocationFacade to perform the investigation
            bool success = GameFacade.GetLocationFacade().InvestigateLocation(CurrentSpot.Id);

            if (success)
            {
                Console.WriteLine($"[LocationContent] Successfully investigated {CurrentLocation.Name}");
                // Refresh the UI to show updated familiarity and resources
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                Console.WriteLine($"[LocationContent] Failed to investigate {CurrentLocation.Name}");
            }
        }

        /// <summary>
        /// Check if investigation is available at the current location.
        /// </summary>
        protected bool CanInvestigate()
        {
            if (CurrentLocation == null || CurrentSpot == null) return false;


            // Check if location is already at max familiarity
            Player player = GameWorld.GetPlayer();
            int currentFamiliarity = player.GetLocationFamiliarity(CurrentLocation.Id);
            return currentFamiliarity < CurrentSpot.MaxFamiliarity;
        }


        protected string GetStateClass(string connectionState)
        {
            return connectionState?.ToLower() switch
            {
                "disconnected" => "disconnected",
                "hostile" => "hostile",
                "crisis" => "crisis",
                _ => ""
            };
        }

        protected string GetActionClass(string conversationTypeId)
        {
            return conversationTypeId switch
            {
                "friendly_chat" => "talk",
                "request" => "request",
                "delivery" => "delivery",
                "resolution" => "resolution",
                _ => ""
            };
        }

        protected List<NPC> GetNPCsAtSpot(string LocationId)
        {
            // For the current location, use the cached NPCs
            if (CurrentSpot != null && CurrentSpot.Name == LocationId)
            {
                return NPCsAtSpot;
            }

            // For other Locations, get all NPCs at the current Venue and filter by location
            Venue currentLocation = GameFacade.GetCurrentLocation();
            if (currentLocation == null) return new List<NPC>();

            // Get the location we're checking
            Location? location = GameWorld.Locations.FirstOrDefault(s => s.VenueId == currentLocation.Id && s.Name == LocationId);
            if (location == null) return new List<NPC>();

            // Get ALL NPCs at this Venue and filter by LocationId
            List<NPC> npcsAtLocation = GameFacade.GetNPCsAtLocation(currentLocation.Id);
            return npcsAtLocation.Where(n => n.LocationId == location.Id).ToList();
        }

        protected string GetNPCName(string npcId)
        {
            NPC npc = GameFacade.GetNPCById(npcId);
            return npc?.Name ?? "";
        }

        protected string GetNPCStateDisplay(NPC npc)
        {
            // Get display text for NPC connection state
            ConnectionState connectionState = GameFacade.GetNPCConnectionState(npc.ID);
            return connectionState.ToString();
        }

        protected string GetConversationLabel(string conversationTypeId)
        {
            return conversationTypeId switch
            {
                "friendly_chat" => "Talk",
                "request" => "Request", // Actual label comes from Goal.Name
                "delivery" => "Deliver Letter",
                "resolution" => "Make Amends",
                _ => conversationTypeId
            };
        }


        protected string GetNPCDescription(NpcViewModel npc)
        {
            // Get the actual NPC object to access its description from JSON data
            NPC actualNPC = GameFacade.GetNPCById(npc.Id);
            if (actualNPC != null && !string.IsNullOrEmpty(actualNPC.Description))
            {
                return actualNPC.Description;
            }

            // Fallback to generic descriptions if no specific description is available
            string? state = npc.ConnectionState?.ToLower();
            string? personality = npc.PersonalityType?.ToLower();

            if (state == "hostile")
            {
                return "Glaring at anyone who approaches, clearly not in a talking mood.";
            }
            else if (personality == "gruff")
            {
                return "Arranging goods with practiced efficiency, occasionally grunting.";
            }
            else if (personality == "friendly")
            {
                return "Going about their business with a welcoming demeanor.";
            }
            else
            {
                return "Focused on the task at hand.";
            }
        }

        protected string GetTokenCount(string npcId, ConnectionType tokenType)
        {
            // This would get the actual token count from the ConnectionTokenManager
            // For now returning placeholder
            return "0";
        }

        protected string GetTokenEffect(string npcId, ConnectionType tokenType)
        {
            // This would calculate the effect based on token count
            // For now returning placeholder
            return "+0%";
        }

        protected string GetNPCPersonalityDescription(NpcViewModel npc)
        {
            // Format personality type for display
            if (string.IsNullOrEmpty(npc.PersonalityType))
                return "";

            return $"{npc.PersonalityType} type";
        }


        protected string GetDoubtDisplay(NpcViewModel npc, Location location)
        {
            // Patience system removed - NPCs are always available
            return "Available";
        }


        protected string GetTimeOfDayTrait()
        {
            // Show time-specific Venue traits based on current time and Venue context
            string timeStr = CurrentTime switch
            {
                TimeBlocks.Morning => "Morning",
                TimeBlocks.Midday => "Midday",
                TimeBlocks.Afternoon => "Afternoon",
                TimeBlocks.Evening => "Evening",
                _ => "Unknown"
            };

            // Add context-specific modifiers based on Venue and time
            string modifier = GetLocationTimeModifier();

            return string.IsNullOrEmpty(modifier) ? timeStr : $"{timeStr}: {modifier}";
        }

        protected string GetLocationTimeModifier()
        {
            // Generate time-specific Venue traits based on Venue type and current time
            if (CurrentLocation == null) return "";

            string locationName = CurrentLocation.Name?.ToLower() ?? "";

            // Market locations during different times
            if (locationName.Contains("market") || locationName.Contains("square"))
            {
                return CurrentTime switch
                {
                    TimeBlocks.Morning => "Opening",
                    TimeBlocks.Midday => "Busy",
                    TimeBlocks.Afternoon => "Closing",
                    TimeBlocks.Evening => "Empty",
                    _ => ""
                };
            }

            // Tavern locations
            if (locationName.Contains("tavern") || locationName.Contains("kettle"))
            {
                return CurrentTime switch
                {
                    TimeBlocks.Morning => "Quiet",
                    TimeBlocks.Midday => "Quiet",
                    TimeBlocks.Afternoon => "Busy",
                    TimeBlocks.Evening => "Lively",
                    _ => ""
                };
            }

            // Noble/Manor locations
            if (locationName.Contains("noble") || locationName.Contains("manor"))
            {
                return CurrentTime switch
                {
                    TimeBlocks.Morning => "Formal",
                    TimeBlocks.Midday => "Active",
                    TimeBlocks.Afternoon => "Reception",
                    TimeBlocks.Evening => "Private",
                    _ => ""
                };
            }

            return "";
        }

        protected string GetSpotTraitClass(LocationPropertyType prop)
        {
            // Return a CSS class based on the property type
            return prop.ToString().ToLower().Replace("_", "-");
        }

        protected string GetSpotTraitDisplay(LocationPropertyType prop)
        {
            // Display location properties with their mechanical effects
            return prop switch
            {
                LocationPropertyType.Private => "Private (+1 patience)",
                LocationPropertyType.Public => "Public (-1 patience)",
                LocationPropertyType.Discrete => "Discrete (+1 patience)",
                LocationPropertyType.Exposed => "Exposed (-1 patience)",
                LocationPropertyType.Crossroads => "Crossroads",
                LocationPropertyType.Commercial => "Commercial",
                LocationPropertyType.Quiet => "Quiet (+1 flow)",
                LocationPropertyType.Loud => "Loud (-1 flow)",
                LocationPropertyType.Warm => "Warm (+1 flow)",
                _ => prop.ToString()
            };
        }

        // Investigation Approach Helper Methods
        protected async Task InvestigateWithApproach(InvestigationApproach approach)
        {
            if (CurrentLocation == null || CurrentSpot == null)
            {
                Console.WriteLine("[LocationContent] Cannot investigate - no current Venue or location");
                return;
            }

            Console.WriteLine($"[LocationContent] Starting investigation with {approach} approach");

            bool success = GameFacade.GetLocationFacade().InvestigateLocation(CurrentSpot.Id, approach);

            if (success)
            {
                Console.WriteLine($"[LocationContent] Successfully investigated {CurrentLocation.Name} with {approach}");
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                Console.WriteLine($"[LocationContent] Failed to investigate with {approach}");
            }
        }

        protected bool CanAffordInvestigation(InvestigationApproach approach)
        {
            // Check if player has required stat levels
            Player player = GameWorld.GetPlayer();

            bool hasStatRequirement = approach switch
            {
                InvestigationApproach.Standard => true,
                InvestigationApproach.Systematic => player.Stats.GetLevel(PlayerStatType.Insight) >= 2,
                InvestigationApproach.LocalInquiry => player.Stats.GetLevel(PlayerStatType.Rapport) >= 2,
                InvestigationApproach.DemandAccess => player.Stats.GetLevel(PlayerStatType.Authority) >= 2,
                InvestigationApproach.PurchaseInfo => player.Stats.GetLevel(PlayerStatType.Diplomacy) >= 2,
                InvestigationApproach.CovertSearch => player.Stats.GetLevel(PlayerStatType.Cunning) >= 2,
                _ => false
            };

            return hasStatRequirement;
        }

        protected string GetApproachDisplayName(InvestigationApproach approach)
        {
            return approach switch
            {
                InvestigationApproach.Standard => "Standard Investigation",
                InvestigationApproach.Systematic => "Systematic Observation",
                InvestigationApproach.LocalInquiry => "Local Inquiry",
                InvestigationApproach.DemandAccess => "Demand Access",
                InvestigationApproach.PurchaseInfo => "Purchase Information",
                InvestigationApproach.CovertSearch => "Covert Search",
                _ => approach.ToString()
            };
        }

        protected string GetApproachDescription(InvestigationApproach approach)
        {
            return approach switch
            {
                InvestigationApproach.Standard => "Basic investigation methods",
                InvestigationApproach.Systematic => "+1 familiarity bonus (Insight 2+)",
                InvestigationApproach.LocalInquiry => "Learn NPC preferences (Rapport 2+)",
                InvestigationApproach.DemandAccess => "Access restricted areas (Authority 2+)",
                InvestigationApproach.PurchaseInfo => "Pay coins for information (Diplomacy 2+)",
                InvestigationApproach.CovertSearch => "Investigate without alerts (Cunning 2+)",
                _ => "Unknown approach"
            };
        }

        protected string GetApproachRequirement(InvestigationApproach approach)
        {
            return approach switch
            {
                InvestigationApproach.Standard => "",
                InvestigationApproach.Systematic => "Requires Insight Level 2",
                InvestigationApproach.LocalInquiry => "Requires Rapport Level 2",
                InvestigationApproach.DemandAccess => "Requires Authority Level 2",
                InvestigationApproach.PurchaseInfo => "Requires Diplomacy Level 2",
                InvestigationApproach.CovertSearch => "Requires Cunning Level 2",
                _ => "Unknown requirement"
            };
        }

        /// <summary>
        /// Returns list of player stats for display in conversation UI
        /// </summary>
        protected List<PlayerStatInfo> GetPlayerStats()
        {
            List<PlayerStatInfo> result = new List<PlayerStatInfo>();

            if (GameFacade == null) return result;

            try
            {
                PlayerStats stats = GameFacade.GetPlayerStats();

                // Get all five core stats
                foreach (PlayerStatType stat in Enum.GetValues<PlayerStatType>())
                {
                    result.Add(new PlayerStatInfo
                    {
                        Name = GetStatDisplayName(stat),
                        Level = stats.GetLevel(stat),
                        CurrentXP = stats.GetXP(stat),
                        RequiredXP = stats.GetXPToNextLevel(stat)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent.GetPlayerStats] Error retrieving player stats: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Get display name for a stat type
        /// </summary>
        private string GetStatDisplayName(PlayerStatType stat)
        {
            return stat switch
            {
                PlayerStatType.Insight => "Insight",
                PlayerStatType.Rapport => "Rapport",
                PlayerStatType.Authority => "Authority",
                PlayerStatType.Diplomacy => "Diplomacy",
                PlayerStatType.Cunning => "Cunning",
                _ => stat.ToString()
            };
        }

        /// <summary>
        /// Get investigation label color based on hash of investigation name.
        /// Maps to predefined palette to ensure consistent colors across UI.
        /// </summary>
        protected string GetInvestigationColor(string investigationLabel)
        {
            if (string.IsNullOrEmpty(investigationLabel)) return "#8b5a26";

            // Hash investigation name to palette index
            int hash = investigationLabel.GetHashCode();
            int paletteIndex = Math.Abs(hash) % 6;

            return paletteIndex switch
            {
                0 => "#4a7c9e", // waterwheel blue
                1 => "#7a9e5c", // missing-tools green
                2 => "#9e6a4a", // missing-merchant brown
                3 => "#8b5a7a", // purple variant
                4 => "#7a8b5a", // olive variant
                5 => "#9e7c4a", // gold variant
                _ => "#8b5a26"  // default
            };
        }

        /// <summary>
        /// Get calculated difficulty for a goal (transparent difficulty system)
        /// Shows players EXACT difficulty before starting challenge (Perfect Information principle)
        /// </summary>
        protected int GetGoalDifficulty(Goal goal)
        {
            if (goal == null) return 0;
            if (DifficultyService == null || ItemRepository == null)
                return goal.BaseDifficulty;

            // Calculate actual difficulty with all modifiers
            DifficultyResult result = DifficultyService.CalculateDifficulty(goal, ItemRepository);
            return result.FinalDifficulty;
        }

        // ============================================
        // VISUAL NOVEL NAVIGATION METHODS
        // ============================================
        // These methods manage internal sub-scene navigation within LocationContent,
        // following the proven TravelContent pattern.

        /// <summary>
        /// Navigate to a new view within LocationContent.
        /// Pushes current view to history stack for back button support.
        /// </summary>
        protected void NavigateToView(LocationViewState newView, object context = null)
        {
            Console.WriteLine($"[LocationContent] Navigating from {ViewState} to {newView}");

            // Push current view to history for back button
            NavigationStack.Push(ViewState);

            // Set new view state
            ViewState = newView;

            // Handle context based on view type
            if (newView == LocationViewState.ApproachNPC && context is string npcId)
            {
                SelectedNpcId = npcId;
                Console.WriteLine($"[LocationContent] Selected NPC: {npcId}");
            }
            else if (newView == LocationViewState.GoalDetail && context is Goal goal)
            {
                SelectedGoal = goal;
                Console.WriteLine($"[LocationContent] Selected Goal: {goal.Name}");
            }

            StateHasChanged();
        }

        /// <summary>
        /// Navigate back to previous view using history stack.
        /// </summary>
        protected void NavigateBack()
        {
            if (NavigationStack.Count > 0)
            {
                LocationViewState previousView = NavigationStack.Pop();
                Console.WriteLine($"[LocationContent] Navigating back from {ViewState} to {previousView}");
                ViewState = previousView;

                // Clear context when leaving specific views
                if (ViewState != LocationViewState.ApproachNPC)
                {
                    SelectedNpcId = null;
                }
                if (ViewState != LocationViewState.GoalDetail)
                {
                    SelectedGoal = null;
                }

                StateHasChanged();
            }
            else
            {
                // No history - reset to Landing
                Console.WriteLine($"[LocationContent] No navigation history, resetting to Landing");
                ResetNavigation();
            }
        }

        /// <summary>
        /// Reset navigation state to Landing view.
        /// Called when entering LocationContent or when spot changes.
        /// </summary>
        protected void ResetNavigation()
        {
            Console.WriteLine($"[LocationContent] Resetting navigation to Landing");
            ViewState = LocationViewState.Landing;
            NavigationStack.Clear();
            SelectedNpcId = null;
            SelectedGoal = null;
            StateHasChanged();
        }

        // ============================================
        // VIEWMODEL PREPARATION METHODS
        // ============================================

        protected List<LocationActionViewModel> GetTravelActions()
        {
            return LocationActions.Where(a => a.ActionType == "travel").ToList();
        }

        protected NPCDetailViewModel GetSelectedNPCViewModel()
        {
            if (string.IsNullOrEmpty(SelectedNpcId)) return null;

            NPC npc = GameFacade.GetNPCById(SelectedNpcId);
            if (npc == null) return null;

            return new NPCDetailViewModel
            {
                Id = npc.ID,
                Name = npc.Name,
                PersonalityDescription = npc.PersonalityType.ToString(),
                ConnectionState = GameFacade.GetNPCConnectionState(npc.ID).ToString(),
                StateClass = GetStateClass(GameFacade.GetNPCConnectionState(npc.ID).ToString()),
                Description = npc.Description ?? "",
                Tokens = new List<TokenViewModel>()
            };
        }

        protected List<GoalViewModel> GetSocialGoalsForNPC()
        {
            if (string.IsNullOrEmpty(SelectedNpcId)) return new List<GoalViewModel>();

            return AvailableSocialGoals
                .Where(g => g.PlacementNpcId == SelectedNpcId)
                .Select(g => new GoalViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Difficulty = GetGoalDifficulty(g).ToString(),
                    IsIntroAction = !string.IsNullOrEmpty(g.InvestigationId),
                    InvestigationId = g.InvestigationId
                }).ToList();
        }

        protected List<ActiveGoalViewModel> GetNPCActiveGoals()
        {
            // TODO: Implement NPC active goals (requests, etc.)
            return new List<ActiveGoalViewModel>();
        }

        protected bool CheckHasExchangeCards()
        {
            // TODO: Implement exchange card check
            return false;
        }

        protected string GetDoubtDisplayForSelectedNPC()
        {
            // TODO: Implement doubt display calculation
            return "Available";
        }

        protected List<GoalViewModel> GetMentalGoalsViewModel()
        {
            return AvailableMentalGoals.Select(g => new GoalViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Difficulty = GetGoalDifficulty(g).ToString(),
                IsIntroAction = !string.IsNullOrEmpty(g.InvestigationId),
                InvestigationId = g.InvestigationId
            }).ToList();
        }

        protected List<GoalViewModel> GetPhysicalGoalsViewModel()
        {
            return AvailablePhysicalGoals.Select(g => new GoalViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Difficulty = GetGoalDifficulty(g).ToString(),
                IsIntroAction = !string.IsNullOrEmpty(g.InvestigationId),
                InvestigationId = g.InvestigationId
            }).ToList();
        }

        protected GoalDetailViewModel GetGoalDetailViewModel()
        {
            if (SelectedGoal == null) return null;

            return new GoalDetailViewModel
            {
                Goal = SelectedGoal,
                Name = SelectedGoal.Name,
                Description = SelectedGoal.Description,
                SystemType = SelectedGoal.SystemType,
                SystemTypeLowercase = SelectedGoal.SystemType.ToString().ToLower(),
                Difficulty = GetGoalDifficulty(SelectedGoal).ToString(),
                HasCosts = SelectedGoal.Costs.Focus > 0 || SelectedGoal.Costs.Stamina > 0,
                FocusCost = SelectedGoal.Costs.Focus,
                StaminaCost = SelectedGoal.Costs.Stamina
            };
        }

        protected List<SpotViewModel> GetSpotsViewModel()
        {
            return AvailableSpots;
        }

        // ============================================
        // EVENT HANDLER METHODS
        // ============================================

        protected void HandleNavigateToView(LocationViewState newView)
        {
            NavigateToView(newView);
        }

        protected async Task HandleExecuteLocationAction(LocationActionViewModel action)
        {
            await ExecuteLocationAction(action);
        }

        protected void HandleNavigateToNPC(string npcId)
        {
            NavigateToView(LocationViewState.ApproachNPC, npcId);
        }

        protected void HandleNavigateToGoal(string goalId)
        {
            if (GameWorld.Goals.TryGetValue(goalId, out Goal goal))
            {
                NavigateToView(LocationViewState.GoalDetail, goal);
            }
        }

        protected async Task HandleStartConversationWithRequest((string npcId, string goalId) request)
        {
            await StartConversationWithRequest(request.npcId, request.goalId);
        }

        protected async Task HandleStartExchange(string npcId)
        {
            await StartExchange(npcId);
        }

        protected async Task HandleCommitToGoal(Goal goal)
        {
            if (goal.SystemType == TacticalSystemType.Social)
            {
                await StartSocialGoal(goal);
            }
            else if (goal.SystemType == TacticalSystemType.Mental)
            {
                await StartMentalGoal(goal);
            }
            else if (goal.SystemType == TacticalSystemType.Physical)
            {
                await StartPhysicalGoal(goal);
            }
        }

        protected async Task HandleMoveToSpot(string spotId)
        {
            await MoveToSpot(spotId);
        }

    }

    // ============================================
    // LOCATION VIEW STATE ENUM
    // ============================================

    /// <summary>
    /// Defines the internal view states for visual novel scene navigation within LocationContent.
    /// Mirrors TravelContent pattern: scene controller with conditional view rendering.
    /// </summary>
    public enum LocationViewState
    {
        /// <summary>
        /// Landing view: High-level navigation hub (3-5 choices)
        /// Player chooses: Look Around, Examine Location, Move to Spot, or Travel
        /// </summary>
        Landing,

        /// <summary>
        /// Looking Around view: List of NPCs at current spot
        /// Shows who's present, click NPC to see their details
        /// </summary>
        LookingAround,

        /// <summary>
        /// Approach NPC view: One NPC's full details and available goals
        /// Shows NPC info, tokens, description, and their goals
        /// Requires SelectedNpcId context
        /// </summary>
        ApproachNPC,

        /// <summary>
        /// Location Challenges view: Ambient Mental/Physical goals at location
        /// Shows Mental and Physical goals available at current spot
        /// Investigation goals appear here as regular goals with InvestigationId property
        /// </summary>
        LocationChallenges,

        /// <summary>
        /// Goal Detail view: Full goal information before commitment
        /// Shows goal description, difficulty, costs, rewards
        /// Requires SelectedGoal context
        /// Player decides: BEGIN CHALLENGE or Cancel
        /// </summary>
        GoalDetail,

        /// <summary>
        /// Spots view: Other spots in current venue for movement
        /// Shows all spots, who's at each spot, current spot indicator
        /// Click to move (instant, free)
        /// </summary>
        Spots
    }

    // ============================================
    // View Models
    // ============================================

    /// <summary>
    /// Helper class for player stat display information
    /// </summary>
    public class PlayerStatInfo
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int CurrentXP { get; set; }
        public int RequiredXP { get; set; }
    }

    public class NpcViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PersonalityType { get; set; }
        public string ConnectionState { get; set; }
    }

    public class LocationObservationViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class SpotViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsCurrentSpot { get; set; }
        public List<NPCAtSpotViewModel> NPCs { get; set; } = new();
    }

    public class NPCAtSpotViewModel
    {
        public string Name { get; set; }
        public string State { get; set; }
    }
}
