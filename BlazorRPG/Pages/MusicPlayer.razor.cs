using BlazorRPG.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorRPG.Pages
{
    // Main Music Player component
    public partial class MusicPlayer : ComponentBase, IDisposable
    {
        [Inject] private MusicService MusicService { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }

        private string _formattedCurrentPosition = "0:00";
        private string _formattedDuration = "0:00";
        private bool _isInitialized = false;

        protected override async Task OnInitializedAsync()
        {
            MusicService.OnQueueChanged += StateHasChanged;
            MusicService.OnPlaybackStateChanged += StateHasChanged;
            MusicService.OnPositionChanged += UpdatePosition;

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("musicPlayer.initialize");
                _isInitialized = true;
            }
        }

        private void UpdatePosition(TimeSpan position)
        {
            _formattedCurrentPosition = FormatTimeSpan(position);
            StateHasChanged();
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:00}";
        }

        private async Task TogglePlayPauseAsync()
        {
            if (MusicService.IsPlaying)
            {
                await MusicService.PauseAsync();
            }
            else
            {
                await MusicService.PlayAsync();
            }
        }

        private async Task NextTrackAsync()
        {
            await MusicService.PlayNextTrackAsync();
        }

        public void Dispose()
        {
            MusicService.OnQueueChanged -= StateHasChanged;
            MusicService.OnPlaybackStateChanged -= StateHasChanged;
            MusicService.OnPositionChanged -= UpdatePosition;
        }
    }
}
