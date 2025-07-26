using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Wayfarer.GameState.Constants;

namespace Wayfarer.Tests;

public class CollapseManagerTests
{
    private readonly Mock<ITimeManager> _mockTimeManager;
    private readonly MessageSystem _messageSystem;
    private readonly Mock<RestManager> _mockRestManager;
    private readonly Mock<ILogger<CollapseManager>> _mockLogger;
    private readonly GameWorld _gameWorld;
    private readonly CollapseManager _collapseManager;
    private readonly Player _player;

    public CollapseManagerTests()
    {
        _gameWorld = new GameWorld();
        _player = _gameWorld.GetPlayer();
        
        _mockTimeManager = new Mock<ITimeManager>();
        _messageSystem = new MessageSystem(_gameWorld);
        _mockRestManager = new Mock<RestManager>(null, null, null, null);
        _mockLogger = new Mock<ILogger<CollapseManager>>();
        
        _collapseManager = new CollapseManager(
            _gameWorld,
            _mockTimeManager.Object,
            _messageSystem,
            _mockRestManager.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public void CheckAndHandleCollapse_WhenStaminaAboveZero_DoesNotCollapse()
    {
        // Arrange
        _player.Stamina = 5;
        _player.MaxStamina = 10;
        
        // Act
        bool collapsed = _collapseManager.CheckAndHandleCollapse();
        
        // Assert
        Assert.False(collapsed);
        _mockTimeManager.Verify(tm => tm.AdvanceTime(It.IsAny<int>()), Times.Never);
        Assert.DoesNotContain(_gameWorld.SystemMessages, 
            msg => msg.Message.Contains("collapse"));
    }

    [Fact]
    public void CheckAndHandleCollapse_WhenStaminaReachesZero_TriggersCollapse()
    {
        // Arrange
        _player.Stamina = 0;
        _player.MaxStamina = 10;
        _player.Coins = 10;
        _player.CurrentLocation = new Location("test_location", "Test Location");
        
        _gameWorld.CurrentDay = 1;
        
        // Act
        bool collapsed = _collapseManager.CheckAndHandleCollapse();
        
        // Assert
        Assert.True(collapsed);
        
        // Verify time advanced by 4 hours
        _mockTimeManager.Verify(tm => tm.AdvanceTime(4), Times.Once);
        
        // Verify collapse messages were shown
        Assert.Contains(_gameWorld.SystemMessages, 
            msg => msg.Message.Contains("vision blurs") && msg.Type == SystemMessageTypes.Danger);
        
        Assert.Contains(_gameWorld.SystemMessages, 
            msg => msg.Message.Contains("collapse from exhaustion") && msg.Type == SystemMessageTypes.Danger);
        
        // Verify player recovered some stamina
        Assert.Equal(2, _player.Stamina);
        
        // Verify some coins were lost (between 1 and 3 coins)
        Assert.InRange(_player.Coins, 7, 9);
    }

    [Fact]
    public void CheckAndHandleCollapse_WhenPlayerHasNoCoins_DoesNotLoseCoins()
    {
        // Arrange
        _player.Stamina = 0;
        _player.MaxStamina = 10;
        _player.Coins = 0;
        _player.CurrentLocation = new Location("test_location", "Test Location");
        
        // Act
        bool collapsed = _collapseManager.CheckAndHandleCollapse();
        
        // Assert
        Assert.True(collapsed);
        Assert.Equal(0, _player.Coins); // No coins to lose
        
        // Should not show coin loss message
        Assert.DoesNotContain(_gameWorld.SystemMessages, 
            msg => msg.Message.Contains("coins are missing"));
    }

    [Fact]
    public void IsAtRiskOfCollapse_WhenStaminaIsOne_ReturnsTrue()
    {
        // Arrange
        _player.Stamina = 1;
        
        // Act
        bool atRisk = _collapseManager.IsAtRiskOfCollapse();
        
        // Assert
        Assert.True(atRisk);
    }

    [Fact]
    public void IsAtRiskOfCollapse_WhenStaminaIsTwo_ReturnsTrue()
    {
        // Arrange
        _player.Stamina = 2;
        
        // Act
        bool atRisk = _collapseManager.IsAtRiskOfCollapse();
        
        // Assert
        Assert.True(atRisk);
    }

    [Fact]
    public void IsAtRiskOfCollapse_WhenStaminaIsZero_ReturnsFalse()
    {
        // Arrange
        _player.Stamina = 0;
        
        // Act
        bool atRisk = _collapseManager.IsAtRiskOfCollapse();
        
        // Assert
        Assert.False(atRisk); // Already collapsed
    }

    [Fact]
    public void IsAtRiskOfCollapse_WhenStaminaIsHigh_ReturnsFalse()
    {
        // Arrange
        _player.Stamina = 5;
        
        // Act
        bool atRisk = _collapseManager.IsAtRiskOfCollapse();
        
        // Assert
        Assert.False(atRisk);
    }

    [Fact]
    public void GetLowStaminaWarning_WhenStaminaIsOne_ReturnsUrgentWarning()
    {
        // Arrange
        _player.Stamina = 1;
        
        // Act
        string warning = _collapseManager.GetLowStaminaWarning();
        
        // Assert
        Assert.NotNull(warning);
        Assert.Contains("verge of collapse", warning);
        Assert.Contains("Rest immediately", warning);
    }

    [Fact]
    public void GetLowStaminaWarning_WhenStaminaIsTwo_ReturnsWarning()
    {
        // Arrange
        _player.Stamina = 2;
        
        // Act
        string warning = _collapseManager.GetLowStaminaWarning();
        
        // Assert
        Assert.NotNull(warning);
        Assert.Contains("exhausted", warning);
    }

    [Fact]
    public void HandleCollapse_WithLocationSpot_GeneratesContextualNarrative()
    {
        // Arrange
        _player.Stamina = 0;
        _player.Coins = 20;
        
        var location = new Location("millbrook", "Millbrook");
        var locationSpot = new LocationSpot("tavern", "The Rusty Mug Tavern");
        
        _player.CurrentLocation = location;
        _player.CurrentLocationSpot = locationSpot;
        
        // Act
        _collapseManager.CheckAndHandleCollapse();
        
        // Assert - Should generate tavern-specific narrative
        Assert.Contains(_gameWorld.SystemMessages, 
            msg => msg.Message.Contains("tavern table") && msg.Message.Contains("barkeep") 
                && msg.Type == SystemMessageTypes.Warning);
    }

    [Fact]
    public void HandleCollapse_AddsMemoryOfCollapse()
    {
        // Arrange
        _player.Stamina = 0;
        _player.Coins = 10;
        _player.CurrentLocation = new Location("test", "Test");
        _gameWorld.CurrentDay = 5;
        
        // Act
        _collapseManager.CheckAndHandleCollapse();
        
        // Assert
        Assert.Single(_player.Memories);
        var memory = _player.Memories[0];
        Assert.StartsWith("collapse_day_", memory.Key);
        Assert.Contains("Collapsed from exhaustion", memory.Description);
        Assert.Equal(5, memory.CreationDay);
    }
}