using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Loads narrative definitions from JSON files
/// </summary>
public class NarrativeLoader
{
    private readonly string _contentPath;
    
    public NarrativeLoader(string contentPath)
    {
        _contentPath = contentPath;
    }
    
    /// <summary>
    /// Load all narrative definitions from the narratives.json file
    /// </summary>
    public async Task<List<NarrativeDefinition>> LoadNarrativesAsync()
    {
        var narrativesPath = Path.Combine(_contentPath, "Templates", "narratives.json");
        
        if (!File.Exists(narrativesPath))
        {
            throw new FileNotFoundException($"Narratives file not found at: {narrativesPath}");
        }
        
        var json = await File.ReadAllTextAsync(narrativesPath);
        var narrativeData = JsonConvert.DeserializeObject<List<NarrativeJsonData>>(json);
        
        return narrativeData.Select(ConvertToNarrativeDefinition).ToList();
    }
    
    /// <summary>
    /// Load a single narrative definition by ID
    /// </summary>
    public async Task<NarrativeDefinition> LoadNarrativeAsync(string narrativeId)
    {
        var narratives = await LoadNarrativesAsync();
        var narrative = narratives.FirstOrDefault(n => n.Id == narrativeId);
        
        if (narrative == null)
        {
            throw new ArgumentException($"Narrative with ID '{narrativeId}' not found");
        }
        
        return narrative;
    }
    
    /// <summary>
    /// Convert JSON data to narrative definition
    /// </summary>
    private NarrativeDefinition ConvertToNarrativeDefinition(NarrativeJsonData data)
    {
        var definition = new NarrativeDefinition
        {
            Id = data.Id,
            Name = data.Name,
            Description = data.Description,
            IntroductionMessage = data.IntroductionMessage,
            CompletionMessage = data.CompletionMessage,
            RestrictedNPCs = data.RestrictedNPCs
        };
        
        // Convert starting conditions
        if (data.StartingConditions != null)
        {
            definition.StartingConditions = new NarrativeStartingConditions
            {
                PlayerCoins = data.StartingConditions.PlayerCoins,
                PlayerStamina = data.StartingConditions.PlayerStamina,
                StartingLocation = data.StartingConditions.StartingLocation,
                StartingSpot = data.StartingConditions.StartingSpot,
                ClearInventory = data.StartingConditions.ClearInventory,
                ClearLetterQueue = data.StartingConditions.ClearLetterQueue,
                ClearObligations = data.StartingConditions.ClearObligations
            };
        }
        
        // Convert steps
        if (data.Steps != null)
        {
            definition.Steps = data.Steps.Select(ConvertToNarrativeStep).ToList();
        }
        
        // Convert rewards
        if (data.Rewards != null)
        {
            definition.Rewards = new NarrativeRewards
            {
                Coins = data.Rewards.Coins,
                Stamina = data.Rewards.Stamina,
                Items = data.Rewards.Items,
                Message = data.Rewards.Message
            };
        }
        
        return definition;
    }
    
    /// <summary>
    /// Convert JSON step data to narrative step
    /// </summary>
    private NarrativeStep ConvertToNarrativeStep(NarrativeStepJsonData data)
    {
        var step = new NarrativeStep
        {
            Id = data.Id,
            Name = data.Name,
            Description = data.Description,
            RequiredLocation = data.RequiredLocation,
            RequiredNPC = data.RequiredNPC,
            GuidanceText = data.GuidanceText,
            CompletionFlag = data.CompletionFlag,
            ConversationIntroduction = data.ConversationIntroduction
        };
        
        // Parse required action
        if (!string.IsNullOrEmpty(data.RequiredAction))
        {
            if (Enum.TryParse<LocationAction>(data.RequiredAction, out var action))
            {
                step.RequiredAction = action;
            }
        }
        
        // Parse allowed actions
        if (data.AllowedActions != null)
        {
            step.AllowedActions = new List<LocationAction>();
            foreach (var actionStr in data.AllowedActions)
            {
                if (Enum.TryParse<LocationAction>(actionStr, out var action))
                {
                    step.AllowedActions.Add(action);
                }
            }
        }
        
        // Convert obligation if present
        if (data.ObligationToCreate != null)
        {
            step.ObligationToCreate = ConvertToStepObligation(data.ObligationToCreate);
        }
        
        return step;
    }
    
    /// <summary>
    /// Convert JSON obligation data to step obligation
    /// </summary>
    private NarrativeStepObligation ConvertToStepObligation(ObligationJsonData data)
    {
        var obligation = new NarrativeStepObligation
        {
            Id = data.Id,
            Name = data.Name,
            Description = data.Description,
            SourceNpcId = data.SourceNpcId
        };
        
        // Parse token type
        if (!string.IsNullOrEmpty(data.RelatedTokenType))
        {
            if (Enum.TryParse<ConnectionType>(data.RelatedTokenType, out var tokenType))
            {
                obligation.RelatedTokenType = tokenType;
            }
        }
        
        // Parse benefit effects
        if (data.BenefitEffects != null)
        {
            foreach (var effectStr in data.BenefitEffects)
            {
                if (Enum.TryParse<ObligationEffect>(effectStr, out var effect))
                {
                    obligation.BenefitEffects.Add(effect);
                }
            }
        }
        
        // Parse constraint effects
        if (data.ConstraintEffects != null)
        {
            foreach (var effectStr in data.ConstraintEffects)
            {
                if (Enum.TryParse<ObligationEffect>(effectStr, out var effect))
                {
                    obligation.ConstraintEffects.Add(effect);
                }
            }
        }
        
        return obligation;
    }
}

// JSON data structures for deserialization
public class NarrativeJsonData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IntroductionMessage { get; set; }
    public string CompletionMessage { get; set; }
    public StartingConditionsJsonData StartingConditions { get; set; }
    public List<NarrativeStepJsonData> Steps { get; set; }
    public RewardsJsonData Rewards { get; set; }
    public List<string> RestrictedNPCs { get; set; }
}

public class StartingConditionsJsonData
{
    public int? PlayerCoins { get; set; }
    public int? PlayerStamina { get; set; }
    public string StartingLocation { get; set; }
    public string StartingSpot { get; set; }
    public bool ClearInventory { get; set; }
    public bool ClearLetterQueue { get; set; }
    public bool ClearObligations { get; set; }
}

public class NarrativeStepJsonData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string RequiredAction { get; set; }
    public string RequiredLocation { get; set; }
    public string RequiredNPC { get; set; }
    public List<string> AllowedActions { get; set; }
    public string GuidanceText { get; set; }
    public string CompletionFlag { get; set; }
    public string ConversationIntroduction { get; set; }
    public ObligationJsonData ObligationToCreate { get; set; }
}

public class ObligationJsonData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SourceNpcId { get; set; }
    public string RelatedTokenType { get; set; }
    public List<string> BenefitEffects { get; set; }
    public List<string> ConstraintEffects { get; set; }
}

public class RewardsJsonData
{
    public int? Coins { get; set; }
    public int? Stamina { get; set; }
    public List<string> Items { get; set; }
    public string Message { get; set; }
}