using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Wayfarer.Controllers;

[ApiController]
[Route("api/tutorial")]
public class TutorialTestController : ControllerBase
{
    private readonly FlagService _flagService;
    private readonly NarrativeManager _narrativeManager;
    private readonly GameWorld _gameWorld;
    private readonly ILogger<TutorialTestController> _logger;

    public TutorialTestController(
        FlagService flagService,
        NarrativeManager narrativeManager,
        GameWorld gameWorld,
        ILogger<TutorialTestController> logger)
    {
        _flagService = flagService;
        _narrativeManager = narrativeManager;
        _gameWorld = gameWorld;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetTutorialStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Tutorial Status Report ===");
        sb.AppendLine($"Tutorial Active Flag: {_flagService.HasFlag("tutorial_active")}");
        sb.AppendLine($"Tutorial Complete Flag: {_flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
        sb.AppendLine($"Patron Accepted Flag: {_flagService.HasFlag(FlagService.TUTORIAL_PATRON_ACCEPTED)}");
        sb.AppendLine();
        
        sb.AppendLine("Active Narratives:");
        var activeNarratives = _narrativeManager.GetActiveNarratives();
        foreach (var narrative in activeNarratives)
        {
            sb.AppendLine($"  - {narrative}");
        }
        sb.AppendLine();
        
        sb.AppendLine("Current Narrative Step:");
        if (_narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
        {
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            if (currentStep != null)
            {
                sb.AppendLine($"  ID: {currentStep.Id}");
                sb.AppendLine($"  Name: {currentStep.Name}");
                sb.AppendLine($"  Description: {currentStep.Description}");
                sb.AppendLine($"  Guidance: {currentStep.GuidanceText}");
                sb.AppendLine($"  Allowed Actions: {currentStep.AllowedActions?.Count ?? 0}");
                if (currentStep.AllowedActions != null && currentStep.AllowedActions.Any())
                {
                    foreach (var action in currentStep.AllowedActions)
                    {
                        sb.AppendLine($"    - {action}");
                    }
                }
                sb.AppendLine($"  Visible NPCs: {currentStep.VisibleNPCs?.Count ?? 0}");
                sb.AppendLine($"  Visible Locations: {currentStep.VisibleLocations?.Count ?? 0}");
            }
            else
            {
                sb.AppendLine("  No current step found");
            }
        }
        else
        {
            sb.AppendLine("  Tutorial narrative is not active");
        }
        sb.AppendLine();
        
        sb.AppendLine("Player State:");
        var player = _gameWorld.GetPlayer();
        sb.AppendLine($"  Coins: {player.Coins}");
        sb.AppendLine($"  Stamina: {player.Stamina}/{player.MaxStamina}");
        sb.AppendLine($"  Has Patron: {player.HasPatron}");
        sb.AppendLine($"  Current Location: {player.CurrentLocation?.Name ?? "Unknown"}");
        sb.AppendLine();
        
        sb.AppendLine("Tutorial Actions Blocked:");
        // Check if we're in tutorial mode by looking at allowed actions
        if (_narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
        {
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            if (currentStep != null && currentStep.AllowedActions != null && currentStep.AllowedActions.Any())
            {
                sb.AppendLine($"  Actions Blocked: Yes (only {currentStep.AllowedActions.Count} actions allowed)");
            }
            else
            {
                sb.AppendLine($"  Actions Blocked: No (all actions allowed)");
            }
        }
        else
        {
            sb.AppendLine($"  Tutorial not active");
        }
        
        return Ok(sb.ToString());
    }
    
    [HttpPost("start")]
    public IActionResult StartTutorial()
    {
        try
        {
            _logger.LogInformation("Starting tutorial via API");
            
            // Clear tutorial complete flag
            _flagService.SetFlag(FlagService.TUTORIAL_COMPLETE, false);
            
            // Set tutorial starting conditions
            var player = _gameWorld.GetPlayer();
            player.Coins = 2;
            player.Stamina = 4;
            
            // Load and start tutorial
            NarrativeContentBuilder.BuildAllNarratives();
            _narrativeManager.LoadNarrativeDefinitions(NarrativeDefinitions.All);
            _narrativeManager.StartNarrative("wayfarer_tutorial");
            
            return Ok("Tutorial started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start tutorial");
            return StatusCode(500, $"Failed to start tutorial: {ex.Message}");
        }
    }
    
    [HttpPost("advance")]
    public IActionResult AdvanceTutorial([FromQuery] string stepId)
    {
        try
        {
            if (!_narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
            {
                return BadRequest("Tutorial is not active");
            }
            
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            if (currentStep == null)
            {
                return BadRequest("No current tutorial step");
            }
            
            // For now, we'll just return info about manual advancement
            // The actual advancement happens through game actions
            return Ok($"To advance tutorial, complete the current step requirements. Current step: {currentStep.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to advance tutorial");
            return StatusCode(500, $"Failed to advance tutorial: {ex.Message}");
        }
    }
    
    [HttpPost("create-test-player")]
    public IActionResult CreateTestPlayer()
    {
        try
        {
            var player = _gameWorld.GetPlayer();
            if (player.IsInitialized)
            {
                return BadRequest("Player already initialized");
            }
            
            // Initialize player for testing
            player.Initialize("TestCourier", Professions.Merchant, Genders.Male);
            
            _logger.LogInformation("Test player created: TestCourier");
            
            return Ok("Test player created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create test player");
            return StatusCode(500, $"Failed to create test player: {ex.Message}");
        }
    }
    
    [HttpGet("npcs")]
    public IActionResult GetNPCVisibilityStatus([FromServices] NPCRepository npcRepository)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== NPC Visibility Status ===");
        
        // Get current location
        var player = _gameWorld.GetPlayer();
        sb.AppendLine($"Current Location: {player.CurrentLocation?.Name} ({player.CurrentLocation?.Id})");
        sb.AppendLine($"Current Spot: {player.CurrentLocationSpot?.Name} ({player.CurrentLocationSpot?.SpotID})");
        sb.AppendLine();
        
        // Get all NPCs at current spot
        var allNPCs = npcRepository.GetAllNPCs()
            .Where(npc => npc.SpotId == player.CurrentLocationSpot?.SpotID)
            .ToList();
            
        sb.AppendLine($"Total NPCs at current spot: {allNPCs.Count}");
        foreach (var npc in allNPCs)
        {
            bool isVisible = _narrativeManager.IsNPCVisible(npc.ID);
            sb.AppendLine($"  - {npc.Name} ({npc.ID}): {(isVisible ? "VISIBLE" : "HIDDEN")}");
        }
        sb.AppendLine();
        
        // Check what the current tutorial step says about visible NPCs
        if (_narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
        {
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            if (currentStep != null)
            {
                sb.AppendLine($"Current tutorial step visible NPCs: {currentStep.VisibleNPCs.Count}");
                foreach (var npcId in currentStep.VisibleNPCs)
                {
                    sb.AppendLine($"  - {npcId}");
                }
            }
        }
        
        return Ok(sb.ToString());
    }
}