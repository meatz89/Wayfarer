using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class ConversationContentBase : ComponentBase
    {
        [Parameter] public string NpcId { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }

        [Inject] protected ConversationManager ConversationManager { get; set; }
        [Inject] protected GameFacade GameFacade { get; set; }
        [Inject] protected NavigationCoordinator NavigationCoordinator { get; set; }
        [Inject] protected ObservationManager ObservationManager { get; set; }

        protected ConversationSession Session { get; set; }
        protected HashSet<ConversationCard> SelectedCards { get; set; } = new();
        protected int TotalSelectedWeight => SelectedCards.Sum(c => c.Weight);
        protected bool IsProcessing { get; set; }
        
        protected string NpcName { get; set; }
        protected string LastNarrative { get; set; }
        protected string LastDialogue { get; set; }
        protected int ComfortThreshold => 10; // For letter generation

        protected override async Task OnInitializedAsync()
        {
            await StartConversation();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Session?.NpcId != NpcId)
            {
                await StartConversation();
            }
        }

        private async Task StartConversation()
        {
            try
            {
                var conversationType = NavigationCoordinator.GetConversationType();
                
                // Get observation cards from inventory
                var observationCards = GetObservationCards();
                
                // Start conversation
                Session = ConversationManager.StartConversation(NpcId, conversationType, observationCards);
                
                if (Session != null)
                {
                    var npc = GameFacade.GetNPC(NpcId);
                    NpcName = npc?.Name ?? "Unknown";
                    
                    // Generate initial narrative
                    LastNarrative = "The conversation begins...";
                    LastDialogue = GetInitialDialogue();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConversationContent] Failed to start conversation: {ex.Message}");
                await OnConversationEnd.InvokeAsync();
            }
        }

        private List<ConversationCard> GetObservationCards()
        {
            // Get observation cards from the observation manager
            return ObservationManager?.GetCollectedObservationCards() ?? new List<ConversationCard>();
        }

        protected async Task ExecuteListen()
        {
            if (IsProcessing || Session == null) return;
            
            IsProcessing = true;
            SelectedCards.Clear();
            
            try
            {
                var result = ConversationManager.ExecuteListen();
                ProcessResult(result);
                
                if (Session.IsComplete)
                {
                    await Task.Delay(1000);
                    await OnConversationEnd.InvokeAsync();
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        protected async Task ExecuteSpeak()
        {
            if (IsProcessing || Session == null || !SelectedCards.Any()) return;
            
            IsProcessing = true;
            
            try
            {
                var result = ConversationManager.ExecuteSpeak(SelectedCards.ToList());
                ProcessResult(result);
                SelectedCards.Clear();
                
                if (Session.IsComplete)
                {
                    await Task.Delay(1000);
                    await OnConversationEnd.InvokeAsync();
                }
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        private void ProcessResult(CardPlayResult result)
        {
            if (result != null)
            {
                LastNarrative = result.NarrativeText;
                LastDialogue = result.DialogueText;
                
                // Check for letter generation
                if (Session.ComfortBuilt >= ComfortThreshold)
                {
                    GenerateLetter();
                }
            }
        }

        private void GenerateLetter()
        {
            // TODO: Implement letter generation
            Console.WriteLine($"[ConversationContent] Letter generation triggered at comfort {Session.ComfortBuilt}");
        }

        protected void ToggleCardSelection(ConversationCard card)
        {
            if (Session.ConversationType == ConversationType.QuickExchange)
            {
                // For exchanges, selecting a card immediately plays it
                SelectedCards.Clear();
                SelectedCards.Add(card);
                _ = ExecuteSpeak();
                return;
            }
            
            if (SelectedCards.Contains(card))
            {
                SelectedCards.Remove(card);
            }
            else if (CanSelectCard(card))
            {
                SelectedCards.Add(card);
            }
            
            StateHasChanged();
        }

        protected bool CanSelectCard(ConversationCard card)
        {
            if (Session == null) return false;
            
            // Check weight limit
            var newWeight = TotalSelectedWeight + card.Weight;
            return newWeight <= GetWeightLimit();
        }

        protected bool IsCardSelected(ConversationCard card)
        {
            return SelectedCards.Contains(card);
        }

        protected bool CanSpeak()
        {
            return SelectedCards.Any() && TotalSelectedWeight <= GetWeightLimit();
        }

        protected async Task EndConversation()
        {
            ConversationManager.EndConversation();
            await OnConversationEnd.InvokeAsync();
        }

        // UI Helper Methods
        protected string GetConversationModeTitle()
        {
            return Session?.ConversationType switch
            {
                ConversationType.QuickExchange => "Quick Exchange",
                ConversationType.Crisis => "Crisis Resolution",
                ConversationType.Standard => "Standard Conversation",
                _ => "Conversation"
            };
        }

        protected string GetStateClass()
        {
            return Session?.NPCState switch
            {
                EmotionalState.Desperate => "desperate",
                EmotionalState.Hostile => "hostile",
                EmotionalState.Tense => "tense",
                _ => ""
            };
        }

        protected int GetWeightLimit()
        {
            if (Session == null) return 3;
            
            return Session.NPCState switch
            {
                EmotionalState.Guarded => 2,
                EmotionalState.Tense => 1,
                EmotionalState.Overwhelmed => 1,
                EmotionalState.Connected => 4,
                EmotionalState.Hostile => 0,
                _ => 3
            };
        }

        protected string GetListenDetails()
        {
            if (Session == null) return "";
            
            return Session.NPCState switch
            {
                EmotionalState.Desperate => "Draw 2 + Crisis • State → Hostile",
                EmotionalState.Hostile => "Draw 1 + 2 Crisis • Ends conversation",
                EmotionalState.Guarded => "Draw 1 • State → Neutral",
                EmotionalState.Open => "Draw 3 • State unchanged",
                EmotionalState.Eager => "Draw 3 • State unchanged",
                _ => "Draw 2 • State unchanged"
            };
        }

        protected string GetSpeakDetails()
        {
            return $"Weight limit: {GetWeightLimit()}";
        }

        protected string GetStateEffects()
        {
            if (Session == null) return "";
            
            return Session.NPCState switch
            {
                EmotionalState.Desperate => "• Draw 2 + crisis • Crisis free • Listen worsens",
                EmotionalState.Hostile => "• Only crisis cards playable",
                EmotionalState.Connected => "• Weight limit 4 • All comfort +2",
                EmotionalState.Eager => "• 2+ same type → +3 comfort",
                _ => ""
            };
        }

        protected int GetComfortProgress()
        {
            if (Session == null) return 0;
            return Math.Min(100, (Session.ComfortBuilt * 100) / ComfortThreshold);
        }

        protected string GetDepthLabel()
        {
            return Session?.CurrentDepth switch
            {
                0 => "Surface",
                1 => "Personal",
                2 => "Intimate",
                _ => "Surface"
            };
        }

        protected int GetDepthProgress()
        {
            if (Session == null) return 0;
            return (Session.CurrentDepth * 100) / 3;
        }

        protected string GetCardClass(ConversationCard card)
        {
            if (card.Template == CardTemplateType.Crisis)
                return "crisis";
            if (card.Template == CardTemplateType.State)
                return "state";
            if (card.Persistence == PersistenceType.OneShot)
                return "observation";
            return "comfort";
        }

        protected string GetCardName(ConversationCard card)
        {
            return card.Context?.SimpleText ?? card.Template.ToString();
        }

        protected List<string> GetCardTags(ConversationCard card)
        {
            var tags = new List<string>();
            
            tags.Add(card.Template.ToString());
            
            if (card.TokenType != TokenType.None)
                tags.Add(card.TokenType.ToString());
                
            tags.Add(card.Persistence.ToString());
            
            return tags;
        }

        protected string GetSuccessEffect(ConversationCard card)
        {
            if (card.Template == CardTemplateType.State)
                return "Change state";
            return $"Success: +{card.ComfortOnSuccess} comfort";
        }

        protected string GetFailureEffect(ConversationCard card)
        {
            if (card.Template == CardTemplateType.State)
                return "No change";
            return $"Fail: +{card.ComfortOnFailure} comfort";
        }

        private string GetInitialDialogue()
        {
            return Session?.NPCState switch
            {
                EmotionalState.Desperate => "Please, I need your help urgently!",
                EmotionalState.Hostile => "What do you want?!",
                EmotionalState.Tense => "I don't have much time...",
                _ => "Hello, what brings you here?"
            };
        }
    }
}