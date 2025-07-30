using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Phase 4: Load narrative definitions from JSON.
/// This includes tutorial narratives and any other narrative content.
/// </summary>
public class Phase4_Narratives : IInitializationPhase
{
    public int PhaseNumber => 4;
    public string Name => "Narrative Definitions";
    public bool IsCritical => false; // Game can run without narratives

    public void Execute(InitializationContext context)
    {
        // Clear any existing narrative definitions (from legacy NarrativeBuilder)
        NarrativeDefinitions.Clear();
        
        // Load narratives from JSON
        LoadNarratives(context);
    }
    
    private void LoadNarratives(InitializationContext context)
    {
        // First try to load individual narrative files
        var tutorialPath = Path.Combine(context.ContentPath, "tutorial_narrative.json");
        
        if (File.Exists(tutorialPath))
        {
            try
            {
                var json = File.ReadAllText(tutorialPath);
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>
                    {
                        new FlexibleEnumConverter<EffectType>(),
                        new FlexibleEnumConverter<ConditionType>()
                    }
                };
                var narrative = JsonConvert.DeserializeObject<NarrativeDefinition>(json, settings);
                
                if (narrative != null)
                {
                    // Ensure all collections are initialized
                    narrative.Steps = narrative.Steps ?? new List<NarrativeStep>();
                    narrative.StartingConditions = narrative.StartingConditions ?? new List<NarrativeCondition>();
                    narrative.StartingEffects = narrative.StartingEffects ?? new List<NarrativeEffect>();
                    narrative.CompletionRewards = narrative.CompletionRewards ?? new List<NarrativeEffect>();
                    
                    // Initialize step collections
                    foreach (var step in narrative.Steps)
                    {
                        step.AllowedActions = step.AllowedActions ?? new List<string>();
                        step.VisibleNPCs = step.VisibleNPCs ?? new List<string>();
                        step.VisibleLocations = step.VisibleLocations ?? new List<string>();
                        step.DialogueOverrides = step.DialogueOverrides ?? new Dictionary<string, string>();
                        step.CompletionRequirements = step.CompletionRequirements ?? new List<NarrativeCondition>();
                        step.CompletionEffects = step.CompletionEffects ?? new List<NarrativeEffect>();
                        
                        // Map JSON properties that might have different names
                        if (step.VisibleSpots != null && step.VisibleSpots.Any())
                        {
                            // Store visible spots in shared data for other systems to use
                            context.SharedData[$"narrative_{narrative.Id}_step_{step.Id}_visibleSpots"] = step.VisibleSpots;
                        }
                    }
                    
                    NarrativeDefinitions.Add(narrative);
                    Console.WriteLine($"  Loaded narrative: {narrative.Id} - {narrative.Title}");
                    Console.WriteLine($"    Steps: {narrative.Steps.Count}");
                    
                    // Log first few steps for debugging
                    for (int i = 0; i < Math.Min(3, narrative.Steps.Count); i++)
                    {
                        var step = narrative.Steps[i];
                        Console.WriteLine($"    Step {i + 1}: {step.Id} - {step.Name}");
                        if (step.DialogueOverrides.Any())
                        {
                            Console.WriteLine($"      Dialogue overrides: {string.Join(", ", step.DialogueOverrides.Keys)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.Errors.Add($"Failed to load tutorial_narrative.json: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("INFO: tutorial_narrative.json not found");
        }
        
        // Also try to load from a narratives array file
        var narrativesPath = Path.Combine(context.ContentPath, "narratives.json");
        if (File.Exists(narrativesPath))
        {
            try
            {
                var json = File.ReadAllText(narrativesPath);
                var narratives = JsonConvert.DeserializeObject<List<NarrativeDefinition>>(json);
                
                if (narratives != null && narratives.Any())
                {
                    foreach (var narrative in narratives)
                    {
                        NarrativeDefinitions.Add(narrative);
                        Console.WriteLine($"  Loaded narrative: {narrative.Id} - {narrative.Title}");
                    }
                }
            }
            catch (Exception ex)
            {
                context.Warnings.Add($"Failed to load narratives.json: {ex.Message}");
            }
        }
        
        Console.WriteLine($"Loaded {NarrativeDefinitions.All.Count} narrative definitions total");
    }
}

// Extension to NarrativeStep to handle JSON properties
public partial class NarrativeStep
{
    // Additional properties from JSON that aren't in the base class
    [JsonProperty("visibleSpots")]
    public List<string> VisibleSpots { get; set; } = new List<string>();
}