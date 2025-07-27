using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// CRITICAL: These tests ensure GameWorld initialization remains static.
    /// This is fundamental to the entire game startup architecture.
    /// DO NOT modify GameWorldInitializer to be non-static or require DI.
    /// </summary>
    public class GameWorldInitializationTests
    {
        [Fact]
        public void GameWorldInitializer_MustBeStaticClass()
        {
            // Arrange
            var type = typeof(GameWorldInitializer);

            // Assert
            Assert.True(type.IsAbstract && type.IsSealed, 
                "GameWorldInitializer MUST be a static class. This is critical for startup architecture.");
        }

        [Fact]
        public void CreateGameWorld_MustBeStaticMethod()
        {
            // Arrange
            var type = typeof(GameWorldInitializer);
            var method = type.GetMethod("CreateGameWorld", BindingFlags.Public | BindingFlags.Static);

            // Assert
            Assert.NotNull(method);
            Assert.True(method.IsStatic, 
                "CreateGameWorld MUST be a static method. This prevents circular dependencies during startup.");
        }

        [Fact]
        public void CreateGameWorld_MustNotHaveParameters()
        {
            // Arrange
            var type = typeof(GameWorldInitializer);
            var method = type.GetMethod("CreateGameWorld", BindingFlags.Public | BindingFlags.Static);

            // Assert
            Assert.NotNull(method);
            Assert.Empty(method.GetParameters());
        }

        [Fact]
        public void CreateGameWorld_MustReturnGameWorld()
        {
            // Arrange
            var type = typeof(GameWorldInitializer);
            var method = type.GetMethod("CreateGameWorld", BindingFlags.Public | BindingFlags.Static);

            // Assert
            Assert.NotNull(method);
            Assert.Equal(typeof(GameWorld), method.ReturnType);
        }

        [Fact]
        public void GameWorldInitializer_MustNotHaveInstanceConstructor()
        {
            // Arrange
            var type = typeof(GameWorldInitializer);
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Assert
            Assert.Empty(constructors);
        }

        [Fact]
        public void GameWorldInitializer_MustNotImplementInterfaces()
        {
            // Arrange
            var type = typeof(GameWorldInitializer);

            // Assert
            Assert.Empty(type.GetInterfaces());
        }

        [Fact]
        public void GameWorld_CanBeCreatedWithoutDependencyInjection()
        {
            // Act & Assert - This should not throw
            var gameWorld = GameWorldInitializer.CreateGameWorld();
            
            Assert.NotNull(gameWorld);
            Assert.NotNull(gameWorld.WorldState);
            Assert.NotNull(gameWorld.GetPlayer());
        }

        [Fact]
        public void ServiceConfiguration_MustRegisterGameWorldUsingStaticInitializer()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            ServiceConfiguration.AddGameServices(services, null, null);
            var provider = services.BuildServiceProvider();
            
            // Assert - GameWorld should be resolvable
            var gameWorld = provider.GetService<GameWorld>();
            Assert.NotNull(gameWorld);
        }

        [Fact]
        public void GameWorld_MustBeRegisteredAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            ServiceConfiguration.AddGameServices(services, null, null);
            
            // Act
            var gameWorldDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(GameWorld));
            
            // Assert
            Assert.NotNull(gameWorldDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, gameWorldDescriptor.Lifetime);
        }

        [Fact]
        public void GameWorld_SingletonMustReturnSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            ServiceConfiguration.AddGameServices(services, null, null);
            var provider = services.BuildServiceProvider();
            
            // Act
            var gameWorld1 = provider.GetRequiredService<GameWorld>();
            var gameWorld2 = provider.GetRequiredService<GameWorld>();
            
            // Assert
            Assert.Same(gameWorld1, gameWorld2);
        }
    }
}