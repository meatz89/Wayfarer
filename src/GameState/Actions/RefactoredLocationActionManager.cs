using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Actions;
using Wayfarer.GameState.Actions.Executors;

namespace Wayfarer.GameState;

/// <summary>
/// Refactored location action manager using the new unified action system
/// </summary>
public class RefactoredLocationActionManager
{
    private readonly GameWorld _gameWorld;
    private readonly ActionDiscovery _actionDiscovery;
    private readonly UnifiedActionExecutor _actionExecutor;
    private readonly MessageSystem _messageSystem;
    private readonly NarrativeManager _narrativeManager;
    private readonly FlagService _flagService;
    private readonly NPCLetterOfferService _letterOfferService;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly LetterQueueManager _letterQueueManager;
    
    public RefactoredLocationActionManager(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ConnectionTokenManager tokenManager,
        LetterQueueManager letterQueueManager,
        NPCRepository npcRepository,
        ItemRepository itemRepository,
        ConversationFactory conversationFactory,
        RouteDiscoveryManager routeDiscoveryManager,
        NPCLetterOfferService letterOfferService,
        LetterCategoryService letterCategoryService,
        NoticeBoardService noticeBoardService,
        DebugLogger debugLogger,
        FlagService flagService,
        NarrativeManager narrativeManager,
        IGameRuleEngine ruleEngine,
        GameConfiguration config)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _narrativeManager = narrativeManager;
        _flagService = flagService;
        _letterOfferService = letterOfferService;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _letterQueueManager = letterQueueManager;
        
        // Create prerequisite factory with all required dependencies
        var prerequisiteFactory = new ActionPrerequisiteFactory(
            ruleEngine, 
            narrativeManager,
            npcRepository,
            itemRepository,
            tokenManager,
            flagService
        );
        
        // Create action discovery
        _actionDiscovery = new ActionDiscovery(
            gameWorld,
            npcRepository,
            tokenManager,
            ruleEngine,
            prerequisiteFactory
        );
        
        // Create executors
        var conversationExecutor = new ConversationActionExecutor(
            conversationFactory,
            npcRepository,
            narrativeManager,
            letterOfferService
        );
        
        var directExecutor = new DirectActionExecutor(
            messageSystem,
            tokenManager,
            npcRepository,
            itemRepository,
            config
        );
        
