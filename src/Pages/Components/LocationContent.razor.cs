using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class LocationContentBase : ComponentBase
    {
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected DevModeService DevMode { get; set; }

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

                    // Get ACTUAL available conversation types from ConversationManager
                    ConversationFacade conversationFacade = GameFacade.GetConversationFacade();
                    List<ConversationType> availableTypes = conversationFacade.GetAvailableConversationTypes(npc);

                    // Add each available type as an option
                    foreach (ConversationType type in availableTypes)
                    {
                        // Get attention cost from backend mechanics
                        int attentionCost = conversationFacade.GetConversationAttentionCost(type);

                        ConversationOptionViewModel option = new ConversationOptionViewModel
                        {
                            Type = type,
                            Label = GetConversationLabel(type),
                            AttentionCost = attentionCost,
                            IsAvailable = true
                        };
                        options.Add(option);
                    }

                    // Get actual emotional state using the same logic as conversations
                    EmotionalState emotionalState = GameFacade.GetNPCEmotionalState(npc.ID);

                    AvailableNpcs.Add(new NpcViewModel
                    {
                        Id = npc.ID,
                        Name = npc.Name,
                        PersonalityType = npc.PersonalityType.ToString(),
                        EmotionalState = emotionalState.ToString(),
                        ConversationOptions = options
                    });
                }
            }

            // Get available observations
            AvailableObservations.Clear();
            TakenObservations.Clear();
            Console.WriteLine("[LocationContent] Getting observations from GameFacade...");
            // GetTakenObservations() works correctly, but need to implement GetAvailableObservations() method
            List<TakenObservation>? takenObservations = GameFacade.GetTakenObservations();
            Console.WriteLine($"[LocationContent] Got {takenObservations?.Count ?? 0} taken observations");

            // Temporarily disable observations until proper implementation
            if (false) // allObservations?.AvailableObservations != null)
            {
                AvailableObservations = new List<LocationObservationViewModel>()
                    .Select(obs => new LocationObservationViewModel
                    {
                        Id = obs.Id,
                        Name = obs.Name,
                        Type = obs.Type
                    }).ToList();
                Console.WriteLine($"[LocationContent] Final AvailableObservations count: {AvailableObservations.Count}");
            }
            else
            {
                Console.WriteLine("[LocationContent] No observations available or null");
            }

            if (takenObservations != null)
            {
                TakenObservations = takenObservations;
            }

            // Get other spots in this location
            AvailableSpots.Clear();
            if (location != null && location.Spots != null)
            {
                AvailableSpots = location.Spots
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
                    var locationActionManager = GameFacade.GetLocationActionManager();
                    if (locationActionManager != null)
                    {
                        var actions = locationActionManager.GetLocationActions(location, spot);
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

        protected async Task StartTypedConversation(string npcId, ConversationType type)
        {
            Console.WriteLine($"[LocationContent] Starting {type} conversation with NPC ID: '{npcId}'");

            if (GameScreen != null)
            {
                await GameScreen.StartConversation(npcId, type);
            }
            else
            {
                Console.WriteLine($"[LocationContent] GameScreen not available for conversation with NPC '{npcId}'");
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

            // Check attention first (quick check before calling facade)
            AttentionStateInfo attentionState = GameFacade.GetCurrentAttentionState();
            if (attentionState.Current < 1)
            {
                Console.WriteLine("[LocationContent] Not enough attention for observation");
                return;
            }

            // Call GameFacade to take the observation
            // This will handle attention spending, card generation, and adding to hand
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

        protected async Task NavigateToTravel()
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
            
            // For now, we'll implement a simple placeholder that just logs the action
            // TODO: Implement proper action handling through GameFacade
            try
            {
                // Eventually this should call GameFacade.PerformLocationAction(action)
                // For now, just simulate the action
                Console.WriteLine($"[LocationContent] Simulating action: {action.Title}");
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

        protected string GetStateClass(string emotionalState)
        {
            return emotionalState?.ToLower() switch
            {
                "desperate" => "desperate",
                "hostile" => "hostile",
                "crisis" => "crisis",
                _ => ""
            };
        }

        protected string GetActionClass(ConversationType type)
        {
            return type switch
            {
                ConversationType.Commerce => "exchange",
                ConversationType.FriendlyChat => "talk",
                ConversationType.Promise => "promise",
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
            LocationSpot? spot = currentLocation.Spots?.FirstOrDefault(s => s.Name == spotId);
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
                   firstObligation.DeadlineInMinutes < 360; // Urgent if less than 6 hours
        }

        protected string GetNPCName(string npcId)
        {
            NPC npc = GameFacade.GetNPCById(npcId);
            return npc?.Name ?? "";
        }

        protected string GetNPCStateDisplay(NPC npc)
        {
            // Get display text for NPC emotional state
            EmotionalState emotionalState = GameFacade.GetNPCEmotionalState(npc.ID);
            return emotionalState.ToString();
        }

        protected string GetConversationLabel(ConversationType type)
        {
            return type switch
            {
                ConversationType.Commerce => "Quick Exchange",
                ConversationType.FriendlyChat => "Talk",
                ConversationType.Promise => "Letter Offer",
                ConversationType.Delivery => "Deliver Letter",
                ConversationType.Resolution => "Make Amends",
                _ => type.ToString()
            };
        }

        protected string GetDeadlineClass(int deadlineInMinutes)
        {
            if (deadlineInMinutes <= 0)
                return "expired";
            else if (deadlineInMinutes <= 120)
                return "critical";
            else if (deadlineInMinutes <= 360)
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
            string? state = npc.EmotionalState?.ToLower();
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

        protected string GetDeadlineText(int deadlineInMinutes)
        {
            if (deadlineInMinutes <= 0)
                return "EXPIRED!";

            int hours = deadlineInMinutes / 60;
            int minutes = deadlineInMinutes % 60;

            if (hours == 0)
                return $"{minutes}m";
            else if (minutes == 0)
                return $"{hours}h";
            else
                return $"{hours}h {minutes}m";
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

        protected bool HasLetterGoal(string npcId)
        {
            // Check if this NPC has an active letter goal
            // This would check against the game state
            return false;
        }

        protected int GetBurdenCount(string npcId)
        {
            // Get burden count for this NPC
            // This would check against the game state
            return 0;
        }

        protected int GetPatience(NpcViewModel npc, LocationSpot spot)
        {
            // Get base patience based on personality
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
                TimeBlocks.Morning => "Morning",
                TimeBlocks.Afternoon => "Afternoon",
                TimeBlocks.Evening => "Evening",
                TimeBlocks.Night => "Night",
                TimeBlocks.LateNight => "Late Night",
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
                    TimeBlocks.Afternoon => "Busy",
                    TimeBlocks.Evening => "Closing",
                    TimeBlocks.Night => "Empty",
                    _ => ""
                };
            }

            // Tavern locations
            if (locationName.Contains("tavern") || locationName.Contains("kettle"))
            {
                return CurrentTime switch
                {
                    TimeBlocks.Morning => "Quiet",
                    TimeBlocks.Afternoon => "Quiet",
                    TimeBlocks.Evening => "Busy",
                    TimeBlocks.Night => "Lively",
                    _ => ""
                };
            }

            // Noble/Manor locations
            if (locationName.Contains("noble") || locationName.Contains("manor"))
            {
                return CurrentTime switch
                {
                    TimeBlocks.Morning => "Formal",
                    TimeBlocks.Afternoon => "Active",
                    TimeBlocks.Evening => "Reception",
                    TimeBlocks.Night => "Private",
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
                    return "Any→Tense";
                }
                else if (name.Contains("carriage") || name.Contains("preparation"))
                {
                    return "Tense→Eager";
                }
                else if (name.Contains("family") || name.Contains("letter"))
                {
                    return "Neutral→Open";
                }
                else if (name.Contains("trembling") || name.Contains("desperat"))
                {
                    return "Desperate→Open";
                }

                // Fall back to type-based display
                return type switch
                {
                    "authority" => "Any→Tense",
                    "commerce" => "Tense→Eager",
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
                SpotPropertyType.Quiet => "Quiet (+1 comfort)",
                SpotPropertyType.Loud => "Loud (-1 comfort)",
                SpotPropertyType.Warm => "Warm (+1 comfort)",
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
        public string EmotionalState { get; set; }
        public List<ConversationOptionViewModel> ConversationOptions { get; set; } = new();
    }

    public class ConversationOptionViewModel
    {
        public ConversationType Type { get; set; }
        public string Label { get; set; }
        public int AttentionCost { get; set; }
        public bool IsAvailable { get; set; }
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