using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components;

public class MessageDisplayBase : ComponentBase, IDisposable
{
    [Inject] public GameFacade GameFacade { get; set; }
    
    protected List<SystemMessage> Messages { get; private set; } = new();
    protected bool HasMessages => Messages.Any();
    
    private System.Threading.Timer _refreshTimer;

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
        Messages = GameFacade.GetSystemMessages()
            .Where(m => !m.IsExpired)
            .OrderByDescending(m => m.Timestamp)
            .Take(3)
            .ToList();
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