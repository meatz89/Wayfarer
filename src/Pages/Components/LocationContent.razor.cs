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
    /// 
    /// IMPLEMENTATION REQUIREMENTS:
    /// - RefreshLocationData() fetches display data only (no mutations)
    /// - All actions go through GameScreen parent via CascadingValue pattern
    /// - Venue state read from GameWorld.GetPlayer().CurrentLocation
    /// - NPCs, actions, observations fetched fresh each render (read-only)
    /// </summary>
    public class LocationContentBase : ComponentBase
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected DevModeService DevMode { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected ObstacleGoalFilter ObstacleGoalFilter { get; set; }

        [Parameter] public EventCallback OnActionExecuted { get; set; }

        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        protected Location CurrentSpot { get; set; }
        protected List<NpcViewModel> AvailableNpcs { get; set; } = new();
        protected List<LocationObservationViewModel> AvailableObservations { get; set; } = new();
        protected List<TakenObservation> TakenObservations { get; set; } = new();
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
        protected List<Obstacle> CurrentObstacles { get; set; } = new();
        protected List<Investigation> DiscoveredInvestigationsAtLocation { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await RefreshLocationData();
        }

        protected override async Task OnParametersSetAsync()
        {
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

            // Get obstacles at current location
            CurrentObstacles.Clear();
            if (CurrentSpot != null && CurrentSpot.ObstacleIds != null)
            {
                // Lookup obstacles by ID from GameWorld
                foreach (string obstacleId in CurrentSpot.ObstacleIds)
                {
                    Obstacle obstacle = GameWorld.Obstacles.FirstOrDefault(o => o.Id == obstacleId);
                    if (obstacle != null)
                    {
                        CurrentObstacles.Add(obstacle);
                    }
                }
                Console.WriteLine($"[LocationContent] Got {CurrentObstacles.Count} obstacles at {CurrentSpot.Name}");
            }

            // Get available observations
            AvailableObservations.Clear();
            TakenObservations.Clear();

            // Get taken observations
            List<TakenObservation>? takenObservations = GameFacade.GetTakenObservations();
            Console.WriteLine($"[LocationContent] Got {takenObservations?.Count ?? 0} taken observations");

            if (takenObservations != null)
            {
                TakenObservations = takenObservations;
            }

            // Get other Locations in this Venue from GameWorld
            AvailableSpots.Clear();
            if (venue != null && GameWorld != null)
            {
                IEnumerable<Location> allSpots = GameWorld.Locations.GetAllSpots()
                    .Where(s => s.VenueId == venue.Id);
                AvailableSpots = allSpots
                    .Where(s => s != location)
                    .Select(s => new SpotViewModel
                    {
                        Id = s.Name, // Use name as ID
                        Name = s.Name,
                        Properties = s.Properties ?? new List<string>()
                    }).ToList();
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
            Console.WriteLine($"[LocationContent] Starting investigation intro: '{investigation.Name}'");

            // Call GameFacade to complete intro action (activates investigation + spawns Phase 1)
            GameFacade.CompleteInvestigationIntro(investigation.Id);

            // Refresh UI to show Phase 1 obstacles
            await RefreshLocationData();
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
            bool success = GameFacade.GetLocationFacade().InvestigateLocation(CurrentLocation.Id, CurrentSpot.Id);

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
            Location? location = GameWorld.Locations.GetAllSpots().FirstOrDefault(s => s.VenueId == currentLocation.Id && s.Name == LocationId);
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
                TimeBlocks.Dawn => "Dawn",
                TimeBlocks.Morning => "Midday",
                TimeBlocks.Midday => "Afternoon",
                TimeBlocks.Afternoon => "Evening",
                TimeBlocks.Evening => "Night",
                TimeBlocks.Night => "Night",
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

            bool success = GameFacade.GetLocationFacade().InvestigateLocation(CurrentLocation.Id, CurrentSpot.Id, approach);

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

    }

    // View Models

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
        public List<string> Properties { get; set; } = new();
    }
}
