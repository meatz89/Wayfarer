using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

public class MusicPlayerBase : ComponentBase, IDisposable
{
    [Inject] public MusicService MusicService { get; set; }

    public string _formattedCurrentPosition = "0:00";
    public string _formattedDuration = "0:00";
    public System.Timers.Timer _uiTimer;
    private int _lastStateVersion = -1;

    protected override void OnInitialized()
    {
        // Razor code-behind files ARE allowed to use events
        // But we need to poll since MusicService can't have events
        _uiTimer = new System.Timers.Timer(100); // Poll every 100ms
        _uiTimer.Elapsed += async (sender, e) =>
        {
            await InvokeAsync(() =>
            {
                // Check for state changes via polling
                if (MusicService.StateVersion != _lastStateVersion)
                {
                    _lastStateVersion = MusicService.StateVersion;
                    StateHasChanged();
                }

                // Update position if playing
                if (MusicService.IsPlaying)
                {
                    MusicService.IncrementPosition(TimeSpan.FromMilliseconds(100));
                    UpdatePosition(MusicService.CurrentPosition);
                }
            });
        };
        _uiTimer.Start();
    }

    public void UpdatePosition(TimeSpan position)
    {
        _formattedCurrentPosition = FormatTimeSpan(position);
        InvokeAsync(StateHasChanged);
    }

    public string FormatTimeSpan(TimeSpan timeSpan)
    {
        return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:00}";
    }

    public async Task TogglePlayPauseAsync()
    {
        if (MusicService.IsPlaying)
            await MusicService.PauseAsync();
        else
            await MusicService.PlayAsync();
    }

    public async Task NextTrackAsync()
    {
        await MusicService.PlayNextTrackAsync();
    }

    public void Dispose()
    {
        _uiTimer?.Stop();
        _uiTimer?.Dispose();
    }
}
