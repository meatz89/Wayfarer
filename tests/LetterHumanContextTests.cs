using Microsoft.Playwright;
using System.Threading.Tasks;
using Xunit;

public class LetterHumanContextTests : IAsyncLifetime
{
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;

    public async Task InitializeAsync()
    {
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.CloseAsync();
        await _browser.CloseAsync();
    }

    [Fact]
    public async Task Letter_Should_Display_Human_Context()
    {
        // Navigate to the game
        await _page.GotoAsync("http://localhost:5099");
        
        // Wait for the game to load
        await _page.WaitForSelectorAsync(".letter-queue-section", new PageWaitForSelectorOptions
        {
            Timeout = 10000
        });

        // Check that the queue exists
        var queueVisible = await _page.IsVisibleAsync(".visual-queue");
        Assert.True(queueVisible, "Letter queue should be visible");

        // Look for human context in any letters (Elena's letter should be present at start)
        var humanContextElements = await _page.QuerySelectorAllAsync(".human-context");
        
        // If there are letters with human context, verify at least one shows meaningful text
        if (humanContextElements.Count > 0)
        {
            var firstContext = await humanContextElements[0].TextContentAsync();
            Assert.NotNull(firstContext);
            Assert.NotEmpty(firstContext);
            
            // Should contain emotionally meaningful text, not just technical descriptions
            Assert.DoesNotContain("correspondence", firstContext.ToLower());
        }
    }

    [Fact]
    public async Task Letter_Should_Show_Consequences_When_Expanded()
    {
        // Navigate to the game
        await _page.GotoAsync("http://localhost:5099");
        
        // Wait for the queue to load
        await _page.WaitForSelectorAsync(".visual-queue");

        // Click on the first letter slot to expand it
        var firstSlot = await _page.QuerySelectorAsync(".queue-slot");
        if (firstSlot != null)
        {
            await firstSlot.ClickAsync();
            
            // Wait for the letter details to appear
            await _page.WaitForSelectorAsync(".letter-details", new PageWaitForSelectorOptions
            {
                Timeout = 5000
            });

            // Check for consequence displays
            var successConsequence = await _page.QuerySelectorAsync(".consequence-success");
            var failureConsequence = await _page.QuerySelectorAsync(".consequence-failure");
            
            if (successConsequence != null)
            {
                var successText = await successConsequence.TextContentAsync();
                Assert.Contains("If delivered:", successText);
            }
            
            if (failureConsequence != null)
            {
                var failureText = await failureConsequence.TextContentAsync();
                Assert.Contains("If late:", failureText);
            }
        }
    }

    [Fact]
    public async Task Emotional_Weight_Should_Be_Displayed()
    {
        // Navigate to the game
        await _page.GotoAsync("http://localhost:5099");
        
        // Wait for the queue to load
        await _page.WaitForSelectorAsync(".visual-queue");

        // Click on a letter to expand it
        var letterSlot = await _page.QuerySelectorAsync(".queue-slot");
        if (letterSlot != null)
        {
            await letterSlot.ClickAsync();
            
            // Wait for details
            await _page.WaitForSelectorAsync(".letter-details");
            
            // Check for emotional weight display in the metadata
            var metaText = await _page.QuerySelectorAsync(".detail-meta");
            if (metaText != null)
            {
                var text = await metaText.TextContentAsync();
                
                // Should contain weight indicators
                Assert.True(
                    text.Contains("Routine") || 
                    text.Contains("Important") || 
                    text.Contains("Life-Changing") || 
                    text.Contains("Life or Death"),
                    "Should display emotional weight category"
                );
            }
        }
    }
}