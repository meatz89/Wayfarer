using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Controllers;

/// <summary>
/// Test controller that uses THE SAME GameFacade interface as the UI.
/// This ensures we're testing the actual game flow, not a parallel implementation.
/// </summary>
[ApiController]
[Route("api/tutorial")]
public class TutorialTestController : ControllerBase
{
    private readonly IGameFacade _gameFacade;
    private readonly ILogger<TutorialTestController> _logger;

    public TutorialTestController(
        IGameFacade gameFacade,
        ILogger<TutorialTestController> logger)
    {
        _gameFacade = gameFacade;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetTutorialStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Tutorial Status Report ===");
        
        // Get narrative state
        var narrativeState = _gameFacade.GetNarrativeState();
        sb.AppendLine($"Tutorial Active: {narrativeState.IsTutorialActive}");
        sb.AppendLine($"Tutorial Complete: {narrativeState.TutorialComplete}");
        sb.AppendLine();
        
        sb.AppendLine("Active Narratives:");
        foreach (var narrative in narrativeState.ActiveNarratives)
        {
            sb.AppendLine($"  - {narrative.NarrativeId}: {narrative.StepName}");
        }
        sb.AppendLine();
        
        // Get tutorial guidance
        var tutorialGuidance = _gameFacade.GetTutorialGuidance();
        if (tutorialGuidance.IsActive)
        {
            sb.AppendLine("Current Tutorial Step:");
            sb.AppendLine($"  Step: {tutorialGuidance.CurrentStep}/{tutorialGuidance.TotalSteps}");
            sb.AppendLine($"  Title: {tutorialGuidance.StepTitle}");
            sb.AppendLine($"  Guidance: {tutorialGuidance.GuidanceText}");
            sb.AppendLine($"  Allowed Actions: {tutorialGuidance.AllowedActions.Count}");
            foreach (var action in tutorialGuidance.AllowedActions)
            {
                sb.AppendLine($"    - {action}");
            }
        }
        else
        {
            sb.AppendLine("Tutorial is not active");
        }
        sb.AppendLine();
        
        // Get player state
        var player = _gameFacade.GetPlayer();
        var (location, spot) = _gameFacade.GetCurrentLocation();
        sb.AppendLine("Player State:");
        sb.AppendLine($"  Coins: {player.Coins}");
        sb.AppendLine($"  Stamina: {player.Stamina}/{player.MaxStamina}");
        sb.AppendLine($"  Has Patron: {player.HasPatron}");
        sb.AppendLine($"  Current Location: {location?.Name ?? "Unknown"} - {spot?.Name ?? "Unknown"}");
        
        return Ok(sb.ToString());
    }
    
    [HttpPost("start-game")]
    public async Task<IActionResult> StartGame()
    {
        try
        {
            _logger.LogInformation("Starting game via API");
            
            await _gameFacade.StartGameAsync();
            
            return Ok("Game started successfully. Tutorial should auto-start for new players.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game");
            return StatusCode(500, $"Failed to start game: {ex.Message}");
        }
    }
    
    [HttpGet("location-actions")]
    public IActionResult GetLocationActions()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Location Actions ===");
        
        var (location, spot) = _gameFacade.GetCurrentLocation();
        sb.AppendLine($"Current Location: {location?.Name} - {spot?.Name}");
        sb.AppendLine();
        
        var actions = _gameFacade.GetLocationActions();
        sb.AppendLine($"Time: {actions.CurrentTimeBlock}, {actions.HoursRemaining} hours remaining");
        sb.AppendLine($"Resources: {actions.PlayerStamina} stamina, {actions.PlayerCoins} coins");
        sb.AppendLine();
        
        foreach (var group in actions.ActionGroups)
        {
            sb.AppendLine($"{group.ActionType}:");
            foreach (var action in group.Actions)
            {
                var status = action.IsAvailable ? "Available" : "Unavailable";
                var tutorialStatus = action.IsAllowedInTutorial ? "" : " [BLOCKED BY TUTORIAL]";
                sb.AppendLine($"  [{action.Id}] {action.Description} - {status}{tutorialStatus}");
                
                if (action.TimeCost > 0 || action.StaminaCost > 0 || action.CoinCost > 0)
                {
                    sb.AppendLine($"    Cost: {action.TimeCost}h, {action.StaminaCost} stamina, {action.CoinCost} coins");
                }
                
                if (!action.IsAvailable)
                {
                    foreach (var reason in action.UnavailableReasons)
                    {
                        sb.AppendLine($"    Reason: {reason}");
                    }
                }
            }
        }
        
