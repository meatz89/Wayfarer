using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Core location actions that consume hours
/// Each action is independent and has its own purpose
/// </summary>
public enum LocationAction
{
    // Basic Actions (1 hour each)
    Converse,       // Talk with someone
    Work,           // Earn coins through labor
    Socialize,      // Build relationship with NPC
    Rest,           // Recover stamina
    Collect,        // Get physical letter from sender
    Deliver,        // Complete letter delivery
    Trade,          // Buy or sell items
    
    // Environmental Actions (1 hour each)
    GatherResources,    // Gather berries, herbs, etc. based on location
    Browse,             // Browse market stalls or notice boards
    Observe,            // Listen to gossip or watch for opportunities
    
    // Debt Actions (1 hour each)
    RequestPatronFunds,     // Request money from patron (-1 token)
    RequestPatronEquipment, // Request equipment from patron (-2 tokens)
    BorrowMoney,           // Borrow from NPC (-2 tokens)
    PleedForAccess,        // Maintain route access (-1 token)
    AcceptIllegalWork,     // Accept shadow work (-1 token)
}

/// <summary>
/// Manages location-based actions that consume hours and other resources.
/// Each action has a clear resource cost and benefit.
/// </summary>
public class LocationActionManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;
    
    private TimeManager _timeManager => _gameWorld.TimeManager;
    
    public LocationActionManager(
        GameWorld gameWorld, 
        MessageSystem messageSystem,
        ConnectionTokenManager tokenManager,
        LetterQueueManager letterQueueManager,
        NPCRepository npcRepository,
        ItemRepository itemRepository)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
        _tokenManager = tokenManager;
        _letterQueueManager = letterQueueManager;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
    }
    
    
    /// <summary>
    /// Get available actions at current location
    /// </summary>
    public List<ActionOption> GetAvailableActions()
    {
        var player = _gameWorld.GetPlayer();
        var location = player.CurrentLocation;
        var spot = player.CurrentLocationSpot;
        var actions = new List<ActionOption>();
        
        if (location == null || spot == null) return actions;
        
        // Get NPCs at current location
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        var npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTimeBlock);
        
        // NOTE: Basic rest is handled in RestUI screen via RestManager
        // Only add NPC-specific contextual actions here
        
        // ALL actions must come from NPCs present at the spot
        foreach (var npc in npcsHere)
        {
            // Check NPC availability in current time period
            if (!IsNPCAvailable(npc)) continue;
            
            // Add basic NPC interactions
            AddBasicNPCActions(npc, actions);
            
            // Add profession-specific actions
            AddProfessionSpecificActions(npc, actions, npcsHere);
            
            // Add letter-related actions
            AddLetterActions(npc, actions);
            
            // Add debt/emergency actions specific to this NPC
            AddNPCEmergencyActions(npc, actions);
        }
        
        // Environmental actions based on location spot properties
        AddEnvironmentalActions(actions, spot);
        
        // Patron request actions - available at writing locations
        AddPatronRequestActions(actions);
        
        return actions;
    }
    
    /// <summary>
    /// Add basic NPC interaction actions (converse, socialize)
    /// </summary>
    private void AddBasicNPCActions(NPC npc, List<ActionOption> actions)
    {
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var totalTokens = tokens.Values.Sum();
        
        // Basic conversation - always available when NPC is present
        if (_timeManager.HoursRemaining >= 1)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Converse,
                Name = $"Talk with {npc.Name}",
                Description = npc.Description,
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = totalTokens == 0 ? "Meet someone new" : "Catch up"
            });
        }
        
        // Socialize - spend quality time if relationship exists
        if (_timeManager.HoursRemaining >= 1 && totalTokens > 0)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Socialize,
                Name = $"Spend time with {npc.Name}",
                Description = "Deepen your connection",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = $"+1 {npc.LetterTokenTypes.FirstOrDefault()} token"
            });
        }
    }
    
    /// <summary>
    /// Add profession-specific actions for an NPC
    /// </summary>
    private void AddProfessionSpecificActions(NPC npc, List<ActionOption> actions, List<NPC> allNPCsPresent)
    {
        var player = _gameWorld.GetPlayer();
        var currentTime = _timeManager.GetCurrentTimeBlock();
        
        // Work actions are available even without existing relationship!
        switch (npc.Profession)
        {
            case Professions.Merchant:
                // Merchants offer work during business hours
                if (_timeManager.HoursRemaining >= 1 && 
                    player.Stamina >= GameRules.STAMINA_COST_WORK &&
                    (currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon))
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Help {npc.Name} with inventory",
                        Description = "Earn coins through honest labor",
                        HourCost = 1,
                        StaminaCost = GameRules.STAMINA_COST_WORK,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+4 coins"
                    });
                }
                
                // Trade option
                if (_timeManager.HoursRemaining >= 1)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Trade,
                        Name = $"Trade with {npc.Name}",
                        Description = "Buy or sell goods",
                        HourCost = 1,
                        StaminaCost = 0,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "Access market prices"
                    });
                }
                break;
                
            case Professions.TavernKeeper:
                // Tavern keepers offer work in evenings
                if (_timeManager.HoursRemaining >= 1 && 
                    player.Stamina >= 2 &&
                    currentTime == TimeBlocks.Evening)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Serve drinks for {npc.Name}",
                        Description = "Help with the evening rush",
                        HourCost = 1,
                        StaminaCost = 2,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+4 coins"
                    });
                }
                
                // Drink and rest option (if you have coins)
                if (_timeManager.HoursRemaining >= 1 && player.Coins >= 3)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Rest,
                        Name = $"Buy drinks and rest with {npc.Name}",
                        Description = "Rest while building connection",
                        HourCost = 1,
                        StaminaCost = 0,
                        CoinCost = 3,
                        NPCId = npc.ID,
                        Effect = $"+3 Stamina, +1 {npc.LetterTokenTypes.FirstOrDefault()} token"
                    });
                }
                break;
                
            case Professions.Scribe:
                // Scribes offer copying work
                if (_timeManager.HoursRemaining >= 1 && player.Stamina >= 1)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Copy documents for {npc.Name}",
                        Description = "Careful work with quill and ink",
                        HourCost = 1,
                        StaminaCost = 1,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+3 coins"
                    });
                }
                break;
                
            case Professions.Noble:
                // Nobles don't offer work directly, but may have other opportunities later
                break;
                
            case Professions.Innkeeper:
                // Innkeepers offer room cleaning work
                if (_timeManager.HoursRemaining >= 1 && 
                    player.Stamina >= 2 &&
                    currentTime == TimeBlocks.Morning)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.Work,
                        Name = $"Clean rooms for {npc.Name}",
                        Description = "Morning cleaning duties",
                        HourCost = 1,
                        StaminaCost = 2,
                        CoinCost = 0,
                        NPCId = npc.ID,
                        Effect = "+4 coins"
                    });
                }
                break;
        }
        
        // Special case: Baker work at dawn (if this is a merchant baker)
        if (npc.Profession == Professions.Merchant && 
            npc.Name.ToLower().Contains("baker") &&
            currentTime == TimeBlocks.Dawn &&
            player.Stamina >= 1 &&
            _timeManager.HoursRemaining >= 1)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Work,
                Name = $"Help {npc.Name} with morning baking",
                Description = "Early work with benefits",
                HourCost = 1,
                StaminaCost = 1,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "+2 coins, +1 bread (restores 2 stamina when eaten)"
            });
        }
    }
    
    /// <summary>
    /// Add letter-related actions for an NPC
    /// </summary>
    private void AddLetterActions(NPC npc, List<ActionOption> actions)
    {
        // Deliver - if this NPC is a letter recipient
        if (_timeManager.HoursRemaining >= 1 && IsLetterRecipient(npc.ID))
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Deliver,
                Name = $"Deliver letter to {npc.Name}",
                Description = "Complete your obligation",
                HourCost = 1,
                StaminaCost = 1,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "Earn payment and tokens"
            });
        }
        
        // Collect - if this NPC has letters to give
        var lettersFromNPC = GetAcceptedLettersFromNPC(npc.ID);
        foreach (var letter in lettersFromNPC)
        {
            if (_timeManager.HoursRemaining >= 1)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Collect,
                    Name = $"Collect letter from {npc.Name}",
                    Description = $"Get the physical letter",
                    HourCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    NPCId = npc.ID,
                    LetterId = letter.Id,
                    Effect = $"Uses {letter.GetRequiredSlots()} inventory slots"
                });
            }
        }
    }
    
    /// <summary>
    /// Add emergency actions specific to an NPC
    /// </summary>
    private void AddNPCEmergencyActions(NPC npc, List<ActionOption> actions)
    {
        var player = _gameWorld.GetPlayer();
        
        if (_timeManager.HoursRemaining < 1) return;
        
        // Borrow money from merchants when broke
        if (player.Coins < 10 && npc.Profession == Professions.Merchant)
        {
            var tradeTokens = _tokenManager.GetTokensWithNPC(npc.ID)[ConnectionType.Trade];
            actions.Add(new ActionOption
            {
                Action = LocationAction.BorrowMoney,
                Name = $"Borrow money from {npc.Name}",
                Description = $"20 coins now, -2 Trade leverage (current: {tradeTokens})",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "20 coins, -2 Trade tokens"
            });
        }
        
        // Accept illegal work from shadow NPCs
        if (npc.LetterTokenTypes.Contains(ConnectionType.Shadow) && player.Coins < 15)
        {
            var shadowTokens = _tokenManager.GetTokensWithNPC(npc.ID)[ConnectionType.Shadow];
            actions.Add(new ActionOption
            {
                Action = LocationAction.AcceptIllegalWork,
                Name = $"Accept questionable job from {npc.Name}",
                Description = $"30 coins, but they'll have leverage (current: {shadowTokens})",
                HourCost = 1,
                StaminaCost = 1,
                CoinCost = 0,
                NPCId = npc.ID,
                Effect = "30 coins, -1 Shadow token (they have dirt on you)"
            });
        }
    }
    
    /// <summary>
    /// Execute an action
    /// </summary>
    public bool ExecuteAction(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        // Validate resources
        if (!_timeManager.CanPerformAction(option.HourCost))
        {
            _messageSystem.AddSystemMessage(
                $"❌ Not enough hours remaining! Need {option.HourCost}, have {_timeManager.HoursRemaining}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        if (player.Stamina < option.StaminaCost)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Not enough stamina! Need {option.StaminaCost}, have {player.Stamina}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        if (player.Coins < option.CoinCost)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Not enough coins! Need {option.CoinCost}, have {player.Coins}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Spend resources
        _timeManager.SpendHours(option.HourCost);
        player.ModifyStamina(-option.StaminaCost);
        player.ModifyCoins(-option.CoinCost);
        
        // Execute action
        switch (option.Action)
        {
            case LocationAction.Rest:
                return ExecuteRest(option);
                
            case LocationAction.Converse:
                return ExecuteConverse(option.NPCId);
                
            case LocationAction.Socialize:
                return ExecuteSocialize(option.NPCId);
                
            case LocationAction.Work:
                return ExecuteWork(option);
                
            case LocationAction.Collect:
                return ExecuteCollect(option.NPCId, option.LetterId);
                
            case LocationAction.Deliver:
                return ExecuteDeliver(option.NPCId, option.LetterId);
                
            case LocationAction.Trade:
                return ExecuteTrade(option.NPCId);
                
            case LocationAction.GatherResources:
                return ExecuteGatherResources(option);
                
            case LocationAction.Browse:
                return ExecuteBrowse(option);
                
            case LocationAction.Observe:
                return ExecuteObserve(option);
                
            case LocationAction.RequestPatronFunds:
                return ExecuteRequestPatronFunds(option);
                
            case LocationAction.RequestPatronEquipment:
                return ExecuteRequestPatronEquipment(option);
                
            case LocationAction.BorrowMoney:
                return ExecuteBorrowMoney(option);
                
            case LocationAction.PleedForAccess:
                return ExecutePleedForAccess(option);
                
            case LocationAction.AcceptIllegalWork:
                return ExecuteAcceptIllegalWork(option);
                
            default:
                return false;
        }
    }
    
    private bool ExecuteRest(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        player.ModifyStamina(GameRules.STAMINA_RECOVERY_REST);
        
        // Check if this is the special "buy drinks" rest with specific NPC
        if (option.CoinCost > 0 && !string.IsNullOrEmpty(option.NPCId))
        {
            var npc = _npcRepository.GetNPCById(option.NPCId);
            if (npc != null)
            {
                _messageSystem.AddSystemMessage(
                    $"🍺 You buy drinks and share stories with {npc.Name}.",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    $"  • Stamina restored: {player.Stamina}/10",
                    SystemMessageTypes.Info
                );
                
                // Add token with the specific NPC
                _tokenManager.AddTokensToNPC(ConnectionType.Common, 1, option.NPCId);
                _messageSystem.AddSystemMessage(
                    $"  • Your bond with {npc.Name} strengthens over drinks",
                    SystemMessageTypes.Success
                );
            }
        }
        else
        {
            _messageSystem.AddSystemMessage(
                "💤 You rest for an hour and recover some energy.",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • Stamina restored: {player.Stamina}/10",
                SystemMessageTypes.Info
            );
        }
        
        return true;
    }
    
    private bool ExecuteConverse(string npcId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null) return false;
        
        var tokens = _tokenManager.GetTokensWithNPC(npcId);
        var totalTokens = tokens.Values.Sum();
        
        if (totalTokens == 0)
        {
            // First meeting
            _messageSystem.AddSystemMessage(
                $"👋 You introduce yourself to {npc.Name}.",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • \"{npc.Description}\"",
                SystemMessageTypes.Info
            );
            _messageSystem.AddSystemMessage(
                $"  • They seem to work in {npc.LetterTokenTypes.FirstOrDefault()} circles",
                SystemMessageTypes.Info
            );
            
            // Grant first token to establish connection
            _tokenManager.AddTokensToNPC(npc.LetterTokenTypes.FirstOrDefault(), 1, npcId);
            
            _messageSystem.AddSystemMessage(
                $"  • Gained +1 {npc.LetterTokenTypes.FirstOrDefault()} token with {npc.Name}",
                SystemMessageTypes.Success
            );
        }
        else
        {
            // Ongoing relationship
            _messageSystem.AddSystemMessage(
                $"💬 You spend time talking with {npc.Name}.",
                SystemMessageTypes.Success
            );
            
            // May reveal letter opportunities at 3+ tokens
            if (totalTokens >= 3)
            {
                _messageSystem.AddSystemMessage(
                    $"  • {npc.Name} mentions they might have some letters that need delivering...",
                    SystemMessageTypes.Info
                );
            }
        }
        
        return true;
    }
    
    private bool ExecuteSocialize(string npcId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null) return false;
        
        var tokenType = npc.LetterTokenTypes.FirstOrDefault();
        _tokenManager.AddTokensToNPC(tokenType, 1, npcId);
        
        _messageSystem.AddSystemMessage(
            $"🤝 You spend quality time with {npc.Name}.",
            SystemMessageTypes.Success
        );
        _messageSystem.AddSystemMessage(
            $"  • Your bond strengthens: +1 {tokenType} token",
            SystemMessageTypes.Success
        );
        
        var tokens = _tokenManager.GetTokensWithNPC(npcId);
        var totalTokens = tokens.Values.Sum();
        
        _messageSystem.AddSystemMessage(
            $"  • Total relationship: {totalTokens} tokens",
            SystemMessageTypes.Info
        );
        
        return true;
    }
    
    private bool ExecuteWork(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        var npc = _npcRepository.GetNPCById(option.NPCId);
        
        if (npc == null) 
        {
            _messageSystem.AddSystemMessage(
                "❌ Cannot find the NPC to work for!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Different work types based on NPC profession and context
        string workDescription = "";
        int coinsEarned = 0;
        
        // Special case: Baker work at dawn
        if (npc.Name.ToLower().Contains("baker") && option.StaminaCost == 1)
        {
            // Baker work: 1 stamina → 2 coins + bread
            coinsEarned = 2;
            player.ModifyCoins(coinsEarned);
            
            workDescription = $"🥖 You help {npc.Name} with the morning baking.";
            
            // Add bread to inventory if space available
            var breadItem = _itemRepository.GetItemById("bread");
            if (breadItem != null && player.Inventory.CanAddItem(breadItem, _itemRepository))
            {
                player.Inventory.AddItem(breadItem.Id);
                _messageSystem.AddSystemMessage(workDescription, SystemMessageTypes.Success);
                _messageSystem.AddSystemMessage($"  • Earned: {coinsEarned} coins", SystemMessageTypes.Success);
                _messageSystem.AddSystemMessage("  • Received: 1 bread (restores 2 stamina when eaten)", SystemMessageTypes.Success);
            }
            else
            {
                _messageSystem.AddSystemMessage(workDescription, SystemMessageTypes.Success);
                _messageSystem.AddSystemMessage($"  • Earned: {coinsEarned} coins", SystemMessageTypes.Success);
                if (breadItem == null)
                {
                    _messageSystem.AddSystemMessage("  • (Bread item not found in game data)", SystemMessageTypes.Warning);
                }
                else
                {
                    _messageSystem.AddSystemMessage("  • No room for bread in inventory!", SystemMessageTypes.Warning);
                }
            }
        }
        else
        {
            // Standard work based on profession
            switch (npc.Profession)
            {
                case Professions.Merchant:
                    coinsEarned = 4;
                    workDescription = $"💪 You help {npc.Name} organize inventory and serve customers.";
                    break;
                    
                case Professions.TavernKeeper:
                    coinsEarned = 4;
                    workDescription = $"🍺 You serve drinks and clean tables for {npc.Name}.";
                    break;
                    
                case Professions.Scribe:
                    coinsEarned = 3;
                    workDescription = $"✍️ You carefully copy documents for {npc.Name}.";
                    break;
                    
                case Professions.Innkeeper:
                    coinsEarned = 4;
                    workDescription = $"🧹 You clean rooms and change linens for {npc.Name}.";
                    break;
                    
                default:
                    coinsEarned = 3;
                    workDescription = $"💼 You complete various tasks for {npc.Name}.";
                    break;
            }
            
            player.ModifyCoins(coinsEarned);
            
            _messageSystem.AddSystemMessage(workDescription, SystemMessageTypes.Success);
            _messageSystem.AddSystemMessage($"  • Earned: {coinsEarned} coins", SystemMessageTypes.Success);
        }
        
        _messageSystem.AddSystemMessage($"  • Stamina: {player.Stamina}/10", SystemMessageTypes.Info);
        
        // Work builds a small connection with the NPC
        if (npc.LetterTokenTypes.Any())
        {
            var tokenType = npc.LetterTokenTypes.First();
            _tokenManager.AddTokensToNPC(tokenType, 1, option.NPCId);
            _messageSystem.AddSystemMessage(
                $"  • {npc.Name} appreciates your help (+1 {tokenType} token)",
                SystemMessageTypes.Success
            );
        }
        
        return true;
    }
    
    private bool ExecuteCollect(string npcId, string letterId)
    {
        var letter = _gameWorld.GetPlayer().LetterQueue
            .FirstOrDefault(l => l != null && l.Id == letterId);
            
        if (letter == null || letter.State != LetterState.Accepted)
        {
            _messageSystem.AddSystemMessage(
                "❌ Letter not found or already collected!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Check inventory space
        var player = _gameWorld.GetPlayer();
        var requiredSlots = letter.GetRequiredSlots();
        var availableSlots = player.Inventory.GetAvailableSlots(
            _itemRepository, 
            null // No transport bonus during collection
        );
        
        if (availableSlots < requiredSlots)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Not enough inventory space! Need {requiredSlots} slots, have {availableSlots}",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                "  • Drop some items or equipment to make room",
                SystemMessageTypes.Info
            );
            return false;
        }
        
        // Add to carried letters
        letter.State = LetterState.Collected;
        player.CarriedLetters.Add(letter);
        
        _messageSystem.AddSystemMessage(
            $"📦 You collect the letter from {letter.SenderName}.",
            SystemMessageTypes.Success
        );
        _messageSystem.AddSystemMessage(
            $"  • Letter now takes {requiredSlots} inventory slots",
            SystemMessageTypes.Info
        );
        _messageSystem.AddSystemMessage(
            $"  • Ready for delivery when in position 1",
            SystemMessageTypes.Info
        );
        
        return true;
    }
    
    private bool ExecuteDeliver(string npcId, string letterId)
    {
        var player = _gameWorld.GetPlayer();
        var letter = player.LetterQueue[0];
        
        if (letter == null || letter.State != LetterState.Collected || letter.RecipientId != npcId)
        {
            _messageSystem.AddSystemMessage(
                "❌ Cannot deliver - check letter position and collection status!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Remove from carried letters
        player.CarriedLetters.Remove(letter);
        
        // Use LetterQueueManager to handle the delivery properly
        if (_letterQueueManager.DeliverFromPosition1())
        {
            _messageSystem.AddSystemMessage(
                $"✉️ Letter delivered to {letter.RecipientName}!",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"💰 Earned {letter.Payment} coins",
                SystemMessageTypes.Success
            );
            
            // Add token with recipient
            var npc = _npcRepository.GetNPCById(npcId);
            if (npc != null && npc.LetterTokenTypes.Any())
            {
                var tokenType = npc.LetterTokenTypes.First();
                _tokenManager.AddTokensToNPC(tokenType, 1, npcId);
                _messageSystem.AddSystemMessage(
                    $"🤝 +1 {tokenType} token with {npc.Name}",
                    SystemMessageTypes.Success
                );
            }
            
            return true;
        }
        
        return false;
    }
    
    private bool ExecuteTrade(string npcId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        if (npc == null || npc.Profession != Professions.Merchant)
        {
            _messageSystem.AddSystemMessage(
                "❌ This person is not a merchant!",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // For now, just acknowledge the trade action
        // The actual trading UI will be handled separately
        _messageSystem.AddSystemMessage(
            $"💰 You browse {npc.Name}'s wares...",
            SystemMessageTypes.Info
        );
        _messageSystem.AddSystemMessage(
            "Trading interface would open here",
            SystemMessageTypes.Info
        );
        
        // Trading happens through the existing market system
        // This is just the action that opens access to it
        // No special compound mechanics - just access to trade
        
        return true;
    }
    
    private bool IsNPCAvailable(NPC npc)
    {
        var currentPeriod = _timeManager.GetCurrentTimeBlock();
        
        // Simple availability for now - expand later
        return currentPeriod switch
        {
            TimeBlocks.Night => npc.LetterTokenTypes.Contains(ConnectionType.Shadow),
            TimeBlocks.Dawn => npc.LetterTokenTypes.Contains(ConnectionType.Common),
            _ => true // Most NPCs available during day
        };
    }
    
    private bool IsLetterRecipient(string npcId)
    {
        var player = _gameWorld.GetPlayer();
        
        // Check if this NPC is the recipient of the letter in position 1
        var letterAtPosition1 = player.LetterQueue[0];
        if (letterAtPosition1 == null) return false;
        
        // Must be collected to deliver
        if (letterAtPosition1.State != LetterState.Collected) return false;
        
        // Check if this NPC is the recipient
        return letterAtPosition1.RecipientId == npcId;
    }
    
    private List<Letter> GetAcceptedLettersFromNPC(string npcId)
    {
        var player = _gameWorld.GetPlayer();
        return player.LetterQueue
            .Where(l => l != null && 
                       l.SenderId == npcId && 
                       l.State == LetterState.Accepted)
            .ToList();
    }
    
    
    /// <summary>
    /// Add environmental actions based on location spot domain tags
    /// </summary>
    private void AddEnvironmentalActions(List<ActionOption> actions, LocationSpot spot)
    {
        if (spot == null || !spot.DomainTags.Any()) return;
        
        var player = _gameWorld.GetPlayer();
        
        // Only add environmental actions if we have hours available
        if (_timeManager.HoursRemaining < 1) return;
        
        // RESOURCES tag - forest/nature locations
        if (spot.DomainTags.Contains("RESOURCES"))
        {
            // Gather berries - simple food gathering
            if (player.Stamina >= 1)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.GatherResources,
                    Name = "Gather wild berries",
                    Description = "Search the area for edible berries",
                    HourCost = 1,
                    StaminaCost = 1,
                    CoinCost = 0,
                    Effect = "+2 food items"
                });
            }
            
            // Collect herbs - slightly more effort
            if (player.Stamina >= 2)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.GatherResources,
                    Name = "Collect medicinal herbs",
                    Description = "Look for valuable herbs in the undergrowth",
                    HourCost = 1,
                    StaminaCost = 2,
                    CoinCost = 0,
                    Effect = "+1-3 herbs (can sell for 3-5 coins each)"
                });
            }
        }
        
        // COMMERCE tag - market/trade locations (when no merchant NPCs present)
        if (spot.DomainTags.Contains("COMMERCE"))
        {
            var npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(
                spot.SpotID, 
                _timeManager.GetCurrentTimeBlock()
            );
            var hasMerchants = npcsHere.Any(npc => npc.Profession == Professions.Merchant);
            
            if (!hasMerchants)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Browse,
                    Name = "Browse empty market stalls",
                    Description = "Check unattended stalls for posted prices",
                    HourCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    Effect = "Learn current market prices"
                });
            }
        }
        
        // SOCIAL tag - gathering places
        if (spot.DomainTags.Contains("SOCIAL"))
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Observe,
                Name = "Listen to local gossip",
                Description = "Sit quietly and overhear conversations",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                Effect = "Learn about recent events or opportunities"
            });
        }
        
        // Notice boards at town centers or market areas
        if ((spot.Type == LocationSpotTypes.FEATURE && spot.Name.ToLower().Contains("market")) || 
            (spot.Type == LocationSpotTypes.FEATURE && spot.Name.ToLower().Contains("square")))
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Browse,
                Name = "Read the notice board",
                Description = "Check for public announcements and opportunities",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                Effect = "Discover new letter opportunities or warnings"
            });
        }
    }
    
    private void AddPatronRequestActions(List<ActionOption> actions)
    {
        var player = _gameWorld.GetPlayer();
        
        // Can write to patron at any desk/room location
        if (player.CurrentLocationSpot?.SpotID.Contains("room") == true || 
            player.CurrentLocationSpot?.SpotID.Contains("desk") == true ||
            player.CurrentLocationSpot?.SpotID.Contains("study") == true)
        {
            if (_timeManager.HoursRemaining >= 1)
            {
                var patronTokens = _tokenManager.GetTokensWithNPC("patron")[ConnectionType.Noble];
                
                // Request funds action
                actions.Add(new ActionOption
                {
                    Action = LocationAction.RequestPatronFunds,
                    Name = "Write to patron requesting funds",
                    Description = $"Receive 30 coins, -1 Patron leverage (current: {patronTokens})",
                    HourCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                    Effect = "30 coins, -1 Noble token with patron"
                });
                
                // Request equipment if low on stamina
                if (player.Stamina <= 4)
                {
                    actions.Add(new ActionOption
                    {
                        Action = LocationAction.RequestPatronEquipment,
                        Name = "Request equipment from patron",
                        Description = $"Receive climbing gear, -2 Patron leverage (current: {patronTokens})",
                        HourCost = 1,
                        StaminaCost = 0,
                        CoinCost = 0,
                        Effect = "Climbing gear, -2 Noble tokens with patron"
                    });
                }
            }
        }
    }
    
    
    private bool ExecuteRequestPatronFunds(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        _messageSystem.AddSystemMessage(
            "📜 You pen a carefully worded letter to your patron...",
            SystemMessageTypes.Info
        );
        
        _messageSystem.AddSystemMessage(
            "  • \"Circumstances require additional resources for your important work...\"",
            SystemMessageTypes.Info
        );
        
        // Reduce tokens with patron (can go negative)
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Noble, 1, "patron");
        
        // Grant funds
        player.ModifyCoins(30);
        
        _messageSystem.AddSystemMessage(
            "💰 Your patron's courier arrives with a small purse. No questions asked... this time.",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            "  • Received 30 coins",
            SystemMessageTypes.Success
        );
        
        var patronTokens = _tokenManager.GetTokensWithNPC("patron")[ConnectionType.Noble];
        if (patronTokens < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ You now owe your patron {Math.Abs(patronTokens)} favors. Their letters will demand priority.",
                SystemMessageTypes.Warning
            );
        }
        
        return true;
    }
    
    private bool ExecuteRequestPatronEquipment(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        
        _messageSystem.AddSystemMessage(
            "📜 You write urgently to your patron about equipment needs...",
            SystemMessageTypes.Info
        );
        
        // Reduce tokens with patron
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Noble, 2, "patron");
        
        // Grant equipment (simplified - just add to inventory)
        // In full implementation, would use ItemRepository
        _messageSystem.AddSystemMessage(
            "📦 A package arrives from your patron. Quality climbing gear, no note.",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            "  • Received climbing gear",
            SystemMessageTypes.Success
        );
        
        var patronTokens = _tokenManager.GetTokensWithNPC("patron")[ConnectionType.Noble];
        if (patronTokens < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ Your debt deepens. You owe your patron {Math.Abs(patronTokens)} favors.",
                SystemMessageTypes.Danger
            );
        }
        
        return true;
    }
    
    private bool ExecuteBorrowMoney(ActionOption option)
    {
        var npc = _npcRepository.GetNPCById(option.NPCId);
        if (npc == null) return false;
        
        _messageSystem.AddSystemMessage(
            $"💸 You swallow your pride and ask {npc.Name} for a loan...",
            SystemMessageTypes.Info
        );
        
        _messageSystem.AddSystemMessage(
            $"  • \"{npc.Name} sighs. 'I'll help, but this isn't charity.'\"",
            SystemMessageTypes.Warning
        );
        
        // Create debt
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Trade, 2, option.NPCId);
        
        // Grant money
        var player = _gameWorld.GetPlayer();
        player.ModifyCoins(20);
        
        _messageSystem.AddSystemMessage(
            $"💰 {npc.Name} counts out 20 coins. 'I expect repayment... with interest.'",
            SystemMessageTypes.Success
        );
        
        var tradeTokens = _tokenManager.GetTokensWithNPC(option.NPCId)[ConnectionType.Trade];
        if (tradeTokens < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ You now owe {npc.Name}. Their letters will take priority in your queue.",
                SystemMessageTypes.Warning
            );
        }
        
        return true;
    }
    
    private bool ExecutePleedForAccess(ActionOption option)
    {
        // This would be implemented when route system is ready
        _messageSystem.AddSystemMessage(
            "Route access maintenance not yet implemented",
            SystemMessageTypes.Info
        );
        return true;
    }
    
    private bool ExecuteAcceptIllegalWork(ActionOption option)
    {
        var npc = _npcRepository.GetNPCById(option.NPCId);
        if (npc == null) return false;
        
        _messageSystem.AddSystemMessage(
            $"🌑 {npc.Name} leans in close. 'I have a job that pays well... if you don't ask questions.'",
            SystemMessageTypes.Warning
        );
        
        _messageSystem.AddSystemMessage(
            "  • You nod silently. Some things are better left unspoken.",
            SystemMessageTypes.Info
        );
        
        // Create leverage
        _tokenManager.RemoveTokensFromNPC(ConnectionType.Shadow, 1, option.NPCId);
        
        // Grant payment
        var player = _gameWorld.GetPlayer();
        player.ModifyCoins(30);
        
        _messageSystem.AddSystemMessage(
            $"💰 {npc.Name} slides a heavy purse across the table. 'Remember, you never saw me.'",
            SystemMessageTypes.Success
        );
        
        _messageSystem.AddSystemMessage(
            $"⚠️ {npc.Name} now has dirt on you. Refusing their future requests may have consequences.",
            SystemMessageTypes.Danger
        );
        
        return true;
    }
    
    private bool ExecuteGatherResources(ActionOption option)
    {
        var player = _gameWorld.GetPlayer();
        var spot = player.CurrentLocationSpot;
        
        if (option.Name.Contains("berries"))
        {
            // Gather berries - simple and reliable
            _messageSystem.AddSystemMessage(
                "🫐 You spend an hour carefully picking ripe berries from the bushes.",
                SystemMessageTypes.Success
            );
            
            // Add berries to inventory (simplified for now)
            // In full implementation, would use ItemRepository to add actual food items
            _messageSystem.AddSystemMessage(
                "  • Found 2 portions of wild berries",
                SystemMessageTypes.Success
            );
            
            _messageSystem.AddSystemMessage(
                "  • Each portion restores 1 stamina when eaten",
                SystemMessageTypes.Info
            );
        }
        else if (option.Name.Contains("herbs"))
        {
            // Collect herbs - variable results
            var herbCount = new Random().Next(1, 4); // 1-3 herbs
            
            _messageSystem.AddSystemMessage(
                "🌿 You search the undergrowth for medicinal herbs.",
                SystemMessageTypes.Success
            );
            
            _messageSystem.AddSystemMessage(
                $"  • Found {herbCount} medicinal herb{(herbCount > 1 ? "s" : "")}",
                SystemMessageTypes.Success
            );
            
            _messageSystem.AddSystemMessage(
                "  • Can be sold to merchants for 3-5 coins each",
                SystemMessageTypes.Info
            );
        }
        
        return true;
    }
    
    private bool ExecuteBrowse(ActionOption option)
    {
        if (option.Name.Contains("market stalls"))
        {
            _messageSystem.AddSystemMessage(
                "🏪 You browse the empty market stalls, checking posted prices.",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Bread: 2 coins",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Herbs: 3-5 coins (depending on quality)",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Basic supplies: 5-10 coins",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage(
                "  • Note: \"Marcus returns at morning market hours\"",
                SystemMessageTypes.Info
            );
        }
        else if (option.Name.Contains("notice board"))
        {
            _messageSystem.AddSystemMessage(
                "📋 You read the notices posted on the board.",
                SystemMessageTypes.Info
            );
            
            var currentTime = _timeManager.GetCurrentTimeBlock();
            
            // Different notices based on time of day
            if (currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon)
            {
                _messageSystem.AddSystemMessage(
                    "  • \"Warning: Bandits spotted on eastern road at night\"",
                    SystemMessageTypes.Warning
                );
                
                _messageSystem.AddSystemMessage(
                    "  • \"Elena the Scribe seeks carriers for urgent letters\"",
                    SystemMessageTypes.Info
                );
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    "  • \"Night work available - ask at the docks\"",
                    SystemMessageTypes.Info
                );
                
                _messageSystem.AddSystemMessage(
                    "  • \"Lost: Merchant's ledger. Reward offered.\"",
                    SystemMessageTypes.Info
                );
            }
        }
        
        return true;
    }
    
    private bool ExecuteObserve(ActionOption option)
    {
        _messageSystem.AddSystemMessage(
            "👂 You find a quiet corner and listen to the local gossip.",
            SystemMessageTypes.Info
        );
        
        // Random gossip based on game state
        var gossipOptions = new List<string>
        {
            "  • \"Did you hear? The noble's courier never arrived...\"",
            "  • \"Marcus has been looking for reliable carriers lately\"",
            "  • \"The mountain pass is treacherous without proper gear\"",
            "  • \"I heard Elena pays well for discrete deliveries\"",
            "  • \"The river route saves time, but ruins letters in the rain\""
        };
        
        var random = new Random();
        var selectedGossip = gossipOptions[random.Next(gossipOptions.Count)];
        
        _messageSystem.AddSystemMessage(
            selectedGossip,
            SystemMessageTypes.Info
        );
        
        _messageSystem.AddSystemMessage(
            "  • You make a mental note of this information",
            SystemMessageTypes.Info
        );
        
        return true;
    }
}

/// <summary>
/// Represents an action option with its resource costs
/// </summary>
public class ActionOption
{
    public LocationAction Action { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int HourCost { get; set; }
    public int StaminaCost { get; set; }
    public int CoinCost { get; set; }
    public int FocusCost { get; set; } // For future mental stamina system
    public string NPCId { get; set; }
    public string LetterId { get; set; }
    public string Effect { get; set; }
}