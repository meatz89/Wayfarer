using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class NavigationServiceTests
{
    private NavigationService _navigationService;
    private Mock<GameWorld> _mockGameWorld;
    private Mock<ITimeManager> _mockTimeManager;
    private Mock<INavigationHandler> _mockNavigationHandler;
    private Mock<ILogger<NavigationService>> _mockLogger;

    [SetUp]
    public void Setup()
    {
        _mockGameWorld = new Mock<GameWorld>();
        _mockTimeManager = new Mock<ITimeManager>();
        _mockNavigationHandler = new Mock<INavigationHandler>();
        _mockLogger = new Mock<ILogger<NavigationService>>();
        
        _navigationService = new NavigationService(
            _mockGameWorld.Object,
            _mockTimeManager.Object,
            _mockNavigationHandler.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public void NavigateTo_Should_Notify_Handler_On_Successful_Navigation()
    {
        // Arrange
        var targetScreen = CurrentViews.LetterQueueScreen;
        // Time is now part of GameWorld, not needed here

        // Act
        _navigationService.NavigateTo(targetScreen);

        // Assert
        _mockNavigationHandler.Verify(h => h.HandleNavigationChange(
            It.IsAny<CurrentViews>(), 
            targetScreen), 
            Times.Once);
    }

    [Test]
    public void NavigateTo_Should_Update_Current_Screen()
    {
        // Arrange
        var targetScreen = CurrentViews.LocationScreen;

        // Act
        _navigationService.NavigateTo(targetScreen);

        // Assert
        NUnit.Framework.Assert.That(_navigationService.CurrentScreen, Is.EqualTo(targetScreen));
    }

    [Test]
    public void NavigateTo_Should_Track_Navigation_History()
    {
        // Arrange
        var firstScreen = CurrentViews.LetterQueueScreen;
        var secondScreen = CurrentViews.CharacterScreen;

        // Act
        _navigationService.NavigateTo(firstScreen);
        _navigationService.NavigateTo(secondScreen);

        // Assert
        var history = _navigationService.NavigationHistory;
        NUnit.Framework.Assert.That(history.Count, Is.EqualTo(1));
        // NavigationHistory is IReadOnlyCollection, convert to array to access
        var historyArray = new List<CurrentViews>(history).ToArray();
        NUnit.Framework.Assert.That(historyArray[0], Is.EqualTo(firstScreen));
    }

    [Test]
    public void NavigateBack_Should_Return_To_Previous_Screen()
    {
        // Arrange
        var firstScreen = CurrentViews.LetterQueueScreen;
        var secondScreen = CurrentViews.LocationScreen;
        _navigationService.NavigateTo(firstScreen);
        _navigationService.NavigateTo(secondScreen);

        // Act
        _navigationService.NavigateBack();

        // Assert
        NUnit.Framework.Assert.That(_navigationService.CurrentScreen, Is.EqualTo(firstScreen));
        _mockNavigationHandler.Verify(h => h.HandleNavigationChange(
            secondScreen, 
            firstScreen), 
            Times.Once);
    }
}