        return Ok(sb.ToString());
    }
    
    [HttpPost("execute-action/{actionId}")]
    public async Task<IActionResult> ExecuteAction(string actionId)
    {
        try
        {
            _logger.LogInformation($"Executing action: {actionId}");
            
            var success = await _gameFacade.ExecuteLocationActionAsync(actionId);
            
            if (success)
            {
                // Get updated state
                var player = _gameFacade.GetPlayer();
                var messages = _gameFacade.GetSystemMessages();
                
                var sb = new StringBuilder();
                sb.AppendLine($"Action executed successfully!");
                sb.AppendLine($"Player state: {player.Stamina}/{player.MaxStamina} stamina, {player.Coins} coins");
                
                if (messages.Any())
                {
                    sb.AppendLine("\nSystem messages:");
                    foreach (var msg in messages)
                    {
                        sb.AppendLine($"  [{msg.Type}] {msg.Message}");
                    }
                }
                
                // Clear messages after reading
                _gameFacade.ClearSystemMessages();
                
                return Ok(sb.ToString());
            }
            else
            {
                // Get system messages to see what went wrong
                var messages = _gameFacade.GetSystemMessages();
                var errorDetails = new StringBuilder();
                errorDetails.AppendLine("Action execution failed. System messages:");
                foreach (var msg in messages)
                {
                    errorDetails.AppendLine($"  [{msg.Type}] {msg.Message}");
                }
                _gameFacade.ClearSystemMessages();
                return BadRequest(errorDetails.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to execute action: {actionId}");
            return StatusCode(500, $"Failed to execute action: {ex.Message}");
        }
    }
    
    [HttpGet("travel-options")]
    public IActionResult GetTravelOptions()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Travel Options ===");
        
        var destinations = _gameFacade.GetTravelDestinations();
        
        foreach (var dest in destinations)
        {
            sb.AppendLine($"\n{dest.LocationName} ({dest.LocationId}):");
            sb.AppendLine($"  {dest.Description}");
            
            if (dest.CanTravel)
            {
                sb.AppendLine($"  Min cost: {dest.MinimumCost} coins, {dest.MinimumTime} hours");
                
                var routes = _gameFacade.GetRoutesToDestination(dest.LocationId);
                sb.AppendLine("  Routes:");
                foreach (var route in routes)
                {
                    var status = route.CanTravel ? "Available" : "Unavailable";
                    sb.AppendLine($"    [{route.RouteId}] {route.RouteName} - {status}");
                    sb.AppendLine($"      Method: {route.TransportMethod}, Cost: {route.TimeCost}h, {route.StaminaCost} stamina, {route.CoinCost} coins");
                    
                    if (!route.CanTravel)
                    {
                        sb.AppendLine($"      Reason: {route.CannotTravelReason}");
                    }
                }
            }
            else
            {
                sb.AppendLine($"  Cannot travel: {dest.CannotTravelReason}");
            }
        }
        
        return Ok(sb.ToString());
    }
    
    [HttpPost("travel/{destinationId}/{routeId}")]
    public async Task<IActionResult> Travel(string destinationId, string routeId)
    {
        try
        {
            _logger.LogInformation($"Traveling to {destinationId} via {routeId}");
            
            var success = await _gameFacade.TravelToDestinationAsync(destinationId, routeId);
            
            if (success)
            {
                var (location, spot) = _gameFacade.GetCurrentLocation();
                return Ok($"Travel successful! Now at: {location.Name} - {spot?.Name ?? "arrival point"}");
            }
            else
            {
                return BadRequest("Travel failed. Check system messages.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to travel");
            return StatusCode(500, $"Failed to travel: {ex.Message}");
        }
    }
    
    [HttpGet("conversation/{npcId}")]
    public async Task<IActionResult> StartConversation(string npcId)
    {
        try
        {
            var conversation = await _gameFacade.StartConversationAsync(npcId);
            
            if (conversation == null)
            {
                return NotFound($"Cannot start conversation with NPC: {npcId}");
            }
            
            return Ok(FormatConversation(conversation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to start conversation with {npcId}");
            return StatusCode(500, $"Failed to start conversation: {ex.Message}");
        }
    }
    
    [HttpPost("conversation/choice/{choiceId}")]
    public async Task<IActionResult> SelectConversationChoice(string choiceId)
    {
        try
        {
            var conversation = await _gameFacade.ContinueConversationAsync(choiceId);
            
            if (conversation == null)
            {
                return BadRequest("No active conversation or invalid choice");
            }
            
            return Ok(FormatConversation(conversation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to select choice: {choiceId}");
            return StatusCode(500, $"Failed to continue conversation: {ex.Message}");
        }
    }
    
    private string FormatConversation(ConversationViewModel conv)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== Conversation with {conv.NpcName} ===");
        sb.AppendLine($"Topic: {conv.ConversationTopic}");
        sb.AppendLine();
        sb.AppendLine(conv.CurrentText);
        sb.AppendLine();
        
        if (conv.IsComplete)
        {
            sb.AppendLine("[Conversation Complete]");
        }
        else if (conv.Choices.Any())
        {
            sb.AppendLine("Choices:");
            foreach (var choice in conv.Choices)
            {
                var status = choice.IsAvailable ? "" : " [UNAVAILABLE]";
                sb.AppendLine($"  [{choice.Id}] {choice.Text}{status}");
                if (!choice.IsAvailable)
                {
                    sb.AppendLine($"    Reason: {choice.UnavailableReason}");
                }
            }
        }
        
        return sb.ToString();
    }
}