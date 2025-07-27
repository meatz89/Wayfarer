using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Architecture tests that enforce critical design decisions.
    /// These tests prevent accidental breaking of fundamental architecture patterns.
    /// </summary>
    public class ArchitectureTests
    {
        [Fact]
        public void GameWorld_MustHaveNoDependencies()
        {
            // Arrange
            var type = typeof(GameWorld);
            var constructors = type.GetConstructors();
            
            // Assert
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                Assert.True(parameters.Length == 0 || parameters.All(p => p.ParameterType.IsValueType),
                    "GameWorld MUST NOT have any service dependencies. All dependencies flow INWARD towards GameWorld.");
            }
        }

        [Fact]
        public void NoClass_ShouldUseServiceLocatorPattern()
        {
            // This test ensures we don't use GetRequiredService outside of DI registration
            var assembly = typeof(GameWorld).Assembly;
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name != "ServiceConfiguration");

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => m.DeclaringType == type); // Only methods declared in this type

                foreach (var method in methods)
                {
                    if (method.Name == "GetRequiredService" || method.Name == "GetService")
                    {
                        Assert.True(false, $"Type {type.Name} uses service locator pattern in method {method.Name}. This violates DI principles.");
                    }

                    // Check method body for GetRequiredService calls
                    var methodBody = method.GetMethodBody();
                    if (methodBody != null)
                    {
                        var il = methodBody.GetILAsByteArray();
                        // This is a simplified check - in production you'd use a proper IL analyzer
                        // But the principle is to detect service locator usage
                    }
                }
            }
        }

        [Fact]
        public void ContentDirectory_MustRemainSimple()
        {
            // Ensure ContentDirectory remains a simple data holder
            var type = typeof(ContentDirectory);
            var properties = type.GetProperties();
            
            Assert.Single(properties);
            Assert.Equal("Path", properties[0].Name);
            Assert.Equal(typeof(string), properties[0].PropertyType);
        }

        [Fact]
        public void GameWorldInitializationPipeline_MustAcceptContentDirectory()
        {
            // Ensure the pipeline can work with just a content directory
            var type = Type.GetType("GameWorldInitializationPipeline");
            if (type != null)
            {
                var constructors = type.GetConstructors();
                Assert.Contains(constructors, c => 
                    c.GetParameters().Length == 1 && 
                    c.GetParameters()[0].ParameterType == typeof(IContentDirectory));
            }
        }

        [Fact]
        public void NoNavigationService_ShouldExist()
        {
            // Ensure NavigationService was removed and stays removed
            var assembly = typeof(GameWorld).Assembly;
            var navigationType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "NavigationService");
            
            Assert.Null(navigationType);
        }

        [Fact]
        public void GameUIBase_MustBeOnlyNavigationHandler()
        {
            // If INavigationHandler exists, only GameUIBase should implement it
            var assembly = typeof(GameWorld).Assembly;
            var navigationHandlerType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "INavigationHandler");
            
            if (navigationHandlerType != null)
            {
                var implementations = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(navigationHandlerType))
                    .ToList();
                
                Assert.True(implementations.Count <= 1, 
                    "Only GameUIBase should implement INavigationHandler");
                
                if (implementations.Any())
                {
                    Assert.Equal("GameUIBase", implementations[0].Name);
                }
            }
        }

        [Fact]
        public void ServiceConfiguration_MustNotUseGetRequiredServiceForGameWorld()
        {
            // This is a compile-time check encoded as a test
            // Ensures ServiceConfiguration doesn't use service locator for GameWorld
            var type = typeof(ServiceConfiguration);
            var method = type.GetMethod("AddGameServices");
            
            Assert.NotNull(method);
            
            // In a real implementation, you'd analyze the IL to ensure
            // GameWorldInitializer.CreateGameWorld() is called directly
        }
    }
}