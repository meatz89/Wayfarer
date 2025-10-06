using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Location screen component that displays the current location, available spots, NPCs, and actions.
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
    /// - Location state read from GameWorld.GetPlayer().CurrentLocationSpot
    /// - NPCs, actions, observations fetched fresh each render (read-only)
    /// </summary>
    public class LocationContentBase : ComponentBase
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected DevModeService DevMode { get; set; }
        [Inject] protected GameWorld GameWorld { get; set; }

        [Parameter] public EventCallback OnActionExecuted { get; set; }

        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        protected LocationSpot CurrentSpot { get; set; }
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
        protected IEnumerable<DeliveryObligation> ActiveObligations { get; set; } = new List<DeliveryObligation>();
        protected Location CurrentLocation { get; set; }
        protected List<LocationGoal> AvailableSocialGoals { get; set; } = new();
        protected List<LocationGoal> AvailableMentalGoals { get; set; } = new();
        protected List<LocationGoal> AvailablePhysicalGoals { get; set; } = new();

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
            // Evaluate investigation discovery when location refreshes
            GameFacade.EvaluateInvestigationDiscovery();

            Location location = GameFacade.GetCurrentLocation();
            CurrentLocation = location;
            LocationSpot? spot = GameFacade.GetCurrentLocationSpot();
            CurrentSpot = spot;
            TimeInfo timeInfo = GameFacade.GetTimeInfo();
            CurrentTime = timeInfo.TimeBlock;

            // Get NPCs at current spot
            AvailableNpcs.Clear();
            NPCsAtSpot.Clear();
            if (CurrentSpot != null)
            {
                // Get NPCs at the current spot for the current time
                List<NPC> npcsAtSpot = GameFacade.GetNPCsAtCurrentSpot();
                NPCsAtSpot = npcsAtSpot ?? new List<NPC>();
                foreach (NPC npc in NPCsAtSpot)
                {
                    List<ConversationOptionViewModel> options = new List<ConversationOptionViewModel>();

                    // Get ACTUAL available conversation options with specific goal cards
                    List<ConversationOption> availableOptions = GameFacade.GetAvailableConversationOptions(npc.ID);

                    // Add each available option with its specific goal card
                    foreach (ConversationOption conversationOption in availableOptions)
                    {

                        ConversationOptionViewModel option = new ConversationOptionViewModel
                        {
                            RequestId = conversationOption.RequestId,
                            ConversationTypeId = conversationOption.ChallengeTypeId,
                            GoalCardId = conversationOption.GoalCardId,
                            Label = conversationOption.DisplayName ?? GetConversationLabel(conversationOption.ChallengeTypeId),
                            Description = conversationOption.Description,
                            IsAvailable = true,
                            EngagementType = "Conversation",
                            InvestigationLabel = null // Investigation system not yet integrated
                        };
                        options.Add(option);
                    }

                    // Check if NPC has exchange cards (separate from conversations!)
                    if (npc.HasExchangeCards())
                    {
                        // Add exchange as a special option (not a conversation type)
                        ConversationOptionViewModel exchangeOption = new ConversationOptionViewModel
                        {
                            ConversationTypeId = null, // No conversation type - this is an exchange!
                            Label = "Quick Exchange",
                            IsAvailable = true,
                            IsExchange = true // Mark this as an exchange
                        };
                        options.Add(exchangeOption);
                    }


                    // Get actual connection state using the same logic as conversations
                    ConnectionState connectionState = GameFacade.GetNPCConnectionState(npc.ID);

                    // Display connection state (Flow system removed in 4-resource system)
                    string stateDisplay = connectionState.ToString();

                    AvailableNpcs.Add(new NpcViewModel
                    {
                        Id = npc.ID,
                        Name = npc.Name,
                        PersonalityType = npc.PersonalityType.ToString(),
                        ConnectionState = stateDisplay,
                        ConversationOptions = options
                    });
                }
            }

            // Get available observations
            AvailableObservations.Clear();
            TakenObservations.Clear();
            Console.WriteLine("[LocationContent] Getting observations from GameFacade...");

            // Get taken observations
            List<TakenObservation>? takenObservations = GameFacade.GetTakenObservations();
            Console.WriteLine($"[LocationContent] Got {takenObservations?.Count ?? 0} taken observations");

            // Get available observation rewards and convert to view models
            List<ObservationReward> availableRewards = GameFacade.GetAvailableObservationRewards();
            Console.WriteLine($"[LocationContent] Got {availableRewards?.Count ?? 0} available observation rewards");

            if (availableRewards != null && availableRewards.Any())
            {
                AvailableObservations = availableRewards
                    .Select(reward => new LocationObservationViewModel
                    {
                        Id = reward.ObservationCard.Id,
                        Name = reward.ObservationCard.Name,
                        Type = reward.ObservationCard.Effect ?? "General"
                    }).ToList();
                Console.WriteLine($"[LocationContent] Final AvailableObservations count: {AvailableObservations.Count}");
            }
            else
            {
                Console.WriteLine("[LocationContent] No observation rewards available");
            }

            if (takenObservations != null)
            {
                TakenObservations = takenObservations;
            }

            // Get other spots in this location from GameWorld
            AvailableSpots.Clear();
            if (location != null && GameWorld != null)
            {
                IEnumerable<LocationSpot> allSpots = GameWorld.Spots.GetAllSpots()
                    .Where(s => s.LocationId == location.Id);
                AvailableSpots = allSpots
                    .Where(s => s != spot)
                    .Select(s => new SpotViewModel
                    {
                        Id = s.Name, // Use name as ID
                        Name = s.Name,
                        Properties = s.Properties ?? new List<string>()
                    }).ToList();
            }

            // Check if can travel from this spot
            CanTravel = spot?.SpotProperties?.Contains(SpotPropertyType.Crossroads) ?? false;
            Console.WriteLine($"[LocationContent] Spot: {spot?.Name}, Properties: {string.Join(", ", spot?.SpotProperties ?? new List<SpotPropertyType>())}, CanTravel: {CanTravel}");

            // Check if can work at this spot
            CanWork = spot?.SpotProperties?.Contains(SpotPropertyType.Commercial) ?? false;

            // Get dynamic location actions
            LocationActions.Clear();
            if (location != null && spot != null)
            {
                try
                {
                    LocationActionManager locationActionManager = GameFacade.GetLocationActionManager();
                    if (locationActionManager != null)
                    {
                        List<LocationActionViewModel> actions = locationActionManager.GetLocationActions(location, spot);
                        LocationActions = actions ?? new List<LocationActionViewModel>();
                        Console.WriteLine($"[LocationContent] Got {LocationActions.Count} location actions");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LocationContent] Error getting location actions: {ex.Message}");
                    LocationActions = new List<LocationActionViewModel>();
                }
            }

            // Get active obligations from the queue manager
            ObligationFacade queueManager = GameFacade.GetObligationQueueManager();
            if (queueManager != null)
            {
                DeliveryObligation[] obligations = queueManager.GetActiveObligations();
                // Take only the first 3 for the preview panel
                ActiveObligations = obligations?.Take(3) ?? new List<DeliveryObligation>();
                Console.WriteLine($"[LocationContent] Got {ActiveObligations.Count()} obligations for display");
            }
            else
            {
                ActiveObligations = new List<DeliveryObligation>();
            }

            // Get available work actions at this location
            AvailableWorkActions.Clear();
            if (location != null)
            {
                LocationFacade locationFacade = GameFacade.GetLocationFacade();
                if (locationFacade != null)
                {
                    List<WorkAction> workActions = locationFacade.GetAvailableWork(location.Id);
                    AvailableWorkActions = workActions ?? new List<WorkAction>();
                    Console.WriteLine($"[LocationContent] Got {AvailableWorkActions.Count} work actions available");
                }
            }

            // Get Social, Mental, and Physical investigation goals available at current spot
            // THREE PARALLEL SYSTEMS: LocationGoals can spawn any tactical system type
            AvailableSocialGoals.Clear();
            AvailableMentalGoals.Clear();
            AvailablePhysicalGoals.Clear();
            if (location != null && CurrentSpot != null && location.Goals != null)
            {
                string currentSpotId = CurrentSpot.SpotID;

                AvailableSocialGoals = location.Goals
                    .Where(g => g.SystemType == TacticalSystemType.Social)
                    .Where(g => g.IsAvailable && !g.IsCompleted)
                    .Where(g => string.IsNullOrEmpty(g.SpotId) || g.SpotId == currentSpotId)
                    .Where(g => string.IsNullOrEmpty(g.InvestigationId) || g.IsIntroAction) // Show intro actions, hide regular investigation goals
                    .ToList();

                AvailableMentalGoals = location.Goals
                    .Where(g => g.SystemType == TacticalSystemType.Mental)
                    .Where(g => g.IsAvailable && !g.IsCompleted)
                    .Where(g => string.IsNullOrEmpty(g.SpotId) || g.SpotId == currentSpotId)
                    .Where(g => string.IsNullOrEmpty(g.InvestigationId) || g.IsIntroAction) // Show intro actions, hide regular investigation goals
                    .ToList();

                AvailablePhysicalGoals = location.Goals
                    .Where(g => g.SystemType == TacticalSystemType.Physical)
                    .Where(g => g.IsAvailable && !g.IsCompleted)
                    .Where(g => string.IsNullOrEmpty(g.SpotId) || g.SpotId == currentSpotId)
                    .Where(g => string.IsNullOrEmpty(g.InvestigationId) || g.IsIntroAction) // Show intro actions, hide regular investigation goals
                    .ToList();

                Console.WriteLine($"[LocationContent] Got {AvailableSocialGoals.Count} Social, {AvailableMentalGoals.Count} Mental, and {AvailablePhysicalGoals.Count} Physical goals available");
            }
        }


        protected async Task StartConversationWithRequest(string npcId, string requestId)
        {
            Console.WriteLine($"[LocationContent] Starting conversation with NPC ID: '{npcId}', RequestId: '{requestId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartConversation(npcId, requestId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for conversation with NPC '{npcId}'");
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

        protected async Task StartSocialGoal(LocationGoal goal)
        {
            Console.WriteLine($"[LocationContent] Starting Social goal: '{goal.Name}' with NPC: '{goal.NpcId}', Request: '{goal.RequestId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartConversation(goal.NpcId, goal.RequestId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for Social goal '{goal.Name}'");
            }
        }

        protected async Task StartMentalGoal(LocationGoal goal)
        {
            Console.WriteLine($"[LocationContent] Starting Mental goal: '{goal.Name}' with engagementTypeId: '{goal.ChallengeTypeId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartMentalSession(goal.ChallengeTypeId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for Mental goal '{goal.Name}'");
            }
        }

        protected async Task StartPhysicalGoal(LocationGoal goal)
        {
            Console.WriteLine($"[LocationContent] Starting Physical goal: '{goal.Name}' with engagementTypeId: '{goal.ChallengeTypeId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartPhysicalSession(goal.ChallengeTypeId);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for Physical goal '{goal.Name}'");
            }
        }

        protected async Task ViewNPCDeck(string npcId)
        {
            Console.WriteLine($"[LocationContent] Viewing deck for NPC: {npcId}");
            if (GameScreen != null)
            {
                await GameScreen.NavigateToDeckViewer(npcId);
            }
        }

        protected async Task TakeObservation(string observationId)
        {
            Console.WriteLine($"[LocationContent] Taking observation: {observationId}");

            // Observations are free (0 attention cost), no need to check attention

            // Call GameFacade to take the observation
            // This will handle card generation and adding to hand
            bool success = await GameFacade.TakeObservationAsync(observationId);

            if (success)
            {
                Console.WriteLine($"[LocationContent] Successfully took observation {observationId}");
                // Refresh the UI to show the observation is now taken
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                Console.WriteLine($"[LocationContent] Failed to take observation {observationId}");
            }
        }

        protected async Task MoveToSpot(string spotId)
        {
            Console.WriteLine($"[LocationContent] Moving to spot: {spotId}");

            // Call GameFacade to move to the spot (free movement within location)
            bool success = GameFacade.MoveToSpot(spotId);

            if (success)
            {
                Console.WriteLine($"[LocationContent] Successfully moved to spot {spotId}");
                // Refresh the UI to show the new spot
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                Console.WriteLine($"[LocationContent] Failed to move to spot {spotId}");
            }
        }

        protected async Task ExecuteLocationAction(LocationActionViewModel action)
        {
            Console.WriteLine($"[LocationContent] Executing location action: {action.ActionType}");

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

        protected async Task PerformWork()
        {
            Console.WriteLine("[LocationContent] Performing work action");

            WorkResult result = await GameFacade.PerformWork();

            if (result.Success)
            {
                Console.WriteLine($"[LocationContent] Work successful: {result.Message}");
                // Refresh UI to show updated coins and attention
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                // Error is already logged via MessageSystem
                Console.WriteLine($"[LocationContent] Work failed: {result.Message}");
            }
        }

        protected async Task PerformLocationAction(LocationActionViewModel action)
        {
            Console.WriteLine($"[LocationContent] Performing location action: {action.ActionType}");

            try
            {
                // Location actions are executed through specific handlers based on type
                Console.WriteLine($"[LocationContent] Executing action: {action.Title}");
                Console.WriteLine($"[LocationContent] Action cost: {action.Cost}");
                Console.WriteLine($"[LocationContent] Action detail: {action.Detail}");

                // Refresh the UI
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocationContent] Error performing location action {action.ActionType}: {ex.Message}");
            }
        }

        /// <summary>
        /// Investigate the current location to gain familiarity. Costs 1 attention and 1 segment.
        /// Familiarity gain depends on spot properties.
        /// </summary>
        protected async Task InvestigateLocation()
        {
            if (CurrentLocation == null || CurrentSpot == null)
            {
                Console.WriteLine("[LocationContent] Cannot investigate - no current location or spot");
                return;
            }

            Console.WriteLine($"[LocationContent] Starting investigation of {CurrentLocation.Name} at {CurrentSpot.Name}");


            // Call LocationFacade to perform the investigation
            bool success = GameFacade.GetLocationFacade().InvestigateLocation(CurrentLocation.Id, CurrentSpot.SpotID);

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
            return currentFamiliarity < CurrentLocation.MaxFamiliarity;
        }

        /// <summary>
        /// Get display text for current location familiarity.
        /// </summary>
        protected string GetFamiliarityDisplay()
        {
            if (CurrentLocation == null) return "";

            Player player = GameWorld.GetPlayer();
            int currentFamiliarity = player.GetLocationFamiliarity(CurrentLocation.Id);
            return $"{currentFamiliarity}/{CurrentLocation.MaxFamiliarity}";
        }

        /// <summary>
        /// Get expected familiarity gain for investigation at current spot.
        /// </summary>
        protected int GetExpectedFamiliarityGain()
        {
            if (CurrentSpot == null) return 1;

            List<SpotPropertyType> activeProperties = CurrentSpot.GetActiveProperties(CurrentTime);

            // Quiet spots give +2, Busy spots give +1, others give +1
            if (activeProperties.Contains(SpotPropertyType.Quiet))
            {
                return 2;
            }
            return 1;
        }

        /// <summary>
        /// Perform a work action, consuming entire time block and generating coins.
        /// </summary>
        protected async Task PerformWork(WorkAction work)
        {
            if (work == null || CurrentLocation == null)
            {
                Console.WriteLine("[LocationContent] Cannot perform work - invalid work action or location");
                return;
            }

            Console.WriteLine($"[LocationContent] Starting work: {work.Name} at {CurrentLocation.Name}");

            // Call LocationFacade to perform the work
            LocationFacade locationFacade = GameFacade.GetLocationFacade();
            bool success = locationFacade.PerformWork(work.Id, CurrentLocation.Id);

            if (success)
            {
                Console.WriteLine($"[LocationContent] Successfully completed work: {work.Name}");
                // Refresh the UI to show updated resources and time
                await RefreshLocationData();
                await OnActionExecuted.InvokeAsync();
            }
            else
            {
                Console.WriteLine($"[LocationContent] Failed to perform work: {work.Name}");
            }
        }

        /// <summary>
        /// Calculate work output preview showing hunger impact.
        /// </summary>
        protected (int baseCoins, int hungerPenalty, int actualCoins) GetWorkOutputPreview(WorkAction work)
        {
            if (work == null) return (0, 0, 0);

            LocationFacade locationFacade = GameFacade.GetLocationFacade();
            return locationFacade.CalculateWorkOutput(work);
        }

        /// <summary>
        /// Get display text for work type.
        /// </summary>
        protected string GetWorkTypeDisplay(WorkType type)
        {
            return type switch
            {
                WorkType.Service => "Service",
                WorkType.Enhanced => "Enhanced",
                _ => "Standard"
            };
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

        protected List<NPC> GetNPCsAtSpot(string spotId)
        {
            // For the current spot, use the cached NPCs
            if (CurrentSpot != null && CurrentSpot.Name == spotId)
            {
                return NPCsAtSpot;
            }

            // For other spots, get all NPCs at the current location and filter by spot
            Location currentLocation = GameFacade.GetCurrentLocation();
            if (currentLocation == null) return new List<NPC>();

            // Get the spot we're checking
            LocationSpot? spot = GameWorld.Spots.GetAllSpots().FirstOrDefault(s => s.LocationId == currentLocation.Id && s.Name == spotId);
            if (spot == null) return new List<NPC>();

            // Get ALL NPCs at this location and filter by SpotId
            List<NPC> npcsAtLocation = GameFacade.GetNPCsAtLocation(currentLocation.Id);
            return npcsAtLocation.Where(n => n.SpotId == spot.SpotID).ToList();
        }

        protected bool HasUrgentLetter(string npcId)
        {
            // Check if NPC has urgent letter in queue position 1
            ObligationFacade queueManager = GameFacade.GetObligationQueueManager();
            if (queueManager == null) return false;

            DeliveryObligation[] obligations = queueManager.GetActiveObligations();
            if (obligations == null || !obligations.Any()) return false;

            // Check if position 1 has this NPC's letter
            DeliveryObligation? firstObligation = obligations.FirstOrDefault();
            return firstObligation != null &&
                   (firstObligation.SenderId == npcId || firstObligation.SenderName == GetNPCName(npcId)) &&
                   firstObligation.DeadlineInSegments < 360; // Urgent if less than 360 segments
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
                "request" => "Request", // Actual label comes from NPCRequest.Name
                "delivery" => "Deliver Letter",
                "resolution" => "Make Amends",
                _ => conversationTypeId
            };
        }

        protected string GetDeadlineClass(int deadlineInSegments)
        {
            if (deadlineInSegments <= 0)
                return "expired";
            else if (deadlineInSegments <= 4) // 4 segments
                return "critical";
            else if (deadlineInSegments <= 12) // 12 segments
                return "urgent";
            else
                return "normal";
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

        protected string GetDeadlineText(int deadlineInSegments)
        {
            if (deadlineInSegments <= 0)
                return "EXPIRED!";

            return $"{deadlineInSegments} seg";
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

        protected bool HasLetterRequest(string npcId)
        {
            // Check if this NPC has an active letter request
            // This would check against the game state
            return false;
        }

        protected int GetBurdenCount(string npcId)
        {
            // Get burden count for this NPC
            // This would check against the game state
            return 0;
        }

        protected string GetDoubtDisplay(NpcViewModel npc, LocationSpot spot)
        {
            // Patience system removed - NPCs are always available
            return "Available";
        }

        protected int GetPatience(NpcViewModel npc, LocationSpot spot)
        {
            // Patience system removed - return default value
            return 100;
        }

        protected string GetTimeOfDayTrait()
        {
            // Show time-specific location traits based on current time and location context
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

            // Add context-specific modifiers based on location and time
            string modifier = GetLocationTimeModifier();

            return string.IsNullOrEmpty(modifier) ? timeStr : $"{timeStr}: {modifier}";
        }

        protected string GetLocationTimeModifier()
        {
            // Generate time-specific location traits based on location type and current time
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

        protected string GetObservationReward(LocationObservationViewModel obs)
        {
            // Generate state transition display based on observation type and name
            try
            {
                // For now, use type-based logic and name analysis to generate realistic transitions
                // This matches the mockup which shows context-appropriate state transitions

                string name = obs.Name?.ToLower() ?? "";
                string type = obs.Type?.ToLower() ?? "";

                // Analyze observation name for context clues
                if (name.Contains("checkpoint") || name.Contains("guard"))
                {
                    return "Any→Guarded";
                }
                else if (name.Contains("carriage") || name.Contains("preparation"))
                {
                    return "Guarded→Eager";
                }
                else if (name.Contains("family") || name.Contains("letter"))
                {
                    return "Neutral→Open";
                }
                else if (name.Contains("trembling") || name.Contains("desperat"))
                {
                    return "Disconnected→Open";
                }

                // Fall back to type-based display
                return type switch
                {
                    "authority" => "Any→Guarded",
                    "diplomacy" => "Guarded→Eager",
                    "social" => "Neutral→Open",
                    "secret" => "Any→Shadow",
                    _ => "Any→Neutral"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocationContent] Error getting observation reward for {obs.Id}: {ex.Message}");
                return "Any→Neutral";
            }
        }

        protected string GetSpotTraitClass(SpotPropertyType prop)
        {
            // Return a CSS class based on the property type
            return prop.ToString().ToLower().Replace("_", "-");
        }

        protected string GetSpotTraitDisplay(SpotPropertyType prop)
        {
            // Display spot properties with their mechanical effects
            return prop switch
            {
                SpotPropertyType.Private => "Private (+1 patience)",
                SpotPropertyType.Public => "Public (-1 patience)",
                SpotPropertyType.Discrete => "Discrete (+1 patience)",
                SpotPropertyType.Exposed => "Exposed (-1 patience)",
                SpotPropertyType.Crossroads => "Crossroads",
                SpotPropertyType.Commercial => "Commercial",
                SpotPropertyType.Quiet => "Quiet (+1 flow)",
                SpotPropertyType.Loud => "Loud (-1 flow)",
                SpotPropertyType.Warm => "Warm (+1 flow)",
                _ => prop.ToString()
            };
        }


        protected int GetBasePatience(NpcViewModel npc)
        {
            // Base patience by personality type
            return npc.PersonalityType?.ToLower() switch
            {
                "devoted" => 12,
                "mercantile" => 10,
                "proud" => 8,
                "cunning" => 9,
                "gruff" => 7,
                "steadfast" => 11,
                _ => 10
            };
        }

        // Investigation Approach Helper Methods
        protected async Task InvestigateWithApproach(InvestigationApproach approach)
        {
            if (CurrentLocation == null || CurrentSpot == null)
            {
                Console.WriteLine("[LocationContent] Cannot investigate - no current location or spot");
                return;
            }

            Console.WriteLine($"[LocationContent] Starting investigation with {approach} approach");

            bool success = GameFacade.GetLocationFacade().InvestigateLocation(CurrentLocation.Id, CurrentSpot.SpotID, approach);

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
        public List<ConversationOptionViewModel> ConversationOptions { get; set; } = new();
    }

    public class ConversationOptionViewModel
    {
        public string RequestId { get; set; }  // The actual request ID for NPC requests
        public string ConversationTypeId { get; set; }
        public string GoalCardId { get; set; }  // The specific card ID from the NPC's requests
        public string Label { get; set; }
        public string Description { get; set; }  // Full description of the conversation option
        public bool IsAvailable { get; set; }
        public bool IsExchange { get; set; } // True if this is an exchange, not a conversation
        public string EngagementType { get; set; }
        public string InvestigationLabel { get; set; }
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