        // Create unified executor
        _actionExecutor = new UnifiedActionExecutor(
            ruleEngine,
            messageSystem,
            prerequisiteFactory,
            new IActionExecutor[] { conversationExecutor, directExecutor }
        );
    }
    
    /// <summary>
    /// Get all actions (available and locked) at current location
    /// </summary>
    public ActionDiscoveryResult DiscoverAllActions()
    {
        var player = _gameWorld.GetPlayer();
        var spot = player.CurrentLocationSpot;
        
        if (spot == null)
        {
            return new ActionDiscoveryResult();
        }
        
        var result = _actionDiscovery.DiscoverActions(spot, player);
        
        // Apply narrative filtering if active
        if (_narrativeManager.HasActiveNarrative())
        {
            var allowedActions = _narrativeManager.FilterActions(
                result.AvailableActions.Select(a => a.Action).ToList()
            );
            
            result.AvailableActions = result.AvailableActions
                .Where(a => allowedActions.Contains(a.Action))
                .ToList();
        }
        
        return result;
    }
    
    /// <summary>
    /// Get only available actions (backward compatibility)
    /// </summary>
    public List<ActionOption> GetAvailableActions()
    {
        var discovery = DiscoverAllActions();
        return discovery.AvailableActions;
    }
    
    /// <summary>
    /// Execute an action with full validation and feedback
    /// </summary>
    public async Task<bool> ExecuteAction(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        try
        {
            // Execute through unified executor
            var result = await _actionExecutor.Execute(option, player, _gameWorld);
            
            if (!result.Success)
            {
                return false;
            }
            
            // Handle conversation requirement
            if (result.RequiresConversation)
            {
                _gameWorld.PendingAction = option;
                _gameWorld.ConversationPending = true;
                _gameWorld.PendingConversationManager = result.ConversationManager;
                return true;
            }
            
            // Apply effects
            foreach (var effect in result.Effects)
            {
                ApplyEffect(effect, player);
            }
            
            // Report to narrative manager
            if (_narrativeManager.HasActiveNarrative())
            {
                _narrativeManager.OnActionCompleted(option.Action, option.NPCId);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Action failed: {ex.Message}",
                SystemMessageTypes.Danger
            );
            return false;
        }
    }
    
    /// <summary>
    /// Complete action after conversation (for conversation-based actions)
    /// </summary>
    public bool CompleteActionAfterConversation(ActionOption option, ConversationChoice selectedChoice = null)
    {
        var player = _gameWorld.GetPlayer();
        
        // Apply resource costs (already validated)
        _gameWorld.TimeManager.SpendHours(option.HourCost);
        player.ModifyStamina(-option.StaminaCost);
        player.ModifyCoins(-option.CoinCost);
        
        // Handle conversation outcomes
        HandleConversationOutcome(option, selectedChoice);
        
        // Report to narrative manager
        if (_narrativeManager.HasActiveNarrative())
        {
            _narrativeManager.OnActionCompleted(option.Action, option.NPCId);
        }
        
        return true;
    }
    
    private void ApplyEffect(ActionEffect effect, Player player)
    {
        switch (effect.Type)
        {
            case "coins":
                player.ModifyCoins(effect.Amount);
                break;
                
            case "stamina":
                player.ModifyStamina(effect.Amount);
                break;
                
            case "tokens":
                // Token effects handled by executors
                break;
                
            case "items":
                // Item effects would be handled by inventory system
                break;
        }
    }
    
    private void HandleConversationOutcome(ActionOption option, ConversationChoice choice)
    {
        if (choice == null) return;
        
        // Letter offer acceptance
        if (choice.ChoiceType == ConversationChoiceType.AcceptLetterOffer)
        {
            HandleLetterOfferAcceptance(option, choice);
        }
        // Introduction
        else if (choice.ChoiceType == ConversationChoiceType.Introduction)
        {
            HandleIntroduction(option, choice);
        }
        // Delivery outcome
        else if (choice.DeliveryOutcome != null)
        {
            HandleDeliveryOutcome(option, choice);
        }
        // Travel encounter
        else if (option.Action == LocationAction.TravelEncounter)
        {
            HandleTravelEncounterOutcome(option, choice);
        }
    }
    
    private void HandleLetterOfferAcceptance(ActionOption option, ConversationChoice choice)
    {
        if (choice.OfferTokenType.HasValue && choice.OfferCategory.HasValue)
        {
            var offers = _letterOfferService.GenerateNPCLetterOffers(option.NPCId);
            
            var matchingOffer = offers.FirstOrDefault(o => 
                o.LetterType == choice.OfferTokenType.Value && 
                o.Category == choice.OfferCategory.Value);
            
            if (matchingOffer != null)
            {
                var success = _letterOfferService.AcceptNPCLetterOffer(option.NPCId, matchingOffer.Id);
                
                if (!success)
                {
                    _messageSystem.AddSystemMessage(
                        "❌ Your letter queue is full! Make room before accepting more letters.",
                        SystemMessageTypes.Danger
                    );
                }
                else
                {
                    // Track first letter accepted
                    if (!_flagService.GetFlag(FlagService.FIRST_LETTER_ACCEPTED))
                    {
                        _flagService.SetFlag(FlagService.FIRST_LETTER_ACCEPTED, true);
                    }
                }
            }
        }
    }
    
    private void HandleIntroduction(ActionOption option, ConversationChoice choice)
    {
        var npc = _npcRepository.GetNPCById(option.NPCId);
        
        if (npc != null)
        {
            _tokenManager.AddTokensToNPC(npc.LetterTokenTypes.FirstOrDefault(), 1, option.NPCId);
            
            _messageSystem.AddSystemMessage(
                $"👋 You introduce yourself to {npc.Name}.",
                SystemMessageTypes.Success
            );
            
            // Track first conversation
            if (!_flagService.GetFlag(FlagService.FIRST_CONVERSATION))
            {
                _flagService.SetFlag(FlagService.FIRST_CONVERSATION, true);
            }
        }
    }
    
    private void HandleDeliveryOutcome(ActionOption option, ConversationChoice choice)
    {
        var outcome = choice.DeliveryOutcome;
        var player = _gameWorld.GetPlayer();
        var letter = player.LetterQueue[0];
        
        if (letter == null || outcome == null) return;
        
        // Remove letter and process delivery
        player.CarriedLetters.Remove(letter);
        player.Inventory.RemoveItem($"letter_{letter.Id}");
        _letterQueueManager.RemoveLetterFromQueue(1);
        _letterQueueManager.RecordLetterDelivery(letter);
        
        // Apply payment
        int totalPayment = outcome.BasePayment + outcome.BonusPayment;
        if (outcome.ChanceForTip > 0 && new Random().NextDouble() < outcome.ChanceForTip)
        {
            totalPayment += outcome.TipAmount;
        }
        player.ModifyCoins(totalPayment);
        
        // Apply token effects
        if (outcome.TokenReward && outcome.TokenAmount > 0)
        {
            _tokenManager.AddTokensToNPC(outcome.TokenType, outcome.TokenAmount, option.NPCId);
        }
        
        // Track first delivery
        if (!_flagService.GetFlag(FlagService.FIRST_LETTER_DELIVERED))
        {
            _flagService.SetFlag(FlagService.FIRST_LETTER_DELIVERED, true);
        }
    }
    
    private void HandleTravelEncounterOutcome(ActionOption option, ConversationChoice choice)
    {
        var player = _gameWorld.GetPlayer();
        
        // Apply choice effects
        if (choice.StaminaCost > 0)
        {
            player.SpendStamina(choice.StaminaCost.Value);
        }
        
        if (choice.CoinReward > 0)
        {
            player.ModifyCoins(choice.CoinReward.Value);
        }
        
        // Time modifiers handled by travel system
    }
}