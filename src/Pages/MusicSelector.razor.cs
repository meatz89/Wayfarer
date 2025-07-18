using Microsoft.AspNetCore.Components;
namespace Wayfarer.Pages;

public class MusicSelectorBase : ComponentBase
{
    [Inject] private MusicService MusicService { get; set; }
    [Parameter] public string MusicDirectory { get; set; } = "forest";
    [Parameter] public List<Track> AvailableTracks { get; set; } = new List<Track>();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(MusicDirectory))
        {
            await LoadFolderAsync(MusicDirectory);
        }
    }

    public async Task LoadFolderAsync(string folderName)
    {
        MusicService.ClearQueue();

        List<Track> tracks = MusicService.LoadTracksFromFolder(folderName);

        if (tracks.Count == 0)
            return;

        List<Track> shuffledTracks = ShuffleTracks(tracks);
        MusicService.EnqueueTracks(shuffledTracks);
    }

    public async Task UpdateMusicForContextAsync(List<string> contextTags)
    {
        await MusicService.StopCurrentTrackAsync();
        MusicService.ClearQueue();

        List<Track> matchingTracks = FilterTracksByTags(contextTags);

        if (matchingTracks.Count == 0)
            return;

        List<Track> shuffledTracks = ShuffleTracks(matchingTracks);
        MusicService.EnqueueTracks(shuffledTracks);
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
        return tracks.OrderBy(_ => random.Next()).ToList();
    }
}
