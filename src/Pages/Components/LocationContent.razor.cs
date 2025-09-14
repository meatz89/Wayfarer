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
        protected TimeBlocks CurrentTime { get; set; }
        protected List<NPC> NPCsAtSpot { get; set; } = new();
        protected IEnumerable<DeliveryObligation> ActiveObligations { get; set; } = new List<DeliveryObligation>();
        protected Location CurrentLocation { get; set; }

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
                        // Get attention cost from backend mechanics
                        ConversationFacade conversationFacade = GameFacade.GetConversationFacade();
                        int attentionCost = conversationFacade.GetConversationAttentionCost(conversationOption.Type);

                        ConversationOptionViewModel option = new ConversationOptionViewModel
                        {
                            Type = conversationOption.Type,
                            GoalCardId = conversationOption.GoalCardId,
                            Label = conversationOption.DisplayName ?? GetConversationLabel(conversationOption.Type),
                            Description = conversationOption.Description,
                            AttentionCost = attentionCost,
                            IsAvailable = true
                        };
                        options.Add(option);
                    }

                    // Check if NPC has exchange cards (separate from conversations!)
                    if (npc.HasExchangeCards())
                    {
                        // Add exchange as a special option (not a conversation type)
                        ConversationOptionViewModel exchangeOption = new ConversationOptionViewModel
                        {
                            Type = null, // No conversation type - this is an exchange!
                            Label = "Quick Exchange",
                            AttentionCost = 1, // Exchanges always cost 1 attention
                            IsAvailable = true,
                            IsExchange = true // Mark this as an exchange
                        };
                        options.Add(exchangeOption);
                    }

                    // Get actual connection state using the same logic as conversations
                    ConnectionState connectionState = GameFacade.GetNPCConnectionState(npc.ID);
                    
                    // Include flow in the display
                    string stateDisplay = connectionState.ToString();
                    if (npc.CurrentFlow != 0)
                    {
                        stateDisplay += npc.CurrentFlow > 0 ? $" (+{npc.CurrentFlow})" : $" ({npc.CurrentFlow})";
                    }

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
                IEnumerable<LocationSpot> allSpots = GameWorld.Spots.Values
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

            // Check if can work at this spot (legacy)
            CanWork = spot?.SpotProperties?.Contains(SpotPropertyType.Commercial) ?? false;

            // Get dynamic location actions
            LocationActions.Clear();
            if (location != null && spot != null)
            {
                try
                {
                    Subsystems.LocationSubsystem.LocationActionManager locationActionManager = GameFacade.GetLocationActionManager();
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
            Subsystems.ObligationSubsystem.ObligationFacade queueManager = GameFacade.GetObligationQueueManager();
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
        }

        protected async Task StartConversation(string npcId)
        {
            await StartTypedConversation(npcId, ConversationType.FriendlyChat);
        }

        protected async Task StartTypedConversation(string npcId, ConversationType type, string goalCardId = null)
        {
            Console.WriteLine($"[LocationContent] Starting {type} conversation with NPC ID: '{npcId}', GoalCard: '{goalCardId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartConversation(npcId, type, goalCardId);
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

            // Check attention first (quick check before calling facade)
            AttentionStateInfo attentionState = GameFacade.GetCurrentAttentionState();
            if (attentionState.Current < 1)
            {
                Console.WriteLine("[LocationContent] Not enough attention for investigation");
                return;
            }

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

            // Check attention requirement
            AttentionStateInfo attentionState = GameFacade.GetCurrentAttentionState();
            if (attentionState.Current < 1) return false;

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
            
            var activeProperties = CurrentSpot.GetActiveProperties(CurrentTime);
            
            // Quiet spots give +2, Busy spots give +1, others give +1
            if (activeProperties.Contains(SpotPropertyType.Quiet))
            {
                return 2;
            }
            return 1;
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

        protected string GetActionClass(ConversationType? type)
        {
            return type switch
            {
                ConversationType.FriendlyChat => "talk",
                ConversationType.Request => "request",
                ConversationType.Delivery => "delivery",
                ConversationType.Resolution => "resolution",
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
            LocationSpot? spot = GameWorld.Spots.Values.FirstOrDefault(s => s.LocationId == currentLocation.Id && s.Name == spotId);
            if (spot == null) return new List<NPC>();

            // Get ALL NPCs at this location and filter by SpotId
            List<NPC> npcsAtLocation = GameFacade.GetNPCsAtLocation(currentLocation.Id);
            return npcsAtLocation.Where(n => n.SpotId == spot.SpotID).ToList();
        }

        protected bool HasUrgentLetter(string npcId)
        {
            // Check if NPC has urgent letter in queue position 1
            Subsystems.ObligationSubsystem.ObligationFacade queueManager = GameFacade.GetObligationQueueManager();
            if (queueManager == null) return false;

            DeliveryObligation[] obligations = queueManager.GetActiveObligations();
            if (obligations == null || !obligations.Any()) return false;

            // Check if position 1 has this NPC's letter
            DeliveryObligation? firstObligation = obligations.FirstOrDefault();
            return firstObligation != null &&
                   (firstObligation.SenderId == npcId || firstObligation.SenderName == GetNPCName(npcId)) &&
                   firstObligation.DeadlineInSegments < 360; // Urgent if less than 6 hours
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

        protected string GetConversationLabel(ConversationType type)
        {
            return type switch
            {
                ConversationType.FriendlyChat => "Talk",
                ConversationType.Request => "Request", // Actual label comes from NPCRequest.Name
                ConversationType.Delivery => "Deliver Letter",
                ConversationType.Resolution => "Make Amends",
                _ => type.ToString()
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

        protected string GetPatienceDisplay(NpcViewModel npc, LocationSpot spot)
        {
            // Get the actual NPC to access daily patience
            NPC actualNPC = GameFacade.GetNPCById(npc.Id);
            if (actualNPC != null)
            {
                // Initialize patience if not yet set
                if (actualNPC.MaxDailyPatience == 0)
                {
                    actualNPC.InitializeDailyPatience();
                }
                
                // Return formatted patience display (current/max)
                return $"{actualNPC.DailyPatience}/{actualNPC.MaxDailyPatience} patience today";
            }

            // Fallback display
            int basePatience = GetBasePatience(npc);
            return $"{basePatience} patience";
        }

        protected int GetPatience(NpcViewModel npc, LocationSpot spot)
        {
            // Get the actual NPC to access daily patience
            NPC actualNPC = GameFacade.GetNPCById(npc.Id);
            if (actualNPC != null)
            {
                // Initialize patience if not yet set
                if (actualNPC.MaxDailyPatience == 0)
                {
                    actualNPC.InitializeDailyPatience();
                }
                
                // Return current daily patience remaining
                return actualNPC.DailyPatience;
            }

            // Fallback to old calculation if NPC not found
            int basePatience = GetBasePatience(npc);

            // Apply spot modifiers
            if (spot?.Properties != null)
            {
                if (spot.Properties.Contains("Public"))
                    basePatience -= 1;
                else if (spot.Properties.Contains("Private"))
                    basePatience += 1;
            }

            return Math.Max(1, basePatience); // Minimum 1 patience
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
                    "commerce" => "Guarded→Eager",
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
    }

    // View Models
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
        public ConversationType? Type { get; set; }
        public string GoalCardId { get; set; }  // The specific card ID from the NPC's requests
        public string Label { get; set; }
        public string Description { get; set; }  // Full description of the conversation option
        public int AttentionCost { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsExchange { get; set; } // True if this is an exchange, not a conversation
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