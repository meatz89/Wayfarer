using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests that validate the entire startup process works correctly.
    /// These tests ensure the game can start without circular dependencies or hangs.
    /// </summary>
    public class StartupValidationTests
    {
        [Fact]
        public async Task Application_CanStartWithoutHanging()
        {
            // Arrange
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<TestStartup>();
                    webBuilder.UseUrls("http://localhost:0"); // Use random port
                });

            // Act & Assert - Should complete without hanging
            using var host = await hostBuilder.StartAsync();
            
            // Verify GameWorld was created
            var gameWorld = host.Services.GetRequiredService<GameWorld>();
            Assert.NotNull(gameWorld);
            
            await host.StopAsync();
        }

        [Fact]
        public void GameWorld_CanBeCreatedDuringPrerendering()
        {
            // This simulates what happens during ServerPrerendered mode
            var services = new ServiceCollection();
            ServiceConfiguration.AddGameServices(services, null, null);
            
            // Build provider (simulates startup)
            var provider = services.BuildServiceProvider();
            
            // Simulate multiple concurrent requests during prerendering
            var tasks = new Task<GameWorld>[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => provider.GetRequiredService<GameWorld>());
            }
            
            // All tasks should complete and return the same instance
            Task.WaitAll(tasks);
            
            var firstInstance = tasks[0].Result;
            foreach (var task in tasks)
            {
                Assert.Same(firstInstance, task.Result);
            }
        }

        [Fact]
        public void AllRepositories_CanBeCreatedWithGameWorld()
        {
            // Arrange
            var services = new ServiceCollection();
            ServiceConfiguration.AddGameServices(services, null, null);
            var provider = services.BuildServiceProvider();
            
            // Act & Assert - All repositories should be resolvable
            Assert.NotNull(provider.GetService<LocationRepository>());
            Assert.NotNull(provider.GetService<NPCRepository>());
            Assert.NotNull(provider.GetService<ItemRepository>());
            Assert.NotNull(provider.GetService<RouteRepository>());
            Assert.NotNull(provider.GetService<LetterTemplateRepository>());
        }

        [Fact]
        public void ContentValidator_CanBeCreatedWithGameWorld()
        {
            // Arrange
            var services = new ServiceCollection();
            ServiceConfiguration.AddGameServices(services, null, null);
            var provider = services.BuildServiceProvider();
            
            // Act
            var contentValidator = provider.GetRequiredService<ContentValidator>();
            
            // Assert
            Assert.NotNull(contentValidator);
            var result = contentValidator.ValidateContent();
            Assert.NotNull(result);
        }

        [Fact]
        public void GameWorldManager_CanBeCreatedWithAllDependencies()
        {
            // Arrange
            var services = new ServiceCollection();
            ServiceConfiguration.AddGameServices(services, null, null);
            ServiceConfiguration.AddTimeSystem(services);
            var provider = services.BuildServiceProvider();
            
            // Act
            var gameWorldManager = provider.GetService<GameWorldManager>();
            
            // Assert
            Assert.NotNull(gameWorldManager);
        }

        [Fact]
        public void NoCircularDependencies_InServiceRegistration()
        {
            // Arrange
            var services = new ServiceCollection();
            ServiceConfiguration.AddGameServices(services, null, null);
            ServiceConfiguration.AddTimeSystem(services);
            ServiceConfiguration.AddAIServices(services);
            
            // Act & Assert - Building the provider validates no circular dependencies
            var provider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
            
            Assert.NotNull(provider);
        }

        private class TestStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddRazorPages();
                services.AddServerSideBlazor();
                ServiceConfiguration.AddGameServices(services, null, null);
                ServiceConfiguration.AddTimeSystem(services);
            }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app.UseStaticFiles();
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapBlazorHub();
                    endpoints.MapFallbackToPage("/_Host");
                });
            }
        }
    }
}