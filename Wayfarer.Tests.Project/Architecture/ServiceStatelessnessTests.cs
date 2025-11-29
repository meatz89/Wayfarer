using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Service Statelessness compliance (HIGHLANDER principle).
/// Services contain logic, not state. All state belongs in GameWorld.
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
    private static readonly HashSet<string> AllowedStatefulTypes = new HashSet<string>
    {
        "TimeManager",      // Time is global mutable state (by design)
        "GameFacade",       // Coordinator, holds references to state containers
        "StreamingContentState" // Content loading state
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
        violations = violations
            .Where(v => !v.Contains("_world"))          // GameWorld dependency is okay
            .Where(v => !v.Contains("_player"))         // Player dependency is okay
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
    /// Verify services receive state through method parameters.
    /// </summary>
    [Fact]
    public void Services_ReceiveState_ThroughMethodParameters()
    {
        Assembly assembly = typeof(GameWorld).Assembly;

        foreach (Type type in GetServiceTypes(assembly))
        {
            if (AllowedStatefulTypes.Contains(type.Name)) continue;

            MethodInfo[] publicMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            int methodsReceivingState = 0;
            int totalBusinessMethods = 0;

            foreach (MethodInfo method in publicMethods)
            {
                // Skip property accessors and standard methods
                if (method.IsSpecialName) continue;
                if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) continue;
                if (method.Name == "ToString" || method.Name == "GetHashCode" || method.Name == "Equals") continue;

                totalBusinessMethods++;

                // Check if method receives state objects as parameters
                bool receivesState = method.GetParameters().Any(p =>
                    p.ParameterType == typeof(Player) ||
                    p.ParameterType == typeof(GameWorld) ||
                    p.ParameterType == typeof(NPC) ||
                    p.ParameterType == typeof(Location) ||
                    p.Name.Contains("context", StringComparison.OrdinalIgnoreCase));

                if (receivesState) methodsReceivingState++;
            }

            // Services with business methods should receive state parameters
            // (allows for coordinator/orchestration services that don't)
            if (totalBusinessMethods > 3)
            {
                Assert.True(methodsReceivingState > 0,
                    $"Service {type.Name} has {totalBusinessMethods} business methods but none receive state parameters");
            }
        }
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
        HashSet<string> dependencyTypeNames = new HashSet<string>
        {
            "GameWorld", "Player", "TimeManager", "EntityResolver",
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
