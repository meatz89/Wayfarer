using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

public class MusicPlayerBase : ComponentBase, IDisposable
{
    [Inject] public MusicService MusicService { get; set; }

    public string _formattedCurrentPosition = "0:00";
    public string _formattedDuration = "0:00";
    public System.Timers.Timer _uiTimer;

    protected override void OnInitialized()
    {
        MusicService.OnQueueChanged += StateHasChanged;
        MusicService.OnPlaybackStateChanged += StateHasChanged;
        MusicService.OnPositionChanged += UpdatePosition;

        _uiTimer = new System.Timers.Timer(1000);
        _uiTimer.Elapsed += async (sender, e) =>
        {
            if (MusicService.IsPlaying)
            {
                await InvokeAsync(() =>
                {
                    MusicService.IncrementPosition(TimeSpan.FromSeconds(1));
                    UpdatePosition(MusicService.CurrentPosition);
                });
            }
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

        MusicService.OnQueueChanged -= StateHasChanged;
        MusicService.OnPlaybackStateChanged -= StateHasChanged;
        MusicService.OnPositionChanged -= UpdatePosition;
    }
}
