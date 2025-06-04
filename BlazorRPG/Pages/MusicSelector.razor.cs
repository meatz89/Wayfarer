using BlazorRPG.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorRPG.Pages
{
    public partial class MusicSelector : ComponentBase
    {
        [Inject] private MusicService MusicService { get; set; }
        [Parameter] public string MusicDirectory { get; set; } = "forest";
        [Parameter] public List<Track> AvailableTracks { get; set; } = new List<Track>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !string.IsNullOrEmpty(MusicDirectory))
            {
                await LoadAndPlayFolderAsync(MusicDirectory);
            }
        }

        public async Task LoadAndPlayFolderAsync(string folderName)
        {
            MusicService.ClearQueue();

            List<Track> tracks = MusicService.LoadTracksFromFolder(folderName);

            if (tracks.Count == 0)
                return;

            MusicService.EnqueueTracks(tracks);
            await MusicService.StartPlayingQueueAsync();
        }

        public async Task UpdateMusicForContextAsync(List<string> contextTags)
        {
            // Stop current playback
            await MusicService.StopCurrentTrackAsync();

            // Clear the queue
            MusicService.ClearQueue();

            // Filter tracks based on tags
            List<Track> matchingTracks = FilterTracksByTags(contextTags);

            // If no matching tracks, return
            if (matchingTracks.Count == 0)
                return;

            // Shuffle tracks for variety
            List<Track> shuffledTracks = ShuffleTracks(matchingTracks);

            // Enqueue filtered tracks
            MusicService.EnqueueTracks(shuffledTracks);

            // Start playing the first track
            await MusicService.StartPlayingQueueAsync();
        }

        private List<Track> FilterTracksByTags(List<string> contextTags)
        {
            if (contextTags == null || contextTags.Count == 0)
                return new List<Track>();

            return AvailableTracks
                .Where(track => track.Tags != null && track.Tags.Any(tag => contextTags.Contains(tag)))
                .ToList();
        }

        private List<Track> ShuffleTracks(List<Track> tracks)
        {
            Random random = new Random();
            return tracks.OrderBy(x => random.Next()).ToList();
        }
    }
}
