using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Deterministic narrative provider for non-AI conversations.
/// Used for action conversations with pre-defined choices and outcomes.
/// </summary>
public class DeterministicNarrativeProvider : INarrativeProvider
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;
    private readonly LetterCategoryService _letterCategoryService;
    private readonly DeliveryConversationService _deliveryConversationService;
    
    public DeterministicNarrativeProvider(
        GameWorld gameWorld, 
        ConnectionTokenManager tokenManager,
        RouteDiscoveryManager routeDiscoveryManager,
        LetterCategoryService letterCategoryService,
        DeliveryConversationService deliveryConversationService)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _routeDiscoveryManager = routeDiscoveryManager;
        _letterCategoryService = letterCategoryService;
        _deliveryConversationService = deliveryConversationService;
    }
    
    public Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        // Look for initial narrative in context
        if (context is ActionConversationContext actionContext && !string.IsNullOrEmpty(actionContext.InitialNarrative))
        {
            return Task.FromResult(actionContext.InitialNarrative);
        }
        
        // Check if this is an action conversation with special narrative handling
        if (context is ActionConversationContext actionCtx && actionCtx.SourceAction != null)
        {
            var specialIntroduction = GetActionSpecificIntroduction(actionCtx);
            if (specialIntroduction != null)
            {
                return Task.FromResult(specialIntroduction);
            }
        }
        
        // Check if this is a queue management conversation
        if (context is QueueManagementContext queueCtx)
        {
            var queueIntroduction = GetQueueManagementIntroduction(queueCtx);
            return Task.FromResult(queueIntroduction);
        }
        
        // Simple one-sentence narratives for thin layer
        var narrative = context.ConversationTopic switch
        {
            "Action_GatherResources" => "You carefully search the area for edible berries.",
            "Action_Browse" => "You examine the market stalls, noting prices and goods.",
            "Action_Observe" => "You blend into the crowd, listening to local gossip.",
            "Action_Work" => $"You begin your work for {context.TargetNPC?.Name ?? "your employer"}.",
            "Action_Socialize" => $"You engage {context.TargetNPC?.Name ?? "someone"} in friendly conversation.",
            "Action_Converse" => $"You approach {context.TargetNPC?.Name ?? "someone"} to talk.",
            "Action_Collect" => $"{context.TargetNPC?.Name ?? "The sender"} hands you a sealed letter with instructions for safe delivery.",
            "Action_Trade" => $"You browse {context.TargetNPC?.Name ?? "the merchant"}'s wares.",
            "Action_Rest" => "You find a comfortable spot to rest.",
            _ => "You proceed with your action."
        };
        
        return Task.FromResult(narrative);
    }
    
    public Task<List<ConversationChoice>> GenerateChoices(
        ConversationContext context, 
        ConversationState state,
        List<ChoiceTemplate> availableTemplates)
    {
        // Check if this is a queue management conversation
        if (context is QueueManagementContext queueContext)
        {
            var choices = GenerateQueueManagementChoices(queueContext);
            return Task.FromResult(choices);
        }
        
        // Check if this is an action conversation with special handling
        if (context is ActionConversationContext actionContext && actionContext.SourceAction != null)
        {
            var choices = GenerateActionChoices(actionContext);
            if (choices != null)
            {
                return Task.FromResult(choices);
            }
        }
        
        // For thin narrative layer, always return a single "Continue" button
        var defaultChoices = new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "1",
                NarrativeText = "Continue",
                FocusCost = 0,
                IsAffordable = true,
                ChoiceType = ConversationChoiceType.Continue,
                TemplatePurpose = "Proceed with the action"
            }
        };
        
        return Task.FromResult(defaultChoices);
    }
    
    public Task<string> GenerateReaction(
        ConversationContext context,
        ConversationState state,
        ConversationChoice selectedChoice,
        bool success)
    {
        // Handle specific choice templates that need reactions
        if (selectedChoice.ChoiceType == ConversationChoiceType.AcceptLetterOffer)
        {
            var npc = context.TargetNPC;
            return Task.FromResult($"{npc?.Name ?? "They"} smile and hand you a sealed letter. 'I knew I could count on you.'");
        }
        else if (selectedChoice.ChoiceType == ConversationChoiceType.DeclineLetterOffer)
        {
            var npc = context.TargetNPC;
            return Task.FromResult($"{npc?.Name ?? "They"} nod understandingly. 'Perhaps another time then.'");
        }
        else if (selectedChoice.ChoiceType == ConversationChoiceType.Introduction)
        {
            var npc = context.TargetNPC;
            return Task.FromResult($"{npc?.Name ?? "They"} seem pleased to make your acquaintance.");
        }
        else if (selectedChoice.ChoiceType == ConversationChoiceType.DiscoverRoute)
        {
            var npc = context.TargetNPC;
            return Task.FromResult($"{npc?.Name ?? "They"} look around carefully. 'I do know a few paths... Let me tell you about them.'");
        }
        
        // For most thin narrative layer actions, no reaction needed
        return Task.FromResult("");
    }
    
    public Task<string> GenerateConclusion(
        ConversationContext context,
        ConversationState state,
        ConversationChoice lastChoice)
    {
        // For thin narrative layer, no conclusion needed - action completes immediately
        return Task.FromResult("");
    }
    
    public Task<bool> IsAvailable()
    {
        // Deterministic provider is always available
        return Task.FromResult(true);
    }
    
    private string GetNarrativeTextForTemplate(ChoiceTemplate template, ConversationContext context)
    {
        // Generate appropriate narrative text based on template name
        return template.TemplateName switch
        {
            "GatherSafely" => "Take time to gather carefully",
            "GatherRiskily" => "Rush to gather more quickly",
            "WorkHard" => "Put in maximum effort (2 stamina)",
            "WorkLight" => "Do lighter tasks (1 stamina)",
            "DeliverForTokens" => "Accept token payment",
            "DeliverForCoins" => "Request coin payment",
            "SocializePersonal" => "Discuss personal matters",
            "SocializeBusiness" => "Talk about business",
            _ => template.TemplatePurpose ?? "Continue"
        };
    }
    
    private List<ConversationChoice> GenerateDefaultChoices(ConversationContext context, ConversationState state)
    {
        // Provide simple default choice to continue
        return new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "1",
                NarrativeText = "Continue",
                FocusCost = 0,
                IsAffordable = true,
                ChoiceType = ConversationChoiceType.Continue,
                TemplatePurpose = "Continue the conversation"
            }
        };
    }
    
    private string GetDeliveryIntroduction(ConversationContext context)
    {
        // Check if letter is collected for delivery action
        if (context is ActionConversationContext actionContext && actionContext.SourceAction != null)
        {
            var player = _gameWorld.GetPlayer();
            var letter = player.LetterQueue.FirstOrDefault();
            
            if (letter == null)
            {
                return $"{context.TargetNPC?.Name ?? "The recipient"} looks expectant. 'Do you have something for me?' You realize you have no letter in position 1.";
            }
            else if (letter.State != LetterState.Collected)
            {
                return $"{context.TargetNPC?.Name ?? "The recipient"} waits patiently. 'I heard you have a letter for me?' You realize you haven't collected it yet.";
            }
            else if (letter.RecipientName != context.TargetNPC?.Name)
            {
                return $"{context.TargetNPC?.Name ?? "The recipient"} looks confused. 'Are you sure that letter is for me?'";
            }
            else
            {
                return $"You present the sealed letter to {context.TargetNPC?.Name ?? "the recipient"}. They examine the seal carefully.";
            }
        }
        return $"You approach {context.TargetNPC?.Name ?? "the recipient"} to deliver a letter.";
    }
    
    private string GetActionSpecificIntroduction(ActionConversationContext context)
    {
        // Handle actions that need special state checks categorically
        var action = context.SourceAction;
        
        // Check if this is a delivery action (has letter context)
        if (action.Action == LocationAction.Deliver)
        {
            return GetDeliveryIntroduction(context);
        }
        
        // Check if this is a conversation action that might offer letters
        if (action.Action == LocationAction.Converse && context.TargetNPC != null)
        {
            var tokens = _tokenManager.GetTokensWithNPC(context.TargetNPC.ID);
            var totalTokens = tokens.Values.Sum();
            
            if (totalTokens == 0)
            {
                return $"You approach {context.TargetNPC.Name} to introduce yourself.";
            }
            else if (totalTokens >= 3)
            {
                // Check if they know routes to make intro more contextual
                var routeDiscoveries = _routeDiscoveryManager.GetDiscoveriesFromNPC(context.TargetNPC);
                var hasRoutes = routeDiscoveries.Any(r => !r.Route.IsDiscovered && r.CanAfford);
                
                if (hasRoutes && context.TargetNPC.LetterTokenTypes.Any())
                {
                    return $"{context.TargetNPC.Name} greets you warmly. 'Good to see you! I have work if you need it, and I know some shortcuts you might find useful.'";
                }
                else if (hasRoutes)
                {
                    return $"{context.TargetNPC.Name} leans in conspiratorially. 'You've proven trustworthy. I know some paths that could save you time.'";
                }
                else if (context.TargetNPC.LetterTokenTypes.Any())
                {
                    return $"{context.TargetNPC.Name} greets you warmly. 'Good to see you! I might have some work for you, if you're interested.'";
                }
                else
                {
                    return $"You spend some time chatting with {context.TargetNPC.Name}.";
                }
            }
            else
            {
                return $"You spend some time chatting with {context.TargetNPC.Name}.";
            }
        }
        
        return null; // Use default narrative
    }
    
    private List<ConversationChoice> GenerateActionChoices(ActionConversationContext context)
    {
        var action = context.SourceAction;
        
        // Handle delivery actions with validation
        if (action.Action == LocationAction.Deliver)
        {
            return GetDeliveryChoices(context);
        }
        
        // Handle conversation actions that might offer letters
        if (action.Action == LocationAction.Converse && context.TargetNPC != null)
        {
            return GetConverseChoices(context);
        }
        
        return null; // Use default choices
    }
    
    private List<ConversationChoice> GetDeliveryChoices(ConversationContext context)
    {
        var player = _gameWorld.GetPlayer();
        var letter = player.LetterQueue.FirstOrDefault();
        
        // If letter is not valid for delivery, provide explanation choice
        if (letter == null || letter.State != LetterState.Collected || 
            (context.TargetNPC != null && letter.RecipientName != context.TargetNPC.Name))
        {
            return new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "1",
                    NarrativeText = "Leave (can't deliver)",
                    FocusCost = 0,
                    IsAffordable = true,
                    ChoiceType = ConversationChoiceType.CannotDeliver,
                    TemplatePurpose = "Cannot complete delivery"
                }
            };
        }
        
        // Use the delivery conversation service to generate contextual choices
        var deliveryContext = _deliveryConversationService.AnalyzeDeliveryContext(letter, context.TargetNPC);
        return _deliveryConversationService.GenerateDeliveryChoices(deliveryContext);
    }
    
    private List<ConversationChoice> GetConverseChoices(ActionConversationContext context)
    {
        var npc = context.TargetNPC;
        if (npc == null) return null;
        
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var totalTokens = tokens.Values.Sum();
        
        // No tokens = just introduction
        if (totalTokens == 0)
        {
            return new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "1",
                    NarrativeText = "Nice to meet you",
                    FocusCost = 0,
                    IsAffordable = true,
                    ChoiceType = ConversationChoiceType.Introduction,
                    TemplatePurpose = "Establish first connection"
                }
            };
        }
        
        // Build choices based on relationship level and what NPC offers
        var choices = new List<ConversationChoice>();
        
        // Check letter categories available from this NPC
        if (npc.LetterTokenTypes.Any())
        {
            // Get available categories for each token type this NPC offers
            var availableCategories = _letterCategoryService.GetAvailableCategories(npc.ID);
            
            if (availableCategories.Any())
            {
                // Show available letter categories
                foreach (var kvp in availableCategories)
                {
                    var tokenType = kvp.Key;
                    var category = kvp.Value;
                    var paymentRange = _letterCategoryService.GetCategoryPaymentRange(category);
                    
                    var categoryText = category switch
                    {
                        LetterCategory.Basic => $"Basic {tokenType} delivery ({paymentRange.min}-{paymentRange.max} coins)",
                        LetterCategory.Quality => $"Quality {tokenType} delivery ({paymentRange.min}-{paymentRange.max} coins)",
                        LetterCategory.Premium => $"Premium {tokenType} delivery ({paymentRange.min}-{paymentRange.max} coins)",
                        _ => "Delivery work"
                    };
                    
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = choices.Count + 1 + "",
                        NarrativeText = categoryText,
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                        TemplatePurpose = $"Accept {category} {tokenType} letter",
                        OfferTokenType = tokenType,
                        OfferCategory = category
                    });
                }
            }
        }
        
        // 3+ tokens = can discover routes (quality threshold)
        if (totalTokens >= GameRules.TOKENS_QUALITY_THRESHOLD)
        {
            // Check if NPC knows any undiscovered routes
            var routeDiscoveries = _routeDiscoveryManager.GetDiscoveriesFromNPC(npc);
            var discoverableRoutes = routeDiscoveries.Where(r => !r.Route.IsDiscovered && r.CanAfford).ToList();
            
            if (discoverableRoutes.Any())
            {
                // Just show one route discovery option, details will be in follow-up conversation
                choices.Add(new ConversationChoice
                {
                    ChoiceID = choices.Count + 1 + "",
                    NarrativeText = "Ask about hidden routes",
                    FocusCost = 0,
                    IsAffordable = true,
                    ChoiceType = ConversationChoiceType.DiscoverRoute,
                    TemplatePurpose = "Learn about secret paths"
                });
            }
        }
        
        // Always have option to just chat
        choices.Add(new ConversationChoice
        {
            ChoiceID = choices.Count + 1 + "",
            NarrativeText = choices.Any() ? "Just catching up today" : "Good to see you too",
            FocusCost = 0,
            IsAffordable = true,
            ChoiceType = choices.Any() ? ConversationChoiceType.DeclineOffers : ConversationChoiceType.FriendlyChat,
            TemplatePurpose = "Continue conversation"
        });
        
        return choices;
    }
    
    private string GetQueueManagementIntroduction(QueueManagementContext context)
    {
        if (context.ManagementAction == "SkipDeliver")
        {
            var letter = context.TargetLetter;
            if (context.SkippedLetters?.Any() == true)
            {
                var skippedNames = string.Join(", ", context.SkippedLetters.Values.Select(l => l.SenderName));
                return $"To deliver {letter.SenderName}'s letter immediately, you'll need to skip ahead of {skippedNames}. This will cost {context.TokenCost} {letter.TokenType} tokens.";
            }
            return $"You consider prioritizing {letter.SenderName}'s letter to position 1. This will cost {context.TokenCost} {letter.TokenType} tokens.";
        }
        else if (context.ManagementAction == "Purge")
        {
            var letter = context.TargetLetter;
            return $"You're about to discard {letter.SenderName}'s letter from your queue. This will permanently remove the obligation but costs 3 tokens of any type.";
        }
        
        return "You consider managing your letter queue.";
    }
    
    private List<ConversationChoice> GenerateQueueManagementChoices(QueueManagementContext context)
    {
        if (context.ManagementAction == "SkipDeliver")
        {
            var letter = context.TargetLetter;
            var tokenCount = _tokenManager.GetTokenCount(letter.TokenType);
            
            return new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "1",
                    NarrativeText = $"Skip and deliver ({context.TokenCost} {letter.TokenType} tokens)",
                    FocusCost = 0,
                    IsAffordable = tokenCount >= context.TokenCost,
                    ChoiceType = ConversationChoiceType.SkipAndDeliver,
                    TemplatePurpose = "Pay tokens to skip queue order"
                },
                new ConversationChoice
                {
                    ChoiceID = "2",
                    NarrativeText = "Respect queue order",
                    FocusCost = 0,
                    IsAffordable = true,
                    ChoiceType = ConversationChoiceType.RespectQueueOrder,
                    TemplatePurpose = "Cancel skip action"
                }
            };
        }
        else if (context.ManagementAction == "Purge")
        {
            // For purge, player can choose any combination of 3 tokens
            return new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "1",
                    NarrativeText = "Discard the letter (3 tokens)",
                    FocusCost = 0,
                    IsAffordable = _tokenManager.GetPlayerTokens().Values.Sum() >= 3,
                    ChoiceType = ConversationChoiceType.PurgeLetter,
                    TemplatePurpose = "Remove letter from queue"
                },
                new ConversationChoice
                {
                    ChoiceID = "2",
                    NarrativeText = "Keep the obligation",
                    FocusCost = 0,
                    IsAffordable = true,
                    ChoiceType = ConversationChoiceType.KeepLetter,
                    TemplatePurpose = "Cancel purge action"
                }
            };
        }
        
        return GenerateDefaultChoices(context, null);
    }
}

/// <summary>
/// Extended conversation context for action-based conversations
/// </summary>
public class ActionConversationContext : ConversationContext
{
    public ActionOption SourceAction { get; set; }
    public string InitialNarrative { get; set; }
    public List<ChoiceTemplate> AvailableTemplates { get; set; }
}