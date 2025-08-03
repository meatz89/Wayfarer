using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Wayfarer.E2ETests.Infrastructure
{
    /// <summary>
    /// Provides a minimal service container for E2E testing with real services.
    /// No mocks - uses actual implementations to test the real system.
    /// </summary>
    public static class TestServiceProvider
    {
        public static IServiceProvider CreateServiceProvider(GameWorld gameWorld)
        {
            ServiceCollection services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder => 
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise during tests
            });
            
            // Register GameWorld as singleton
            services.AddSingleton(gameWorld);
            
            // Register repositories as singletons
            services.AddSingleton<LocationRepository>();
            services.AddSingleton<RouteRepository>();
            services.AddSingleton<NPCRepository>();
            services.AddSingleton<ItemRepository>();
            services.AddSingleton<LetterTemplateRepository>();
            services.AddSingleton<MarketDataRepository>();
            services.AddSingleton<ConversationRepository>();
            services.AddSingleton<StandingObligationRepository>();
            
            // Register core services
            services.AddSingleton<MessageSystem>();
            services.AddSingleton<SpecialLetterService>();
            services.AddSingleton<TokenSpendingService>();
            services.AddSingleton<NPCService>();
            services.AddSingleton<LetterQueueService>();
            services.AddSingleton<LetterOfferService>();
            services.AddSingleton<TimeImpactCalculator>();
            services.AddSingleton<InformationDiscoveryManager>();
            services.AddSingleton<UIMessageBuffer>();
            services.AddSingleton<NPCVisibilityService>();
            services.AddSingleton<NPCLetterOfferService>();
            services.AddSingleton<ConversationStateManager>();
            
            // Register conversation system
            services.AddSingleton<ConversationFactory>();
            services.AddSingleton<INarrativeProvider, DeterministicNarrativeProvider>();
            services.AddSingleton<ConversationChoiceValidator>();
            
            // Register GameFacade
            services.AddSingleton<GameFacade>();
            
            // Build service provider
            IServiceProvider provider = services.BuildServiceProvider();
            
            // Set ServiceLocator for static access (required by some legacy code)
            ServiceLocator.SetServiceProvider(provider);
            
            return provider;
        }
    }
    
    /// <summary>
    /// Simple service locator for tests - mimics the pattern used in production.
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;
        
        public static void SetServiceProvider(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }
        
        public static T GetService<T>()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceProvider not initialized");
                
            return _serviceProvider.GetService<T>() 
                ?? throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
        }
    }
}