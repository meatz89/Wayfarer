using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

public class TutorialStartupTests
{
    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "useMemory", "false" },
                { "processStateChanges", "true" },
                { "DefaultAIProvider", "Ollama" }
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        
        services.ConfigureServices();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void GameWorldManager_InitializeTutorialIfNeeded_StartsTutorial()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var narrativeManager = provider.GetRequiredService<NarrativeManager>();
        var flagService = provider.GetRequiredService<FlagService>();
        
        // Act - Call InitializeTutorialIfNeeded directly
        // First load narrative definitions
        NarrativeContentBuilder.BuildAllNarratives();
        narrativeManager.LoadNarrativeDefinitions(NarrativeDefinitions.All);
        
        // Start the tutorial
        narrativeManager.StartNarrative("wayfarer_tutorial");
        
        // Assert
        Assert.True(flagService.HasFlag("narrative_wayfarer_tutorial_started"), "Tutorial started flag should be set");
        Assert.True(narrativeManager.IsNarrativeActive("wayfarer_tutorial"), "Tutorial narrative should be active");
        
        var activeNarratives = narrativeManager.GetActiveNarratives();
        Assert.Contains("wayfarer_tutorial", activeNarratives);
        
        var currentStep = narrativeManager.GetCurrentStep("wayfarer_tutorial");
        Assert.NotNull(currentStep);
        Assert.Equal("day1_wake", currentStep.Id);
        Assert.Equal("Awakening", currentStep.Name);
    }
    
    [Fact]
    public void NarrativeManager_LoadsWayfarerTutorial()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var narrativeManager = provider.GetRequiredService<NarrativeManager>();
        
        // Act
        NarrativeContentBuilder.BuildAllNarratives();
        narrativeManager.LoadNarrativeDefinitions(NarrativeDefinitions.All);
        
        // Assert
        var tutorialDef = narrativeManager.GetNarrativeDefinition("wayfarer_tutorial");
        Assert.NotNull(tutorialDef);
        Assert.Equal("From Destitute to Patronage", tutorialDef.Title);
        Assert.NotEmpty(tutorialDef.Steps);
        Assert.Equal(23, tutorialDef.Steps.Count); // Tutorial has 23 steps
    }
    
    [Fact]
    public void Tutorial_FirstStep_HasCorrectConfiguration()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var narrativeManager = provider.GetRequiredService<NarrativeManager>();
        var gameWorldManager = provider.GetRequiredService<GameWorldManager>();
        
        // Act
        gameWorldManager.StartGame().GetAwaiter().GetResult();
        
        var currentStep = narrativeManager.GetCurrentStep("wayfarer_tutorial");
        
        // Assert
        Assert.NotNull(currentStep);
        Assert.Equal("day1_wake", currentStep.Id);
        Assert.Equal("Awakening", currentStep.Name);
        Assert.Contains("You wake in the Abandoned Warehouse", currentStep.Description);
        Assert.Contains("You need to gather your strength", currentStep.GuidanceText);
        Assert.Single(currentStep.AllowedActions);
        Assert.Contains("Rest", currentStep.AllowedActions);
        Assert.Empty(currentStep.VisibleNPCs); // No NPCs visible in first step
    }
    
    [Fact]
    public void Tutorial_StartsWithCorrectPlayerState()
    {
        // Arrange
        var provider = CreateServiceProvider();
        var gameWorldManager = provider.GetRequiredService<GameWorldManager>();
        var gameWorld = provider.GetRequiredService<GameWorld>();
        
        // Act
        gameWorldManager.StartGame().GetAwaiter().GetResult();
        var player = gameWorld.GetPlayer();
        
        // Assert
        Assert.Equal(2, player.Coins); // Tutorial starts with 2 coins
        Assert.Equal(4, player.Stamina); // Tutorial starts with 4/10 stamina
        Assert.Equal("abandoned_warehouse", player.CurrentLocationSpot?.SpotID);
        Assert.Equal("lower_ward", player.CurrentLocationSpot?.LocationId);
    }
}