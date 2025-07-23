using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// Registry for narrative effects that can be applied during narrative progression
/// </summary>
public class NarrativeEffectRegistry
{
    private readonly Dictionary<string, Type> _effectTypes = new Dictionary<string, Type>();
    private readonly IServiceProvider _serviceProvider;
    
    public NarrativeEffectRegistry(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisterBuiltInEffects();
    }
    
    /// <summary>
    /// Register built-in narrative effects
    /// </summary>
    private void RegisterBuiltInEffects()
    {
        RegisterEffect<SetFlagEffect>("SetFlag");
        RegisterEffect<GrantItemEffect>("GrantItem");
        RegisterEffect<ModifyCoinsEffect>("ModifyCoins");
        RegisterEffect<ModifyStaminaEffect>("ModifyStamina");
        RegisterEffect<CreateObligationEffect>("CreateObligation");
        RegisterEffect<GrantTokenEffect>("GrantToken");
        RegisterEffect<CreateLetterEffect>("CreateLetter");
        RegisterEffect<UnlockLocationEffect>("UnlockLocation");
        RegisterEffect<ShowNPCEffect>("ShowNPC");
        RegisterEffect<StartNarrativeEffect>("StartNarrative");
    }
    
    /// <summary>
    /// Register a new effect type
    /// </summary>
    public void RegisterEffect<T>(string effectType) where T : INarrativeEffect
    {
        _effectTypes[effectType] = typeof(T);
    }
    
    /// <summary>
    /// Create an effect instance from JSON configuration
    /// </summary>
    public INarrativeEffect CreateEffect(string effectType, JObject parameters)
    {
        if (!_effectTypes.ContainsKey(effectType))
        {
            throw new ArgumentException($"Unknown effect type: {effectType}");
        }
        
        var type = _effectTypes[effectType];
        var effect = Activator.CreateInstance(type) as INarrativeEffect;
        
        // If effect has dependencies, try to inject them from service provider
        var constructor = type.GetConstructors().FirstOrDefault();
        if (constructor != null && constructor.GetParameters().Length > 0)
        {
            var constructorParams = constructor.GetParameters()
                .Select(p => _serviceProvider.GetService(p.ParameterType))
                .ToArray();
            effect = Activator.CreateInstance(type, constructorParams) as INarrativeEffect;
        }
        
        return effect;
    }
    
    /// <summary>
    /// Apply multiple effects in sequence
    /// </summary>
    public async Task<List<NarrativeEffectResult>> ApplyEffects(GameWorld world, List<NarrativeEffectDefinition> effectDefinitions)
    {
        var results = new List<NarrativeEffectResult>();
        
        foreach (var effectDef in effectDefinitions)
        {
            try
            {
                var effect = CreateEffect(effectDef.Type, effectDef.Parameters);
                var parameters = effectDef.Parameters?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
                
                if (!effect.ValidateParameters(parameters))
                {
                    results.Add(NarrativeEffectResult.Failed($"Invalid parameters for effect {effectDef.Type}"));
                    continue;
                }
                
                var result = await effect.Apply(world, parameters);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(NarrativeEffectResult.Failed($"Failed to apply effect {effectDef.Type}: {ex.Message}"));
            }
        }
        
        return results;
    }
}

/// <summary>
/// Definition of a narrative effect from JSON
/// </summary>
public class NarrativeEffectDefinition
{
    public string Type { get; set; }
    public JObject Parameters { get; set; }
}