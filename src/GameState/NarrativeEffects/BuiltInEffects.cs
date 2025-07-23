using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Set a flag in the game state
/// </summary>
public class SetFlagEffect : INarrativeEffect
{
    private readonly FlagService _flagService;
    
    public string EffectType => "SetFlag";
    
    public SetFlagEffect(FlagService flagService)
    {
        _flagService = flagService;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var flagName = parameters.GetValueOrDefault("flag")?.ToString();
        var value = parameters.GetValueOrDefault("value", true);
        
        if (value is bool boolValue)
        {
            _flagService.SetFlag(flagName, boolValue);
        }
        else if (int.TryParse(value.ToString(), out int intValue))
        {
            // FlagService doesn't have SetCounter, use IncrementCounter to set value
            var currentValue = _flagService.GetCounter(flagName);
            _flagService.IncrementCounter(flagName, intValue - currentValue);
        }
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Set flag {flagName} to {value}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("flag") && !string.IsNullOrEmpty(parameters["flag"]?.ToString());
    }
}

/// <summary>
/// Grant an item to the player
/// </summary>
public class GrantItemEffect : INarrativeEffect
{
    private readonly ItemRepository _itemRepository;
    
    public string EffectType => "GrantItem";
    
    public GrantItemEffect(ItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var itemId = parameters.GetValueOrDefault("itemId")?.ToString();
        var quantity = Convert.ToInt32(parameters.GetValueOrDefault("quantity", 1));
        
        var player = world.GetPlayer();
        var item = _itemRepository.GetItemById(itemId);
        
        if (item == null)
        {
            return Task.FromResult(NarrativeEffectResult.Failed($"Item {itemId} not found"));
        }
        
        // Add item to inventory
        for (int i = 0; i < quantity; i++)
        {
            if (!player.Inventory.AddItem(item.Id))
            {
                return Task.FromResult(NarrativeEffectResult.Failed($"Inventory full, could only add {i} of {quantity} items"));
            }
        }
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Granted {quantity}x {item.Name}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("itemId") && !string.IsNullOrEmpty(parameters["itemId"]?.ToString());
    }
}

/// <summary>
/// Modify player's coins
/// </summary>
public class ModifyCoinsEffect : INarrativeEffect
{
    public string EffectType => "ModifyCoins";
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var amount = Convert.ToInt32(parameters.GetValueOrDefault("amount", 0));
        var player = world.GetPlayer();
        
        var oldCoins = player.Coins;
        player.Coins = Math.Max(0, player.Coins + amount);
        
        var message = amount >= 0 
            ? $"Gained {amount} coins" 
            : $"Lost {Math.Abs(amount)} coins";
            
        return Task.FromResult(NarrativeEffectResult.Succeeded(message));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("amount");
    }
}

/// <summary>
/// Modify player's stamina
/// </summary>
public class ModifyStaminaEffect : INarrativeEffect
{
    public string EffectType => "ModifyStamina";
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var amount = Convert.ToInt32(parameters.GetValueOrDefault("amount", 0));
        var player = world.GetPlayer();
        
        var oldStamina = player.Stamina;
        player.Stamina = Math.Clamp(player.Stamina + amount, 0, 10); // Max stamina is 10
        
        var message = amount >= 0 
            ? $"Gained {amount} stamina" 
            : $"Lost {Math.Abs(amount)} stamina";
            
        return Task.FromResult(NarrativeEffectResult.Succeeded(message));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("amount");
    }
}

/// <summary>
/// Create a standing obligation
/// </summary>
public class CreateObligationEffect : INarrativeEffect
{
    private readonly StandingObligationManager _obligationManager;
    
    public string EffectType => "CreateObligation";
    
    public CreateObligationEffect(StandingObligationManager obligationManager)
    {
        _obligationManager = obligationManager;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var obligation = new StandingObligation
        {
            ID = parameters.GetValueOrDefault("id")?.ToString(),
            Name = parameters.GetValueOrDefault("name")?.ToString(),
            Description = parameters.GetValueOrDefault("description")?.ToString(),
            Source = parameters.GetValueOrDefault("sourceNpcId")?.ToString()
        };
        
        // Parse token type
        if (parameters.TryGetValue("relatedTokenType", out var tokenTypeValue) && 
            Enum.TryParse<ConnectionType>(tokenTypeValue.ToString(), out var tokenType))
        {
            obligation.RelatedTokenType = tokenType;
        }
        
        // Parse benefit effects
        if (parameters.TryGetValue("benefitEffects", out var benefitEffects) && benefitEffects is List<object> benefits)
        {
            foreach (var effect in benefits)
            {
                if (Enum.TryParse<ObligationEffect>(effect.ToString(), out var obligationEffect))
                {
                    obligation.BenefitEffects.Add(obligationEffect);
                }
            }
        }
        
        // Parse constraint effects
        if (parameters.TryGetValue("constraintEffects", out var constraintEffects) && constraintEffects is List<object> constraints)
        {
            foreach (var effect in constraints)
            {
                if (Enum.TryParse<ObligationEffect>(effect.ToString(), out var obligationEffect))
                {
                    obligation.ConstraintEffects.Add(obligationEffect);
                }
            }
        }
        
        _obligationManager.AddObligation(obligation);
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Created obligation: {obligation.Name}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("id") && 
               parameters.ContainsKey("name") && 
               !string.IsNullOrEmpty(parameters["id"]?.ToString());
    }
}

