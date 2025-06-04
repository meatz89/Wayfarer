using BlazorRPG.Components;
using Microsoft.AspNetCore.Components;
using System.Timers;

namespace BlazorRPG.Pages
{
    public partial class MusicPlayer : ComponentBase, IDisposable
    {
        [Inject] private MusicService MusicService { get; set; }

        private string _formattedCurrentPosition = "0:00";
        private string _formattedDuration = "0:00";
        private System.Timers.Timer _uiTimer;

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

        private void UpdatePosition(TimeSpan position)
        {
            _formattedCurrentPosition = FormatTimeSpan(position);
            InvokeAsync(StateHasChanged);
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:00}";
        }

        private async Task TogglePlayPauseAsync()
        {
            if (MusicService.IsPlaying)
                await MusicService.PauseAsync();
            else
                await MusicService.PlayAsync();
        }

        private async Task NextTrackAsync()
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
}
