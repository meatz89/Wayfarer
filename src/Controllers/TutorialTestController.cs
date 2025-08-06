using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wayfarer.ViewModels;

namespace Wayfarer.Controllers;

/// <summary>
/// Test controller that uses THE SAME GameFacade interface as the UI.
/// This ensures we're testing the actual game flow, not a parallel implementation.
/// </summary>
[ApiController]
[Route("api/tutorial")]
public class TutorialTestController : ControllerBase
{
    private readonly GameFacade _gameFacade;
    private readonly ILogger<TutorialTestController> _logger;

    public TutorialTestController(
       GameFacade gameFacade,
        ILogger<TutorialTestController> logger)
    {
        _gameFacade = gameFacade;
        _logger = logger;
    }

    [HttpGet("status")]
    public IActionResult GetTutorialStatus()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Tutorial Status Report ===");

        // Get narrative state
        NarrativeStateViewModel narrativeState = _gameFacade.GetNarrativeState();
        sb.AppendLine($"Tutorial Active: {narrativeState.IsTutorialActive}");
        sb.AppendLine($"Tutorial Complete: {narrativeState.TutorialComplete}");
        sb.AppendLine();

        sb.AppendLine("Active Narratives:");
        foreach (NarrativeViewModel narrative in narrativeState.ActiveNarratives)
        {
            sb.AppendLine($"  - {narrative.NarrativeId}: {narrative.StepName}");
        }
        sb.AppendLine();

        // Get tutorial guidance
        TutorialGuidanceViewModel tutorialGuidance = _gameFacade.GetTutorialGuidance();
        if (tutorialGuidance.IsActive)
        {
            sb.AppendLine("Current Tutorial Step:");
            sb.AppendLine($"  Step: {tutorialGuidance.CurrentStep}/{tutorialGuidance.TotalSteps}");
            sb.AppendLine($"  Title: {tutorialGuidance.StepTitle}");
            sb.AppendLine($"  Guidance: {tutorialGuidance.GuidanceText}");
            sb.AppendLine($"  Allowed Actions: {tutorialGuidance.AllowedActions.Count}");
            foreach (string action in tutorialGuidance.AllowedActions)
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
        Player player = _gameFacade.GetPlayer();
        (Location location, LocationSpot spot) = _gameFacade.GetCurrentLocation();
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
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Location Actions ===");

        (Location location, LocationSpot spot) = _gameFacade.GetCurrentLocation();
        sb.AppendLine($"Current Location: {location?.Name} - {spot?.Name}");
        sb.AppendLine();

        LocationActionsViewModel actions = _gameFacade.GetLocationActions();
        sb.AppendLine($"Time: {actions.CurrentTimeBlock}, {actions.HoursRemaining} hours remaining");
        sb.AppendLine($"Resources: {actions.PlayerStamina} stamina, {actions.PlayerCoins} coins");
        sb.AppendLine();

        foreach (ActionGroupViewModel group in actions.ActionGroups)
        {
            sb.AppendLine($"{group.ActionType}:");
            foreach (ActionOptionViewModel action in group.Actions)
            {
                string status = action.IsAvailable ? "Available" : "Unavailable";
                string tutorialStatus = action.IsAllowedInTutorial ? "" : " [BLOCKED BY TUTORIAL]";
                sb.AppendLine($"  [{action.Id}] {action.Description} - {status}{tutorialStatus}");

                if (action.TimeCost > 0 || action.StaminaCost > 0 || action.CoinCost > 0)
                {
                    sb.AppendLine($"    Cost: {action.TimeCost}h, {action.StaminaCost} stamina, {action.CoinCost} coins");
                }

                if (!action.IsAvailable)
                {
                    foreach (string reason in action.UnavailableReasons)
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

            bool success = await _gameFacade.ExecuteLocationActionAsync(actionId);

            if (success)
            {
                // Get updated state
                Player player = _gameFacade.GetPlayer();
                List<SystemMessage> messages = _gameFacade.GetSystemMessages();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Action executed successfully!");
                sb.AppendLine($"Player state: {player.Stamina}/{player.MaxStamina} stamina, {player.Coins} coins");

                if (messages.Any())
                {
                    sb.AppendLine("\nSystem messages:");
                    foreach (SystemMessage msg in messages)
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
                List<SystemMessage> messages = _gameFacade.GetSystemMessages();
                StringBuilder errorDetails = new StringBuilder();
                errorDetails.AppendLine("Action execution failed. System messages:");
                foreach (SystemMessage msg in messages)
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
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Travel Options ===");

        List<TravelDestinationViewModel> destinations = _gameFacade.GetTravelDestinations();

        foreach (TravelDestinationViewModel dest in destinations)
        {
            sb.AppendLine($"\n{dest.LocationName} ({dest.LocationId}):");
            sb.AppendLine($"  {dest.Description}");

            if (dest.CanTravel)
            {
                sb.AppendLine($"  Min cost: {dest.MinimumCost} coins, {dest.MinimumTime} hours");

                List<TravelRouteViewModel> routes = _gameFacade.GetRoutesToDestination(dest.LocationId);
                sb.AppendLine("  Routes:");
                foreach (TravelRouteViewModel route in routes)
                {
                    string status = route.CanTravel ? "Available" : "Unavailable";
                    sb.AppendLine($"    [{route.RouteId}] {route.RouteName} - {status}");
                    sb.AppendLine($"      Method: {route.TransportMethod}, Cost: {route.TimeCost}h, {route.TotalStaminaCost} stamina, {route.CoinCost} coins");

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

            bool success = await _gameFacade.TravelToDestinationAsync(destinationId, routeId);

            if (success)
            {
                (Location location, LocationSpot spot) = _gameFacade.GetCurrentLocation();
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
            ConversationViewModel conversation = await _gameFacade.StartConversationAsync(npcId);

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
            ConversationViewModel conversation = await _gameFacade.ContinueConversationAsync(choiceId);

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
        StringBuilder sb = new StringBuilder();
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
            foreach (ConversationChoiceViewModel choice in conv.Choices)
            {
                string status = choice.IsAvailable ? "" : " [UNAVAILABLE]";
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