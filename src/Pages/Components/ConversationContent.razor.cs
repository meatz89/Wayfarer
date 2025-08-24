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
            if (Session?.NPC?.ID != NpcId)
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
                    var npc = GameFacade.GetNPCById(NpcId);
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
                if (Session.CurrentComfort >= ComfortThreshold)
                {
                    GenerateLetter();
                }
            }
        }

        private void GenerateLetter()
        {
            // Prevent duplicate generation in same conversation
            if (Session == null || Session.LetterGenerated) return;
            
            // Determine letter tier based on comfort level
            LetterTier tier = DetermineLetterTier(Session.CurrentComfort);
            
            // Create the delivery obligation
            var obligation = CreateLetterFromComfort(tier);
            if (obligation == null)
            {
                Console.WriteLine("[ConversationContent] Failed to create letter - no valid recipients");
                return;
            }
            
            // Mark as generated to prevent duplicates
            Session.LetterGenerated = true;
            
            // Log the letter generation
            var npcName = GameFacade.GetNPCById(NpcId)?.Name ?? "The NPC";
            Console.WriteLine($"[ConversationContent] Letter generated from {npcName}!");
            Console.WriteLine($"   → Tier: {GetTierDescription(tier)}");
            Console.WriteLine($"   → Deadline: {obligation.DeadlineInMinutes / 60}h | Payment: {obligation.Payment} coins");
            Console.WriteLine($"   → Comfort Level: {Session.CurrentComfort}");
            
            // Store the obligation for later processing
            // In a real implementation, this would be added to the queue
            // For now, we just mark it as generated
        }
        
        private LetterTier DetermineLetterTier(int comfort)
        {
            // Algorithm: Tier = Floor((Comfort - 5) / 5)
            // 5-9: Simple (T1)
            // 10-14: Important (T2)  
            // 15-19: Urgent (T3)
            // 20+: Critical (T4)
            
            if (comfort >= 20) return LetterTier.Critical;
            if (comfort >= 15) return LetterTier.Urgent;
            if (comfort >= 10) return LetterTier.Important;
            return LetterTier.Simple;
        }
        
        private DeliveryObligation CreateLetterFromComfort(LetterTier tier)
        {
            // For now, hardcode a recipient since we don't have access to all NPCs
            // In a real implementation, this would select from available NPCs
            var recipientId = "merchant_thomas"; // Default recipient
            var recipientName = "Thomas the Merchant";
            
            // Determine letter parameters based on tier
            var (deadline, payment, stakes, weight) = GetTierParameters(tier);
            
            // Get the NPC
            var npc = GameFacade.GetNPCById(NpcId);
            if (npc == null) return null;
            
            // Determine token type from NPC's available types
            var tokenType = DetermineTokenType(npc);
            
            return new DeliveryObligation
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = npc.ID,
                SenderName = npc.Name,
                RecipientId = recipientId,
                RecipientName = recipientName,
                TokenType = tokenType,
                DeadlineInMinutes = deadline,
                Payment = payment,
                Stakes = stakes,
                EmotionalWeight = weight,
                Tier = ConvertToTierLevel(tier),
                Description = GenerateLetterDescription(npc.Name, recipientName, tier),
                GenerationReason = $"Generated from {Session.CurrentComfort} comfort in conversation"
            };
        }
        
        private (int deadline, int payment, StakeType stakes, EmotionalWeight weight) GetTierParameters(LetterTier tier)
        {
            // EXACT specifications as requested
            return tier switch
            {
                LetterTier.Simple => (1440, 5, StakeType.REPUTATION, EmotionalWeight.LOW),      // 24h, 5 coins
                LetterTier.Important => (720, 10, StakeType.WEALTH, EmotionalWeight.MEDIUM),    // 12h, 10 coins
                LetterTier.Urgent => (360, 15, StakeType.STATUS, EmotionalWeight.HIGH),         // 6h, 15 coins
                LetterTier.Critical => (120, 20, StakeType.SAFETY, EmotionalWeight.CRITICAL),   // 2h, 20 coins
                _ => (1440, 5, StakeType.REPUTATION, EmotionalWeight.LOW)
            };
        }
        
        private ConnectionType DetermineTokenType(NPC npc)
        {
            // Use NPC's primary letter token type, or default to Trust
            if (npc.LetterTokenTypes != null && npc.LetterTokenTypes.Any())
            {
                return npc.LetterTokenTypes.First();
            }
            return ConnectionType.Trust;
        }
        
        private TierLevel ConvertToTierLevel(LetterTier tier)
        {
            return tier switch
            {
                LetterTier.Simple => TierLevel.T1,
                LetterTier.Important => TierLevel.T2,
                LetterTier.Urgent => TierLevel.T3,
                LetterTier.Critical => TierLevel.T3, // Map Critical to T3 as there's no T4
                _ => TierLevel.T1
            };
        }
        
        private string GenerateLetterDescription(string senderName, string recipientName, LetterTier tier)
        {
            return tier switch
            {
                LetterTier.Simple => $"A routine message from {senderName} to {recipientName}",
                LetterTier.Important => $"Important correspondence requiring timely delivery",
                LetterTier.Urgent => $"Urgent matter that cannot wait",
                LetterTier.Critical => $"CRITICAL: Lives may depend on this delivery",
                _ => $"Letter from {senderName}"
            };
        }
        
        private string GetTierDescription(LetterTier tier)
        {
            return tier switch
            {
                LetterTier.Simple => "simple",
                LetterTier.Important => "important",
                LetterTier.Urgent => "urgent",
                LetterTier.Critical => "CRITICAL",
                _ => "standard"
            };
        }
        
        // Internal enum for letter tiers
        private enum LetterTier
        {
            Simple,    // 5-9 comfort
            Important, // 10-14 comfort
            Urgent,    // 15-19 comfort
            Critical   // 20+ comfort
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
            return Math.Min(100, (Session.CurrentComfort * 100) / ComfortThreshold);
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