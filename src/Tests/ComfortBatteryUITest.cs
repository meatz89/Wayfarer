using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

public class ComfortBatteryUITest
{
    public static async Task Main(string[] args)
    {
        // This is a quick test to verify the comfort battery UI works
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        try
        {
            // Navigate to the game
            await page.GotoAsync("http://localhost:5000");
            await page.WaitForTimeoutAsync(2000);
            
            // Try to start a conversation to see the comfort battery
            // This would depend on the actual game flow
            Console.WriteLine("Launched browser. Check for comfort battery display in conversations.");
            
            // Take a screenshot
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "comfort-battery-test.png"
            });
            
            Console.WriteLine("Screenshot saved as comfort-battery-test.png");
            
            // Keep browser open for manual inspection
            await page.WaitForTimeoutAsync(30000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        await browser.CloseAsync();
    }
}