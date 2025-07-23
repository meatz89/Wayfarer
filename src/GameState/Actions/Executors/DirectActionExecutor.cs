using System.Threading.Tasks;

namespace Wayfarer.GameState.Actions.Executors;

/// <summary>
/// Executor for direct actions that don't require conversation
/// </summary>
public class DirectActionExecutor : IActionExecutor
{
    private readonly MessageSystem _messageSystem;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;
    private readonly GameConfiguration _config;
    
    public DirectActionExecutor(
        MessageSystem messageSystem,
        ConnectionTokenManager tokenManager,
        NPCRepository npcRepository,
        ItemRepository itemRepository,
        GameConfiguration config)
    {
        _messageSystem = messageSystem;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
        _config = config;
    }
    
    public bool CanHandle(LocationAction actionType)
    {
        return actionType == LocationAction.Rest ||
               actionType == LocationAction.Socialize ||
               actionType == LocationAction.Work ||
               actionType == LocationAction.GatherResources ||
               actionType == LocationAction.Browse ||
               actionType == LocationAction.Observe;
    }
    
    public async Task<ActionExecutionResult> Execute(ActionOption action, Player player, GameWorld world)
    {
        var result = new ActionExecutionResult { Success = true };
        
        switch (action.Action)
        {
            case LocationAction.Rest:
                ExecuteRest(action, player, result);
                break;
                
            case LocationAction.Socialize:
                ExecuteSocialize(action, player, result);
                break;
                
            case LocationAction.Work:
                ExecuteWork(action, player, result);
                break;
                
            case LocationAction.GatherResources:
                ExecuteGatherResources(action, player, result);
                break;
                
            case LocationAction.Browse:
                ExecuteBrowse(action, player, world, result);
                break;
                
            case LocationAction.Observe:
                ExecuteObserve(action, player, result);
                break;
        }
        
        return await Task.FromResult(result);
    }
    
