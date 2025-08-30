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
            var location = GameFacade.GetCurrentLocation();
            CurrentLocation = location;
            var spot = GameFacade.GetCurrentLocationSpot();
            CurrentSpot = spot;
            var timeInfo = GameFacade.GetTimeInfo();
            CurrentTime = timeInfo.timeBlock;
            
            // Get NPCs at current spot
            AvailableNpcs.Clear();
            NPCsAtSpot.Clear();
            if (CurrentSpot != null)
            {
                // Get NPCs at the current spot for the current time
                var npcsAtSpot = GameFacade.GetNPCsAtCurrentSpot();
                NPCsAtSpot = npcsAtSpot ?? new List<NPC>();
                foreach (var npc in NPCsAtSpot)
                {
                    var options = new List<ConversationOptionViewModel>();
                    
                    // Get ACTUAL available conversation types from ConversationManager
                    var conversationManager = GameFacade.GetConversationManager();
                    var availableTypes = conversationManager.GetAvailableConversationTypes(npc);
                    
                    // Add each available type as an option
                    foreach (var type in availableTypes)
                    {
                        // Get attention cost from backend mechanics
                        var attentionCost = conversationManager.GetConversationAttentionCost(type);
                        
                        var option = new ConversationOptionViewModel
                        {
                            Type = type,
                            Label = GetConversationLabel(type),
                            AttentionCost = attentionCost,
                            IsAvailable = true
                        };
                        options.Add(option);
                    }
                    
                    // Get actual emotional state using the same logic as conversations
                    var emotionalState = GameFacade.GetNPCEmotionalState(npc.ID);
                    
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
            var allObservations = GameFacade.GetObservationsViewModel();
            var takenObservations = GameFacade.GetTakenObservations();
            Console.WriteLine($"[LocationContent] Got ObservationsViewModel, AvailableObservations count: {allObservations?.AvailableObservations?.Count ?? 0}");
            Console.WriteLine($"[LocationContent] Got {takenObservations?.Count ?? 0} taken observations");
            
            if (allObservations?.AvailableObservations != null)
            {
                AvailableObservations = allObservations.AvailableObservations
                    .Select(obs => new LocationObservationViewModel
                    {
                        Id = obs.Id,
                        Name = obs.Title,
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
            CanTravel = spot?.Properties?.Contains("Crossroads") ?? false;
            Console.WriteLine($"[LocationContent] Spot: {spot?.Name}, Properties: {string.Join(", ", spot?.Properties ?? new List<string>())}, CanTravel: {CanTravel}");
            
            // Check if can work at this spot
            CanWork = spot?.Properties?.Contains("Commercial") ?? false;
            
            // Get active obligations from the queue manager
            var queueManager = GameFacade.GetObligationQueueManager();
            if (queueManager != null)
            {
                var obligations = queueManager.GetActiveObligations();
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
            var attentionState = GameFacade.GetCurrentAttentionState();
            if (attentionState.Current < 1)
            {
                Console.WriteLine("[LocationContent] Not enough attention for observation");
                return;
            }
            
            // Call GameFacade to take the observation
            // This will handle attention spending, card generation, and adding to hand
            var success = await GameFacade.TakeObservationAsync(observationId);
            
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
            
            var result = await GameFacade.PerformWork();
            
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
            var currentLocation = GameFacade.GetCurrentLocation();
            if (currentLocation == null) return new List<NPC>();
            
            // Get the spot we're checking
            var spot = currentLocation.Spots?.FirstOrDefault(s => s.Name == spotId);
            if (spot == null) return new List<NPC>();
            
            // Get ALL NPCs at this location and filter by SpotId
            var npcsAtLocation = GameFacade.GetNPCsAtLocation(currentLocation.Id);
            return npcsAtLocation.Where(n => n.SpotId == spot.SpotID).ToList();
        }
        
        protected bool HasUrgentLetter(string npcId)
        {
            // Check if NPC has urgent letter in queue position 1
            var queueManager = GameFacade.GetObligationQueueManager();
            if (queueManager == null) return false;
            
            var obligations = queueManager.GetActiveObligations();
            if (obligations == null || !obligations.Any()) return false;
            
            // Check if position 1 has this NPC's letter
            var firstObligation = obligations.FirstOrDefault();
            return firstObligation != null && 
                   (firstObligation.SenderId == npcId || firstObligation.SenderName == GetNPCName(npcId)) &&
                   firstObligation.DeadlineInMinutes < 360; // Urgent if less than 6 hours
        }
        
        protected string GetNPCName(string npcId)
        {
            var npc = GameFacade.GetNPCById(npcId);
            return npc?.Name ?? "";
        }
        
        protected string GetNPCStateDisplay(NPC npc)
        {
            // Get display text for NPC emotional state
            var emotionalState = GameFacade.GetNPCEmotionalState(npc.ID);
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
            // Generate contextual descriptions based on NPC state
            var state = npc.EmotionalState?.ToLower();
            var personality = npc.PersonalityType?.ToLower();
            
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
            return CurrentTime switch
            {
                TimeBlocks.Dawn => "Dawn",
                TimeBlocks.Morning => "Morning",
                TimeBlocks.Afternoon => "Afternoon",
                TimeBlocks.Evening => "Evening",
                TimeBlocks.Night => "Night",
                TimeBlocks.LateNight => "Late Night",
                _ => "Unknown"
            };
        }
        
        protected string GetSpotTraitClass(SpotPropertyType prop)
        {
            // Return a CSS class based on the property type
            return prop.ToString().ToLower().Replace("_", "-");
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