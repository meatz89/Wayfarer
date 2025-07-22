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
    // These services are needed to generate appropriate choices and narratives
    private readonly ConnectionTokenManager _tokenManager;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;
    private readonly LetterCategoryService _letterCategoryService;
    private readonly DeliveryConversationService _deliveryConversationService;
    private readonly GameWorld _gameWorld;
    private readonly DeterministicStreamingService _streamingService;
    
    public DeterministicNarrativeProvider(
        ConnectionTokenManager tokenManager,
        RouteDiscoveryManager routeDiscoveryManager,
        LetterCategoryService letterCategoryService,
        DeliveryConversationService deliveryConversationService,
        GameWorld gameWorld,
        DeterministicStreamingService streamingService)
    {
        _tokenManager = tokenManager;
        _routeDiscoveryManager = routeDiscoveryManager;
        _letterCategoryService = letterCategoryService;
        _deliveryConversationService = deliveryConversationService;
        _gameWorld = gameWorld;
        _streamingService = streamingService;
    }
    
    public async Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        string narrativeText;
        
        // Look for initial narrative in context
        if (context is ActionConversationContext actionContext && !string.IsNullOrEmpty(actionContext.InitialNarrative))
        {
            narrativeText = actionContext.InitialNarrative;
        }
        // Check if this is an action conversation with special narrative handling
        else if (context is ActionConversationContext actionCtx && actionCtx.SourceAction != null)
        {
            narrativeText = GetActionNarrative(actionCtx);
        }
        // Check if this is a queue management conversation
        else if (context is QueueManagementContext queueCtx)
        {
            narrativeText = GetQueueManagementIntroduction(queueCtx);
        }
        // Check if this is a travel conversation
        else if (context is TravelConversationContext travelCtx)
        {
            narrativeText = GetTravelIntroduction(travelCtx);
        }
        else
        {
            // All conversations must be properly categorized - no fallbacks
            throw new InvalidOperationException($"Unknown conversation context type: {context.GetType().Name}");
        }
        
        // Create watchers like AIGameMaster does
        var watchers = new List<IResponseStreamWatcher>
        {
            new StreamingContentStateWatcher(_gameWorld.StreamingContentState)
        };
        
        // Stream the text
        await _streamingService.StreamTextAsync(narrativeText, watchers);
        
        return narrativeText;
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
        
        // Check if this is a travel conversation
        if (context is TravelConversationContext travelContext)
        {
            var choices = GenerateTravelChoices(travelContext);
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
    
    public async Task<string> GenerateReaction(
        ConversationContext context,
        ConversationState state,
        ConversationChoice selectedChoice,
        bool success)
    {
        string reactionText;
        
        // Handle specific choice templates that need reactions
        if (selectedChoice.ChoiceType == ConversationChoiceType.AcceptLetterOffer)
        {
            var npc = context.TargetNPC;
            reactionText = $"{npc?.Name ?? "They"} smile and hand you a sealed letter. 'I knew I could count on you.'";
        }
        else if (selectedChoice.ChoiceType == ConversationChoiceType.DeclineLetterOffer)
        {
            var npc = context.TargetNPC;
            reactionText = $"{npc?.Name ?? "They"} nod understandingly. 'Perhaps another time then.'";
        }
        else if (selectedChoice.ChoiceType == ConversationChoiceType.Introduction)
        {
            var npc = context.TargetNPC;
            reactionText = $"{npc?.Name ?? "They"} seem pleased to make your acquaintance.";
        }
        else if (selectedChoice.ChoiceType == ConversationChoiceType.DiscoverRoute)
        {
            var npc = context.TargetNPC;
            reactionText = $"{npc?.Name ?? "They"} look around carefully. 'I do know a few paths... Let me tell you about them.'";
        }
        else
        {
            // For most thin narrative layer actions, no reaction needed
            reactionText = "";
        }
        
        // Only stream if there's text to stream
        if (!string.IsNullOrEmpty(reactionText))
        {
            // Create watchers like AIGameMaster does
            var watchers = new List<IResponseStreamWatcher>
            {
                new StreamingContentStateWatcher(_gameWorld.StreamingContentState)
            };
            
            // Stream the text
            await _streamingService.StreamTextAsync(reactionText, watchers);
        }
        
        return reactionText;
    }
    
    public async Task<string> GenerateConclusion(
        ConversationContext context,
        ConversationState state,
        ConversationChoice lastChoice)
    {
        // For thin narrative layer, no conclusion needed - action completes immediately
        // Return empty string without streaming
        return "";
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
            var player = context.Player;
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
        
        // No special introduction needed, use general action narrative
        return GetActionNarrative(context);
    }
    
    private string GetActionNarrative(ActionConversationContext context)
    {
        var action = context.SourceAction;
        var npc = context.TargetNPC;
        
        // Use categorical action enum, never string comparisons
        return action.Action switch
        {
            LocationAction.GatherResources => "You carefully search the area for edible berries.",
            LocationAction.Browse => "You examine the market stalls, noting prices and goods.",
            LocationAction.Observe => "You blend into the crowd, listening to local gossip.",
            LocationAction.Work => $"You begin your work for {npc?.Name ?? "your employer"}.",
            LocationAction.Socialize => $"You engage {npc?.Name ?? "someone"} in friendly conversation.",
            LocationAction.Converse => $"You approach {npc?.Name ?? "someone"} to talk.",
            LocationAction.Collect => $"{npc?.Name ?? "The sender"} hands you a sealed letter with instructions for safe delivery.",
            LocationAction.Trade => $"You browse {npc?.Name ?? "the merchant"}'s wares.",
            LocationAction.Rest => "You find a comfortable spot to rest.",
            LocationAction.Deliver => GetDeliveryIntroduction(context),
            LocationAction.RequestPatronFunds => $"You humbly request financial assistance from {npc?.Name ?? "your patron"}.",
            LocationAction.RequestPatronEquipment => $"You ask {npc?.Name ?? "your patron"} for equipment to help with your duties.",
            LocationAction.BorrowMoney => $"You negotiate a loan with {npc?.Name ?? "the lender"}.",
            LocationAction.PleedForAccess => $"You plead with {npc?.Name ?? "the gatekeeper"} to maintain your access.",
            LocationAction.AcceptIllegalWork => $"{npc?.Name ?? "A shadowy figure"} offers you questionable employment.",
            LocationAction.TravelEncounter => "You must deal with an unexpected situation on your journey.",
            _ => throw new InvalidOperationException($"Unknown action type: {action.Action}")
        };
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
        
        // Handle travel encounters
        if (action.Action == LocationAction.TravelEncounter)
        {
            return GetTravelEncounterChoices(context);
        }
        
        // For all other actions, return a simple continue choice
        return GenerateDefaultChoices(context, state: null);
    }
    
    private List<ConversationChoice> GetDeliveryChoices(ConversationContext context)
    {
        var player = context.Player;
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
    
    private string GetTravelIntroduction(TravelConversationContext context)
    {
        var origin = context.Origin?.Name ?? "here";
        var destination = context.Destination?.Name ?? "your destination";
        
        // Generate narrative based on encounter type
        return context.EncounterType switch
        {
            TravelEncounterType.WildernessObstacle => $"The path to {destination} is blocked by a fallen tree. You'll need to find a way around or through.",
            TravelEncounterType.DarkChallenge => $"As you travel toward {destination}, darkness falls suddenly. The path ahead is barely visible.",
            TravelEncounterType.WeatherEvent => $"A sudden storm catches you on the road to {destination}. Rain begins to fall heavily.",
            TravelEncounterType.MerchantEncounter => $"You encounter a merchant whose cart has broken down on the road to {destination}. They look like they could use help.",
            TravelEncounterType.FellowTraveler => $"You meet a fellow traveler heading toward {destination}. They wave in greeting.",
            _ => $"You encounter an unexpected situation on your journey from {origin} to {destination}."
        };
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
    
    private List<ConversationChoice> GenerateTravelChoices(TravelConversationContext context)
    {
        var choices = new List<ConversationChoice>();
        var player = context.Player;
        
        // Get player equipment using proper categories
        var hasClimbingGear = HasEquipmentCategory(player, ItemCategory.Climbing_Equipment);
        var hasLightSource = HasEquipmentCategory(player, ItemCategory.Light_Source);
        var hasWeatherProtection = HasEquipmentCategory(player, ItemCategory.Weather_Protection);
        var hasCart = HasEquipmentCategory(player, ItemCategory.Load_Distribution);
        
        // Always add a default cautious option
        choices.Add(new ConversationChoice
        {
            ChoiceID = "1",
            NarrativeText = "Proceed with caution",
            FocusCost = 0,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.TravelCautious,
            TemplatePurpose = "Take the safe but slow approach",
            TravelEffect = TravelChoiceEffect.None
        });
        
        // Add equipment-based choices based on encounter type
        switch (context.EncounterType)
        {
            case TravelEncounterType.WildernessObstacle:
                if (hasClimbingGear)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Use climbing gear to go over",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelUseEquipment,
                        TemplatePurpose = "Save time with equipment",
                        TravelEffect = TravelChoiceEffect.SaveTime,
                        RequiredEquipment = EquipmentType.ClimbingGear,
                        TimeModifierMinutes = -30
                    });
                }
                
                // Always offer a risky option
                if (player.Stamina >= 2)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = choices.Count + 1 + "",
                        NarrativeText = "Force through the obstacle",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelForceThrough,
                        TemplatePurpose = "Spend stamina to save time",
                        TravelEffect = TravelChoiceEffect.SpendStamina,
                        StaminaCost = 2,
                        TimeModifierMinutes = -30
                    });
                }
                break;
                
            case TravelEncounterType.DarkChallenge:
                if (hasLightSource)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Light torch and continue safely",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelUseEquipment,
                        TemplatePurpose = "Navigate safely with light",
                        TravelEffect = TravelChoiceEffect.None,
                        RequiredEquipment = EquipmentType.LightSource
                    });
                }
                else
                {
                    // Risky option without light
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Stumble forward in darkness",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelSlowProgress,
                        TemplatePurpose = "Continue slowly without light",
                        TravelEffect = TravelChoiceEffect.LoseTime,
                        TimeModifierMinutes = 60
                    });
                }
                break;
                
            case TravelEncounterType.WeatherEvent:
                if (hasWeatherProtection)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Use weather gear and press on",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelUseEquipment,
                        TemplatePurpose = "Continue protected",
                        TravelEffect = TravelChoiceEffect.None,
                        RequiredEquipment = EquipmentType.WeatherProtection
                    });
                }
                else
                {
                    // Risk getting wet
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Get soaked but keep moving",
                        FocusCost = 0,
                        IsAffordable = player.Stamina >= 1,
                        ChoiceType = ConversationChoiceType.TravelForceThrough,
                        TemplatePurpose = "Continue unprotected",
                        TravelEffect = TravelChoiceEffect.SpendStamina,
                        StaminaCost = 1
                    });
                }
                break;
                
            case TravelEncounterType.MerchantEncounter:
                if (hasCart)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Offer cart space for a ride",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelTradeHelp,
                        TemplatePurpose = "Trade help for faster travel",
                        TravelEffect = TravelChoiceEffect.SaveTime,
                        RequiredEquipment = EquipmentType.LoadDistribution,
                        TimeModifierMinutes = -60
                    });
                }
                
                // Always offer to help on foot
                choices.Add(new ConversationChoice
                {
                    ChoiceID = choices.Count + 1 + "",
                    NarrativeText = "Help push their cart",
                    FocusCost = 0,
                    IsAffordable = player.Stamina >= 1,
                    ChoiceType = ConversationChoiceType.TravelTradeHelp,
                    TemplatePurpose = "Earn coins helping",
                    TravelEffect = TravelChoiceEffect.EarnCoins,
                    StaminaCost = 1,
                    CoinReward = 3
                });
                break;
                
            case TravelEncounterType.FellowTraveler:
                // Information exchange
                choices.Add(new ConversationChoice
                {
                    ChoiceID = "2",
                    NarrativeText = "Exchange travel information",
                    FocusCost = 0,
                    IsAffordable = true,
                    ChoiceType = ConversationChoiceType.TravelExchangeInfo,
                    TemplatePurpose = "Learn about route conditions",
                    TravelEffect = TravelChoiceEffect.GainInformation
                });
                break;
        }
        
        return choices;
    }
    
    private List<ConversationChoice> GetTravelEncounterChoices(ActionConversationContext context)
    {
        var choices = new List<ConversationChoice>();
        var player = context.Player;
        
        // Get the encounter type from the action
        var encounterType = context.SourceAction.EncounterType ?? TravelEncounterType.FellowTraveler;
        
        // Get player equipment using proper categories
        var hasClimbingGear = HasEquipmentCategory(player, ItemCategory.Climbing_Equipment);
        var hasLightSource = HasEquipmentCategory(player, ItemCategory.Light_Source);
        var hasWeatherProtection = HasEquipmentCategory(player, ItemCategory.Weather_Protection);
        var hasCart = HasEquipmentCategory(player, ItemCategory.Load_Distribution);
        
        // Always add a default cautious option
        choices.Add(new ConversationChoice
        {
            ChoiceID = "1",
            NarrativeText = "Proceed with caution",
            FocusCost = 0,
            IsAffordable = true,
            ChoiceType = ConversationChoiceType.TravelCautious,
            TemplatePurpose = "Take the safe but slow approach",
            TravelEffect = TravelChoiceEffect.None
        });
        
        // Add equipment-based choices based on encounter type
        switch (encounterType)
        {
            case TravelEncounterType.WildernessObstacle:
                if (hasClimbingGear)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Use climbing gear to go over",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelUseEquipment,
                        TemplatePurpose = "Save time with equipment",
                        TravelEffect = TravelChoiceEffect.SaveTime,
                        RequiredEquipment = EquipmentType.ClimbingGear,
                        TimeModifierMinutes = -30
                    });
                }
                
                // Always offer a risky option
                if (player.Stamina >= 2)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = choices.Count + 1 + "",
                        NarrativeText = "Force through the obstacle",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelForceThrough,
                        TemplatePurpose = "Spend stamina to save time",
                        TravelEffect = TravelChoiceEffect.SpendStamina,
                        StaminaCost = 2,
                        TimeModifierMinutes = -30
                    });
                }
                break;
                
            case TravelEncounterType.DarkChallenge:
                if (hasLightSource)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Light torch and continue safely",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelUseEquipment,
                        TemplatePurpose = "Navigate safely with light",
                        TravelEffect = TravelChoiceEffect.None,
                        RequiredEquipment = EquipmentType.LightSource
                    });
                }
                else
                {
                    // Risky option without light
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Stumble forward in darkness",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelSlowProgress,
                        TemplatePurpose = "Continue slowly without light",
                        TravelEffect = TravelChoiceEffect.LoseTime,
                        TimeModifierMinutes = 60
                    });
                }
                break;
                
            case TravelEncounterType.WeatherEvent:
                if (hasWeatherProtection)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Use weather gear and press on",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelUseEquipment,
                        TemplatePurpose = "Continue protected",
                        TravelEffect = TravelChoiceEffect.None,
                        RequiredEquipment = EquipmentType.WeatherProtection
                    });
                }
                else
                {
                    // Risk getting wet
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Get soaked but keep moving",
                        FocusCost = 0,
                        IsAffordable = player.Stamina >= 1,
                        ChoiceType = ConversationChoiceType.TravelForceThrough,
                        TemplatePurpose = "Continue unprotected",
                        TravelEffect = TravelChoiceEffect.SpendStamina,
                        StaminaCost = 1
                    });
                }
                break;
                
            case TravelEncounterType.MerchantEncounter:
                if (hasCart)
                {
                    choices.Add(new ConversationChoice
                    {
                        ChoiceID = "2",
                        NarrativeText = "Offer cart space for a ride",
                        FocusCost = 0,
                        IsAffordable = true,
                        ChoiceType = ConversationChoiceType.TravelTradeHelp,
                        TemplatePurpose = "Trade help for faster travel",
                        TravelEffect = TravelChoiceEffect.SaveTime,
                        RequiredEquipment = EquipmentType.LoadDistribution,
                        TimeModifierMinutes = -60
                    });
                }
                
                // Always offer to help on foot
                choices.Add(new ConversationChoice
                {
                    ChoiceID = choices.Count + 1 + "",
                    NarrativeText = "Help push their cart",
                    FocusCost = 0,
                    IsAffordable = player.Stamina >= 1,
                    ChoiceType = ConversationChoiceType.TravelTradeHelp,
                    TemplatePurpose = "Earn coins helping",
                    TravelEffect = TravelChoiceEffect.EarnCoins,
                    StaminaCost = 1,
                    CoinReward = 3
                });
                break;
                
            case TravelEncounterType.FellowTraveler:
                // Information exchange
                choices.Add(new ConversationChoice
                {
                    ChoiceID = "2",
                    NarrativeText = "Exchange travel information",
                    FocusCost = 0,
                    IsAffordable = true,
                    ChoiceType = ConversationChoiceType.TravelExchangeInfo,
                    TemplatePurpose = "Learn about route conditions",
                    TravelEffect = TravelChoiceEffect.GainInformation
                });
                break;
        }
        
        return choices;
    }
    
    
    /// <summary>
    /// Check if player has equipment matching a category
    /// </summary>
    private bool HasEquipmentCategory(Player player, ItemCategory category)
    {
        // Check player's inventory for items with the specified category
        var itemRepository = new ItemRepository(_gameWorld);
        foreach (var itemId in player.Inventory.ItemSlots.Where(id => !string.IsNullOrEmpty(id)))
        {
            var item = itemRepository.GetItemById(itemId);
            if (item != null && item.Categories.Contains(category))
            {
                return true;
            }
        }
        return false;
    }
}

/// <summary>
/// Extended conversation context for action-based conversations
/// </summary>