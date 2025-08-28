using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Defines the game mode for content loading
/// </summary>
public enum GameMode
{
    MainGame,
    Tutorial
}

/// <summary>
/// Manages the initialization of GameWorld from JSON content in a deterministic order.
/// Ensures all dependencies are created before entities that depend on them.
/// </summary>
public class GameWorldInitializationPipeline
{
    private readonly IContentDirectory _contentDirectory;
    private readonly ValidatedContentLoader _contentLoader;
    private readonly List<IInitializationPhase> _phases;

    public GameWorldInitializationPipeline(IContentDirectory contentDirectory)
    {
        _contentDirectory = contentDirectory;
        _contentLoader = new ValidatedContentLoader();

        // CRITICAL: Order matters! Each phase can only depend on previous phases
        _phases = new List<IInitializationPhase>
        {
            // Phase 0: Card system initialization (must happen first)
            new Phase0_CardSystem(),
            
            // Phase 1: Base entities with no dependencies
            new Phase1_CoreEntities(),
            
            // Phase 2: Entities that depend on locations
            new Phase2_LocationDependents(), 
            
            // Phase 3: Entities that depend on NPCs and locations
            new Phase3_NPCDependents(),
            
            // Phase 5: Complex entities with multiple dependencies
            new Phase5_ComplexEntities(),
            
            // Phase 6: Player initialization (depends on locations and spots)
            new Phase6_PlayerInitialization(),
            
            // Phase 7: Final validation and cross-references
            new Phase7_FinalValidation(),
            
            // Phase 8: Initial letters for mockup UI
            new Phase8_InitialLetters()
        };
    }

    public GameWorld Initialize(GameMode gameMode = GameMode.MainGame)
    {
        Console.WriteLine($"=== GAME WORLD INITIALIZATION PIPELINE ({gameMode}) ===");

        // Create empty GameWorld
        Console.WriteLine("[PIPELINE] Creating new GameWorld instance...");
        GameWorld gameWorld = new GameWorld();
        Console.WriteLine("[PIPELINE] GameWorld instance created");
        InitializationContext context = new InitializationContext
        {
            GameWorld = gameWorld,
            ContentPath = Path.Combine(_contentDirectory.Path, "Templates"),
            ContentLoader = _contentLoader,
            Errors = new List<string>(),
            Warnings = new List<string>(),
            GameMode = gameMode
        };

        // Execute each phase in order
        foreach (IInitializationPhase phase in _phases)
        {
            Console.WriteLine($"\n=== PHASE {phase.PhaseNumber}: {phase.Name} ===");

            try
            {
                phase.Execute(context);

                if (context.Errors.Any())
                {
                    Console.WriteLine($"ERROR: Phase {phase.PhaseNumber} failed with {context.Errors.Count} errors:");
                    foreach (string error in context.Errors)
                    {
                        Console.WriteLine($"  - {error}");
                    }

                    if (phase.IsCritical)
                    {
                        throw new GameWorldInitializationException(
                            $"Critical phase {phase.PhaseNumber} ({phase.Name}) failed",
                            context.Errors);
                    }
                }

                Console.WriteLine($"Phase {phase.PhaseNumber} completed successfully");
            }
            catch (Exception ex) when (!(ex is GameWorldInitializationException))
            {
                Console.WriteLine($"ERROR: Phase {phase.PhaseNumber} threw exception: {ex.Message}");

                if (phase.IsCritical)
                {
                    throw new GameWorldInitializationException(
                        $"Critical phase {phase.PhaseNumber} ({phase.Name}) failed",
                        new[] { ex.Message });
                }
            }

            // Clear errors for next phase (warnings accumulate)
            context.Errors.Clear();
        }

        // Log any warnings
        if (context.Warnings.Any())
        {
            Console.WriteLine($"\n=== INITIALIZATION WARNINGS ({context.Warnings.Count}) ===");
            foreach (string? warning in context.Warnings.Take(10))
            {
                Console.WriteLine($"  - {warning}");
            }
            if (context.Warnings.Count > 10)
            {
                Console.WriteLine($"  ... and {context.Warnings.Count - 10} more warnings");
            }
        }

        Console.WriteLine("\n=== INITIALIZATION COMPLETE ===");
        return gameWorld;
    }
}

/// <summary>
/// Context passed between initialization phases
/// </summary>
public class InitializationContext
{
    public GameWorld GameWorld { get; set; }
    public string ContentPath { get; set; }
    public ValidatedContentLoader ContentLoader { get; set; }
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
    public GameMode GameMode { get; set; } = GameMode.MainGame;

    // Validation tracking - used only during initialization for cross-reference checks
    // This is NOT game state and is discarded after initialization completes
    public ValidationTracker ValidationTracker { get; set; } = new();
    
    // Letter deck repository for loading letter configurations
    public LetterDeckRepository LetterDeckRepository { get; set; }

    /// <summary>
    /// Gets the content path based on game mode
    /// </summary>
    public string GetContentPath()
    {
        return GameMode == GameMode.Tutorial
            ? Path.Combine(ContentPath, "Tutorial")
            : ContentPath;
    }
}

/// <summary>
/// Tracks entity references for validation during initialization only
/// This is discarded after initialization completes
/// </summary>
public class ValidationTracker
{
    // Track obligation source NPCs for validation
    public Dictionary<string, string> ObligationNPCs { get; set; } = new();
    
    // Track route discovery references for validation  
    public Dictionary<string, List<string>> RouteDiscoveryNPCs { get; set; } = new();
    public HashSet<string> RouteDiscoveryRoutes { get; set; } = new();
}

/// <summary>
/// Base interface for initialization phases
/// </summary>
public interface IInitializationPhase
{
    int PhaseNumber { get; }
    string Name { get; }
    bool IsCritical { get; }
    void Execute(InitializationContext context);
}

/// <summary>
/// Exception thrown when initialization fails
/// </summary>
public class GameWorldInitializationException : Exception
{
    public IEnumerable<string> Errors { get; }

    public GameWorldInitializationException(string message, IEnumerable<string> errors)
        : base(message)
    {
        Errors = errors;
    }
}