    private void ExecuteRest(ActionOption action, Player player, ActionExecutionResult result)
    {
        player.ModifyStamina(_config.Stamina.RecoveryRest);
        
        result.Effects.Add(new ActionEffect
        {
            Type = "stamina",
            Description = "Stamina restored",
            Amount = _config.Stamina.RecoveryRest
        });
        
        // Special case: buying drinks with NPC
        if (action.CoinCost > 0 && !string.IsNullOrEmpty(action.NPCId))
        {
            var npc = _npcRepository.GetNPCById(action.NPCId);
            if (npc != null)
            {
                _tokenManager.AddTokensToNPC(ConnectionType.Common, 1, action.NPCId);
                
                result.Effects.Add(new ActionEffect
                {
                    Type = "tokens",
                    Description = $"Bond with {npc.Name} strengthened",
                    Amount = 1,
                    TargetId = action.NPCId
                });
                
                _messageSystem.AddSystemMessage(
                    $"🍺 You buy drinks and share stories with {npc.Name}.",
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
        }
        
        result.Message = "Rest completed successfully";
    }
    
    private void ExecuteSocialize(ActionOption action, Player player, ActionExecutionResult result)
    {
        if (string.IsNullOrEmpty(action.NPCId))
        {
            result.Success = false;
            result.Message = "No NPC specified for socializing";
            return;
        }
        
        var npc = _npcRepository.GetNPCById(action.NPCId);
        if (npc == null)
        {
            result.Success = false;
            result.Message = "NPC not found";
            return;
        }
        
        var tokenType = npc.LetterTokenTypes.FirstOrDefault();
        _tokenManager.AddTokensToNPC(tokenType, 1, action.NPCId);
        
        result.Effects.Add(new ActionEffect
        {
            Type = "tokens",
            Description = $"{tokenType} token with {npc.Name}",
            Amount = 1,
            TargetId = action.NPCId
        });
        
        _messageSystem.AddSystemMessage(
            $"🤝 You spend quality time with {npc.Name}.",
            SystemMessageTypes.Success
        );
        
        var tokens = _tokenManager.GetTokensWithNPC(action.NPCId);
        var totalTokens = tokens.Values.Sum();
        
        _messageSystem.AddSystemMessage(
            $"  • Total relationship: {totalTokens} tokens",
            SystemMessageTypes.Info
        );
        
        result.Message = "Socialization completed successfully";
    }
    
    private void ExecuteWork(ActionOption action, Player player, ActionExecutionResult result)
    {
        var npc = action.NPCId != null ? _npcRepository.GetNPCById(action.NPCId) : null;
        int coinsEarned = 0;
        string workDescription = "";
        
        if (npc == null)
        {
            // Environmental work
            coinsEarned = 3;
            workDescription = "You find various odd jobs around the area.";
        }
        else
        {
            // NPC-specific work
            switch (npc.Profession)
            {
                case Professions.Merchant:
                    coinsEarned = 4;
                    workDescription = $"You help {npc.Name} organize inventory and serve customers.";
                    break;
                    
                case Professions.TavernKeeper:
                    coinsEarned = 4;
                    workDescription = $"You serve drinks and clean tables for {npc.Name}.";
                    break;
                    
                case Professions.Scribe:
                    coinsEarned = 3;
                    workDescription = $"You carefully copy documents for {npc.Name}.";
                    break;
                    
                default:
                    coinsEarned = 3;
                    workDescription = $"You complete various tasks for {npc.Name}.";
                    break;
            }
            
            // Work builds relationship
            if (npc.LetterTokenTypes.Any())
            {
                var tokenType = npc.LetterTokenTypes.First();
                _tokenManager.AddTokensToNPC(tokenType, 1, action.NPCId);
                
                result.Effects.Add(new ActionEffect
                {
                    Type = "tokens",
                    Description = $"{npc.Name} appreciates your help",
                    Amount = 1,
                    TargetId = action.NPCId
                });
            }
        }
        
        player.ModifyCoins(coinsEarned);
        
        result.Effects.Add(new ActionEffect
        {
            Type = "coins",
            Description = "Earned from work",
            Amount = coinsEarned
        });
        
        _messageSystem.AddSystemMessage($"💪 {workDescription}", SystemMessageTypes.Success);
        _messageSystem.AddSystemMessage($"  • Earned: {coinsEarned} coins", SystemMessageTypes.Success);
        
        result.Message = "Work completed successfully";
    }
    
    private void ExecuteGatherResources(ActionOption action, Player player, ActionExecutionResult result)
    {
        if (action.Name.Contains("berries"))
        {
            _messageSystem.AddSystemMessage(
                "🫐 You spend an hour carefully picking ripe berries from the bushes.",
                SystemMessageTypes.Success
            );
            
            result.Effects.Add(new ActionEffect
            {
                Type = "items",
                Description = "Wild berries gathered",
                Amount = 2
            });
        }
        else if (action.Name.Contains("herbs"))
        {
            var herbCount = new Random().Next(1, 4);
            
            _messageSystem.AddSystemMessage(
                "🌿 You search the undergrowth for medicinal herbs.",
                SystemMessageTypes.Success
            );
            
            result.Effects.Add(new ActionEffect
            {
                Type = "items",
                Description = "Medicinal herbs found",
                Amount = herbCount
            });
        }
        
        result.Message = "Resource gathering completed";
    }
    
    private void ExecuteBrowse(ActionOption action, Player player, GameWorld world, ActionExecutionResult result)
    {
        if (action.Name.Contains("market stalls"))
        {
            _messageSystem.AddSystemMessage(
                "🏪 You browse the empty market stalls, checking posted prices.",
                SystemMessageTypes.Info
            );
            
            _messageSystem.AddSystemMessage("  • Bread: 2 coins", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage("  • Herbs: 3-5 coins (depending on quality)", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage("  • Basic supplies: 5-10 coins", SystemMessageTypes.Info);
        }
        else if (action.Name.Contains("notice board"))
        {
            _messageSystem.AddSystemMessage(
                "📋 You read the notices posted on the board.",
                SystemMessageTypes.Info
            );
            
            // Dynamic content based on game state
            var currentTime = world.TimeManager.GetCurrentTimeBlock();
            if (currentTime == TimeBlocks.Dawn)
            {
                _messageSystem.AddSystemMessage(
                    "  • \"The letter board is active now - check for new opportunities!\"",
                    SystemMessageTypes.Success
                );
            }
        }
        
        result.Message = "Browsing completed";
    }
    
    private void ExecuteObserve(ActionOption action, Player player, ActionExecutionResult result)
    {
        _messageSystem.AddSystemMessage(
            "👂 You find a quiet corner and listen to the local gossip.",
            SystemMessageTypes.Info
        );
        
        var gossipOptions = new[]
        {
            "\"Did you hear? The noble's courier never arrived...\"",
            "\"Marcus has been looking for reliable carriers lately\"",
            "\"The mountain pass is treacherous without proper gear\"",
            "\"I heard Elena pays well for discrete deliveries\"",
            "\"The river route saves time, but ruins letters in the rain\""
        };
        
        var selectedGossip = gossipOptions[new Random().Next(gossipOptions.Length)];
        
        _messageSystem.AddSystemMessage($"  • {selectedGossip}", SystemMessageTypes.Info);
        _messageSystem.AddSystemMessage("  • You make a mental note of this information", SystemMessageTypes.Info);
        
        result.Message = "Observation completed";
    }
}