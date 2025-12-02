using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Service Statelessness compliance (HIGHLANDER principle).
/// Services contain logic, not state. All state belongs in GameWorld.
///
/// KEY PRINCIPLES:
/// 1. GameWorld: Inject via DI constructor, NEVER pass as method parameter
/// 2. Player: NEVER inject or pass - always access via _gameWorld.Player
///
/// BEHAVIOR-ONLY TESTING (§8.21): Tests verify structural compliance.
/// </summary>
public class ServiceStatelessnessTests
{
    // Services that should be stateless
    private static readonly List<string> StatelessServicePatterns = new List<string>
    {
        "Service", "Activity", "Executor", "Evaluator", "Validator", "Calculator"
    };

    // Known stateful components (by design)
    private static readonly List<string> AllowedStatefulTypes = new List<string>
    {
        "TimeManager",           // Time is global mutable state (by design)
        "GameOrchestrator",      // Coordinator, holds references to state containers
        "StreamingContentState", // Content loading state
        "LoadingStateService",   // UI coordination service for loading indicators (Blazor pattern)
        "MusicService"           // Audio playback state (track queue, playback position - inherently stateful)
    };

    /// <summary>
    /// Verify service classes do not have mutable instance fields.
    /// Services should receive state through method parameters, not store it.
    /// </summary>
    [Fact]
    public void Services_ShouldNot_HaveMutableInstanceFields()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetServiceTypes(assembly))
        {
            if (AllowedStatefulTypes.Contains(type.Name)) continue;

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                // Skip readonly fields (dependencies)
                if (field.IsInitOnly) continue;

                // Skip compiler-generated backing fields for readonly properties
                if (field.Name.StartsWith("<") && field.Name.Contains(">k__BackingField"))
                {
                    // Check if corresponding property is init-only
                    string propName = field.Name.Substring(1, field.Name.IndexOf('>') - 1);
                    PropertyInfo prop = type.GetProperty(propName);
                    if (prop != null && (prop.SetMethod == null || prop.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Any()))
                        continue;
                }

                // Flag mutable fields that aren't dependency references
                if (!field.FieldType.IsInterface && !IsDependencyType(field.FieldType))
                {
                    violations.Add($"{type.Name}.{field.Name} is mutable instance field (services should be stateless)");
                }
            }
        }

        // Filter out injected dependencies and known acceptable patterns
        // NOTE: _player is NOT filtered - Player should NEVER be injected (use _gameWorld.Player)
        violations = violations
            .Where(v => !v.Contains("_world"))          // GameWorld dependency is okay
            .Where(v => !v.Contains("_gameWorld"))      // GameWorld dependency is okay
            .Where(v => !v.Contains("_time"))           // TimeManager dependency is okay
            .Where(v => !v.Contains("_facade"))         // Facade dependencies are okay
            .Where(v => !v.Contains("_resolver"))       // EntityResolver dependency is okay
            .Where(v => !v.Contains("_random"))         // Random is okay (deterministic via seed)
            .ToList();

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify services do not have properties with public setters that store state.
    /// </summary>
    [Fact]
    public void Services_ShouldNot_HaveStatefulProperties()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetServiceTypes(assembly))
        {
            if (AllowedStatefulTypes.Contains(type.Name)) continue;

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo prop in properties)
            {
                // Check for writable properties that look like state
                if (prop.CanWrite && prop.SetMethod.IsPublic)
                {
                    // Skip dependency injection properties
                    if (prop.PropertyType.IsInterface) continue;
                    if (IsDependencyType(prop.PropertyType)) continue;

                    // Flag state-like properties
                    bool looksLikeState =
                        prop.Name.StartsWith("Current") ||
                        prop.Name.StartsWith("Last") ||
                        prop.Name.StartsWith("Pending") ||
                        prop.Name.Contains("State") ||
                        prop.Name.Contains("Session") ||
                        prop.Name.Contains("Result");

                    if (looksLikeState)
                    {
                        violations.Add($"{type.Name}.{prop.Name} has public setter and looks like state (services should be stateless)");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify pending results are stored in GameWorld, not in services.
    /// </summary>
    [Fact]
    public void PendingResults_StoredInGameWorld_NotServices()
    {
        Type gameWorldType = typeof(GameWorld);
        PropertyInfo[] gwProperties = gameWorldType.GetProperties();

        // GameWorld should have pending result properties
        List<string> expectedPendingResults = new List<string>
        {
            "PendingDiscoveryResult",
            "PendingActivationResult",
            "PendingProgressResult",
            "PendingCompleteResult",
            "PendingIntroResult"
        };

        foreach (string resultName in expectedPendingResults)
        {
            PropertyInfo prop = gwProperties.FirstOrDefault(p => p.Name == resultName);
            Assert.NotNull(prop);
        }

        // ObligationActivity should NOT have these properties
        Assembly assembly = typeof(GameWorld).Assembly;
        Type obligationActivityType = assembly.GetTypes()
            .FirstOrDefault(t => t.Name == "ObligationActivity");

        if (obligationActivityType != null)
        {
            PropertyInfo[] activityProps = obligationActivityType.GetProperties();

            foreach (string resultName in expectedPendingResults)
            {
                PropertyInfo prop = activityProps.FirstOrDefault(p => p.Name == resultName);
                Assert.Null(prop);
            }
        }
    }

    /// <summary>
    /// PRINCIPLE: GameWorld should be injected via DI constructor, NEVER passed as method parameter.
    /// Services access GameWorld via their _gameWorld field, not method arguments.
    /// </summary>
    [Fact]
    public void Services_ShouldNot_HaveGameWorldAsMethodParameter()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetServiceTypes(assembly))
        {
            if (AllowedStatefulTypes.Contains(type.Name)) continue;

            // Only check PUBLIC methods - private methods are implementation details
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                // Skip constructors (GameWorld in constructor is correct - DI)
                if (method.IsConstructor) continue;
                // Skip property accessors
                if (method.IsSpecialName) continue;

                foreach (ParameterInfo param in method.GetParameters())
                {
                    if (param.ParameterType == typeof(GameWorld))
                    {
                        violations.Add($"{type.Name}.{method.Name}({param.Name}) - GameWorld should be injected via constructor, not passed as parameter");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// PRINCIPLE: Player should NEVER be passed as method parameter.
    /// Services access Player via _gameWorld.GetPlayer(), not method arguments.
    /// </summary>
    [Fact]
    public void Services_ShouldNot_HavePlayerAsMethodParameter()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetServiceTypes(assembly))
        {
            if (AllowedStatefulTypes.Contains(type.Name)) continue;

            // Only check PUBLIC methods - private methods are implementation details
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                // Skip property accessors
                if (method.IsSpecialName) continue;

                foreach (ParameterInfo param in method.GetParameters())
                {
                    if (param.ParameterType == typeof(Player))
                    {
                        violations.Add($"{type.Name}.{method.Name}({param.Name}) - Player should be accessed via _gameWorld.GetPlayer(), not passed as parameter");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// PRINCIPLE: Player should NEVER be injected via constructor.
    /// Services should access Player via _gameWorld.Player instead.
    /// </summary>
    [Fact]
    public void Services_ShouldNot_InjectPlayerViaConstructor()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        foreach (Type type in GetServiceTypes(assembly))
        {
            if (AllowedStatefulTypes.Contains(type.Name)) continue;

            // Check constructor parameters
            ConstructorInfo[] constructors = type.GetConstructors();

            foreach (ConstructorInfo ctor in constructors)
            {
                foreach (ParameterInfo param in ctor.GetParameters())
                {
                    if (param.ParameterType == typeof(Player))
                    {
                        violations.Add($"{type.Name} constructor injects Player - use _gameWorld.Player instead");
                    }
                }
            }

            // Check for Player field (injected dependency)
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(Player))
                {
                    violations.Add($"{type.Name}.{field.Name} stores Player reference - use _gameWorld.Player instead");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify executors are stateless (execute logic, return results).
    /// </summary>
    [Fact]
    public void Executors_AreStateless()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        List<Type> executorTypes = assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Executor"))
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (Type executorType in executorTypes)
        {
            // Check for state-tracking fields
            FieldInfo[] fields = executorType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                if (field.IsInitOnly) continue;
                if (field.FieldType.IsInterface) continue;

                // Look for state-like fields
                if (field.Name.Contains("last", StringComparison.OrdinalIgnoreCase) ||
                    field.Name.Contains("current", StringComparison.OrdinalIgnoreCase) ||
                    field.Name.Contains("pending", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"Executor {executorType.Name}.{field.Name} appears to store state");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify activities are stateless.
    /// </summary>
    [Fact]
    public void Activities_AreStateless()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        List<Type> activityTypes = assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Activity"))
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (Type activityType in activityTypes)
        {
            PropertyInfo[] properties = activityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo prop in properties)
            {
                // Skip readonly/init-only properties
                if (!prop.CanWrite) continue;
                if (prop.SetMethod != null && !prop.SetMethod.IsPublic) continue;

                // Look for state-like properties
                if (prop.Name.StartsWith("Pending") ||
                    prop.Name.StartsWith("Last") ||
                    prop.Name.StartsWith("Current") ||
                    prop.Name.Contains("Result"))
                {
                    violations.Add($"Activity {activityType.Name}.{prop.Name} appears to store state (move to GameWorld)");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify evaluators are pure functions (no side effects or state).
    /// </summary>
    [Fact]
    public void Evaluators_ArePureFunctions()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        List<Type> evaluatorTypes = assembly.GetTypes()
            .Where(t => t.Name.EndsWith("Evaluator"))
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (Type evaluatorType in evaluatorTypes)
        {
            // Evaluators should have NO writable properties
            PropertyInfo[] properties = evaluatorType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.CanWrite && prop.SetMethod.IsPublic && !prop.SetMethod.IsAbstract)
                {
                    if (!prop.PropertyType.IsInterface && !IsDependencyType(prop.PropertyType))
                    {
                        violations.Add($"Evaluator {evaluatorType.Name}.{prop.Name} has public setter (evaluators should be pure)");
                    }
                }
            }

            // Evaluators should have NO mutable instance fields
            FieldInfo[] fields = evaluatorType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                if (!field.IsInitOnly && !field.FieldType.IsInterface && !IsDependencyType(field.FieldType))
                {
                    violations.Add($"Evaluator {evaluatorType.Name}.{field.Name} is mutable field (evaluators should be pure)");
                }
            }
        }

        Assert.Empty(violations);
    }

    // ========== HELPER METHODS ==========

    private List<Type> GetServiceTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => StatelessServicePatterns.Any(p => t.Name.EndsWith(p)))
            .ToList();
    }

    private bool IsDependencyType(Type type)
    {
        // Types that are injected dependencies (not state)
        // NOTE: Player is NOT here - Player should NEVER be injected (use _gameWorld.Player)
        List<string> dependencyTypeNames = new List<string>
        {
            "GameWorld", "TimeManager", "EntityResolver",
            "Random", "ILogger", "HttpClient"
        };

        return dependencyTypeNames.Contains(type.Name) ||
               type.IsInterface ||
               type.Name.EndsWith("Service") ||
               type.Name.EndsWith("Facade") ||
               type.Name.EndsWith("Factory") ||
               type.Name.EndsWith("Manager");
    }
}
