using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Conversation tree screen component that handles simple NPC dialogue trees.
    /// Can escalate to tactical Social challenges when tension rises.
    ///
    /// ARCHITECTURAL PRINCIPLES:
    /// - Follows the authoritative parent pattern - receives GameScreen via CascadingParameter
    /// - ConversationTreeContext passed as Parameter from parent (created by GameFacade)
    /// - All game state mutations go through GameFacade
    /// - Simple dialogue tree navigation with potential escalation to Social challenges
    /// </summary>
    public class ConversationTreeContentBase : ComponentBase
    {
        [Parameter] public ConversationTreeContext Context { get; set; }
        [Parameter] public EventCallback OnConversationEnd { get; set; }
        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }

        protected async Task HandleSelectResponse(DialogueResponse response)
        {
            if (Context == null || !Context.IsValid) return;
            if (Context.CurrentNode == null) return;

            ConversationTreeResult result = GameFacade.SelectConversationResponse(
                Context.Tree,
                Context.CurrentNode,
                response);

            if (!result.Success)
            {
                // Show error message through GameFacade message system
                GameFacade.GetMessageSystem().AddSystemMessage(
                    result.Message ?? "Failed to select response",
                    SystemMessageTypes.Danger);
                return;
            }

            if (result.EscalatesToChallenge)
            {
                // Navigate to Social Challenge screen
                await GameScreen.StartConversationSession(
                    Context.Npc,
                    result.ChallengeSituation);
            }
            else if (result.IsComplete)
            {
                // Conversation ended, return to location
                await OnConversationEnd.InvokeAsync();
            }
            else if (result.NextNode != null)
            {
                // Refresh context with new node
                Context = GameFacade.CreateConversationTreeContext(Context.Tree);
                StateHasChanged();
            }
        }

        protected async Task HandleReturn()
        {
            await OnConversationEnd.InvokeAsync();
        }
    }
}
