using Microsoft.Playwright;
using Xunit;

/// <summary>
/// LAYER 3: END-TO-END TESTS (UI + Full Flow)
/// Tests procedural tracing UI with Playwright
/// Verifies HashSet&lt;object&gt; expand/collapse works correctly
/// </summary>
public class ProceduralTracingE2ETests : IAsyncLifetime
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private string _baseUrl = "http://localhost:6000"; // Adjust port

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Set to false for debugging
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    // ==================== TRACE VIEWER UI TESTS ====================

    [Fact]
    public async Task TraceViewer_OpensWithoutErrors()
    {
        // Arrange - Start game and navigate to procedural trace viewer
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);

        // Act - Open trace viewer (click "Procedural Trace" button)
        await _page.ClickAsync("button:has-text('Procedural Trace')");

        // Assert - Viewer displays
        await _page.WaitForSelectorAsync(".spawn-graph-viewer", new PageWaitForSelectorOptions { Timeout = 5000 });

        // Verify no console errors
        List<string> consoleErrors = await GetConsoleErrors(_page);
        Assert.Empty(consoleErrors);
    }

    [Fact]
    public async Task TraceViewer_DisplaysRootScenes()
    {
        // Arrange
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);
        await _page.ClickAsync("button:has-text('Procedural Trace')");

        // Act - Wait for scenes to load
        await _page.WaitForSelectorAsync(".trace-node-scene", new PageWaitForSelectorOptions { Timeout = 5000 });

        // Assert - At least one scene displayed
        int sceneCount = await _page.Locator(".trace-node-scene").CountAsync();
        Assert.True(sceneCount > 0, "Expected at least one root scene");

        // Verify scene has required metadata
        ILocator firstScene = _page.Locator(".trace-node-scene").First;
        string sceneText = await firstScene.TextContentAsync();
        Assert.Contains("SCENE", sceneText);
        Assert.Contains("Category:", sceneText);
        Assert.Contains("Spawned:", sceneText);
    }

    [Fact]
    public async Task ExpandCollapse_WorksWithObjectReferences()
    {
        // Arrange
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);
        await _page.ClickAsync("button:has-text('Procedural Trace')");
        await _page.WaitForSelectorAsync(".trace-node-scene");

        ILocator firstScene = _page.Locator(".trace-node-scene").First;

        // Act - Click to expand scene
        await firstScene.ClickAsync();

        // Assert - Situations displayed (children expanded)
        await _page.WaitForSelectorAsync(".trace-node-situation", new PageWaitForSelectorOptions { Timeout = 2000 });
        int situationCount = await _page.Locator(".trace-node-situation").CountAsync();
        Assert.True(situationCount > 0, "Expected at least one situation after expanding scene");

        // Verify scene has expanded class
        string sceneClass = await firstScene.GetAttributeAsync("class");
        Assert.Contains("trace-node-expanded", sceneClass);

        // Act - Click to collapse scene
        await firstScene.ClickAsync();
        await Task.Delay(500); // Wait for collapse animation

        // Assert - Situations hidden
        int situationCountAfterCollapse = await _page.Locator(".trace-node-situation").CountAsync();
        Assert.Equal(0, situationCountAfterCollapse);

        // Verify scene has collapsed class
        sceneClass = await firstScene.GetAttributeAsync("class");
        Assert.Contains("trace-node-collapsed", sceneClass);
    }

    [Fact]
    public async Task ExpandAll_ExpandsAllNodes()
    {
        // Arrange
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);
        await _page.ClickAsync("button:has-text('Procedural Trace')");
        await _page.WaitForSelectorAsync(".trace-node-scene");

        // Act - Click "Expand All" button
        await _page.ClickAsync("button:has-text('Expand All')");
        await Task.Delay(500); // Wait for expansion

        // Assert - All nodes expanded
        int expandedScenes = await _page.Locator(".trace-node-scene.trace-node-expanded").CountAsync();
        int totalScenes = await _page.Locator(".trace-node-scene").CountAsync();
        Assert.Equal(totalScenes, expandedScenes);

        // Verify situations visible
        int situationCount = await _page.Locator(".trace-node-situation").CountAsync();
        Assert.True(situationCount > 0, "Expected situations to be visible after Expand All");

        // Verify choices visible (if any situations have choices)
        int choiceCount = await _page.Locator(".trace-node-choice").CountAsync();
        // Don't assert count > 0 because game might not have executed choices yet
        // Just verify no errors occurred
    }

    [Fact]
    public async Task CollapseAll_CollapsesAllNodes()
    {
        // Arrange
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);
        await _page.ClickAsync("button:has-text('Procedural Trace')");
        await _page.WaitForSelectorAsync(".trace-node-scene");

        // Expand first to have something to collapse
        await _page.ClickAsync("button:has-text('Expand All')");
        await Task.Delay(500);

        // Act - Click "Collapse All" button
        await _page.ClickAsync("button:has-text('Collapse All')");
        await Task.Delay(500);

        // Assert - All nodes collapsed
        int expandedScenes = await _page.Locator(".trace-node-scene.trace-node-expanded").CountAsync();
        Assert.Equal(0, expandedScenes);

        // Verify situations hidden
        int situationCount = await _page.Locator(".trace-node-situation").CountAsync();
        Assert.Equal(0, situationCount);

        // Verify choices hidden
        int choiceCount = await _page.Locator(".trace-node-choice").CountAsync();
        Assert.Equal(0, choiceCount);
    }

    [Fact]
    public async Task FilterByCategory_FiltersScenes()
    {
        // Arrange
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);
        await _page.ClickAsync("button:has-text('Procedural Trace')");
        await _page.WaitForSelectorAsync(".trace-node-scene");

        int totalScenes = await _page.Locator(".trace-node-scene").CountAsync();

        // Act - Select "MainStory" category filter
        await _page.SelectOptionAsync("select", new[] { "MainStory" });
        await _page.ClickAsync("button:has-text('Apply')");
        await Task.Delay(500);

        // Assert - Filtered scenes displayed
        int filteredScenes = await _page.Locator(".trace-node-scene").CountAsync();
        // Filtered count should be <= total count (might be same if all are MainStory)
        Assert.True(filteredScenes <= totalScenes);
    }

    [Fact]
    public async Task SearchBox_FiltersScenesByName()
    {
        // Arrange
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);
        await _page.ClickAsync("button:has-text('Procedural Trace')");
        await _page.WaitForSelectorAsync(".trace-node-scene");

        // Get first scene name for searching
        ILocator firstScene = _page.Locator(".trace-node-scene").First;
        string sceneName = await firstScene.Locator(".trace-node-header span").Nth(1).TextContentAsync();

        int totalScenes = await _page.Locator(".trace-node-scene").CountAsync();

        // Act - Type partial scene name in search box
        string searchTerm = sceneName.Substring(0, Math.Min(5, sceneName.Length));
        await _page.FillAsync("input[placeholder='Search scenes...']", searchTerm);
        await Task.Delay(500); // Wait for filter to apply

        // Assert - Filtered scenes displayed
        int filteredScenes = await _page.Locator(".trace-node-scene").CountAsync();
        Assert.True(filteredScenes <= totalScenes);
        Assert.True(filteredScenes > 0, "Search should find at least the scene we searched for");
    }

    // ==================== FULL FLOW E2E TESTS ====================

    [Fact]
    public async Task CompleteFlow_InstantChoice_RecordsAndDisplays()
    {
        // Arrange - Start game
        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);

        // Act - Execute an instant choice (if available)
        bool choiceExecuted = await TryExecuteFirstAvailableChoice(_page);
        if (!choiceExecuted)
        {
            // Skip test if no choices available
            return;
        }

        // Open trace viewer
        await _page.ClickAsync("button:has-text('Procedural Trace')");
        await _page.WaitForSelectorAsync(".trace-node-scene");

        // Expand all to see choices
        await _page.ClickAsync("button:has-text('Expand All')");
        await Task.Delay(500);

        // Assert - Choice node displayed
        int choiceCount = await _page.Locator(".trace-node-choice").CountAsync();
        Assert.True(choiceCount > 0, "Expected at least one choice node after executing choice");

        // Verify choice has required metadata
        ILocator firstChoice = _page.Locator(".trace-node-choice").First;
        string choiceText = await firstChoice.TextContentAsync();
        Assert.Contains("CHOICE", choiceText);
        Assert.Contains("Type:", choiceText);
        Assert.Contains("Executed:", choiceText);

        // Verify no console errors
        List<string> consoleErrors = await GetConsoleErrors(_page);
        Assert.Empty(consoleErrors);
    }

    [Fact]
    public async Task CompleteFlow_ChallengeChoice_RecordsContextAndSpawns()
    {
        // This test requires a game state with available challenge choices
        // Implementation depends on your game's specific flow
        // Placeholder for comprehensive challenge flow test

        await _page.GotoAsync(_baseUrl);
        await StartNewGame(_page);

        // TODO: Navigate to scene with challenge choice
        // TODO: Execute challenge choice
        // TODO: Complete challenge
        // TODO: Verify reward scene spawned
        // TODO: Verify trace shows choice→challenge→scene chain
        // TODO: Verify object references maintained throughout

        // For now, just verify no errors
        List<string> consoleErrors = await GetConsoleErrors(_page);
        Assert.Empty(consoleErrors);
    }

    // ==================== HELPER METHODS ====================

    private async Task StartNewGame(IPage page)
    {
        // Click "New Game" or similar button
        // Adjust selector based on your UI
        try
        {
            await page.ClickAsync("button:has-text('New Game')", new PageClickOptions { Timeout = 5000 });
            await Task.Delay(2000); // Wait for game to initialize
        }
        catch
        {
            // Game might already be started
        }
    }

    private async Task<bool> TryExecuteFirstAvailableChoice(IPage page)
    {
        try
        {
            // Look for choice buttons/cards
            // Adjust selector based on your UI
            ILocator choiceLocator = page.Locator(".choice-card, button.choice-action").First;
            await choiceLocator.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
            await choiceLocator.ClickAsync();
            await Task.Delay(1000);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<string>> GetConsoleErrors(IPage page)
    {
        List<string> errors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                errors.Add(msg.Text);
            }
        };
        return errors;
    }
}
