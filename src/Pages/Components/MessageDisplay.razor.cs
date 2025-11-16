using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace Wayfarer.Pages.Components;

public class MessageSegment
{
    public bool IsIcon { get; set; }
    public string Content { get; set; }
}

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
        List<SystemMessage> allMessages = GameFacade.GetSystemMessages();

        // Check if player has taken a new action (more messages added)
        if (allMessages.Count > _lastActionCount && _lastActionCount > 0)
        {
            // Clear old messages when new action is taken
            foreach (SystemMessage oldMessage in Messages)
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

    protected List<MessageSegment> ParseMessageSegments(string message)
    {
        List<MessageSegment> segments = new List<MessageSegment>();

        if (string.IsNullOrEmpty(message))
            return segments;

        Regex iconPattern = new Regex(@"\{icon:([a-z-]+)\}", RegexOptions.IgnoreCase);
        int lastIndex = 0;

        foreach (Match match in iconPattern.Matches(message))
        {
            if (match.Index > lastIndex)
            {
                string textBefore = message.Substring(lastIndex, match.Index - lastIndex);
                segments.Add(new MessageSegment { IsIcon = false, Content = textBefore });
            }

            string iconName = match.Groups[1].Value;
            segments.Add(new MessageSegment { IsIcon = true, Content = iconName });

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < message.Length)
        {
            string textAfter = message.Substring(lastIndex);
            segments.Add(new MessageSegment { IsIcon = false, Content = textAfter });
        }

        return segments;
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}