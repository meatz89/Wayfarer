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
        
        // Basic actions available everywhere
        if (_timeManager.HoursRemaining >= 1)
        {
            actions.Add(new ActionOption
            {
                Action = LocationAction.Rest,
                Name = "Rest",
                Description = "Recover stamina (+3)",
                HourCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                Effect = "+3 Stamina"
            });
        }
        
        // Check what NPCs are present and available
        foreach (var npc in npcsHere)
        {
            // Check NPC availability in current time period
            if (!IsNPCAvailable(npc)) continue;
            
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
            
            // Trade - if this NPC is a merchant
            if (_timeManager.HoursRemaining >= 1 && npc.Profession == Professions.Merchant)
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
        
        // Location-specific actions based on what's naturally available
        AddLocationSpecificActions(location, spot, actions, npcsHere);
        
        // Patron request actions - available when desperate
        AddPatronRequestActions(actions);
        
        // Emergency assistance actions - context sensitive
        AddEmergencyActions(actions, npcsHere);
        
        return actions;
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
        
        // Baker work is a separate action with different mechanics
        if (!string.IsNullOrEmpty(option.NPCId) && option.StaminaCost == 1)
        {
            // Special baker work: 1 stamina → 2 coins + bread
            player.ModifyCoins(2);
            
            var npc = _npcRepository.GetNPCById(option.NPCId);
            if (npc != null)
            {
                _messageSystem.AddSystemMessage(
                    $"🥖 You help {npc.Name} with the morning baking.",
                    SystemMessageTypes.Success
                );
                _messageSystem.AddSystemMessage(
                    "  • Earned: 2 coins",
                    SystemMessageTypes.Success
                );
                
                // Add bread to inventory if space available
                var breadItem = _itemRepository.GetItemById("bread");
                if (breadItem != null && player.Inventory.CanAddItem(breadItem, _itemRepository))
                {
                    player.Inventory.AddItem(breadItem.Id);
                    _messageSystem.AddSystemMessage(
                        "  • Received: 1 bread (restores 2 stamina when eaten)",
                        SystemMessageTypes.Success
                    );
                }
                else if (breadItem == null)
                {
                    _messageSystem.AddSystemMessage(
                        "  • (Bread item not found in game data)",
                        SystemMessageTypes.Warning
                    );
                }
                else
                {
                    _messageSystem.AddSystemMessage(
                        "  • No room for bread in inventory!",
                        SystemMessageTypes.Warning
                    );
                }
                
                _messageSystem.AddSystemMessage(
                    $"  • Stamina: {player.Stamina}/10",
                    SystemMessageTypes.Info
                );
            }
        }
        else
        {
            // Standard work: 2 stamina → 4 coins
            player.ModifyCoins(4);
            
            _messageSystem.AddSystemMessage(
                "💪 You work hard for an hour.",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                "  • Earned: 4 coins",
                SystemMessageTypes.Success
            );
            _messageSystem.AddSystemMessage(
                $"  • Stamina: {player.Stamina}/10",
                SystemMessageTypes.Info
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
    
    private void AddLocationSpecificActions(Location location, LocationSpot spot, List<ActionOption> actions, List<NPC> npcsPresent)
    {
        var player = _gameWorld.GetPlayer();
        var currentTime = _timeManager.GetCurrentTimeBlock();
        
        // Market - work opportunities naturally available
        if (spot.SpotID.Contains("market"))
        {
            // Work is available during business hours if you have stamina
            if (_timeManager.HoursRemaining >= 1 && 
                player.Stamina >= GameRules.STAMINA_COST_WORK &&
                (currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon))
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Work,
                    Name = "Help load merchant wagons",
                    Description = "Physical labor for quick coins",
                    HourCost = 1,
                    StaminaCost = GameRules.STAMINA_COST_WORK,
                    CoinCost = 0,
                    Effect = "+4 coins"
                });
            }
        }
        
        // Tavern - additional options available, not enhanced versions
        if (spot.SpotID.Contains("tavern") || spot.SpotID.Contains("inn"))
        {
            // Evening taverns are busy - natural social opportunities
            if (currentTime == TimeBlocks.Evening && player.Coins >= 3)
            {
                // For each NPC present who accepts Common tokens, offer drink-sharing option
                var commonNPCs = npcsPresent.Where(n => n.LetterTokenTypes.Contains(ConnectionType.Common)).ToList();
                foreach (var npc in commonNPCs)
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
                        Effect = $"+3 Stamina, +1 Common token with {npc.Name}"
                    });
                }
            }
        }
        
        // Workshop - equipment context (future)
        if (spot.SpotID.Contains("workshop") || spot.SpotID.Contains("smithy"))
        {
            // Future: Repair equipment, commission items
        }
        
        // Dawn opportunities - baker example
        if (currentTime == TimeBlocks.Dawn && spot.SpotID.Contains("bakery"))
        {
            var baker = npcsPresent.FirstOrDefault(n => n.Profession == Professions.Merchant && n.Name.ToLower().Contains("baker"));
            if (baker != null && player.Stamina >= 1)
            {
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Work,
                    Name = $"Help {baker.Name} with morning baking",
                    Description = "Early work with benefits",
                    HourCost = 1,
                    StaminaCost = 1, // Lighter work
                    CoinCost = 0,
                    NPCId = baker.ID,
                    Effect = "+2 coins, +1 bread (restores 2 stamina when eaten)"
                });
            }
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
    
    private void AddEmergencyActions(List<ActionOption> actions, List<NPC> npcsPresent)
    {
        var player = _gameWorld.GetPlayer();
        
        if (_timeManager.HoursRemaining < 1) return;
        
        foreach (var npc in npcsPresent)
        {
            // Borrow money when broke
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