/// <summary>
/// Grant connection tokens
/// </summary>
public class GrantTokenEffect : INarrativeEffect
{
    private readonly ConnectionTokenManager _tokenManager;
    
    public string EffectType => "GrantToken";
    
    public GrantTokenEffect(ConnectionTokenManager tokenManager)
    {
        _tokenManager = tokenManager;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var npcId = parameters.GetValueOrDefault("npcId")?.ToString();
        var amount = Convert.ToInt32(parameters.GetValueOrDefault("amount", 1));
        
        if (!Enum.TryParse<ConnectionType>(parameters.GetValueOrDefault("tokenType")?.ToString(), out var tokenType))
        {
            return Task.FromResult(NarrativeEffectResult.Failed("Invalid token type"));
        }
        
        _tokenManager.AddTokensToNPC(tokenType, amount, npcId);
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Granted {amount} {tokenType} tokens for {npcId}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("npcId") && 
               parameters.ContainsKey("tokenType") && 
               !string.IsNullOrEmpty(parameters["npcId"]?.ToString());
    }
}

/// <summary>
/// Create a letter in the queue
/// </summary>
public class CreateLetterEffect : INarrativeEffect
{
    private readonly LetterQueueManager _letterQueueManager;
    private readonly LetterTemplateRepository _letterTemplateRepository;
    
    public string EffectType => "CreateLetter";
    
    public CreateLetterEffect(LetterQueueManager letterQueueManager, LetterTemplateRepository letterTemplateRepository)
    {
        _letterQueueManager = letterQueueManager;
        _letterTemplateRepository = letterTemplateRepository;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var templateId = parameters.GetValueOrDefault("templateId")?.ToString();
        var senderId = parameters.GetValueOrDefault("senderId")?.ToString();
        
        var template = _letterTemplateRepository.GetTemplateById(templateId);
        if (template == null)
        {
            return Task.FromResult(NarrativeEffectResult.Failed($"Letter template {templateId} not found"));
        }
        
        // Create letter using current Letter class structure
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = senderId, // TODO: Get NPC name from repository
            RecipientName = "Recipient", // TODO: Get from template
            SenderId = senderId,
            RecipientId = "recipient_id", // TODO: Get from template  
            Deadline = 3, // TODO: Get from template
            Payment = 10, // TODO: Get from template
            TokenType = ConnectionType.Common, // TODO: Get from template
            State = LetterState.Offered,
            Description = $"Letter from narrative: {templateId}"
        };
        
        _letterQueueManager.AddLetterToQueue(letter);
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Created letter: {letter.Name}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("templateId") && !string.IsNullOrEmpty(parameters["templateId"]?.ToString());
    }
}

/// <summary>
/// Unlock a location for access
/// </summary>
public class UnlockLocationEffect : INarrativeEffect
{
    private readonly FlagService _flagService;
    
    public string EffectType => "UnlockLocation";
    
    public UnlockLocationEffect(FlagService flagService)
    {
        _flagService = flagService;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var locationId = parameters.GetValueOrDefault("locationId")?.ToString();
        
        _flagService.SetFlag($"location_{locationId}_unlocked", true);
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Unlocked location: {locationId}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("locationId") && !string.IsNullOrEmpty(parameters["locationId"]?.ToString());
    }
}

/// <summary>
/// Show a previously hidden NPC
/// </summary>
public class ShowNPCEffect : INarrativeEffect
{
    private readonly FlagService _flagService;
    
    public string EffectType => "ShowNPC";
    
    public ShowNPCEffect(FlagService flagService)
    {
        _flagService = flagService;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var npcId = parameters.GetValueOrDefault("npcId")?.ToString();
        var narrativeId = parameters.GetValueOrDefault("narrativeId")?.ToString();
        
        // Set flag to show NPC
        _flagService.SetFlag($"narrative_{narrativeId}_show_{npcId}", true);
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Revealed NPC: {npcId}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("npcId") && 
               parameters.ContainsKey("narrativeId") && 
               !string.IsNullOrEmpty(parameters["npcId"]?.ToString());
    }
}

/// <summary>
/// Start another narrative
/// </summary>
public class StartNarrativeEffect : INarrativeEffect
{
    private readonly NarrativeManager _narrativeManager;
    
    public string EffectType => "StartNarrative";
    
    public StartNarrativeEffect(NarrativeManager narrativeManager)
    {
        _narrativeManager = narrativeManager;
    }
    
    public Task<NarrativeEffectResult> Apply(GameWorld world, Dictionary<string, object> parameters)
    {
        var narrativeId = parameters.GetValueOrDefault("narrativeId")?.ToString();
        
        if (_narrativeManager.IsNarrativeActive(narrativeId))
        {
            return Task.FromResult(NarrativeEffectResult.Failed($"Narrative {narrativeId} is already active"));
        }
        
        _narrativeManager.StartNarrative(narrativeId);
        
        return Task.FromResult(NarrativeEffectResult.Succeeded($"Started narrative: {narrativeId}"));
    }
    
    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        return parameters.ContainsKey("narrativeId") && !string.IsNullOrEmpty(parameters["narrativeId"]?.ToString());
    }
}