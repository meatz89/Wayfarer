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
        
        [Parameter] public EventCallback OnActionExecuted { get; set; }
        
        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        protected LocationSpot CurrentSpot { get; set; }
        protected List<NpcViewModel> AvailableNpcs { get; set; } = new();
        protected List<LocationObservationViewModel> AvailableObservations { get; set; } = new();
        protected List<SpotViewModel> AvailableSpots { get; set; } = new();
        protected bool CanTravel { get; set; }
        protected bool CanWork { get; set; }
        protected TimeBlocks CurrentTime { get; set; }
        protected List<NPC> NPCsAtSpot { get; set; } = new();

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
                    
                    // Add available conversation types
                    if (npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
                    {
                        options.Add(new ConversationOptionViewModel
                        {
                            Type = ConversationType.QuickExchange,
                            Label = "Quick Exchange",
                            AttentionCost = 0,
                            IsAvailable = true
                        });
                    }
                    
                    if (npc.HasCrisisCards())
                    {
                        options.Add(new ConversationOptionViewModel
                        {
                            Type = ConversationType.Crisis,
                            Label = "Crisis Resolution",
                            AttentionCost = 1,
                            IsAvailable = true
                        });
                    }
                    else if (npc.ConversationDeck != null)
                    {
                        options.Add(new ConversationOptionViewModel
                        {
                            Type = ConversationType.Standard,
                            Label = "Standard Conversation",
                            AttentionCost = 2,
                            IsAvailable = true
                        });
                    }
                    
                    // Get actual emotional state using the same logic as conversations
                    var emotionalState = GameFacade.GetNPCEmotionalState(npc.ID);
                    
                    AvailableNpcs.Add(new NpcViewModel
                    {
                        Id = npc.ID,
                        Name = npc.Name,
                        PersonalityType = npc.PersonalityType.ToString(),
                        EmotionalState = emotionalState.ToString(),
                        HasCrisis = npc.HasCrisisCards(),
                        ConversationOptions = options
                    });
                }
            }
            
            // Get available observations
            AvailableObservations.Clear();
            Console.WriteLine("[LocationContent] Getting observations from GameFacade...");
            var allObservations = GameFacade.GetObservationsViewModel();
            Console.WriteLine($"[LocationContent] Got ObservationsViewModel, AvailableObservations count: {allObservations?.AvailableObservations?.Count ?? 0}");
            
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
            
            // Check if can work at this spot
            CanWork = spot?.Properties?.Contains("Commercial") ?? false;
        }

        protected async Task StartConversation(string npcId)
        {
            await StartTypedConversation(npcId, ConversationType.Standard);
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

        private string GetConversationLabel(ConversationType type)
        {
            return type switch
            {
                ConversationType.QuickExchange => "Quick Exchange",
                ConversationType.Crisis => "Crisis Resolution",
                ConversationType.Standard => "Standard Conversation",
                _ => type.ToString()
            };
        }

        private int GetAttentionCost(ConversationType type)
        {
            return type switch
            {
                ConversationType.QuickExchange => 0,
                ConversationType.Crisis => 1,
                ConversationType.Standard => 2,
                _ => 0
            };
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
                ConversationType.QuickExchange => "exchange",
                ConversationType.Crisis => "crisis",
                _ => ""
            };
        }

        protected string GetNPCDescription(NpcViewModel npc)
        {
            // Generate contextual descriptions based on NPC state
            var state = npc.EmotionalState?.ToLower();
            var personality = npc.PersonalityType?.ToLower();
            
            if (state == "desperate" && npc.HasCrisis)
            {
                return "Clutching a sealed letter with white knuckles, eyes darting nervously.";
            }
            else if (state == "hostile")
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
    }

    // View Models
    public class NpcViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PersonalityType { get; set; }
        public string EmotionalState { get; set; }
        public bool HasCrisis { get; set; }
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