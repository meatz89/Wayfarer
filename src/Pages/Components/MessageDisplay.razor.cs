using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components;

public class MessageDisplayBase : ComponentBase, IDisposable
{
    [Inject] public GameFacade GameFacade { get; set; }
    
    protected List<SystemMessage> Messages { get; private set; } = new();
    protected bool HasMessages => Messages.Any();
    
    private System.Threading.Timer _refreshTimer;
    private int _lastActionCount = 0;

    protected override void OnInitialized()
    {
        RefreshMessages();
        _refreshTimer = new System.Threading.Timer(_ => 
        {
            InvokeAsync(() => 
            {
                RefreshMessages();
                StateHasChanged();
            });
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
    }

    private void RefreshMessages()
    {
        var allMessages = GameFacade.GetSystemMessages();
        
        // Check if player has taken a new action (more messages added)
        if (allMessages.Count > _lastActionCount && _lastActionCount > 0)
        {
            // Clear old messages when new action is taken
            foreach (var oldMessage in Messages)
            {
                oldMessage.IsExpired = true;
            }
        }
        _lastActionCount = allMessages.Count;
        
        // Get active (non-expired) messages
        Messages = allMessages
            .Where(m => !m.IsExpired)
            .OrderByDescending(m => m.Timestamp)
            .Take(5) // Show up to 5 messages
            .ToList();
    }
    
    protected void DismissMessage(SystemMessage message)
    {
        message.IsExpired = true;
        RefreshMessages();
        StateHasChanged();
    }

    protected string GetMessageClass(SystemMessageTypes type)
    {
        return type switch
        {
            SystemMessageTypes.Success => "success",
            SystemMessageTypes.Warning => "warning", 
            SystemMessageTypes.Danger => "danger",
            SystemMessageTypes.Tutorial => "tutorial",
            _ => "info"
        };
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}